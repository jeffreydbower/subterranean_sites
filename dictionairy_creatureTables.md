CREATURE / MONSTER POPULATION TABLE CANDIDATES FOR BASICLAIR.TABLE

Source: parsed PopulationTables.xml end-to-end from the uploaded file.
Population entries parsed: 1511

Purpose:
BasicLair.Table is passed to SultanDungeon as monstertable.
It should be an exact PopulationTables.xml population name that expands to creature blueprints or creature-like encounter groups.

How to enter a table argument:
lair.Table = "Tier3CavePopulation";
lair.Table = "GoatfolkParty";
lair.Table = "DynamicInheritsTable:Creature:Tier{zonetier}";

Do not add Data:, Lairs_, or XML syntax unless the exact table name includes it.

Important:
This file classifies candidate monster/creature tables by table name and table structure.
PopulationTables.xml does not label tables as 'monster' or 'loot'. Some tables are mixed encounters and should be tested.

Best first tests:
lair.Table = "Tier3CavePopulation";
lair.Table = "Tier{zonetier}CavePopulation";   // may not exist as a literal table; use dynamic table below instead if needed
lair.Table = "DynamicInheritsTable:Creature:Tier{zonetier}";
lair.Table = "GoatfolkParty";
lair.Table = "LairOwners_Ruins";

Dynamic table forms that should be valid even though they are not literal XML entries:
DynamicInheritsTable:Creature:Tier{zonetier}
DynamicInheritsTable:Creature:Tier1
DynamicInheritsTable:Creature:Tier2
DynamicInheritsTable:Creature:Tier3
DynamicInheritsTable:Creature:Tier4
DynamicInheritsTable:Creature:Tier5
DynamicInheritsTable:Creature:Tier6
DynamicInheritsTable:Creature:Tier7
DynamicInheritsTable:Creature:Tier8

HIGH-CONFIDENCE DIRECT CREATURE SOURCE TABLES
---------------------------------------------
These are the strongest candidates for BasicLair.Table because their names indicate cave/ruin populations, creature tables, parties, bosses, or lair owners.
BaboonParty
BananaGroveCavePopulation
BananaGroveCreatures
BaroqueRuinsZoneGlobals-Creatures
BaroqueSubterraneanRuinsPopulation
BigBaboonParty
CatacombsPopulation
CrematoryPopulation
CryptCreatures
CryptPopulation
DeepJungleCavePopulation
DeepJungleZoneGlobals-Creatures
DesertCanyonCavePopulation
DesertCanyonCreatures
FlowerfieldCavePopulation
FlowerfieldCreatures
FungalJunglePopulation
GenericLairOwner
GoatfolkParty
GoatfolkQlippothParty
Golgotha Creatures
HillCavePopulation
HillCreatures
IssachariParty
JungleCavePopulation
JungleCreatures
LairBosses1
LairBosses2
LairBosses3
LairBosses4
LairBosses5
LairBosses6
LairBosses7
LairBosses8
LairOwners_BananaGrove
LairOwners_BaroqueRuins
LairOwners_DeepJungle
LairOwners_DesertCanyon
LairOwners_Flowerfields
LairOwners_Fungal
LairOwners_Hills
LairOwners_Jungle
LairOwners_LakeHinnom
LairOwners_MoonStair
LairOwners_Mountains
LairOwners_PalladiumReef
LairOwners_Ruins
LairOwners_Saltdunes
LairOwners_Saltmarsh
LairOwners_TheSpindle
LairOwners_Water
LairOwners_Watervine
LakeHinnomCavePopulation
MoonStairCavePopulation
MoonStairZoneGlobals-Creatures
MountainCavePopulation
MountainCreatures
PalladiumReefCavePopulation
RainbowWoodCavePopulation
Redrock1PerSector-Creatures
Redrock2PerSector-Creatures
Redrock3PerSector-Creatures
RedrockSnapjawCaveRoomCreatures
RuinsZoneGlobals-Creatures
SaltDuneCavePopulation
SaltDuneCreatures
SnapjawParty0
SnapjawParty1
SnapjawParty1-with-Warlord
SnapjawParty2
SubterraneanRuinsPopulation
Templar War Party Major
Templar War Party Minor
Tier1BossEncounter
Tier1CavePopulation
Tier1RuinsCreatures
Tier1RuinsPopulation
Tier2BossEncounter
Tier2CaveCreatures
Tier2CavePopulation
Tier2RuinsCreatures
Tier2RuinsPopulation
Tier3BossEncounter
Tier3CavePopulation
Tier3RuinsCreatures
Tier3RuinsPopulation
Tier4BossEncounter
Tier4CavePopulation
Tier4RuinsCreatures
Tier4RuinsPopulation
Tier5BossEncounter
Tier5CaveCreatures
Tier5CavePopulation
Tier5RuinsCreatures
Tier5RuinsPopulation
Tier6BossEncounter
Tier6CavePopulation
Tier6RuinsCreatures
Tier6RuinsPopulation
Tier7BossEncounter
Tier7CavePopulation
Tier7RuinsCreatures
Tier7RuinsPopulation
Tier8BossEncounter
Tier8CavePopulation
Tier8RuinsCreatures
Tier8RuinsPopulation
TombCreatures
WaterCavePopulation
WaterCreatures
WatervineCavePopulation
WatervineCreatures
YdFreeholdOutskirtsPopulation

