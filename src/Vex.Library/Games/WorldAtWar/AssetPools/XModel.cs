using Saluki.Library.Utility;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Saluki.Library
{
    partial class WorldAtWar
    {
        /// <summary>
        /// Black Ops 3 Raw File Logic
        /// </summary>
        private class XModel : IAssetPool
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
            public override string Name => "Model";

            /// <summary>
            /// Gets the Index of this Pool
            /// </summary>
            public override int Index => (int)AssetPool.xmodel;

            /// <summary>
            /// Loads Assets from this Asset Pool
            /// </summary>
            public override List<Asset> Load(SalukiInstance instance)
            {
                var results = new List<Asset>();

                StartAddress = (long)instance.Game.GameOffsetInfos[1] + 4;
                AssetCount = (int)instance.Game.GamePoolSizes[1];
                AssetSize = Marshal.SizeOf<WAWXModel>();

                var headers = instance.Reader.ReadArray<WAWXModel>(StartAddress, AssetCount);

                // Store the placeholder model
                var PlaceholderModel = new WAWXModel();

                for (var i = 0; i < AssetCount; i++)
                {
                    var header = headers[i];

                    if (IsNullAsset(header.NamePointer))
                        continue;

                    var address = StartAddress + (i * AssetSize);

                    var name = instance.Reader.ReadNullTerminatedString(header.NamePointer);

                    var status = AssetStatus.Loaded;

                    if (name == "void")
                    {
                        PlaceholderModel = header;
                        status = AssetStatus.Placeholder;
                    }
                    else if (header.BoneIdPointer == PlaceholderModel.BoneIdPointer && header.ParentListPointer == PlaceholderModel.ParentListPointer && header.QuaternionsPointer == PlaceholderModel.QuaternionsPointer && header.TranslationsPointer == PlaceholderModel.TranslationsPointer && header.PartClassificationPointer == PlaceholderModel.PartClassificationPointer && header.BaseMatPointer == PlaceholderModel.BaseMatPointer && header.NumLods == PlaceholderModel.NumLods && header.MaterialHandlesPointer == PlaceholderModel.MaterialHandlesPointer && header.BoneCount == PlaceholderModel.BoneCount)
                    {
                        // Set as placeholder, data matches void
                        status = AssetStatus.Placeholder;
                    }

                    results.Add(new ModelAsset()
                    {
                        Name = name,
                        InformationString = string.Format("Bones: {0} LODs: {1}", header.BoneCount, header.NumLods),
                        BoneCount = header.BoneCount,
                        LodCount = header.NumLods,
                        Status = status,
                        AssetPointer = address,
                        LoadMethod = ExportAsset,
                        BuildPreviewMethod = GetModelForRendering,
                        GetModelImages = GetModelImages
                    });
                }
                return results;
            }

            /// <summary>
            /// Exports the given asset from this pool
            /// </summary>
            public void ExportAsset(Asset asset, SalukiInstance instance)
            {
                var GenericModel = ReadXModel(asset, instance);
                ExportManager.ExportModel(GenericModel, asset.Name, instance);
            }

            /// <summary>
            /// Build the model for rendering
            /// </summary>
            public PhilLibX.Media3D.Model GetModelForRendering(Asset asset, SalukiInstance instance)
            {
                var GenericModel = ReadXModel(asset, instance);
                var BiggestLodIndex = ModelHelper.CalculateBiggestLodIndex(GenericModel);
                return ModelHelper.TranslateXModel(GenericModel, BiggestLodIndex, false, instance);
            }

            public Dictionary<string, XImageDDS> GetModelImages(Asset asset, SalukiInstance instance)
            {
                var GenericModel = ReadXModel(asset, instance);
                Dictionary<string, XImageDDS> Images = [];
                var BiggestLodIndex = ModelHelper.CalculateBiggestLodIndex(GenericModel);
                foreach (var material in GenericModel.ModelLods[BiggestLodIndex].Materials)
                {
                    ModelHelper.LoadMaterialImages(material, Images, instance);
                }
                return Images;
            }

            public static XModel_t ReadXModel(Asset asset, SalukiInstance instance)
            {
                var ModelData = instance.Reader.ReadStruct<WAWXModel>(asset.AssetPointer);
                var ModelAsset = new XModel_t(ModelData.NumLods)
                {
                    ModelName = asset.DisplayName,
                    // Bone counts
                    BoneCount = ModelData.BoneCount,
                    RootBoneCount = ModelData.RootCount,
                    // Bone data type
                    BoneRotationData = BoneDataTypes.DivideBySize,

                    BoneIDsPtr = ModelData.BoneIdPointer,
                    BoneIndexSize = 2,

                    BoneParentsPtr = ModelData.ParentListPointer,
                    BoneParentSize = 1,

                    RotationsPtr = ModelData.QuaternionsPointer,
                    TranslationsPtr = ModelData.TranslationsPointer,

                    BaseMatriciesPtr = ModelData.BaseMatPointer
                };

                for (int i = 0; i < ModelData.NumLods; i++)
                {
                    var LodReference = new XModelLod_t(ModelData.ModelLods[i].NumSurfs)
                    {
                        LodDistance = ModelData.ModelLods[i].LodDistance
                    };

                    var XSurfacePtr = ModelData.SurfacesPointer + (ModelData.ModelLods[i].SurfacesIndex * Marshal.SizeOf<WAWXModelSurface>());

                    for (var s = 0; s < ModelData.ModelLods[i].NumSurfs; s++)
                    {
                        var SubmeshReference = new XModelSubmesh_t();

                        var SurfaceInfo = instance.Reader.ReadStruct<WAWXModelSurface>(XSurfacePtr);

                        // Apply surface info
                        SubmeshReference.VertListCount = (uint)SurfaceInfo.VertListCount;
                        SubmeshReference.RigidWeightsPtr = SurfaceInfo.RigidWeightsPtr;
                        SubmeshReference.VertexCount = SurfaceInfo.VertexCount;
                        SubmeshReference.FaceCount = SurfaceInfo.FacesCount;
                        SubmeshReference.VertexPtr = SurfaceInfo.VerticiesPtr;
                        SubmeshReference.FacesPtr = SurfaceInfo.FacesPtr;

                        // Assign weights
                        SubmeshReference.WeightCounts[0] = SurfaceInfo.WeightCounts[0];
                        SubmeshReference.WeightCounts[1] = SurfaceInfo.WeightCounts[1];
                        SubmeshReference.WeightCounts[2] = SurfaceInfo.WeightCounts[2];
                        SubmeshReference.WeightCounts[3] = SurfaceInfo.WeightCounts[3];
                        // Weight pointer
                        SubmeshReference.WeightsPtr = SurfaceInfo.WeightsPtr;

                        var MaterialHandle = instance.Reader.ReadUInt32(ModelData.MaterialHandlesPointer);

                        LodReference.Materials.Add(instance.Game.ReadXMaterial(MaterialHandle, instance));

                        LodReference.Submeshes.Add(SubmeshReference);

                        XSurfacePtr += Marshal.SizeOf<WAWXModelSurface>();
                        ModelData.MaterialHandlesPointer += sizeof(uint);
                    }
                    ModelAsset.ModelLods.Add(LodReference);
                }
                // Return it
                return ModelAsset;
            }
        }
    }
}
