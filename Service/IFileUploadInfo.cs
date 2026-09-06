namespace BeeQ.FileStorage.Service;

/// <summary>
/// Contains information about an uploading file, including its content payload.
/// </summary>
/// <typeparam name="TId">Type of the file identifier.</typeparam>
/// <typeparam name="T">Type of the content payload (for example Stream, byte[] or string).</typeparam>
public interface IFileUploadInfo<TId, T> : IFileInfo<TId>
{
    /// <summary>
    /// The content payload being uploaded.
    /// </summary>
    T Content { get; set; }
}

internal class FileUploadInfoStream<TId> : FileInfo<TId>, IFileUploadInfo<TId, Stream>
{
    public Stream Content { get; set; }

    internal FileUploadInfoStream(string? storage, TId id, string key, string fullkey, Stream stream) : base(storage, id, key, fullkey)
    {
        this.Content = stream;
    }
}

internal class FileUploadInfoBase64<TId> : FileInfo<TId>, IFileUploadInfo<TId, string>
{
    public string Content { get; set; }

    internal FileUploadInfoBase64(string? storage, TId id, string key, string fullkey, string base64) : base(storage, id, key, fullkey)
    {
        this.Content = base64;
    }
}

internal class FileUploadInfoBytes<TId> : FileInfo<TId>, IFileUploadInfo<TId, byte[]>
{
    public byte[] Content { get; set; }

    internal FileUploadInfoBytes(string? storage, TId id, string key, string fullkey, byte[] bytes) : base(storage, id, key, fullkey)
    {
        this.Content = bytes;
    }
}
