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
        public PackageCache GameCache;

        /// <summary>
        /// Gets or Sets the current game flags
        /// </summary>
        public GameFlags LoadedGameFlags { get; set; }

        /// <summary>
        /// Gets or Sets the current Settings
        /// </summary>
        public VexSettings Settings = new();

        /// <summary>
        /// Gets or Sets the loaded Assets
        /// </summary>
        public List<Asset> Assets { get; set; }

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
            LoadedGameFlags = GameFlags.None;

            GameCache?.Dispose();
            Assets?.Clear();

            Assets = null;
            GameCache = null;

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
            var FileExt = file.Extension;
            Assets ??= [];

            switch (FileExt)
            {
                case ".index":
                    var DS2 = new VoidSupport();
                    Assets.AddRange(DS2.DishonoredMasterIndex(FileName));
                    LoadedGameFlags = GameFlags.Files;
                    break;
                default:
                    throw new InvalidFileException();
            }
        }

        public bool ShouldLoad(string Name)
        {
            return Name switch
            {
                "Image" => Settings.LoadImages,
                "Model" => Settings.LoadModels,
                "Material" => Settings.LoadMaterials,
                "Animation" => Settings.LoadAnimations,
                "Rawfile" => Settings.LoadRawFiles,
                "Sound" => Settings.LoadSounds,
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

        public string GetAudioExportFormat()
        {
            return Settings.AudioExportFormat switch
            {
                (int)SoundExportFormat.WAV => ".wav",
                (int)SoundExportFormat.FLAC => ".flac",
                _ => ".wav",
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
    }
}
