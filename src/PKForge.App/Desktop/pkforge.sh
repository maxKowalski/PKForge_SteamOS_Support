#!/bin/sh
# Starts PKForge from its own folder. Add it to Steam as a non-Steam game to run it in Game Mode.
#   PKFORGE_SCALE=2       scales the whole UI (use it when everything looks tiny on a 1600p screen)
#   PKFORGE_FULLSCREEN=1  starts fullscreen outside Game Mode too
HERE=$(dirname "$(readlink -f "$0")")
if [ -n "$PKFORGE_SCALE" ]; then
    export AVALONIA_GLOBAL_SCALE_FACTOR="$PKFORGE_SCALE"
fi
exec "$HERE/PKForge.App" "$@"
