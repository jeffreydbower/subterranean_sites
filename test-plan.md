# Subterranean Sites — Test Plan

## Purpose

This test plan tracks functional, safety, determinism, and release-readiness testing for Subterranean Sites.

The most important release requirement is:

    The mod must not overwrite or damage important vanilla generated content.

If safety initialization fails, generation should fail closed.

---

## 1. Runtime System / Bootstrap Tests

### 1A. Genesis bootstrap

Scenario:

    Mod installed before creating a new world.

Expected:

- `JoppaWorldBuilderExtension.OnAfterBuild(...)` runs.
- `RuntimeZoneBuilderInjectionSystem` is required.
- Safety initialization runs.
- Site/path generation can proceed if safety passes.

Status:

    Passed

### 1B. Retrofit bootstrap

Scenario:

    Mod installed into an existing save.

Expected:

- `[CallAfterGameLoaded]` bootstrap runs on save load.
- `RuntimeZoneBuilderInjectionSystem` is required.
- Safety initialization runs.
- Site/path generation can proceed if safety passes.

Status:

    Passed

Notes:

- This resolved the earlier new-world-only assumption.
- Existing saves are now supported in principle, pending collision testing.

---

## 2. Runtime Pre-Registration Tests

Goal:

Confirm that generated content is registered before the player reaches it.

Expected:

- `ZoneManager.AddZoneBuilder(...)` registers builders for future zones.
- Future zones build correctly when entered.
- Current-zone direct building is not required.
- Generated zones persist after re-entry.

Status:

    Passed

Known constraint:

    AddZoneBuilder(...) does not affect the zone already in the current build pipeline.

---

## 3. Determinism Tests

Goal:

Confirm stable mod-level decisions from stable inputs.

Tested:

- world seed
- zone ID / future matrix ID style inputs
- `XRLCore.Core.Game.GetWorldSeed(...)`
- `System.Random(seed)`

Expected:

- Same world seed + same input gives same site decision.
- Same matrix ID should eventually give same site definition.
- Site type, origin, layer count, and path definition should be deterministic.

Status:

    Basic deterministic RNG passed.

Still needed after matrix implementation:

- Same seed produces same matrix/site decisions.
- Same matrix ID produces same archetype.
- Same matrix ID produces same origin/layer count.
- Processed matrices do not duplicate generation.

---

## 4. Site Archetype Functional Tests

### 4A. SultanHistoric

Expected:

- Multi-layer SultanDungeon site generates.
- Sultan/history/region data are reused successfully.
- Cult mobs appear.
- Cult social-role text appears.
- Bottom vault/relic behavior works.
- Cult leader/hero behavior works.
- Relic chest appears with tier-appropriate relic.

Status:

    Passed

### 4B. ProperLair

Expected:

- Tier-appropriate lair owner table is selected.
- Lair population is coherent.
- Extra heroes can spawn.
- Reward chest appears.
- Upgraded/rare chest blueprint works.

Status:

    Passed

### 4C. BasicLairChaos

Expected:

- BasicLair layout generates.
- Tiered singles/team tables populate.
- Extra faction encounters can spawn.
- Reward chests can spawn.
- `SubterraneanSiteMobs` builder remains present and functional.

Status:

    Passed

### 4D. MerchantHive / Underworld Bazaar

Expected:

- Multi-layer merchant site generates.
- Merchants use tier-appropriate behavior/stock.
- Guards or equivalent supporting NPCs appear.
- Site feels distinct from hostile dungeon archetypes.

Status:

    Passed

---

## 5. Path Functional Tests

Current status:

    Prototype working

Expected current behavior:

- Path zone IDs generate from site origin.
- Path instructions contain entry/exit directions.
- Path builder draws visible path material.
- Vertical transitions can place holes/pits.
- Path zones pass through safety checks before registration.

Still needed:

- Tie path generation to matrix/site definitions.
- Clip paths at matrix boundaries.
- Confirm paths remain discoverable mid-route.
- Confirm path holes/vertical transitions are usable and visible.
- Confirm paths do not overwrite protected content.

---

## 6. Runtime Safety Initialization Tests

Goal:

Confirm required protected-location sources are available before generation.

Expected:

- `EnsureSafetyReady()` runs before generation.
- If safety init fails, no content is registered.
- Future release behavior should show one player-facing failure popup.

Tested sources:

