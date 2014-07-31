using WoWFormatLib.Utils;
namespace WoWFormatLib.Structs.ADT
{
    public struct ADT
    {
        public MVER mver;
    }

    public struct MVER
    {
        public uint version;
    }

    public enum MHDRFlags{
        mhdr_MFBO = 1,                // contains a MFBO chunk.
        mhdr_northrend = 2,           // is set for some northrend ones.
    }

    public struct MHDR
    {
        public uint flags;
        public uint ofsMCIN;
        public uint ofsMTEX;
        public uint ofsMMDX;
        public uint ofsMMID;
        public uint ofsMWMO;
        public uint ofsMWID;
        public uint ofsMDDF;
        public uint ofsMODF;
        public uint ofsMFBO;
        public uint ofsMH2O;
        public uint ofsMTXF;
        public uint unk1;
        public uint unk2;
        public uint unk3;
        public uint unk4;
    }

    public struct MCNK
    {
        public uint flags;
        public uint indexX;
        public uint indexY;
        public uint nLayers;
        public uint nDoodadRefs;
        public uint ofsMCVT;
        public uint ofsMCNR;
        public uint ofsMCLY;
        public uint ofsMCRF;
        public uint ofsMCAL;
        public uint sizeAlpha;
        public uint ofsMCSH;
        public uint sizeShadows;
        public uint areaID;
        public uint nMapObjRefs;
        public uint holes;
        public unsafe fixed short lowQualityTexturingMap[8];
        public uint predTex;
        public uint noEffectDoodad;
        public uint ofsMCSE;
        public uint numMCSE;
        public uint ofsMCLQ;
        public uint sizeMCLQ;
        public Vector3 position;
        public uint ofsMCCV;
        public uint ofsMCLV;
        public uint unused;
    }

    public struct MCVT
    {
        public unsafe fixed float vertice[145];
    }

    public struct MCLV
    {
        public unsafe fixed ushort color[145];
    }
    public struct MCCV
    {
        public byte red;
        public byte green;
        public byte blue;
        public byte alpha;
    } //times 145

    public struct MCNR
    {
        public unsafe fixed short normal[3];
    }

}