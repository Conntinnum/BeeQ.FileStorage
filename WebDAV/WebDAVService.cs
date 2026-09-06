using BeeQ.FileStorage.Service;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using static System.Net.WebRequestMethods;

namespace BeeQ.FileStorage.WebDAV;

#pragma warning disable S101

public interface IWebDAVService : IWebDAVService<Guid> { }
public interface IWebDAVService<TId> : IFileStorage<TId> 
{
    /// <summary>
    /// Returns the headers of the file stored in WebDAV, given its identifier.
    /// </summary>
    /// <param name="id">Identifier of the file.</param>
    /// <returns>Headers of the file as a read-only dictionary, or null if the file does not exist.</returns>
    Task<IReadOnlyDictionary<string, string[]>?> GetHead(TId id);

    /// <summary>
    /// Returns a list of files and directories stored in WebDAV, given the identifier of a directory.
    /// </summary>
    /// <param name="id">Identifier of the directory to list.</param>
    /// <returns>List of WebDAV entries contained in the specified directory.</returns>
    Task<List<Dto.WebDavEntry>> ListFiles(TId id);
    /// <summary>
    /// Returns a list of files and directories stored in WebDAV, given the identifier of a directory and the depth of the search.
    /// </summary>
    /// <param name="id">Identifier of the directory to list.</param>
    /// <param name="depth">Depth of recursion to include when listing entries (1 = direct children).</param>
    /// <returns>List of WebDAV entries matching the requested depth under the specified directory.</returns>
    Task<List<Dto.WebDavEntry>> ListFiles(TId id, int depth);

    /// <summary>
    /// Creates a directory in WebDAV, given its identifier.
    /// </summary>
    /// <param name="path">Path to create or ensure exists on the WebDAV server.</param>
    Task MakeDirectory(string path);

    /// <summary>
    /// Copies a file stored in WebDAV, given its identifier and the destination path.
    /// </summary>
    /// <param name="source">Identifier of the source file to copy.</param>
    /// <param name="destination">Destination path where the file will be copied.</param>
    /// <param name="overwrite">Whether to overwrite the destination file if it exists.</param>
    /// <returns>A task representing the asynchronous copy operation.</returns>
    Task CopyFile(TId source, string destination, bool overwrite = false);

    /// <summary>
    /// Moves a file stored in WebDAV, given its identifier and the destination path.
    /// </summary>
    /// <param name="source">Identifier of the source file to move.</param>
    /// <param name="destination">Destination path where the file will be moved.</param>
    /// <param name="overwrite">Whether to overwrite the destination file if it exists.</param>
    /// <returns>A task representing the asynchronous move operation.</returns>
    Task MoveFile(TId source, string destination, bool overwrite = false);

    /// <summary>
    /// Locks a file stored in WebDAV, given its identifier, the owner of the lock, and the timeout for the lock.
    /// </summary>
    /// <param name="id">Filename id</param>
    /// <param name="owner">Owner of the lock</param>
    /// <param name="timeout">Timeout for the lock</param>
    /// <returns>Lock token</returns>
    Task<string?> LockFile(TId id, string owner, TimeSpan timeout);

    /// <summary>
    /// Unlocks a file stored in WebDAV, given its identifier and the lock token.
    /// </summary>
    /// <param name="id">Filename id</param>
    /// <param name="lockToken">Lock token</param>
    Task UnlockFile(TId id, string lockToken);
}

internal class WebDAVService : WebDAVService<Guid>, IWebDAVService
{
    public WebDAVService(WebDAVConfiguration cfg) : base(cfg)
    {
        cfg.Validate();
        base.Interceptors.OnCreateId = cfg.OnCreateId ?? OnCreateId;
        base.Interceptors.OnGetFilename = cfg.GetFilename ?? GetFilename;
        base.FullPathTemplate = cfg.PathTemplate ?? (info => WebDAVService.UseCustomFullPath(info));
    }

    private static string? GetFilename(Guid id)
    {
        return $"{id}";
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

    private static string UseCustomFullPath(IFileStorageFullIdentifier<Guid> info)
    {
        return $"{info.Id}";
    }
}

internal class WebDAVService<TId> : FileStorageService<TId>, IWebDAVService<TId>
{
    private static readonly XNamespace Dav = "DAV:";
    public WebDAVConfiguration<TId> Options { get; }

