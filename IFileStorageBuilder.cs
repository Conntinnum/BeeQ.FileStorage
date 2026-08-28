namespace BeeQ;

public partial class FileStorage
{
    public interface IFileStorageBuilder
    {
        string SchemaName { get; }

        IFileStorageBuilder UseStandardFullPath();
        IFileStorageBuilder UseCustomFullPath(Func<IFileStorageFullIdentifier, string> template);

        IFileStorageBuilder OnUploadStream(Func<IFileUploadInfo<Stream>, Task> onUpload);
        IFileStorageBuilder OnUploadBytes(Func<IFileUploadInfo<byte[]>, Task> onUpload);
        IFileStorageBuilder OnUploadBase64(Func<IFileUploadInfo<string>, Task> onUpload);

        IFileStorageBuilder OnCreateId(Func<(string SchemaName, string Filename), Task<Guid>> onCreateId);
        IFileStorageBuilder OnGetFileId(Func<string, Guid?> onGetFileId);
        IFileStorageBuilder OnGetFilename(Func<Guid, string> onGetFilename);

        IFileStorageBuilder OnGetBytes(Func<IFileInfo, Task<byte[]?>> getBytes);
        IFileStorageBuilder OnGetBase64(Func<IFileInfo, Task<string?>> getBase64);
        IFileStorageBuilder OnGetStream(Func<IFileInfo, Task<Stream?>> getStream);

        IFileStorage Build();
    }

}