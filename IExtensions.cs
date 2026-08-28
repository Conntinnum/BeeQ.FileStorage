using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeQ;

public static class IFileStorageExtensions
{
    public static async Task<byte[]> GetBytes(this Stream stream)
    {
        if (stream is MemoryStream ms)
            return ms.ToArray();

        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }
}
