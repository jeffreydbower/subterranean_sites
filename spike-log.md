Spike Log

Runtime Injection Discovery
- AddZoneBuilder(...) does not affect zones during runtime
- BeforeZoneBuiltEvent fires early enough to modify zones
- ZoneManager.ApplyBuilderToZone(...) successfully applies builders

Conclusion:
- Runtime builder application must be done via ApplyBuilderToZone inside BeforeZoneBuiltEvent



IGameSystem Behavior
- Systems are NOT auto-registered
- Must call:
  The.Game.RequireSystem<T>()

Conclusion:
- Runtime systems must be explicitly registered during world initialization



Event Timing
- ZoneActivatedEvent → too late (zone already built)
- BeforeZoneBuiltEvent → correct timing for mutation

Conclusion:
- All procedural injection must occur in BeforeZoneBuiltEvent



Builder Types Identified
1. Full builders
   - SnapjawStockadeMaker (confirmed working)

2. Decorators
   - Mines2 (adds features, does not define full layout)

3. Context-dependent
   - SultanDungeon (requires setup pipeline and arguments)



Zone Naming Test
- The.ZoneManager.SetZoneName(...) works in runtime hooks
- Useful for debugging deterministic behavior



Current Capability
- Deterministic runtime injection into specific underground zones
- Verified for at least one full builder (BasicLair / SnapjawStockadeMaker)
- Zone modifications persist after re-entry



ZoneBuilder Definition Tests
- Moved to: Spike-Log_ZoneBuilderTestCatalog.md



Deterministic Zone RNG Test

Goal:
- Confirm that a stable string input produces repeatable deterministic results across separate new games

Test Setup:
- Targeted underground zones only (Z > 10)
- Used ZoneID as the stable input
- Generated seed using:
  XRLCore.Core.Game.GetWorldSeed(ZoneID + worldSeed)
- Created System.Random(seed)
- Rolled:
  - rollA = rng.Next(0, 2)
  - rollB = rng.Next(1, 101)
- Wrote seed and rolls into zone name
- Repeated test across two separate new games

Result:
- Same zones selected in both games
- Selected zones had identical generated names
- Out of 4 tested zones, the same 2 were selected in both runs

Conclusion:
- Deterministic selection and RNG behavior is confirmed when using stable inputs



Important Limitation
- This does NOT prove that BasicLair output is deterministic
- BasicLair uses Qud internal RNG
- Only the pre-builder decision layer is deterministic



Decision
- Deterministic RNG model is validated
- Future site generation should use:
  world seed + matrix ID

Preferred API:
- XRLCore.Core.Game.GetWorldSeed("SubterraneanSites:" + matrixId)



Notes
- Displayed game seed and GetWorldSeed() output are different formats
- GetWorldSeed() returns the internal numeric seed derived from the game seed
- GetStringGameState("WorldSeed") is not suitable
- Correct API:
  XRLCore.Core.Game.GetWorldSeed()
- World seed is constant across a given game