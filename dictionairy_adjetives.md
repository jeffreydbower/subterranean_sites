BASICLAIR / SULTANDUNGEON ADJECTIVE DICTIONARY

Purpose:
This file documents the adjective/property mapping used by SultanDungeonArgs.

BasicLair exposes:
- Table
- Adjectives
- Stairs

BasicLair calls SultanDungeon.BuildRandomZoneWithArgs using the table prefix:

Lairs_

The Adjectives string is split by commas. Each input term is lowercased and passed through SultanDungeonArgs.translateArg(). If the input term appears in the dictionary below, it maps to the corresponding internal property. If it does not appear, the raw input term is used as the property.

Example:
lair.Adjectives = "market,middle-class";

market maps to Market
middle-class maps to Residential

Both properties are then active. They do not have positional meaning. The first adjective is not primary and the second adjective is not secondary.

For each property, the system checks whether matching population tables exist. If they exist, they are added to the candidate lists used during dungeon generation.

Checked table patterns:
<tablePrefix>Wall_<Property>
<tablePrefix>PrimaryTemplate_<Property>
<tablePrefix>SecondaryTemplate_<Property>
<tablePrefix>DetailTemplate_<Property>
<tablePrefix>Green_<Property>
<tablePrefix>Blue_<Property>
<tablePrefix>Yellow_<Property>
<tablePrefix>Segmentations_<Property>
<tablePrefix>Furnishings_<Property>
<tablePrefix>Halls_<Property>
<tablePrefix>Cubbies_<Property>
<tablePrefix>Encounters_<Property>
<tablePrefix>PreconnectEncounters_<Property>

For BasicLair, tablePrefix is usually:

Lairs_

So examples include:
Lairs_Wall_Temple
Lairs_PrimaryTemplate_Temple
Lairs_Encounters_Temple
Lairs_Furnishings_Temple

If no matching table exists for a category, the system falls back to defaults.


FULL ADJECTIVE / PROPERTY MAPPING

Input terms mapping to Scholarship:
scholarship
scholar
academy
scribe
scriptorium
philosopher
college
historian
scientist
laboratory

Input terms mapping to Circuitry:
tinker
electrician
circuitry

Input terms mapping to Travel:
travel
explorer
nomad

Input terms mapping to Warrior:
might
soldier
barracks
arena
gladiator

Input terms mapping to Stars:
stars
astrologist
observatory
astronomer
stargazer
light

Input terms mapping to Glass:
glass
window maker
glassblower

Input terms mapping to Jewels:
jewels
jeweler
geologist
dig site

Input terms mapping to Time:
time

Input terms mapping to Salt:
salt
cook
tavern

Input terms mapping to Ice:
ice
winter eremite
hermitage

Input terms mapping to Chance:
chance
gambler
gambling hall

Input terms mapping to Plants:
gardens

Input terms mapping to Liquids:
pools

Input terms mapping to Art:
museums
theaters

Input terms mapping to Waste:
waste

Input terms mapping to Consulate:
consulate

Input terms mapping to Market:
market

Input terms mapping to Forums:
forums

Input terms mapping to Residential:
middle-class
residential

Input terms mapping to Storage:
food storage

Input terms mapping to Pipes:
pipe hub

Input terms mapping to Prison:
prison

Input terms mapping to Temple:
temple

Input terms mapping to City-States:
city-state

Input terms mapping to Precincts:
precinct

Input terms mapping to Provinces:
province

Input terms mapping to Districts:
district
districts

Input terms mapping to Quarters:
quarter
quarters

Input terms mapping to Poor:
poor

Input terms mapping to Rich:
rich

Input terms mapping to Godless:
godless

Input terms mapping to Sultans:
sultans


INTERNAL LISTS POPULATED BY SULTANDUNGEONARGS

wallTypes:
List of wall blueprint names selected from matching Wall tables.

primaryTemplate:
List of primary WaveCollapse template names selected from matching PrimaryTemplate tables.
Default count setting: primaryTemplateN = 3

