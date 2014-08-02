using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D11;

namespace WoWRenderLib.Structs
{
    public struct WoWWMO
    {
        public string name;
        public float[] vertices;
        public ushort[] indices;
        public Texture2D[] textures;
        public WMOMaterial[] materials;
        public MaterialInfo[] materialInfo;
        public RenderBatch[] renderBatches;
    }

    public struct RenderBatch
    {
        public uint numFaces;
        public uint firstFace;
        public uint materialID;
    }

    public struct WMOMaterial
    {
        public uint materialID;
        public string filename;
        public Texture2D texture;
    }

    public struct MaterialInfo
    {
        public byte flags;
        public byte materialID;
    }
}
