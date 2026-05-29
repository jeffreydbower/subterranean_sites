using System;
using System.Collections.Generic;
using HistoryKit;
using XRL;
using XRL.Rules;
using XRL.World;
using XRL.World.WorldBuilders;
using XRL.World.ZoneBuilders;
using Qud.API;
using XRL.Language;

namespace SubterraneanSites
{
    internal class SiteLayerContext
    {
        public string ZoneId;
        public int LayerIndex;
        public int LayerCount;
        public int Z;
        public int Tier;
        public string Stairs;
        public bool IsOrigin;
        public bool IsBottom;
    }

    internal static class SiteContentHelpers
    {
        public static int GetRewardChestTier(int baseTier)
        {
            int rewardTier = baseTier + 1;

            if (rewardTier < 2)
            {
                rewardTier = 2;
            }

            if (rewardTier > 8)
            {
                rewardTier = 8;
            }

            return rewardTier;
        }

        public static string GetRewardChestBlueprint(int tier)
        {
            if (tier < 1)
            {
                tier = 1;
            }

            if (tier > 8)
            {
                tier = 8;
            }

            return "Rare Chest" + tier.ToString();
        }

        public static void AddMusic(string zoneId, string track)
        {
            if (zoneId == null || zoneId == "" || track == null || track == "")
            {
                return;
            }

            The.ZoneManager.AddZoneBuilder(
                zoneId,
                6000,
                "Music",
                "Track", track
            );
        }
    }


    internal class MerchantHiveSiteRegistrar
    {
        private readonly RuntimeZoneBuilderInjectionSystem parent;

        private string siteDisplayName;
        private string discoveryKey;

        private Dictionary<int, string> cachedMerchantByLayer =
            new Dictionary<int, string>();

        private Dictionary<int, string> adjectivesByLayer =
            new Dictionary<int, string>();

        public MerchantHiveSiteRegistrar(RuntimeZoneBuilderInjectionSystem parent)
        {
            this.parent = parent;
        }

        public bool Register(List<string> siteZoneIds)
        {
            if (siteZoneIds == null || siteZoneIds.Count == 0)
            {
                return false;
            }

            if (!PrepareMerchantHive(siteZoneIds))
            {
                return false;
            }

            return parent.RegisterLayeredSite(
                siteZoneIds,
                siteDisplayName,
                discoveryKey,
                RegisterLayer
            );
        }

        private bool PrepareMerchantHive(List<string> siteZoneIds)
        {
            cachedMerchantByLayer.Clear();
            adjectivesByLayer.Clear();

            XRL.World.GameObject bottomMerchant = null;

            for (int i = 0; i < siteZoneIds.Count; i++)
            {
                string zoneId = siteZoneIds[i];

                int z = parent.GetZFromZoneId(zoneId);
                //int tier = parent.GetTierFromZ(z);
                int tier = parent.GetTierForZoneId(zoneId);

                bool isBottom = i == siteZoneIds.Count - 1;

                int merchantTier = tier;

                if (isBottom)
                {
                    merchantTier = ClampTier(tier + 1);
                }

                string terrainBlueprint = PickMerchantTerrainForTier(merchantTier);

                XRL.World.GameObject merchant =
                    GenerateMerchantForTier(terrainBlueprint, merchantTier);

                if (merchant == null)
                {
                    return false;
                }

                cachedMerchantByLayer[i] = The.ZoneManager.CacheObject(merchant);
                adjectivesByLayer[i] = BuildAdjectives(merchant, terrainBlueprint);

                if (isBottom)
                {
                    bottomMerchant = merchant;
                }
            }

            if (bottomMerchant == null)
            {
                return false;
            }

            siteDisplayName = BuildBazaarName(bottomMerchant);

            discoveryKey =
                "SubterraneanSites_Discovered_MerchantHive_" +
                siteZoneIds[0] +
                "_" +
                bottomMerchant.ID;

            return true;
        }

        private void RegisterLayer(SiteLayerContext context)
        {
            string adjectives = "workshop";

            if (adjectivesByLayer.ContainsKey(context.LayerIndex))
            {
                adjectives = adjectivesByLayer[context.LayerIndex];
            }

            // Use BasicLair primarily as a native lair/workshop layout generator.
            // MerchantHive intentionally does not add hostile population or reward chests.
            The.ZoneManager.AddZonePostBuilder(
                context.ZoneId,
                "BasicLair",
                "Table", "",
                "Adjectives", adjectives,
                "Stairs", context.Stairs
            );

            SiteContentHelpers.AddMusic(
                context.ZoneId,
                "Music/Moghrayi Remembrance Circle"
            );

            if (cachedMerchantByLayer.ContainsKey(context.LayerIndex))
            {
                The.ZoneManager.AddZonePostBuilder(
                    context.ZoneId,
                    "AddObjectBuilder",
                    "Object", cachedMerchantByLayer[context.LayerIndex]
                );
            }
        }

