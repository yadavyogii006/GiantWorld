#!/bin/bash
# Extract Unity serial from a .ulf license file (Personal or Pro).
# Usage: ./scripts/extract-unity-serial.sh ~/Downloads/Unity_v2022.3.50f1.ulf
#
# Add the output as GitHub secret UNITY_SERIAL (not UNITY_LICENSE).

set -euo pipefail

ULF="${1:?Usage: $0 path/to/Unity.ulf}"

if [ ! -f "$ULF" ]; then
  echo "File not found: $ULF" >&2
  exit 1
fi

SERIAL=$(grep -o 'DeveloperData[^>]*Value="[^"]*"' "$ULF" | head -1 | sed -E 's/.*Value="([^"]+)".*/\1/' | base64 --decode 2>/dev/null || true)

if [ -z "$SERIAL" ]; then
  echo "Could not extract serial. Open the .ulf in TextEdit and check it contains DeveloperData." >&2
  exit 1
fi

echo "$SERIAL"
echo ""
echo "Add this as GitHub secret: UNITY_SERIAL"
echo "Also required: UNITY_EMAIL and UNITY_PASSWORD"
echo "Remove secret UNITY_LICENSE if you added it (serial method works better in CI)"
