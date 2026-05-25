# Subterranean Sites — Spike Log

## Runtime Injection / Registration

Early testing confirmed that `BeforeZoneBuiltEvent` fires early enough to modify zone metadata and register future builders.

Important finding:

    ZoneManager.AddZoneBuilder(...) does not affect the zone already in the current build pipeline.
    It does work for future zones that have not yet been built.

Conclusion:

- Direct builder application can work as a diagnostic or fallback.
- The primary architecture is runtime pre-registration.
- Generated sites and paths should be registered before the player reaches them.

Current model:

    BeforeZoneBuiltEvent
    → detect/generate site or matrix definition
    → register future zone builders
    → let ZoneManager build zones normally on entry

---

## IGameSystem / Bootstrap Discovery

Initial understanding:

- `IGameSystem` classes are not automatically added to the save.
- The runtime system must be explicitly required with:

    The.Game.RequireSystem<RuntimeZoneBuilderInjectionSystem>()

Genesis bootstrap:

    JoppaWorldBuilderExtension.OnAfterBuild(...)
    → RequireSystem<RuntimeZoneBuilderInjectionSystem>()

This works for new worlds, but not for existing saves where world generation has already happened.

Retrofit bootstrap discovery:

- `XRLGame.LoadGame(...)` calls `ModManager.CallAfterGameLoaded()`.
- `CallAfterGameLoaded` invokes methods marked with:
  - `[HasCallAfterGameLoaded]`
  - `[CallAfterGameLoaded]`

Conclusion:

- Genesis installs use `OnAfterBuild`.
- Retrofit installs use `CallAfterGameLoaded`.
- Both paths now require the same runtime system and call the same safety initialization gate.

---

## Event Timing

Findings:

- `ZoneActivatedEvent` is too late to modify the already-built zone.
- `ZoneActivatedEvent` is useful for discovery behavior after a generated site exists.
- `BeforeZoneBuiltEvent` is the correct runtime hook for generation/pre-registration logic.

Current use:

- `BeforeZoneBuiltEvent`: generation and registration trigger.
- `ZoneActivatedEvent`: site discovery popup when entering a site origin zone.

---

## Deterministic RNG Test

Goal:

Confirm that stable string inputs produce repeatable mod decisions across separate new games.

Test setup:

- Used underground ZoneID strings as stable inputs.
- Generated seed with:

    XRLCore.Core.Game.GetWorldSeed(ZoneID + worldSeed)

- Created `System.Random(seed)`.
- Wrote seed/roll results into zone names.
- Repeated test across separate new games.

Result:

- Same zones selected across repeated new games.
- Same generated names/rolls appeared for matching zones.

Conclusion:

- The mod can make deterministic registration decisions from stable inputs.
- Future site generation should use:

    world seed + matrix ID

Important limitation:

- This does not prove vanilla builders produce identical internal layouts.
- Only the mod-controlled registration layer must be deterministic.

---

## Builder Registration Timing Test

Observation:

- Registering builders with `AddZoneBuilder(...)` during the build event does not affect the current zone.
- The same registration affects future unbuilt zones.
- Zone names/properties can update immediately.

Conclusion:

- Runtime pre-registration is the correct long-term approach.
- Direct current-zone build calls are not the preferred architecture.

---

## BasicLair / Proper Lair Spikes

Goal:

Determine whether vanilla lair systems can support vertical site archetypes.

Results:

- Multi-layer BasicLair-style sites work.
- Stairs can be controlled per layer.
- Tiered XML population tables load from the active mod folder.
- Repeated population rolls work for density.
- Team/packet-style tables feel more Qud-like than flat individual mobs.
- Proper lair generation can use tier-appropriate lair-owner tables.
- Extra heroes and upgraded reward chests work.
- Rare chest blueprints can be used for visible reward chests.

Conclusion:

- BasicLair is usable as a chaotic/combat archetype.
- ProperLair is usable as a coherent lair archetype.
- Lair-style sites should use vanilla table/owner logic where possible.

---

## SultanDungeon / Historical Site Spike

Goal:

Reuse Qud’s `SultanDungeon` system for additional historical-site-like underground sites.

Key discovery:

`SultanDungeon` requires a matching game-state object:

    The.Game.SetObjectGameState("sultanDungeonArgs_" + regionName, args)

Then each layer can register:

    The.ZoneManager.AddZoneBuilder(
        zoneId,
        6000,
        "SultanDungeon",
        "locationName", locationName,
        "regionName", regionName,
        "stairs", stairs
    );

Working approach:

- Reuse existing generated sultan history.
- Reuse existing generated historical region snapshots.
- Build `SultanDungeonArgs` from sultan/region snapshots.
- Store args under a mod-specific region key.
- Register `SultanDungeon` across vertical layers.

Results:

- Multi-layer historical-site-like dungeons generate successfully.
- WFC/template structures appear correctly.
- Cult mobs appear.
- Cult social-role text appears.
- Bottom-layer vault/relic behavior works.
- Cult leader/hero behavior works.
- Relic chest and tier-appropriate relic placement work.

Conclusion:

- `SultanDungeon` is a working site archetype.
- The archetype is now represented as `SultanHistoric`.

---

## Merchant Hive / Underworld Bazaar Spike

Goal:

Create a non-hostile or semi-hostile merchant-focused underground site.

Results:

- Multi-layer merchant-heavy site works.
- Tier-appropriate merchant behavior works.
- Legendary/vendor-style gameplay is viable as a rare site type.
- The site is currently named “Underworld Bazaar.”

Conclusion:

- MerchantHive is a working archetype.
- It should remain a rarer site type.

---

## Chaos Lair Spike

Goal:

Create a less coherent combat site with mixed encounters.

Results:

