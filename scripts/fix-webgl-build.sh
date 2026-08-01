#!/bin/bash
# Prepare Unity WebGL build for itch.io / python -m http.server (no Content-Encoding headers).
set -euo pipefail

find_webgl_root() {
  local candidate
  for candidate in \
    "build/WebGL/WebGL" \
    "build/WebGL" \
    "WebGL"; do
    if [ -f "$candidate/index.html" ] && [ -d "$candidate/Build" ]; then
      echo "$candidate"
      return 0
    fi
  done

  candidate="$(find build -name index.html 2>/dev/null | while read -r html; do
    dir="$(dirname "$html")"
    if [ -d "$dir/Build" ]; then
      echo "$dir"
      break
    fi
  done)"
  if [ -n "$candidate" ]; then
    echo "$candidate"
    return 0
  fi
  return 1
}

ROOT="${1:-}"
if [ -z "$ROOT" ]; then
  ROOT="$(find_webgl_root)" || {
    echo "Could not find WebGL build (index.html + Build/). Searched under build/." >&2
    find build -maxdepth 4 -type f 2>/dev/null | head -30 || true
    exit 1
  }
fi

BUILD="$ROOT/Build"
HTML="$ROOT/index.html"

if [ ! -d "$BUILD" ]; then
  echo "Build folder not found: $BUILD" >&2
  ls -la "$ROOT" 2>/dev/null || true
  exit 1
fi

shopt -s nullglob

decompress_gz() {
  local files=("$BUILD"/*.gz)
  [ ${#files[@]} -eq 0 ] && return 0
  echo "Decompressing ${#files[@]} .gz file(s)..."
  for f in "${files[@]}"; do
    gunzip -f "$f"
  done
}

decompress_unityweb() {
  local files=("$BUILD"/*.unityweb)
  [ ${#files[@]} -eq 0 ] && return 0

  if ! command -v brotli >/dev/null 2>&1; then
    echo "brotli CLI not found; install brotli to decompress .unityweb files." >&2
    exit 1
  fi

  echo "Decompressing ${#files[@]} .unityweb file(s)..."
  for f in "${files[@]}"; do
    out="${f%.unityweb}"
    brotli -d -f "$f" -o "$out"
    rm -f "$f"
  done
}

patch_index_html() {
  [ -f "$HTML" ] || return 0
  sed -i.bak \
    -e 's/WebGL\.data\.gz/WebGL.data/g' \
    -e 's/WebGL\.framework\.js\.gz/WebGL.framework.js/g' \
    -e 's/WebGL\.wasm\.gz/WebGL.wasm/g' \
    -e 's/WebGL\.data\.unityweb/WebGL.data/g' \
    -e 's/WebGL\.framework\.js\.unityweb/WebGL.framework.js/g' \
    -e 's/WebGL\.wasm\.unityweb/WebGL.wasm/g' \
    -e 's/WebGL\.data\.br/WebGL.data/g' \
    -e 's/WebGL\.framework\.js\.br/WebGL.framework.js/g' \
    -e 's/WebGL\.wasm\.br/WebGL.wasm/g' \
    "$HTML"
  rm -f "$HTML.bak"
  echo "Patched index.html for uncompressed Build/ files."
}

decompress_gz
decompress_unityweb
patch_index_html

remaining=("$BUILD"/*.gz "$BUILD"/*.unityweb "$BUILD"/*.br)
if [ ${#remaining[@]} -gt 0 ] && [ -e "${remaining[0]}" ]; then
  echo "Warning: compressed files remain in Build/:" >&2
  ls -la "$BUILD" >&2
  exit 1
fi

echo "WebGL build ready at: $ROOT"
echo "Done. Safe to upload to itch.io or use: python3 -m http.server 8080"
