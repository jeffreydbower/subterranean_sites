Subterranean Sites

Overview

Subterranean Sites is a Caves of Qud mod that introduces deterministic, multi-zone underground “sites” that players can discover through exploration.

These sites:
- are generated/registered when the player first approaches or encounters their broader area
- exist at arbitrary underground depths
- consist primarily of stacked dungeon layouts of about 3–7 layers
- use vertical column structure: same X/Y, varying Z
- use existing vanilla zone builders where possible
- will eventually have generated paths extending outward and upward from the site
- may use path materials such as water, dirt, stone, brick, ruins-like material, or Girsh resin

Critical note:
- Sites are vertical structures, not multi-parasang horizontal layouts.
- Path systems may extend across many zones, but the site itself is currently treated as a vertical stack.



Core Mechanics

Site Generation

Original plan:
- Sites were injected at runtime using BeforeZoneBuiltEvent
- Builders were applied directly to zones using:
  ZoneManager.ApplyBuilderToZone(...)

Updated plan:
- BeforeZoneBuiltEvent remains the runtime trigger
- The preferred architecture is now runtime pre-registration
- Builders are registered for future zones using:
  ZoneManager.AddZoneBuilder(...)
- ZoneManager then builds those zones normally when the player enters them

Critical note:
- AddZoneBuilder(...) does not affect the zone already in the current build pipeline
- Therefore, site/path zones must be registered before the player enters them
- Direct BuildZone / ApplyBuilderToZone behavior is retained only as a fallback or diagnostic approach

Site membership:
- Site membership must be computed deterministically
- Future site identity should derive from:
  world seed + matrix ID
- Site layers must agree on shared site parameters:
  - site type
  - origin
  - depth/layer count
  - path definition
  - reward/bottom-layer behavior



Navigation

Players should discover sites organically.

The planned discovery system is:
- an outward and upward path from each generated site
- paths may extend roughly 30–40 zones, subject to tuning
- paths should provide a traversable and visible route toward the site
- paths may be discovered mid-route
- paths may use distinct materials such as:
  - water
  - dirt
  - stone
  - brick
  - ruins-like material
  - Girsh resin

Removed:
- compass system
- directional feedback system
- attunement stones

Critical note:
- The path system must be deterministic and reconstructible from the same matrix/site seed
- Paths are separate from site generation
- Paths should not depend on the internals of any particular site builder
- Paths may use holes or other vertical transitions, while sites generally use stairs



Progression

There is no explicit progression system.

Player goal:
- discover a path or entrance
- follow it to the site
- explore downward through the stacked site
- reach the bottom layer

Bottom layer should usually contain:
- artifact/relic reward
- boss, cult leader, legendary NPC, or equivalent special encounter

Potential future additions:
- rare legendary merchant sites
- vendor/workshop lairs
- dense combat lairs
- alternate reward structures

Critical note:
- Reward placement should be tied to the site definition
- Reward behavior should not depend on uncontrolled per-zone randomness where avoidable



Current Working Site Archetypes

SultanHistoric

Status:
- Working
- Current active archetype
- Feature-complete for the first historical-site-style prototype

Description:
- Uses Qud’s SultanDungeon builder
- Reuses existing generated sultan history
- Reuses existing generated historical region snapshots
- Builds and stores SultanDungeonArgs
- Registers SultanDungeon across multiple vertical layers
- Produces historical-site-like layouts and cult populations

Working behavior:
- Multi-layer SultanDungeon site generates successfully
- Cult mobs appear
- Cult mobs receive cult-member social role text
- Region name, tier, and sultan period diagnostics worked
- Top layer preserves existing builders/connectors so natural entrances can survive
- Lower layers clear existing builders and let SultanDungeon control the zone
- Bottom layer uses Relicstyle="Vault"
- Relicstyle="Vault" causes SultanDungeon to create a vault region and cult leader/hero
- PlaceRelicBuilder places a tier-appropriate relic/artifact into the vault chest

Important implementation bridge:
- SultanDungeon requires:
  The.Game.SetObjectGameState("sultanDungeonArgs_" + regionName, args)

- Then each layer registers:
  The.ZoneManager.AddZoneBuilder(
      zoneId,
      6000,
      "SultanDungeon",
      "locationName", locationName,
      "regionName", regionName,
      "stairs", stairs
  );

Current code organization:
- Sultan-specific logic is wrapped in:
  SultanHistoricSiteRegistrar



BasicLair

Status:
- Working prototype
- Not currently wired into the active site selector

