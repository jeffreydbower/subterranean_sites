## ZoneBuilder Test Catelog

AddBlueprintBuilder
Type: PARTIAL
Behavior: places 1 object randomly in empty cell
Use: content injection only
Decision: SKIP (not layout)

AddLocationFinder
Type: SYSTEM
Behavior: registers a hidden location/secret marker
Use: internal tracking
Decision: SKIP

AddObjectBuilder
Type: CONTEXT
Behavior: places cached object instance
Dependency: requires object to be pre-cached
Decision: SKIP

AddPresetAtLocation
Type: STRUCTURE
Behavior: places predefined map chunk at fixed location
Use: anchor rooms / special structures
Decision: MAYBE (later)

AddWidgetBuilder
Type: CONTEXT
Behavior: ensures specific object/blueprint exists at (0,0)
Use: system/controller objects, not visible content
Dependency: often relies on cached objects or specific pipelines
Decision: SKIP

AgolgotColumn
Type: FULL (specialized, multi-layer)
Behavior: Girsh lair generator with vertical structure
Key patterns:
- depth-based logic
- terrain carving (ClearWalls + RequireObject)
- region population
- connectivity enforcement
- path carving
Decision: SKIP (but extract patterns)

ApegodCave
Type: PARTIAL  
Behavior: noise-based vegetation + ground paint; adds daylight  
Decision: SKIP

BananaGrove
Type: PARTIAL (rich)
Behavior: noise-based vegetation; supports underground; no wall/structure generation
Test 1 (overlay):
- Applied without clearing
Result:
- Grove layered over cave
- Terrain mismatch; pathing not guaranteed
Test 2 (clean):
- Cleared zone, then applied builder
Result:
- Full-screen grove
- No walls / enclosure
- Default zone creatures (no population control)
Decision:
USE (situational)

BananaSky
Type: CONTEXT (surface/exterior)
Behavior: places tomb exterior wall prefabs + air + daylight; depends on spindle adjacency
Decision: SKIP

BasicLair
Type: FULL
Behavior: SultanDungeon wrapper; generates lair-style layout with zone-appropriate materials
Tests:
- cleared zone first
- Table="", Adjectives="", Stairs=""
Result:
- generated valid lair structure
- used appropriate underground materials
- default zone monsters remained
- no west-side access from entry
- layout occupied only part of zone, likely random/segmentation-dependent
Notes:
- Tables argument is creature tables
- Adjectives argument flavors the zone based on 2 descriptors (clever/insane original designers here)
- detailed info on tables and adjetice arguments are in disctionary_creatureTables.md and dictioary_adjetives.md 
Decision:
TEST PASS (primary full-layout candidate)

BasicRoomHall
Type: STRUCTURE
Behavior: simple room-and-hall layout generation
Observation:
- produces basic rooms connected by hallways
- no strong identity or distinguishing features
Decision:
SKIP (too generic; BasicLair provides stronger structure)

BathHalls
Type: STRUCTURE / DECORATOR
Behavior: generates bathhouse-style halls
Observation:
- narrow, theme-specific content
- not aligned with general subterranean site goals
Decision:
SKIP (out of scope)

BathPools
Type: STRUCTURE / DECORATOR
Behavior: adds bathhouse-style pools and water features
Observation:
- highly thematic
- better suited for handcrafted or specific content
Decision:
SKIP (out of scope)

BethesdaColdZone
Type: FULL (specialized)
Behavior: generates Bethesda Susa-style cold dungeon zones
Observation:
- tied to existing in-game dungeon
- includes global temperature manipulation
- not modular for reuse
Notes:
- temperature logic may be useful later in custom builders
- not appropriate for current procedural generation phase
Decision:
SKIP (for now)

BethsaidaColumn
Type: FULL (specialized, multi-layer Girsh lair)
Behavior:
- generates Bethsaida cradle / Girsh lair content
- uses Pitted to create wells/pits across layers
- includes cyst structures, special wall placement, liquid placement, and Bethsaida-specific population
- uses cleanup/connectivity helpers after pit generation
Useful patterns:
- Pitted can generate controlled wells/pits
- PitTop and PitDepth appear important for vertical hole behavior
- ZoneBuilderSandbox.EnsureAllVoidsConnected may help keep carved regions traversable
- Z.FireEvent("FirmPitEdges") may help clean up pit boundaries
- BridgeOver may be useful when pits interfere with traversal
Reason:
- too tightly tied to Bethsaida / Girsh lair content
- not appropriate as a reusable subterranean site zone
- vertical behavior is mostly straight-column lair logic, not path signaling
Design note:
- useful as reference code for making path holes
- likely need a custom PathHole builder for the mod
- PathHole should be simpler than Girsh/Cradle builders
- intended use is to place occasional downward/upward holes along generated paths
- resin is a candidate path material, possibly inspired by Shug'ruith-style trail generation
Current implication:
- path system may need two layers:
  1. path surface material builder (resin, dirt, stone, water)
  2. path-hole builder for controlled vertical transitions
  Decision:
  SKIP as direct builder

BuildingTemplate
Type: STRUCTURE / TEMPLATE
Behavior: applies a 2D layout template to a zone, including walls, doors, and optional stair tiles
Observation:
- operates per-zone (2D), not true multi-zone structure
- supports stair placement via template tiles (StairsUp / StairsDown)
- does not manage vertical stacking or cross-zone linking
Notes:
- stair placement is handled implicitly by site structure, no need to control explicitly
- not aligned with path/holes system
Decision:
SKIP (not needed)
CanyonBuilder
Type: PATH / CONNECTIVITY BUILDER
Behavior:
- reads Canyon and Stairs zone connections
- identifies a Canyon Start point when available
- chooses a random start point if none exists
- clears a small local start area
- uses FindPath to connect the start point to Canyon/Stairs connection points
- clears cells along the route
- widens the route by clearing adjacent cells
- places CanyonMarker objects along and near the route
Observation:
- not useful as a direct subterranean site builder
- very relevant as a simple path-carving reference
- shows how to use existing zone connections as path endpoints
- shows how to carve and mark a path inside one zone
- local only; does not define a multi-zone path by itself
Decision:
SKIP as direct builder
REFERENCE for path carving

CanyonNorthMouth / CanyonSouthMouth / CanyonWestMouth / CanyonStartMouth
Type: CONNECTION BUILDER
Behavior:
- creates Canyon connection mouths at zone edges or start points
- uses IConnectionBuilder helpers:
  - ConnectionMouth(...)
  - ConnectionStart(...)
Observation:
- useful for understanding how Qud defines path endpoints
- may be relevant if using ZoneConnection records for custom path segments
- not directly useful for current builder testing
Decision:
REFERENCE only

CanyonReacher
Type: REACHABILITY BUILDER
Behavior:
- clears the zone reachable map
- rebuilds reachability from Canyon connection points
Observation:
- useful support logic for path systems
- may help ensure carved paths are recognized as connected/traversable
- not a content builder
Decision:
REFERENCE only

Design Note:
The Canyon builder family is one of the best references so far for the planned path system.
Useful pattern:
1. define path endpoints through connections
2. carve a route between endpoints with FindPath
3. widen the route
4. place visible marker/path material
5. rebuild or update reachability

For Subterranean Sites:
- replace CanyonMarker with resin/dirt/stone path material
- replace canyon endpoints with deterministic path entry/exit/hole targets
- likely use a simpler custom PathMaterial builder rather than these builders directly

## ZoneBuilder Evaluation – CatacombsMapTemplate

CatacombsMapTemplate
Type: GLOBAL LAYOUT GENERATOR
Behavior:
- generates large-scale 2D layout using WFC ("crypt")
- partitions layout into regions
- connects regions using pathfinding
- stores grid, regions, and stair zone references
Observation:
- not a zone builder
- defines multi-zone structure, not per-zone content
- uses pathfinding to create connected layout
Notes:
- strong conceptual match to planned site/path system
- demonstrates region connection via pathfinding
- useful for path topology generation, not rendering
- Limited to 2d, we need 3d
Decision:
REFERENCE (high value)

## ZoneBuilder Evaluation – CatacombsPublicus

CatacombsPublicus
Type: FULL ZONE BUILDER (renderer over global layout)
Behavior:
- loads shared CatacombsMapTemplate
- renders this zone as a slice of a larger global grid
- places walls/floors based on template
- uses pathfinding to connect local areas
- builds rooms and places content via InfluenceMap
Observation:
- tightly coupled to catacombs system
- not reusable as a general builder
- demonstrates global-layout + per-zone rendering pattern
Notes:
- confirms separation of global structure vs per-zone rendering
- similar to planned site system (matrix-based deterministic layout)
- not directly useful for path/holes implementation
Decision:
REFERENCE (architecture only)

## ZoneBuilder Evaluation – Catacombs System
CatacombsMapTemplate / CatacombsPublicus
Type: MULTI-ZONE HORIZONTAL STRUCTURE
Behavior:
- generates a larger shared layout
- renders individual zones as slices of that layout
- supports traversal across connected zones in a quest area
- useful model for flat multi-zone areas such as a 3x3 parasang site
Observation:
- relevant to earlier 3x3 flat-site concept
- less relevant to current vertical-site direction
- does not solve x,y,z path guidance or path-hole behavior
- demonstrates how related horizontal zones can share one generated layout
Notes:
- good example of connecting same-theme horizontal zones
- not a priority for current one-path-to-vertical-site implementation
- may be revisited if flat multi-zone sites return later
Decision:
REFERENCE (old flat-site concept / architecture only)

## ZoneBuilder Evaluation – Cave / Cave Mouth Builders

Cave
Type: FULL / BASE TERRAIN BUILDER
Behavior:
- fills zone with default wall
- carves cave space using cellular noise and Perlin noise
- uses a static 3D maze to determine cross-zone tunnel directions
- places cached cave/tunnel connections at zone edges
- places stairs up/down based on 3D maze connectivity
- uses FindPath to connect tunnel nodes inside the zone
- clears/widens paths between connection points
- rebuilds reachability from connections
Observation:
- not useful as direct builder for special sites because it is normal cave terrain
- very useful as reference for cross-zone path continuity
- shows how Qud creates edge tunnel mouths based on a larger 3D tunnel model
- confirms that horizontal and vertical cave connectivity can be driven by a precomputed/deterministic 3D structure
Notes:
- stronger reference than CanyonBuilder for x,y,z path logic
- relevant to deterministic path coordinate generation
- path system may use similar concepts: path nodes, edge mouths, internal carving, reachability
- do not call directly for site/path generation
Decision:
REFERENCE (high value)

