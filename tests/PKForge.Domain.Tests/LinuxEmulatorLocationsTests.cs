using PKForge.Domain;
using PKForge.Infrastructure;
using Xunit;

namespace PKForge.Domain.Tests;

public sealed class LinuxEmulatorLocationsTests : IDisposable
{
    private readonly string _home = Directory.CreateTempSubdirectory("pkforge-home-").FullName;

    public void Dispose() => Directory.Delete(_home, recursive: true);

    private string Folder(string relative) => Directory.CreateDirectory(Path.Combine(_home, relative)).FullName;

    [Fact]
    public void FindsOnlyFoldersThatExist()
    {
        Folder(".var/app/org.libretro.RetroArch/config/retroarch");
        Folder(".local/share/dolphin-emu");

        var found = LinuxEmulatorLocations.Find(_home, []);

        Assert.Equal([EmulatorKind.RetroArch, EmulatorKind.Dolphin], found.Select(f => f.Kind));
        Assert.Equal("RetroArch (Flatpak)", found[0].DisplayName);
    }

    [Fact]
    public void EmuDeckLinkIntoAnEmulatorFolderIsNotListedTwice()
    {
        if (!OperatingSystem.IsLinux()) return; // symlinks need no privilege only on Linux
        var flatpakSaves = Folder(".var/app/org.libretro.RetroArch/config/retroarch/saves");
        Folder("Emulation/saves");
        Directory.CreateSymbolicLink(Path.Combine(_home, "Emulation/saves/retroarch"), flatpakSaves);

        var found = LinuxEmulatorLocations.Find(_home, []);

        var only = Assert.Single(found);
        Assert.Equal("RetroArch (Flatpak)", only.DisplayName);
    }

    [Fact]
    public void FolderInsideOneAlreadyLinkedIsSkippedButOtherEmulatorsAreNot()
    {
        Folder(".config/retroarch");
        Folder(".local/share/azahar-emu");

        var found = LinuxEmulatorLocations.Find(_home, [(EmulatorKind.RetroArch, _home)]);

        Assert.Equal([EmulatorKind.Azahar], found.Select(f => f.Kind));
    }
}