    public WebDAVService(WebDAVConfiguration<TId> cfg)
    {
        cfg.Validate();
        Options = cfg;
        base.Interceptors.OnCreateId = Options.OnCreateId;
        base.Interceptors.OnGetFilename = Options.GetFilename ?? GetFilename;
        base.FullPathTemplate = Options.PathTemplate ?? (info => WebDAVService<TId>.UseCustomFullPath(info));

        base.Interceptors.OnUploadBytes = (async info => await OnUploadBytes(info, cfg));
        base.Interceptors.OnGetStream = (async info => await OnGetStream(info, cfg));
        base.Interceptors.OnDelete = (async info => await OnDelete(info, cfg));
    }

    private static async Task OnUploadBytes(IFileUploadInfo<TId, byte[]> info, WebDAVConfiguration<TId> cfg)
    {
        var http = GetHttpClient(cfg);
        var content = new ByteArrayContent(info.Content);
        var directories = string.Join('/', info.FullPath.Split('/', StringSplitOptions.RemoveEmptyEntries)[..^1]);
        await EnsureWebDavDirectoryAsync(http, cfg.Url!, directories);
        var fullurl = $"{cfg.Url}{info.FullPath}";
        var response = await http.PutAsync(fullurl, content);
        response.EnsureSuccessStatusCode();
    }

