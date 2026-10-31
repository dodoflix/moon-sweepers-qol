using System;
using System.Collections.Generic;
using System.Reflection;
using MoonCleaners;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MoonSweepersQol;

// Landmark flags and sites on the map screen opened from the station hologram, as clones of the
// game's own flag and site images in its marker layer.
static class MapMarks
{
    delegate bool Project(MoonWorld world, Vector3 direction, out Vector2 screen);

    const float MatchRadius = 20f;

    static readonly List<Image> flagClones = new List<Image>();
    static readonly List<Image> siteClones = new List<Image>();
    static MoonMapScreen calibratedFor;
    static Project project;
    static Project onNearSide;
    static Vector2 shift;
    static float firstTry;
    static float nextTry;

    public static void LateUpdate()
    {
        int flags = 0, sites = 0;
        if (Landmarks.Show && Upgrades.ScannerOn && Game.MapOpen && Calibrate())
            Place(out flags, out sites);
        Hide(flagClones, flags);
        Hide(siteClones, sites);
    }

    // Index of the unraised landmark under a screen point, or -1. The game's own FlagUnder only knows raised
    // ones, so patching this in lets its hover and click handling work on ours.
    internal static int Hit(Vector2 screen)
    {
        if (project == null || calibratedFor != Game.Map || !Landmarks.Show || !Upgrades.ScannerOn) return -1;
        int best = -1;
        float bestDistance = MoonMapScreen.FlagPick;
        for (int i = 0; i < Landmarks.Marks.Count; i++)
        {
            var m = Landmarks.Marks[i];
            if (m.Raised || !onNearSide(Game.World, m.Dir, out _) || !project(Game.World, m.Dir, out var at)) continue;
            float d = (at + shift - screen).magnitude;
            if (d < bestDistance) { bestDistance = d; best = i; }
        }
        return best;
    }

    // The game positions its marks in anchor space: anchoredPosition = screen - anchor * layer size.
    static Vector2 ToScreen(RectTransform rt) =>
        rt.anchoredPosition + Vector2.Scale(rt.anchorMin, LayerSize(rt));

    // Il2Cpp objects can't be cast with (RectTransform); fetch the component instead.
    static Vector2 LayerSize(RectTransform rt) => rt.parent.GetComponent<RectTransform>().rect.size;

    // Projected and ScreenOf share a signature; use whichever lands on the game's own flags, and
    // keep the average offset between them. Without any game flags to compare, assume Projected.
    static bool Calibrate()
    {
        var map = Game.Map;
        if (calibratedFor == map) return project != null;
        if (firstTry == 0f) firstTry = Time.unscaledTime;
        if (Time.unscaledTime < nextTry) return false;
        nextTry = Time.unscaledTime + 0.5f;

        const BindingFlags any = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        var projected = (Project)Delegate.CreateDelegate(typeof(Project), map, typeof(MoonMapScreen).GetMethod("Projected", any));
        var screenOf = (Project)Delegate.CreateDelegate(typeof(Project), map, typeof(MoonMapScreen).GetMethod("ScreenOf", any));

        var targets = new List<Vector2>();
        foreach (var img in map.flagMarks.ToArray()) if (img.gameObject.activeSelf) targets.Add(ToScreen(img.rectTransform));
        foreach (var img in map.siteMarks.ToArray()) if (img.gameObject.activeSelf) targets.Add(ToScreen(img.rectTransform));

        var log = Plugin.Instance.Log;
        if (targets.Count == 0)
        {
            if (Time.unscaledTime - firstTry < 1.5f) return false;
            Accept(map, projected, Vector2.zero, screenOf);
            log.LogInfo("map calibration: no game flags to compare, assuming Projected");
            return true;
        }

        Fit(projected, targets, out int pj, out var pjShift);
        Fit(screenOf, targets, out int so, out var soShift);
        log.LogInfo($"map calibration: {targets.Count} game markers, Projected matches {pj} shift {pjShift}, ScreenOf matches {so} shift {soShift}");
        if (Mathf.Max(pj, so) == 0)
        {
            foreach (var t in targets) log.LogInfo($"map game marker at screen {t}");
            for (int i = 0; i < Landmarks.Marks.Count && i < 12; i++)
                log.LogInfo($"map mark {i} raised {Landmarks.Marks[i].Raised}: Projected {(projected(Game.World, Landmarks.Marks[i].Dir, out var a) ? a.ToString() : "false")} ScreenOf {(screenOf(Game.World, Landmarks.Marks[i].Dir, out var b) ? b.ToString() : "false")}");
            return false;
        }
        if (pj >= so) Accept(map, projected, pjShift, screenOf); else Accept(map, screenOf, soShift, screenOf);
        return true;
    }

    // ScreenOf reports false for the far side of the moon, where Projected still returns a position.
    static void Accept(MoonMapScreen map, Project fn, Vector2 offset, Project nearSide)
    {
        calibratedFor = map;
        project = fn;
        shift = offset;
        onNearSide = nearSide;
    }

    static void Fit(Project fn, List<Vector2> targets, out int hits, out Vector2 offset)
    {
        hits = 0;
        offset = Vector2.zero;
        foreach (var t in targets)
        {
            float best = MatchRadius;
            Vector2 delta = default;
            bool found = false;
            foreach (var m in Landmarks.Marks)
                if (fn(Game.World, m.Dir, out var s) && (t - s).magnitude < best) { best = (t - s).magnitude; delta = t - s; found = true; }
            if (found) { hits++; offset += delta; }
        }
        if (hits > 0) offset /= hits;
    }

    static void Place(out int flags, out int sites)
    {
        flags = sites = 0;
        var map = Game.Map;
        var flagTemplate = map.flagMarks.ToArray()[0];
        var siteTemplate = map.siteMarks.ToArray()[0];
        int nearest = Landmarks.Nearest();
        for (int k = 0; k < Landmarks.Marks.Count; k++)
        {
            var m = Landmarks.Marks[k];
            if (m.Raised) continue;
            if (!onNearSide(Game.World, m.Dir, out _) || !project(Game.World, m.Dir, out var at)) continue;
            var clones = m.Site ? siteClones : flagClones;
            int i = m.Site ? sites++ : flags++;
            if (i >= clones.Count) clones.Add(null);
            var template = m.Site ? siteTemplate : flagTemplate;
            if (clones[i] == null)
            {
                var go = Object.Instantiate(template.gameObject, template.transform.parent);
                go.name = (m.Site ? "Landmark site " : "Landmark flag ") + i;
                clones[i] = go.GetComponent<Image>();
            }
            var img = clones[i];
            img.gameObject.SetActive(true);
            img.color = k == nearest ? MoonServiceCompass.Amber : MoonServiceCompass.Mint;
            img.rectTransform.localScale = Vector3.one * (k == nearest ? 1.6f : 1f);
            img.rectTransform.anchoredPosition = at + shift - Vector2.Scale(template.rectTransform.anchorMin, LayerSize(template.rectTransform));
        }
    }

    static void Hide(List<Image> list, int used)
    {
        for (int i = used; i < list.Count; i++)
            if (list[i] != null) list[i].gameObject.SetActive(false);
    }
}
