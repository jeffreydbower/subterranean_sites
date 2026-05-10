Development Phases

1. Runtime Injection POC

Goals:
- Prove the mod can intercept zone generation
- Confirm BeforeZoneBuiltEvent is the correct hook
- Confirm generated zones persist after re-entry

Status:
- Complete

Results:
- BeforeZoneBuiltEvent is the correct runtime hook
- ZoneActivatedEvent is too late
- Runtime systems must be explicitly registered via:
  The.Game.RequireSystem<T>()



2. Source Code Catalog + Builder Testing

Goals:
- Review relevant vanilla builders
- Identify usable builders vs reference-only builders
- Test builders in controlled zones

Status:
- Mostly complete for the current phase
- Continuing as needed

Results:
- BasicLair is usable
- SultanDungeon is usable but requires a setup pipeline
- Mines2 and similar builders are decorator-style
- Path-related builders still need additional study



3. Procedural Generation POC

Goals:
- Validate deterministic selection
- Validate stable seeded decisions
- Confirm zones can independently recompute their role

Status:
- Complete

Results:
- Deterministic RNG model validated
- Site identity should derive from:
  world seed + matrix ID
- Builder internals do not need to be perfectly deterministic as long as site identity and registration decisions are deterministic



3.5. Runtime Pre-Registration Test

This occurred during stacked site development.

Goal:
- Test the preferred architecture where generated site builders are registered before the player reaches the zones

Steps:
- Run a one-time registration trigger near game start
- Register a fixed stacked test site using ZoneManager.AddZoneBuilder
- Do not directly call BasicLair.BuildZone
- Enter the registered zones and observe whether they build correctly

Success Criteria:
- Registered zones build correctly when entered
- Current-zone direct build is not required
- Site layers retain intended stair behavior

Status:
- Complete

Results:
- Pre-registration works
- AddZoneBuilder(...) works for future zones
- Direct current-zone building is not required for the intended architecture



4. Stacked Site Development

Goals:
- Generate vertical multi-layer sites
- Use existing vanilla builders where possible
- Confirm stairs and bottom-layer content
- Control unwanted lateral exits

Status:
- Active / partially complete

4A. BasicLair Stacked Site

Status:
- Working prototype

Results:
- BasicLair-style vertical site works
- Custom tiered singles/team population tables work
- BasicLair can serve as a future archetype for:
  - legendary lair
  - dense mob lair
  - vendor/workshop lair

Not currently wired into selector:
- Intentional for now

Reason:
- Keep current committed code tight around the working SultanHistoric archetype
- Add BasicLair archetypes later after path/matrix framework is clearer



4B. SultanDungeon / Historical-Site Stacked Site

Status:
- Working prototype
- Feature-complete for first archetype

Results:
- SultanDungeon can be reused outside vanilla historical site placement
- Existing generated sultan/region history can be reused
- Cult mobs populate correctly
- Region name, tier, and period diagnostics worked
- Bottom vault relic works
- Bottom vault hero/leader works via vanilla Relicstyle = Vault behavior
- Site code now sits behind archetype selector as SultanHistoric



5. Path Development

Goals:
- Generate deterministic discovery paths
- Render path mouths and path materials
- Add holes / vertical path transitions as needed
- Connect paths to site entrances

Status:
- Next major phase

Expected direction:
- Path system should be separate from site builders
- Path builder likely runs near site registration
- Path should guide players toward site entrance
- Paths may extend upward/outward from the site
- Paths should avoid destructive overwrites

Open design questions:
- Path material types:
  - stone
  - dirt
  - river/water
  - ruins-like path
- How often paths cross Z-levels
- How visible path mouths should be
- How path interacts with natural caves and special zones



6. Matrix Development

Goals:
- Partition underground space into deterministic matrices
- Assign at most one site per matrix
- Ensure sites and paths remain within intended bounds
- Register relevant nearby matrices before the player enters generated content

Status:
- Upcoming

Core idea:
- world seed + matrix ID
  → deterministic site decision
  → deterministic site type
  → deterministic site origin
  → deterministic path

Need dynamic detection:
- On zone entry:
  - determine current matrix
  - process current matrix
  - if at matrix edge, process neighbor
  - if at matrix corner, process diagonal as needed

Important edge case:
- Portal/drop/forced movement may place player in a matrix before normal edge-trigger registration

Fallback needed:
- If matrix has not been processed:
  - attempt late registration
  - avoid overwriting current/player-occupied zone
  - skip/defer if unsafe



7. Safeguards / Base Game Protection

Goals:
- Avoid overwriting story sites, quest zones, historical sites, and special content
- Reject risky matrices or zones
- Prefer skipping mod content over damaging vanilla content

Status:
- Partially implemented
- Needs formal testing

Current safety behavior:
- Site zones check existing builders
- Owned zones use:
  SubterraneanSites_Owner

Future behavior likely needed:
- If any critical site zone conflicts:
  - reject whole site

- If path zone conflicts:
  - skip or route around where possible

- If player is already in a zone:
  - do not overwrite that zone

Known safety test target:
- Waterlogged Tunnel



8. Final Tuning and Gameplay Decisions

Goals:
- Tune site density
- Tune path length and visibility
- Tune rewards, bosses, merchants, factions, and difficulty
- Decide what feels fun and Qud-like

Status:
- Future

Candidate site archetype weights discussed earlier:
- SultanHistoric: about 40%
- BasicLairLegendary: about 30%
- BasicLairDense: about 20%
- BasicLairVendor: about 10%

These are not final.



9. Test Plan / Release Confidence

Testing will be difficult because:
- Qud world space is large
- Generation is procedural
- Sites may be rare
- Safety failures could be location-specific
- Special content may be distributed unpredictably

Future test categories:

Functional tests:
- site generation
- path generation
- stairs/holes
- relic placement
- cult mobs
- BasicLair population
- matrix selection

Safety tests:
- Waterlogged Tunnel
- known story locations
- vanilla historical sites
- villages
- lairs
- ruins
- surface regions
- current-zone late-registration edge case

Determinism tests:
- same seed produces same matrix/site decisions
- same matrix ID produces same archetype
- registration does not duplicate

Possible publication artifact:
- Safety test list / results summary

Only publish safety tests if they are good enough and complete enough to provide real confidence.
