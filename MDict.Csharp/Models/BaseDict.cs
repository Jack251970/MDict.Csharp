using MDict.Csharp.Utils;
using System.Text;
using Common = MDict.Csharp.Utils.Utils;

namespace MDict.Csharp.Models;

/// <summary>
/// MDict options for configuring the behavior of the MDict parser.
/// </summary>
public class MDictOptions
{
    /// <summary>
    /// The passcode for decrypting the MDict file, if it is encrypted.
    /// </summary>
    public string? Passcode { get; set; }

    /// <summary>
    /// Debug mode flag. If true, enables debug output.
    /// </summary>
    public bool? Debug { get; set; }

    /// <summary>
    /// Indicates whether to resort the keyword list after loading.
    /// </summary>
    public bool? Resort { get; set; }

    /// <summary>
    /// Indicates whether to strip keys (remove special characters).
    /// This is useful for normalizing keys before searching.
    /// Default is true, meaning keys will be stripped.
    /// </summary>
    public bool? IsStripKey { get; set; }

    /// <summary>
    /// Indicates whether the keys are case-sensitive.
    /// </summary>
    public bool? IsCaseSensitive { get; set; }

    /// <summary>
    /// The type of encryption used in the MDict file.
    /// </summary>
    public int? EncryptType { get; set; }
}

/// <summary>
/// Represents the header of an MDict dictionary.
/// </summary>
public class MDictHeader : Dictionary<string, object?>
{
    internal new object? this[string key]
    {
        get
        {
            TryGetValue(key, out var value);
            return value;
        }
        set => base[key] = value;
    }
}

/// <summary>
/// Represents the key header of an MDict dictionary.
/// </summary>
public class KeyHeader
{
    /// <summary>
    /// Gets or sets the number of keyword blocks in the dictionary.
    /// </summary>
    public long KeywordBlocksNum { get; set; }

    /// <summary>
    /// Gets or sets the total number of keywords in the dictionary.
    /// </summary>
    public long KeywordNum { get; set; }

    /// <summary>
    /// Gets or sets the unpacked size of the key info block.
    /// </summary>
    public long KeyInfoUnpackSize { get; set; }

    /// <summary>
    /// Gets or sets the packed size of the key info block.
    /// </summary>
    public long KeyInfoPackedSize { get; set; }

    /// <summary>
    /// Gets or sets the packed size of the keyword block.
    /// </summary>
    public long KeywordBlockPackedSize { get; set; }
}

/// <summary>
/// Represents an item in the key info list of the MDict dictionary.
/// </summary>
public class KeyInfoItem
{
    /// <summary>
    /// Gets or sets the first key in the key block.
    /// </summary>
    public string FirstKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last key in the key block.
    /// </summary>
    public string LastKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the packed size of the key block.
    /// </summary>
    public long KeyBlockPackSize { get; set; }

    /// <summary>
    /// Gets or sets the accumulated offset for the packed key block.
    /// </summary>
    public long KeyBlockPackAccumulator { get; set; }

    /// <summary>
    /// Gets or sets the unpacked size of the key block.
    /// </summary>
    public long KeyBlockUnpackSize { get; set; }

    /// <summary>
    /// Gets or sets the accumulated offset for the unpacked key block.
    /// </summary>
    public long KeyBlockUnpackAccumulator { get; set; }

    /// <summary>
    /// Gets or sets the number of entries in the key block.
    /// </summary>
    public long KeyBlockEntriesNum { get; set; }

    /// <summary>
    /// Gets or sets the accumulated number of entries in the key block.
    /// </summary>
    public long KeyBlockEntriesNumAccumulator { get; set; }

    /// <summary>
    /// Gets or sets the index of the key block info in the key info list.
    /// </summary>
    public int KeyBlockInfoIndex { get; set; }
}

/// <summary>
/// Represents a keyword item in the MDict dictionary.
/// </summary>
public class KeyWordItem
{
    /// <summary>
    /// Gets or sets the start offset of the record for this keyword.
    /// </summary>
    public long RecordStartOffset { get; set; }

    /// <summary>
    /// Gets or sets the end offset of the record for this keyword.
    /// </summary>
    public long RecordEndOffset { get; set; }

    /// <summary>
    /// Gets or sets the text of the keyword.
    /// </summary>
    public string KeyText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the index of the key block that this keyword belongs to.
    /// </summary>
    public int KeyBlockIdx { get; set; }
}

/// <summary>
/// Represents the header information for a record in the MDict dictionary.
/// </summary>
public class RecordHeader
{
    /// <summary>
    /// Gets or sets the number of record blocks in the dictionary.
    /// </summary>
    public long RecordBlocksNum { get; set; }

    /// <summary>
    /// Gets or sets the total number of entries in the dictionary.
    /// </summary>
    public long EntriesNum { get; set; }

    /// <summary>
    /// Gets or sets the unpacked size of the record info block.
    /// </summary>
    public long RecordInfoCompSize { get; set; }

    /// <summary>
    /// Gets or sets the packed size of the record info block.
    /// </summary>
    public long RecordBlockCompSize { get; set; }
}

