using DBFilesClient.NET;

namespace WoWFormatLib.DBC
{
    public sealed class CreatureEntry
    {
        public uint[] unk0;
        public uint unk1;
        public uint[] DisplayID;
        public float[] unk2;
        public string Name;
        public string AlternateName;
        public string Title;
        public string unk3;
        public ushort unk4;
        public ushort unk5;
        public ushort unk6;
        public ushort unk7;
    }

    public sealed class CreatureDisplayInfoEntry
    {
        public uint ExtendedDisplayInfoID;
        public float CreatureModelScale;
        public float PlayerModelScale;
        public uint[] TextureVariation;
        public string PortraitTextureName;
        public uint PortraitCreatureDisplayInfoID;
        public uint CreatureGeosetData;
        public uint StateSpellVisualKitID;
        public float InstanceOtherPlayerPetScale;
        public ushort ModelID;
        public ushort SoundID;
        public ushort NPCSoundID;
        public ushort ParticleColorID;
        public ushort ObjectEffectPackageID;
        public ushort AnimReplacementSetID;
        public byte CreatureModelAlpha;
        public byte SizeClass;
        public byte BloodID;
        public byte Flags;
        public sbyte Gender;
        public sbyte Unk700;
    }

    public sealed class CreatureModelDataEntry
    {
        public float scale;
        public float footprintTextureLength;
        public float footprintTextureWidth;
        public float footprintParticleScale;
        public float collisionWidth;
        public float collisionHeight;
        public float mountHeight;
        public float[] geoBoxMin;
        public float worldEffectScale;
        public float attachedEffectScale;
        public float missileCollisionRadius;
        public float missileCollisionPush;
        public float missileCollisionRaise;
        public float overrideLootEffectScale;
        public float overrideNameScale;
        public float overrideSelectionRadius;
        public float tamedPetBaseScale;
        public float hoverHeight;
        public uint flags;
        public int fileDataID;
        public int sizeClass;
        public int bloodID;
        public int footprintTextureID;
        public int unk0;
        public int unk1;
        public int foleyMaterialID;
        public int footstepShakeSize;
        public int deathThudShakeSize;
        public int soundID;
        public int creatureGeosetDataID;
    }

    public sealed class MapEntry
    {
        public string directory;
        public uint[] Flags;
        public float MinimapIconScale;
        public float[] CorpsePos;
        public string mapname_lang;
        public string MapDescription0;
        public string MapDescription1;
        public int AreaTableID;
        public int LoadingScreenID;
        public int CorpseMapID;
        public short TimeOfDayOverride;
        public int ParentMapID;
        public int CosmeticParentMapID;
        public ushort WindSettingsID;
        public byte InstanceType;
        public byte MapType;
        public byte ExpansionID;
        public byte MaxPlayers;
        public byte TimeOffset;
    }

    public sealed class MapEntry72
    {
        public string directory;
        public int[] flags;
        public float minimapIconScale;
        public float[] corpseCoords;
        public string mapname_lang;
        public string mapDescription0_lang;
        public string mapDescription1_lang;
        public string unk0;
        public string unk1;
        public ushort areaTableID;
        public short loadingScreenID;
        public short corpseMapID;
        public short timeOfDayoverride;
        public short parentMapID;
        public short cosmeticParentMapID;
        public short unk2;
        public byte instanceType;
        public byte mapType;
        public byte expansionID;
        public byte maxPlayers;
        public byte timeOffset;
    }

    public sealed class ModelFileDataEntry
    {
        public byte whoCares;
        public uint fileDataID;
        public ushort modelFileDataID;
    }

    public sealed class ItemDisplayInfoMaterialResEntry
    {
        public uint itemDisplayInfoID;
        public uint textureFileDataID;
        public byte whocares;
    }

    public sealed class TextureFileDataEntry
    {
        public int textureFileDataID;
        public byte whoCares;
        public uint fileDataID;
    }

    public class AchievementRecord
    {
        public int ID { get; private set; }
        public int faction { get; private set; }
        public int instance_id { get; private set; }
        public int supercedes { get; private set; }
        public string title_lang { get; private set; }
        public string description_lang { get; private set; }
        public int category { get; private set; }
        public int points { get; private set; }
        public int ui_order { get; private set; }
        public int flags { get; private set; }
        public int iconID { get; private set; }
        public string reward_lang { get; private set; }
        public int minimucriteria { get; private set; }
        public int shares_criteria { get; private set; }
        public int criteria_tree { get; private set; }
    }

    public class AchievementCategoryRecord
    {
        public int ID { get; private set; }
        public int parent { get; private set; }
        public string name_lang { get; private set; }
        public int ui_order { get; private set; }
    }

