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
// File: IO/BinaryReaderExtensions.cs
// Author: Philip/Scobalula
// Description: BinaryReader extensions for reading null terminated strings, scanning files, etc.
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace PhilLibX.IO
{
    /// <summary>
    /// IO Utilities/Extensions
    /// </summary>
    public static class BinaryReaderExtensions
    {
        /// <summary>
        /// Reads a string terminated by a null byte
        /// </summary>
        /// <returns>Read String</returns>
        public static string ReadNullTerminatedString(this BinaryReader br, int maxSize = -1)
        {
            // Create String Builder
            StringBuilder str = new();
            // Current Byte Read
            int byteRead;
            // Size of String
            int size = 0;
            // Loop Until we hit terminating null character
            while ((byteRead = br.BaseStream.ReadByte()) != 0x0 && size++ != maxSize)
                str.Append(Convert.ToChar(byteRead));
            // Ship back Result
            return str.ToString();
        }

        /// <summary>
        /// Reads a string terminated by a null byte
        /// </summary>
        /// <param name="br">Reader</param>
        /// <param name="offset">Absolute offset of the string</param>
        /// <param name="maxSize">Max size of the string to read</param>
        /// <returns>Resulting string</returns>
        public static string ReadNullTerminatedString(this BinaryReader br, long offset, int maxSize = -1)
        {
            long currentOffset = br.BaseStream.Position;
            br.BaseStream.Position = offset;
            var result = br.ReadNullTerminatedString(maxSize);
            br.BaseStream.Position = currentOffset;
            return result;
        }


        /// <summary>
        /// Reads a UTF16 string terminated by a null byte
        /// </summary>
        /// <returns>Read String</returns>
        public static string ReadUTF16NullTerminatedString(this BinaryReader br, int maxSize = -1)
        {
            // Create String Builder
            StringBuilder str = new();
            // Current Byte Read
            ushort byteRead;
            // Size of String
            int size = 0;
            // Loop Until we hit terminating null character
            while ((byteRead = br.ReadUInt16()) != 0x0 && size++ != maxSize)
                str.Append(Convert.ToChar(byteRead));
            // Ship back Result
            return str.ToString();
        }

        /// <summary>
        /// Reads a UTF16 string terminated by a null byte
        /// </summary>
        /// <param name="br">Reader</param>
        /// <param name="offset">Absolute offset of the string</param>
        /// <param name="maxSize">Max size of the string to read</param>
        /// <returns>Resulting string</returns>
        public static string ReadUTF16NullTerminatedString(this BinaryReader br, long offset, int maxSize = -1)
        {
            long currentOffset = br.BaseStream.Position;
            br.BaseStream.Position = offset;
            var result = br.ReadUTF16NullTerminatedString(maxSize);
            br.BaseStream.Position = currentOffset;
            return result;
        }

        /// <summary>
        /// Reads a string of fixed size
        /// </summary>
        /// <param name="br">Reader</param>
        /// <param name="numBytes">Size of string in bytes</param>
        /// <returns>Read String</returns>
        public static string ReadFixedString(this BinaryReader br, int numBytes)
        {
            // Purge Null Bytes and Return 
            return Encoding.UTF8.GetString(br.ReadBytes(numBytes)).TrimEnd('\0');
        }

        public static string ReadFixedPrefixString(this BinaryReader br)
        {
            var stringLength = br.ReadInt32();
            return br.ReadFixedString(stringLength);
        }

        /// <summary>
        /// Reads a string of fixed size
        /// </summary>
        /// <param name="br">Reader</param>
        /// <param name="offset">Absolute offset of the string</param>
        /// <param name="numBytes">Size of string in bytes</param>
        /// <returns>Read String</returns>
        public static string ReadFixedString(this BinaryReader br, long offset, int numBytes)
        {
            long currentOffset = br.BaseStream.Position;
            br.BaseStream.Position = offset;
            var result = br.ReadFixedString(numBytes);
            br.BaseStream.Position = currentOffset;
            return result;
        }

        /// <summary>
        /// Reads an array of the given type
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="br">Reader</param>
        /// <param name="count">Number of items</param>
        /// <returns>Resulting array</returns>
        public static T[] ReadArray<T>(this BinaryReader br, int count) where T : struct
        {
            // Get Byte Count
            var size = count * Marshal.SizeOf<T>();
            // Allocate Array
            var result = new T[count];
            // Check for primitives, we can use BlockCopy for them
            if (typeof(T).IsPrimitive)
            {
                // Copy
                Buffer.BlockCopy(br.ReadBytes(size), 0, result, 0, size);
            }
            // Slightly more complex structures, we can use the struct functs
            else
            {
                // Loop through
                for (int i = 0; i < count; i++)
                {
                    // Read it into result
                    result[i] = br.ReadStruct<T>();
                }
            }
            // Done
            return result;
        }

        /// <summary>
        /// Reads a native data structure from the current stream and advances the current position of the stream by the size of the array
        /// </summary>
        /// <typeparam name="T">The structure type to read</typeparam>
        /// <param name="reader">Current <see cref="BinaryReader"/></param>
        /// <param name="count">The number of items to read. This value must be 0 or a non-negative number or an exception will occur.</param>
        /// <param name="position">Position of the data</param>
        /// <returns>A structure array of the given type from the current stream</returns>
        public unsafe static void ReadArray<T>(this BinaryReader reader, ref Span<T> input) where T : unmanaged
        {
            if (input.Length == 0)
                return;

            var asBytes = MemoryMarshal.Cast<T, byte>(input);

            if (reader.Read(asBytes) < asBytes.Length)
                throw new IOException();
        }

        /// <summary>
        /// Reads an array of the given type
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="br">Reader</param>
        /// <param name="offset">Absolute offset of the string</param>
        /// <param name="count">Number of items</param>
        /// <returns>Resulting array</returns>
        public static T[] ReadArray<T>(this BinaryReader br, long offset, int count) where T : struct
        {
            long currentOffset = br.BaseStream.Position;
            br.BaseStream.Position = offset;
            var result = br.ReadArray<T>(count);
            br.BaseStream.Position = currentOffset;
            return result;
        }

        /// <summary>
        /// Reads the given structure from the reader
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="br"></param>
        /// <returns></returns>
        public static T ReadStruct<T>(this BinaryReader br) where T : struct
        {
            byte[] data = br.ReadBytes(Marshal.SizeOf<T>());
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            T theStructure = Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
            handle.Free();
            return theStructure;
        }

        /// <summary>
        /// Reads the given structure from the reader
        /// </summary>
        /// <param name="offset">Absolute offset of the string</param>
        /// <typeparam name="T"></typeparam>
        /// <param name="br"></param>
        /// <returns></returns>
        public static T ReadStruct<T>(this BinaryReader br, long offset) where T : struct
        {
            long currentOffset = br.BaseStream.Position;
            br.BaseStream.Position = offset;
            var result = br.ReadStruct<T>();
            br.BaseStream.Position = currentOffset;
            return result;
        }

        /// <summary>
        /// Sets the position of the Base Stream
        /// </summary>
        /// <param name="br"></param>
        /// <param name="offset">Offset to seek to.</param>
        /// <param name="seekOrigin">Seek Origin</param>
        public static void Seek(this BinaryReader br, long offset, SeekOrigin seekOrigin = SeekOrigin.Begin)
        {
            // Set stream position
            br.BaseStream.Seek(offset, seekOrigin);
        }

        /// <summary>
        /// Sets the position of the Base Stream
        /// </summary>
        /// <param name="br"></param>
        /// <param name="bytes">Number of bytes to advance</param>
        public static void Advance(this BinaryReader br, long bytes)
        {
            br.BaseStream.Position += bytes;
        }

        /// <summary>
        /// Finds occurences of a string in the stream
        /// </summary>
        /// <param name="br">Reader to use for scanning</param>
        /// <param name="needle">String Needle to search for</param>
        /// <param name="firstOccurence">Stops at first result</param>
        /// <returns>Resulting offsets</returns>
        public static long[] FindString(this BinaryReader br, string needle, bool firstOccurence = false)
        {
            // Convert to bytes and scan
            return br.FindBytes(Encoding.UTF8.GetBytes(needle), firstOccurence);
        }

        /// <summary>
        /// Reads a 4-byte big endian signed integer from the current stream and advances the current position of the stream by four bytes.
        /// </summary>
        /// <param name="br">Reader</param>
        /// <returns>Resulting 32Bit Integer</returns>
        public static int ReadBEInt32(this BinaryReader br)
        {
            // Read bytes
            byte[] buffer = br.ReadBytes(4);
            // Return resulting 4 byte int
            return (buffer[0] << 24) | (buffer[1] << 16) | (buffer[2] << 8) | buffer[3];
        }

        public static uint ReadBEUInt32(this BinaryReader br)
        {
            // Read bytes
            byte[] buffer = br.ReadBytes(4);
            // Return resulting 4 byte int
            return (uint)((buffer[0] << 24) | (buffer[1] << 16) | (buffer[2] << 8) | buffer[3]);
        }

        public static short ReadBEInt16(this BinaryReader br)
        {
            byte[] buffer = br.ReadBytes(2);
            var highByte = buffer[0];
            var lowByte = buffer[0 + 1];
            return (short)(highByte << 8 | lowByte);
        }

        public static ushort ReadBEUInt16(this BinaryReader br)
        {
            byte[] buffer = br.ReadBytes(2);
            var highByte = buffer[0];
            var lowByte = buffer[0 + 1];
            return (ushort)(highByte << 8 | lowByte);
        }

        public static ulong ReadBEUInt64(this BinaryReader br)
        {
            // Read bytes
            byte[] buffer = br.ReadBytes(8);

            // Return resulting 8 byte ulong
            ulong value = 0;
            for (int i = 0; i < 8; i++)
            {
                value <<= 8;
                value |= buffer[i];
            }
            return value;
        }

        public static long ReadBEInt64(this BinaryReader br)
        {
            // Read bytes
            byte[] buffer = br.ReadBytes(8);

            // Return resulting 8 byte long
            long value = 0;
            for (int i = 0; i < 8; i++)
            {
                value <<= 8;
                value |= buffer[i];
            }
            return value;
        }

        /// <summary>
        /// Reads a 3-byte big endian signed integer from the current stream and advances the current position of the stream by three bytes.
        /// </summary>
        /// <param name="br">Reader</param>
        /// <returns>Resulting 32Bit Integer</returns>
        public static int ReadBEInt24(this BinaryReader br)
        {
            // Read bytes
            byte[] buffer = br.ReadBytes(3);
            // Return resulting 3 byte int
            return (buffer[0] << 16) | (buffer[1] << 8) | (buffer[2]);
        }

        /// <summary>
        /// Reads a variable length integer from the current stream 
        /// </summary>
        /// <param name="br">Reader</param>
        /// <returns>Resulting 32Bit Integer</returns>
        public static int Read7BitEncodedInt(this BinaryReader br)
        {
            int result = 0;
            int shift = 0;

            // Loop until the high bit is 0
            while (true)
            {
                byte value = br.ReadByte();
                result |= (value & 0x7F) << shift;

                if ((value & 0x80) == 0) return result;

                shift += 7;
            }
        }

        /// <summary>
        /// Finds occurences of bytes
        /// </summary>
        /// <param name="br">Reader</param>
        /// <param name="needle">Byte Array Needle to search for</param>
        /// <param name="firstOccurence">Stops at first result</param>
        /// <returns>Resulting offsets</returns>
        public static long[] FindBytes(this BinaryReader br, byte[] needle, bool firstOccurence = false)
        {
            // List of offsets in file.
            List<long> offsets = [];

            // Buffer
            byte[] buffer = new byte[1048576];

            // Starting Offset
            long readBegin = br.BaseStream.Position;

            // Needle Index
            int needleIndex = 0;

            // Bytes Read
            int bytesRead;
            // Read chunk of file
            while ((bytesRead = br.BaseStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                // Loop through byte array
                for (var bufferIndex = 0; bufferIndex < bytesRead; bufferIndex++)
                {
                    // Check if current bytes match
                    if (needle[needleIndex] == buffer[bufferIndex])
                    {
                        // Increment
                        needleIndex++;

                        // Check if we have a match
                        if (needleIndex == needle.Length)
                        {
                            // Add Offset
                            offsets.Add(readBegin + bufferIndex + 1 - needle.Length);

                            // Reset Index
                            needleIndex = 0;

                            // If only first occurence, end search
                            if (firstOccurence)
                                return [.. offsets];
                        }
                    }
                    else
                    {
                        // Reset Index
                        needleIndex = 0;
                    }
                }
                // Set next offset
                readBegin += bytesRead;
            }
            // Return offsets as an array
            return [.. offsets];
        }
    }
}
