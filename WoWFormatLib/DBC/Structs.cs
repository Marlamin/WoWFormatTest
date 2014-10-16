using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWFormatLib.DBC
{
    public class AreaAssignmentRecord
    {
        public int ID { get; private set; }
        public int MapID { get; private set; }
        public int AreaID { get; private set; }
        public int Tile_X { get; private set; }
        public int Tile_Y { get; private set; }
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


    public class CreatureModelDataRecord
    {
        public int ID { get; private set; }
        public int flags { get; private set; }
        public int fileDataID { get; private set; }
        public int sizeClass { get; private set; }
        public float modelScale { get; private set; }
        public int bloodID { get; private set; }
        public int footprintTextureID { get; private set; }
        public float footprintTextureLength { get; private set; }
        public float footprintTextureWidth { get; private set; }
        public float footprintParticleScale { get; private set; }
        public int foleyMaterialID { get; private set; }
        public int footstepShakeSize { get; private set; }
        public int deathThudShakeSize { get; private set; }
        public int soundID { get; private set; }
        public float collisionWidth { get; private set; }
        public float collisionHeight { get; private set; }
        public float mountHeight { get; private set; }
        public float geoBoxMin_0 { get; private set; }
        public float geoBoxMin_1 { get; private set; }
        public float geoBoxMin_2 { get; private set; }
        public float geoBoxMax_0 { get; private set; }
        public float geoBoxMax_1 { get; private set; }
        public float geoBoxMax_2 { get; private set; }
        public float worldEffectScale { get; private set; }
        public float attachedEffectScale { get; private set; }
        public float missileCollisionRadius { get; private set; }
        public float missileCollisionPush { get; private set; }
        public float missileCollisionRaise { get; private set; }
        public float overrideLootEffectScale { get; private set; }
        public float overrideNameScale { get; private set; }
        public float overrideSelectionRadius { get; private set; }
        public float tamedPetBaseScale { get; private set; }
        public int creatureGeosetDataID { get; private set; }
        public float hoverHeight { get; private set; }
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
    
}

/*
 * Temporary list of DB(C/2)'s and whether they've been added and/or checked
 * [X] Done
 * [-] In progress
 * [ ] Nope
ADD CHECK NAME 
[ ]  [ ]  Achievement.dbc
[ ]  [ ]  Achievement_Category.dbc
[ ]  [ ]  AnimationData.dbc
[ ]  [ ]  AnimKit.dbc
[ ]  [ ]  AnimKitBoneSet.dbc
[ ]  [ ]  AnimKitBoneSetAlias.dbc
[ ]  [ ]  AnimKitConfig.dbc
[ ]  [ ]  AnimKitConfigBoneSet.dbc
[ ]  [ ]  AnimKitPriority.dbc
[ ]  [ ]  AnimKitSegment.dbc
[ ]  [ ]  AnimReplacement.dbc
[ ]  [ ]  AnimReplacementSet.dbc
[X]  [X]  AreaAssignment.dbc
[ ]  [ ]  AreaGroup.dbc
[X]  [X]  AreaPOI.db2
[ ]  [ ]  AreaPOIState.db2
[X]  [X]  AreaTable.dbc
[X]  [X]  AreaTrigger.dbc
[ ]  [ ]  AreaTriggerActionSet.dbc
[ ]  [ ]  AreaTriggerBox.dbc
[ ]  [ ]  AreaTriggerCylinder.dbc
[ ]  [ ]  AreaTriggerSphere.dbc
[ ]  [ ]  ArmorLocation.dbc
[ ]  [ ]  AttackAnimKits.dbc
[ ]  [ ]  AttackAnimTypes.dbc
[ ]  [ ]  AuctionHouse.dbc
[ ]  [ ]  BankBagSlotPrices.dbc
[ ]  [ ]  BannedAddOns.dbc
[ ]  [ ]  BarberShopStyle.dbc
[ ]  [ ]  BattleMasterList.dbc
[ ]  [ ]  BattlePetAbility.db2
[ ]  [ ]  BattlePetAbilityEffect.db2
[ ]  [ ]  BattlePetAbilityState.db2
[ ]  [ ]  BattlePetAbilityTurn.db2
[ ]  [ ]  BattlePetBreedQuality.db2
[ ]  [ ]  BattlePetBreedState.db2
[ ]  [ ]  BattlePetEffectProperties.db2
[ ]  [ ]  BattlePetNPCTeamMember.db2
[ ]  [ ]  BattlePetSpecies.db2
[ ]  [ ]  BattlePetSpeciesState.db2
[ ]  [ ]  BattlePetSpeciesXAbility.db2
[ ]  [ ]  BattlePetState.db2
[ ]  [ ]  BattlePetVisual.db2
[ ]  [ ]  BroadcastText.db2
[ ]  [ ]  CameraMode.dbc
[ ]  [ ]  CameraShakes.dbc
[ ]  [ ]  CastableRaidBuffs.dbc
[ ]  [ ]  Cfg_Categories.db2
[ ]  [ ]  Cfg_Configs.db2
[ ]  [ ]  Cfg_Regions.db2
[ ]  [ ]  CharacterFaceBoneSet.db2
[ ]  [ ]  CharacterFacialHairStyles.dbc
[ ]  [ ]  CharacterLoadout.dbc
[ ]  [ ]  CharacterLoadoutItem.dbc
[ ]  [ ]  CharBaseInfo.dbc
[ ]  [ ]  CharBaseSection.dbc
[ ]  [ ]  CharComponentTextureLayouts.dbc
[ ]  [ ]  CharComponentTextureSections.dbc
[ ]  [ ]  CharHairGeosets.dbc
[ ]  [ ]  CharSections.dbc
[ ]  [ ]  CharShipment.db2
[ ]  [ ]  CharShipmentContainer.db2
[ ]  [ ]  CharStartOutfit.dbc
[ ]  [ ]  CharTitles.dbc
[ ]  [ ]  ChatChannels.dbc
[ ]  [ ]  ChatProfanity.dbc
[ ]  [ ]  ChrClasses.dbc
[ ]  [ ]  ChrClassesXPowerTypes.dbc
[ ]  [ ]  ChrRaces.dbc
[ ]  [ ]  ChrSpecialization.dbc
[ ]  [ ]  ChrUpgradeBucket.db2
[ ]  [ ]  ChrUpgradeBucketSpell.db2
[ ]  [ ]  ChrUpgradeTier.db2
[ ]  [ ]  CinematicCamera.dbc
[ ]  [ ]  CinematicSequences.dbc
[ ]  [ ]  CombatCondition.dbc
[-]  [-]  Creature.db2
[ ]  [ ]  CreatureDifficulty.db2
[X]  [X]  CreatureDisplayInfo.dbc
[X]  [X]  CreatureDisplayInfoExtra.dbc
[X]  [X]  CreatureFamily.dbc
[ ]  [ ]  CreatureImmunities.dbc
[X]  [X]  CreatureModelData.dbc
[ ]  [ ]  CreatureMovementInfo.dbc
[ ]  [ ]  CreatureSoundData.dbc
[ ]  [ ]  CreatureSpellData.dbc
[X]  [X]  CreatureType.dbc
[ ]  [ ]  Criteria.dbc
[ ]  [ ]  CriteriaTree.dbc
[ ]  [ ]  CriteriaTreeXEffect.dbc
[ ]  [ ]  CurrencyCategory.dbc
[ ]  [ ]  CurrencyTypes.dbc
[ ]  [ ]  Curve.db2
[ ]  [ ]  CurvePoint.db2
[ ]  [ ]  DeathThudLookups.dbc
[ ]  [ ]  DestructibleModelData.dbc
[ ]  [ ]  DeviceBlacklist.db2
[ ]  [ ]  DeviceDefaultSettings.db2
[ ]  [ ]  Difficulty.dbc
[ ]  [ ]  DriverBlacklist.db2
[ ]  [ ]  DungeonEncounter.dbc
[ ]  [ ]  DungeonMap.dbc
[ ]  [ ]  DungeonMapChunk.dbc
[ ]  [ ]  DurabilityCosts.dbc
[ ]  [ ]  DurabilityQuality.dbc
[ ]  [ ]  Emotes.dbc
[ ]  [ ]  EmotesText.dbc
[ ]  [ ]  EmotesTextData.dbc
[ ]  [ ]  EmotesTextSound.dbc
[ ]  [ ]  EnvironmentalDamage.dbc
[ ]  [ ]  Exhaustion.dbc
[ ]  [ ]  Faction.dbc
[ ]  [ ]  FactionGroup.dbc
[ ]  [ ]  FactionTemplate.dbc
[ ]  [ ]  FeedbackPath.dbc
[X]  [X]  FileData.dbc
[ ]  [ ]  FootprintTextures.dbc
[ ]  [ ]  FootstepTerrainLookup.dbc
[ ]  [ ]  FriendshipRepReaction.dbc
[ ]  [ ]  FriendshipReputation.dbc
[ ]  [ ]  GameObjectArtKit.dbc
[ ]  [ ]  GameObjectDiffAnimMap.dbc
[ ]  [ ]  GameObjectDisplayInfo.dbc
[ ]  [ ]  GameObjects.db2
[ ]  [ ]  GameTables.dbc
[ ]  [ ]  GameTips.dbc
[ ]  [ ]  GarrAbility.db2
[ ]  [ ]  GarrAbilityCategory.db2
[ ]  [ ]  GarrAbilityEffect.db2
[ ]  [ ]  GarrBuilding.db2
[ ]  [ ]  GarrBuildingDoodadSet.db2
[ ]  [ ]  GarrBuildingPlotInst.db2
[ ]  [ ]  GarrClassSpec.db2
[ ]  [ ]  GarrEncounter.db2
[ ]  [ ]  GarrEncounterXMechanic.db2
[ ]  [ ]  GarrFollItemSet.db2
[ ]  [ ]  GarrFollItemSetMember.db2
[ ]  [ ]  GarrFollower.db2
[ ]  [ ]  GarrFollowerLevelXP.db2
[ ]  [ ]  GarrFollowerQuality.db2
[ ]  [ ]  GarrFollowerXAbility.db2
[ ]  [ ]  GarrMechanic.db2
[ ]  [ ]  GarrMechanicType.db2
[ ]  [ ]  GarrMission.db2
[ ]  [ ]  GarrMissionReward.db2
[ ]  [ ]  GarrMissionType.db2
[ ]  [ ]  GarrMissionXEncounter.db2
[ ]  [ ]  GarrPlot.db2
[ ]  [ ]  GarrPlotBuilding.db2
[ ]  [ ]  GarrPlotInstance.db2
[ ]  [ ]  GarrPlotUICategory.db2
[ ]  [ ]  GarrSiteLevel.db2
[ ]  [ ]  GarrSiteLevelPlotInst.db2
[ ]  [ ]  GarrSpecialization.db2
[ ]  [ ]  GarrUiAnimClassInfo.dbc
[ ]  [ ]  GarrUiAnimRaceInfo.dbc
[ ]  [ ]  GemProperties.dbc
[ ]  [ ]  GlueScreenEmote.dbc
[ ]  [ ]  GlyphExclusiveCategory.db2
[ ]  [ ]  GlyphProperties.dbc
[ ]  [ ]  GlyphRequiredSpec.db2
[ ]  [ ]  GlyphSlot.dbc
[ ]  [ ]  GMSurveyAnswers.dbc
[ ]  [ ]  GMSurveyCurrentSurvey.dbc
[ ]  [ ]  GMSurveyQuestions.dbc
[ ]  [ ]  GMSurveySurveys.dbc
[ ]  [ ]  GMTicketCategory.dbc
[ ]  [ ]  GroundEffectDoodad.dbc
[ ]  [ ]  GroundEffectTexture.dbc
[ ]  [ ]  GroupFinderActivity.db2
[ ]  [ ]  GroupFinderActivityGrp.db2
[ ]  [ ]  GroupFinderCategory.db2
[ ]  [ ]  gtArmorMitigationByLvl.dbc
[ ]  [ ]  gtBarberShopCostBase.dbc
[ ]  [ ]  gtBattlePetTypeDamageMod.dbc
[ ]  [ ]  gtBattlePetXP.dbc
[ ]  [ ]  gtChanceToMeleeCrit.dbc
[ ]  [ ]  gtChanceToMeleeCritBase.dbc
[ ]  [ ]  gtChanceToSpellCrit.dbc
[ ]  [ ]  gtChanceToSpellCritBase.dbc
[ ]  [ ]  gtCombatRatings.dbc
[ ]  [ ]  gtItemSocketCostPerLevel.dbc
[ ]  [ ]  gtNPCManaCostScaler.dbc
[ ]  [ ]  gtOCTBaseHPByClass.dbc
[ ]  [ ]  gtOCTBaseMPByClass.dbc
[ ]  [ ]  gtOCTClassCombatRatingScalar.dbc
[ ]  [ ]  gtOCTHpPerStamina.dbc
[ ]  [ ]  gtOCTLevelExperience.dbc
[ ]  [ ]  gtRegenMPPerSpt.dbc
[ ]  [ ]  gtResilienceDR.dbc
[ ]  [ ]  gtSpellScaling.dbc
[ ]  [ ]  GuildColorBackground.dbc
[ ]  [ ]  GuildColorBorder.dbc
[ ]  [ ]  GuildColorEmblem.dbc
[ ]  [ ]  GuildPerkSpells.dbc
[ ]  [ ]  HelmetAnimScaling.dbc
[ ]  [ ]  HelmetGeosetVisData.dbc
[ ]  [ ]  HighlightColor.db2
[ ]  [ ]  HolidayDescriptions.db2
[ ]  [ ]  HolidayNames.db2
[ ]  [ ]  Holidays.db2
[ ]  [ ]  ImportPriceArmor.dbc
[ ]  [ ]  ImportPriceQuality.dbc
[ ]  [ ]  ImportPriceShield.dbc
[ ]  [ ]  ImportPriceWeapon.dbc
[ ]  [ ]  Item.db2
[ ]  [ ]  ItemAppearance.db2
[ ]  [ ]  ItemArmorQuality.dbc
[ ]  [ ]  ItemArmorShield.dbc
[ ]  [ ]  ItemArmorTotal.dbc
[ ]  [ ]  ItemBagFamily.dbc
[ ]  [ ]  ItemBonus.db2
[ ]  [ ]  ItemBonusTreeNode.db2
[ ]  [ ]  ItemClass.dbc
[ ]  [ ]  ItemCurrencyCost.db2
[ ]  [ ]  ItemDamageAmmo.dbc
[ ]  [ ]  ItemDamageOneHand.dbc
[ ]  [ ]  ItemDamageOneHandCaster.dbc
[ ]  [ ]  ItemDamageRanged.dbc
[ ]  [ ]  ItemDamageThrown.dbc
[ ]  [ ]  ItemDamageTwoHand.dbc
[ ]  [ ]  ItemDamageTwoHandCaster.dbc
[ ]  [ ]  ItemDamageWand.dbc
[ ]  [ ]  ItemDisenchantLoot.dbc
[ ]  [ ]  ItemDisplayInfo.dbc
[ ]  [ ]  ItemEffect.db2
[ ]  [ ]  ItemExtendedCost.db2
[ ]  [ ]  ItemGroupSounds.dbc
[ ]  [ ]  ItemLimitCategory.dbc
[ ]  [ ]  ItemModifiedAppearance.db2
[ ]  [ ]  ItemNameDescription.dbc
[ ]  [ ]  ItemPetFood.dbc
[ ]  [ ]  ItemPriceBase.dbc
[ ]  [ ]  ItemPurchaseGroup.dbc
[ ]  [ ]  ItemRandomProperties.dbc
[ ]  [ ]  ItemRandomSuffix.dbc
[ ]  [ ]  ItemSet.dbc
[ ]  [ ]  ItemSetSpell.dbc
[ ]  [ ]  Item-sparse.db2
[ ]  [ ]  ItemSpec.dbc
[ ]  [ ]  ItemSpecOverride.dbc
[ ]  [ ]  ItemSubClass.dbc
[ ]  [ ]  ItemSubClassMask.dbc
[ ]  [ ]  ItemToBattlePetSpecies.db2
[ ]  [ ]  ItemToMountSpell.db2
[ ]  [ ]  ItemUpgrade.db2
[ ]  [ ]  ItemUpgradePath.dbc
[ ]  [ ]  ItemVisualEffects.dbc
[ ]  [ ]  ItemVisuals.dbc
[ ]  [ ]  ItemXBonusTree.db2
[ ]  [ ]  JournalEncounter.dbc
[ ]  [ ]  JournalEncounterCreature.dbc
[ ]  [ ]  JournalEncounterItem.dbc
[ ]  [ ]  JournalEncounterSection.dbc
[ ]  [ ]  JournalEncounterXDifficulty.dbc
[ ]  [ ]  JournalInstance.dbc
[ ]  [ ]  JournalItemXDifficulty.dbc
[ ]  [ ]  JournalSectionXDifficulty.dbc
[ ]  [ ]  JournalTier.dbc
[ ]  [ ]  JournalTierXInstance.dbc
[ ]  [ ]  KeyChain.db2
[ ]  [ ]  Languages.dbc
[ ]  [ ]  LanguageWords.db2
[ ]  [ ]  LFGDungeonExpansion.dbc
[ ]  [ ]  LfgDungeonGroup.dbc
[ ]  [ ]  LfgDungeons.dbc
[ ]  [ ]  LFGDungeonsGroupingmap.dbc
[ ]  [ ]  LfgRoleRequirement.db2
[ ]  [ ]  Light.dbc
[ ]  [ ]  LightData.dbc
[ ]  [ ]  LightParams.dbc
[ ]  [ ]  LightSkybox.dbc
[ ]  [ ]  LiquidMaterial.dbc
[ ]  [ ]  LiquidObject.dbc
[ ]  [ ]  LiquidType.dbc
[ ]  [ ]  LoadingScreens.dbc
[ ]  [ ]  LoadingScreenTaxiSplines.dbc
[ ]  [ ]  Locale.db2
[ ]  [ ]  Location.db2
[ ]  [ ]  Lock.dbc
[ ]  [ ]  LockType.dbc
[ ]  [ ]  MailTemplate.dbc
[ ]  [ ]  ManifestInterfaceActionIcon.dbc
[ ]  [ ]  ManifestInterfaceData.dbc
[ ]  [ ]  ManifestInterfaceItemIcon.dbcl
[ ]  [ ]  ManifestInterfaceTOCData.dbc
[ ]  [ ]  ManifestMP3.dbc
[X]  [X]  Map.dbc
[ ]  [ ]  MapChallengeMode.db2
[ ]  [ ]  MapDifficulty.dbc
[ ]  [ ]  MarketingPromotionsXLocale.db2
[ ]  [ ]  Material.dbc
[ ]  [ ]  MinorTalent.dbc
[ ]  [ ]  ModelFileData.dbc
[ ]  [ ]  ModelManifest.db2
[ ]  [ ]  ModelNameToManifest.db2
[ ]  [ ]  ModifierTree.dbc
[ ]  [ ]  Mount.db2
[ ]  [ ]  MountCapability.dbc
[ ]  [ ]  MountType.dbc
[ ]  [ ]  Movie.dbc
[ ]  [ ]  MovieFileData.dbc
[ ]  [ ]  MovieOverlays.dbc
[ ]  [ ]  MovieVariation.dbc
[ ]  [ ]  NameGen.dbc
[ ]  [ ]  NamesProfanity.dbc
[ ]  [ ]  NamesReserved.dbc
[ ]  [ ]  NamesReservedLocale.dbc
[ ]  [ ]  NPCSounds.dbc
[ ]  [ ]  ObjectEffect.dbc
[ ]  [ ]  ObjectEffectGroup.dbc
[ ]  [ ]  ObjectEffectModifier.dbc
[ ]  [ ]  ObjectEffectPackage.dbc
[ ]  [ ]  ObjectEffectPackageElem.dbc
[ ]  [ ]  OverrideSpellData.db2
[ ]  [ ]  Package.dbc
[ ]  [ ]  PageTextMaterial.dbc
[ ]  [ ]  PaperDollItemFrame.dbc
[ ]  [ ]  ParticleColor.dbc
[ ]  [ ]  Path.db2
[ ]  [ ]  PathNode.db2
[ ]  [ ]  PathNodeProperty.db2
[ ]  [ ]  PathProperty.db2
[ ]  [ ]  Phase.dbc
[ ]  [ ]  PhaseShiftZoneSounds.dbc
[ ]  [ ]  PhaseXPhaseGroup.db2
[ ]  [ ]  PlayerCondition.db2
[ ]  [ ]  PowerDisplay.dbc
[ ]  [ ]  PvpDifficulty.dbc
[ ]  [ ]  PvpItem.db2
[ ]  [ ]  QuestFactionReward.dbc
[ ]  [ ]  QuestFeedbackEffect.dbc
[ ]  [ ]  QuestInfo.dbc
[ ]  [ ]  QuestLine.db2
[ ]  [ ]  QuestLineXQuest.db2
[ ]  [ ]  QuestMoneyReward.dbc
[ ]  [ ]  QuestObjectiveCliTask.db2
[ ]  [ ]  QuestPackageItem.db2
[ ]  [ ]  QuestPOIBlob.dbc
[ ]  [ ]  QuestPOIPoint.dbc
[ ]  [ ]  QuestPOIPointCliTask.db2
[ ]  [ ]  QuestSort.dbc
[ ]  [ ]  QuestV2.dbc
[ ]  [ ]  QuestV2CliTask.db2
[ ]  [ ]  QuestXP.dbc
[ ]  [ ]  RacialMounts.dbc
[ ]  [ ]  RandPropPoints.dbc
[ ]  [ ]  ResearchBranch.dbc
[ ]  [ ]  ResearchField.dbc
[ ]  [ ]  ResearchProject.dbc
[ ]  [ ]  ResearchSite.dbc
[ ]  [ ]  Resistances.dbc
[ ]  [ ]  RulesetItemUpgrade.db2
[ ]  [ ]  RulesetRaidLootUpgrade.db2
[ ]  [ ]  RulesetRaidOverride.dbc
[ ]  [ ]  ScalingStatDistribution.dbc
[ ]  [ ]  Scenario.dbc
[ ]  [ ]  ScenarioEventEntry.dbc
[ ]  [ ]  ScenarioStep.dbc
[ ]  [ ]  SceneScript.db2
[ ]  [ ]  SceneScriptPackage.db2
[ ]  [ ]  SceneScriptPackageMember.db2
[ ]  [ ]  ScreenEffect.dbc
[ ]  [ ]  ScreenLocation.dbc
[ ]  [ ]  ServerMessages.dbc
[ ]  [ ]  SkillLine.dbc
[ ]  [ ]  SkillLineAbility.dbc
[ ]  [ ]  SkillLineAbilitySortedSpell.dbc
[ ]  [ ]  SkillRaceClassInfo.dbc
[ ]  [ ]  SkillTiers.dbc
[ ]  [ ]  SoundAmbience.dbc
[ ]  [ ]  SoundAmbienceFlavor.dbc
[ ]  [ ]  SoundBus.dbc
[ ]  [ ]  SoundBusName.dbc
[ ]  [ ]  SoundEmitterPillPoints.dbc
[ ]  [ ]  SoundEmitters.dbc
[ ]  [ ]  SoundEntries.dbc
[ ]  [ ]  SoundEntriesAdvanced.dbc
[ ]  [ ]  SoundEntriesFallbacks.dbc
[ ]  [ ]  SoundFilter.dbc
[ ]  [ ]  SoundFilterElem.dbc
[ ]  [ ]  SoundOverride.dbc
[ ]  [ ]  SoundProviderPreferences.dbc
[ ]  [ ]  SpamMessages.dbc
[ ]  [ ]  SpecializationSpells.dbc
[ ]  [ ]  Spell.dbc
[ ]  [ ]  SpellActionBarPref.db2
[ ]  [ ]  SpellActivationOverlay.dbc
[ ]  [ ]  SpellAuraOptions.dbc
[ ]  [ ]  SpellAuraRestrictions.db2
[ ]  [ ]  SpellAuraRestrictionsDifficulty.db2
[ ]  [ ]  SpellAuraVisibility.dbc
[ ]  [ ]  SpellAuraVisXChrSpec.dbc
[ ]  [ ]  SpellCastingRequirements.db2
[ ]  [ ]  SpellCastTimes.dbc
[ ]  [ ]  SpellCategories.dbc
[ ]  [ ]  SpellCategory.dbc
[ ]  [ ]  SpellChainEffects.dbc
[ ]  [ ]  SpellClassOptions.db2
[ ]  [ ]  SpellCooldowns.dbc
[ ]  [ ]  SpellDescriptionVariables.dbc
[ ]  [ ]  SpellDispelType.dbc
[ ]  [ ]  SpellDuration.dbc
[ ]  [ ]  SpellEffect.dbc
[ ]  [ ]  SpellEffectCameraShakes.db2
[ ]  [ ]  SpellEffectGroupSize.db2
[ ]  [ ]  SpellEffectScaling.dbc
[ ]  [ ]  SpellEquippedItems.dbc
[ ]  [ ]  SpellFlyout.dbc
[ ]  [ ]  SpellFlyoutItem.dbc
[ ]  [ ]  SpellFocusObject.dbc
[ ]  [ ]  SpellIcon.dbc
[ ]  [ ]  SpellInterrupts.dbc
[ ]  [ ]  SpellItemEnchantment.dbc
[ ]  [ ]  SpellItemEnchantmentCondition.dbc
[ ]  [ ]  SpellKeyboundOverride.dbc
[ ]  [ ]  SpellLearnSpell.db2
[ ]  [ ]  SpellLevels.dbc
[ ]  [ ]  SpellMechanic.db2
[ ]  [ ]  SpellMisc.db2
[ ]  [ ]  SpellMiscDifficulty.db2
[ ]  [ ]  SpellMissile.db2
[ ]  [ ]  SpellMissileMotion.db2
[ ]  [ ]  SpellPower.db2
[ ]  [ ]  SpellPowerDifficulty.db2
[ ]  [ ]  SpellProcsPerMinute.dbc
[ ]  [ ]  SpellProcsPerMinuteMod.dbc
[ ]  [ ]  SpellRadius.dbc
[ ]  [ ]  SpellRange.dbc
[ ]  [ ]  SpellReagents.db2
[ ]  [ ]  SpellRuneCost.db2
[ ]  [ ]  SpellScaling.dbc
[ ]  [ ]  SpellShapeshift.dbc
[ ]  [ ]  SpellShapeshiftForm.dbc
[ ]  [ ]  SpellSpecialUnitEffect.dbc
[ ]  [ ]  SpellTargetRestrictions.dbc
[ ]  [ ]  SpellTotems.db2
[ ]  [ ]  SpellVisual.db2
[ ]  [ ]  SpellVisualEffectName.db2
[ ]  [ ]  SpellVisualKit.db2
[ ]  [ ]  SpellVisualKitAreaModel.db2
[ ]  [ ]  SpellVisualKitModelAttach.db2
[ ]  [ ]  SpellVisualMissile.db2
[ ]  [ ]  Startup_Strings.dbc
[ ]  [ ]  Stationery.dbc
[ ]  [ ]  StringLookups.dbc
[ ]  [ ]  SummonProperties.dbc
[ ]  [ ]  Talent.dbc
[ ]  [ ]  TaxiNodes.db2
[ ]  [ ]  TaxiPath.db2
[ ]  [ ]  TaxiPathNode.db2
[ ]  [ ]  TerrainMaterial.dbc
[ ]  [ ]  TerrainType.dbc
[ ]  [ ]  TerrainTypeSounds.dbc
[ ]  [ ]  TextureFileData.db2
[ ]  [ ]  TotemCategory.dbc
[ ]  [ ]  Toy.db2
[ ]  [ ]  TradeSkillCategory.dbc
[ ]  [ ]  TransportAnimation.dbc
[ ]  [ ]  TransportPhysics.dbc
[ ]  [ ]  TransportRotation.dbc
[ ]  [ ]  Trophy.db2
[ ]  [ ]  TrophyInstance.db2
[ ]  [ ]  TrophyType.db2
[ ]  [ ]  UiTextureAtlas.db2
[ ]  [ ]  UiTextureAtlasMember.db2
[ ]  [ ]  UiTextureKit.db2
[ ]  [ ]  UnitBlood.dbc
[ ]  [ ]  UnitBloodLevels.dbc
[ ]  [ ]  UnitCondition.dbc
[ ]  [ ]  UnitPowerBar.dbc
[ ]  [ ]  Vehicle.dbc
[ ]  [ ]  VehicleSeat.dbc
[ ]  [ ]  VehicleUIIndicator.dbc
[ ]  [ ]  VehicleUIIndSeat.dbc
[ ]  [ ]  VideoHardware.dbc
[ ]  [ ]  Vignette.db2
[ ]  [ ]  VocalUISounds.dbc
[ ]  [ ]  WbAccessControlList.db2
[ ]  [ ]  WbCertBlacklist.db2
[ ]  [ ]  WbCertWhitelist.db2
[ ]  [ ]  WbPermissions.db2
[ ]  [ ]  WeaponImpactSounds.dbc
[ ]  [ ]  WeaponSwingSounds2.dbc
[ ]  [ ]  WeaponTrail.db2
[ ]  [ ]  Weather.dbc
[ ]  [ ]  WindSettings.db2
[ ]  [ ]  WMOAreaTable.dbc
[ ]  [ ]  world_PVP_Area.dbc
[ ]  [ ]  WorldBossLockout.db2
[ ]  [ ]  WorldChunkSounds.dbc
[ ]  [ ]  WorldEffect.dbc
[ ]  [ ]  WorldElapsedTimer.dbc
[ ]  [ ]  WorldMapArea.dbc
[ ]  [ ]  WorldMapContinent.dbc
[ ]  [ ]  WorldMapOverlay.dbc
[ ]  [ ]  WorldMapTransforms.dbc
[ ]  [ ]  WorldSafeLocs.dbc
[ ]  [ ]  WorldState.dbc
[ ]  [ ]  WorldStateExpression.dbc
[ ]  [ ]  WorldStateUI.dbc
[ ]  [ ]  WorldStateZoneSounds.dbc
[ ]  [ ]  WowError_Strings.dbc
[ ]  [ ]  ZoneIntroMusicTable.dbc
[ ]  [ ]  ZoneLight.dbc
[ ]  [ ]  ZoneLightPoint.dbc
[ ]  [ ]  ZoneMusic.dbc
*/