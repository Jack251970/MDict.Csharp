using System.Text;
using System.Text.RegularExpressions;

namespace MDict.Csharp.Utils;

/// <summary>
/// Example usage:
/// string input = "café";
/// 
/// bool has = LatinHelper.HasLatinies(input); // true
/// string replaced = LatinHelper.ReplaceLatinies(input); // "cafe\u9999"
/// </summary>
internal static partial class LatinUtils
{
    /// <summary>
    /// Replace Latin combining diacritical marks with '\u9999'
    /// </summary>
    /// <param name="word">The input string to process</param>
    /// <returns>A string with Latin diacritics replaced by '\u9999'</returns>
    public static string ReplaceLatinies(string word)
    {
        // Normalize to FormD (NFD) to decompose characters with diacritics
        string normalized = word.Normalize(NormalizationForm.FormD);

        // Replace Unicode combining diacritical marks (U+0300 to U+036F) with U+9999
        return LatinDiacriticRegex().Replace(normalized, "\u9999");
    }

    /// <summary>
    /// Check if a word contains Latin combining diacritical marks
    /// </summary>
    /// <param name="word">The input string to check</param>
    /// <returns>True if it contains Latin diacritics, otherwise false</returns>
    public static bool HasLatinies(string word)
    {
        // Normalize to FormD (NFD) to decompose characters with diacritics
        string normalized = word.Normalize(NormalizationForm.FormD);

        // Search for any character in the diacritical mark range (U+0300 to U+036F)
        return LatinDiacriticRegex().IsMatch(normalized);
    }

    [GeneratedRegex("[\\u0300-\\u036f]")]
    private static partial Regex LatinDiacriticRegex();
}
