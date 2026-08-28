
namespace BeeQ;

public interface IFileStorage
{
    string SchemaKey { get; }

    Task<IFileStorageFullIdentifier> Upload(string filename, string base64);
    Task<IFileStorageFullIdentifier> Upload(string filename, byte[] bytes);
    Task<IFileStorageFullIdentifier> Upload(string filename, Stream content);

    Task<byte[]?> GetBytes(Guid id);
    Task<byte[]?> GetBytes(string filename);

    Task<string?> GetBase64(Guid id);
    Task<string?> GetBase64(string filename);

    Task<Stream?> GetStream(Guid id);
    Task<Stream?> GetStream(string filename);
}
