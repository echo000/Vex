using PhilLibX.Cryptography.Hash;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;

namespace Vex.Library
{

    public enum AssetType
    {
        Animation,
        Image,
        Model,
        Sound,
        RawFile,
        Material,
        Unknown
    }

    public enum AssetStatus
    {
        Loaded,
        Exported,
        NotLoaded,
        Placeholder,
        Processing,
        Error
    }

    /// <summary>
    /// A class to hold a generic asset
    /// </summary>
    public class Asset : Notifiable, IDisposable, IComparable
    {
        /// <summary>
        /// Gets or Sets the Asset Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or Sets the Asset Pointer
        /// </summary>
        public long AssetPointer { get; set; }

        /// <summary>
        /// Gets or Sets whether or not this is a file entry
        /// </summary>
        public bool FileEntry { get; set; }

        /// <summary>
        /// Gets or Sets the Asset Information
        /// </summary>
        public string InformationString { get; set; }

        /// <summary>
        /// Gets or Sets the Asset Size
        /// </summary>
        public long AssetSize { get; set; }

        /// <summary>
        /// Gets or Sets if the Asset is streamed
        /// </summary>
        public bool Streamed { get; set; }

        /// <summary>
        /// Gets or Sets the Asset Type
        /// </summary>
        public AssetType Type
        {
            get => GetValue<AssetType>(nameof(Type));
            set
            {
                SetValue(value, nameof(Type));
                // Update Foreground
                NotifyPropertyChanged(nameof(TypeForegroundColor));
            }
        }

        /// <summary>
        /// Gets or Sets the Asset Status
        /// </summary>
        public AssetStatus Status
        {
            get => GetValue<AssetStatus>(nameof(Status));
            set
            {
                SetValue(value, nameof(Status));
                // Update Foreground
                NotifyPropertyChanged(nameof(StatusForegroundColor));
            }
        }

        /// <summary>
        /// Gets the Asset Display Name
        /// </summary>
        public string DisplayName => Path.GetFileName(Name);

        public Brush StatusForegroundColor => Status switch
        {
            AssetStatus.Loaded => new SolidColorBrush(Color.FromRgb(35, 206, 107)),
            AssetStatus.Exported => new SolidColorBrush(Color.FromRgb(33, 184, 235)),
            AssetStatus.Processing => new SolidColorBrush(Color.FromRgb(144, 122, 214)),
            AssetStatus.Placeholder => new SolidColorBrush(Color.FromRgb(236, 52, 202)),
            AssetStatus.Error => new SolidColorBrush(Color.FromRgb(212, 175, 55)),
            _ => new SolidColorBrush(Color.FromRgb(255, 255, 255)),
        };

        public Brush TypeForegroundColor => Type switch
        {
            AssetType.Image => new SolidColorBrush(Color.FromRgb(202, 97, 195)),
            AssetType.Model => new SolidColorBrush(Color.FromRgb(0, 157, 220)),
            AssetType.Material => new SolidColorBrush(Color.FromRgb(27, 153, 139)),
            AssetType.Animation => new SolidColorBrush(Color.FromRgb(219, 80, 74)),
            AssetType.Sound => new SolidColorBrush(Color.FromRgb(216, 30, 91)),
            AssetType.RawFile => new SolidColorBrush(Color.FromRgb(255, 255, 0)),
            _ => new SolidColorBrush(Color.FromRgb(255, 255, 255)),
        };

        /// <summary>
        /// Gets or Sets the method to load the Asset data
        /// </summary>
        public Action<Asset, VexInstance> LoadMethod { get; set; }

        /// <summary>
        /// Gets or Sets the method to load the Asset data
        /// </summary>
        public Func<Asset, VexInstance, PhilLibX.Media3D.Model> BuildPreviewMethod { get; set; }

        /// <summary>
        /// Gets or Sets the method to load the Asset data
        /// </summary>
        public Func<Asset, VexInstance, ImageSource> BuildPreviewTextureMethod { get; set; }

