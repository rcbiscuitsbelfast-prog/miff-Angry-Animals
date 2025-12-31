#!/usr/bin/env bash
set -euo pipefail

# Exports a signed Android App Bundle using Godot CLI.
# Prereqs:
# - Godot 4.4 installed and available as `godot` or `godot4`.
# - export_presets.cfg exists locally with an Android preset configured for Release + signing.

GODOT_BIN=${GODOT_BIN:-"godot"}
PRESET_NAME=${PRESET_NAME:-"Android"}
OUT_PATH=${1:-"exports/android/AngryAnimals.aab"}

mkdir -p "$(dirname "${OUT_PATH}")"

"${GODOT_BIN}" --headless --path . --export-release "${PRESET_NAME}" "${OUT_PATH}"

echo "Exported: ${OUT_PATH}"
