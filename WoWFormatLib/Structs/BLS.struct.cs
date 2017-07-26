using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWFormatLib.Structs.BLS
{
    public struct BLS
    {
        public uint version;
        public uint permutationCount;
        public uint nShaders;
        public uint ofsCompressedChunks;
        public uint nCompressedChunks;
        public uint ofsCompressedData;
    }
}
