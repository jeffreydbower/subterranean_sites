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