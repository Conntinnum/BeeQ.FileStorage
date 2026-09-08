using BeeQ.FileStorage.Service;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace BeeQ.FileStorage.Ftp;

public interface IFtpService : IFileStorage<Guid> { }
public interface IFtpService<TId> : IFileStorage<TId>
{
    /// <summary>
    /// Gets the list of files in the specified directory.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<string[]?> GetFiles(TId id);

    /// <summary>
    /// Gets the size of the specified file.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<long?> GetFileSize(TId id);

    /// <summary>
    /// Gets the last modified date of the specified file.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<DateTime?> GetFileDate(TId id);

    /// <summary>
    /// Ensures that the specified directory exists on the FTP server. If it does not exist, it will be created.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task EnsureMakeDirectory(TId id);

    /// <summary>
    /// Removes the specified directory from the FTP server. If the directory does not exist, no action is taken.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task RemoveDirectory(TId id);

    /// <summary>
    /// Renames the specified file on the FTP server to a new filename. If the file does not exist, an exception will be thrown.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="newFilename"></param>
    /// <returns></returns>
    Task Rename(TId id, string newFilename);
}

internal class FtpService : FtpService<Guid>, IFtpService
{
    public FtpService(FtpConfiguration<Guid> config) : base(config)
    {
        base.Interceptors.OnCreateId = OnCreateId;
        base.FullPathTemplate = info => UseCustomFullPath(info, Config);
    }

    private static async Task<Guid> OnCreateId((string? SchemaName, string Filename) cfg)
    {
        var key = $"{cfg.SchemaName}.{cfg.Filename}";
        var bytes = Encoding.UTF8.GetBytes(key);
        var hash = MD5.HashData(bytes);

        return new Guid(hash);
    }

    private static string UseCustomFullPath(IFileStorageFullIdentifier<Guid> info, FtpConfiguration<Guid> cfg)
    {
        var id = $"{info.Id}";
        var route = Path.Combine(cfg.BasePath, id[..2], id[2..4], info.Id.ToString()).Replace('\\', '/');

        if (cfg.Port.HasValue)
            return $"{cfg.Protocol}://{cfg.Host}:{cfg.Port}/{route}.bin";
        else
            return $"{cfg.Protocol}://{cfg.Host}/{route}.bin";
    }
}

internal class FtpService<TId> : FileStorageService<TId>, IFtpService<TId>
{
    public FtpConfiguration<TId> Config { get; set; }

    public FtpService(FtpConfiguration<TId> config)
    {
        this.Config = config;
        this.Config.Validate();
        base.Interceptors.OnCreateId = Config.OnCreateId;
        base.Interceptors.OnGetFilename = Config.GetFilename ?? GetFilename;
        base.FullPathTemplate = Config.PathTemplate ?? (info => UseCustomFullPath(info, Config));

        base.Interceptors.OnUploadStream = OnUploadStream;
        base.Interceptors.OnGetStream = OnGetStream;
        base.Interceptors.OnDelete = OnDelete;
    }

    private async Task<Stream?> OnGetStream(IFileInfo<TId> info)
    {
        var ftp = CreateRequest(info.FullPath, WebRequestMethods.Ftp.DownloadFile, this.Config);
        using var response = (FtpWebResponse)await ftp.GetResponseAsync();
        return response.GetResponseStream();
    }

    private async Task OnDelete(IFileInfo<TId> info)
    {
        var ftp = CreateRequest(info.FullPath, WebRequestMethods.Ftp.DeleteFile, this.Config);
        using var response = (FtpWebResponse)await ftp.GetResponseAsync();
    }

    private async Task OnUploadStream(IFileUploadInfo<TId, Stream> info)
    {
        using (info.Content)
        {
            // Ensure FTP directory exists before uploading the file
            var ftpBaseUrl = $"{this.Config.Protocol ?? "ftp"}://{this.Config.Host}{(this.Config.Port.HasValue ? $":{this.Config.Port}" : "")}";
            var directories = string.Join('/', info.FullPath.Replace(ftpBaseUrl, "").Split('/', StringSplitOptions.RemoveEmptyEntries)[..^1]);
            await EnsureFtpDirectoryAsync(ftpBaseUrl, directories);

            // Reset the stream position to the beginning before uploading
            if (info.Content.CanSeek)
                info.Content.Position = 0;

            // upload file
            var ftp = CreateRequest(info.FullPath, WebRequestMethods.Ftp.UploadFile, this.Config);
            using (var uploadStream = await ftp.GetRequestStreamAsync())
            {
                await info.Content.CopyToAsync(uploadStream);
            }
            using var response = (FtpWebResponse)await ftp.GetResponseAsync();
        }
    }

