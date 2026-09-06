namespace BeeQ.FileStorage.Service;

/// <summary>
/// Represents a complete identifier for a stored file, including its id and filename.
/// </summary>
/// <typeparam name="TId">Type of the file identifier.</typeparam>
public interface IFileStorageFullIdentifier<TId>
{
    /// <summary>
    /// Gets the identifier value for the stored file.
    /// </summary>
    public TId Id { get; }

    /// <summary>
    /// Gets the filename associated with the stored file.
    /// </summary>
    public string Filename { get; }
}

/// <summary>
/// Internal concrete implementation of <see cref="IFileStorageFullIdentifier{TId}"/>.
/// </summary>
internal class FileStorageFullIdentifier<TId>(TId id, string filename) : IFileStorageFullIdentifier<TId>
{
    public TId Id { get; set; } = id;
    public string Filename { get; set; } = filename;
}
