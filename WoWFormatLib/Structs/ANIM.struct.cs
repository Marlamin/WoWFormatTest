using System;
using WoWFormatLib.Utils;
namespace WoWFormatLib.Structs.ANIM
{
    public enum ANIMChunks
    {
        AFM2 = 'A' << 24 | 'F' << 16 | 'M' << 8 | '2' << 0,
        AFSA = 'A' << 24 | 'F' << 16 | 'S' << 8 | 'A' << 0,
        AFSB = 'A' << 24 | 'F' << 16 | 'S' << 8 | 'B' << 0,
    }
}
