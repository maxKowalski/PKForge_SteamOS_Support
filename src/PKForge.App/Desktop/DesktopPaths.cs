namespace PKForge.App;

internal static class DesktopPaths
{
    /// <summary>
    /// Avalonia's Essentials (11.0.0-rc.3) reports <c>FileSystem.AppDataDirectory</c> and its
    /// preferences file as paths relative to the working directory ("PKForge.App/..."), and a
    /// Steam or desktop launcher can start the app anywhere. Running from the XDG data folder
    /// puts the bank, backups, logs and settings in ~/.local/share/PKForge.App on every launch.
    /// Call before anything touches FileSystem or Preferences.
    /// </summary>
    public static void UseXdgDataHome()
    {
        var dataHome = Environment.GetEnvironmentVariable("XDG_DATA_HOME") is { Length: > 0 } xdg
            ? xdg
            : Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create);
        Directory.CreateDirectory(dataHome);
        Directory.SetCurrentDirectory(dataHome);
    }
}
