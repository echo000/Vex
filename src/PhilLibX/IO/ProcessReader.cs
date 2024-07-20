// ------------------------------------------------------------------------
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
// File: IO/ProcessReader.cs
// Author: Philip/Scobalula
// Description: A class to help with reading the memory of other processes.
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.ProcessStatus;

namespace PhilLibX.IO
{
    /// <summary>
    /// A class to help with reading the memory of other processes.
    /// </summary>
    [SupportedOSPlatform("windows7.0")]
    public class ProcessReader : IDisposable
    {
        /// <summary>
        /// A structure to hold information about a module from a Process IO
        /// </summary>
        /// <remarks>
        /// Creates a new Module structure holds information about a module from a Process IO
        /// </remarks>
        /// <param name="path">Path of the Module</param>
        /// <param name="baseAddress">File Name of the Module</param>
        /// <param name="size">Size of the module. This size should only include the static module and data.</param>
        /// <param name="entryPoint">Entry point for the module</param>
        public struct Module(string? path, IntPtr baseAddress, IntPtr entryPoint, int size)
        {
            /// <summary>
            /// Gets the File Path of the Module
            /// </summary>
            public string FilePath { get; private set; } = path ?? string.Empty;

            /// <summary>
            /// Gets the File Name of the Module
            /// </summary>
            public readonly string FileName { get => Path.GetFileName(FilePath); }

            /// <summary>
            /// Gets the Directory of the Module
            /// </summary>
            public readonly string FileDirectory { get => Path.GetDirectoryName(FilePath) ?? String.Empty; }

            /// <summary>
            /// Gets the Base Address of the Module
            /// </summary>
            public IntPtr BaseAddress { get; private set; } = baseAddress;

            /// <summary>
            /// Gets the address of the entry point for the module
            /// </summary>
            public IntPtr EntryPointAddress { get; private set; } = entryPoint;

            /// <summary>
            /// Gets the size of the module. This size only includes the static module and data.
            /// </summary>
            public int Size { get; private set; } = size;
        }

        /// <summary>
        /// Internal Handle Property
        /// </summary>
        internal HANDLE Handle { get; set; }

        public ProcessReader(string processName)
        {
            var processes = Process.GetProcessesByName(processName);

            if (processes.Length == 0)
                throw new IOException($"");

        }

        public ProcessReader(Process process) : this(process.Id) { }

        public ProcessReader(int processID)
        {
            Handle = PInvoke.OpenProcess(Windows.Win32.System.Threading.PROCESS_ACCESS_RIGHTS.PROCESS_ALL_ACCESS, false, (uint)processID);

            if (Handle == HANDLE.Null)
                throw new Win32Exception();
        }

        public static Module GetModule(string moduleName, IEnumerable<Module> modules)
        {
            foreach (var module in modules)
            {
                if (module.FileName.Equals(moduleName, StringComparison.CurrentCultureIgnoreCase))
                {
                    return module;
                }
            }

            return new Module(null, IntPtr.Zero, IntPtr.Zero, -1);
        }

        public byte ReadByte(long address)
        {
            return ReadBytes(address, 1)[0];
        }

        public unsafe string ReadNullTerminatedString(long address, int bufferSize = 0xFF)
        {
            var result = stackalloc byte[bufferSize];
            ReadBytes(address, bufferSize, result);
            int sizeOf;
            for (sizeOf = 0; sizeOf < bufferSize; sizeOf++)
            {
                if (result[sizeOf] == 0x0)
                    break;
            }
            return Encoding.UTF8.GetString(result, sizeOf);
        }

        public T[] ReadArray<T>(long address, int count) where T : struct
        {
            // Get Byte Count
            var structSize = Marshal.SizeOf<T>();
            var size = count * structSize;
            // Allocate Array
            var result = new T[count];
            // Check for primitives, we can use BlockCopy for them
            if (typeof(T).IsPrimitive)
            {
                // Copy
                Buffer.BlockCopy(ReadBytes(address, size), 0, result, 0, size);
            }
            // Slightly more complex structures, we can use the struct functs
            else
            {
                // Loop through
                for (int i = 0; i < count; i++)
                {
                    // Read it into result
                    result[i] = ReadStruct<T>(address + (i * structSize));
                }
            }
            // Done
            return result;
        }

        public T ReadStruct<T>(long address) where T : struct
        {
            byte[] data = ReadBytes(address, Marshal.SizeOf<T>());
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            T theStructure = Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
            handle.Free();
            return theStructure;
        }

