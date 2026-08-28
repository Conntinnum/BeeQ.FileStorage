namespace BeeQ;

public partial class FileStorage
{
    public interface IFileInfo : IFileStorageFullIdentifier
    {
        public string StorageName { get; }
        public string FullPath { get; }

    }
    /// <summary>
    /// Uploading File Info
    /// </summary>
    internal class FileInfo : FileStorageFullIdentifier, IFileInfo
    {
        public string StorageName { get; private set; }
        public string FullPath { get; private set; }

        internal FileInfo(string storage, Guid id, string key, string fullpath) : base(id, key)
        {
            this.StorageName = storage;
            this.Id = id;
            this.Filename = key;
            this.FullPath = fullpath;
        }
    }
}