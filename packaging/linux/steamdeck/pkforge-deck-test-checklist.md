# PKForge on Steam Deck: test checklist

## Before touching anything
- [ ] Copy your emulator save folders (or whole `~/Emulation/saves`) to a backup, e.g. `cp -a ~/Emulation/saves ~/saves-backup-$(date +%F)`
- [ ] Test on a COPY of one save first, not your main one. Use File > open a single save.
- [ ] Close the emulator completely. Save in-game (not a save state).
- [ ] PKForge keeps only the last 20 restore points per save. A long living-dex run can push the
      untouched original out, so the manual copy above is the one that really protects you.
- [ ] If PKForge says "the save changed on disk after PKForge opened it", the emulator wrote the save
      while PKForge had it open. Nothing was written. Close the game, reopen the save in PKForge, redo the edit.

## Install and lock down
- [ ] Easiest: download `pkforge-linux-x64` (tarball) or the Flatpak bundle from the latest GitHub Actions run
      of this branch, or build with `build-pkforge-deck.sh`
- [ ] Flatpak bundle: `flatpak install --user PKForge.flatpak`
- [ ] `flatpak override --user --nonetwork org.pkforge.app` (add network back later only for sprite packs)
- [ ] `flatpak info --show-permissions org.pkforge.app` and check only emulator folders are listed
- [ ] Add to Steam, launch from **Game Mode**
- [ ] If the UI is tiny: `flatpak override --user --env=PKFORGE_SCALE=2 org.pkforge.app`

## Smoke tests, in order
1. App launches, fullscreen in Game Mode. Note any crash; log: `~/.var/app/org.pkforge.app/data/PKForge.App/logs/pkforge.log`
2. Controller navigates menus (look for "Controller connected" in the log)
3. Touch drag scrolls lists; right stick moves Poképark camera
4. Emulators auto-link (EmuDeck paths). Link mGBA by hand if you use it.
5. Open a save, view boxes. Make NO edits yet.
6. Make one harmless edit on the copied save, write it, load it in the emulator, confirm it boots and the change shows.
7. Bank: move one Pokémon between two copied saves, load both in emulators.
8. Living dex / event gift / Poképark: try one each on copies.

## Known gaps (per README)
No music, no second-screen inspector, no Poképark widget, no in-app update, Share opens ~/Documents/PKForge.

## Report back
Save the log and note which step failed, game/emulator, and SteamOS version.
