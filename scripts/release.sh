#!/bin/bash
# Cut a release: build the zip, tag the version in src/Plugin.cs, push, and publish a GitHub
# release with the zip attached. The Nexus workflow takes it from there.
#
# usage: ./scripts/release.sh "what changed"
set -euo pipefail

REPO="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$REPO"

NOTES="${1:-}"
[ -n "$NOTES" ] || { echo "usage: $0 \"what changed\"" >&2; exit 1; }
[ -z "$(git status --porcelain)" ] || { echo "commit your changes first" >&2; exit 1; }

ZIP="$(./scripts/package.sh | tail -1)"
VERSION="$(basename "$ZIP" .zip)"
VERSION="${VERSION#MoonSweepersQol-}"
TAG="v$VERSION"

git tag -a "$TAG" -m "$NOTES"
git push origin HEAD "$TAG"
gh release create "$TAG" "$ZIP" --title "$TAG" --notes "$NOTES"
echo "released $TAG"
