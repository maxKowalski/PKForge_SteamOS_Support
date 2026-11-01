namespace PKForge.App.Views;

internal static class ScrollIntoView
{
    /// <summary>
    /// Scrolls just enough to show <paramref name="view"/>, for pad focus moving through a list.
    /// On desktop the Avalonia backend does not honour ScrollToAsync(element, MakeVisible), so the
    /// view's own Avalonia control asks its scroll viewer to bring it into view instead.
    /// </summary>
    public static void MakeVisible(this ScrollView scroll, View view)
    {
#if ANDROID
        _ = scroll.ScrollToAsync(view, ScrollToPosition.MakeVisible, false);
#else
        if (view.Handler?.PlatformView is Avalonia.Controls.Control control)
            Avalonia.Controls.ControlExtensions.BringIntoView(control);
#endif
    }
}
