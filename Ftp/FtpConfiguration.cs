using BeeQ.FileStorage.Service;
using System.Net;

namespace BeeQ.FileStorage.Ftp;

public class FtpConfiguration : FtpConfiguration<Guid> { }

public class FtpConfiguration<TId>
{
    /// <summary>
    /// Protocol to use. ftp (default) or ftps
    /// </summary>
    public string Protocol { get; set; } = "ftp";
    /// <summary>
    /// Ftp Access Host 
    /// </summary>
    public string Host { get; set; } = string.Empty;
    /// <summary>
    /// Ftp Access Port Host. (null for default)
    /// </summary>
    public int? Port { get; set; }

    /// <summary>
    /// Optional Username to autentificate
    /// </summary>
    public string? Username { get; set; }
    /// <summary>
    /// Optional Password to autentificate
    /// </summary>
    public string? Password { get; set; }
    /// <summary>
    /// BasePath of files
    /// </summary>
    public string BasePath { get; set; } = string.Empty;
    /// <summary>
    /// SSL connection must be used
    /// </summary>
    public bool EnableSsl { get; set; } = false;
    /// <summary>
    /// Optional Interceptor of Ftp Web Request just before invoque methods for custom code
    /// </summary>
    public Action<FtpWebRequest>? OnCreateRequest { get; set; }

    /// <summary>
    /// (optional) Custom Path Template to use for full path of files. If not set, the default will be used.
    /// </summary>
    public Func<IFileStorageFullIdentifier<TId>, string>? PathTemplate { get; set; }

    /// <summary>
    /// (optional) Custom function to get the filename from the id. If not set, the default will be used.
    /// </summary>
    public Func<TId, string>? GetFilename { get; set; }

    /// <summary>
    /// (optional) Custom function to create the id from the filename. If not set, the default will be used.
    /// </summary>
    public Func<(string? SchemaName, string Filename), Task<TId>>? OnCreateId { get; set; }

    internal void Validate()
    {
        if (string.IsNullOrEmpty(Host))
            throw new FtpUrlConfigratedException();
    }
}
