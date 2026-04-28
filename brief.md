# Subterranean Sites

## Overview
Subterranean Sites is a Caves of Qud mod that introduces deterministic, multi-zone underground “sites” that players can discover through exploration.

These sites:
- are generated when the player first encounters their zones
- exist at arbitrary depths
- consist of structured multi-zone layouts (target: 3×3 parasangs)
- use existing zone builders where possible

## Core Mechanics (Planned)

### Site Generation
- Sites are injected at runtime using BeforeZoneBuiltEvent
- Builders are applied directly to zones using:
  ZoneManager.ApplyBuilderToZone(...)

### Navigation
- Players locate sites using a compass system
- Compass indicates whether movement is closer or further from a site center
- Attunement stones act as anchors for navigation

### Progression
- Site centers may contain:
  - attunement stones
  - navigation clues (directions or riddles)
  - recoiler-like mechanics (e.g., consumable pebble)

## Current Focus
- Identify usable zone builders
- Validate multi-zone injection (starting with 1 → 2 zones)
- Establish stable runtime generation pattern