using System.Collections.Generic;

namespace Vex.Library
{
    public abstract class IAssetPool
    {
        /// <summary>
        /// Gets the Asset Pool Name
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Gets the Asset Pool Index
        /// </summary>
        public abstract int Index { get; }

        /// <summary>
        /// Gets the Asset Header Size
        /// </summary>
        public abstract int AssetSize { get; set; }

        /// <summary>
        /// Gets or Sets the number of Asset slots in this pool
        /// </summary>
        public abstract int AssetCount { get; set; }

        /// <summary>
        /// Gets or Sets the start Address of this pool
        /// </summary>
        public abstract long StartAddress { get; set; }

        /// <summary>
        /// Gets or Sets the end Address of this pool
        /// </summary>
        public abstract long EndAddress { get; set; }

        /// <summary>
        /// Loads Assets from the given Asset Pool
        /// </summary>
        public abstract List<Asset> Load(VexInstance instance);

        /// <summary>
        /// Determines if the specified XAsset is a null slot.
        /// </summary>
        public bool IsNullAsset(long nameAddress)
        {
            return nameAddress > StartAddress && nameAddress < EndAddress || nameAddress == 0;
        }
    }
}
