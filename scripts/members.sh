#!/bin/bash
# List a game type's members with their types, from the interop assemblies BepInEx generated.
#   ./scripts/members.sh MoonWorld 'Belt|Conveyor'
#   ./scripts/members.sh Glyph .            # an enum lists its values
#   ./scripts/members.sh '*' 'Splitter'     # search every type
set -euo pipefail

REPO="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
GAME="${MS_GAME_DIR:-$HOME/.local/share/Steam/steamapps/common/Moon Sweepers}"
INTEROP="$GAME/BepInEx/interop"

[ -d "$INTEROP" ] || { echo "BepInEx interop not found in: $GAME (run the game once with BepInEx and the patcher)" >&2
                       echo "Set MS_GAME_DIR to override." >&2; exit 1; }

exec dotnet run --project "$REPO/tools/members" -c Release -v q -- "$INTEROP" "$@"
