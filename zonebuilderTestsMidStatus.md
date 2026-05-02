SUBTERRANEAN SITES MOD – HANDOFF STATE

PROJECT
Caves of Qud mod: Subterranean Sites.

Goal:
Generate deterministic underground sites with path-based discovery. The player finds an unusual path underground/surface that leads toward a vertical subterranean site. Current direction is NOT flat 3x3 parasang sites; it is one path leading to a vertical site stack.

USER PREFERENCES
- Keep responses practical and grounded.
- Do not over-explain obvious points.
- Prefer small testable steps.
- Avoid guessing; verify against code when possible.
- For notes the user asks to copy, use plain text in a code block with no markdown formatting inside except plain ASCII text.
- Be concise unless the builder is high-value.
- User wants evaluative/critical guidance, not automatic agreement.

CURRENT MOD STATUS
Runtime injection works.

Known working pattern:
- JoppaWorldBuilderExtension registers runtime system with:
  The.Game.RequireSystem<RuntimeZoneBuilderInjectionSystem>();
- Runtime system registers BeforeZoneBuiltEvent.
- BeforeZoneBuiltEvent is early enough to mutate/replace a zone.
- ZoneActivatedEvent was too late.
- AddZoneBuilder does not affect zones during runtime.
- ZoneManager.ApplyBuilderToZone works for runtime builder application.
- Direct builder calls also work.
- SetZoneName works.

Current test harness targets fixed zone:
JoppaWorld.11.22.0.1.11

Basic pattern:
var Z = zoneBuildEvent.Zone;
foreach (var cell in Z.GetCells()) cell.Clear();
new SomeBuilder().BuildZone(Z);

IMPORTANT DESIGN DECISIONS

1. Deterministic matrix model
- Underground is divided into fixed-size 3D matrices/buckets.
- Each matrix can contain at most one site.
- Site is fully contained within its matrix.
- Matrix ID + world seed gives deterministic RNG seed.
- Same matrix always recomputes same site definition.
- Storage/cache optional later.

2. Site definition includes:
- site existence
- site local position
- vertical stack length
- adjectives/theme
- creature table
- path definition

3. Zone membership
When a zone is entered:
- compute matrix ID
- recompute site definition
- check:
  A) zone is site layer
  B) zone is path zone
  C) otherwise normal zone

4. Current site structure
- Sites are vertical stacks, not flat 3x3 parasang sites.
- Site anchor is top/entrance layer.
- Site extends downward N levels.
- Any layer can be entered first and still resolves correctly.

5. Current path model
- One path per site.
- Path is an ordered list of x,y,z zone coordinates.
- Path extends outward/upward from site, never deeper than the site entrance/top layer.
- Path can include horizontal moves and vertical hole transitions.
- Path material types may include resin, dirt, stone, river/water.
- Path and holes should be visually unusual and attention-grabbing.
- Holes are preferred over stairs for path signaling.
- Stairs may exist inside sites, but path uses holes/pits.

6. Architecture model
Global layer:
- PathDefinition chooses which x,y,z zones are on the path and in what order.

Per-zone layer:
- PathSegment describes what happens in one zone:
  - entry direction
  - exit direction
  - material type
  - whether there is a hole/pit transition

Rendering layer:
- ApplyPathMouths opens edge mouths for horizontal entry/exit.
- RenderPathMaterial carves/paints path between endpoints.
- ApplyPathHoleIfNeeded places pit/hole for vertical transitions.
- Run connectivity fix after rendering.

Important distinction:
- Coordinate list decides WHERE the path exists.
- Shug/FungalTrail/Canyon-style rendering decides HOW it looks and connects locally.

HIGH-VALUE SOURCE SYSTEMS IDENTIFIED

IConnectionBuilder
Type: CONNECTION UTILITY / BASE CLASS
Importance: high
Summary:
- Base class behind Cave/Canyon/FungalTrail/ShugBurrow mouth builders.
- ConnectionMouth creates edge connection points.
- ConnectionStart creates internal start points.
- It caches current-zone connection and matching opposite connection in neighbor zone.
- Clears the mouth cell.
- This explains how paths continue across zone edges.
Relevance:
- Very relevant for custom PathMouthBuilder.

