namespace BeeQ;

public interface IFileStorageFullIdentifier
{
    public Guid Id { get; }
    public string Filename { get; }
}

/// <summary>
/// Complete Resource Identifier
/// </summary>
internal class FileStorageFullIdentifier(Guid id, string filename) : IFileStorageFullIdentifier
{
    public Guid Id { get; set; } = id;
    public string Filename { get; set; } = filename;
}
