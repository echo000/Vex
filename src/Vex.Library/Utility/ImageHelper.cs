using DirectXTex;
using DirectXTexNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Vex.Library.Utility
{
    public class ImageHelper
    {
        public static BitmapImage ConvertImage(BImage Image, ImagePatch patch)
        {
            using var scratchImage = ConvertBImage(Image, patch);
            using var mem = scratchImage.SaveToWICMemory(0, WIC_FLAGS.NONE, TexHelper.Instance.GetWICCodec(WICCodecs.PNG));
            return MakeBitmapImage(mem, (int)Image.m_Opts.m_curWidth, (int)Image.m_Opts.m_curHeight);
        }

        public static UnmanagedMemoryStream ConvertImageToStream(BImage Image, ImagePatch patch = ImagePatch.NoPatch)
        {
            using var scratchImage = ConvertBImage(Image, patch);
            return scratchImage.SaveToDDSMemory(0, DDS_FLAGS.NONE);
        }

        //This is much faster than the above, however it doesn't support patching alpha
        //When patching the alpha, it first converts the image to RGBA
        //Then loops through each pixel and sets the alpha to 255 which can be slow
        public static MemoryStream ConvertImageForModel(BImage Image)
        {
            var bytes = Image.m_Slices[0].m_Content;
            if (Image.m_Opts.m_type == BImage.ImageOptions.TYPE.TT_2D)
            {
                if (Image.m_Slices.Length > 0)
                {
                    bytes = Stitch2DMips(Image);
                }
            }
            var ImageBuffer = AddDDSHeaderToBytes(bytes, (int)Image.m_Opts.m_curWidth, (int)Image.m_Opts.m_curHeight, (int)Image.m_Opts.m_curNumLevels, Image.m_Opts.m_format.GetDirectXFormat(), false);
            return new MemoryStream(ImageBuffer);
        }

        public static ScratchImage ConvertBImage(BImage Image, ImagePatch patch)
        {
            var bytes = Image.m_Slices[0].m_Content;
            bool IsCubemap = false;
            if (Image.m_Opts.m_type == BImage.ImageOptions.TYPE.TT_2D)
            {
                if (Image.m_Slices.Length > 0)
                {
                    bytes = Stitch2DMips(Image);
                }
            }
            else if (Image.m_Opts.m_type == BImage.ImageOptions.TYPE.TT_CUBIC)
            {
                if (Image.m_Slices.Length > 0)
                {
                    bytes = StitchCubemapMips(Image);
                    IsCubemap = true;
                }
            }
            else if (Image.m_Opts.m_type == BImage.ImageOptions.TYPE.TT_3D)
            {
                throw new Exception("3D textures are not yet supported!");
            }
            var ImageBuffer = AddDDSHeaderToBytes(bytes, (int)Image.m_Opts.m_curWidth, (int)Image.m_Opts.m_curHeight, (int)Image.m_Opts.m_curNumLevels, Image.m_Opts.m_format.GetDirectXFormat(), IsCubemap);

            return ConvertToFormat(ImageBuffer, patch);
        }

        static ScratchImage ConvertToFormat(byte[] array, ImagePatch Patch = ImagePatch.NoPatch)
        {
            //Load the ScratchImage from the byte array (need to pin the array)
            //The array pinning etc was taken from:
            //https://github.com/MontagueM/Charm/blob/3ea3e4ae4a3a588ba0c88e5354583a408c9cf58e/Tiger/Schema/Shaders/Texture.cs#L66
            GCHandle gcHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
            IntPtr pixelPtr = gcHandle.AddrOfPinnedObject();
            var scratchImage = TexHelper.Instance.LoadFromDDSMemory(pixelPtr, array.Length, DDS_FLAGS.NONE);
            gcHandle.Free();

            // Stage 1: Check if the image is planar, if so, convert to a single plane
            if (TexHelper.Instance.IsPlanar(scratchImage.GetMetadata().Format))
            {
                scratchImage = scratchImage.ConvertToSinglePlane();
            }
            // Stage 2: Decompress the texture if necessary, or ensure it's in the proper format
            if (TexHelper.Instance.IsCompressed(scratchImage.GetMetadata().Format))
            {
                scratchImage = scratchImage.Decompress(DXGI_FORMAT.R8G8B8A8_UNORM);
            }
            else if (scratchImage.GetMetadata().Format != DXGI_FORMAT.R8G8B8A8_UNORM)
            {
                scratchImage = scratchImage.Convert(DXGI_FORMAT.R8G8B8A8_UNORM, TEX_FILTER_FLAGS.DEFAULT, 0.5f);
            }
            // Stage 3: Patching, apply an image patch if need be
            switch (Patch)
            {
                case ImagePatch.Normal_Bumpmap: PatchNormalFromBumpmap(scratchImage); break;
                case ImagePatch.Normal_Expand: PatchNormalFromCompressed(scratchImage); break;
                case ImagePatch.Unpack_Packed:  break;
                case ImagePatch.Color_StripAlpha: PatchAlphaChannel(scratchImage); break;
            }

            return scratchImage;
        }

        /// <summary>
        /// Adds the DDS header to the provided byte array and returns the result
        /// </summary>
        /// <param name="tempBuffer">byte array input</param>
        /// <param name="ImageWidth">The image width</param>
        /// <param name="ImageHeight">The image height</param>
        /// <param name="MipLevels">Number of Mips</param>
        /// <param name="ImageDataFormat">The format of the image</param>
        /// <returns></returns>
        static byte[] AddDDSHeaderToBytes(byte[] tempBuffer, int ImageWidth, int ImageHeight, int MipLevels, DirectXTexUtility.DXGIFormat ImageDataFormat, bool IsCubemap = false)
        {
            var metadata = DirectXTexUtility.GenerateMataData(ImageWidth, ImageHeight, MipLevels, ImageDataFormat, IsCubemap);
            DirectXTexUtility.GenerateDDSHeader(metadata, DirectXTexUtility.DDSFlags.NONE, out var header, out var dx10h);
            var headerBuffer = DirectXTexUtility.EncodeDDSHeader(header, dx10h);

            var ImageBuffer = new byte[headerBuffer.Length + tempBuffer.Length];
            Array.Copy(headerBuffer, ImageBuffer, headerBuffer.Length);
            Array.Copy(tempBuffer, 0, ImageBuffer, headerBuffer.Length, tempBuffer.Length);

            return ImageBuffer;
        }

        static unsafe void PatchNormalFromBumpmap(ScratchImage img)
        {
            var image = img.GetImage(0);
            var width = image.Width;
            var height = image.Height;

            byte* pixelData = (byte*)image.Pixels;
            int rowLength = width * 4; // number of bytes in a row (assuming 4 bytes per pixel)

            Parallel.For(0, height, y =>
            {
                byte* row = pixelData + (y * rowLength);
                for (int x = 0; x < rowLength; x += 4)
                {
                    row[x] = row[x + 3];
                    row[x + 2] = 255;
                    row[x + 3] = 255;
                }
            });
        }

        static unsafe void PatchNormalFromCompressed(ScratchImage img)
        {
            var image = img.GetImage(0);
            var width = image.Width;
            var height = image.Height;

            byte* pixelData = (byte*)image.Pixels;
            int rowLength = width * 4; // number of bytes in a row (assuming 4 bytes per pixel)

            Parallel.For(0, height, y =>
            {
                byte* row = pixelData + (y * rowLength);
                for (int x = 0; x < rowLength; x += 4)
                {
                    var nX = row[x] / 255.0f;
                    nX = nX * 2.0f - 1;
                    var nY = row[x + 1] / 255.0f;
                    nY = nY * 2.0f - 1;
                    var nZ = 0.0f;

                    if (1 - nX * nX - nY * nY > 0) nZ = MathF.Sqrt(1 - nX * nX - nY * nY);
                    float ResultBlueVal = Math.Clamp(((nZ + 1) / 2.0f), 0, 1.0f);
                    row[x + 2] = (byte)(ResultBlueVal * 255);
                    row[x + 3] = 255;
                }
            });
        }

        static unsafe void PatchAlphaChannel(ScratchImage img)
        {
            var image = img.GetImage(0);
            var width = image.Width;
            var height = image.Height;

            byte* pixelData = (byte*)image.Pixels;
            int rowLength = width * 4; // number of bytes in a row (assuming 4 bytes per pixel)

            Parallel.For(0, height, y =>
            {
                byte* row = pixelData + (y * rowLength);
                for (int x = 0; x < rowLength; x += 4)
                {
                    row[x + 3] = 255; // set alpha channel
                }
            });
        }

        private static BitmapImage MakeBitmapImage(UnmanagedMemoryStream ms, int width, int height)
        {
            BitmapImage bitmapImage = new();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = ms;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.DecodePixelWidth = width;
            bitmapImage.DecodePixelHeight = height;
            bitmapImage.EndInit();
            bitmapImage.Freeze();
            return bitmapImage;
        }

        static byte[] Stitch2DMips(BImage image)
        {
            if (image.m_Opts.m_type != BImage.ImageOptions.TYPE.TT_2D)
                throw new Exception("The image is not a 2D texture!");

            var result = new List<byte>();
            uint ArraySize = image.m_Opts.m_arraySize;
            uint MipsCount = image.m_Opts.m_curNumLevels;
            for (uint SliceIndex = 0; SliceIndex < ArraySize; SliceIndex++)
            {
                for (uint MipLevelIndex = 0; MipLevelIndex < MipsCount; MipLevelIndex++)
                {
                    var Slice = image.m_Slices[MipLevelIndex * ArraySize + SliceIndex];

                    result.AddRange(Slice.m_Content);
                }
            }
            return [.. result];
        }

        static byte[] StitchCubemapMips(BImage image)
        {
            if (image.m_Opts.m_type != BImage.ImageOptions.TYPE.TT_CUBIC)
                throw new Exception("The image is not a 2D texture!");

            var result = new List<byte>();
            uint ArraySize = 6 * image.m_Opts.m_arraySize;
            uint MipsCount = image.m_Opts.m_curNumLevels;
            for (uint SliceIndex = 0; SliceIndex < ArraySize; SliceIndex++)
            {
                for (uint MipLevelIndex = 0; MipLevelIndex < MipsCount; MipLevelIndex++)
                {
                    var Slice = image.m_Slices[MipLevelIndex * ArraySize + SliceIndex];
                    result.AddRange(Slice.m_Content);
                }
            }
            return [.. result];
        }
    }
}
