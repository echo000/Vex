﻿using DirectXTex;
using DirectXTexNet;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;

namespace Vex.Library.Utility
{
    public class ImageHelper
    {
        public static BitmapImage ConvertImage(byte[] array, int width, int height)
        {
            using var scratchImage = ConvertToFormat(array);
            PatchAlphaChannel(scratchImage);
            using var mem = scratchImage.SaveToWICMemory(0, WIC_FLAGS.NONE, TexHelper.Instance.GetWICCodec(WICCodecs.PNG));
            return MakeBitmapImage(mem, width, height);
        }

        public static UnmanagedMemoryStream ConvertImageToStream(byte[] array)
        {
            using var scratchImage = ConvertToFormat(array);
            PatchAlphaChannel(scratchImage);
            var mem = scratchImage.SaveToDDSMemory(0, DDS_FLAGS.NONE);
            return mem;
        }

        public static ScratchImage ConvertToFormat(byte[] array, ImagePatch Patch = ImagePatch.NoPatch)
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
                case ImagePatch.Normal_COD_NOG: PatchNormalCODFromNOG(scratchImage); break;
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
        public static byte[] AddDDSHeaderToBytes(byte[] tempBuffer, int ImageWidth, int ImageHeight, int MipLevels, int ImageDataFormat)
        {
            var metadata = DirectXTexUtility.GenerateMataData(ImageWidth, ImageHeight, MipLevels, ImageDataFormat
            switch
            {
                24 => DirectXTexUtility.DXGIFormat.R10G10B10A2UNORM,
                28 => DirectXTexUtility.DXGIFormat.R8G8B8A8UNORM,
                29 => DirectXTexUtility.DXGIFormat.R8G8B8A8UNORMSRGB,
                61 => DirectXTexUtility.DXGIFormat.R8UNORM,
                62 => DirectXTexUtility.DXGIFormat.R8UINT,
                71 => DirectXTexUtility.DXGIFormat.BC1UNORM,
                72 => DirectXTexUtility.DXGIFormat.BC1UNORMSRGB,
                74 => DirectXTexUtility.DXGIFormat.BC2UNORM,
                75 => DirectXTexUtility.DXGIFormat.BC2UNORMSRGB,
                77 => DirectXTexUtility.DXGIFormat.BC3UNORM,
                78 => DirectXTexUtility.DXGIFormat.BC3UNORMSRGB,
                80 => DirectXTexUtility.DXGIFormat.BC4UNORM,
                81 => DirectXTexUtility.DXGIFormat.BC4SNORM,
                83 => DirectXTexUtility.DXGIFormat.BC5UNORM,
                84 => DirectXTexUtility.DXGIFormat.BC5SNORM,
                95 => DirectXTexUtility.DXGIFormat.BC6HUF16,
                96 => DirectXTexUtility.DXGIFormat.BC6HSF16,
                98 => DirectXTexUtility.DXGIFormat.BC7UNORM,
                99 => DirectXTexUtility.DXGIFormat.BC7UNORMSRGB,
                _ => 0
            }, false);
            DirectXTexUtility.GenerateDDSHeader(metadata, DirectXTexUtility.DDSFlags.NONE, out var header, out var dx10h);
            var headerBuffer = DirectXTexUtility.EncodeDDSHeader(header, dx10h);

            var ImageBuffer = new byte[headerBuffer.Length + tempBuffer.Length];
            Array.Copy(headerBuffer, ImageBuffer, headerBuffer.Length);
            Array.Copy(tempBuffer, 0, ImageBuffer, headerBuffer.Length, tempBuffer.Length);

            return ImageBuffer;
        }

        public static void PatchNormalFromBumpmap(ScratchImage img)
        {
            for (int x = 0; x < img.GetImage(0).Width; x++)
            {
                for (int y = 0; y < img.GetImage(0).Height; y++)
                {
                    // Get Pixel
                    var pixel = GetPixelValue(img, 0, 0, 0, x, y);
                    var ResultRedValue = pixel.A;
                    var ResultGreenValue = pixel.G;
                    SetPixelValue(img, 0, 0, 0, x, y, new Color(ResultRedValue, ResultGreenValue, 255, 255));
                }
            }
        }

