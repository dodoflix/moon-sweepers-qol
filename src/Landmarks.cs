using System.Collections.Generic;
using MoonCleaners;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MoonSweepersQol;

// Marks landmarks that are not raised yet on the compass, using clones of the game's own
// compass icons and range labels so they look like the game's.
static class Landmarks
{
    internal struct Mark
    {
        public Vector3 Pos;
        public Vector3 Dir;
        public bool Raised;
        public bool Site;
    }

    struct Shown
    {
        public float Distance;
        public float X;
        public bool Edge;
        public bool Site;
    }

    const int MaxOnCompass = 5;

    // The ribbon fades out toward +-HalfTape; the nearest landmark pins to the side it lies on, short of the fade.
    static readonly float EdgeX = MoonServiceCompass.HalfTape * 0.85f;

    public static bool Show = true;

    static readonly List<Mark> marks = new List<Mark>();
    internal static List<Mark> Marks => marks;
    static readonly List<MoonCompassServiceIcon> pool = new List<MoonCompassServiceIcon>();
    static readonly List<TMP_Text> labels = new List<TMP_Text>();
    static readonly List<Shown> shown = new List<Shown>();

    public static void Scan()
    {
        marks.Clear();
        var world = Game.World;
        if (world == null) return;

        var origin = world.transform.position;
        int n = world.LandmarkCount;
        for (int i = 0; i < n; i++)
        {
            var dir = world.LandmarkDirection(i).normalized;
            marks.Add(new Mark { Pos = origin + dir * world.MeshGround(dir), Dir = dir, Raised = world.IsLandmarkRaised(i), Site = world.Landmark(i).IsSite });
        }
    }

    // Index in Marks of the unraised landmark closest to the player, or -1.
    internal static int Nearest()
    {
        if (Game.Walker == null) return -1;
        var from = Game.Walker.transform.position;
        int best = -1;
        float bestDistance = float.MaxValue;
        for (int i = 0; i < marks.Count; i++)
        {
            if (marks[i].Raised) continue;
            float d = (marks[i].Pos - from).sqrMagnitude;
            if (d < bestDistance) { bestDistance = d; best = i; }
        }
        return best;
    }

    public static void LateUpdate()
    {
        var compass = Game.Compass;
        int count = 0;
        if (Show && Upgrades.ScannerOn && marks.Count > 0 && !Game.MenuOpen && !Game.MapOpen && compass != null
            && compass.icons != null && compass.icons.Length > 2 && compass.viewport.gameObject.activeInHierarchy)
            count = PlaceCompassMarkers();
        for (int i = count; i < pool.Count; i++)
        {
            if (pool[i] != null) pool[i].gameObject.SetActive(false);
            if (labels[i] != null) labels[i].gameObject.SetActive(false);
        }
    }

    // The compass places each marker at anchoredPosition.x == TapePosition, Period px per 360 degrees.
    // Clones of one of its own icons and range labels sit on the same ribbon, so they get its mask, fade and look.
    static int PlaceCompassMarkers()
    {
        var cam = Camera.main;
        if (cam == null) return 0;
        var compass = Game.Compass;
        var from = Game.Walker != null ? Game.Walker.transform.position : cam.transform.position;
        var up = (from - Game.World.transform.position).normalized;
        var fwd = Vector3.ProjectOnPlane(cam.transform.forward, up);
        if (fwd.sqrMagnitude < 1e-4f) return 0;

        // Everything in view, plus the nearest landmark even when it is off to the side or behind.
        shown.Clear();
        var all = new List<Shown>();
        foreach (var m in marks)
        {
            if (m.Raised) continue;
            var to = Vector3.ProjectOnPlane(m.Pos - from, up);
            if (to.sqrMagnitude < 1f) continue;
            float angle = Vector3.SignedAngle(fwd, to, up);
            float distance = Vector3.Distance(from, m.Pos);
            bool inView = Mathf.Abs(angle) <= MoonServiceCompass.HalfField;
            all.Add(new Shown { Distance = distance, X = inView ? angle * MoonServiceCompass.Period / 360f : Mathf.Sign(angle) * EdgeX, Edge = !inView, Site = m.Site });
        }
        all.Sort((a, b) => a.Distance.CompareTo(b.Distance));
        for (int i = 0; i < all.Count; i++)
            if (i == 0 || !all[i].Edge)
                shown.Add(all[i]);
        if (shown.Count > MaxOnCompass) shown.RemoveRange(MaxOnCompass, shown.Count - MaxOnCompass);

        var template = compass.icons[2];
        var labelTemplate = compass.ranges[2];
        for (int i = 0; i < shown.Count; i++)
        {
            if (i >= pool.Count) { pool.Add(null); labels.Add(null); }
            if (pool[i] == null)
            {
                var clone = Object.Instantiate(template.gameObject, template.transform.parent);
                clone.name = "Landmark marker " + i;
                pool[i] = clone.GetComponent<MoonCompassServiceIcon>();
            }
            if (labels[i] == null)
            {
                var clone = Object.Instantiate(labelTemplate.gameObject, template.transform.parent);
                clone.name = "Landmark range " + i;
                labels[i] = clone.GetComponent<TMP_Text>();
            }

            var icon = pool[i];
            icon.gameObject.SetActive(true);
            // The nearest landmark is always shown[0]: waypoint pin shape in amber. Flags and sites get the
            // shapes of the compass's other two service kinds.
            var kind = i == 0 ? MoonServiceBeacon.ServiceKind.Waypoint
                : shown[i].Site ? MoonServiceBeacon.ServiceKind.Equipment : MoonServiceBeacon.ServiceKind.Other;
            if (icon.kind != kind) { icon.kind = kind; icon.SetVerticesDirty(); }
            icon.color = i == 0 ? MoonServiceCompass.Amber : MoonServiceCompass.Mint;
            icon.TapePosition = shown[i].X;
            var rt = icon.rectTransform;
            float y = rt.anchoredPosition.y == 0f ? -23f : rt.anchoredPosition.y;
            rt.anchoredPosition = new Vector2(shown[i].X, y);

            var label = labels[i];
            label.gameObject.SetActive(true);
            label.text = MoonWaypoint.Distance(shown[i].Distance);
            label.rectTransform.anchoredPosition = new Vector2(shown[i].X, y + MoonServiceCompass.RangeY);
            var tint = label.color;
            tint.a = shown[i].Edge ? 1f : MoonCompassRibbon.EdgeFade(shown[i].X);
            label.color = tint;
        }
        return shown.Count;
    }
}
