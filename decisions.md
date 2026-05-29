# Subterranean Sites — Design Decisions

## Runtime Generation Approach

Decision:

Use runtime pre-registration rather than direct mutation of the currently building zone.

Current model:

* `ZoneActivatedEvent` triggers matrix processing.
* `ZoneManager.AddZoneBuilder(...)` registers builders for future zones.
* Qud’s normal zone-building pipeline builds those zones when entered.
* `BeforeZoneBuiltEvent` remains available for lightweight diagnostics only.

Important constraint:

```text
AddZoneBuilder(...) works for future zones.
AddZoneBuilder(...) does not affect the current zone already in the build pipeline.
```

Status:

```text
Direct runtime injection → deprecated as primary approach
Runtime pre-registration → primary architecture
Direct BuildZone/application → diagnostic/fallback only
```

---

## Runtime System Bootstrap

Decision:

Support both genesis and existing-save installs.

Definitions:

```text
Genesis       = mod installed before a new world is created
Existing save = mod installed into an already-created save
```

Genesis bootstrap:

```text
JoppaWorldBuilderExtension.OnAfterBuild(...)
→ RequireSystem<RuntimeZoneBuilderInjectionSystem>()
→ EnsureSafetyReady()
```

Existing-save bootstrap:

```text
[HasCallAfterGameLoaded]
[CallAfterGameLoaded]
→ RequireSystem<RuntimeZoneBuilderInjectionSystem>()
→ EnsureSafetyReady()
```

Rationale:

* `OnAfterBuild(...)` is valid for world-generation flow.
* Existing saves do not re-run world generation.
* `CallAfterGameLoaded` provides a load-time bootstrap for existing-save installs.

---

## Safety Initialization Gate

Decision:

No site or path generation may occur unless dynamic protected-location safety initializes successfully.

Current rule:

```text
If EnsureSafetyReady() fails:
    register nothing
```

Rationale:

* The mod can register builders into arbitrary generated world locations.
* Missing protection data could cause collisions with vanilla generated content.
* Failing closed is safer than generating unsafely.

Release behavior:

```text
If safety init fails:
    show one player-facing warning popup
    disable generation for that save/session
```

---

## Protected-Location Source Model

Decision:

Use multiple source adapters feeding one protection system.

Unified check:

```text
SubterraneanSafety.IsProtected(...)
```

Current protected sources:

* hardcoded static protected locations
* historical sites from persistent sultan-region game state
* vanilla lairs / legendary merchant lairs from `JoppaWorldInfo.lairs`
* named special/Girsh-related sites from `JournalAPI` map-note secrets

Rationale:

* Qud persists different generated systems in different ways.
* Forcing all safety into one source would be less robust.
* The unified layer is the protection check, not the data source.

---

## Vanilla Lair Protection

Decision:

Recover vanilla lair locations from persistent runtime world state.

Implementation:

```text
The.Game.GetObjectGameState("JoppaWorldInfo")
→ reflected field: lairs
→ GeneratedLocationInfo.zoneLocation
```

Protection:

```text
Each vanilla lair = protected zone column, Z 10–14
```

Rationale:

* Earlier `builder.worldInfo.lairs` capture worked only during genesis.
* `JoppaWorldInfo.lairs` persists into existing saves.
* This supports both new games and existing saves.

---

## Historical Site Protection

Decision:

Use persistent sultan-region game state for historical site protection.

Source:

```text
SultanDungeonPlacementOrder_i
sultanRegionPosition_[regionName]
```

Protection:

```text
Historical sites = protected center-zone columns across expected historical-site depth.
```

Rationale:

* This source is specific to historical site placement.
* It is available at runtime.
* It does not require reconstructing historical sites from broader world-info tables.

---

## Named Special Site Protection

Decision:

Use `JournalAPI` map-note secrets for specific named special lairs.

Protected targets include:

* Oboroqoru
* Qas/Qon
* Rermadon
* Shug’ruith mouth
* Shug’ruith lair/cradle

Rationale:

* These are known named locations with persistent map-note identifiers.
* Some need exact zone-column protection.
* Others need broader parasang-column protection.

---

## Shug’ruith Intermediate Path

Decision:

Protect Shug’ruith’s mouth and lair/cradle, but do not currently attempt to reconstruct and protect the full intermediate route.

Rationale:

* The mouth and cradle/lair are recoverable and protected.
* The intermediate route appears to be generated through builder/connection behavior rather than a simple persistent route list.
* Overbuilding a speculative line or bounding shape could protect too much territory or create new bugs.
* Testing found the route remained followable despite nearby mod paths.

Release stance:

```text
Rare known edge case.
Document honestly.
Patch later only if real reports show the route can be meaningfully broken.
```

---

## Redundant Safety Enforcement

Decision:

Protect both sites and paths at multiple stages.

Current enforcement:

* Origin picker rejects protected site stacks.
* Site stack is checked again before registration.
* `RegisterLayeredSite(...)` checks protection again.
* Path candidate generation rejects protected/owned zones before committing.
* Final path filtering remains as a safety backstop.
* `RegisterRoadPathZone(...)` checks protection again.

Rationale:

```text
Safety should fail closed.
Redundant checks are acceptable because collision damage would be worse than skipped/partial generation.
```

---

## Builder Strategy

Decision:

Prefer vanilla builders where possible.

Working site-builder families:

* `SultanDungeon`
* `BasicLair`
* vanilla lair-owner generation
* merchant/lair population systems
* small custom support builders for path material, holes, and targeted features

Rationale:

* Vanilla builders produce Qud-native layouts and encounters.
* Reusing vanilla systems reduces custom content burden.
* Custom builders are appropriate for connective/path features and targeted additions.

---

## Site Structure

Decision:

Sites are vertical structures, not broad horizontal 3×3 parasang structures.

Current structure:

```text
same X/Y
varying Z
typically 3–6 layers
```

Rationale:

* Qud already uses vertical dungeon structure naturally.
* Vertical sites are easier to register deterministically.
* Paths provide broader spatial discovery without requiring horizontal site sprawl.

---

## Site Archetypes

Decision:

Support multiple deterministic site archetypes.

Current working archetypes:

* `SultanHistoric`
* `ProperLair`
* `BasicLairChaos`
* `MerchantHive` / Underworld Bazaar

Site selection:

```text
world seed + matrix ID + slot ID
→ deterministic weighted site type
```

Rationale:

* Archetype variety makes repeated discoveries more interesting.
* Each archetype can reuse different vanilla systems.
* The selector can be tuned without changing matrix architecture.

---

## Determinism

Decision:

Site identity should be deterministic, but vanilla builder internals do not need to be perfectly controlled.

Required deterministic inputs:

```text
world seed + matrix ID + slot ID
```

Controls:

* site existence
* site origin
* site type
* layer count
* path direction tendency
* path material
* reward/site parameters

Important distinction:

```text
Builder RNG may vary internally.
The mod’s registration decisions should remain deterministic.
```

---

## Matrix-Based Procedural Generation

Decision:

Use a direct matrix system as the processing/status unit.

Current model:

```text
World underground space is divided into 3D matrices.
Each matrix contains four deterministic site slots.
Each slot may attempt one site.
```

Current matrix dimensions:

```text
4 × 5 parasangs
5 Z-levels deep
```

Current matrix behavior:

* detect current matrix from player zone
* process each matrix once
* store matrix status in game state
* attempt four site slots per matrix
* generate each slot from matrix seed + slot ID
* use safety checks to reject blocked origins/site stacks
* allow site/path content to extend outside the matrix if safe

Rationale:

* A single site per large matrix felt too sparse.
* Four slots per matrix provides better density while retaining structure.
* Matrix remains the status/processing unit.
* Slot remains the site-origin opportunity unit.

---

## Matrix Slot Layout

Decision:

Divide each processing matrix into four origin-selection slots.

Slots:

```text
A = upper-left
B = upper-right
C = lower-left
D = lower-right
```

Design:

* Slot bounds constrain origin selection only.
* Site stacks and paths may extend outside slot bounds if safe.
* Top and bottom slot pairs share a middle Y band.
* Ownership/protection checks resolve conflicts.

Rationale:

* The overlapping middle band avoids awkward unused rows.
* Slot conflicts are acceptable because the safety/ownership system is authoritative.
* This gives more discoverable content without making the matrix system too complex.

---

## Neighbor Matrix Activation

Decision:

On zone activation, process a surrounding `3 × 3 × 3` block of matrices.

Current behavior:

```text
current matrix
+ adjacent horizontal matrices
+ diagonal horizontal matrices
+ one matrix band above
+ one matrix band below
```

Invalid matrices are skipped:

* outside JoppaWorld
* outside world-map bounds
* above the surface matrix band

Rationale:

* A player may enter an already-processed matrix from a new side.
* Nearby matrices should be seeded before the player reaches them.
* Matrix status prevents duplicate processing.
* Broader activation improves path/site encounter density without adding extra paths per site.

---

## Navigation System

Decision:

Do not use compass, attunement, map markers, or explicit directional feedback.

Replacement:

```text
Natural path discovery
```

Rationale:

* Paths are more Qud-like than UI guidance.
* Players can discover paths mid-route.
* Physical paths make deep sites feel integrated into the underground world.

---

## Path Generation

Decision:

Each generated site gets one outward/upward path independent of the site archetype.

Path behavior:

* deterministic from the site/matrix/slot seed
* separate from site archetype logic
* may cross zones and Z-levels
* rejects protected/owned candidate zones before committing
* continues around blocked candidates when possible
* stops only when it reaches termination conditions or has no safe candidate