FungalTrailBuilder
Type: PATH / TRAIL BUILDER
Importance: very high
Summary:
- Reads FungalTrail zone connections.
- Uses mouths/start points as endpoints.
- Uses FindPath to connect endpoints inside the zone.
- Paints path with FungalTrailBrick.
- Paints adjacent cells too, making trail broad/visible.
- If only one connection exists, generates a local patch of trail material.
Relevance:
- Best simple reference for visible non-hole horizontal path rendering.
- Good model for dirt/stone/resin path material.
- Does not handle vertical holes.

ShugBurrowBuilder
Type: PATH / BURROW BUILDER
Importance: very high
Summary:
- Reads ShugBurrow zone connections.
- Separates Ascending and Descending connection points.
- Uses Pathfinder with custom noise-based weights for organic paths.
- Clears/skins circular area around path steps.
- Replaces surrounding walls with BaseNephilimWall_Shug'rith.
- For descending connections, clears around point and places LazyPit.
- Adds StairBlocker and InfluenceMapBlocker around descending pits.
- Executes ShugruithTunnel template.
Relevance:
- Closest match to desired path + hole system.
- Strong model for PathMaterialBuilder + PathHoleBuilder.
- Do not call directly; too Shug-specific.
- Resin path type may borrow visual logic.

ShugBurrow mouths
Type: CONNECTION BUILDERS
Importance: high
Summary:
- North/South/East/West mouths call ConnectionMouth with Range=3.
- Ascending/Descending call ConnectionStart with special suffix.
Relevance:
- Good model for horizontal path mouths and vertical endpoint tagging.

Pitted
Type: VERTICAL FEATURE / HOLE SYSTEM
Importance: very high
Summary:
- Builds multiple circular pits/wells in a zone column.
- Uses GetOracleIntColumn to align pit x/y/radius across Z levels.
- Top layers get Pit objects.
- Deeper layers can become liquid pools.
- Adds FlyingWhitelistArea, StairBlocker, InfluenceMapBlocker.
- Pit object has StairsDown.ConnectLanding=false.
- Supports CenterConnectionType.
Relevance:
- Qud’s canonical multi-level pit/hole system.
- Borrow pattern, not full implementation.
- For our first PathHoleBuilder, use simpler version:
  single hole, fixed radius, deterministic point, clear around it, place pit/lazy pit, add blockers, ensure reachable.

ForceConnections
Type: CONNECTIVITY / FIX-UP
Importance: high
Summary:
- Collects stairs, zone connections, cached zone connections, and optional additional points.
- Ensures all connection points are reachable.
- Uses pathfinding with noise weights to clear tunnels if needed.
- CaveLike option widens paths.
Relevance:
- Very useful after path/hole/site generation.
- Better for endpoint-specific fixing than ForceConnectionsPlus.

ForceConnectionsPlus
Type: CONNECTIVITY / FIX-UP
Importance: medium
Summary:
- Finds all passable cells and connects isolated passable regions.
- More aggressive than ForceConnections.
- May over-carve and erase intended separation.
Relevance:
- Reference only; use cautiously.

CanyonBuilder
Type: PATH / CONNECTIVITY BUILDER
Importance: medium-high
Summary:
- Reads Canyon/Stairs zone connections.
- Finds/chooses start point.
- Uses FindPath to connect start to connection targets.
- Clears and widens path.
- Adds CanyonMarker.
Relevance:
- Good simple path-carving reference.

Cave
Type: FULL / BASE TERRAIN BUILDER
Importance: high reference
Summary:
- Builds normal cave terrain using cellular + Perlin noise.
- Uses static Maze3D to determine N/S/E/W/U/D connectivity.
- Places cave/tunnel edge connections.
- Uses FindPath to connect nodes inside zone.
- Places stairs based on 3D maze.
Relevance:
- Strong reference for x,y,z path logic and cross-zone continuity.
- Do not use directly for site/path.

