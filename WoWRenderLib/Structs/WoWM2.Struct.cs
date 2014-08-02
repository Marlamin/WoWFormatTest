using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWRenderLib.Structs
{
    public struct WoWM2
    {
        public float[] indices;
        public string name;
        public Texture2D[] textures;
        public float[] vertices;
    }
}