using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Channels;
using PKForge.Domain;

namespace PKForge.Infrastructure;

/// <summary>An entry of a folder tree being scanned: a SAF document on Android, a path on desktop.</summary>
public sealed record SaveTreeEntry(string DocId, string Name, bool IsDirectory, DateTimeOffset? LastModified);

/// <summary>
/// A granted folder as the scanner sees it. Document ids are the platform's own (SAF document
/// ids, absolute paths); <see cref="DocumentIdFor"/> turns one into the id saves are opened by.
/// </summary>
public interface ISaveTree
{
    string RootDocId { get; }
    /// <summary>One line for the scan diagnostics: which folder this is.</summary>
    string Describe { get; }
    IReadOnlyList<SaveTreeEntry> ListChildren(string docId);
    Stream OpenRead(string docId);
    string DocumentIdFor(string docId);
}

/// <summary>Where the parse cache is kept between launches (MAUI preferences in the app).</summary>
public interface IScanCacheStore
{
    string? Load();
    void Save(string json);
}

/// <summary>
/// Walks a granted folder tree looking for parseable Pokémon saves. File-name filters are only a
/// pre-filter; every candidate's bytes must pass engine validation before being reported. The
/// platforms supply the tree (<see cref="ISaveTree"/>); the rules are the same everywhere.
/// </summary>
public abstract class EmulatorSaveScanner(ISaveEngine engine, IScanCacheStore cacheStore) : IIncrementalEmulatorDetectionService
{
    private const int EdenMaxDepth = 8;
    private readonly ScanCache _cache = new(cacheStore);

    /// <summary>Opens the folder a tree id names; throws when it cannot be read.</summary>
    protected abstract ISaveTree OpenTree(string treeId);

    /// <summary>
    /// Changes when the app is installed or updated: part of every cache key, so each new build
    /// re-reads every file once and detection improvements reach unchanged saves.
    /// </summary>
    protected abstract long InstallEpoch { get; }

    private sealed class ScanState
    {
        public int FilesSeen;
        /// <summary>The emulator this root was linked for; decides which folders are pruned.</summary>
        public EmulatorKind Kind;
        /// <summary>Names in the folder currently being walked, for ROM-name hints.</summary>
        public IReadOnlyList<string>? Siblings;
        public readonly List<string> Rejected = [];
        public readonly List<string> Diagnostics = [];

        public void Trace(string message)
        {
            const int maxLines = 2000;
            if (Diagnostics.Count < maxLines)
                Diagnostics.Add(message);
            else if (Diagnostics.Count == maxLines)
                Diagnostics.Add("DIAGNOSTICS TRUNCATED after 2000 lines");
        }
    }

    public ValueTask<EmulatorScanResult> ScanAsync(string treeId, EmulatorKind kind, CancellationToken cancellationToken = default)
    {
        return new ValueTask<EmulatorScanResult>(Task.Run(() =>
            ScanCore(treeId, kind, cancellationToken, null), cancellationToken));
    }

    public async IAsyncEnumerable<EmulatorScanUpdate> ScanIncrementalAsync(
        string treeId, EmulatorKind kind,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var channel = Channel.CreateUnbounded<EmulatorScanUpdate>(
            new UnboundedChannelOptions { SingleWriter = true, SingleReader = true });
        var producer = Task.Run(() =>
        {
            try
            {
                var result = ScanCore(treeId, kind, cancellationToken,
                    save => channel.Writer.TryWrite(new EmulatorScanUpdate(save, false)));
                channel.Writer.TryWrite(new EmulatorScanUpdate(
                    null, true, result.FilesSeen, result.Saves.Count,
                    result.RejectedCandidates, result.Diagnostics));
                _cache.Flush();
                channel.Writer.TryComplete();
            }
            catch (Exception error)
            {
                channel.Writer.TryComplete(error);
            }
        });

        await foreach (var update in channel.Reader.ReadAllAsync(cancellationToken))
            yield return update;
        await producer;
    }

