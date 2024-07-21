using PhilLibX;
using PhilLibX.Compression;
using PhilLibX.IO;
using PhilLibX.Media3D;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Vex.Library.Utility;

namespace Vex.Library.Package
{
    public class VoidSupport
    {
        List<VoidContainer> Containers;

        public List<Asset> VoidMasterIndex(string FilePath)
        {
            Containers = [];
            using var stream = File.OpenRead(FilePath);
            using var Reader = new BinaryReader(stream);
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
                var IndexCount = Reader.ReadBEInt16();
                for (int i = 0; i < IndexCount; i++)
                {
                    var IndexName = Reader.ReadFixedPrefixString();
                    var ResourceName = Reader.ReadFixedPrefixString();
                    var Container = new VoidContainer { Directory = Path.GetDirectoryName(FilePath), Path = IndexName };
                    Container.Resources.Add(ResourceName);
                    Containers.Add(Container);
                    Trace.WriteLine($"Index: {IndexName}, Resource: {ResourceName}");
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
                return ParseDishonoredResources();
            }
            //Deathloop
            else if (Header.Version == 0x3)
            {
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
                Trace.WriteLine($"Index: {Containers[0].Path}, Resources: {Containers[0].Resources.Count}");
                return ParseDeathloopResources();
            }

            return null;
        }

        public List<Asset> ParseDishonoredResources()
        {
            var Assets = new List<Asset>();
            for (var ci = 0; ci < Containers.Count; ci++)
            {
                var container = Containers[ci];
                using var stream = File.OpenRead(Path.Combine(container.Directory, container.Path));
                using var Reader = new BinaryReader(stream);
                var Magic = Reader.ReadBEInt32();
                if (Magic != 0x05534552)
                    throw new Exception();
                Reader.Advance(28);

                var EntryCount = Reader.ReadBEInt32();
                for (int i = 0; i < EntryCount; i++)
                {
                    var Entry = new D2Entry
                    {
                        AssetPointer = Reader.BaseStream.Position,
                        Container = ci,
                        Id = Reader.ReadBEUInt32(),
                        EntryType = Reader.ReadFixedPrefixString(),
                        Name = Reader.ReadFixedPrefixString(),
                        Destination = Reader.ReadFixedPrefixString(),
                        ResourcePosition = Reader.ReadBEUInt64(),
                        AssetSize = Reader.ReadBEInt32(),
                        CompressedSize = Reader.ReadBEInt32(),
                        dummy = Reader.ReadBEInt32(),
                        unk = Reader.ReadBEInt32(),
                        flag2 = Reader.ReadBEInt16()
                    };

                    if (Entry.EntryType == "baseModel" || Entry.EntryType == "model")
                    {
                        Entry.Status = AssetStatus.Loaded;
                        Entry.Type = AssetType.Model;
                        Entry.BuildPreviewMethod = BuildPreviewDishonored;
                        Entry.LoadMethod = ExportAssetDishonored;
                        Entry.InformationString = $"{Entry.EntryType}";
                        Assets.Add(Entry);
                    }
                    container.Entries.Add(Entry);
                }

            }
            return Assets;
        }

        public List<Asset> ParseDeathloopResources()
        {
            var Assets = new List<Asset>();
            for (var ci = 0; ci < Containers.Count; ci++)
            {
                var container = Containers[ci];
                using var stream = File.OpenRead(Path.Combine(container.Directory, container.Path));
                using var Reader = new BinaryReader(stream);
                var Magic = Reader.ReadBEInt32();
                if (Magic != 0x05534552)
                    throw new Exception();
                Reader.Advance(28);
                var EntryCount = Reader.ReadBEInt32();
                for (int i = 0; i < EntryCount; i++)
                {
                    var Entry = new D2Entry
                    {
                        AssetPointer = Reader.BaseStream.Position,
                        Container = ci,
                        Id = Reader.ReadUInt32(),
                        EntryType = Reader.ReadFixedPrefixString(),
                        Name = Reader.ReadFixedPrefixString(),
                        Destination = Reader.ReadFixedPrefixString(),
                        ResourcePosition = Reader.ReadUInt64(),
                        AssetSize = Reader.ReadInt32(),
                        CompressedSize = Reader.ReadInt32(),
                        dummy = Reader.ReadInt32(),
                        unk = Reader.ReadInt32(),
                        flag3 = Reader.ReadInt32(),
                        flag2 = Reader.ReadInt16()
                    };
                    if (Entry.EntryType == "baseModel" || Entry.EntryType == "model")
                    {
                        Entry.Status = AssetStatus.Loaded;
                        Entry.Type = AssetType.Model;
                        Entry.LoadMethod = ExportAssetDeathloop;
                        Entry.BuildPreviewMethod = BuildPreviewDeathloop;
                        Entry.InformationString = $"{Entry.EntryType}";
                        Assets.Add(Entry);
                    }
                    container.Entries.Add(Entry);
                }
            }
            return Assets;
        }

