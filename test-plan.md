# Subterranean Sites — Test Plan

## Purpose

This test plan tracks functional, safety, determinism, and release-readiness testing for Subterranean Sites.

Primary release requirement:

```text
The mod must not overwrite or damage important vanilla generated content.
```

If protected-location safety initialization fails, generation should fail closed.

---

## 1. Runtime System / Bootstrap Tests

### 1A. Genesis bootstrap

Scenario:

```text
Mod installed before creating a new world.
```

Expected:

* `JoppaWorldBuilderExtension.OnAfterBuild(...)` runs.
* `RuntimeZoneBuilderInjectionSystem` is required.
* Safety initialization runs.
* Site/path generation proceeds only if safety passes.

Status:

```text
Passed
```

### 1B. Existing-save bootstrap

Scenario:

```text
Mod installed into an existing save.
```

Expected:

* `[CallAfterGameLoaded]` bootstrap runs on save load.
* `RuntimeZoneBuilderInjectionSystem` is required.
* Safety initialization runs.
* Site/path generation proceeds only if safety passes.

Status:

```text
Passed
```

Notes:

* Existing saves are supported.
* Already-built zones may not retroactively receive generated content, but this is acceptable.

---

## 2. Runtime Pre-Registration Tests

Goal:

Confirm generated content is registered before the player reaches it.

Expected:

* `ZoneManager.AddZoneBuilder(...)` registers builders for future zones.
* Future zones build correctly when entered.
* Current-zone direct building is not required.
* Generated zones persist after re-entry.

Status:

```text
Passed
```

Known constraint:

```text
AddZoneBuilder(...) does not affect a zone already in the current build pipeline.
```

---

## 3. Determinism Tests

Goal:

Confirm stable mod-level decisions from stable inputs.

Tested inputs:

* world seed
* matrix ID
* site slot ID
* `XRLCore.Core.Game.GetWorldSeed(...)`
* `System.Random(seed)`

Expected:

* Same world seed + same matrix/slot input gives same site decision.
* Same matrix/slot input gives same archetype, origin, layer count, and path definition.
* Processed matrices do not duplicate generation.

Status:

```text
Passed
```

---

## 4. Site Archetype Functional Tests

### 4A. SultanHistoric

Expected:

* Multi-layer SultanDungeon site generates.
* Sultan/history/region data are reused successfully.
* Cult mobs appear.
* Cult social-role text appears.
* Bottom vault/relic behavior works.
* Cult leader/hero behavior works.
* Relic chest appears with tier-appropriate relic.

Status:

```text
Passed
```

### 4B. ProperLair

Expected:

* Tier-appropriate lair owner table is selected.
* Lair population is coherent.
* Extra heroes can spawn.
* Reward chest appears.
* Upgraded/rare chest blueprint works.

Status:

```text
Passed
```

### 4C. BasicLairChaos

Expected:

* BasicLair layout generates.
* Tiered singles/team tables populate.
* Extra faction encounters can spawn.
* Reward chests can spawn.
* `SubterraneanSiteMobs` builder remains present and functional.

Status:

```text
Passed
```

### 4D. MerchantHive / Underworld Bazaar

Expected:

* Multi-layer merchant site generates.
* Merchants use tier-appropriate behavior/stock.
* Supporting NPCs appear.
* Site feels distinct from hostile dungeon archetypes.

Status:

```text
Passed
```

---

## 5. Matrix Generation Tests

Current design:

* Matrix size: `4 x 5 x 5`

  * 4 parasangs wide
  * 5 parasangs tall
  * 5 Z-levels deep
* Each matrix attempts 4 deterministic site slots:

  * A: upper-left
  * B: upper-right
  * C: lower-left
  * D: lower-right
* Each slot selects an origin within its assigned region.
* Slot regions overlap across the middle Y band.
* Protection and ownership checks resolve slot conflicts.
* Matrix status is stored in game state.
* Processed matrices do not generate again.

