using BeeQ.FileStorage.Service;

namespace BeeQ.FileStorage.LocalDisk;

public class LocalDiskConfiguration : LocalDiskConfiguration<Guid> { }

public class LocalDiskConfiguration<TId>
{
    public string? BasePath { get; set; }
    public Func<IFileStorageFullIdentifier<TId>, string>? PathTemplate { get; set; }
    public Func<TId, string>? GetFilename { get; set; }
    public Func<(string? SchemaName, string Filename), Task<TId>>? OnCreateId { get; set; }

    internal virtual void Validate()
    {
        if (PathTemplate == null && string.IsNullOrEmpty(BasePath))
            throw new BasePathLocalDiskConfigratedException();

        if (typeof(TId) != typeof(Guid) && OnCreateId == null)
            throw new CreateIdLocalDiskConfigratedException();
    }
}
