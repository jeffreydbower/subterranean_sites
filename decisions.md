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
Use:
- gradient-based compass (closer/further)
- attunement stones as anchors

Avoid:
- direct coordinate systems
- impossible discovery chains (e.g., deep-only clues)

---

## Site Structure (Tentative)
Target:
- 3×3 parasang layout
- single Z-level per site

Open question:
- whether builders align cleanly across zones

---

## State Tracking
Do NOT introduce custom per-zone flags yet.

Reason:
- engine likely already manages build lifecycle
- avoid redundant state until proven necessary