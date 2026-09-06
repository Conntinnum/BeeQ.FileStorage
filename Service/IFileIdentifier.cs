namespace BeeQ.FileStorage.Service;

/// <summary>
/// Represents a partial identifier for a file resource. Either <see cref="Id"/> or <see cref="Key"/> may be provided.
/// </summary>
/// <typeparam name="TId">Type of the identifier value.</typeparam>
public interface IFileIdentifier<TId>
{
    /// <summary>
    /// The identifier value if available.
    /// </summary>
    public TId? Id { get; }

    /// <summary>
    /// An alternate key or lookup value for the file if available.
    /// </summary>
    public string? Key { get; }
}

/// <summary>
/// Internal implementation of <see cref="IFileIdentifier{TId}"/> used by the library.
/// </summary>
internal class FileIdentifier<TId> : IFileIdentifier<TId>
{
    public TId? Id { get; set; }
    public string? Key { get; set; }

    public FileIdentifier(TId id)
    {
        this.Id = id;
    }
    public FileIdentifier(string key)
    {
        this.Key = key;
    }
}
