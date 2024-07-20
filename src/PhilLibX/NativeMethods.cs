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
// File: NativeMethods.cs
// Author: Philip/Scobalula
// Description: Native/Unmanaged Methods (DLLs required for certain components)
using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace PhilLibX
{
    /// <summary>
    /// Description: Native/Unmanaged Methods (DLLs required for certain components)
    /// </summary>
    public static unsafe partial class NativeMethods
    {
        #region Oodle
        internal static IntPtr OodleHandle;

        internal static delegate* unmanaged[Cdecl]<byte*, int, byte*, int, int, int, int, byte*, int, long, long, byte*, int, int, long> OodleDecompress;


        /// <summary>
        /// Sets the library to use for working with Oodle.
        /// </summary>
        /// <param name="path">The path of the library to use.</param>
        public static void SetOodleLibrary(string path)
        {
            // Since Oodle is a closed source lib and there are multiple versions
            // we must dynamically allow defining the exact DLL being loaded for it.
            // For other compression libs we have control over building their source.
            FreeOodleLibrary();
            OodleHandle = NativeLibrary.Load(path);
            OodleDecompress = (delegate* unmanaged[Cdecl]<byte*, int, byte*, int, int, int, int, byte*, int, long, long, byte*, int, int, long>)NativeLibrary.GetExport(OodleHandle, "OodleLZ_Decompress");
        }

        /// <summary>
        /// Frees the library used for working with Oodle.
        /// </summary>
        public static void FreeOodleLibrary()
        {
            if (OodleHandle != IntPtr.Zero)
            {
                NativeLibrary.Free(OodleHandle);
                OodleHandle = IntPtr.Zero; // Reset handle after freeing
            }
        }

        #endregion

        #region ZLIB
        internal enum MiniZReturnStatus
        {
            OK = 0,
            StreamEnd = 1,
            NeedDict = 2,
            ErrorNo = -1,
            StreamError = -2,
            DataError = -3,
            MemoryError = -4,
            BufferError = -5,
            VersionError = -6,
            ParamError = -10000
        };

        const string MiniZLibraryName = "MiniZ";

        [LibraryImport(MiniZLibraryName, EntryPoint = "mz_uncompress")]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        internal static partial int MZ_uncompress(byte* dest, ref int destLen, byte* source, int sourceLen, int windowBits);

        [LibraryImport(MiniZLibraryName, EntryPoint = "mz_deflateBound")]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        internal static partial int MZ_deflateBound(IntPtr stream, int inputSize);

        [LibraryImport(MiniZLibraryName, EntryPoint = "mz_compress")]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        internal static partial int MZ_compress(byte* dest, ref int destLen, byte* source, int sourceLen);
        #endregion

        #region LibFlac

        const string Dll = "LibFlac";
        [LibraryImport(Dll)]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        internal static partial IntPtr FLAC__stream_decoder_new();

        [LibraryImport(Dll)]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool FLAC__stream_decoder_set_md5_checking(IntPtr context, [MarshalAs(UnmanagedType.Bool)] bool value);

        [LibraryImport(Dll)]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool FLAC__stream_decoder_init_stream(IntPtr context, IntPtr buffer);

        [LibraryImport(Dll)]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        internal static partial IntPtr FLAC__stream_encoder_new();

        [LibraryImport(Dll)]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool FLAC__stream_encoder_set_verify(IntPtr context, [MarshalAs(UnmanagedType.Bool)] bool value);

        [LibraryImport(Dll)]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool FLAC__stream_encoder_set_compression_level(IntPtr context, int value);

        [LibraryImport(Dll)]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool FLAC__stream_encoder_set_channels(IntPtr context, int value);

        [LibraryImport(Dll)]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool FLAC__stream_encoder_set_bits_per_sample(IntPtr context, int value);

        [LibraryImport(Dll)]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool FLAC__stream_encoder_set_sample_rate(IntPtr context, int value);

        [LibraryImport(Dll)]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        internal static partial FLAC__StreamEncoderInitStatus FLAC__stream_encoder_init_file(IntPtr context, [MarshalAs(UnmanagedType.LPStr)] string filename, IntPtr progress, IntPtr userData);

        [LibraryImport(Dll)]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool FLAC__stream_encoder_process_interleaved(IntPtr context, IntPtr buffer, int samples);

        [LibraryImport(Dll)]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool FLAC__stream_encoder_finish(IntPtr context);

        [LibraryImport(Dll)]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool FLAC__stream_encoder_delete(IntPtr context);

        public enum FLAC__StreamEncoderInitStatus
        {
            FLAC__STREAM_ENCODER_INIT_STATUS_OK = 0,
            FLAC__STREAM_ENCODER_INIT_STATUS_UNSUPPORTED_CONTAINER = 1,
            FLAC__STREAM_ENCODER_INIT_STATUS_INVALID_CALLBACKS = 2,
            FLAC__STREAM_ENCODER_INIT_STATUS_MEMORY_ALLOCATION_ERROR = 3,
            FLAC__STREAM_ENCODER_INIT_STATUS_ERROR_OPENING_FILE = 4,
            FLAC__STREAM_ENCODER_INIT_STATUS_ALREADY_INITIALIZED = 5,
            FLAC__STREAM_ENCODER_INIT_STATUS_INVALID_NUMBER_OF_CHANNELS = 6,
            FLAC__STREAM_ENCODER_INIT_STATUS_INVALID_BITS_PER_SAMPLE = 7,
            FLAC__STREAM_ENCODER_INIT_STATUS_INVALID_SAMPLE_RATE = 8,
            FLAC__STREAM_ENCODER_INIT_STATUS_INVALID_BLOCK_SIZE = 9,
            FLAC__STREAM_ENCODER_INIT_STATUS_INVALID_MAX_LPC_ORDER = 10,
            FLAC__STREAM_ENCODER_INIT_STATUS_INVALID_QLP_COEFF_PRECISION = 11,
            FLAC__STREAM_ENCODER_INIT_STATUS_BLOCK_SIZE_TOO_SMALL_FOR_LPC_ORDER = 12,
            FLAC__STREAM_ENCODER_INIT_STATUS_NOT_STREAMABLE = 13,
            FLAC__STREAM_ENCODER_INIT_STATUS_OTHER = 14
        }

        #endregion

        #region Resolver
        static NativeMethods()
        {
            NativeLibrary.SetDllImportResolver(typeof(NativeMethods).Assembly, DllImportResolver);
        }

        public static IntPtr DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            var path = $"Native\\{RuntimeInformation.ProcessArchitecture}\\{libraryName}";

            try
            {
                return NativeLibrary.Load(path, assembly, searchPath);
            }
            // If we didn't find the exception, we don't want a fatal 
            // error here, we want to fall back to default resolver
            catch (DllNotFoundException)
            {
                return IntPtr.Zero;
            }
            // Everything else, we might have something more serious
            catch
            {
                throw;
            }
        }
        #endregion
    }
}
