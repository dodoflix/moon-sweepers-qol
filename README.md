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

**Belt Drive** (Logistics, needs Rapid Transfer) speeds up conveyor belts, up to +60%.
**Machine Overdrive** (Logistics, needs Belt Drive) speeds up stone presses and pressure
chambers, up to +75%. They draw proportionally more power, so energy per product stays the same.

## Install

1. Install BepInEx for the game with [MoonSweepers BepInEx Patcher](https://www.nexusmods.com/moonsweepers/mods/9).
2. Extract the release zip over the game folder, or build it yourself:

```sh
./scripts/install.sh
```

Close the game first. Set `MS_GAME_DIR` if it is not in the default Steam library.

`BepInEx/config/dodo.moonsweepers.qol.cfg` has `RequireUpgrade`; set it to `false` to
have everything on without buying it. `OverdrivePower` in the same file turns off the extra
power draw.

## Releasing

Tagging is the trigger:

```sh
./scripts/check.sh
git tag -a v1.0.0 -m "what changed"   # must match Version in src/Plugin.cs
git push origin v1.0.0
```

That builds the zip on a runner, attaches it to a GitHub release, syncs the Nexus page text
(`assets/nexus-description.bbcode`) and uploads the zip to Nexus Mods. The tag message becomes
the changelog. The **Store page** workflow syncs the text alone, without a release. Needs the `NEXUSMODS_API_KEY` secret and the
`NEXUS_FILE_ID` and `NEXUS_MOD_ID` variables (see [NOTES.md](NOTES.md#nexus)).

## Development

`scripts/check.sh` compiles against the interop assemblies BepInEx generated from your copy
of the game, or against the signature-only copies in `refs/` when there is no game, which is
how CI builds. Run `scripts/refs.sh` after a game update to refresh them.

To add a feature, write a static class beside `Landmarks.cs` and call it from `Qol` in
`src/Plugin.cs`. Sell it with a node in `src/Upgrades.cs`.

[NOTES.md](NOTES.md) is the long version: how it works and the traps that cost a day each.

## License

MIT, see [LICENSE](LICENSE).
