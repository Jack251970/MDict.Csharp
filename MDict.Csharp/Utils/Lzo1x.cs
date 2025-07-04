/*
 * minilzo-js
 * JavaScript port of minilzo by Alistair Braidwood
 *
 *
 * This library is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public License as
 *  published by the Free Software Foundation; either version 2 of
 *  the License, or (at your option) any later version.
 *
 * You should have received a copy of the GNU General Public License
 *  along with the minilzo-js library; see the file COPYING.
 *  If not, write to the Free Software Foundation, Inc.,
 *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301, USA.
 */

/*
 * original minilzo.c by:
 *
 * Markus F.X.J. Oberhumer
 * <markus@oberhumer.com>
 * http://www.oberhumer.com/opensource/lzo/
 */

/*
 * NOTE:
 *   the full LZO package can be found at
 *   http://www.oberhumer.com/opensource/lzo/
 */

namespace MDict.Csharp.Utils;

internal class Lzo1x
{
    public const int BlockSize = 4096;

    public const int OK = 0;
    public const int INPUT_OVERRUN = -4;
    public const int OUTPUT_OVERRUN = -5;
    public const int LOOKBEHIND_OVERRUN = -6;
    public const int EOF_FOUND = -999;

    private byte[] buf = Array.Empty<byte>();
    private uint[] buf32 = Array.Empty<uint>();

    private byte[] outBuf = Array.Empty<byte>();
    private uint[] out32 = Array.Empty<uint>();
    private int cbl;
    private int ip_end;
    private int op_end;
    private int t;

    private int ip;
    private int op;
    private int m_pos;

    private bool skipToFirstLiteralFun;

    private State state = new();

    public class State
    {
        public byte[] inputBuffer = Array.Empty<byte>();
        public byte[] outputBuffer = Array.Empty<byte>();
    }

    // Count trailing zeros in an integer (ctzl)
    public int Ctzl(uint v)
    {
        int c;
        if ((v & 0x1) != 0)
        {
            c = 0;
        }
        else
        {
            c = 1;
            if ((v & 0xFFFF) == 0) { v >>= 16; c += 16; }
            if ((v & 0xFF) == 0) { v >>= 8; c += 8; }
            if ((v & 0xF) == 0) { v >>= 4; c += 4; }
            if ((v & 0x3) == 0) { v >>= 2; c += 2; }
            c -= (int)(v & 0x1);
        }
        return c;
    }

    private void ExtendBuffer()
    {
        byte[] newBuffer = new byte[cbl + BlockSize];
        Buffer.BlockCopy(outBuf, 0, newBuffer, 0, op);
        outBuf = newBuffer;
        out32 = new uint[outBuf.Length / 4];
        Buffer.BlockCopy(outBuf, 0, out32, 0, outBuf.Length);
        state.outputBuffer = outBuf;
        cbl = outBuf.Length;
    }

    private int EofFound()
    {
        return ip == ip_end ? 0 : ip < ip_end ? -8 : -4;
    }

    private void MatchNext()
    {
        while (op + 3 > cbl)
            ExtendBuffer();

        outBuf[op++] = buf[ip++];
        if (t > 1)
        {
            outBuf[op++] = buf[ip++];
            if (t > 2)
                outBuf[op++] = buf[ip++];
        }

        t = buf[ip++];
    }

    private int MatchDone()
    {
        t = buf[ip - 2] & 3;
        return t;
    }

    private void CopyMatch()
    {
        t += 2;
        while (op + t > cbl)
            ExtendBuffer();

        if (t > 4 && op % 4 == m_pos % 4)
        {
            while (op % 4 > 0)
            {
                outBuf[op++] = outBuf[m_pos++];
                t--;
            }

            while (t > 4)
            {
                out32[op / 4] = out32[m_pos / 4];
                op += 4;
                m_pos += 4;
                t -= 4;
            }
        }

        do
        {
            outBuf[op++] = outBuf[m_pos++];
        } while (--t > 0);
    }

    private void CopyFromBuf()
    {
        while (op + t > cbl)
            ExtendBuffer();

        if (t > 4 && op % 4 == ip % 4)
        {
            while (op % 4 > 0)
            {
                outBuf[op++] = buf[ip++];
                t--;
            }

            while (t > 4)
            {
                out32[op / 4] = BitConverter.ToUInt32(buf, ip);
                op += 4;
                ip += 4;
                t -= 4;
            }
        }

        do
        {
            outBuf[op++] = buf[ip++];
        } while (--t > 0);
    }

