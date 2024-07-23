using DirectXTexNet;
using PhilLibX.Media3D;
using System.IO;

namespace Vex.Library.Utility
{
    /// <summary>
    /// Mesh Data
    /// </summary>
    public class ExportManager
    {
        public static void ExportModel(Model Result, string Name, VexInstance instance)
        {
            if (Result == null)
                return;
            var dir = Path.Combine(instance.ExportFolder, instance.GetGameName(), "Models", Name);
            var ImagesPath = instance.Settings.GlobalImages ? Path.Combine(Path.GetDirectoryName(dir), "_Images") : Path.Combine(dir, "_Images");
            var ImageRelativePath = instance.Settings.GlobalImages ? "..\\\\_Images\\\\" : "_Images\\\\";
            Directory.CreateDirectory(dir);
            Directory.CreateDirectory(ImagesPath);
            foreach (var Material in Result.Materials)
            {
                ExportMaterialImages(Material, ImagesPath, instance);
                foreach (var texture in Material.Textures)
                {
                    texture.Value.Name = $"{ImageRelativePath}{texture.Value.Name}{instance.GetImageExportFormat()}";
                }
            }
            string format = "";
            switch ((MdlExportFormat)instance.Settings.ModelExportFormat)
            {
                case MdlExportFormat.XMODEL:
                    format = ".xmodel_export";
                    break;
                case MdlExportFormat.SEMODEL:
                    format = ".semodel";
                    break;
                case MdlExportFormat.CAST:
                    format = ".cast";
                    break;
            }
            instance.Translator.Save($"{dir}\\{Result.Name}{format}", Result);
        }


        public static void ExportMaterialImages(Material material, string ImagesPath, VexInstance instance)
        {
            foreach (var texture in material.Textures)
            {
                var Patch = ImagePatch.NoPatch;
                if (texture.Key == "NormalMap")
                    Patch = ImagePatch.Normal_Expand;
                var path = Path.Combine(ImagesPath, $"{texture.Value.Name}{instance.GetImageExportFormat()}");
                var ImageAsset = instance.VoidSupport.GetEntryFromName(texture.Value.FilePath);
                if (ImageAsset != null)
                {
                    var output = instance.VoidSupport.ExtractEntry(ImageAsset, instance);
                    var img = new BImage(output, Path.GetFileName(ImageAsset.Destination), instance);
                    ExportBImage(img, path, Patch, instance);
                }
            }
        }

        public static void ExportAnimation(Animation animation, string OutputFolder, string name, VexInstance instance)
        {
            var extension = "";
            switch ((AnimExportFormat)instance.Settings.AnimationExportFormat)
            {
                case AnimExportFormat.CAST:
                    extension = ".cast";
                    break;
                case AnimExportFormat.SEANIM:
                    extension = ".seanim";
                    break;
                case AnimExportFormat.XANIM:
                    extension = ".xanim_export";
                    break;
            }
            instance.Translator.Save($"{OutputFolder}\\{name}{extension}", animation);
        }

        public static void ExportImageFromByte(byte[] array, string OutputFile, ImagePatch Patch, VexInstance instance)
        {
            using var scratchImage = ImageHelper.ConvertToFormat(array, Patch);
            // Saving to a file
            switch ((ImgExportFormat)instance.Settings.ImageExportFormat)
            {
                case ImgExportFormat.TGA:
                    scratchImage.SaveToTGAFile(0, OutputFile);
                    break;
                case ImgExportFormat.TIFF:
                    scratchImage.SaveToWICFile(0, 1, WIC_FLAGS.NONE, TexHelper.Instance.GetWICCodec(WICCodecs.TIFF), OutputFile);
                    break;
                case ImgExportFormat.DDS:
                    scratchImage.SaveToDDSFile(DDS_FLAGS.NONE, OutputFile);
                    break;
                case ImgExportFormat.PNG:
                    scratchImage.SaveToWICFile(0, 1, WIC_FLAGS.NONE, TexHelper.Instance.GetWICCodec(WICCodecs.PNG), OutputFile);
                    break;
            }
        }

        public static void ExportBImage(BImage Image, string OutputFile, ImagePatch Patch, VexInstance instance)
        {
            using var scratchImage = ImageHelper.ConvertBImage(Image, Patch);
            switch ((ImgExportFormat)instance.Settings.ImageExportFormat)
            {
                case ImgExportFormat.TGA:
                    scratchImage.SaveToTGAFile(0, OutputFile);
                    break;
                case ImgExportFormat.TIFF:
                    scratchImage.SaveToWICFile(0, 1, WIC_FLAGS.NONE, TexHelper.Instance.GetWICCodec(WICCodecs.TIFF), OutputFile);
                    break;
                case ImgExportFormat.DDS:
                    scratchImage.SaveToDDSFile(DDS_FLAGS.NONE, OutputFile);
                    break;
                case ImgExportFormat.PNG:
                    scratchImage.SaveToWICFile(0, 1, WIC_FLAGS.NONE, TexHelper.Instance.GetWICCodec(WICCodecs.PNG), OutputFile);
                    break;
            }
        }
    }
}
