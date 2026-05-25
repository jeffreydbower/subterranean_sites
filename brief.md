# Subterranean Sites — Brief

## Overview

Subterranean Sites is a Caves of Qud mod that introduces deterministic underground sites that can be discovered through exploration.

The mod currently generates vertical, multi-zone underground sites composed of stacked Z-levels at a shared X/Y coordinate. Sites are registered at runtime and built by vanilla zone builders where possible.

The long-term design is that each site is discoverable through an upward/outward path system, allowing players to find generated deep content organically without relying on map markers, compasses, or explicit quest pointers.

## Current Design

Subterranean Sites uses a runtime registration system rather than directly mutating already-built zones.

Current model:

- The runtime system registers site and path builders before target zones are built.
- `ZoneManager.AddZoneBuilder(...)` is used for future zones.
- Zones are then built normally by the game when entered.
- Site identity and layout decisions are deterministic.
- Generation decisions are based on world seed plus deterministic site/matrix inputs.
- Sites currently use vertical stacks of about 3–7 layers.
- Paths are generated separately from site archetypes.

Important constraint:

- `AddZoneBuilder(...)` does not affect the zone already in the current build pipeline.
- Therefore, final matrix/path logic must register generated content before the player reaches it.

## Site Archetypes

Several site archetypes are now working or near-working.

### SultanHistoric

Status: working.

Uses Qud’s `SultanDungeon` builder to create historical-site-like underground structures.

Behavior:

- Builds stacked SultanDungeon layers.
- Reuses vanilla sultan history and generated region data.
- Uses `SultanDungeonArgs` stored in game state.
- Can create cult populations, cult leaders, and relic vault behavior.
- Bottom layers can use vault/relic-style reward behavior.

### ProperLair

Status: working.

Uses vanilla lair-owner logic and tier-appropriate lair owner tables selected from terrain/lair-owner sources.

Behavior:

- Selects a tier-appropriate terrain/lair-owner table.
- Generates a coherent lair around a lair owner/minion population.
- Supports extra hero encounters.
- Supports upgraded reward chests.
- Uses shared reward chest helpers.

### MerchantHive / Underworld Bazaar

Status: working.

Creates a multi-layer underground merchant site.

Behavior:

- Generates merchant-heavy layers.
- Uses tier-appropriate merchant stock behavior.
- Uses guard/merchant-style gameplay rather than hostile dungeon reward structure.
- Currently named “Underworld Bazaar.”

### BasicLairChaos

Status: working.

Creates a mixed combat/dungeon site using BasicLair plus extra hostile population and occasional faction/chest encounters.

Behavior:

- Uses `BasicLair`.
- Adds tiered singles/team population rolls.
- Can add faction encounters and reward chests.
- Intended to provide a less coherent but more chaotic combat site.

## Current Site Selection

Site type is selected deterministically from the site seed.

Current rough weighting:

- SultanHistoric: major/default historical archetype
- ProperLair: major lair archetype
- BasicLairChaos: combat-chaos archetype
- MerchantHive: rarer merchant archetype

The exact weighting is still tunable.

## Path System

The path system is partially implemented.

Current behavior:

- Generates deterministic path zone IDs from a site origin.
- Paths move outward and upward.
- Path instructions describe entry/exit connections.
- A custom path zone builder can draw visible paths through zones.
- Paths may use road/brick/stone/ruin-like materials.
- Vertical path transitions can place holes/pits.

Planned behavior:

- Path generation will be tied to matrix/site generation.
- Paths should be clipped at matrix boundaries.
- Paths should avoid protected vanilla content.
- Paths should be discoverable mid-route.
- Paths should make deep sites findable without direct map hints.

## Matrix System

The matrix system is the next major development step.

Planned matrix model:

- Divide underground space into deterministic 3D matrices.
- Candidate size currently favored: 8 × 5 parasangs horizontally and 20 Z-levels deep.
- Each matrix may generate at most one site.
- Matrix ID plus world seed determines:
  - site type
  - origin
  - layer count
  - path direction/material
  - reward/site parameters

Planned constraints:

- Site origins should not appear on horizontal matrix edges.
- In the surface-containing matrix, origins should begin at depth −6 or deeper.
- Site origin must leave enough downward room for the full vertical site.
- Generation should happen once per matrix and be recorded in game state.

