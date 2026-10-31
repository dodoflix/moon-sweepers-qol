using System.Linq;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MoonCleaners;
using UnityEngine;

namespace MoonSweepersQol;

// Adds this mod's upgrades to the game's upgrade tree. The tree stores levels by node id,
// so bought upgrades are saved and loaded by the game itself.
static class Upgrades
{
    public const string LandmarkScanner = "qol.landmark-scanner";
    public const string BeltDrive = "qol.belt-drive";
    public const string MachineOverdrive = "qol.machine-overdrive";
    public const string RobotDrive = "qol.robot-drive";

    static bool registered;
    static int scannerLevel, beltLevel, machineLevel, robotLevel;

    // Whether landmarks are marked on the compass and map.
    public static bool ScannerOn => !Plugin.RequireUpgrade.Value || scannerLevel > 0;

    // Bought levels, or the top level when upgrades are not required.
    public static int BeltLevel => Plugin.RequireUpgrade.Value ? beltLevel : Machines.BeltBoost.Length - 1;
    public static int RobotLevel => Plugin.RequireUpgrade.Value ? robotLevel : Robots.Boost.Length - 1;
    public static int MachineLevel => Plugin.RequireUpgrade.Value ? machineLevel : Machines.MachineBoost.Length - 1;

    public static void Register()
    {
        if (Game.World != null)
        {
            var me = MoonWorld.LocalPlayer;
            scannerLevel = Game.World.TreeLevel(me, LandmarkScanner);
            beltLevel = Game.World.TreeLevel(me, BeltDrive);
            machineLevel = Game.World.TreeLevel(me, MachineOverdrive);
            robotLevel = Game.World.TreeLevel(me, RobotDrive);
        }
        if (registered) return;
        var nodes = MoonUpgradeTree.Nodes;
        if (nodes == null || nodes.Length == 0) return;
        if (nodes.Any(n => n.id == LandmarkScanner)) { registered = true; return; }

        var added = new[]
        {
            Make(LandmarkScanner, "Landmark Scanner", "Marks landmarks you have not raised yet on the compass and on the map.",
                MoonUpgradeTree.Branch.Suit, MoonUpgradeTree.Glyph.Reach, new[] { 100 }, ("suit.stride", 1), new Vector2(-445f, 25f),
                level => "Landmarks show on the compass and the map"),
            Make(BeltDrive, "Belt Drive", "Faster motors for conveyor belts. Capsules and riders move quicker, so one belt carries more.",
                MoonUpgradeTree.Branch.Logistics, MoonUpgradeTree.Glyph.Belt, new[] { 150, 450 }, ("logi.transfer", 1), new Vector2(-300f, -330f),
                level => $"+{Percent(Machines.BeltBoost, level)}% belt speed"),
            Make(MachineOverdrive, "Machine Overdrive", "Presses and pressure chambers cycle faster. They draw proportionally more power, so the energy per product stays the same.",
                MoonUpgradeTree.Branch.Logistics, MoonUpgradeTree.Glyph.Turbo, new[] { 300, 900, 2400 }, (BeltDrive, 1), new Vector2(-445f, -400f),
                level => $"+{Percent(Machines.MachineBoost, level)}% machine speed"),
            Make(RobotDrive, "Robot Drive", "Tuned drive trains for robot vacuums. They clean and travel faster, on and off screen.",
                MoonUpgradeTree.Branch.Cleaning, MoonUpgradeTree.Glyph.Suction, new[] { 250, 700, 1800 }, ("clean.reach", 1), new Vector2(330f, 360f),
                level => $"+{Percent(Robots.Boost, level)}% robot speed"),
        };

        var all = new Il2CppReferenceArray<MoonUpgradeTree.Node>(nodes.Length + added.Length);
        for (int i = 0; i < nodes.Length; i++) all[i] = nodes[i];
        for (int i = 0; i < added.Length; i++) all[nodes.Length + i] = added[i];
        MoonUpgradeTree.Nodes = all;
        MoonUpgradeTree.byId = MoonUpgradeTree.Index();
        registered = true;
        Plugin.Instance.Log.LogInfo($"registered {added.Length} upgrades, Find: {MoonUpgradeTree.Find(BeltDrive) != null}");
    }

    public static int Percent(float[] boosts, int level) => Mathf.RoundToInt((boosts[Mathf.Clamp(level, 0, boosts.Length - 1)] - 1f) * 100f);

    static MoonUpgradeTree.Node Make(string id, string title, string summary, MoonUpgradeTree.Branch branch, MoonUpgradeTree.Glyph glyph,
        int[] prices, (string id, int level) needs, Vector2 position, System.Func<int, string> effect) => new()
    {
        id = id,
        title = title,
        summary = summary,
        branch = branch,
        glyph = glyph,
        maxLevel = prices.Length,
        prices = new Il2CppStructArray<int>(prices),
        requires = new Il2CppReferenceArray<Il2CppSystem.ValueTuple<string, int>>(new[] { new Il2CppSystem.ValueTuple<string, int>(needs.id, needs.level) }),
        requiresAny = false,
        tool = null,
        position = position,
        capstone = false,
        effect = (Il2CppSystem.Func<int, string>)(level => effect(level)),
    };
}
