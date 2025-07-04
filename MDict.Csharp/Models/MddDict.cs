namespace MDict.Csharp.Models;

/// <summary>
/// MDict Mdd file model.
/// </summary>
public class MddDict : Dict
{
    /// <summary>
    /// Create a new instance with the specified file name and options.
    /// </summary>
    /// <param name="fname"></param>
    /// <param name="options"></param>
    public MddDict(string fname, MDictOptions? options = null)
        : base(fname, options)
    {
    }

    /// <summary>
    /// Result of locating a resource key in the Mdd file.
    /// </summary>
    /// <param name="KeyText"></param>
    /// <param name="Definition"></param>
    public record LocateResult(string KeyText, string? Definition);

    /// <summary>
    /// Locate the resource key
    /// </summary>
    /// <param name="resourceKey">Resource key</param>
    /// <returns>The key text and base64-encoded definition</returns>
    public LocateResult Locate(string resourceKey)
    {
        var item = LookupKeyBlockByWord(resourceKey);
        if (item == null)
        {
            return new LocateResult(resourceKey, null);
        }

        byte[] meaningBuff = LookupRecordByKeyBlock(item);
        if (meaningBuff == null || meaningBuff.Length == 0)
        {
            return new LocateResult(resourceKey, null);
        }

        string base64 = Convert.ToBase64String(meaningBuff);
        return new LocateResult(resourceKey, base64);
    }
}
