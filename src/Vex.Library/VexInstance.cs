using PhilLibX;
using PhilLibX.Media3D.Translator;
using System;
using System.Collections.Generic;
using System.IO;
using Vex.Library.Package;

namespace Vex.Library
{
    /// <summary>
    /// A Class to hold an Instance of the Vex Library
    /// </summary>
    public class VexInstance : IDisposable
    {
        /// <summary>
        /// Game Cache
        /// </summary>
        public VoidSupport VoidSupport;

        /// <summary>
        /// Gets or Sets the current Settings
        /// </summary>
        public VexSettings Settings = new();

        /// <summary>
        /// Gets or Sets the loaded Assets
        /// </summary>
        public List<Asset> Assets { get; set; }

        /// <summary>
        /// Gets or Sets the loaded Game
        /// </summary>
        public SupportedGames Game { get; set; }

        /// <summary>
        /// Gets the Export Path
        /// </summary>
        public string ExportFolder => Settings.ExportDirectory;

        public readonly Graphics3DTranslatorFactory Translator = new();

        public VexInstance()
        {
            Translator.WithDefaultTranslators();
        }

        public void Clear()
        {
            VoidSupport?.Clear();
            Assets?.Clear();

            Assets = null;
            VoidSupport = null;

            NativeMethods.FreeOodleLibrary();
        }

        public void BeginGameFileMode(string FileName)
        {
            Clear();
            LoadFile(FileName);
        }

        public void LoadFile(string FileName)
        {
            // Determine based on file extension first
            var file = new FileInfo(FileName);
            Assets ??= [];
            if (file.Name.Equals("master.index", StringComparison.CurrentCultureIgnoreCase))
            {
                VoidSupport = new VoidSupport();
                VoidSupport.VoidMasterIndex(FileName, this);
                VoidSupport.ReloadAssets(this);
            }
            else
            {
                throw new InvalidFileException();
            }
        }

        public bool ShouldLoad(AssetType Name)
        {
            return Name switch
            {
                AssetType.Image => Settings.LoadImages,
                AssetType.Model => Settings.LoadModels,
                AssetType.Material => Settings.LoadMaterials,
                AssetType.Animation => Settings.LoadAnimations,
                AssetType.RawFile => Settings.LoadRawFiles,
                AssetType.Sound => Settings.LoadSounds,
                _ => false,
            };
        }

        public string GetImageExportFormat()
        {
            return Settings.ImageExportFormat switch
            {
                (int)ImgExportFormat.DDS => ".dds",
                (int)ImgExportFormat.PNG => ".png",
                (int)ImgExportFormat.TIFF => ".tiff",
                (int)ImgExportFormat.TGA => ".tga",
                _ => ".png",
            };
        }

        private bool disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Clear();
                }
                disposedValue = true;
            }
        }
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public string GetGameName()
        {
            return Game switch
            {
                SupportedGames.None => "None",
                SupportedGames.Dishonored2 => "Dishonored 2",
                SupportedGames.Deathloop => "Deathloop",
                _ => throw new ArgumentOutOfRangeException(nameof(Game), Game, "Unsupported game type"),
            };
        }
    }
}
