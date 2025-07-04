using Common = MDict.Csharp.Utils.Utils;

namespace MDict.Csharp.Models;

/// <summary>
/// MDX file model for MDict.
/// </summary>
public class MdxDict : Dict
{
    /// <summary>
    /// Create a new instance with the specified file name and options.
    /// </summary>
    /// <param name="fname"></param>
    /// <param name="options"></param>
    public MdxDict(string fname, MDictOptions? options = null) : base(fname, options)
    {
    }

    /// <summary>
    /// Lookup the definition of a word.
    /// </summary>
    /// <param name="word">
    /// The word to look up. It should be a full word.
    /// </param>
    /// <returns>
    /// A tuple containing the key text and its definition.
    /// </returns>
    public (string KeyText, string? Definition) Lookup(string word)
    {
        var keyWordItem = LookupKeyBlockByWord(word);
        if (keyWordItem == null)
        {
            return (word, null);
        }

        var def = LookupRecordByKeyBlock(keyWordItem);
        if (def == null)
        {
            return (word, null);
        }

        return (word, meta.Decoder.Decode(def));
    }

    /// <summary>
    /// Fetch the definition of a keyword item.
    /// </summary>
    /// <param name="keywordItem">
    /// The keyword item to fetch the definition for.
    /// </param>
    /// <returns>
    /// A tuple containing the key text and its definition.
    /// </returns>
    public (string KeyText, string? Definition) Fetch(KeyWordItem keywordItem)
    {
        var def = LookupRecordByKeyBlock(keywordItem);
        if (def == null)
        {
            return (keywordItem.KeyText, null);
        }

        return (keywordItem.KeyText, meta.Decoder.Decode(def));
    }

    /// <summary>
    /// Fetch the definition of a keyword item by its prefix.
    /// </summary>
    /// <param name="prefix">
    /// The prefix to search for. It should be a word prefix.
    /// </param>
    /// <returns>
    /// A list of keyword items that start with the given prefix.
    /// </returns>
    public List<KeyWordItem> Prefix(string prefix)
    {
        var keywordList = Associate(prefix);
        return keywordList.Where(item => item.KeyText.StartsWith(prefix)).ToList();
    }

    /// <summary>
    /// Associate a phrase with its keyword items.
    /// </summary>
    /// <param name="phrase">
    /// The phrase to associate with keyword items. It should be a full word or phrase.
    /// </param>
    /// <returns>
    /// A list of keyword items associated with the given phrase.
    /// </returns>
    public List<KeyWordItem> Associate(string phrase)
    {
        var keyBlockItem = LookupKeyBlockByWord(phrase, true);
        if (keyBlockItem == null)
        {
            return new List<KeyWordItem>();
        }

        return keywordList.Where(keyword => keyword.KeyBlockIdx == keyBlockItem.KeyBlockIdx).ToList();
    }

    /// <summary>
    /// Suggest keyword items based on a phrase and an edit distance.
    /// </summary>
    /// <param name="phrase">
    ///The phrase to associate with keyword items. It should be a full word or phrase.
    /// </param>
    /// <param name="distance">
    /// The maximum edit distance allowed for suggestions, like 3 or 5.
    /// </param>
    /// <returns>
    /// A list of keyword items that are suggested based on the given phrase and edit distance.
    /// </returns>
    public List<KeyWordItem> Suggest(string phrase, uint distance)
    {
        var keywordList = Associate(phrase);
        var suggestList = new List<KeyWordItem>();

        foreach (var item in keywordList)
        {
            var key = Strip(item.KeyText);
            var ed = Common.LevenshteinDistance(key, Strip(phrase));
            if (ed <= distance)
            {
                suggestList.Add(item);
            }
        }

        return suggestList;
    }

    /// <summary>
    /// Fetch the definition of a keyword item.
    /// </summary>
    /// <param name="keywordItem">
    /// The keyword item to fetch the definition for.
    /// </param>
    /// <returns>
    /// A tuple containing the key text and its definition.
    /// </returns>
    public (string KeyText, string? Definition) FetchDefinition(KeyWordItem keywordItem)
    {
        var def = LookupRecordByKeyBlock(keywordItem);
        if (def == null)
        {
            return (keywordItem.KeyText, null);
        }

        return (keywordItem.KeyText, meta.Decoder.Decode(def));
    }

    /// <summary>
    /// Perform a fuzzy search for words based on a given word, fuzzy size, and edit distance gap.
    /// </summary>
    /// <param name="word">
    /// The word to search for. It should be a full word or phrase.
    /// </param>
    /// <param name="fuzzySize">
    /// The maximum number of fuzzy words to return. It should be a positive integer.
    /// </param>
    /// <param name="distance">
    /// The maximum edit distance allowed for fuzzy search, like 3 or 5.
    /// </param>
    /// <returns>
    /// A list of fuzzy words that match the given word within the specified edit distance.
    /// </returns>
    public List<FuzzyWord> FuzzySearch(string word, int fuzzySize, uint distance)
    {
        var fuzzyWords = new List<FuzzyWord>();
        var keywordList = Associate(word);

        foreach (var item in keywordList)
        {
            var key = Strip(item.KeyText);
            var ed = Common.LevenshteinDistance(key, Strip(word));
            if (ed <= distance)
            {
                fuzzyWords.Add(new FuzzyWord
                {
                    RecordStartOffset = item.RecordStartOffset,
                    RecordEndOffset = item.RecordEndOffset,
                    KeyText = item.KeyText,
                    KeyBlockIdx = item.KeyBlockIdx,
                    Ed = ed
                });
            }
        }

        return fuzzyWords
            .OrderBy(f => f.Ed)
            .Take(fuzzySize)
            .ToList();
    }
}

/// <summary>
/// Represents a fuzzy word with its edit distance.
/// </summary>
public class FuzzyWord : KeyWordItem
{
    /// <summary>
    /// The edit distance of the fuzzy word from the original word.
    /// </summary>
    public int Ed { get; set; }
}
