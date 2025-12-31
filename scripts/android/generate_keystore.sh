#!/usr/bin/env bash
set -euo pipefail

# Generates a release keystore for Android signing.
# IMPORTANT:
# - Back up the generated keystore securely (you cannot rotate it without losing update ability).
# - Do NOT commit the keystore to git.

KEYSTORE_PATH=${1:-"./angry_animals.keystore"}
ALIAS_NAME=${2:-"angry_animals_key"}
VALIDITY_DAYS=${3:-10000}

if ! command -v keytool >/dev/null 2>&1; then
  echo "keytool not found. Install a JDK (e.g. Temurin/OpenJDK) and try again." >&2
  exit 1
fi

echo "Generating keystore at: ${KEYSTORE_PATH}"

keytool -genkeypair -v \
  -keystore "${KEYSTORE_PATH}" \
  -keyalg RSA \
  -keysize 2048 \
  -validity "${VALIDITY_DAYS}" \
  -alias "${ALIAS_NAME}"

echo "Done. Configure this keystore in Godot → Project → Export… → Android preset → Signing."
