using PhilLibX.IO;
using PhilLibX.Media3D;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Vex.Library.Utility
{
    internal class ModelHelper
    {
        public static Model BuildDishonoredPreviewModel(byte[] ModelBytes, VexInstance instance, out string SkeletonPath)
        {
            using var ModelStream = new MemoryStream(ModelBytes);
            using var Reader = new BinaryReader(ModelStream);

            try
            {
                var Magic = Reader.ReadFixedString(4);
                var isMM6 = Magic != "1LMB";

                if (Magic != "MM6B" && Magic != "MM6@" && Magic != "1LMB")
                {
                    throw new Exception();
                }
                var ResultModel = new Model();

                if (!isMM6)
                {
                    Reader.Advance(4);
                }

                SkeletonPath = Reader.ReadFixedPrefixString();
                if (isMM6)
                {
                    if (SkeletonPath != "")
                    {
                        Reader.Advance(2);
                    }
                    //Skip the bounding box (2 Vector3)
                    Reader.Advance(Marshal.SizeOf<Vector3>() * 2);
                }

                var numMeshes = Reader.ReadUInt32();
                var boneIdOffsets = new List<uint>((int)numMeshes);
                for (int i = 0; i < numMeshes; i++)
                {
                    var Weights = new List<WeightsData>();
                    var Mesh = new Mesh();
                    if (isMM6)
                    {
                        var partName = Reader.ReadFixedPrefixString();
                    }

                    var materialPath = Reader.ReadFixedPrefixString();
                    Reader.Advance(isMM6 ? 9 : 4);
                    //Skip BRTI
                    Reader.Advance(4);
                    var Header = Reader.ReadStruct<VoidMesh>();
                    if (Header.Flags1 != Header.Flags2)
                    {
                        Trace.WriteLine($"Headers don't match {materialPath}");
                    }
                    if (isMM6)
                    {
                        for (int v = 0; v < Header.VertexCount; v++)
                        {
                            var position = Reader.ReadStruct<Vector3>();
                            Mesh.Positions.Add(position);
                            //Add vertex colours as they are not in MM6?
                            Mesh.Colours.Add(Vector4.One);
                        }
                        for (int v = 0; v < Header.VertexCount; v++)
                        {
                            var UVU = new Vector2((float)BitConverter.Int16BitsToHalf(Reader.ReadInt16()),
                                (float)BitConverter.Int16BitsToHalf(Reader.ReadInt16()));
                            var UVV = new Vector2((float)BitConverter.Int16BitsToHalf(Reader.ReadInt16()),
                                (float)BitConverter.Int16BitsToHalf(Reader.ReadInt16()));
                            //Mesh.UVLayers.Add(UVV, v);
                            Mesh.UVLayers.Add(UVU, v);
                        }
                        for (int v = 0; v < Header.VertexCount; v++)
                        {
                            //Normals are 3 sbytes in the range of -128 and 127 followed by a null byte
                            var normal = Reader.ReadBytes(4);
                            Mesh.Normals.Add(new Vector3((sbyte)normal[0], (sbyte)normal[1], (sbyte)normal[2]) / 127f);
                        }
                        for (int v = 0; v < Header.VertexCount; v++)
                        {
                            var tangent = Reader.ReadBytes(4);
                        }
                        for (int v = 0; v < Header.VertexCount; v++)
                        {
                            var Weight = new WeightsData();
                            var weightCount = 0;
                            //Depending on flag 9 or 10, the weights are either 8 or 16 bytes
                            if (Header.Flags2 == 10)
                            {
                                var weightValues = Reader.ReadBytes(8);
                                var boneIndices = Reader.ReadBytes(8);
                                for (int w = 0; w < 8; w++)
                                {
                                    if (weightValues[w] != 0)
                                    {
                                        weightCount++;
                                        Weight.BoneValues[w] = boneIndices[w];
                                        Weight.WeightValues[w] = weightValues[w] / 255f;
                                    }
                                }
                            }
                            else
                            {
                                var weightValues = Reader.ReadBytes(4);
                                var boneIndices = Reader.ReadBytes(4);
                                for (int w = 0; w < 4; w++)
                                {
                                    if (weightValues[w] != 0)
                                    {
                                        weightCount++;
                                        Weight.BoneValues[w] = boneIndices[w];
                                        Weight.WeightValues[w] = weightValues[w] / 255f;
                                    }
                                }
                            }
                            Weight.WeightCount = (byte)weightCount;
                            Weights.Add(Weight);
                        }
                    }
                    else
                    {
                        for (int v = 0; v < Header.VertexCount; v++)
                        {
                            var Position = Reader.ReadStruct<Vector3>();
                            Mesh.Positions.Add(Position);
                            var UVU = new Vector2((float)BitConverter.Int16BitsToHalf(Reader.ReadInt16()),
                                (float)BitConverter.Int16BitsToHalf(Reader.ReadInt16()));
                            var UVV = new Vector2((float)BitConverter.Int16BitsToHalf(Reader.ReadInt16()),
                                (float)BitConverter.Int16BitsToHalf(Reader.ReadInt16()));
                            //Mesh.UVLayers.Add(UVV, v);
                            Mesh.UVLayers.Add(UVU, v);
                            var normal = Reader.ReadBytes(4);
                            Mesh.Normals.Add(new Vector3((sbyte)normal[0], (sbyte)normal[1], (sbyte)normal[2]) / 127f);
                            var tangent = Reader.ReadBytes(4); // Currently doing nothing with this
                            var colours = Reader.ReadBytes(4);
                            Mesh.Colours.Add(new Vector4(colours[0], colours[1], colours[2], colours[3]) / 255f);
                        }
                    }

                    var TriangleCount = Reader.ReadInt32();
                    for (int f = 0; f < TriangleCount / 3; f++)
                    {
                        var v0 = Reader.ReadUInt16();
                        var v1 = Reader.ReadUInt16();
                        var v2 = Reader.ReadUInt16();
                        Mesh.Faces.Add((v2, v1, v0));
                    }
                    Reader.Advance(28);
                    uint BoneIdOffset = 0;
                    if (isMM6)
                    {
                        //Skip Facesets
                        var facesetCount = Reader.ReadUInt32();
                        for (int t = 0; t < facesetCount; t++)
                        {
                            Reader.Advance(12);
                        }
                        BoneIdOffset = Reader.ReadUInt32();
                        Reader.Advance(11 * 4);
                        for (int v = 0; v < Header.VertexCount; v++)
                        {
                            for (int w = 0; w < Weights[v].WeightCount; w++)
                            {
                                Mesh.Influences.Add(((int)(Weights[v].BoneValues[w] + BoneIdOffset), Weights[v].WeightValues[w]), v);
                            }
                        }
                    }
                    else
                    {
                        Reader.Advance(4);
                    }
                    ResultModel.Meshes.Add(Mesh);
                }

                var numMaterials = Reader.ReadInt32();

                for (int i = 0; i < numMaterials; i++)
                {
                    var MaterialPath = Reader.ReadFixedPrefixString();
                    //skip unknown
                    if (isMM6 && isMM6 && Magic == "MM6@")
                        Reader.Advance(4);

                    var MaterialHeader = Reader.ReadStruct<VoidMaterial>();
                    var Material = MaterialHelper.GetMaterial(MaterialPath, instance);
                    ResultModel.Materials.Add(Material);
                    ResultModel.Meshes[MaterialHeader.MeshId].Materials.Add(Material);
                }
                return ResultModel;
            }
            finally
            {
                Reader.Close();
                ModelStream.Close();
            }
        }

        public static Model BuildDeathloopPreviewModel(byte[] ModelBytes, VexInstance instance, out string SkeletonPath)
        {
            using var SkeletonStream = new MemoryStream(ModelBytes);
            using var Reader = new BinaryReader(SkeletonStream);

            try
            {
                var Magic = Reader.ReadUInt32();
                var isMM6 = Magic != 0x19847D60;
                Reader.Seek(0);
                SkeletonPath = string.Empty;

                var ResultModel = new Model();

                if (isMM6)
                {
                    SkeletonPath = Reader.ReadFixedPrefixString();
                    if (SkeletonPath != "")
                        Reader.Advance(2);

                    //Skip bounding box
                    Reader.Advance(Marshal.SizeOf<Vector3>() * 2);
                }
                else
                {
                    Reader.Advance(12);
                }

                var numMeshes = Reader.ReadUInt32();

                for (int i = 0; i < numMeshes; i++)
                {
                    var Weights = new List<WeightsData>();
                    var Mesh = new Mesh();
                    if (isMM6)
                    {
                        var partName = Reader.ReadFixedPrefixString();
                    }
                    else
                    {
                        Reader.Advance(2);
                    }

                    var materialPath = Reader.ReadFixedPrefixString();

                    Reader.Advance(isMM6 ? 9 : 4);
                    //Skip BRTI
                    Reader.Advance(4);
                    var Header = Reader.ReadStruct<VoidMesh>();
                    if (Header.Flags1 != Header.Flags2)
                    {
                        Trace.WriteLine($"Headers don't match {materialPath}");
                    }
                    if (isMM6)
                    {
                        for (int v = 0; v < Header.VertexCount; v++)
                        {
                            var position = Reader.ReadStruct<Vector3>();
                            Mesh.Positions.Add(position);
                            Mesh.Colours.Add(Vector4.One);
                        }
                        for (int v = 0; v < Header.VertexCount; v++)
                        {
                            var UVU = new Vector2((float)BitConverter.Int16BitsToHalf(Reader.ReadInt16()),
                                (float)BitConverter.Int16BitsToHalf(Reader.ReadInt16()));
                            var UVV = new Vector2((float)BitConverter.Int16BitsToHalf(Reader.ReadInt16()),
                                (float)BitConverter.Int16BitsToHalf(Reader.ReadInt16()));
                            Mesh.UVLayers.Add(UVU, v);
                            //Mesh.UVLayers.Add(UVV, v);
                        }
                        for (int v = 0; v < Header.VertexCount; v++)
                        {
                            var normal = Reader.ReadBytes(4);
                            Mesh.Normals.Add(new Vector3((sbyte)normal[0], (sbyte)normal[1], (sbyte)normal[2]) / 127f);
                        }
                        for (int v = 0; v < Header.VertexCount; v++)
                        {
                            var tangent = Reader.ReadBytes(4);
                        }
                        for (int v = 0; v < Header.VertexCount; v++)
                        {
                            var Weight = new WeightsData();
                            var weightCount = 0;
                            if (Header.Flags1 == 10)
                            {
                                var weightValues = Reader.ReadBytes(4);
                                var boneIndices = Reader.ReadBytes(4);
                                for (int w = 0; w < 4; w++)
                                {
                                    if (weightValues[w] != 0)
                                    {
                                        weightCount++;
                                        Weight.BoneValues[w] = boneIndices[w];
                                        Weight.WeightValues[w] = weightValues[w] / 255f;
                                    }
                                }
                            }
                            else
                            {
                                var weightValues = Reader.ReadBytes(8);
                                var boneIndices = Reader.ReadBytes(8);
                                for (int w = 0; w < 8; w++)
                                {
                                    if (weightValues[w] != 0)
                                    {
                                        weightCount++;
                                        Weight.BoneValues[w] = boneIndices[w];
                                        Weight.WeightValues[w] = weightValues[w] / 255f;
                                    }
                                }
                            }
                            Weight.WeightCount = (byte)weightCount;
                            Weights.Add(Weight);
                        }
                    }
                    else
                    {
                        for (int v = 0; v < Header.VertexCount; v++)
                        {
                            var Position = Reader.ReadStruct<Vector3>();
                            Mesh.Positions.Add(Position);
                            var UVU = new Vector2((float)BitConverter.Int16BitsToHalf(Reader.ReadInt16()),
                                (float)BitConverter.Int16BitsToHalf(Reader.ReadInt16()));
                            var UVV = new Vector2((float)BitConverter.Int16BitsToHalf(Reader.ReadInt16()),
                                (float)BitConverter.Int16BitsToHalf(Reader.ReadInt16()));
                            Mesh.UVLayers.Add(UVU, v);
                            //Mesh.UVLayers.Add(UVV, v);
                            var normal = Reader.ReadBytes(4);
                            Mesh.Normals.Add(new Vector3((sbyte)normal[0], (sbyte)normal[1], (sbyte)normal[2]) / 127f);
                            var tangent = Reader.ReadBytes(4);
                            var colours = Reader.ReadBytes(4);
                            Mesh.Colours.Add(new Vector4(colours[0], colours[1], colours[2], colours[3]) / 255f);
                        }
                    }

                    var TriangleCount = Reader.ReadInt32();
                    for (int f = 0; f < TriangleCount / 3; f++)
                    {
                        var v0 = Reader.ReadUInt16();
                        var v1 = Reader.ReadUInt16();
                        var v2 = Reader.ReadUInt16();
                        Mesh.Faces.Add((v2, v1, v0));
                    }
                    Reader.Advance(28);
                    uint BoneIdOffset = 0;
                    if (isMM6)
                    {
                        var facesetCount = Reader.ReadUInt32();
                        for (int t = 0; t < facesetCount; t++)
                        {
                            Reader.Advance(12);
                        }
                        BoneIdOffset = Reader.ReadUInt32();
                        Reader.Advance(11 * 4);

                        for (int v = 0; v < Header.VertexCount; v++)
                        {
                            for (int w = 0; w < Weights[v].WeightCount; w++)
                            {
                                Mesh.Influences.Add(((int)(Weights[v].BoneValues[w] + BoneIdOffset), Weights[v].WeightValues[w]), v);
                            }
                        }
                    }
                    ResultModel.Meshes.Add(Mesh);
                }

                var numMaterials = Reader.ReadInt32();
                for (int i = 0; i < numMaterials; i++)
                {
                    var MaterialPath = Reader.ReadFixedPrefixString();
                    var MaterialHeader = Reader.ReadStruct<VoidMaterial>();
                    var Material = MaterialHelper.GetMaterial(MaterialPath, instance);
                    ResultModel.Materials.Add(Material);
                    ResultModel.Meshes[MaterialHeader.MeshId].Materials.Add(Material);
                }
                return ResultModel;
            }
            finally
            {
                Reader.Close();
                SkeletonStream.Close();
            }
        }

        public static Skeleton BuildVoidSkeleton(byte[] SkeletonBytes, bool Deathloop)
        {
            using var ModelStream = new MemoryStream(SkeletonBytes);
            using var Reader = new BinaryReader(ModelStream);
            try
            {
                var Magic = Reader.ReadFixedString(4);
                if (Magic != "60SE")
                    throw new Exception();

                var Skeleton = new Skeleton();

                var SkeletonHeader = Reader.ReadStruct<VoidSkeleton>();
                var transformOffset = SkeletonHeader.TransformOffset + 0x18;
                var test = (transformOffset - Reader.BaseStream.Position) / 4;
                var parent = Reader.ReadArray<VoidParents>((int)test);
                var transforms = Reader.ReadArray<VoidTransforms>((int)SkeletonHeader.BoneCount);
                Reader.Seek(SkeletonHeader.Data2Offset + 0x1c);
                var udata2 = Reader.ReadArray<ushort>((int)SkeletonHeader.BoneCount);
                if (Reader.BaseStream.Position > SkeletonHeader.Data2EndOffset + 0x2c)
                    throw new Exception();
                Reader.Seek(SkeletonHeader.MainDataOffset + 0x30);
                if (!Deathloop)
                {
                    Reader.Advance(4);
                }

                //For any model that has a base_xxxxx.edgeskel
                //Advancing 6 will put it 2 bytes ahead of where it should be
                //I can't figure out the cause of this, or how to fix it
                //Also for the wolfhound skeleton, they don't have any data3
                //it skips the 6 bytes, reads 105 (data3count)
                //but then goes straight into the bone names
                //On other models, the int before the first bone name size is also the number of bones/data3
                //Oddly these issues only occur in D2/DOTO - not a single model in DL has this skeleton issue
                var SkeletonName = Reader.ReadFixedPrefixString();
                Reader.Advance(6);
                var Data3Count = Reader.ReadUInt16();
                if (Data3Count > 0)
                {
                    var unk = Reader.ReadArray<ushort>(Reader.ReadUInt16());
                    var Data3 = Reader.ReadArray<ushort>(Data3Count);
                }
                for (int i = 0; i < SkeletonHeader.BoneCount; i++)
                {
                    Skeleton.Bones.Add(new(Reader.ReadFixedPrefixString())
                    {
                        BaseLocalTranslation = transforms[i].Position,
                        BaseLocalRotation = new Quaternion(transforms[i].Rotation.X, transforms[i].Rotation.Y, transforms[i].Rotation.Z, transforms[i].Rotation.W),
                        BaseScale = transforms[i].Scale
                    });
                }
                for (int i = 0; i < (Deathloop ? 12 : 10); i++)
                {
                    var boneflags = Reader.ReadBytes((int)SkeletonHeader.BoneCount);
                }
                var ZoneCount = Reader.ReadUInt16();
                if (!Deathloop)
                {
                    for (int i = 0; i < ZoneCount; i++)
                    {
                        var ZoneName = Reader.ReadFixedPrefixString();
                    }
                    for (int i = 0; i < 10; i++)
                    {
                        var zoneflags = Reader.ReadBytes(ZoneCount);
                    }
                }
                for (int i = 0; i < ((SkeletonHeader.BoneCount + 7) / 8) * 8; i++)
                {
                    //This isn't actually used, as all of the transformations are
                    //inherantly in the bones and I'm yet to see an example that
                    //actually NEEDS this
                    var bytes = Reader.ReadBytes(4 * 3 * 4);
                    var floats = UnpackFloats(bytes);
                    // Construct the 4x4 matrix
                    var matrix = new Matrix4x4(
                        floats[0], floats[4], floats[8], 0,
                        floats[1], floats[5], floats[9], 0,
                        floats[2], floats[6], floats[10], 0,
                        floats[3], floats[7], floats[11], 1);
                }
                for (int i = 0; i < parent.Length; i++)
                {
                    if (parent[i].test[1] != short.MaxValue)
                        Skeleton.Bones[parent[i].test[0]].Parent = Skeleton.Bones[parent[i].test[1]];
                }
                Skeleton.GenerateGlobalTransforms();
                return Skeleton;
            }
            finally
            {
                Reader.Close();
                ModelStream.Close();
            }
        }

        private static float[] UnpackFloats(byte[] bytes)
        {
            float[] floats = new float[bytes.Length / 4];
            for (int i = 0; i < floats.Length; i++)
            {
                floats[i] = BitConverter.ToSingle(bytes, i * 4);
            }
            return floats;
        }
    }
}
