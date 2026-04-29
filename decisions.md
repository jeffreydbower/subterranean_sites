# Design Decisions

## Runtime Generation Approach
Use:
- BeforeZoneBuiltEvent
- ZoneManager.ApplyBuilderToZone(...)

Reason:
AddZoneBuilder(...) occurs too late in the build pipeline.

---

## Builder Strategy
Prefer existing builders:
- SnapjawStockadeMaker (confirmed working)
- Mines2 (decorator)
- Others to be evaluated

Avoid:
- custom builders (for now)
- SultanDungeon (until context system understood)

---

## Determinism
Sites must be deterministic:
- same world → same site locations
- generation occurs at first encounter, not pre-baked

---

## Navigation System Direction
***Decided to not use this systen at all****
***DEveloping natural paths to entrance is the replacement***
Use:
- gradient-based compass (closer/further)
- attunement stones as anchors

Avoid:
- direct coordinate systems
- impossible discovery chains (e.g., deep-only clues)

## Path generation feature
- Each site will have an outward and upward path that connets an arbitrary point to the entrance
- Path will createan easy way to traverse to the site if the player finds one
- Pathing will be based / co-opted from shug'ruith's cradel path and other path based quests like amaranthine prism, the path to Klang, and the Path at Bey Lah.
- Spawn desnity will be tunes to allow apprpriate rate of dicsovery.
- sites randomly closer to the surface can connect to an unused or non-inportant zone making a surface entrance
- Developing the paths is expected to be a coding challenge



---

## Site Structure (Tentative)
Target:
- Traditional stacked layout 3 to 7 sones connected by stairs or holes. entry layer will be subteranean always.
- (out)3×3 parasang layout
- (out) single Z-level per site

Open question:
- (out) whether builders align cleanly across zones

---

## State Tracking
Do NOT introduce custom per-zone flags yet.

Reason:
- engine likely already manages build lifecycle
- avoid redundant state until proven necessary