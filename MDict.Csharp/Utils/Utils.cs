using System.Buffers.Binary;
using System.Text;
using System.Text.RegularExpressions;

namespace MDict.Csharp.Utils;

internal static partial class Utils
{
    /// <summary>
    /// 模式映射，用于剥离特定关键字中的字符
    /// </summary>
    public static readonly Dictionary<string, Regex> REGEXP_STRIPKEY = new()
    {
        { "mdx", new Regex(@"[().,\-&、 '/\\@_$\\!]()", RegexOptions.Compiled) },
        { "mdd", new Regex(@"([.][^.]*$)|[()., '/@]", RegexOptions.Compiled) },
    };

    private static readonly Encoding UTF16Encoding = Encoding.Unicode;

    /// <summary>
    /// 从给定的 Buffer 创建一个新的 byte[]。
    /// </summary>
    public static byte[] NewByteArray(byte[] buf, int offset, int len)
    {
        return buf.Skip(offset).Take(len).ToArray();
    }

    /// <summary>
    /// 从 Buffer 中读取 UTF-16 编码的字符串。
    /// </summary>
    public static string ReadUTF16(byte[] buf, int offset, int length)
    {
        return UTF16Encoding.GetString(buf, offset, length);
    }

    /// <summary>
    /// 获取文件名的扩展名。
    /// </summary>
    public static string GetExtension(string filename, string defaultExt)
    {
        var match = Regex.Match(filename, @"(?:\.([^.]+))?$");
        return match.Success ? match.Groups[1].Value : defaultExt;
    }

    /// <summary>
    /// 返回三个数字中的最小值。
    /// </summary>
    public static int TripleMin(int a, int b, int c)
    {
        return Math.Min(a, Math.Min(b, c));
    }