SultanDungeon
Type: FULL DUNGEON GENERATOR
Importance: very high
Summary:
- Main flexible generator for historic-style dungeon sites.
- Uses SultanDungeonArgs.
- Supports random historic-entity-driven generation via BuildRandomZone.
- Supports explicit argument-driven generation via BuildRandomZoneWithArgs.
- Uses segmentation, WFC templates, walls, floors, liquids, objects, furnishings, halls, cubbies, encounters, preconnect encounters.
- Connects regions internally.
- Places stairs when requested.
- Runs ForceConnections.
- Can optionally place relics.
Relevance:
- Best candidate for major site levels.
- Use as SITE generator, not path generator.
- BasicLair is a thin wrapper around SultanDungeon using Lairs_ prefix.
- SultanDungeons_ prefix should be much broader and worth testing.

SultanDungeonArgs
Type: ARGUMENT / THEME RESOLUTION SYSTEM
Importance: very high
Summary:
- Translates adjectives/properties into theme clusters.
- Checks for matching tables by prefix:
  Wall_
  PrimaryTemplate_
  SecondaryTemplate_
  DetailTemplate_
  Green_
  Blue_
  Yellow_
  Segmentations_
  Furnishings_
  Halls_
  Cubbies_
  Encounters_
  PreconnectEncounters_
- Falls back to defaults if no matching table exists.
- Can mutate templates for variation.
Relevance:
- Theme-control layer for SultanDungeon.
- Table prefix matters greatly.
- Lairs_ was limited/vendor-ish.
- SultanDungeons_ likely has real broad adjective support.

BasicLair
Type: FULL LAYOUT CANDIDATE / SultanDungeon wrapper
Importance: medium
Summary:
- BuildZone calls:
  new SultanDungeon().BuildRandomZoneWithArgs(Z, 0, true, Adjectives.Split(','), Stairs, Table, "Lairs_");
- BasicLair works as a structure generator.
- Table maps to SultanDungeon.monstertable.
- Adjectives use Lairs_ table prefix.
Testing:
- DynamicInheritsTable:Creature:Tier3 and Tier5 worked mostly tier-appropriate but with some tier leakage.
- GenericLairOwner works but chaotic and merchant/station heavy.
- BasicLair.Adjectives high-level terms like soldier, temple, market, stars, tinker did not visibly change much under Lairs_.
- Workshop produced many chests.
Conclusion:
- BasicLair adjective support is limited and vendor/workshop oriented.
- BasicLair may still be useful as simple structural shell.
- Better to use SultanDungeon directly for real site theming.

BasicLair Lairs_ supported adjectives found:
workshop
armorer
chef
glover
gunsmith
haberdasher
hatter
ichormerchant
modernscribe
shoemaker
gemcutter
gutsmonger

FactionEncounters
Type: ENCOUNTER INJECTION
Importance: high
Summary:
- Randomly selects a faction from population table default GenericFactionPopulation.
- Spawns coherent faction encounter group: leader + squad.
- Equips members by zone tier.
- Filters members by zone level.
- May add faction-related objects.
- Additive; does not clear or override layout.
Testing:
- Works when layered after BasicLair.
- Spawned Mechanimists once, Girshling legendary + squad once.
- Early-zone encounters can be tough.
Decision:
- USE for ambient faction encounters/sites/paths.
- Keep chance low in final implementation.
- May want custom population table later.

PopTableZoneBuilder
Type: POPULATION / OBJECT PLACEMENT
Importance: medium
Summary:
- Requires explicit Table argument.
- Does not choose random fallback if no table.
- Generates objects from population table.
- Places aquatic objects in liquid, wall-living objects on walls, otherwise reachable empty cells.
- Can apply zone faction to spawned objects.
Relevance:
- Maybe useful for supplementing sparse site populations.
- More direct than FactionEncounters, but less flavorful.

PlaceRelicBuilder
Type: LOOT / RELIC PLACEMENT
Importance: medium
Summary:
- Places relic into RelicContainer if present, otherwise random container, combat creature inventory, or empty reachable cell.
- Marks relic holder important.
- Optionally adds cybernetics credit wedges by tier.
Relevance:
- Maybe for major site reward.
- SultanDungeon already integrates relic placement.

GenericChutes / GenericChuteTemplate
Type: GOLGOTHA VERTICAL SYSTEM
Importance: concept reference
Summary:
- Golgotha-specific chute/conveyor/shaft descent system.
- Uses ZoneProperty/ZoneColumnProperty to persist alignment across Z levels.
- Overbuilt for this mod.
Important concept:
- ZoneProperty = key/value stored on specific zone.
- ZoneColumnProperty = key/value shared across vertical column.
- Useful if cross-zone storage needed, but initial system should prefer deterministic recomputation.
Decision:
- Reference concept only.

