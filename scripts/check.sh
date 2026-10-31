#!/bin/bash
# Compile the plugin. Uses your game's generated assemblies when MS_GAME_DIR (or the default
# Steam path) has them, otherwise the signature-only copies in refs/, which is what CI does.
set -euo pipefail

REPO="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
GAME="${MS_GAME_DIR:-$HOME/.local/share/Steam/steamapps/common/Moon Sweepers}"

dotnet build "$REPO" -c Release -p:GameDir="$GAME" -o "$REPO/bin/out" --nologo -v q
