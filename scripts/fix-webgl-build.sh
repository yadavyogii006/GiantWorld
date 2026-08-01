#!/bin/bash
# Decompress .gz Unity WebGL files and patch index.html for itch.io / simple servers.
set -euo pipefail

ROOT="${1:-.}"
BUILD="$ROOT/Build"
HTML="$ROOT/index.html"

if [ ! -d "$BUILD" ]; then
  echo "Build folder not found: $BUILD" >&2
  exit 1
fi

shopt -s nullglob
files=("$BUILD"/*.gz)
if [ ${#files[@]} -eq 0 ]; then
  echo "No .gz files — build already uncompressed."
  exit 0
fi

echo "Decompressing ${#files[@]} file(s)..."
for f in "${files[@]}"; do
  gunzip -f "$f"
done

if [ -f "$HTML" ]; then
  sed -i.bak \
    -e 's/WebGL\.data\.gz/WebGL.data/g' \
    -e 's/WebGL\.framework\.js\.gz/WebGL.framework.js/g' \
    -e 's/WebGL\.wasm\.gz/WebGL.wasm/g' \
    "$HTML"
  rm -f "$HTML.bak"
  echo "Patched index.html (removed .gz references)."
fi

echo "Done. Safe to upload to itch.io or use: python3 -m http.server 8080"
