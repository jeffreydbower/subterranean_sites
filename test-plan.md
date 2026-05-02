# Test Plan

## Phase 1 — Builder Identification

Goal:
Identify which builders can be applied standalone.

Steps:
- Apply builder to known test zone
- Observe:
  - full layout vs partial decoration
  - stability (no broken zones)

Success Criteria:
- Builder produces consistent, usable zone

---

## Phase 2 — Single Zone Injection

Goal:
Confirm stable runtime injection

Steps:
- Inject one zone using ApplyBuilderToZone
- Enter zone

Success Criteria:
- Zone generates correctly
- No crashes or artifacts

---

## Phase 2B — Deterministic RNG Selection Test

Goal:
Confirm that deterministic rolls can be reproduced from stable zone input.

Steps:
- Use ZoneID as seed input
- Hash ZoneID into an integer seed
- Create System.Random(seed)
- Roll one or more values
- Write those values into the generated zone name
- Test the same traversed zones across two separate new games

Success Criteria:
- Same zone IDs produce same rolls
- Same zones are selected for modification
- Zone names match across games

Result:
PASS

Observed:
- Out of 4 tested zones, the same 2 zones were selected in both new games
- Selected zones had matching deterministic names/rolls

Limitation:
- Did not test whether the BasicLair layout itself is deterministic.
- This only tested deterministic pre-builder selection logic.



## Phase 3 — Multi-Zone Injection (Immediate Next Step)

Goal:
Validate multiple independent injections

Steps:
- Add a second target zone
- Apply same or different builder

Success Criteria:
- Both zones generate correctly
- No cross-interference

---

## Phase 4 — Adjacency Test

Goal:
Test interaction between nearby zones

Steps:
- Place two injected zones adjacent
- Traverse between them

Success Criteria:
- No broken transitions
- No invalid geometry

---

## Phase 5 — Site Prototype (Later)

Goal:
Test small multi-zone structure

Steps:
- Define small cluster (e.g., 2–4 zones)
- Apply builders

Success Criteria:
- Zones feel connected
- No major layout conflicts

---

## Notes

- Do NOT optimize or generalize early
- Focus on proving behavior before scaling