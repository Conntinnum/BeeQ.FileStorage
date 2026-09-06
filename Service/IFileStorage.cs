namespace BeeQ.FileStorage.Service;

/// <summary>
/// Represents a file storage service capable of uploading and retrieving files.
/// </summary>
/// <typeparam name="TId">Type used as the primary identifier for stored files.</typeparam>
public interface IFileStorage<TId>
{
    /// <summary>
    /// The configured schema key or name for this storage instance (if any).
    /// </summary>
    string? SchemaKey { get; }

    /// <summary>
    /// Upload a file using a base64 encoded content string.
    /// </summary>
    /// <param name="filename">Original filename of the uploaded file.</param>
    /// <param name="base64">Base64 encoded file content.</param>
    /// <returns>The identifier assigned to the stored file.</returns>
    Task<TId> Upload(string filename, string base64);

    /// <summary>
    /// Upload a file using a byte array content.
    /// </summary>
    /// <param name="filename">Original filename of the uploaded file.</param>
    /// <param name="bytes">File content as bytes.</param>
    /// <returns>The identifier assigned to the stored file.</returns>
    Task<TId> Upload(string filename, byte[] bytes);

    /// <summary>
    /// Upload a file using a stream as the content source.
    /// </summary>
    /// <param name="filename">Original filename of the uploaded file.</param>
    /// <param name="content">Content stream for the file.</param>
    /// <returns>The identifier assigned to the stored file.</returns>
    Task<TId> Upload(string filename, Stream content);

    /// <summary>
    /// Retrieve the raw bytes for a stored file by id.
    /// </summary>
    /// <param name="id">Identifier of the file to retrieve.</param>
    /// <returns>Byte content or null if not found.</returns>
    Task<byte[]?> GetBytes(TId id);

    /// <summary>
    /// Retrieve the file content as a base64 encoded string.
    /// </summary>
    /// <param name="id">Identifier of the file to retrieve.</param>
    /// <returns>Base64 string or null if not found.</returns>
    Task<string?> GetBase64(TId id);

    /// <summary>
    /// Retrieve a readable stream for the stored file.
    /// </summary>
    /// <param name="id">Identifier of the file to retrieve.</param>
    /// <returns>A stream positioned at the beginning of the content, or null if not found.</returns>
    Task<Stream?> GetStream(TId id);

    /// <summary>
    /// Delete a stored file by its identifier.
    /// </summary>
    /// <param name="id">Identifier of the file to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Delete(TId id);
}
