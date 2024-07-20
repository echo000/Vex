using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Vex.Library
{
    public abstract class PackageCache : IDisposable
    {
        public struct PackageCacheObject
        {
            /// <summary>
            /// Gets or sets the offset to the data
            /// </summary>
            public ulong Offset { get; set; }

            /// <summary>
            /// Gets or Sets the total size of the data
            /// </summary>
            public ulong Size { get; set; }

            /// <summary>
            /// Gets or Sets the total size of the data
            /// </summary>
            public ulong UncompressedSize { get; set; }

            /// <summary>
            /// The index of the package
            /// </summary>
            public int PackageFileIndex { get; set; }
        };

        protected IFileSystem FileSystem;

        protected string PackageFilesPath;
        protected string SubDirectory;
        protected List<String> PackageFilePaths = [];
        protected Dictionary<ulong, PackageCacheObject> CacheObjects = [];
        private bool HasLoaded;
        private bool CacheLoading;

        public PackageCache()
        {
            // Defaults
            HasLoaded = false;
            CacheLoading = false;
        }

        private bool disposedValue = false; // To detect redundant calls
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
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Clear()
        {
            PackageFilePaths.Clear();
            CacheObjects.Clear();
            FileSystem = null;
            PackageFilePaths = null;
            CacheObjects = null;
        }

        public bool HasCacheLoaded()
        {
            return HasLoaded;
        }

        public bool IsCacheLoading()
        {
            return CacheLoading;
        }

        public virtual void LoadPackageCache(string BasePath)
        {
#if DEBUG
            Debug.Write("LoadPackageCache(): Begin loading...");
#endif
            // Set that we are loading
            CacheLoading = true;

            FileSystem = new WinFileSystem(BasePath);

            if (!FileSystem.IsValid())
            {
                FileSystem = null;
            }
        }

        public virtual bool LoadPackage(string FilePath)
        {
            // Nothing, default, but ensure status is loading
            CacheLoading = true;
            HasLoaded = false;
            return true;
        }

        public void SetLoadedState()
        {
#if DEBUG
            Debug.WriteLine(
                string.Format("SetLoadedState(): Cache loaded {0} objects.", CacheObjects.Count)
            );
#endif
            // Set loading complete
            HasLoaded = true;
            CacheLoading = false;
        }

        public async void LoadPackageAsync(string FilePath)
        {
            await Task.Run(() =>
            {
                LoadPackage(FilePath);
                SetLoadedState();
            });
        }

        public async void LoadPackageCacheAsync(string FilePath)
        {
            await Task.Run(() =>
            {
                LoadPackageCache(FilePath);
            });
        }

        public virtual byte[] ExtractPackageObjectRaw(ulong CacheID, VexInstance instance)
        {
            return null;
        }

        public virtual byte[] ExtractPackageObject(ulong CacheID, VexInstance instance)
        {
            return this.ExtractPackageObject(CacheID, -1, instance);
        }

        public virtual byte[] ExtractPackageObject(ulong CacheID, int size, VexInstance instance)
        {
            return null;
        }

        public virtual byte[] ExtractPackageObject(
            string PackageName,
            int AssetOffset,
            int size,
            VexInstance instance
        )
        {
            return null;
        }

        public virtual bool Exists(ulong CacheID)
        {
            return CacheObjects.ContainsKey(CacheID);
        }

        public string GetPackagesPath()
        {
            return PackageFilesPath;
        }

        public virtual ulong HashPackageID(string value)
        {
            return 0;
        }
    }
}
