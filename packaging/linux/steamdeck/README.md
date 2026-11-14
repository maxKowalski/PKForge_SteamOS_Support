# PKForge + Pokémon Fulldex on the Steam Deck

Play your DS and GBA Pokémon games in **Pokémon Fulldex** (one widescreen picture, 60 fps) and manage
them with **PKForge** (Bank, living dex, events, transfers between games), both in Game Mode.

```
 Pokémon Fulldex                          PKForge
 plays the games            saves         edits the saves
 ~/.local/share/pokemon-ds-wide/saves  ◄──────►  Bank · living dex · events · legality
 (emerald.sav, black2.sav ...)                   (finds that folder by itself)
```

> [!IMPORTANT]
> **Status.** Everything here was built and tested on x86-64 Linux: FireRed and Emerald were played in
> Fulldex, a Charmander was moved FireRed → Bank → Emerald with PKForge, made shiny there, and both games
> loaded the edited saves. **It has not been run on a Steam Deck yet.** Follow the steps in order, and keep
> the backup from step 0 until you have checked your own saves in the games.

## Contents

0. [Back up your saves](#0-back-up-your-saves)
1. [Check the Deck (read-only)](#1-check-the-deck-read-only)
2. [Install Pokémon Fulldex](#2-install-pokémon-fulldex)
3. [Install PKForge](#3-install-pkforge)
4. [Add PKForge to Game Mode](#4-add-pkforge-to-game-mode)
5. [Link your saves in PKForge](#5-link-your-saves-in-pkforge)
6. [Everyday use: play, edit, play](#6-everyday-use-play-edit-play)
7. [Optional: the Game Mode window test](#7-optional-the-game-mode-window-test)
- [Troubleshooting](#troubleshooting) · [Where everything lives](#where-everything-lives) · [Updating and removing](#updating-and-removing)

## What you need

| | |
|---|---|
| Steam Deck | SteamOS 3.5 or newer (Fulldex needs glibc 2.38+) |
| Space | about 300 MB, plus your ROMs |
| Your games | ROMs dumped from cartridges you own. Neither app includes any games. |
| Time | about 20 minutes, in **Desktop Mode** (Steam button → Power → Switch to Desktop) |

Everything below installs into your home folder: no `sudo`, no password, nothing changed in SteamOS.
Commands go in **Konsole** (the app menu → System → Konsole).

## 0. Back up your saves

Copy every save folder you have before anything touches them:

```bash
mkdir -p ~/saves-backup-$(date +%F)
cp -a ~/Emulation/saves              ~/saves-backup-$(date +%F)/emudeck   2>/dev/null
cp -a ~/.local/share/pokemon-ds-wide ~/saves-backup-$(date +%F)/fulldex   2>/dev/null
cp -a ~/.config/retroarch/saves      ~/saves-backup-$(date +%F)/retroarch 2>/dev/null
ls ~/saves-backup-$(date +%F)
```

> [!TIP]
> PKForge also makes a restore point before every write, but it keeps only the last 20 per save. A long
> living-dex run can push your untouched original out of that list, so this copy is the one that really protects you.

## 1. Check the Deck (read-only)

Get this guide's scripts, then run the preflight check. It changes nothing; it only reports.

```bash
mkdir -p ~/pkforge-deck && cd ~/pkforge-deck
BASE=https://raw.githubusercontent.com/maxKowalski/PKForge_SteamOS_Support/claude/pkforge-steamos-support-m2jali/packaging/linux/steamdeck
for f in deck-preflight.sh gamemode-window-test.sh build-pkforge-deck.sh; do curl -fLO "$BASE/$f"; done
chmod +x *.sh
./deck-preflight.sh
```

It prints your SteamOS version, free space, controllers, which emulator folders PKForge will find, the
saves it sees, and whether anything is running. Fix or note every `[WARN]`. The report is saved as
`~/pkforge-preflight-<date>.txt`.

## 2. Install Pokémon Fulldex

### 2a. Install the AppImage with Gear Lever

1. Open **Discover**, search for **Gear Lever**, and install it.
2. Download Fulldex's Linux AppImage and check it:

   ```bash
   cd ~/Downloads
   curl -fLO https://github.com/mamesj/pokemon-fulldex/releases/latest/download/pokemon-ds-wide-pc-linux-x86_64.AppImage
   curl -fLO https://github.com/mamesj/pokemon-fulldex/releases/latest/download/SHA256SUMS.txt
   grep 'linux-x86_64.AppImage' SHA256SUMS.txt | sha256sum -c -     # must say: OK
   ```

3. Open the AppImage with **Gear Lever** and choose to add it to the app menu. Gear Lever keeps it in
   `~/AppImages` and updates it for you.

With the AppImage, Fulldex keeps everything in **`~/.local/share/pokemon-ds-wide`**: `roms/`, `saves/` and
`settings.cfg`. PKForge looks in that `saves` folder by itself.

### 2b. Add your ROMs

Copy your ROMs into `~/.local/share/pokemon-ds-wide/roms` (any file names). In Fulldex's game list, **O**
opens that folder. Supported: Black 2, White 2, Black, White, HeartGold, SoulSilver, Platinum, Diamond and
Pearl (DS); Emerald, Ruby, Sapphire, FireRed, LeafGreen and Ultra Violet (GBA).

### 2c. Bring over existing saves (optional)

Fulldex names each save after the game. To carry on a game from another emulator, **copy** (don't move)
its save into `~/.local/share/pokemon-ds-wide/saves` under the right name:

| Game | Save name | | Game | Save name |
|---|---|---|---|---|
| Black 2 / White 2 | `black2.sav` / `white2.sav` | | Emerald | `emerald.sav` |
| Black / White | `black.sav` / `white.sav` | | FireRed / LeafGreen | `firered.sav` / `leafgreen.sav` |
| HeartGold / SoulSilver | `heartgold.sav` / `soulsilver.sav` | | Ruby / Sapphire | `ruby.sav` / `sapphire.sav` |
| Platinum | `platinum.sav` | | Ultra Violet | `ultraviolet.sav` |
| Diamond / Pearl | `diamond.sav` / `pearl.sav` | | | |

Other languages add a suffix: `heartgold-fr.sav`, `black2-es.sav`, `pearl-de.sav`.

Where your current saves usually are with EmuDeck (the preflight report from step 1 lists the
Pokémon saves it found, with their full paths):

| Emulator | Save to copy |
|---|---|
| RetroArch (mGBA, melonDS cores) | `~/Emulation/saves/retroarch/saves/<ROM name>.srm` (GBA) or `.sav` (DS), sometimes in a per-core subfolder |
| melonDS standalone | `~/Emulation/saves/melonds/<ROM name>.sav` |
| mGBA standalone | `~/Emulation/saves/mgba/saves/<ROM name>.sav` |

Example, Emerald from RetroArch:

```bash
cp "$HOME/Emulation/saves/retroarch/saves/Pokemon - Emerald Version (USA, Europe).srm" \
   ~/.local/share/pokemon-ds-wide/saves/emerald.sav
```

(Use the exact file name you see in that folder; quotes keep the spaces.) Start the game in Fulldex and
check you're at the same place before going further.

### 2d. Add Fulldex to Steam

1. Steam → **Games → Add a Non-Steam Game to My Library** → tick **Pokémon Fulldex** (or **Browse** to the
   AppImage in `~/AppImages`, file type *All Files*).
2. Fulldex's own **Settings → DESKTOP SHORTCUTS** can also make one entry per game, so each starts straight
   into its game from the Non-Steam list.
3. In Fulldex, set **3D RESOLUTION → 720P** (the Deck's screen is 1280×800).
4. To use the touchscreen with the trackpad, in the game's Steam controller layout set the **right trackpad
   to Mouse** and its click to **Left Mouse Click**; keep the buttons as gamepad buttons.

In a game: **Start** (or the Guide button) opens Fulldex's pause menu, **QUIT** there goes back to its list,
and holding the **right trigger** fast-forwards.

## 3. Install PKForge

Get the build from this fork's GitHub Actions: open
<https://github.com/maxKowalski/PKForge_SteamOS_Support/actions>, open the newest green **build** run, and
download from **Artifacts** (you need to be signed in to GitHub). Artifacts expire; if they're gone, use
**Run workflow** on that page to make fresh ones.

| | Folder build (recommended) | Flatpak |
|---|---|---|
| Artifact | `pkforge-linux-x64` | `PKForge-x86_64.flatpak` |
| Sandbox | none, like any desktop app | only the emulator folders, `/run/media` and `~/Documents/PKForge` |
| Saves in other folders | yes | only after a `flatpak override` |
| Future "Play in Fulldex" button | possible | not possible (a sandboxed app can't start Fulldex) |

### Option A: folder build

```bash
mkdir -p ~/Applications && cd ~/Applications
python3 -m zipfile -e ~/Downloads/pkforge-linux-x64.zip .
tar -xzf pkforge-linux-x64.tar.gz && rm pkforge-linux-x64.tar.gz
rm -rf PKForge && mv pkforge-linux-x64 PKForge
~/pkforge-deck/deck-preflight.sh ~/Applications/PKForge     # checks the build too
```

PKForge starts with `~/Applications/PKForge/pkforge.sh`.

### Option B: Flatpak

```bash
cd ~/Downloads && python3 -m zipfile -e PKForge-x86_64.flatpak.zip .
flatpak install --user PKForge*.flatpak                 # the bundle that came out of the zip
flatpak override --user --nonetwork org.pkforge.app     # it only needs network for sprite packs
flatpak info --show-permissions org.pkforge.app         # emulator folders only
```

### Option C: build it on the Deck

`~/pkforge-deck/build-pkforge-deck.sh` builds the folder build from source inside a podman container
(needs about 5 GB free and internet; nothing is installed on SteamOS itself).

## 4. Add PKForge to Game Mode

1. Steam → **Games → Add a Non-Steam Game** → **Browse** → `~/Applications/PKForge/pkforge.sh`
   (file type *All Files*). For the Flatpak, tick **PKForge** in the list instead.
2. Switch back to Game Mode and start PKForge from **Library → Non-Steam**. Use PKForge in Game Mode: that's
   where Steam passes the controller to it as a gamepad.
3. If the interface is too small or too large, set a scale in the game's **Properties → Launch options**:
   `PKFORGE_SCALE=1.25 %command%` (folder build), or for the Flatpak run
   `flatpak override --user --env=PKFORGE_SCALE=1.25 org.pkforge.app` once.

| Control | Controller | Keyboard |
|---|---|---|
| Move | D-pad / left stick | Arrows |
| Open / confirm | A | X |
| Back | B | Z, Esc, Backspace |
| Link a folder | Y | A |
| File / tools | X | S |
| Previous / next box (in a save) | L / R | Q / W |
| Restore points (home screen) | R | W |
| Menu | Start | Enter |
| Poképark camera | Right stick | |

Touch works too: tap to select, drag a list to scroll.

## 5. Link your saves in PKForge

On first launch PKForge links every emulator folder it finds, once each, and shows its games on the
home screen:

| Found by itself | Folder |
|---|---|
| Pokémon Fulldex (AppImage) | `~/.local/share/pokemon-ds-wide/saves` |
| EmuDeck | `~/Emulation/saves/retroarch`, `melonds`, `dolphin`, `azahar`, `citra`, `eden` |
| RetroArch, Dolphin, Azahar/Citra, Eden | their own folders and Flatpak folders |
| RetroDECK | `~/retrodeck/saves` |

Link anything else by hand with **Y (Link) → platform → emulator → Choose folder**: mGBA standalone, a
Fulldex tarball (its `saves` folder sits beside the program), or saves on the SD card
(`/run/media/...`). Link Fulldex as **melonDS**.

Each game should appear once. Fulldex's own copies (`saves/backups/` and `<game>.sav.prelaunch.bak`) are
skipped on purpose.

## 6. Everyday use: play, edit, play

> [!WARNING]
> **One app at a time.** Save in the game and **quit the game in Fulldex** (pause → QUIT) before editing in
> PKForge. A running game keeps its save in memory and writes it back at your next in-game save,
> over PKForge's changes. Never edit save states: PKForge works on the real save only.

1. **Play** in Fulldex, save in the game, QUIT.
2. **Edit** in PKForge: open the game from the home screen, change what you want; PKForge validates each
   change and makes a restore point before writing.
3. **Play** again: start the game in Fulldex and pick CONTINUE.

**Moving a Pokémon between games** (the tested path): open the first game, move the Pokémon to the
**Bank**, then open the other game and take it from the Bank. Each move is all or nothing: the Pokémon is
never lost or left half-moved. After that you can edit it there (nickname, shiny, nature, IVs...) and
check it with the legality check.

**"Write refused: the save changed on disk after PKForge opened it"** means the game wrote the save while
PKForge had it open. Nothing was written. Close the game, reopen the save in PKForge, and redo the change.

**Getting a save back:** PKForge's restore points (**R** on the home screen) or Fulldex's own **RESTORE
BACKUP** menu, and failing that your copy from step 0.

**Fulldex cloud saves:** if you turned them on, keep them off until you've checked an edited save in the
game, or the cloud copy and the edited one can disagree.

## 7. Optional: the Game Mode window test

A **Play in Fulldex** button in PKForge would let you start a game straight from PKForge. Before building
it, this test checks that Game Mode shows a second app's window. Add `~/pkforge-deck/gamemode-window-test.sh`
as a non-Steam game, start it in Game Mode, and note:

- Does Fulldex appear on top of the "PKForge stand-in" window?
- Does the controller work in it?
- After quitting Fulldex, does the stand-in come back?

The log is in `~/pkforge-gamemode-window-test.txt`. Send it back with your answers.

## Troubleshooting

| Problem | Fix |
|---|---|
| PKForge shows no saves | Run `~/pkforge-deck/deck-preflight.sh`. A folder marked *outside the Flatpak's allowed paths* needs the folder build, or `flatpak override --user --filesystem=<folder> org.pkforge.app`. Otherwise link it by hand (Y). |
| A game appears twice | Update PKForge (older builds listed Fulldex's `.prelaunch.bak` copies). |
| The controller does nothing in PKForge | Start it from Game Mode. The log should show `Controller connected`. |
| Interface too small or large | `PKFORGE_SCALE` (step 4). |
| Fulldex says the save is new / empty | Check the file name against the table in 2c, and that it's in `saves/`. |
| Fulldex won't start | `deck-preflight.sh` shows the glibc and SteamOS version; Fulldex needs SteamOS 3.5+. |
| Anything else | Find PKForge's log with `find ~/.config/PKForge.App ~/.var/app/org.pkforge.app -name pkforge.log`; Fulldex's is `~/.local/share/pokemon-ds-wide/game.log`. |

## Where everything lives

| What | Where |
|---|---|
| Fulldex ROMs, saves, settings (AppImage) | `~/.local/share/pokemon-ds-wide/` |
| Fulldex's own save backups | `~/.local/share/pokemon-ds-wide/saves/backups/` |
| PKForge Bank, restore points, settings, log (folder build) | `~/.config/PKForge.App/` |
| PKForge data (Flatpak) | `~/.var/app/org.pkforge.app/` |
| Files PKForge shares or exports | `~/Documents/PKForge/` |
| This guide's scripts | `~/pkforge-deck/` |

## Updating and removing

- **Fulldex:** Gear Lever updates the AppImage; ROMs and saves stay in `~/.local/share/pokemon-ds-wide`.
- **PKForge folder build:** unpack the new build over `~/Applications/PKForge` (step 3A). The Bank, links and
  restore points stay in `~/.config/PKForge.App`.
- **PKForge Flatpak:** `flatpak install --user PKForge*.flatpak` again; remove with
  `flatpak uninstall --user org.pkforge.app` (its data stays in `~/.var/app/org.pkforge.app` until you delete it).
- Removing either app never touches your saves.

## Not on Linux yet

PKForge's background music, second-screen inspector, Poképark widget and in-app updates; sharing opens
`~/Documents/PKForge` instead of a share sheet. A **Play in Fulldex** button waits on the window test in step 7.
