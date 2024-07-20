using Saluki.Library.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Saluki.Library
{
    partial class WorldAtWar
    {
        internal class XAnim : IAssetPool
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
            public override string Name => "Animation";

            /// <summary>
            /// Gets the Index of this Pool
            /// </summary>
            public override int Index => (int)AssetPool.xanim;

            public override List<Asset> Load(SalukiInstance instance)
            {
                var results = new List<Asset>();

                StartAddress = (long)instance.Game.GameOffsetInfos[0] + 4;
                AssetCount = (int)instance.Game.GamePoolSizes[0];
                AssetSize = Marshal.SizeOf<WAWXAnim>();

                var headers = instance.Reader.ReadArray<WAWXAnim>(StartAddress, AssetCount);
                var PlaceholderAnim = new WAWXAnim();
                for (var i = 0; i < AssetCount; i++)
                {
                    var header = headers[i];

                    if (IsNullAsset((long)header.NamePtr))
                        continue;

                    var address = StartAddress + (i * AssetSize);

                    var name = instance.Reader.ReadNullTerminatedString((long)header.NamePtr);

                    var status = AssetStatus.Loaded;

                    if (name == "void")
                    {
                        PlaceholderAnim = header;
                        status = AssetStatus.Placeholder;
                    }
                    else if (header.BoneIDsPtr == PlaceholderAnim.BoneIDsPtr && header.DataBytePtr == PlaceholderAnim.DataBytePtr && header.DataShortPtr == PlaceholderAnim.DataShortPtr
                        && header.DataIntPtr == PlaceholderAnim.DataIntPtr && header.RandomDataBytePtr == PlaceholderAnim.RandomDataBytePtr && header.RandomDataIntPtr == PlaceholderAnim.RandomDataIntPtr &&
                        header.RandomDataShortPtr == PlaceholderAnim.RandomDataShortPtr && header.NotificationsPtr == PlaceholderAnim.NotificationsPtr && header.DeltaPartsPtr == PlaceholderAnim.DeltaPartsPtr)
                    {
                        status = AssetStatus.Placeholder;
                    }

                    results.Add(new AnimationAsset()
                    {
                        Name = name,
                        InformationString = $"Framerate: {header.Framerate} Frames: {header.NumFrames} Bones: {header.TotalBoneCount}",
                        Framerate = header.Framerate,
                        FrameCount = header.NumFrames,
                        BoneCount = header.TotalBoneCount,
                        Status = status,
                        AssetPointer = address,
                        LoadMethod = ExportAsset
                    });
                }
                return results;
            }

            public void ExportAsset(Asset asset, SalukiInstance instance)
            {
                var dir = Path.Combine(instance.ExportFolder, instance.Game.Name, "xanims");
                Directory.CreateDirectory(dir);
                var GenericAnim = ReadXAnim(asset, instance);
                var Anim = AnimationHelper.TranslateXAnim(GenericAnim, instance);
                ExportManager.ExportAnimation(Anim, dir, asset.Name, instance);
            }

            public static XAnim_t ReadXAnim(Asset asset, SalukiInstance instance)
            {
                // Prepare to read the xanim
                XAnim_t Anim = new();

                // Read the XAnim structure
                var AnimData = instance.Reader.ReadStruct<WAWXAnim>(asset.AssetPointer);

                // Copy over default properties
                Anim.AnimationName = asset.DisplayName;
                // Frames and Rate
                Anim.FrameCount = AnimData.NumFrames;
                Anim.FrameRate = AnimData.Framerate;

                // Check for viewmodel animations
                if ((asset.Name.StartsWith("viewmodel_", StringComparison.OrdinalIgnoreCase)))
                {
                    // This is a viewmodel animation
                    Anim.ViewModelAnimation = true;
                }
                // Check for looping
                Anim.LoopingAnimation = (AnimData.Looped > 0);

                // Read the delta data
                var AnimDeltaData = instance.Reader.ReadStruct<WAWXAnimDeltaParts>(AnimData.DeltaPartsPtr);

                // Copy over pointers
                Anim.BoneIDsPtr = AnimData.BoneIDsPtr;
                Anim.DataBytesPtr = AnimData.DataBytePtr;
                Anim.DataShortsPtr = AnimData.DataShortPtr;
                Anim.DataIntsPtr = AnimData.DataIntPtr;
                Anim.RandomDataBytesPtr = AnimData.RandomDataBytePtr;
                Anim.RandomDataShortsPtr = AnimData.RandomDataShortPtr;
                Anim.RandomDataIntsPtr = AnimData.RandomDataIntPtr;
                Anim.LongIndiciesPtr = AnimData.LongIndiciesPtr;
                Anim.NotificationsPtr = AnimData.NotificationsPtr;

                // Bone ID index size
                Anim.BoneIndexSize = 2;

                // Copy over counts
                Anim.NoneRotatedBoneCount = AnimData.NoneRotatedBoneCount;
                Anim.TwoDRotatedBoneCount = AnimData.TwoDRotatedBoneCount;
                Anim.NormalRotatedBoneCount = AnimData.NormalRotatedBoneCount;
                Anim.TwoDStaticRotatedBoneCount = AnimData.TwoDStaticRotatedBoneCount;
                Anim.NormalStaticRotatedBoneCount = AnimData.NormalStaticRotatedBoneCount;
                Anim.NormalTranslatedBoneCount = AnimData.NormalTranslatedBoneCount;
                Anim.PreciseTranslatedBoneCount = AnimData.PreciseTranslatedBoneCount;
                Anim.StaticTranslatedBoneCount = AnimData.StaticTranslatedBoneCount;
                Anim.NoneTranslatedBoneCount = AnimData.NoneTranslatedBoneCount;
                Anim.TotalBoneCount = AnimData.TotalBoneCount;
                Anim.NotificationCount = AnimData.NotificationCount;

                // Copy delta
                Anim.DeltaTranslationPtr = AnimDeltaData.DeltaTranslationsPtr;
                Anim.Delta2DRotationsPtr = AnimDeltaData.Delta2DRotationsPtr;

                // Set types, we use dividebysize for WAW
                Anim.RotationType = AnimationKeyTypes.DivideBySize;
                Anim.TranslationType = AnimationKeyTypes.MinSizeTable;

                // World at War supports inline indicies
                Anim.SupportsInlineIndicies = true;

                // Return it
                return Anim;
            }
        }
    }
}
