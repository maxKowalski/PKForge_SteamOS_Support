<p align="center"><img src="docs/images/logo-transparent.png" alt="PKForge" width="140" /></p>

<p align="center"><b>A Pokémon save editor and bank for Android, with support for dual-screen handhelds.</b></p>

<p align="center">
  <a href="https://discord.gg/bMtzZmTDfu"><img src="https://img.shields.io/badge/Discord-Join%20the%20server-5865F2?logo=discord&logoColor=white" alt="Discord" /></a>
  <a href="https://github.com/sofianeelhor/PKForge/releases"><img src="https://img.shields.io/badge/Download-APK-2B4E95" alt="Download" /></a>
  <a href="https://github.com/sofianeelhor/PKForge/wiki"><img src="https://img.shields.io/badge/Wiki-Read%20the%20manual-2B4E95" alt="Wiki" /></a>
  <a href="LICENSE"><img src="https://img.shields.io/badge/License-GPLv3-blue" alt="License" /></a>
</p>

---

A Pokémon save editor and cross-generation Bank for Android, tuned for dual-screen
handhelds like the AYN Thor.
Built on [PKHeX.Core](https://github.com/kwsch/PKHeX) with the
[Auto Legality Mod](https://github.com/santacrab2/PKHeX-Plugins) built in, so
legalizing a Pokémon works fully offline, on device.

<p align="center">
  <img src="docs/screenshots/dual-screen.png" alt="A box on the main screen, the Pokémon inspector on the second screen" width="100%" />
</p>

Every screen, every emulator and every feature is explained with screenshots in the
**[PKForge wiki](https://github.com/sofianeelhor/PKForge/wiki)**.

Not affiliated with Nintendo, Game Freak, or The Pokémon Company.

> [!CAUTION]
> pkforgeapp.com is **not** an official PKForge site and is not run by the maintainer. Do not download anything from it. Only use releases from this repository.

## Features

- **Manage your Pokémon.** Open any save from a linked emulator or a single file and
  take full control of your team and boxes. Edit stats, IVs, EVs, moves, nature,
  ability, held item, Met/Origin, Tera type, Hyper Training, shininess, friendship,
  Pokérus, and more, with a live legality check as you go.
- **One-tap legalize.** The Auto Legality Mod is built into the app, so legalizing a
  Pokémon works fully offline, on device.
- **Move Pokémon between games.** Grab a Pokémon from one save and drop it into another,
  across generations, from Gen 1 to Gen 9 and back. A preview shows every change and
  the legality verdict before anything is written.
- **A Bank like Pokémon Home.** Store your whole collection in a cross-game Bank with
  as many themed boxes as you need. Organize with the gamepad, keep living dexes, and pull
  anything back out into any save whenever you need it.
- **Track your collection.** The Living Dex tracker follows all 1025 Pokémon across
  every generation, including shiny variants, so you always know what you still need.
- **Living Dex Autopilot.** Builds a living dex from the Pokémon you already own across
  all your games and the Bank. It moves Pokémon (never copies them), evolves trade
  evolutions on the way, and writes nothing until you confirm.
- **Browse and inject events.** The full Mystery Gift database is bundled and works
  offline. Browse Wonder Cards from old distributions and inject them straight into
  your save.
- **Edit the bag.** The bag editor handles any pouch, any item, any quantity, with
  presets and one-tap refills.
- **Edit the trainer.** Change your name, IDs, money, and gender on the trainer card,
  and manage trainer profiles and records.
- **RNG tools.** Inspect PID, IVs, and nature, and reroll a nature while keeping
  shininess.
- **Batch edit.** The organizer handles multi-select operations: copy, move, duplicate,
  delete, release, and send to the Bank or Poképark in bulk.
- **Breed and hatch.** The egg factory and Day Care / Nursery tools generate eggs and
  hatch Pokémon, including one egg of every species.
- **Complete the Pokédex.** Mark everything seen, generate a living dex, and track
  what is missing.
- **Play with your Pokémon.** The Poképark is a living world where up to 24 of your
  Pokémon live by their tempers, friendships and rivalries, act out scenes together and
  have rare events. Feed them, play with them, take photos. The Nuzlocke report tracks
  first encounters and dupes per route, and the encounter browser shows every way to
  catch a species in each game.
- **Share your teams.** Import and export Showdown sets, and generate QR codes to move
  a set between devices.
- **Game-specific editors.** Grand Underground, Poké Beans, Fashion, and more for each
  game.
- **World & events.** Clock repair for Ruby / Sapphire / Emerald, roaming legendaries,
  re-arming one-time encounters, Entralink and Dream World, Apricorn Box and Pokéwalker,
  honey trees, event flags, and key item events such as the Eon Ticket or the GS Ball.
- **Organize boxes.** Box manager (move, lock, copy, rename, wallpapers), sort by dex,
  level, IVs, type or age, and a legality check and audit of the whole save.
- **ROM hacks.** Pokémon Unbound, Radical Red, GS Chronicles, Luminescent Platinum and
  Pokémon Compass are supported with their own data. Unknown hacks open read-only with a
  warning.
- **HaX and Hardcore modes.** HaX lifts the legality limits. Hardcore turns data editing
  off for challenge runs and RetroAchievements players, while moving Pokémon stays
  possible.
- **Built for handhelds.** Fully playable with a gamepad or by touch. On dual-screen
  devices the second screen shows the Pokémon inspector, game art, Pokédex entries or
  the Poképark journal.
- **Keep your saves safe.** Every write is validated, then preceded by a restore point.
  Only the Pokémon you touched may change, and risky saves open read-only. The 20 most
  recent restore points are kept.

## Screenshots

<p align="center">
  <img src="docs/screenshots/home.png" alt="Home" width="49%" />
  <img src="docs/screenshots/editor.png" alt="Editor" width="49%" />
</p>

<p align="center">
  <img src="docs/screenshots/bank.png" alt="Bank" width="49%" />
  <img src="docs/screenshots/living-dex.png" alt="Living Dex" width="49%" />
</p>

<p align="center">
  <img src="docs/screenshots/mystery-gift.png" alt="Mystery Gift" width="49%" />
  <img src="docs/screenshots/bag.png" alt="Bag" width="49%" />
</p>

<p align="center">
  <img src="docs/screenshots/rng.png" alt="RNG tools" width="49%" />
</p>

<p align="center">
  <img src="docs/screenshots/trainer-card.png" alt="Trainer card" width="49%" />
  <img src="docs/screenshots/showdown-qr.png" alt="Showdown QR" width="49%" />
</p>

<p align="center">
  <img src="docs/screenshots/nuzlocke.png" alt="Nuzlocke report" width="49%" />
  <img src="docs/screenshots/encounters.png" alt="Encounter browser" width="49%" />
</p>

<p align="center">
  <img src="docs/screenshots/generate.png" alt="Generate a Pokémon" width="49%" />
  <img src="docs/screenshots/transfer.png" alt="Transfer preview" width="49%" />
</p>

<p align="center">
  <img src="docs/screenshots/autopilot.png" alt="Living Dex Autopilot" width="49%" />
  <img src="docs/screenshots/link.png" alt="Linking an emulator" width="49%" />
</p>

## Supported games

PKForge reads and edits saves from every mainline generation, plus the GameCube side
games and popular romhacks:

- **Generation I:** Red, Blue, Green (JP), Yellow
- **Generation II:** Gold, Silver, Crystal
- **Generation III:** Ruby, Sapphire, Emerald, FireRed, LeafGreen, Pokémon Box: Ruby & Sapphire, Colosseum, XD: Gale of Darkness
- **Generation IV:** Diamond, Pearl, Platinum, HeartGold, SoulSilver
- **Generation V:** Black, White, Black 2, White 2
- **Generation VI:** X, Y, Omega Ruby, Alpha Sapphire
- **Generation VII:** Sun, Moon, Ultra Sun, Ultra Moon, Let's Go Pikachu, Let's Go Eevee
- **Generation VIII:** Sword, Shield, Brilliant Diamond, Shining Pearl, Legends: Arceus
- **Generation IX:** Scarlet, Violet
- **Romhacks:** Pokémon Unbound, Radical Red, GS Chronicles, Luminescent Platinum, Pokémon Compass

## Supported emulators

Link a storage unit and PKForge finds your saves automatically:

- **Game Boy / Game Boy Color:** RetroArch, Linkboy, Pizza Boy C
- **Game Boy Advance:** RetroArch, Linkboy, Pizza Boy A
- **Nintendo DS:** melonDS, DraStic, RetroArch
- **GameCube:** Dolphin
- **Nintendo 3DS:** Azahar, Lime3DS, Citra MMJ
- **Nintendo Switch:** Eden

You can also open a single save file directly. On Linux and SteamOS, PKForge also links
standalone mGBA and finds EmuDeck and RetroDECK saves: see
[Linux and SteamOS](#linux-and-steamos-preview).

## Installation

Download the APK from [Releases](https://github.com/sofianeelhor/PKForge/releases) and
allow installs from unknown sources. First run walks you through linking an emulator or
opening a single save file.

New to PKForge? Start with [Getting Started](https://github.com/sofianeelhor/PKForge/wiki/Getting-Started).
If PKForge finds no saves, the folder picked is usually the wrong one: see
[Choosing the right folder](https://github.com/sofianeelhor/PKForge/wiki/Choosing-the-Right-Folder).

On Linux or SteamOS (a preview), build and install the Flatpak as described in
[Linux and SteamOS](#linux-and-steamos-preview).

## 💬 Discord

Join for updates, support, bug reports, feature requests, or just to chat about the project.

👉 **[Join the PKForge Discord](https://discord.gg/bMtzZmTDfu)**

[![Discord](https://discordapp.com/api/guilds/1542192456018427926/widget.png?style=banner3&time-)](https://discord.gg/XkeD2vKJCZ)

## Building

.NET 10 SDK with the `maui-android` workload, Android SDK (API 36).

```bash
git submodule update --init --recursive
dotnet test tests/PKForge.Domain.Tests/PKForge.Domain.Tests.csproj
dotnet test tests/PKForge.Engine.Tests/PKForge.Engine.Tests.csproj
dotnet build src/PKForge.App/PKForge.App.csproj -f net10.0-android
```

Version tags build and publish the APK from CI.

### Linux and SteamOS (preview)

PKForge also builds as a Linux desktop app, tuned for SteamOS handhelds in Game Mode
(tested on a Legion Go; the Steam Deck runs the same SteamOS). The desktop build runs the
same MAUI app on the [Avalonia MAUI backend](https://github.com/AvaloniaUI/Avalonia.Controls.Maui),
which is still a preview, so expect rough edges. Android builds are unaffected: the desktop target
is opt-in with `-p:PKForgeDesktop=true`.

#### Requirements

- Linux x86_64 with glibc. Tested on SteamOS, Arch and Debian, and as a Flatpak on
  Fedora; other desktop distributions should work too. The published build is
  self-contained, so the machine that runs it needs no .NET.
- To build: the **.NET 11 SDK** (a preview until .NET 11 ships) and the `maui-tizen`
  workload, which carries MAUI's Linux build targets:

  ```bash
  dotnet workload install maui-tizen
  ```

- To package a Flatpak: `flatpak` and `flatpak-builder`, with Flathub added as a remote.
- At runtime: an X11 session (Avalonia draws through X11; XWayland under Plasma and Game
  Mode is fine) and the usual `libICE`, `libSM` and `fontconfig`, which desktop
  distributions already have.

#### Build

```bash
git submodule update --init --recursive
dotnet publish src/PKForge.App/PKForge.App.csproj -p:PKForgeDesktop=true \
  -c Release -r linux-x64 --self-contained -o packaging/linux/pkforge-linux-x64
```

The folder `packaging/linux/pkforge-linux-x64` is the whole app: run `./pkforge.sh` from
it, or copy it anywhere. To share it as a single file, pack it on Linux so the executable
bits survive:

```bash
tar -C packaging/linux -czf pkforge-linux-x64.tar.gz pkforge-linux-x64
```

No .NET 11 SDK on your machine? The build also runs in a container, for example
`mcr.microsoft.com/dotnet/sdk:11.0` after `dotnet workload install maui-tizen`.

#### Package and install the Flatpak

The manifest at `packaging/linux/org.pkforge.app.yml` packages the published folder above,
so publish first. It uses the freedesktop 25.08 runtime and grants access to the emulator
folders PKForge looks in, plus `/run/media` for SD cards.

To install straight from the build:

```bash
flatpak-builder --user --install --install-deps-from=flathub --force-clean \
  build-dir packaging/linux/org.pkforge.app.yml
```

To make a single `PKForge.flatpak` file to copy to another device:

```bash
flatpak-builder --repo=repo --install-deps-from=flathub --force-clean \
  build-dir packaging/linux/org.pkforge.app.yml
flatpak build-bundle --runtime-repo=https://dl.flathub.org/repo/flathub.flatpakrepo \
  repo PKForge.flatpak org.pkforge.app
```

Then, on the device:

```bash
flatpak install --user PKForge.flatpak   # fetches the runtime from Flathub the first time
flatpak run org.pkforge.app
```

`.github/workflows/build.yml` has jobs that build the tarball and the Flatpak on each
push and attach both to tagged releases. They are new, so until they have run, build
locally as above.

#### SteamOS (Steam Deck, Legion Go)

A step-by-step Steam Deck guide, including playing the DS and GBA games in Pokémon Fulldex, is in
[packaging/linux/steamdeck/README.md](packaging/linux/steamdeck/README.md).

1. Switch to Desktop Mode (Steam button → Power → Switch to Desktop).
2. Copy `PKForge.flatpak` to the device, for example to Downloads with a USB stick, or over
   the network from Dolphin (`smb://your-pc/your-share`).
3. In Konsole, run `flatpak install --user ~/Downloads/PKForge.flatpak`. `--user` needs no
   password. When updating, answer `y` to reinstall.
4. Add it to Steam: right-click PKForge in the app menu and choose **Add to Steam**, or in
   Steam use **Games → Add a Non-Steam Game**.
5. Return to Game Mode and launch it from your library.

Use PKForge in **Game Mode**. There, Steam hands the controller to PKForge as a gamepad.
In Desktop Mode, Steam's desktop controller layout turns the controller into keyboard and
mouse input instead, so the controller only partly works there. Touch and a mouse work in
both.

If the UI looks tiny on a high-resolution screen (such as the Legion Go's 2560×1600), set
a scale once:

```bash
flatpak override --user --env=PKFORGE_SCALE=2 org.pkforge.app
```

To update, install the new `PKForge.flatpak` the same way: linked folders, the Bank and
settings are kept. To remove it, run `flatpak uninstall --user org.pkforge.app`. Its data
stays in `~/.var/app/org.pkforge.app` until you delete it.

#### Linking emulators on Linux

On first launch, PKForge links the emulator folders it finds, once each. Unlinking one
keeps it unlinked.

| Emulator | Folders it looks in |
| --- | --- |
| RetroArch | `~/.config/retroarch`, Flatpak `~/.var/app/org.libretro.RetroArch/config/retroarch`, Steam `~/.local/share/Steam/steamapps/common/RetroArch` |
| Dolphin | `~/.local/share/dolphin-emu`, Flatpak `~/.var/app/org.DolphinEmu.dolphin-emu/data/dolphin-emu` |
| Azahar, Lime3DS, Citra | `~/.local/share/azahar-emu`, `lime3ds-emu`, `citra-emu`, Flatpak `~/.var/app/org.azahar_emu.Azahar/data/azahar-emu` |
| Eden | `~/.local/share/eden`, Flatpak `~/.var/app/dev.eden_emu.eden/data/eden` |
| EmuDeck | `~/Emulation/saves/retroarch`, `melonds`, `dolphin`, `azahar`, `citra`, `eden` |
| RetroDECK | `~/retrodeck/saves` |

Symlinks are resolved, so an EmuDeck link into an emulator folder that is already linked
isn't listed twice. Anything else, such as an SD card or a custom folder, is linked by
hand: **Y (Link)** → platform → emulator → **Choose folder**.

**mGBA** (desktop only) is always linked by hand: **Link → Game Boy Advance** (or **Game
Boy / Game Boy Color**) **→ mGBA**, then pick its save folder. EmuDeck sets that to
`Emulation/saves/mgba/saves`, on internal storage (`~/Emulation`) or the SD card
(`/run/media/…/Emulation`). If mGBA has no save folder set, pick your ROM folder.

As on Android, save in game and close the emulator before editing, then load the save
normally afterwards, not a save state.

#### Controls on Linux

- **Controller**: read through SDL2, which is how Steam Input passes the controller in
  Game Mode. The d-pad and left stick navigate with the same repeat timing as Android, and
  the right stick moves the Poképark camera.
- **Touch and mouse**: tap to select, and drag a list to scroll. Game Mode delivers the
  touchscreen as a mouse, which is why PKForge scrolls on mouse drags.
- **Keyboard** (RetroArch's defaults): arrows to move, X = A, Z / Escape / Backspace = B,
  S = X, A = Y, Q / W = L / R, Enter = Start, Right Shift = Select, and F11 toggles
  fullscreen.

#### Options

| Variable | Effect |
| --- | --- |
| `PKFORGE_SCALE=2` | Scales the whole UI. Any factor works, such as `1.5`. |
| `PKFORGE_FULLSCREEN=1` | Starts fullscreen outside Game Mode too. In Game Mode it always is. |

For the Flatpak, set them with `flatpak override --user --env=NAME=VALUE org.pkforge.app`.
For the folder build, set them before `./pkforge.sh`.

#### Where things are

| | Flatpak | Folder build |
| --- | --- | --- |
| Bank, backups, settings | under `~/.var/app/org.pkforge.app` | `~/.config/PKForge.App` |
| Log | `…/PKForge.App/logs/pkforge.log` | `…/PKForge.App/logs/pkforge.log` |
| Shared and exported files | `~/Documents/PKForge` | `~/Documents/PKForge` |

#### Not available on Linux yet

- Background music, and the Bad Apple easter egg's sound (it plays silently).
- The second-screen inspector, the Poképark home-screen widget, and in-app updates (update
  the Flatpak instead).
- Sharing opens `~/Documents/PKForge` in the file manager instead of a share sheet.

#### Troubleshooting

- **No saves found**: check that the folder is one the Flatpak can read (the table above,
  `~/Emulation`, `~/retrodeck` or `/run/media`), or link it by hand with the picker.
- **The controller does nothing**: use Game Mode, and check the log for a
  `Controller connected` line (`grep pad …/logs/pkforge.log`).
- **The UI is tiny**: set `PKFORGE_SCALE`.
- **Anything else**: attach the log to your bug report.

### Layout

```
src/PKForge.Domain          contracts and DTOs, no engine or Android dependencies
src/PKForge.Engine          adapters over pinned PKHeX.Core
src/PKForge.AutoMod         compiles the Auto Legality Mod against our Core
src/PKForge.Infrastructure  bank, backups, atomic save writer
src/PKForge.App             MAUI app, SkiaSharp UI, gamepad and second screen
src/PKForge.App/Desktop     Linux/SteamOS platform services (Avalonia MAUI backend)
src/PKForge.Chrome          the design system: tokens and painters, pure Skia
tools/ChromePreview         renders the design system off-device
packaging/linux             Flatpak manifest, desktop entry, metainfo and icon
docs/                       architecture, bank model, development, art direction
```

## Credits

- [PKHeX](https://github.com/kwsch/PKHeX) by Kaphotics and every contributor over the
  years, the engine everything runs on
- [spritedmistery](https://www.instagram.com/spritedmistery/), the PKForge logo, the app's
  look, its color schemes and logos, its buttons, and the Poképark's map
- AlchemyBats, the tileset the Poképark's map is built on (tiles by AlchemyBats)
- [PKHeX-Plugins / Auto Legality Mod](https://github.com/santacrab2/PKHeX-Plugins), the
  offline legalizer built into the app
- [PKSM](https://github.com/FlagBrew/PKSM), the pixel chrome this UI builds on (GPL-3,
  see [UI attribution](src/PKForge.App/Resources/UI/ATTRIBUTION.md))
- [SteamGridDB](https://www.steamgriddb.com), cartridge icons, second-screen logos, and
  hero banners for the game library (community submissions; see
  [game art attribution](src/PKForge.App/Resources/GameArt/ATTRIBUTION.md))
- [Pokémon Showdown](https://pokemonshowdown.com) by [Smogon](https://www.smogon.com) and
  the artists of its sprite projects, front sprites and box icons
- [PMDCollab SpriteCollab](https://sprites.pmdcollab.org) and its artists, the Poképark's
  animated sprites and portraits (CC BY-NC 4.0), fetched at runtime and credited by name in
  the park
- [PokeAPI](https://pokeapi.co), item art fetched at runtime and cached on device, and the
  evolution data of the Poképark (BSD-3)
- [game-icons.net](https://game-icons.net) (CC-BY 3.0, © Lorc, Delapouite, Guard13007,
  Carl Olsen and other contributing artists), UI symbols
- [Bulbagarden Archives](https://archives.bulbagarden.net), Pokérus status sprites
- [Avalonia](https://avaloniaui.net) and its [MAUI backend](https://github.com/AvaloniaUI/Avalonia.Controls.Maui)
  (MIT), which draw the Linux build
- [SDL](https://www.libsdl.org) (zlib), game controllers on Linux, bundled from
  [Silk.NET's native build](https://github.com/dotnet/Silk.NET)
- Sprites and Pokémon names © Nintendo, Creatures Inc., GAME FREAK inc.

## License

GPLv3 or later, inherited from PKHeX.Core. See [LICENSE](LICENSE).
