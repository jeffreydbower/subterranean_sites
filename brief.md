# Subterranean Sites

## Overview
Subterranean Sites is a Caves of Qud mod that introduces deterministic, multi-zone underground “sites” that players can discover through exploration.

These sites:
- are generated when the player first encounters their zones
- exist at arbitrary depths
- (Current Idea) Consist of traditional stacked dungeon layout of 3-7 layers (old idea)consist of structured multi-zone layouts (target: 3×3 parasangs)
- use existing zone builders where possible
- (new idea) each site will have a generated path that will go outward and upward for ~30-40 zones (water, dirt, brick, grish resin)

## Core Mechanics (Planned)

### Site Generation
- Sites are injected at runtime using BeforeZoneBuiltEvent
- Builders are applied directly to zones using:
  ZoneManager.ApplyBuilderToZone(...)

### Navigation
- Players will need to discover the sites organically. 
- an outward and upward path of 30-40 zones (current guess of best length) generated path will create an easy traversal path player can also discover that will lead them to the site.
- Site generation density will be sufficiently dense to allow discovery, but not completely dominate every move through the under ground. Perhaps this can be a tunable setting. 
- (Not moving forward)Players locate sites using a compass system
- (Not Moving forward) Compass indicates whether movement is closer or further from a site center
- (Not moving forward)Attunement stones act as anchors for navigation

### Progression
- No explicit prgression besides finding the site and getting tothe bottom of it where the artifact and maybe a boss (legendary npc) is.
- (out)Site centers may contain:
  - (out)attunement stones
  - (out)navigation clues (directions or riddles)
  - (out) recoiler-like mechanics (e.g., consumable pebble)

## Current Focus
- Identify usable zone builder (ongoing)
- Understand zonebuilding parameters (creaturetables, adjetives) (ongoing)
- Develop generative multi-zone stacked structure
- Establish stable runtime generation pattern
- (out)Validate multi-zone injection (starting with 1 → 2 zones)
