using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhilLibX.Cryptography.Hash
{
    public class CRC32
    {
        public static uint EdgeAnimGenerateNameHash(string name)
        {
            uint crcValue = ~0U;

            if (name != null)
            {
                foreach (char c in name)
                {
                    uint tableTemp = (uint)((crcValue & 0xFF) ^ (byte)c);
                    for (int bitLoop = 0; bitLoop < 8; bitLoop++)
                    {
                        if ((tableTemp & 0x01) != 0)
                        {
                            tableTemp = (tableTemp >> 1) ^ 0xEDB88320;
                        }
                        else
                        {
                            tableTemp = tableTemp >> 1;
                        }
                    }
                    crcValue = (crcValue >> 8) ^ tableTemp;
                }
            }
            return crcValue;
        }
    }
}
