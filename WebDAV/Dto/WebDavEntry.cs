namespace BeeQ.FileStorage.WebDAV.Dto;

public sealed record WebDavEntry(
    string Href,
    bool IsDirectory,
    long? ContentLength,
    DateTimeOffset? LastModified,
    string? ETag
);