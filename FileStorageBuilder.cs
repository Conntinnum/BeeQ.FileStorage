namespace BeeQ;

public partial class FileStorage
{
    internal class FileStorageBuilder(string key) : IFileStorageBuilder
    {
        public string SchemaName { get; private set; } = key;
        public Func<IFileStorageFullIdentifier, string>? FullPathTemplate { get; internal set; }
        public Func<IFileUploadInfo<Stream>, Task>? UploadStream { get; internal set; }
        public Func<IFileUploadInfo<byte[]>, Task>? UploadBytes { get; internal set; }
        public Func<IFileUploadInfo<string>, Task>? UploadBase64 { get; internal set; }
        public Func<(string SchemaName, string Filename), Task<Guid>>? CreateId { get; internal set; }
        public Func<string, Guid?>? GetFileId { get; internal set; }
        public Func<Guid, string?>? GetFilename { get; internal set; }
        public Func<IFileInfo, Task<byte[]?>>? GetBytes { get; internal set; }
        public Func<IFileInfo, Task<string?>>? GetBase64 { get; internal set; }
        public Func<IFileInfo, Task<Stream?>>? GetStream { get; internal set; }

        public IFileStorageBuilder UseStandardFullPath()
        {
            this.FullPathTemplate = cfg => $"./{this.SchemaName}/{cfg.Id.ToString()[..2]}/{cfg.Id.ToString()[2..4]}/{cfg.Filename}";
            return this;
        }

        public IFileStorageBuilder UseCustomFullPath(Func<IFileStorageFullIdentifier, string> template)
        {
            this.FullPathTemplate = template; 
            return this;
        }

        public IFileStorageBuilder OnUploadStream(Func<IFileUploadInfo<Stream>, Task> onUpload)
        {
            this.UploadStream = onUpload;
            return this;
        }

        public IFileStorageBuilder OnUploadBytes(Func<IFileUploadInfo<byte[]>, Task> onUpload)
        {
            this.UploadBytes = onUpload;
            return this;
        }

        public IFileStorageBuilder OnUploadBase64(Func<IFileUploadInfo<string>, Task> onUpload)
        {
            this.UploadBase64 = onUpload;
            return this;
        }

        public IFileStorageBuilder OnCreateId(Func<(string SchemaName, string Filename), Task<Guid>> onCreateId)
        {
            this.CreateId = onCreateId;
            return this;
        }

        public IFileStorageBuilder OnGetFileId(Func<string, Guid?> onGetFileId)
        {
            this.GetFileId = onGetFileId;
            return this;
        }

        public IFileStorageBuilder OnGetFilename(Func<Guid, string> onGetFilename)
        {
            this.GetFilename = onGetFilename;
            return this;
        }

        public IFileStorageBuilder OnGetBytes(Func<IFileInfo, Task<byte[]?>> getBytes)
        {
            this.GetBytes = getBytes;
            return this;
        }

        public IFileStorageBuilder OnGetBase64(Func<IFileInfo, Task<string?>> getBase64)
        {
            this.GetBase64 = getBase64;
            return this;
        }

        public IFileStorageBuilder OnGetStream(Func<IFileInfo, Task<Stream?>> getStream)
        {
            this.GetStream = getStream;
            return this;
        }

        public IFileStorage Build()
        {
            if (OnUploadBase64 == null && OnUploadBytes == null && OnUploadStream == null)
                throw new UploadInterceptorConfigratedException();
            if (GetBase64 == null && GetBytes == null && GetStream == null)
                throw new UploadInterceptorConfigratedException();

            if (GetFileId == null)
                throw new GetFileIdInterceptorConfigratedException();
            if (GetFilename == null)
                throw new GetFilenameInterceptorConfigratedException();

            if (FullPathTemplate == null)
                UseStandardFullPath();

            if (CreateId == null)
                CreateId = async (cfg) => Guid.NewGuid();

            return new FileStorage(SchemaName, FullPathTemplate!, new InterceptorsType(CreateId, GetFileId, GetFilename)
            {
                OnUploadBase64 = UploadBase64,
                OnUploadBytes = UploadBytes,
                OnUploadStream = UploadStream,
                OnGetBase64 = GetBase64,
                OnGetBytes = GetBytes,
                OnGetStream = GetStream,
            });
        }

    }
}