Spike Log

Runtime Injection Discovery
- AddZoneBuilder(...) does not affect the zone that is already in the current build pipeline
- BeforeZoneBuiltEvent fires early enough to modify zones and/or register future zones
- ZoneManager.ApplyBuilderToZone(...) successfully applies builders directly
- Direct builder application is no longer the preferred primary architecture

Conclusion:
- Direct runtime builder application can work inside BeforeZoneBuiltEvent
- However, the preferred architecture is now runtime pre-registration:
  - register builders before the player reaches the generated zone
  - allow ZoneManager to build the zone normally when entered



IGameSystem Behavior
- Systems are NOT auto-registered
- Must call:
  The.Game.RequireSystem<T>()

Current entry point:
[JoppaWorldBuilderExtension]
public class UndergroundSiteJoppaWorldBuilderExtension : IJoppaWorldBuilderExtension
{
    public override void OnAfterBuild(JoppaWorldBuilder builder)
    {
        The.Game.RequireSystem<RuntimeZoneBuilderInjectionSystem>();
    }
}

Conclusion:
- Runtime systems must be explicitly registered during world initialization
- OnAfterBuild is a good place to require the runtime site-registration system



Event Timing
- ZoneActivatedEvent → too late; the zone has already been built
- BeforeZoneBuiltEvent → correct timing for mutation and pre-registration logic

Conclusion:
- Procedural site registration should occur from a runtime system responding to BeforeZoneBuiltEvent
- The long-term design is not to mutate the current zone directly
- Instead, use the event as a trigger to pre-register future site/path zones



Builder Types Identified

1. Full builders
   - SnapjawStockadeMaker
   - BasicLair
   - SultanDungeon

2. Decorators
   - Mines2
   - Adds features, but does not necessarily define a full layout

3. Context-dependent builders
   - SultanDungeon
   - Requires setup pipeline, arguments, zone properties, and/or game-state objects



Zone Naming Test
- The.ZoneManager.SetZoneName(...) works in runtime hooks
- Useful for debugging:
  - deterministic behavior
  - selected tier
  - selected sultan period
  - selected historic region
  - site layer index



Current Capability
- Deterministic runtime registration into specific underground zones
- Verified with registered future-zone builders
- Verified stacked vertical sites
- Verified that zone modifications persist after re-entry
- Verified two working site archetype prototypes:
  - BasicLair-style vertical site
  - SultanDungeon / historical-site-style vertical site



ZoneBuilder Definition Tests
- Moved to: Spike-Log_ZoneBuilderTestCatalog.md



Deterministic Zone RNG Test

Goal:
- Confirm that a stable string input produces repeatable deterministic results across separate new games

Test Setup:
- Targeted underground zones only (Z > 10)
- Used ZoneID as the stable input
- Generated seed using:
  XRLCore.Core.Game.GetWorldSeed(ZoneID + worldSeed)
- Created:
  System.Random(seed)
- Rolled:
  - rollA = rng.Next(0, 2)
  - rollB = rng.Next(1, 101)
- Wrote seed and rolls into zone name
- Repeated test across two separate new games

Result:
- Same zones selected in both games
- Selected zones had identical generated names
- Out of 4 tested zones, the same 2 were selected in both runs

Conclusion:
- Deterministic selection and RNG behavior is confirmed when using stable inputs



Important Limitation
- This does NOT prove that every vanilla builder output is deterministic
- Builders such as BasicLair and SultanDungeon use Qud internal RNG
- Only the pre-builder decision layer is deterministic
- The deterministic layer we control is:
  - whether a site exists
  - where it exists
  - what type it is
  - what registered builders/properties it receives



Decision
- Deterministic RNG model is validated
- Future site generation should use:
  world seed + matrix ID

Preferred API:
- XRLCore.Core.Game.GetWorldSeed("SubterraneanSites:" + matrixId)



Notes
- Displayed game seed and GetWorldSeed() output are different formats
- GetWorldSeed() returns the internal numeric seed derived from the game seed
- GetStringGameState("WorldSeed") is not suitable
- Correct API:
  XRLCore.Core.Game.GetWorldSeed()
- World seed is constant across a given game



Builder Registration Timing Test

Observation:
- Registering a builder with ZoneManager.AddZoneBuilder during BeforeZoneBuiltEvent does not affect the current zone already being built
- The same registration does affect future zones that have not yet been built
- Zone metadata such as names/properties can still update immediately

Conclusion:
- Direct BuildZone calls work for the current zone, but can produce behavior that differs from vanilla registered builders
- Future site generation should prefer pre-registering builders before the player enters generated zones



BasicLair Vertical Site Spike

Goal:
- Test a stacked underground site using BasicLair as a reusable vertical-site archetype

Results:
- Multi-layer BasicLair-style site generation works
- Stairs can be controlled per layer:
  - top: down
  - middle: up/down
  - bottom: up
- BasicLair structure is usable as a site archetype
- Additional custom population can be layered on after layout generation

Population Work:
- Created tiered single-mob XML tables
- Created tiered team/encounter XML tables
- Confirmed that XML population tables can be loaded from the active Qud mod folder
- Confirmed that custom builder calls can roll XML population tables
- Confirmed that Number="2-4" style entries spawn multiple objects when selected
- Confirmed that repeated table rolls are useful for density

Important Finding:
- Vanilla-style team tables are better than flat random individual mobs
- Qud encounter feel often comes from groups/packets:
  - snapjaw parties
  - baboon groups
  - spider/ooze lairs
  - faction parties
  - boss/leader packets

