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
        public static void ExportModel(XModel_t GenericModel, string Name, VexInstance instance)
        {
            if (GenericModel == null)
                return;

            var dir = Path.Combine(instance.ExportFolder, "Models", Name);
            var ImagesPath = instance.Settings.GlobalImages ? Path.Combine(Path.GetDirectoryName(dir), "_Images") : Path.Combine(dir, "_Images");
            var ImageRelativePath = instance.Settings.GlobalImages ? "..\\\\_Images\\\\" : "_Images\\\\";
            Directory.CreateDirectory(dir);
            Directory.CreateDirectory(ImagesPath);
            foreach (var LOD in GenericModel.ModelLods)
            {
                foreach (var material in LOD.Materials)
                {
                    var CompleteImagesPath = ImagesPath;
                    var CompleteImageRelativePath = ImageRelativePath;
                    ExportMaterialImages(material, CompleteImagesPath, instance);

                    foreach (var image in material.Images)
                    {
                        image.ImageName = CompleteImageRelativePath + image.ImageName + instance.GetImageExportFormat();
                    }
                }
            }

            if (instance.Settings.ExportLods)
            {
                var LodCount = GenericModel.ModelLods.Count;

                for (int i = 0; i < LodCount; i++)
                {
                    var Result = new Model();

                    if (Result != null)
                    {
                        Result.Name += $"_LOD{i}";
                        string format = "";
                        switch ((MdlExportFormat)instance.Settings.ModelExportFormat)
                        {
                            case MdlExportFormat.XMODEL:
                                format = ".xmodel_export";
                                break;
                            case MdlExportFormat.SEMODEL:
                                format = ".semodel";
                                Result.Scale(2.54f);
                                break;
                            case MdlExportFormat.CAST:
                                format = ".cast";
                                Result.Scale(2.54f);
                                break;
                        }
                        instance.Translator.Save($"{dir}\\{Result.Name}{format}", Result);
                    }
                }
            }
        }

        public static void ExportMaterialImages(XMaterial_t material, string ImagesPath, VexInstance instance)
        {

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
    }
}
