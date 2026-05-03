Development Phases

1. Runtime Injection POC
- Prove the mod can intercept zone generation
- Confirm BeforeZoneBuiltEvent is the correct hook
- Confirm generated zones persist after re-entry

2. Source Code Catalog + Builder Testing
- Review relevant vanilla builders
- Identify usable builders vs reference-only builders
- Test builders in controlled zones

3. Procedural Generation POC
- Validate deterministic selection
- Validate stable seeded decisions
- Confirm zones can independently recompute their role

4. Stacked Site Development
- Generate vertical multi-layer sites
- Use BasicLair/SultanDungeon-style builders
- Confirm stairs and bottom-layer content
- Control unwanted lateral exits

5. Path Development
- Generate deterministic discovery paths
- Render path mouths and path materials
- Add holes / vertical path transitions as needed
- Connect paths to site entrances

6. Matrix Development
- Partition underground space into deterministic matrices
- Assign at most one site per matrix
- Ensure sites and paths remain within intended bounds

7. Safeguards / Base Game Protection
- Avoid overwriting story sites, quest zones, historical sites, and special content
- Reject risky matrices or zones
- Prefer skipping mod content over damaging vanilla content

8. Final Tuning and Gameplay Decisions
- Tune site density
- Tune path length and visibility
- Tune rewards, bosses, merchants, factions, and difficulty
- Decide what feels fun and Qud-like