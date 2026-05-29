# Subterranean Sites — Spike Log

## Purpose

This spike log records the main development discoveries behind Subterranean Sites.

It is not a full test plan. It exists to preserve why the current architecture works, what was learned along the way, and what future-me or other modders should know before changing the system.

---

## Runtime Injection / Registration

Early testing confirmed that `BeforeZoneBuiltEvent` can modify zone metadata and is useful for diagnostics.

Important finding:

```text
ZoneManager.AddZoneBuilder(...) does not affect the zone already in the current build pipeline.
It does work for future zones that have not yet been built.
```

Conclusion:

* Runtime pre-registration is the correct architecture.
* Generated sites and paths should be registered before the player reaches them.
* Direct current-zone mutation should not be the normal generation path.

---

## IGameSystem / Bootstrap Discovery

Initial finding:

* `IGameSystem` classes are not automatically added to a save.
* The runtime system must be explicitly required with:

```csharp
The.Game.RequireSystem<RuntimeZoneBuilderInjectionSystem>()
```

Genesis bootstrap:

```text
JoppaWorldBuilderExtension.OnAfterBuild(...)
→ RequireSystem<RuntimeZoneBuilderInjectionSystem>()
→ EnsureSafetyReady()
```

This works for new worlds.

Retrofit bootstrap discovery:

* `XRLGame.LoadGame(...)` calls `ModManager.CallAfterGameLoaded()`.
* Methods marked with `[HasCallAfterGameLoaded]` and `[CallAfterGameLoaded]` can run after save load.

Retrofit bootstrap:

```text
CallAfterGameLoaded
→ RequireSystem<RuntimeZoneBuilderInjectionSystem>()
→ EnsureSafetyReady()
```

Conclusion:

* New games and existing saves use separate bootstrap paths.
* Both paths require the same runtime system.
* Both paths use the same safety initialization gate.

---

## Event Timing

Findings:

* `BeforeZoneBuiltEvent` is useful for debug/metadata work during zone build.
* `ZoneActivatedEvent` is too late to alter the already-built active zone.
* `ZoneActivatedEvent` is useful as the player-facing trigger point.

Current use:

* `BeforeZoneBuiltEvent`

  * optional debug zone naming only
* `ZoneActivatedEvent`

  * safety gate
  * matrix activation
  * site discovery popup

Conclusion:

* The active-zone event should trigger registration of nearby future zones, not attempt to rewrite the current zone.

---

## Deterministic RNG Spike

Goal:

Confirm stable string inputs can produce repeatable mod decisions.

Test setup:

* Used stable zone/matrix-like string inputs.
* Generated seeds with `XRLCore.Core.Game.GetWorldSeed(...)`.
* Created `System.Random(seed)`.
* Compared output across new games.

Result:

* Same world seed + same string input produced repeatable mod decisions.

Conclusion:

* Matrix and slot generation can be deterministic.
* Current architecture uses world seed + matrix ID + slot ID to make stable generation decisions.

Important limitation:

* This only controls the mod registration layer.
* Vanilla builders may still have their own internal randomness/layout behavior.

---

## BasicLair / Proper Lair Spikes

Goal:

Determine whether vanilla lair systems can support vertical underground sites.

Results:

* Multi-layer BasicLair-style sites work.
* Stairs can be controlled per layer.
* Tiered population tables load from the mod folder.
* Team/packet-style population tables feel more Qud-like than flat individual spawns.
* Proper lair generation can use tier-appropriate lair-owner logic.
* Extra heroes and upgraded reward chests work.
* Rare chest blueprints work as visible rewards.

Conclusion:

* `BasicLair` is usable as a chaotic/combat archetype.
* `ProperLair` is usable as a coherent lair archetype.
* Vanilla table/owner logic should be reused where practical.

---

## SultanDungeon / Historical Site Spike

Goal:

Reuse Qud’s `SultanDungeon` system for additional historical-site-like underground sites.

Key discovery:

`SultanDungeon` requires a matching game-state object:

```csharp
The.Game.SetObjectGameState("sultanDungeonArgs_" + regionName, args)
```

Then each layer can register:

```csharp
The.ZoneManager.AddZoneBuilder(
    zoneId,
    6000,
    "SultanDungeon",
    "locationName", locationName,
    "regionName", regionName,
    "stairs", stairs
);
```

Working approach:

* Reuse existing generated sultan history.
* Reuse existing generated historical region snapshots.
* Build `SultanDungeonArgs` from sultan/region snapshots.
* Store args under a mod-specific region key.
* Register `SultanDungeon` across vertical layers.

Results:

* Multi-layer historical-site-like dungeons generate.
* WFC/template structures appear.
* Cult mobs appear.
* Cult social-role text appears.
* Bottom vault/relic behavior works.
* Cult leader/hero behavior works.
* Relic chest and tier-appropriate relic placement work.

Conclusion:

* `SultanDungeon` is a working site archetype.
* The archetype is represented as `SultanHistoric`.

---

## Merchant Hive / Underworld Bazaar Spike

Goal:

Create a merchant-focused underground site.

Results:

