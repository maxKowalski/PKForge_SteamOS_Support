using PKForge.Domain;
using PKForge.Infrastructure;
using PKHeX.Core;
using Xunit;

namespace PKForge.Engine.Tests;

public sealed class EmulatorSaveScannerTests : IDisposable
{
    private readonly string _root = Directory.CreateTempSubdirectory("pkforge-scan-").FullName;

    public void Dispose() => Directory.Delete(_root, recursive: true);

    private sealed class MemoryCache : IScanCacheStore
    {
        public string? Json;
        public string? Load() => Json;
        public void Save(string json) => Json = json;
    }

    /// <summary>The filesystem tree, counting how many files the scanner actually opens.</summary>
    private sealed class CountingTree(string root) : ISaveTree
    {
        private readonly FileSystemSaveTree _inner = new(root);
        public int Opened;
        public string RootDocId => _inner.RootDocId;
        public string Describe => _inner.Describe;
        public IReadOnlyList<SaveTreeEntry> ListChildren(string docId) => _inner.ListChildren(docId);
        public Stream OpenRead(string docId) { Opened++; return _inner.OpenRead(docId); }
        public string DocumentIdFor(string docId) => _inner.DocumentIdFor(docId);
    }

    private sealed class TestScanner(IScanCacheStore cache, Func<string, ISaveTree> open)
        : EmulatorSaveScanner(new SaveEngine(), cache)
    {
        protected override ISaveTree OpenTree(string treeId) => open(treeId);
        protected override long InstallEpoch => 1;
    }

    private string Write(string relative, byte[] bytes)
    {
        var path = Path.Combine(_root, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllBytes(path, bytes);
        return path;
    }

    private static byte[] BlackSave() => BlankSaveFile.Get(GameVersion.B, "DECK", LanguageID.English).Write().ToArray();

    [Fact]
    public async Task RetroArchFolderFindsNestedSavesAndSkipsPrunedFolders()
    {
        var save = Write("saves/melonDS/Pokemon Black.sav", BlackSave());
        Write("states/Pokemon Black.sav", BlackSave());
        Write("saves/notes.sav", [1, 2, 3, 4]);

        var scanner = new TestScanner(new MemoryCache(), root => new FileSystemSaveTree(root));
        var result = await scanner.ScanAsync(_root, EmulatorKind.RetroArch);

        var found = Assert.Single(result.Saves);
        Assert.Equal(save, found.DocumentId);
        Assert.Equal(5, found.Generation);
        Assert.Equal("DECK", found.TrainerName);
        Assert.Equal("saves/melonDS", found.FolderHint);
        Assert.Contains("notes.sav", result.RejectedCandidates);
    }

    [Fact]
    public async Task MGbaEmuDeckFolderReadsBatterySavesNotStates()
    {
        // EmuDeck: Emulation/saves/mgba/{saves,states}; linking the mgba folder prefers saves/.
        var save = Write("mgba/saves/Pokemon Black.sav", BlackSave());
        Write("mgba/states/Pokemon Black.ss1", [5, 5, 5]);
        Write("mgba/states/Pokemon Black.sav", BlackSave());

        var scanner = new TestScanner(new MemoryCache(), root => new FileSystemSaveTree(root));
        var result = await scanner.ScanAsync(Path.Combine(_root, "mgba"), EmulatorKind.MGba);

        var found = Assert.Single(result.Saves);
        Assert.Equal(save, found.DocumentId);
        Assert.Equal(EmulatorKind.MGba, found.Emulator);
    }

    [Fact]
    public async Task UnchangedFilesAreAnsweredFromTheCacheOnTheNextScan()
    {
        Write("saves/Pokemon Black.sav", BlackSave());
        Write("saves/junk.sav", [9, 9, 9]);
        var cache = new MemoryCache();
        var tree = new CountingTree(_root);
        var scanner = new TestScanner(cache, _ => tree);

        var first = await scanner.ScanAsync(_root, EmulatorKind.RetroArch);
        Assert.Equal(2, tree.Opened);
        Assert.NotNull(cache.Json);

        // A fresh scanner on the persisted cache: the save and the rejection are both remembered.
        var reloaded = new TestScanner(cache, _ => tree);
        var second = await reloaded.ScanAsync(_root, EmulatorKind.RetroArch);
        Assert.Equal(2, tree.Opened);
        Assert.Equal(first.Saves.Select(s => s.DocumentId), second.Saves.Select(s => s.DocumentId));
    }

    [Fact]
    public async Task EdenFindsSavesUnderNandFromAnyGrantedLevel()
    {
        Write("nand/user/save/0000000000000000/0123456789ABCDEF0123456789ABCDEF/0100ABF008968000/main", [0, 1, 2]);
        var scanner = new TestScanner(new MemoryCache(), root => new FileSystemSaveTree(root));

        var fromRoot = await scanner.ScanAsync(_root, EmulatorKind.Eden);
        var fromUser = await scanner.ScanAsync(Path.Combine(_root, "nand", "user"), EmulatorKind.Eden);

        // The bytes are not a save, but the walk reaches the candidate from either level.
        Assert.Equal(1, fromRoot.FilesSeen);
        Assert.Equal(1, fromUser.FilesSeen);
        Assert.Equal(["main"], fromRoot.RejectedCandidates);
    }

    [Fact]
    public async Task MissingFolderFailsTheScanInsteadOfReportingNothing()
    {
        var scanner = new TestScanner(new MemoryCache(), root => new FileSystemSaveTree(root));
        await Assert.ThrowsAsync<DirectoryNotFoundException>(() =>
            scanner.ScanAsync(Path.Combine(_root, "nope"), EmulatorKind.RetroArch).AsTask());
    }
}
