using PKForge.Domain;

namespace PKForge.App;

/// <summary>Desktop has no music player yet: the library stays empty and nothing plays.</summary>
public sealed class SilentMusicPlayer : IMusicPlayer
{
    public IReadOnlyList<MusicTrack> Library => [];
    public bool IsPlaying => false;
    public bool IsOn => false;
    public int? CurrentIndex => null;
    public MusicOrder Order => MusicOrder.InOrder;
    public MusicPlace Place { get; private set; }
    public bool Autostart => false;

    public int Add(IReadOnlyList<PickedDocument> documents) => 0;
    public void Remove(int index) { }
    public void Clear() { }
    public void Play() { }
    public void Pause() { }
    public void Skip() { }
    public void SetOrder(MusicOrder order) { }
    public void SetAutostart(bool on) { }
    public MusicSource SourceFor(MusicPlace place) => MusicSource.Default;
    public void SetSource(MusicPlace place, MusicSource source) { }
    public void EnterPlace(MusicPlace place) => Place = place;
}

/// <summary>Single-screen devices (Steam Deck, Legion Go): there is never a lower screen.</summary>
public sealed class NoSecondaryDisplay : ISecondaryDisplayHost
{
    public bool IsAvailable => false;
    public ValueTask ShowAsync(CancellationToken cancellationToken = default) => ValueTask.CompletedTask;
    public ValueTask DismissAsync(CancellationToken cancellationToken = default) => ValueTask.CompletedTask;
}
