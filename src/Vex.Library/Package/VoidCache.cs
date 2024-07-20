using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vex.Library.Package
{
    internal class VoidCache : PackageCache
    {
        public VoidCache()
        {
        }

        public override bool LoadPackage(string FilePath)
        {
            // Call Base function first
            base.LoadPackage(FilePath);

            using var stream = File.OpenRead(FilePath);
            using var Reader = new BinaryReader(stream);

            var Magic = Reader.ReadUInt32();

            if(Magic != 0x04534552)
            {
                return false;
            }

            return false;
        }
    }
}
