namespace BeeQ;

public partial class FileStorage : IFileStorage
{
    /// <summary>
    /// Opens a new Schema of FileStorage
    /// </summary>
    /// <param name="schema">Schema's name</param>
    /// <returns>New File Storage Builder</returns>
    public static IFileStorageBuilder OpenSchema(string schema)
        => new FileStorageBuilder(schema);

    internal class InterceptorsType(Func<(string SchemaName, string Filename), Task<Guid>> onCreateId, Func<string, Guid?> onGetFileId, Func<Guid, string?> onGetFilename)
    {
        internal Func<IFileUploadInfo<Stream>, Task>? OnUploadStream { get; set; }
        internal Func<IFileUploadInfo<byte[]>, Task>? OnUploadBytes { get; set; }
        internal Func<IFileUploadInfo<string>, Task>? OnUploadBase64 { get; set; }

        public Func<(string SchemaName, string Filename), Task<Guid>> OnCreateId { get; } = onCreateId;
        public Func<string, Guid?> OnGetFileId { get; } = onGetFileId;
        public Func<Guid, string?> OnGetFilename { get; } = onGetFilename;

        internal Func<IFileInfo, Task<byte[]?>>? OnGetBytes { get; set; }
        internal Func<IFileInfo, Task<string?>>? OnGetBase64 { get; set; }
        internal Func<IFileInfo, Task<Stream?>>? OnGetStream { get; set; }
    }

    public string SchemaKey { get; private set; }
    internal Func<IFileStorageFullIdentifier, string> FullPathTemplate { get; set; }
    internal InterceptorsType Interceptors { get; set; }

    internal FileStorage(string schemaKey, Func<IFileStorageFullIdentifier, string> fullPathTemplate, InterceptorsType interceptors)
    {
        this.SchemaKey = schemaKey;
        this.FullPathTemplate = fullPathTemplate;
        this.Interceptors = interceptors;
    }

    #region Upload

    public async Task<IFileStorageFullIdentifier> Upload(string filename, string base64)
    {
        var id = new FileStorageFullIdentifier( await Interceptors.OnCreateId((SchemaKey, filename)), filename);
                
        if (Interceptors.OnUploadBase64 != null)
        {
            var fullpath = FullPathTemplate(id);
            var info = new FileUploadInfoBase64(this.SchemaKey, id.Id, id.Filename, fullpath, base64);
            await Interceptors.OnUploadBase64(info);
            return id;
        }
        if (Interceptors.OnUploadBytes != null)
        {
            byte[] bytes = Convert.FromBase64String(base64);
            return await Upload(filename, bytes);
        }
        if (Interceptors.OnUploadStream != null)
        {
            byte[] bytes = Convert.FromBase64String(base64);
            using var stream = new MemoryStream(bytes);
            return await Upload(filename, stream);
        }
        throw new UploadInterceptorConfigratedException();
    }

    public async Task<IFileStorageFullIdentifier> Upload(string filename, byte[] bytes)
    {
        var id = new FileStorageFullIdentifier(await Interceptors.OnCreateId((SchemaKey, filename)), filename);

        if (Interceptors.OnUploadBytes != null)
        {
            var fullpath = FullPathTemplate(id);
            var info = new FileUploadInfoBytes(this.SchemaKey, id.Id, id.Filename, fullpath, bytes);
            await Interceptors.OnUploadBytes(info);
            return id;
        }
        if (Interceptors.OnUploadBase64 != null)
        {
            string base64 = Convert.ToBase64String(bytes);
            return await Upload(filename, base64);
        }
        if (Interceptors.OnUploadStream != null)
        {
            using var stream = new MemoryStream(bytes);
            return await Upload(filename, stream);
        }
        throw new UploadInterceptorConfigratedException();
    }

