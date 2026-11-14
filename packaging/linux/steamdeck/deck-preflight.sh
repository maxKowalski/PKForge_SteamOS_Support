#!/bin/bash
# Read-only preflight for PKForge on a Steam Deck. It changes nothing: no installs, no
# writes except the report file. Run it in Desktop Mode (Konsole):
#   bash deck-preflight.sh                       # system, emulators, saves, Flatpak
#   bash deck-preflight.sh ~/pkforge-linux-x64   # also check an unpacked tarball build
# The report is printed and saved to ~/pkforge-preflight-<date>.txt; paste it back.
set -u
BUILD_DIR="${1:-}"
REPORT="$HOME/pkforge-preflight-$(date +%F-%H%M).txt"
exec > >(tee "$REPORT") 2>&1

ok()   { printf '  [ok]   %s\n' "$*"; }
warn() { printf '  [WARN] %s\n' "$*"; }
info() { printf '  [info] %s\n' "$*"; }
section() { printf '\n== %s ==\n' "$*"; }

section "System"
. /etc/os-release 2>/dev/null
info "OS: ${PRETTY_NAME:-unknown} (${VERSION_ID:-?}, build ${BUILD_ID:-?})"
info "Kernel: $(uname -r), arch $(uname -m)"
[ "$(uname -m)" = x86_64 ] && ok "x86_64 (the build is linux-x64)" || warn "Not x86_64: the linux-x64 build will not run"
info "glibc: $(ldd --version 2>/dev/null | head -1)"
grep -qi 'jupiter\|galileo' /sys/class/dmi/id/product_name 2>/dev/null \
    && ok "Steam Deck hardware: $(cat /sys/class/dmi/id/product_name)" \
    || info "Hardware: $(cat /sys/class/dmi/id/product_name 2>/dev/null || echo unknown)"
info "Memory: $(free -h | awk '/Mem:/ {print $2 " total, " $7 " available"}')"

section "Display session"
info "XDG_SESSION_TYPE=${XDG_SESSION_TYPE:-unset} DISPLAY=${DISPLAY:-unset} WAYLAND_DISPLAY=${WAYLAND_DISPLAY:-unset}"
[ -n "${GAMESCOPE_WAYLAND_DISPLAY:-}" ] && info "Running inside gamescope (Game Mode)" || info "Not in gamescope (Desktop Mode, expected for this script)"
command -v Xwayland >/dev/null && ok "Xwayland present (PKForge draws through X11)" || warn "Xwayland not found"
[ -e /dev/dri/renderD128 ] && ok "GPU render node /dev/dri/renderD128" || warn "No /dev/dri/renderD128"