CaveEastMouth / CaveNorthMouth / CaveSouthMouth / CaveWestMouth
Type: CONNECTION BUILDER
Behavior:
- creates Cave connection mouths on zone edges
- sets Range = 3 before calling ConnectionMouth
Observation:
- same general role as Canyon mouth builders
- useful for understanding edge opening / zone transition support
Notes:
- likely relevant when implementing custom path edge mouths
- Range = 3 may indicate a wider mouth/opening than a single-cell connector
Decision:
REFERENCE only

CaveCity
Type: STRUCTURE (region-based)
Behavior:
- converts InfluenceMap regions into enclosed cave rooms
- adds walls, one door, optional population
- tunnels to connect rooms
Observation:
- depends on existing cave + InfluenceMap
- builds small enclosed structures inside caves
Decision:
SKIP

CentralitySorter
Type: UTILITY
Behavior:
- sorts objects by distance from a center point
Observation:
- simple helper for ordering objects (e.g., nearest to center first)
- not a builder or generation system
Decision:
SKIP

## ZoneBuilder Evaluation – ChavvahRoots

ChavvahRoots
Type: VERTICAL SYSTEM (root network)
Behavior:
- operates across Z 0–50
- places pits on most levels (skips every 5th level)
- adds stairs at regular intervals
- generates branching tunnels using TunnelTo
Observation:
- not a simple vertical column
- mixes pits and structured transitions
- separates path carving from vertical transitions
Decision:
REFERENCE (vertical design)
Notes:
- useful example of intermittent hole placement
- supports idea that holes and paths should be separate systems

## ZoneBuilder Evaluation – ChildrenOfTheTomb / ChildrenOfTheTombQuestHandler

ChildrenOfTheTomb
ChildrenOfTheTombQuestHandler
Type: QUEST / EVENT LOGIC
Behavior:
- handles quest-specific spawning, events, and progression
Observation:
- tightly coupled to specific quest content
- not reusable for general generation
Decision:
SKIP

## ZoneBuilder Evaluation – ClearAll

ClearAll
Type: UTILITY / CLEANUP
Behavior:
- clears every cell in the zone
- optionally includes combat objects
Observation:
- same general pattern already used manually in test harness
- useful as a simple pre-builder before applying clean layouts
Decision:
MAYBE (utility)
Notes:
- could replace manual foreach cell.Clear() code
- useful when a builder needs a fully clean zone first

ClearWallAddObject
Type: UTILITY
Behavior:
- clears walls in target cells
- adds object clears walls
Decision:
SKIP

CollapseAtLevel
Type: EVENT / CONDITIONAL OVERRIDE
Behavior:
- checks player level
- if below threshold: does nothing
- if at/above threshold:
    - clears entire zone
    - fills with Halite
Observation:
- destroys/replaces zone based on player progression
- not procedural generation, more like a triggered transformation
Decision:
SKIP
Notes:
- example of conditional zone override
- not relevant to site/path generation

## ZoneBuilder Evaluation – Connecter

Connecter
Type: UTILITY / CONNECTIVITY
Behavior:
- finds a stairs connection (up/down)
- clears walls and impassable objects at that location
- rebuilds reachability from that point
Observation:
- ensures stairs are usable and connected to the zone
- does not generate content, only fixes connectivity
Decision:
SKIP
Notes:
- useful pattern: ensure connection point is reachable
- relevant if custom path holes or entrances need guaranteed accessibility

## Design Note – Connectivity Fix Pattern

Observation:
- Qud ensures connection points (stairs, holes) are reachable after generation
- may clear walls or carve tunnels to connect isolated points
Implication:
- PathHoleBuilder should include a connectivity fix step
- hole/entry points must always connect to the local navigable space
Pattern:
generate feature → check reachability → fix if needed

## ZoneBuilder Evaluation – Additional Builders

ConveyorBelt
Type: SPECIALIZED / QUEST DUNGEON
Behavior:
- tomb-specific conveyor belt content
Observation:
- tied to Tomb of the Eaters systems
- not relevant to subterranean site/path generation
Decision:
SKIP

---

Craters
Type: SURFACE FEATURE
Behavior:
- creates crater terrain/features
Observation:
- surface-oriented
- not relevant to underground site generation
Decision:
SKIP

---

Cryobarrio / Cryobarrio1 / Cryobarrio2
Type: SPECIALIZED / QUEST DUNGEON
Behavior:
- cryobarrio-specific dungeon content
Observation:
- tied to a specific quest dungeon
- not reusable for general subterranean sites
Decision:
SKIP

---

CryptOfLandlords
Type: SPECIALIZED / QUEST AREA
Behavior:
- crypt-specific quest area generation
Observation:
- tied to named quest content
Decision:
SKIP

---

CryptOfPriests / CryptOfWarriors
Type: SPECIALIZED / QUEST AREA
Behavior:
- crypt-specific quest area generation
Observation:
- tied to named quest content
Decision:
SKIP

---

DenseBrinestalk
Type: DECORATOR / VEGETATION
Behavior:
- adds dense brinestalk-style growth
Observation:
- not aligned with site/path goals
Decision:
SKIP

EmptyGround
Type: NO-OP / PLACEHOLDER
Behavior:
- does nothing
Observation:
- likely used as a placeholder or default builder
- no effect on zone content
Decision:
SKIP

FactionEncounters
Type: ENCOUNTER INJECTION
Behavior:
- randomly selects a faction from a population table
- spawns a faction encounter group (leader + squad)
- equips members based on zone tier
- filters members based on zone level
- may add faction-related objects
Arguments:
- Population: population table used to select faction (default GenericFactionPopulation)
- Chance: percent chance per roll
- Rolls: number of attempts (each success spawns one encounter)
Observation:
- additive; does not clear or override existing layout
- produces coherent faction groups (leader + followers)
- can spawn legendary leaders with squads
- scaling is tied to zone level/tier but may feel strong in early zones
- works cleanly when layered after other builders
Decision:
USE
Notes:
- good for adding ambient faction encounters to sites and paths
- keep Chance low in final implementation to avoid overcrowding
- may want custom population table later for tighter control
- useful for adding variability without defining full site population

FindA...DynamicQuest (group)
Type: QUEST / DYNAMIC CONTENT
Behavior:
- quest-specific generation and event handling
Observation:
- tied to dynamic quest systems
- not reusable for general procedural generation
Decision:
SKIP

FlagInside
Type: UTILITY / FLAGGING
Behavior:
- likely marks interior cells or regions
Observation:
- low-level helper
- not relevant to current generation goals
Decision:
SKIP

FlowerFields
Type: DECORATOR / SURFACE
Behavior:
- adds flower field terrain/features
Observation:
- surface aesthetic builder
- not relevant to subterranean sites
Decision:
SKIP

ForceConnections
Type: CONNECTIVITY / FIX-UP
Behavior:
- collects all connection points (stairs, zone connections, etc.)
- ensures all points are reachable from each other
- uses pathfinding with noise to carve tunnels if needed
- clears walls along carved paths
- optionally widens paths (CaveLike behavior)
Observation:
- general-purpose “make everything connected” pass
- similar to Canyon/Shug carving but used as a safety/fix layer
- works on any zone regardless of how it was generated
Decision:
REFERENCE (high value)
Notes:
- very relevant for path/holes system
- can be used after generation to guarantee connectivity
- likely useful for fixing unreachable holes or path segments
- CaveLike option gives natural-looking tunnels

ForceConnectionsPlus
Type: CONNECTIVITY / FIX-UP
Behavior:
- finds all passable cells in the zone
- builds reachability from the first passable cell
- for any isolated passable cell, carves a path to connect it
- clears walls along paths
- optionally widens paths with CaveLike behavior
Observation:
- more aggressive than ForceConnections
- connects all passable regions, not just stairs/connections
- useful for preventing isolated pockets
- may over-carve zones if used casually
Decision:
REFERENCE
Notes:
- ForceConnections is probably better for path/hole endpoints
- ForceConnectionsPlus is a broader safety pass
- use cautiously; may erase intended separation between areas

FortMaker
Type: STRUCTURE (surface/fort layout)
Behavior:
- generates large boxed fort structure
- creates walls, rooms, and doors using templates
- fills interior with floor
- places additional objects from population tables
- optionally clears combat objects first
Observation:
- builds structured, rectangular, aboveground-style layouts
- not aligned with natural cave/path systems
- not relevant to subterranean site/path goals
Decision:
SKIP

Type: DECORATOR / PLANT GROWTH
Behavior:
- plants Fracti at a point
- grows outward over allowed floor types
Observation:
- localized vegetation growth
- not relevant to path or site structure
Decision:
SKIP

FungalJungle
Type: FULL BIOME / GLOBAL STRUCTURE
Behavior:
- generates large-scale maze structure across zones
- uses maze to define cross-zone connections (river mouths)
- builds rivers between zones
- populates fungal terrain and vegetation via influence maps + noise
- ensures reachability
Observation:
- another example of global multi-zone structure (like Catacombs)
- uses connections to define traversal between zones
- heavy biome + decoration logic
- not aligned with targeted path + vertical site system
Decision:
REFERENCE (low)
Notes:
- confirms connection-based multi-zone layout pattern
- not needed for current path/holes implementation

FungalTrailBuilder
Type: PATH / TRAIL BUILDER
Behavior:
- reads FungalTrail zone connections
- uses connection mouths/start points as endpoints
- uses FindPath to connect endpoints inside the zone
- paints the path with FungalTrailBrick
- paints adjacent cells too, making the trail visually broad
- if only one connection exists, generates a local patch of trail material
Observation:
- very close to planned path rendering system
- simpler than ShugBurrowBuilder
- good reference for non-hole path materials
- shows how Qud creates a visible multi-zone trail players can follow
Decision:
REFERENCE (high value)
Notes:
- strong model for dirt/stone/resin path rendering
- likely useful for horizontal path segments
- does not handle vertical holes by itself
- pair with ShugBurrow-style descending/ascending logic for holes

---

FungalTrailNorthMouth / SouthMouth / EastMouth / WestMouth
Type: CONNECTION BUILDER
Behavior:
- creates FungalTrail edge mouths
Observation:
- same mouth/endpoint pattern as Canyon, Cave, and ShugBurrow
Decision:
REFERENCE

FungalTrailStartMouth
Type: CONNECTION BUILDER
Behavior:
- creates FungalTrail start point
Observation:
- useful model for path origin / terminal points
Decision:
REFERENCE

FungalTrailExileCorpse / FungalTrailKlanqHut
Type: QUEST CONTENT
Behavior:
- places trail-related quest endpoint content
Observation:
- specific to Klanq/fungal trail quest
Decision:
SKIP

