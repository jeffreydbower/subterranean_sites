using System;
using System.Collections.Generic;
using XRL;
using XRL.Core;
using XRL.World;
using XRL.World.WorldBuilders;

namespace SubterraneanSites
{
    [JoppaWorldBuilderExtension]
    public class UndergroundSiteJoppaWorldBuilderExtension : IJoppaWorldBuilderExtension
    {
        public override void OnAfterBuild(JoppaWorldBuilder builder)
        {
            The.Game.RequireSystem<RuntimeZoneBuilderInjectionSystem>();
        }
    }

    public class RuntimeZoneBuilderInjectionSystem : IGameSystem
    {
        private const string TargetZoneId = "JoppaWorld.11.22.0.1.11";
        private const string OwnerProperty = "SubterraneanSites_Owner";
        private const string InitFlag = "SubterraneanSites_TestSiteRegistered";

        /*
        Site Safety / Ownership Model

        Problem:
        Subterranean Sites may eventually use the same builders as vanilla content,
        especially SultanDungeon. Therefore, checking only whether a zone has a
        SultanDungeon builder is not enough. It may be vanilla content, or it may
        be one of our generated sites.

        Decision:
        When Subterranean Sites registers a zone builder, it must also mark the zone with:

        SubterraneanSites_Owner = Yes

        Safety rule:
        - If a candidate zone has important existing builders and is not marked as ours,
          reject the whole candidate site.
        - If a candidate zone is marked as ours, it can be safely recognized as already
          assigned by this mod.
        - Prefer skipping a generated site over overwriting vanilla or quest content.

        Current test strategy:
        - Simulate future matrix-adjacent pre-registration by registering the fixed test site once.
        - The first time this system sees any zone build, it registers the test site.
        - After that, the system exits cheaply and does not recompute registration every zone.

        Future build strategy:
        - Detect current matrix from player/current zone position.
        - If current zone is on a matrix edge, register the adjacent matrix.
        - If current zone is on a matrix corner, register the two side-adjacent matrices
          and the diagonal matrix.
        - Surface zones are not site zones, but they can trigger underground matrix registration.
        */

        public override void Register(XRLGame game, IEventRegistrar registrar)
        {
            registrar.Register(BeforeZoneBuiltEvent.ID);
        }

        public override bool HandleEvent(BeforeZoneBuiltEvent zoneBuildEvent)
        {
            // One-time registration test.
            // Later this becomes matrix-boundary / matrix-corner registration logic.

            if (The.Game.GetStringGameState(InitFlag) == "Yes")
            {
                return true;
            }

            The.Game.SetStringGameState(InitFlag, "Yes");

            int rawSeed = XRLCore.Core.Game.GetWorldSeed();
            int zoneSeed = XRLCore.Core.Game.GetWorldSeed(TargetZoneId + rawSeed);
            System.Random rng = new System.Random(zoneSeed);
            int layers = rng.Next(2, 7);

            List<string> siteZoneIds = BuildSiteZoneIds(TargetZoneId, layers);

            RegisterSiteAllowPartial(siteZoneIds);

            return true;
        }

        private List<string> BuildSiteZoneIds(string originZoneId, int layers)
        {
            List<string> siteZoneIds = new List<string>();

            string[] parts = originZoneId.Split('.');

            string baseWorld = parts[0];
            int parasangX = int.Parse(parts[1]);
            int parasangY = int.Parse(parts[2]);
            int zoneX = int.Parse(parts[3]);
            int zoneY = int.Parse(parts[4]);
            int startZ = int.Parse(parts[5]);

            for (int i = 0; i < layers; i++)
            {
                int z = startZ + i;

                string zoneId =
                    baseWorld + "." +
                    parasangX + "." +
                    parasangY + "." +
                    zoneX + "." +
                    zoneY + "." +
                    z;

                siteZoneIds.Add(zoneId);
            }

            return siteZoneIds;
        }

