# Subterranean Sites — Design Decisions

---

## Runtime Generation Approach

Decision:

Use runtime pre-registration rather than direct mutation of the currently building zone.

Current model:

- BeforeZoneBuiltEvent remains the primary runtime generation hook.
- ZoneManager.AddZoneBuilder(...) is used to register builders for future zones.
- Qud’s normal zone-building pipeline then builds those zones when entered.

Important constraint:

    AddZoneBuilder(...) works for future zones.
    AddZoneBuilder(...) does not affect the current zone already in the build pipeline.

Status:

    Direct runtime injection → deprecated as primary approach
    Runtime pre-registration → primary architecture
    Direct BuildZone/application → diagnostic/fallback only

---

## Runtime System Bootstrap

Decision:

Support both genesis and retrofit installs.

Definitions:

    Genesis  = mod installed before a new world is created
    Retrofit = mod installed into an existing save

Genesis bootstrap:

    JoppaWorldBuilderExtension.OnAfterBuild(...)
    → RequireSystem<RuntimeZoneBuilderInjectionSystem>()
    → EnsureSafetyReady()

Retrofit bootstrap:

    [HasCallAfterGameLoaded]
    [CallAfterGameLoaded]
    → RequireSystem<RuntimeZoneBuilderInjectionSystem>()
    → EnsureSafetyReady()

Rationale:

- OnAfterBuild(...) is valid for world generation.
- Existing saves do not re-run world generation.
- CallAfterGameLoaded provides a load-time bootstrap for retrofit installs.

---

## Safety Initialization Gate

Decision:

No site or path generation may occur unless dynamic protected-location safety initializes successfully.

Current rule:

    If EnsureSafetyReady() fails:
        register nothing

Rationale:

- The mod can register builders into arbitrary generated world locations.
- Missing protection data could cause collisions with vanilla generated content.
- Failing closed is safer than generating unsafely.

Future release behavior:

    If safety init fails:
        show one player-facing warning popup
        disable generation for that save/session

---

## Protected-Location Source Model

Decision:

Use multiple authoritative source adapters feeding one safety system.

Unified output:

    SubterraneanSafety.IsProtected(...)

Current protected sources:

- hardcoded static protected locations
- historical sites from game-state vectors
- vanilla lairs / legendary merchant lairs from JoppaWorldInfo.lairs
- named special/Girsh-related sites from JournalAPI map-note secrets

Rationale:

- Qud persists different generated systems in different ways.
- Forcing all safety into one source would be less robust.
- The unified layer is the protection check, not the data source.

---

## Vanilla Lair Protection

Decision:

Recover vanilla lair locations from persistent runtime world state.

Implementation:

    The.Game.GetObjectGameState("JoppaWorldInfo")
    → reflected field: lairs
    → GeneratedLocationInfo.zoneLocation

Confirmed:

    JoppaWorldInfo.lairs count = 125
    Recovered coordinates spot-checked in game

Protection:

    Each vanilla lair is protected as a zone column from Z 10–14.

Rationale:

- Earlier builder.worldInfo.lairs capture worked only during genesis.
- JoppaWorldInfo.lairs persists into existing saves.
- This supports both genesis and retrofit installs.

---

## Historical Site Protection

Decision:

Continue using persistent sultan-region game state for historical site protection.

Source:

    SultanDungeonPlacementOrder_i
    sultanRegionPosition_[regionName]

Protection:

    Historical sites are protected as center-zone columns across their expected Z range.

Rationale:

- This source is specific to historical site placement.
- It is available at runtime.
- It does not require reconstructing historical sites from broad world-info tables.

---

## Named Special Site Protection

Decision:

Use JournalAPI map-note secrets for specific named special lairs.

Protected targets include:

- Oboroqoru
- Qas/Qon
- Rermadon
- Shug’ruith mouth
- Shug’ruith lair

Rationale:

- These are known named locations with persistent map-note identifiers.
- Some need exact zone-column protection.
- Others need broader parasang-column protection.

---

## Redundant Safety Enforcement

Decision:

Protect both sites and paths at multiple stages.

Current enforcement:

- Candidate site zones are filtered before registration.
- RegisterLayeredSite(...) checks protection again.
- Candidate path instructions are filtered before registration.
- RegisterRoadPathZone(...) checks protection again.

Rationale:

    Safety should fail closed.
    Redundant checks are acceptable because collision damage would be worse than partial generation.

---

## Builder Strategy

Decision:

Prefer vanilla builders where possible.

Working site-builder families:

- SultanDungeon
- BasicLair
- vanilla lair-owner generation
- merchant/lair population systems
- small custom support builders for path material, holes, and targeted features

Rationale:

- Vanilla builders produce Qud-native layouts and encounters.
- Reusing vanilla systems reduces custom content burden.
- Custom builders are still appropriate for small connective/path features.

---

## Site Structure

Decision:

Sites are vertical columns, not broad horizontal 3×3 parasang structures.