GenericChutes
Type: VERTICAL SYSTEM / STRUCTURED DESCENT
Behavior:
- generates chute structures across multiple Z-levels
- uses predefined tile patterns (N/S/E/W, turns, starts/ends)
- aligns vertical transitions using zone properties (StartY/EndY)
- builds continuous vertical pathways between layers
- places SlimyShaft (vertical drop), conveyors, and traps
- alternates layout patterns by depth (Z parity)
Observation:
- explicit example of multi-layer (Z) connectivity system
- uses deterministic per-column properties to align vertical paths
- separates:
    - global structure (chute template across layers)
    - per-zone rendering (tiles applied locally)
- enforces continuity between zones above/below via stored properties
Decision:
REFERENCE (high value)
Notes:
- strong reference for vertical path alignment across Z levels
- relevant to hole/path consistency between stacked zones
- shows how to persist alignment data across zones (ZoneProperty)
- more structured/grid-based than desired organic path system
- not directly usable, but conceptually important for vertical continuity

GenericChutes
Type: VERTICAL SYSTEM / STRUCTURED DESCENT
Behavior:
- generates chute structures across multiple Z-levels
- uses predefined tile patterns (N/S/E/W, turns, starts/ends)
- aligns vertical transitions using zone properties (StartY/EndY)
- builds continuous vertical pathways between layers
- places SlimyShaft (vertical drop), conveyors, and traps
- alternates layout patterns by depth (Z parity)
Observation:
- explicit example of multi-layer (Z) connectivity system
- uses deterministic per-column properties to align vertical paths
- separates:
    - global structure (chute template across layers)
    - per-zone rendering (tiles applied locally)
- enforces continuity between zones above/below via stored properties
Decision:
REFERENCE (high value)
Notes:
- strong reference for vertical path alignment across Z levels
- relevant to hole/path consistency between stacked zones
- shows how to persist alignment data across zones (ZoneProperty)
- more structured/grid-based than desired organic path system
- not directly usable, but conceptually important for vertical continuity

GenericSolid
Type: UTILITY / FILL
Behavior:
- optionally clears every cell
- fills entire zone with one material
Arguments:
- Material: wall/object blueprint to fill with (default Shale)
- ClearFirst: whether to clear cells before filling
Observation:
- simple full-zone fill utility
- useful for controlled tests or blank solid starting conditions
- not a site/path builder
Decision:
MAYBE (utility)
Notes:
- could be useful before testing custom carving/path builders
- not needed for final procedural generation unless a fully solid test zone is desired

## ZoneBuilder Evaluation – GirshLairMakerBase

GirshLairMakerBase
Type: SPECIALIZED DUNGEON FRAMEWORK (GIRSH / NEPHILIM)
Behavior:
- wrapper around dungeon generation (often calls SultanDungeon internally)
- sets metadata (name, XP, secrets, etc.)
- delegates layout to BuildLair() in subclasses (e.g., ShugLair)
- applies thematic mutation to creatures (burrowing, faction flavor, visuals)
- adds special features (holy places, bosses, pits, etc.)
Observation:
- not a generator itself; framework + post-processing layer
- subclasses define highly specific dungeon layouts and behaviors
- tightly coupled to Girsh/Nephilim content and quest systems
Decision:
REFERENCE (low priority)
Notes:
- useful example of how to wrap a generator with theme + mutation logic
- not suitable as a general site generator
- keep in mind for:
    - boss site variants
    - special “deep site” mutations
- otherwise prefer SultanDungeon for flexibility

## ZoneBuilder Evaluation – Skipped Set

GoatfolkQlippothYurts / GoatfolkYurts
Type: FACTION CAMP
Decision: SKIP
Notes:
- surface encampments
- not relevant to subterranean/path system

GolgathaChutes / GolgathaTemplate
Type: SPECIAL DUNGEON (GOLGOTHA)
Decision: SKIP (already reviewed core chute system)
Notes:
- specific to Golgotha mechanics

HamilcrabShop
Type: SPECIAL LOCATION / SHOP
Decision: SKIP
Notes:
- one-off content

Hills
Type: SURFACE TERRAIN
Decision: SKIP

HindrenClues
Type: QUEST LOGIC
Decision: SKIP

Hive
Type: FACTION-SPECIFIC STRUCTURE
Decision: SKIP
Notes:
- potentially interesting pattern-wise, but out of scope for now

Hydrolics
Type: SPECIAL FEATURE / MECHANIC
Decision: SKIP
Notes:
- likely environmental/industrial feature, not core to path/site system

IConnectionBuilder
Type: CONNECTION UTILITY / BASE CLASS
Behavior:
- creates local zone connection points
- creates matching opposite-side connection points in neighboring zones
- supports edge mouths and internal start points
- clears the mouth cell when placing the connection
- avoids duplicate connections if one already exists
Key methods:
- ConnectionMouth(...)
    creates an edge connection for North/South/East/West paths
- ConnectionStart(...)
    creates an internal start point
- MatchConnection(...)
    checks neighboring built zones for matching opposite connection
Observation:
- this is the shared base system behind Cave, Canyon, FungalTrail, and ShugBurrow mouths
- confirms how Qud makes cross-zone path continuity
- mouth builders do more than mark the current zone; they also cache the matching connection in the adjacent zone
Decision:
REFERENCE (high value)
Notes:
- very relevant for custom path mouths
- custom path system may either use this pattern directly or implement a simplified version
- key pattern: create current mouth + matching opposite mouth in neighboring zone

## ZoneBuilder Evaluation – InfluenceMapSeedStrategy

InfluenceMapSeedStrategy
Type: ENUM / REGION SEEDING STRATEGY
Behavior:
- defines how seed points are chosen in an InfluenceMap
- used for region generation, partitioning, and layout logic

Options:
- FurthestPoint
    → spreads seeds as far apart as possibl (max separation)
- LargestRegion
    → biases toward largest available region
- RandomPointFurtherThan4 / FurtherThan1
    → random but enforces minimum distance
- RandomPoint
    → fully random
Observation:
- used in dungeon/layout builders (Sultan, CaveCity, etc.)
- controls how regions/rooms/clusters are distributed spatially
- affects layout structure indirectly via seed placement
Decision:
REFERENCE (low–medium value)
Notes:
- not directly needed for path system
- potentially useful later for:
    - distributing rooms within a site
    - placing sub-features (chambers, hubs, etc.)
- FurthestPoint could be useful for evenly spaced key features

InsertPresetFromPopulation
Type: PRESET / MAP CHUNK PLACEMENT
Behavior:
- rolls a blueprint from a population table
- creates that object
- checks for MapChunkPlacement or MultiMapChunkPlacement size
- attempts to place it in the zone away from stairs
- runs ForceConnections after placement
Observation:
- useful for inserting premade map chunks or structures
- population-driven, so it can randomly select from a set of presets
- not directly useful for path generation
- may be useful later for special fixed features or entrance structures
Decision:
MAYBE (later)
Notes:
- possible use for handcrafted site rooms or landmark inserts
- ForceConnections after placement is a useful pattern
- not needed for current path/vertical-site system

## ZoneBuilder Evaluation – InteractWithAnObject... (Quest Builders)

Type: QUEST / INTERACTION LOGIC
Behavior:
- places or configures quest-specific objects
- ties objects to interaction events or triggers
- used for scripted quest progression
Observation:
- not related to terrain, layout, or pathing
- operates at gameplay/interaction layer, not generation layer
Decision:
SKIP
Notes:
- ignore for world generation work
- relevant only if adding custom quests later

InteriorGround
Type: BASE TERRAIN / INTERIOR FILL
Behavior:
- clears reachable map
- fills every cell with InteriorVoid
- paints terrain using Rocky.Paint (stone floor/wall styling)
Observation:
- creates a uniform interior baseline terrain
- similar to GenericSolid but uses InteriorVoid + terrain paint instead of a single material object
- used as a foundation for interior-style zones
Decision:
SKIP (for now)

IsCheckpoint
Type: GAMEPLAY FLAG / CHECKPOINT
Behavior:
- places a CheckpointWidget at (0,0)
- assigns a checkpoint key string
- marks the zone as a checkpoint location
Observation:
- not related to terrain or generation
- purely gameplay/system-level marker
- likely used for respawn, fast travel, or progression tracking
Decision:
SKIP


ISultanDungeonSegment
Type: GENERATION SHAPE
Summary:
- defines a region/shape inside a dungeon (rectangle, circle, etc.)
- used during generation to decide where things go
- not stored data, just used while building the layout
Decision:
SKIP

## ZoneBuilder Evaluation – Skipped Set

JoppaOutskirts / JoppaOutskirtsRuins
Type: START AREA / SCRIPTED CONTENT
Decision: SKIP
Notes:
- tightly tied to Joppa start region
- not reusable for generic generation

JungleRuins
Type: BIOME-SPECIFIC RUINS
Decision: SKIP
Notes:
- surface/biome-specific content
- not aligned with subterranean system

LakeOfTheDamned
Type: UNIQUE LOCATION
Decision: SKIP
Notes:
- one-off special zone
- not relevant to general generation system

## ZoneBuilder Evaluation – LiquidPools

LiquidPools
Type: DECORATOR / LIQUID FEATURE
Behavior:
- creates liquid pools using NoiseMap
- biases pool generation around existing zone connections
- only places liquid in empty cells
- can replace nearby plants using a population table
- can set some pools on fire/heated
Observation:
- useful environmental decorator
- not a structure or path builder
- connection-aware, but only for liquid placement
Decision:
MAYBE (decorator)
Notes:
- possible use for special site variants
- not core to path/vertical-site system

## ZoneBuilder Evaluation – MapBuilder

MapBuilder
Type: PRESET MAP PLACEMENT
Behavior:
- loads a map file by ID
- places the map into the current zone
- can clear cells before placement
- can clear chasms
- can require target cells to be empty
- optionally rebuilds reachability after placement
Arguments:
- ID / FileName: map file to place
- X / Y: placement origin
- Width / Height: placement size
- ClearBeforePlace: clear cells before applying map
- ClearChasms: remove chasm material
- CheckEmpty: only place into empty cells
- BuildReachability: rebuild reachability after placement
Observation:
- useful for handcrafted maps or fixed set pieces
- not procedural by itself
- appears in specialized builders as a way to place predefined layouts
Decision:
MAYBE (later)
Notes:
- possible use for special entrances, boss rooms, or rare fixed site variants
- not core to path generation

Mines/Mines2
Type: TERRAIN / NOISE CARVING
Behavior:
- builds a NoiseMap using zone connections as extra nodes
- clears cells in generated noise regions
- places AsphaltPuddle in deeper noise nodes
- rebuilds reachability from the largest generated region
Observation:
- connection-aware terrain decorator/carver
- not a full structure or path builder
- may create mine-like open areas but does not provide path logic
Decision:
SKIP\\## ZoneBuilder Evaluation – Skipped Set