        public void ExportAssetDishonored(Asset asset, VexInstance instance)
        {
            var Entry = asset as D2Entry;
            byte[] output;
            var container = Containers[Entry.Container];
            using var stream = File.OpenRead(Path.Combine(container.Directory, container.ResourcePath((ushort)Entry.flag2)));
            using var Reader = new BinaryReader(stream);
            Reader.Seek((long)Entry.ResourcePosition);
            var rawData = Reader.ReadBytes(Entry.CompressedSize);
            output = Entry.AssetSize != Entry.CompressedSize ? ZLIB.Decompress(rawData, (int)Entry.AssetSize) : rawData;

            var dir = Path.Combine(instance.Settings.ExportDirectory, "Dishonored 2", "Model");
            Directory.CreateDirectory(dir);
            var AssetPath = Path.Combine(instance.Settings.ExportDirectory, "Dishonored 2", "Model", Path.GetFileName(Entry.Destination));

            File.WriteAllBytes(AssetPath, output);
        }

        public void ExportAssetDeathloop(Asset asset, VexInstance instance)
        {
            var Entry = asset as D2Entry;
            byte[] output = new byte[Entry.AssetSize];
            var container = Containers[Entry.Container];
            using var stream = File.OpenRead(Path.Combine(container.Directory, container.Resources[Entry.flag2]));
            using var Reader = new BinaryReader(stream);
            Reader.Seek((long)Entry.ResourcePosition);
            byte[] ReadData;
            var oodle = Reader.ReadFixedString(4);
            if (oodle == "OOD")
            {
                var dummy = Reader.ReadInt32();
                var xsize = Reader.ReadInt32();
                var bytesToRead = Entry.CompressedSize - 12;
                ReadData = Reader.ReadBytes(bytesToRead);
                Oodle.Decompress(ReadData, 0, bytesToRead, output, 0, (int)Entry.AssetSize);
            }
            else
            {
                //Go back 4 bytes if not OOD
                Reader.BaseStream.Position -= 4;
                ReadData = Reader.ReadBytes(Entry.CompressedSize);
                if (Entry.AssetSize != Entry.CompressedSize)
                {
                    ZLIB.Decompress(ReadData, 0, Entry.CompressedSize, output, 0, (int)Entry.AssetSize);
                }
                else
                {
                    output = ReadData;
                }
            }
            var dir = Path.Combine(instance.Settings.ExportDirectory, "Deathloop", "Model");
            Directory.CreateDirectory(dir);
            var AssetPath = Path.Combine(instance.Settings.ExportDirectory, "Deathloop", "Model", Path.GetFileName(Entry.Destination));

            File.WriteAllBytes(AssetPath, output);
        }

        public Model BuildPreviewDishonored(Asset asset, VexInstance instance)
        {
            var Entry = asset as D2Entry;
            byte[] output;
            var container = Containers[Entry.Container];
            using var stream = File.OpenRead(Path.Combine(container.Directory, container.ResourcePath((ushort)Entry.flag2)));
            using var Reader = new BinaryReader(stream);
            Reader.Seek((long)Entry.ResourcePosition);
            var rawData = Reader.ReadBytes(Entry.CompressedSize);
            output = Entry.AssetSize != Entry.CompressedSize ? ZLIB.Decompress(rawData, (int)Entry.AssetSize) : rawData;

            var model = ModelHelper.BuildDishonoredPreviewModel(output);

            return model;
        }

        public Model BuildPreviewDeathloop(Asset asset, VexInstance instance)
        {
            var Entry = asset as D2Entry;
            byte[] output = new byte[Entry.AssetSize];
            var container = Containers[Entry.Container];
            using var stream = File.OpenRead(Path.Combine(container.Directory, container.Resources[Entry.flag2]));
            using var Reader = new BinaryReader(stream);
            Reader.Seek((long)Entry.ResourcePosition);
            byte[] ReadData;
            var oodle = Reader.ReadFixedString(4);
            if (oodle == "OOD")
            {
                var dummy = Reader.ReadInt32();
                var xsize = Reader.ReadInt32();
                var bytesToRead = Entry.CompressedSize - 12;
                ReadData = Reader.ReadBytes(bytesToRead);
                Oodle.Decompress(ReadData, 0, bytesToRead, output, 0, (int)Entry.AssetSize);
            }
            else
            {
                //Go back 4 bytes if not OOD
                Reader.BaseStream.Position -= 4;
                ReadData = Reader.ReadBytes(Entry.CompressedSize);
                if (Entry.AssetSize != Entry.CompressedSize)
                {
                    ZLIB.Decompress(ReadData, 0, Entry.CompressedSize, output, 0, (int)Entry.AssetSize);
                }
                else
                {
                    output = ReadData;
                }
            }

            var model = ModelHelper.BuildDeathloopPreviewModel(output);

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

        public void Clear()
        {
            Containers.Clear();
            Containers = null;
        }
    }
}