Current structure:

    same X/Y
    varying Z
    typically 3–7 layers

Rationale:

- Qud already uses vertical dungeon structure naturally.
- Vertical sites are easier to register deterministically.
- Paths provide broader spatial discovery without requiring horizontal site sprawl.

---

## Site Archetypes

Decision:

Support multiple deterministic site archetypes.

Current working archetypes:

- SultanHistoric
- ProperLair
- BasicLairChaos
- MerchantHive / Underworld Bazaar

Site selection:

    world seed + site/matrix seed
    → deterministic weighted site type

Rationale:

- Archetype variety makes repeated discoveries more interesting.
- Each archetype can reuse different vanilla systems.
- The selector can be tuned without changing matrix architecture.

---

## Determinism

Decision:

Site identity must be deterministic, but vanilla builder internals do not need to be perfectly controlled.

Required deterministic inputs:

    world seed + matrix ID

Controls:

- site existence
- site origin
- site type
- layer count
- path direction
- path material
- reward/site parameters

Important distinction:

    Builder RNG may vary internally.
    The mod’s site registration decisions must remain deterministic.

---

## Matrix-Based Procedural Generation

Decision:

Use a direct matrix system rather than zone-level approximation.

Planned model:

    World underground space is divided into 3D matrices.
    Each matrix may contain at most one site.

Current preferred matrix dimensions:

    8 × 5 parasangs
    20 Z-levels deep

Planned matrix behavior:

- detect current matrix from player zone
- process each matrix once
- generate site definition from matrix seed
- avoid edge origins
- avoid insufficient-depth origins
- clip paths at matrix boundaries
- record processed matrices in game state

---

## Matrix Boundary Processing

Decision:

Process adjacent matrices only when needed.

Planned behavior:

    Normal interior:
        process current matrix

    Matrix edge:
        process current matrix + adjacent side matrix

    Matrix corner:
        process current matrix + 2 side neighbors + 1 diagonal neighbor

Rationale:

- Diagonal crossing only matters at corners.
- Most movement requires only one matrix.
- Processed matrices are cached/marked to avoid repeat work.

---

## Navigation System

Decision:

Do not use compass, attunement, or explicit directional feedback.

Replacement:

    Natural path discovery

Rationale:

- Paths are more Qud-like than UI guidance.
- Players can discover paths mid-route.
- Physical paths make deep sites feel integrated into the underground world.

---

## Path Generation

Decision:

Each generated site should have an outward/upward path independent of the site builder.

Path behavior:

- deterministic from the site/matrix seed
- separate from site archetype logic
- may cross zones and Z-levels
- should avoid protected vanilla content
- should eventually be clipped at matrix boundaries

Current implementation:

- path coordinate generation works
- path instructions are generated
- custom path builder places visible path material
- vertical transitions can use holes/pits

---

## Vertical Movement

Decision:

Sites generally use stairs; paths may use holes.

Rationale:

- Builders already handle stairs well inside sites.
- Holes are more visible and signal an unusual route.
- Holes help distinguish path traversal from ordinary dungeon descent.

---

## State Tracking

Decision:

Use minimal game-state and zone metadata.

Allowed metadata:

    SubterraneanSites_Owner
    SubterraneanSites_IsSiteOrigin
    SubterraneanSites_SiteDisplayName
    SubterraneanSites_DiscoveryKey
    MatrixProcessed markers

Rationale:

- Generation should be reconstructible from seeds.
- State is still needed to prevent duplicate processing and support discovery behavior.
- Minimal metadata is acceptable when it preserves deterministic architecture.

---

## Discovery Behavior

Decision:

Site discovery is tied to entering the site origin zone.

Implementation:

- origin zone gets SubterraneanSites_IsSiteOrigin
- site display name and discovery key are stored as zone properties
- ZoneActivatedEvent shows discovery popup once

Rationale:

- Discovery should be simple and nonintrusive.
- Full path discovery logic can evolve separately.

---

## Surface Interaction

Decision:

Surface zones are not site zones.

Current/planned behavior:

- surface zones may eventually trigger underground matrix registration
- paths may approach or reach the surface
- optional surface holes may be added later

Rationale:

- The core mod content is underground.
- Surface interaction should support discovery, not replace underground exploration.

---

## Current Status

Accepted / working:

    ✔ runtime pre-registration
    ✔ genesis bootstrap
    ✔ retrofit bootstrap
    ✔ deterministic site selection
    ✔ vertical site structure
    ✔ multiple working archetypes
    ✔ path coordinate/path builder prototype
    ✔ dynamic safety initialization
    ✔ vanilla lair recovery from JoppaWorldInfo.lairs
    ✔ historical-site protection
    ✔ named special-site protection

Next major decision area:

    matrix implementation details

Next major development work:

- implement matrix detection
- generate per-matrix site definitions
- register current/adjacent matrices
- clip paths at matrix boundaries
- run collision tests