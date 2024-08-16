using DirectXTex;
using PhilLibX.IO;
using System;
using System.Collections.Generic;
using System.IO;

namespace Vex.Library.Utility
{
    public class BImage
    {
        #region CONSTANTS

        const uint SUPPORTED_VERSION = 24;
        const uint MAGIC = 0x004D4942 | (SUPPORTED_VERSION << 24);

        #endregion

        #region NESTED TYPES

        public class PixelFormat
        {
            #region ENUMS

            public enum Layout
            {
                LAYOUT_NONE = 0,

                LAYOUT_8,
                LAYOUT_16,
                LAYOUT_32,

                LAYOUT_8_8,
                LAYOUT_16_16,
                LAYOUT_32_32,

                LAYOUT_8_8_8,
                LAYOUT_16_16_16,
                LAYOUT_32_32_32,

                LAYOUT_8_8_8_8,
                LAYOUT_16_16_16_16,
                LAYOUT_32_32_32_32,

                LAYOUT_10_10_10_2,
                //	LAYOUT_4_4_4_4
                //	LAYOUT_5_6_5
                //	LAYOUT_5_5_5_1

                LAYOUT_BC1,
                LAYOUT_BC2,
                LAYOUT_BC3,
                LAYOUT_BC4,
                LAYOUT_BC5,
                LAYOUT_BC6,
                LAYOUT_BC7,

                LAYOUT_16_8, //Depth-stencil, NB: not supported on D3D
                LAYOUT_24_8, //Depth-stencil, NB: not supported on Mantle
                LAYOUT_32_8, //Depth-stencil, NB: some platforms have internally seperated surfaces (one 32 bits and one 8 bits) - ATI, Durango, Orbis, and others may rely on one 64 bits surfaces (the second one will be D32S8X24) - NVidia ?

                LAYOUT_R11G11B10,

                LAYOUT_MAX
            };

            public enum Type
            {
                NONE = 0,

                TYPELESS,
                SINT,
                UINT,
                SNORM,
                UNORM,
                UNORM_sRGB, //gamma corrected format
                FLOAT,

                MAX,
            };

            public enum Swizzle
            {
                NONE = 0,

                RGBA,
                ARGB,
                BGRA,

                DEPTH,
                DEPTH_STENCIL,

                MAX
            };

            #endregion

            public Layout m_layout = Layout.LAYOUT_NONE;
            public Type m_type = Type.NONE;
            public Swizzle m_swizzle = Swizzle.NONE;

            public bool IsCompressed { get { return m_layout >= Layout.LAYOUT_BC1 && m_layout <= Layout.LAYOUT_BC7; } }
            public bool IsDepth { get { return m_swizzle == Swizzle.DEPTH || m_swizzle == Swizzle.DEPTH_STENCIL; } }
            public bool HasStencil { get { return m_swizzle == Swizzle.DEPTH_STENCIL; } }
            public bool Is_sRGB { get { return m_type == Type.UNORM_sRGB; } }

            public uint GetBitsCount()
            {
                return m_layout switch
                {
                    Layout.LAYOUT_8 => 8,
                    Layout.LAYOUT_16 => 16,
                    Layout.LAYOUT_32 => 32,
                    Layout.LAYOUT_8_8 => 16,
                    Layout.LAYOUT_16_16 => 32,
                    Layout.LAYOUT_32_32 => 64,
                    Layout.LAYOUT_8_8_8 => 24,
                    Layout.LAYOUT_16_16_16 => 48,
                    Layout.LAYOUT_32_32_32 => 96,
                    Layout.LAYOUT_8_8_8_8 => 32,
                    Layout.LAYOUT_16_16_16_16 => 64,
                    Layout.LAYOUT_32_32_32_32 => 128,
                    Layout.LAYOUT_10_10_10_2 => 32,
                    Layout.LAYOUT_R11G11B10 => 32,
                    Layout.LAYOUT_BC1 => 4,
                    Layout.LAYOUT_BC2 => 8,
                    Layout.LAYOUT_BC3 => 8,
                    Layout.LAYOUT_BC4 => 4,
                    Layout.LAYOUT_BC5 => 8,
                    Layout.LAYOUT_BC6 => 8,
                    Layout.LAYOUT_BC7 => 8,
                    _ => throw new Exception("Invalid Format"),
                };
            }

