using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;

namespace MoonSweepersQol;

[BepInPlugin(Guid, "Moon Sweepers QoL", Version)]
public class Plugin : BasePlugin
{
    // The Package target in MoonSweepersQol.csproj reads Version from here.
    public const string Guid = "dodo.moonsweepers.qol";
    public const string Version = "1.1.1";

    public static Plugin Instance;
    public static ConfigEntry<bool> RequireUpgrade;
    public static ConfigEntry<bool> OverdrivePower;

    public override void Load()
    {
        Instance = this;
        RequireUpgrade = Config.Bind("General", "RequireUpgrade", true,
            "Features that are sold as upgrades only work once bought. Set to false to have them always on.");
        OverdrivePower = Config.Bind("Machines", "OverdrivePower", true,
            "Machines sped up by Machine Overdrive draw proportionally more power. Set to false to keep their power draw.");
        Harmony.CreateAndPatchAll(typeof(StepProductionDtPatch));
        Harmony.CreateAndPatchAll(typeof(FlagUnderPatch));
        Harmony.CreateAndPatchAll(typeof(ShowInfoPatch));
        Harmony.CreateAndPatchAll(typeof(RobotDrivePatch));
        Harmony.CreateAndPatchAll(typeof(StepConveyorsPatch));
        Harmony.CreateAndPatchAll(typeof(BeltSurfaceVelocityPatch));
        ClassInjector.RegisterTypeInIl2Cpp<Qol>();
        AddComponent<Qol>();
        Log.LogInfo("loaded");
        try { Upgrades.Register(); }
        catch (Exception e) { Log.LogWarning($"early upgrade registration failed, retrying in game: {e.Message}"); }
    }
}

// Drives every feature from one component: a slow scan for game objects, plus per-frame work.
public class Qol : MonoBehaviour
{
    float nextScan;

    public Qol(IntPtr ptr) : base(ptr) { }

    void Update()
    {
        if (Time.unscaledTime < nextScan) return;
        nextScan = Time.unscaledTime + 1f;
        try
        {
            Game.Refresh();
            Upgrades.Register();
            Landmarks.Scan();
            Hologram.Scan();
            Machines.Apply();
            Robots.Apply();
        }
        catch (Exception e) { Plugin.Instance.Log.LogError(e); }
    }

    void LateUpdate()
    {
        try { Landmarks.LateUpdate(); MapMarks.LateUpdate(); Hologram.LateUpdate(); }
        catch (Exception e) { Plugin.Instance.Log.LogError(e); }
    }

    void OnGUI()
    {
        var e = Event.current;
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.F8) Landmarks.Show = !Landmarks.Show;
    }
}
