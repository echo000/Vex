using PhilLibX;
using PhilLibX.Compression;
using PhilLibX.IO;
using PhilLibX.Media3D;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Media;
using Vex.Library.Utility;

namespace Vex.Library.Package
{
    public class VoidSupport
    {
        //These has to be a better way to do this
        List<VoidContainer> Containers;

        public List<Asset> VoidMasterIndex(string FilePath, VexInstance instance)
        {
            Containers = [];
            using var Stream = File.OpenRead(FilePath);
            using var Reader = new BinaryReader(Stream);
            try
            {
                var Header = new VoidResourceHeader
                {
                    Magic = Reader.ReadBEInt32(),
                    Version = Reader.ReadBEInt16()
                };

                //First byte is type of file (0x04 for index, 0x05 for resource)
                if (Header.Magic != 0x04534552)
                    throw new Exception();

                //Dishonored 2
                if (Header.Version == 0x2)
                {
                    instance.Game = SupportedGames.Dishonored2;
                    var IndexCount = Reader.ReadBEInt16();
                    for (int i = 0; i < IndexCount; i++)
                    {
                        var IndexName = Reader.ReadFixedPrefixString();
                        var ResourceName = Reader.ReadFixedPrefixString();
                        var Container = new VoidContainer { Directory = Path.GetDirectoryName(FilePath), Path = IndexName };
                        Container.Resources.Add(ResourceName);
                        Containers.Add(Container);
                    }
                    var ResourceCount = Reader.ReadBEInt32();
                    for (int i = 0; i < ResourceCount; i++)
                    {
                        var Name = Reader.ReadFixedPrefixString();
                        var index = Reader.ReadBEInt16();
                        Containers[index].Resources.Add(Name);
                    }
                    var RscCount = Reader.ReadBEInt32();
                    for (int i = 0; i < RscCount; i++)
                    {
                        var sharedIndexCount = Reader.ReadBEInt32();
                        var sharedIndices = new List<short>();
                        for (int j = 0; j < sharedIndexCount; j++)
                        {
                            sharedIndices.Add(Reader.ReadBEInt16());
                        }
                        var Name = Reader.ReadFixedPrefixString();
                        foreach (var index in sharedIndices)
                        {
                            Containers[index].Resources.Add(Name);
                        }
                    }
                    return ParseVoidResources(instance);
                }
                //Deathloop
                else if (Header.Version == 0x3)
                {
                    instance.Game = SupportedGames.Deathloop;
                    var BaseDirectory = Path.GetDirectoryName(FilePath);
                    GetOodleDLL(BaseDirectory);
                    var IndexName = Reader.ReadFixedPrefixString();
                    var Container = new VoidContainer { Directory = BaseDirectory, Path = IndexName };
                    Containers.Add(Container);
                    var ResourceCount = Reader.ReadBEInt16();
                    for (int i = 0; i < ResourceCount; i++)
                    {
                        var Name = Reader.ReadFixedPrefixString();
                        Containers[0].Resources.Add(Name);
                    }
                    return ParseVoidResources(instance);
                }
            }
            finally
            {
                Reader.Close();
                Stream.Close();
            }

            return null;
        }

