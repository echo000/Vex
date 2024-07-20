namespace Vex.Library.Utility
{
    // A list of supported image patch functions, applied on export
    public enum ImagePatch
    {
        // -- Default

        // Do nothing to the export image
        NoPatch,

        // -- Normal map patches

        // Convert a gray-scale bumpmap to a regular normalmap
        Normal_Bumpmap,
        // Convert a yellow-scale, compressed normalmap to a regular normalmap
        Normal_Expand,
        // Convert a normal, gloss, and occlusion map to a regular normalmap (Call of Duty: Infinite Warfare)
        Normal_COD_NOG,

        // -- Color map patches

        // Removes the alpha channel from the colormap
        Color_StripAlpha
    };

    internal class ImageTranslator
    {

    }
}
