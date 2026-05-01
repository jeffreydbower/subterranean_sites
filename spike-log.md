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