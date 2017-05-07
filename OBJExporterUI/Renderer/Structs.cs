using OpenTK;

namespace OBJExporterUI.Renderer
{
    public class Structs
    {
        public struct Terrain
        {
            public int vertexBuffer;
            public int indiceBuffer;
            public RenderBatch[] renderBatches;
            public Doodad[] doodads;
            public WorldModelBatch[] worldModelBatches;
        }

        public struct Vertex
        {
            public Vector3 Normal;
            public Vector3 Color;
            public Vector2 TexCoord;
            public Vector3 Position;
        }

        public struct M2Vertex
        {
            public Vector3 Normal;
            public Vector2 TexCoord;
            public Vector3 Position;

        }
        public struct Material
        {
            public string filename;
            public int textureID;
            internal WoWFormatLib.Structs.M2.TextureFlags flags;
            internal uint texture1;
        }

        public struct RenderBatch
        {
            public uint firstFace;
            public uint numFaces;
            public uint[] materialID;
            /* WMO ONLY */
            public uint groupID;
            public uint blendType;
            /* ADT ONLY */
            public int[] alphaMaterialID;
        }

        public struct Doodad
        {
            public string filename;
            public Vector3 position;
            public Vector3 rotation;
            public float scale;
        }

        public struct DoodadBatch
        {
            public int vertexBuffer;
            public int indiceBuffer;
            public uint[] indices;
            public BoundingBox boundingBox;
            public Submesh[] submeshes;
            public Material[] mats;
        }

        public struct WorldModelBatch
        {
            public Vector3 position;
            public Vector3 rotation;
            public WorldModel worldModel;
        }

        public struct WMODoodad
        {
            public string filename;
            public short flags;
            public Vector3 position;
            public Quaternion rotation;
            public float scale;
            public Vector4 color;
        }

        public struct BoundingBox
        {
            public Vector3 min;
            public Vector3 max;
        }

        public struct Submesh
        {
            public uint firstFace;
            public uint numFaces;
            public uint material;
            public uint blendType;
            public uint groupID;
        }

        public struct WorldModel
        {
            public WorldModelGroupBatches[] groupBatches;
            public Material[] mats;
            public RenderBatch[] wmoRenderBatch;
            public WMODoodad[] doodads;
        }

        public struct WorldModelGroupBatches
        {
            public int vertexBuffer;
            public int indiceBuffer;
            public uint[] indices;
        }
    }
}