MinorRazedGoatfolkVillage
Type: FACTION / SURFACE EVENT
Decision: SKIP

MoonStair
Type: UNIQUE REGION / WORLD FEATURE
Decision: SKIP
Notes:
- interesting style but not part of generic dungeon system
- unlikely reusable via SultanDungeon directly

Music
Type: AUDIO
Decision: SKIP

Odditorium
Type: UNIQUE LOCATION
Decision: SKIP

Omonporch / OmonporchGroveBuilder
Type: UNIQUE LOCATION
Decision: SKIP

OverlandAlgeaLake / OverlandRuins / OverlandWater
Type: SURFACE TERRAIN
Decision: SKIP

Pharmacorium
Type: UNIQUE LOCATION
Decision: SKIP

PigFarm / PigFarmMaker
Type: FACTION / SURFACE STRUCTURE
Decision: SKIP

Pitted
Type: UTILITY (PIT / HOLE CREATION)
Decision: REFERENCE (high value)
Notes:
- used in Girsh/Shug builders
- directly relevant for hole placement
- worth revisiting when implementing vertical transitions

PlaceAClam
Type: DECORATOR
Decision: SKIP

PlaceRelicBuilder
Type: LOOT / RELIC PLACEMENT
Behavior:
- places a relic object into the zone
- prefers objects tagged RelicContainer
- otherwise may place relic in:
    - random container
    - inventory of combat creature
    - empty reachable cell
- marks relic holder/object as important
- optionally adds cybernetics credit wedges based on zone tier
Observation:
- useful for rewarding major sites
- already integrated with SultanDungeon relic generation
- not a layout or path builder
Decision:
MAYBE (site reward system)

## ZoneBuilder Evaluation – Skipped Set

PlaceWedgeBuilder1 / PlaceWedgeBuilder2 / PlaceWedgeBuilder3 / PlaceWedgeBuilder4 / PlaceWedgeBuilder5 / PlaceWedgeBuilder6
Type: LOOT / CREDIT WEDGE PLACEMENT
Decision: SKIP
Notes:
- narrow reward placement helpers
- PlaceRelicBuilder already covers the more relevant reward pattern

Plains
Type: SURFACE TERRAIN
Decision: SKIP

PopTableZoneBuilder
Type: POPULATION / OBJECT PLACEMENT
Behavior:
- generates objects from a population table
- places aquatic objects in liquid when possible
- places wall-living objects on walls when possible
- otherwise places objects in reachable empty cells
- can apply the zone faction to spawned objects
Arguments:
- Table: population table to generate from
- Density: present but appears unused here
- bApplyZoneFactionToObjects: if true, makes brain-bearing objects use the zone faction
Observation:
- useful generic population placement helper
- additive; does not build terrain
- more direct and flexible than full faction encounters
- could be useful for supplementing sparse site populations
Decision:
MAYBE

TYPE:
POPULATION / OBJECT PLACEMENT
Arguments:
- Table: population table to generate from
- Density: present but not used in this BuildZone method
Behavior:
- Checks that Table exists in PopulationManager
- Generates objects from Table using zonetier = NewZone.NewTier
- Expands population results into GameObjects
- Sorts possible placement cells into:
  - liquid cells
  - wall cells
  - reachable empty solid-free cells
- Places aquatic/liquid-start objects in liquid if available
- Places wall-living creatures on walls if available
- Places normal objects in reachable empty cells
- Falls back to any cell if no preferred cells exist
Observation:
- This is very similar in role to PopTableZoneBuilder.
- It is a direct population overlay, not a layout builder.
- Density appears misleading here because it is declared but unused.
- Useful for adding creatures/objects after a structure builder has already made reachable space.
- Because it relies on IsReachable(), it likely works better after a connectivity pass or after the base builder has already built reachability.
- It does not clear the zone or alter geometry.
- If Table is invalid, it logs an error but does not crash.
- Supplemental site population after BasicLair/SultanDungeon/custom site generation
- Controlled object/creature placement from a known valid population table
Decision:
MAYBE USE

PopulationLayout
TYPE: Utility / region layout data object
ARGS: zone, InfluenceMapRegion, innerRect, optional position
BEHAVIOR: Stores region/layout placement context: inside/outside cells, wall/corner lists, seed index, rect, and position. Position setter updates the InfluenceMap seed.
OBSERVATION: Not a builder. Support object for region-aware furnishing/population systems, likely related to InfluenceMap-based placement.
DECISION: REFERENCE ONLY


PowerGrid
TYPE: Infrastructure / pathfinding-based system builder
ARGS: DamageChance, DamageIsBreakageChance, ConduitBlueprint, ConduitModPart, MissingConsumers, MissingProducers, charge rates, location specs, flags (PreferWalls, AvoidWalls, Noise, debug flags)
BEHAVIOR: Scans zone for power producers/consumers, optionally spawns missing ones, then uses pathfinding to connect them via conduits (either modifying walls or placing PowerLine objects). Applies weights to map, routes connections, may damage/break segments. Uses ElectricalPowerTransmission system.
OBSERVATION: This is a full system builder, not flavor. It constructs a functional electrical network across the zone. Uses pathfinding + weighted maps to route connections intelligently (prefers walls, avoids obstacles, etc). Can modify existing objects or place new conduits. Has debug visualization options. Very heavy and specialized.
DECISION: IGNORE (for current project)


PulldownLocation
TYPE: Metadata / zone property setter
ARGS: x, y
BEHAVIOR: Stores a coordinate string ("x,y") in the zone property "PullDownLocation".
OBSERVATION: Does not modify geometry or population. Likely used by other systems (UI, events, or special interactions) that read this property. No standalone effect.
DECISION: IGNORE

