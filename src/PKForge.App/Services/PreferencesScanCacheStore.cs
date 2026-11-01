using PKForge.Infrastructure;

namespace PKForge.App.Services;

/// <summary>The scan parse cache in MAUI preferences, under the key earlier releases used.</summary>
public sealed class PreferencesScanCacheStore : IScanCacheStore
{
    // Detection support can expand between releases. Keep cached negative
    // results versioned so a newly supported save is always retried once.
    private const string Key = "scan_cache_v5"; // v5: cards re-read after the out-of-range party count fix

    public string? Load() => Preferences.Default.Get(Key, string.Empty);

    public void Save(string json) => Preferences.Default.Set(Key, json);
}
