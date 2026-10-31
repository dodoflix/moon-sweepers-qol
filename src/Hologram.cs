using System.Collections.Generic;
using MoonCleaners;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MoonSweepersQol;

// Station map hologram: a miniature of the moon whose local frame is the moon's own frame.
// Landmarks are small dots parented to the miniature, so they spin and sit with it.
static class Hologram
{
    const float Offset = 0.02f;
    const float Dot = 0.03f;
    const float NearestDot = 0.06f;

    static readonly Color Raised = new Color(0.949f, 0.929f, 0.894f, 0.4f);
    static readonly Dictionary<int, List<Transform>> dots = new Dictionary<int, List<Transform>>();
    static Material material;

    public static void Scan()
    {
        var marks = Landmarks.Marks;
        var globes = Object.FindObjectsByType<MoonCollectorGlobe>(FindObjectsSortMode.None);
        if (globes.Length == 0) return;

        bool on = Upgrades.ScannerOn && Landmarks.Show;
        int nearest = on ? Landmarks.Nearest() : -1;
        foreach (var g in globes)
        {
            if (g.displayMesh == null) continue;
            var parent = g.displayMesh.transform;
            int id = g.GetInstanceID();
            if (!dots.TryGetValue(id, out var list)) dots[id] = list = new List<Transform>();

            for (int i = 0; i < list.Count || i < marks.Count; i++)
            {
                if (i >= marks.Count || !on) { if (i < list.Count && list[i] != null) list[i].gameObject.SetActive(false); continue; }
                if (i >= list.Count) list.Add(null);
                if (list[i] == null) list[i] = NewDot(parent, i, marks[i].Site);
                var dot = list[i];
                dot.localPosition = marks[i].Dir * (g.radius + Offset);
                dot.localScale = Vector3.one * (i == nearest ? NearestDot : Dot);
                dot.GetComponent<Renderer>().material.color = i == nearest ? MoonServiceCompass.Amber : marks[i].Raised ? Raised : MoonServiceCompass.Mint;
                dot.gameObject.SetActive(true);
            }
        }
    }

    // The dots are not occluded by the miniature, so hide the ones on the far side from the viewer.
    public static void LateUpdate()
    {
        var cam = Camera.main;
        if (cam == null) return;
        foreach (var list in dots.Values)
            foreach (var dot in list)
            {
                if (dot == null || !dot.gameObject.activeSelf) continue;
                var normal = (dot.position - dot.parent.position).normalized;
                dot.GetComponent<Renderer>().enabled = Vector3.Dot(normal, cam.transform.position - dot.position) > 0f;
            }
    }

    // Flags are round dots, sites are cubes.
    static Transform NewDot(Transform parent, int index, bool site)
    {
        var go = GameObject.CreatePrimitive(site ? PrimitiveType.Cube : PrimitiveType.Sphere);
        Object.Destroy(go.GetComponent<Collider>());
        go.name = "Landmark dot " + index;
        go.layer = parent.gameObject.layer;
        go.transform.SetParent(parent, false);
        material ??= new Material(Graphic.defaultGraphicMaterial);
        go.GetComponent<Renderer>().sharedMaterial = material;
        return go.transform;
    }
}