secondaryTemplate:
List of secondary WaveCollapse template names selected from matching SecondaryTemplate tables.
Default count setting: secondaryTemplateN = 3

detailTemplate:
List of detail template names selected from matching DetailTemplate tables.
Default count setting: detailTemplateN = 3

greenObjects:
List of population table names for green-coded map output objects.

yellowObjects:
List of population table names for yellow-coded map output objects.

blueObjects:
List of population table names for blue-coded map output objects.

segmentations:
List of segmentation table names.

preconnectencounters:
List of population table names placed before connection/pathing steps.

furnishings:
List of population table names used to furnish rooms.

encounters:
List of population table names used for main encounters.

halls:
List of population table names used in hall regions.

cubbies:
List of population table names used in small side regions.

cultFactions:
Used when a true Sultan/historic cult context exists.

enemyFactions:
Used when a true Sultan/historic cult context exists.

properties:
The final list of translated properties produced from the adjective inputs.

tablePrefix:
Defaults to SultanDungeons_ in SultanDungeonArgs.
BasicLair passes Lairs_.


DEFAULT FALLBACK TABLES

If no wallTypes were found:
<tablePrefix>Wall_*Default

If no primaryTemplate entries were found:
<tablePrefix>PrimaryTemplate_*Default

If no detailTemplate entries were found:
<tablePrefix>SecondaryTemplate_*Default

If no secondaryTemplate entries were found:
<tablePrefix>SecondaryTemplate_*Default

If no greenObjects were found:
<tablePrefix>Green_*Default

If no yellowObjects were found:
<tablePrefix>Yellow_*Default

If no blueObjects were found:
<tablePrefix>Blue_*Default

If no preconnectencounters were found:
<tablePrefix>PreconnectEncounters_*Default

If no encounters were found:
<tablePrefix>Encounters_*Default

If no furnishings were found:
<tablePrefix>Furnishings_*Default

If no cubbies were found:
<tablePrefix>Cubbies_*Default

If no halls were found:
<tablePrefix>Halls_*Default


IMPORTANT NOTES

Multiple adjectives are additive, not positional.

Example:
lair.Adjectives = "market,middle-class";

This produces:
Market
Residential

The generator then checks table support for both Market and Residential across all supported categories.

There is no rule that the first adjective controls primary templates or that the second adjective controls secondary templates.

Aliases exist because history/narrative generation may describe the same theme in different ways.

Example:
cook, tavern, and salt all map to Salt.

For intentional mod design, it may be clearer to use canonical property names or obvious inputs:
temple
warrior
market
residential
waste
gardens

However, if using canonical property names directly, remember that translateArg only maps lowercase dictionary keys. Unknown inputs pass through unchanged, so using "Warrior" may work as a property if table names match, but using known lowercase inputs is safer unless tested.

Known-safe input style:
lair.Adjectives = "temple";
lair.Adjectives = "soldier";
lair.Adjectives = "market,middle-class";

Less clear until tested:
lair.Adjectives = "Warrior";
lair.Adjectives = "Residential";


OPEN QUESTIONS

Which properties have real Lairs_ table support?
Which properties affect layout versus only decoration or population?
Which adjective combinations produce good underground lair sites?
Can we enumerate Lairs_ tables directly from population XML or compiled population data?

## Testing update
## BasicLair Adjective Support (Tested)
BasicLair uses the Lairs_ table prefix. In testing, most high-level thematic adjectives (e.g., soldier, temple, market, stars, tinker) did not produce observable changes and appear to fall back to default tables.
The following adjectives are confirmed or highly likely to be supported (non-default behavior observed or strongly implied by table presence):
workshop
armorer
chef
glover
gunsmith
haberdasher
hatter
ichormerchant
modernscribe
shoemaker
gemcutter
gutsmonger
These appear to be primarily vendor/workshop-oriented and tend to produce increased container/loot presence (e.g., chests) and merchant/vendor-style furnishing patterns.
Conclusion:
BasicLair adjective support is limited and specialized. It is not suitable for broad thematic control of dungeon style and is better suited for merchant/outpost-style lairs.