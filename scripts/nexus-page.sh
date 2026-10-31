#!/bin/bash
# Push the Nexus Mods page text from this repo: summary and description. The
# release workflow runs it on every tag, so the page is generated from source
# instead of edited in a web form and then forgotten.
#
#   NEXUSMODS_API_KEY=<key> NEXUS_MOD_ID=103 ./scripts/nexus-page.sh
#
# The v3 API has no endpoint for images or tags: the header, gallery and tags
# are set by hand on the mod page (assets/logo.png is the image used there).
set -euo pipefail

GAME=moonsweepers
REPO="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
API=https://api.nexusmods.com/v3

: "${NEXUSMODS_API_KEY:?set NEXUSMODS_API_KEY}"
: "${NEXUS_MOD_ID:?set NEXUS_MOD_ID to the number in the mod page URL}"

SUMMARY="Quality-of-life additions for Moon Sweepers, sold as upgrades in the game's own upgrade tree. Starts with the Landmark Scanner, which marks landmark flags and lunar sites you have not raised yet on the compass, the station hologram and the map."

WORK="$(mktemp -d)"
trap 'rm -rf "$WORK"' EXIT

# curl -f hides the API's error body, so status and body are checked here.
call() {
  local code
  code=$(curl -sS -o "$WORK/out.json" -w '%{http_code}' -H "apikey: $NEXUSMODS_API_KEY" "$@")
  if [ "$code" -ge 300 ]; then
    echo "Nexus answered HTTP $code:" >&2
    cat "$WORK/out.json" >&2
    exit 1
  fi
}

# The edit endpoint takes the mod's unique id, not the number in the page URL.
call "$API/games/$GAME/mods/$NEXUS_MOD_ID"
UID_=$(python3 -c 'import json,sys;print(json.load(open(sys.argv[1]))["data"]["id"])' "$WORK/out.json")

python3 - "$SUMMARY" "$REPO/assets/nexus-description.bbcode" > "$WORK/body.json" <<'PY'
import json, sys
print(json.dumps({"summary": sys.argv[1], "description": open(sys.argv[2]).read().strip()}))
PY

echo "updating the page text on mod $NEXUS_MOD_ID"
call -X PATCH "$API/mods/$UID_" -H 'Content-Type: application/json' --data @"$WORK/body.json"
echo "done"
