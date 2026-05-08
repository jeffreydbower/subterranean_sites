using System;
using System.Collections.Generic;
using XRL;
using XRL.Core;
using XRL.World;
using XRL.World.WorldBuilders;
using XRL.Rules;

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
        Current test strategy:
        - Register a fixed stacked BasicLair site once.
        - Let ZoneManager build the site naturally when entered.
        - Add ordinary mobs with a custom builder using creature blueprint Level.
        - Keep FactionEncounters on the bottom layer as the special encounter.

        Future strategy:
        - Replace one-time registration with matrix-adjacent registration.
        - Use tier/depth to scale ordinary mobs and special encounters.
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

            int layers = rng.Next(3, 7);

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
                    // Current temporary behavior:
                    // skip only this layer.
                    // Later we may reject the whole site if any critical collision exists.
                    continue;
                }

                The.ZoneManager.ClearZoneBuilders(zoneId);
                The.ZoneManager.SetZoneProperty(zoneId, OwnerProperty, "Yes");

                // Let top layer keep normal terrain builders for now.
                // Lower levels suppress ordinary terrain generation.
                if (i != 0)
                {
                    The.ZoneManager.SetZoneProperty(zoneId, "SkipTerrainBuilders", true);
                }

                string stairs = GetStairsForLayer(i, siteZoneIds.Count);

                int z = GetZFromZoneId(zoneId);
                int tier = GetTierFromZ(z);

                The.ZoneManager.SetZoneProperty(zoneId, "ZoneTierOverride", tier.ToString());

                // Layout / chests / stairs.
                // This is required. Without BasicLair, lower levels with SkipTerrainBuilders
                // can become empty/void-like because no terrain/layout builder is creating the lair.
                The.ZoneManager.AddZonePostBuilder(
                    zoneId,
                    "BasicLair",
                    "Table", "",
                    "Adjectives", "",
                    "Stairs", stairs
                );

                string singlesTable = "SubterraneanSites_Tier" + tier + "_Mobs";
                string teamsTable = "SubterraneanSites_Tier" + tier + "_FightableTeams";

                // One vanilla-style encounter/team packet.
                // Rolls = how many times to roll this XML population table.
                The.ZoneManager.AddZonePostBuilder(
                    zoneId,
                    "SubterraneanSiteMobs",
                    "Rolls", "2",
                    "Tier", tier.ToString(),
                    "Table", teamsTable
                );

                // A few single/filler rolls from our curated tier table.
                // Each roll may still produce multiple creatures if the XML entry has Number="2-4", etc.
                The.ZoneManager.AddZonePostBuilder(
                    zoneId,
                    "SubterraneanSiteMobs",
                    "Rolls", "4",
                    "Tier", tier.ToString(),
                    "Table", singlesTable
                );

                // Bottom-layer special encounter.
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

namespace XRL.World.ZoneBuilders
{
    using Genkit;
    using System.Collections.Generic;
    using XRL;
    using XRL.Rules;
    using XRL.World;

    public class SubterraneanSiteMobs : ZoneBuilderSandbox
    {
        public int Rolls = 1;
        public int Tier = 1;
        public string Table = "";

        public bool BuildZone(Zone Z)
        {
            if (Tier < 1) Tier = 1;
            if (Tier > 8) Tier = 8;

            string table = Table;

            if (table.IsNullOrEmpty())
            {
                table = "SubterraneanSites_Tier" + Tier + "_Mobs";
            }

            List<Location2D> locations = new List<Location2D>();

            foreach (Cell cell in Z.GetCells())
            {
                if (cell.IsReachable() && cell.IsEmptyOfSolid() && !cell.HasSpawnBlocker())
                {
                    locations.Add(cell.Location);
                }
            }

            if (locations.Count == 0)
            {
                return true;
            }

            LocationList area = new LocationList(locations);

            for (int roll = 0; roll < Rolls; roll++)
            {
                List<GameObject> objects = PopulationManager.Expand(
                    PopulationManager.Generate(
                        table,
                        "zonetier",
                        Tier.ToString()
                    )
                );

                if (objects == null)
                {
                    continue;
                }

                int placementIndex = 0;

                foreach (GameObject obj in objects)
                {
                    if (obj == null)
                    {
                        continue;
                    }

                    ZoneBuilderSandbox.PlaceObjectInArea(
                        Z,
                        area,
                        obj,
                        placementIndex,
                        0,
                        null,
                        null,
                        true
                    );

                    placementIndex++;
                }
            }

            return true;
        }
    }
}