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