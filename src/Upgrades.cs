using System.Linq;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MoonCleaners;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MoonSweepersQol;

// Adds this mod's upgrades to the game's upgrade tree. The tree stores levels by node id,
// so bought upgrades are saved and loaded by the game itself.
static class Upgrades
{
    public const string LandmarkScanner = "qol.landmark-scanner";

    static bool registered;
    static int scannerLevel;

    // Whether landmarks are marked on the compass and map.
    public static bool ScannerOn => !Plugin.RequireUpgrade.Value || scannerLevel > 0;

    public static void Register()
    {
        if (Game.World != null)
            scannerLevel = Game.World.TreeLevel(MoonWorld.LocalPlayer, LandmarkScanner);
        if (registered) return;
        var nodes = MoonUpgradeTree.Nodes;
        if (nodes == null || nodes.Length == 0) return;
        if (nodes.Any(n => n.id == LandmarkScanner)) { registered = true; return; }

        var log = Plugin.Instance.Log;
        foreach (var n in nodes)
            log.LogInfo($"tree node {n.id} '{n.title}' {n.branch} {n.glyph} max {n.maxLevel} prices [{(n.prices == null ? "" : string.Join(",", n.prices))}] at {n.position} capstone {n.capstone} requires {(n.requires == null ? 0 : n.requires.Length)}");

        var node = new MoonUpgradeTree.Node
        {
            id = LandmarkScanner,
            title = "Landmark Scanner",
            summary = "Marks landmarks you have not raised yet on the compass and on the map.",
            branch = MoonUpgradeTree.Branch.Suit,
            glyph = MoonUpgradeTree.Glyph.Reach,
            maxLevel = 1,
            prices = new Il2CppStructArray<int>(new[] { 100 }),
            requires = new Il2CppReferenceArray<Il2CppSystem.ValueTuple<string, int>>(new[] { new Il2CppSystem.ValueTuple<string, int>("suit.stride", 1) }),
            requiresAny = false,
            tool = null,
            position = new Vector2(-445f, 25f),
            capstone = false,
            effect = (Il2CppSystem.Func<int, string>)(level => "Landmarks show on the compass and the map"),
        };

        var all = new Il2CppReferenceArray<MoonUpgradeTree.Node>(nodes.Length + 1);
        for (int i = 0; i < nodes.Length; i++) all[i] = nodes[i];
        all[nodes.Length] = node;
        MoonUpgradeTree.Nodes = all;
        MoonUpgradeTree.byId = MoonUpgradeTree.Index();
        registered = true;
        log.LogInfo($"registered upgrade {LandmarkScanner} at {node.position}, price {node.prices[0]}, Find: {MoonUpgradeTree.Find(LandmarkScanner) != null}, tree UIs already built: {Object.FindObjectsByType<MoonUpgradeTreeUI>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length}");
    }

}