        public List<Asset> ParseVoidResources(VexInstance instance)
        {
            var Assets = new List<Asset>();
            var uniqueElements = new HashSet<string>();
            for (var ci = 0; ci < Containers.Count; ci++)
            {
                var container = Containers[ci];
                using var Stream = File.OpenRead(Path.Combine(container.Directory, container.Path));
                using var Reader = new BinaryReader(Stream);
                try
                {
                    var Magic = Reader.ReadBEInt32();
                    if (Magic != 0x05534552)
                        throw new Exception();
                    Reader.Advance(28);

                    var EntryCount = Reader.ReadBEInt32();
                    for (int i = 0; i < EntryCount; i++)
                    {
                        var Asset = new Asset();
                        switch (instance.Game)
                        {
                            case SupportedGames.Dishonored2:
                                Asset.Container = ci;
                                Asset.Id = Reader.ReadBEUInt32();
                                Asset.EntryType = Reader.ReadFixedPrefixString();
                                Asset.Name = Reader.ReadFixedPrefixString();
                                Asset.Destination = Reader.ReadFixedPrefixString();
                                Asset.AssetPointer = (long)Reader.ReadBEUInt64();
                                Asset.AssetSize = Reader.ReadBEInt32();
                                Asset.CompressedSize = Reader.ReadBEInt32();
                                Asset.Dummy = Reader.ReadBEInt32();
                                Asset.Unk = Reader.ReadBEInt32();
                                Asset.Flag2 = Reader.ReadBEInt16();
                                break;
                            case SupportedGames.Deathloop:
                                Asset.Container = ci;
                                Asset.Id = Reader.ReadUInt32();
                                Asset.EntryType = Reader.ReadFixedPrefixString();
                                Asset.Name = Reader.ReadFixedPrefixString();
                                Asset.Destination = Reader.ReadFixedPrefixString();
                                Asset.AssetPointer = (long)Reader.ReadUInt64();
                                Asset.AssetSize = Reader.ReadInt32();
                                Asset.CompressedSize = Reader.ReadInt32();
                                Asset.Dummy = Reader.ReadInt32();
                                Asset.Unk = Reader.ReadInt32();
                                Asset.Flag3 = Reader.ReadInt32();
                                Asset.Flag2 = Reader.ReadInt16();
                                break;
                        }

                        if ((Asset.EntryType == "baseModel" || Asset.EntryType == "model") && instance.Settings.LoadModels)
                        {
                            Asset.Status = AssetStatus.Loaded;
                            Asset.Type = AssetType.Model;
                            Asset.InformationString = $"{Asset.EntryType}";
                            Assets.Add(Asset);
                        }
                        if (Asset.EntryType == "skeleton" && instance.Settings.LoadRawFiles)
                        {
                            Asset.Status = AssetStatus.Loaded;
                            Asset.Type = AssetType.RawFile;
                            Asset.InformationString = $"{Asset.EntryType}";
                            Assets.Add(Asset);
                        }
                        if (Asset.EntryType == "image" /*&& !Entry.DisplayName.Contains("_mip")*/ && instance.Settings.LoadImages)
                        {
                            Asset.Status = AssetStatus.Loaded;
                            Asset.Type = AssetType.Image;
                            Asset.InformationString = $"{Asset.EntryType}";
                            Assets.Add(Asset);
                        }
                        if (Asset.EntryType == "anim" && instance.Settings.LoadAnimations)
                        {
                            Asset.Status = AssetStatus.Loaded;
                            Asset.Type = AssetType.Animation;
                            Asset.InformationString = $"{Asset.EntryType}";
                            Assets.Add(Asset);
                        }
                        if (Asset.EntryType == "material" && instance.Settings.LoadMaterials)
                        {
                            Asset.Status = AssetStatus.Loaded;
                            Asset.Type = AssetType.Material;
                            Asset.InformationString = $"{Asset.EntryType}";
                            Assets.Add(Asset);
                        }
                        //This adds all entries to the container so can take up a lot of space,
                        //I think we only need required types
                        //model, material, image, anim + their required types - Skeleton, image
                        //Then can switch what is shown in the UI based on user settings
                        container.Entries.Add(Asset);
                    }
                }
                finally
                {
                    Reader.Close();
                    Stream.Close();
                }
            }
            return Assets;
        }

        public void ExportVoidModel(Asset asset, VexInstance instance)
        {
            var model = BuildVoidModel(asset, instance);
            var modelName = Path.GetFileNameWithoutExtension(asset.Destination);
            model.Name = modelName;
            ExportManager.ExportModel(model, modelName, instance);
        }

        public void ExportAllAssetBytes(Asset asset, VexInstance instance)
        {
            var output = ExtractEntry(asset, instance);
            var dir = Path.Combine(instance.Settings.ExportDirectory, instance.GetGameName(), asset.EntryType, Path.GetDirectoryName(asset.Destination));
            Directory.CreateDirectory(dir);
            var AssetPath = Path.Combine(instance.Settings.ExportDirectory, instance.GetGameName(), asset.EntryType, asset.Destination);
            File.WriteAllBytes(AssetPath, output);
        }

        public void ExportVoidImage(Asset asset, VexInstance instance)
        {
            var output = ExtractEntry(asset, instance);
            var img = new BImage(output, Path.GetFileName(asset.Destination), instance);
            var dir = Path.Combine(instance.ExportFolder, instance.GetGameName(), "Images");
            Directory.CreateDirectory(dir);
            ExportManager.ExportBImage(img, Path.Combine(dir, Path.GetFileNameWithoutExtension(asset.DisplayName) + instance.GetImageExportFormat()), ImagePatch.NoPatch, instance);
        }

        public void ExportMaterialAsset(Asset asset, VexInstance instance)
        {
            var Material = MaterialHelper.GetMaterialFromAsset(asset, instance);
            var dir = Path.Combine(instance.ExportFolder, instance.GetGameName(), "Materials", Path.GetFileNameWithoutExtension(asset.DisplayName));
            Directory.CreateDirectory(dir);
            ExportManager.ExportMaterialImages(Material, dir, instance);
        }

        public ImageSource BuildPreviewImage(Asset asset, VexInstance instance)
        {
            if(asset.AssetSize == 0)
            {
                return null;
            }
            var output = ExtractEntry(asset, instance);
            var img = new BImage(output, Path.GetFileName(asset.Destination), instance);
            ImagePatch patch = ImagePatch.NoPatch;
            if (Path.GetFileNameWithoutExtension(asset.DisplayName).EndsWith("_n") && instance.Settings.PatchNormals)
            {
                patch = ImagePatch.Normal_Expand;
            }
            var Result = ImageHelper.ConvertImage(img, patch);
            Trace.WriteLine(img);
            return Result;
        }

