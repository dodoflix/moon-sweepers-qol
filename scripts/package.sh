#!/bin/bash
# Build the plugin and zip it the way Nexus and mod managers expect: laid out from the
# game folder, so extracting over a Moon Sweepers install puts the DLL in BepInEx/plugins.
#
# usage: ./scripts/package.sh   (writes dist/MoonSweepersQol-<version>.zip, prints its path)
set -euo pipefail

REPO="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

VERSION="$(sed -n 's/.*const string Version = "\(.*\)";/\1/p' "$REPO/src/Plugin.cs")"
[ -n "$VERSION" ] || { echo "no Version constant found in src/Plugin.cs" >&2; exit 1; }

"$REPO/scripts/check.sh"

STAGE="$(mktemp -d)"
trap 'rm -rf "$STAGE"' EXIT
mkdir -p "$STAGE/BepInEx/plugins" "$REPO/dist"
cp "$REPO/bin/out/MoonSweepersQol.dll" "$STAGE/BepInEx/plugins/"

ZIP="$REPO/dist/MoonSweepersQol-$VERSION.zip"
rm -f "$ZIP"
(cd "$STAGE" && python3 -m zipfile -c "$ZIP" BepInEx)
echo "$ZIP"