    public async Task<IFileStorageFullIdentifier> Upload(string filename, Stream content)
    {
        var id = new FileStorageFullIdentifier(await Interceptors.OnCreateId((SchemaKey, filename)), filename);

        if (Interceptors.OnUploadStream != null)
        {
            var fullpath = FullPathTemplate(id);
            var info = new FileUploadInfoStream(this.SchemaKey, id.Id, id.Filename, fullpath, content);
            await Interceptors.OnUploadStream(info);
            return id;
        }
        if (Interceptors.OnUploadBytes != null)
        {
            var bytes = await content.GetBytes();
            return await Upload(filename, bytes);
        }
        if (Interceptors.OnUploadBase64 != null)
        {
            var bytes = await content.GetBytes();
            string base64 = Convert.ToBase64String(bytes);
            return await Upload(filename, base64);
        }
        throw new UploadInterceptorConfigratedException();
    }

    #endregion


    public async Task<string?> GetBase64(Guid id)
    {
        return await InternalGetBase64(id, null);
    }

    public async Task<string?> GetBase64(string filename)
    {
        return await InternalGetBase64(null, filename);
    }

    public async Task<byte[]?> GetBytes(Guid id)
    {
        return await InternalGetBytes(id, null);
    }

    public async Task<byte[]?> GetBytes(string filename)
    {
        return await InternalGetBytes(null, filename);
    }

    public async Task<Stream?> GetStream(Guid id)
    {
        return await InternalGetStream(id, null);
    }

    public async Task<Stream?> GetStream(string filename)
    {
        return await InternalGetStream(null, filename);
    }

    public async Task<string?> InternalGetBase64(Guid? id, string? filename)
    {
        var info = GetFileInfo(id, filename);
        if (info == null)
            return null;

        if (Interceptors.OnGetBase64 != null)
        {
            return await Interceptors.OnGetBase64(info);
        }
        if (Interceptors.OnGetBytes != null)
        {
            var bytes = await Interceptors.OnGetBytes(info);
            if (bytes == null)
                return null;
            return Convert.ToBase64String(bytes);
        }
        if (Interceptors.OnGetStream != null)
        {
            var stream = await Interceptors.OnGetStream(info);
            if (stream == null)
                return null;
            var bytes = await stream.GetBytes();
            return Convert.ToBase64String(bytes);
        }

        throw new GetInterceptorConfigratedException();
    }

    public async Task<byte[]?> InternalGetBytes(Guid? id, string? filename)
    {
        var info = GetFileInfo(id, filename);
        if (info == null)
            return null;

        if (Interceptors.OnGetBytes != null)
        {
            return await Interceptors.OnGetBytes(info);
        }
        if (Interceptors.OnGetBase64 != null)
        {
            var base64 = await Interceptors.OnGetBase64(info);
            if (base64 == null)
                return null;
            return Convert.FromBase64String(base64);
        }
        if (Interceptors.OnGetStream != null)
        {
            var stream = await Interceptors.OnGetStream(info);
            if (stream == null)
                return null;
            return await stream.GetBytes();
        }

        throw new GetInterceptorConfigratedException();
    }

    public async Task<Stream?> InternalGetStream(Guid? id, string? filename)
    {
        var info = GetFileInfo(id, filename);
        if (info == null)
            return null;

        if (Interceptors.OnGetStream != null)
        {
            return await Interceptors.OnGetStream(info);
        }
        if (Interceptors.OnGetBytes != null)
        {
            var bytes = await Interceptors.OnGetBytes(info);
            if (bytes == null)
                return null;
            return new MemoryStream(bytes);
        }
        if (Interceptors.OnGetBase64 != null)
        {
            var base64 = await Interceptors.OnGetBase64(info);
            if (base64 == null)
                return null;
            var bytes = Convert.FromBase64String(base64);
            return new MemoryStream(bytes);
        }

        throw new GetInterceptorConfigratedException();
    }


    private FileInfo? GetFileInfo(Guid? id, string? filename)
    {
        var _id = !id.HasValue && filename != null ? Interceptors.OnGetFileId(filename) : id;
        var _filename = id.HasValue && filename == null ? Interceptors.OnGetFilename(id.Value) : filename;
        if (_id == null || _filename == null)
            return null;

        return new FileInfo(this.SchemaKey, _id.Value, _filename, FullPathTemplate(new FileStorageFullIdentifier(_id.Value, _filename)));
    }
}
