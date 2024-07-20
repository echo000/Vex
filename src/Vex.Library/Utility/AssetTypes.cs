using DirectXTex;
using Vex.Library.Utility;
using System;
using System.Collections.Generic;

namespace Vex.Library
{
    public enum BoneDataTypes
    {
        DivideBySize,
        QuatPackingA,
        HalfFloat
    };

    public class XModelSubmesh_t
    {
        public uint VertListCount;
        public ulong RigidWeightsPtr;
        public uint VertexCount;
        public uint FaceCount;
        public uint PackedIndexTableCount;
        public ulong FacesPtr;
        public ulong VertexPtr;
        public ulong VertexNormalsPtr;
        public ulong VertexUVsPtr;
        public ulong VertexColorPtr;
        public ulong PackedIndexTablePtr;
        public ulong PackedIndexBufferPtr;
        public ushort[] WeightCounts;
        public ulong WeightsPtr;
        public ulong BlendShapesPtr;
        public float Scale;
        public float XOffset;
        public float YOffset;
        public float ZOffset;
        public int MaterialIndex;

        public XModelSubmesh_t()
        {
            // Defaults
            VertListCount = 0;
            RigidWeightsPtr = 0;
            VertexCount = 0;
            FaceCount = 0;
            VertexPtr = 0;
            WeightsPtr = 0;
            FacesPtr = 0;
            VertexPtr = 0;
            VertexNormalsPtr = 0;
            VertexUVsPtr = 0;
            VertexColorPtr = 0;
            PackedIndexTablePtr = 0;
            PackedIndexBufferPtr = 0;
            BlendShapesPtr = 0;
            PackedIndexTableCount = 0;
            Scale = 0;
            XOffset = 0;
            YOffset = 0;
            ZOffset = 0;

            WeightCounts = new ushort[8];

            // Set
            WeightsPtr = 0;
            MaterialIndex = -1;
        }
    }

    public enum ImageUsageType : byte
    {
        Unknown,
        DiffuseMap,
        NormalMap,
        SpecularMap,
        GlossMap
    }

    public class XImage_t(ImageUsageType usage, uint hash, ulong pointer, string name)
    {
        public ImageUsageType ImageUsage = usage;
        public ulong ImagePtr = pointer;
        public uint SemanticHash = hash;
        public string ImageName = name;
    }

    public class XMaterialSetting_t
    {
        public string Name;
        public string Type;
        public float[] Data;

        public XMaterialSetting_t(string name, string type, float[] data, int numElements)
        {
            Name = name;
            Type = type;
            Data = data;
            // Append element count to match HLSL names
            if (numElements > 1)
                Type += numElements.ToString();
        }

        public XMaterialSetting_t(string name, string type, int[] data, int numElements)
        {
            Name = name;
            Type = type;
            for (int i = 0; i < numElements; i++)
            {
                Data[i] = data[i];
            }
            // Append element count to match HLSL names
            if (numElements > 1)
                Type += numElements.ToString();
        }

        public XMaterialSetting_t(string name, string type, uint[] data, int numElements)
        {
            Name = name;
            Type = type;
            for (int i = 0; i < numElements; i++)
            {
                Data[i] = data[i];
            }
            // Append element count to match HLSL names
            if (numElements > 1)
                Type += numElements.ToString();
        }
    }

    public class XMaterial_t(int imageCount)
    {
        public string MaterialName = "vex_material";
        public string TechsetName;
        public string SurfaceTypeName;
        public List<XImage_t> Images = new(imageCount);
        public List<XMaterialSetting_t> Settings;
    }

    public class XModelLod_t(int submeshCount)
    {
        public string Name = string.Empty;
        public List<XModelSubmesh_t> Submeshes = new(submeshCount);
        public List<XMaterial_t> Materials = new(submeshCount);
        public ulong LODStreamKey = 0;
        public ulong LODStreamInfoPtr = 0;
        public float LodDistance = 100.0f;
        public float LodMaxDistance = 200.0f;
    }

