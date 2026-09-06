using BeeQ.FileStorage.Builder;
using BeeQ.FileStorage.LocalDisk;
using BeeQ.FileStorage.Service;
using BeeQ.FileStorage.WebDAV;
using System.Security.Cryptography;
using System.Text;

#pragma warning disable IDE0130 // Namespace does not match folder structure - Extension Methods

namespace BeeQ.FileStorage;

public static class IWebDAVExtensions
{
    /// <summary>
    /// Extension to use the Local Disk to storage the Files
    /// </summary>
    /// <param name="builder">Empty File Storage Builder</param>
    /// <param name="schemaName">Optional schema name used to distinguish storage namespaces</param>
    /// <param name="basePath">BasePath in Disk</param>
    /// <returns>The configured file storage</returns>
    public static IWebDAVService UseWebDAV(this IFileStorageBuilder builder, string schemaName, Action<WebDAVConfiguration> options)
    {
        var opt = new WebDAVConfiguration();
        options.Invoke(opt);
        return builder.Use<Guid>(schemaName)
            .CustomBuild<WebDAVService, IWebDAVService>(() => new WebDAVService(opt));
    }

        /*
        var cfg = new WebDAVConfiguration();
        config(cfg);

        cfg.Validate();

        return builder
            .Use<Guid>(schemaName)
            .OnCreateId(OnCreateId)
            .UseCustomFullPath(info => UseCustomFullPath(info, cfg))

            .OnUploadBytes(async info =>
            {
                var http = new HttpClient();
                var content = new ByteArrayContent(info.Content);
                cfg.OnOnCreateRequest?.Invoke(http, content);
                
                var response = await http.PutAsync(info.FullPath, content);
                response.EnsureSuccessStatusCode();
            })

            .OnGetStream(async info =>
            {
                var http = new HttpClient();
                cfg.OnOnCreateRequest?.Invoke(http, null);

                var response = await http.GetAsync(info.FullPath,HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStreamAsync();
            })

            .Build();
        */
}


#pragma warning restore IDE0130 // Namespace does not match folder structure