        private void RegisterSiteAllowPartial(List<string> siteZoneIds)
        {
            int GetZFromZoneId(string id)
            {
                string[] parts = id.Split('.');
                return int.Parse(parts[5]);
            }

            for (int i = 0; i < siteZoneIds.Count; i++)
            {
                string zoneId = siteZoneIds[i];

                if (IsClaimedByOtherContent(zoneId))
                {
                    // skip this layer only
                    continue;
                }

                The.ZoneManager.ClearZoneBuilders(zoneId);
                The.ZoneManager.SetZoneProperty(zoneId, OwnerProperty, "Yes");
                if(i != 0)
                {
                    The.ZoneManager.SetZoneProperty(zoneId, "SkipTerrainBuilders", true);
                }

                string stairs = GetStairsForLayer(i, siteZoneIds.Count);

                int z = GetZFromZoneId(zoneId);
                int tier = GetTierFromZ(z);

                The.ZoneManager.SetZoneProperty(zoneId, "ZoneTierOverride", tier.ToString());

                The.ZoneManager.AddZoneBuilder(
                    zoneId,
                    6000,
                    "BasicLair",
                    "Table", "DynamicInheritsTable:Creature:Tier" + tier,
                    //"Table", "",
                    "Adjectives", "",
                    "Stairs", stairs
                );

                /*The.ZoneManager.AddZoneBuilder(
                    zoneId,
                    6000,
                    "Population",
                    "Table", "Lairs_Tier" + tier
                );*/

                /*The.ZoneManager.AddZoneBuilder(
                    zoneId,
                    6000,
                    "Population",
                    "Table", "DynamicInheritsTable:Creature:Tier" + tier,
                    "Density", "medium"
                );*/

                if (i == siteZoneIds.Count - 1)
                {
                    The.ZoneManager.AddZoneBuilder(
                        zoneId,
                        6000,
                        "FactionEncounters",
                        "Chance", "100",
                        "Rolls", "1",
                        "Population", "GenericFactionPopulation"
                    );
                }

                The.ZoneManager.SetZoneName(
                    zoneId,
                    "Stacked Lair Test: Layer " + (i + 1) + " of " + siteZoneIds.Count,
                    Proper: false
                );
            }
        }

        /*private bool RegisterWholeSiteIfSafe(List<string> siteZoneIds)
        {
            foreach (string zoneId in siteZoneIds)
            {
                if (IsClaimedByOtherContent(zoneId))
                {
                    return false;
                }
            }

            for (int i = 0; i < siteZoneIds.Count; i++)
            {
                string zoneId = siteZoneIds[i];

                The.ZoneManager.ClearZoneBuilders(zoneId);
                The.ZoneManager.SetZoneProperty(zoneId, OwnerProperty, "Yes");
                The.ZoneManager.SetZoneProperty(zoneId, "SkipTerrainBuilders", true);

                string stairs = GetStairsForLayer(i, siteZoneIds.Count);

                The.ZoneManager.AddZoneBuilder(
                    zoneId,
                    6000,
                    "BasicLair",
                    //"Table" = "DynamicInheritsTable:Creature:Tier" + Z.NewTier,
                    "Table", "",
                    "Adjectives", "",
                    "Stairs", stairs
                );

                
                if (currentLayer == siteZoneIds.Count - 1)
                {
                    var factionEncounters = new XRL.World.ZoneBuilders.FactionEncounters();
                    factionEncounters.Chance = 100;
                    factionEncounters.Rolls = 1;
                    factionEncounters.Population = "GenericFactionPopulation";
                    factionEncounters.BuildZone(Z);
                }
                

                The.ZoneManager.SetZoneName
                (
                    zoneId,
                    "Stacked Lair Test: Layer " + (i + 1) + " of " + siteZoneIds.Count,
                    Proper: false
                );
            }

            return true;
        }*/

        private string GetStairsForLayer(int layerIndex, int layerCount)
        {
            if (layerCount <= 1)
            {
                return "";
            }

            if (layerIndex == 0)
            {
                return "D";
            }

            if (layerIndex == layerCount - 1)
            {
                return "U";
            }

            return "UD";
        }

        private bool IsClaimedByOtherContent(string zoneId)
        {
            string owner = The.ZoneManager.GetZoneProperty(zoneId, OwnerProperty) as string;

            if (owner == "Yes")
            {
                return false;
            }

            if (The.ZoneManager.ZoneHasBuilder(zoneId, "SultanDungeon"))
            {
                return true;
            }

            if (The.ZoneManager.ZoneHasBuilder(zoneId, "Village"))
            {
                return true;
            }

            if (The.ZoneManager.ZoneHasBuilder(zoneId, "BasicLair"))
            {
                return true;
            }

            return false;
        }
        private int GetTierFromZ(int z)
        {
            int tier = 1;

            if (z > 15)
            {
                tier = Math.Abs(z - 16) / 5 + 2;
            }

            if (tier < 1) tier = 1;
            if (tier > 8) tier = 8;

            return tier;
        }
    }
}