        public static void PatchNormalFromCompressed(ScratchImage img)
        {
            for (int x = 0; x < img.GetImage(0).Width; x++)
            {
                for (int y = 0; y < img.GetImage(0).Height; y++)
                {
                    // Get Pixel
                    var pixel = GetPixelValue(img, 0, 0, 0, x, y);

                    var nX = pixel.R / 255.0f;
                    nX = nX * 2.0f - 1;
                    var nY = pixel.G / 255.0f;
                    nY = nY * 2.0f - 1;
                    var nZ = 0.0f;
                    if (1 - nX * nX - nY * nY > 0) nZ = MathF.Sqrt(1 - nX * nX - nY * nY);

                    float ResultBlueVal = Math.Clamp(((nZ + 1) / 2.0f), 0, 1.0f);

                    SetPixelValue(img, 0, 0, 0, x, y, new Color(pixel.R, pixel.G, (byte)(ResultBlueVal * 255), pixel.A));
                }
            }
        }

        public static void PatchNormalCODFromNOG(ScratchImage img)
        {
            for (int x = 0; x < img.GetImage(0).Width; x++)
            {
                for (int y = 0; y < img.GetImage(0).Height; y++)
                {
                    // Get Pixel
                    var pixel = GetPixelValue(img, 0, 0, 0, x, y);

                    var RedValue = pixel.G;
                    var GreenValue = pixel.A;

                    var nX = RedValue / 255.0f;
                    nX = nX * 2.0f - 1;
                    var nY = GreenValue / 255.0f;
                    nY = nY * 2.0f - 1;
                    var nZ = 0.0f;
                    if (1 - nX * nX - nY * nY > 0) nZ = MathF.Sqrt(1 - nX * nX - nY * nY);

                    float ResultBlueVal = Math.Clamp(((nZ + 1) / 2.0f), 0, 1.0f);

                    SetPixelValue(img, 0, 0, 0, x, y, new Color(RedValue, GreenValue, (byte)ResultBlueVal, 255));
                }
            }
        }

        public static void PatchAlphaChannel(ScratchImage img)
        {
            for (int x = 0; x < img.GetImage(0).Width; x++)
            {
                for (int y = 0; y < img.GetImage(0).Height; y++)
                {
                    // Get Pixel
                    var pixel = GetPixelValue(img, 0, 0, 0, x, y);

                    SetPixelValue(img, 0, 0, 0, x, y, new Color(pixel.R, pixel.G, pixel.B, 255));
                }
            }
        }

        static unsafe Color GetPixelValue(ScratchImage img, int mip, int item, int slice, int x, int y)
        {
            var color = new Color();
            var image = img.GetImage(0);
            var pixelIndex = ((y * image.Width) + x) * 4;
            switch (image.Format)
            {
                case DXGI_FORMAT.R8G8B8A8_UNORM:
                    byte* pixelsB = (byte*)image.Pixels;
                    color.R = pixelsB[pixelIndex + 0];
                    color.G = pixelsB[pixelIndex + 1];
                    color.B = pixelsB[pixelIndex + 2];
                    color.A = pixelsB[pixelIndex + 3];
                    break;
                case DXGI_FORMAT.R16G16B16A16_UNORM:
                    ushort* pixelsS = (ushort*)image.Pixels;
                    color.R = (byte)(pixelsS[pixelIndex + 0] / 65535.0f);
                    color.G = (byte)(pixelsS[pixelIndex + 1] / 65535.0f);
                    color.B = (byte)(pixelsS[pixelIndex + 2] / 65535.0f);
                    color.A = (byte)(pixelsS[pixelIndex + 3] / 65535.0f);
                    break;
                case DXGI_FORMAT.R32G32B32A32_FLOAT:
                    float* pixels = (float*)image.Pixels;
                    color.R = (byte)pixels[pixelIndex + 0];
                    color.G = (byte)pixels[pixelIndex + 1];
                    color.B = (byte)pixels[pixelIndex + 2];
                    color.A = (byte)pixels[pixelIndex + 3];
                    break;
            }
            return color;
        }

        static unsafe void SetPixelValue(ScratchImage img, int mip, int item, int slice, int x, int y, Color color)
        {
            var image = img.GetImage(0);
            var pixelIndex = ((y * image.Width) + x) * 4;
            switch (image.Format)
            {
                case DXGI_FORMAT.R8G8B8A8_UNORM:
                    byte* pixelsB = (byte*)image.Pixels;
                    pixelsB[pixelIndex + 0] = color.R;
                    pixelsB[pixelIndex + 1] = color.G;
                    pixelsB[pixelIndex + 2] = color.B;
                    pixelsB[pixelIndex + 3] = color.A;
                    break;
            }
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
    }
}