Activation design:

* On zone activation, the current matrix is identified.
* A surrounding `3 x 3 x 3` matrix block is checked.
* Invalid world-edge or above-surface matrices are skipped.
* Each unprocessed valid matrix attempts its four site slots.

Expected:

* Current and nearby matrices are seeded before exploration reaches them.
* Matrix processing is deterministic.
* Matrix processing does not duplicate content.
* Matrix edge behavior is acceptable; edge areas have lower density but remain playable.

Status:

```text
Passed
```

Notes:

* The map-edge density drop is expected because fewer surrounding matrices are valid.
* Joppa/start surface edge correctly produced fewer activation popups than central locations.

---

## 6. Path Functional Tests

Current design:

* Path length target: `30–40` zones.
* Paths start at the site origin.
* Paths are generated as ordered zone IDs.
* Path instructions derive entry/exit directions from adjacent zone IDs.
* Path registration uses visible path materials only.
* Path candidate generation rejects protected or already-owned candidate zones before committing.
* Final path filtering remains as a safety backstop.

Path ascent tuning:

* Shallow origins use low upward bias to avoid immediate surface termination.
* Deeper origins use stronger upward bias to create upward drift without making paths into elevators.

Current tested behavior:

* Paths are discoverable mid-route.
* Paths can continue after protected-zone candidate rejection.
* Paths avoid protected vanilla content when protection is present.
* Vertical transitions are usable and visible.
* Coral/FungalTrailBrick was removed after rendering failures.

Status:

```text
Passed
```

---

## 7. Runtime Safety Initialization Tests

Goal:

Confirm required protected-location sources are available before generation.

Expected:

* `EnsureSafetyReady()` runs before generation.
* If safety initialization fails, no content is registered.
* One player-facing failure popup is shown if safety fails.

Tested sources:

### 7A. Vanilla lairs / legendary merchant lairs

Source:

```text
The.Game.GetObjectGameState("JoppaWorldInfo")
reflected field: lairs
```

Expected:

* Recover vanilla lairs.
* Convert lairs to protected Z columns.
* Spot-check coordinates in game.

Status:

```text
Passed
```

### 7B. Historical sites

Source:

```text
SultanDungeonPlacementOrder_i
sultanRegionPosition_[regionName]
```

Expected:

* Recover historical-site parasang positions.
* Protect historical site center-zone columns.

Status:

```text
Passed
```

### 7C. Named special lairs

Source:

```text
JournalAPI map-note secrets
```

Targets:

* Oboroqoru
* Qas/Qon
* Rermadon
* Shug’ruith mouth
* Shug’ruith lair/cradle

Expected:

* Recover map-note zone IDs.
* Protect exact zone columns or parasang columns as appropriate.

Status:

```text
Passed
```

---

## 8. Collision Safety Tests

Goal:

Confirm site/path registration skips or rejects protected vanilla content.

Expected behavior:

* Site origins are rerolled if their site stack intersects protected content.
* `RegisterLayeredSite(...)` checks protection again.
* Path candidate generation rejects protected/owned zones before committing.
* Protected path instructions are removed before registration if any survive.
* `RegisterRoadPathZone(...)` checks protection again before registering a path builder.
* If a protected location blocks site generation, the slot fails safely.

Priority targets:

* vanilla lairs
* legendary merchant lairs
* vanilla historical sites
* named special lairs
* story/special-builder zones
* Waterlogged Tunnel
* existing-save generation edge cases

Status:

```text
Passed
```

Notes:

* Protected zones were avoided reliably in testing.
* Two historical sites were checked; paths avoided them and continued.
* When a zone is correctly protected, the system appears robust.
* Remaining risk is incomplete or incorrect protected-location coverage, not the avoidance mechanism itself.

---

## 9. Shug’ruith Path Review

Goal:

Evaluate risk of interfering with Shug’ruith’s unusual mouth-to-cradle path.

