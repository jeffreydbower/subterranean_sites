BASICLAIR.TABLE / MONSTERTABLE DICTIONARY
Updated after live BasicLair.Table testing

Purpose:
This file documents which population table strings appear useful as BasicLair.Table inputs.

BasicLair.Table is passed to SultanDungeon as monstertable. It is used as a creature/inhabitant source for Sultan-style role resolution, not as a layout table, loot table, or adjective/theme selector.

How to enter a table argument:
lair.Table = "LairOwners_Ruins";
lair.Table = "DynamicInheritsTable:Creature:Tier3";
lair.Table = "GoatfolkQlippothParty";

Do not add:
Data:
XML syntax
Lairs_ prefix unless the exact table name already includes Lairs_


CORE MODEL

BasicLair has three useful levers:

1. Table
Creature / inhabitant source pool.
This file is about this lever.

2. Adjectives
Theme/layout/furnishing/encounter selector.
These feed SultanDungeonArgs and look for Lairs_* tables.

3. Stairs
Stair placement.
""  = no stairs
"U" = stairs up
"D" = stairs down
"UD" = both


CURRENT BEST UNDERSTANDING OF BASICLAIR.TABLE

BasicLair.Table is best understood as:
"what pool of creatures or inhabitants can the lair use?"

It can be used in at least three working ways:

1. Dynamic tier creature pools
Example:
lair.Table = "DynamicInheritsTable:Creature:Tier3";

2. Lair owner / biome identity pools
Example:
lair.Table = "LairOwners_Ruins";

3. Party / faction encounter pools
Example:
lair.Table = "GoatfolkQlippothParty";

These behave differently and should not be treated as interchangeable.


TESTED RESULTS

Table: ""
Result: WORKS
Meaning: default BasicLair behavior.
Use: safest baseline.

Table: DynamicInheritsTable:Creature:Tier3
Result: WORKS
Observed:
- spawns creatures from the correct tier
- behavior matched expectation
- population density seemed random
Decision: VALID / TIER-SAFE
Use: strong candidate for depth-scaled vertical sites.

Table: DynamicInheritsTable:Creature:Tier1
Result: WORKS
Observed:
- similar behavior to Tier3, with tier-appropriate creatures
Decision: VALID / TIER-SAFE

Table: DynamicInheritsTable:Creature:Tier5
Result: WORKS
Observed:
- similar behavior to Tier3, with tier-appropriate creatures
Decision: VALID / TIER-SAFE

Table: GoatfolkQlippothParty
Result: WORKS
Observed:
- behaved as intended
- produced goatfolk qlippoth party-style inhabitants
Decision: VALID / FACTION-SPECIFIC
Use: themed sites or special encounter sites.

Table: GenericLairOwner
Result: WORKS BUT CHAOTIC
Observed:
- repeatedly spawned merchants with their stations
- sometimes spawned many merchants, around 10 in one test
- may include other mobs
Decision: VALID BUT NOT RECOMMENDED FOR NORMAL HOSTILE SITES
Use: possible merchant/outpost/anomalous site, not standard dungeon population.

Table: LairOwners_Ruins
Result: WORKS
Observed:
- generated valid lair
- changed inhabitants
- repeated test produced same monster set with different lair layout
- produced mobs associated with ruins/jungle-style environment, above tier of test zone
- generated multiple exits/paths to adjacent zones, but not necessarily toward expected neighboring zone
Decision: VALID BUT NOT TIER-SAFE
Use: biome/flavor sites where out-of-depth risk is acceptable or controlled elsewhere.

Table: LairOwners_PalladiumReef
Result: WORKS
Observed:
- generated palladium reef inhabitants
- clams observed
Decision: VALID BUT BIOME-SPECIFIC
Use: special themed sites, probably not default underground generation.

Table: Tier3CavePopulation
Result: CRASH
Decision: INVALID for BasicLair.Table
Reason: broad full-zone population table, not clean monstertable / role source.

Table: Tier3RuinsCreatures
Result: CRASH
Decision: INVALID for BasicLair.Table
Reason: despite creature-oriented name, not valid in this BasicLair.Table context.

Table: Tier8BossEncounter
Result: CRASH
Decision: INVALID for BasicLair.Table
Reason: encounter table, not clean BasicLair.Table source.