        private XRL.World.GameObject GenerateMerchantForTier(
            string terrainBlueprint,
            int tier
        )
        {
            // GenericChance 100 forces the GenericLairOwner path.
            // That path is the merchant/workshop-heavy vanilla lair-owner pool.
            XRL.World.GameObject merchant =
                JoppaWorldBuilder.GenerateLairOwner(
                    terrainBlueprint,
                    tier,
                    100
                );

            if (merchant == null)
            {
                return null;
            }

            if (!IsMerchantLike(merchant))
            {
                return null;
            }

            return merchant;
        }

        private bool IsMerchantLike(XRL.World.GameObject obj)
        {
            if (obj == null)
            {
                return false;
            }

            try
            {
                if (obj.IsMerchant())
                {
                    return true;
                }
            }
            catch
            {
            }

            if (obj.HasTag("Merchant"))
            {
                return true;
            }

            if (obj.HasPart<XRL.World.Parts.GenericInventoryRestocker>())
            {
                return true;
            }

            return false;
        }

        private string BuildAdjectives(
            XRL.World.GameObject merchant,
            string terrainBlueprint
        )
        {
            string ownerAdjectives = "";

            if (merchant != null)
            {
                ownerAdjectives = merchant.GetPropertyOrTag("LairAdjectives", "");
            }

            if (ownerAdjectives == null)
            {
                ownerAdjectives = "";
            }

            if (ownerAdjectives.Length > 0)
            {
                ownerAdjectives += ",";
            }

            string terrainAdjectives = "workshop";

            try
            {
                GameObjectBlueprint terrain =
                    GameObjectFactory.Factory.Blueprints[terrainBlueprint];

                if (terrain != null)
                {
                    terrainAdjectives = terrain.GetTag("LairAdjectives", "workshop");
                }
            }
            catch
            {
                terrainAdjectives = "workshop";
            }

            return ownerAdjectives + terrainAdjectives;
        }

        private string BuildBazaarName(XRL.World.GameObject bottomMerchant)
        {
            if (bottomMerchant == null)
            {
                return "Subterranean Bazaar";
            }

            string merchantName =
                bottomMerchant.GetReferenceDisplayName(
                    Context: "LairName"
                );

            if (merchantName == null || merchantName == "")
            {
                return "Subterranean Bazaar";
            }

            return "The Underworld Bazaar of " + merchantName;
        }

        private string PickMerchantTerrainForTier(int tier)
        {
            // GenericChance=100 means the terrain mostly exists to satisfy
            // GenerateLairOwner's expected terrain-blueprint argument and provide
            // reasonable adjective fallback. Use broad, valid terrain blueprints.
            if (tier <= 1)
            {
                return "TerrainDesertCanyon";
            }

            if (tier == 2)
            {
                return "TerrainSaltdunes";
            }

            if (tier == 3)
            {
                return "TerrainJungle";
            }

            if (tier == 4)
            {
                return "TerrainRuins";
            }

            if (tier == 5)
            {
                return "TerrainFungalBase";
            }

            if (tier == 6)
            {
                return "TerrainDeepJungle";
            }

            if (tier == 7)
            {
                return "TerrainPalladiumReef";
            }

            return "TerrainMoonStair";
        }

        private int ClampTier(int tier)
        {
            if (tier < 1)
            {
                return 1;
            }

            if (tier > 8)
            {
                return 8;
            }

            return tier;
        }

        private void AddBazaarMusic(string zoneId)
        {
            The.ZoneManager.AddZoneBuilder(
                zoneId,
                6000,
                "Music",
                "Track", "Music/Moghrayi Remembrance Circle"
            );
        }
    }

    internal class ProperLairSiteRegistrar
    {
        private readonly RuntimeZoneBuilderInjectionSystem parent;

        private const int ExtraHeroChance = 15;
        private const int MaxInheritanceWalk = 32;

        private string siteDisplayName;
        private string discoveryKey;
        private string minionTable;
        private string adjectives;
        private string cachedOwnerId;
        
        private int chestTier;