/// <summary>
/// Represents information about a record in the MDict dictionary.
/// </summary>
public class RecordInfo
{
    /// <summary>
    /// Gets or sets the start offset of the record in the packed data.
    /// </summary>
    public long PackSize { get; set; }

    /// <summary>
    /// Gets or sets the accumulated offset for the packed record data.
    /// </summary>
    public long PackAccumulateOffset { get; set; }

    /// <summary>
    /// Gets or sets the unpacked size of the record data.
    /// </summary>
    public long UnpackSize { get; set; }

    /// <summary>
    /// Gets or sets the accumulated offset for the unpacked record data.
    /// </summary>
    public long UnpackAccumulatorOffset { get; set; }
}

/// <summary>
/// Represents metadata for an MDict dictionary file.
/// </summary>
public class MdictMeta
{
    /// <summary>
    /// The file name of the MDict dictionary.
    /// </summary>
    public string Fname { get; set; } = string.Empty;

    /// <summary>
    /// The passcode used to decrypt the MDict file, if it is encrypted.
    /// </summary>
    public string Passcode { get; set; } = string.Empty;

    /// <summary>
    /// The file extension of the MDict dictionary, default is "mdx".
    /// </summary>
    public string Ext { get; set; } = "mdx";

    /// <summary>
    /// The version of the MDict engine that generated this dictionary file.
    /// </summary>
    public double Version { get; set; } = 2.0;

    /// <summary>
    /// The width of the number used in the dictionary, either 4 or 8 bytes.
    /// </summary>
    public int NumWidth { get; set; } = 4;

    /// <summary>
    /// The number format used in the dictionary, either UInt32 or UInt64.
    /// </summary>
    public NumFmt NumFmt { get; set; } = NumFmt.UInt32;

    /// <summary>
    /// The encoding used in the MDict dictionary, such as "UTF-8", "UTF-16", "BIG5", or "GB18030".
    /// </summary>
    public string Encoding { get; set; } = string.Empty;

    /// <summary>
    /// The text decoder used to decode byte arrays into strings based on the encoding.
    /// </summary>
    public TextDecoder Decoder { get; set; } = new();

    /// <summary>
    /// The encryption type used in the MDict file.
    /// </summary>
    public int Encrypt { get; set; } = 0;
}

/// <summary>
/// Base class for MDict file model.
/// </summary>
public class BaseDict
{
    private static readonly TextDecoder UTF_16LE_DECODER = new(TextDecoder.DecoderType.UTF16LE);
    private const string UTF16 = "UTF-16";

    private static readonly TextDecoder UTF_8_DECODER = new(TextDecoder.DecoderType.UTF8);
    private const string UTF8 = "UTF-8";

    private static readonly TextDecoder BIG5_DECODER = new(TextDecoder.DecoderType.Big5);
    private const string BIG5 = "BIG5";

    private static readonly TextDecoder GB18030_DECODER = new(TextDecoder.DecoderType.GB18030);
    private const string GB18030 = "GB18030";

    /// <summary>
    /// File path of this dictionary file.
    /// </summary>
    public readonly string FileName;

    // File scanner
    internal FileScanner scanner;

    // mdx meta
    internal MdictMeta meta = new();

    // Options
    internal MDictOptions options;

    // -------------------------
    // PART1: header
    // -------------------------

    // header start offset
    internal long _headerStartOffset;
    // header end offset
    internal long _headerEndOffset;
    // header data
    internal MDictHeader header;

    // ------------------------
    // PART2: keyHeader
    // ------------------------

    // keyHeader start offset
    internal long _keyHeaderStartOffset;
    // keyHeader end offset
    internal long _keyHeaderEndOffset;
    // keyHeader data
    internal KeyHeader keyHeader;

    // ------------------------
    // PART2: keyBlockInfo
    // ------------------------

    // keyBlockInfo start offset
    internal long _keyBlockInfoStartOffset;
    // keyBlockInfo end offset
    internal long _keyBlockInfoEndOffset;
    // keyBlockInfo data (Key Block Info list)
    internal List<KeyInfoItem> keyInfoList;

    // ------------------------
    // PART2: keyBlock
    // ------------------------

    // keyBlock start offset
    internal long _keyBlockStartOffset;
    // keyBlock end offset
    internal long _keyBlockEndOffset;
    // keyList data (Term list)
    internal List<KeyWordItem> keywordList;

    // ------------------------
    // PART2: recordHeader
    // ------------------------

    // recordHeader start offset
    internal long _recordHeaderStartOffset;
    // recordHeader end offset
    internal long _recordHeaderEndOffset;
    // recordHeader 数据
    internal RecordHeader recordHeader;

    // ------------------------
    // PART2: recordBlockInfo
    // ------------------------

    // recordInfo start offset
    internal long _recordInfoStartOffset;
    // recordInfo end offset
    internal long _recordInfoEndOffset;
    // recordBlockInfo 数据
    internal List<RecordInfo> recordInfoList;

    // ------------------------
    // PART2: recordBlock
    // ------------------------

