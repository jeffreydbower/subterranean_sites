# Subterranean Sites

Caves of Qud mod for deterministic underground site generation.

Subterranean Sites adds discoverable underground sites that are intended to feel compatible with Qud’s existing procedural world. Sites are generated as deterministic vertical structures, registered through Qud’s zone-building system, and protected by safety checks intended to avoid overwriting important vanilla content.

## Current Focus

- Generate deterministic stacked underground sites
- Use runtime pre-registration with `ZoneManager.AddZoneBuilder(...)`
- Support both new worlds and existing saves
- Protect vanilla generated content before registering mod content
- Build path systems that lead players toward generated underground sites
- Prepare for matrix-based site/path placement

## Current Working Capabilities

Runtime registration:
- Runtime `IGameSystem` registration works
- New-world bootstrap works through `JoppaWorldBuilderExtension.OnAfterBuild(...)`
- Existing-save bootstrap works through `[CallAfterGameLoaded]`
- `BeforeZoneBuiltEvent` works as the main runtime generation trigger
- `ZoneManager.AddZoneBuilder(...)` works for future zones
- Generated/registered zones persist after re-entry

Safety:
- Safety initialization runs before site/path generation
- Vanilla lairs and legendary merchant lairs are recovered from `JoppaWorldInfo.lairs`
- Historical sites are protected from persistent sultan-region game state
- Named special lairs are protected through `JournalAPI` map-note secrets
- Sites and paths are skipped when protected zones are detected

Site infrastructure:
- Vertical site stacks are generated from a shared origin zone
- Site archetypes are selected deterministically
- Shared helpers handle stairs, tier calculation, reward chests, music, and zone metadata
- Site discovery popup works on entering a site origin zone

## Working Site Archetypes

Current working archetypes:

- `SultanHistoric`
- `ProperLair`
- `BasicLairChaos`
- `MerchantHive` / Underworld Bazaar

### SultanHistoric

Historical-site-style vertical dungeon using Qud’s `SultanDungeon` builder.

Working features:
- Reuses generated sultan history and region data
- Registers `SultanDungeon` across multiple vertical layers
- Generates cult-themed mobs
- Supports relic/vault behavior on lower layers

### ProperLair

Coherent vertical lair using vanilla lair-owner and minion logic.

Working features:
- Tier-appropriate lair owner selection
- Lair-style population coherence
- Extra hero encounters
- Upgraded reward chests

### BasicLairChaos

Mixed combat site using `BasicLair` plus additional population and faction encounters.

Working features:
- Tiered singles/team population tables
- Extra faction encounters
- Reward chest support

### MerchantHive / Underworld Bazaar

Merchant-heavy underground site.

Working features:
- Multi-layer merchant site
- Tier-appropriate merchant behavior
- Guard/merchant-style structure rather than dungeon reward structure

## Planned Path System

The path system is partially implemented.

Current:
- Deterministic path zone IDs
- Entry/exit path instructions
- Custom path builder for visible path material
- Vertical holes/pits for path transitions

Planned:
- Tie paths to matrix/site generation
- Clip paths at matrix boundaries
- Avoid protected vanilla content
- Make paths discoverable mid-route

## Planned Matrix System

The next major system is deterministic matrix placement.

Planned behavior:
- Divide underground space into 3D matrices
- Generate at most one site per matrix
- Derive site definition from world seed + matrix ID
- Process each matrix once
- Register current and adjacent matrices before the player reaches generated content
- Reject or skip protected site/path zones

Current favored matrix size:
- 8 × 5 parasangs
- 20 Z-levels deep

## Safety Philosophy

Primary rule:

- Never overwrite important vanilla content.

Preferred behavior:
- Skip mod content rather than damage vanilla content
- Reject unsafe sites or paths
- Allow partial paths only when safe
- Disable generation if required safety data cannot initialize

Safety testing remains required before release.

## Current Status

Working:
- runtime pre-registration
- genesis bootstrap
- retrofit bootstrap
- dynamic safety initialization
- vanilla lair recovery from persistent runtime state
- historical-site protection
- named special-site protection
- multiple vertical site archetypes
- path coordinate/path builder prototype

Next:
- implement matrix detection and generation
- connect matrix system to site/path registration
- clip paths at matrix boundaries
- run collision tests against vanilla lairs, historical sites, special lairs, and other important content
- add a player-facing failure popup if safety initialization fails