Table: Lairs_Encounters_ichormerchant
Result: NOT TESTED / NOT RECOMMENDED
Expectation:
- likely internal encounter-pattern table or specialized merchant encounter table
- may spawn merchant-style content if used elsewhere
Decision: DO NOT USE as BasicLair.Table unless intentionally testing merchant/special cases.
Note:
Lairs_Encounters_* tables are normally selected indirectly by Adjectives/SultanDungeonArgs, not passed as Table.


RECOMMENDED INPUTS FOR THE MOD

PRIMARY RECOMMENDATION: DYNAMIC TIER CREATURE TABLES

These are now the strongest BasicLair.Table candidates for vertical subterranean sites because they are tier-controlled and not tied to surface biome identity.

DynamicInheritsTable:Creature:Tier1
DynamicInheritsTable:Creature:Tier2
DynamicInheritsTable:Creature:Tier3
DynamicInheritsTable:Creature:Tier4
DynamicInheritsTable:Creature:Tier5
DynamicInheritsTable:Creature:Tier6
DynamicInheritsTable:Creature:Tier7
DynamicInheritsTable:Creature:Tier8

Expected use:
Choose tier based on zone depth or site tier.

Example:
lair.Table = "DynamicInheritsTable:Creature:Tier3";

Known behavior:
- works in BasicLair.Table
- produces tier-appropriate creatures
- density may be inconsistent/random

Open issue:
Need to watch for population density. Sites should feel more populated than normal zones. If dynamic tables produce too few mobs, add supplemental direct placement later.


VARIABLE-FORM DYNAMIC TABLE

Potential:
DynamicInheritsTable:Creature:Tier{zonetier}

Status:
Not confirmed in this latest test set.

Concern:
Variable substitution may depend on exact variable casing. PopulationManager supports tier expressions, but BasicLair/SultanDungeon may not pass variables in the expected case.

Recommendation:
Use explicit numeric tier strings first:
DynamicInheritsTable:Creature:Tier3

Then later, if desired, test:
DynamicInheritsTable:Creature:Tier{zonetier}


SECONDARY RECOMMENDATION: FACTION / PARTY TABLES

Useful when a site should have a clear faction identity.

Known valid:
GoatfolkQlippothParty

Likely worth limited testing:
BaboonParty
BigBaboonParty
GoatfolkParty
IssachariParty
SnapjawParty0
SnapjawParty1
SnapjawParty1-with-Warlord
SnapjawParty2
Templar War Party Major
Templar War Party Minor

Use:
Specific themed vertical sites, special faction sites, or fixed archetypes.

Caution:
Party tables may produce narrow faction identity and may not scale cleanly with depth unless selected manually by site tier.


TERTIARY RECOMMENDATION: LAIROWNERS_* TABLES

Useful for biome/ecosystem identity, but not automatically tier-safe.

Known valid:
LairOwners_Ruins
LairOwners_PalladiumReef

Likely valid:
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
LairOwners_Saltdunes
LairOwners_Saltmarsh
LairOwners_TheSpindle
LairOwners_Water
LairOwners_Watervine

Use:
Special themed sites, not generic depth-scaled sites.

Caution:
May pull biome-specific mobs that are too strong or strange for the local zone tier.


SPECIAL / CHAOTIC TABLES

GenericLairOwner
Result: works but chaotic.
Observed repeated merchant/station spawning.
Use only if designing merchant/outpost/settlement-style sites or intentionally weird mixed sites.

Lairs_Encounters_ichormerchant
Not recommended as BasicLair.Table.
Likely an internal encounter-pattern table or specialized encounter definition. If it spawns a legendary merchant, that is probably because it is meant to be selected by the lair encounter system, not used as the raw monstertable.


LIKELY INVALID FOR BASICLAIR.TABLE

Tiered broad zone population tables:
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

Reason:
Tier3CavePopulation crashed. These are broad zone population packages and may include nested tables, zone content, lairs, odd encounters, and non-role-source entries.

Correct later use:
Direct zone/region population placement, not BasicLair.Table.


Likely invalid tiered creature / ruins tables:
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

