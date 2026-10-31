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

    // Touching the world's shared tuning while MoonWorld.Initialize runs crashes the game, so
    // features that write to it wait until the walker has been up for a while.
    static float readyAt;
    public static bool Ready => readyAt > 0 && Time.unscaledTime >= readyAt;

    public static void Refresh()
    {
        if (World == null) World = Object.FindFirstObjectByType<MoonWorld>();
        if (Walker == null) Walker = Object.FindFirstObjectByType<MoonWalker>();
        if (Compass == null) Compass = Object.FindFirstObjectByType<MoonServiceCompass>();
        if (Map == null) Map = Object.FindFirstObjectByType<MoonMapScreen>();
        if (Pause == null) Pause = Object.FindFirstObjectByType<MoonPauseMenu>();
        if (World == null || Walker == null) readyAt = 0;
        else if (readyAt == 0) readyAt = Time.unscaledTime + 20f;
    }

    public static bool MenuOpen => (Pause != null && Pause.IsOpen) || MoonCinematicHud.Hidden;
    public static bool MapOpen => Map != null && Map.IsOpen;
}
