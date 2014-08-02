using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D11;

namespace WoWRenderLib.Structs.WoWWMO
{
    public struct WoWWMO
    {
        public string name;
        public WMOMaterial[] materials;
        public WoWWMOGroup[] groups;
    }

    public struct WoWWMOGroup
    {
        public string groupName;
        public float[] vertices;
        public ushort[] indices;
        public WMORenderBatch[] renderBatches;
        public MaterialInfo[] materialInfo;
    }

    public struct WMORenderBatch
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
