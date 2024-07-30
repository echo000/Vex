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
                            Mesh.UVLayers.Add(UVU, v);
                            Mesh.UVLayers.Add(UVV, v);
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
                            Mesh.UVLayers.Add(UVU, v);
                            Mesh.UVLayers.Add(UVV, v);
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
                if(numMaterials == 0)
                {
                    var Material = new Material("CastMaterial");
                    ResultModel.Materials.Add(Material);
                    for(int i = 0; i < ResultModel.Meshes.Count; i++)
                    {
                        ResultModel.Meshes[i].Materials.Add(Material);
                    }
                }
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
            using var Stream = new MemoryStream(ModelBytes);
            using var Reader = new BinaryReader(Stream);
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
                            Mesh.UVLayers.Add(UVV, v);
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
                            Mesh.UVLayers.Add(UVV, v);
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
                Stream.Close();
            }
        }

        public static Skeleton BuildVoidSkeleton(byte[] SkeletonBytes)
        {
            using var Stream = new MemoryStream(SkeletonBytes);
            using var Reader = new BinaryReader(Stream);
            try
            {
                var SkeletonHeader = Reader.ReadStruct<VoidSkeletonHeader>();
                if (SkeletonHeader.Magic != 0x45533036)
                    throw new Exception();

                var Skeleton = new Skeleton();
                var jointLinkage = Reader.ReadArray<ushort>((int)SkeletonHeader.JointLinkageCount * 2);
                var transforms = Reader.ReadArray<VoidTransforms>(SkeletonHeader.BoneCount);

                List<uint> Hashes = [];
                Reader.Seek(SkeletonHeader.JointNameHashArrayOffset + 0x20);
                for (int i = 0; i < SkeletonHeader.BoneCount; i++)
                {
                    Hashes.Add(Reader.ReadUInt32());
                }

                //The real bone names are stored in the Custom Data potion of the Skeleton file
                //However it's never in the same section of the Custom Data, so is not easily accessible
                //Hashes (JointNameHashArray) are the EdgeAnimGenerateNameHash() of the bone names
                //First bone EdgeAnimGenerateNameHash("origin") is hashed as 554609121
                for (int i = 0; i < SkeletonHeader.BoneCount; i++)
                {
                    Skeleton.Bones.Add(new($"Bone_{Hashes[i]:x}")
                    {
                        BaseLocalTranslation = transforms[i].Position,
                        BaseLocalRotation = new Quaternion(transforms[i].Rotation.X, transforms[i].Rotation.Y, transforms[i].Rotation.Z, transforms[i].Rotation.W),
                        BaseScale = transforms[i].Scale
                    });
                }
                for (int i = 0; i < SkeletonHeader.JointLinkageCount; i++)
                {
                    ushort idJoint = jointLinkage[2 * i + 0];
                    ushort idParent = jointLinkage[2 * i + 1];
                    int parent = (idParent & 0x7fff) >= 0x4000 ? -1 : (idParent & 0x7fff);

                    Skeleton.Bones[idJoint].Parent = parent != -1 ? Skeleton.Bones[parent] : null;
                }
                Skeleton.GenerateGlobalTransforms();
                return Skeleton;
            }
            finally
            {
                Reader.Close();
                Stream.Close();
            }
        }
    }
}