        public ProperLairSiteRegistrar(RuntimeZoneBuilderInjectionSystem parent)
        {
            this.parent = parent;
        }

        public bool Register(List<string> siteZoneIds)
        {
            if (siteZoneIds == null || siteZoneIds.Count == 0)
            {
                return false;
            }

            if (!PrepareProperLair(siteZoneIds))
            {
                return false;
            }

            return parent.RegisterLayeredSite(
                siteZoneIds,
                siteDisplayName,
                discoveryKey,
                RegisterLayer
            );
        }

        private bool PrepareProperLair(List<string> siteZoneIds)
        {
            int tier = parent.GetTierForZoneId(siteZoneIds[0]);

            string terrainBlueprint = PickProperLairTerrainForTier(tier);

            XRL.World.GameObject lairOwner =
                JoppaWorldBuilder.GenerateLairOwner(
                    terrainBlueprint,
                    tier,
                    0
                );
                

            if (lairOwner == null)
            {
                return false;
            }

            minionTable = BuildMinionTableFromLairOwner(lairOwner, tier);

            if (minionTable == null || minionTable == "")
            {
                return false;
            }

            adjectives = BuildAdjectives(lairOwner, terrainBlueprint);

            siteDisplayName = BuildVanillaLairName(lairOwner);

            discoveryKey =
                "SubterraneanSites_Discovered_ProperLair_" +
                siteZoneIds[0] +
                "_" +
                lairOwner.ID;

            cachedOwnerId = The.ZoneManager.CacheObject(lairOwner);

            chestTier = BuildRewardChestTierFromLairOwner(lairOwner);

            return true;
        }

        private void RegisterLayer(SiteLayerContext context)
        {
            The.ZoneManager.AddZonePostBuilder(
                context.ZoneId,
                "BasicLair",
                "Table", minionTable,
                "Adjectives", adjectives,
                "Stairs", context.Stairs
            );

            MaybeAddExtraHero(context);

            if (context.IsBottom)
            {
                The.ZoneManager.AddZonePostBuilder(
                    context.ZoneId,
                    "AddObjectBuilder",
                    "Object", cachedOwnerId
                );

                The.ZoneManager.AddZonePostBuilder(
                    context.ZoneId,
                    "AddBlueprintBuilder",
                    "Object", SiteContentHelpers.GetRewardChestBlueprint(chestTier)
                );
            }
        }

        private string BuildMinionTableFromLairOwner(
            XRL.World.GameObject lairOwner,
            int tier
        )
        {
            if (lairOwner == null)
            {
                return "";
            }

            if (lairOwner.HasTag("LairMinionsInherit"))
            {
                string inheritSource = lairOwner.GetTag("LairMinionsInherit");

                if (inheritSource == null || inheritSource == "")
                {
                    return "";
                }

                return
                    "DynamicInheritsTable:" +
                    inheritSource +
                    ":Tier" +
                    tier.ToString();
            }

            if (lairOwner.HasTag("LairMinions"))
            {
                string explicitMinions = lairOwner.GetTag("LairMinions");

                if (explicitMinions == null)
                {
                    return "";
                }

                return explicitMinions;
            }

            GameObjectBlueprint blueprint = lairOwner.GetBlueprint();

            if (blueprint == null)
            {
                return "";
            }

            string baseCreatureType = blueprint.Inherits;
            string key = baseCreatureType;

            int steps = 0;

            while (key != null && key != "" && steps < MaxInheritanceWalk)
            {
                steps++;

                GameObjectBlueprint currentBlueprint;

                if (!GameObjectFactory.Factory.Blueprints.TryGetValue(key, out currentBlueprint))
                {
                    break;
                }

                bool isBaseCandidate =
                    key.StartsWith("Base") ||
                    currentBlueprint.Tags.ContainsKey("BaseObject");

                bool skipAsBase =
                    currentBlueprint.Tags.ContainsKey("SkipAsLairBaseCreatureType");

                if (isBaseCandidate && !skipAsBase)
                {
                    baseCreatureType = key;
                    break;
                }

                key = currentBlueprint.Inherits;
            }

            if (baseCreatureType == null || baseCreatureType == "")
            {
                return "";
            }

            return
                "DynamicInheritsTable:" +
                baseCreatureType +
                ":Tier" +
                tier.ToString();
        }
        // Extra heroes are generated from the same minion table used by BasicLair,
        // so ProperLair stays a single-ecology lair rather than a mixed-owner site.
        private void MaybeAddExtraHero(SiteLayerContext context)
        {
            if (context == null)
            {
                return;
            }

            if (context.IsBottom)
            {
                return;
            }

            if (!ExtraHeroChance.in100())
            {
                return;
            }

            XRL.World.GameObject hero = GenerateExtraHeroFromMinionTable(context.Tier);

            if (hero == null)
            {
                return;
            }

            The.ZoneManager.AddZonePostBuilder(
                context.ZoneId,
                "AddBlueprintBuilder",
                "Object", SiteContentHelpers.GetRewardChestBlueprint(SiteContentHelpers.GetRewardChestTier(context.Tier))
            );

            The.ZoneManager.AddZonePostBuilder(
                context.ZoneId,
                "AddObjectBuilder",
                "Object", The.ZoneManager.CacheObject(hero)
            );
        }