### 6A. Vanilla lairs / legendary merchant lairs

Source:

    The.Game.GetObjectGameState("JoppaWorldInfo")
    reflected field: lairs

Expected:

- Recover 125 lairs.
- Convert lairs to protected Z columns.
- Spot-check coordinates in game.

Status:

    Passed

### 6B. Historical sites

Source:

    SultanDungeonPlacementOrder_i
    sultanRegionPosition_[regionName]

Expected:

- Recover historical-site parasang positions.
- Protect historical site center-zone columns.

Status:

    Passed / previously verified

### 6C. Named special lairs

Source:

    JournalAPI map-note secrets

Targets:

- Oboroqoru
- Qas/Qon
- Rermadon
- Shug’ruith mouth
- Shug’ruith lair

Expected:

- Recover map-note zone IDs.
- Protect exact zone columns or parasang columns as appropriate.

Status:

    Passed / previously verified

---

## 7. Collision Safety Tests

Goal:

Confirm site/path registration skips or rejects protected vanilla content.

General expected behavior:

- Protected site zones are removed before registration.
- `RegisterLayeredSite(...)` checks protection again.
- Protected path instructions are removed before registration.
- `RegisterRoadPathZone(...)` checks protection again.
- If all critical site zones are protected, site generation should skip safely.

Priority targets:

- vanilla lairs
- legendary merchant lairs
- vanilla historical sites
- named special lairs
- Girsh-related locations
- villages / settlements if needed
- known story or special-builder zones
- Waterlogged Tunnel
- current-zone late-registration edge case

Status:

    Partially tested.
    More collision tests required after matrix implementation.

---

## 8. Matrix System Tests

Status:

    Upcoming

Expected behavior:

- Player zone converts to matrix ID.
- Current matrix is processed once.
- Processed matrix is recorded in game state.
- Matrix site definition is deterministic.
- Matrix origin avoids horizontal edges.
- Surface-containing matrix only places origins at safe depth.
- Origin leaves enough downward room for site layers.
- Current/adjacent matrices are registered before the player reaches generated content.
- Edge and corner logic process needed neighbors.
- Matrix paths are clipped at matrix boundaries.

Test categories:

### 8A. Basic matrix detection

- enter zones in same matrix
- confirm same matrix ID
- cross boundary
- confirm new matrix ID

### 8B. Process-once behavior

- enter same matrix repeatedly
- confirm generation does not duplicate

### 8C. Edge/corner behavior

- enter matrix interior
- enter matrix edge
- enter matrix corner
- confirm intended neighbor processing

### 8D. Origin constraints

- confirm no origin appears on matrix edge
- confirm no invalid shallow origin in surface matrix
- confirm enough Z-depth for full site

### 8E. Safety interaction

- force/test matrix origin near protected locations
- confirm protected zones are skipped or site generation fails safely

---

## 9. Retrofit Save Tests

Goal:

Confirm existing-save installs behave safely.

Test scenarios:

- install mod into an existing save near ordinary underground zones
- install mod into an existing save near known lairs
- install mod into an existing save near historical/special content if possible
- load/save/reload after runtime system has been added
- confirm system does not duplicate behavior after reload

Expected:

- retrofit bootstrap runs once per load as needed
- runtime system exists after load
- safety init succeeds or fails closed
- generated content does not collide with protected vanilla content

Status:

    Bootstrap passed.
    Full matrix/collision testing still needed.

---

## 10. Release Cleanup Tests

Before release:

- remove obsolete debug popups
- keep only intentional debug flags
- add one player-facing safety failure popup
- confirm no debug zone names appear unless debug flag is enabled
- confirm README / brief / decisions / spike log are current
- run genesis and retrofit smoke tests
- run repeated collision tests after matrix implementation

Expected release behavior:

- no unexplained debug popups
- no accidental zone renaming
- no generation when safety init fails
- no known collisions with protected vanilla content

---

## 11. Current Status Summary

Passed:

- runtime pre-registration
- genesis bootstrap
- retrofit bootstrap
- deterministic RNG proof of concept
- multiple vertical site archetypes
- path builder prototype
- runtime lair recovery from `JoppaWorldInfo.lairs`
- historical-site protection source
- named special-site protection source

Remaining critical tests:

- matrix detection
- matrix process-once behavior
- matrix edge/corner registration
- path clipping
- full collision safety testing
- retrofit collision safety testing
- release smoke tests