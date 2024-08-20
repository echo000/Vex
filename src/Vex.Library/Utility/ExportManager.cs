using DirectXTexNet;
using PhilLibX.Media3D;
using System;
using System.IO;
using System.Threading.Tasks;

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

            string format = (MdlExportFormat)instance.Settings.ModelExportFormat switch
            {
                MdlExportFormat.SEMODEL => ".semodel",
                MdlExportFormat.XMODEL => ".xmodel_export",
                MdlExportFormat.CAST => ".cast",
                _ => throw new Exception("Invalid export format")
            };
            instance.Translator.Save($"{dir}\\{Result.Name}{format}", Result);
        }

        public static void ExportMaterialImages(Material material, string ImagesPath, VexInstance instance)
        {
            Parallel.ForEach(material.Textures, (texture) =>
            {
                var Patch = texture.Key == "NormalMap" ? ImagePatch.Normal_Expand : ImagePatch.NoPatch;
                var path = Path.Combine(ImagesPath, $"{texture.Value.Name}{instance.GetImageExportFormat()}");
                var ImageAsset = instance.VoidSupport.GetEntryFromName(texture.Value.FilePath);
                if (ImageAsset != null)
                {
                    var output = instance.VoidSupport.ExtractEntryBytes(ImageAsset, instance);
                    var img = new BImage(output, Path.GetFileName(ImageAsset.Destination), instance);
                    ExportBImage(img, path, Patch, instance);
                }
            });
        }

        public static void ExportAnimation(Animation animation, string OutputFolder, string name, VexInstance instance)
        {
            string format = (AnimExportFormat)instance.Settings.AnimationExportFormat switch
            {
                AnimExportFormat.CAST => ".cast",
                AnimExportFormat.SEANIM => ".seanim",
                AnimExportFormat.XANIM => ".xanim_export",
                _ => throw new Exception("Invalid export format")
            };
            instance.Translator.Save($"{OutputFolder}\\{name}{format}", animation);
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