- `BasicLair` can provide base structure.
- Custom tiered singles/team population tables can add density.
- Extra faction encounters work.
- Reward chests work.
- Required `SubterraneanSiteMobs` custom builder must remain available even if IDE shows few direct references.

Conclusion:

- BasicLairChaos is a working archetype.
- It provides useful variation from coherent lairs and historical sites.

---

## Path System Spike

Goal:

Prototype deterministic paths leading outward/upward from generated sites.

Results:

- Path zone IDs can be generated deterministically.
- Path instructions can describe entry/exit directions.
- Custom path builder can draw visible path material.
- Path builder can place vertical holes/pits.
- Path material selection works.
- Path system is separate from site archetype logic.

Conclusion:

- Path concept is viable.
- Final path generation should be tied to matrix/site generation.
- Paths must be clipped at matrix boundaries.
- Paths must run through the same protection checks as sites.

---

## Runtime Safety Spike

Problem:

The mod can register builders into arbitrary world locations. This creates a risk of overwriting vanilla generated content.

Design rule:

    Never overwrite important vanilla content.
    If safety data is unavailable, generate nothing.

Safety architecture:

- Static protected locations remain hardcoded.
- Dynamic protected locations are recovered from runtime game state.
- All protection sources feed `SubterraneanSafety.IsProtected(...)`.

Enforcement points:

- Site zones are filtered before registration.
- `RegisterLayeredSite(...)` checks again.
- Path instructions are filtered before registration.
- `RegisterRoadPathZone(...)` checks again.

Conclusion:

- Safety should be redundant and fail closed.

---

## Vanilla Lair Protection Spike

Problem:

Earlier lair protection used `builder.worldInfo.lairs`, which only works during genesis/worldgen.

Retrofit requirement:

- Existing saves need the same protection.
- Therefore lair data must be recovered from persistent runtime state.

Discovery:

- `The.Game.GetObjectGameState("JoppaWorldInfo")` exists in loaded saves.
- It contains a reflected `lairs` field.
- `JoppaWorldInfo.lairs` contains 125 vanilla lairs.

Test results:

- Runtime reflection recovered 125 lairs.
- Coordinates were spot-checked in game and confirmed.
- One checked lair was a legendary merchant lair.

Conclusion:

- Vanilla lair and legendary merchant protection can work in both genesis and retrofit.
- Use `JoppaWorldInfo.lairs` instead of transient `builder.worldInfo.lairs`.

Current protection:

    each vanilla lair = protected zone column, Z 10–14

---

## Historical Site Protection Spike

Goal:

Protect vanilla historical sites from site/path collision.

Source:

    SultanDungeonPlacementOrder_i
    sultanRegionPosition_[regionName]

Finding:

- Historical site region positions are stored in persistent game state.
- Position objects expose `x` and `y` fields.
- Historical sites can be protected as center-zone columns.

Conclusion:

- Historical site protection works from runtime game state.
- No need to reconstruct historical sites from broader world-info tables.

---

## Named Special Site Protection Spike

Goal:

Protect important named special lairs and Girsh-related locations.

Source:

    JournalAPI.GetMapNote(secretId)

Protected targets:

- Oboroqoru
- Qas/Qon
- Rermadon
- Shug’ruith mouth
- Shug’ruith lair

Finding:

- Map-note secrets provide persistent zone IDs.
- Some locations should be protected as exact zone columns.
- Some should be protected as broader parasang columns.

Conclusion:

- JournalAPI map notes are the right source for named special-site protection.

---

## Code Organization Spike

Current organization:

- `RuntimeZoneBuilderInjectionSystem`
  - runtime hooks
  - site selection
  - shared registration helpers
  - path registration
  - safety gate

- `SubterraneanDynamicProtectedLocations`
  - dynamic protected-location recovery
  - lair/historical/special protection checks

- `SubterraneanZoneBuilders.cs`
  - site registrar classes
  - archetype-specific registration logic

- `SubterraneanPathBuilder`
  - custom path drawing and hole placement

Decision:

- Keep shared registration helpers in the runtime system for now.
- Keep archetype-specific logic in registrar classes.
- Move site content code away from the main runtime file where practical.

---

## Matrix System Spike

Planned model:

    underground space → deterministic 3D matrices
    one site at most per matrix
    world seed + matrix ID → site definition

Current favored dimensions:

    8 × 5 parasangs
    20 Z-levels deep

Rules under consideration:

- No site origins on horizontal matrix edges.
- Surface-containing matrix should place origins at −6 or deeper.
- Origin must leave enough downward room for full site layers.
- Paths should be clipped at matrix boundaries.
- Processed matrices should be marked in game state.

Conclusion:

- Matrix system is the final major generation system.
- Direct implementation is preferred over zone-level approximation.

---

## Crash / GPU Note

A prior crash during testing was associated with system/GPU instability rather than clear mod fault.

Summary:

- BSOD: SYSTEM_THREAD_EXCEPTION_NOT_HANDLED / 0x7E
- Faulting module: nvlddmkm.sys
- Same mod scenario later loaded successfully.

Conclusion:

- Track recurrence, but current interpretation is GPU/driver instability, not confirmed mod fault.

---

## Current Status

Working:

- genesis bootstrap
- retrofit bootstrap
- runtime pre-registration
- deterministic site decisions
- multiple vertical site archetypes
- path coordinate/path builder prototype
- dynamic safety initialization
- vanilla lair protection from `JoppaWorldInfo.lairs`
- historical site protection
- named special site protection

Next work:

- implement matrix detection
- generate per-matrix site definitions
- register current/adjacent matrices
- clip paths at matrix boundaries
- add player-facing failure popup if safety initialization fails
- run collision tests against protected vanilla content