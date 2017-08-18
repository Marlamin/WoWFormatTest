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
        public Scene[] scenes;
        public uint scene;
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
        public string name;
    }

    public struct Asset
    {
        public string version;
        public string minVersion;
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
        public string alphaMode;
        public float alphaCutoff;
    }

    public struct PBRMetallicRoughness
    {
        public TextureIndex baseColorTexture;
        public float metallicFactor;
        public TextureIndex[] metallicRoughnessTexture;
    }

    public struct TextureIndex
    {
        public int index;
    }

    public struct Mesh
    {
        public string name;
        public Primitive[] primitives;
    }

    public struct Primitive
    {
        public Dictionary<string, int> attributes;
        public uint indices;
        public uint material;
        public uint mode;
    }

    public struct Node
    {
        public int mesh;
        public string name;
        public float[] rotation;
    }

    public struct Sampler
    {
        public string name;
        public int magFilter;
        public int minFilter;
        public int wrapS;
        public int wrapT;
    }

    public struct Scene
    {
        public string name;
        public int[] nodes;
    }

    public struct Texture
    {
        public int sampler;
        public int source;
    }
}