TIERED CAVE / RUINS POPULATION TABLES
-------------------------------------
Good for depth-scaled generic inhabitants. Exact input is the table name.
Tier1CavePopulation
Tier1RuinsPopulation
Tier2CavePopulation
Tier2RuinsPopulation
Tier3CavePopulation
Tier3RuinsPopulation
Tier4CavePopulation
Tier4RuinsPopulation
Tier5CavePopulation
Tier5RuinsPopulation
Tier6CavePopulation
Tier6RuinsPopulation
Tier7CavePopulation
Tier7RuinsPopulation
Tier8CavePopulation
Tier8RuinsPopulation

BIOME / LOCATION POPULATION TABLES
----------------------------------
Good for themed site inhabitants.
BananaGroveCavePopulation
BaroqueSubterraneanRuinsPopulation
CatacombsPopulation
CrematoryPopulation
CryptPopulation
DeepJungleCavePopulation
DesertCanyonCavePopulation
FlowerfieldCavePopulation
FungalJunglePopulation
HillCavePopulation
JungleCavePopulation
LakeHinnomCavePopulation
MoonStairCavePopulation
MountainCavePopulation
PalladiumReefCavePopulation
RainbowWoodCavePopulation
SaltDuneCavePopulation
SubterraneanRuinsPopulation
WaterCavePopulation
WatervineCavePopulation
YdFreeholdOutskirtsPopulation

CREATURE-SPECIFIC TABLES
------------------------
These usually contain creatures or creature-region globals.
BananaGroveCreatures
BaroqueRuinsZoneGlobals-Creatures
CryptCreatures
DeepJungleZoneGlobals-Creatures
DesertCanyonCreatures
FlowerfieldCreatures
Golgotha Creatures
HillCreatures
JungleCreatures
MoonStairZoneGlobals-Creatures
MountainCreatures
Redrock1PerSector-Creatures
Redrock2PerSector-Creatures
Redrock3PerSector-Creatures
RedrockSnapjawCaveRoomCreatures
RuinsZoneGlobals-Creatures
SaltDuneCreatures
Tier1RuinsCreatures
Tier2CaveCreatures
Tier2RuinsCreatures
Tier3RuinsCreatures
Tier4RuinsCreatures
Tier5CaveCreatures
Tier5RuinsCreatures
Tier6RuinsCreatures
Tier7RuinsCreatures
Tier8RuinsCreatures
TombCreatures
WaterCreatures
WatervineCreatures

PARTY TABLES
------------
These usually create grouped faction encounters.
BaboonParty
BigBaboonParty
GoatfolkParty
GoatfolkQlippothParty
IssachariParty
SnapjawParty0
SnapjawParty1
SnapjawParty1-with-Warlord
SnapjawParty2
Templar War Party Major
Templar War Party Minor

