# BeeQ.FileStorage
Ligero y extensible constructor de almacenamiento de archivos para .NET. BeeQ.FileStorage ofrece una pequeña API fluida para configurar cómo se identifican, suben y recuperan los archivos. Incluye utilidades para almacenar archivos en disco local y un builder que permite registrar manejadores personalizados de subida y recuperación.

Targets soportados: .NET 6, .NET 7, .NET 8, .NET 9, .NET 10

## Características

- Builder fluido para configurar la creación de ids, mapeo de nombres de archivo y rutas de almacenamiento
- Manejadores de subida para Stream, byte[] y cadenas en base64
- Manejadores de recuperación que retornan Stream, byte[] o cadenas base64
- Extensión de conveniencia para disco local (UseLocalDisk)

## Instalación

Instala el paquete NuGet:

```bash
dotnet add package BeeQ.FileStorage
```

## Inicio rápido

Este ejemplo muestra cómo configurar una instancia de storage, subir un archivo y obtenerlo como stream.

```csharp
using BeeQ.FileStorage;

// Ejemplo de configuración
var contentBase64 = "..."; // contenido en base64

var database = ...;

// Configurar un storage personalizado para ids GUID
var logos = Storage.Use<Guid>("logos")
	.UseCustomFullPath(info => $"logos/{DateTime.Now:yyyy/MM}/{info.Filename}")

	// Opcional: mapear un id de vuelta a un filename
	.OnGetFilename(id => database.ObtenerFilename(id))

	// Manejar subida cuando el contenido se provee como Stream
	.OnUploadStream(async info =>
	{
		using FileStream file = File.Create(info.FullPath);
		await info.Content.CopyToAsync(file);
	})

	// Indicar cómo abrir el contenido almacenado como stream
	.OnGetStream(async info => File.Open(info.FullPath, FileMode.Open))

	.Build();

// Subir (ejemplo con base64)
var id = await logos.Upload("coca-cola.png", contentBase64);

// Recuperar stream
var stream = await logos.GetStream(id);
```

## Conveniencia para disco local

BeeQ.FileStorage incluye una extensión de conveniencia para configurar rápidamente un storage respaldado por disco local:

```csharp
var imagenes = Storage.Create().UseLocalDisk("imagenes", "C:\\");
var id = await imagenes.Upload("coca-cola.png", contentBase64);
var stream = await imagenes.GetStream(id);
```

Esta extensión configura valores por defecto razonables y una generación determinista de ids usando MD5 sobre schema+filename (GUID).

## Resumen de la API

- Storage: punto de entrada estático
  - Storage.Use<TId>(schemaKey) -> IFileStorageBuilder<TId>
  - Storage.Create() -> IFileStorageBuilder
- IFileStorageBuilder<TId> 
  - UseStandardFullPath(), UseCustomFullPath(Func<IFileStorageFullIdentifier<TId>, string>)
  - OnUploadStream/OnUploadBytes/OnUploadBase64(...) para registrar manejadores de subida
  - OnGetStream/OnGetBytes/OnGetBase64(...) para registrar manejadores de recuperación
  - OnCreateId(Func<(string? SchemaName, string Filename), Task<TId>>) para proveer la creación de ids
  - Build() -> IFileStorage<TId>
- IFileStorage<TId>
  - Upload(filename, content) sobrecargas para base64, byte[] y Stream
  - GetStream/GetBytes/GetBase64 para recuperar contenido almacenado

Revisa la documentación XML en la librería para más detalles sobre las interfaces y miembros.

## Notas de integración

- La librería es ligera y no impone un modelo de registro DI. Puedes crear instancias de storage en el arranque y registrarlas en tu contenedor DI si lo deseas.
- Asegúrate de que tus manejadores de subida administren streams y recursos correctamente (dispose o copiar streams cuando sea necesario).

## Contribuciones

Son bienvenidas. Abre issues o pull requests en el repositorio. Sigue el estilo de código existente e incluye pruebas cuando corresponda.

## Licencia

Consulta el archivo LICENSE en este repositorio.