    public class AnimationDataRecord
    {
        public int ID { get; private set; }
        public string Name { get; private set; }
        public int Flags { get; private set; }
        public int fallBack { get; private set; }
        public int BehaviorID { get; private set; }
        public int BehaviorTier { get; private set; }
    }

    public class AnimKitRecord
    {
        public int ID;
        public int oneShotDuration;
        public int oneShotStopAnimKitID;
        public int lowDefAnimKitID;
    }
    
    public class AreaAssignmentRecord
    {
        public int ID { get; private set; }
        public int MapID { get; private set; }
        public int AreaID { get; private set; }
        public int Tile_X { get; private set; }
        public int Tile_Y { get; private set; }
    }
    
    public class AreaAssignmentRecordLegion
    {
        public short MapID { get; private set; }
        public short AreaID { get; private set; }
        public byte Tile_X { get; private set; }
        public byte Tile_Y { get; private set; }
    }

    public class AreaGroupRecord
    {
        public int ID { get; private set; }
        public int areaID_0 { get; private set; }
        public int areaID_1 { get; private set; }
        public int areaID_2 { get; private set; }
        public int areaID_3 { get; private set; }
        public int areaID_4 { get; private set; }
        public int areaID_5 { get; private set; }
        public int nextAreaID { get; private set; }
    }
    public class AreaPOIRecord
    {
        public int ID { get; private set; }
        public int flags { get; private set; }
        public int importance { get; private set; }
        public int factionID { get; private set; }
        public int continentID { get; private set; }
        public int areaID { get; private set; }
        public int icon { get; private set; }
        public float pos_x { get; private set; }
        public float pos_y { get; private set; }
        public string name_lang { get; private set; }
        public string description_lang { get; private set; }
        public int worldStateID { get; private set; }
        public int playerConditionID { get; private set; }
        public int worldMapLink { get; private set; }
        public int portLocID { get; private set; }
    }

    public class AreaTableRecord
    {
        public int ID { get; private set; }
        public int ContinentID { get; private set; }
        public int ParentAreaID { get; private set; }
        public int AreaBit { get; private set; }
        public int flags_0 { get; private set; }
        public int flags_1 { get; private set; }
        public int SoundProviderPref { get; private set; }
        public int SoundProviderPrefUnderwater { get; private set; }
        public int AmbienceID { get; private set; }
        public int ZoneMusic { get; private set; }
        public string ZoneName { get; private set; }
        public int IntroSound { get; private set; }
        public int ExplorationLevel { get; private set; }
        public string AreaName_lang { get; private set; }
        public int factionGroupMask { get; private set; }
        public int liquidTypeID_0 { get; private set; }
        public int liquidTypeID_1 { get; private set; }
        public int liquidTypeID_2 { get; private set; }
        public int liquidTypeID_3 { get; private set; }
        public float ambient_multiplier { get; private set; }
        public int mountFlags { get; private set; }
        public int uwIntroSound { get; private set; }
        public int uwZoneMusic { get; private set; }
        public int uwAmbience { get; private set; }
        public int world_pvp_id { get; private set; }
        public int pvpCombatWorldStateID { get; private set; }
        public int wildBattlePetLevelMin { get; private set; }
        public int wildBattlePetLevelMax { get; private set; }
        public int windSettingsID { get; private set; }
    }

    public class AreaTableRecordLegion
    {
        public int ID { get; private set; }
        public int ContinentID { get; private set; }
        public int ParentAreaID { get; private set; }
        public int AreaBit { get; private set; }
        public int flags_0 { get; private set; }
        public int flags_1 { get; private set; }
        public int SoundProviderPref { get; private set; }
        public int SoundProviderPrefUnderwater { get; private set; }
        public int AmbienceID { get; private set; }
        public int ZoneMusic { get; private set; }
        public string ZoneName { get; private set; }
        public int IntroSound { get; private set; }
        public int ExplorationLevel { get; private set; }
        public string AreaName_lang { get; private set; }
        public int factionGroupMask { get; private set; }
        public int liquidTypeID_0 { get; private set; }
        public int liquidTypeID_1 { get; private set; }
        public int liquidTypeID_2 { get; private set; }
        public int liquidTypeID_3 { get; private set; }
        public float ambient_multiplier { get; private set; }
        public int mountFlags { get; private set; }
        public int uwIntroSound { get; private set; }
        public int uwZoneMusic { get; private set; }
        public int uwAmbience { get; private set; }
        public int pvpCombatWorldStateID { get; private set; }
        public int wildBattlePetLevelMin { get; private set; }
        public int wildBattlePetLevelMax { get; private set; }
        public int windSettingsID { get; private set; }
    }

