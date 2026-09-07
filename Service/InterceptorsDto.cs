namespace BeeQ.FileStorage.Service;

public class InterceptorsDto<TId>
{
    public Func<IFileUploadInfo<TId, Stream>, Task>? OnUploadStream { get; set; }
    public Func<IFileUploadInfo<TId, byte[]>, Task>? OnUploadBytes { get; set; }
    public Func<IFileUploadInfo<TId, string>, Task>? OnUploadBase64 { get; set; }

    public Func<(string? SchemaName, string Filename), Task<TId>>? OnCreateId { get; set; }
    public Func<TId, string?>? OnGetFilename { get; set; }

    public Func<IFileInfo<TId>, Task<byte[]?>>? OnGetBytes { get; set; }
    public Func<IFileInfo<TId>, Task<string?>>? OnGetBase64 { get; set; }
    public Func<IFileInfo<TId>, Task<Stream?>>? OnGetStream { get; set; }

    public Func<IFileInfo<TId>, Task>? OnDelete { get; set; }
}