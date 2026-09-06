# BeeQ.FileStorage
Lightweight, extensible file storage builder for .NET. BeeQ.FileStorage provides a small fluent API to configure how files are identified, uploaded and retrieved. It includes helpers to store files on local disk and a builder that allows registering custom upload and retrieval handlers.

Supported targets: .NET 6, .NET 7, .NET 8, .NET 9, .NET 10

## Features

- Fluent builder to configure file id creation, filename mapping and storage paths
- Upload handlers for Stream, byte[] and base64 string payloads
- Retrieval handlers returning Stream, byte[] or base64 string
- Local disk convenience extension (UseLocalDisk)
 - Built-in or extensible support for multiple storage transports: Local Disk, FTP, FTPS and WebDAV

## Installation

Install the NuGet package:

```bash
dotnet add package BeeQ.FileStorage
```

## Quick Start

This example shows configuring a storage instance, uploading a file and retrieving it as a stream.

```csharp
using BeeQ.FileStorage;

// Example setup
var contentBase64 = "..."; // base64 content

var database = ...;

// Configure a custom storage for GUID ids
var logos = Storage.Use<Guid>("logos")
	.UseCustomFullPath(info => $"logos/{DateTime.Now:yyyy/MM}/{info.Filename}")

	// Optionally map an id back to a filename
	.OnGetFilename(id => database.ObtenerFilename(id))

	// Handle upload when content is provided as a Stream
	.OnUploadStream(async info =>
	{
		using FileStream file = File.Create(info.FullPath);
		await info.Content.CopyToAsync(file);
	})

	// Provide how to open stored content as a stream
	.OnGetStream(async info => File.Open(info.FullPath, FileMode.Open))

	.Build();

// Upload (base64 example)
var id = await logos.Upload("coca-cola.png", contentBase64);

// Retrieve stream
var stream = await logos.GetStream(id);
```

## Local disk convenience

BeeQ.FileStorage includes a convenience extension to quickly configure a local-disk-backed storage:

```csharp
var imagenes = Storage.Create().UseLocalDisk("imagenes", "C:\\");
var id = await imagenes.Upload("coca-cola.png", contentBase64);
var stream = await imagenes.GetStream(id);
```

This extension sets up sensible defaults and a deterministic id generation using MD5 of schema+filename (GUID).

Note on other transports
-----------------------
The library is designed to be transport-agnostic. In addition to the Local Disk convenience extension, BeeQ.FileStorage can be used with remote storage transports such as FTP, FTPS and WebDAV. These transports can be integrated either by using provider-specific extensions (if available) or by registering custom upload and retrieval handlers with the builder (OnUpload*/OnGet* handlers).

## API overview

- Storage: static entry point
  - Storage.Use<TId>(schemaKey) -> IFileStorageBuilder<TId>
  - Storage.Create() -> IFileStorageBuilder
- IFileStorageBuilder<TId>
  - UseStandardFullPath(), UseCustomFullPath(Func<IFileStorageFullIdentifier<TId>, string>)
  - OnUploadStream/OnUploadBytes/OnUploadBase64(...) to register upload handlers
  - OnGetStream/OnGetBytes/OnGetBase64(...) to register retrieval handlers
  - OnCreateId(Func<(string? SchemaName, string Filename), Task<TId>>) to provide id creation
  - Build() -> IFileStorage<TId>
- IFileStorage<TId>
  - Upload(filename, content) overloads for base64, byte[] and Stream
  - GetStream/GetBytes/GetBase64 to retrieve stored content

Refer to the XML documentation in the library for more details on interfaces and members.

## Integration notes

- The library is lightweight and does not enforce a DI registration model. You may create storage instances at startup and register them in your DI container if desired.
- Ensure your upload handlers manage streams and resources correctly (dispose or copy streams when required).

## Contributing

Contributions are welcome. Please open issues or pull requests on the repository. Follow the existing code style and include tests where appropriate.

## License

See the LICENSE file in this repository.

