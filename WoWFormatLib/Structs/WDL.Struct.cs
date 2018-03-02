namespace WoWFormatLib.Structs.WDL
{
    public enum WDLChunks
    {
        MVER = 'M' << 24 | 'V' << 16 | 'E' << 8 | 'R' << 0,
        MWMO = 'M' << 24 | 'W' << 16 | 'M' << 8 | 'O' << 0,
        MWID = 'M' << 24 | 'W' << 16 | 'I' << 8 | 'D' << 0,
        MODF = 'M' << 24 | 'O' << 16 | 'D' << 8 | 'F' << 0,
        MAOF = 'M' << 24 | 'A' << 16 | 'O' << 8 | 'F' << 0,
        MARE = 'M' << 24 | 'A' << 16 | 'R' << 8 | 'E' << 0,
        MAOC = 'M' << 24 | 'A' << 16 | 'O' << 8 | 'C' << 0,
        MAHO = 'M' << 24 | 'A' << 16 | 'H' << 8 | 'O' << 0,
    }

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
