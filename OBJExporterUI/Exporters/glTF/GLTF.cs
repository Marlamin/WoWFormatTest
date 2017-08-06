using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBJExporterUI.Exporters.glTF
{
    public struct glTF
    {
        public Accessor[] accessors;
        public Asset asset;
        public BufferView[] bufferViews;
        public Buffer[] buffers;
        public Image[] images;
        public Material[] materials;
        public Mesh[] meshes;
        public Node[] nodes;
        public Sampler[] samplers;
        public uint scene;
        public Scene[] scenes;
        public Texture[] textures;
    }

    public struct Accessor
    {
        public int bufferView;
        public uint byteOffset;
        public uint componentType;
        public uint count;
        public float[] max;
        public float[] min;
        public string type;
    }

    public struct Asset
    {
        public string version;
        public string generator;
        public string copyright;
    }

    public struct BufferView
    {
        public uint buffer;
        public uint byteLength;
        public uint byteOffset;
        public uint target;
    }
    
    public struct Buffer
    {
        public uint byteLength;
        public string uri;
    }

    public struct Image
    {
        public string uri;
    }

    public struct Material
    {
        public float[] emissiveFactor;
        public TextureIndex[] emissiveTexture;
        public string name;
        public TextureIndex[] normalTexture;
        public TextureIndex[] occlusionTexture;
        public PBRMetallicRoughness pbrMetallicRoughness;
    }

    public struct PBRMetallicRoughness
    {
        public TextureIndex[] baseColorTexture;
        public TextureIndex[] metallicRoughnessTexture;
    }

    public struct TextureIndex
    {
        public uint index;
    }

    public struct Mesh
    {
        public string name;
        public Primitive[] primitives;
    }

    public struct Primitive
    {
        public Dictionary<string, uint> attributes;
        public uint indices;
        public uint material;
    }

    public struct Node
    {
        public uint mesh;
        public string name;
        public float[] rotation;
    }

    public struct Sampler
    {

    }

    public struct Scene
    {
        public string name;
        public uint[] nodes;
    }

    public struct Texture
    {
        public uint sampler;
        public uint source;
    }
}