            public DirectXTexUtility.DXGIFormat GetDirectXFormat()
            {
                switch (m_layout)
                {
                    case Layout.LAYOUT_8:
                        {
                            switch (m_type)
                            {
                                case Type.UNORM: return DirectXTexUtility.DXGIFormat.R8UNORM;
                                case Type.SNORM: return DirectXTexUtility.DXGIFormat.R8SNORM;
                                case Type.UINT: return DirectXTexUtility.DXGIFormat.R8UINT;
                                case Type.SINT: return DirectXTexUtility.DXGIFormat.R8SINT;
                                case Type.TYPELESS: return DirectXTexUtility.DXGIFormat.R8TYPELESS;
                            }
                            break;
                        }
                    case Layout.LAYOUT_8_8:
                        {
                            switch (m_type)
                            {
                                case Type.UNORM: return DirectXTexUtility.DXGIFormat.R8G8UNORM;
                                case Type.SNORM: return DirectXTexUtility.DXGIFormat.R8G8SNORM;
                                case Type.UINT: return DirectXTexUtility.DXGIFormat.R8G8UINT;
                                case Type.SINT: return DirectXTexUtility.DXGIFormat.R8G8SINT;
                                case Type.TYPELESS: return DirectXTexUtility.DXGIFormat.R8G8TYPELESS;
                            }
                            break;
                        }
                    case Layout.LAYOUT_8_8_8_8:
                        {
                            switch (m_type)
                            {
                                case Type.UNORM: return DirectXTexUtility.DXGIFormat.R8G8B8A8UNORM;
                                case Type.UNORM_sRGB: return DirectXTexUtility.DXGIFormat.R8G8B8A8UNORMSRGB;
                                case Type.SNORM: return DirectXTexUtility.DXGIFormat.R8G8B8A8SNORM;
                                case Type.UINT: return DirectXTexUtility.DXGIFormat.R8G8B8A8UINT;
                                case Type.SINT: return DirectXTexUtility.DXGIFormat.R8G8B8A8SINT;
                                case Type.TYPELESS: return DirectXTexUtility.DXGIFormat.R8G8B8A8TYPELESS;
                            }
                            break;
                        }
                    case Layout.LAYOUT_16:
                        {
                            switch (m_type)
                            {
                                case Type.UNORM: return DirectXTexUtility.DXGIFormat.R16UNORM;
                                case Type.FLOAT: return DirectXTexUtility.DXGIFormat.R16FLOAT;
                                case Type.UINT: return DirectXTexUtility.DXGIFormat.R16UINT;
                                case Type.SINT: return DirectXTexUtility.DXGIFormat.R16SINT;
                                case Type.TYPELESS: return DirectXTexUtility.DXGIFormat.R16TYPELESS;
                            }
                            break;
                        }
                    case Layout.LAYOUT_16_16:
                        {
                            switch (m_type)
                            {
                                case Type.UNORM: return DirectXTexUtility.DXGIFormat.R16G16UNORM;
                                case Type.SNORM: return DirectXTexUtility.DXGIFormat.R16G16SNORM;
                                case Type.FLOAT: return DirectXTexUtility.DXGIFormat.R16G16FLOAT;
                                case Type.UINT: return DirectXTexUtility.DXGIFormat.R16G16UINT;
                                case Type.SINT: return DirectXTexUtility.DXGIFormat.R16G16SINT;
                                case Type.TYPELESS: return DirectXTexUtility.DXGIFormat.R16G16TYPELESS;
                            }
                            break;
                        }
                    case Layout.LAYOUT_16_16_16_16:
                        {
                            switch (m_type)
                            {
                                case Type.UNORM: return DirectXTexUtility.DXGIFormat.R16G16B16A16UNORM;
                                case Type.FLOAT: return DirectXTexUtility.DXGIFormat.R16G16B16A16FLOAT;
                                case Type.SNORM: return DirectXTexUtility.DXGIFormat.R16G16B16A16SNORM;
                                case Type.UINT: return DirectXTexUtility.DXGIFormat.R16G16B16A16UINT;
                                case Type.SINT: return DirectXTexUtility.DXGIFormat.R16G16B16A16SINT;
                                case Type.TYPELESS: return DirectXTexUtility.DXGIFormat.R16G16B16A16TYPELESS;
                            }
                            break;
                        }
                    case Layout.LAYOUT_32:
                        {
                            switch (m_type)
                            {
                                case Type.FLOAT: return DirectXTexUtility.DXGIFormat.R32FLOAT;
                                case Type.UINT: return DirectXTexUtility.DXGIFormat.R32UINT;
                                case Type.SINT: return DirectXTexUtility.DXGIFormat.R32SINT;
                            }
                            break;
                        }
                    case Layout.LAYOUT_32_32:
                        {
                            switch (m_type)
                            {
                                case Type.FLOAT: return DirectXTexUtility.DXGIFormat.R32G32FLOAT;
                                case Type.UINT: return DirectXTexUtility.DXGIFormat.R32G32UINT;
                                case Type.SINT: return DirectXTexUtility.DXGIFormat.R32G32SINT;
                            }
                            break;
                        }
                    case Layout.LAYOUT_32_32_32_32:
                        {
                            switch (m_type)
                            {
                                case Type.FLOAT: return DirectXTexUtility.DXGIFormat.R32G32B32A32FLOAT;
                                case Type.UINT: return DirectXTexUtility.DXGIFormat.R32G32B32A32UINT;
                                case Type.SINT: return DirectXTexUtility.DXGIFormat.R32G32B32A32SINT;
                                case Type.TYPELESS: return DirectXTexUtility.DXGIFormat.R32G32B32A32TYPELESS;
                            }
                            break;
                        }
                    case Layout.LAYOUT_10_10_10_2:
                        {
                            switch (m_type)
                            {
                                case Type.TYPELESS: return DirectXTexUtility.DXGIFormat.R10G10B10A2TYPELESS;
                                case Type.UNORM: return DirectXTexUtility.DXGIFormat.R10G10B10A2UNORM;
                                case Type.UINT: return DirectXTexUtility.DXGIFormat.R10G10B10A2UINT;
                            }
                            break;
                        }
                    case Layout.LAYOUT_24_8:
                        {
                            switch (m_type)
                            {
                                case Type.TYPELESS: return DirectXTexUtility.DXGIFormat.R24G8TYPELESS;
                            }
                            break;
                        }
                    case Layout.LAYOUT_32_8:
                        {
                            switch (m_type)
                            {
                                case Type.TYPELESS: return DirectXTexUtility.DXGIFormat.R32G8X24TYPELESS;
                            }
                            break;
                        }
                    case Layout.LAYOUT_R11G11B10:
                        {
                            switch (m_type)
                            {
                                case Type.FLOAT: return DirectXTexUtility.DXGIFormat.R11G11B10FLOAT;
                            }
                            break;
                        }
                    case Layout.LAYOUT_BC1:
                        switch (m_type)
                        {
                            case Type.UNORM: return DirectXTexUtility.DXGIFormat.BC1UNORM;
                            case Type.TYPELESS: return DirectXTexUtility.DXGIFormat.BC1TYPELESS;
                            case Type.UNORM_sRGB: return DirectXTexUtility.DXGIFormat.BC1UNORMSRGB;
                        }
                        break;
                    case Layout.LAYOUT_BC3:
                        {
                            switch (m_type)
                            {
                                case Type.UNORM: return DirectXTexUtility.DXGIFormat.BC3UNORM;
                                case Type.TYPELESS: return DirectXTexUtility.DXGIFormat.BC3TYPELESS;
                                case Type.UNORM_sRGB: return DirectXTexUtility.DXGIFormat.BC3UNORMSRGB;
                            }
                            break;
                        }
                    case Layout.LAYOUT_BC4:
                        {
                            switch (m_type)
                            {
                                case Type.UNORM: return DirectXTexUtility.DXGIFormat.BC4UNORM;
                                case Type.TYPELESS: return DirectXTexUtility.DXGIFormat.BC4TYPELESS;
                                case Type.SNORM: return DirectXTexUtility.DXGIFormat.BC4SNORM;
                            }
                            break;
                        }
                    case Layout.LAYOUT_BC5:
                        {
                            switch (m_type)
                            {
                                case Type.UNORM: return DirectXTexUtility.DXGIFormat.BC5UNORM;
                                case Type.SNORM: return DirectXTexUtility.DXGIFormat.BC5SNORM;
                                case Type.TYPELESS: return DirectXTexUtility.DXGIFormat.BC5TYPELESS;
                            }
                            break;
                        }
                    case Layout.LAYOUT_BC6:
                        {
                            switch (m_type)
                            {
                                case Type.UNORM: return DirectXTexUtility.DXGIFormat.BC6HUF16;
                                case Type.UINT: return DirectXTexUtility.DXGIFormat.BC6HUF16;
                                case Type.SNORM: return DirectXTexUtility.DXGIFormat.BC6HSF16;
                                case Type.TYPELESS: return DirectXTexUtility.DXGIFormat.BC6HTYPELESS;
                            }
                            break;
                        }
                    case Layout.LAYOUT_BC7:
                        {
                            switch (m_type)
                            {
                                case Type.UNORM: return DirectXTexUtility.DXGIFormat.BC7UNORM;
                                case Type.UNORM_sRGB: return DirectXTexUtility.DXGIFormat.BC7UNORMSRGB;
                                case Type.TYPELESS: return DirectXTexUtility.DXGIFormat.BC7TYPELESS;
                            }
                            break;
                        }
                    default:
                        throw new Exception("Unsupported image format " + ToString());
                }
                throw new Exception("Unsupported image format " + ToString());
            }

