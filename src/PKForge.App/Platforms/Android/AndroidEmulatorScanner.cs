using Android.Provider;
using PKForge.App.Services;
using PKForge.Domain;
using PKForge.Infrastructure;
using AndroidUri = Android.Net.Uri;

namespace PKForge.App;

/// <summary>
/// Scans a granted SAF tree for Pokémon saves. The rules live in <see cref="EmulatorSaveScanner"/>;
/// this supplies the tree through Android's DocumentsContract.
/// </summary>
public sealed class AndroidEmulatorScanner(ISaveEngine engine)
    : EmulatorSaveScanner(engine, new PreferencesScanCacheStore())
{
    protected override ISaveTree OpenTree(string treeId) => new SafSaveTree(treeId);

    /// <summary>Android's last package update time: zero-once per install, stable after.</summary>
    private static long _installEpoch = -1;

    protected override long InstallEpoch
    {
        get
        {
            if (_installEpoch >= 0) return _installEpoch;
            try
            {
                var context = Platform.AppContext;
                _installEpoch = context.PackageManager!.GetPackageInfo(context.PackageName!, 0)?.LastUpdateTime ?? 0;
            }
            catch
            {
                _installEpoch = 0; // unbustable cache is still better than a crashing scan
            }
            return _installEpoch;
        }
    }

    private sealed class SafSaveTree : ISaveTree
    {
        private readonly AndroidUri _treeUri;

        public SafSaveTree(string treeId)
        {
            _treeUri = AndroidUri.Parse(treeId) ?? throw new ArgumentException("Invalid tree URI.", nameof(treeId));
            RootDocId = DocumentsContract.GetTreeDocumentId(_treeUri)
                ?? throw new InvalidOperationException("The folder grant has no tree document id.");
        }

        public string RootDocId { get; }
        public string Describe => $"TreeUri={_treeUri}";

        public string DocumentIdFor(string docId) =>
            DocumentsContract.BuildDocumentUriUsingTree(_treeUri, docId)?.ToString()
            ?? throw new IOException("The document URI could not be built.");

        public Stream OpenRead(string docId) =>
            Platform.AppContext.ContentResolver?.OpenInputStream(AndroidUri.Parse(DocumentIdFor(docId))!)
            ?? throw new IOException("ContentResolver.OpenInputStream returned null.");

        public IReadOnlyList<SaveTreeEntry> ListChildren(string parentDocId)
        {
            var results = new List<SaveTreeEntry>();
            var childrenUri = DocumentsContract.BuildChildDocumentsUriUsingTree(_treeUri, parentDocId);
            if (childrenUri is null) return results;

            string[] projection =
            [
                DocumentsContract.Document.ColumnDocumentId,
                DocumentsContract.Document.ColumnDisplayName,
                DocumentsContract.Document.ColumnMimeType,
                DocumentsContract.Document.ColumnLastModified,
            ];
            using var cursor = Platform.AppContext.ContentResolver?.Query(childrenUri, projection, null, null, null);
            if (cursor is null) return results;

            while (cursor.MoveToNext())
            {
                var docId = cursor.GetString(0);
                var name = cursor.GetString(1);
                if (docId is null || name is null) continue;
                var isDirectory = cursor.GetString(2) == DocumentsContract.Document.MimeTypeDir;
                var modifiedMs = cursor.GetLong(3);
                results.Add(new SaveTreeEntry(docId, name, isDirectory,
                    modifiedMs > 0 ? DateTimeOffset.FromUnixTimeMilliseconds(modifiedMs) : null));
            }
            return results;
        }
    }
}
