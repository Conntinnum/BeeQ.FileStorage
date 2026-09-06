using BeeQ.FileStorage.Exception;

namespace BeeQ.FileStorage.Service;

public class FileStorageService<TId> : IFileStorage<TId>
{
    internal class InterceptorsType
    {
        internal Func<IFileUploadInfo<TId, Stream>, Task>? OnUploadStream { get; set; }
        internal Func<IFileUploadInfo<TId, byte[]>, Task>? OnUploadBytes { get; set; }
        internal Func<IFileUploadInfo<TId, string>, Task>? OnUploadBase64 { get; set; }

        public Func<(string? SchemaName, string Filename), Task<TId>>? OnCreateId { get; set; }
        public Func<TId, string?>? OnGetFilename { get; set; }

        internal Func<IFileInfo<TId>, Task<byte[]?>>? OnGetBytes { get; set; }
        internal Func<IFileInfo<TId>, Task<string?>>? OnGetBase64 { get; set; }
        internal Func<IFileInfo<TId>, Task<Stream?>>? OnGetStream { get; set; }

        internal Func<IFileInfo<TId>, Task>? OnDelete { get; set; }
    }

    public string? SchemaKey { get; internal set; }
    internal Func<IFileStorageFullIdentifier<TId>, string> FullPathTemplate { get; set; } = id => id.Filename;
    internal InterceptorsType Interceptors { get; set; } = new();

    #region Upload

    public async virtual Task<TId> Upload(string filename, string base64)
    {
        if (Interceptors.OnCreateId == null)
            throw new CreateIdInterceptorConfigratedException();

        var id = new FileStorageFullIdentifier<TId>(await Interceptors.OnCreateId((SchemaKey, filename)), filename);

        if (Interceptors.OnUploadBase64 != null)
        {
            var fullpath = FullPathTemplate(id);
            var info = new FileUploadInfoBase64<TId>(this.SchemaKey, id.Id, id.Filename, fullpath, base64);
            await Interceptors.OnUploadBase64(info);
            return id.Id;
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

    public async virtual Task<TId> Upload(string filename, byte[] bytes)
    {
        if (Interceptors.OnCreateId == null)
            throw new CreateIdInterceptorConfigratedException();
        var id = new FileStorageFullIdentifier<TId>(await Interceptors.OnCreateId((SchemaKey, filename)), filename);

        if (Interceptors.OnUploadBytes != null)
        {
            var fullpath = FullPathTemplate(id);
            var info = new FileUploadInfoBytes<TId>(this.SchemaKey, id.Id, id.Filename, fullpath, bytes);
            await Interceptors.OnUploadBytes(info);
            return id.Id;
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

    public async virtual Task<TId> Upload(string filename, Stream content)
    {
        if (Interceptors.OnCreateId == null)
            throw new CreateIdInterceptorConfigratedException();
        var id = new FileStorageFullIdentifier<TId>(await Interceptors.OnCreateId((SchemaKey, filename)), filename);

        if (Interceptors.OnUploadStream != null)
        {
            var fullpath = FullPathTemplate(id);
            var info = new FileUploadInfoStream<TId>(this.SchemaKey, id.Id, id.Filename, fullpath, content);
            await Interceptors.OnUploadStream(info);
            return id.Id;
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

    #region Get

    public async virtual Task<string?> GetBase64(TId id)
    {
        var info = GetFileInfo(id);
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

    public async virtual Task<byte[]?> GetBytes(TId id)
    {
        var info = GetFileInfo(id);
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

    public async virtual Task<Stream?> GetStream(TId id)
    {
        var info = GetFileInfo(id);
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

    #endregion

    #region Delete

    public async virtual Task Delete(TId id)
    {
        if (Interceptors.OnDelete == null)
            throw new DeleteInterceptorConfigratedException();

        var info = GetFileInfo(id) ?? throw new Exception.FileNotFoundException();
        await Interceptors.OnDelete(info);
    }

    #endregion

    protected IFileInfo<TId>? GetFileInfo(TId? id)
    {
        if (id == null)
            return null;

        if (Interceptors.OnGetFilename == null)
            throw new GetFilenameInterceptorConfigratedException();
        var filename = Interceptors.OnGetFilename(id);
        if (filename == null)
            return null;

        return new FileInfo<TId>(this.SchemaKey, id, filename, FullPathTemplate(new FileStorageFullIdentifier<TId>(id, filename)));
    }
}
