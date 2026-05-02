# Spike Log

## Runtime Injection Discovery
- AddZoneBuilder(...) does not affect zones during runtime
- BeforeZoneBuiltEvent fires early enough to modify zones
- ZoneManager.ApplyBuilderToZone(...) successfully applies builders

Conclusion:
Runtime builder application must be done via ApplyBuilderToZone

---

## IGameSystem Behavior
- Systems are NOT auto-registered
- Must call:
  The.Game.RequireSystem<T>()

---

## Event Timing
- ZoneActivatedEvent → too late
- BeforeZoneBuiltEvent → correct timing for mutation

---

## Builder Types Identified
1. Full builders
   - SnapjawStockadeMaker (works)

2. Decorators
   - Mines2 (adds lava/features)

3. Context-dependent
   - SultanDungeon (requires setup pipeline)

---

## Zone Naming Test
- SetZoneName works in both worldgen and runtime hooks

---

## Current Capability
- Deterministic runtime injection into specific underground zone
- Verified for at least one builder

## ZoneBuilder DEfinition Tests from XLR.World.ZoneBuilders

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
Behavior: noise-based trees; supports underground mode; overlays terrain
Decision: TEST PASS (overlay layer candidate)

Test 1:
- Direct builder call (Underground = true)
- Applied during BeforeZoneBuiltEvent
- No clearing (overlay test)
Result:
- Grove generated successfully underground
- Overlaid on existing cave terrain
- No guaranteed pathing; may block access without digging

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