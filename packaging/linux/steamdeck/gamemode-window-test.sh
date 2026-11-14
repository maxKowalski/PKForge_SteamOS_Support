#!/bin/bash
# Game Mode window test: can an app started from Steam open Pokémon Fulldex as its own child
# process, show it on top, and get the screen back when Fulldex quits? (What "Play in Fulldex"
# in PKForge would do.) Touches no saves and installs nothing.
#
# Why it should work: gamescope gives a window to the Steam game whose cgroup
# (app-steam-app<appid>-<pid>.scope) or "reaper SteamLaunch AppId=" ancestor the window's process
# has, so a child process belongs to the same game; among one game's windows the most recently
# mapped one is shown (gamescope src/Utils/Process.cpp GetAppIdFromPid, src/steamcompmgr.cpp
# is_focus_priority_greater). Programs started through D-Bus or xdg-open are not children and
# stay hidden, which is why links opened from games do not appear in Game Mode.
#
# Setup (Desktop Mode): Steam > Add a Game > Add a Non-Steam Game > Browse > this file
# (file type: All Files). Then switch to Game Mode and start it from Library > Non-Steam.
#   bash gamemode-window-test.sh [path to Fulldex: AppImage or pokemon-ds-wide]
set -u
LOG="$HOME/pkforge-gamemode-window-test.txt"
exec > >(tee "$LOG") 2>&1

FULLDEX="${1:-}"
if [ -z "$FULLDEX" ]; then
    for c in "$HOME"/AppImages/*okemon*ds*wide*.AppImage "$HOME"/AppImages/*ulldex*.AppImage \
             "$HOME"/Applications/*okemon*ds*wide*.AppImage "$HOME"/Applications/*ulldex*.AppImage; do
        [ -x "$c" ] && FULLDEX="$c" && break
    done
fi
[ -z "$FULLDEX" ] && FULLDEX=$(find "$HOME" -maxdepth 4 -type f -name pokemon-ds-wide -perm -u+x 2>/dev/null | head -1)
say() { zenity --info --width=520 --title="PKForge window test" --text="$1" 2>/dev/null || echo "$1"; }

echo "date: $(date)"
echo "session: XDG_SESSION_TYPE=${XDG_SESSION_TYPE:-} DISPLAY=${DISPLAY:-} WAYLAND_DISPLAY=${WAYLAND_DISPLAY:-} GAMESCOPE_WAYLAND_DISPLAY=${GAMESCOPE_WAYLAND_DISPLAY:-}"
echo "steam app: SteamAppId=${SteamAppId:-} SteamGameId=${SteamGameId:-}"
echo "cgroup: $(grep -o 'app-steam-app[^/]*' /proc/self/cgroup | head -1)"
echo "fulldex: ${FULLDEX:-not found}"
if [ -z "$FULLDEX" ]; then
    say "Fulldex was not found. Give its path as the launch option, e.g.\n$HOME/AppImages/pokemon-ds-wide-pc-linux-x86_64.AppImage"
    exit 1
fi
[ -n "${GAMESCOPE_WAYLAND_DISPLAY:-}" ] || echo "note: not in Game Mode (no gamescope); the test only means something in Game Mode"

# 1. This window stands in for PKForge and stays open the whole time.
zenity --info --width=520 --title="PKForge stand-in" \
    --text="This window stands in for PKForge.\n\nIn 5 seconds Fulldex opens on top of it, started as a child process exactly as PKForge would.\n\n1. Check Fulldex appears and the controller works in it.\n2. Quit Fulldex (Esc / B in its game list, then QUIT).\n3. This window should come back. Press OK here to finish." 2>/dev/null &
STANDIN=$!
sleep 5

# 2. Fulldex as a child of this process (same cgroup and Steam app id).
echo "starting fulldex at $(date +%T)"
"$FULLDEX"
echo "fulldex exited with $? at $(date +%T)"

# 3. Back to the stand-in.
wait "$STANDIN"
echo "stand-in closed at $(date +%T)"
say "Test finished. The log is in $LOG.\n\nWhat to report: did Fulldex appear on top? Did the controller work in it? Did this window come back after quitting Fulldex?"
