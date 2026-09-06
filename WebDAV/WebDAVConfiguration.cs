using BeeQ.FileStorage.Service;

namespace BeeQ.FileStorage.WebDAV;

public class WebDAVConfiguration : WebDAVConfiguration<Guid> { }

public class WebDAVConfiguration<TId>
{

    /// <summary>
    /// Access Url to service
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// Optional Interceptor of WebDAV Request just before invoque methods for custom code
    /// </summary>
    public Action<HttpClient>? OnCreateRequest { get; set; }
    public Func<TId, string?>? GetFilename;
    public Func<(string? SchemaName, string Filename), Task<TId>>? OnCreateId { get; set; }
    public Func<IFileStorageFullIdentifier<TId>, string>? PathTemplate { get; set; }

    internal void Validate() 
    {
        Url ??= string.Empty;
        if (!Url.EndsWith('/')) Url += "/";
    }
}
