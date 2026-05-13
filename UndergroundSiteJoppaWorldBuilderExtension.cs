using System;
using System.Collections.Generic;
using HistoryKit;
using XRL;
using XRL.Core;
using XRL.Rules;
using XRL.World;
using XRL.World.WorldBuilders;
using XRL.World.ZoneBuilders;
using Genkit;

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
        //private const string TargetZoneId = "JoppaWorld.11.22.0.1.11"; // this is 1-down, 1-west from Joppa
        private const string TargetZoneId = "JoppaWorld.11.22.0.1.16"; // this is 6-down, 1-west from Joppa
        private const string OwnerProperty = "SubterraneanSites_Owner";
        private const string InitFlag = "SubterraneanSites_TestSultanSiteRegistered";

        private enum SiteKind
        {
            SultanHistoric,

            //later the full set will be 
            //SultanHistoric,
            //BasicLairLegendary,
            //BasicLairDense,
            //BasicLairVendor
        }

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

            //RegisterSultanDungeonSite(siteZoneIds);
            RegisterSelectedSite(siteZoneIds, rng);

            // Temporary path coordinate test.
            // For now this only generates zone IDs. It does not render path material yet.
            SubterraneanPathCoordinateGenerator pathGenerator =
                new SubterraneanPathCoordinateGenerator();

            int steps = rng.Next(10, 16);

            List<string> pathZoneIds = pathGenerator.BuildPathZoneIds(
                siteZoneIds[0],
                steps,
                rng
            );

            List<SubterraneanPathCoordinateGenerator.PathZoneInstruction> pathInstructions =
                pathGenerator.BuildPathInstructions(pathZoneIds);

            string pathMaterial = PickPathMaterial(rng);

            RegisterHorizontalRoadPath(pathInstructions, pathMaterial);

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

        private void RegisterSelectedSite(List<string> siteZoneIds, System.Random rng)
        {
            SiteKind siteKind = RollSiteKind(rng);

            switch (siteKind)
            {
            case SiteKind.SultanHistoric:
                new SultanHistoricSiteRegistrar(this).Register(siteZoneIds);
                break;

            default:
                new SultanHistoricSiteRegistrar(this).Register(siteZoneIds);
                break;
            }
        }

        private SiteKind RollSiteKind(System.Random rng)
        {
            // For now, force the historical-site archetype while this path is stabilized.
            // Later this will become a deterministic weighted roll from rng.
            return SiteKind.SultanHistoric;
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
            //This needs better struture. like a list of the excluded zonebuilders
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
        private class SultanHistoricSiteRegistrar
        {
            private readonly RuntimeZoneBuilderInjectionSystem parent;

            public SultanHistoricSiteRegistrar(RuntimeZoneBuilderInjectionSystem parent)
            {
                this.parent = parent;
            }

            public void Register(List<string> siteZoneIds)
            {
                if (siteZoneIds == null || siteZoneIds.Count == 0)
                {
                    return;
                }

                int originZ = parent.GetZFromZoneId(siteZoneIds[0]);
                int targetTier = parent.GetTierFromZ(originZ);
                int period = SultanDungeon.GetSultanPeriodFromTier(targetTier);

                // 1. Select existing history inputs for this site.
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

                // 2. Build and store the SultanDungeonArgs package.
                // SultanDungeon will fetch this using "sultanDungeonArgs_" + regionName.
                string sourceRegionName =
                    regionSnapshot.GetProperty("newName", regionSnapshot.GetProperty("name", "Unknown Region"));

                string regionName = "SubterraneanSites_" + sourceRegionName;

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

                // 3. Register each vertical layer.
                for (int i = 0; i < siteZoneIds.Count; i++)
                {
                    string zoneId = siteZoneIds[i];

                    if (parent.IsClaimedByOtherContent(zoneId))
                    {
                        continue;
                    }

                    string stairs = parent.GetStairsForLayer(i, siteZoneIds.Count);
                    int z = parent.GetZFromZoneId(zoneId);
                    int tier = parent.GetTierFromZ(z);

                    if (i != 0)
                    {
                        The.ZoneManager.ClearZoneBuilders(zoneId);
                        The.ZoneManager.SetZoneProperty(zoneId, "SkipTerrainBuilders", true);
                    }

                    The.ZoneManager.SetZoneProperty(zoneId, OwnerProperty, "Yes");
                    The.ZoneManager.SetZoneProperty(zoneId, "ZoneTierOverride", tier.ToString());

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

                    if (i == siteZoneIds.Count - 1)
                    {
                        AddBottomLayerVaultWithRelicAndHero(zoneId, regionSnapshot, tier);
                    }

                    The.ZoneManager.SetZoneName(
                        zoneId,
                        "Test: " + sourceRegionName +
                        " T" + tier +
                        " P" + period +
                        " Layer " + (i + 1) + " of " + siteZoneIds.Count,
                        Proper: false
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

                // This is the vanilla hook:
                // SultanDungeon sees Relicstyle="Vault", creates a vault region,
                // and places a cult leader there. PlaceRelicBuilder then puts the relic
                // into the vault chest if one exists.
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
        private void RegisterHorizontalRoadPath(
            List<SubterraneanPathCoordinateGenerator.PathZoneInstruction> pathInstructions,
            string pathMaterial
        )
        {
            if (pathInstructions == null || pathInstructions.Count == 0)
            {
                return;
            }

            foreach (SubterraneanPathCoordinateGenerator.PathZoneInstruction instruction in pathInstructions)
            {
                RegisterRoadPathZone(instruction, pathMaterial);
            }
        }

        private void RegisterRoadPathZone(
            SubterraneanPathCoordinateGenerator.PathZoneInstruction instruction,
            string pathMaterial
        )
        {
            int entryHoleX = GetEntryHoleXForInstruction(instruction);
            int entryHoleY = GetEntryHoleYForInstruction(instruction);
            int exitHoleX = GetExitHoleXForInstruction(instruction);
            int exitHoleY = GetExitHoleYForInstruction(instruction);

            string entryHole = entryHoleX.ToString() + "," + entryHoleY.ToString();
            string exitHole = exitHoleX.ToString() + "," + exitHoleY.ToString();

            The.ZoneManager.AddZoneBuilder(
                instruction.ZoneId,
                6200,
                "SubterraneanPathBuilder",
                "Entry", instruction.Entry,
                "Exit", instruction.Exit,
                "EntryHole", entryHole,
                "ExitHole", exitHole,
                "PathMaterial", pathMaterial
            );
        }

        private string PickPathMaterial(System.Random rng)
        {
            string[] materials =
            {
                "DirtRoad",
                //"DirtPath",
                "SaltPath",
                "FungalTrailBrick",
                "CryptTrail",
                "BrickWalkway",
                "MarbleWalkway",
                "BlackMarbleWalkway",
                "GreyMarbleWalkway"//,
                //"FoamcreteFloor",
                //"SmallHexFloor"
            };

            return materials[rng.Next(materials.Length)];
        }
        

        private int GetEntryHoleXForInstruction(
            SubterraneanPathCoordinateGenerator.PathZoneInstruction instruction
        )
        {
            if (instruction.Entry == "Down")
            {
                return GetHoleXForVerticalTransition(instruction.Index - 1);
            }

            return GetHoleXForVerticalTransition(instruction.Index);
        }

        private int GetEntryHoleYForInstruction(
            SubterraneanPathCoordinateGenerator.PathZoneInstruction instruction
        )
        {
            if (instruction.Entry == "Down")
            {
                return GetHoleYForVerticalTransition(instruction.Index - 1);
            }

            return GetHoleYForVerticalTransition(instruction.Index);
        }

        private int GetExitHoleXForInstruction(
            SubterraneanPathCoordinateGenerator.PathZoneInstruction instruction
        )
        {
            if (instruction.Exit == "Up")
            {
                return GetHoleXForVerticalTransition(instruction.Index);
            }

            return GetHoleXForVerticalTransition(instruction.Index);
        }

        private int GetExitHoleYForInstruction(
            SubterraneanPathCoordinateGenerator.PathZoneInstruction instruction
        )
        {
            if (instruction.Exit == "Up")
            {
                return GetHoleYForVerticalTransition(instruction.Index);
            }

            return GetHoleYForVerticalTransition(instruction.Index);
        }
        private int GetHoleXForVerticalTransition(int verticalIndex)
        {
            int seed = XRLCore.Core.Game.GetWorldSeed(
                TargetZoneId + ":HoleX:" + verticalIndex
            );

            System.Random rng = new System.Random(seed);

            return 40 + rng.Next(-16, 17);
        }

        private int GetHoleYForVerticalTransition(int verticalIndex)
        {
            int seed = XRLCore.Core.Game.GetWorldSeed(
                TargetZoneId + ":HoleY:" + verticalIndex
            );

            System.Random rng = new System.Random(seed);

            return 12 + rng.Next(-8, 9);
        }
    }

    internal class SubterraneanPathCoordinateGenerator
    {
        private enum PathDirection
        {
            Up,
            North,
            South,
            East,
            West
        }

        private enum PathHeading
        {
            North,
            South,
            East,
            West,
            NorthEast,
            NorthWest,
            SouthEast,
            SouthWest
        }

        private enum PathConnection
        {
            None,
            North,
            South,
            East,
            West,
            Up,
            Down,
            Site,
            Surface
        }

        public struct PathZoneInstruction
        {
            public string ZoneId;
            public int Index;
            public string Entry;
            public string Exit;
            public bool IsOrigin;
            public bool IsSurface;

            public PathZoneInstruction(
                string zoneId,
                int index,
                string entry,
                string exit,
                bool isOrigin,
                bool isSurface
            )
            {
                this.ZoneId = zoneId;
                this.Index = index;
                this.Entry = entry;
                this.Exit = exit;
                this.IsOrigin = isOrigin;
                this.IsSurface = isSurface;
            }
        }


        private struct WeightedDirection
        {
            public PathDirection Direction;
            public int Weight;

            public WeightedDirection(PathDirection direction, int weight)
            {
                this.Direction = direction;
                this.Weight = weight;
            }
        }

        private const int MainWeight = 33;
        private const int SideWeight = 17;
        private const int DiagonalComponentWeight = 33;
        private const int UpWeight = 33;

        private struct ZoneCoord
        {
            public string World;
            public int ParasangX;
            public int ParasangY;
            public int ZoneX;
            public int ZoneY;
            public int Z;

            public ZoneCoord(
                string world,
                int parasangX,
                int parasangY,
                int zoneX,
                int zoneY,
                int z
            )
            {
                World = world;
                ParasangX = parasangX;
                ParasangY = parasangY;
                ZoneX = zoneX;
                ZoneY = zoneY;
                Z = z;
            }

            public string ToZoneId()
            {
                return
                    World + "." +
                    ParasangX + "." +
                    ParasangY + "." +
                    ZoneX + "." +
                    ZoneY + "." +
                    Z;
            }
        }

        public List<string> BuildPathZoneIds(
            string originZoneId,
            int steps,
            System.Random rng
        )
        {
            List<string> pathZoneIds = new List<string>();

            if (steps <= 0)
            {
                return pathZoneIds;
            }

            ZoneCoord current = ParseZoneId(originZoneId);

            PathHeading heading = PickPathHeading(rng);
            PathDirection? previousDirection = null;

            pathZoneIds.Add(current.ToZoneId());

            for (int i = 0; i < steps; i++)
            {
                PathDirection direction = PickNextDirection(rng, previousDirection, heading);

                current = Step(current, direction);

                pathZoneIds.Add(current.ToZoneId());

                if (current.Z <= 10)
                {
                    break;
                }

                previousDirection = direction;
            }

            return pathZoneIds;
        }

        public List<PathZoneInstruction> BuildPathInstructions(
            List<string> pathZoneIds
        )
        {
            List<PathZoneInstruction> instructions = new List<PathZoneInstruction>();

            if (pathZoneIds == null || pathZoneIds.Count == 0)
            {
                return instructions;
            }

            for (int i = 0; i < pathZoneIds.Count; i++)
            {
                string currentZoneId = pathZoneIds[i];

                PathConnection entry = PathConnection.None;
                PathConnection exit = PathConnection.None;

                if (i == 0)
                {
                    entry = PathConnection.Site;
                }
                else
                {
                    entry = GetConnectionFromCurrentToOther(
                        currentZoneId,
                        pathZoneIds[i - 1]
                    );
                }

                if (i == pathZoneIds.Count - 1)
                {
                    exit = PathConnection.None;
                }
                else
                {
                    exit = GetConnectionFromCurrentToOther(
                        currentZoneId,
                        pathZoneIds[i + 1]
                    );
                }

                int z = ParseZoneId(currentZoneId).Z;
                bool isSurface = z <= 10;

                if (isSurface && i == pathZoneIds.Count - 1)
                {
                    exit = PathConnection.Surface;
                }

                instructions.Add(new PathZoneInstruction(
                    currentZoneId,
                    i,
                    entry.ToString(),
                    exit.ToString(),
                    i == 0,
                    isSurface
                ));
            }

            return instructions;
        }

        private PathConnection GetConnectionFromCurrentToOther(
            string currentZoneId,
            string otherZoneId
        )
        {
            ZoneCoord current = ParseZoneId(currentZoneId);
            ZoneCoord other = ParseZoneId(otherZoneId);

            if (other.Z < current.Z)
            {
                return PathConnection.Up;
            }

            if (other.Z > current.Z)
            {
                return PathConnection.Down;
            }

            if (IsNorthOf(current, other))
            {
                return PathConnection.North;
            }

            if (IsSouthOf(current, other))
            {
                return PathConnection.South;
            }

            if (IsEastOf(current, other))
            {
                return PathConnection.East;
            }

            if (IsWestOf(current, other))
            {
                return PathConnection.West;
            }

            return PathConnection.None;
        }
        private bool IsNorthOf(ZoneCoord current, ZoneCoord other)
        {
            if (other.ParasangX != current.ParasangX)
            {
                return false;
            }

            if (other.ParasangY == current.ParasangY && other.ZoneX == current.ZoneX && other.ZoneY == current.ZoneY - 1)
            {
                return true;
            }

            if (other.ParasangY == current.ParasangY - 1 && other.ZoneX == current.ZoneX && current.ZoneY == 0 && other.ZoneY == 2)
            {
                return true;
            }

            return false;
        }

        private bool IsSouthOf(ZoneCoord current, ZoneCoord other)
        {
            if (other.ParasangX != current.ParasangX)
            {
                return false;
            }

            if (other.ParasangY == current.ParasangY && other.ZoneX == current.ZoneX && other.ZoneY == current.ZoneY + 1)
            {
                return true;
            }

            if (other.ParasangY == current.ParasangY + 1 && other.ZoneX == current.ZoneX && current.ZoneY == 2 && other.ZoneY == 0)
            {
                return true;
            }

            return false;
        }

        private bool IsEastOf(ZoneCoord current, ZoneCoord other)
        {
            if (other.ParasangY != current.ParasangY)
            {
                return false;
            }

            if (other.ParasangX == current.ParasangX && other.ZoneY == current.ZoneY && other.ZoneX == current.ZoneX + 1)
            {
                return true;
            }

            if (other.ParasangX == current.ParasangX + 1 && other.ZoneY == current.ZoneY && current.ZoneX == 2 && other.ZoneX == 0)
            {
                return true;
            }

            return false;
        }

        private bool IsWestOf(ZoneCoord current, ZoneCoord other)
        {
            if (other.ParasangY != current.ParasangY)
            {
                return false;
            }

            if (other.ParasangX == current.ParasangX && other.ZoneY == current.ZoneY && other.ZoneX == current.ZoneX - 1)
            {
                return true;
            }

            if (other.ParasangX == current.ParasangX - 1 && other.ZoneY == current.ZoneY && current.ZoneX == 0 && other.ZoneX == 2)
            {
                return true;
            }

            return false;
        }

        private PathDirection PickNextDirection(
            System.Random rng,
            PathDirection? previousDirection,
            PathHeading heading
        )
        {
            List<WeightedDirection> candidates = GetWeightedDirectionsForHeading(heading);

            // Simple anti-backtracking rule.
            // This still matters even with heading bias.
            if (previousDirection.HasValue)
            {
                PathDirection? reverse = GetReverseDirection(previousDirection.Value);

                if (reverse.HasValue)
                {
                    for (int i = candidates.Count - 1; i >= 0; i--)
                    {
                        if (candidates[i].Direction == reverse.Value)
                        {
                            candidates.RemoveAt(i);
                        }
                    }
                }
            }

            return PickWeightedDirection(rng, candidates);
        }

        private List<WeightedDirection> GetWeightedDirectionsForHeading(PathHeading heading)
        {
            List<WeightedDirection> directions = new List<WeightedDirection>();

            switch (heading)
            {
                case PathHeading.North:
                    directions.Add(new WeightedDirection(PathDirection.North, MainWeight));
                    directions.Add(new WeightedDirection(PathDirection.Up, UpWeight));
                    directions.Add(new WeightedDirection(PathDirection.East, SideWeight));
                    directions.Add(new WeightedDirection(PathDirection.West, SideWeight));
                    break;

                case PathHeading.South:
                    directions.Add(new WeightedDirection(PathDirection.South, MainWeight));
                    directions.Add(new WeightedDirection(PathDirection.Up, UpWeight));
                    directions.Add(new WeightedDirection(PathDirection.East, SideWeight));
                    directions.Add(new WeightedDirection(PathDirection.West, SideWeight));
                    break;

                case PathHeading.East:
                    directions.Add(new WeightedDirection(PathDirection.East, MainWeight));
                    directions.Add(new WeightedDirection(PathDirection.Up, UpWeight));
                    directions.Add(new WeightedDirection(PathDirection.North, SideWeight));
                    directions.Add(new WeightedDirection(PathDirection.South, SideWeight));
                    break;

                case PathHeading.West:
                    directions.Add(new WeightedDirection(PathDirection.West, MainWeight));
                    directions.Add(new WeightedDirection(PathDirection.Up, UpWeight));
                    directions.Add(new WeightedDirection(PathDirection.North, SideWeight));
                    directions.Add(new WeightedDirection(PathDirection.South, SideWeight));
                    break;

                case PathHeading.NorthEast:
                    directions.Add(new WeightedDirection(PathDirection.North, DiagonalComponentWeight));
                    directions.Add(new WeightedDirection(PathDirection.East, DiagonalComponentWeight));
                    directions.Add(new WeightedDirection(PathDirection.Up, UpWeight));
                    break;

                case PathHeading.NorthWest:
                    directions.Add(new WeightedDirection(PathDirection.North, DiagonalComponentWeight));
                    directions.Add(new WeightedDirection(PathDirection.West, DiagonalComponentWeight));
                    directions.Add(new WeightedDirection(PathDirection.Up, UpWeight));
                    break;

                case PathHeading.SouthEast:
                    directions.Add(new WeightedDirection(PathDirection.South, DiagonalComponentWeight));
                    directions.Add(new WeightedDirection(PathDirection.East, DiagonalComponentWeight));
                    directions.Add(new WeightedDirection(PathDirection.Up, UpWeight));
                    break;
                case PathHeading.SouthWest:
                    directions.Add(new WeightedDirection(PathDirection.South, DiagonalComponentWeight));
                    directions.Add(new WeightedDirection(PathDirection.West, DiagonalComponentWeight));
                    directions.Add(new WeightedDirection(PathDirection.Up, UpWeight));
                    break;
            }

            return directions;
        }

        private PathDirection PickWeightedDirection(
            System.Random rng,
            List<WeightedDirection> candidates
        )
        {
            int totalWeight = 0;

            foreach (WeightedDirection candidate in candidates)
            {
                totalWeight += candidate.Weight;
            }

            if (totalWeight <= 0)
            {
                return PathDirection.Up;
            }

            int roll = rng.Next(1, totalWeight + 1);
            int cumulative = 0;

            foreach (WeightedDirection candidate in candidates)
            {
                cumulative += candidate.Weight;

                if (roll <= cumulative)
                {
                    return candidate.Direction;
                }
            }

            return candidates[candidates.Count - 1].Direction;
        }

        private PathHeading PickPathHeading(System.Random rng)
        {
            Array values = Enum.GetValues(typeof(PathHeading));
            return (PathHeading)values.GetValue(rng.Next(values.Length));
        }


        private PathDirection? GetReverseDirection(PathDirection direction)
        {
            switch (direction)
            {
                case PathDirection.North:
                    return PathDirection.South;

                case PathDirection.South:
                    return PathDirection.North;

                case PathDirection.East:
                    return PathDirection.West;

                case PathDirection.West:
                    return PathDirection.East;

                // Paths only move upward for now, never downward.
                // There is no reverse Up move in the candidate list,
                // so returning Up here has no practical effect.
                case PathDirection.Up:
                default:
                    return null;
            }
        }

        private ZoneCoord Step(ZoneCoord coord, PathDirection direction)
        {
            switch (direction)
            {
                case PathDirection.Up:
                    coord.Z -= 1;
                    break;

                case PathDirection.North:
                    coord = StepNorth(coord);
                    break;

                case PathDirection.South:
                    coord = StepSouth(coord);
                    break;

                case PathDirection.East:
                    coord = StepEast(coord);
                    break;

                case PathDirection.West:
                    coord = StepWest(coord);
                    break;
            }

            return coord;
        }

        private ZoneCoord StepNorth(ZoneCoord coord)
        {
            coord.ZoneY -= 1;

            if (coord.ZoneY < 0)
            {
                coord.ZoneY = 2;
                coord.ParasangY -= 1;
            }

            return coord;
        }

        private ZoneCoord StepSouth(ZoneCoord coord)
        {
            coord.ZoneY += 1;

            if (coord.ZoneY > 2)
            {
                coord.ZoneY = 0;
                coord.ParasangY += 1;
            }

            return coord;
        }

        private ZoneCoord StepEast(ZoneCoord coord)
        {
            coord.ZoneX += 1;

            if (coord.ZoneX > 2)
            {
                coord.ZoneX = 0;
                coord.ParasangX += 1;
            }

            return coord;
        }

        private ZoneCoord StepWest(ZoneCoord coord)
        {
            coord.ZoneX -= 1;

            if (coord.ZoneX < 0)
            {
                coord.ZoneX = 2;
                coord.ParasangX -= 1;
            }

            return coord;
        }

        private ZoneCoord ParseZoneId(string zoneId)
        {
            string[] parts = zoneId.Split('.');

            return new ZoneCoord(
                parts[0],
                int.Parse(parts[1]),
                int.Parse(parts[2]),
                int.Parse(parts[3]),
                int.Parse(parts[4]),
                int.Parse(parts[5])
            );
        }
    }
}
namespace XRL.World.ZoneBuilders
{
    public class SubterraneanPathBuilder
    {
        public string Entry = "None";
        public string Exit = "None";
        public string EntryHole = "40,12";
        public string ExitHole = "40,12";
        public string PathMaterial = "DirtRoad";
        public string HoleObject = "Pit";

        public bool ClearAdjacent = true;
        public bool ClearSolids = true;

        public bool BuildZone(Zone Z)
        {
            Location2D start = GetPointForConnection(Z, Entry);
            Location2D end = GetPointForConnection(Z, Exit);

            if (start == null && end == null)
            {
                return true;
            }

            if (start == null)
            {
                start = GetCenterPoint(Z);
            }

            if (end == null)
            {
                end = GetCenterPoint(Z);
            }

            DrawPath(Z, start, end);

            if (Entry == "Down")
            {
                PlaceHole(Z);
            }

            return true;
        }

        private Location2D GetPointForConnection(Zone Z, string connection)
        {
            switch (connection)
            {
                case "North":
                    return Location2D.Get(40, 0);

                case "South":
                    return Location2D.Get(40, Z.Height - 1);

                case "East":
                    return Location2D.Get(Z.Width - 1, 12);

                case "West":
                    return Location2D.Get(0, 12);

                case "Site":
                    return GetCenterPoint(Z);

                case "Up":
                    return Location2D.Get(GetExitHoleX(), GetExitHoleY());

                case "Down":
                    return Location2D.Get(GetEntryHoleX(), GetEntryHoleY());

                case "Surface":
                    return GetCenterPoint(Z);

                case "None":
                default:
                    return null;
            }
        }

        private Location2D GetCenterPoint(Zone Z)
        {
            return Location2D.Get(Z.Width / 2, Z.Height / 2);
        }

        private void DrawPath(Zone Z, Location2D start, Location2D end)
        {
            Cell startCell = Z.GetCell(start);
            Cell endCell = Z.GetCell(end);

            if (startCell == null || endCell == null)
            {
                return;
            }

            startCell.ClearTerrain();
            endCell.ClearTerrain();

            XRL.World.AI.Pathfinding.FindPath findPath =
                new XRL.World.AI.Pathfinding.FindPath(
                    Z,
                    start.X,
                    start.Y,
                    Z,
                    end.X,
                    end.Y,
                    false,
                    true,
                    null,
                    true
                );

            if (!findPath.Usable)
            {
                return;
            }

            foreach (Cell step in findPath.Steps)
            {
                PaintPathCell(Z, step);

                if (ClearAdjacent)
                {
                    foreach (Cell adjacent in step.GetLocalAdjacentCells())
                    {
                        PaintPathCell(Z, adjacent);
                    }
                }
            }
        }

        private void PaintPathCell(Zone Z, Cell cell)
        {
            if (cell == null)
            {
                return;
            }

            Z.ReachableMap[cell.X, cell.Y] = true;

            if (ClearSolids)
            {
                cell.ClearTerrain();
            }

            if (!cell.HasObjectWithBlueprint(PathMaterial))
            {
                cell.AddObject(GameObjectFactory.Factory.CreateObject(PathMaterial));
            }
        }
        private void PlaceHole(Zone Z)
        {
            int radius = 3;

            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    int x = GetEntryHoleX() + dx;
                    int y = GetEntryHoleY() + dy;

                    Cell cell = Z.GetCell(x, y);

                    if (cell == null)
                    {
                        continue;
                    }

                    int distance = Math.Abs(dx) + Math.Abs(dy);

                    if (distance > radius && !50.in100())
                    {
                        continue;
                    }

                    if (distance > radius + 1)
                    {
                        continue;
                    }

                    cell.Clear();

                    GameObject hole = GameObjectFactory.Factory.CreateObject(HoleObject);

                    if (hole == null)
                    {
                        continue;
                    }

                    XRL.World.Parts.StairsDown stairsDown =
                        hole.GetPart<XRL.World.Parts.StairsDown>();

                    if (stairsDown != null)
                    {
                        stairsDown.ConnectLanding = false;
                    }

                    cell.AddObject("FlyingWhitelistArea");
                    cell.AddObject(hole);
                    cell.AddObject("StairBlocker");
                    cell.AddObject("InfluenceMapBlocker");
                }
            }
        }
        private int GetEntryHoleX()
        {
            return GetCoordPart(EntryHole, 0, 40);
        }

        private int GetEntryHoleY()
        {
            return GetCoordPart(EntryHole, 1, 12);
        }

        private int GetExitHoleX()
        {
            return GetCoordPart(ExitHole, 0, 40);
        }

        private int GetExitHoleY()
        {
            return GetCoordPart(ExitHole, 1, 12);
        }

        private int GetCoordPart(string coord, int index, int fallback)
        {
            if (coord == null)
            {
                return fallback;
            }

            string[] parts = coord.Split(',');

            if (parts.Length <= index)
            {
                return fallback;
            }

            int value;

            if (int.TryParse(parts[index], out value))
            {
                return value;
            }

            return fallback;
        }
    }

    
}