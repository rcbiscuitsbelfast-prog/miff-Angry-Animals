#!/bin/bash
set -e

echo "Restoring dependencies..."
dotnet restore

echo "Building project..."
dotnet build

echo "Build complete."
