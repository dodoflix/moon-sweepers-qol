#!/bin/bash
# Compile against the game's generated interop assemblies. A runner has none of them
# (they are made from the player's own copy of the game), so this only runs locally.
set -euo pipefail

REPO="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
GAME="${MS_GAME_DIR:-$HOME/.local/share/Steam/steamapps/common/Moon Sweepers}"

[ -d "$GAME/BepInEx/interop" ] || { echo "BepInEx interop not found in: $GAME (run the game once with BepInEx and the patcher)" >&2
                                    echo "Set MS_GAME_DIR to override." >&2; exit 1; }

dotnet build "$REPO" -c Release -p:GameDir="$GAME" -o "$REPO/bin/out" --nologo -v q