    // recordBlock start offset
    internal long _recordBlockStartOffset;
    // recordBlock end offset
    internal long _recordBlockEndOffset;
    // keyData data
    internal List<Dictionary<string, object>> recordBlockDataList;

    /// <summary>
    /// BaseMDict constructor
    /// </summary>
    /// <param name="fname"></param>
    /// <param name="passcode"></param>
    /// <param name="options"></param>
    public BaseDict(string fname, string passcode = null!, MDictOptions options = null!)
    {
        FileName = fname;
        // the mdict file name
        meta.Fname = fname;
        // the dictionary file decrypt pass code
        meta.Passcode = passcode;
        // the dictionary file extension
        meta.Ext = Common.GetExtension(fname, "mdx");
        // the file scanner
        scanner = new FileScanner(fname);

        // set options
        this.options = options ?? new MDictOptions
        {
            Passcode = passcode,
            Debug = false,
            Resort = true,
            IsStripKey = true,
            IsCaseSensitive = false,
            EncryptType = -1
        };

        // -------------------------
        // dict header section
        //--------------------------
        // read the diction header info
        _headerStartOffset = 0;
        _headerEndOffset = 0;
        header = new MDictHeader();

        // -------------------------
        // dict key header section
        // --------------------------
        _keyHeaderStartOffset = 0;
        _keyHeaderEndOffset = 0;
        keyHeader = new KeyHeader();

        // -------------------------
        // dict key info section
        // --------------------------
        _keyBlockInfoStartOffset = 0;
        _keyBlockInfoEndOffset = 0;
        // key block info list
        keyInfoList = new List<KeyInfoItem>();

        // -------------------------
        // dict key block section
        // --------------------------
        _keyBlockStartOffset = 0;
        _keyBlockEndOffset = 0;
        keywordList = new List<KeyWordItem>();

        // -------------------------
        // dict record header section
        // --------------------------
        _recordHeaderStartOffset = 0;
        _recordHeaderEndOffset = 0;
        recordHeader = new RecordHeader();

        // -------------------------
        // dict record info section
        // --------------------------
        _recordInfoStartOffset = 0;
        _recordInfoEndOffset = 0;
        recordInfoList = new List<RecordInfo>();

        // -------------------------
        // dict record block section
        // --------------------------
        _recordBlockStartOffset = 0;
        _recordBlockEndOffset = 0;
        recordBlockDataList = new List<Dictionary<string, object>>();

        ReadDict();
    }

    internal string Strip(string key)
    {
        if (IsStripKey())
        {
            key = Common.REGEXP_STRIPKEY[meta.Ext].Replace(key, "$1");
        }
        if (!IsKeyCaseSensitive())
        {
            key = key.ToLower();
        }
        if (meta.Ext == "mdd")
        {
            key = Common.REGEXP_STRIPKEY[meta.Ext].Replace(key, "$1");
            key = key.Replace("_", "!");
        }
        return key.ToLower().Trim();
    }

    private bool IsKeyCaseSensitive()
    {
        return options.IsCaseSensitive == true || Common.IsTrue(header["isCaseSensitive"]?.ToString());
    }

    private bool IsStripKey()
    {
        return options.IsStripKey == true || Common.IsTrue(header["StripKey"]?.ToString());
    }

    private void ReadDict()
    {
        // STEP1: read header
        ReadHeader();

        // STEP2: read key header
        ReadKeyHeader();

        // STEP3: read key block info
        ReadKeyInfos();

        // STEP4: read key block
        // @depreciated
        // _readKeyBlock method is very slow, avoid invoke dirctly
        // this method will return the whole words list of the dictionaries file, this is very slow
        // NOTE: 本方法非常缓慢，也有可能导致内存溢出，请不要直接调用
        ReadKeyBlocks();

        // STEP5: read record header
        ReadRecordHeader();

        // STEP6: read record block info
        ReadRecordInfos();

        // STEP7: read record block
        // _readRecordBlock method is very slow, avoid invoke directly
        // this._readRecordBlock();

        // Finally: resort the keyword list
        keywordList.Sort((k1, k2) => string.Compare(k1.KeyText, k2.KeyText, StringComparison.Ordinal));
    }

    /// <summary>
    /// STEP 4.2. split keys from key block
    /// split key from key block buffer
    /// @param {Buffer} keyBlock key block buffer
    /// @param {number} keyBlockIdx
    /// </summary>
    /// <param name="keyBlock"></param>
    /// <param name="keyBlockIdx"></param>
    /// <returns></returns>
    protected List<KeyWordItem> SplitKeyBlock(byte[] keyBlock, int keyBlockIdx)
    {
        int width = meta.Encoding == "UTF-16" || meta.Ext == "mdd" ? 2 : 1;
        var keyList = new List<KeyWordItem>();

        int keyStartIndex = 0;
        while (keyStartIndex < keyBlock.Length)
        {
            byte[] meaningOffsetBuff = keyBlock.Skip(keyStartIndex).Take(meta.NumWidth).ToArray();
            var meaningOffset = Common.B2N(meaningOffsetBuff);

            int keyEndIndex = -1;
            int i = keyStartIndex + meta.NumWidth;
            while (i < keyBlock.Length)
            {
                if (width == 1 && keyBlock[i] == 0 || width == 2 && keyBlock[i] == 0 && keyBlock[i + 1] == 0)
                {
                    keyEndIndex = i;
                    break;
                }
                i += width;
            }

            if (keyEndIndex == -1) break;

            byte[] keyTextBuffer = keyBlock.Skip(keyStartIndex + meta.NumWidth).Take(keyEndIndex - (keyStartIndex + meta.NumWidth)).ToArray();
            string keyText = meta.Decoder.Decode(keyTextBuffer);

            if (keyList.Count > 0)
            {
                keyList[^1].RecordEndOffset = meaningOffset;
            }

            keyList.Add(new KeyWordItem
            {
                RecordStartOffset = meaningOffset,
                KeyText = keyText,
                KeyBlockIdx = keyBlockIdx,
                RecordEndOffset = -1
            });

            keyStartIndex = keyEndIndex + width;
        }

        return keyList;
    }

