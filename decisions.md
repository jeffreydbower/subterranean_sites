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

## PROCEDURAL GENERATION MODEL (SUBTERRANEAN SITES)

We use a deterministic, matrix-based system to define site placement and structure.

1. WORLD PARTITIONING
- Underground is divided into fixed-size 3D matrices (X, Y, Z buckets).
- Each matrix can contain at most one site.
- A site must be fully contained within its matrix (no cross-matrix sites).

2. SITE DEFINITION (DETERMINISTIC)
- Each matrix has a unique ID: (matrixX, matrixY, matrixZ).
- Site parameters are generated from:
    worldSeed + matrixID → deterministic RNG seed
- This RNG produces:
    - site existence (yes/no)
    - site position (local X/Y/Z inside matrix)
    - number of levels (vertical stack)
    - adjectives (layout/theme)
    - creature table
    - path definition (list of zones)

- Same matrix always produces the same site definition.
- No storage required for generation (can optionally cache later).

3. ZONE MEMBERSHIP
When a zone is entered:
- Compute its matrixID.
- Recompute that matrix’s site definition.
- Check:
    A) Is zone at site X/Y and within Z range? → site layer
    B) Is zone in path coordinate list? → path zone
    C) Else → normal zone

4. VERTICAL SITE STRUCTURE
- Site anchor is the top (entrance) zone.
- Site extends downward N levels.
- Any layer can be entered first; system still resolves correctly.

5. PATH SYSTEM (PLANNED)
- Path is a deterministic sequence of zones generated from site seed.
- Constrained to remain inside matrix (initial implementation).
- Path moves horizontally + upward bias (never deeper than site).
- Zones check membership by coordinate match.

6. DESIGN PRINCIPLES
- Deterministic > stored state for generation
- Matrix containment removes ambiguity (no “nearest anchor” logic)
- Separation of concerns:
    Table → creature pool
    Adjectives → structure/theme
    Path → navigation layer
- Zone generation is stateless; persistence handled by the game engine

STATUS:
- Concept validated
- Implementation pending (after adjective testing and builder exploration)