* Multi-layer merchant-heavy sites work.
* Tier-appropriate merchant generation works.
* Legendary/vendor-style gameplay is viable as a rare site type.
* BasicLair can provide workshop/lair layout support without hostile population.

Conclusion:

* `MerchantHive` is a working archetype.
* It should remain a rarer site type.

---

## Chaos Lair Spike

Goal:

Create a less coherent combat site with mixed encounters.

Results:

* `BasicLair` can provide base structure.
* Custom tiered singles/team population tables can add density.
* Extra faction encounters work.
* Reward chests work.
* Required custom builder `SubterraneanSiteMobs` must remain available even if IDE references look sparse.

Conclusion:

* `BasicLairChaos` is a working archetype.
* It provides useful variation from coherent lairs and historical sites.

---

## Path System Spike

Goal:

Create deterministic paths leading outward/upward from generated sites.

Results:

* Path zone IDs can be generated deterministically.
* Path instructions can describe entry/exit directions.
* Custom path builder can draw visible path material.
* Path builder can place vertical transitions.
* Path material selection works.
* Path system is separate from site archetype logic.

Important later discovery:

* Strong upward bias made shallow paths terminate at the surface too quickly.
* Lower shallow up-weight preserved path length.
* Deeper origins can tolerate stronger upward bias.

Current path model:

* Path length target is approximately 30–40 zones.
* Shallow origins use low upward bias.
* Deeper origins use stronger upward bias.
* Path candidate generation checks protected/owned zones before committing.
* Final path filtering remains as a safety backstop.

Conclusion:

* Paths are central to discovery.
* Candidate-level protection is better than generating blindly and removing blocked path zones afterward.

---

## Path Material Spike

Finding:

* Some floor/path materials are not readable enough underground.
* `FungalTrailBrick` displays as “coral path” but did not visibly render as expected in testing.
* Associated features appeared, but the visible path did not.

Conclusion:

* Path readability matters more than material variety.
* `FungalTrailBrick`, `FoamcreteFloor`, and other low-visibility candidates should stay out unless manually verified.

---

## Runtime Safety Spike

Problem:

The mod can register builders into arbitrary world locations. This creates a risk of overwriting vanilla generated content.

Design rule:

```text
Never overwrite important vanilla content.
If safety data is unavailable, generate nothing.
```

Safety architecture:

* Static protected locations remain hardcoded.
* Dynamic protected locations are recovered from runtime game state.
* All protection sources feed `SubterraneanSafety.IsProtected(...)`.

Enforcement points:

* Origin picker rejects protected site stacks.
* Site stack is checked again before registration.
* `RegisterLayeredSite(...)` checks again.
* Path candidates reject protected/owned zones before committing.
* Path instructions are filtered again before registration.
* `RegisterRoadPathZone(...)` checks again.

Conclusion:

* Safety should be redundant and fail closed.
* When a protected zone is correctly identified, avoidance is reliable.

---

## Vanilla Lair Protection Spike

Problem:

Earlier lair protection used `builder.worldInfo.lairs`, which only works during genesis/worldgen.

Retrofit requirement:

* Existing saves need the same protection.
* Therefore lair data must be recovered from persistent runtime state.

Discovery:

* `The.Game.GetObjectGameState("JoppaWorldInfo")` exists in loaded saves.
* It contains a reflected `lairs` field.
* `JoppaWorldInfo.lairs` contains vanilla lairs.

Test results:

* Runtime reflection recovered lairs.
* Coordinates were spot-checked in game and confirmed.
* Legendary merchant lairs were included.

Conclusion:

* Vanilla lair and legendary merchant protection can work in both new games and existing saves.
* Use `JoppaWorldInfo.lairs` instead of transient `builder.worldInfo.lairs`.

Current protection:

```text
each vanilla lair = protected zone column, Z 10–14
```

---

## Historical Site Protection Spike

Goal:

Protect vanilla historical sites from site/path collision.

Source:

```text
SultanDungeonPlacementOrder_i
sultanRegionPosition_[regionName]
```

Finding:

* Historical site region positions are stored in persistent game state.
* Position objects expose `x` and `y` fields.
* Historical sites can be protected as center-zone columns.

Conclusion:

* Historical site protection works from runtime game state.
* No need to reconstruct historical sites from broader world-info tables.

---

## Named Special Site Protection Spike

Goal:

Protect important named special lairs and Girsh-related locations.

Source:

```text
JournalAPI.GetMapNote(secretId)
```

Protected targets:

* Oboroqoru
* Qas/Qon
* Rermadon
* Shug’ruith mouth
* Shug’ruith lair/cradle

Finding:

* Map-note secrets provide persistent zone IDs.
* Some locations should be protected as exact zone columns.
* Some should be protected as broader parasang columns.

Conclusion:

* JournalAPI map notes are the right source for named special-site protection.

---

## Shug’ruith Path Spike

Concern:

Shug’ruith’s mouth and cradle are protected, but the intermediate route is unusual and may not be fully represented as a simple persistent coordinate list.

Findings:

