Design Decisions

---

Runtime Generation Approach

Original Decision:
Use:

* BeforeZoneBuiltEvent
* ZoneManager.ApplyBuilderToZone(...)

Reason:
AddZoneBuilder(...) occurs too late in the build pipeline.

CRITICAL NOTE (UPDATED):

This is no longer fully correct.

Updated Understanding:

* AddZoneBuilder DOES work
* BUT only for future zones, not the current zone being built

Updated Decision:

* Continue using BeforeZoneBuiltEvent as the runtime hook
* BUT shift from direct injection → pre-registration

New Model:

* Runtime system registers builders for future zones
* ZoneManager handles actual construction when zones are entered

Status:

```text
Direct runtime injection → deprecated as primary approach
Runtime pre-registration → primary architecture
Direct BuildZone → fallback only
```

---

Runtime System Entry Point

Decision:
Keep:

```csharp
[JoppaWorldBuilderExtension]
public class UndergroundSiteJoppaWorldBuilderExtension : IJoppaWorldBuilderExtension
{
    public override void OnAfterBuild(JoppaWorldBuilder builder)
    {
        The.Game.RequireSystem<RuntimeZoneBuilderInjectionSystem>();
    }
}
```

Updated Role:

```text
Old:
Inject and build zones at runtime

New:
Register builders ahead of player movement
Act as world-generation controller
```

---

Builder Strategy

Prefer existing builders:

* SnapjawStockadeMaker (confirmed working)
* Mines2 (decorator)
* BasicLair (current test target)
* SultanDungeon (now back in scope)

Avoid (initially):

* heavy custom builders

UPDATED:
Custom builders are now acceptable for:

* surface entrances
* path holes
* small targeted features

---

Determinism

Sites must be deterministic:

* same world → same site locations
* generation occurs at first encounter, not pre-baked

CRITICAL NOTE (UNCHANGED):

Determinism must NOT depend on the current zone being built.

All layers must derive from:

```text
world seed + matrix ID
```

---

Deterministic Site Definition RNG

Decision:
Use:

```text
GetWorldSeed("SubterraneanSites:" + matrixId)
→ System.Random
```

Controls:

* site existence
* site depth
* theme / table
* path structure

Important:

```text
Builder RNG does NOT need to be deterministic
Only site identity must be deterministic
```

---

Navigation System Direction

Decision:
Do NOT use compass/navigation system

Replace with:

```text
Natural path discovery
```

---

Path Generation

Each site will have:

```text
An outward and upward path
Connecting arbitrary point → site entrance
```

Path sources:

* Shug’ruith cradle
* Amaranthine Prism
* Klang path
* Bey Lah path

CRITICAL NOTE:

```text
Path system is separate from site builders
Must be deterministic
Must be reconstructible from seed
```

---

Vertical Movement Strategy

Decision:

```text
Sites use stairs (handled by builders)
Paths use holes
```

Rationale:

* holes are more visible
* stronger signal to player
* more “unnatural” / guided feel

Implications:

* need PathHole builder
* path material leads to hole
* hole should visually stand out

---

Site Structure

Decision:

```text
Sites are vertical columns (3–7 layers)
```

Removed:

* 3×3 parasang layout
* horizontal-only structures

CRITICAL NOTE:

```text
Site is defined as vertical structure
```

---

BasicLair Limitation

CRITICAL NOTE:

```text
BasicLair introduces lateral exits (E/W)
These must be overridden or replaced for final design
```

---

State Tracking

Original Decision:
Do NOT introduce custom per-zone flags

UPDATED:

We DO introduce minimal metadata:

```text
SubterraneanSites_Owner
MatrixProcessed markers
```

But:

```text
Generation itself remains stateless
All results must be recomputable
```

---

Procedural Generation Model

Matrix-based system:

World is divided into:

```text
3D matrices (X, Y, Z buckets)
```

Each matrix:

```text
- contains at most one site
- is fully deterministic
```

---

Matrix Pre-Registration System

Decision:

```text
Register site builders BEFORE player enters zones
```

Reason:

```text
AddZoneBuilder does not affect current zone
```

---

Matrix Boundary Rule

When processing matrices:

```text
Always process current matrix

If on edge:
    process adjacent matrix

If on corner:
    process:
        - 2 side neighbors
        - 1 diagonal neighbor
```

CRITICAL INSIGHT:

```text
Diagonal movement only occurs from corners
Therefore diagonal registration only needed at corners
```

---

Performance Model

```text
Normal: 1 matrix
Edge:   2 matrices
Corner: up to 4 matrices
```

Matrices are cached:

```text
Processed once only
```

---

Decision

```text
Do NOT implement zone-level approximation
Matrix system is implemented directly
```

---

Site Safety / Ownership Model

Problem:

```text
Builders overlap with vanilla (SultanDungeon)
```

Solution:

```csharp
SetZoneProperty("SubterraneanSites_Owner", "Yes")
```

---

Safety Rule

```text
If builder exists AND not ours:
    skip that zone

Allow:
- partial sites
- partial paths
```

---

Design Philosophy

```text
Never overwrite important vanilla content
Allow imperfect generation
```

---

Surface Interaction

Decision:

```text
Surface zones are NOT site zones
BUT trigger underground registration
```

---

Surface Entrance (Future)

```text
Optional surface hole builder
Triggered if path reaches near surface
```

---

Runtime Trigger Strategy

Current:

```text
Single-run initialization test
```

Future:

```text
Detect matrix boundaries dynamically
Pre-register adjacent matrices
```

---

Current Status

```text
✔ Builder registration works for future zones
✔ Direct-build confirmed unnecessary long-term
✔ Deterministic generation validated
✔ Matrix model defined
✔ Ownership system implemented

Next:
- SultanDungeon integration
- path system
- matrix implementation
```