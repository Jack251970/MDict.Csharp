using MDict.Csharp.Utils;
using Common = MDict.Csharp.Utils.Utils;

namespace MDict.Csharp.Models;

/// <summary>
/// MDict file model for MDict.
/// </summary>
/// <remarks>
/// After a series of tests, it was found that mdx format files have significant issues with word sorting.
/// 1. Case sensitivity issues, such as the coexistence of a-zA-Z and aA-zZ.
/// 2. Multilingual issues, where English and Chinese characters are compared, generally English should precede Chinese.
/// 3. Minor language issues.
/// These issues can occur, and cannot be resolved through the settings in the dictionary header.
/// Therefore, it is not possible to quickly index using the internal keyInfoList.
/// Under modern computer performance conditions, directly traversing all entries can yield good results.
/// So the current strategy is to read all entries and sort them internally.
/// </remarks>
public class Dict : BaseDict
{
    /// <summary>
    /// Constructor for MDict.
    /// </summary>
    /// <param name="fname"></param>
    /// <param name="options"></param>
    protected Dict(string fname, MDictOptions? options = null)
        : base(fname, options?.Passcode ?? "", options ?? new MDictOptions())
    {
        // default options
        options ??= new MDictOptions();
        options.Passcode ??= "";
        options.Debug ??= false;
        options.Resort ??= true;
        options.IsStripKey ??= true;
        options.IsCaseSensitive ??= true;
        options.EncryptType ??= -1;
    }

    /**
    * lookupKeyInfoItem lookup the `keyInfoItem`
    * the `keyInfoItem` contains key-word record block location: recordStartOffset
    * the `recordStartOffset` should indicate the unpacked record data relative offset
    * @param word the target word phrase
    */
    public KeyWordItem? LookupKeyBlockByWord(string word, bool isAssociate = false)
    {
        // const keyBlockInfoId = this.lookupKeyInfoByWord(word);
        // if (keyBlockInfoId < 0) {
        //   return undefined;
        // }

        // TODO: if the this.list length parse too slow, can decode by below code
        // const list = this.lookupPartialKeyBlockListByKeyInfoId(keyInfoId);
        var list = keywordList;
        // binary search
        var left = 0;
        var right = list.Count - 1;
        var mid = 0;

        while (left <= right)
        {
            mid = left + (right - left >> 1);
            int compRes = Comp(word, list[mid].KeyText);
            if (compRes > 0)
                left = mid + 1;
            else if (compRes == 0)
                break;
            else
                right = mid - 1;
        }

        if (Comp(word, list[mid].KeyText) != 0 && !isAssociate)
            return null;

        return list[mid];
    }

    /**
    * locate the record meaning buffer by `keyListItem`
    * the `KeyBlockItem.recordStartOffset` should indicate the record block info location
    * use the record block info, we can get the `recordBuffer`, then we need decrypt and decompress
    * use decompressed `recordBuffer` we can get the total block which contains meanings
    * then, use:
    *  const start = item.recordStartOffset - recordBlockInfo.unpackAccumulatorOffset;
    *  const end = item.recordEndOffset - recordBlockInfo.unpackAccumulatorOffset;
    *  the finally meaning's buffer is `unpackRecordBlockBuff[start, end]`
    * @param item
    */
    public byte[] LookupRecordByKeyBlock(KeyWordItem item)
    {
        var recordBlockIndex = ReduceRecordBlockInfo((int)item.RecordStartOffset);
        var recordBlockInfo = recordInfoList[recordBlockIndex];
        var recordBuffer = scanner.ReadBuffer(
            _recordBlockStartOffset + recordBlockInfo.PackAccumulateOffset,
            (int)recordBlockInfo.PackSize
        );
        var unpackedRecordBlockBuff = DecompressBuff(recordBuffer, (int)recordBlockInfo.UnpackSize);

        var start = item.RecordStartOffset - recordBlockInfo.UnpackAccumulatorOffset;
        var end = item.RecordEndOffset - recordBlockInfo.UnpackAccumulatorOffset;

        return unpackedRecordBlockBuff.Skip((int)start).Take((int)(end - start)).ToArray();
    }