    /// <summary>
    /// 计算两个字符串之间的 Levenshtein 距离。
    /// </summary>
    public static int LevenshteinDistance(string a, string b)
    {
        if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b)) return 9999;

        int m = a.Length, n = b.Length;
        var dp = new int[m + 1, n + 1];

        for (int i = 0; i <= m; i++) dp[i, 0] = i;
        for (int j = 0; j <= n; j++) dp[0, j] = j;

        for (int i = 1; i <= m; i++)
        {
            for (int j = 1; j <= n; j++)
            {
                if (a[i - 1] == b[j - 1])
                    dp[i, j] = dp[i - 1, j - 1];
                else
                    dp[i, j] = TripleMin(dp[i - 1, j] + 1, dp[i, j - 1] + 1, dp[i - 1, j - 1] + 1);
            }
        }

        return dp[m, n];
    }

    /// <summary>
    /// 解析 XML 格式的头部文本，提取属性。
    /// </summary>
    public static Dictionary<string, object> ParseHeader(string headerText)
    {
        var headerAttr = new Dictionary<string, object>();

        foreach (Match match in Regex.Matches(headerText, @"(\w+)=""((.|\r|\n)*?)"""))
        {
            headerAttr[match.Groups[1].Value] = System.Net.WebUtility.HtmlDecode(match.Groups[2].Value);
        }

        if (headerAttr.TryGetValue("StyleSheet", out var val) && val is string sheetText)
        {
            var lines = sheetText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var styleSheet = new Dictionary<string, string[]>();
            for (int i = 0; i + 2 < lines.Length; i += 3)
            {
                styleSheet[lines[i]] = new[] { lines[i + 1], lines[i + 2] };
            }
            headerAttr["StyleSheet"] = styleSheet;
        }

        return headerAttr;
    }

    public static byte UInt8BEtoNumber(byte[] bytes) => bytes[0];

    public static ushort UInt16BEtoNumber(byte[] bytes) =>
        BinaryPrimitives.ReadUInt16BigEndian(bytes);

    public static uint UInt32BEtoNumber(byte[] bytes) =>
        BinaryPrimitives.ReadUInt32BigEndian(bytes);

    public static long UInt64BEtoNumber(byte[] bytes)
    {
        if (bytes[1] >= 0x20 || bytes[0] > 0)
            throw new InvalidOperationException("uint64 larger than 2^53, JS may lose accuracy");

        // only use lower 53-bits for JS safe integer
        long value = 0;
        for (int i = 0; i < 8; i++)
        {
            value <<= 8;
            value |= bytes[i];
        }

        return value;
    }

    public static long ReadNumber(byte[] buffer, NumFmt format)
    {
        return format switch
        {
            NumFmt.UInt8 => UInt8BEtoNumber(buffer),
            NumFmt.UInt16 => UInt16BEtoNumber(buffer),
            NumFmt.UInt32 => UInt32BEtoNumber(buffer),
            NumFmt.UInt64 => UInt64BEtoNumber(buffer),
            _ => 0
        };
    }

    public static long B2N(byte[] data)
    {
        return data.Length switch
        {
            1 => UInt8BEtoNumber(data),
            2 => UInt16BEtoNumber(data),
            4 => UInt32BEtoNumber(data),
            8 => UInt64BEtoNumber(data),
            _ => 0
        };
    }

    /// <summary>
    /// 使用简单的加密算法快速解密数据。
    /// </summary>
    public static byte[] FastDecrypt(byte[] data, byte[] key)
    {
        byte previous = 0x36;
        for (int i = 0; i < data.Length; i++)
        {
            byte t = (byte)((data[i] >> 4 | data[i] << 4) & 0xff);
            t ^= previous;
            t ^= (byte)(i & 0xff);
            t ^= key[i % key.Length];
            previous = data[i];
            data[i] = t;
        }
        return data;
    }

    /// <summary>
    /// 占位函数：Salsa20 解密
    /// </summary>
    public static byte[] SalsaDecrypt(byte[] data, byte[] key)
    {
        // TODO: implement Salsa20 decryption (8 rounds)
        return data;
    }

    /// <summary>
    /// 解密 MDX 格式的压缩块。
    /// </summary>
    public static byte[] MdxDecrypt(byte[] compBlock)
    {
        var keyinBuffer = new byte[8];
        Array.Copy(compBlock, 4, keyinBuffer, 0, 4);
        keyinBuffer[4] ^= 0x95;
        keyinBuffer[5] ^= 0x36;
        keyinBuffer[6] ^= 0x00;
        keyinBuffer[7] ^= 0x00;

        var key = Ripemd128.ComputeHash(keyinBuffer);
        var decrypted = FastDecrypt(compBlock.Skip(8).ToArray(), key);

        return compBlock.Take(8).Concat(decrypted).ToArray();
    }

    /// <summary>
    /// 将两个 ArrayBuffer 连接成一个新的 Buffer。
    /// </summary>
    public static byte[] AppendBuffer(byte[] buffer1, byte[] buffer2)
    {
        return buffer1.Concat(buffer2).ToArray();
    }

    /// <summary>
    /// Checks whether the given string represents a true value.
    /// </summary>
    /// <param name="v">The string to check.</param>
    /// <returns>True if the string represents a true value; otherwise, false.</returns>
    public static bool IsTrue(string? v)
    {
        if (string.IsNullOrEmpty(v)) return false;
        v = v.ToLowerInvariant();
        return v == "yes" || v == "true";
    }

    /// <summary>
    /// Compares two words in a case-insensitive manner with additional logic.
    /// </summary>
    /// <param name="word1">First word.</param>
    /// <param name="word2">Second word.</param>
    /// <returns>
    /// 0 if equal, -1 if word1 is less, 1 if word1 is greater.
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    public static int WordCompare(string word1, string word2)
    {
        if (string.IsNullOrEmpty(word1) || string.IsNullOrEmpty(word2))
        {
            throw new ArgumentException($"Invalid word comparison: '{word1}' and '{word2}'");
        }

        if (word1 == word2)
        {
            return 0;
        }

        int len = Math.Min(word1.Length, word2.Length);

        for (int i = 0; i < len; i++)
        {
            char w1 = word1[i];
            char w2 = word2[i];

            if (w1 == w2)
            {
                continue;
            }
            else if (char.ToLowerInvariant(w1) == char.ToLowerInvariant(w2))
            {
                continue;
            }
            else if (char.ToLowerInvariant(w1) < char.ToLowerInvariant(w2))
            {
                return -1;
            }
            else
            {
                return 1;
            }
        }

        return word1.Length < word2.Length ? -1 : 1;
    }

    /// <summary>
    /// Unescapes HTML entities in the given text.
    /// </summary>
    /// <param name="text">The input string containing entities.</param>
    /// <returns>The unescaped string.</returns>
    public static string UnescapeEntities(string text)
    {
        return text
            .Replace("&lt;", "<")
            .Replace("&gt;", ">")
            .Replace("&quot;", "\"")
            .Replace("&amp;", "&");
    }

    /// <summary>
    /// Applies styles from a style sheet to marked text fragments.
    /// </summary>
    /// <param name="styleSheet">Dictionary mapping style indices to style wrappers.</param>
    /// <param name="txt">Text containing style markers in the form of `number`.</param>
    /// <returns>Styled text.</returns>
    public static string SubstituteStylesheet(Dictionary<string, string[]> styleSheet, string txt)
    {
        var txtTagMatches = Regex.Matches(txt, @"`(\d+)`");
        var txtList = Regex.Split(txt, @"`\d+`").Skip(1).ToArray();

        var styledTxt = "";

        for (int i = 0; i < txtList.Length; i++)
        {
            var styleIndex = txtTagMatches[i].Groups[1].Value;
            if (!styleSheet.TryGetValue(styleIndex, out var style) || style.Length != 2)
            {
                throw new ArgumentException($"Missing or invalid style for index `{styleIndex}`");
            }

            styledTxt += style[0] + txtList[i] + style[1];
        }

        return styledTxt;
    }
}

/// <summary>
/// Enumeration for number formats.
/// </summary>
public enum NumFmt
{
    /// <summary>
    /// Represents an 8-bit unsigned integer.
    /// </summary>
    UInt8,

    /// <summary>
    /// Represents a 16-bit unsigned integer.
    /// </summary>
    UInt16,

    /// <summary>
    /// Represents a 32-bit unsigned integer.
    /// </summary>
    UInt32,

    /// <summary>
    /// Represents a 64-bit unsigned integer.
    /// </summary>
    UInt64
}
