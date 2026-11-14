using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Point = Avalonia.Point;
using PointerEventArgs = Avalonia.Input.PointerEventArgs;

namespace PKForge.App;

/// <summary>
/// Drag-to-scroll with the mouse, as a finger scrolls on Android. SteamOS Game Mode hands the
/// touchscreen to non-Steam apps as a mouse, and Avalonia scrolls only for real touch, so
/// without this the lists cannot be dragged there. The backend fires a MAUI tap on press, so a
/// mouse press inside a scrollable list is held back until it is clear what it is: moving past
/// the slop makes it a drag (the row never sees it), letting go or holding still replays it to
/// the row as the press it was (taps land on release, long presses after a short hold). Real
/// touch is left to Avalonia's own scrolling.
/// </summary>
internal static class DesktopDragScroll
{
    private const double Slop = 10;
    private static readonly TimeSpan HoldToPress = TimeSpan.FromMilliseconds(250);

    private static ScrollViewer? _target;
    private static PointerPressedEventArgs? _held;
    private static Interactive? _heldSource;
    private static Point _start;
    private static Vector _startOffset;
    private static bool _dragging;
    private static bool _replaying;
    private static int _press;

    public static void Attach(Avalonia.Controls.Window window)
    {
        window.AddHandler(InputElement.PointerPressedEvent, OnPressed, RoutingStrategies.Tunnel);
        window.AddHandler(InputElement.PointerMovedEvent, OnMoved, RoutingStrategies.Tunnel);
        window.AddHandler(InputElement.PointerReleasedEvent, OnReleased, RoutingStrategies.Tunnel);
    }

    private static void OnPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_replaying) return;
        Reset();
        if (e.Pointer.Type != PointerType.Mouse || !e.GetCurrentPoint(null).Properties.IsLeftButtonPressed) return;
        if (e.Source is not Interactive source || ScrollableAncestor(e.Source as Visual) is not { } scroll) return;
        _target = scroll;
        _start = e.GetPosition(scroll);
        _startOffset = scroll.Offset;
        _held = e;
        _heldSource = source;
        e.Handled = true; // held: the row's tap would fire right now
        var press = ++_press;
        Avalonia.Threading.DispatcherTimer.RunOnce(() => { if (press == _press && !_dragging) Replay(); }, HoldToPress);
    }

    private static void OnMoved(object? sender, PointerEventArgs e)
    {
        if (_target is not { } scroll) return;
        var delta = e.GetPosition(scroll) - _start;
        var (vertical, horizontal) = CanScroll(scroll);
        if (!_dragging)
        {
            if (_held is null) return; // already handed to the row
            if ((vertical ? Math.Abs(delta.Y) : Math.Abs(delta.X)) < Slop) return;
            _dragging = true;
            _held = null;
            e.Pointer.Capture(scroll);
        }
        scroll.Offset = new Vector(
            horizontal && !vertical ? _startOffset.X - delta.X : _startOffset.X,
            vertical ? _startOffset.Y - delta.Y : _startOffset.Y);
        e.Handled = true;
    }

    private static void OnReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_dragging)
        {
            e.Pointer.Capture(null);
            e.Handled = true;
        }
        else
        {
            Replay(); // a tap: the row gets its press now, then this release
        }
        Reset();
    }

    /// <summary>Hands the held press to the row it landed on.</summary>
    private static void Replay()
    {
        if (_held is not { } press || _heldSource is not { } source) return;
        _held = null;
        press.Handled = false;
        _replaying = true;
        try { source.RaiseEvent(press); }
        finally { _replaying = false; }
    }

    private static void Reset()
    {
        _press++;
        _target = null;
        _held = null;
        _heldSource = null;
        _dragging = false;
    }

    /// <summary>The nearest scroll viewer that actually has something to scroll.</summary>
    private static ScrollViewer? ScrollableAncestor(Visual? visual)
    {
        for (; visual is not null; visual = visual.GetVisualParent())
        {
            if (visual is ScrollViewer scroll && CanScroll(scroll) is (true, _) or (_, true))
                return scroll;
        }
        return null;
    }

    private static (bool Vertical, bool Horizontal) CanScroll(ScrollViewer scroll) =>
        (scroll.Extent.Height > scroll.Viewport.Height + 1, scroll.Extent.Width > scroll.Viewport.Width + 1);
}
