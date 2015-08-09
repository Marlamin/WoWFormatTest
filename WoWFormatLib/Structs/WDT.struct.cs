using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWFormatLib.Structs.WDT
{
    public struct WDT
    {
        // public MWMO mwno; //WMO filenames (zero terminated)
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
