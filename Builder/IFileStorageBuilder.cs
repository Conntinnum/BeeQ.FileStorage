using BeeQ.FileStorage.Service;

namespace BeeQ.FileStorage.Builder;

/// <summary>
/// Builder used to configure and create an <see cref="Service.IFileStorage{TId}"/> instance.
/// </summary>
/// <remarks>
/// Use this builder to select the primary key type and to configure behavior for uploads and retrievals.
/// </remarks>
public interface IFileStorageBuilder
{
    /// <summary>
    /// Specify the primary key type for the file storage and an optional schema name.
    /// </summary>
    /// <typeparam name="TId">The type used as the primary identifier for stored files.</typeparam>
    /// <param name="schemaName">Optional schema name used to distinguish storage namespaces.</param>
    /// <returns>A typed <see cref="IFileStorageBuilder{TId}"/> to continue configuration.</returns>
    IFileStorageBuilder<TId> Use<TId>(string? schemaName = null);
}

public interface IFileStorageBuilder<TId>
{
    /// <summary>
    /// The configured schema name (if any) for this builder.
    /// </summary>
    string? SchemaName { get; }

    /// <summary>
    /// Use the default full path implementation provided by the storage.
    /// </summary>
    /// <returns>The builder for further configuration.</returns>
    IFileStorageBuilder<TId> UseStandardFullPath();

    /// <summary>
    /// Provide a custom template function that builds the storage full path for a file.
    /// </summary>
    /// <param name="template">Function that receives the full identifier and returns the path.</param>
    /// <returns>The builder for further configuration.</returns>
    IFileStorageBuilder<TId> UseCustomFullPath(Func<IFileStorageFullIdentifier<TId>, string> template);

    /// <summary>
    /// Register a callback invoked when a file is uploaded as a stream.
    /// </summary>
    /// <param name="onUpload">Callback receiving upload information and the content stream.</param>
    /// <returns>The builder for further configuration.</returns>
    IFileStorageBuilder<TId> OnUploadStream(Func<IFileUploadInfo<TId, Stream>, Task> onUpload);

    /// <summary>
    /// Register a callback invoked when a file is uploaded as a byte array.
    /// </summary>
    /// <param name="onUpload">Callback receiving upload information and the content bytes.</param>
    /// <returns>The builder for further configuration.</returns>
    IFileStorageBuilder<TId> OnUploadBytes(Func<IFileUploadInfo<TId, byte[]>, Task> onUpload);

    /// <summary>
    /// Register a callback invoked when a file is uploaded as a base64 string.
    /// </summary>
    /// <param name="onUpload">Callback receiving upload information and the base64 content.</param>
    /// <returns>The builder for further configuration.</returns>
    IFileStorageBuilder<TId> OnUploadBase64(Func<IFileUploadInfo<TId, string>, Task> onUpload);

    /// <summary>
    /// Provide a function to create a new id for an uploaded file.
    /// </summary>
    /// <param name="onCreateId">Callback receiving schema name and filename and returning the new id.</param>
    /// <returns>The builder for further configuration.</returns>
    IFileStorageBuilder<TId> OnCreateId(Func<(string? SchemaName, string Filename), Task<TId>> onCreateId);

    /// <summary>
    /// Provide a function to derive a filename given an id.
    /// </summary>
    /// <param name="onGetFilename">Function that receives an id and returns the filename.</param>
    /// <returns>The builder for further configuration.</returns>
    IFileStorageBuilder<TId> OnGetFilename(Func<TId, string> onGetFilename);

    /// <summary>
    /// Provide the function that retrieves raw bytes for a stored file.
    /// </summary>
    /// <param name="getBytes">Function that receives file info and returns the bytes or null.</param>
    /// <returns>The builder for further configuration.</returns>
    IFileStorageBuilder<TId> OnGetBytes(Func<IFileInfo<TId>, Task<byte[]?>> getBytes);

    /// <summary>
    /// Provide the function that retrieves base64 content for a stored file.
    /// </summary>
    /// <param name="getBase64">Function that receives file info and returns the base64 string or null.</param>
    /// <returns>The builder for further configuration.</returns>
    IFileStorageBuilder<TId> OnGetBase64(Func<IFileInfo<TId>, Task<string?>> getBase64);

    /// <summary>
    /// Provide the function that retrieves a stream for a stored file.
    /// </summary>
    /// <param name="getStream">Function that receives file info and returns a stream or null.</param>
    /// <returns>The builder for further configuration.</returns>
    IFileStorageBuilder<TId> OnGetStream(Func<IFileInfo<TId>, Task<Stream?>> getStream);

    /// <summary>
    /// Build the configured <see cref="IFileStorage{TId}"/> instance.
    /// </summary>
    /// <returns>The configured file storage.</returns>
    IFileStorage<TId> Build();

    /// <summary>
    /// Build a custom service that implements <see cref="IFileStorage{TId}"/> and inherits from <see cref="FileStorageService{TId}"/>.
    /// </summary>
    /// <typeparam name="TService">Custom File Storage Service</typeparam>
    /// <typeparam name="TIService">Custom File Storage Interface</typeparam>
    /// <param name="builder">Builder function</param>
    /// <returns>The custom file storage instance.</returns>
    TIService CustomBuild<TService, TIService>(Func<TService> builder)
        where TIService : IFileStorage<TId>
        where TService : FileStorageService<TId>, TIService;
}