Status:
- BasicLair is a proven site archetype
- Current code does not yet include BasicLair in the site selector
- This is intentional for now to keep the current committed code tight around the working SultanHistoric branch



SultanDungeon / Historical Site Spike

Goal:
- Reuse Qud’s SultanDungeon system to create additional historical-site-like underground sites
- Avoid creating new sultans, quests, or map secrets during the first working implementation

Key Discovery:
- SultanDungeon requires a matching game-state object:
  The.Game.SetObjectGameState("sultanDungeonArgs_" + regionName, args)

- Then each layer can register:
  The.ZoneManager.AddZoneBuilder(
      zoneId,
      6000,
      "SultanDungeon",
      "locationName", locationName,
      "regionName", regionName,
      "stairs", stairs
  );

- regionName must match the suffix used in:
  sultanDungeonArgs_<regionName>

SultanDungeonArgs Construction:
- Working pattern:
  SultanDungeonArgs args = new SultanDungeonArgs();

  args.UpdateFromEntity(periodSultan.GetCurrentSnapshot());
  args.UpdateWalls(period);
  args.UpdateFromEntity(regionSnapshot);

  if (50.in100())
  {
      args.wallTypes.Add("*SultanWall*");
  }

Historical Inputs:
- Existing generated sultan history can be reused
- Existing generated region snapshots can be reused
- Existing sultan periods can be mapped from site tier using:
  SultanDungeon.GetSultanPeriodFromTier(targetTier)

Working Runtime Strategy:
- Pick an existing historical region matching the target period when possible
- Build SultanDungeonArgs from:
  - a period-matched sultan snapshot
  - the selected region snapshot
- Store args under a mod-specific region key:
  SubterraneanSites_<sourceRegionName>
- Register SultanDungeon on each vertical layer

Test Results:
- SultanDungeon site generation works outside vanilla AddSultanHistoryLocations
- Sites generated as multi-layer historical-site-like dungeons
- WFC/template structures appeared correctly
- Cult mobs appeared
- Cult mobs had cult-member social role text
- Region name appeared in diagnostic zone naming
- Tier/period diagnostic worked
- Repeated crash testing passed after one non-repeating early crash
- Top layer can preserve existing builders/connectors while lower layers are fully controlled

Vault / Relic / Hero Discovery:
- Setting:
  The.ZoneManager.SetZoneProperty(zoneId, "Relicstyle", "Vault");

- before SultanDungeon builds the bottom layer causes vanilla behavior:
  Relicstyle = Vault
  → SultanDungeon creates a vault region
  → SultanDungeon places a cult leader/hero
  → SultanDungeon creates or uses a relic chest/container
  → PlaceRelicBuilder places the relic in the vault chest if available

Relic Test Result:
- Bottom-layer pink relic chest appeared
- Tier-appropriate relic appeared inside
- Cybernetics credit wedge appeared
- Cult leader/hero appeared consistently or near-consistently

Status:
- SultanDungeon / historical-site-like archetype is now working
- Current committed selector forces this archetype
- The archetype has been wrapped in a nested registrar class:
  SultanHistoricSiteRegistrar



Current Code Organization Spike

Current Architecture:
RuntimeZoneBuilderInjectionSystem
    HandleEvent
    BuildSiteZoneIds
    RegisterSelectedSite
    RollSiteKind

    Shared helpers:
        GetStairsForLayer
        GetZFromZoneId
        GetTierFromZ
        IsClaimedByOtherContent

    Nested archetype registrar:
        SultanHistoricSiteRegistrar
            Register
            PickRegionForPeriod
            BuildSultanDungeonArgsFromHistory
            AddBottomLayerVaultWithRelicAndHero

Decision:
- Keep BuildSiteZoneIds(...) shared
- Keep site type selection outside the individual site archetypes
- Keep Sultan-specific helper code grouped inside SultanHistoricSiteRegistrar
- Add future site types as separate registrar classes rather than mixing all helper functions at the same level

Future likely structure:
- SultanHistoricSiteRegistrar
- BasicLairLegendarySiteRegistrar
- BasicLairDenseSiteRegistrar
- BasicLairVendorSiteRegistrar



Crash / GPU Note

A prior crash occurred during testing and was later associated with an NVIDIA driver/system instability rather than clear mod fault.

Summary:
GPU Driver Crash Note (May 5, 2026)
- BSOD: SYSTEM_THREAD_EXCEPTION_NOT_HANDLED (0x7E)
- Faulting module: nvlddmkm.sys
- Occurred after a game crash while loading with mod script
- Same mod scenario later loaded successfully
- Current interpretation: likely GPU driver/rendering edge case, not direct mod fault

Mitigation:
- Keep current driver if it fixes worse Project Zomboid crashes
- Prefer maximum performance GPU power mode if instability recurs
- Monitor for recurrence



Next Work

Path System:
- Next major implementation target:
  deterministic discovery paths leading toward generated underground sites

Likely work:
- Study vanilla path/trail systems again
- Build path material placement
- Build path holes / vertical transitions
- Connect path to site entrance
- Keep path system separate from site archetype builders

Safety Testing:
- Important future tests:
  - Waterlogged Tunnel adjacency / collision test
  - Known story-site collision tests
  - Existing special builder collision tests
  - Partial site behavior
  - Whole-site rejection behavior
  - Path collision behavior
  - Portal/drop/forced-arrival fallback behavior

Matrix System:
- Future generation model:
  matrix detection
  → matrix site selection
  → site pre-registration
  → path pre-registration

Need fallback for unusual arrival:
- If player arrives in a matrix before normal edge-trigger registration:
  - attempt late registration
  - avoid overwriting current/player-occupied zone
  - skip/defer if unsafe
