using System;
using System.Collections.Generic;
using HistoryKit;
using XRL;
using XRL.Core;
using XRL.Rules;
using XRL.World;
using XRL.World.WorldBuilders;
using XRL.World.ZoneBuilders;

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
        private const string InitFlag = "SubterraneanSites_TestSultanSiteRegistered";

        /*
        Current test strategy:
        - Register one fixed stacked SultanDungeon site once.
        - Reuse existing generated sultan/history data.
        - Build a SultanDungeonArgs object manually.
        - Store it under sultanDungeonArgs_<regionName>.
        - Register SultanDungeon on each layer using the same regionName.
        - Skip secrets, quests, SultanRegionSurface, and relics for this first test.

        Purpose:
        - Test whether SultanDungeon can be used outside vanilla AddSultanHistoryLocations.
        - Test whether existing sultan/region snapshots produce geometry and cult mobs.
        */

        public override void Register(XRLGame game, IEventRegistrar registrar)
        {
            registrar.Register(BeforeZoneBuiltEvent.ID);
        }

        public override bool HandleEvent(BeforeZoneBuiltEvent zoneBuildEvent)
        {
            if (The.Game.GetStringGameState(InitFlag) == "Yes")
            {
                return true;
            }

            The.Game.SetStringGameState(InitFlag, "Yes");

            int rawSeed = XRLCore.Core.Game.GetWorldSeed();
            int zoneSeed = XRLCore.Core.Game.GetWorldSeed(TargetZoneId + rawSeed);
            System.Random rng = new System.Random(zoneSeed);

            int layers = rng.Next(3, 6);

            List<string> siteZoneIds = BuildSiteZoneIds(TargetZoneId, layers);

            RegisterSultanDungeonSite(siteZoneIds);

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

        private void RegisterSultanDungeonSite(List<string> siteZoneIds)
        {
            if (siteZoneIds == null || siteZoneIds.Count == 0)
            {
                return;
            }

            int originZ = GetZFromZoneId(siteZoneIds[0]);
            int targetTier = GetTierFromZ(originZ);
            int period = GetSultanPeriodForTier(targetTier);

            History sultanHistory = The.Game.sultanHistory;

            if (sultanHistory == null)
            {
                return;
            }

            HistoricEntity region = PickRegionForPeriod(sultanHistory, period);

            if (region == null)
            {
                return;
            }

            HistoricEntitySnapshot regionSnapshot = region.GetCurrentSnapshot();

            if (regionSnapshot == null)
            {
                return;
            }

            string sourceRegionName = regionSnapshot.GetProperty("newName", regionSnapshot.GetProperty("name", "Unknown Region"));

            // Use a modded runtime key so we do not overwrite vanilla's sultanDungeonArgs_<region>.
            // Mechanically, SultanDungeon only needs this to match the regionName builder argument.
            string regionName = "SubterraneanSites_" + sourceRegionName;

            // Use a real historical name where possible. SultanDungeon may use this to pull an
            // additional snapshot during BuildZoneFromArgs.
            string locationName = regionSnapshot.GetProperty("name", sourceRegionName);

            SultanDungeonArgs args = BuildSultanDungeonArgsFromHistory(
                sultanHistory,
                regionSnapshot,
                period
            );

            if (args == null)
            {
                return;
            }

            The.Game.SetObjectGameState("sultanDungeonArgs_" + regionName, args);

            for (int i = 0; i < siteZoneIds.Count; i++)
            {
                string zoneId = siteZoneIds[i];

                if (IsClaimedByOtherContent(zoneId))
                {
                    continue;
                }

                string stairs = GetStairsForLayer(i, siteZoneIds.Count);
                int z = GetZFromZoneId(zoneId);
                int tier = GetTierFromZ(z);

                // Match vanilla SultanDungeon behavior more closely:
                // - Top layer keeps existing terrain/builders so natural connections/entrances can survive.
                // - Lower layers become fully controlled SultanDungeon levels.
                if (i != 0)
                {
                    The.ZoneManager.ClearZoneBuilders(zoneId);
                    The.ZoneManager.SetZoneProperty(zoneId, "SkipTerrainBuilders", true);
                }

                The.ZoneManager.SetZoneProperty(zoneId, OwnerProperty, "Yes");
                The.ZoneManager.SetZoneProperty(zoneId, "ZoneTierOverride", tier.ToString());

                // Optional but useful for seeing that this is our test site.
                The.ZoneManager.SetZoneProperty(zoneId, "HistoricSite", regionName);

                The.ZoneManager.AddZoneBuilder(
                    zoneId,
                    6000,
                    "SultanDungeon",
                    "locationName", locationName,
                    "regionName", regionName,
                    "stairs", stairs
                );

                The.ZoneManager.AddZoneBuilder(
                    zoneId,
                    6000,
                    "Music",
                    "Track", "Music/of Chrome and How"
                );

                The.ZoneManager.SetZoneName(
                    zoneId,
                    "Subterranean Historic Site Test: Layer " + (i + 1) + " of " + siteZoneIds.Count,
                    Proper: false
                );
            }
        }

        private SultanDungeonArgs BuildSultanDungeonArgsFromHistory(
            History sultanHistory,
            HistoricEntitySnapshot regionSnapshot,
            int period
        )
        {
            SultanDungeonArgs args = new SultanDungeonArgs();

            HistoricEntityList sultans = sultanHistory.GetEntitiesWherePropertyEquals("type", "sultan");

            if (sultans != null && sultans.entities != null && sultans.entities.Count > 0)
            {
                HistoricEntity periodSultan = null;

                HistoricEntityList matchingSultans =
                    sultans.GetEntitiesWherePropertyEquals("period", period.ToString());

                if (matchingSultans != null && matchingSultans.entities != null && matchingSultans.entities.Count > 0)
                {
                    periodSultan = matchingSultans.entities[Stat.Random(0, matchingSultans.entities.Count - 1)];
                }
                else
                {
                    periodSultan = sultans.entities[Stat.Random(0, sultans.entities.Count - 1)];
                }

                if (periodSultan != null)
                {
                    args.UpdateFromEntity(periodSultan.GetCurrentSnapshot());
                }
            }

            args.UpdateWalls(period);
            args.UpdateFromEntity(regionSnapshot);

            if (50.in100())
            {
                args.wallTypes.Add("*SultanWall*");
            }

            return args;
        }

        private HistoricEntity PickRegionForPeriod(History sultanHistory, int period)
        {
            HistoricEntityList regions = sultanHistory.GetEntitiesWherePropertyEquals("type", "region");

            if (regions == null || regions.entities == null || regions.entities.Count == 0)
            {
                return null;
            }

            List<HistoricEntity> matchingPeriodRegions = new List<HistoricEntity>();

            foreach (HistoricEntity region in regions.entities)
            {
                HistoricEntitySnapshot snap = region.GetCurrentSnapshot();

                if (snap == null)
                {
                    continue;
                }

                string periodString = snap.GetProperty("period", "-1");

                int regionPeriod;

                if (int.TryParse(periodString, out regionPeriod) && regionPeriod == period)
                {
                    matchingPeriodRegions.Add(region);
                }
            }

            if (matchingPeriodRegions.Count > 0)
            {
                return matchingPeriodRegions[Stat.Random(0, matchingPeriodRegions.Count - 1)];
            }

            return regions.entities[Stat.Random(0, regions.entities.Count - 1)];
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

        private int GetZFromZoneId(string id)
        {
            string[] parts = id.Split('.');
            return int.Parse(parts[5]);
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

        private int GetSultanPeriodForTier(int tier)
        {
            if (tier <= 2)
            {
                return 5;
            }

            if (tier <= 4)
            {
                return 4;
            }

            if (tier <= 6)
            {
                return 3;
            }

            if (tier == 7)
            {
                return 2;
            }

            return 1;
        }
    }
}