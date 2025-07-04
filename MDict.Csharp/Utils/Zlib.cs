using System.IO.Compression;

namespace MDict.Csharp.Utils;

/// <summary>
/// A utility class for decompressing data.
/// </summary>
internal class Zlib
{
    public static byte[] Decompress(byte[] compressedData)
    {
        using var outputStream = new MemoryStream();
        using (var compressedStream = new MemoryStream(compressedData))
        using (var decompressor = new ZLibStream(compressedStream, CompressionMode.Decompress, leaveOpen: false))
        {
            decompressor.CopyTo(outputStream);
        }
        return outputStream.ToArray();
    }
}
