using System;
using WoWFormatLib.Utils;
namespace WoWFormatLib.Structs.ADT
{
    public enum ADTChunks
    {
        // Root ADT
        MVER = 'M' << 24 | 'V' << 16 | 'E' << 8 | 'R' << 0,
        MCNK = 'M' << 24 | 'C' << 16 | 'N' << 8 | 'K' << 0,
        MHDR = 'M' << 24 | 'H' << 16 | 'D' << 8 | 'R' << 0,
        MH2O = 'M' << 24 | 'H' << 16 | '2' << 8 | 'O' << 0,
        MFBO = 'M' << 24 | 'F' << 16 | 'B' << 8 | 'O' << 0,
        MBMH = 'M' << 24 | 'B' << 16 | 'M' << 8 | 'H' << 0,
        MBBB = 'M' << 24 | 'B' << 16 | 'B' << 8 | 'B' << 0,
        MBMI = 'M' << 24 | 'B' << 16 | 'M' << 8 | 'I' << 0,
        MBNV = 'M' << 24 | 'B' << 16 | 'N' << 8 | 'V' << 0,

        // Root MCNK
        MCVT = 'M' << 24 | 'C' << 16 | 'V' << 8 | 'T' << 0,
        MCCV = 'M' << 24 | 'C' << 16 | 'C' << 8 | 'V' << 0,
        MCNR = 'M' << 24 | 'C' << 16 | 'N' << 8 | 'R' << 0,
        MCSE = 'M' << 24 | 'C' << 16 | 'S' << 8 | 'E' << 0,
        MCBB = 'M' << 24 | 'C' << 16 | 'B' << 8 | 'B' << 0,
        MCLQ = 'M' << 24 | 'C' << 16 | 'L' << 8 | 'Q' << 0,
        MCLV = 'M' << 24 | 'C' << 16 | 'L' << 8 | 'V' << 0,

        // OBJ
        MMDX = 'M' << 24 | 'M' << 16 | 'D' << 8 | 'X' << 0,
        MMID = 'M' << 24 | 'M' << 16 | 'I' << 8 | 'D' << 0,
        MWMO = 'M' << 24 | 'W' << 16 | 'M' << 8 | 'O' << 0,
        MWID = 'M' << 24 | 'W' << 16 | 'I' << 8 | 'D' << 0,
        MDDF = 'M' << 24 | 'D' << 16 | 'D' << 8 | 'F' << 0,
        MODF = 'M' << 24 | 'O' << 16 | 'D' << 8 | 'F' << 0,

        // TEX
        MTEX = 'M' << 24 | 'T' << 16 | 'E' << 8 | 'X' << 0,
        MTXP = 'M' << 24 | 'T' << 16 | 'X' << 8 | 'P' << 0,
        MAMP = 'M' << 24 | 'A' << 16 | 'M' << 8 | 'P' << 0,

        // TEX MCNK
        MCLY = 'M' << 24 | 'C' << 16 | 'L' << 8 | 'Y' << 0,
        MCAL = 'M' << 24 | 'C' << 16 | 'A' << 8 | 'L' << 0,
        MCSH = 'M' << 24 | 'C' << 16 | 'S' << 8 | 'H' << 0,
        MCMT = 'M' << 24 | 'C' << 16 | 'M' << 8 | 'T' << 0,

        // LOD
        MLHD = 'M' << 24 | 'L' << 16 | 'H' << 8 | 'D' << 0,
        MLVH = 'M' << 24 | 'L' << 16 | 'V' << 8 | 'H' << 0,
        MLVI = 'M' << 24 | 'L' << 16 | 'V' << 8 | 'I' << 0,
        MLLL = 'M' << 24 | 'L' << 16 | 'L' << 8 | 'L' << 0,
        MLND = 'M' << 24 | 'L' << 16 | 'N' << 8 | 'D' << 0,
        MLSI = 'M' << 24 | 'L' << 16 | 'S' << 8 | 'I' << 0,
        MBMB = 'M' << 24 | 'B' << 16 | 'M' << 8 | 'B' << 0,
        MLLD = 'M' << 24 | 'L' << 16 | 'L' << 8 | 'D' << 0,
        MLLN = 'M' << 24 | 'L' << 16 | 'L' << 8 | 'N' << 0,
        MLLI = 'M' << 24 | 'L' << 16 | 'L' << 8 | 'I' << 0,
        MLLV = 'M' << 24 | 'L' << 16 | 'L' << 8 | 'V' << 0,
    }

    public struct ADT
    {
        public uint version;
        public MHDR header;
        public MTEX textures;
        public MTXP[] texParams;
        public MCNK[] chunks;
        public TexMCNK[] texChunks;
        public MH2O mh2o;
        public Obj objects;
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
        public byte holesHighRes_0;
        public byte holesHighRes_1;
        public byte holesHighRes_2;
        public byte holesHighRes_3;
        public byte holesHighRes_4;
        public byte holesHighRes_5;
        public byte holesHighRes_6;
        public byte holesHighRes_7;
        public uint ofsMCLY;
        public uint ofsMCRF;
        public uint ofsMCAL;
        public uint sizeAlpha;
        public uint ofsMCSH;
        public uint sizeShadows;
        public uint areaID;
        public uint nMapObjRefs;
        public ushort holesLowRes;
        public ushort unknownPad;
        public short lowQualityTexturingMap_0;
        public short lowQualityTexturingMap_1;
        public short lowQualityTexturingMap_2;
        public short lowQualityTexturingMap_3;
        public short lowQualityTexturingMap_4;
        public short lowQualityTexturingMap_5;
        public short lowQualityTexturingMap_6;
        public short lowQualityTexturingMap_7;
        public long noEffectDoodad;
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
        public MCCV vertexShading;
        public MCSE soundEmitters;
        public MCBB[] blendBatches;
    }

