namespace Vex.Library
{
    public enum ImgExportFormat : byte
    {
        DDS = 0,
        PNG = 1,
        TIFF = 2,
        TGA = 3
    }

    public enum MdlExportFormat : byte
    {
        SEMODEL,
        XMODEL,
        CAST,
    }

    public enum SoundExportFormat : byte
    {
        WAV,
        FLAC
    }

    public enum AnimExportFormat : byte
    {
        CAST,
        SEANIM,
        XANIM
    }
}
