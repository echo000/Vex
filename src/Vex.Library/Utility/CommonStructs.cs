using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Vex.Library
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct DishonoredResourceHeader
    {
        public int Magic;
        public short Version;
    }

    struct DishonoredContainer
    {
        public string Directory;
        public string Path;
        public List<string> Resources;
        public List<D2Entry> Entries;
        public DishonoredContainer()
        {
            Resources = [];
            Entries = [];
        }

        public readonly string ResourcePath(ushort flags)
        {
            string Result = string.Empty;

            int index = (flags & 0x8000) != 0 ? Resources.Count - 1 : (int)(flags >> 2);
            if(Resources.Count > index)
            {
                Result = Resources[index];
            }

            return Result;
        }
    }
}