    public struct TexMCNK
    {
        public MCLY[] layers;
        public MCAL[] alphaLayer;
    }

    public struct Obj
    {
        public MDDF models;
        public MMDX m2Names;
        public MMID m2NameOffsets;

        public MODF worldModels;
        public MWMO wmoNames;
        public MWID wmoNameOffsets;

    }

    //WMO placement
    public struct MODF
    {
        public MODFEntry[] entries;
    }

    public struct MODFEntry
    {
        public uint mwidEntry;
        public uint uniqueId;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 lowerBounds;
        public Vector3 upperBounds;
        public MODFFlags flags;
        public ushort doodadSet;
        public ushort nameSet;
        public ushort scale;
    }

    [Flags]
    public enum MODFFlags : ushort
    {
        modf_destroyable            = 0x1,
        modf_use_lod                = 0x2,
        modf_0x4_unk                = 0x4,
        modf_entry_is_filedataid    = 0x8,
    }

    //M2 placement
    public struct MDDF
    {
        public MDDFEntry[] entries;
    }

    public struct MDDFEntry
    {
        public uint mmidEntry;
        public uint uniqueId;
        public Vector3 position;
        public Vector3 rotation;
        public ushort scale;
        public MDDFFlags flags;
    }

    [Flags]
    public enum MDDFFlags : ushort
    {
        mddf_biodome                = 0x1,
        mddf_shrubbery              = 0x2, //probably deprecated < 18179
        mddf_0x4                    = 0x4,
        mddf_0x8                    = 0x8,
        mddf_liquid_known           = 0x20,
        mddf_entry_is_filedataid    = 0x40,
        mddf_0x100                  = 0x100,
    }

    //List of filenames for M2 models that appear in this map tile.
    public struct MMDX
    {
        public string[] filenames; // zero-terminated strings with complete paths to models. Referenced in MMID.
        public uint[] offsets; //not part of official struct, filled manually during parsing with where the string started
    }

    //List of offsets of M2 filenames in the MMDX chunk.
    public struct MMID
    {
        public uint[] offsets; // filename starting position in MMDX chunk. These entries are getting referenced in the MDDF chunk.
    }

    //List of offsets of WMO filenames in the MWMO chunk.
    public struct MWID
    {
        public uint[] offsets; // filename starting position in MWMO chunk. These entries are getting referenced in the MODF chunk.
    }

    //List of filenames for WMOs (world map objects) that appear in this map tile.
    public struct MWMO
    {
        public string[] filenames;
        public uint[] offsets; //not part of official struct, filled manually during parsing with where the string started
    }

    public struct MCVT
    {
        public float[] vertices; //make manually, 145
    }

    public struct MCLV
    {
        public ushort[] color; //make manually, 145
    }

    public struct MCLY
    {
        public uint textureId;
        public mclyFlags flags;
        public uint offsetInMCAL;
        public int effectId;
    }

    public struct MCSE
    {
        public byte[] raw; //TODO
    }

    public struct MCBB
    {
        public uint mbmhIndex;
        public uint indexCount;
        public uint indexFirst;
        public uint vertexCount;
        public uint vertexFirst;
    }

    public struct MH2O
    {
        public MH2OHeader[] headers;
    }

    public struct MH2OHeader
    {
        public uint offsetInstances;
        public uint layerCount;
        public uint offsetAttributes;
    }

    [Flags]
    public enum mclyFlags : uint
    {
        Flag_0x1 = 0x1,
        Flag_0x2 = 0x2,
        Flag_0x4 = 0x4,
        Flag_0x8 = 0x8,
        Flag_0x10 = 0x10,
        Flag_0x20 = 0x20,
        Flag_0x40 = 0x40,
        Flag_0x80 = 0x80,
        Flag_0x100 = 0x100,
        Flag_0x200 = 0x200,
        Flag_0x400 = 0x400,
        Flag_0x800 = 0x800,
        Flag_0x1000 = 0x1000
    }

    public struct MCAL
    {
        public byte[] layer;
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
        public short[] normal_0;
        public short[] normal_1;
        public short[] normal_2;
    }

    public struct MTEX
    {
        public string[] filenames;
    }

    public struct MTXP
    {
        public uint flags;
        public float height;
        public float offset;
        public uint unk3;
    }

    /* _lod */
    public struct LODADT
    {
        public float[] heights;
        public short[] indices;
        public MLLLEntry[] lodLevels;
        public MLNDEntry[] quadTree;
        public short[] skirtIndices;
    }

    public struct MLLLEntry
    {
        public float lod;
        public uint heightLength;
        public uint heightIndex;
        public uint mapAreaLowLength;
        public uint mapAreaLowIndex;
    }

    public struct MLNDEntry
    {
        public uint index;
        public uint length;
        public uint unk0;
        public uint unk1;
        public short indice_1;
        public short indice_2;
        public short indice_3;
        public short indice_4;
    }
}
