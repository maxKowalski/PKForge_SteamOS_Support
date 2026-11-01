using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;

namespace PKForge.App;

// Desktop stand-ins for the MAUI Essentials APIs the Avalonia backend does not implement yet
// (it covers FilePicker, FileSystem, HapticFeedback, MediaPicker and Preferences). Like
// MainThread, each type wins name lookup inside the PKForge.App namespaces, so the shared
// call sites compile unchanged; only the members PKForge uses exist.

/// <summary>The app's main Avalonia window, for the services that need one.</summary>
internal static class DesktopWindow
{
    public static TopLevel? Current =>
        (Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow is { } window
            ? TopLevel.GetTopLevel(window)
            : null;

    public static TopLevel Required =>
        Current ?? throw new InvalidOperationException("The main window is not open yet.");

    /// <summary>Opens a file, folder or URL with the desktop's default handler.</summary>
    public static bool Open(string target)
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo("xdg-open") { ArgumentList = { target }, UseShellExecute = false });
            return process is not null;
        }
        catch (System.ComponentModel.Win32Exception)
        {
            return false;
        }
    }
}

public static class AppInfo
{
    public static Info Current { get; } = new();

    public sealed class Info
    {
        private static readonly Version Version = typeof(App).Assembly.GetName().Version ?? new Version(0, 0, 0);

        public string VersionString { get; } = $"{Version.Major}.{Version.Minor}.{Version.Build}";
        public string BuildString => VersionString;
        public string PackageName => "org.pkforge.app";
    }
}

public static class DeviceDisplay
{
    /// <summary>
    /// The screen the window is on, in physical pixels with its scale factor, the way Android
    /// reports its display: PKForge sizes its chrome from the screen, not the window.
    /// </summary>
    public static DisplayInfo MainDisplayInfo
    {
        get
        {
            var window = DesktopWindow.Current;
            var screen = window is null ? null : window.Screens?.ScreenFromTopLevel(window) ?? window.Screens?.Primary;
            return screen is null
                ? new DisplayInfo(1280, 800, 1, DisplayOrientation.Landscape, DisplayRotation.Rotation0)
                : new DisplayInfo(screen.Bounds.Width, screen.Bounds.Height, screen.Scaling,
                    screen.Bounds.Width >= screen.Bounds.Height ? DisplayOrientation.Landscape : DisplayOrientation.Portrait,
                    DisplayRotation.Rotation0);
        }
    }
}

public static class Launcher
{
    public static Task<bool> OpenAsync(string uri) => Task.FromResult(DesktopWindow.Open(uri));

    public static Task<bool> OpenAsync(Uri uri) => OpenAsync(uri.ToString());
}

public static class Clipboard
{
    public static Impl Default { get; } = new();

    public sealed class Impl
    {
        public async Task SetTextAsync(string? text)
        {
            if (DesktopWindow.Current?.Clipboard is { } clipboard)
                await clipboard.SetTextAsync(text ?? "");
        }
    }
}

/// <summary>
/// Desktop has no share sheet: shared files are copied to Documents/PKForge and that folder is
/// opened in the file manager, where they can be sent on.
/// </summary>
public static class Share
{
    public static Impl Default { get; } = new();

    public sealed class Impl
    {
        private static string Folder => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments, Environment.SpecialFolderOption.Create), "PKForge");

        public Task RequestAsync(ShareFileRequest request) => Export(request.File is { } file ? [file] : []);

        public Task RequestAsync(ShareMultipleFilesRequest request) => Export(request.Files ?? []);

        private static Task Export(IEnumerable<ShareFile> files)
        {
            Directory.CreateDirectory(Folder);
            foreach (var file in files)
                File.Copy(file.FullPath, Path.Combine(Folder, Path.GetFileName(file.FullPath)), overwrite: true);
            DesktopWindow.Open(Folder);
            return Task.CompletedTask;
        }
    }
}

/// <summary>Desktop is treated as online over a wired link; downloads handle their own failures.</summary>
public static class Connectivity
{
    public static Impl Current { get; } = new();

    public sealed class Impl
    {
        public NetworkAccess NetworkAccess => NetworkAccess.Internet;
        public IEnumerable<ConnectionProfile> ConnectionProfiles => [ConnectionProfile.Ethernet];
    }
}