    private EmulatorScanResult ScanCore(
        string treeId, EmulatorKind kind, CancellationToken cancellationToken,
        Action<DetectedSave>? onSave)
    {
        var tree = OpenTree(treeId);
        var state = new ScanState { Kind = kind };
        state.Trace($"Scanner={GetType().Name} kind={kind}");
        state.Trace(tree.Describe);
        state.Trace($"RootDocId={tree.RootDocId}");
        List<DetectedSave> found;
        try
        {
            found = kind switch
            {
                EmulatorKind.RetroArch or EmulatorKind.MelonDS or EmulatorKind.Linkboy or
                EmulatorKind.DraStic or EmulatorKind.PizzaBoyGba or EmulatorKind.PizzaBoyGbc or EmulatorKind.Dolphin or
                EmulatorKind.MGba =>
                    ScanFlatFolder(tree, kind, cancellationToken, state, onSave),
                EmulatorKind.Eden => ScanEden(tree, cancellationToken, state, onSave),
                EmulatorKind.Azahar or EmulatorKind.CitraMmj => ScanThreeDs(tree, kind, cancellationToken, state, onSave),
                _ => [],
            };
        }
        catch (Exception error)
        {
            if (error is OperationCanceledException)
                throw;
            state.Trace($"SCAN EXCEPTION {error}");
            found = [];
        }
        state.Trace($"Scanner complete files={state.FilesSeen} saves={found.Count} rejected={state.Rejected.Count}");
        _cache.Flush();
        return new EmulatorScanResult(EmulatorSaveHeuristics.Normalize(found), state.FilesSeen,
            [.. state.Rejected], [.. state.Diagnostics]);
    }

    /// <summary>Emulator folders whose contents can never be saves - pruned so a whole-RetroArch grant stays fast.</summary>
    private static readonly HashSet<string> PrunedDirectories = new(StringComparer.OrdinalIgnoreCase)
    {
        "assets", "autoconfig", "cheats", "config", "cores", "database", "downloads", "filters",
        "info", "logs", "overlays", "playlists", "remaps", "screenshots", "shaders", "states",
        "system", "thumbnails", "cache", "temp", "roms", "shader_cache",
    };

