# Encounter Density Test

## Purpose

This test estimated how far a player may need to travel underground before encountering a Subterranean Sites path or site under the current generation settings.

The goal was not to measure exact encounter probability under all normal play conditions, but to get a practical estimate of discovery distance after the matrix/path density changes.

## Generation Setup Tested

Current tested setup:

* Matrix size: `4 x 5 x 5`

  * 4 parasangs wide
  * 5 parasangs tall
  * 5 Z-levels deep
* Each matrix attempts 4 site slots:

  * Slot A: upper-left origin band
  * Slot B: upper-right origin band
  * Slot C: lower-left origin band
  * Slot D: lower-right origin band
* Site slot chance during testing: `100%`
* On zone activation, the mod processes a surrounding matrix block:

  * Current matrix
  * Adjacent matrices in a `3 x 3 x 3` block around the player’s current matrix
  * Invalid matrices at world/map edges are skipped
* Path length target:

  * `30–40` zones
* Path upward weighting:

  * `originZ <= 15`: low upward bias, `UpWeight = 2`
  * `originZ > 15`: stronger upward bias, approximately `UpWeight = 9–10` during tuning
* Paths use protected-zone-aware candidate rejection:

  * Candidate path steps into protected vanilla content or already-claimed Subterranean Sites zones are rejected before the path commits to them.
  * Final path filtering remains as a safety check.

## Method

A “find” was counted when the tester encountered either a Subterranean Sites path or site.

Distance was counted in zones traveled. For vertical tests, up/down zone transitions were included in the count.

Five informal search-condition tests were run:

1. Optimal condition:

   * Full surrounding matrix activation expected
   * Godmode descent to approximately Z12
   * Movement continued in one direction until a path/site was found

2. World-map edge condition:

   * Similar to Test 1, but near map edges where fewer surrounding matrices can be activated

3. Straight-down condition:

   * Continued from prior edge location
   * Godmode movement straight downward

4. Deep underground directional condition:

   * Movement in one direction underground
   * After each find, move up or down 2 layers and continue

5. Deep horizontal condition:

   * Mostly horizontal movement at approximately levels -8 to -12
   * After each find, move up or down 2 layers and continue

## Raw Data

### Test 1: Optimal

```text
2, 7, 1, 21, 1, 5, 13, 1, 5, 2, 8
```

### Test 2: Map Edge

```text
2, 4, 15, 2, 11, 22, 15, 47, 1, 22
```

### Test 3: Straight Down

```text
28, 1, 19, 3, 4, 28, 18, 1, 4, 1
```

### Test 4: Deep Directional + ±2 Z Shifts

```text
6, 32, 7, 1, 8, 9, 35, 5, 1, 2
```

### Test 5: Deep Horizontal + ±2 Z Shifts

```text
16, 5, 7, 5, 2, 3, 9, 11, 8, 6
```

## Summary Statistics

| Test                    |  n | Mean Zones |    SD | Median | Min | Max |
| ----------------------- | -: | ---------: | ----: | -----: | --: | --: |
| Optimal                 | 11 |        6.0 |  6.23 |    5.0 |   1 |  21 |
| Map Edge                | 10 |       14.1 | 14.08 |   13.0 |   1 |  47 |
| Straight Down           | 10 |       10.7 | 11.31 |    4.0 |   1 |  28 |
| Deep Directional + ±2 Z | 10 |       10.6 | 12.41 |    6.5 |   1 |  35 |
| Deep Horizontal + ±2 Z  | 10 |        7.2 |  4.10 |    6.5 |   2 |  16 |

## Grand Total

Across all tests:

```text
n = 51
mean = 9.65 zones
sample SD = 10.28 zones
median = 6 zones
min = 1
max = 47
```

Encounter distance thresholds:

| Distance Threshold | Encounters | Percent |
| ------------------ | ---------: | ------: |
| ≤ 5 zones          |    25 / 51 |   49.0% |
| ≤ 10 zones         |    35 / 51 |   68.6% |
| ≤ 15 zones         |    40 / 51 |   78.4% |
| ≤ 20 zones         |    43 / 51 |   84.3% |
| ≤ 30 zones         |    48 / 51 |   94.1% |

## Group Comparison

A one-way ANOVA comparing the five test categories did not detect a statistically clear difference between test conditions:

```text
F(4, 46) = 1.00
p = 0.415
```

A nonparametric Kruskal-Wallis check was consistent with this:

```text
p = 0.644
```

Interpretation: observed means varied by condition, especially at map edges, but the sample size was small and within-condition variability was high.

## Interpretation

The current density appears acceptable for release testing.

The typical search experience is reasonably short:

* Median encounter distance: approximately 6 zones
* Mean encounter distance: approximately 10 zones
* About 69% of searches found a path/site within 10 zones
* About 84% found one within 20 zones

However, the system has a real long tail. Searches longer than 10 zones are plausible, and searches longer than 15 zones can occur, especially near world-map edges or under less favorable path geometry.

This should be communicated to players: Subterranean Sites are discoverable by underground searching, but they are not guaranteed to appear immediately. A dry search of more than 10 zones is possible; more than 15 zones is an unusually poor but observed case.

## Design Conclusion

The current density is probably good enough.

Increasing density further would likely require either:

* More matrix fanout,
* More site slots per matrix,
* Higher slot chance,
* Longer paths,
* Or additional paths per site.

Generating additional paths per site is not recommended at this stage. Multiple paths per site would likely make the underground feel overbuilt and artificial. The current system already produces a meaningful number of paths while keeping sites themselves relatively special.

Remaining tuning should focus on minor path-weight adjustments and bug fixes rather than major density increases.

## Known Issues Noted During Testing

### Coral Path Rendering

Observed twice:

* The debug/reporting indicated a coral path.
* Associated path features such as statues appeared.
* The visible coral path/floor did not render.

Likely issue: path material blueprint/rendering mismatch, not coordinate generation failure.

### Discovery Popup Only on Origin Layer

Current discovery popup appears when entering the site origin layer. If a player digs into or otherwise enters a non-origin layer first, no discovery popup appears.

Desired behavior:

* Any generated site layer should be able to trigger discovery.
* All layers should share the same discovery key so the popup only appears once per site.
