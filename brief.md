Subterranean Sites

Overview
Subterranean Sites is a Caves of Qud mod that introduces deterministic, multi-zone underground “sites” that players can discover through exploration.

These sites:
- are generated when the player first encounters their zones
- exist at arbitrary depths
- consist of traditional stacked dungeon layouts of 3–7 layers (vertical column structure)
- use existing zone builders where possible
- each site will have a generated path that extends outward and upward for ~30–40 zones (water, dirt, brick, Girsh resin)

CRITICAL NOTE:
- Sites are vertical structures (same X/Y, varying Z), not multi-parasang layouts.



Core Mechanics (Planned)

Site Generation
- Sites are injected at runtime using BeforeZoneBuiltEvent
- Builders are applied directly to zones using:
  ZoneManager.ApplyBuilderToZone(...)

CRITICAL NOTE:
- Generation must occur before vanilla builders finalize the zone.
- Site membership must be computed deterministically for every zone entered.



Navigation
- Players will discover sites organically
- An outward and upward path of ~30–40 zones will provide a discoverable traversal route to the site
- Site generation density will allow discovery without dominating all underground exploration (potentially tunable)

Removed:
- Compass system
- Directional feedback system
- Attunement stones

CRITICAL NOTE:
- Path system must be deterministic and reconstructible from the same seed as the site.
- Paths are separate from site generation and should not depend on zone builder behavior.



Progression
- No explicit progression system
- Player goal is to find the site and reach the bottom layer
- Bottom layer contains:
  - artifact reward
  - potential boss (legendary NPC)

- Potential future addition:
  - rare legendary merchant sites

CRITICAL NOTE:
- Reward placement must be deterministic and tied to site definition, not per-zone randomness.



Current Focus
- Understand zone builder parameters (creature tables, adjectives) (ongoing)
- Develop generative multi-zone stacked structure connected via game systems (not just visual stacking)
- Establish stable runtime generation pattern

Completed:
- Identify usable zone builder (BasicLair)



Current Technical Status

Confirmed:
- Runtime injection through BeforeZoneBuiltEvent works
- Generated zones persist after re-entry
- Multi-zone deterministic selection works
- Stable ZoneID-based RNG produces repeatable decisions across separate new games
- Constructed static-location multi-layer site
- Deterministic layer count implemented
- Bottom-layer boss (legendary NPC) successfully placed

CRITICAL NOTE:
- Current system must ensure all layers agree on site parameters (seed must not vary by layer)



Next Technical Direction
- Link layers using game systems (stairs, holes, or custom connections)
- Control or eliminate lateral exits introduced by BasicLair
- Integrate deterministic path generation
- Evaluate use of SultanDungeon for more complex, themed site generation


Architecture Update

The project still uses a runtime system registered through JoppaWorldBuilderExtension. However, the runtime system is shifting from direct zone mutation to pre-registration.

The runtime system will eventually:
- detect current matrix / nearby matrix boundaries
- generate deterministic site definitions
- register builders for future site/path zones
- avoid directly building the current zone except as a fallback