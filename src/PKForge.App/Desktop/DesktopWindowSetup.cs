using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using PKForge.App.Services;

namespace PKForge.App;

/// <summary>
/// Desktop window behaviour: fullscreen in SteamOS Game Mode (maximized on a desktop, F11
/// toggles), game controllers through SDL (<see cref="DesktopGamepad"/>), and the keyboard as
/// a gamepad too.
/// Keys follow RetroArch's defaults: arrows, X = A, Z = B, S = X, A = Y, Q/W = L/R,
/// Enter = Start, Right Shift = Select, plus Escape/Backspace = B.
/// </summary>
internal static class DesktopWindowSetup
{
    private static readonly HashSet<Key> Held = [];

    public static void Attach(Microsoft.Maui.Controls.Window window) =>
        window.Created += (_, _) =>
        {
            if (DesktopWindow.Current is not Avalonia.Controls.Window main) return;
            main.WindowState = InGameMode() ? WindowState.FullScreen : WindowState.Maximized;
            main.AddHandler(InputElement.KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
            main.AddHandler(InputElement.KeyUpEvent, OnKeyUp, RoutingStrategies.Tunnel);
            DesktopGamepad.Start(window.Dispatcher);
        };

    /// <summary>Steam's Game Mode runs apps inside gamescope, which sets these.</summary>
    private static bool InGameMode() =>
        Environment.GetEnvironmentVariable("PKFORGE_FULLSCREEN") == "1"
        || Environment.GetEnvironmentVariable("GAMESCOPE_WAYLAND_DISPLAY") is { Length: > 0 }
        || Environment.GetEnvironmentVariable("SteamGamepadUI") == "1";

    private static void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.F11 && sender is Avalonia.Controls.Window window)
        {
            window.WindowState = window.WindowState == WindowState.FullScreen ? WindowState.Maximized : WindowState.FullScreen;
            e.Handled = true;
            return;
        }
        if (IsTyping(sender) || Resolve(e.Key) is not { } button) return;
        // The OS repeats a held key: directions and shoulders scroll, other buttons fire once.
        var repeat = !Held.Add(e.Key);
        if (repeat && button is not (PadButton.Up or PadButton.Down or PadButton.Left or PadButton.Right or PadButton.L or PadButton.R))
        {
            e.Handled = true;
            return;
        }
        var router = IPlatformApplication.Current?.Services.GetService<GamepadRouter>();
        e.Handled = router?.Dispatch(button) == true;
    }

    private static void OnKeyUp(object? sender, KeyEventArgs e)
    {
        Held.Remove(e.Key);
        if (IsTyping(sender) || Resolve(e.Key) is not { } button) return;
        IPlatformApplication.Current?.Services.GetService<GamepadRouter>()?.DispatchRelease(button);
    }

    /// <summary>A focused text box keeps every key, so a nickname can be typed.</summary>
    private static bool IsTyping(object? sender) =>
        sender is TopLevel top && top.FocusManager?.GetFocusedElement() is TextBox;

    private static PadButton? Resolve(Key key) => key switch
    {
        Key.X => PadButton.A,
        Key.Z or Key.Escape or Key.Back => PadButton.B,
        Key.S => PadButton.X,
        Key.A => PadButton.Y,
        Key.Q => PadButton.L,
        Key.W => PadButton.R,
        Key.Enter => PadButton.Start,
        Key.RightShift => PadButton.Select,
        Key.Up => PadButton.Up,
        Key.Down => PadButton.Down,
        Key.Left => PadButton.Left,
        Key.Right => PadButton.Right,
        _ => null,
    };
}
