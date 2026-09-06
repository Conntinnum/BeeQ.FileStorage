using BeeQ.FileStorage.Service;
using System.Security.Cryptography;
using System.Text;

namespace BeeQ.FileStorage.LocalDisk;

public interface ILocalDiskService : ILocalDiskService<Guid> { }
public interface ILocalDiskService<TId> : IFileStorage<TId> 
{
    /// <summary>
    /// Returns FileInfo for the given id, or null if the file does not exist.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    System.IO.FileInfo? GetFileInfo(TId id);
}

internal class LocalDiskService : LocalDiskService<Guid>, ILocalDiskService
{
    public LocalDiskService(LocalDiskConfiguration<Guid> options) : base(options)
    {
        base.Interceptors.OnCreateId = options.OnCreateId ?? OnCreateId;
        base.FullPathTemplate = options.PathTemplate ?? (info => LocalDiskService.UseCustomFullPath(info, options.BasePath!));
    }

    private static async Task<Guid> OnCreateId((string? SchemaName, string Filename) cfg)
    {
        var key = $"{cfg.SchemaName}.{cfg.Filename}";
        var bytes = Encoding.UTF8.GetBytes(key);
#pragma warning disable S4790
        var hash = MD5.HashData(bytes);
#pragma warning restore S4790

        return new Guid(hash);
    }

    private static string UseCustomFullPath(IFileStorageFullIdentifier<Guid> info, string basePath)
    {
        var id = $"{info.Id}";
        return Path.Combine(basePath, id[..2], id[2..4], info.Id.ToString());
    }
}

internal class LocalDiskService<TId> : FileStorageService<TId>, ILocalDiskService<TId>
{
    public LocalDiskConfiguration<TId> Options { get; set; }

    public LocalDiskService(LocalDiskConfiguration<TId> options)
    {
        options.Validate();
        this.Options = options;
        base.Interceptors.OnCreateId = options.OnCreateId;
        base.Interceptors.OnGetFilename = options.GetFilename ?? GetFilename;
        base.FullPathTemplate = options.PathTemplate ?? (info => LocalDiskService<TId>.UseCustomFullPath(info, options.BasePath!));

        base.Interceptors.OnUploadStream = (async info =>
        {
            if (Path.GetDirectoryName(info.FullPath) is string folder)
                Directory.CreateDirectory(folder);

            using FileStream file = File.Create(info.FullPath);
            await info.Content.CopyToAsync(file);
        });

        base.Interceptors.OnGetStream = (async info =>
        {
            return File.Open(info.FullPath, FileMode.Open);
        });

        base.Interceptors.OnDelete = (async info =>
        {
            File.Delete(info.FullPath);
        });
    }

    public new FileInfo? GetFileInfo(TId id)
    {
        var info = base.GetFileInfo(id);
        if (info == null) return null;

        return new FileInfo(info.FullPath);
    }

    private static string GetFilename(TId id)
    {
        return $"{id}";
    }

    private static string UseCustomFullPath(IFileStorageFullIdentifier<TId> info, string basePath)
    {
        return Path.Combine(basePath, info.Filename);
    }

}