section "Disk space"
df -h "$HOME" | awk 'NR==2 {print "  [info] Home: " $4 " free of " $2}'
for m in /run/media/*/* /run/media/*; do
    [ -d "$m" ] && mountpoint -q "$m" 2>/dev/null && df -h "$m" | awk -v m="$m" 'NR==2 {print "  [info] SD/external " m ": " $4 " free"}'
done
avail_kb=$(df -Pk "$HOME" | awk 'NR==2 {print $4}')
[ "${avail_kb:-0}" -gt 6000000 ] && ok "More than 6 GB free (container build needs ~5 GB)" || warn "Less than 6 GB free in home: a source build may not fit (the CI tarball needs ~200 MB)"

section "Tools"
for t in flatpak podman distrobox docker git curl tar sha256sum; do
    command -v "$t" >/dev/null && ok "$t: $(command -v "$t")" || info "$t: not installed"
done
if command -v flatpak >/dev/null; then
    flatpak remotes --user 2>/dev/null | grep -q flathub || flatpak remotes 2>/dev/null | grep -q flathub \
        && ok "Flathub remote configured" || warn "No Flathub remote: the Flatpak needs org.freedesktop.Platform 25.08 from Flathub"
    flatpak info org.freedesktop.Platform//25.08 >/dev/null 2>&1 \
        && ok "Runtime org.freedesktop.Platform 25.08 installed" \
        || info "Runtime org.freedesktop.Platform 25.08 not installed yet (flatpak installs it with the bundle)"
    if flatpak info org.pkforge.app >/dev/null 2>&1; then
        info "PKForge Flatpak already installed:"
        flatpak info --show-permissions org.pkforge.app 2>/dev/null | sed 's/^/         /'
        flatpak override --user --show org.pkforge.app 2>/dev/null | sed 's/^/         override: /'
    else
        info "PKForge Flatpak not installed"
    fi
fi

section "Controllers"
grep -E '^N: Name=' /proc/bus/input/devices 2>/dev/null | grep -iE 'steam|valve|x-?box|controller|gamepad|dualsense|8bitdo' \
    | sed 's/^N: Name=/  [info] /' || true
ls /dev/hidraw* >/dev/null 2>&1 && ok "hidraw devices present (SDL reads the Deck controls through Steam Input in Game Mode)" \
    || warn "No /dev/hidraw devices"

section "Running emulators (must be closed while PKForge writes)"
running=$(pgrep -af 'retroarch|dolphin-emu|azahar|citra|lime3ds|eden|melonds|mgba|desmume' 2>/dev/null | grep -v pgrep)
[ -n "$running" ] && { warn "Emulators running now:"; echo "$running" | sed 's/^/         /'; } || ok "No emulator running"

# Folders the Flatpak may read (packaging/linux/org.pkforge.app.yml). A save whose real
# location is outside these is invisible inside the sandbox, even if a symlink points to it.
FLATPAK_ALLOWED=(
    "$HOME/.config/retroarch" "$HOME/.var/app/org.libretro.RetroArch"
    "$HOME/.local/share/Steam/steamapps/common/RetroArch"
    "$HOME/.local/share/dolphin-emu" "$HOME/.var/app/org.DolphinEmu.dolphin-emu"
    "$HOME/.local/share/azahar-emu" "$HOME/.var/app/org.azahar_emu.Azahar"
    "$HOME/.local/share/lime3ds-emu" "$HOME/.local/share/citra-emu"
    "$HOME/.local/share/eden" "$HOME/.var/app/dev.eden_emu.eden"
    "$HOME/Emulation" "$HOME/retrodeck" "/run/media"
)
in_sandbox() {
    local real="$1" a ra
    for a in "${FLATPAK_ALLOWED[@]}"; do
        ra=$(readlink -f "$a" 2>/dev/null) || continue
        case "$real/" in "$a"/*|"$ra"/*) return 0 ;; esac
    done
    return 1
}

section "Emulator folders PKForge looks in (src/PKForge.Infrastructure/LinuxEmulatorLocations.cs)"
CANDIDATES=(
    ".config/retroarch" ".var/app/org.libretro.RetroArch/config/retroarch"
    ".local/share/Steam/steamapps/common/RetroArch"
    ".local/share/dolphin-emu" ".var/app/org.DolphinEmu.dolphin-emu/data/dolphin-emu"
    ".local/share/azahar-emu" ".var/app/org.azahar_emu.Azahar/data/azahar-emu"
    ".local/share/lime3ds-emu" ".local/share/citra-emu"
    ".local/share/eden" ".var/app/dev.eden_emu.eden/data/eden"
    "Emulation/saves/retroarch" "Emulation/saves/melonds" "Emulation/saves/dolphin"
    "Emulation/saves/azahar" "Emulation/saves/citra" "Emulation/saves/eden" "retrodeck/saves"
)
found_any=0
for rel in "${CANDIDATES[@]}"; do
    p="$HOME/$rel"
    [ -e "$p" ] || continue
    found_any=1
    real=$(readlink -f "$p")
    note=""; [ "$real" != "$p" ] && note=" -> $real"
    if [ ! -d "$real" ]; then warn "~/$rel$note (broken link)"; continue; fi
    in_sandbox "$real" && ok "~/$rel$note" || warn "~/$rel$note (real folder is OUTSIDE the Flatpak's allowed paths: the Flatpak cannot see it; the tarball build can)"
done
[ $found_any = 1 ] || warn "None found: you will link save folders by hand"
for p in "$HOME/.config/mgba" "$HOME/.var/app/io.mgba.mGBA" "$HOME/Emulation/saves/mgba"; do
    [ -e "$p" ] && info "mGBA folder $p (not auto-detected; link it by hand in PKForge)"
done

section "Save files found (the ones to back up before testing)"
for root in "$HOME/Emulation/saves" "$HOME/retrodeck/saves" "$HOME/.config/retroarch/saves" \
            "$HOME/.var/app/org.libretro.RetroArch/config/retroarch/saves" /run/media/*/Emulation/saves /run/media/*/*/Emulation/saves; do
    [ -d "$root" ] || continue
    n=$(find -L "$root" -type f \( -iname '*.srm' -o -iname '*.sav' -o -iname '*.dsv' -o -iname '*.dat' -o -name main \) 2>/dev/null | wc -l)
    size=$(du -shL "$root" 2>/dev/null | cut -f1)
    info "$root: $n save-like files, $size total"
    find -L "$root" -type f -iname '*pok*' 2>/dev/null | head -10 | sed 's/^/         /'
done
[ -d "$HOME/Emulation/saves" ] && info "Backup command: cp -a ~/Emulation/saves ~/saves-backup-$(date +%F)"

section "PKForge data (bank, restore points, logs)"
for d in "$HOME/.local/share/PKForge.App" "$HOME/.var/app/org.pkforge.app/data"; do
    [ -d "$d" ] && info "$d exists ($(du -sh "$d" | cut -f1))" || info "$d: not created yet"
done

if [ -n "$BUILD_DIR" ]; then
    section "Tarball build at $BUILD_DIR"
    if [ ! -f "$BUILD_DIR/PKForge.App" ]; then
        warn "No PKForge.App in $BUILD_DIR"
    else
        [ -x "$BUILD_DIR/pkforge.sh" ] && ok "pkforge.sh is executable" || warn "pkforge.sh not executable: chmod +x \"$BUILD_DIR/pkforge.sh\""
        [ -x "$BUILD_DIR/PKForge.App" ] && ok "PKForge.App is executable" || warn "PKForge.App not executable"
        # libcoreclrtraceptprovider.so (optional .NET tracing) wants liblttng-ust; it is never needed.
        missing=$(for f in "$BUILD_DIR/PKForge.App" "$BUILD_DIR"/*.so; do
            [ "${f##*/}" = libcoreclrtraceptprovider.so ] && continue
            ldd "$f" 2>/dev/null | grep 'not found'; done | sort -u)
        [ -z "$missing" ] && ok "All native libraries resolve (ldd)" || { warn "Missing system libraries:"; echo "$missing" | sed 's/^/         /'; }
        for lib in libX11.so.6 libICE.so.6 libSM.so.6 libfontconfig.so.1 libGL.so.1; do
            ldconfig -p 2>/dev/null | grep -q "$lib" && ok "$lib available" || warn "$lib not found (Avalonia X11 needs it)"
        done
    fi
fi

section "Done"
info "Nothing was changed. Report saved to $REPORT"
