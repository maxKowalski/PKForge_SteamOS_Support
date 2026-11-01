namespace PKForge.App;

/// <summary>Desktop has no launcher widgets: the park never has one to publish to.</summary>
public static class PokeparkWidgetPublisher
{
    public const int FrameWidth = 640;
    public const int FrameHeight = 360;
    public const int FrameCount = 8;

    public static bool HasActiveWidgets() => false;

    public static void Publish(IReadOnlyList<byte[]> pngFrames) { }
}