        private XRL.World.GameObject GenerateExtraHeroFromMinionTable(int tier)
        {
            if (minionTable == null || minionTable == "")
            {
                return null;
            }

            List<XRL.World.GameObject> candidates = null;

            try
            {
                candidates = PopulationManager.Expand(
                    PopulationManager.Generate(
                        minionTable,
                        "zonetier",
                        tier.ToString()
                    )
                );
            }
            catch
            {
                return null;
            }

            if (candidates == null || candidates.Count == 0)
            {
                return null;
            }

            XRL.World.GameObject baseCreature = PickHeroCandidate(candidates);

            if (baseCreature == null)
            {
                return null;
            }

            baseCreature.SetStringProperty("Role", "Hero");

            XRL.World.GameObject hero = HeroMaker.MakeHero(
                baseCreature,
                Array.Empty<string>(),
                Array.Empty<string>(),
                tier,
                "Lair"
            );

            if (hero == null)
            {
                return null;
            }

            if (hero.HasStat("Hitpoints"))
            {
                hero.GetStat("Hitpoints").BaseValue *= 2;
            }

            return hero;
        }

        private XRL.World.GameObject PickHeroCandidate(
            List<XRL.World.GameObject> candidates
        )
        {
            if (candidates == null || candidates.Count == 0)
            {
                return null;
            }

            List<XRL.World.GameObject> validCandidates =
                new List<XRL.World.GameObject>();

            foreach (XRL.World.GameObject candidate in candidates)
            {
                if (candidate == null)
                {
                    continue;
                }

                if (candidate.IsPlayer())
                {
                    continue;
                }

                if (candidate.HasTag("Merchant"))
                {
                    continue;
                }

                if (candidate.HasPart<XRL.World.Parts.GivesRep>())
                {
                    continue;
                }

                if (!candidate.HasStat("Hitpoints"))
                {
                    continue;
                }

                validCandidates.Add(candidate);
            }

            if (validCandidates.Count == 0)
            {
                return null;
            }

            return validCandidates[Stat.Random(0, validCandidates.Count - 1)];
        }

        private int BuildRewardChestTierFromLairOwner(XRL.World.GameObject lairOwner)
        {
            return SiteContentHelpers.GetRewardChestTier(BuildChestTierFromLairOwner(lairOwner));
        }

        private string BuildAdjectives(
            XRL.World.GameObject lairOwner,
            string terrainBlueprint
        )
        {
            string ownerAdjectives = "";

            if (lairOwner != null)
            {
                ownerAdjectives = lairOwner.GetPropertyOrTag("LairAdjectives", "");
            }

            if (ownerAdjectives == null)
            {
                ownerAdjectives = "";
            }

            if (ownerAdjectives.Length > 0)
            {
                ownerAdjectives += ",";
            }

            string terrainAdjectives = "lair";

            try
            {
                GameObjectBlueprint terrain =
                    GameObjectFactory.Factory.Blueprints[terrainBlueprint];

                if (terrain != null)
                {
                    terrainAdjectives = terrain.GetTag("LairAdjectives", "lair");
                }
            }
            catch
            {
                terrainAdjectives = "lair";
            }

            return ownerAdjectives + terrainAdjectives;
        }

        private string BuildVanillaLairName(XRL.World.GameObject lairOwner)
        {
            if (lairOwner == null)
            {
                return "lair";
            }

            GameObjectBlueprint blueprint = lairOwner.GetBlueprint();

            string lairType = "lair";

            if (blueprint != null)
            {
                lairType = blueprint.GetTag("LairName", "lair");
            }

            string ownerName =
                lairOwner.GetReferenceDisplayName(
                    Context: "LairName"
                );

            //string possessive = Grammar.MakePossessive(ownerName);

            return
                "The " +
                lairType +
                " of " +
                ownerName;
        }

