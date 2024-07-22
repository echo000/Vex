using PhilLibX.Audio;
using System;

namespace Vex.Library
{
    // A list of supported sound formats, input and output
    public enum SoundFormat
    {
        // Standard sound formats

        Standard_WAV,
        Standard_FLAC,
        Standard_OGG,

        // This is used only for InFormat, which will read the WAV format from the header in the stream
        WAV_WithHeader,
        // This is used only for InFormat, which will read the FLAC format from the header in the stream
        FLAC_WithHeader
    };

    public class Sound
    {
        public static bool ConvertSoundMemory(byte[] SoundBuffer, long SoundSize, SoundFormat InFormat, string OutputFile, SoundFormat OutFormat)
        {
            var ConversionResult = false;

            switch (InFormat)
            {
                case SoundFormat.FLAC_WithHeader:
                    if (OutFormat == SoundFormat.Standard_WAV)
                    {
                        ConversionResult = FlacUtil.TranscodeFLACToWav(SoundBuffer, OutputFile);
                    }
                    break;
                case SoundFormat.WAV_WithHeader:
                    if (OutFormat == SoundFormat.Standard_FLAC)
                    {
                        ConversionResult = WAVUtil.TranscodeWAVToFlac(SoundBuffer, (ulong)SoundSize, OutputFile);
                    }
                    break;
            }

            return ConversionResult;
        }

        public static bool ExportSoundAsset(string SoundPath, string AssetName, VexInstance instance)
        {
            throw new NotImplementedException();
        }
    }
}