BOSS / LAIR OWNER TABLES
------------------------
Useful for choosing site bosses or owner-style inhabitants. May be better for custom placement than BasicLair.Table.
GenericLairOwner
LairBosses1
LairBosses2
LairBosses3
LairBosses4
LairBosses5
LairBosses6
LairBosses7
LairBosses8
LairOwners_BananaGrove
LairOwners_BaroqueRuins
LairOwners_DeepJungle
LairOwners_DesertCanyon
LairOwners_Flowerfields
LairOwners_Fungal
LairOwners_Hills
LairOwners_Jungle
LairOwners_LakeHinnom
LairOwners_MoonStair
LairOwners_Mountains
LairOwners_PalladiumReef
LairOwners_Ruins
LairOwners_Saltdunes
LairOwners_Saltmarsh
LairOwners_TheSpindle
LairOwners_Water
LairOwners_Watervine
Tier1BossEncounter
Tier2BossEncounter
Tier3BossEncounter
Tier4BossEncounter
Tier5BossEncounter
Tier6BossEncounter
Tier7BossEncounter
Tier8BossEncounter

MIXED OR SPECIAL ENCOUNTER TABLES
---------------------------------
These may include terrain, loot, liquids, or structured encounters in addition to creatures. Use cautiously as BasicLair.Table.
CaveOddEncounters
CommonOddEncounters
FlowerFieldOddEncounters
GritGateDungeonEncounters
NaphtaaliEncounter
PilgrimEncounter
RedrockLevel2Encounters
RuinsOddEncounters
TierXCaveEncounters
TombCultistsEncounter
TombCultistsEncounterNoShrine

LAIRS_ ENCOUNTER TABLES (USUALLY NOT FOR BASICLAIR.TABLE)
---------------------------------------------------------
These are internal encounter pattern tables used by BasicLair/SultanDungeon. They contain role placeholders like =Minion and =Leader, not normal creature source pools.
Lairs_Encounters_*Default
Lairs_Encounters_armorer
Lairs_Encounters_chef
Lairs_Encounters_gemcutter
Lairs_Encounters_glover
Lairs_Encounters_gunsmith
Lairs_Encounters_gutsmonger
Lairs_Encounters_haberdasher
Lairs_Encounters_hatter
Lairs_Encounters_ichormerchant
Lairs_Encounters_modernscribe
Lairs_Encounters_shoemaker
Lairs_Encounters_workshop
Lairs_PreconnectEncounters_*Default

FACTION-RELATED TABLES (LIKELY NOT BASICLAIR.TABLE SOURCES)
-----------------------------------------------------------
These often produce faction names, inventory, or zone objects rather than creature blueprints.
FactionEncounterLeaderInventory_*Default
FactionEncounterLeaderInventory_Antelopes
FactionEncounterLeaderInventory_Apes
FactionEncounterLeaderInventory_Arachnids
FactionEncounterLeaderInventory_Barathrumites
FactionEncounterLeaderInventory_Bears
FactionEncounterLeaderInventory_Birds
FactionEncounterLeaderInventory_Mechanimists
FactionEncounterLeaderInventory_Seekers
FactionEncounterLeaderInventory_Templar
FactionEncounterLeaderInventory_Tortoises
FactionEncounterMemberInventory_*Default
FactionEncounterMemberInventory_Barathrumites
FactionEncounterPartyObjects_*Default
FactionEncounterPartyObjects_Arachnids
FactionEncounterPartyObjects_Barathrumites
FactionEncounterPartyObjects_Bears
FactionEncounterPartyObjects_Birds
FactionEncounterPartyObjects_Mechanimists
FactionEncounterPartyObjects_Seekers
FactionEncounterPartyObjects_Templar
FactionEncounterPartyObjects_Tortoises
FactionEncounterZoneObjects_*Default
FactionEncounterZoneObjects_Antelopes
FactionEncounterZoneObjects_Arachnids
FactionEncounterZoneObjects_Barathrumites
FactionEncounterZoneObjects_Mechanimists
FactionEncounterZoneObjects_Tortoises
GenericFactionPopulation
RandomFaction
RandomFaction_Period4
RandomFaction_Period5
RandomZoneTemplateFaction
SafeFactionPopulation

NOTES ON BASICLAIR.TABLE BEHAVIOR
-------------------------------
BasicLair.Table becomes SultanDungeon.monstertable.
SultanDungeon uses PopulationManager.GetEach(monstertable, ...) to enumerate possible blueprints.
Each returned blueprint is processed by ProcessBlueprintForCultRole().
If a blueprint has a Role tag/property, it can fill that role. If not, it defaults to Minion.
Role placeholders in Lairs_Encounters_*Default include =Minion, =Leader, =Hero, =Brute, =Tank, =Skirmisher, =Artillery, and =Lurker.