        private int BuildChestTierFromLairOwner(XRL.World.GameObject lairOwner)
        {
            if (lairOwner == null)
            {
                return 2;
            }

            int tier = lairOwner.Stat("Level") / 5 + 1;

            if (tier < 2)
            {
                tier = 2;
            }

            if (tier > 8)
            {
                tier = 8;
            }

            return tier;
        }
        // ProperLair intentionally selects from curated terrain blueprints by depth tier
        // instead of using the literal surface terrain. Special surface locations like
        // TerrainJoppa can lack LairOwnerTable and fall back to GenericLairOwner,
        // producing merchant/workshop lairs instead of creature lairs.
        private string PickProperLairTerrainForTier(int tier)
        {
            string[] terrains = GetProperLairTerrainsForTier(tier);

            if (terrains == null || terrains.Length == 0)
            {
                return "TerrainRuins";
            }

            return terrains[Stat.Random(0, terrains.Length - 1)];
        }

        private string[] GetProperLairTerrainsForTier(int tier)
        {
            if (tier <= 0)
            {
                return new string[]
                {
                    "TerrainSaltmarsh",
                    "TerrainWatervine"
                };
            }

            if (tier == 1)
            {
                return new string[]
                {
                    "TerrainDesertCanyon",
                    "TerrainSaltmarsh",
                    "TerrainWatervine"
                };
            }

            if (tier == 2)
            {
                return new string[]
                {
                    "TerrainSaltdunes",
                    "TerrainFlowerfields",
                    "TerrainHills"//,
                    //"TerrainCraters"//,
                    //"RandomCrater"
                };
            }

            if (tier == 3)
            {
                return new string[]
                {
                    "TerrainFlowerfields",
                    "TerrainJungle"
                };
            }

            if (tier == 4)
            {
                return new string[]
                {
                    "TerrainMountains",
                    "TerrainWater",
                    "TerrainRuins"
                };
            }

            if (tier == 5)
            {
                return new string[]
                {
                    "TerrainFungalBase",
                    "TerrainBananaGrove",
                    "TerrainTheSpindle"//,
                    //"TerrainOmonporch"
                };
            }

            if (tier == 6)
            {
                return new string[]
                {
                    "TerrainDeepJungle",
                    "TerrainLakeHinnom"
                };
            }

            if (tier == 7)
            {
                return new string[]
                {
                    "TerrainPalladiumReef",
                    "TerrainBaroqueRuins"
                };
            }

            return new string[]
            {
                "TerrainMoonStair"
            };
        }
    }

    internal class BasicLairChaosSiteRegistrar
    {
        private const int ExtraFactionEncounterChance = 25;

        private readonly RuntimeZoneBuilderInjectionSystem parent;

        public BasicLairChaosSiteRegistrar(RuntimeZoneBuilderInjectionSystem parent)
        {
            this.parent = parent;
        }

        public bool Register(List<string> siteZoneIds)
        {
            if (siteZoneIds == null || siteZoneIds.Count == 0)
            {
                return false;
            }

            string siteDisplayName = "A Contested Subterranean Lair";
            string discoveryKey =
                "SubterraneanSites_Discovered_BasicLairChaos_" +
                siteZoneIds[0];

            return parent.RegisterLayeredSite(
                siteZoneIds,
                siteDisplayName,
                discoveryKey,
                RegisterLayer
            );
        }

        private void RegisterLayer(SiteLayerContext context)
        {
            The.ZoneManager.AddZonePostBuilder(
                context.ZoneId,
                "BasicLair",
                "Table", "",
                "Adjectives", "",
                "Stairs", context.Stairs
            );

            string singlesTable =
                "SubterraneanSites_Tier" +
                context.Tier.ToString() +
                "_Mobs";

            string teamsTable =
                "SubterraneanSites_Tier" +
                context.Tier.ToString() +
                "_FightableTeams";

            The.ZoneManager.AddZonePostBuilder(
                context.ZoneId,
                "SubterraneanSiteMobs",
                "Rolls", "2",
                "Tier", context.Tier.ToString(),
                "Table", teamsTable
            );

            The.ZoneManager.AddZonePostBuilder(
                context.ZoneId,
                "SubterraneanSiteMobs",
                "Rolls", "4",
                "Tier", context.Tier.ToString(),
                "Table", singlesTable
            );

            MaybeAddFactionEncounterWithChest(context);

            if (context.IsBottom)
            {
                AddFactionEncounter(context.ZoneId);

                The.ZoneManager.AddZonePostBuilder(
                    context.ZoneId,
                    "AddBlueprintBuilder",
                    "Object", SiteContentHelpers.GetRewardChestBlueprint(SiteContentHelpers.GetRewardChestTier(context.Tier))
                );
            }
        }

