using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Vex.Library
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct VoidResourceHeader
    {
        public int Magic;
        public short Version;
    }

    struct VoidContainer
    {
        public string Directory;
        public string Path;
        public List<string> Resources;
        public List<Asset> Entries;
        public VoidContainer()
        {
            Resources = [];
            Entries = [];
        }
        public readonly string ResourcePath(ushort flags)
        {
            string Result = string.Empty;
            int index = (flags & 0x8000) != 0 ? Resources.Count - 1 : (int)(flags >> 2);
            if (Resources.Count > index)
            {
                Result = Resources[index];
            }
            return Result;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct VoidMesh
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public short[] Unk1;
        public uint Flags1;
        public uint Flags2;
        public Vector3 unkVec;
        public Vector3 unkVec2;
        public Vector2 unkVec3;
        public Vector2 unkVec4;
        public uint VertexCount;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public uint[] Unk3;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct VoidMaterial
    {
        public int MeshId;
        public int VertexStart;
        public int VertexEnd;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct VoidSkeletonHeader
    {
        public uint Magic;
        public uint FileSize;
        public uint CustomDataSize;
        public uint NameHashesSize;
        public ushort BoneCount;
        public ushort UserChannelCount;
        public ushort Flags;
        public ushort LocomotionJointIndex;
        public uint BasePoseOffset;
        public uint JointLinkageMapOffset;
        public uint JointNameHashArrayOffset;
        public uint UserChannelNameHashArrayOffset;
        public uint UserChannelNodeNameHashArrayOffset;
        public uint UserChannelFlagsArrayOffset;
        public uint CustomDataOffset;
        public uint Padding1;
        public uint Padding2;
        public uint JointLinkageCount;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct VoidTransforms
    {
        public Vector4 Rotation;
        //Technically position + scale are Vector4s, but only the XYZ values of each are used
        public Vector4 Position;
        public Vector4 Scale;
    }
}
