using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWFormatLib.DBC
{
    public struct DBCHeader
    {
        public uint magic;
        public uint record_count;
        public uint field_count;
        public uint record_size;
        public uint string_block_size;
    }

    public struct MapRecord
    {
        public int ID;
        public uint Directory;
        public int InstanceType;
        public int Flags;
        public int MapType;
        public uint Mapname_lang;
        public int areaTableID;
        public uint MapDescription0_lang;
        public uint MapDescription1_lang;
        public int LoadingScreenID;
        public float minimapIconScale; //Like I'll ever have to use this. HA!
        public int corpseMapID;
        public float corpse_x;
        public float corpse_y;
        public int timeOfDayoverride;
        public int expansionID;
        public int raidOffset;
        public int maxPlayers;
        public int parentMapID;
        public int cosmeticParentMapID;
        public int timeOffset;
        public int unk1;
    }
}
