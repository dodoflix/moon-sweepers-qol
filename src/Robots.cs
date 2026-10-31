using System.Collections.Generic;
using HarmonyLib;
using MoonCleaners;
using UnityEngine;

namespace MoonSweepersQol;

// Robot Drive and Robot Heads: scale how fast robot vacuums clean and travel, and how much ground they cover.
// Robots near the player run as MoonRobotVacuum objects with their own fields; the rest are simulated from a
// per-dock-kind rate.
static class Robots
{
    // Speed multiplier per level, index 0 is not bought.
    public static readonly float[] Boost = { 1f, 1.2f, 1.4f, 1.6f };
    // Coverage multiplier per level: sweep width, or pad size for Joonas.
    public static readonly float[] WidthBoost = { 1f, 1.15f, 1.3f, 1.45f };

    struct Base { public float Clean, Travel, Accel, Motion, Width, Pad; public int Level; }

    static readonly Dictionary<int, Base> live = new();
    static readonly Dictionary<int, float> farRate = new();
    static readonly Dictionary<int, int> farLevel = new();
    static bool used;

    public static void Apply()
    {
        if (!Game.Ready) return;
        int speedLevel = Upgrades.RobotLevel, widthLevel = Upgrades.RobotWidthLevel;
        if (speedLevel == 0 && widthLevel == 0 && !used) return;
        used = true;
        int level = speedLevel * 10 + widthLevel;
        float boost = Boost[speedLevel], width = WidthBoost[widthLevel];

        var robots = MoonRobotVacuum.active.ToArray();
        var seen = new HashSet<int>();
        foreach (var r in robots)
        {
            int id = r.GetInstanceID();
            seen.Add(id);
            if (!live.TryGetValue(id, out var b))
                b = new Base { Clean = r.cleaningSpeed, Travel = r.travelSpeed, Accel = r.acceleration, Motion = r.launcher == null ? 1f : r.launcher.motionRate, Width = r.cleaningWidth, Pad = r.launcher == null ? 0f : r.launcher.padSize, Level = -1 };
            if (b.Level != level)
            {
                r.cleaningSpeed = b.Clean * boost;
                r.travelSpeed = b.Travel * boost;
                r.acceleration = b.Accel * boost;
                r.cleaningWidth = b.Width * width;
                if (r.launcher != null) { r.launcher.padSize = b.Pad * width; r.launcher.motionRate = b.Motion * boost; }
                b.Level = level;
            }
            live[id] = b;
        }
        foreach (var id in new List<int>(live.Keys)) if (!seen.Contains(id)) live.Remove(id);

        var kinds = new HashSet<int> { MoonWorld.RobotDockKind, MoonWorld.RobotDockMk2Kind };
        foreach (var d in new Il2CppSystem.Collections.Generic.List<MoonDepotSnapshot>(Game.World.BuiltDepots).ToArray())
            if (MoonWorld.IsRobotDock(d.kind)) kinds.Add(d.kind);
        foreach (int kind in kinds)
        {
            var spec = Game.World.FarSpec(kind);
            if (spec == null) continue;
            if (!farRate.ContainsKey(kind)) { farRate[kind] = spec.rate; farLevel[kind] = -1; }
            if (farLevel[kind] == level) continue;
            spec.rate = farRate[kind] * boost * width;
            farLevel[kind] = level;
            Plugin.Instance.Log.LogInfo($"robot dock kind {kind}: far rate {spec.rate:0.00} (speed {speedLevel}, width {widthLevel})");
        }
    }
}

// Joonas's movement step clamps its speed while sweeping, whatever limit it is given, so Robot Drive
// would not show. Learn the clamp from the speed actually reached and make up the difference by
// stepping Drive for longer.
[HarmonyPatch(typeof(MoonRobotVacuum), nameof(MoonRobotVacuum.Drive))]
static class RobotDrivePatch
{
    static float reached;

    static bool Sweeping(MoonRobotVacuum r) => r.launcher != null && r.state.ToString() == "Cleaning";

    static void Prefix(MoonRobotVacuum __instance, float maxSpeed, ref float dt)
    {
        if (Upgrades.RobotLevel > 0 && Sweeping(__instance) && reached > 0 && maxSpeed > reached)
            dt *= Mathf.Min(maxSpeed / reached, Robots.Boost[Robots.Boost.Length - 1]);
    }

    static void Postfix(MoonRobotVacuum __instance)
    {
        if (Sweeping(__instance)) reached = Mathf.Max(reached, __instance.speed);
    }
}