    /// <summary>
    /// STEP 1. read dictionary header
    /// Get mdx header info (xml content to object)
    /// [0:4], 4 bytes header length (header_byte_size), big-endian, 4 bytes, 16 bits
    /// [4:header_byte_size + 4] header_bytes
    /// [header_bytes_size + 4:header_bytes_size +8] adler32 checksum
    /// should be:
    /// <c>zlib.adler32(header_bytes) &amp; 0xffffffff == adler32</c>
    /// </summary>
    private void ReadHeader()
    {
        // [0:4], 4 bytes header length (header_byte_size), big-endian, 4 bytes, 16 bits
        var headerByteSizeBuff = scanner.ReadBuffer(0, 4);
        var headerByteSize = Common.B2N(headerByteSizeBuff);

        // [4:header_byte_size + 4] header_bytes
        var headerBuffer = scanner.ReadBuffer(4, (int)headerByteSize);

        // TODO: SKIP 4 bytes alder32 checksum
        // header_b_cksum should skip for now, because cannot get alder32 sum by js
        // const header_b_cksum = readChunk.sync(this.meta.fname, header_byte_size + 4, 4);
        // assert(header_b_cksum), "header_bytes checksum failed");

        // 4 bytes header size + header_bytes_size + 4bytes alder checksum
        _headerEndOffset = headerByteSize + 8;
        _keyHeaderStartOffset = _headerEndOffset;

        // header text in utf-16 encoding ending with `\x00\x00`, so minus 2
        // const headerText = common.readUTF16(headerBuffer, 0, headerByteSize - 2);
        var headerText = Encoding.Unicode.GetString(headerBuffer);

        // parse header info
        foreach (var kv in Common.ParseHeader(headerText))
        {
            header[kv.Key] = kv.Value;
        }

        // set header default configuration
        header["KeyCaseSensitive"] ??= "No";
        header["StripKey"] ??= "Yes";

        // encrypted flag
        // 0x00 - no encryption
        // 0x01 - encrypt record block
        // 0x02 - encrypt key info block
        meta.Encrypt = header["Encrypted"] switch
        {
            null or "" or "No" => 0,
            "Yes" => 1,
            _ => int.Parse(header["Encrypted"]?.ToString() ?? string.Empty)
        };

        if (options.EncryptType != null && options.EncryptType != -1)
        {
            meta.Encrypt = (int)options.EncryptType;
        }

        // stylesheet attribute if present takes from of:
        // style_number # 1-255
        // style_begin # or ''
        // style_end # or ''
        // TODO: splitstyle info

        // header_info['_stylesheet'] = {}
        // if header_tag.get('StyleSheet'):
        //   lines = header_tag['StyleSheet'].splitlines()
        //   for i in range(0, len(lines), 3):
        //        header_info['_stylesheet'][lines[i]] = (lines[i + 1], lines[i + 2])

        // before version 2.0, number is 4 bytes integer alias, int32
        // version 2.0 and above use 8 bytes, alias int64
        meta.Version = float.Parse(header["GeneratedByEngineVersion"]?.ToString() ?? string.Empty);
        if (meta.Version >= 2.0)
        {
            meta.NumWidth = 8;
            meta.NumFmt = NumFmt.UInt64;
        }
        else
        {
            meta.NumWidth = 4;
            meta.NumFmt = NumFmt.UInt32;
        }

        var enc = header["Encoding"]?.ToString() ?? string.Empty;
        switch (enc.ToLower())
        {
            case "":
                meta.Encoding = UTF8;
                meta.Decoder = UTF_8_DECODER;
                break;
            case "gbk":
            case "gb2312":
                meta.Encoding = GB18030;
                meta.Decoder = GB18030_DECODER;
                break;
            case "big5":
                meta.Encoding = BIG5;
                meta.Decoder = BIG5_DECODER;
                break;
            case "utf16":
            case "utf-16":
                meta.Encoding = UTF16;
                meta.Decoder = UTF_16LE_DECODER;
                break;
            default:
                meta.Encoding = UTF8;
                meta.Decoder = UTF_8_DECODER;
                break;
        }

        // determine the encoding and decoder, if extension is *.mdd
        if (meta.Ext == "mdd")
        {
            meta.Encoding = UTF16;
            meta.Decoder = UTF_16LE_DECODER;
        }
    }

