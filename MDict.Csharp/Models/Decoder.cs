using System.Text;

namespace MDict.Csharp.Models;

/// <summary>
/// A class for decoding byte arrays into strings using various encodings.
/// </summary>
public class TextDecoder
{
    private static readonly Encoding utf16le = Encoding.Unicode; // UTF-16LE
    private static readonly Encoding utf8 = Encoding.UTF8;
    private static readonly Encoding big5 = Encoding.GetEncoding("big5");
    private static readonly Encoding gb18030 = Encoding.GetEncoding("gb18030");

    private readonly DecoderType _decoderType = DecoderType.UTF8;

    /// <summary>
    /// Default constructor that initializes the decoder to use UTF-8 encoding.
    /// </summary>
    internal TextDecoder() : this(DecoderType.UTF8)
    {
        
    }

    /// <summary>
    /// Constructor that initializes the decoder with a specific encoding type.
    /// </summary>
    /// <param name="type"></param>
    internal TextDecoder(DecoderType type)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        _decoderType = type;
    }

    /// <summary>
    /// Decodes a byte array into a string using the specified encoding type.
    /// </summary>
    /// <param name="buf"></param>
    /// <returns></returns>
    public string Decode(byte[] buf)
    {
        return _decoderType switch
        {
            DecoderType.UTF16LE => utf16le.GetString(buf),
            DecoderType.UTF8 => utf8.GetString(buf),
            DecoderType.Big5 => big5.GetString(buf),
            DecoderType.GB18030 => gb18030.GetString(buf),
            _ => utf8.GetString(buf)
        };
    }

    /// <summary>
    /// Enumeration of supported decoder types.
    /// </summary>
    public enum DecoderType
    {
        /// <summary>
        /// Decodes using UTF-16 Little Endian encoding.
        /// </summary>
        UTF16LE,

        /// <summary>
        /// Decodes using UTF-8 encoding.
        /// </summary>
        UTF8,

        /// <summary>
        /// Decodes using Big5 encoding.
        /// </summary>
        Big5,

        /// <summary>
        /// Decodes using GB18030 encoding.
        /// </summary>
        GB18030
    }
}
