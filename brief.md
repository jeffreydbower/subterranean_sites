# Subterranean Sites — Brief

## Concept

**Subterranean Sites** is a Caves of Qud mod that adds hidden underground sites connected by long paths through the caves.

The core idea is simple:

```text
The underground should contain things worth finding.
```

Instead of placing sites directly on the world map or revealing them with explicit markers, the mod lets players discover them through exploration. Paths snake through nearby underground zones and may lead to ruins, lairs, bazaars, or historical-site-like structures.

## Player Experience

Players may encounter:

* winding underground paths
* vertical passages between strata
* hidden multi-level sites
* lairs and combat sites
* historical ruins
* rare underground bazaars

The paths are meant to be discoverable but not constant. A player may find one quickly, or may need to search for a while. The goal is to make underground travel feel more authored without making every cave zone feel crowded.

## Main Features

* Procedurally generated subterranean sites
* Multi-level vertical site structures
* Long discoverable underground paths
* Dynamic generation during exploration
* Deterministic placement by world/location
* Works in new games and existing saves
* Safety checks to avoid important vanilla content
* Discovery popup when entering a generated site

## Site Types

Current site archetypes:

* **Historic sites**
  SultanDungeon-style ruins using Qud’s historical-site systems.

* **Proper lairs**
  Coherent creature lairs using vanilla lair-owner and minion logic.

* **Chaos lairs**
  Denser combat sites with mixed encounters and rewards.

* **Underworld bazaars**
  Rare merchant-focused subterranean sites.

## Generation Model

The mod divides underground space into deterministic regions.

When the player explores, nearby regions are processed and may generate sites. Each site gets a path that can extend through nearby underground layers. This lets paths be found before the player reaches the site itself.

Generated content is registered through Qud’s zone-building system, so zones still build normally when entered.

## Safety Philosophy

The main rule is:

```text
Do not overwrite important vanilla content.
```

The mod protects known vanilla lairs, historical sites, special locations, and major fixed areas before placing sites or paths. If the required safety data cannot be initialized, generation fails closed.

The safety system is intentionally conservative. It is better for a site to fail to generate than to damage an important vanilla location.

## Compatibility Notes

Existing saves are supported. The mod begins generating content as new underground areas are explored.

Already-built zones in older saves may not retroactively receive generated content, which can create local gaps. This is expected and safer than trying to rewrite built zones.

A rare known edge case remains around intermediate segments of Shug’ruith’s path. The mouth and cradle/lair are protected, and testing found the route remained followable, but minor interference with the route may be possible in unusual cases.

## Project Status

The core release-candidate code is complete.

Current focus:

* documentation
* Steam Workshop setup
* art and preview images
* broader playtesting
* small bug fixes if reports come in