    /// <summary>
    /// STEP 2. read key block header
    /// read key block header
    /// </summary>
    /// <exception cref="Exception"></exception>
    /// <exception cref="NotSupportedException"></exception>
    void ReadKeyHeader()
    {
        _keyHeaderStartOffset = _headerEndOffset;

        var headerMetaSize = meta.Version >= 2.0 ? 8 * 5 : 4 * 4;
        var keyHeaderBuff = scanner.ReadBuffer((int)_keyHeaderStartOffset, headerMetaSize);

        if ((meta.Encrypt & 1) != 0)
        {
            if (string.IsNullOrEmpty(meta.Passcode)) throw new Exception("User identification is needed to read encrypted file");
            throw new NotSupportedException("Encrypted file not supported yet");
        }

        var offset = 0;
        // [0:8]   - number of key blocks
        var keywordBlockNumBuff = keyHeaderBuff.Skip(offset).Take(meta.NumWidth).ToArray();
        keyHeader.KeywordBlocksNum = Common.B2N(keywordBlockNumBuff);
        offset += meta.NumWidth;

        // [8:16]  - number of entries
        var keywordNumBuff = keyHeaderBuff.Skip(offset).Take(meta.NumWidth).ToArray();
        keyHeader.KeywordNum = Common.B2N(keywordNumBuff);
        offset += meta.NumWidth;

        // [16:24] - number of key block info decompress size
        if (meta.Version >= 2.0)
        {
            // only for version > 2.0
            var keyInfoUnpackSizeBuff = keyHeaderBuff.Skip(offset).Take(meta.NumWidth).ToArray();
            keyHeader.KeyInfoUnpackSize = Common.B2N(keyInfoUnpackSizeBuff);
            offset += meta.NumWidth;
        }

        // [24:32] - number of key block info compress size
        var keyInfoPackedSizeBuff = keyHeaderBuff.Skip(offset).Take(meta.NumWidth).ToArray();
        var keyInfoPackedSize = Common.B2N(keyInfoPackedSizeBuff);
        offset += meta.NumWidth;
        keyHeader.KeyInfoPackedSize = keyInfoPackedSize;

        // [32:40] - number of key blocks total size, note, key blocks total size, not key block info
        var keywordBlockPackedSizeBuff = keyHeaderBuff.Skip(offset).Take(meta.NumWidth).ToArray();
        var keywordBlockPackedSize = Common.B2N(keywordBlockPackedSizeBuff);
        //offset += meta.NumWidth;
        keyHeader.KeywordBlockPackedSize = keywordBlockPackedSize;

        // 4 bytes alder32 checksum, after key info block (only >= v2.0)
        // set end offset
        _keyHeaderEndOffset = _keyHeaderStartOffset +
            headerMetaSize + (meta.Version >= 2.0 ? 4 : 0); /* 4 bytes adler32 checksum length, only for version >= 2.0 */
    }

    /// <summary>
    /// STEP 3. read key block info, if you want quick search, read at here already enough
    /// read key block info
    /// key block info list
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    private void ReadKeyInfos()
    {
        _keyBlockInfoStartOffset = _keyHeaderEndOffset;
        var keyBlockInfoBuff = scanner.ReadBuffer((int)_keyBlockInfoStartOffset, (int)keyHeader.KeyInfoPackedSize);

        var keyBlockInfoList = DecodeKeyInfo(keyBlockInfoBuff);

        _keyBlockInfoEndOffset = _keyBlockInfoStartOffset + keyHeader.KeyInfoPackedSize;

        if (keyHeader.KeywordBlocksNum != keyBlockInfoList.Count)
        {
            throw new InvalidOperationException("The number of key blocks does not match the key info list.");
        }

        keyInfoList = keyBlockInfoList;

        // NOTE: must set at here, otherwise, if we haven't invoked the _decodeKeyBlockInfo method,
        // var `_recordBlockStartOffset` will not be set.
        _recordBlockStartOffset = _keyBlockInfoEndOffset + keyHeader.KeywordBlockPackedSize;

        // NOTE: Fully checked here.
    }

