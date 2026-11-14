# PKForge on Steam Deck: test checklist

## Preflight (read-only, changes nothing)
- [ ] In Desktop Mode (Konsole): `bash deck-preflight.sh` (or `bash deck-preflight.sh ~/pkforge-linux-x64` after unpacking the tarball)
- [ ] Fix every `[WARN]`, or note it. Paste `~/pkforge-preflight-*.txt` back to Claude if anything is unclear.
- [ ] A save folder flagged "OUTSIDE the Flatpak's allowed paths" is invisible to the Flatpak: use the tarball build, or grant it with
      `flatpak override --user --filesystem=<real folder> org.pkforge.app`

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

## Playing DS and GBA games with Pokémon Fulldex (optional)
[Pokémon Fulldex](https://github.com/mamesj/pokemon-fulldex) plays Black/White, Black 2/White 2,
HeartGold/SoulSilver, Platinum, Diamond/Pearl and the GBA games in one widescreen picture
(melonDS and mGBA built in). It ships its own Steam Deck build; you bring your own ROMs dumped
from your cartridges.
- [ ] Install the **AppImage** with Gear Lever (Discover > Gear Lever). Its ROMs, saves and settings go
      in `~/.local/share/pokemon-ds-wide`, the folder PKForge finds by itself. (The tarball works too, but
      keeps its saves beside the program: link that `saves` folder by hand in PKForge, as melonDS.)
- [ ] Put your ROMs in its `roms` folder (**O** in its game list opens it), add it to Steam, set 3D RESOLUTION to 720P.
- [ ] To carry on a save from another emulator, copy it in as `black2.sav`, `soulsilver.sav`, `platinum.sav` ...
- [ ] PKForge and Fulldex never edit a save at the same time: **QUIT** the game in Fulldex (pause menu) before
      editing in PKForge. Fulldex keeps the save in memory while a game runs and would write over PKForge's edit.
- [ ] If Fulldex cloud saves are on, keep them off until you've checked an edited save in the game.
- [ ] Fulldex keeps its own dated copies in `saves/backups/` (its RESTORE BACKUP menu); PKForge skips that folder.