    public class AreaTriggerRecord
    {
        public int ID { get; private set; }
        public int ContinentID { get; private set; }
        public float pos_x { get; private set; }
        public float pos_y { get; private set; }
        public float pos_z { get; private set; }
        public int phaseUseFlags { get; private set; }
        public int phaseID { get; private set; }
        public int phaseGroupID { get; private set; }
        public float radius { get; private set; }
        public float box_length { get; private set; }
        public float box_width { get; private set; }
        public float box_height { get; private set; }
        public float box_yaw { get; private set; }
        public int shapeType { get; private set; }
        public int shapeID { get; private set; }
        public int areaTriggerActionSetID { get; private set; }
        public int flags { get; private set; }
    }

    public class CharSectionRecord
    {
        public int ID { get; private set; }
        public int raceID { get; private set; }
        public int sexID { get; private set; }
        public int baseSection { get; private set; }
        public string TextureName_0 { get; private set; }
        public string TextureName_1 { get; private set; }
        public string TextureName_2 { get; private set; }
        public int flags { get; private set; }
        public int variationIndex { get; private set; }
        public int colorIndex { get; private set; }
    }

    public class ChrRaceRecord
    {
        public int ID { get; private set; }
        public int flags { get; private set; }
        public int factionID { get; private set; }
        public int ExplorationSoundID { get; private set; }
        public int MaleDisplayId { get; private set; }
        public int FemaleDisplayId { get; private set; }
        public string ClientPrefix { get; private set; }
        public int BaseLanguage { get; private set; }
        public int creatureType { get; private set; }
        public int ResSicknessSpellID { get; private set; }
        public int SplashSoundID { get; private set; }
        public string clientFileString { get; private set; }
        public int cinematicSequenceID { get; private set; }
        public int alliance { get; private set; }
        public string name_lang { get; private set; }
        public string name_female_lang { get; private set; }
        public string name_male_lang { get; private set; }
        public string facialHairCustomization_0 { get; private set; }
        public string facialHairCustomization_1 { get; private set; }
        public string hairCustomization { get; private set; }
        public int race_related { get; private set; }
        public int unalteredVisualRaceID { get; private set; }
        public int uaMaleCreatureSoundDataID { get; private set; }
        public int uaFemaleCreatureSoundDataID { get; private set; }
        public int charComponentTextureLayoutID { get; private set; }
        public int defaultClassID { get; private set; }
        public int createScreenFileDataID { get; private set; }
        public int selectScreenFileDataID { get; private set; }
        public float maleCustomizeOffset_0 { get; private set; }
        public float maleCustomizeOffset_1 { get; private set; }
        public float maleCustomizeOffset_2 { get; private set; }
        public float femaleCustomizeOffset_0 { get; private set; }
        public float femaleCustomizeOffset_1 { get; private set; }
        public float femaleCustomizeOffset_2 { get; private set; }
        public int neutralRaceID { get; private set; }
        public int lowResScreenFileDataID { get; private set; }
        public int HighResMaleDisplayId { get; private set; }
        public int HighResFemaleDisplayId { get; private set; }
        public int charComponentTexLayoutHiResID { get; private set; }
    }

    public class CreatureRecord
    {
        public uint ID { get; private set; }
        public uint type { get; private set; } //Probably CreatureType.ID (1-12, 14-15)
        public uint unk2 { get; private set; }
        public uint unk3 { get; private set; }
        public uint unk4 { get; private set; }
        public uint unk5 { get; private set; }
        public uint displayID_0 { get; private set; } //CreatureDisplayInfoRecord.ID
        public uint displayID_1 { get; private set; } //CreatureDisplayInfoRecord.ID
        public uint displayID_2 { get; private set; } //CreatureDisplayInfoRecord.ID
        public uint displayID_3 { get; private set; } //CreatureDisplayInfoRecord.ID
        public uint unk10 { get; private set; } //0, 1, 2, 3, 9, 25, 50, 99, 100
        public uint unk11 { get; private set; } //0, 1, 2, 3, 9, 25, 50
        public uint unk12 { get; private set; } //0, 1, 2, 3, 25
        public uint unk13 { get; private set; } //0, 1, 25
        public string name { get; private set; }
        public string somekindoftitle { get; private set; } //Bigger Warsong Wolf is only value
        public string title { get; private set; }
        public uint unk17 { get; private set; } //0
        public uint unk18 { get; private set; } //1, 2, 3, 4, 6, 
        public uint unk19 { get; private set; } //0, 2, 3, 4
    }