        public BImage GetBImageFromAsset(Asset asset, VexInstance instance)
        {
            if (asset.AssetSize == 0)
            {
                return null;
            }
            var output = ExtractEntry(asset, instance);
            var img = new BImage(output, Path.GetFileName(asset.Destination), instance);
            return img;
        }

        public Model BuildVoidModel(Asset asset, VexInstance instance)
        {
            var output = ExtractEntry(asset, instance);
            var model = instance.Game == SupportedGames.Dishonored2 ? ModelHelper.BuildDishonoredPreviewModel(output, instance, out string SkeletonPath) : ModelHelper.BuildDeathloopPreviewModel(output, instance, out SkeletonPath);
            if (!string.IsNullOrWhiteSpace(SkeletonPath))
            {
                var SkeletonEntry = Containers.SelectMany(c => c.Entries)
                    .FirstOrDefault(e => e.Name == SkeletonPath);
                if (SkeletonEntry != null)
                {
                    var SkeletonBytes = ExtractEntry(SkeletonEntry, instance);
                    //Improve this
                    var skeleton = ModelHelper.BuildVoidSkeleton(SkeletonBytes, instance.Game == SupportedGames.Deathloop);
                    model.Skeleton = skeleton;
                }
            }
            model.Scale(100);
            return model;
        }

        public static void GetOodleDLL(string BaseDirectory)
        {
            var OurPath = Path.Combine(AppContext.BaseDirectory, "oo2core_8_win64.dll");
            if (!File.Exists(OurPath))
            {
                var oodlePath = Path.Combine(Path.GetDirectoryName(BaseDirectory), "oo2core_8_win64.dll");
                if (File.Exists(oodlePath))
                {
                    File.Copy(oodlePath, OurPath);
                }
            }
            NativeMethods.SetOodleLibrary("oo2core_8_win64.dll");
        }

        public byte[] ExtractEntry(Asset Asset, VexInstance instance)
        {
            byte[] output;
            var container = Containers[Asset.Container];
            var Resource = instance.Game == SupportedGames.Dishonored2 ? container.ResourcePath((ushort)Asset.Flag2) : container.Resources[Asset.Flag2];
            using var Stream = File.OpenRead(Path.Combine(container.Directory, Resource));
            using var Reader = new BinaryReader(Stream);
            try
            {
                Reader.Seek(Asset.AssetPointer);
                byte[] ReadData;
                var oodle = Reader.ReadFixedString(4);
                if (oodle == "OOD")
                {
                    var dummy = Reader.ReadInt32();
                    //This is just the (uncompressed) size of the data
                    var xsize = Reader.ReadInt32();
                    var bytesToRead = Asset.CompressedSize - 12;
                    ReadData = Reader.ReadBytes(bytesToRead);
                    output = Oodle.Decompress(ReadData, (int)Asset.AssetSize);
                }
                else
                {
                    //Go back 4 bytes if not OOD
                    Reader.BaseStream.Position -= 4;
                    ReadData = Reader.ReadBytes(Asset.CompressedSize);
                    if (Asset.AssetSize != Asset.CompressedSize)
                    {
                        output = ZLIB.Decompress(ReadData, (int)Asset.AssetSize);
                    }
                    else
                    {
                        output = ReadData;
                    }
                }
                return output;
            }
            finally
            {
                Stream.Close();
                Reader.Close();
            }
        }

        public List<Asset> GetEntriesFromName(string match)
        {
            var ImageEntries = Containers.SelectMany(c => c.Entries)
                .Where(e => e.Name.Contains(match, StringComparison.CurrentCultureIgnoreCase)).ToList();
            return ImageEntries;
        }

        public Asset GetEntryFromName(string match)
        {
            var ImageEntry = Containers.SelectMany(c => c.Entries)
                .FirstOrDefault(e => e.Name.Contains(match, StringComparison.CurrentCultureIgnoreCase));
            return ImageEntry;
        }

        public void ExportEntry(Asset asset, VexInstance instance)
        {
            switch(asset.Type)
            {
                case AssetType.Image:
                    ExportVoidImage(asset, instance);
                    break;
                case AssetType.Model:
                    ExportVoidModel(asset, instance);
                    break;
                case AssetType.Material:
                    ExportMaterialAsset(asset, instance);
                    break;
                case AssetType.RawFile:
                case AssetType.Animation:
                    ExportAllAssetBytes(asset, instance);
                    break;
            }
        }

        public void Clear()
        {
            Containers.Clear();
            Containers = null;
        }
    }
}