QasQonLair
TYPE: Structure + multi-zone vertical lair (Girsh system)
ARGS: Nephal flag
BEHAVIOR: Builds pit-based multi-level lair using Pitted + connection type "CysticPit". Links pits across Z-levels via ZoneConnections. Generates cyst rooms, populates with QasQonCyst, spawns bosses (Qas/Qon) at depth. Applies additional effects (hologram walls, mutations).
OBSERVATION: Same core pattern as Shug: pits + connection types = vertical traversal. Uses PopulateCysticPitConnections to sync holes across levels. More advanced: depth scaling (# of pits), multi-entity boss, environmental effects (holograms). :contentReference[oaicite:0]{index=0}
DECISION: REFERENCE (important pattern, but Shug already sufficient for path system)

RandomAmbientStabilization
TYPE: Environmental / zone effect setter
ARGS: Strength (string roll spec)
BEHAVIOR: Adds AmbientStabilization part to zone and sets its strength via roll.
OBSERVATION: Does not affect geometry or population. Likely influences ambient/environmental effects (possibly psychic/reality stability mechanics). Very lightweight.
DECISION: IGNORE

RazedGoatfolkVillage
TYPE: Structure (wrapper for VillageMaker preset)
ARGS: none (hardcoded VillageMaker parameters)
BEHAVIOR: Builds a razed/destroyed goatfolk village using VillageMaker with specific wall, layout, and population templates.
OBSERVATION: Thin wrapper around VillageMaker. Not a new system—just a preset configuration (theme + population + structures). VillageMaker itself is the real reusable system.
DECISION: IGNORE (unless VillageMaker becomes relevant later)

Reachability*Edge (East/South/West + typo variant)
TYPE: Utility / reachability map builder
ARGS: ClearFirst (bool)
BEHAVIOR: Clears and rebuilds zone reachability map starting from one edge (east/south/west), marking accessible tiles.
OBSERVATION: Duplicate typo class exists (ReachabilitEastEdge). Used to ensure zones are navigable from map edges. Does not modify geometry—only pathing metadata.
DECISION: IGNORE (unless debugging reachability issues)

RedrockOutcrop
TYPE: Structure (minor terrain feature)
ARGS: none (sets Blueprint = "Shale")
BEHAVIOR: Inherits WallOutcrop and places shale outcrop terrain.
OBSERVATION: Simple themed terrain feature. Not important beyond indicating how biome flavor is applied.
DECISION: IGNORE

Redrock
TYPE: Full structure / multi-layer dungeon generator
ARGS: none
BEHAVIOR: Builds Redrock canyon dungeon using noise maps, tunnels, pools, and influence maps. Adds stairs, connects regions, and injects special structures (stockade, cave city, fort, river) depending on depth. Populates encounters by tier. :contentReference[oaicite:0]{index=0}
OBSERVATION: Major example of:
- NoiseMap-based terrain carving
- InfluenceMap region logic
- Multi-builder orchestration (StockadeMaker, CaveCity, RiverBuilder)
- Depth-based feature switching
- Post-process connectivity enforcement
DECISION: REFERENCE (good for patterns, not direct reuse)

RedrockStockadeMaker
TYPE: Structure (compound building generator)
ARGS: many (wall type, populations, room sizes, etc.)
BEHAVIOR: Generates fortified compound:
- creates outer box walls
- subdivides into rooms
- places populations per room (including legendary chance)
- cuts doors, arrow slits, entrances
- ensures connectivity and populates interior/exterior zones :contentReference[oaicite:1]{index=1}
OBSERVATION: Strong reusable pattern for:
- “site within a zone”
- room subdivision + population tables
- guaranteed boss/legendary placement logic
- defensive layouts
DECISION: REFERENCE (useful for later site structuring ideas)

Reef
TYPE: Full procedural biome generator (noise + multi-system)
ARGS: many (noise seeds, internal configs)
BEHAVIOR: Generates entire zone using multiple layout systems (reef, cave, random walk), blends them via noise, assigns wall types by weighted depth-dependent selection, places biome objects (coral, pools, etc), paints tiles, then connects zones using a global 3D maze system. :contentReference[oaicite:0]{index=0}
OBSERVATION: Extremely complex. Combines:
- noise-driven layout selection
- weighted material selection by depth
- deterministic seeded noise (world-seed based)
- global 3D maze for cross-zone connectivity
- post-pass reachability + path carving
DECISION: IGNORE (but extract patterns)

RegionPopulator
TYPE: Debug / influence-map region generator
ARGS: RegionSize=100
BEHAVIOR: Generates an InfluenceMap using FurthestPoint seed strategy and draws it.
OBSERVATION: Does not populate despite the name. Looks like a visualization/debug helper for region partitioning.
DECISION: IGNORE

RermadonLair
TYPE: Structure / multi-zone vertical lair (Girsh system)
ARGS: Nephal flag
BEHAVIOR: Builds pit/cyst-based lair using Pitted and connection type "CysticPit". Surface gets pits + cysts. Underground levels reuse/extend pit connections, add more cysts by depth, populate with RermadonCyst, and spawn Rermadon asleep when Nephal=true. Applies RermadonCradle template and connectivity fix.
OBSERVATION: Same reusable pattern as QasQon: CysticPit connections + Pitted + cyst rooms + depth-scaled pit count. Main difference is theme: algal water, Rermadon wall/cyst populations, plasmatic cultist mutation/corpse behavior.
DECISION: REFERENCE ONLY

River*Mouth
TYPE: Connection builders / path endpoints
ARGS: none
BEHAVIOR: RiverEast/North/South/West create edge mouths via IConnectionBuilder.ConnectionMouth("River", direction). RiverStartMouth creates an internal start point via ConnectionStart("River").
OBSERVATION: Same mouth pattern as Shug/FungalTrail/Canyon. Defines endpoints only; RiverBuilder consumes them.
DECISION: REFERENCE

RiverBuilder
TYPE: Path / liquid trail builder
ARGS: Puddle, Pairs, HardClear; constructor can set VillageMode
BEHAVIOR: Reads River connections from ZoneConnectionCache and ZoneManager, chooses terrain-appropriate liquid, pathfinds between river endpoints, clears terrain, places liquid along path, widens river, optionally bridges village floors, and sets river ambience.
OBSERVATION: Strong reusable pattern for visible path rendering. Similar to ShugBurrowBuilder but simpler and liquid-based. Important separation: mouth builders define where river enters/exits; RiverBuilder renders how the river looks locally.
DECISION: REFERENCE HIGH VALUE

Road*Mouth
TYPE: Connection builders / path endpoints
ARGS: none
BEHAVIOR: RoadEast/North/South/West create edge mouths via IConnectionBuilder.ConnectionMouth("Road", direction). RoadStartMouth creates an internal start point via ConnectionStart("Road").
OBSERVATION: Same connection-mouth pattern as River/Shug/FungalTrail/Canyon. Defines endpoints only; RoadBuilder renders the road.
DECISION: REFERENCE

RoadBuilder
TYPE: Path / terrain trail builder
ARGS: HardClear, ClearSolids=true, ClearAdjacent=true, Noise=true, BeforePlacement action
BEHAVIOR: Reads Road connections, pathfinds between start/edge points, clears terrain/solids as configured, places DirtRoad along path and adjacent cells, bridges non-blood liquids, optionally places DirtPath patch near RoadStart. 
OBSERVATION: Very relevant simple path renderer. Unlike RiverBuilder, it paints a dry traversable path. BeforePlacement hook is interesting for custom preprocessing. Good model for dirt/stone path material.
DECISION: REFERENCE HIGH VALUE

RoomData
TYPE: Utility / room data container (serialization)
ARGS: Left, Right, Top, Bottom, Width, Height, Size, Room[int[,] grid]
BEHAVIOR: Stores rectangular room bounds and a 2D grid, supports serialization/deserialization.
OBSERVATION: Not a builder. Used by systems that generate/subdivide rooms (e.g., dungeon/compound makers). Pure data holder.
DECISION: IGNORE

RuinedWharf
TYPE: Structure / maze-like ruin layout
ARGS: none
BEHAVIOR: Fills zone with Limestone, generates a deterministic 20x6 recursive-backtracker maze keyed by "WharfMaze" + ZoneID, clears maze corridors, clears random non-overlapping box rooms, clears some fixed rectangular areas, then builds reachable map.
OBSERVATION: Standalone structure generator. Interesting only because it uses a zone-keyed maze seed for deterministic local layout. Otherwise too specific and not aligned with underground site/path goals.
DECISION: REFERENCE ONLY

Ruiner
TYPE: Destructive post-processor / ruin decorator
ARGS: RuinAmount=50; RuinZone also accepts RuinLevel, bUnderground, SurfaceExplosionForce, UndergroundExplosionForce
BEHAVIOR: Applies random explosions across the zone, clears connection points, cleans narrow diagonal chokepoints, removes orphan doors, then rebuilds reachability. Returns false if it cannot find a reachable area >400 cells.
OBSERVATION: Useful as a generic “damage this generated structure” pass. It preserves/clears connection points after explosions, so it is safer than random destruction alone. Could be interesting later for ruined site variants, but not core.
DECISION: MAYBE LATER

RuinPowerGrids
TYPE: Infrastructure post-processor / damaged utility networks
ARGS: DamageChance, missing producer/consumer chances/counts for electrical/hydraulic/mechanical systems, HydraulicLiquid/Table, PreferWalls, AvoidWalls
BEHAVIOR: Rolls shared damage chance, then applies PowerGrid, Hydraulics, and MechanicalPower builders with optional missing producers/consumers. Rebuilds reachability afterward.
OBSERVATION: Wrapper/orchestrator for ruined infrastructure systems. Too specialized unless making technological ruin variants.
DECISION: IGNORE / MAYBE LATER FOR TECH RUINS

Ruins
TYPE: Structure / multi-zone ruin generator
ARGS: RuinLevel=100, ZonesWide="1d3", ZonesHigh="1d2"
BEHAVIOR: Creates or reuses a BuildingZoneTemplate stored as a zone-column property, assigns semantic tags to cells from the template map, adds ruin semantic tags from population tables, builds the template, then applies Ruiner destruction and rebuilds reachability.
OBSERVATION: Important pattern: shared zone-column template lets multiple Z-level zones in the same column use consistent ruin structure/semantics. Uses semantic tags heavily for later replacement/decoration. Strong system, but probably too building/ruin-specific for current vertical site/path work.
DECISION: REFERENCE

Rustwells
TYPE: Structure / vertical pit dungeon
ARGS: none
BEHAVIOR: Builds a large single pit/well using Pitted, adds patterned Fulcrete rings/spokes around it, carves extra underground voids with NoiseMap, places stairs up/down, firms pit edges, ensures void connectivity, and adds bottom-pool population at Z=13.
OBSERVATION: Specialized fixed-depth dungeon pattern. Useful as another reference for large vertical pit structures and pit-edge cleanup, but less relevant than Shug/QasQon/Rermadon because it is a named static dungeon and not path-oriented.
DECISION: REFERENCE ONLY

SaltDunes
TYPE: Surface terrain painter / biome decorator
ARGS: none
BEHAVIOR: Adds DaylightWidget, paints cells with salt dune/desert tiles, occasionally places HighSaltDune objects, then rebuilds reachability from (0,0).
OBSERVATION: Simple visual/terrain biome builder. Uses cosmetic randomness heavily. Not useful for underground sites except as a basic example of tile painting.
DECISION: IGNORE

ShugBurrow*Mouth
TYPE: Connection builders / path endpoints (vertical + horizontal)
ARGS: none (Range=3 for directional mouths)
BEHAVIOR: Ascending/Descending use ConnectionStart with type "ShugBurrow" + suffix. East/North/South/West use ConnectionMouth to create edge endpoints.
OBSERVATION: Slightly richer than River/Road because it includes vertical start points (ascending/descending), not just horizontal edges. Defines full 3D path endpoints.
DECISION: REFERENCE HIGH VALUE

ShugBurrowBuilder
TYPE: Path / tunnel builder (3D burrow system)
ARGS: Wall="BaseNephilimWall_Shug'rith", Pairs, HardClear
BEHAVIOR: Reads ShugBurrow connections (including ascending/descending), pathfinds between them, clears terrain in a radius to form tunnels, replaces walls with themed material, decorates sparsely, and creates special vertical features:
- Ascending: clears/open cavern
- Descending: places LazyPit (downward connection), blocks stairs with blockers
Applies ShugruithTunnel template at end. :contentReference[oaicite:0]{index=0}
OBSERVATION: This is the most directly relevant builder so far:
- uses connection system (your core mechanic)
- renders visible traversable paths
- supports vertical transitions explicitly
- separates endpoint definition (mouths) from rendering (builder)
DECISION: REFERENCE CRITICAL

ShugLair
TYPE: Structure / vertical Girsh lair
ARGS: l (lair level/depth index)
BEHAVIOR: Builds Shug'ruith lair level with cyst-like wall clusters, sludge pits via Pitted, ShugruithTunnel template, FirmPitEdges, connectivity cleanup, cyst populations, and boss placement when l == 3.
OBSERVATION: Complements ShugBurrowBuilder but is more “site/boss lair” than “path system.” Useful pattern: level index controls complexity and final boss placement. MutateObject gives cultists burrowing, trail-leaving, icon color, and adjective.
DECISION: REFERENCE

SixDayTents
TYPE: Structure / multi-zone settlement generator
ARGS: Noise flag
BEHAVIOR: Defines fixed path segments per world-grid position, builds paths, generates InfluenceMap regions, places tents/fences/fields/populations per region using directional templates, and ensures reachability.
OBSERVATION: Complex handcrafted settlement system (Stilt). Mixes fixed layout + region-based placement + population tables. Not procedural in the way you need—more content scripting than generative system design. :contentReference[oaicite:0]{index=0}
DECISION: IGNORE

Sky
TYPE: Surface/environment filler
ARGS: none
BEHAVIOR: Adds DaylightWidget, fills every cell with Air, rebuilds reachability from (0,0).
OBSERVATION: Simple environment builder. No structure, path, population, or useful reusable logic.
DECISION: IGNORE

SlimePools
TYPE: Environmental feature / noise-based liquid placement
ARGS: none
BEHAVIOR: Generates a NoiseMap (seeded partly by zone connections), places SlimePuddle on cells where noise > threshold and cell is empty.
OBSERVATION: Simple example of noise-driven feature placement. Connections slightly influence layout via ExtraNodes. Not structurally important.
DECISION: IGNORE

SmokingArea*
TYPE: Special encounter / directional hazard setup
ARGS: none
BEHAVIOR: Randomly samples 600 cells; if a sampled cell has liquid and it is not blood, mixes blood into that liquid. Then places three directional Smokecaster objects along one map edge, aimed inward.
OBSERVATION: Hardcoded directional hazard/ambush setup. Interesting only as an example of liquid mutation + fixed edge emitters. Not reusable for current site/path system.
DECISION: IGNORE

SnapjawFortMaker / SnapjawStockadeMaker
TYPE: Structure wrappers (preset builders)
ARGS: none (hardcoded FortMaker / StockadeMaker params)
BEHAVIOR: Thin wrappers that call FortMaker or StockadeMaker with Snapjaw-specific materials and globals.
OBSERVATION: Same pattern as other “*Maker” classes — just themed presets over generic structure systems. No new mechanics.
DECISION: IGNORE

SolidEarth
TYPE: Terrain filler / reset
ARGS: none
BEHAVIOR: Clears every cell and fills the entire zone with Shale.
OBSERVATION: Hard reset to solid rock. Likely used as a base layer before carving (e.g., tunnels/pits). Simple but conceptually important as a “blank slate” initializer.
DECISION: REFERENCE (low)

Spire
TYPE: Structure / vertical ruin template generator
ARGS: ZonesWide="1", ZonesHigh="1", RuinLevel computed
BEHAVIOR: Creates or reuses a SpireZoneTemplate stored on the zone column, generates rooms based on depth, retries until connections are valid, builds the template, applies Ruiner with decreasing ruin level by depth, then rebuilds reachability from stairs or first open cell.
OBSERVATION: Similar to Ruins but spire-specific. Important pattern: shared column template + per-depth room generation + connection validation retry loop. More relevant as reference architecture than direct reuse.
DECISION: REFERENCE

StairConnector
TYPE: Connectivity fixer / stair path carver
ARGS: none
BEHAVIOR: Finds StairsUp and StairsDown, pathfinds between them using Drillbot, clears walls and impassable objects along the path.
OBSERVATION: Simple and potentially useful utility. Ensures vertical traversal is possible after generation. Less sophisticated than ForceConnections, but very direct.
DECISION: REFERENCE

StairsDown / StairsUp
TYPE: Vertical connection builders
ARGS:
- Number
- x, y coordinate/range specs
- Reachable
- StairsDown also has EmptyOnly
BEHAVIOR: Places stair objects, respects existing ZoneManager stair connections, avoids conflicting stair/pit/blocker objects, chooses reachable/empty cells when possible, and caches reciprocal zone connections:
- StairsDown caches "d" -> StairsUp
- StairsUp caches "u" -> StairsDown
Also caches local "Connection" marker.
OBSERVATION: Core vertical traversal utility. Important: stairs are not just objects; they also create cached zone connections to the adjacent Z-level. This explains why later builders can read ZoneConnections and why forced connectivity can target them.
DECISION: REFERENCE HIGH VALUE

RuinPowerGrids
TYPE: Infrastructure post-processor / damaged utility networks
ARGS: DamageChance, missing producer/consumer chances/counts for electrical/hydraulic/mechanical systems, HydraulicLiquid/Table, PreferWalls, AvoidWalls
BEHAVIOR: Rolls shared damage chance, then applies PowerGrid, Hydraulics, and MechanicalPower builders with optional missing producers/consumers. Rebuilds reachability afterward.
OBSERVATION: Wrapper/orchestrator for ruined infrastructure systems. Too specialized unless making technological ruin variants.
DECISION: IGNORE / MAYBE LATER FOR TECH RUINS


Ruins
TYPE: Structure / multi-zone ruin generator
ARGS: RuinLevel=100, ZonesWide="1d3", ZonesHigh="1d2"
BEHAVIOR: Creates or reuses a BuildingZoneTemplate stored as a zone-column property, assigns semantic tags to cells from the template map, adds ruin semantic tags from population tables, builds the template, then applies Ruiner destruction and rebuilds reachability.
OBSERVATION: Important pattern: shared zone-column template lets multiple Z-level zones in the same column use consistent ruin structure/semantics. Uses semantic tags heavily for later replacement/decoration. Strong system, but probably too building/ruin-specific for current vertical site/path work.
DECISION: REFERENCE
Key note: this is another example of shared generated structure stored at column scope, unlike your preferred deterministic recomputation model. Useful pattern, but not necessary yet.

Rustwells
TYPE: Structure / vertical pit dungeon
ARGS: none
BEHAVIOR: Builds a large single pit/well using Pitted, adds patterned Fulcrete rings/spokes around it, carves extra underground voids with NoiseMap, places stairs up/down, firms pit edges, ensures void connectivity, and adds bottom-pool population at Z=13.
OBSERVATION: Specialized fixed-depth dungeon pattern. Useful as another reference for large vertical pit structures and pit-edge cleanup, but less relevant than Shug/QasQon/Rermadon because it is a named static dungeon and not path-oriented.
DECISION: REFERENCE ONLY
Important small note:
 

ShugBurrow*Mouth
TYPE: Connection builders / path endpoints (vertical + horizontal)
ARGS: none (Range=3 for directional mouths)
BEHAVIOR: Ascending/Descending use ConnectionStart with type "ShugBurrow" + suffix. East/North/South/West use ConnectionMouth to create edge endpoints.
OBSERVATION: Slightly richer than River/Road because it includes vertical start points (ascending/descending), not just horizontal edges. Defines full 3D path endpoints.
DECISION: REFERENCE HIGH VALUE
ShugBurrowBuilder
TYPE: Path / tunnel builder (3D burrow system)
ARGS: Wall="BaseNephilimWall_Shug'rith", Pairs, HardClear
BEHAVIOR: Reads ShugBurrow connections (including ascending/descending), pathfinds between them, clears terrain in a radius to form tunnels, replaces walls with themed material, decorates sparsely, and creates special vertical features:
- Ascending: clears/open cavern
- Descending: places LazyPit (downward connection), blocks stairs with blockers
Applies ShugruithTunnel template at end. :contentReference[oaicite:0]{index=0}
OBSERVATION: This is the most directly relevant builder so far:
- uses connection system (your core mechanic)
- renders visible traversable paths
- supports vertical transitions explicitly
- separates endpoint definition (mouths) from rendering (builder)
DECISION: REFERENCE CRITICAL
One important clarification (this is worth locking in)
You now have the full pattern:

1) Mouth builders → define endpoints (deterministic, cross-zone)
2) Builder (ShugBurrowBuilder) → renders the path locally
This is exactly what you are building.

