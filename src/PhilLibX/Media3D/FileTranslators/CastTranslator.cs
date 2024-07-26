using K4os.Hash.xxHash;
using PhilLibX.Media3D.FileTranslators.Cast;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace PhilLibX.Media3D.Cast
{
    /// <summary>
    /// A class to handle translating data from SEAnim files.
    /// </summary>
    public sealed class CastModelTranslator : Graphics3DTranslator
    {
        /// <summary>
        /// Cast Magic
        /// </summary>
        public static readonly byte[] Magic = [0x53, 0x45, 0x41, 0x6E, 0x69, 0x6D];

        /// <inheritdoc/>
        public override string Name => "CastTranslator";

        /// <inheritdoc/>
        public override string[] Extensions { get; } =
        [
            ".cast"
        ];

        /// <inheritdoc/>
        public override bool SupportsReading => false;

        /// <inheritdoc/>
        public override bool SupportsWriting => true;

        /// <inheritdoc/>
        public override void Read(Stream stream, string filePath, Graphics3DTranslatorIO output) => throw new NotSupportedException("Cast format does not support reading");

        private static void WriteCastProperty(BinaryWriter Writer, string name, CastProperty prop)
        {
            Writer.Write((ushort)prop.Identifier);
            Writer.Write((ushort)name.Length);
            Writer.Write((uint)prop.Elements);
            Writer.Write(Encoding.UTF8.GetBytes(name), 0, name.Length);
            if (prop.Elements > 0)
            {
                Writer.Write(prop.Buffer.ToArray(), 0, prop.Buffer.Count);
            }
        }

        private static void WriteCastNode(BinaryWriter Writer, CastNode node)
        {
            Writer.Write((uint)node.Identifier);
            Writer.Write((uint)node.Size());
            Writer.Write(node.Hash);
            Writer.Write((uint)node.Properties.Count);
            Writer.Write((uint)node.Children.Count);

            foreach (var prop in node.Properties)
            {
                WriteCastProperty(Writer, prop.Key, prop.Value);
            }

            foreach (var child in node.Children)
            {
                WriteCastNode(Writer, child);
            }

        }

        private static void WriteCastFile(BinaryWriter Writer, CastNode node)
        {
            Writer.Write(0x74736163);
            Writer.Write(1);
            Writer.Write(1);
            Writer.Write(0);

            WriteCastNode(Writer, node);
        }

        /// <inheritdoc/>
        public override void Write(Stream stream, string filePath, Graphics3DTranslatorIO input)
        {
            var data = input.GetFirstInstance<Model>();
            input.TryGetFirstSkeleton(out var skeleton);
            using var Writer = new BinaryWriter(stream, Encoding.Default, true);
            CastNode CastRoot = new();

            if (data != null)
            {
                var boneCount = skeleton?.Bones.Count ?? 0;

                var CastModel = CastRoot.AddNode(CastNodeID.Model);
                var CastSkeleton = CastModel.AddNode(CastNodeID.Skeleton);

                if (skeleton != null)
                {
                    if (skeleton.Bones.Count > 0)
                        AddSkeletonNodes(CastSkeleton, skeleton, boneCount, input.Scale);
                }

                var BoneIndexType = GetBoneIndexType(boneCount);

                List<ulong> MaterialHashes = [];

                foreach (var Material in data.Materials)
                {
                    var MaterialNameHash = XXH64.DigestOf(Encoding.UTF8.GetBytes(Material.Name));

                    var CastMaterial = CastModel.AddNode(CastNodeID.Material, MaterialNameHash);

                    CastMaterial.SetProperty("n", Material.Name);
                    CastMaterial.SetProperty("t", "pbr");

                    MaterialHashes.Add(CastMaterial.Hash);

                    // Process textures
                    ProcessTexture(Material.Textures, "DiffuseMap", "albedo", CastMaterial);
                    ProcessTexture(Material.Textures, "NormalMap", "normal", CastMaterial);
                    ProcessTexture(Material.Textures, "SpecularMap", "specular", CastMaterial);
                    ProcessTexture(Material.Textures, "EmissiveMap", "emissive", CastMaterial);
                    ProcessTexture(Material.Textures, "GlossMap", "gloss", CastMaterial);
                    ProcessTexture(Material.Textures, "RoughnessMap", "roughness", CastMaterial);
                    ProcessTexture(Material.Textures, "OcclusionMap", "ao", CastMaterial);
                }

                int p = 0;
                foreach (var mesh in data.Meshes)
                {
                    var meshName = p == 0 ? "CastMesh" : $"CastMesh{p}";
                    var MeshNameHash = XXH64.DigestOf(Encoding.UTF8.GetBytes(meshName));
                    p++;

                    var CastMesh = CastModel.AddNode(CastNodeID.Mesh, MeshNameHash);

                    var VertCount = mesh.Positions.Count;
                    var FaceIndexType = VertCount <= 0xFFFF ? VertCount <= 0xFF ? CastPropertyId.Byte : CastPropertyId.Short : CastPropertyId.Integer32;
                    var influences = mesh.Influences.Count <= 0 || boneCount <= 0 ? 0 : mesh.Influences.Dimension;
                    var MaxSkinInfluenceBuffer = 0;

                    // Iterate to dynamically calculate max weight influence
                    if (skeleton != null)
                    {
                        for (int i = 0; i < mesh.Positions.Count; i++)
                        {
                            if (mesh.Influences.CountPerVertex(i) > MaxSkinInfluenceBuffer)
                                MaxSkinInfluenceBuffer = mesh.Influences.CountPerVertex(i);
                        }
                    }

                    byte MaterialCountBuffer = (byte)mesh.Materials.Count;

                    var Positions = CastMesh.AddProperty("vp", CastPropertyId.Vector3, VertCount * Marshal.SizeOf<Vector3>());
                    var Normals = CastMesh.AddProperty("vn", CastPropertyId.Vector3, VertCount * Marshal.SizeOf<Vector3>());
                    var Colours = CastMesh.AddProperty("vc", CastPropertyId.Integer32, VertCount * sizeof(int));
                    var Materials = CastMesh.AddProperty("m", CastPropertyId.Integer64, VertCount * sizeof(long));
                    var FaceIndices = CastMesh.AddProperty("f", FaceIndexType, VertCount * sizeof(int));
                    var BoneWeights = CastMesh.AddProperty("wv", CastPropertyId.Float, VertCount * sizeof(int));
                    var BoneIndices = CastMesh.AddProperty("wb", BoneIndexType);
                    var UVLayer1 = CastMesh.AddProperty("u0", CastPropertyId.Vector2, VertCount * sizeof(float));
                    var UVLayer2 = CastMesh.AddProperty("u1", CastPropertyId.Vector2, VertCount * sizeof(float));

                    if (skeleton != null)
                        CastMesh.SetProperty("mi", CastPropertyId.Byte, (byte)MaxSkinInfluenceBuffer);
                    //These models seem to have 2 UV layers
                    CastMesh.SetProperty("ul", CastPropertyId.Byte, (byte)mesh.UVLayers.Dimension);

                    for (int i = 0; i < VertCount; i++)
                    {
                        byte[] col = [(byte)255, (byte)255, (byte)255, (byte)255];
                        if (mesh.Colours != null && mesh.Colours.Count != 0)
                        {
                            col = [(byte)mesh.Colours[i].X, (byte)mesh.Colours[i].Y, (byte)mesh.Colours[i].Z, (byte)mesh.Colours[i].W];
                        }
                        Colours.Write(BitConverter.ToInt32(col, 0));
                        Positions.Write(mesh.Positions[i]);
                        Normals.Write(mesh.Normals[i]);
                        UVLayer1.Write(mesh.UVLayers[i, 0]);
                        UVLayer2.Write(mesh.UVLayers[i, 1]);

                        if (skeleton != null)
                        {
                            for (int w = 0; w < MaxSkinInfluenceBuffer; w++)
                            {
                                var (index, value) = mesh.Influences[i, w];

                                switch (BoneIndexType)
                                {
                                    case CastPropertyId.Byte:
                                        BoneIndices.Write((byte)index);
                                        BoneWeights.Write(value); break;
                                    case CastPropertyId.Short:
                                        BoneIndices.Write((short)index);
                                        BoneWeights.Write(value); break;
                                    case CastPropertyId.Integer32:
                                        BoneIndices.Write((int)index);
                                        BoneWeights.Write(value); break;
                                }
                            }
                        }
                    }

                    foreach (var Face in mesh.Faces)
                    {
                        if (Face.Item1 == Face.Item2 || Face.Item2 == Face.Item3 || Face.Item3 == Face.Item1)
                            continue;

                        switch (FaceIndexType)
                        {
                            case CastPropertyId.Byte:
                                FaceIndices.Write((byte)Face.Item1);
                                FaceIndices.Write((byte)Face.Item2);
                                FaceIndices.Write((byte)Face.Item3);
                                break;
                            case CastPropertyId.Short:
                                FaceIndices.Write((short)Face.Item1);
                                FaceIndices.Write((short)Face.Item2);
                                FaceIndices.Write((short)Face.Item3);
                                break;
                            case CastPropertyId.Integer32:
                                FaceIndices.Write(Face.Item1);
                                FaceIndices.Write(Face.Item2);
                                FaceIndices.Write(Face.Item3);
                                break;
                        }
                    }

                    Materials.Write(MaterialHashes[data.Materials.IndexOf(mesh.Materials[0])]);

                    //A lot of blendshape stuff
                    if (mesh.DeltaPositions.Count > 0 && data.Morph != null)
                    {
                        Dictionary<int, CastNode> BlendMap = [];
                        for (int i = 0; i < mesh.Positions.Count; i++)
                        {
                            var count = mesh.DeltaPositions.CountPerVertex(i);
                            for (int w = 0; w < count; w++)
                            {
                                var (index, DeltaPosition) = mesh.DeltaPositions[i, w];
                                if (DeltaPosition != Vector3.Zero)
                                {
                                    if (!BlendMap.TryGetValue(index, out var blend))
                                    {
                                        blend = CastModel.AddNode(CastNodeID.BlendShape);
                                        blend.SetProperty("n", data.Morph.Targets[index].Name);
                                        blend.SetProperty("b", CastPropertyId.Integer64, MeshNameHash);
                                        blend.SetProperty("ts", CastPropertyId.Float, 1.0f);
                                        blend.AddProperty("vp", CastPropertyId.Vector3, count * Marshal.SizeOf<Vector3>());
                                        blend.AddProperty("vi", FaceIndexType, count * sizeof(int));
                                        BlendMap[index] = blend;
                                    }
                                    blend.Properties["vp"].Write(mesh.Positions[i] + DeltaPosition);
                                    switch (FaceIndexType)
                                    {
                                        case CastPropertyId.Byte: blend.Properties["vi"].Write((byte)i); break;
                                        case CastPropertyId.Short: blend.Properties["vi"].Write((short)i); break;
                                        case CastPropertyId.Integer32: blend.Properties["vi"].Write(i); break;
                                    }
                                }
                                else
                                {
                                    Trace.WriteLine("Skipping 0 value");
                                }
                            }
                        }
                    }
                }
                WriteCastFile(Writer, CastRoot);
                return;
            }
            else
            {
                var animData = input.GetFirstInstance<Animation>();
                if (animData != null)
                {
                    var boneModifiers = new Dictionary<string, byte>();

                    if (animData.SkeletonAnimation != null)
                    {
                        var animationType = animData.SkeletalTransformType;

                        foreach (var bone in animData.SkeletonAnimation.Targets)
                        {
                            if (bone.TransformType != TransformType.Parent && bone.TransformType != animationType)
                            {
                                switch (bone.TransformType)
                                {
                                    case TransformType.Absolute: boneModifiers[bone.BoneName] = 0; break;
                                    case TransformType.Additive: boneModifiers[bone.BoneName] = 1; break;
                                    case TransformType.Relative: boneModifiers[bone.BoneName] = 2; break;
                                }
                            }
                        }
                    }

                    var CastAnim = CastRoot.AddNode(CastNodeID.Animation);
                    var frameCount = animData.GetAnimationFrameCount();
                    var actionCount = animData.GetAnimationActionCount();
                    var targetCount = animData.SkeletalTargetCount;
                    var transformType = animData.SkeletalTransformType;

                    CastAnim.SetProperty("fr", CastPropertyId.Float, animData.Framerate);
                    CastAnim.SetProperty("lo", CastPropertyId.Byte, Convert.ToByte(0));

                    if (animData.SkeletonAnimation != null)
                    {
                        //Translations
                        foreach (var bone in animData.SkeletonAnimation.Targets)
                        {
                            if (bone.TranslationFrames == null)
                                continue;

                            var XCurve = CastAnim.AddNode(CastNodeID.Curve);
                            var YCurve = CastAnim.AddNode(CastNodeID.Curve);
                            var ZCurve = CastAnim.AddNode(CastNodeID.Curve);

                            XCurve.SetProperty("nn", bone.BoneName);
                            YCurve.SetProperty("nn", bone.BoneName);
                            ZCurve.SetProperty("nn", bone.BoneName);

                            XCurve.SetProperty("kp", "tx");
                            YCurve.SetProperty("kp", "ty");
                            ZCurve.SetProperty("kp", "tz");

                            switch (bone.TransformType)
                            {
                                case TransformType.Absolute:
                                    XCurve.SetProperty("m", "absolute");
                                    YCurve.SetProperty("m", "absolute");
                                    ZCurve.SetProperty("m", "absolute"); break;
                                case TransformType.Additive:
                                    XCurve.SetProperty("m", "additive");
                                    YCurve.SetProperty("m", "additive");
                                    ZCurve.SetProperty("m", "additive"); break;
                                default:
                                    XCurve.SetProperty("m", "relative");
                                    YCurve.SetProperty("m", "relative");
                                    ZCurve.SetProperty("m", "relative"); break;
                            }

                            var KeyFrameBufferType = frameCount <= 0xFFFF ? frameCount <= 0xFF ? CastPropertyId.Byte : CastPropertyId.Short : CastPropertyId.Integer32;

                            var XKeyFrameBuffer = XCurve.AddProperty("kb", KeyFrameBufferType, bone.TranslationFrameCount * frameCount <= 0xFFFF ? frameCount <= 0xFF ? sizeof(byte) : sizeof(short) : sizeof(int));
                            var YKeyFrameBuffer = YCurve.AddProperty("kb", KeyFrameBufferType, bone.TranslationFrameCount * frameCount <= 0xFFFF ? frameCount <= 0xFF ? sizeof(byte) : sizeof(short) : sizeof(int));
                            var ZKeyFrameBuffer = ZCurve.AddProperty("kb", KeyFrameBufferType, bone.TranslationFrameCount * frameCount <= 0xFFFF ? frameCount <= 0xFF ? sizeof(byte) : sizeof(short) : sizeof(int));
                            var XKeyValueBuffer = XCurve.AddProperty("kv", CastPropertyId.Float, bone.TranslationFrameCount * sizeof(float));
                            var YKeyValueBuffer = YCurve.AddProperty("kv", CastPropertyId.Float, bone.TranslationFrameCount * sizeof(float));
                            var ZKeyValueBuffer = ZCurve.AddProperty("kv", CastPropertyId.Float, bone.TranslationFrameCount * sizeof(float));

                            foreach (var Position in bone.TranslationFrames)
                            {
                                XKeyValueBuffer.Write(Position.Value.X * input.Scale);
                                YKeyValueBuffer.Write(Position.Value.Y * input.Scale);
                                ZKeyValueBuffer.Write(Position.Value.Z * input.Scale);

                                switch (KeyFrameBufferType)
                                {
                                    case CastPropertyId.Byte:
                                        XKeyFrameBuffer.Write((byte)Position.Time);
                                        YKeyFrameBuffer.Write((byte)Position.Time);
                                        ZKeyFrameBuffer.Write((byte)Position.Time); break;
                                    case CastPropertyId.Short:
                                        XKeyFrameBuffer.Write((short)Position.Time);
                                        YKeyFrameBuffer.Write((short)Position.Time);
                                        ZKeyFrameBuffer.Write((short)Position.Time); break;
                                    case CastPropertyId.Integer32:
                                        XKeyFrameBuffer.Write((int)Position.Time);
                                        YKeyFrameBuffer.Write((int)Position.Time);
                                        ZKeyFrameBuffer.Write((int)Position.Time); break;
                                }
                            }
                        }

                        //Rotations
                        foreach (var bone in animData.SkeletonAnimation.Targets)
                        {
                            if (bone.RotationFrames == null)
                                continue;

                            var Curve = CastAnim.AddNode(CastNodeID.Curve);

                            Curve.SetProperty("nn", bone.BoneName);
                            Curve.SetProperty("kp", "rq");
                            switch (bone.TransformType)
                            {
                                case TransformType.Absolute:
                                    Curve.SetProperty("m", "absolute"); break;
                                case TransformType.Additive:
                                    Curve.SetProperty("m", "additive"); break;
                                default:
                                    Curve.SetProperty("m", "absolute"); break;
                            }

                            var KeyFrameBufferType = frameCount <= 0xFFFF ? frameCount <= 0xFF ? CastPropertyId.Byte : CastPropertyId.Short : CastPropertyId.Integer32;
                            var KeyFrameBuffer = Curve.AddProperty("kb", KeyFrameBufferType, bone.RotationFrameCount * frameCount <= 0xFFFF ? frameCount <= 0xFF ? sizeof(byte) : sizeof(short) : sizeof(int));
                            var KeyValueBuffer = Curve.AddProperty("kv", CastPropertyId.Vector4, bone.RotationFrameCount * Marshal.SizeOf<Vector4>());

                            foreach (var Rotation in bone.RotationFrames)
                            {
                                KeyValueBuffer.Write(Rotation.Value);

                                switch (KeyFrameBufferType)
                                {
                                    case CastPropertyId.Byte:
                                        KeyFrameBuffer.Write((byte)Rotation.Time); break;
                                    case CastPropertyId.Short:
                                        KeyFrameBuffer.Write((short)Rotation.Time); break;
                                    case CastPropertyId.Integer32:
                                        KeyFrameBuffer.Write((int)Rotation.Time); break;
                                }
                            }
                        }
                    }

                    foreach (var note in animData.Actions)
                    {
                        var CastNode = CastAnim.AddNode(CastNodeID.NotificationTrack);
                        CastNode.SetProperty("n", note.Name);
                        var KeyFrameBufferType = frameCount <= 0xFFFF ? frameCount <= 0xFF ? CastPropertyId.Byte : CastPropertyId.Short : CastPropertyId.Integer32;
                        var KeyFrameBuffer = CastNode.AddProperty("kb", CastPropertyId.Integer32);
                        foreach (var frame in note.Frames)
                        {
                            switch (KeyFrameBufferType)
                            {
                                case CastPropertyId.Byte:
                                    KeyFrameBuffer.Write((byte)frame.Time); break;
                                case CastPropertyId.Short:
                                    KeyFrameBuffer.Write((short)frame.Time); break;
                                case CastPropertyId.Integer32:
                                    KeyFrameBuffer.Write((int)frame.Time); break;
                            }
                        }
                    }

                    foreach (var BoneModifier in boneModifiers)
                    {
                        var CastOverride = CastAnim.AddNode(CastNodeID.CurveModeOverride);
                        CastOverride.SetProperty("nn", BoneModifier.Key);
                        CastOverride.SetProperty("ot", CastPropertyId.Byte, (byte)1);
                        switch ((TransformType)BoneModifier.Value)
                        {
                            case TransformType.Absolute:
                                CastOverride.SetProperty("m", "absolute"); break;
                            case TransformType.Additive:
                                CastOverride.SetProperty("m", "additive"); break;
                            default:
                                CastOverride.SetProperty("m", "relative"); break;
                        }
                    }

                    if (animData.MorphAnimation != null && animData.MorphTargetCount > 0)
                    {
                        foreach (var morph in animData.MorphAnimation.Targets)
                        {
                            if (morph.TranslationFrames == null)
                                continue;

                            var MorphCurve = CastAnim.AddNode(CastNodeID.Curve);
                            MorphCurve.SetProperty("nn", morph.MorphName);
                            MorphCurve.SetProperty("kp", "bs");
                            MorphCurve.SetProperty("m", "absolute");

                            var KeyFrameBufferType = frameCount <= 0xFFFF ? frameCount <= 0xFF ? CastPropertyId.Byte : CastPropertyId.Short : CastPropertyId.Integer32;
                            var KeyFrameBuffer = MorphCurve.AddProperty("kb", KeyFrameBufferType, morph.TranslationFrameCount * frameCount <= 0xFFFF ? frameCount <= 0xFF ? sizeof(byte) : sizeof(short) : sizeof(int));
                            var KeyValueBuffer = MorphCurve.AddProperty("kv", CastPropertyId.Float, morph.TranslationFrameCount * sizeof(float));
                            foreach (var frame in morph.TranslationFrames)
                            {
                                KeyValueBuffer.Write(frame.Value.X);

                                switch (KeyFrameBufferType)
                                {
                                    case CastPropertyId.Byte:
                                        KeyFrameBuffer.Write((byte)frame.Time); break;
                                    case CastPropertyId.Short:
                                        KeyFrameBuffer.Write((short)frame.Time); break;
                                    case CastPropertyId.Integer32:
                                        KeyFrameBuffer.Write((int)frame.Time); break;
                                }
                            }
                        }
                    }
                    WriteCastFile(Writer, CastRoot);
                }
            }
        }

        /// <inheritdoc/>
        public override bool IsValid(Span<byte> startOfFile, Stream stream, string? filePath, string? ext)
        {
            return !string.IsNullOrWhiteSpace(ext) && Extensions.Contains(ext);
        }

        static void ProcessTexture(Dictionary<string, Texture> textures, string textureKey, string propertyKey, CastNode castMaterial)
        {
            if (textures.TryGetValue(textureKey, out var texture))
            {
                var castTexture = castMaterial.AddNode(CastNodeID.File, XXH64.DigestOf(Encoding.UTF8.GetBytes(texture.Name)));
                castTexture.SetProperty("p", texture.Name);
                castMaterial.SetProperty(propertyKey, CastPropertyId.Integer64, castTexture.Hash);
            }
            /*            else
                        {
                            var castTexture = castMaterial.AddNode(CastNodeID.File, XXH64.DigestOf(Encoding.UTF8.GetBytes("")));
                            castTexture.SetProperty("p", "");
                            castMaterial.SetProperty(propertyKey, CastPropertyId.Integer64, castTexture.Hash);
                        }*/
        }

        static CastPropertyId GetBoneIndexType(int boneCount)
        {
            return boneCount switch
            {
                <= 0xFF => CastPropertyId.Byte,
                <= 0xFFFF => CastPropertyId.Short,
                _ => CastPropertyId.Integer32
            };
        }

        static void AddSkeletonNodes(CastNode CastSkeleton, Skeleton skeleton, int boneCount, float scale)
        {
            for (int i = 0; i < boneCount; i++)
            {
                SkeletonBone? bone = skeleton.Bones[i];
                var CastBone = CastSkeleton.AddNode(CastNodeID.Bone);

                CastBone.SetProperty("n", bone.Name);
                CastBone.SetProperty("p", CastPropertyId.Integer32, (bone.Parent == null) ? -1 : skeleton.Bones.IndexOf(bone.Parent));
                CastBone.SetProperty("lr", CastPropertyId.Vector4, bone.BaseLocalRotation);
                CastBone.SetProperty("lp", CastPropertyId.Vector3, bone.BaseLocalTranslation);
                CastBone.SetProperty("wr", CastPropertyId.Vector4, bone.BaseWorldRotation);
                CastBone.SetProperty("wp", CastPropertyId.Vector3, bone.BaseWorldTranslation);
                CastBone.SetProperty("s", CastPropertyId.Vector3, bone.BaseScale * scale);
            }
        }


        /// <summary>
        /// Reads a UTF8 string from the file
        /// </summary>
        internal static string ReadUTF8String(BinaryReader reader)
        {
            var output = new StringBuilder(32);

            while (true)
            {
                var c = reader.ReadByte();
                if (c == 0)
                    break;
                output.Append(Convert.ToChar(c));
            }

            return output.ToString();
        }
    }
}
