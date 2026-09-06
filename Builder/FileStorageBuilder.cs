using BeeQ.FileStorage.Exception;
using BeeQ.FileStorage.Service;

namespace BeeQ.FileStorage.Builder;

internal class FileStorageBuilder : IFileStorageBuilder
{
    public IFileStorageBuilder<TId> Use<TId>(string? schemaName = null)
    {
        return new FileStorageBuilder<TId>(schemaName);
    }
}

internal class FileStorageBuilder<TId>(string? schemaKey) : IFileStorageBuilder<TId>
{
    public string? SchemaName { get; private set; } = schemaKey;
    public Func<IFileStorageFullIdentifier<TId>, string>? FullPathTemplate { get; internal set; }
    public Func<IFileUploadInfo<TId, Stream>, Task>? UploadStream { get; internal set; }
    public Func<IFileUploadInfo<TId, byte[]>, Task>? UploadBytes { get; internal set; }
    public Func<IFileUploadInfo<TId, string>, Task>? UploadBase64 { get; internal set; }
    public Func<(string? SchemaName, string Filename), Task<TId>>? CreateId { get; internal set; }
    public Func<TId, string?>? GetFilename { get; internal set; }
    public Func<IFileInfo<TId>, Task<byte[]?>>? GetBytes { get; internal set; }
    public Func<IFileInfo<TId>, Task<string?>>? GetBase64 { get; internal set; }
    public Func<IFileInfo<TId>, Task<Stream?>>? GetStream { get; internal set; }
    public Func<IFileInfo<TId>, Task>? Delete { get; internal set; }

    public IFileStorageBuilder<TId> UseStandardFullPath()
    {
        this.FullPathTemplate = cfg => $"./{this.SchemaName}/{cfg.Filename}";
        return this;
    }

    public IFileStorageBuilder<TId> UseCustomFullPath(Func<IFileStorageFullIdentifier<TId>, string> template)
    {
        this.FullPathTemplate = template;
        return this;
    }

    public IFileStorageBuilder<TId> OnUploadStream(Func<IFileUploadInfo<TId, Stream>, Task> onUpload)
    {
        this.UploadStream = onUpload;
        return this;
    }

    public IFileStorageBuilder<TId> OnUploadBytes(Func<IFileUploadInfo<TId, byte[]>, Task> onUpload)
    {
        this.UploadBytes = onUpload;
        return this;
    }

    public IFileStorageBuilder<TId> OnUploadBase64(Func<IFileUploadInfo<TId, string>, Task> onUpload)
    {
        this.UploadBase64 = onUpload;
        return this;
    }

    public IFileStorageBuilder<TId> OnCreateId(Func<(string? SchemaName, string Filename), Task<TId>> onCreateId)
    {
        this.CreateId = onCreateId;
        return this;
    }

    public IFileStorageBuilder<TId> OnGetFilename(Func<TId, string> onGetFilename)
    {
        this.GetFilename = onGetFilename;
        return this;
    }

    public IFileStorageBuilder<TId> OnGetBytes(Func<IFileInfo<TId>, Task<byte[]?>> getBytes)
    {
        this.GetBytes = getBytes;
        return this;
    }

    public IFileStorageBuilder<TId> OnGetBase64(Func<IFileInfo<TId>, Task<string?>> getBase64)
    {
        this.GetBase64 = getBase64;
        return this;
    }

    public IFileStorageBuilder<TId> OnGetStream(Func<IFileInfo<TId>, Task<Stream?>> getStream)
    {
        this.GetStream = getStream;
        return this;
    }

    public IFileStorageBuilder<TId> OnDelete(Func<IFileInfo<TId>, Task> onDelete)
    {
        this.Delete = onDelete;
        return this;
    }

    public IFileStorage<TId> Build()
    {
        if (UploadBase64 == null && UploadBytes == null && UploadStream == null)
            throw new UploadInterceptorConfigratedException();
        if (GetBase64 == null && GetBytes == null && GetStream == null)
            throw new UploadInterceptorConfigratedException();
        if (Delete == null)
            throw new DeleteInterceptorConfigratedException();

        if (GetFilename == null)
            throw new GetFilenameInterceptorConfigratedException();

        if (FullPathTemplate == null)
            UseStandardFullPath();

        if (CreateId == null)
            throw new CreateIdInterceptorConfigratedException();

        return new FileStorageService<TId>()
        {
            SchemaKey = SchemaName,
            FullPathTemplate = FullPathTemplate!,
            Interceptors = new FileStorageService<TId>.InterceptorsType()
            {
                OnUploadBase64 = UploadBase64,
                OnUploadBytes = UploadBytes,
                OnUploadStream = UploadStream,
                OnCreateId = CreateId,
                OnGetFilename = GetFilename,
                OnGetBase64 = GetBase64,
                OnGetBytes = GetBytes,
                OnGetStream = GetStream,
                OnDelete = Delete,
            }
        };
    }

    public TIService CustomBuild<TService, TIService>(Func<TService> builder)
        where TIService : IFileStorage<TId>
        where TService : FileStorageService<TId>, TIService
    {
        ArgumentNullException.ThrowIfNull(builder);
        var service = builder();

        service.SchemaKey ??= SchemaName;
        service.FullPathTemplate ??= FullPathTemplate!;
        service.Interceptors = new FileStorageService<TId>.InterceptorsType()
        {
            OnUploadBase64 = service.Interceptors.OnUploadBase64 ?? UploadBase64,
            OnUploadBytes = service.Interceptors.OnUploadBytes ?? UploadBytes,
            OnUploadStream = service.Interceptors.OnUploadStream ?? UploadStream,
            OnCreateId = service.Interceptors.OnCreateId ?? CreateId,
            OnGetFilename = service.Interceptors.OnGetFilename ?? GetFilename,
            OnGetBase64 = service.Interceptors.OnGetBase64 ?? GetBase64,
            OnGetBytes = service.Interceptors.OnGetBytes ?? GetBytes,
            OnGetStream = service.Interceptors.OnGetStream ?? GetStream,
            OnDelete = service.Interceptors.OnDelete ?? Delete,
        };
        return (TIService)service;
    }
}