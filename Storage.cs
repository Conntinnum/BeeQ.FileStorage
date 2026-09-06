using BeeQ.FileStorage.Builder;

namespace BeeQ.FileStorage;

/// <summary>
/// Startup Class of File Storage
/// </summary>
public static class Storage
{
    /// <summary>
    /// Use a new Custom Storage
    /// </summary>
    /// <typeparam name="TId">Primary Key type</typeparam>
    /// <param name="schemaKey">Name of schema</param>
    /// <returns>A new File Storage Builder</returns>
    public static IFileStorageBuilder<TId> Use<TId>(string? schemaKey = null)
    {
        return new FileStorageBuilder<TId>(schemaKey);
    }

    /// <summary>
    /// Creates a Empty File Storage Builder
    /// </summary>
    /// <returns>A new File Storage Builder</returns>
    public static IFileStorageBuilder Create()
    {
        return new FileStorageBuilder();
    }
}
