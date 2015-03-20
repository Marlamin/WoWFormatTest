using WoWFormatLib.Utils;
namespace WoWFormatLib.Structs.ADT
{
    public struct ADT
    {
        public uint version;
        public MHDR header;
        public MCNK[] chunks;
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

    public struct MCNKheader
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
        public short lowQualityTexturingMap_0;
        public short lowQualityTexturingMap_1;
        public short lowQualityTexturingMap_2;
        public short lowQualityTexturingMap_3;
        public short lowQualityTexturingMap_4;
        public short lowQualityTexturingMap_5;
        public short lowQualityTexturingMap_6;
        public short lowQualityTexturingMap_7;
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

    public struct MCNK
    {
        public MCNKheader header;
        public MCVT vertices;
        public MCNR normals;
        public MCLV colors;
        public MCCV vertexshading;
    }
    public struct MCVT
    {
        public float[] vertices; //make manually, 145
    }

    public struct MCLV
    {
        public ushort[] color; //make manually, 145
    }
    public struct MCCV
    {
        public byte[] red;
        public byte[] green;
        public byte[] blue;
        public byte[] alpha;
    }

    public struct MCNR
    {
        public short normal_0;
        public short normal_1;
        public short normal_2;
        //has entries too
    }

}