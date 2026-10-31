#!/bin/bash
# Push the Nexus Mods page text from this repo: assets/nexus-summary.txt (the short
# description, 350 characters at most) and assets/nexus-description.bbcode. The
# release workflow runs it on every tag, so the page is generated from source
# instead of edited in a web form and then forgotten.
#
#   NEXUSMODS_API_KEY=<key> NEXUS_MOD_ID=<unique id> ./scripts/nexus-page.sh
#
# The v3 API has no endpoint for images or tags: the header, gallery and tags
# are set by hand on the mod page (assets/logo.png is the image used there).
set -euo pipefail

REPO="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
API=https://api.nexusmods.com/v3

: "${NEXUSMODS_API_KEY:?set NEXUSMODS_API_KEY}"
: "${NEXUS_MOD_ID:?set NEXUS_MOD_ID to the unique mod id (page Files tab, Advanced)}"

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

python3 - "$REPO/assets/nexus-summary.txt" "$REPO/assets/nexus-description.bbcode" > "$WORK/body.json" <<'PY'
import json, sys
print(json.dumps({"summary": open(sys.argv[1]).read().strip(), "description": open(sys.argv[2]).read().strip()}))
PY

echo "updating the page text on mod $NEXUS_MOD_ID"
call -X PATCH "$API/mods/$NEXUS_MOD_ID" -H 'Content-Type: application/json' --data @"$WORK/body.json"
echo "done"