        /// <summary>
        /// Saves the Asset
        /// </summary>
        public virtual void Save(VexInstance instance)
        {
            LoadMethod?.Invoke(this, instance);
        }

        /// <summary>
        /// Saves the Asset
        /// </summary>
        public virtual PhilLibX.Media3D.Model BuildPreview(VexInstance instance)
        {
            return BuildPreviewMethod?.Invoke(this, instance);
        }

        /// <summary>
        /// Saves the Asset
        /// </summary>
        public virtual ImageSource BuildPreviewTexture(VexInstance instance)
        {
            return BuildPreviewTextureMethod?.Invoke(this, instance);
        }

        /// <summary>
        /// Clears loaded asset data
        /// </summary>
        public virtual void ClearData()
        {
        }

        /// <summary>
        /// Disposes of the Asset
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                ClearData();
            }
        }


        /// <summary>
        /// Clones the Asset
        /// </summary>
        /// <returns>Cloned Asset</returns>
        public Asset Clone() => (Asset)MemberwiseClone();

        /// <summary>
        /// Che
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is Asset asset)
            {
                return Name.Equals(asset.Name);
            }

            return base.Equals(obj);
        }

        /// <summary>
        /// Compares the Assets
        /// </summary>
        /// <param name="obj">Object to compare</param>
        /// <returns>A value that indicates the relative order of the objects being compared.</returns>
        public virtual int CompareTo(object obj)
        {
            if (obj is Asset asset)
            {
                return Type.CompareTo(asset.Type);
            }

            return 1;
        }

        /// <summary>
        /// Compares the asset to the search query
        /// </summary>
        /// <param name="query">Query with data to check against the asset</param>
        /// <returns></returns>
        public virtual bool CompareToSearch(SearchQuery query)
        {
            var assetName = Name.ToLower();
            var assetType = Type.ToLower();

            foreach (var item in query.Pass)
            {
                switch (item.Key)
                {
                    case "name":
                        foreach (var value in item.Value)
                            if (!assetName.Contains(value) && !assetName.Contains($"{FNV1a.Calculate64String(value)}"))
                                return false;
                        break;
                    case "type":
                        foreach (var value in item.Value)
                            if (!assetType.Contains(value))
                                return false;
                        break;
                }
            }

            foreach (var item in query.Reject)
            {
                switch (item.Key)
                {
                    case "name":
                        foreach (var value in item.Value)
                            if (assetName.Contains(value) && assetName.Contains($"{FNV1a.Calculate64String(value)}"))
                                return false;
                        break;
                    case "type":
                        foreach (var value in item.Value)
                            if (assetType.Contains(value))
                                return false;
                        break;
                }
            }

            foreach (var item in query.ExplicitPass)
            {
                switch (item.Key)
                {
                    case "name":
                        if (!item.Value.Contains(assetName))
                            return false;
                        break;
                    case "type":
                        if (!item.Value.Contains(assetType))
                            return false;
                        break;
                }
            }

            foreach (var item in query.ExplicitReject)
            {
                switch (item.Key)
                {
                    case "name":
                        if (item.Value.Contains(assetName))
                            return false;
                        break;
                    case "type":
                        if (item.Value.Contains(assetType))
                            return false;
                        break;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns the Name of this Asset
        /// </summary>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public Asset()
        {
            Type = AssetType.Unknown;
            Name = "Asset";
            Status = AssetStatus.NotLoaded;
            AssetSize = -1;
            Streamed = false;
            FileEntry = false;
            AssetPointer = 0;
        }
    }

    public class AnimationAsset : Asset
    {
        public float Framerate { get; set; }
        public int FrameCount { get; set; }
        public int BoneCount { get; set; }
        public int ShapeCount { get; set; }
        public bool IsSiegeAnim { get; set; }

        public AnimationAsset()
        {
            Framerate = 30.0f;
            FrameCount = 0;
            BoneCount = 0;
            ShapeCount = 0;
            Type = AssetType.Animation;
            AssetSize = -1;
            IsSiegeAnim = false;
        }
    }

    public class ModelAsset : Asset
    {
        public int BoneCount { get; set; }
        public int CosmeticBoneCount { get; set; }
        public int LodCount { get; set; }

        /// <summary>
        /// Gets or Sets the method to load the Asset data
        /// </summary>
        public Func<Asset, VexInstance, Dictionary<string, XImageDDS>> GetModelImages { get; set; }

        /// <summary>
        /// Saves the Asset
        /// </summary>
        public virtual Dictionary<string, XImageDDS> BuildPreviewImages(VexInstance instance)
        {
            return GetModelImages?.Invoke(this, instance);
        }

        public ModelAsset()
        {
            BoneCount = 0;
            CosmeticBoneCount = 0;
            LodCount = 0;
            Type = AssetType.Model;
            AssetSize = -1;
        }
    }

    public class ImageAsset : Asset
    {
        public short Width { get; set; }
        public short Height { get; set; }
        public short Format { get; set; }

        public ImageAsset()
        {
            Width = 0;
            Height = 0;
            Format = 0;
            Type = AssetType.Image;
            AssetSize = -1;
        }
    }

    public class RawFileAsset : Asset
    {
        public string RawFilePath { get; set; }
        public long RawDataPointer { get; set; }

        public RawFileAsset()
        {
            RawFilePath = String.Empty;
            RawDataPointer = 0;
            Type = AssetType.RawFile;
            AssetSize = -1;
        }
    }

    public class AudioAsset : Asset
    {
        public string FullPath { get; set; }
        public int FrameRate { get; set; }
        public int FrameCount { get; set; }
        public byte ChannelsCount { get; set; }
        public int Length { get; set; }
        public short PackageIndex { get; set; }
        public bool IsLocalized { get; set; }
        public SoundDataTypes DataType { get; set; }

        public AudioAsset()
        {
            FullPath = String.Empty;
            FrameRate = 0;
            FrameCount = 0;
            ChannelsCount = 0;
            PackageIndex = 0;
            Length = 0;
            IsLocalized = false;
            DataType = SoundDataTypes.WAV_WithHeader;
            Type = AssetType.Sound;
            AssetSize = -1;
        }
    }

    public class MaterialAsset : Asset
    {
        public int ImageCount { get; set; }
        public MaterialAsset()
        {
            ImageCount = 0;
            Type = AssetType.Material;
        }
    }

    public class D2Entry : Asset
    {
        public int Container;
        public int Entry;
        public uint Id;
        public string EntryType;
        public string Destination;
        public ulong ResourcePosition;
        public int CompressedSize;
        public int dummy;
        public int unk;
        public int flag3;
        public short flag2;

        public D2Entry()
        {
            Container = 0;
            Entry = 0;
            Id = 0;
            EntryType = String.Empty;
            Destination = String.Empty;
            ResourcePosition = 0;
            CompressedSize = 0;
            dummy = 0;
            unk = 0;
            flag2 = 0;
        }
    }

    public class DeathloopEntry : Asset
    {
        public int Container;
        public uint Id;
        public string EntryType;
        public string ShortName;
        public string Destination;
        public long ResourcePosition;
        public int sizePacked;
        public int dummy;
        public int unk;
        public short archive;
    }

    public static class AssetTypeExtensions
    {
        public static string ToLower(this AssetType assetType)
        {
            return assetType switch
            {
                AssetType.Animation => "animation",
                AssetType.Image => "image",
                AssetType.Model => "model",
                AssetType.Sound => "sound",
                AssetType.RawFile => "rawfile",
                AssetType.Material => "material",
                _ => "unknown",
            };
        }
    }
}