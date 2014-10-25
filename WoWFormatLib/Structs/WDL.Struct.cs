using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWFormatLib.Structs.WDL
{
    public struct WDL
    {
        public MWMO mwno; //WMO filenames (zero terminated)
        public MWID mwid; //Indexes to MWMO chunk
        public MODT modf; //Placement info for WMOs
        public MAOF maof; //Map Area Offset 64x64
    }

    public struct MWMO
    {
        
    }

    public struct MWID
    {
        
    }

    public struct MODT
    {

    }

    public struct MAOF
    {
        public uint[] areaLowOffsets; //4096 entries, make manually
    }
}