Current implementation:

* path coordinate generation works
* path instructions are generated from adjacent zone IDs
* custom path builder places visible path material
* vertical transitions can use holes/pits
* final safety filtering remains

Rationale:

* One path per site is enough.
* A second path would likely make the underground feel overbuilt and artificial.
* Candidate-level rerouting is better than generating a path and deleting unsafe segments afterward.

---

## Path Ascent Tuning

Decision:

Use depth-sensitive upward weighting.

Rationale:

* Surface-adjacent paths terminated too quickly when upward bias was high.
* Deeper paths benefit from upward drift because it connects strata.
* The goal is drift, not an elevator to the surface.

Current model:

```text
shallow origins → low upward bias
deeper origins  → stronger upward bias
```

Release tuning may remain adjustable.

---

## Path Materials

Decision:

Use only path materials that are visibly readable in underground zones.

Rationale:

* Path readability matters more than variety.
* `FungalTrailBrick` displayed as “coral path” but did not visibly render in testing.
* Low-visibility materials should stay out unless manually verified.

---

## Vertical Movement

Decision:

Sites generally use stairs; paths may use holes.

Rationale:

* Builders already handle stairs well inside sites.
* Holes are visible and signal an unusual route.
* Holes distinguish path traversal from ordinary dungeon descent.

---

## State Tracking

Decision:

Use minimal game-state and zone metadata.

Allowed metadata includes:

```text
SubterraneanSites_Owner
SubterraneanSites_IsSiteLayer
SubterraneanSites_IsSiteOrigin
SubterraneanSites_SiteDisplayName
SubterraneanSites_DiscoveryKey
SubterraneanSites_MatrixStatus_...
```

Rationale:

* Generation should mostly be reconstructible from seeds.
* State is still needed to prevent duplicate matrix processing.
* Zone metadata is needed for ownership checks and discovery behavior.
* Minimal metadata is acceptable when it supports safety and determinism.

---

## Discovery Behavior

Decision:

Any generated site layer can trigger discovery.

Implementation:

* all site layers get `SubterraneanSites_IsSiteLayer`
* all site layers get the same site display name
* all site layers get the same discovery key
* origin layer also gets `SubterraneanSites_IsSiteOrigin`
* `ZoneActivatedEvent` shows discovery popup once per site

Rationale:

* Players may dig into a non-origin layer first.
* Discovery should still work.
* Shared discovery key prevents repeated popups.

---

## Surface Interaction

Decision:

Surface zones are not site zones.

Current behavior:

* Surface zones can trigger matrix activation.
* Sites start underground.
* Paths may approach or reach the surface.
* Surface emergence is acceptable but not required.

Rationale:

* The core mod content is underground.
* Surface interaction should support discovery, not replace underground exploration.

---

## Existing Saves

Decision:

Support existing saves, but do not attempt to retroactively rewrite already-built zones.

Rationale:

* Existing-save support is important.
* Already-built zones may not receive generated content.
* Rewriting built zones would add risk.
* Local gaps are preferable to damaging existing world state.

---

## Encounter Density

Decision:

Accept the current density rather than adding more path systems.

Current density model:

* four site slots per matrix
* `3 × 3 × 3` neighbor matrix activation
* one path per generated site
* path length around 30–40 zones

Rationale:

* Testing found acceptable median encounter distance.
* Long dry searches can happen, especially at map edges.
* Adding a second path per site would likely feel artificial.
* Remaining density behavior can be explained to players.

---

## Manifest / Attribution

Decision:

Use `manifest.json` with release-facing metadata.

Current approach:

* `Author` uses Steam name.
* Source repository can carry real-name attribution.
* Preview image is stored in the mod root and referenced by `PreviewImage`.

Rationale:

* Qud expects root-level mod configuration.
* Steam-facing identity can differ from source-code identity.
* Preview image should be simple and symbolic at small size.

---

## Current Status

Accepted / working:

```text
✔ runtime pre-registration
✔ genesis bootstrap
✔ existing-save bootstrap
✔ deterministic matrix/slot site selection
✔ vertical site structure
✔ multiple working archetypes
✔ quad-slot matrix generation
✔ 3 × 3 × 3 neighbor matrix activation
✔ path coordinate/path builder system
✔ path candidate protection/rerouting
✔ dynamic safety initialization
✔ vanilla lair recovery from JoppaWorldInfo.lairs
✔ historical-site protection
✔ named special-site protection
✔ any-layer discovery popup
✔ manifest and preview image loading
```

Known residual risks:

```text
- rare possible interference with intermediate Shug’ruith path segments
- existing saves may have already-built zones that do not retroactively receive content
- encounter density has a long tail; players may occasionally search more than 10–15 zones
```

Current release posture:

```text
Release-candidate code is committed.
Remaining work is documentation, Workshop setup, and broader playtesting.
```