Why this is better than River/Road for you
Shug adds:

+ vertical connections (ascending / descending)
+ tunnel geometry (not just surface path)
+ environmental theming
+ controlled stair behavior (LazyPit + blockers)
That maps almost perfectly to:

ShugLair
TYPE: Structure / vertical Girsh lair
ARGS: l (lair level/depth index)
BEHAVIOR: Builds Shug'ruith lair level with cyst-like wall clusters, sludge pits via Pitted, ShugruithTunnel template, FirmPitEdges, connectivity cleanup, cyst populations, and boss placement when l == 3.
OBSERVATION: Complements ShugBurrowBuilder but is more “site/boss lair” than “path system.” Useful pattern: level index controls complexity and final boss placement. MutateObject gives cultists burrowing, trail-leaving, icon color, and adjective.
DECISION: REFERENCE

SixDayTents
TYPE: Structure / multi-zone settlement generator
ARGS: Noise flag
BEHAVIOR: Defines fixed path segments per world-grid position, builds paths, generates InfluenceMap regions, places tents/fences/fields/populations per region using directional templates, and ensures reachability.
OBSERVATION: Complex handcrafted settlement system (Stilt). Mixes fixed layout + region-based placement + population tables. Not procedural in the way you need—more content scripting than generative system design. :contentReference[oaicite:0]{index=0}
DECISION: IGNORE
Quick sanity note (worth stating once)
You’re now seeing two distinct categories very clearly:

Sky
TYPE: Surface/environment filler
ARGS: none
BEHAVIOR: Adds DaylightWidget, fills every cell with Air, rebuilds reachability from (0,0).
OBSERVATION: Simple environment builder. No structure, path, population, or useful reusable logic.
DECISION: IGNORE

SlimePools
TYPE: Environmental feature / noise-based liquid placement
ARGS: none
BEHAVIOR: Generates a NoiseMap (seeded partly by zone connections), places SlimePuddle on cells where noise > threshold and cell is empty.
OBSERVATION: Simple example of noise-driven feature placement. Connections slightly influence layout via ExtraNodes. Not structurally important.
DECISION: IGNORE
 

SmokingArea*
TYPE: Special encounter / directional hazard setup
ARGS: none
BEHAVIOR: Randomly samples 600 cells; if a sampled cell has liquid and it is not blood, mixes blood into that liquid. Then places three directional Smokecaster objects along one map edge, aimed inward.
OBSERVATION: Hardcoded directional hazard/ambush setup. Interesting only as an example of liquid mutation + fixed edge emitters. Not reusable for current site/path system.
DECISION: IGNORE

SnapjawFortMaker / SnapjawStockadeMaker
TYPE: Structure wrappers (preset builders)
ARGS: none (hardcoded FortMaker / StockadeMaker params)
BEHAVIOR: Thin wrappers that call FortMaker or StockadeMaker with Snapjaw-specific materials and globals.
OBSERVATION: Same pattern as other “*Maker” classes — just themed presets over generic structure systems. No new mechanics.
DECISION: IGNORE

Spire
TYPE: Structure / vertical ruin template generator
ARGS: ZonesWide="1", ZonesHigh="1", RuinLevel computed
BEHAVIOR: Creates or reuses a SpireZoneTemplate stored on the zone column, generates rooms based on depth, retries until connections are valid, builds the template, applies Ruiner with decreasing ruin level by depth, then rebuilds reachability from stairs or first open cell.
OBSERVATION: Similar to Ruins but spire-specific. Important pattern: shared column template + per-depth room generation + connection validation retry loop. More relevant as reference architecture than direct reuse.
DECISION: REFERENCE
Small note:

Useful pattern: template.EnsureConnections(Z) retry loop before committing build.

StairConnector
TYPE: Connectivity fixer / stair path carver
ARGS: none
BEHAVIOR: Finds StairsUp and StairsDown, pathfinds between them using Drillbot, clears walls and impassable objects along the path.
OBSERVATION: Simple and potentially useful utility. Ensures vertical traversal is possible after generation. Less sophisticated than ForceConnections, but very direct.
DECISION: REFERENCE
Useful later if a generated site level has both stairs but the builder leaves them disconnected.

StairsDown / StairsUp
TYPE: Vertical connection builders
ARGS:
- Number
- x, y coordinate/range specs
- Reachable
- StairsDown also has EmptyOnly
BEHAVIOR: Places stair objects, respects existing ZoneManager stair connections, avoids conflicting stair/pit/blocker objects, chooses reachable/empty cells when possible, and caches reciprocal zone connections:
- StairsDown caches "d" -> StairsUp
- StairsUp caches "u" -> StairsDown
Also caches local "Connection" marker.
OBSERVATION: Core vertical traversal utility. Important: stairs are not just objects; they also create cached zone connections to the adjacent Z-level. This explains why later builders can read ZoneConnections and why forced connectivity can target them.
DECISION: REFERENCE HIGH VALUE
Important practical note:
If you manually place custom stairs/pits later, you need to think about both:
1) the visible object in the cell
2) the cached zone connection
This is directly relevant to your vertical site design.

StarappleFarm / StarappleFarmMaker
TYPE: Structure / surface settlement-farm generator
ARGS: ClearCombatObjectsFirst, WallObject, ZoneTable, Widgets (mostly unused here)
BEHAVIOR: Generates 1–3 large farm boxes, clears/
DECISION: SKIP