        private void MaybeAddFactionEncounterWithChest(SiteLayerContext context)
        {
            if (context == null)
            {
                return;
            }

            if (context.IsBottom)
            {
                return;
            }

            if (!ExtraFactionEncounterChance.in100())
            {
                return;
            }

            AddFactionEncounter(context.ZoneId);

            The.ZoneManager.AddZonePostBuilder(
                context.ZoneId,
                "AddBlueprintBuilder",
                "Object", SiteContentHelpers.GetRewardChestBlueprint(SiteContentHelpers.GetRewardChestTier(context.Tier))
            );
        }

        private void AddFactionEncounter(string zoneId)
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

    }

    internal class SultanHistoricSiteRegistrar
    {
        private readonly RuntimeZoneBuilderInjectionSystem parent;

        private string regionName;
        private string siteDisplayName;
        private HistoricEntitySnapshot regionSnapshot;

        public SultanHistoricSiteRegistrar(RuntimeZoneBuilderInjectionSystem parent)
        {
            this.parent = parent;
        }

        public bool Register(List<string> siteZoneIds)
        {
            if (siteZoneIds == null || siteZoneIds.Count == 0)
            {
                return false;
            }

            if (!PrepareSultanSite(siteZoneIds))
            {
                return false;
            }

            string discoveryKey = "SubterraneanSites_Discovered_" + regionName;

            return parent.RegisterLayeredSite(
                siteZoneIds,
                siteDisplayName,
                discoveryKey,
                RegisterLayer
            );
        }

        private bool PrepareSultanSite(List<string> siteZoneIds)
        {
            //int originZ = parent.GetZFromZoneId(siteZoneIds[0]);
            //int targetTier = parent.GetTierFromZ(originZ);
            int targetTier = parent.GetTierForZoneId(siteZoneIds[0]);

            int period = SultanDungeon.GetSultanPeriodFromTier(targetTier);

            History sultanHistory = The.Game.sultanHistory;

            if (sultanHistory == null)
            {
                return false;
            }

            HistoricEntity region = PickRegionForPeriod(sultanHistory, period);

            if (region == null)
            {
                return false;
            }

            regionSnapshot = region.GetCurrentSnapshot();

            if (regionSnapshot == null)
            {
                return false;
            }

            string sourceRegionName =
                regionSnapshot.GetProperty("newName", regionSnapshot.GetProperty("name", "Unknown Region"));

            regionName = "SubterraneanSites_" + sourceRegionName;
            siteDisplayName = "Forgotten Site of " + sourceRegionName;

            SultanDungeonArgs args = BuildSultanDungeonArgsFromHistory(
                sultanHistory,
                regionSnapshot,
                period
            );

            if (args == null)
            {
                return false;
            }

            The.Game.SetObjectGameState("sultanDungeonArgs_" + regionName, args);

            return true;
        }

        private void RegisterLayer(SiteLayerContext context)
        {
            The.ZoneManager.SetZoneProperty(context.ZoneId, "HistoricSite", regionName);

            The.ZoneManager.AddZoneBuilder(
                context.ZoneId,
                6000,
                "SultanDungeon",
                "locationName", siteDisplayName,
                "regionName", regionName,
                "stairs", context.Stairs
            );

            SiteContentHelpers.AddMusic(
                context.ZoneId,
                "Music/of Chrome and How"
            );

            if (context.IsBottom)
            {
                AddBottomLayerVaultWithRelicAndHero(
                    context.ZoneId,
                    regionSnapshot,
                    context.Tier
                );
            }
        }

        private void AddBottomLayerVaultWithRelicAndHero(
            string zoneId,
            HistoricEntitySnapshot regionSnapshot,
            int tier
        )
        {
            XRL.World.GameObject relic =
                RelicGenerator.GenerateRelic(regionSnapshot, tier, RandomName: true);

            if (relic == null)
            {
                return;
            }

            The.ZoneManager.SetZoneProperty(zoneId, "Relicstyle", "Vault");

            The.ZoneManager.AddZoneBuilder(
                zoneId,
                6000,
                "PlaceRelicBuilder",
                "Relic", The.ZoneManager.CacheObject(relic)
            );
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
    }
}