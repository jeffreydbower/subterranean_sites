Subterranean Sites

Caves of Qud mod for deterministic underground site injection.

The mod creates additional underground sites in a way that should feel compatible with Qud’s existing procedural world. The long-term goal is to place deterministic vertical sites underground, guide players toward them through discoverable paths, and avoid overwriting important vanilla content.



Current Focus

- Develop deterministic stacked underground sites using registered ZoneManager builders
- Move from direct runtime zone building toward runtime pre-registration
- Prepare for matrix-based site/path registration
- Build a path system that leads players toward generated underground sites
- Preserve vanilla content by skipping or rejecting unsafe zones
- Keep site generation deterministic from world seed + matrix ID



Current Working Capabilities

Runtime registration:
- The mod can load a runtime IGameSystem
- The system is registered from a JoppaWorldBuilderExtension using:
  The.Game.RequireSystem<RuntimeZoneBuilderInjectionSystem>()
- BeforeZoneBuiltEvent works as the main runtime trigger
- ZoneManager.AddZoneBuilder works for future zones
- Generated/registered zones persist after re-entry

Shared site infrastructure:
- BuildSiteZoneIds(...) creates a vertical stack of zone IDs from an origin zone
- RegisterSelectedSite(...) provides an archetype-selection wrapper
- RollSiteKind(...) currently forces one archetype, but is ready to become a deterministic weighted roll
- Shared helpers handle:
  - stairs per layer
  - Z-depth parsing
  - tier calculation
  - basic ownership/collision checks



Current Active Archetype: SultanHistoric

The current active site archetype is a historical-site-like vertical dungeon using Qud’s SultanDungeon builder.

Working features:
- Reuses existing generated sultan history
- Reuses existing generated historical region snapshots
- Builds and stores SultanDungeonArgs
- Registers SultanDungeon across multiple vertical layers
- Uses existing SultanDungeon WFC/template layout behavior
- Generates cult-themed mobs
- Cult mobs receive cult-member social role text
- Bottom layer creates a vault
- Bottom layer receives a tier-appropriate relic/artifact
- Relicstyle="Vault" causes SultanDungeon to create a cult leader/hero near the vault
- Top layer preserves existing builders/connectors so natural entrances can survive
- Lower layers clear existing builders and let SultanDungeon control the level

Current status:
- Working prototype
- Feature-complete for first archetype
- Currently the only archetype selected by RollSiteKind(...)



Secondary Proven Archetype: BasicLair

A BasicLair-style vertical site was also prototyped successfully.

Working features:
- Multi-layer lair generation
- Controlled stairs
- Tiered single-mob XML population tables
- Tiered team/encounter XML population tables
- Custom population post-builder
- Bottom-layer special content logic

Current status:
- Working prototype
- Not currently wired into the selector
- Intentionally left out of the active code for now to keep the committed implementation tight around SultanHistoric



Past Direction

Originally explored:
- world generation injection
- direct runtime zone mutation
- direct builder application to current zones

Current direction:
- runtime pre-registration
- generated content registered before the player reaches it
- deterministic decisions based on world seed + matrix/site IDs

Reason for transition:
- AddZoneBuilder works cleanly for future zones
- Direct current-zone building can behave differently from vanilla registered builders
- Runtime pre-registration better matches how Qud expects zones to be built



Planned Path System

The next major feature is deterministic discovery paths.

Goals:
- Create paths leading outward/upward from generated sites
- Allow players to discover paths naturally underground
- Use visible path material so players can follow them
- Add holes or vertical transitions where needed
- Connect paths to site entrances
- Keep path generation separate from site archetype generation

Possible inspirations:
- Shug’ruith-style cradle paths
- Amaranthine Prism river/path behavior
- Klang path behavior
- Girsh/cradle-style vertical transitions



Planned Matrix System

Long-term site placement will use deterministic 3D matrices.

Each matrix should:
- be derived from world seed + matrix ID
- contain at most one generated site
- determine site existence, site type, site origin, depth, and path behavior
- be processed once

Future runtime behavior:
- detect current matrix on zone entry
- process current matrix
- process adjacent matrices when near an edge
- process diagonal neighbors when at a corner
- support late registration if the player arrives unexpectedly by portal/drop/forced movement



Safety Philosophy

Primary rule:
- Never overwrite important vanilla content

Preferred behavior:
- Skip mod content rather than damage vanilla content
- Reject unsafe sites or matrices when needed
- Allow imperfect or partial paths if safe
- Eventually reject whole sites if critical site zones collide with important content

Known future safety test target:
- Waterlogged Tunnel

Other safety areas:
- vanilla historical sites
- story zones
- villages
- special builders
- surface entrances
- player-current-zone late registration edge cases



Current Code Shape

RuntimeZoneBuilderInjectionSystem:
- handles runtime trigger
- builds shared vertical site zone IDs
- selects site archetype
- owns shared helper functions

SultanHistoricSiteRegistrar:
- nested archetype-specific registrar
- selects historical region/sultan data
- builds SultanDungeonArgs
- registers SultanDungeon layers
- adds bottom vault/relic behavior

Future expected registrar classes:
- BasicLairLegendarySiteRegistrar
- BasicLairDenseSiteRegistrar
- BasicLairVendorSiteRegistrar



Current Status

Working:
- Runtime system loads
- BeforeZoneBuiltEvent hook works
- Future-zone builder registration works
- Deterministic RNG model validated
- BasicLair vertical prototype works
- SultanDungeon historical-site prototype works
- Relic vault + cult leader behavior works
- Site selector wrapper exists
- SultanHistoric archetype is wrapped in a nested registrar

Next:
- start path system
- safety tests near Waterlogged Tunnel
- matrix implementation
- later add BasicLair archetypes back into selector


