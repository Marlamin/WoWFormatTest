using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWFormatLib.Utils
{
    public struct RGBColor
    {
        public float R;
        public float G;
        public float B;

        public RGBColor(float r, float g, float b)
            : this()
        {
            R = r;
            G = g;
            B = b;
        }
    }
}