    public async Task<string[]?> GetFiles(TId id)
    {
        var info = GetFileInfo(id);
        if (info == null) return null;

        var ftp = CreateRequest(info.FullPath, WebRequestMethods.Ftp.ListDirectory, Config);

        using var response = (FtpWebResponse)await ftp.GetResponseAsync();
        using var stream = response.GetResponseStream();
        using var reader = new StreamReader(stream);

        var items = new List<string>();

        while (await reader.ReadLineAsync() is { } item)
        {
            if (!string.IsNullOrWhiteSpace(item))
            {
                items.Add(item);
            }
        }

        return [.. items];
    }

    public async Task<long?> GetFileSize(TId id)
    {
        var info = GetFileInfo(id);
        if (info == null) return null;

        var ftp = CreateRequest(info.FullPath, WebRequestMethods.Ftp.GetFileSize, Config);
        using var response = (FtpWebResponse) await ftp.GetResponseAsync();
        return response.ContentLength;
    }

    public async Task<DateTime?> GetFileDate(TId id)
    {
        var info = GetFileInfo(id);
        if (info == null) return null;

        var ftp = CreateRequest(info.FullPath, WebRequestMethods.Ftp.GetDateTimestamp, Config);
        using var response = (FtpWebResponse)await ftp.GetResponseAsync();
        return response.LastModified;
    }

    public async Task EnsureMakeDirectory(TId id)
    {
        var info = GetFileInfo(id);
        if (info == null) return;

        var ftpBaseUrl = $"{this.Config.Protocol ?? "ftp"}://{this.Config.Host}{(this.Config.Port.HasValue ? $":{this.Config.Port}" : "")}";
        var directories = string.Join('/', info.FullPath.Replace(ftpBaseUrl, "").Split('/', StringSplitOptions.RemoveEmptyEntries)[..^1]);
        await EnsureFtpDirectoryAsync(ftpBaseUrl, directories);
    }

    public async Task RemoveDirectory(TId id)
    {
        var info = GetFileInfo(id);
        if (info == null) return;

        var ftp = CreateRequest(info.FullPath, WebRequestMethods.Ftp.RemoveDirectory, Config);
        using var response = (FtpWebResponse)await ftp.GetResponseAsync();
    }

    public async Task Rename(TId id, string newFilename)
    {
        var info = GetFileInfo(id);
        if (info == null) return;

        var ftp = CreateRequest(info.FullPath, WebRequestMethods.Ftp.Rename, Config);
        ftp.RenameTo = newFilename;
        using var response = (FtpWebResponse)await ftp.GetResponseAsync();
    }

    private static FtpWebRequest CreateRequest(string url, string method, FtpConfiguration<TId> config)
    {
#pragma warning disable SYSLIB0014 // Type or member is obsolete
        var request = (FtpWebRequest)WebRequest.Create(url);
#pragma warning restore SYSLIB0014 // Type or member is obsolete
        request.Method = method;
        if (config.Username != null)
            request.Credentials = new NetworkCredential(config.Username, config.Password);
        request.UseBinary = true;
        request.KeepAlive = false;
        request.EnableSsl = config.EnableSsl;
        config.OnCreateRequest?.Invoke(request);
        return request;
    }

    private static string GetFilename(TId id)
    {
        return $"{id}";
    }

    private static string UseCustomFullPath(IFileStorageFullIdentifier<TId> info, FtpConfiguration<TId> cfg)
    {
        var id = $"{info.Id}";
        var route = Path.Combine(cfg.BasePath, id).Replace('\\', '/');

        if (cfg.Port.HasValue)
            return $"{cfg.Protocol}://{cfg.Host}:{cfg.Port}/{route}.bin";
        else
            return $"{cfg.Protocol}://{cfg.Host}/{route}.bin";
    }

    public async Task EnsureFtpDirectoryAsync(string ftpBaseUrl, string remoteDirectory)
    {
        var currentPath = ftpBaseUrl.TrimEnd('/');

        foreach (var part in remoteDirectory.Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            currentPath += "/" + part;

#pragma warning disable SYSLIB0014
            var request = (FtpWebRequest)WebRequest.Create(currentPath);
#pragma warning restore SYSLIB0014

            request.Method = WebRequestMethods.Ftp.MakeDirectory;
            request.Credentials = new NetworkCredential(
                this.Config.Username,
                this.Config.Password);

            request.EnableSsl = this.Config.EnableSsl;
            request.KeepAlive = false;

            try
            {
                using var response = (FtpWebResponse)await request.GetResponseAsync();
            }
            catch (WebException ex)
                when ((ex.Response as FtpWebResponse)?.StatusCode ==
                      FtpStatusCode.ActionNotTakenFileUnavailable)
            {
                // Usualmente significa que la carpeta ya existe.
                // Si puede haber problemas de permisos, verificá con ListDirectory.
            }
        }
    }
}
