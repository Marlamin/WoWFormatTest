namespace WoWFormatLib.Structs.TEX
{
    public enum TEXChunks
    {
        TXVR = 'T' << 24 | 'X' << 16 | 'V' << 8 | 'R' << 0,
        TXFN = 'T' << 24 | 'X' << 16 | 'F' << 8 | 'N' << 0,
        TXBT = 'T' << 24 | 'X' << 16 | 'B' << 8 | 'T' << 0,
        TXMD = 'T' << 24 | 'X' << 16 | 'M' << 8 | 'D' << 0,
    }
}
