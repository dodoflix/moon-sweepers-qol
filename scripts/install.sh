#!/bin/bash
# Build and copy the plugin into BepInEx/plugins.
set -euo pipefail

REPO="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
GAME="${MS_GAME_DIR:-$HOME/.local/share/Steam/steamapps/common/Moon Sweepers}"

if pgrep -f 'MoonSweepers\.exe' >/dev/null; then
  echo "Moon Sweepers is running, close it first." >&2
  exit 1
fi

"$REPO/scripts/check.sh"
mkdir -p "$GAME/BepInEx/plugins"
cp "$REPO/bin/out/MoonSweepersQol.dll" "$GAME/BepInEx/plugins/"

echo
echo "installed to: $GAME/BepInEx/plugins/MoonSweepersQol.dll"
echo "launch the game, then check the log for [Moon Sweepers QoL] lines:"
echo "  grep 'Moon Sweepers QoL' \"$GAME/BepInEx/LogOutput.log\""
