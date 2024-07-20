using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saluki.Library
{
    public partial class Deathloop : IGame
    {
        /// <summary>
        /// Gets Deathloop's Game Name
        /// </summary>
        public string Name => "Deathloop";
        public List<IAssetPool> AssetPools { get; set; }
        public bool Initialize(SalukiInstance instance)
        {
            return true;
        }
    }
}