    /// <summary>
    /// STEP 3.1. decode key block info, this function will invokde in `_readKeyBlockInfo`
    /// and decode the first key and last key infomation, etc.
    /// @param { Uint8Array } keyInfoBuff key block info buffer
    /// </summary>
    /// <param name="keyInfoBuff"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private List<KeyInfoItem> DecodeKeyInfo(byte[] keyInfoBuff)
    {
        var keyBlockNum = keyHeader.KeywordBlocksNum;

        if (meta.Version == 2.0)
        {
            var packType = string.Join("", keyInfoBuff[0..4]);
            // const _alder32Buff = keyInfoBuff.slice(4, 8)

            // const numEntries = this.keyHeader.entriesNum;
            if (meta.Encrypt == 2)
            {
                keyInfoBuff = Common.MdxDecrypt(keyInfoBuff);
            }

            if (keyHeader.KeyInfoPackedSize != keyInfoBuff.Length)
            {
                throw new InvalidOperationException($"key_block_info keyInfoPackedSize {keyHeader.KeyInfoPackedSize} should equal to key-info buffer length {keyInfoBuff.Length}");
            }

            if (meta.Version >= 2.0 && packType == "2000")
            {
                // For version 2.0, will compress by zlib, lzo just for 1.0
                // key_block_info_compressed[0:8] => compress_type
                var keyInfoBuffUnpacked = Zlib.Decompress(keyInfoBuff[8..]);

                // TODO: check the alder32 checksum
                // adler32 = unpack('>I', key_block_info_compressed[4:8])[0]
                // assert(adler32 == zlib.adler32(key_block_info) & 0xffffffff)

                // this.keyHeader.keyInfoUnpackSize only exist when version >= 2.0
                if (keyHeader.KeyInfoUnpackSize != keyInfoBuffUnpacked.Length)
                {
                    throw new InvalidOperationException($"key_block_info keyInfoUnpackSize {keyHeader.KeyInfoUnpackSize} should equal to keyInfoBuffUnpacked buffer length {keyInfoBuffUnpacked.Length}");
                }

                keyInfoBuff = keyInfoBuffUnpacked;
            }
        }

        var keyBlockInfoList = new List<KeyInfoItem>();
        var entriesCount = 0L;
        var kbCount = 0;
        var indexOffset = 0;
        var kbPackSizeAccu = 0L;
        var kbUnpackSizeAccu = 0L;

        while (kbCount < keyBlockNum)
        {
            var blockWordCount = Common.B2N(keyInfoBuff.Skip(indexOffset).Take(meta.NumWidth).ToArray());
            indexOffset += meta.NumWidth;

            var firstWordSize = Common.B2N(keyInfoBuff.Skip(indexOffset).Take(meta.NumWidth / 4).ToArray());
            indexOffset += meta.NumWidth / 4;
            if (meta.Version >= 2.0)
            {
                firstWordSize = meta.Encoding == UTF16 ? (firstWordSize + 1) * 2 : firstWordSize + 1;
            }
            else if (meta.Encoding == UTF16)
            {
                firstWordSize *= 2;
            }

            var firstWordBuffer = keyInfoBuff.Skip(indexOffset).Take(indexOffset + (int)firstWordSize).ToArray();
            indexOffset += (int)firstWordSize;

            var lastWordSize = Common.B2N(keyInfoBuff.Skip(indexOffset).Take(meta.NumWidth / 4).ToArray());
            indexOffset += meta.NumWidth / 4;
            if (meta.Version >= 2.0)
            {
                lastWordSize = meta.Encoding == "UTF-16" ? (lastWordSize + 1) * 2 : lastWordSize + 1;
            }
            else if (meta.Encoding == "UTF-16")
            {
                lastWordSize *= 2;
            }

            var lastWordBuffer = keyInfoBuff.Skip(indexOffset).Take(indexOffset + (int)lastWordSize).ToArray();
            indexOffset += (int)lastWordSize;

            var packSize = Common.B2N(keyInfoBuff.Skip(indexOffset).Take(meta.NumWidth).ToArray());
            indexOffset += meta.NumWidth;

            var unpackSize = Common.B2N(keyInfoBuff.Skip(indexOffset).Take(meta.NumWidth).ToArray());
            indexOffset += meta.NumWidth;

            var firstKey = meta.Decoder.Decode(firstWordBuffer);
            var lastKey = meta.Decoder.Decode(lastWordBuffer);

            keyBlockInfoList.Add(new KeyInfoItem
            {
                FirstKey = firstKey,
                LastKey = lastKey,
                KeyBlockPackSize = packSize,
                KeyBlockPackAccumulator = kbPackSizeAccu,
                KeyBlockUnpackSize = unpackSize,
                KeyBlockUnpackAccumulator = kbUnpackSizeAccu,
                KeyBlockEntriesNum = blockWordCount,
                KeyBlockEntriesNumAccumulator = entriesCount,
                KeyBlockInfoIndex = kbCount
            });

            kbCount++; // key block number
            entriesCount += blockWordCount;
            kbPackSizeAccu += packSize;
            kbUnpackSizeAccu += unpackSize;
        }

        // assert(
        //   countEntriesNum === numEntries,
        //   `the number_entries ${numEntries} should equal the count_num_entries ${countEntriesNum}`
        // );
        if (kbPackSizeAccu != keyHeader.KeywordBlockPackedSize)
        {
            throw new InvalidOperationException($"key_block_info keyBlockPackSize {keyHeader.KeywordBlockPackedSize} should equal to keyBlockInfo buffer length {kbPackSizeAccu}");
        }

        return keyBlockInfoList;
    }

