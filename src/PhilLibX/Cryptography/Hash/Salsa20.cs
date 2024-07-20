/*
   Salsa.NET: A .NET implementation of Salsa20, Salsa20/12, and Salsa20/8.
   Copyright (c) 2022 Samuel Lucas

   Permission is hereby granted, free of charge, to any person obtaining a copy of
   this software and associated documentation files (the "Software"), to deal in
   the Software without restriction, including without limitation the rights to
   use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
   the Software, and to permit persons to whom the Software is furnished to do so,
   subject to the following conditions:

   The above copyright notice and this permission notice shall be included in all
   copies or substantial portions of the Software.

   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
   IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
   FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
   AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
   LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
   OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
   SOFTWARE.
*/

using System;
using System.Security.Cryptography;

namespace PhilLibX.Cryptography.Hash
{
    internal static class Salsa
    {
        internal const int KeySize = 32;
        internal const int NonceSize = 8;
        internal const int BlockSize = 64;

        internal static unsafe ulong Encrypt(Span<byte> ciphertext, ReadOnlySpan<byte> plaintext, ReadOnlySpan<byte> nonce, ReadOnlySpan<byte> key, ulong counter = 0, int rounds = 20)
        {
            if (ciphertext.Length != plaintext.Length) { throw new ArgumentOutOfRangeException(nameof(ciphertext), ciphertext.Length, $"{nameof(ciphertext)} must be {plaintext.Length} bytes long."); }
            if (nonce.Length != NonceSize) { throw new ArgumentOutOfRangeException(nameof(nonce), nonce.Length, $"{nameof(nonce)} must be {NonceSize} bytes long."); }
            if (key.Length != KeySize) { throw new ArgumentOutOfRangeException(nameof(key), key.Length, $"{nameof(key)} must be {KeySize} bytes long."); }
            if (rounds != 20 && rounds != 12 && rounds != 8) { throw new ArgumentOutOfRangeException(nameof(rounds), rounds, $"{nameof(rounds)} must be 20, 12, or 8."); }

            int bytesRemaining = plaintext.Length;
            Span<byte> finalBlock = stackalloc byte[BlockSize];
            const uint j0 = 0x61707865;
            const uint j5 = 0x3320646e;
            const uint j10 = 0x79622d32;
            const uint j15 = 0x6b206574;
            uint j1 = ReadUInt32LittleEndian(key, offset: 0);
            uint j2 = ReadUInt32LittleEndian(key, offset: 4);
            uint j3 = ReadUInt32LittleEndian(key, offset: 8);
            uint j4 = ReadUInt32LittleEndian(key, offset: 12);
            uint j11 = ReadUInt32LittleEndian(key, offset: 16);
            uint j12 = ReadUInt32LittleEndian(key, offset: 20);
            uint j13 = ReadUInt32LittleEndian(key, offset: 24);
            uint j14 = ReadUInt32LittleEndian(key, offset: 28);
            uint j6 = ReadUInt32LittleEndian(nonce, offset: 0);
            uint j7 = ReadUInt32LittleEndian(nonce, offset: 4);
            uint j8 = (uint)(counter & 0xFFFFFFFF);
            uint j9 = (uint)((counter >> 32) & 0xFFFFFFFF);

            fixed (byte* cPtr = ciphertext, pPtr = plaintext, fPtr = finalBlock)
            {
                byte* c = cPtr, p = pPtr, cFinal = cPtr;
                while (true)
                {
                    if (bytesRemaining < 64)
                    {
                        for (int i = 0; i < bytesRemaining; i++)
                        {
                            finalBlock[i] = p[i];
                        }
                        p = fPtr;
                        cFinal = c;
                        c = fPtr;
                    }

                    uint x0 = j0, x1 = j1, x2 = j2, x3 = j3, x4 = j4, x5 = j5, x6 = j6, x7 = j7, x8 = j8, x9 = j9, x10 = j10, x11 = j11, x12 = j12, x13 = j13, x14 = j14, x15 = j15;

                    for (int i = 0; i < rounds / 2; i++)
                    {
                        QuarterRound(ref x0, ref x4, ref x8, ref x12);
                        QuarterRound(ref x5, ref x9, ref x13, ref x1);
                        QuarterRound(ref x10, ref x14, ref x2, ref x6);
                        QuarterRound(ref x15, ref x3, ref x7, ref x11);
                        QuarterRound(ref x0, ref x1, ref x2, ref x3);
                        QuarterRound(ref x5, ref x6, ref x7, ref x4);
                        QuarterRound(ref x10, ref x11, ref x8, ref x9);
                        QuarterRound(ref x15, ref x12, ref x13, ref x14);
                    }

                    x0 += j0;
                    x1 += j1;
                    x2 += j2;
                    x3 += j3;
                    x4 += j4;
                    x5 += j5;
                    x6 += j6;
                    x7 += j7;
                    x8 += j8;
                    x9 += j9;
                    x10 += j10;
                    x11 += j11;
                    x12 += j12;
                    x13 += j13;
                    x14 += j14;
                    x15 += j15;

                    x0 ^= ReadUInt32LittleEndian(p + 0);
                    x1 ^= ReadUInt32LittleEndian(p + 4);
                    x2 ^= ReadUInt32LittleEndian(p + 8);
                    x3 ^= ReadUInt32LittleEndian(p + 12);
                    x4 ^= ReadUInt32LittleEndian(p + 16);
                    x5 ^= ReadUInt32LittleEndian(p + 20);
                    x6 ^= ReadUInt32LittleEndian(p + 24);
                    x7 ^= ReadUInt32LittleEndian(p + 28);
                    x8 ^= ReadUInt32LittleEndian(p + 32);
                    x9 ^= ReadUInt32LittleEndian(p + 36);
                    x10 ^= ReadUInt32LittleEndian(p + 40);
                    x11 ^= ReadUInt32LittleEndian(p + 44);
                    x12 ^= ReadUInt32LittleEndian(p + 48);
                    x13 ^= ReadUInt32LittleEndian(p + 52);
                    x14 ^= ReadUInt32LittleEndian(p + 56);
                    x15 ^= ReadUInt32LittleEndian(p + 60);

                    j8++;
                    if (j8 == 0)
                    {
                        j9++;
                    }

                    WriteUInt32LittleEndian(c + 0, x0);
                    WriteUInt32LittleEndian(c + 4, x1);
                    WriteUInt32LittleEndian(c + 8, x2);
                    WriteUInt32LittleEndian(c + 12, x3);
                    WriteUInt32LittleEndian(c + 16, x4);
                    WriteUInt32LittleEndian(c + 20, x5);
                    WriteUInt32LittleEndian(c + 24, x6);
                    WriteUInt32LittleEndian(c + 28, x7);
                    WriteUInt32LittleEndian(c + 32, x8);
                    WriteUInt32LittleEndian(c + 36, x9);
                    WriteUInt32LittleEndian(c + 40, x10);
                    WriteUInt32LittleEndian(c + 44, x11);
                    WriteUInt32LittleEndian(c + 48, x12);
                    WriteUInt32LittleEndian(c + 52, x13);
                    WriteUInt32LittleEndian(c + 56, x14);
                    WriteUInt32LittleEndian(c + 60, x15);

                    if (bytesRemaining <= 64)
                    {
                        if (bytesRemaining < 64)
                        {
                            for (int i = 0; i < bytesRemaining; i++)
                            {
                                cFinal[i] = c[i];
                            }
                            CryptographicOperations.ZeroMemory(finalBlock);
                        }
                        return (ulong)j9 << 32 | j8;
                    }

                    p += BlockSize;
                    c += BlockSize;
                    bytesRemaining -= BlockSize;
                }
            }
        }

