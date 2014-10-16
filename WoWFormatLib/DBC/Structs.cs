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

    public struct DB2Header
    {
        public uint magic;
        public uint record_count;
        public uint field_count;
        public uint record_size;
        public uint string_block_size;
        public uint tablehash;
        public uint build;
        public uint timestamp_last_written;
        public uint min_id;
        public uint max_id;
        public uint locale;
        public uint unk;
    }

    public struct CreatureRecord
    {
        public uint ID;
        public uint type; //Probably CreatureType.ID (1-12, 14-15)
        public uint unk2;
        public uint unk3;
        public uint unk4;
        public uint unk5;
        public uint displayID1; //CreatureDisplayInfoRecord.ID
        public uint displayID2; //CreatureDisplayInfoRecord.ID
        public uint displayID3; //CreatureDisplayInfoRecord.ID
        public uint displayID4; //CreatureDisplayInfoRecord.ID
        public uint unk10; //0, 1, 2, 3, 9, 25, 50, 99, 100
        public uint unk11; //0, 1, 2, 3, 9, 25, 50
        public uint unk12; //0, 1, 2, 3, 25
        public uint unk13; //0, 1, 25
        public uint name;
        public uint somekindoftitle; //Bigger Warsong Wolf is only value
        public uint title;
        public uint unk17; //0
        public uint unk18; //1, 2, 3, 4, 6, 
        public uint unk19; //0, 2, 3, 4
    }

    public struct CreatureDisplayInfoRecord
    {
        public int ID;
        public int modelID;
        public int soundID;
        public int extendedDisplayInfoID;
        public float creatureModelScale;
        public int creatureModelAlpha;
        public uint textureVariation_0;
        public uint textureVariation_1;
        public uint textureVariation_2;
        public uint portraitTextureName;
        public int portraitCreatureDisplayInfoID;
        public int sizeClass;
        public int bloodID;
        public int NPCSoundID;
        public int particleColorID;
        public int creatureGeosetData;
        public int objectEffectPackageID;
        public int animReplacementSetID;
        public int flags;
        public int gender;
        public int stateSpellVisualKitID;
    }

    public struct CreatureDisplayInfoExtraRecord
    {
        public int ID;
        public int DisplayRaceID;
        public int DisplaySexID;
        public int SkinID;
        public int FaceID;
        public int HairStyleID;
        public int HairColorID;
        public int FacialHairID;
        public int NPCItemDisplay_0;
        public int NPCItemDisplay_1;
        public int NPCItemDisplay_2;
        public int NPCItemDisplay_3;
        public int NPCItemDisplay_4;
        public int NPCItemDisplay_5;
        public int NPCItemDisplay_6;
        public int NPCItemDisplay_7;
        public int NPCItemDisplay_8;
        public int NPCItemDisplay_9;
        public int NPCItemDisplay_10;
        public int flags;
        public int fileDataID;
        public int hdFileDataID;
    }

    public struct CreatureModelDataRecord
    {
        public int ID;
        public int flags;
        public int fileDataID;
        public int sizeClass;
        public float modelScale;
        public int bloodID;
        public int footprintTextureID;
        public float footprintTextureLength;
        public float footprintTextureWidth;
        public float footprintParticleScale;
        public int foleyMaterialID;
        public int footstepShakeSize;
        public int deathThudShakeSize;
        public int soundID;
        public float collisionWidth;
        public float collisionHeight;
        public float mountHeight;
        public float geoBoxMin_0;
        public float geoBoxMin_1;
        public float geoBoxMin_2;
        public float geoBoxMax_0;
        public float geoBoxMax_1;
        public float geoBoxMax_2;
        public float worldEffectScale;
        public float attachedEffectScale;
        public float missileCollisionRadius;
        public float missileCollisionPush;
        public float missileCollisionRaise;
        public float overrideLootEffectScale;
        public float overrideNameScale;
        public float overrideSelectionRadius;
        public float tamedPetBaseScale;
        public int creatureGeosetDataID;
        public float hoverHeight;
    }

    public struct FileDataRecord
    {
        public int ID;
        public uint FileName;
        public uint FilePath;
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
