using System;
using HarmonyLib;
using MoonCleaners;
using UnityEngine;

namespace MoonSweepersQol;

// The map screen finds the flag under the cursor with FlagUnder, which returns a landmark index.
// Falling back to our unraised markers gives them the game's own hover panel and click handling.
[HarmonyPatch(typeof(MoonMapScreen), nameof(MoonMapScreen.FlagUnder))]
static class FlagUnderPatch
{
    static void Postfix(Vector2 screen, ref int __result)
    {
        if (__result >= 0) return;
        try
        {
            int hit = MapMarks.Hit(screen);
            if (hit >= 0) __result = hit;
        }
        catch (Exception e) { Plugin.Instance.Log.LogError(e); }
    }
}

// The info panel describes whatever FlagUnder found as a raised flag; ours are not raised yet.
[HarmonyPatch(typeof(MoonMapScreen), nameof(MoonMapScreen.ShowInfo))]
static class ShowInfoPatch
{
    // The line reads "<way>  ·  flag raised  ·  <money> when cleaned"; the state is for a raised flag.
    static readonly string[] Separator = { "  ·  " };

    static void Postfix(MoonMapScreen __instance, MoonWorld world)
    {
        try
        {
            int i = __instance.FlagUnder(__instance.pointer);
            if (i < 0 || world.IsLandmarkRaised(i)) return;

            var parts = __instance.infoLine.text.Split(Separator, StringSplitOptions.None);
            if (parts.Length < 3) return;
            parts[1] = "not raised yet";
            __instance.infoLine.text = string.Join(Separator[0], parts);
        }
        catch (Exception e) { Plugin.Instance.Log.LogError(e); }
    }
}
