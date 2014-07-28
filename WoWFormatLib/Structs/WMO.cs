using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWFormatLib.Utils;

namespace WoWFormatLib.Structs.WMO
{
    public struct WMO
    {
        public MOHD header;
        public MVER version;
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

    public struct MOTX
    {

    }

    public struct MOMT
    {

    }

    public struct MOGN
    {

    }


}
