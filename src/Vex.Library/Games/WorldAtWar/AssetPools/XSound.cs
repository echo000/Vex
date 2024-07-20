using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Saluki.Library
{
    partial class WorldAtWar
    {
        private class XSound : IAssetPool
        {
            /// <summary>
            /// Size of each asset
            /// </summary>
            public override int AssetSize { get; set; }

            /// <summary>
            /// Gets or Sets the number of Assets 
            /// </summary>
            public override int AssetCount { get; set; }

            /// <summary>
            /// Gets or Sets the Start Address
            /// </summary>
            public override long StartAddress { get; set; }

            /// <summary>
            /// Gets or Sets the End Address
            /// </summary>
            public override long EndAddress { get { return StartAddress + (AssetCount * AssetSize); } set => throw new NotImplementedException(); }

            /// <summary>
            /// Gets the Name of this Pool
            /// </summary>
            public override string Name => "Sound";

            /// <summary>
            /// Gets the Index of this Pool
            /// </summary>
            public override int Index => (int)AssetPool.sound;


            public override List<Asset> Load(SalukiInstance instance)
            {
                var results = new List<Asset>();

                StartAddress = (long)instance.Game.GameOffsetInfos[2] + 4;
                AssetCount = (int)instance.Game.GamePoolSizes[2];
                AssetSize = Marshal.SizeOf<WAWLoadedSound>();

                var headers = instance.Reader.ReadArray<WAWLoadedSound>(StartAddress, AssetCount);

                for (var i = 0; i < AssetCount; i++)
                {
                    var header = headers[i];

                    if (IsNullAsset(header.NamePtr))
                        continue;

                    var address = StartAddress + (i * AssetSize);

                    var SoundName = instance.Reader.ReadNullTerminatedString((long)header.NamePtr);

                    results.Add(new AudioAsset()
                    {
                        Name = Path.GetFileNameWithoutExtension(SoundName),
                        InformationString = "N/A",
                        AssetPointer = header.SoundDataPtr,
                        AssetSize = header.SoundDataSize,
                        FileEntry = false,
                        FullPath = Path.GetDirectoryName(SoundName),
                        DataType = SoundDataTypes.WAV_WithHeader,
                        Length = 0,
                        Status = AssetStatus.Loaded,
                        LoadMethod = ExportAsset
                    });
                }

                return results;
            }


            public void ExportAsset(Asset asset, SalukiInstance instance)
            {
                var SoundAsset = asset as AudioAsset;
                var SoundData = WorldWar2.ReadXSound(SoundAsset, instance);

                var FullSoundPath = Path.Combine(instance.ExportFolder, instance.Game.Name, "sound");
                Sound.ExportSoundAsset(SoundData, FullSoundPath, SoundAsset.Name, instance);
            }
        }
    }
}
