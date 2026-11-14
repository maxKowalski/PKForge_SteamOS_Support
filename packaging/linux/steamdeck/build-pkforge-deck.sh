#!/bin/bash
# Build PKForge (Linux port) from source on a Steam Deck, inside a container.
# The same container build was verified on x86_64 Linux (2026-11-14); not yet run on a Deck itself.
# Steps follow the fork's README. Run in Desktop Mode (Konsole). Needs ~5 GB free and internet.
# Usage: bash build-pkforge-deck.sh [git-url] [branch]
set -euo pipefail

REPO="${1:-https://github.com/maxKowalski/PKForge_SteamOS_Support}"
BRANCH="${2:-claude/pkforge-steamos-support-m2jali}"
WORK="$HOME/pkforge-build"
ENGINE="$(command -v podman || command -v docker || true)"
[ -n "$ENGINE" ] || { echo "Need podman or docker (SteamOS: install distrobox/podman)"; exit 1; }

mkdir -p "$WORK" && cd "$WORK"
[ -d src ] || git clone --recurse-submodules -b "$BRANCH" "$REPO" src
cd src
git submodule update --init --recursive
git rev-parse HEAD > "$WORK/BUILT_FROM_COMMIT.txt"

# Build inside the .NET 11 preview SDK image; nothing is installed on SteamOS itself.
"$ENGINE" run --rm -v "$PWD":/src:Z -w /src mcr.microsoft.com/dotnet/sdk:11.0 bash -c '
  set -e
  dotnet workload install maui-tizen
  dotnet publish src/PKForge.App/PKForge.App.csproj -p:PKForgeDesktop=true \
    -c Release -r linux-x64 --self-contained -o packaging/linux/pkforge-linux-x64
  chmod +x packaging/linux/pkforge-linux-x64/pkforge.sh packaging/linux/pkforge-linux-x64/PKForge.App
'

# Package. flatpak-builder is not preinstalled on SteamOS: install it with
#   flatpak install --user flathub org.flatpak.Builder
cd packaging/linux
tar -czf "$WORK/pkforge-linux-x64.tar.gz" pkforge-linux-x64
echo "Folder build ready: $PWD/pkforge-linux-x64/pkforge.sh"
echo "Tarball: $WORK/pkforge-linux-x64.tar.gz"
echo "Flatpak (optional):"
echo "  flatpak run --command=flatpak-builder org.flatpak.Builder --user --install --install-deps-from=flathub --force-clean build-dir org.pkforge.app.yml"
echo "Then lock it down:  flatpak override --user --nonetwork org.pkforge.app"
