Path Development Plan

Purpose

The path system will generate deterministic discovery paths that lead players toward generated underground sites.

A path should:
- begin at or near a generated site entrance
- extend outward and generally upward
- create a visible route through underground zones
- include vertical transitions where the path changes Z-level
- eventually stop at the surface if it reaches the surface
- remain deterministic from the same site/matrix seed
- stay separate from the site archetype builder system



Core Concept

The path generator will first produce an ordered sequence of absolute zone coordinates.

Example:
- path[0] = site entrance zone
- path[1] = one step north
- path[2] = one step north
- path[3] = one step up
- path[4] = one step west

Important:
- The coordinate sequence is the source of truth.
- Actual path materials, openings, holes, and transitions are derived from the sequence afterward.
- Path generation and path rendering should be separate steps.



Coordinate System

The generator should work in absolute directions:

- North
- South
- East
- West
- Up

Do not treat path instructions as purely relative to the previous segment.

Reason:
- When rendering a zone, the builder needs to know which absolute edges/connections are involved.
- A zone may need instructions like:
  - path enters from south and exits north
  - path enters from east and exits up
  - path enters from below/inside and exits west
- Absolute N/S/E/W/U instructions should be easier to translate into correct zone modifications later.

Future note:
- The path may also need downward information when rendering from the perspective of the zone above a vertical transition, even though generation should only move upward from the site.



Early Test Setup

For the first stage of path development, do NOT implement the full matrix system yet.

Use a fixed test site:

- Same current test site location
- Move the site entrance/top layer to depth -6
- This likely corresponds to Z = 16 if surface is Z = 10

Reason:
- -6 is deep enough to test underground path behavior
- It is near enough to Joppa / Waterlogged Tunnel to expose possible collisions
- It gives room for upward path transitions
- It keeps testing concrete and repeatable

Testing note:
- Use wish/debug movement if available to descend layers quickly.
- In early tests, overwriting or colliding with Waterlogged Tunnel is acceptable as a diagnostic.
- If the path or site hits Waterlogged Tunnel, that tells us it is in the crossfire and must be handled by later safety logic.



Temporary Test Matrix

Before implementing the real matrix system, define an arbitrary local test matrix around the fixed site.

Purpose:
- Allow early testing of:
  - path coordinate generation
  - path length
  - path movement rules
  - later directional bias
  - later matrix-edge behavior
  - later surface-stop behavior

Important:
- This test matrix does not need to match the final matrix dimensions.
- It is a development scaffold only.
- Later, the real matrix definition will be deterministic from matrix ID and world coordinates.

Initial implementation may ignore matrix limits entirely.

Recommended staged approach:
1. Generate path coordinates with no matrix limit.
2. Add path rendering.
3. Add vertical transition rendering.
4. Add no-immediate-backtracking rule.
5. Add surface stop rule.
6. Add temporary matrix boundary checks.
7. Add directional/depth bias.
8. Replace temporary matrix with real matrix system.



Initial Path Generation

Start simple.

Initial movement options:
- Up
- North
- South
- East
- West

Initial weights:
- Up: 20–25%
- North: 20–25%
- South: 20–25%
- East: 20–25%
- West: 20–25%

For the very first test, exact equal weights are acceptable.

Path length:
- Eventually target around 30–40 zones
- For early testing, use a shorter configurable length first if needed
- Example early test length: 10–15 steps
- Later test length: 30–40 steps

Generation direction:
- Generate outward/upward from the site
- The path should never intentionally move downward



Anti-Backtracking Rule

Not required for the first raw coordinate test, but should be added early.

Simple rule:
- Do not immediately reverse the previous horizontal move

Examples:
- North cannot be followed immediately by South
- South cannot be followed immediately by North
- East cannot be followed immediately by West
- West cannot be followed immediately by East

Reason:
- Prevents ugly one-step curls
- Keeps the path from wasting length
- Simple enough to implement without full pathfinding

Later improvement:
- Avoid any coordinate already present in the path, if possible.
- This prevents loops and self-crossing.
- If all options are blocked, allow fallback behavior rather than failing hard.



Bias Rules

Bias should be added after the basic path works.

Depth-based vertical bias:
- Closer to surface:
  - reduce Up weight
- Deeper underground:
  - increase Up weight

Reason:
- Deep sites need paths that climb enough to become discoverable.
- Shallow sites should not immediately punch to the surface every time.

Matrix-position bias:
- If the site starts near the north side of its matrix:
  - reduce North weight
  - increase South weight
- If the site starts near the south side:
  - reduce South weight
  - increase North weight
- If the site starts near the east side:
  - reduce East weight
  - increase West weight
- If the site starts near the west side:
  - reduce West weight
  - increase East weight

Reason:
- Give paths more room to develop inside the matrix.
- Reduce accidental matrix exits.
- Preserve the idea that each matrix owns its site/path content.

Important:
- Bias is not the first implementation target.
- First prove the coordinate path and rendering system.



Surface Rule

Eventually, if the path reaches the surface:

- generate a visible surface hole / entrance
- stop path generation
- do not continue generating path across the surface

