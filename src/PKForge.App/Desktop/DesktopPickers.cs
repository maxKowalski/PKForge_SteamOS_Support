using Avalonia.Platform.Storage;
using PKForge.Domain;

namespace PKForge.App;

/// <summary>File and folder pickers from Avalonia's storage provider (the desktop portal on Linux).</summary>
public sealed class DesktopPickers : IDocumentPicker, IFolderPicker
{
    private static IStorageProvider Storage => DesktopWindow.Required.StorageProvider;

    public async ValueTask<PickedDocument?> PickSaveAsync(CancellationToken cancellationToken = default)
    {
        var files = await Storage.OpenFilePickerAsync(new FilePickerOpenOptions { Title = "Open a Pokémon save" });
        return files.Count > 0 && ToDocument(files[0]) is { } document ? document : null;
    }

    public async ValueTask<IReadOnlyList<PickedDocument>> PickManyAsync(CancellationToken cancellationToken = default)
    {
        var files = await Storage.OpenFilePickerAsync(new FilePickerOpenOptions { Title = "Choose files", AllowMultiple = true });
        return [.. files.Select(ToDocument).OfType<PickedDocument>()];
    }

    public async ValueTask<PickedFolder?> PickFolderAsync(CancellationToken cancellationToken = default)
    {
        var folders = await Storage.OpenFolderPickerAsync(new FolderPickerOpenOptions { Title = "Choose the emulator's folder" });
        if (folders.Count == 0 || folders[0].TryGetLocalPath() is not { } path) return null;
        return new PickedFolder(path, folders[0].Name);
    }

    private static PickedDocument? ToDocument(IStorageFile file) =>
        file.TryGetLocalPath() is { } path ? new PickedDocument(path, file.Name) : null;
}
