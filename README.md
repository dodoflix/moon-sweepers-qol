# Moon Sweepers QoL

Quality-of-life additions for Moon Sweepers, as one BepInEx plugin. Everything is sold
as an upgrade in the game's own upgrade tree, so it is earned, not handed out.

## Features

**Landmark Scanner** (Suit branch, needs Servo Stride) marks the landmark flags and lunar
sites you have not raised yet:

- on the compass, in the game's own style, with distance. The nearest one always shows,
  pinned to the edge when it is behind you.
- on the station hologram, and on the map screen opened from it, with the game's own
  hover panel and click handling.

Raised ones are left to the game, which already draws them.

## Install

1. Install BepInEx for the game with [MoonSweepers BepInEx Patcher](https://www.nexusmods.com/moonsweepers/mods/9).
2. Extract the release zip over the game folder, or build it yourself:

```sh
./scripts/install.sh
```

Close the game first. Set `MS_GAME_DIR` if it is not in the default Steam library.

`BepInEx/config/dodo.moonsweepers.qol.cfg` has `RequireUpgrade`; set it to `false` to
have everything on without buying it.

## Releasing

```sh
git commit ...
./scripts/release.sh "what changed"
```

That builds the zip, tags the version in `src/Plugin.cs`, and publishes a GitHub release.
Publishing the release triggers the workflow that uploads it to Nexus Mods. It needs the
`NEXUSMODS_API_KEY` secret and the `NEXUS_FILE_ID` and `NEXUS_MOD_ID` variables
(see [NOTES.md](NOTES.md#nexus)).

## Development

`scripts/check.sh` compiles against the interop assemblies BepInEx generated from your copy
of the game, so it only runs where the game is installed.

To add a feature, write a static class beside `Landmarks.cs` and call it from `Qol` in
`src/Plugin.cs`. Sell it with a node in `src/Upgrades.cs`.

[NOTES.md](NOTES.md) is the long version: how it works and the traps that cost a day each.

## License

MIT, see [LICENSE](LICENSE).
