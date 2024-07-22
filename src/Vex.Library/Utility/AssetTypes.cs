using DirectXTex;
using System.Collections.Generic;
using Vex.Library.Utility;

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
}
