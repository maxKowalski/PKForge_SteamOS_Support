using PKForge.Domain;

namespace PKForge.App;

/// <summary>
/// Save and folder access on a desktop filesystem: document ids are absolute paths and folder
/// tree ids are directory paths, so no grant has to be persisted.
/// </summary>
public sealed class DesktopFileAccess : ISaveFileAccess, IFolderFileAccess
{
    public async ValueTask<ReadOnlyMemory<byte>> ReadAsync(string documentId, CancellationToken cancellationToken = default) =>
        await File.ReadAllBytesAsync(documentId, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Writes beside the save, flushes to disk, then renames over it, so a crash leaves either the
    /// old save or the new one. A symlinked save (EmuDeck links its folders into ~/Emulation) is
    /// replaced at its real location, never turned into a plain file.
    /// </summary>
    public async ValueTask WriteAtomicallyAsync(string documentId, ReadOnlyMemory<byte> bytes, CancellationToken cancellationToken = default)
    {
        var target = File.ResolveLinkTarget(documentId, returnFinalTarget: true)?.FullName ?? documentId;
        var temporary = target + ".pkforge-tmp";
        try
        {
            await using (var output = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true))
            {
                await output.WriteAsync(bytes, cancellationToken).ConfigureAwait(false);
                output.Flush(flushToDisk: true);
            }
            File.Move(temporary, target, overwrite: true);
        }
        catch
        {
            try { File.Delete(temporary); } catch (IOException) { }
            throw;
        }
    }

    public ValueTask<IReadOnlyList<PickedDocument>> ListFilesAsync(string treeId, CancellationToken cancellationToken = default) =>
        ValueTask.FromResult<IReadOnlyList<PickedDocument>>(
            [.. Directory.EnumerateFiles(treeId).Select(path => new PickedDocument(path, Path.GetFileName(path)))]);

    public ValueTask<ReadOnlyMemory<byte>> ReadFileAsync(string documentId, CancellationToken cancellationToken = default) =>
        ReadAsync(documentId, cancellationToken);

    public async ValueTask WriteFileAsync(string treeId, string fileName, ReadOnlyMemory<byte> bytes, CancellationToken cancellationToken = default) =>
        await WriteAtomicallyAsync(Path.Combine(treeId, fileName), bytes, cancellationToken).ConfigureAwait(false);
}