            public void Read(BinaryReader Reader)
            {
                m_layout = (Layout)Reader.ReadUInt32();
                m_type = (Type)Reader.ReadUInt32();
                m_swizzle = (Swizzle)Reader.ReadUInt32();
            }

            public override string ToString()
            {
                return "Layout " + m_layout + " - Type " + m_type + " - Swizzle " + m_swizzle;
            }
        }

        public class ImageOptions
        {

            const uint SUPPORTED_IMAGEOPTS_VERSION_DH = 19;
            const uint SUPPORTED_IMAGEOPTS_VERSION_DL = 20;

            public enum TYPE
            {
                TT_2D,
                TT_3D,
                TT_CUBIC,
            }

            [Flags]
            public enum FLAGS
            {
                GENERATE_MIPS = 1 << 0,
                LINEAR = 1 << 1,        // don't use tiled layout to allow fast CPU addressing
                PARTIALLY_RESIDENT = 1 << 2,        // // PC, Durango, Orbis: partially resident
                                                    // 				CUBE_FILTER				= 1 << 3,		// perform power map mip level filtering on environment maps
                OVERLAY_MEMORY = 1 << 4,        // allocate from the dedicated overlay memory block on consoles
                START_PURGED = 1 << 5,      // don't do the Alloc() when created
                CAN_BE_UAV = 1 << 6,        // can be bind as an UAV // ARKANE : nsilvagni () add an image view object
                DONT_SKIP_MIPS = 1 << 7,        // don't skip higher mips when r_skipMipMaps > 0
                USE_ESRAM = 1 << 8,     //ARKANE: gmarion (2013-09-23) - for Durango
                USE_HTILE_OR_CMASK = 1 << 9,        // only for orbis for now
                STAGING_CPU_WRITE_ONLY = 1 << 10,       // indicate the staging texture is cpu writable, used only for staging on PC_D3D which are read by default

