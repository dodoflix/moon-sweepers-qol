# Notes

How the mod works, and what it cost to find out. The README is the short version.

## Getting BepInEx to run

- The game is Unity 6000.6 IL2CPP with metadata v108, which stock BepInEx's Cpp2IL rejects
  ("Unsupported metadata version found! We support 23-106, got 108"). The Nexus
  [MoonSweepers BepInEx Patcher](https://www.nexusmods.com/moonsweepers/mods/9) dumps the types from
  the live runtime and generates `BepInEx/interop/*.dll`. Swapping parser libraries by hand did not work.
- Under Proton the doorstop `winhttp.dll` proxy only loads with a Wine DLL override
  (`winhttp=native,builtin`), set either as a Steam launch option or in the prefix `user.reg`.
- `scripts/refs.sh` and a local `check.sh` build need `BepInEx/interop`, so the game has to have run once with the patcher.

## CI builds from refs/

A runner has no game, and the interop assemblies are generated from the player's own copy, so they cannot
be fetched. `refs/` holds signature-only copies (made with JetBrains Refasmer) of the BepInEx core libraries and
the interop assemblies the plugin uses; the csproj falls back to them when the game is not there. Refasmer fails
on `UnityEngine.CoreModule` with "Read out of bounds" unless private members are kept, which `refs.sh` handles.
Regenerate after a game update that changes a type the plugin uses.

## IL2CPP traps

- `Object.FindObjectOfType` throws `Method unstripping failed`; use `FindFirstObjectByType` and
  `FindObjectsByType`.
- `(RectTransform)transform` throws `InvalidCastException`; use `GetComponent<RectTransform>()`.
- Il2Cpp `List<T>` has no indexer; call `.ToArray()`.
- Static game constants (`MoonServiceCompass.Period`, `MoonMapScreen.NearZoom`) are read through the type.
- Injected components need `ClassInjector.RegisterTypeInIl2Cpp<T>()` and an `(IntPtr)` constructor.

## Drawing like the game

Nothing is drawn with IMGUI. Each marker is a clone of one of the game's own objects, so it
inherits its mask, fade, font and sprite:

- **Compass:** `compass.icons[2]` and `compass.ranges[2]`, cloned under the same `Surround ribbon`.
  An icon sits at `anchoredPosition.x == TapePosition`, `Period` pixels per 360 degrees, fading
  with `MoonCompassRibbon.EdgeFade`. Icon shape comes from `kind`; call `SetVerticesDirty()` after changing it.
  The labels' original parent is a per-service pin that the game moves and hides, so clone them under the ribbon.
- **Map screen:** `flagMarks[0]` and `siteMarks[0]`. The game places a mark at
  `anchoredPosition = screen - anchor * layer size`, so `Projected`/`ScreenOf` results need that conversion
  plus a few pixels of measured offset. `Projected` also answers for the far side of the moon; `ScreenOf` does not.
- **Hologram:** the miniature is drawn by a shader that only knows stations, a waypoint and up to
  `MaxVehicles` vehicles, so landmarks are small primitives parented to its mesh, hidden when facing away.

## Hover and click on the map

The map finds what is under the cursor with `FlagUnder(screen)`, which returns a landmark index
for raised ones only. A Harmony postfix falls back to our nearest unraised marker, so the game's own
info panel and click handling work on it. A second postfix on `ShowInfo` swaps "flag raised" for
"not raised yet". The `+$40.00 when cleaned` on that panel is the game's cleaning payout for the
sector under the cursor, not the landmark's reward.

## The upgrade

The tree stores levels per player by node id, so a new node with a unique id is bought and saved by
the game's own code. `Upgrades.Register` appends it to `MoonUpgradeTree.Nodes` and rebuilds `byId`.
It has to run when the plugin loads, before the upgrade terminal builds its UI; a node added
later is not shown.

## Nexus

The upload goes through [`Nexus-Mods/upload-action`](https://github.com/Nexus-Mods/upload-action) (v3 API, still in beta).

1. Upload the first version by hand on the mod page, so there is a file to add versions to.
2. Create an API key in your Nexus account settings and save it as the repository secret `NEXUSMODS_API_KEY`.
3. Save the file's ID (Files tab, "Advanced") as the repository variable `NEXUS_FILE_ID`, and the mod's
   ID as `NEXUS_MOD_ID`.

Until all three exist the workflow skips the upload instead of failing.
