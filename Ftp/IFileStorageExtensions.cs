using BeeQ.FileStorage.Builder;
using BeeQ.FileStorage.Ftp;
using BeeQ.FileStorage.LocalDisk;
using BeeQ.FileStorage.Service;
using System.Net;
using System.Security.Cryptography;
using System.Text;

#pragma warning disable IDE0130 // Namespace does not match folder structure - Extension Methods
namespace BeeQ.FileStorage;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class IFileStorageFtpExtensions
{
    /// <summary>
    /// Extension to use the Local Disk to storage the Files
    /// </summary>
    /// <param name="builder">Empty File Storage Builder</param>
    /// <param name="schemaName">Optional schema name used to distinguish storage namespaces</param>
    /// <param name="config">Configure the access to FTP</param>
    /// <returns>The configured file storage</returns>
    public static IFtpService UseFtp(this IFileStorageBuilder builder, string schemaName, Action<FtpConfiguration> config)
    {
        var cfg = new FtpConfiguration();
        config(cfg);
        return builder.Use<Guid>(schemaName)
            .CustomBuild<FtpService, IFtpService>(() => new FtpService(cfg));
    }

    /// <summary>
    /// Extension to use the Local Disk to storage the Files
    /// </summary>
    /// <typeparam name="TId">The type of the Id used to identify the files</typeparam>
    /// <param name="builder">Empty File Storage Builder</param>
    /// <param name="schemaName">Optional schema name used to distinguish storage namespaces</param>
    /// <param name="config">Configure the access to FTP</param>
    /// <returns>The configured file storage</returns>
    public static IFtpService<TId> UseFtp<TId>(this IFileStorageBuilder builder, string schemaName, Action<FtpConfiguration<TId>> config)
    {
        var cfg = new FtpConfiguration<TId>();
        config(cfg);
        return builder.Use<TId>(schemaName)
            .CustomBuild<FtpService<TId>, IFtpService<TId>>(() => new FtpService<TId>(cfg));
    }

}
