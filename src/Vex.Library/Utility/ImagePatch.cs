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
        // Convert a Deathloop packed map into AO, Roughness, and Metalness maps
        Packed_Unpack,

        // -- Color map patches

        // Removes the alpha channel from the colormap
        Color_StripAlpha
    };
}
