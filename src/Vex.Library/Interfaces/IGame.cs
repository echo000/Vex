using Vex.Library.Utility;
using System.Collections.Generic;

namespace Vex.Library
{
    public interface IGame
    {
        /// <summary>
        /// Gets or Sets the Game's name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets or Sets the List of Asset Pools
        /// </summary>
        List<IAssetPool> AssetPools { get; set; }

        /// <summary>
        /// Validates the games addresses
        /// </summary>
        /// <returns>True if the addresses are valid, otherwise false</returns>
        bool Initialize(VexInstance instance);

        /// <summary>
        /// Creates a shallow copy of the Game Object
        /// </summary>
        /// <returns>Copied Game Object</returns>
        object Clone();

        void Clear()
        {
        }
    }
}