Stillvine
TYPE: Biome/environment decorator
ARGS: Underground flag
BEHAVIOR: Adds daylight/dirty or pale dirty, creates noise maps, uses world-seeded large-scale WatervineNoise, places SaltyWaterPuddle and Stillvine objects based on noise. Underground mode only decorates passable cells.
OBSERVATION: Relevant pattern: cached world-scale noise creates continuity across zones. Otherwise just biome decoration.
DECISION: REFERENCE LOW

StockadeMaker
TYPE: Structure (compound builder)
BEHAVIOR:
- Builds walled compound
- Subdivides into rooms
- Populates rooms (can include leader/loot)
- Cuts entrances + arrowslits
- Ensures connectivity
OBSERVATION:
Standard “site pattern”: enclosure → rooms → populate → fix paths
DECISION:
REFERENCE (not needed now)

Strata
TYPE: Full terrain + connectivity builder
CATEGORY: Generic subterranean cave/strata generator
BEHAVIOR:
- Selects primary and secondary wall/material types by depth, weighted tables, and world-seeded noise.
- Sets zone DefaultWall to the strongest selected material.
- Chooses cave layout algorithm based on selected material:
  - Oolite/Marl: box-filter cave
  - Black Marble: pillars
  - Halite/Gypsum/Porous Coral Rag: porous
  - Limestone/Coral Rag: random-walk cave
  - Serpentinite/Sandstone: windy
  - Quartzite: blocky
  - fallback: cellular cave
- Blends primary/secondary layouts using zone-scale noise.
- Places walls/detail liquids/materials and paints floor colors from selected wall materials.
- Uses a static world-seeded 3D Maze3D to add cross-zone cave connections.
- Carves tunnels between connection points using FindPath + Drillbot.
- Optionally places stairs up/down when maze cell has U/D links.
- Sometimes replaces the zone with a SultanDungeon-style pocket, then adds ForceConnections if stairs are enabled.
ARGS:
- Noise: declared, not obviously central here
- Stairs=true: controls whether U/D maze links create StairsUp/StairsDown and ForceConnections
OBSERVATION:
- This is very similar to Reef, but for generic underground strata rather than palladium reef biome.
- Very important reference for how Qud’s normal deep underground terrain is built.
- Confirms a high-value pattern:
  world seed + named noise key + global coordinates + depth → stable local terrain variation.
- Also confirms another high-value pattern:
  global 3D maze → local edge/stair connections → local path carving.
- Not a good direct builder for your special sites because it is the normal background cave generator.
- Useful to understand what your path/site system will be interrupting or overlaying.
- `Stairs=false` could be relevant if testing terrain generation without extra vertical links.
DECISION:
REFERENCE HIGH VALUE


## ZoneBuilder Evaluation – SultanDungeon / SultanDungeonArgs

SultanDungeon
Type: FULL DUNGEON GENERATOR
Behavior:
- generates structured dungeon layouts using SultanDungeonArgs
- supports random historic-entity-driven generation
- supports explicit argument-driven generation via BuildRandomZoneWithArgs
- uses segmentation, WFC templates, walls, floors, objects, furnishings, halls, cubbies, encounters, and preconnect encounters
- connects regions internally
- places stairs when requested
- runs ForceConnections after generation
- can optionally place relics
Observation:
- likely the main flexible generator for historic-style sites
- much more general than BasicLair
- BasicLair is effectively a thin wrapper around SultanDungeon with the Lairs_ prefix
- SultanDungeons_ prefix should expose the broader thematic adjective system
- good candidate for generating major site levels
Decision:
TEST / HIGH PRIORITY
Notes:
- most likely builder for “real” subterranean sites
- use as site generator, not path generator
- test explicit arguments first using BuildRandomZoneWithArgs
- compare SultanDungeons_ behavior to limited Lairs_ behavior
- keep path system separate and let path lead to SultanDungeon-generated sites

---

SultanDungeonArgs
Type: ARGUMENT / THEME RESOLUTION SYSTEM
Behavior:
- translates adjectives/properties into internal theme names
- checks for matching population tables under a table prefix
- fills lists for walls, templates, objects, segmentation, furnishings, halls, cubbies, encounters, and preconnect encounters
- falls back to default tables when no matching theme table exists
- can mutate template choices to increase variation
Observation:
- same argument system used by BasicLair
- power depends heavily on tablePrefix
- Lairs_ support was vendor/workshop-limited
- SultanDungeons_ is likely much broader and worth testing
Decision:
REFERENCE / TEST WITH SULTANDUNGEON
Notes:
- test known adjective themes like temple, soldier, tinker, stars, waste, market, residential
- verify which SultanDungeons_ tables actually exist before heavy testing
- this may become the main theme-control layer for major sites

SurfaceCave
TYPE: Terrain builder
CATEGORY: Simple surface cave / noise terrain
BEHAVIOR:
- Adds DaylightWidget.
- Generates a NoiseMap.
- Places Sandstone on cells where noise > 1.
- Clears and rebuilds reachability from (0,0).
- Returns true only if reachable area is at least 400 cells.
ARGS:
- none
OBSERVATION:
- Simple terrain decorator/carver compared with Cave or Strata.
- Surface-oriented because it adds daylight.
- No path endpoint system, no vertical behavior, no population.
- Not useful for your subterranean path/site system except as a basic NoiseMap example.
DECISION:
IGNORE

Tarland
TYPE: Terrain builder
CATEGORY: Surface biome / cellular terrain
BEHAVIOR:
- Generates two CellularGrids with different seed chances (55 and 45).
- For each cell:
  - If grid1 == open → place AsphaltPuddle (tar-like terrain).
  - Else if grid2 == open → place Shale.
- Leaves remaining cells unchanged (implicit base terrain).
- Adds Dirty at (0,0).
- Does NOT enforce reachability.
ARGS:
- none
OBSERVATION:
- Layered cellular approach → produces patchy tar + rock regions.
- No connectivity guarantees → may produce fragmented zones.
- Purely cosmetic/terrain shaping; no paths, stairs, or structure logic.
- Good minimal example of combining multiple cellular fields for biome variation.
DECISION:
IGNORE (but minor reference for layered cellular terrain patterns)

TileBuilding / TileManager / TileType
TYPE: Structure / tile-template building generator
CATEGORY: Modular prefab-like structure system
BEHAVIOR:
- Loads 6x6 building tiles from BuildingTiles.txt and modded BuildingTiles files.
- TileManager can add mirrored, reversed, and rotated variants of each tile.
- TileBuilding assembles a larger building from compatible tiles.
- Supports marker ranges for selecting tile sets:
  - TileStartMarker / TileEndMarker
  - VaultStartMarker / VaultEndMarker
  - second vault set also supported
- Can insert special vault regions before filling the rest of the tile grid.
- Enforces edge compatibility between neighboring tiles.
- Converts tile symbols into TileType values:
  - Open
  - OpenConnect
  - LittleChest
  - BigChest
  - Creature1
  - Wall
  - Door
  - Liquid
  - Garbage
  - Any
  - Custom
- Builds actual zone content from tile types:
  - WallMaterial for walls
  - Door
  - Locker2 / MedLocker
  - Boosterbot
  - ConvalessencePuddle / RedTile
  - MedScrap
  - SpawnBlocker for vault regions
- Supports CustomCharMap for mapping custom template chars to objects or wall clearing.
- Optional Shell adds hollow outer boxes around the generated building.
- Clears local cached connection cells.
- Runs ForceConnections and Connecter after building.
ARGS:
- WallMaterial
- ShellMaterial
- Shell
- Wide / High
- XCorner / YCorner
- TileStartMarker / TileEndMarker
- NumberOfVaults / ChancePerVault / VaultWidth / VaultHeight / VaultStartMarker / VaultEndMarker
- NumberOfVaults2 / ChancePerVault2 / VaultWidth2 / VaultHeight2 / VaultStartMarker2 / VaultEndMarker2
- CustomCharMap
OBSERVATION:
- This is a serious modular building generator, not just a decorator.
- It is closer to wave-function/tile assembly than simple room generation.
- Useful pattern: template assembly → content interpretation → connectivity cleanup.
- It is probably too specialized and tile-file-dependent for the current subterranean path/site system.
- Could become useful later for fixed “constructed” site interiors, vaults, ruins, or tech structures.
- The post-build ForceConnections + Connecter pass is another confirmation that Qud builders often generate first and repair connectivity afterward.
DECISION:
REFERENCE (later structured/constructed sites)

Torchposts
TYPE: Decorator / path-adjacent object placement
CATEGORY: Lighting / trail decoration
BEHAVIOR:
- Picks a start point:
  - prefers existing StairsUp or StairsDown if found
  - otherwise starts near a random north/south edge
- Pathfinds from that start point to a random east/west-ish edge target.
- Walks along the found path.
- Every 3–5 steps, places two Torchpost objects on nearby adjacent cells.
ARGS:
- none
OBSERVATION:
- Does not carve the path; it only decorates along an existing/pathfindable route.
- Interesting small pattern for placing repeated markers along a route.
- Could be adapted later for path-side signage, lights, bones, cairns, resin nodules, etc.
- Not core path generation.
DECISION:
REFERENCE LOW / MAYBE DECORATOR LATER

TunnelMaker
TYPE: Utility / abstract tunnel path generator
CATEGORY: Path topology support
BEHAVIOR:
- Creates a 2D string map representing tunnel connectivity.
- Constructor takes:
  - Width
  - Height
  - StartY roll string
  - EndY roll string
  - Directions string
- Tunnel always runs horizontally from one side to the other:
  - default: west → east
  - if Directions contains "W": east → west
- Randomly steps through allowed Directions until it reaches EndX/EndY.
- Stores direction strings in each map cell:
  - current cell gets outgoing direction
  - destination cell gets opposite direction
- Retries up to 100 full attempts if it fails.
- Each step gets up to 10 direction attempts before abandoning that full attempt.
ARGS:
- Width
- Height
- StartY
- EndY
- Directions
OBSERVATION:
- This does not modify a Zone directly.
- It generates a symbolic tunnel route/map for another builder to interpret.
- Useful pattern for precomputing constrained paths before rendering them.
- Much simpler than FindPath-based builders.
- Could be relevant if you want deterministic “path plan first, render later” logic for your site paths.
- Limitation: it is grid/path-topology oriented and horizontally biased, not a full 3D path system.
DECISION:
REFERENCE

UpperBethesdaElevator
TYPE: Special structure
CATEGORY: Unique vertical transport
BEHAVIOR:
- Clears fixed area, places OpenShaft + ElevatorSwitch.
- Links two Z-levels via switch (TopLevel / FloorLevel).
- Adds Platform on top level.
- Ensures reachability.
OBSERVATION:
- Hardcoded Bethesda feature.
- Example of non-stair vertical travel.
DECISION:
IGNORE