    /**
    * lookupPartialKeyInfoListById
    * decode key block by key block id, and we can get the partial key list
    * the key list just contains the partial key list
    * @param {number} keyInfoId key block id
    * @return {KeyWordItem[]}
    */
    public List<KeyWordItem> LookupPartialKeyBlockListByKeyInfoId(int keyInfoId)
    {
        var info = keyInfoList[keyInfoId];
        var startOffset = info.KeyBlockPackAccumulator + _keyBlockStartOffset;
        var keyBlockPackedBuff = scanner.ReadBuffer(startOffset, (int)info.KeyBlockPackSize);
        var keyBlock = UnpackKeyBlock(keyBlockPackedBuff, (int)info.KeyBlockUnpackSize);
        return SplitKeyBlock(keyBlock, keyInfoId);
    }

    /**
    * lookupInfoBlock reduce word find the nearest key block
    * @param {string} word searching phrase
    * @param keyInfoList
    */
    public int LookupKeyInfoByWord(string word, List<KeyInfoItem>? keyInfoList = null)
    {
        var list = keyInfoList ?? this.keyInfoList;
        int left = 0, right = list.Count - 1, mid;

        // when compare the word, the uppercase words are less than lowercase words
        // so we compare with the greater symbol is wrong, we need to use the `common.wordCompare` function
        while (left <= right)
        {
            mid = left + (right - left >> 1);
            if (Comp(word, list[mid].FirstKey) >= 0 &&
                Comp(word, list[mid].LastKey) <= 0)
                return mid;

            if (Comp(word, list[mid].LastKey) >= 0)
                left = mid + 1;
            else
                right = mid - 1;
        }

        return -1;
    }

    private static int Comp(string word1, string word2)
    {
        return string.Compare(word1, word2, StringComparison.Ordinal);
    }

    private byte[] DecompressBuff(byte[] recordBuffer, int unpackSize)
    {
        // decompress
        // 4 bytes: compression type
        var compType = BitConverter.ToString(recordBuffer.Take(4).ToArray()).Replace("-", "").ToLower();
        // record_block stores the final record data
        var unpackRecordBlockBuff = new byte[recordBuffer.Length];

        // TODO: igore adler32 offset
        // Note: here ignore the checksum part
        // bytes: adler32 checksum of decompressed record block
        // adler32 = unpack('>I', record_block_compressed[4:8])[0]
        if (compType == "00000000")
        {
            unpackRecordBlockBuff = recordBuffer.Skip(8).ToArray();
        }
        else
        {
            // decrypt
            byte[]? blockBufDecrypted = null;

            // if encrypt type == 1, the record block was encrypted
            if (meta.Encrypt == 1 /* || (this.meta.ext == "mdd" && this.meta.encrypt === 2 ) */)
            {
                // const passkey = new Uint8Array(8);
                // record_block_compressed.copy(passkey, 0, 4, 8);
                // passkey.set([0x95, 0x36, 0x00, 0x00], 4); // key part 2: fixed data
                blockBufDecrypted = Common.MdxDecrypt(recordBuffer);
            }
            else
            {
                blockBufDecrypted = recordBuffer.Skip(8).ToArray();
            }

            // decompress
            if (compType == "01000000")
            {
                unpackRecordBlockBuff = Lzo1xWrapper.Decompress(blockBufDecrypted, unpackSize, 1308672);
                // TODO
                /*unpackRecordBlockBuff = Buffer.from(unpackRecordBlockBuff).subarray(
                    unpackRecordBlockBuff.byteOffset,
                    unpackRecordBlockBuff.byteOffset + unpackRecordBlockBuff.byteLength
                );*/
            }
            else if (compType == "02000000")
            {
                // zlib decompress
                unpackRecordBlockBuff = Zlib.Decompress(blockBufDecrypted);
            }
            else
            {
                throw new InvalidOperationException("Unknown compression type");
            }
        }

        return unpackRecordBlockBuff;
    }

    /**
    * find record which record start locate
    * @param {number} recordStart record start offset
    */
    private int ReduceRecordBlockInfo(int recordStart)
    {
        var left = 0;
        var right = recordInfoList.Count - 1;
        var mid = 0;
        while (left <= right)
        {
            mid = left + (right - left >> 1);
            if (recordStart >= recordInfoList[mid].UnpackAccumulatorOffset)
                left = mid + 1;
            else
                right = mid - 1;
        }
        return left - 1;
    }

    /// <summary>
    /// Close the MDict instance and release resources.
    /// </summary>
    public void Close()
    {
        scanner.Close();
        keywordList.Clear();
        keyInfoList.Clear();
        recordInfoList.Clear();
    }
}
