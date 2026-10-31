#!/bin/bash
# Regenerate refs/: signature-only reference assemblies of the BepInEx core libraries and the
# interop assemblies generated from your copy of the game. The release workflow compiles
# against these because a runner has no game. Run again after a game update that changes the
# types the plugin uses.
set -euo pipefail

REPO="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
GAME="${MS_GAME_DIR:-$HOME/.local/share/Steam/steamapps/common/Moon Sweepers}"
BEPINEX="$GAME/BepInEx"

[ -d "$BEPINEX/interop" ] || { echo "BepInEx interop not found in: $GAME (run the game once with BepInEx and the patcher)" >&2
                               echo "Set MS_GAME_DIR to override." >&2; exit 1; }

TOOLS="$(mktemp -d)"
trap 'rm -rf "$TOOLS"' EXIT
dotnet tool install JetBrains.Refasmer.CliTool --tool-path "$TOOLS" >/dev/null

CORE="BepInEx.Core BepInEx.Unity.IL2CPP 0Harmony Il2CppInterop.Runtime Il2CppInterop.Common"
INTEROP="Assembly-CSharp Il2Cppmscorlib UnityEngine UnityEngine.CoreModule UnityEngine.IMGUIModule \
         UnityEngine.TextRenderingModule UnityEngine.UI UnityEngine.UIModule UnityEngine.PhysicsModule Unity.TextMeshPro"

rm -rf "$REPO/refs"
for set in core interop; do
  mkdir -p "$REPO/refs/$set"
  names=$([ "$set" = core ] && echo "$CORE" || echo "$INTEROP")
  for name in $names; do
    # Some assemblies (UnityEngine.CoreModule) fail with "Read out of bounds" when private members are dropped.
    "$TOOLS/refasmer" -q -O "$REPO/refs/$set" --omit-non-api-members=true "$BEPINEX/$set/$name.dll" ||
      "$TOOLS/refasmer" -q -O "$REPO/refs/$set" --omit-non-api-members=false "$BEPINEX/$set/$name.dll"
  done
done
du -sh "$REPO/refs"
