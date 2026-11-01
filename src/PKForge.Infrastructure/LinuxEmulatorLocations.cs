using PKForge.Domain;

namespace PKForge.Infrastructure;

/// <summary>A folder found on this machine that can be linked as a watched root.</summary>
public sealed record FoundEmulatorFolder(EmulatorKind Kind, string Path, string DisplayName);

/// <summary>
/// Where Linux and SteamOS installs keep emulator data: native packages, Flatpaks, Steam's
/// RetroArch, EmuDeck (~/Emulation/saves) and RetroDECK (~/retrodeck/saves). Each folder is
/// given whole; the scanner already narrows a data folder down to its saves.
/// </summary>
public static class LinuxEmulatorLocations
{
    // Broad emulator folders come before the EmuDeck/RetroDECK links into them, so a link whose
    // real target is already covered is the one dropped.
    private static readonly (EmulatorKind Kind, string Relative, string Name)[] Candidates =
    [
        (EmulatorKind.RetroArch, ".config/retroarch", "RetroArch"),
        (EmulatorKind.RetroArch, ".var/app/org.libretro.RetroArch/config/retroarch", "RetroArch (Flatpak)"),
        (EmulatorKind.RetroArch, ".local/share/Steam/steamapps/common/RetroArch", "RetroArch (Steam)"),
        (EmulatorKind.Dolphin, ".local/share/dolphin-emu", "Dolphin"),
        (EmulatorKind.Dolphin, ".var/app/org.DolphinEmu.dolphin-emu/data/dolphin-emu", "Dolphin (Flatpak)"),
        (EmulatorKind.Azahar, ".local/share/azahar-emu", "Azahar"),
        (EmulatorKind.Azahar, ".var/app/org.azahar_emu.Azahar/data/azahar-emu", "Azahar (Flatpak)"),
        (EmulatorKind.Azahar, ".local/share/lime3ds-emu", "Lime3DS"),
        (EmulatorKind.Azahar, ".local/share/citra-emu", "Citra"),
        (EmulatorKind.Eden, ".local/share/eden", "Eden"),
        (EmulatorKind.Eden, ".var/app/dev.eden_emu.eden/data/eden", "Eden (Flatpak)"),
        (EmulatorKind.RetroArch, "Emulation/saves/retroarch", "RetroArch (EmuDeck)"),
        (EmulatorKind.MelonDS, "Emulation/saves/melonds", "melonDS (EmuDeck)"),
        (EmulatorKind.Dolphin, "Emulation/saves/dolphin", "Dolphin (EmuDeck)"),
        (EmulatorKind.Azahar, "Emulation/saves/azahar", "Azahar (EmuDeck)"),
        (EmulatorKind.Azahar, "Emulation/saves/citra", "Citra (EmuDeck)"),
        (EmulatorKind.Eden, "Emulation/saves/eden", "Eden (EmuDeck)"),
        (EmulatorKind.RetroArch, "retrodeck/saves", "RetroDECK"),
    ];

    /// <summary>
    /// The candidate folders that exist under <paramref name="home"/>, minus any whose real
    /// location (symlinks resolved) is the same as, or inside, one already listed for the same
    /// emulator, or inside one of <paramref name="linked"/> (roots the player already has).
    /// </summary>
    public static IReadOnlyList<FoundEmulatorFolder> Find(string home, IEnumerable<(EmulatorKind Kind, string Path)> linked)
    {
        var covered = linked.Select(root => (root.Kind, Real: RealPath(root.Path))).ToList();
        var found = new List<FoundEmulatorFolder>();
        foreach (var (kind, relative, name) in Candidates)
        {
            var path = System.IO.Path.Combine(home, relative);
            if (!Directory.Exists(path)) continue;
            var real = RealPath(path);
            if (covered.Any(c => c.Kind == kind && IsSameOrInside(real, c.Real))) continue;
            covered.Add((kind, real));
            found.Add(new FoundEmulatorFolder(kind, path, name));
        }
        return found;
    }

    private static bool IsSameOrInside(string path, string folder) =>
        path == folder || path.StartsWith(folder.TrimEnd('/') + "/", StringComparison.Ordinal);

    /// <summary>The path with every symlinked component resolved, like realpath(3).</summary>
    public static string RealPath(string path)
    {
        var full = System.IO.Path.GetFullPath(path);
        var root = System.IO.Path.GetPathRoot(full) ?? "/";
        var current = root;
        foreach (var segment in full[root.Length..].Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            current = System.IO.Path.Combine(current, segment);
            try
            {
                if (new DirectoryInfo(current).ResolveLinkTarget(returnFinalTarget: true) is { } target)
                    current = RealPath(target.FullName);
            }
            catch (IOException) { /* a broken link stays as written */ }
        }
        return current;
    }
}