    /// <summary>
    /// step 4.1. decode key block
    /// find the key block by the phrase
    /// </summary>
    /// <param name="kbPackedBuff"></param>
    /// <param name="unpackSize"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    protected static byte[] UnpackKeyBlock(byte[] kbPackedBuff, int unpackSize)
    {
        string compType = BitConverter.ToString(kbPackedBuff, 0, 4).Replace("-", "").ToLowerInvariant();

        if (compType == "00000000")
        {
            return kbPackedBuff[8..];
        }
        else if (compType == "01000000")
        {
            return Lzo1xWrapper.Decompress(kbPackedBuff[8..], unpackSize, 0);
        }
        else if (compType == "02000000")
        {
            return Zlib.Decompress(kbPackedBuff[8..]);
        }
        else
        {
            throw new Exception($"Cannot determine the compress type: {compType}");
        }
    }

    /// <summary>
    /// STEP 4. decode key block
    /// decode key block return the total keys list,
    /// Note: this method runs very slow, please do not use this unless special target
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    private void ReadKeyBlocks()
    {
        _keyBlockStartOffset = _keyBlockInfoEndOffset;

        var keyBlockList = new List<KeyWordItem>();
        long kbStartOffset = _keyBlockStartOffset;

        for (int idx = 0; idx < keyInfoList.Count; idx++)
        {
            long packSize = keyInfoList[idx].KeyBlockPackSize;
            long unpackSize = keyInfoList[idx].KeyBlockUnpackSize;

            long start = kbStartOffset;

            if (start != keyInfoList[idx].KeyBlockPackAccumulator + _keyBlockStartOffset)
            {
                throw new InvalidOperationException($"key block start offset {start} should equal to keyBlockPackAccumulator {keyInfoList[idx].KeyBlockPackAccumulator} + _keyBlockStartOffset {_keyBlockStartOffset}");
            }

            byte[] kbCompBuff = scanner.ReadBuffer(start, (int)packSize);
            byte[] keyBlock = UnpackKeyBlock(kbCompBuff, (int)unpackSize);

            var splitKeyBlock = SplitKeyBlock(keyBlock, idx);

            if (keyBlockList.Count > 0 && keyBlockList[^1].RecordEndOffset == -1)
            {
                keyBlockList[^1].RecordEndOffset = splitKeyBlock[0].RecordStartOffset;
            }

            keyBlockList.AddRange(splitKeyBlock);
            kbStartOffset += packSize;
        }

        if (keyBlockList[^1].RecordEndOffset == -1)
        {
            keyBlockList[^1].RecordEndOffset = -1;
        }

        if (keyBlockList.Count != keyHeader.KeywordNum)
        {
            throw new InvalidOperationException($"key block list length {keyBlockList.Count} should equal to key entries num {keyHeader.KeywordNum}");
        }

        _keyBlockEndOffset = _keyBlockStartOffset + keyHeader.KeywordBlockPackedSize;
        keywordList = keyBlockList;
    }

    /// <summary>
    /// STEP 5. decode record header,
    /// includes:
    /// [0:8 / 4]    - record block number
    /// [8:16 / 4:8] - num entries the key-value entries number
    /// [16:24 / 8:12] - record block info size
    /// [24:32 / 12:16] - record block size
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    private void ReadRecordHeader()
    {
        _recordHeaderStartOffset = _keyBlockInfoEndOffset + keyHeader.KeywordBlockPackedSize;

        int recordHeaderLen = meta.Version >= 2.0 ? 32 : 16;
        _recordHeaderEndOffset = _recordHeaderStartOffset + recordHeaderLen;

        byte[] recordHeaderBuffer = scanner.ReadBuffer(_recordHeaderStartOffset, recordHeaderLen);

        int offset = 0;
        var recordBlocksNum = Common.B2N(recordHeaderBuffer.Skip(offset).Take(meta.NumWidth).ToArray());
        offset += meta.NumWidth;

        var entriesNum = Common.B2N(recordHeaderBuffer.Skip(offset).Take(meta.NumWidth).ToArray());
        
        if (entriesNum != keyHeader.KeywordNum)
        {
            throw new InvalidOperationException($"record header entries num {entriesNum} should equal to key header keyword num {keyHeader.KeywordNum}");
        }

        offset += meta.NumWidth;

        var recordInfoCompSize = Common.B2N(recordHeaderBuffer.Skip(offset).Take(meta.NumWidth).ToArray());
        offset += meta.NumWidth;

        var recordBlockCompSize = Common.B2N(recordHeaderBuffer.Skip(offset).Take(meta.NumWidth).ToArray());

        recordHeader = new RecordHeader
        {
            RecordBlocksNum = recordBlocksNum,
            EntriesNum = entriesNum,
            RecordInfoCompSize = recordInfoCompSize,
            RecordBlockCompSize = recordBlockCompSize
        };
    }