    private int Match()
    {
        while (true)
        {
            if (t >= 64)
            {
                m_pos = op - 1;
                m_pos -= t >> 2 & 7;
                m_pos -= buf[ip++] << 3;
                t = (t >> 5) - 1;

                CopyMatch();
                if (MatchDone() == 0) break;
                MatchNext();
                continue;
            }
            else if (t >= 32)
            {
                t &= 31;
                if (t == 0)
                {
                    while (buf[ip] == 0)
                    {
                        t += 255;
                        ip++;
                    }
                    t += 31 + buf[ip++];
                }
                m_pos = op - 1;
                m_pos -= (buf[ip] >> 2) + (buf[ip + 1] << 6);
                ip += 2;
            }
            else if (t >= 16)
            {
                m_pos = op;
                m_pos -= (t & 8) << 11;
                t &= 7;
                if (t == 0)
                {
                    while (buf[ip] == 0)
                    {
                        t += 255;
                        ip++;
                    }
                    t += 7 + buf[ip++];
                }

                m_pos -= (buf[ip] >> 2) + (buf[ip + 1] << 6);
                ip += 2;

                if (m_pos == op)
                {
                    state.outputBuffer = new byte[op];
                    Array.Copy(outBuf, 0, state.outputBuffer, 0, op);
                    return EOF_FOUND;
                }

                m_pos -= 0x4000;
            }
            else
            {
                m_pos = op - 1;
                m_pos -= t >> 2;
                m_pos -= buf[ip++] << 2;

                while (op + 2 > cbl)
                    ExtendBuffer();

                outBuf[op++] = outBuf[m_pos++];
                outBuf[op++] = outBuf[m_pos];

                if (MatchDone() == 0) break;
                MatchNext();
                continue;
            }

            CopyMatch();
            if (MatchDone() == 0) break;

            MatchNext();
        }

        return OK;
    }

    public int Decompress(State inputState)
    {
        state = inputState;
        buf = state.inputBuffer;
        int paddedLength = buf.Length + (4 - buf.Length % 4) % 4;
        byte[] paddedBuf = new byte[paddedLength];
        Array.Copy(buf, paddedBuf, buf.Length);
        buf32 = new uint[paddedBuf.Length / 4];
        Buffer.BlockCopy(paddedBuf, 0, buf32, 0, paddedBuf.Length);

        int outLen = buf.Length + (BlockSize - buf.Length % BlockSize) % BlockSize;
        outBuf = new byte[outLen];
        out32 = new uint[outBuf.Length / 4];
        cbl = outBuf.Length;
        state.outputBuffer = outBuf;
        ip_end = buf.Length;
        op_end = outBuf.Length;
        t = 0;

        ip = 0;
        op = 0;
        m_pos = 0;

        skipToFirstLiteralFun = false;

        if (buf[ip] > 17)
        {
            t = buf[ip++] - 17;
            if (t < 4)
            {
                MatchNext();
                int ret = Match();
                if (ret != OK)
                    return ret == EOF_FOUND ? OK : ret;
            }
            else
            {
                CopyFromBuf();
                skipToFirstLiteralFun = true;
            }
        }

        while (true)
        {
            if (!skipToFirstLiteralFun)
            {
                t = buf[ip++];
                if (t >= 16)
                {
                    int ret = Match();
                    if (ret != OK)
                        return ret == EOF_FOUND ? OK : ret;
                    continue;
                }

                if (t == 0)
                {
                    while (buf[ip] == 0)
                    {
                        t += 255;
                        ip++;
                    }
                    t += 15 + buf[ip++];
                }

                t += 3;
                CopyFromBuf();
            }
            else
            {
                skipToFirstLiteralFun = false;
            }

            t = buf[ip++];
            if (t < 16)
            {
                m_pos = op - (1 + 0x0800);
                m_pos -= t >> 2;
                m_pos -= buf[ip++] << 2;

                while (op + 3 > cbl)
                    ExtendBuffer();

                outBuf[op++] = outBuf[m_pos++];
                outBuf[op++] = outBuf[m_pos++];
                outBuf[op++] = outBuf[m_pos];

                if (MatchDone() == 0)
                    continue;

                MatchNext();
            }

            int result = Match();
            if (result != OK)
                return result == EOF_FOUND ? OK : result;
        }
    }

