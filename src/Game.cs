using MoonCleaners;
using UnityEngine;

namespace MoonSweepersQol;

// Cached lookups of the game objects features depend on, refreshed by Qol once a second.
static class Game
{
    public static MoonWorld World;
    public static MoonWalker Walker;
    public static MoonServiceCompass Compass;
    public static MoonMapScreen Map;
    public static MoonPauseMenu Pause;

    public static void Refresh()
    {
        if (World == null) World = Object.FindFirstObjectByType<MoonWorld>();
        if (Walker == null) Walker = Object.FindFirstObjectByType<MoonWalker>();
        if (Compass == null) Compass = Object.FindFirstObjectByType<MoonServiceCompass>();
        if (Map == null) Map = Object.FindFirstObjectByType<MoonMapScreen>();
        if (Pause == null) Pause = Object.FindFirstObjectByType<MoonPauseMenu>();
    }

    public static bool MenuOpen => (Pause != null && Pause.IsOpen) || MoonCinematicHud.Hidden;
    public static bool MapOpen => Map != null && Map.IsOpen;
}