* The mouth and cradle/lair are recoverable from secrets.
* The builder route appears to be created through zone builders and zone connection data.
* The path itself may not be easy to recover as one explicit route list.
* During testing, protected mouth/cradle areas were avoided.
* Mod paths appeared near Shug resin/holes but did not prevent following the route.

Conclusion:

* Protecting the endpoints works.
* Exact intermediate-path protection is not currently implemented.
* This remains a rare known edge case rather than a release blocker.

---

## Matrix System Spike

Original model:

```text
underground space → deterministic 3D matrices
one site at most per matrix
world seed + matrix ID → site definition
```

Later density testing showed one site per large matrix felt too sparse.

Final model:

```text
processing matrix → 4 deterministic site slots
site slot → one origin opportunity
zone activation → current/surrounding matrices are processed
```

Current dimensions:

```text
4 × 5 parasangs
5 Z-levels deep
```

Current site-slot layout:

* A: upper-left
* B: upper-right
* C: lower-left
* D: lower-right
* Top/bottom slot pairs share a middle Y band.
* Protection/ownership checks resolve conflicts.

Activation model:

```text
On zone activation:
    identify current matrix
    process valid matrices in a 3 × 3 × 3 block around it
    skip already-processed matrices
```

Conclusion:

* Matrix = processing/status unit.
* Slot = site-origin opportunity unit.
* Path = discovery/advertising unit.
* This separation works and keeps the system understandable.

---

## Encounter Density Spike

Problem:

Even when many paths exist, the player can miss them because underground visibility is limited and paths are linear.

Density changes made:

* Four site slots per matrix.
* Surrounding `3 x 3 x 3` matrix activation.
* Path length kept around 30–40 zones.
* No second path per site.

Findings:

* Median encounter distance was acceptable.
* Long dry streaks can still happen.
* Map edges are naturally lower density because fewer surrounding matrices are valid.
* Generating a second path per site would likely feel artificial.

Conclusion:

* Current density is acceptable.
* Do not add more paths per site.
* Communicate that searches over 10–15 zones can happen.

---

## Discovery Popup Spike

Original issue:

* Discovery popup only triggered on the site origin layer.
* If a player dug into a non-origin layer first, no popup appeared.

Fix:

* All site layers receive:

  * `SubterraneanSites_IsSiteLayer`
  * `SubterraneanSites_SiteDisplayName`
  * shared `SubterraneanSites_DiscoveryKey`
* Origin layer still receives the origin marker.

Conclusion:

* Any site layer can trigger discovery.
* Shared discovery key prevents repeated popups.

---

## Code Organization Spike

Current organization:

* `RuntimeZoneBuilderInjectionSystem`

  * runtime hooks
  * matrix activation
  * site selection
  * shared registration helpers
  * path registration
  * safety gate

* `SubterraneanDynamicProtectedLocations`

  * dynamic protected-location recovery
  * lair/historical/special protection checks

* `SubterraneanProtectedLocations`

  * static protected locations

* Site registrar classes

  * archetype-specific registration logic

* `SubterraneanPathCoordinateGenerator`

  * deterministic path coordinate generation

* `SubterraneanPathBuilder`

  * path drawing and vertical transition placement

Decision:

* Keep shared registration helpers in the runtime system for now.
* Keep archetype-specific logic in registrar classes.
* Keep path coordinate generation separate from path drawing.

---

## Manifest / Preview Image Spike

Finding:

* Qud uses `manifest.json` at the mod root for Mod Manager display.
* Documented keys include `ID`, `Title`, `Description`, `Tags`, `Version`, `Author`, and `PreviewImage`.
* Keys are case-insensitive, but documented casing is clearer.
* Preview image path must match the file name exactly.

Conclusion:

* Use a release-facing `manifest.json`.
* Keep preview image in the mod root unless a relative subfolder is specified.
* Use the Steam display name in `Author`; source repo can carry real-name attribution.

---

## Crash / GPU Note

A prior crash during testing was associated with system/GPU instability rather than a clear mod fault.

Summary:

* BSOD: `SYSTEM_THREAD_EXCEPTION_NOT_HANDLED / 0x7E`
* Faulting module: `nvlddmkm.sys`
* Same mod scenario later loaded successfully.

Conclusion:

* Track recurrence, but current interpretation is GPU/driver instability, not confirmed mod fault.

---

## Current Status

Working:

* genesis bootstrap
* existing-save bootstrap
* runtime pre-registration
* deterministic matrix/slot decisions
* four vertical site archetypes
* quad-slot matrix generation
* surrounding `3 x 3 x 3` matrix activation
* path coordinate generation
* path builder
* path candidate protection/rerouting
* dynamic safety initialization
* vanilla lair protection from `JoppaWorldInfo.lairs`
* historical site protection
* named special site protection
* any-layer site discovery
* manifest and preview image loading

Known residual risks:

* Rare possible interference with intermediate Shug’ruith path segments.
* Existing saves may have already-built zones that do not retroactively receive content.
* Encounter density has a long tail; players may occasionally search more than 10–15 zones.

Current release posture:

```text
Release candidate code is committed.
Remaining work is documentation, Workshop setup, and broader playtesting.
```
