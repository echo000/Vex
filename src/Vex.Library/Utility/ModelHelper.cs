using PhilLibX.IO;
using PhilLibX.Media3D;
using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;

namespace Vex.Library.Utility
{
    internal class ModelHelper
    {
        public static Model BuildDishonoredPreviewModel(byte[] ModelBytes)
        {
            using var ModelStream = new MemoryStream(ModelBytes);
            using var Reader = new BinaryReader(ModelStream);

            var Magic = Reader.ReadFixedString(4);
            var isMM6 = Magic != "1LMB";

            if (Magic != "MM6B" && Magic != "MM6@" && Magic != "1LMB")
            {
                throw new Exception();
            }
            var ResultModel = new Model();

            if(!isMM6)
            {
                Reader.Advance(4);
            }

            var SkeletonPath = Reader.ReadFixedPrefixString();
            if (isMM6)
            {
                if (SkeletonPath != "")
                {
                    Reader.Advance(2);
                }
                var boundingMin = Reader.ReadStruct<Vector3>();
                var boundingMax = Reader.ReadStruct<Vector3>();
            }
            var numMeshes = Reader.ReadUInt32();

            for (int i = 0; i < numMeshes; i++)
            {
                var Mesh = new Mesh();
                if (isMM6)
                {
                    var partName = Reader.ReadFixedPrefixString();
                }

                var materialPath = Reader.ReadFixedPrefixString();
                if (isMM6)
                    Reader.Advance(9);
                else
                    Reader.Advance(4);
                var str = Reader.ReadFixedString(4);

                Reader.Advance(8);
                var flags = Reader.ReadInt32();
                Reader.Advance(40);
                var numVertices = Reader.ReadInt32();
                Reader.Advance(8);
                if (isMM6)
                {
                    for (int v = 0; v < numVertices; v++)
                    {
                        var position = Reader.ReadStruct<Vector3>();
                        Mesh.Positions.Add(position);
                        Mesh.Colours.Add(Vector4.One);
                    }
                    for (int v = 0; v < numVertices; v++)
                    {
                        var uvu0 = (float)BitConverter.Int16BitsToHalf(Reader.ReadInt16());
                        var uvu1 = (float)BitConverter.Int16BitsToHalf(Reader.ReadInt16());
                        var uvv0 = (float)BitConverter.Int16BitsToHalf(Reader.ReadInt16());
                        var uvv1 = (float)BitConverter.Int16BitsToHalf(Reader.ReadInt16());
                        var UVU = new Vector2(uvu0, uvu1);
                        var UVV = new Vector2(uvv0, uvv1);
                        Mesh.UVLayers.Add(UVU, v);
                        Mesh.UVLayers.Add(UVV, v);
                    }
                    for (int v = 0; v < numVertices; v++)
                    {
                        var normal = Reader.ReadBytes(4);
                        Mesh.Normals.Add(new Vector3((sbyte)normal[0], (sbyte)normal[1], (sbyte)normal[2]) / 127f);
                    }
                    for (int v = 0; v < numVertices; v++)
                    {
                        var tangent = Reader.ReadBytes(4);
                    }
                    for (int v = 0; v < numVertices; v++)
                    {
                        byte[] weights;
                        if (flags == 10)
                            weights = Reader.ReadBytes(16);
                        else
                            weights = Reader.ReadBytes(8);
                    }
                }
                else
                {
                    for (int v = 0; v < numVertices; v++)
                    {
                        var Position = Reader.ReadStruct<Vector3>();
                        Mesh.Positions.Add(Position);
                        var uvu0 = (float)BitConverter.Int16BitsToHalf(Reader.ReadInt16());
                        var uvu1 = (float)BitConverter.Int16BitsToHalf(Reader.ReadInt16());
                        var uvv0 = (float)BitConverter.Int16BitsToHalf(Reader.ReadInt16());
                        var uvv1 = (float)BitConverter.Int16BitsToHalf(Reader.ReadInt16());
                        var UVU = new Vector2(uvu0, uvu1);
                        var UVV = new Vector2(uvv0, uvv1);
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
                    Mesh.Faces.Add((v0, v1, v2));
                }
                Reader.Advance(28);
                if (isMM6)
                {
                    var throwaway = Reader.ReadUInt32();
                    for (int t = 0; t < throwaway; t++)
                    {
                        Reader.Advance(12);
                    }
                    Reader.Advance(4);
                    Reader.Advance(11 * 4);
                }
                else
                {
                    var tempstr = Reader.ReadFixedString(4);
                }
                ResultModel.Meshes.Add(Mesh);
            }

            var numMaterials = Reader.ReadInt32();

            for(int i = 0; i < numMaterials; i++)
            {
                var Path = Reader.ReadFixedPrefixString();
                if (isMM6 && Magic == "MM6@")
                    Reader.Advance(16);
                else
                {
                    Reader.Advance(12);
                }
            }
            ResultModel.Scale(50);
            return ResultModel;
        }

        public static Model BuildDeathloopPreviewModel(byte[] ModelBytes)
        {
            using var ModelStream = new MemoryStream(ModelBytes);
            using var Reader = new BinaryReader(ModelStream);

            var Magic = Reader.ReadUInt32();
            var isMM6 = Magic != 0x19847D60;
            Reader.Seek(0);

            var ResultModel = new Model();

            if (isMM6)
            {
                var SkeletonPath = Reader.ReadFixedPrefixString();
                if (SkeletonPath != "")
                    Reader.Advance(2);

                var boundingMin = Reader.ReadStruct<Vector3>();
                var boundingMax = Reader.ReadStruct<Vector3>();
            }
            else
            {
                Reader.Advance(12);
            }

            var numMeshes = Reader.ReadUInt32();

            for (int i = 0; i < numMeshes; i++)
            {
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

                if (isMM6)
                    Reader.Advance(9);
                else
                    Reader.Advance(4);
                var str = Reader.ReadFixedString(4);

                Reader.Advance(4);
                var flags = Reader.ReadInt32();
                Reader.Advance(44);
                var numVertices = Reader.ReadInt32();
                Reader.Advance(8); if (isMM6)
                {
                    for (int v = 0; v < numVertices; v++)
                    {
                        var position = Reader.ReadStruct<Vector3>();
                        Mesh.Positions.Add(position);
                        Mesh.Colours.Add(Vector4.One);
                    }
                    for (int v = 0; v < numVertices; v++)
                    {
                        var uvu0 = (float)BitConverter.Int16BitsToHalf(Reader.ReadInt16());
                        var uvu1 = (float)BitConverter.Int16BitsToHalf(Reader.ReadInt16());
                        var uvv0 = (float)BitConverter.Int16BitsToHalf(Reader.ReadInt16());
                        var uvv1 = (float)BitConverter.Int16BitsToHalf(Reader.ReadInt16());
                        var UVU = new Vector2(uvu0, uvu1);
                        var UVV = new Vector2(uvv0, uvv1);
                        Mesh.UVLayers.Add(UVU, v);
                        Mesh.UVLayers.Add(UVV, v);
                    }
                    for (int v = 0; v < numVertices; v++)
                    {
                        var normal = Reader.ReadBytes(4);
                        Mesh.Normals.Add(new Vector3((sbyte)normal[0], (sbyte)normal[1], (sbyte)normal[2]) / 127f);
                    }
                    for (int v = 0; v < numVertices; v++)
                    {
                        var tangent = Reader.ReadBytes(4);
                    }
                    for (int v = 0; v < numVertices; v++)
                    {
                        byte[] weights;
                        if (flags == 10)
                            weights = Reader.ReadBytes(8);
                        else
                            weights = Reader.ReadBytes(16);
                    }
                }
                else
                {
                    for (int v = 0; v < numVertices; v++)
                    {
                        var Position = Reader.ReadStruct<Vector3>();
                        Mesh.Positions.Add(Position);
                        var uvu0 = (float)BitConverter.Int16BitsToHalf(Reader.ReadInt16());
                        var uvu1 = (float)BitConverter.Int16BitsToHalf(Reader.ReadInt16());
                        var uvv0 = (float)BitConverter.Int16BitsToHalf(Reader.ReadInt16());
                        var uvv1 = (float)BitConverter.Int16BitsToHalf(Reader.ReadInt16());
                        var UVU = new Vector2(uvu0, uvu1);
                        var UVV = new Vector2(uvv0, uvv1);
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
                    Mesh.Faces.Add((v0, v1, v2));
                }
                Reader.Advance(28);
                if (isMM6)
                {
                    var throwaway = Reader.ReadUInt32();
                    for(int t = 0; t < throwaway; t++)
                    {
                        Reader.Advance(12);
                    }
                    Reader.Advance(4);
                    Reader.Advance(11 * 4);
                }
                ResultModel.Meshes.Add(Mesh);
            }

            var numMaterials = Reader.ReadInt32();

            for (int i = 0; i < numMaterials; i++)
            {
                var Path = Reader.ReadFixedPrefixString();
                Reader.Advance(12);
            }
            ResultModel.Scale(50);
            return ResultModel;
        }
    }
}
