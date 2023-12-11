// Perfect Culling (C) 2021 Patrick König
//

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Koenigz.PerfectCulling.IO
{
    /// <summary>
    /// Allows to write individual bits in an intuitive way.
    /// Used for efficient data storage.
    /// </summary>
    public class BitStreamWriter
    {
        public byte[] Buffer => m_buf;
        public int Length => m_currentBufPos;
        
        const uint SingleBitMask = 0x00000001;

        private byte[] m_buf = new byte[65536];

        private byte m_currentByte;
        
        private int m_currenBitPos = 0;
        private int m_currentBufPos = 0;
        
        public void Reset()
        {
            m_currentByte = 0;
            
            m_currenBitPos = 0;
            m_currentBufPos = 0;
        }

        public int Write(uint value, int bits)
        {
#if UNITY_EDITOR
            if (bits > 32 || bits < 0)
            {
                throw new SystemException($"Invalid number of bits: {bits}");
            }
#endif
            
            int bitsWritten = 0;

            for (int i = 0; i < bits; ++i)
            {
                uint singleBit = (value >> i) & SingleBitMask;

                WriteBit(singleBit);

                ++bitsWritten;
            }

            return bitsWritten;
        }

        public void Flush()
        {
            if (m_currenBitPos != 0)
            {
                AppendCurrentByte();
            }
                
            m_currentByte = 0;
            m_currenBitPos = 0;
        }

        private void WriteBit(uint singleBit)
        {
            if (m_currenBitPos == 8)
            {
                AppendCurrentByte();
                
                m_currentByte = 0;
                m_currenBitPos = 0;
            }

            if (singleBit != 0)
            {
                m_currentByte |= (byte) (singleBit << m_currenBitPos);
            }

            ++m_currenBitPos;
        }

        private void AppendCurrentByte()
        {
            m_buf[m_currentBufPos] = m_currentByte;
            
            ++m_currentBufPos;
        }
    }

    /// <summary>
    /// Allows to read individual bits in an intuitive way.
    /// Used for efficient data storage.
    /// </summary>
    public class BitStreamReader
    {
        private const int NumberOfBitsInByte = 8;
        
        private byte[] m_buffer;
        
        private int m_currentBits;
        private int m_bufPos;
        
        private static readonly byte[] BitMask = new byte[]
        {
            0,
            
            0b0000_0001,
            0b0000_0011,
            0b0000_0111,
            0b0000_1111,
            0b0001_1111,
            0b0011_1111,
            0b0111_1111,
            0b1111_1111,
        };

        public uint Read(int bits)
        {
#if UNITY_EDITOR
            if (bits > 8 || bits <= 0)
            {
                throw new SystemException($"Invalid number of bits: {bits}");
            }
#endif
            
            uint value = 0;

#pragma warning disable 162
            if (PerfectCullingConstants.ReadMultipleBitsInBitStream)
            {
                if (m_currentBits == 0)
                {
                    ++m_bufPos;

                    m_currentBits = NumberOfBitsInByte;
                }

                if (m_currentBits >= bits)
                {
                    // We got more or the same amount of bits available. We can read them all in a single call
                    value = (uint)((m_buffer[m_bufPos] >> (NumberOfBitsInByte - m_currentBits)) & BitMask[bits]);

                    m_currentBits -= bits;
                }
                else
                {
                    int prevBitCount = m_currentBits;

                    // We got less than the requested amount of bits available. We first read the rest of the current byte...
                    uint firstRead = (uint)(m_buffer[m_bufPos] >> (NumberOfBitsInByte - m_currentBits));

                    bits -= m_currentBits;

                    ++m_bufPos;
                    m_currentBits = NumberOfBitsInByte - bits;

                    // ... and read the remaining bits from the following byte.
                    uint secondRead = (uint)(m_buffer[m_bufPos] & BitMask[bits]);

                    value = (secondRead << (prevBitCount)) | firstRead;
                }
            }
            else
            {
                // Unrolled loop
                switch (bits)
                {
                    case 1: value |= ReadBit() << 0;break;
                    case 2: value |= ReadBit() << 0 | ReadBit() << 1;break;
                    case 3: value |= ReadBit() << 0 | ReadBit() << 1 | ReadBit() << 2;break;
                    case 4: value |= ReadBit() << 0 | ReadBit() << 1 | ReadBit() << 2 | ReadBit() << 3;break;
                    case 5: value |= ReadBit() << 0 | ReadBit() << 1 | ReadBit() << 2 | ReadBit() << 3 | ReadBit() << 4;break;
                    case 6: value |= ReadBit() << 0 | ReadBit() << 1 | ReadBit() << 2 | ReadBit() << 3 | ReadBit() << 4 | ReadBit() << 5;break;
                    case 7: value |= ReadBit() << 0 | ReadBit() << 1 | ReadBit() << 2 | ReadBit() << 3 | ReadBit() << 4 | ReadBit() << 5 | ReadBit() << 6;break;
                    case 8: value |= ReadBit() << 0 | ReadBit() << 1 | ReadBit() << 2 | ReadBit() << 3 | ReadBit() << 4 | ReadBit() << 5 | ReadBit() << 6 | ReadBit() << 7;break; 
                }
            }
#pragma warning restore 162
            
            return value;
        }
        
        public void Reset(byte[] buffer)
        {
            m_buffer = buffer;
            
            m_bufPos = 0;
            
            m_currentBits = NumberOfBitsInByte;
        }

        private uint ReadBit()
        {
            if (m_currentBits == 0)
            {
                ++m_bufPos;
                
                m_currentBits = NumberOfBitsInByte - 1;
                
                return (uint) (m_buffer[m_bufPos] >> 0) & 1;
            }
            
            uint value = (uint) (m_buffer[m_bufPos] >> (NumberOfBitsInByte - m_currentBits)) & 1;
            
            --m_currentBits;

            return value;
        }
    }
}