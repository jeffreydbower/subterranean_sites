# Subterranean Sites

**Subterranean Sites** is a Caves of Qud mod that adds procedurally generated underground sites connected by long discoverable paths through the underground layers.

The goal is to make underground exploration feel more alive with challenges and rewards without replacing Qud’s existing world. Sites are generated dynamically during exploration and are designed to avoid important vanilla locations, historic sites, lairs, and major special areas.

## Features

* Procedurally generated subterranean sites
* Long paths that snake through nearby underground zones
* Multi-level vertical site structures
* Deterministic generation based on world/matrix location
* Support for both new games and existing saves
* Safety checks to avoid protected vanilla content
* Site discovery popups when entering generated sites

## Site Types

Current site archetypes include:

* **Historic sites** — SultanDungeon-style ruins using Qud’s historical-site systems
* **Proper lairs** — coherent creature lairs using vanilla lair-owner logic
* **Chaos lairs** — denser mixed-combat sites with extra encounters
* **Underworld bazaars** — rare merchant-focused underground sites

## Paths and Discovery

Sites are not simply placed in isolation. Each site generates a path intended to help players discover it while exploring underground.

Paths may move horizontally, climb between strata, and pass around other subterranean features. They are meant to be found through exploration. A statue is placed near where paths exit zones in the direction of the site so players do not go backwards.

In testing, most encounters occurred within a short search distance, but dry stretches can happen. Searching more than 10–15 zones without finding a path or site is possible, especially near world-map edges.

## Safety and Compatibility

Subterranean Sites registers generated content through Qud’s zone-building system and includes multiple safety checks before placing sites or paths.

The mod attempts to protect:

* vanilla lairs
* legendary merchant lairs
* historical sites
* important named special locations
* several fixed story or settlement locations

If required safety data cannot be initialized, the mod fails closed and does not generate new content.

Zero sites were observed to be overwritten in extensive testing once saftey system was fully implemented.

A rare known edge case remains: Shug’ruith’s mouth and cradle/lair are protected, but the intermediate path between them may not be fully protected. Testing found the route remained followable, but minor visual or path interference is possible in unusual cases.

## Existing Saves

Existing saves are supported.

The mod begins generating content as new underground regions are explored. Already-built zones may not retroactively receive generated content, so very old or heavily explored saves may have local gaps.

## Status

Release-candidate code is committed.

Current focus:

* final documentation
* Workshop setup
* broader playtesting
* small bug fixes if reports come in

## Development Notes

This repository also includes internal development documents:

* `brief.md` — short nontechnical concept summary
* `decisions.md` — design decisions and rationale
* `spike-log.md` — development discoveries and implementation notes
* `test-plan.md` — testing checklist and release-readiness notes

These are included partly as project documentation and partly as a record of the development process.
