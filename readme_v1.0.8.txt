[h1]Subterranean Sites — v1.0.8 Now with extradimensional pockets!!! [/h1]

This mod adds procedurally generated subterranean sites connected by long discoverable paths through the underground. Sites are generated dynamically during exploration and are designed to avoid protected vanilla locations, historic sites, lairs, and major special areas.

[h2]Game Features[/h2]

~ Procedurally generated subterranean sites

~Five types of sites, including subterranean historic sites as well as the new fully original site type, extradimensional pockets (see below for more info).
~~~~ Creatures and loot scaled to surface area and depth.
~~~~ Three to six layers in each site.
~~~~ Loot!!!

~ Procedurally generated snaking paths that lead to site entrances
~~~~ Uses holes for downward transitions instead of stairs.
~~~~ Statues near the path exit indicate the direction of the site.
~~~~ Paths are tuned to not ascend more than about 5 layers on average, to allow them to be consistent with the site they lead to.
~~~~ Paths are 12 to 20 zones long and propagate in a manner that avoids breaks, unless they hit the surface.

[h2]What to Expect[/h2]

To find sites look underground for a path or a hole in the ground and follow the path in the direction of the statues near the exit to the zone. Site and path density were carefully tuned in testing to allow discovery after a reasonably short search. However, since this mod bows to the will of the RNG, sometimes long streaks of not discovering a path could occur.

The historic sites use existing lore and are just like in the game, with a tier-appropriate hero and relic. There are two lair variants intended to be longer and more dangerous than normal, but also have more rewards. There are the unpredictable and dangerous extradimensional pockets described below. And finally there is another rare site type.

[h2]extradimensional pockets[/h2]
“The Eleven do not meet peaceably. Where two press their borders together, their laws abrade one another until a little contradictory world is pinched off between them, a pocket bearing the inheritance of both.
I have discerned five aspects by which each realm imposes itself upon creation, and each aspect of a pocket appears to descend from one parent or the other. By the arithmetic of the Binary Honorum, there can be one thousand seven hundred and sixty such conjunctions.
Yet no conjunction is an equality. One realm is ever ascendant, and through attunement the traveler may for a little while submit his flesh to its law, becoming fit to endure the strange ordinance of that place.”
~a fragment from the recovered journal of a Putus Templar scribe 

Extradimensional pockets extend Qud’s existing 11 extradimensions into real places with their own environmental hazards and denizens. 

There has been a hypertranslational event causing collisions between the extradimensions. A pocket appears from the collision of 2 distinct extradimensions and will contain features from both.. 

The extradimensional pockets all have an environmental hazard, unique effects, and their own denizens.

Each pocket will have an attunement stone at the entrance scar and on each level. 

The attunement stone will adapt the player to the dominant extradimensional environmental hazard, grant powers that denizens possess, and possibly more.  One will always be near the entrance scar and placed randomly within each layer. Attunement is temporary and must be renewed while exploring the pocket. It won’t protect you from everything. Aspects from the other non-attuned extradimension could remain dangerous.

Extradimensional pockets may be extremely hostile if the attunement stone is not used and attunement maintained while inside.

On the bottom layer there is an extradimensional hero guarding something. Hopefully it will be with the risk you took entering the pocket.

The bottom layer features a teleporter to bring you back to the entrance because once you enter it’s a one way trip.

[h2]Install and Uninstall Behavior[/h2]

~In the options under Mods enable Allow Scripting Mods. 
~The mod works if you install it before a new game and on an existing save.
~ If you install on an existing save, zones you have already visited will not be affected.
~ IMPORTANT!!! If you uninstall the mod, any changes made by the mod will remain. Once a zone is changed by this mod, the game treats it like its own and that content cannot be removed. The mod will stop affecting new areas.

[h2]Safety First[/h2]

Extensive work and testing was put into ensuring that this mod does not break any vanilla content. Once the safety system was working, there were zero observed instances of the mod overwriting protected content.

However, while Shug'ruith's mouth and cradle are 100% protected, there is a very, very, very small chance the resin path could be affected in a way that makes it difficult to follow. In testing it was never a problem, but be warned if you are going to attempt that.
```
[h2]Compatibility[/h2]

Subterranean Sites tries to avoid vanilla special locations, historic sites, lairs, settlements, and other protected content. If another mod adds a fixed underground location, Subterranean Sites may not know about it automatically.

Version 1.0.3 includes an optional external-exclusion template for other modders (SubterraneanSitesExternalExclusions.cs). A mod author can copy the provided .cs file into their own mod, enter the coordinates they want protected, and Subterranean Sites will avoid generating paths or sites through those areas when both mods are installed. The file safely does nothing if Subterranean Sites is not installed.

[h2]Development[/h2]
[url=https://github.com/jeffreydbower/subterranean_sites]View the source code on GitHub[/url]

[h2]Updates[/h2]
v1.0.1
- Fix Asphalt Mines protection coordinates/range.
- Prevent Subterranean Sites from generating sites or paths inside the Asphalt Mines.

v1.0.2
-Protected the gates and passage to the Tomb of the Eaters.

v1.0.3
-Added a superior redundant system to protect Historic Sites when there are more than 8.
-Added support for external zone and parasang exclusion to be use by other mods for compatability. 

v1.0.4
-Fixed incorrect coordinates for the Rusted Archway which will be properly protected now.

v1.0.5
-Added protection for edge case of Historic Sites being deeper than 8 layers.

v1.0.6
-Added protection for special dynamicly generated locations The Great Artifact Pieces, Kindrish, Stopsvalinn, The Ruin of House Isner, The Hydropon, The Recoming Nook at Gyl, Mamon's Village, The Glowpad Merchant, and The Hollow Tree.

v1.0.7
-Improved creature spawns in the Contested Lair site type. Working as intended now.

V1.0.8
-Added extradimensional pockets
-Increased the density of sites per matrix from 4 to 8 and reduced the path length to 12-20. This will shorten the amount of time a player spends on paths while maintaining the same discovery density.

[h2]Subterranean Sites work amazing with my other mod, Atlas of Qud[/h2]

[url=https://steamcommunity.com/sharedfiles/filedetails/?id=3767493819]Atlas of Qud[/url]
