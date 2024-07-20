using Saluki.Library.Utility;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Saluki.Library
{
    public partial class WorldAtWar : IGame
    {

        #region Structures
        /// <summary>
        /// Asset Pool Data
        /// </summary>
        public struct AssetPoolInfo
        {
            #region AssetPoolInfoProperties
            public uint PoolPointer { get; set; }
            public uint AssetSize { get; set; }
            public uint AssetCount { get; set; }
            public uint FreeSlot { get; set; }
            #endregion
        }
        #endregion

        /// <summary>
        /// Gets World At War's Game Name
        /// </summary>
        public string Name => "World At War";

        /// <summary>
        /// Gets World At War's Process Names
        /// </summary>
        public List<(GameFlags Flags, string Name)> ProcessNames =>
        [
            (GameFlags.SP,"codwaw"),
            (GameFlags.MP,"codwawmp")
        ];

        /// <summary>
        /// Gets or sets the list of Asset Pools
        /// </summary>
        public List<IAssetPool> AssetPools { get; set; }

        public NameCache NameCache { get; set; }
        public NameCache StringCache { get; set; }
        public List<ulong> GameOffsetInfos { get; set; }
        public List<uint> GamePoolSizes { get; set; }

        readonly DBGameInfo[] SinglePlayerOffsets = [new(0x8DC828, 0x8DC5D0, 0x3702400, 0)];
        readonly DBGameInfo[] MultiPlayerOffsets = [new(0x8D0958, 0x8D06E8, 0xF66B400, 0)];

        /// <summary>
        /// World At War Asset Pool Indices
        /// </summary>
        private enum AssetPool : int
        {
            xanim = 4,
            xmodel = 5,
            sound = 0xA,
        }

        public XImageDDS LoadXImage(XImage_t image, SalukiInstance instance)
        {
            var ImageData = instance.GameCache.ExtractPackageObject(instance.GameCache.HashPackageID(image.ImageName), instance);
            if (ImageData != null)
            {
                var Result = ImageTranslator.TranslateIWI(ImageData);
                if (Result != null && image.ImageUsage == ImageUsageType.NormalMap && instance.Settings.PatchNormals)
                {
                    Result.ImagePatchType = ImagePatch.Normal_Bumpmap;
                }
                return Result;
            }
            return null;
        }

        public XMaterial_t ReadXMaterial(ulong MaterialPointer, SalukiInstance instance)
        {
            var MaterialData = instance.Reader.ReadStruct<WAWXMaterial>((long)MaterialPointer);

            var Result = new XMaterial_t(MaterialData.ImageCount)
            {
                MaterialName = Path.GetFileNameWithoutExtension(instance.Reader.ReadNullTerminatedString(MaterialData.NamePtr))
            };

            for (int i = 0; i < MaterialData.ImageCount; i++)
            {
                var ImageInfo = instance.Reader.ReadStruct<WAWXMaterialImage>(MaterialData.ImageTablePtr);
                var ImageName = instance.Reader.ReadNullTerminatedString(instance.Reader.ReadUInt32(ImageInfo.ImagePtr + (Marshal.SizeOf<WAWGfxImage>() - 4)));

                var DefaultUsage = ImageUsageType.Unknown;

                switch (ImageInfo.SemanticHash)
                {
                    case 0xA0AB1041:
                        DefaultUsage = ImageUsageType.DiffuseMap;
                        break;
                    case 0x59D30D0F:
                        DefaultUsage = ImageUsageType.NormalMap;
                        break;
                    case 0x34ECCCB3:
                        DefaultUsage = ImageUsageType.SpecularMap;
                        break;
                }

                Result.Images.Add(new XImage_t(DefaultUsage, ImageInfo.SemanticHash, ImageInfo.ImagePtr, ImageName));

                MaterialData.ImageTablePtr += (uint)Marshal.SizeOf<WAWXMaterialImage>();
            }

            return Result;
        }

        public string GetString(long index, SalukiInstance instance)
        {
            return instance.Reader.ReadNullTerminatedString((long)GameOffsetInfos[3] + (12 * index) + 4);
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public bool Initialize(SalukiInstance instance)
        {
            foreach (var GameOffsets in (instance.LoadedGameFlags == GameFlags.SP) ? SinglePlayerOffsets : MultiPlayerOffsets)
            {
                GameOffsetInfos =
                [
                    instance.Reader.ReadUInt32((long)GameOffsets.DBAssetPools + (4 * (int)AssetPool.xanim)),
                    instance.Reader.ReadUInt32((long)GameOffsets.DBAssetPools + (4 * (int)AssetPool.xmodel)),
                    instance.Reader.ReadUInt32((long)GameOffsets.DBAssetPools + (4 * (int)AssetPool.sound))
                ];
                var FirstXModelName = instance.Reader.ReadNullTerminatedString((long)instance.Reader.ReadUInt32((long)GameOffsetInfos[1] + 4));
                if (FirstXModelName == "void" || FirstXModelName == "defaultactor" || FirstXModelName == "defaultweapon")
                {
                    GameOffsetInfos.Add(GameOffsets.StringTable);
                    if (!string.IsNullOrWhiteSpace(GetString(2, instance)))
                    {
                        GamePoolSizes =
                        [
                            instance.Reader.ReadUInt32((long)GameOffsets.DBPoolSizes + (4 * (int)AssetPool.xanim)),
                            instance.Reader.ReadUInt32((long)GameOffsets.DBPoolSizes + (4 * (int)AssetPool.xmodel)),
                            instance.Reader.ReadUInt32((long)GameOffsets.DBPoolSizes + (4 * (int)AssetPool.sound))
                        ];
                        return true;
                    }
                }
                GameOffsetInfos.Clear();
            }
            var pools = instance.Reader.FindBytes("FF D2 8B F0 83 C4 04 85 F6 75").ToArray();
            var strPool = instance.Reader.FindBytes("F7 EE D1 FA 8B C2 C1 E8 1F").ToArray();

            if (pools[0] > 0 && strPool[0] > 0)
            {
                var GameOffsets = new DBGameInfo(
                    instance.Reader.ReadUInt32(pools[0] - 0xD),
                    instance.Reader.ReadUInt32(pools[0] + 0x25),
                    instance.Reader.ReadUInt32(strPool[0] - 0x9), 0);

                GameOffsetInfos =
                [
                    instance.Reader.ReadUInt32((long)GameOffsets.DBAssetPools + (4 * (int)AssetPool.xanim)),
                    instance.Reader.ReadUInt32((long)GameOffsets.DBAssetPools + (4 * (int)AssetPool.xmodel)),
                    instance.Reader.ReadUInt32((long)GameOffsets.DBAssetPools + (4 * (int)AssetPool.sound))
                ];
                var FirstXModelName = instance.Reader.ReadNullTerminatedString(instance.Reader.ReadUInt32((long)GameOffsetInfos[1] + 4));
                if (FirstXModelName == "void" || FirstXModelName == "defaultactor" || FirstXModelName == "defaultweapon")
                {
                    GameOffsetInfos.Add(GameOffsets.StringTable);
                    if (!string.IsNullOrWhiteSpace(GetString(2, instance)))
                    {
                        GamePoolSizes =
                        [
                            instance.Reader.ReadUInt32((long)GameOffsets.DBPoolSizes + (4 * (int)AssetPool.xanim)),
                            instance.Reader.ReadUInt32((long)GameOffsets.DBPoolSizes + (4 * (int)AssetPool.xmodel)),
                            instance.Reader.ReadUInt32((long)GameOffsets.DBPoolSizes + (4 * (int)AssetPool.sound))
                        ];
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