Description:
- Uses BasicLair as a vertical lair-style archetype
- Tested with custom tiered singles/team population tables
- Intended future variants:
  - BasicLairLegendary
  - BasicLairDense
  - BasicLairVendor

Working behavior:
- Multi-layer BasicLair-style site generation works
- Controlled stairs work
- Tiered XML population tables load from the active mod folder
- Singles and team tables can populate levels
- Bottom-layer special content logic was prototyped

Reason it is not currently active:
- The current committed code keeps only the working SultanHistoric archetype wired into the selector
- BasicLair variants will be reintroduced after path/matrix infrastructure is clearer



Current Focus

Current development focus:
- Start deterministic path generation
- Keep path generation separate from site archetype generation
- Prepare for matrix-based site/path registration
- Begin safety testing near known special content, especially Waterlogged Tunnel
- Preserve vanilla content by skipping/rejecting unsafe zones or matrices

Completed:
- Identify usable zone builders
- Confirm BasicLair as a usable vertical site archetype
- Confirm SultanDungeon as a usable historical-site archetype
- Confirm runtime pre-registration works
- Confirm deterministic RNG model for site decisions
- Confirm relic vault + vanilla cult leader behavior
- Refactor SultanHistoric code into a nested registrar behind a site selector



Current Technical Status

Confirmed:
- Runtime system registration works through JoppaWorldBuilderExtension
- BeforeZoneBuiltEvent works as the runtime trigger
- AddZoneBuilder(...) works for future zones
- Direct current-zone building is not required for the intended architecture
- Generated zones persist after re-entry
- Multi-zone deterministic selection works
- Stable ZoneID/world-seed-based RNG produces repeatable decisions across separate new games
- Constructed static-location multi-layer sites
- Deterministic layer count implemented
- BasicLair stacked site works
- SultanDungeon stacked site works
- SultanDungeon can be used outside vanilla AddSultanHistoryLocations
- Bottom-layer relic vault and cult leader behavior works through vanilla systems

Critical note:
- Future system must ensure all layers agree on site parameters
- The seed should be site/matrix based, not independently recalculated per layer in a way that can diverge
- Builder internals do not need to be perfectly deterministic if site identity and registration decisions remain deterministic



Architecture Update

The project still uses a runtime system registered through JoppaWorldBuilderExtension.

The architecture has shifted:

Old model:
- direct runtime zone mutation
- direct builder application to current zone

Current model:
- runtime pre-registration
- register builders for future zones
- allow ZoneManager to build zones normally when entered

Current runtime system responsibilities:
- respond to BeforeZoneBuiltEvent
- build shared vertical zone ID stacks
- select site archetype
- register future site zones
- mark owned zones
- avoid obvious collisions

Future runtime system responsibilities:
- detect current matrix
- detect nearby matrix boundaries
- generate deterministic site definitions
- register site builders
- register path builders
- process adjacent matrices near edges/corners
- attempt safe late registration if the player arrives by portal/drop/forced movement
- avoid directly building the current zone except as a fallback



Safety Direction

Primary rule:
- Never overwrite important vanilla content

Current safety behavior:
- Uses SubterraneanSites_Owner metadata
- Checks for existing builders such as SultanDungeon, Village, and BasicLair
- Skips zones that appear claimed by other content

Future safety behavior:
- Reject whole sites if critical site zones conflict
- Allow paths to route around, skip, or partially fail if needed
- Avoid overwriting the current player-occupied zone during late registration
- Test near known special content

Known future safety test target:
- Waterlogged Tunnel

Other safety targets:
- vanilla historical sites
- villages
- story zones
- lairs
- ruins
- special builder zones
- surface entrances
- portal/drop/forced-arrival edge cases



Next Technical Direction

Path system:
- Study vanilla path/trail systems
- Build deterministic path material placement
- Add path mouths
- Add path holes or vertical transitions
- Connect paths to site entrances
- Ensure path generation does not depend on a specific site builder

Matrix system:
- Partition underground space into deterministic 3D matrices
- Assign at most one site per matrix
- Generate site definition from world seed + matrix ID
- Register current/adjacent matrices before player reaches generated content
- Add safety checks and processed-matrix markers

Site archetype expansion:
- Keep SultanHistoric active for now
- Later add BasicLair archetypes back into selector:
  - BasicLairLegendary
  - BasicLairDense
  - BasicLairVendor

Testing:
- Build functional tests for path/site generation
- Build safety tests for vanilla-content collisions
- Build determinism tests across repeated worlds/seeds
- Document safety tests if they become complete enough to provide confidence