Reason:
Tier3RuinsCreatures crashed. Despite creature-like names, these are not safe BasicLair.Table inputs based on current evidence.

Correct later use:
Possibly direct placement or different builder context, not BasicLair.Table.


Likely invalid boss encounter tables:
Tier1BossEncounter
Tier2BossEncounter
Tier3BossEncounter
Tier4BossEncounter
Tier5BossEncounter
Tier6BossEncounter
Tier7BossEncounter
Tier8BossEncounter

Reason:
Tier8BossEncounter crashed. These appear to be encounter tables, not clean monstertable role-source tables.

Correct later use:
Custom boss placement or explicit encounter construction, not BasicLair.Table.


Likely invalid Lairs_ encounter-pattern tables:
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

Reason:
These are internal encounter-pattern tables selected by the lair system. They may contain role placeholders like =Minion and =Leader. They are not the raw creature source pool.

Correct use:
Selected indirectly through Adjectives/SultanDungeonArgs.


NOT BASICLAIR.TABLE PRIORITIES, BUT USEFUL LATER

Creature/biome zone-global tables:
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
TombCreatures
WaterCreatures
WatervineCreatures

Reason:
These may contain useful creature pools, but current evidence favors DynamicInheritsTable or LairOwners_* for BasicLair.Table. Save these for direct placement/custom builders unless later testing proves otherwise.


Broad biome/zone population tables for later direct placement:
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

Use later for:
- direct region population
- biome overlays
- custom builders
- special site variants

Do not currently recommend as BasicLair.Table.


CURRENT BEST BASICLAIR.TABLE STRATEGY FOR SUBTERRANEAN SITES

Default depth-scaled vertical site:
Use DynamicInheritsTable:Creature:TierN

Example:
lair.Table = "DynamicInheritsTable:Creature:Tier3";
lair.Adjectives = "";
lair.Stairs = "D";

Biome/ecosystem special site:
Use LairOwners_* carefully.

Example:
lair.Table = "LairOwners_Ruins";

Faction special site:
Use Party tables selectively.

Example:
lair.Table = "GoatfolkQlippothParty";

Merchant/outpost/weird site:
Use GenericLairOwner only intentionally.

Example:
lair.Table = "GenericLairOwner";


OPEN QUESTIONS

1. Density:
DynamicInheritsTable works, but density may be too random/sparse. Need to observe more.

2. Role quality:
Do dynamic creature tables fill roles like Leader/Hero/Brute well, or mostly Minion?

3. Vertical site scaling:
Need a mapping from Z-depth/site tier to explicit dynamic tier.

4. Supplementary population:
If BasicLair produces low density, add extra creatures manually after BuildZone.

5. Adjective interaction:
Later tests should check whether Adjectives can increase encounter density or change room usage while Table controls creature pool.


RECOMMENDED NEXT TESTS

Test A:
lair.Table = "DynamicInheritsTable:Creature:Tier3";
lair.Adjectives = "soldier";
lair.Stairs = "D";

Purpose:
See if adjective-controlled encounter patterns increase structure/density while dynamic table keeps tier-safe creature pool.

Test B:
lair.Table = "DynamicInheritsTable:Creature:Tier5";
lair.Adjectives = "temple";
lair.Stairs = "D";

Purpose:
Mid/deep vertical site prototype.

Test C:
lair.Table = "GoatfolkQlippothParty";
lair.Adjectives = "warrior";
lair.Stairs = "D";

Purpose:
Faction-specific themed site.

Test D:
lair.Table = "GenericLairOwner";
lair.Adjectives = "market";
lair.Stairs = "";

Purpose:
Merchant/outpost archetype only if desired.


SUMMARY

Most useful BasicLair.Table options now appear to be:

1. DynamicInheritsTable:Creature:TierN
Best for tier-scaled generic hostile sites.

2. LairOwners_*
Best for biome/ecosystem identity, but not tier-safe.

3. Party tables
Best for deliberate faction sites.

4. GenericLairOwner
Works, but chaotic and merchant-heavy.

Avoid using Tier#CavePopulation, Tier#RuinsPopulation, Tier#BossEncounter, Tier#RuinsCreatures, and Lairs_Encounters_* as BasicLair.Table inputs.