                LAST_USED_BIT = STAGING_CPU_WRITE_ONLY
            }

            public TYPE m_type;
            public PixelFormat m_format = new();
            public uint m_curWidth;
            public uint m_curHeight;            // not needed for cube maps
            public uint m_minWidth;
            public uint m_minHeight;            // not needed for cube maps
            public uint m_maxWidth;
            public uint m_maxHeight;            // not needed for cube maps
            public uint m_depth;                // only needed for 3D maps
            public uint m_curNumLevels;
            public uint m_pendingNumLevels;     // != curNumLevels if a streaming request / upload is pending
            public uint m_minNumLevels;
            public uint m_maxNumLevels;
            public uint m_arraySize;
            public uint m_numSamples;           // number of samples of a multisampled texture's image
            public ushort m_flags;

            public void SetFixedNumLevels(uint levels)
            {
                m_curNumLevels = levels;
                m_minNumLevels = levels;
                m_maxNumLevels = levels;
                m_pendingNumLevels = levels;
            }
            public void SetFixedWidth(uint width)
            {
                m_curWidth = width;
                m_minWidth = width;
                m_maxWidth = width;
            }
            public void SetFixedHeight(uint height)
            {
                m_curHeight = height;
                m_minHeight = height;
                m_maxHeight = height;
            }

