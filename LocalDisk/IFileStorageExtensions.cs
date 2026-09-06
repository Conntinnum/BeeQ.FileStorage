using BeeQ.FileStorage.Builder;
using BeeQ.FileStorage.Ftp;
using BeeQ.FileStorage.LocalDisk;
using BeeQ.FileStorage.Service;

#pragma warning disable IDE0130 // Namespace does not match folder structure - Extension Methods
namespace BeeQ.FileStorage;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class IFileStorageLocalDiskExtensions
{
    /// <summary>
    /// Extension to use the Local Disk to storage the Files
    /// </summary>
    /// <param name="builder">Empty File Storage Builder</param>
    /// <param name="schemaName">Optional schema name used to distinguish storage namespaces</param>
    /// <param name="options">Configuration options for the Local Disk storage</param>
    /// <returns>The configured file storage</returns>
    public static ILocalDiskService UseLocalDisk(this IFileStorageBuilder builder, string schemaName, Action<LocalDiskConfiguration> options)
    {
        var opt = new LocalDiskConfiguration();
        options.Invoke(opt);
        return builder.Use<Guid>(schemaName)
            .CustomBuild<LocalDiskService, ILocalDiskService>(() => new LocalDiskService(opt));
    }

    /// <summary>
    /// Extension to use the Local Disk to storage the Files
    /// </summary>
    /// <typeparam name="TId">The type of the Id used to identify the files</typeparam>
    /// <param name="builder">Empty File Storage Builder</param>
    /// <param name="schemaName">Optional schema name used to distinguish storage namespaces</param>
    /// <param name="options">Configuration options for the Local Disk storage</param>
    /// <returns>The configured file storage</returns>
    public static ILocalDiskService<TId> UseLocalDisk<TId>(this IFileStorageBuilder builder, string schemaName, Action<LocalDiskConfiguration<TId>> options)
    {
        var opt = new LocalDiskConfiguration<TId>();
        options.Invoke(opt);
        return builder.Use<TId>(schemaName)
            .CustomBuild<LocalDiskService<TId>, ILocalDiskService<TId>>(() => new LocalDiskService<TId>(opt));
    }
}