Reason:
- Once the path reaches the surface, the entrance is enough.
- Continuing across surface zones is unnecessary and may interfere with surface content.

Early test:
- Surface stop can be added after vertical transitions work.
- At fixed test depth -6, the path may hit surface in a short number of upward rolls, which is useful for testing.



Matrix Boundary Rule

Eventually, paths should not leave their owning matrix.

Possible rule:
- If a proposed step would leave the matrix:
  - reject that step and reroll
  - or reduce that direction’s weight to zero
  - or terminate the path if no valid steps remain

Early development:
- Do not implement this immediately.
- Allow paths to roam freely first.
- Use this to observe natural failure/collision behavior.

Later:
- Enforce matrix bounds as a hard rule unless we decide to support cross-matrix paths.



Path Rendering / Zone Instructions

After generating the coordinate list, translate it into per-zone path instructions.

Each path zone should know:
- its coordinate
- previous coordinate, if any
- next coordinate, if any
- entry direction
- exit direction
- whether the path changes Z here
- whether a hole/opening is needed
- whether this is the site entrance
- whether this is a surface entrance

Examples:
- straight horizontal path:
  - entry: South
  - exit: North
  - render path material from south edge to north edge

- corner:
  - entry: East
  - exit: North
  - render turn from east edge to north edge

- vertical transition:
  - entry: West
  - exit: Up
  - render path material from west edge to a hole/opening
  - place upward/downward transition object as needed

- site entrance:
  - connect path material to the top layer of the site

Important:
- The rendering builder should not need to know how the path was generated.
- It should receive explicit instructions.



Path Materials

Candidate materials:
- dirt path
- stone path
- brick path
- water/stream path
- ruins-like path
- Girsh resin-like path

Initial test material:
- pick one simple visible material
- use it consistently

Later:
- path material can be selected by site type, depth, biome, or random deterministic theme.



Connections and Openings

Horizontal path connections:
- Must ensure path material reaches the relevant zone edge.
- May need to open walls or carve through obstructing terrain.

Vertical path connections:
- Use holes or equivalent vertical transition objects.
- Holes should be visually distinct.
- A vertical transition requires coordinated rendering between adjacent Z-level zones.

Important:
- Path generation moves upward from the site.
- Rendering may still need to place complementary connection objects in both involved zones.



Collision / Safety

Early path tests may ignore safety.

This is intentional:
- It helps reveal what important content is in the crossfire.
- Waterlogged Tunnel collision is useful diagnostic information.

Later safety rules:
- Do not overwrite important story/special content.
- Do not overwrite current player-occupied zone during late registration.
- Reject or reroute path if it crosses protected zones.
- Reject whole site if critical site footprint collides with important content.
- Path failures are more acceptable than site overwrites.

Known test target:
- Waterlogged Tunnel below Joppa



Testing Stages

Stage 1: Coordinate-only path
- Generate deterministic ordered coordinates.
- Print/store/name zones for debugging.
- No path material yet.

Stage 2: Simple path material
- Register a path builder for each coordinate.
- Lay down visible material.
- Ignore vertical transitions initially if needed.

Stage 3: Horizontal connection correctness
- Ensure N/S/E/W edges connect correctly.
- Test straight paths and corners.

Stage 4: Vertical transitions
- Add holes/openings for Up steps.
- Ensure transition makes sense between Z-levels.

Stage 5: Site entrance connection
- Ensure generated path visibly connects to the site entrance/top layer.

Stage 6: Surface stop
- If path reaches surface, create surface entrance and stop.

Stage 7: Anti-backtracking
- Prevent immediate reversal.
- Later prevent self-intersection if practical.

Stage 8: Bias
- Add depth-based Up bias.
- Add matrix-position directional bias.

Stage 9: Matrix boundary
- Add temporary test matrix limits.
- Then replace with real matrix system.

Stage 10: Safety tests
- Waterlogged Tunnel test.
- Special builder collision tests.
- Vanilla historical site collision tests.
- Current-zone late-registration test.



Open Questions

- Final matrix dimensions.
- Final path length distribution.
- Whether paths may ever cross matrix boundaries.
- Whether path generation should retry if trapped.
- Whether paths should prefer existing open terrain or carve through it.
- How aggressive path carving should be.
- How to represent water/river-style paths.
- How to handle paths that reach surface very early.
- How to handle paths that collide with protected content.
- Whether different site archetypes should use different path themes.



Near-Term Plan

Immediate next work:
- Keep current SultanHistoric test site but move entrance/top layer to -6.
- Generate a deterministic path coordinate list from the site entrance.
- Start without bias and without matrix limits.
- Use a short path length if needed for debugging.
- Convert coordinate list into per-zone path instructions.
- Add simple visible path material.
- Add vertical transitions after basic horizontal rendering works.

Do not implement yet:
- full matrix system
- final bias system
- full safety rejection
- polished path themes

Rationale:
- The path system is complex enough to develop independently.
- First prove the coordinate sequence and rendering pipeline.
- Then add safety, bias, and matrix integration.