            public void Read(BinaryReader Reader)
            {
                uint version = Reader.ReadUInt32();
                if (version != SUPPORTED_IMAGEOPTS_VERSION_DH && version != SUPPORTED_IMAGEOPTS_VERSION_DL)
                    throw new Exception($"Unsupported image options version {version} (supported version is {SUPPORTED_IMAGEOPTS_VERSION_DH} OR {SUPPORTED_IMAGEOPTS_VERSION_DL})!");

                m_type = (TYPE)Reader.ReadUInt32();
                m_format.Read(Reader);

                m_minWidth = Reader.ReadUInt32();
                m_minHeight = Reader.ReadUInt32();
                m_maxWidth = Reader.ReadUInt32();
                m_maxHeight = Reader.ReadUInt32();
                m_depth = Reader.ReadUInt32();
                m_minNumLevels = Reader.ReadUInt32();
                m_maxNumLevels = Reader.ReadUInt32();
                if (version == SUPPORTED_IMAGEOPTS_VERSION_DH)
                {
                    m_arraySize = Reader.ReadUInt32();
                    m_flags = Reader.ReadUInt16();
                }
                else
                {
                    Reader.Advance(4);
                    m_arraySize = Reader.ReadUInt32();
                    m_flags = Reader.ReadUInt16();
                }
            }
        }

        [System.Diagnostics.DebuggerDisplay("Mip {m_MipLevel.ToString( \"G4\" )} {m_Width.ToString( \"G4\" )}x{m_Height.ToString( \"G4\" )} Slice {m_SliceIndex.ToString( \"G4\" )} Size={m_Content.Length}")]
        public class ImageSlice
        {
            public BImage m_Owner = null;

            public uint m_MipLevel;
            public uint m_SliceIndex;
            public uint m_Width;
            public uint m_Height;

            public byte[] m_Content = null;

            public ImageSlice(BImage _Owner, BinaryReader _R, uint _MipOffset)
            {
                m_Owner = _Owner;
                Read(_R, _MipOffset);
            }

            public ImageSlice(BImage _Owner, byte[] bytes, int _MipOffset, uint width, uint height)
            {
                m_Owner = _Owner;
                m_MipLevel = (uint)_MipOffset;
                m_SliceIndex = 0;
                m_Width = width;
                m_Height = height;
                m_Content = bytes;
            }

            public void Read(BinaryReader Reader, uint _MipOffset)
            {
                m_MipLevel = _MipOffset + Reader.ReadUInt32();
                m_SliceIndex = Reader.ReadUInt32();
                m_Width = Reader.ReadUInt32();
                m_Height = Reader.ReadUInt32();

                int ContentSize = (int)Reader.ReadUInt32();
                if (!m_Owner.m_Opts.m_format.IsDepth && ContentSize != m_Width * m_Height * (m_Owner.m_Opts.m_format.GetBitsCount() / 8f))
                    throw new Exception("Unexpected content size!");

                m_Content = new byte[ContentSize];
                Reader.Read(m_Content, 0, ContentSize);
            }
        }

