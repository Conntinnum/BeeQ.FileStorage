namespace BeeQ.FileStorage;

internal static class IFileStorageExtensions
{
    public static async Task<byte[]> GetBytes(this Stream stream)
    {
        using (stream)
        {
            if (stream.CanSeek)
                stream.Position = 0;

            if (stream is MemoryStream ms)
                return ms.ToArray();

            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
    }
}