Findings:

* Shug’ruith mouth and cradle/lair areas are protected.
* Protected Shug’ruith zones were avoided successfully.
* The mouth was not necessarily centered in its parasang.
* Mod paths may appear near Shug resin/holes.
* In observed testing, Shug’s path remained followable.

Known residual risk:

```text
Intermediate Shug path zones are not fully enumerated/protected.
A generated site or path could theoretically interfere with an intermediate route segment or vertical transition.
```

Status:

```text
Acceptable for release with known-issue note.
```

Rationale:

* The protected endpoints are preserved.
* Observed interference was understandable and navigable.
* Exact route recovery would require additional code and may not be worth the release risk.
* This should be documented as an extremely rare edge case.

---

## 10. Encounter Density Test

Goal:

Estimate how far a player may need to search before encountering a Subterranean Sites path or site.

Current tested setup:

* Matrix size: `4 x 5 x 5`
* 4 site slots per matrix
* `3 x 3 x 3` surrounding matrix activation
* Path length target: `30–40` zones
* Protected-aware path candidate rejection enabled
* Coral/FungalTrailBrick removed from path materials

Grand total across tests:

```text
n = 51
mean = 9.65 zones
sample SD = 10.28 zones
median = 6 zones
min = 1
max = 47
```

Encounter thresholds:

```text
≤ 5 zones:  49.0%
≤ 10 zones: 68.6%
≤ 15 zones: 78.4%
≤ 20 zones: 84.3%
≤ 30 zones: 94.1%
```

Interpretation:

* Typical encounter distance is acceptable.
* Long dry searches can occur.
* Searches over 10 zones are possible.
* Searches over 15 zones are poor but observed.
* Density is acceptable for release testing.
* Additional paths per site are not recommended.

Status:

```text
Passed / accepted for release tuning
```

---

## 11. Discovery Tests

Goal:

Confirm site discovery popup behavior.

Expected:

* Entering any generated site layer can trigger discovery.
* All site layers share one discovery key.
* Discovery popup appears once per site.
* Entering another layer of the same site does not repeat the popup.

Status:

```text
Passed after update
```

---

## 12. Release Cleanup Tests

Before release:

* Debug zone renaming disabled.
* Matrix debug popup disabled.
* Safety failure popup retained.
* Coral/FungalTrailBrick removed from path materials.
* `manifest.json` created and read by the game.
* Preview image added and read by the game.
* README / brief / decisions / spike log / test plan updated.
* New-game smoke test passed.
* Existing-save smoke test passed.
* Protected-zone collision tests passed.

Status:

```text
Release candidate
```

---

## 13. Remaining Suggested Smoke Tests

These are not blockers unless failures are observed.

* Start several new games and confirm no build/load errors.
* Install on one more existing save and confirm safe generation.
* Enter several non-origin site layers and confirm discovery works once.
* Visit map-edge regions and confirm lower density is acceptable.
* Spot-check visible path materials.
* Keep an eye on Shug’ruith reports after release.

---

## 14. Current Status Summary

Passed:

* genesis bootstrap
* existing-save bootstrap
* runtime pre-registration
* deterministic matrix/slot generation
* quad-slot matrix generation
* `3 x 3 x 3` neighbor matrix activation
* all four site archetypes
* protected-aware path generation
* surface/depth path ascent tuning
* runtime lair recovery from `JoppaWorldInfo.lairs`
* historical-site protection
* named special-site protection
* protected-zone collision avoidance
* site discovery from any site layer
* manifest/preview image loading

Known residual risks:

* Rare possible interference with intermediate Shug’ruith path segments.
* Existing saves may have already-built zones that do not retroactively receive content.
* Encounter density has a long tail; players may occasionally search more than 10–15 zones.

Release posture:

```text
Code appears ready for release-candidate testing.
Remaining work is documentation, art, Workshop setup, and post-release observation.
```