    public class CreatureDisplayInfoRecord
    {
        public int ID { get; private set; }
        public int modelID { get; private set; }
        public int soundID { get; private set; }
        public int extendedDisplayInfoID { get; private set; }
        public float creatureModelScale { get; private set; }
        public int creatureModelAlpha { get; private set; }
        public string textureVariation_0 { get; private set; }
        public string textureVariation_1 { get; private set; }
        public string textureVariation_2 { get; private set; }
        public string portraitTextureName { get; private set; }
        public int portraitCreatureDisplayInfoID { get; private set; }
        public int sizeClass { get; private set; }
        public int bloodID { get; private set; }
        public int NPCSoundID { get; private set; }
        public int particleColorID { get; private set; }
        public int creatureGeosetData { get; private set; }
        public int objectEffectPackageID { get; private set; }
        public int animReplacementSetID { get; private set; }
        public int flags { get; private set; }
        public int gender { get; private set; }
        public int stateSpellVisualKitID { get; private set; }
    }

    public class CreatureDisplayInfoExtraRecord
    {
        public int ID { get; private set; }
        public int DisplayRaceID { get; private set; }
        public int DisplaySexID { get; private set; }
        public int SkinID { get; private set; }
        public int FaceID { get; private set; }
        public int HairStyleID { get; private set; }
        public int HairColorID { get; private set; }
        public int FacialHairID { get; private set; }
        public int NPCItemDisplay_0 { get; private set; }
        public int NPCItemDisplay_1 { get; private set; }
        public int NPCItemDisplay_2 { get; private set; }
        public int NPCItemDisplay_3 { get; private set; }
        public int NPCItemDisplay_4 { get; private set; }
        public int NPCItemDisplay_5 { get; private set; }
        public int NPCItemDisplay_6 { get; private set; }
        public int NPCItemDisplay_7 { get; private set; }
        public int NPCItemDisplay_8 { get; private set; }
        public int NPCItemDisplay_9 { get; private set; }
        public int NPCItemDisplay_10 { get; private set; }
        public int flags { get; private set; }
        public int fileDataID { get; private set; }
        public int hdFileDataID { get; private set; }
    }

    public class CreatureFamilyRecord
    {
        public int ID { get; private set; }
        public float minScale { get; private set; }
        public int minScaleLevel { get; private set; }
        public float maxScale { get; private set; }
        public int maxScaleLevel { get; private set; }
        public int skillLine_0 { get; private set; }
        public int skillLine_1 { get; private set; }
        public int petFoodMask { get; private set; }
        public int petTalentType { get; private set; }
        public int categoryEnumID { get; private set; }
        public string name_lang { get; private set; }
        public string iconFile { get; private set; }
    }

    public class CreatureTypeRecord
    {
        public int ID { get; private set; }
        public string name_lang { get; private set; }
        public int flags { get; private set; }
    }

    public class FileDataRecord
    {
        public int ID { get; private set; }
        public string FileName { get; private set; }
        public string FilePath { get; private set; }
    }

    public class MapRecord
    {
        public int ID { get; private set; }
        public string Directory { get; private set; }
        public int InstanceType { get; private set; }
        public int Flags { get; private set; }
        public int unk { get; private set; }
        public int MapType { get; private set; }
        public string Mapname_lang { get; private set; }
        public int areaTableID { get; private set; }
        public string MapDescription0_lang { get; private set; }
        public string MapDescription1_lang { get; private set; }
        public int LoadingScreenID { get; private set; }
        public float minimapIconScale { get; private set; }
        public int corpseMapID { get; private set; }
        public float corpse_x { get; private set; }
        public float corpse_y { get; private set; }
        public int timeOfDayoverride { get; private set; }
        public int expansionID { get; private set; }
        public int raidOffset { get; private set; }
        public int maxPlayers { get; private set; }
        public int parentMapID { get; private set; }
        public int cosmeticParentMapID { get; private set; }
        public int timeOffset { get; private set; }
    }

    public class MapRecordLegion
    {
        public int ID { get; private set; }
        public string Directory { get; private set; }
        public int Flags { get; private set; }
        public int Flags2 { get; private set; }
        public float minimapIconScale { get; private set; }
        public float corpse_x { get; private set; }
        public float corpse_y { get; private set; }
        public int raidOffset { get; private set; }
        public string Mapname_lang { get; private set; }
        public string MapDescription0_lang { get; private set; }
        public string MapDescription1_lang { get; private set; }
        public ushort areaTableID { get; private set; }
        public short loadingScreenID { get; private set; }
        public short corpseMapID { get; private set; }
        public short timeOfDayoverride { get; private set; }
        public short parentMapID { get; private set; }
        public short cosmeticParentMapID { get; private set; }
        public byte instanceType { get; private set; }
        public byte MapType { get; private set; }
        public byte expansionID { get; private set; }
        public byte maxPlayers { get; private set; }
        public byte timeOffset { get; private set; }
    }
}