CatacombsMapTemplate / CatacombsPublicus
Type: MULTI-ZONE HORIZONTAL STRUCTURE
Importance: concept reference
Summary:
- Generates large shared 2D layout and renders each zone as a slice.
- Good reference for old 3x3 flat-site concept.
- Less relevant to current one-path-to-vertical-site design.
Decision:
- Reference architecture only.
- Do not redesign around it unless flat multi-zone sites return.

FungalJungle
Type: FULL BIOME / GLOBAL STRUCTURE
Importance: low-medium reference
Summary:
- Generates maze structure across zones.
- Uses maze to define river mouths.
- Builds rivers with RiverBuilder.
- Fungalizes zone.
Relevance:
- Confirms connection-based multi-zone layout pattern.
- Probably not needed for current path/hole work.

ChavvahRoots
Type: VERTICAL SYSTEM / ROOT NETWORK
Importance: medium-high reference
Summary:
- Operates across Z 0-50.
- Places pits on most levels, skips every 5th level.
- Adds stairs at intervals.
- Generates branching root tunnels using TunnelTo.
Relevance:
- Useful example of intermittent holes and separation of path carving vs vertical transitions.

GirshLairMakerBase
Type: SPECIALIZED DUNGEON FRAMEWORK
Importance: reference
Summary:
- Framework around Girsh/Nephilim lairs.
- Sets metadata/name/XP/secrets.
- Delegates layout to BuildLair in subclasses.
- Mutates creatures and applies Girsh flavor.
- Wraps/specializes dungeon generation.
Decision:
- Reference only.
- Prefer SultanDungeon for flexible site generation.

OTHER BUILDERS / DECISIONS
ClearAll
- Utility. Clears every cell. Maybe useful instead of manual foreach clear.

GenericSolid
- Utility. Fills entire zone with one material. Maybe useful for controlled test zones.

InteriorGround
- Base terrain fill with InteriorVoid + Rocky.Paint. Skip for now.

InsertPresetFromPopulation
- Places a random map chunk/preset from population table, then ForceConnections.
- Maybe later for fixed set pieces.

MapBuilder
- Places map file by ID into zone.
- Maybe later for handcrafted entrances/boss rooms.

LiquidPools
- Decorator. Noise-based liquid pools. Maybe for site variants.

Mines
- Noise carver/decorator, connection-aware. Skip.

CaveCity
- Converts InfluenceMap regions into small cave buildings. Skip.

FortMaker
- Rectangular fort structures. Skip.

FractiPlanter
- Plant decorator. Skip.

IsCheckpoint
- Gameplay checkpoint widget. Skip.

ISultanDungeonSegment
- Not storage. Generation-time shape/region definition for SultanDungeon.

InfluenceMapSeedStrategy
- Enum controlling influence-map seed placement:
  FurthestPoint, LargestRegion, RandomPointFurtherThan4, RandomPointFurtherThan1, RandomPoint.
- Reference only.

Builders skipped as out of scope:
BasicRoomHall
BathHalls
BathPools
BethesdaColdZone
BeyLahOutskirts
BuildingTemplate / BuildingTemplateTile / BuildingTile
CarbonFarm
ChildrenOfTheTomb / QuestHandler
ClearWallAddObject
CollapseAtLevel
ConveyorBelt
Craters
Cryobarrio / Cryobarrio1 / Cryobarrio2
CryptOfLandlords
CryptOfPriests / CryptOfWarriors
DenseBrinestalk
EmptyGround
FindA...DynamicQuest builders
FlagInside
FlowerFields
GoatfolkQlippothYurts / GoatfolkYurts
GolgathaChutes / GolgathaTemplate
HamilcrabShop
Hills
HindrenClues
Hive
Hydrolics
IdolFight
InteractWithAnObject... quest builders
JoppaOutskirts / JoppaOutskirtsRuins
JungleRuins
LakeOfTheDamned
MinorRazedGoatfolkVillage
MoonStair
Music
Odditorium
Omonporch / OmonporchGroveBuilder
OverlandAlgeaLake / OverlandRuins / OverlandWater
Pharmacorium
PigFarm / PigFarmMaker
PlaceAClam
PlaceWedgeBuilder1-6
Plains

