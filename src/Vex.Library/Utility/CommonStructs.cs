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
}
