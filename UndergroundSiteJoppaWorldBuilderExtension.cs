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
                The.ZoneManager.AddZonePostBuilder(
                    zoneId,
                    "BasicLair",
                    "Table", "",
                    "Adjectives", "",
                    "Stairs", stairs
                );

                // Ordinary mobs.
                // This custom builder chooses creature blueprints by Level.
                // Tier is passed in; the builder rolls a random creature level
                // inside that tier's level band for each mob.
                The.ZoneManager.AddZonePostBuilder(
                    zoneId,
                    "SubterraneanSiteMobs",
                    "Count", "12",
                    "Tier", tier.ToString()
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
    using Qud.API;
    using XRL.Rules;
    using XRL.World;

    public class SubterraneanSiteMobs : ZoneBuilderSandbox
    {
        public int Count = 6;
        public int Tier = 1;

        public bool BuildZone(Zone Z)
        {
            for (int i = 0; i < Count; i++)
            {
                Cell cell = GetRandomEmptyReachableCell(Z);

                if (cell == null)
                {
                    continue;
                }

                int targetLevel = GetRandomCreatureLevelFromTier(Tier);

                GameObject mob = EncountersAPI.GetNonLegendaryCreatureAroundLevel(targetLevel);

                if (mob != null)
                {
                    cell.AddObject(mob);
                }
            }

            return true;
        }

        private int GetRandomCreatureLevelFromTier(int tier)
        {
            if (tier < 1) tier = 1;
            if (tier > 8) tier = 8;

            int minLevel;
            int maxLevel;

            if (tier == 1)
            {
                minLevel = 1;
                maxLevel = 4;
            }
            else
            {
                minLevel = ((tier - 1) * 5);
                maxLevel = minLevel + 4;
            }

            return Stat.Random(minLevel, maxLevel);
        }

        private Cell GetRandomEmptyReachableCell(Zone Z)
        {
            List<Cell> candidates = new List<Cell>();

            foreach (Cell cell in Z.GetCells())
            {
                if (cell.IsReachable() && cell.IsEmptyOfSolid() && !cell.HasSpawnBlocker())
                {
                    candidates.Add(cell);
                }
            }

            if (candidates.Count == 0)
            {
                return null;
            }

            return candidates[Stat.Random(0, candidates.Count - 1)];
        }
    }
}