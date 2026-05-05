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

        Current build strategy:
        Option A / Hybrid build:
        - Register the whole site with ZoneManager.
        - Future zones use the registered ZoneManager builders.
        - The current zone is direct-built because AddZoneBuilder during
          BeforeZoneBuiltEvent is too late to affect the current zone's build.

        Future possible build strategy:
        Option D / Ahead-of-player registration:
        - Register builders for current and neighboring matrices before the player enters them.
        - This would make site builders pre-existing when zones build.
        - It may reduce or eliminate the need for direct-building the current zone.
        */

        public override void Register(XRLGame game, IEventRegistrar registrar)
        {
            registrar.Register(BeforeZoneBuiltEvent.ID);
        }

        public override bool HandleEvent(BeforeZoneBuiltEvent zoneBuildEvent)
        {
            var Z = zoneBuildEvent.Zone;
            var zId = Z.ZoneID;

            if (Z.Z <= 10)
            {
                return true;
            }

            int rawSeed = XRLCore.Core.Game.GetWorldSeed();
            int zoneSeed = XRLCore.Core.Game.GetWorldSeed(TargetZoneId + rawSeed);
            System.Random rng = new System.Random(zoneSeed);
            int layers = rng.Next(2, 7);

            List<string> siteZoneIds = BuildSiteZoneIds(layers);

            if (!siteZoneIds.Contains(zId))
            {
                return true;
            }

            if (!RegisterWholeSiteIfSafe(siteZoneIds))
            {
                The.ZoneManager.SetZoneName
                (
                    Z.ZoneID,
                    "Subterranean Sites safety rejection test",
                    Proper: false
                );

                return true;
            }

            // Hybrid build rule:
            // Registered builders work for future zones, but not for the current zone.
            // Therefore, if the current zone is part of this site, direct-build it now.
            int currentLayer = siteZoneIds.IndexOf(zId);
            DirectBuildCurrentLayer(Z, siteZoneIds, currentLayer);

            return true;
        }

        private List<string> BuildSiteZoneIds(int layers)
        {
            List<string> siteZoneIds = new List<string>();

            string baseWorld = "JoppaWorld";
            int parasangX = 11;
            int parasangY = 22;
            int zoneX = 0;
            int zoneY = 1;
            int startZ = 11;

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

        private bool RegisterWholeSiteIfSafe(List<string> siteZoneIds)
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
                    "Table", "",
                    "Adjectives", "",
                    "Stairs", stairs
                );

                The.ZoneManager.SetZoneName
                (
                    zoneId,
                    "Stacked Lair Test: Layer " + (i + 1) + " of " + siteZoneIds.Count,
                    Proper: false
                );
            }

            return true;
        }

        private void DirectBuildCurrentLayer(Zone Z, List<string> siteZoneIds, int currentLayer)
        {
            foreach (var cell in Z.GetCells())
            {
                cell.Clear();
            }

            var lair = new XRL.World.ZoneBuilders.BasicLair();
            lair.Table = "";
            lair.Adjectives = "";
            lair.Stairs = GetStairsForLayer(currentLayer, siteZoneIds.Count);
            lair.BuildZone(Z);
            /*
            if (currentLayer == siteZoneIds.Count - 1)
            {
                var factionEncounters = new XRL.World.ZoneBuilders.FactionEncounters();
                factionEncounters.Chance = 100;
                factionEncounters.Rolls = 1;
                factionEncounters.Population = "GenericFactionPopulation";
                factionEncounters.BuildZone(Z);
            }
            */
            The.ZoneManager.SetZoneName
            (
                Z.ZoneID,
                "Stacked Lair Test: Layer " + (currentLayer + 1) + " of " + siteZoneIds.Count,
                Proper: false
            );
        }

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
    }
}