        public ushort ReadUInt16(long address)
        {
            Span<byte> buf = stackalloc byte[Unsafe.SizeOf<ushort>()];
            ReadExactly(buf, address);
            return BinaryPrimitives.ReadUInt16LittleEndian(buf);
        }
        public short ReadInt16(long address)
        {
            Span<byte> buf = stackalloc byte[Unsafe.SizeOf<short>()];
            ReadExactly(buf, address);
            return BinaryPrimitives.ReadInt16LittleEndian(buf);
        }
        public uint ReadUInt32(long address)
        {
            Span<byte> buf = stackalloc byte[Unsafe.SizeOf<uint>()];
            ReadExactly(buf, address);
            return BinaryPrimitives.ReadUInt32LittleEndian(buf);
        }
        public int ReadInt32(long address)
        {
            Span<byte> buf = stackalloc byte[Unsafe.SizeOf<int>()];
            ReadExactly(buf, address);
            return BinaryPrimitives.ReadInt32LittleEndian(buf);
        }
        public ulong ReadUInt64(long address)
        {
            Span<byte> buf = stackalloc byte[Unsafe.SizeOf<ulong>()];
            ReadExactly(buf, address);
            return BinaryPrimitives.ReadUInt64LittleEndian(buf);
        }
        public long ReadInt64(long address)
        {
            Span<byte> buf = stackalloc byte[Unsafe.SizeOf<long>()];
            ReadExactly(buf, address);
            return BinaryPrimitives.ReadInt64LittleEndian(buf);
        }
        public float ReadSingle(long address)
        {
            Span<byte> buf = stackalloc byte[Unsafe.SizeOf<float>()];
            ReadExactly(buf, address);
            return BinaryPrimitives.ReadSingleLittleEndian(buf);
        }
        public double ReadDouble(long address)
        {
            Span<byte> buf = stackalloc byte[Unsafe.SizeOf<double>()];
            ReadExactly(buf, address);
            return BinaryPrimitives.ReadDoubleLittleEndian(buf);
        }
        public unsafe void ReadExactly(Span<byte> buffer, long address)
        {
            fixed (byte* p = buffer)
            {
                PInvoke.ReadProcessMemory(Handle, (void*)address, p, (nuint)buffer.Length);
            }
        }

        public byte[] ReadBytes(long address, int size)
        {
            var buf = new byte[size];
            ReadExactly(buf, address);
            return buf;
        }

        public int ReadBytes(byte[] buffer, int offset, int size, long address) => ReadBytes(buffer.AsSpan()[offset..size], address);

        public unsafe int ReadBytes(Span<byte> buffer, long address)
        {
            fixed (byte* p = buffer)
            {
                nuint v = 0;
                if (!PInvoke.ReadProcessMemory(Handle, (void*)address, p, (nuint)buffer.Length, &v))
                    return 0;

                return (int)v;
            }
        }

        public unsafe void ReadBytes(long address, int numBytes, byte* buffer)
        {
            PInvoke.ReadProcessMemory(Handle, (void*)address, buffer, (nuint)numBytes);
        }

        /// <summary>
        /// Searches for bytes in the Processes Memory
        /// </summary>
        /// <param name="needle">Byte Sequence to scan for.</param>
        /// <param name="startAddress">Address to start the search at.</param>
        /// <param name="endAddress">Address to end the search at.</param>
        /// <param name="firstMatch">If we should stop the search at the first result.</param>
        /// <param name="bufferSize">Byte Buffer Size</param>
        /// <returns>Results</returns>
        public unsafe long[] FindBytes(byte?[] needle, long startAddress, long endAddress, bool firstMatch = false, int bufferSize = 0xFFFF)
        {
            List<long> results = [];
            long searchAddress = startAddress;

            int needleIndex = 0;
            int bufferIndex = 0;

            while (true)
            {
                try
                {
                    byte[] buffer = ReadBytes(searchAddress, bufferSize);

                    fixed (byte* p = buffer)
                    {
                        for (bufferIndex = 0; bufferIndex < buffer.Length; bufferIndex++)
                        {
                            if (needle[needleIndex] == null)
                            {
                                needleIndex++;
                                continue;
                            }

                            if (needle[needleIndex] == p[bufferIndex])
                            {
                                needleIndex++;

                                if (needleIndex == needle.Length)
                                {
                                    results.Add(searchAddress + bufferIndex - needle.Length + 1);

                                    if (firstMatch)
                                        return [.. results];

                                    needleIndex = 0;
                                }
                            }
                            else
                            {
                                needleIndex = 0;
                            }
                        }
                    }
                }
                catch
                {
                    break;
                }

                searchAddress += bufferSize;

                if (searchAddress > endAddress)
                    break;
            }

            return [.. results];
        }

