namespace MDict.Csharp.Utils;

internal static class Lzo1xWrapper
{
    private static readonly Lzo1x lzo = new();

    public static byte[] Decompress(byte[] input, int initSize = 16000, int blockSize = 8192)
    {
        var state = new Lzo1x.State
        {
            inputBuffer = input,
            outputBuffer = new byte[initSize + blockSize] // Preallocate buffer (can grow dynamically)
        };

        var result = lzo.Decompress(state);
        if (result != Lzo1x.OK)
        {
            throw new InvalidOperationException($"LZO decompression failed with code: {result}");
        }

        return state.outputBuffer;
    }

    public static byte[] Compress(Lzo1x.State state)
    {
        var result = lzo.Compress(state);
        if (result != Lzo1x.OK)
        {
            throw new InvalidOperationException($"LZO compression failed with code: {result}");
        }

        return state.outputBuffer;
    }
}