    private static async Task<Stream?> OnGetStream(IFileInfo<TId> info, WebDAVConfiguration<TId> cfg)
    {
        var http = GetHttpClient(cfg);
        var fullurl = $"{cfg.Url}{info.FullPath}";
        var response = await http.GetAsync(fullurl, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStreamAsync();
    }

    private static async Task OnDelete(IFileInfo<TId> info, WebDAVConfiguration<TId> cfg)
    {
        var http = GetHttpClient(cfg);
        var fullurl = $"{cfg.Url}{info.FullPath}";
        var response = await http.DeleteAsync(fullurl);
        response.EnsureSuccessStatusCode();
    }

    private static HttpClient GetHttpClient(WebDAVConfiguration<TId> cfg)
    {
        var http = new HttpClient();
        cfg.OnCreateRequest?.Invoke(http);
        return http;
    }

    public async Task<IReadOnlyDictionary<string, string[]>?> GetHead(TId id)
    {
        var info = GetFileInfo(id);
        if (info == null) return null;

        using var http = GetHttpClient(this.Options);
        var fullurl = $"{this.Options.Url}{info.FullPath}";
        using var request = new HttpRequestMessage(HttpMethod.Head, fullurl);

        using var response = await http.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return response.Headers
            .Concat(response.Content.Headers)
            .GroupBy(header => header.Key, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.SelectMany(header => header.Value).ToArray(),
                StringComparer.OrdinalIgnoreCase);
    }

    public async Task<List<Dto.WebDavEntry>> ListFiles(TId id) => await ListFiles(id, 1);
    public async Task<List<Dto.WebDavEntry>> ListFiles(TId id, int depth)
    {
        var info = GetFileInfo(id);
        if (info == null) return [];

        using var http = GetHttpClient(this.Options);
        var fullurl = $"{this.Options.Url}{info.FullPath}";
        var body = """
        <?xml version="1.0" encoding="utf-8"?>
        <d:propfind xmlns:d="DAV:">
            <d:allprop />
        </d:propfind>
        """;
        using var request = new HttpRequestMessage(new HttpMethod("PROPFIND"), fullurl);
        request.Content = new StringContent(body, Encoding.UTF8, "application/xml");
        request.Headers.TryAddWithoutValidation("Depth", $"{depth}");

        using var response = await http.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var xml = XDocument.Parse(await response.Content.ReadAsStringAsync());

        return [.. xml.Descendants(Dav + "response")
            .Select(item =>
            {
                var prop = item.Elements(Dav + "propstat").FirstOrDefault(propStat => propStat.Element(Dav + "status")?.Value.Contains(" 200 ") == true)?.Element(Dav + "prop");

                var lengthText = prop?.Element(Dav + "getcontentlength")?.Value;
                var modifiedText = prop?.Element(Dav + "getlastmodified")?.Value;

                return new Dto.WebDavEntry(
                    item.Element(Dav + "href")?.Value ?? string.Empty,
                    prop?.Element(Dav + "resourcetype")?.Element(Dav + "collection") != null,
                    long.TryParse(lengthText, out var length) ? length : null,
                    DateTimeOffset.TryParse(modifiedText, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeLocal, out var modified) ? modified : null,
                    prop?.Element(Dav + "getetag")?.Value);
            })];
    }

    public async Task MakeDirectory(string path)
    {
        var http = GetHttpClient(this.Options);
        var directories = string.Join('/', path.Split('/', StringSplitOptions.RemoveEmptyEntries)[..^1]);
        await EnsureWebDavDirectoryAsync(http, this.Options.Url!, directories);
    }

    public async Task CopyFile(TId source, string destination, bool overwrite = false)
    {
        var info = GetFileInfo(source);
        if (info == null) return;
        var http = GetHttpClient(this.Options);

        var fullurlOri = $"{this.Options.Url}{info.FullPath}";
        var fullurlDes = $"{this.Options.Url}{destination}";
        using var request = new HttpRequestMessage(new HttpMethod("COPY"), fullurlOri);

        request.Headers.TryAddWithoutValidation("Destination", fullurlDes);
        request.Headers.TryAddWithoutValidation("Overwrite", overwrite ? "T" : "F");
        request.Headers.TryAddWithoutValidation("Depth", "infinity");

        using var response = await http.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public async Task MoveFile(TId source, string destination, bool overwrite = false)
    {
        var info = GetFileInfo(source);
        if (info == null) return;
        var http = GetHttpClient(this.Options);

        var fullurlOri = $"{this.Options.Url}{info.FullPath}";
        var fullurlDes = $"{this.Options.Url}{destination}";
        using var request = new HttpRequestMessage(new HttpMethod("MOVE"), fullurlOri);

        request.Headers.TryAddWithoutValidation("Destination", fullurlDes);
        request.Headers.TryAddWithoutValidation("Overwrite", overwrite ? "T" : "F");

        using var response = await http.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public async Task<string?> LockFile(TId id, string owner, TimeSpan timeout)
    {
        var info = GetFileInfo(id);
        if (info == null) return null;
        var http = GetHttpClient(this.Options);

        var fullurl = $"{this.Options.Url}{info.FullPath}";

        var lockInfo = new XDocument(
            new XElement(Dav + "lockinfo",
                new XElement(Dav + "lockscope",
                    new XElement(Dav + "exclusive")),
                new XElement(Dav + "locktype",
                    new XElement(Dav + "write")),
                owner is null
                    ? null
                    : new XElement(Dav + "owner",
                        new XElement(Dav + "href", owner))));

        using var request = new HttpRequestMessage(new HttpMethod("LOCK"), fullurl)
        {
            Content = new StringContent(lockInfo.ToString(),Encoding.UTF8,"application/xml")
        };

        request.Headers.TryAddWithoutValidation("Timeout", $"Second={(int)timeout.TotalSeconds}");

        using var response = await http.SendAsync(request);
        response.EnsureSuccessStatusCode();

        if (!response.Headers.TryGetValues("Lock-Token", out var values))
            throw new InvalidOperationException("El servidor no devolvió Lock-Token.");

        return values.First();
    }

    public async Task UnlockFile(TId id, string lockToken)
    {
        var info = GetFileInfo(id);
        if (info == null) return;
        var http = GetHttpClient(this.Options);

        var fullurl = $"{this.Options.Url}{info.FullPath}";

        using var request = new HttpRequestMessage(new HttpMethod("UNLOCK"), fullurl);
        request.Headers.TryAddWithoutValidation("Lock-Token", lockToken);
        using var response = await http.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    private static string? GetFilename(TId id)
    {
        return $"{id}";
    }

    private static string UseCustomFullPath(IFileStorageFullIdentifier<TId> info)
    {
        return $"{info.Filename}";
    }

    public static async Task EnsureWebDavDirectoryAsync(HttpClient http,string baseUrl,string directoryPath)
    {
        ArgumentNullException.ThrowIfNull(http);

#if NET8_0_OR_GREATER
        ArgumentException.ThrowIfNullOrWhiteSpace(baseUrl);
        ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath);
#else
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new ArgumentException("baseurl cant be empty", nameof(baseUrl));
        if (string.IsNullOrWhiteSpace(directoryPath))
            throw new ArgumentException("directoryPath cant be empty", nameof(directoryPath));
#endif

        var baseUri = new Uri($"{baseUrl.TrimEnd('/')}/");
        var currentPath = string.Empty;

        foreach (var part in directoryPath.Split('/',StringSplitOptions.RemoveEmptyEntries))
        {
            if (part is "." or "..")
                throw new ArgumentException("La ruta no puede contener '.' ni '..'.", nameof(directoryPath));

            currentPath = $"{currentPath}{Uri.EscapeDataString(part)}/";
            var directoryUri = new Uri(baseUri, currentPath);

            using var request = new HttpRequestMessage(new HttpMethod("MKCOL"),directoryUri);

            using var response = await http.SendAsync(request);

            // 201: creada. 405: ya existía (comportamiento habitual en WebDAV).
            if (response.StatusCode is HttpStatusCode.Created or HttpStatusCode.MethodNotAllowed)
                continue;

            response.EnsureSuccessStatusCode();
        }
    }

}

#pragma warning restore S101
