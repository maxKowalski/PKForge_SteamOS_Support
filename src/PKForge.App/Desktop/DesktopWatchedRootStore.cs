using System.Text.Json;
using PKForge.App.Services;
using PKForge.Domain;
using PKForge.Infrastructure;

namespace PKForge.App;

/// <summary>
/// The watched roots, plus the emulator folders found on this machine
/// (<see cref="LinuxEmulatorLocations"/>): each new one is linked the first time the roots are
/// read in a session. A folder is offered once ever, so removing it keeps it removed.
/// </summary>
public sealed class DesktopWatchedRootStore(PreferencesWatchedRootStore inner) : IWatchedRootStore
{
    private const string OfferedKey = "desktop_auto_roots_offered";
    private readonly Lock _gate = new();
    private bool _detected;

    public IReadOnlyList<WatchedRoot> GetRoots()
    {
        lock (_gate)
        {
            if (!_detected)
            {
                _detected = true;
                try { LinkNewFolders(); }
                catch (Exception error) when (error is IOException or UnauthorizedAccessException or JsonException)
                {
                    AppLog.Warn("scan", $"Looking for emulator folders failed: {error.Message}");
                }
            }
        }
        return inner.GetRoots();
    }

    public void AddRoot(WatchedRoot root) => inner.AddRoot(root);

    public void RemoveRoot(WatchedRoot root) => inner.RemoveRoot(root);

    private void LinkNewFolders()
    {
        var offered = JsonSerializer.Deserialize<HashSet<string>>(Preferences.Default.Get(OfferedKey, "[]")) ?? [];
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var linked = inner.GetRoots().Select(root => (root.Kind, root.TreeId));
        var added = 0;
        foreach (var folder in LinuxEmulatorLocations.Find(home, linked))
        {
            if (!offered.Add(folder.Path)) continue;
            inner.AddRoot(new WatchedRoot(folder.Kind, folder.Path, folder.DisplayName));
            AppLog.Info("scan", $"Linked {folder.DisplayName} at {folder.Path}");
            added++;
        }
        if (added > 0)
            Preferences.Default.Set(OfferedKey, JsonSerializer.Serialize(offered));
    }
}
