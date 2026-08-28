namespace BeeQ;

public partial class FileStorage
{
    /// <summary>
    /// Uploading File Info
    /// </summary>
    public interface IFileUploadInfo<T> : IFileInfo
    {
        T Content { get; set; }
    }

    internal class FileUploadInfoStream : FileInfo, IFileUploadInfo<Stream>
    {
        public Stream Content { get; set; }

        internal FileUploadInfoStream(string storage, Guid id, string key, string fullkey, Stream stream) : base(storage, id, key, fullkey)
        {
            this.Content = stream;
        }
    }

    internal class FileUploadInfoBase64 : FileInfo, IFileUploadInfo<string>
    {
        public string Content { get; set; }

        internal FileUploadInfoBase64(string storage, Guid id, string key, string fullkey, string base64) : base(storage, id, key, fullkey)
        {
            this.Content = base64;
        }
    }

    internal class FileUploadInfoBytes : FileInfo, IFileUploadInfo<byte[]>
    {
        public byte[] Content { get; set; }

        internal FileUploadInfoBytes(string storage, Guid id, string key, string fullkey, byte[] bytes) : base(storage, id, key, fullkey)
        {
            this.Content = bytes;
        }
    }
}