        #endregion

        #region FIELDS

        public uint m_sourceFileTime;
        public uint m_Magic;
        public ImageOptions m_Opts = new();
        public ImageSlice[] m_Slices = [];

        #endregion

        #region METHODS

        public BImage(byte[] ImageBytes, string AssetName, VexInstance instance)
        {
            using var Stream = new MemoryStream(ImageBytes);
            using var Reader = new BinaryReader(Stream);

            //It seems deathloop files just don't have magic in them?
            if (instance.Game == SupportedGames.Dishonored2)
            {
                m_Magic = Reader.ReadUInt32();
                if (m_Magic != MAGIC)
                    throw new Exception("Image has unsupported magic!");
            }

            m_Opts.Read(Reader);

            uint mipsCountInFile = m_Opts.m_minNumLevels;
            uint mipsCountTotal = m_Opts.m_maxNumLevels;

            List<ImageSlice> Slices = [];
            if (mipsCountInFile < mipsCountTotal)
            {
                // This means the largest mips are stored elsewhere, for streaming purpose...
                var ImageMips = instance.VoidSupport.GetEntriesFromName(AssetName + "_mip");
                ImageMips.Sort((entry1, entry2) => string.CompareOrdinal(entry1.Name, entry2.Name));
                for (int i = 0; i < ImageMips.Count; i++)
                {
                    if (instance.Game == SupportedGames.Dishonored2)
                    {
                        var bytes = instance.VoidSupport.ExtractEntryBytes(ImageMips[i], instance);
                        using var MipS = new MemoryStream(bytes);
                        using var MipR = new BinaryReader(MipS);
                        Slices.Add(new ImageSlice(this, MipR, 0U));
                    }
                    //This is REALLY hacky but because of the way that _mip* files are stored in Deathloop
                    //They don't have any header data, its only image data, so we have to work out the width and height
                    //based on the mip level
                    else
                    {
                        var bytes = instance.VoidSupport.ExtractEntryBytes(ImageMips[i], instance);
                        Slices.Add(new ImageSlice(this, bytes, i, m_Opts.m_maxWidth / (uint)(i + 1), m_Opts.m_maxHeight / (uint)(i + 1)));
                    }
                }
            }

            m_Opts.SetFixedNumLevels(mipsCountTotal);
            m_Opts.SetFixedWidth(Math.Max(1U, m_Opts.m_maxWidth));
            m_Opts.SetFixedHeight(Math.Max(1U, m_Opts.m_maxHeight));

            uint totalSlicesInFile = mipsCountInFile * m_Opts.m_arraySize;
            if (m_Opts.m_type == ImageOptions.TYPE.TT_3D)
            {
                if (mipsCountInFile != mipsCountTotal)
                    throw new Exception("Min & Max mips count are the same! Can't compute depth reduction on texture 3D!");

                totalSlicesInFile = 0;
                uint depth = m_Opts.m_depth;
                for (int mipLevelIndex = 0; mipLevelIndex < mipsCountInFile; mipLevelIndex++)
                {
                    totalSlicesInFile += depth;
                    depth = Math.Max(1, depth >> 1);
                }
            }
            else if (m_Opts.m_type == ImageOptions.TYPE.TT_CUBIC)
            {
                totalSlicesInFile = mipsCountInFile * 6;
            }

            uint mipOffset = (uint)Slices.Count;
            if (instance.Game == SupportedGames.Dishonored2)
            {
                Reader.Advance(4);
            }
            else
            {
                Reader.Advance(1);
                var unk1 = Reader.ReadUInt32();
                var unk2 = Reader.ReadUInt32();
            }

            for (uint i = 0; i < totalSlicesInFile; i++)
            {
                Slices.Add(new ImageSlice(this, Reader, mipOffset));
            }

            m_Slices = [.. Slices];
        }
        #endregion
    }
}
