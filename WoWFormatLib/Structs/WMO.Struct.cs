using WoWFormatLib.Utils;

namespace WoWFormatLib.Structs.WMO
{
    public struct WMO
    {
        public MOHD header;
        public MVER version;
        public MOTX[] textures;
        public MOGN[] groupNames;
        public MOGI[] groupInfo;
        public WMOGroupFile[] group;
    }

    public struct MVER
    {
        public uint version;
    }

    public struct MOHD
    {
        public uint nMaterials;
        public uint nGroups;
        public uint nPortals;
        public uint nLights;
        public uint nModels;
        public uint nDoodads;
        public uint nSets;
        public uint ambientColor;
        public uint areaTableID;
        public Vector3 boundingBox1;
        public Vector3 boundingBox2;
        public uint flags;
    }

    //Texture filenames
    public struct MOTX
    {
        public string filename;
    }

    public struct MOMT
    {
    }

    //Group names
    public struct MOGN
    {
        public string name;
    }

    //Group information
    public struct MOGI
    {
        public uint flags;
        public Vector3 boundingBox1;
        public Vector3 boundingBox2;
        public int nameIndex; //something else
    }

    public enum MOGPFlags
    {
        Flag_0x1_HasMOBN_MOBR = 0x1, //Has MOBN and MOBR chunk.
        Flag_0x2 = 0x2,
        Flag_0x4_HasMOCV = 0x4, //Has vertex colors (MOCV chunk)
        Flag_0x8_Outdoor = 0x8, //Outdoor
        Flag_0x10 = 0x10,
        Flag_0x20 = 0x20,
        Flag_0x40 = 0x40,
        Flag_0x80 = 0x80,   
        Flag_0x100 = 0x100,
        Flag_0x200_HasMOLR = 0x200, //Has lights  (MOLR chunk)
        Flag_0x400_HasMPBV_MPBP_MPBI_MPBG = 0x400, //Has MPBV, MPBP, MPBI, MPBG chunks.
        Flag_0x800_HasMODR = 0x800, //Has doodads (MODR chunk)
        Flag_0x1000_HasMLIQ = 0x1000, //Has water   (MLIQ chunk)
        Flag_0x2000_Indoor = 0x2000, //Indoor
        Flag_0x8000 = 0x8000,
        Flag_0x10000 = 0x10000,
        Flag_0x20000_HasMORI_MORB = 0x20000, //Has MORI and MORB chunks.
        Flag_0x40000_Skybox = 0x40000, //Show skybox
        Flag_0x80000_isNotOcean = 0x80000, //isNotOcean, LiquidType related, see below in the MLIQ chunk.
        Flag_0x100000 = 0x100000,
        Flag_0x200000 = 0x200000,
        Flag_0x400000 = 0x400000,
        Flag_0x800000 = 0x800000,
        Flag_0x1000000 = 0x1000000, //SMOGroup::CVERTS2: Has two MOCV chunks: Just add two or don't set 0x4 to only use cverts2.
        Flag_0x2000000 = 0x2000000, //SMOGroup::TVERTS2: Has two MOTV chunks: Just add two.
    }

    public struct WMOGroupFile
    {
        public MVER version;
        public MOGP mogp;
    }
    public struct MOGP
    {
        public uint nameOffset;
        public uint descriptiveNameOffset;
        public uint flags;
        public Vector3 boundingBox1;
        public Vector3 boundingBox2;
        public ushort ofsPortals; //Index of portal in MOPR chunk
        public ushort numPortals;   
        public ushort numBatchesA;
        public ushort numBatchesB;
        public uint numBatchesC; //WoWDev: For the "Number of batches" fields, A + B + C == the total number of batches in the WMO/v17 group (in the MOBA chunk).
        public unsafe fixed byte fogIndices[4];
        public uint liquidType;
        public uint groupID;
        public uint unk0;
        public uint unk1;
        //public MOBR[] faceIndices;
        public MOPY[] materialInfo;
        public MOVI[] indices;
        public MOVT[] vertices;
        public MONR[] normals;
        public MOTV[] textureCoords;
        public MOBA[] renderBatches;
    }

    public struct MOVI
    {
        public ushort indice;
    }

    public struct MOVT
    {
        public Vector3 vector;
    }

    public struct MONR
    {
        public Vector3 normal;
    }

    public struct MOTV
    {
        public float X;
        public float Y;
    }

    public struct MOPY
    {
        public byte flags;
        public byte materialID;
    }

    public enum MOPYFlags
    {
        /*
        bool isNoCamCollide (uint8 flags) { return flags & 2; }
        bool isDetailFace (uint8 flags) { return flags & 4; }
        bool isCollisionFace (uint8 flags) { return flags & 8; }
        bool isColor (uint8 flags) { return !(flags & 8); }
        bool isRenderFace (uint8 flags) { return (flags & 0x24) == 0x20; }
        bool isTransFace (uint8 flags) { return (flags & 1) && (flags & 0x24); }
        bool isCollidable (uint8 flags) { return isCollisionFace (flags) || isRenderFace (flags); }
         */
        Flag_0x1 = 0x1,
        Flag_0x2_NoCamCollide = 0x2,
        Flag_0x4_NoCollide = 0x4,
        Flag_0x8_IsCollisionFace = 0x8, //If it's not set it's isColor apparently
    }

    public struct MOBA
    {
        public Vector3 possibleBox1;
        public Vector3 possibleBox2;
        public uint firstFace;
        public ushort numFaces;
        public ushort firstVertex;
        public ushort lastVertex;
        public byte unused;
        public byte materialID;
    }


}