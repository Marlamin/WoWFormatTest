using System;

namespace WoWFormatLib.Structs.WDT
{
    public enum WDTChunks
    {
        MVER = 'M' << 24 | 'V' << 16 | 'E' << 8 | 'R' << 0,
        MAIN = 'M' << 24 | 'A' << 16 | 'I' << 8 | 'N' << 0,
        MWMO = 'M' << 24 | 'W' << 16 | 'M' << 8 | 'O' << 0,
        MPHD = 'M' << 24 | 'P' << 16 | 'H' << 8 | 'D' << 0,
        MPLT = 'M' << 24 | 'P' << 16 | 'L' << 8 | 'T' << 0,
        MODF = 'M' << 24 | 'O' << 16 | 'D' << 8 | 'F' << 0,
    }

    public struct WDT
    {
        public MPHD mphd;
    }

    public struct MPHD
    {
        public mphdFlags flags;
        public uint something;
        public uint[] unused;
    }

    [Flags]
    public enum mphdFlags
    {
        Flag_0x1 = 0x1,
        Flag_0x2= 0x2,
        Flag_0x4 = 0x4,
        Flag_0x8= 0x8,
        Flag_0x10 = 0x10,
        Flag_0x20 = 0x20,
        Flag_0x40 = 0x40,
        Flag_0x80 = 0x80,
    }

    //  public struct MWMO
    //  {
    //
    //  }
}