        private static uint ReadUInt32LittleEndian(ReadOnlySpan<byte> source, int offset)
        {
            return source[offset] | (uint)source[offset + 1] << 8 | (uint)source[offset + 2] << 16 | (uint)source[offset + 3] << 24;
        }

        private static unsafe uint ReadUInt32LittleEndian(byte* source)
        {
            return source[0] | (uint)source[1] << 8 | (uint)source[2] << 16 | (uint)source[3] << 24;
        }

        private static void QuarterRound(ref uint a, ref uint b, ref uint c, ref uint d)
        {
            b ^= RotateLeft(a + d, 7);
            c ^= RotateLeft(b + a, 9);
            d ^= RotateLeft(c + b, 13);
            a ^= RotateLeft(d + c, 18);
        }

        private static uint RotateLeft(uint a, int b)
        {
            return (a << b) | (a >> (32 - b));
        }

        private static unsafe void WriteUInt32LittleEndian(byte* destination, uint value)
        {
            destination[0] = (byte)value;
            destination[1] = (byte)(value >> 8);
            destination[2] = (byte)(value >> 16);
            destination[3] = (byte)(value >> 24);
        }
    }

    public static class Salsa20
    {
        public const int KeySize = Salsa.KeySize;
        public const int NonceSize = Salsa.NonceSize;
        public const int BlockSize = Salsa.BlockSize;

        public static ulong Encrypt(Span<byte> ciphertext, ReadOnlySpan<byte> plaintext, ReadOnlySpan<byte> nonce, ReadOnlySpan<byte> key, ulong counter = 0)
        {
            return Salsa.Encrypt(ciphertext, plaintext, nonce, key, counter, rounds: 20);
        }

        public static void Decrypt(Span<byte> plaintext, ReadOnlySpan<byte> ciphertext, ReadOnlySpan<byte> nonce, ReadOnlySpan<byte> key, ulong counter = 0)
        {
            if (plaintext.Length != ciphertext.Length) { throw new ArgumentOutOfRangeException(nameof(plaintext), plaintext.Length, $"{nameof(plaintext)} must be {ciphertext.Length} bytes long."); }
            Encrypt(plaintext, ciphertext, nonce, key, counter);
        }
    }
}