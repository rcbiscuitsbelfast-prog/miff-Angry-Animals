#!/usr/bin/env bash
set -euo pipefail

# Exports an iOS Xcode project using Godot CLI.
# NOTE: Final signing + IPA creation happens in Xcode on macOS.
# Prereqs:
# - macOS with Xcode
# - Godot 4.4 installed
# - export_presets.cfg exists locally with an iOS preset configured

GODOT_BIN=${GODOT_BIN:-"godot"}
PRESET_NAME=${PRESET_NAME:-"iOS"}
OUT_PATH=${1:-"exports/ios/AngryAnimals"}

mkdir -p "$(dirname "${OUT_PATH}")"

"${GODOT_BIN}" --headless --path . --export-release "${PRESET_NAME}" "${OUT_PATH}"

echo "Exported Xcode project to: ${OUT_PATH}"
echo "Open the exported project in Xcode, configure signing, then Archive → Distribute."
