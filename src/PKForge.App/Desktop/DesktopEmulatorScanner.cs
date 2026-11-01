using PKForge.App.Services;
using PKForge.Domain;
using PKForge.Infrastructure;

namespace PKForge.App;

/// <summary>Scans a desktop folder for Pokémon saves with the shared <see cref="EmulatorSaveScanner"/> rules.</summary>
public sealed class DesktopEmulatorScanner(ISaveEngine engine)
    : EmulatorSaveScanner(engine, new PreferencesScanCacheStore())
{
    protected override ISaveTree OpenTree(string treeId) => new FileSystemSaveTree(treeId);

    /// <summary>
    /// Identifies this build (the app assembly's module id, new with every code change). Not a
    /// file time: Flatpak installs every file with the same zero timestamp.
    /// </summary>
    protected override long InstallEpoch { get; } =
        BitConverter.ToInt64(typeof(DesktopEmulatorScanner).Assembly.ManifestModule.ModuleVersionId.ToByteArray());
}