    private int _compressCore(uint[] dict, int in_len, int ti)
    {
        int ip_start = ip;
        int ip_end = ip + in_len - 20;
        int ii = ip;

        ip += ti < 4 ? 4 - ti : 0;

        int m_pos = 0;
        int m_off = 0;
        int m_len = 0;
        int dv_hi = 0;
        int dv_lo = 0;
        int dindex = 0;

        ip += 1 + (ip - ii >> 5);

        while (true)
        {
            if (ip >= ip_end)
            {
                break;
            }

            // The following code doesn't work in JavaScript due to a lack of 64-bit bitwise operations
            // Instead, use (optimized two's complement integer arithmetic)
            // Optimization is based on us only needing the high 16 bits of the lower 32-bit integer.
            dv_lo = buf[ip] | buf[ip + 1] << 8;
            dv_hi = buf[ip + 2] | buf[ip + 3] << 8;
            dindex = ((dv_lo * 0x429d >> 16) + dv_hi * 0x429d + dv_lo * 0x1824 & 0xffff) >> 2;

            m_pos = ip_start + (int)dict[dindex];
            dict[dindex] = (uint)(ip - ip_start);

            if ((dv_hi << 16) + dv_lo !=
                (buf[m_pos] | buf[m_pos + 1] << 8 | buf[m_pos + 2] << 16 | buf[m_pos + 3] << 24))
            {
                ip += 1 + (ip - ii >> 5);
                continue;
            }

            ii -= ti;
            ti = 0;
            int t = ip - ii;

            if (t != 0)
            {
                if (t <= 3)
                {
                    outBuf[op - 2] |= (byte)t;
                    while (--t >= 0)
                    {
                        outBuf[op++] = buf[ii++];
                    }
                }
                else
                {
                    if (t <= 18)
                    {
                        outBuf[op++] = (byte)(t - 3);
                    }
                    else
                    {
                        int tt = t - 18;
                        outBuf[op++] = 0;
                        while (tt > 255)
                        {
                            tt -= 255;
                            outBuf[op++] = 0;
                        }
                        outBuf[op++] = (byte)tt;
                    }

                    while (--t >= 0)
                    {
                        outBuf[op++] = buf[ii++];
                    }
                }
            }

            m_len = 4;

            if (buf[ip + m_len] == buf[m_pos + m_len])
            {
                do
                {
                    m_len++;
                    if (buf[ip + m_len] != buf[m_pos + m_len]) break;
                    m_len++;
                    if (buf[ip + m_len] != buf[m_pos + m_len]) break;
                    m_len++;
                    if (buf[ip + m_len] != buf[m_pos + m_len]) break;
                    m_len++;
                    if (buf[ip + m_len] != buf[m_pos + m_len]) break;
                    m_len++;
                    if (buf[ip + m_len] != buf[m_pos + m_len]) break;
                    m_len++;
                    if (buf[ip + m_len] != buf[m_pos + m_len]) break;
                    m_len++;
                    if (buf[ip + m_len] != buf[m_pos + m_len]) break;
                    m_len++;
                    if (buf[ip + m_len] != buf[m_pos + m_len]) break;
                    if (ip + m_len >= ip_end) break;
                } while (buf[ip + m_len] == buf[m_pos + m_len]);
            }

            m_off = ip - m_pos;
            ip += m_len;
            ii = ip;

            if (m_len <= 8 && m_off <= 0x0800)
            {
                m_off -= 1;
                outBuf[op++] = (byte)(m_len - 1 << 5 | (m_off & 7) << 2);
                outBuf[op++] = (byte)(m_off >> 3);
            }
            else if (m_off <= 0x4000)
            {
                m_off -= 1;
                if (m_len <= 33)
                {
                    outBuf[op++] = (byte)(32 | m_len - 2);
                }
                else
                {
                    m_len -= 33;
                    outBuf[op++] = 32;
                    while (m_len > 255)
                    {
                        m_len -= 255;
                        outBuf[op++] = 0;
                    }
                    outBuf[op++] = (byte)m_len;
                }

                outBuf[op++] = (byte)(m_off << 2);
                outBuf[op++] = (byte)(m_off >> 6);
            }
            else
            {
                m_off -= 0x4000;
                if (m_len <= 9)
                {
                    outBuf[op++] = (byte)(16 | m_off >> 11 & 8 | m_len - 2);
                }
                else
                {
                    m_len -= 9;
                    outBuf[op++] = (byte)(16 | m_off >> 11 & 8);
                    while (m_len > 255)
                    {
                        m_len -= 255;
                        outBuf[op++] = 0;
                    }
                    outBuf[op++] = (byte)m_len;
                }

                outBuf[op++] = (byte)(m_off << 2);
                outBuf[op++] = (byte)(m_off >> 6);
            }
        }

        return in_len - (ii - ip_start - ti);
    }

    public int Compress(State state)
    {
        this.state = state;
        ip = 0;
        buf = state.inputBuffer;
        int in_len = buf.Length;
        int max_len = in_len + (in_len + 15) / 16 + 64 + 3;
        state.outputBuffer = new byte[max_len];
        outBuf = state.outputBuffer;
        op = 0;
        var dict = new uint[16384];
        int l = in_len;
        int t = 0;

        while (l > 20)
        {
            int ll = l <= 49152 ? l : 49152;
            if (t + ll >> 5 <= 0) break;

            dict = new uint[16384];

            int prev_ip = ip;
            t = _compressCore(dict, ll, t);
            ip = prev_ip + ll;
            l -= ll;
        }

        t += l;

        if (t > 0)
        {
            int ii = in_len - t;

            if (op == 0 && t <= 238)
            {
                outBuf[op++] = (byte)(17 + t);
            }
            else if (t <= 3)
            {
                outBuf[op - 2] |= (byte)t;
            }
            else if (t <= 18)
            {
                outBuf[op++] = (byte)(t - 3);
            }
            else
            {
                int tt = t - 18;
                outBuf[op++] = 0;
                while (tt > 255)
                {
                    tt -= 255;
                    outBuf[op++] = 0;
                }
                outBuf[op++] = (byte)tt;
            }

            while (--t >= 0)
            {
                outBuf[op++] = buf[ii++];
            }
        }

        outBuf[op++] = 17;
        outBuf[op++] = 0;
        outBuf[op++] = 0;

        state.outputBuffer = outBuf.Take(op).ToArray();
        return OK;
    }
}
