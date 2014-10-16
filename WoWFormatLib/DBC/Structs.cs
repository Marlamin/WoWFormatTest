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

    public struct AreaAssignmentRecord
    {
        public int ID;
        public int MapID;
        public int AreaID;
        public int Tile_X;
        public int Tile_Y;
    }

    public struct AreaPOIRecord
    {
        public int ID;
        public int flags;
        public int importance;
        public int factionID;
        public int continentID;
        public int areaID;
        public int icon;
        public float pos_x;
        public float pos_y;
        public uint name_lang;
        public uint description_lang;
        public int worldStateID;
        public int playerConditionID;
        public int worldMapLink;
        public int portLocID;
    }

    public struct AreaTableRecord
    {
        public int ID;
        public int ContinentID;
        public int ParentAreaID;
        public int AreaBit;
        public int flags_0;
        public int flags_1;
        public int SoundProviderPref;
        public int SoundProviderPrefUnderwater;
        public int AmbienceID;
        public int ZoneMusic;
        public uint ZoneName;
        public int IntroSound;
        public int ExplorationLevel;
        public uint AreaName_lang;
        public int factionGroupMask;
        public int liquidTypeID_0;
        public int liquidTypeID_1;
        public int liquidTypeID_2;
        public int liquidTypeID_3;
        public float minElevation;
        public float ambient_multiplier;
        public int lightid;
        public int mountFlags;
        public int uwIntroSound;
        public int uwZoneMusic;
        public int uwAmbience;
        public int world_pvp_id;
        public int pvpCombatWorldStateID;
        public int windSettingsID;
    }

    public struct AreaTriggerRecord
    {
        public int ID;
        public int ContinentID;
        public float pos_x;
        public float pos_y;
        public float pos_z;
        public int phaseUseFlags;
        public int phaseID;
        public int phaseGroupID;
        public float radius;
        public float box_length;
        public float box_width;
        public float box_height;
        public float box_yaw;
        public int shapeType;
        public int shapeID;
        public int areaTriggerActionSetID;
        public int flags;
    }

    public struct CreatureRecord
    {
        public uint ID;
        public uint type; //Probably CreatureType.ID (1-12, 14-15)
        public uint unk2;
        public uint unk3;
        public uint unk4;
        public uint unk5;
        public uint displayID_0; //CreatureDisplayInfoRecord.ID
        public uint displayID_1; //CreatureDisplayInfoRecord.ID
        public uint displayID_2; //CreatureDisplayInfoRecord.ID
        public uint displayID_3; //CreatureDisplayInfoRecord.ID
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

    public struct CreatureFamilyRecord
    {
        public int ID;
        public float minScale;
        public int minScaleLevel;
        public float maxScale;
        public int maxScaleLevel;
        public int skillLine_0;
        public int skillLine_1;
        public int petFoodMask;
        public int petTalentType;
        public int categoryEnumID;
        public uint name_lang;
        public uint iconFile;
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

    public struct CreatureTypeRecord
    {
        public int ID;
        public uint name_lang;
        public int flags;
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