PRACTICAL INTERPRETATION
------------------------
Use Table for the creature source pool.
Use Adjectives for layout/theme/furnishing/encounter table selection.
Use direct PopulationManager.Generate(...) later for rewards, artifacts, and manually placed content.

FULL CANDIDATE LIST
-------------------
BaboonParty
BananaGroveCavePopulation
BananaGroveCreatures
BaroqueRuinsZoneGlobals-Creatures
BaroqueSubterraneanRuinsPopulation
BigBaboonParty
CatacombsPopulation
CaveOddEncounters
CommonOddEncounters
CrematoryPopulation
CryptCreatures
CryptPopulation
DeepJungleCavePopulation
DeepJungleZoneGlobals-Creatures
DesertCanyonCavePopulation
DesertCanyonCreatures
FlowerfieldCavePopulation
FlowerfieldCreatures
FlowerFieldOddEncounters
FungalJunglePopulation
GenericLairOwner
GoatfolkParty
GoatfolkQlippothParty
Golgotha Creatures
GritGateDungeonEncounters
HillCavePopulation
HillCreatures
IssachariParty
JungleCavePopulation
JungleCreatures
LairBosses1
LairBosses2
LairBosses3
LairBosses4
LairBosses5
LairBosses6
LairBosses7
LairBosses8
LairOwners_BananaGrove
LairOwners_BaroqueRuins
LairOwners_DeepJungle
LairOwners_DesertCanyon
LairOwners_Flowerfields
LairOwners_Fungal
LairOwners_Hills
LairOwners_Jungle
LairOwners_LakeHinnom
LairOwners_MoonStair
LairOwners_Mountains
LairOwners_PalladiumReef
LairOwners_Ruins
LairOwners_Saltdunes
LairOwners_Saltmarsh
LairOwners_TheSpindle
LairOwners_Water
LairOwners_Watervine
LakeHinnomCavePopulation
MoonStairCavePopulation
MoonStairZoneGlobals-Creatures
MountainCavePopulation
MountainCreatures
NaphtaaliEncounter
PalladiumReefCavePopulation
PilgrimEncounter
RainbowWoodCavePopulation
Redrock1PerSector-Creatures
Redrock2PerSector-Creatures
Redrock3PerSector-Creatures
RedrockLevel2Encounters
RedrockSnapjawCaveRoomCreatures
RuinsOddEncounters
RuinsZoneGlobals-Creatures
SaltDuneCavePopulation
SaltDuneCreatures
SnapjawParty0
SnapjawParty1
SnapjawParty1-with-Warlord
SnapjawParty2
SubterraneanRuinsPopulation
Templar War Party Major
Templar War Party Minor
Tier1BossEncounter
Tier1CavePopulation
Tier1RuinsCreatures
Tier1RuinsPopulation
Tier2BossEncounter
Tier2CaveCreatures
Tier2CavePopulation
Tier2RuinsCreatures
Tier2RuinsPopulation
Tier3BossEncounter
Tier3CavePopulation
Tier3RuinsCreatures
Tier3RuinsPopulation
Tier4BossEncounter
Tier4CavePopulation
Tier4RuinsCreatures
Tier4RuinsPopulation
Tier5BossEncounter
Tier5CaveCreatures
Tier5CavePopulation
Tier5RuinsCreatures
Tier5RuinsPopulation
Tier6BossEncounter
Tier6CavePopulation
Tier6RuinsCreatures
Tier6RuinsPopulation
Tier7BossEncounter
Tier7CavePopulation
Tier7RuinsCreatures
Tier7RuinsPopulation
Tier8BossEncounter
Tier8CavePopulation
Tier8RuinsCreatures
Tier8RuinsPopulation
TierXCaveEncounters
TombCreatures
TombCultistsEncounter
TombCultistsEncounterNoShrine
WaterCavePopulation
WaterCreatures
WatervineCavePopulation
WatervineCreatures
YdFreeholdOutskirtsPopulation
