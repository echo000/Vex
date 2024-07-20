using NAudio.Flac;
using NAudio.Wave;
using System;
using System.Buffers.Binary;
using System.IO;
using System.Text;

namespace PhilLibX.Audio
{
    public class FlacUtil
    {
        /// <summary>
        /// Writes a FLAC file
        /// </summary>
        /// <param name="fileName">File Path</param>
        /// <param name="sampleRate">Sample Rate in Hertz</param>
        /// <param name="channels">Number of Channels</param>
        /// <param name="audioData">Audio Data</param>
        public static void WriteFlacFile(string fileName, int sampleRate, int channels, byte[] audioData)
        {
            using BinaryWriter bw = new(new FileStream(fileName, FileMode.Create));
            bw.Write(Encoding.ASCII.GetBytes("fLaC"));
            // Constant values
            const ushort MinimumBlockSize = 0x80, MaximumBlockSize = 0, MinimumFrameSize = 0x22, MaximumFramesize = 0x4, SampleRate = 0x4;
            bw.Write((byte)MinimumBlockSize);
            bw.Write(MaximumBlockSize);
            bw.Write((byte)MinimumFrameSize);
            bw.Write(MaximumFramesize);
            bw.Write((ushort)SampleRate);
            bw.Write(new byte[6]);
            {
                ulong Flags = 0;
                Flags += ((ulong)sampleRate << 44);
                Flags += ((ulong)(channels - 1) << 41);
                Flags += ((ulong)(16 - 1) << 36);
                Flags += ((ulong)audioData.Length);
                Flags = BinaryPrimitives.ReverseEndianness(Flags);
                bw.Write(Flags);
            }
            bw.Write(new byte[16]);
            // Write Audio
            bw.Write(audioData);
        }

        /// <summary>
        /// Builds a FLAC header
        /// </summary>
        /// <param name="FrameRate">Sample Rate in Hertz</param>
        /// <param name="Channels">Number of Channels</param>
        /// <param name="BufferSize">Length of the buffer</param>
        public static byte[] BuildFlacHeader(int FrameRate, int Channels, int BufferSize)
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);
            bw.Write(Encoding.ASCII.GetBytes("fLaC"));
            // Constant values
            const ushort MinimumBlockSize = 0x80, MaximumBlockSize = 0, MinimumFrameSize = 0x22, MaximumFramesize = 0x4, SampleRate = 0x4;
            bw.Write((byte)MinimumBlockSize);
            bw.Write(MaximumBlockSize);
            bw.Write((byte)MinimumFrameSize);
            bw.Write(MaximumFramesize);
            bw.Write((ushort)SampleRate);
            bw.Write(new byte[6]);
            {
                ulong Flags = 0;
                Flags += ((ulong)FrameRate << 44);
                Flags += ((ulong)(Channels - 1) << 41);
                Flags += ((ulong)(16 - 1) << 36);
                Flags += ((ulong)BufferSize);
                Flags = BinaryPrimitives.ReverseEndianness(Flags);
                bw.Write(Flags);
            }
            bw.Write(new byte[16]);
            return ms.ToArray();
        }

        public static unsafe bool TranscodeFLACToWav(byte[] SoundBuffer, string OutputFile)
        {
            try
            {
                using var flacStream = new MemoryStream(SoundBuffer);
                using var flacReader = new FlacReader(flacStream);
                using var wavStream = new MemoryStream();
                using var wavWriter = new WaveFileWriter(wavStream, flacReader.WaveFormat);

                var buffer = new byte[flacReader.WaveFormat.AverageBytesPerSecond];
                int bytesRead;
                while ((bytesRead = flacReader.Read(buffer, 0, buffer.Length)) > 0)
                {
                    wavWriter.Write(buffer, 0, bytesRead);
                }

                wavWriter.Flush();
                File.WriteAllBytes(OutputFile, wavStream.ToArray());
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