VillageOver
TYPE: Special structure / village upper layer
CATEGORY: Aerie / above-ground village support
BEHAVIOR:
- Clears zone, fills with Air, adds DaylightWidget.
- Looks for cached/current ZoneConnections of type "aerie".
- Around each aerie point, places WFC building/platform floor pieces.
- Replaces the aerie point with StairsDown marked IdleStairs.
- Sets relaxedbiomes=true.
OBSERVATION:
- Specific support layer for aerie-style villages.
- Interesting because it uses connection points as anchors for structures.
- Not useful directly, but relevant pattern: connection → local structure/platform.
DECISION:
REFERENCE LOW

VillageUnder
TYPE: Special structure / village lower layer
CATEGORY: Burrow / underground village support
BEHAVIOR:
- Sets DisableForcedConnections and relaxedbiomes.
- Clears combat objects.
- Finds cached/current connections whose type starts with "burrow".
- For each burrow point:
  - clears a box around it
  - builds InfluenceMap regions around burrow anchors
  - walls borders with local wall material
  - populates building interiors from associated population table
  - places StairsUp marked IdleStairs at burrow anchor.
- Later applies village faction/abandoned/infrastructure logic.
OBSERVATION:
- More relevant than VillageOver because it is underground and anchor-driven.
- Uses burrow connections as underground dwelling anchors.
- Still village-specific, but confirms a useful pattern:
  connection point → clear local chamber → regionize → populate → add vertical return.
DECISION:
REFERENCE

VillageBase / Village
TYPE: Full settlement framework
CATEGORY: Dynamic village generation
BEHAVIOR:
- Generates village identity, faction, villagers, mayor, warden, merchants, domesticated animals, conversations, signature food/liquid/items, buildings, roads, farms, gardens, ponds, huts, tents, burrows, aeries, infrastructure, music, checkpoint, and discovery widgets.
- Uses HistoricEntitySnapshot heavily.
- Uses InfluenceMap regions and PopulationLayout to place buildings/content.
- Can call RiverBuilder/RoadBuilder if present.
- Adds PowerGrid, Hydraulics, MechanicalPower with damage/missing component chances.
- Handles abandoned villages, ruins, replacement of original plants/creatures/furniture/items.
OBSERVATION:
- Huge content framework, not a simple reusable builder.
- Valuable only architecturally:
  - region → building layout
  - village identity → population/content
  - generate first → cleanup/connectivity/infrastructure after
- Too tied to village history/factions/conversations for your current subterranean site work.
DECISION:
REFERENCE LOW / IGNORE DIRECT

VillageCodaBase / VillageCoda
TYPE: Full settlement framework variant
CATEGORY: Coda-specific village system
BEHAVIOR:
- Variant of Village/VillageBase for Coda quest/state logic.
- Similar village framework but with Coda-specific game states, plague/ruin/despised flags, guaranteed merchant/tinker/apothecary chances, Coda history/event strings, and quest-specific content.
- Uses eligible villagers/faction members and similar villager preprocessing/conversation systems.
OBSERVATION:
- Even more quest-specific than Village.
- Not relevant to procedural path/site generation.
- Only useful as another example of how Qud forks large builder frameworks for special narrative contexts.
DECISION:
IGNORE

VillageMaker
TYPE: Structure / simple village preset builder
CATEGORY: Huts + roads + optional feature
BEHAVIOR:
- Optionally clears combat objects.
- Places a rolled number of huts.
- Avoids placing huts too close to existing connection points.
- Adds hut doors to a road-point list.
- Uses RoadBuilder to connect hut doors / connections.
- Can add feature such as cistern.
- Places hut contents and optional zone/global populations/widgets.
ARGS:
- bRoads
- WallObject
- RoundBuildings
- Huts
- Features
- HutTable
- ZoneTable
- Widgets
- ClearCombatObjectsFirst
OBSERVATION:
- Much simpler than full Village.
- Useful pattern: place small structures → collect door points → RoadBuilder connects them.
- Still surface/village-oriented.
DECISION:
REFERENCE LOW

VillageOutskirts
TYPE: Structure / dynamic village outskirts
CATEGORY: Village extension / partial settlement
BEHAVIOR:
- Extends VillageBase.
- Resolves building contents through dynamic semantic tables.
- Builds initial structures using SultanDungeon-style segments:
  Full, mirrored full, BSP, rings, blocks, circles, towers, etc.
- Uses village tech tier and population semantics for content.
- Handles farms/gardens/outskirt structures and village-flavored placement.
OBSERVATION:
- Interesting hybrid of VillageBase + SultanDungeon segment concepts.
- Too village-specific, but it reinforces that Qud reuses Sultan-style region segmentation outside dungeons.
DECISION:
REFERENCE LOW

WallOutcrop
TYPE: Terrain / obstruction generator
CATEGORY: Noise-based terrain feature
BEHAVIOR:
- Uses two NoiseMaps:
  - Local 20×20 cluster near an anchor (often stairs)
  - Global spread across entire zone
- Places specified Blueprint (wall type) where noise > threshold.
- If StairsDown exists:
  - anchors outcrop around it
  - preserves stairs and forces reachability
- If no stairs:
  - attempts to place StairsDown in reachable empty cell near center
  - falls back to anywhere if needed
- Rebuilds reachability aggressively (multi-pass search)
ARGS:
- Blueprint (wall object to place, e.g., Shale, etc.)
OBSERVATION:
- Dual-scale noise = clustered feature + ambient scatter.
- Strong pattern:
  terrain generation → then enforce connectivity → then fix/insert stairs.
- Good example of “generate first, fix playability after”.
DECISION:
REFERENCE

Waterlogged
TYPE: Terrain modifier
CATEGORY: Environmental effect
BEHAVIOR:
- Adds water/liquid to existing terrain (likely via noise or conditions).
- Converts areas into wet/marsh-like zones.
OBSERVATION:
- Post-process modifier rather than structural generator.
DECISION:
IGNORE
Watervine / Stillvine (combined pattern)
TYPE: Terrain + global noise system
CATEGORY: Large-scale biome generator
BEHAVIOR:
- Uses large precomputed global noise (1200×375 space).
- Combines:
  - local NoiseMap
  - global Perlin noise
- Places:
  - water puddles
  - plants (Stillvine / Watervine)
- Anchors to world coordinates → consistent large-scale biome patterns.
OBSERVATION:
- Important pattern: global noise tied to world coordinates.
- Enables biome continuity across zones.
- Not needed for your current path/site system, but useful conceptually.
DECISION:
REFERENCE LOW

Watervine / Stillvine (combined pattern)
TYPE: Terrain + global noise system
CATEGORY: Large-scale biome generator
BEHAVIOR:
- Uses large precomputed global noise (1200×375 space).
- Combines:
  - local NoiseMap
  - global Perlin noise
- Places:
  - water puddles
  - plants (Stillvine / Watervine)
- Anchors to world coordinates → consistent large-scale biome patterns.
OBSERVATION:
- Important pattern: global noise tied to world coordinates.
- Enables biome continuity across zones.
- Not needed for your current path/site system, but useful conceptually.
DECISION:
REFERENCE LOW

Waterway
TYPE: Connection / terrain feature
CATEGORY: Linear water path
BEHAVIOR:
- Likely similar to RiverBuilder but simpler.
- Creates water-based connection across zone.
OBSERVATION:
- Another example of “connection builder” pattern.
DECISION:
REFERENCE LOW

WideHive
TYPE: Structure / hive variant
CATEGORY: Creature lair
BEHAVIOR:
- Variant of Hive builder with wider/open layout.
- Likely uses region + population + tunnels.
OBSERVATION:
- Same pattern as other lairs (Girsh, Qonqas, etc.).
DECISION:
REFERENCE LOW

WildWatervineMerchant
TYPE: Encounter / population injector
CATEGORY: Special NPC placement
BEHAVIOR:
- Spawns merchant tied to Watervine biome.
OBSERVATION:
- Pure population logic, no structural value.
DECISION:
IGNORE

Weald
TYPE: Terrain / biome painter
CATEGORY: Overworld vegetation
BEHAVIOR:
- Uses NoiseMap to scatter vegetation/terrain (dense plant biome).
- Fills cells with plant objects depending on noise thresholds.
- Likely preserves connectivity or relies on base terrain being passable.
OBSERVATION:
- Standard biome painter.
- Not structurally interesting for dungeon/site work.
DECISION:
IGNORE

ZoneBuilderSandbox
TYPE: Core utility base class / builder helper library
CATEGORY: Foundational methods used by many builders
BEHAVIOR:
Provides shared helper methods for:
- deterministic seeded values
- path carving
- void/region connectivity
- WFC building templates
- hut/rect construction
- object/population placement
- placement hints
- bridges over pits
- zone-column properties
- terrain lookup / default wall lookup
KEY METHODS / PATTERNS:
- getMatchedEdgeConnectionLocation(...)
  - deterministic edge connection placement across neighboring zones.
- TunnelTo(...)
  - pathfinds between two points and clears walls along route.
- EnsureAllVoidsConnected(...)
  - finds separated open regions and carves connections between them.
- EnsureCellReachable(...)
  - carves from a target cell to nearest reachable area.
- GetOracleIntColumn(...)
  - deterministic per-column value using world seed + zone column.
- GetOracleLocationForZone(...)
  - deterministic per-zone location.
- GetSeededRand / GetSeededRange(...)
  - world-seeded randomness by string key.
- BuildPath / BuildPathWithObject / BuildSimplePathWithObject(...)
  - reusable path-rendering helpers.
- PlacePopulationInRegion / Rect / Cells(...)
  - population placement with zonetier.
- PlaceObjectInArea(...)
  - major placement engine using hints like AlongWall, Center, Aquatic, LivesOnWalls, Adjacent, Border, Nonborder, etc.
- BridgeOver(...)
  - places bridges over pit-like objects.
- getWfcBuildingTemplate(...)
  - generates and caches WFC building chunks.
OBSERVATION:
This is absolutely core. It is less a “builder” and more the shared toolbox many builders rely on. For your mod, the most important pieces are:
- deterministic oracle helpers
- TunnelTo
- EnsureAllVoidsConnected
- EnsureCellReachable
- BuildPathWithObject / BuildSimplePathWithObject
- PlacePopulationInRegion
- BridgeOver
This also confirms that your deterministic matrix/site plan is aligned with existing Qud patterns: the engine already uses world-seeded helper methods for stable per-zone/per-column decisions. :contentReference[oaicite:0]{index=0}
DECISION:
REFERENCE CRITICAL