        public IEnumerable<long> FindBytes(string pattern)
        {
            var b = BytePattern.ParseString(pattern);
            var buffer = new byte[65535];
            var address = GetBaseAddress();
            var addressEnd = address + GetModuleSize();

            int needleIndex = 0;

            while (true)
            {
                var bytesToRead = (int)Math.Min(addressEnd - address, buffer.Length);
                var bytesRead = ReadBytes(buffer, 0, buffer.Length, address);

                if (address >= addressEnd)
                    break;

                if (bytesRead == 0)
                {
                    address += bytesToRead;
                    continue;
                }

                for (int i = 0; i < bytesRead; i++)
                {
                    if (b.Needle[needleIndex] == buffer[i] || b.Mask[needleIndex] == 0xFF)
                    {
                        needleIndex++;

                        if (needleIndex == b.Needle.Length)
                        {
                            yield return address + i + 1 - b.Needle.Length;

                            needleIndex = 0;
                        }
                    }
                    else
                    {
                        needleIndex = 0;

                        if (b.Needle[needleIndex] == buffer[i] || b.Needle[needleIndex] == 0xFF)
                        {
                            needleIndex++;

                            if (needleIndex == b.Needle.Length)
                            {
                                yield return address + i + 1 - b.Needle.Length;

                                needleIndex = 0;
                            }
                        }
                    }
                }

                if (bytesRead < buffer.Length)
                    break;

                address += bytesRead;
            }
        }

        /// <summary>
        /// Gets the Base Address of the Process' Main Module
        /// </summary>
        public unsafe long GetBaseAddress()
        {
            // 3. Prepare an array to hold module handles
            Span<HMODULE> moduleHandles = stackalloc HMODULE[1]; // Assuming a maximum of 1024 modules
            uint cbNeeded;

            fixed (HMODULE* mods = moduleHandles)
            {
                // 4. Enumerate modules
                bool success = PInvoke.EnumProcessModulesEx(Handle, mods, (uint)moduleHandles.Length * (uint)IntPtr.Size, &cbNeeded, ENUM_PROCESS_MODULES_EX_FLAGS.LIST_MODULES_ALL);

                int numModules = (int)(cbNeeded / Unsafe.SizeOf<HMODULE>());

                var moduleInfo = new MODULEINFO();
                success = PInvoke.GetModuleInformation(Handle, moduleHandles[0], &moduleInfo, (uint)Unsafe.SizeOf<MODULEINFO>());

                return (long)moduleInfo.lpBaseOfDll;
            }
        }

        /// <summary>
        /// Gets the size of the Main Module Size
        /// </summary>
        /// <returns>Main Module Size</returns>
        public unsafe int GetModuleSize()
        {
            Span<HMODULE> moduleHandles = stackalloc HMODULE[1]; // Assuming a maximum of 1024 modules
            uint cbNeeded;

            fixed (HMODULE* mods = moduleHandles)
            {
                bool success = PInvoke.EnumProcessModulesEx(Handle, mods, (uint)moduleHandles.Length * (uint)IntPtr.Size, &cbNeeded, ENUM_PROCESS_MODULES_EX_FLAGS.LIST_MODULES_ALL);

                int numModules = (int)(cbNeeded / Unsafe.SizeOf<HMODULE>());

                var moduleInfo = new MODULEINFO();
                success = PInvoke.GetModuleInformation(Handle, moduleHandles[0], &moduleInfo, (uint)Unsafe.SizeOf<MODULEINFO>());

                return (int)moduleInfo.SizeOfImage;
            }
        }

        public unsafe string GetProcessPath()
        {
            uint cbNeeded = 256;
            char[] buffer = new char[256];
            fixed (char* ptr = buffer)
            {
                PWSTR pwstr = new(ptr);
                var success = PInvoke.QueryFullProcessImageName(Handle, Windows.Win32.System.Threading.PROCESS_NAME_FORMAT.PROCESS_NAME_WIN32, pwstr, &cbNeeded);
                var st = Marshal.PtrToStringUni((IntPtr)pwstr.Value, (int)cbNeeded) ?? string.Empty;
                return st;
            }
        }

        private bool disposedValue = false; // To detect redundant calls
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    PInvoke.CloseHandle(Handle);
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