    // RetroArch keeps ROMs apart from its saves folder, so skipping "roms" keeps a whole-folder
    // grant fast. Standalone emulators (melonDS, DraStic, Pizza Boy...) save beside the ROM.
    private static bool IsPruned(string folder, EmulatorKind kind) =>
        PrunedDirectories.Contains(folder)
        && (kind == EmulatorKind.RetroArch || !string.Equals(folder, "roms", StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Battery saves and GCI files can be nested: RetroArch uses per-core folders,
    /// DraStic uses backup/, and Dolphin uses GC/&lt;region&gt;/Card A or Card B.
    /// Prefer the emulator's save subtrees when its whole data folder was granted.
    /// Direct save-folder grants and custom locations are also supported.
    /// </summary>
    private List<DetectedSave> ScanFlatFolder(ISaveTree tree, EmulatorKind kind,
        CancellationToken cancellationToken, ScanState state, Action<DetectedSave>? onSave)
    {
        const int maxDepth = 8;
        var results = new List<DetectedSave>();
        void OnFile(SaveTreeEntry child)
        {
            state.FilesSeen++;
            if (kind == EmulatorKind.Dolphin && EmulatorSaveHeuristics.IsDolphinMemoryCard(child.Name))
            {
                state.Rejected.Add($"{child.Name}: export Colosseum/XD as GCI with Dolphin's Memory Card Manager, or use GCI Folder for the card slot.");
                state.Trace($"RAW MEMORY CARD {child.Name}: individual GCI export required");
                return;
            }
            if (!EmulatorSaveHeuristics.IsCandidateFileName(child.Name, kind)) return;
            if (TryDetect(tree, child, kind, gameLabel: child.Name, state: state) is { } save)
            {
                results.Add(save);
                onSave?.Invoke(save);
            }
            else
                state.Rejected.Add(child.Name);
        }

        var rootChildren = tree.ListChildren(tree.RootDocId);
        string[] preferredFolders = kind switch
        {
            EmulatorKind.Dolphin => ["GC"],
            EmulatorKind.DraStic => ["backup"],
            EmulatorKind.PizzaBoyGba or EmulatorKind.PizzaBoyGbc => ["save", "saves"],
            _ => ["saves"],
        };
        var savesDirs = rootChildren.Where(x => x.IsDirectory && preferredFolders.Contains(x.Name, StringComparer.OrdinalIgnoreCase)).ToArray();
        if (savesDirs.Length > 0)
        {
            state.Siblings = rootChildren.Where(x => !x.IsDirectory).Select(x => x.Name).ToArray();
            foreach (var file in rootChildren.Where(x => !x.IsDirectory))
                OnFile(file);
            foreach (var savesDir in savesDirs)
                FindFilesRecursive(tree, savesDir.DocId, maxDepth, cancellationToken, OnFile, state);
        }
        else
        {
            FindFilesRecursive(tree, tree.RootDocId, maxDepth, cancellationToken, OnFile, state);
        }
        return results;
    }

    /// <summary>
    /// Eden (Switch): saves are files named "main" (or "*.bin" for BDSP) under nand/user/save/…
    /// The user may have granted the files root, nand/, or user/ - try each prefix.
    /// </summary>
    private List<DetectedSave> ScanEden(ISaveTree tree, CancellationToken cancellationToken,
        ScanState state, Action<DetectedSave>? onSave)
    {
        string[][] prefixes = [["nand", "user", "save"], ["user", "save"], ["save"]];
        string? saveDirDocId = null;
        foreach (var prefix in prefixes)
        {
            state.Trace($"Trying Eden prefix {string.Join('/', prefix)}");
            saveDirDocId = NavigatePath(tree, tree.RootDocId, prefix, state.Trace);
            state.Trace(saveDirDocId is null ? "Prefix not found" : $"Save root found: {saveDirDocId}");
            if (saveDirDocId is not null)
                break;
        }
        if (saveDirDocId is null)
        {
            state.Trace("EDEN FAILURE: none of nand/user/save, user/save, or save exists under the granted root");
            return [];
        }

        var results = new List<DetectedSave>();
        FindFilesRecursive(tree, saveDirDocId, EdenMaxDepth, cancellationToken, child =>
        {
            state.FilesSeen++;
            if (!EmulatorSaveHeuristics.IsEdenSaveFileName(child.Name))
            {
                state.Trace($"SKIP file name={child.Name} docId={child.DocId}");
                return;
            }
            state.Trace($"CANDIDATE name={child.Name} modified={child.LastModified?.ToString("O") ?? "unknown"} docId={child.DocId}");
            var label = EmulatorSaveHeuristics.GuessSwitchGameLabel(child.DocId);
            if (TryDetect(tree, child, EmulatorKind.Eden, label, state) is { } save)
            {
                results.Add(save);
                onSave?.Invoke(save);
            }
            else
                state.Rejected.Add(child.Name);
        }, state, diagnostic: true);
        return results;
    }

    /// <summary>
    /// Citra-family 3DS (Azahar, Lime3DS, Citra MMJ): sdmc/Nintendo 3DS/&lt;ID0&gt;/&lt;ID1&gt;/title/00040000/&lt;game&gt;/data/00000001/main,
    /// with the root resolved from whichever level the user granted (see <see cref="ThreeDsSaveLayout"/>).
    /// </summary>
    private List<DetectedSave> ScanThreeDs(ISaveTree tree, EmulatorKind kind,
        CancellationToken cancellationToken, ScanState state, Action<DetectedSave>? onSave)
    {
        var results = new List<DetectedSave>();
        IEnumerable<TreeEntry<SaveTreeEntry>> List(SaveTreeEntry doc) =>
            tree.ListChildren(doc.DocId).Select(x => new TreeEntry<SaveTreeEntry>(x.Name, x.IsDirectory, x));
        var rootName = ThreeDsSaveLayout.LastSegment(tree.RootDocId);
        var root = new SaveTreeEntry(tree.RootDocId, rootName, true, null);
        foreach (var hit in ThreeDsSaveLayout.FindMainSaves(root, rootName, List, state.Trace, cancellationToken))
        {
            state.FilesSeen++;
            var main = hit.File;
            if (TryDetect(tree, main, kind, gameLabel: $"3DS save ({hit.TitleId})", state: state) is { } save)
            {
                results.Add(save);
                onSave?.Invoke(save);
            }
            else
                state.Rejected.Add($"3DS save {hit.TitleId}");
        }
        return results;
    }

    /// <summary>Reads a candidate's bytes and reports it only if the engine can parse them.</summary>
    private DetectedSave? TryDetect(ISaveTree tree, SaveTreeEntry child, EmulatorKind kind,
        string gameLabel, ScanState state)
    {
        try
        {
            var documentId = tree.DocumentIdFor(child.DocId);
            state.Trace($"DocumentId={documentId}");

            // A file already parsed at this modification time never gets re-read: rescans
            // are instant. The install epoch in the key makes each fresh install re-read every
            // file exactly once, so detection improvements (new romhack labels, say) reach
            // saves whose timestamps have not changed since the previous install.
            var cacheKey = documentId + "#" + kind + "#" + InstallEpoch;
            var modifiedTicks = child.LastModified?.UtcTicks ?? 0;
            if (_cache.TryGet(cacheKey, modifiedTicks, out var cached))
            {
                state.Trace(cached is null ? "CACHED rejection (unchanged since it last failed to parse)" : $"CACHED {cached.GameLabel}");
                return cached;
            }

            using var stream = tree.OpenRead(child.DocId);
            // Saves are small; a matching extension on a ROM/archive must not stall the scan.
            const long maxSaveBytes = 32 * 1024 * 1024;
            using var buffer = new MemoryStream();
            var chunk = new byte[81920];
            int read;
            while ((read = stream.Read(chunk, 0, chunk.Length)) > 0)
            {
                buffer.Write(chunk, 0, read);
                if (buffer.Length > maxSaveBytes)
                {
                    state.Trace($"REJECT larger than {maxSaveBytes} bytes");
                    return null;
                }
            }
            if (buffer.Length == 0)
            {
                state.Trace("REJECT zero-byte file");
                return null;
            }

            var bytes = buffer.ToArray();
            var headerLength = Math.Min(16, bytes.Length);
            var bdspSize = bytes.Length is 956456 or 973856 or 978316 or 979108;
            state.Trace($"BYTES length={bytes.Length} header16={Convert.ToHexString(bytes.AsSpan(0, headerLength))} knownBdspSize={bdspSize}");

            // Describe consumes a copy: the engine decrypts Switch saves in place during parsing.
            var description = engine.TryDescribe(bytes, child.Name);
            if (description is null)
            {
                state.Trace("PARSER REJECTED: ISaveEngine.TryDescribe returned null");
                // Remembering a rejection keeps big RetroArch folders fast. A 3DS or Switch save
                // is one known file per game: always re-read it, so a read that caught the
                // emulator mid-write does not hide the game until the file changes again.
                if (!EmulatorSaveHeuristics.RequiresExtraCare(kind))
                    _cache.Store(cacheKey, modifiedTicks, null);
                return null;
            }
            state.Trace($"PARSER ACCEPTED game={description.GameName} generation={description.Generation} trainer={description.TrainerName} playTime={description.PlayTime}");

            // The label comes from the bytes alone; the save/ROM name is only a Rename prefill.
            var romName = EmulatorSaveHeuristics.FindSiblingRom(child.Name, state.Siblings);
            var guess = SaveIdentityRules.Guess(description.GameName, description.Generation, child.Name, romName, gameLabel);
            state.Trace($"IDENTITY label={guess.Label} family={guess.Family} format={guess.Format} language={description.Language ?? "-"} rom={romName ?? "-"}");
            var detected = new DetectedSave(
                documentId,
                child.Name,
                guess.Label,
                kind,
                EmulatorSaveHeuristics.RequiresExtraCare(kind),
                child.LastModified,
                description.Generation,
                description.TrainerName,
                description.PlayTime,
                guess,
                romName,
                EmulatorSaveHeuristics.FolderHint(child.DocId),
                Language: description.Language);
            _cache.Store(cacheKey, modifiedTicks, detected);
            return detected;
        }
        catch (Exception error)
        {
            state.Trace($"CANDIDATE EXCEPTION {error}");
            return null; // unreadable candidates are simply not saves
        }
    }

    /// <summary>
    /// Persistent parse cache keyed by document id + last-modified time. A null entry
    /// records "this file is not a save" so rescans skip reading it entirely.
    /// </summary>
    private sealed class ScanCache(IScanCacheStore store)
    {
        private const int MaxEntries = 512;
        private Dictionary<string, CacheEntry>? _entries;
        private readonly Lock _gate = new();
        private bool _dirty;

        private sealed record CacheEntry(long ModifiedTicks, DetectedSave? Save);

        public bool TryGet(string documentId, long modifiedTicks, out DetectedSave? save)
        {
            lock (_gate)
            {
                Load();
                if (_entries!.TryGetValue(documentId, out var entry) && entry.ModifiedTicks == modifiedTicks && modifiedTicks != 0)
                {
                    save = entry.Save;
                    return true;
                }
            }
            save = null;
            return false;
        }

        public void Store(string documentId, long modifiedTicks, DetectedSave? save)
        {
            if (modifiedTicks == 0) return; // no timestamp, no safe caching
            lock (_gate)
            {
                Load();
                _entries![documentId] = new CacheEntry(modifiedTicks, save);
                while (_entries.Count > MaxEntries)
                    _entries.Remove(_entries.Keys.First());
                _dirty = true;
            }
        }

        public void Flush()
        {
            lock (_gate)
            {
                if (!_dirty || _entries is null) return;
                store.Save(JsonSerializer.Serialize(_entries));
                _dirty = false;
            }
        }

        private void Load()
        {
            if (_entries is not null) return;
            try
            {
                var raw = store.Load();
                _entries = string.IsNullOrEmpty(raw)
                    ? []
                    : JsonSerializer.Deserialize<Dictionary<string, CacheEntry>>(raw) ?? [];
            }
            catch
            {
                _entries = [];
            }
        }
    }

    private void FindFilesRecursive(ISaveTree tree, string parentDocId, int maxDepth,
        CancellationToken cancellationToken, Action<SaveTreeEntry> onFile, ScanState state,
        bool diagnostic = false)
    {
        if (maxDepth <= 0)
        {
            if (diagnostic) state.Trace($"DEPTH LIMIT reached at {parentDocId}");
            return;
        }
        var children = tree.ListChildren(parentDocId);
        var siblings = children.Where(x => !x.IsDirectory).Select(x => x.Name).ToArray();
        if (diagnostic) state.Trace($"WALK depthRemaining={maxDepth} parent={parentDocId} children={children.Count}");
        foreach (var child in children)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (child.IsDirectory)
            {
                if (diagnostic) state.Trace($"DIR name={child.Name} docId={child.DocId}");
                if (IsPruned(child.Name, state.Kind))
                {
                    if (diagnostic) state.Trace($"PRUNED directory {child.Name}");
                    continue;
                }
                FindFilesRecursive(tree, child.DocId, maxDepth - 1, cancellationToken, onFile, state, diagnostic);
            }
            else
            {
                state.Siblings = siblings;
                onFile(child);
            }
        }
    }

    private static string? NavigatePath(ISaveTree tree, string fromDocId, string[] segments, Action<string>? trace = null)
    {
        var current = fromDocId;
        foreach (var segment in segments)
        {
            var children = tree.ListChildren(current);
            trace?.Invoke($"At {current}: {children.Count} children [{string.Join(", ", children.Select(x => x.IsDirectory ? x.Name + "/" : x.Name))}]");
            var next = children.FirstOrDefault(x => x.IsDirectory && x.Name == segment);
            if (next is null)
            {
                trace?.Invoke($"Missing directory segment '{segment}'");
                return null;
            }
            trace?.Invoke($"Matched '{segment}' -> {next.DocId}");
            current = next.DocId;
        }
        return current;
    }
}

/// <summary>A folder on a desktop filesystem: document ids are absolute paths.</summary>
public sealed class FileSystemSaveTree : ISaveTree
{
    public FileSystemSaveTree(string root)
    {
        RootDocId = Path.GetFullPath(root);
        if (!Directory.Exists(RootDocId))
            throw new DirectoryNotFoundException($"The folder {RootDocId} does not exist.");
    }

    public string RootDocId { get; }
    public string Describe => $"Folder={RootDocId}";

    public IReadOnlyList<SaveTreeEntry> ListChildren(string docId)
    {
        var results = new List<SaveTreeEntry>();
        try
        {
            foreach (var entry in new DirectoryInfo(docId).EnumerateFileSystemInfos())
            {
                // A symlinked folder (EmuDeck, RetroDECK) is walked like a real one.
                var isDirectory = entry is DirectoryInfo;
                results.Add(new SaveTreeEntry(entry.FullName, entry.Name, isDirectory,
                    isDirectory ? null : new DateTimeOffset(entry.LastWriteTimeUtc)));
            }
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException) { }
        return results;
    }

    public Stream OpenRead(string docId) => File.OpenRead(docId);

    public string DocumentIdFor(string docId) => docId;
}