    public class XModel_t(int lodCount)
    {
        public string ModelName = "";
        public BoneDataTypes BoneRotationData = BoneDataTypes.DivideBySize;
        public bool IsModelStreamed = false;
        public bool HashedBoneIDs;
        public uint BoneCount = 0;
        public uint RootBoneCount = 0;
        public uint CosmeticBoneCount = 0;
        public uint BlendShapeCount;
        public ulong BoneIDsPtr = 0;
        public byte BoneIndexSize = 2;
        public ulong BoneParentsPtr = 0;
        public byte BoneParentSize = 1;
        public ulong RotationsPtr = 0;
        public ulong TranslationsPtr = 0;
        public ulong BaseMatriciesPtr = 0;
        public ulong BoneInfoPtr = 0;
        public ulong BlendShapeNamesPtr;
        public List<XModelLod_t> ModelLods = new(lodCount);
    }

    public enum AnimationKeyTypes
    {
        DivideBySize,
        MinSizeTable,
        QuatPackingA,
        HalfFloat
    }

    public class XAnim_t
    {
        public string AnimationName;
        public float FrameRate;
        public uint FrameCount;
        public bool ViewModelAnimation;
        public bool LoopingAnimation;
        public bool AdditiveAnimation;
        public bool SupportsInlineIndicies;
        public ulong BoneIDsPtr;
        public byte BoneIndexSize;
        public byte BoneTypeSize;
        public AnimationKeyTypes RotationType;
        public AnimationKeyTypes TranslationType;
        public ulong DataBytesPtr;
        public ulong DataShortsPtr;
        public ulong DataIntsPtr;
        public ulong RandomDataBytesPtr;
        public ulong RandomDataShortsPtr;
        public ulong RandomDataIntsPtr;
        public ulong LongIndiciesPtr;
        public ulong NotificationsPtr;
        public ulong BlendShapeNamesPtr;
        public ulong BlendShapeWeightsPtr;
        public ulong DeltaTranslationPtr;
        public ulong Delta2DRotationsPtr;
        public ulong Delta3DRotationsPtr;
        public uint NoneRotatedBoneCount;
        public uint TwoDRotatedBoneCount;
        public uint NormalRotatedBoneCount;
        public uint TwoDStaticRotatedBoneCount;
        public uint NormalStaticRotatedBoneCount;
        public uint NormalTranslatedBoneCount;
        public uint PreciseTranslatedBoneCount;
        public uint StaticTranslatedBoneCount;
        public uint NoneTranslatedBoneCount;
        public uint TotalBoneCount;
        public uint NotificationCount;
        public uint BlendShapeWeightCount;

        //Reader Information
        public CoDXAnimReader Reader;
        public Action<XAnim_t, PhilLibX.Media3D.Animation, VexInstance> ReaderFunction;
        public ulong ReaderInformationPointer;

        public XAnim_t()
        {
            // Defaults
            AnimationName = "";
            FrameRate = 30.0f;
            FrameCount = 0;
            ViewModelAnimation = false;
            LoopingAnimation = false;
            AdditiveAnimation = false;
            SupportsInlineIndicies = true;
            BoneIDsPtr = 0;
            BoneIndexSize = 2;
            BoneTypeSize = 0;
            DataBytesPtr = 0;
            DataShortsPtr = 0;
            DataIntsPtr = 0;
            RandomDataBytesPtr = 0;
            RandomDataShortsPtr = 0;
            RandomDataIntsPtr = 0;
            LongIndiciesPtr = 0;
            NotificationsPtr = 0;
            DeltaTranslationPtr = 0;
            Delta2DRotationsPtr = 0;
            Delta3DRotationsPtr = 0;
            RotationType = AnimationKeyTypes.DivideBySize;
            TranslationType = AnimationKeyTypes.MinSizeTable;
        }
    }

    public class XImageDDS
    {
        // The DDS data buffer
        public byte[] DataBuffer;
        public DirectXTexUtility.DXGIFormat format;
        public int width, height;
        // The requested image patch type
        public ImagePatch ImagePatchType;

        public XImageDDS()
        {
            DataBuffer = null;
            height = 0;
            width = 0;
            ImagePatchType = ImagePatch.NoPatch;
        }
    };

    public enum SoundDataTypes : byte
    {
        WAV_WithHeader,
        WAV_NeedsHeader,
        FLAC_WithHeader,
        FLAC_NeedsHeader,
        Opus_Interleaved,
        Opus_Interleaved_Streamed,
    };

    public class XSound
    {
        public byte[] DataBuffer;
        public uint DataSize;
        public SoundDataTypes DataType;

        public XSound()
        {
            DataBuffer = null;
            DataSize = 0;
            DataType = SoundDataTypes.FLAC_WithHeader;
        }
    }
}
