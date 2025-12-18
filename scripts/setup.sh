#!/bin/bash
set -e

# Update and install dependencies if not present
if ! command -v dotnet &> /dev/null; then
    sudo apt-get update
    sudo apt-get install -y wget unzip dotnet-sdk-8.0
fi

# Godot Version
GODOT_VERSION="4.4-dev4"
GODOT_BASE_URL="https://github.com/godotengine/godot-builds/releases/download/${GODOT_VERSION}"
GODOT_ZIP="Godot_v${GODOT_VERSION}_mono_linux_x86_64.zip"
GODOT_DIR="Godot_v${GODOT_VERSION}_mono_linux_x86_64"
GODOT_EXE="Godot_v${GODOT_VERSION}_mono_linux.x86_64"
TEMPLATES_TPZ="Godot_v${GODOT_VERSION}_mono_export_templates.tpz"

echo "Downloading Godot ${GODOT_VERSION}..."
wget -q "${GODOT_BASE_URL}/${GODOT_ZIP}"
unzip -q "${GODOT_ZIP}"

echo "Installing Godot..."
sudo mkdir -p /usr/local/share/godot
if [ -d "/usr/local/share/godot/${GODOT_DIR}" ]; then
    sudo rm -rf "/usr/local/share/godot/${GODOT_DIR}"
fi
sudo mv "${GODOT_DIR}" /usr/local/share/godot/
sudo ln -sf "/usr/local/share/godot/${GODOT_DIR}/${GODOT_EXE}" /usr/local/bin/godot

rm "${GODOT_ZIP}"

echo "Downloading Export Templates..."
wget -q "${GODOT_BASE_URL}/${TEMPLATES_TPZ}"

echo "Installing Export Templates..."
mkdir -p ~/.local/share/godot/export_templates
unzip -q "${TEMPLATES_TPZ}" -d ~/.local/share/godot/export_templates

# Determine version directory name
# Expecting 4.4.dev4.mono
VERSION_DIR=$(echo ${GODOT_VERSION} | sed 's/-/./').mono

if [ -d ~/.local/share/godot/export_templates/${VERSION_DIR} ]; then
    rm -rf ~/.local/share/godot/export_templates/${VERSION_DIR}
fi

mv ~/.local/share/godot/export_templates/templates ~/.local/share/godot/export_templates/${VERSION_DIR}
rm "${TEMPLATES_TPZ}"

echo "Setup complete."
godot --version
