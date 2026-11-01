# Development and releasing

## Building

`dotnet build` compiles against the interop assemblies BepInEx generated from your copy of
the game, or against the signature-only copies in `refs/` when there is no game, which is
how CI builds. Run `dotnet msbuild tools/refs/refs.proj` after a game update to refresh them.

To add a feature, write a static class beside `Landmarks.cs` and call it from `Qol` in
`src/Plugin.cs`. Sell it with a node in `src/Upgrades.cs`.

`dotnet run --project tools/members -- <Type|*> <regex>` lists a game type's members from
the interop assemblies.

## Releasing

Tagging is the trigger:

```sh
dotnet build
git tag -a v1.0.0 -m "what changed"   # must match Version in src/Plugin.cs
git push origin v1.0.0
```

The Release workflow builds the zip on a runner, attaches it to a GitHub release, syncs the
Nexus page text (`assets/nexus-summary.txt` and `assets/nexus-description.bbcode`) and
uploads the zip to Nexus Mods. The tag message becomes the changelog. The **Store page**
workflow syncs the text alone, without a release.

Needs the `NEXUSMODS_API_KEY` secret and the `NEXUS_FILE_ID` and `NEXUS_MOD_ID` variables
(see [NOTES.md](../NOTES.md#nexus)).

`assets/make-logo.py` draws the logo and the Nexus banner.
