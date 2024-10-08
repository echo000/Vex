﻿// ------------------------------------------------------------------------
// PhilLibX - My Utility Library
// Copyright(c) 2018 Philip/Scobalula
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// ------------------------------------------------------------------------
// File: Bytes.cs
// Author: Philip/Scobalula
// Description: Utilities for working with Bytes and Bits
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace PhilLibX
{
    /// <summary>
    /// Utilities for working with Bytes and Bits
    /// </summary>
    public static class Bytes
    {
        /// <summary>
        /// Reads a null terminated string from a byte array
        /// </summary>
        /// <param name="input">Byte Array input</param>
        /// <param name="startIndex">Start Index</param>
        /// <returns>Resulting string</returns>
        public static string ReadNullTerminatedString(byte[] input, int startIndex)
        {
            List<byte> result = [];

            for (int i = startIndex; i < input.Length && input[i] != 0; i++)
                result.Add(input[i]);

            return Encoding.ASCII.GetString(result.ToArray());
        }

        /// <summary>
        /// Gets the value of the bit at the given position
        /// </summary>
        /// <param name="input">Integer Input</param>
        /// <param name="bit">Position</param>
        /// <returns>Result</returns>
        public static byte GetBit(long input, int bit)
        {
            return (byte)((input >> bit) & 1);
        }

        /// <summary>
        /// Converts an array of bytes to a struct
        /// </summary>
        /// <typeparam name="T">Struct Type</typeparam>
        /// <param name="data">Raw data</param>
        /// <returns>Resulting Structure</returns>
        public static T BytesToStruct<T>(byte[] data) where T : struct
        {
            // Get handles
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            T structure = Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
            handle.Free();
            // Return result
            return structure;
        }

        /// <summary>
        /// Converts an array of bytes to a struct
        /// </summary>
        /// <typeparam name="T">Struct Type</typeparam>
        /// <param name="data">Raw data</param>
        /// <param name="startIndex">Start index to convert from</param>
        /// <returns>Resulting Structure</returns>
        public static T BytesToStruct<T>(byte[] data, int startIndex) where T : struct
        {
            // Size of Struct
            int size = Marshal.SizeOf<T>();
            byte[] buffer = new byte[size];
            Buffer.BlockCopy(data, startIndex, buffer, 0, size);
            // Return result
            return BytesToStruct<T>(buffer);
        }

        /// <summary>
        /// Converts a structure to an array of bytes
        /// </summary>
        /// <typeparam name="T">Struct Type</typeparam>
        /// <param name="structure">Structure to convert</param>
        /// <returns>Resulting byte array</returns>
        public static byte[] StructToBytes<T>(T structure)
        {
            if (structure == null)
            {
                throw new ArgumentNullException(nameof(structure));
            }
            // Size of Struct
            int size = Marshal.SizeOf<T>();
            byte[] buffer = new byte[size];
            IntPtr handle = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(structure, handle, false);
            Marshal.Copy(handle, buffer, 0, size);
            Marshal.FreeHGlobal(handle);
            // Return result
            return buffer;
        }
    }

    /// <summary>
    /// Represents a Pattern with a Needle and a Mask for use in pattern matching and search/find methods
    /// </summary>
    /// <typeparam name="T">Type to search for</typeparam>
    /// <remarks>
    /// Initializes a new <see cref="Pattern{T}"/> of the given type.
    /// </remarks>
    /// <param name="needle">The needle to search for.</param>
    /// <param name="mask">The mask to use for unknown values.</param>
    public struct Pattern<T>(T[] needle, T[] mask)
    {
        /// <summary>
        /// Gets or Sets the Needle
        /// </summary>
        public T[] Needle { get; set; } = needle;

        /// <summary>
        /// Gets or Sets the Mask
        /// </summary>
        public T[] Mask { get; set; } = mask;
    }

    public static class BytePattern
    {
        public static Pattern<byte> ParseString(string hexString)
        {
            // 2 characters per byte
            Span<char> buffer = stackalloc char[2];
            // We're going to need at least the size of this input string
            var pattern = new List<byte>(hexString.Length);
            var mask = new List<byte>(hexString.Length);

            for (int i = 0, j = 0; i < 2 && j < hexString.Length; j++)
            {
                // Skip whitespace
                if (char.IsWhiteSpace(hexString[j]))
                    continue;
                buffer[i++] = hexString[j];

                if (i == 2)
                {
                    i = 0;

                    // Check if unknown vs hex
                    if (buffer[0] == '?' || buffer[1] == '?')
                    {
                        pattern.Add(0);
                        mask.Add(0xFF);
                    }
                    else if (byte.TryParse(buffer, NumberStyles.HexNumber, null, out byte b))
                    {
                        pattern.Add(b);
                        mask.Add(0);
                    }
                }
            }

            return new([.. pattern], [.. mask]);
        }
    }
}
