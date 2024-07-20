/*
    Copyright (c) 2018 Philip/Scobalula - Utility Lib

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

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
using NAudio.Wave;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace PhilLibX.Audio
{
    /// <summary>
    /// Primitive WAV Class
    /// </summary>
    public class WAVUtil
    {
        /// <summary>
        /// Writes a WAV file
        /// </summary>
        /// <param name="fileName">File Path</param>
        /// <param name="sampleRate">Sample Rate in Hertz</param>
        /// <param name="channels">Number of Channels</param>
        /// <param name="audioData">Audio Data</param>
        public static void WriteWavFile(string fileName, int sampleRate, int channels, byte[] audioData)
        {
            using BinaryWriter bw = new(new FileStream(fileName, FileMode.Create));
            // Write RIFF ChunkID
            bw.Write(Encoding.ASCII.GetBytes("RIFF"));
            // Write Chunk Size
            bw.Write(36 + audioData.Length);
            // Write Format / Chunk 1 ID
            bw.Write(Encoding.ASCII.GetBytes("WAVEfmt "));
            // Write Chunk 1 Size
            bw.Write(16);
            // Write Audio Format
            bw.Write((ushort)1);
            // Write Number of Channels
            bw.Write((ushort)channels);
            // Write Sample Rate
            bw.Write(sampleRate);
            // Write Byte Rate (SampleRate * NumChannels * BitsPerSample/8)
            bw.Write(sampleRate * channels * 2);
            // Write Block Align (NumChannels * BitsPerSample/8)
            bw.Write((ushort)(channels * 2));
            // Write Bits Per Sample
            bw.Write((ushort)16);
            // Write data ChunkID
            bw.Write(Encoding.ASCII.GetBytes("data"));
            // Write Size of Audio
            bw.Write(audioData.Length);
            // Write Audio
            bw.Write(audioData);
        }

        /// <summary>
        /// Builds a WAV header
        /// </summary>
        /// <param name="FrameRate">Sample Rate in Hertz</param>
        /// <param name="Channels">Number of Channels</param>
        /// <param name="BufferSize">Length of the buffer</param>
        public static byte[] BuildWavHeader(int FrameRate, int Channels, int BufferSize)
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);
            // Write RIFF ChunkID
            bw.Write(Encoding.ASCII.GetBytes("RIFF"));
            // Write Chunk Size
            bw.Write(36 + BufferSize);
            // Write Format / Chunk 1 ID
            bw.Write(Encoding.ASCII.GetBytes("WAVEfmt "));
            // Write Chunk 1 Size
            bw.Write(16);
            // Write Audio Format
            bw.Write((ushort)1);
            // Write Number of Channels
            bw.Write((ushort)Channels);
            // Write Sample Rate
            bw.Write(FrameRate);
            // Write Byte Rate (SampleRate * NumChannels * BitsPerSample/8)
            bw.Write(FrameRate * Channels * 2);
            // Write Block Align (NumChannels * BitsPerSample/8)
            bw.Write((ushort)(Channels * 2));
            // Write Bits Per Sample
            bw.Write((ushort)16);
            // Write data ChunkID
            bw.Write(Encoding.ASCII.GetBytes("data"));
            // Write Size of Audio
            bw.Write(BufferSize);
            //Return the header
            return ms.ToArray();
        }

        public static unsafe bool TranscodeWAVToFlac(byte[] SoundBuffer, ulong SoundSize, string OutputFile)
        {
            try
            {
                // Load WAV file from buffer
                using var wavStream = new MemoryStream(SoundBuffer);
                using var waveFileReader = new WaveFileReader(wavStream);
                var waveFormat = waveFileReader.WaveFormat;

                // Initialize FLAC encoder
                IntPtr encoder = NativeMethods.FLAC__stream_encoder_new();
                if (encoder == IntPtr.Zero)
                {
                    Console.WriteLine("Failed to initialize FLAC encoder.");
                    return false;
                }

                // Configure the encoder
                NativeMethods.FLAC__stream_encoder_set_verify(encoder, false);
                NativeMethods.FLAC__stream_encoder_set_compression_level(encoder, 5);
                NativeMethods.FLAC__stream_encoder_set_channels(encoder, waveFormat.Channels);
                NativeMethods.FLAC__stream_encoder_set_bits_per_sample(encoder, waveFormat.BitsPerSample);
                NativeMethods.FLAC__stream_encoder_set_sample_rate(encoder, waveFormat.SampleRate);

                // Initialize the encoder to write to a file
                var initStatus = NativeMethods.FLAC__stream_encoder_init_file(encoder, OutputFile, IntPtr.Zero, IntPtr.Zero);
                if (initStatus != NativeMethods.FLAC__StreamEncoderInitStatus.FLAC__STREAM_ENCODER_INIT_STATUS_OK)
                {
                    Trace.WriteLine($"Failed to initialize FLAC file: Status {initStatus}");
                    return false;
                }

                // Read and encode WAV data
                var buffer = new byte[waveFileReader.WaveFormat.AverageBytesPerSecond];
                var pcmBuffer = new int[buffer.Length / (waveFileReader.WaveFormat.BitsPerSample / 8)];

                int bytesRead;
                while ((bytesRead = waveFileReader.Read(buffer, 0, buffer.Length)) > 0)
                {
                    // Convert byte buffer to PCM
                    for (int i = 0; i < bytesRead / (waveFileReader.WaveFormat.BitsPerSample / 8); i++)
                    {
                        pcmBuffer[i] = BitConverter.ToInt16(buffer, i * 2);
                    }

                    // Write PCM buffer to FLAC encoder
                    unsafe
                    {
                        fixed (int* pcmBufferPtr = pcmBuffer)
                        {
                            var input = new IntPtr(pcmBufferPtr);
                            if (!NativeMethods.FLAC__stream_encoder_process_interleaved(encoder, input, bytesRead / waveFileReader.WaveFormat.BlockAlign))
                            {
                                Trace.WriteLine("Error processing PCM data.");
                                return false;
                            }
                        }
                    }
                }

                // Finalize the encoding
                NativeMethods.FLAC__stream_encoder_finish(encoder);
                NativeMethods.FLAC__stream_encoder_delete(encoder);

                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error during transcoding: {ex.Message}");
                return false;
            }

            /*// Continue if we're ok
            if (IsOk)
            {
                // Total size to read
                var Left = WAVInfo.TotalSamples;
                // Buffers
                byte[] buf = new byte[1024 * ((WAVInfo.Bps / 8) * WAVInfo.ChannelsCount)];
                int[] PCMBuffer = new int[1024 * 2];

                // Loop until EOF
                while (IsOk && Left > 0)
                {
                    // Calculate what we need
                    var Need = (Left > 1024 ? 1024 : Left);
                    // Calculate data size to read
                    var SizeToRead = (WAVInfo.ChannelsCount * (WAVInfo.Bps / 8u)) * Need;
                    // Ensure we can read it
                    if (WAVInfoReader.BaseStream.Position + (long)SizeToRead <= WAVInfoReader.BaseStream.Length)
                    {
                        // Read
                        var tempArray = WAVInfoReader.ReadBytes((int)SizeToRead);

                        Buffer.BlockCopy(tempArray, 0, buf, 0, tempArray.Length);

                        // Convert the data to PCM samples
                        for (uint i = 0; i < (Need * WAVInfo.ChannelsCount); i++)
                        {
                            // Convert to PCM
                            PCMBuffer[i] = (int)((short)((ushort)(buf[2 * i + 1] << 8) | (ushort)buf[2 * i]));
                        }

                        fixed (int* fixedInput = PCMBuffer)
                        {
                            IntPtr input = new(fixedInput);
                            // Send off to encoder
                            IsOk = NativeMethods.FLAC__stream_encoder_process_interleaved(Encoder, input, (int)Need);
                        }
                    }

                    else
                    {
                        // Failed
                        IsOk = false;
                    }

                    // Advance
                    Left -= Need;
                }
            }
            // Finalize the encoding and clean up
            NativeMethods.FLAC__stream_encoder_finish(Encoder);
            NativeMethods.FLAC__stream_encoder_delete(Encoder);
            // Result of decode
            return IsOk;*/
        }
    }
}