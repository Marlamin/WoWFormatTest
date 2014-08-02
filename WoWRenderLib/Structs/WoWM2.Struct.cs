using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWRenderLib.Structs.WoWM2
{
    public struct WoWM2
    {
        public string name;
        public ushort[] indices;
        public float[] vertices;
        public M2Material[] materials;
        //public Texture2D[] textures;
        public M2RenderBatch[] renderBatches;
    }

    public struct M2Material
    {
        public string filename;
        public WoWFormatLib.Structs.M2.TextureFlags flags;
        public Texture2D texture;
    }

    public struct M2RenderBatch
    {
        public uint numFaces;
        public uint firstFace;
        public uint materialID;
    }
}