CURRENT IMPLEMENTATION IDEA

PathDefinition:
- deterministic ordered list of x,y,z zones.
- generated from site seed.
- Example:
  Zone A next=East
  Zone B prev=West next=Down
  Zone C prev=Up next=East
  Zone D prev=West next=SiteEntrance

PathSegment:
- current zone’s local rendering requirements.
- Fields initially:
  ZoneID
  EntryDir
  ExitDir
  Material
  HasHole
  HoleType
  EntryPoint
  ExitPoint
  HolePoint

GetPathForSite(siteSeed):
- custom function we create.
- Computes full path coordinate list in sequential deterministic manner.

path.GetSegmentForZone(Z.ZoneID):
- checks if current zone is on path.
- If not, return null.
- If yes, return PathSegment with entry/exit/hole info.

ApplyPathMouths(Z, segment):
- open edges of zone where path enters/exits horizontally.
- Similar to FungalTrailEastMouth/WestMouth or custom IConnectionBuilder pattern.
- If exit is Down, no edge mouth; instead set hole target.

RenderPathMaterial(Z, segment):
- carve/paint path between local endpoints.
- Similar to FungalTrailBuilder or ShugBurrowBuilder.
- Use FindPath or TunnelTo.
- Clear terrain/walls as needed.
- Add material object or paint:
  resin/dirt/stone/etc.
- Widen path for visibility.

ApplyPathHoleIfNeeded(Z, segment):
- if segment changes Z, place pit/hole.
- Similar to Pitted/Shug descending logic.
- Place LazyPit or Pit-like object.
- Clear local region.
- Add blockers if needed.
- Ensure connectivity.

Connectivity:
- run ForceConnections or custom minimal fix after rendering.
- Pattern:
  generate feature -> check reachability -> fix if needed.

NEAR-TERM TEST PLAN

Recommended next tests:
1. Continue remaining ZoneBuilder survey, especially River/RiverBuilder systems.
2. Basic horizontal path test:
   - 2 or 3 adjacent zones.
   - fixed West -> East path.
   - no randomness, no holes.
   - use FungalTrail-style calls or custom minimal renderer.
3. Add edge mouth continuity:
   - confirm zones connect visually and physically across screen edge.
4. Add one vertical transition:
   - one horizontal zone leading to a hole.
   - next lower zone receives continuation.
5. Then test vertical site stack.
6. Then test SultanDungeon as site generator.

Potential simple fake 3-zone horizontal test using existing builders:
ZoneA:
new FungalTrailStartMouth().BuildZone(Z);
new FungalTrailEastMouth().BuildZone(Z);
new FungalTrailBuilder(){ Puddle="FungalTrailBrick" }.BuildZone(Z);

ZoneB:
new FungalTrailWestMouth().BuildZone(Z);
new FungalTrailEastMouth().BuildZone(Z);
new FungalTrailBuilder(){ Puddle="FungalTrailBrick" }.BuildZone(Z);

ZoneC:
new FungalTrailWestMouth().BuildZone(Z);
new FungalTrailBuilder(){ Puddle="FungalTrailBrick" }.BuildZone(Z);

Need adjacent ZoneIDs.

IMPORTANT CAVEATS
- Do not invent new systems before fully understanding existing builders.
- But do not overfit to Qud’s exact builders; use them as patterns.
- Existing builders often separate:
  global/connection definition
  local carving/rendering
  connectivity fix
- This should guide the mod architecture.

WHAT TO UPLOAD FIRST IN NEW THREAD
1. Current mod source:
   - SubterraneanSites.cs or current runtime injection file.
2. Docs:
   - spike-log.md
   - decisions.md
   - test-plan.md
   - adjective dictionary if relevant.
3. Key Qud source files:
   - IConnectionBuilder
   - FungalTrailBuilder + FungalTrail mouths
   - ShugBurrowBuilder + Shug mouths
   - Pitted
   - ForceConnections
   - SultanDungeon
   - SultanDungeonArgs
   - FactionEncounters
   - PopTableZoneBuilder
4. If continuing builder survey:
   - next builders alphabetically, especially River/RiverBuilder when reached.