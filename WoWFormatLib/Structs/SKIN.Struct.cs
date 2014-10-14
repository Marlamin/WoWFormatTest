using WoWFormatLib.Utils;

namespace WoWFormatLib.Structs.SKIN
{
    public struct SKIN
    {
        public uint version;
        public string filename;
        public Indice[] indices;
        public Triangle[] triangles;
        public Property[] properties;
        public Submesh[] submeshes;
        public TextureUnit[] textureunit;
        public uint bones;
    }

    public struct Indice
    {
        public ushort vertex;
    }

    public struct Triangle
    {
        public ushort pt1;
        public ushort pt2;
        public ushort pt3;
    }

    public struct Property
    {
        public unsafe fixed byte properties[4];
    }

    public struct Submesh
    {
        public ushort submeshID;
        public ushort unk1;
        public ushort startVertex;
        public ushort nVertices;
        public uint startTriangle;
        public ushort nTriangles;
        public ushort nBones;
        public ushort startBones;
        public ushort unk2;
        public ushort rootBone;
        public Vector3 centerMas;
        public Vector3 centerBoundingBox;
        public float radius;
    }

    public struct TextureUnit
    {
        public ushort flags;
        public ushort shading;
        public ushort submeshIndex;
        public ushort submeshIndex2;
        public ushort colorIndex;
        public ushort renderFlags;
        public ushort texUnitNumber;
        public ushort mode;
        public ushort texture;
        public ushort texUnitNumber2;
        public ushort transparency;
        public ushort textureAnim;
    }
}