namespace BeeQ.FileStorage.Service;

/// <summary>
/// Provides information about a stored file, including storage name and full path.
/// </summary>
/// <typeparam name="TId">Type of the file identifier.</typeparam>
public interface IFileInfo<TId> : IFileStorageFullIdentifier<TId>
{
    /// <summary>
    /// The name of the storage provider or logical storage (if any).
    /// </summary>
    public string? StorageName { get; }

    /// <summary>
    /// The full physical or logical path where the file is stored.
    /// </summary>
    public string FullPath { get; }

}

/// <summary>
/// Internal concrete implementation of <see cref="IFileInfo{TId}"/> used for upload and retrieval operations.
/// </summary>
internal class FileInfo<TId> : FileStorageFullIdentifier<TId>, IFileInfo<TId>
{
    public string? StorageName { get; private set; }
    public string FullPath { get; private set; }

    internal FileInfo(string? storage, TId id, string key, string fullpath) : base(id, key)
    {
        this.StorageName = storage;
        this.FullPath = fullpath;
    }
}
