using System.Collections.Generic;
using MoonCleaners;

namespace MoonSweepersQol;

// Robot Drive: scales how fast robot vacuums clean and travel. Robots near the player run as
// MoonRobotVacuum objects with their own fields; the rest are simulated from a per-dock-kind rate.
static class Robots
{
    // Speed multiplier per level, index 0 is not bought.
    public static readonly float[] Boost = { 1f, 1.2f, 1.4f, 1.6f };

    struct Base { public float Clean, Travel, Accel, Motion; public int Level; }

    static readonly Dictionary<int, Base> live = new();
    static readonly Dictionary<int, float> farRate = new();
    static readonly Dictionary<int, int> farLevel = new();
    static bool used;

    public static void Apply()
    {
        if (!Game.Ready) return;
        int level = Upgrades.RobotLevel;
        if (level == 0 && !used) return;
        used = true;
        float boost = Boost[level];

        var robots = MoonRobotVacuum.active.ToArray();
        var seen = new HashSet<int>();
        foreach (var r in robots)
        {
            int id = r.GetInstanceID();
            seen.Add(id);
            if (!live.TryGetValue(id, out var b))
                b = new Base { Clean = r.cleaningSpeed, Travel = r.travelSpeed, Accel = r.acceleration, Motion = r.launcher == null ? 1f : r.launcher.motionRate, Level = -1 };
            if (b.Level != level)
            {
                r.cleaningSpeed = b.Clean * boost;
                r.travelSpeed = b.Travel * boost;
                r.acceleration = b.Accel * boost;
                if (r.launcher != null) r.launcher.motionRate = b.Motion * boost;
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
            spec.rate = farRate[kind] * boost;
            farLevel[kind] = level;
            Plugin.Instance.Log.LogInfo($"robot dock kind {kind}: far rate {spec.rate:0.00} (level {level})");
        }
    }
}