## Runtime Bootstrap

The mod now supports both major install scenarios.

### Genesis

“Genesis” means the mod is installed before a new world is created.

Bootstrap path:

- `JoppaWorldBuilderExtension.OnAfterBuild(...)`
- requires `RuntimeZoneBuilderInjectionSystem`
- initializes runtime safety

### Retrofit

“Retrofit” means the mod is installed into an already-existing save.

Bootstrap path:

- `[HasCallAfterGameLoaded]`
- `[CallAfterGameLoaded]`
- requires `RuntimeZoneBuilderInjectionSystem`
- initializes runtime safety after save load

This was added because `OnAfterBuild(...)` only covers world-generation flow and is not sufficient for existing saves.

## Safety System

The safety system is now release-critical and central to the project.

Primary rule:

- Never overwrite important vanilla generated content.

Before any site/path generation, the runtime system calls `EnsureSafetyReady()`.

If safety initialization fails:

- generation does not proceed
- no site/path builders are registered

Protected-content sources currently include:

### Static protected locations

Hardcoded known important areas.

### Historical sites

Read from persistent game state:

- `SultanDungeonPlacementOrder_i`
- `sultanRegionPosition_[regionName]`

Historical site columns are protected across their relevant Z range.

### Vanilla lairs and legendary merchant lairs

Read from persistent runtime world state:

- `The.Game.GetObjectGameState("JoppaWorldInfo")`
- reflected field: `lairs`
- expected count: 125

Each lair is protected as a surface-origin column from Z 10–14.

This replaced the earlier genesis-only capture from `builder.worldInfo.lairs`.

### Named special lairs / Girsh-related sites

Read from `JournalAPI` map-note secrets, including:

- Oboroqoru
- Qas/Qon
- Rermadon
- Shug’ruith mouth
- Shug’ruith lair

Some are protected as exact zone columns; others are protected as broader parasang columns.

## Safety Enforcement Points

Protection is enforced in multiple places:

- Candidate site zones are filtered before registration.
- `RegisterLayeredSite(...)` checks protection again before registering each site layer.
- Candidate path zones are filtered before path registration.
- `RegisterRoadPathZone(...)` checks protection again before adding path builders.

This creates redundant protection against accidental site/path collisions.

## Testing Completed

Confirmed:

- Runtime system registration works in new worlds.
- Runtime system registration works when the mod is installed into existing saves.
- `CallAfterGameLoaded` bootstrap works for retrofit installs.
- `JoppaWorldInfo.lairs` persists in existing saves.
- Runtime reflection can recover 125 vanilla lairs.
- Recovered lair coordinates were spot-checked in game and confirmed.
- Historical site coordinate protection works from game-state vectors.
- Special lair map notes can be read through `JournalAPI`.
- Site/path registration is blocked when protection rejects a zone.
- Multi-layer sites generate and persist.
- ProperLair, MerchantHive, BasicLairChaos, and SultanHistoric archetypes all have working prototypes.
- Path builder can place visible path material and vertical holes.

## Remaining Work

### Matrix generation

Implement deterministic matrix detection and per-matrix site generation.

Needed:

- Convert player zone to matrix ID.
- Track generated matrices in game state.
- Pick deterministic origin within matrix.
- Roll deterministic site type/layers.
- Reject protected origins/site layers.
- Generate and clip paths within matrix bounds.
- Register site/path zones.

### Collision testing

Perform dedicated collision tests against:

- vanilla lairs
- legendary merchant lairs
- historical sites
- Girsh/special lairs
- villages/settlements if needed
- known story/special content
- matrix edge cases
- retrofit saves

### Release cleanup

Before release:

- remove obsolete debug popups
- keep only intentional debug flags
- document safety limitations honestly
- add a player-facing failure popup if safety initialization fails
- finish README, decisions, spike log, and test plan updates
- run repeated no-collision tests after the final matrix implementation

## Current Status

The project has moved from proof-of-concept site injection to a near-release architecture.

The main remaining critical system is matrix-based generation. The major recent detour resolved a release-blocking safety issue: retrofit installs can now initialize the runtime system and recover vanilla generated lair locations from persistent game state before generating any mod content.

This means the mod is no longer limited conceptually to new worlds only, provided continued testing confirms that protected-location detection remains reliable.