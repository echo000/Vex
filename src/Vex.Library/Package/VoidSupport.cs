using PhilLibX;
using PhilLibX.Compression;
using PhilLibX.IO;
using PhilLibX.Media3D;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Vex.Library.Utility;

namespace Vex.Library.Package
{
    public class VoidSupport
    {
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
                        var Entry = new D2Entry();
                        switch (instance.Game)
                        {
                            case SupportedGames.Dishonored2:
                                Entry.AssetPointer = Reader.BaseStream.Position;
                                Entry.Container = ci;
                                Entry.Id = Reader.ReadBEUInt32();
                                Entry.EntryType = Reader.ReadFixedPrefixString();
                                Entry.Name = Reader.ReadFixedPrefixString();
                                Entry.Destination = Reader.ReadFixedPrefixString();
                                Entry.ResourcePosition = Reader.ReadBEUInt64();
                                Entry.AssetSize = Reader.ReadBEInt32();
                                Entry.CompressedSize = Reader.ReadBEInt32();
                                Entry.dummy = Reader.ReadBEInt32();
                                Entry.unk = Reader.ReadBEInt32();
                                Entry.flag2 = Reader.ReadBEInt16();
                                break;
                            case SupportedGames.Deathloop:
                                Entry.AssetPointer = Reader.BaseStream.Position;
                                Entry.Container = ci;
                                Entry.Id = Reader.ReadUInt32();
                                Entry.EntryType = Reader.ReadFixedPrefixString();
                                Entry.Name = Reader.ReadFixedPrefixString();
                                Entry.Destination = Reader.ReadFixedPrefixString();
                                Entry.ResourcePosition = Reader.ReadUInt64();
                                Entry.AssetSize = Reader.ReadInt32();
                                Entry.CompressedSize = Reader.ReadInt32();
                                Entry.dummy = Reader.ReadInt32();
                                Entry.unk = Reader.ReadInt32();
                                Entry.flag3 = Reader.ReadInt32();
                                Entry.flag2 = Reader.ReadInt16();
                                break;
                        }

                        if (Entry.EntryType == "baseModel" || Entry.EntryType == "model")
                        {
                            Entry.Status = AssetStatus.Loaded;
                            Entry.Type = AssetType.Model;
                            Entry.LoadMethod = ExportVoidModel;
                            Entry.BuildPreviewMethod = BuildVoidModel;
                            Entry.InformationString = $"{Entry.EntryType}";
                            Assets.Add(Entry);
                        }
                        /*                    if (Entry.EntryType == "skeleton")
                                            {
                                                Entry.Status = AssetStatus.Loaded;
                                                Entry.Type = AssetType.RawFile;
                                                Entry.LoadMethod = ExportAllAssetBytes;
                                                Entry.InformationString = $"{Entry.EntryType}";
                                                Assets.Add(Entry);
                                            }*/
                        /*                    if(Entry.EntryType == "image")
                                            {
                                                Entry.Status = AssetStatus.Loaded;
                                                Entry.Type = AssetType.Image;
                                                Entry.LoadMethod = ExportAllBytesDishonored;
                                                Entry.InformationString = $"{Entry.EntryType}";
                                                Assets.Add(Entry);
                                            }*/
                        /*                    if(Entry.EntryType == "anim")
                                            {
                                                Entry.Status = AssetStatus.Loaded;
                                                Entry.Type = AssetType.Animation;
                                                Entry.LoadMethod = ExportAllBytesDishonored;
                                                Entry.InformationString = $"{Entry.EntryType}";
                                                Assets.Add(Entry);
                                            }*/
                        container.Entries.Add(Entry);
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
            var Entry = asset as D2Entry;
            var model = BuildVoidModel(asset, instance);
            var modelName = Path.GetFileNameWithoutExtension(Entry.Destination);
            model.Name = modelName;
            ExportManager.ExportModel(model, modelName, instance);
        }

        public void ExportAllAssetBytes(Asset asset, VexInstance instance)
        {
            var Entry = asset as D2Entry;
            var output = ExtractEntry(Entry, instance);
            var dir = Path.Combine(instance.Settings.ExportDirectory, instance.GetGameName(), Entry.EntryType, Path.GetDirectoryName(Entry.Destination));
            Directory.CreateDirectory(dir);
            var AssetPath = Path.Combine(instance.Settings.ExportDirectory, instance.GetGameName(), Entry.EntryType, Entry.Destination);
            File.WriteAllBytes(AssetPath, output);
        }

        public Model BuildVoidModel(Asset asset, VexInstance instance)
        {
            var Entry = asset as D2Entry;
            var output = ExtractEntry(Entry, instance);
            var model = instance.Game == SupportedGames.Dishonored2 ? ModelHelper.BuildDishonoredPreviewModel(output, out string SkeletonPath) : ModelHelper.BuildDeathloopPreviewModel(output, out SkeletonPath);
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

        public byte[] ExtractEntry(D2Entry Entry, VexInstance instance)
        {
            byte[] output = new byte[Entry.AssetSize];
            var container = Containers[Entry.Container];
            var Resource = instance.Game == SupportedGames.Dishonored2 ? container.ResourcePath((ushort)Entry.flag2) : container.Resources[Entry.flag2];
            using var Stream = File.OpenRead(Path.Combine(container.Directory, Resource));
            using var Reader = new BinaryReader(Stream);
            try
            {
                Reader.Seek((long)Entry.ResourcePosition);
                byte[] ReadData;
                var oodle = Reader.ReadFixedString(4);
                if (oodle == "OOD")
                {
                    var dummy = Reader.ReadInt32();
                    var xsize = Reader.ReadInt32();
                    var bytesToRead = Entry.CompressedSize - 12;
                    ReadData = Reader.ReadBytes(bytesToRead);
                    output = Oodle.Decompress(ReadData, (int)Entry.AssetSize);
                }
                else
                {
                    //Go back 4 bytes if not OOD
                    Reader.BaseStream.Position -= 4;
                    ReadData = Reader.ReadBytes(Entry.CompressedSize);
                    if (Entry.AssetSize != Entry.CompressedSize)
                    {
                        output = ZLIB.Decompress(ReadData, (int)Entry.AssetSize);
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

        public void Clear()
        {
            Containers.Clear();
            Containers = null;
        }
    }
}