    private void ReadRecordInfos()
    {
        _recordInfoStartOffset = _recordHeaderEndOffset;
        byte[] recordInfoBuff = scanner.ReadBuffer(_recordInfoStartOffset, (int)recordHeader.RecordInfoCompSize);

        var recordInfoList = new List<RecordInfo>();
        int offset = 0;
        long compressedAdder = 0, decompressionAdder = 0;

        for (int i = 0; i < recordHeader.RecordBlocksNum; i++)
        {
            var packSize = Common.B2N(recordInfoBuff.Skip(offset).Take(meta.NumWidth).ToArray());
            offset += meta.NumWidth;

            var unpackSize = Common.B2N(recordInfoBuff.Skip(offset).Take(meta.NumWidth).ToArray());
            offset += meta.NumWidth;

            recordInfoList.Add(new RecordInfo
            {
                PackSize = packSize,
                PackAccumulateOffset = compressedAdder,
                UnpackSize = unpackSize,
                UnpackAccumulatorOffset = decompressionAdder
            });

            compressedAdder += packSize;
            decompressionAdder += unpackSize;
        }

        if (offset != recordHeader.RecordInfoCompSize)
        {
            throw new InvalidOperationException($"record info size {offset} should equal to record header recordInfoCompSize {recordHeader.RecordInfoCompSize}");
        }

        if (compressedAdder != recordHeader.RecordBlockCompSize)
        {
            throw new InvalidOperationException($"record block size {compressedAdder} should equal to record header recordBlockCompSize {recordHeader.RecordBlockCompSize}");
        }

        this.recordInfoList = recordInfoList;

        if (keywordList.Count > 0)
        {
            var last = recordInfoList[^1];
            keywordList[^1].RecordEndOffset = last.UnpackAccumulatorOffset + last.UnpackSize;
        }

        _recordInfoEndOffset = _recordInfoStartOffset + recordHeader.RecordInfoCompSize;
        _recordBlockStartOffset = _recordInfoEndOffset;
    }

    /// <summary>
    /// STEP 7. read all records block,
    /// this is a slow method, do not use!
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    private void ReadRecordBlocks()
    {
        _recordBlockStartOffset = _recordInfoEndOffset;
        var keyData = new List<Dictionary<string, object>>();

        long sizeCounter = 0;
        long itemCounter = 0;
        long recordOffset = _recordBlockStartOffset;
        long offset = 0;

        for (int idx = 0; idx < recordInfoList.Count; idx++)
        {
            string compressType = "none";
            var packSize = recordInfoList[idx].PackSize;
            var unpackSize = recordInfoList[idx].UnpackSize;

            byte[] rbPackBuff = scanner.ReadBuffer(recordOffset, (int)packSize);
            recordOffset += packSize;

            byte[] rbCompType = rbPackBuff.Take(4).ToArray();
            //byte[] rbAdler32 = rbPackBuff.Skip(4).Take(4).ToArray(); // Ignored
            byte[] recordBlock;

            if (BitConverter.ToString(rbCompType).Replace("-", "").ToLowerInvariant() == "00000000")
            {
                recordBlock = rbPackBuff.Skip(8).ToArray(); // Uncompressed
            }
            else
            {
                byte[] blockBufDecrypted;

                if (meta.Encrypt == 1 /* || (meta.Ext == "mdd" && meta.Encrypt == 2) */)
                {
                    blockBufDecrypted = Common.MdxDecrypt(rbPackBuff); // custom decryption
                }
                else
                {
                    blockBufDecrypted = rbPackBuff.Skip(8).ToArray();
                }

                if (BitConverter.ToString(rbCompType).Replace("-", "").ToLowerInvariant() == "01000000")
                {
                    compressType = "lzo";
                    recordBlock = Lzo1xWrapper.Decompress(blockBufDecrypted, (int)unpackSize, 0);
                }
                else if (BitConverter.ToString(rbCompType).Replace("-", "").ToLowerInvariant() == "02000000")
                {
                    compressType = "zlib";
                    recordBlock = Zlib.Decompress(blockBufDecrypted);
                }
                else
                {
                    throw new InvalidOperationException("Unknown compression type.");
                }
            }

            if (recordBlock.Length != unpackSize)
            {
                throw new InvalidOperationException($"Block size mismatch at index {idx}");
            }

            int i = 0;
            while (i < keywordList.Count)
            {
                var recordStart = keywordList[i].RecordStartOffset;
                string keyText = keywordList[i].KeyText;

                if (recordStart - offset >= recordBlock.Length)
                    break;

                var recordEnd = i < keywordList.Count - 1
                    ? keywordList[i + 1].RecordStartOffset
                    : recordBlock.Length + offset;

                var keyItem = new Dictionary<string, object>
                {
                    ["key"] = keyText,
                    ["idx"] = itemCounter,
                    ["encoding"] = meta.Encoding,
                    ["record_idx"] = idx,
                    ["record_comp_start"] = recordOffset,
                    ["record_compressed_size"] = packSize,
                    ["record_decompressed_size"] = unpackSize,
                    ["record_comp_type"] = compressType,
                    ["record_encrypted"] = meta.Encrypt == 1,
                    ["relative_record_start"] = recordStart - offset,
                    ["relative_record_end"] = recordEnd - offset
                };

                keyData.Add(keyItem);

                itemCounter++;
                i++;
            }

            offset += recordBlock.Length;
            sizeCounter += packSize;
        }

        if (sizeCounter != recordHeader.RecordBlockCompSize)
        {
            throw new InvalidOperationException("Total size mismatch");
        }

        recordBlockDataList = keyData;
        _recordBlockEndOffset = _recordBlockStartOffset + sizeCounter;
    }
}
