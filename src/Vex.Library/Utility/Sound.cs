using PhilLibX.Audio;
using System.IO;

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

        public static bool ExportSoundAsset(XSound Sound, string SoundPath, string AssetName, VexInstance instance)
        {
            var AudioExportFormat = (SoundExportFormat)instance.Settings.AudioExportFormat;
            Directory.CreateDirectory(SoundPath);

            var OutFormat = AudioExportFormat switch
            {
                SoundExportFormat.WAV => SoundFormat.Standard_WAV,
                SoundExportFormat.FLAC => SoundFormat.Standard_FLAC,
                _ => SoundFormat.Standard_WAV,
            };

            var FileName = Path.Combine(SoundPath, AssetName + instance.GetAudioExportFormat());

            if ((AudioExportFormat == SoundExportFormat.WAV && Sound.DataType == SoundDataTypes.WAV_WithHeader) || (AudioExportFormat == SoundExportFormat.FLAC && Sound.DataType == SoundDataTypes.FLAC_WithHeader))
            {
                File.WriteAllBytes(FileName, Sound.DataBuffer);
            }
            else
            {
                var InFormat = (Sound.DataType == SoundDataTypes.FLAC_WithHeader) ? SoundFormat.FLAC_WithHeader : SoundFormat.WAV_WithHeader;
                //CONVERT
                return ConvertSoundMemory(Sound.DataBuffer, Sound.DataSize, InFormat, FileName, OutFormat);
            }

            return false;
        }
    }
}
