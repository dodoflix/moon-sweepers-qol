using HarmonyLib;
using MoonCleaners;
using UnityEngine;

namespace MoonSweepersQol;

// Belt Drive and Machine Overdrive: scale the game's shared tuning values by the bought level.
static class Machines
{
    // Speed multiplier per level, index 0 is not bought.
    public static readonly float[] BeltBoost = { 1f, 1.3f, 1.6f };
    public static readonly float[] MachineBoost = { 1f, 1.25f, 1.5f, 1.75f };

    static bool captured;
    static readonly float[] baseCycle = new float[2], baseKw = new float[2];
    static int appliedMachine = -1;

    // MoonWorld.BeltSpeed is a game constant: writing it crashes the game, so belts are sped up by
    // scaling the time their motion is stepped by.
    public static float BeltFactor => BeltBoost[Upgrades.BeltLevel];

    static MoonWorld.ProcessorSpec[] Specs => new[] { MoonWorld.Processor(MoonWorld.StonePressKind), MoonWorld.Processor(MoonWorld.PressureChamberKind) };

    // Specs are shared by the whole session, so the base values are read once
    // and every level is applied as a multiple of them.
    public static void Apply()
    {
        if (!Game.Ready) return;
        int machine = Upgrades.MachineLevel;
        if (machine == 0 && appliedMachine == -1) return;
        var specs = Specs;
        if (!captured)
        {
            for (int i = 0; i < specs.Length; i++) { baseCycle[i] = specs[i].cycleSeconds; baseKw[i] = specs[i].runningKw; }
            captured = true;
        }

        if (machine != appliedMachine)
        {
            float boost = MachineBoost[machine];
            for (int i = 0; i < specs.Length; i++)
            {
                specs[i].cycleSeconds = baseCycle[i] / boost;
                specs[i].runningKw = Plugin.OverdrivePower.Value ? baseKw[i] * boost : baseKw[i];
                Plugin.Instance.Log.LogInfo($"{specs[i].name}: cycle {specs[i].cycleSeconds:0.0}s, {specs[i].runningKw:0.0} kW (level {machine})");
            }
            appliedMachine = machine;
        }
    }
}

[HarmonyPatch(typeof(MoonWorld), nameof(MoonWorld.StepConveyors))]
static class StepConveyorsPatch
{
    static void Prefix(ref float dt) => dt *= Machines.BeltFactor;
}

// Riders on a belt are carried at the same speed as the capsules.
[HarmonyPatch(typeof(MoonWorld), nameof(MoonWorld.BeltSurfaceVelocity))]
static class BeltSurfaceVelocityPatch
{
    static void Postfix(bool __result, ref Vector3 velocity)
    {
        if (__result) velocity *= Machines.BeltFactor;
    }
}
