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
using XRL.UI;
using System.Reflection;
using System.Text;
using Qud.API;
using XRL.Language;
using System.Text;


namespace SubterraneanSites
{
    internal struct SubterraneanZoneCoord
    {
        public string World;
        public int ParasangX;
        public int ParasangY;
        public int ZoneX;
        public int ZoneY;
        public int Z;

        public SubterraneanZoneCoord(
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

        public static SubterraneanZoneCoord Parse(string zoneId)
        {
            string[] parts = zoneId.Split('.');

            return new SubterraneanZoneCoord(
                parts[0],
                int.Parse(parts[1]),
                int.Parse(parts[2]),
                int.Parse(parts[3]),
                int.Parse(parts[4]),
                int.Parse(parts[5])
            );
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

        public SubterraneanZoneCoord StepNorth()
        {
            SubterraneanZoneCoord coord = this;

            coord.ZoneY -= 1;

            if (coord.ZoneY < 0)
            {
                coord.ZoneY = 2;
                coord.ParasangY -= 1;
            }

            return coord;
        }

        public SubterraneanZoneCoord StepSouth()
        {
            SubterraneanZoneCoord coord = this;

            coord.ZoneY += 1;

            if (coord.ZoneY > 2)
            {
                coord.ZoneY = 0;
                coord.ParasangY += 1;
            }

            return coord;
        }

        public SubterraneanZoneCoord StepEast()
        {
            SubterraneanZoneCoord coord = this;

            coord.ZoneX += 1;

            if (coord.ZoneX > 2)
            {
                coord.ZoneX = 0;
                coord.ParasangX += 1;
            }

            return coord;
        }

        public SubterraneanZoneCoord StepWest()
        {
            SubterraneanZoneCoord coord = this;

            coord.ZoneX -= 1;

            if (coord.ZoneX < 0)
            {
                coord.ZoneX = 2;
                coord.ParasangX -= 1;
            }

            return coord;
        }

        public SubterraneanZoneCoord StepUp()
        {
            SubterraneanZoneCoord coord = this;
            coord.Z -= 1;
            return coord;
        }
    }

    internal class ProtectedZoneColumn
    {
        public string Name;
        public string World;

        public int ParasangX;
        public int ParasangY;
        public int ZoneX;
        public int ZoneY;

        public int MinZ;
        public int MaxZ;

        public ProtectedZoneColumn(
            string name,
            string world,
            int parasangX,
            int parasangY,
            int zoneX,
            int zoneY,
            int minZ,
            int maxZ
        )
        {
            Name = name;
            World = world;

            ParasangX = parasangX;
            ParasangY = parasangY;
            ZoneX = zoneX;
            ZoneY = zoneY;

            MinZ = minZ;
            MaxZ = maxZ;
        }

        public bool Contains(SubterraneanZoneCoord coord)
        {
            if (coord.World != World)
            {
                return false;
            }

            if (coord.ParasangX != ParasangX)
            {
                return false;
            }

            if (coord.ParasangY != ParasangY)
            {
                return false;
            }

            if (coord.ZoneX != ZoneX)
            {
                return false;
            }

            if (coord.ZoneY != ZoneY)
            {
                return false;
            }

            if (coord.Z < MinZ || coord.Z > MaxZ)
            {
                return false;
            }

            return true;
        }
    }

    internal class ProtectedParasangColumn
    {
        public string Name;
        public string World;

        public int ParasangX;
        public int ParasangY;

        public int MinZ;
        public int MaxZ;

        public ProtectedParasangColumn(
            string name,
            string world,
            int parasangX,
            int parasangY,
            int minZ,
            int maxZ
        )
        {
            Name = name;
            World = world;

            ParasangX = parasangX;
            ParasangY = parasangY;

            MinZ = minZ;
            MaxZ = maxZ;
        }

        public bool Contains(SubterraneanZoneCoord coord)
        {
            if (coord.World != World)
            {
                return false;
            }

            if (coord.ParasangX != ParasangX)
            {
                return false;
            }

            if (coord.ParasangY != ParasangY)
            {
                return false;
            }

            if (coord.Z < MinZ || coord.Z > MaxZ)
            {
                return false;
            }

            return true;
        }
    }

    internal static class SubterraneanSafety
    {
        public static bool IsProtected(string zoneId, out string reason)
        {
            SubterraneanZoneCoord coord = SubterraneanZoneCoord.Parse(zoneId);
            return IsProtected(coord, out reason);
        }

        public static bool IsProtected(SubterraneanZoneCoord coord, out string reason)
        {
            foreach (ProtectedZoneColumn column in SubterraneanProtectedLocations.Columns)
            {
                if (column.Contains(coord))
                {
                    reason = column.Name;
                    return true;
                }
            }

            foreach (ProtectedParasangColumn parasangColumn in SubterraneanProtectedLocations.ParasangColumns)
            {
                if (parasangColumn.Contains(coord))
                {
                    reason = parasangColumn.Name;
                    return true;
                }
            }

            if (SubterraneanDynamicProtectedLocations.IsProtected(coord, out reason))
            {
                return true;
            }

            reason = "";
            return false;
        }
    }

    internal static class SubterraneanDynamicProtectedLocations
    {
        internal static string DescribeVanillaLairProtectionSample(int maxCount)
        {
            StringBuilder text = new StringBuilder();

            text.AppendLine("vanilla lairs captured: " + VanillaLairColumns.Count.ToString());

            int count = 0;

            foreach (ProtectedZoneColumn column in VanillaLairColumns)
            {
                if (count >= maxCount)
                {
                    break;
                }

                text.AppendLine(
                    count.ToString() +
                    ": " +
                    column.Name +
                    " = " +
                    column.World + "." +
                    column.ParasangX.ToString() + "." +
                    column.ParasangY.ToString() + "." +
                    column.ZoneX.ToString() + "." +
                    column.ZoneY.ToString() +
                    ".10-14"
                );

                count++;
            }

            return text.ToString();
        }

        internal static string GetVanillaLairTestZoneId(int index, int z)
        {
            if (VanillaLairColumns == null)
            {
                return "";
            }

            if (index < 0 || index >= VanillaLairColumns.Count)
            {
                return "";
            }

            if (z < 10)
            {
                z = 10;
            }

            ProtectedZoneColumn column = VanillaLairColumns[index];

            return
                column.World + "." +
                column.ParasangX.ToString() + "." +
                column.ParasangY.ToString() + "." +
                column.ZoneX.ToString() + "." +
                column.ZoneY.ToString() + "." +
                z.ToString();
        }


        private static List<ProtectedZoneColumn> VanillaLairColumns =  new List<ProtectedZoneColumn>();

        public static bool IsProtected(SubterraneanZoneCoord coord, out string reason)
        {
            if (IsProtectedHistoricalSite(coord, out reason))
            {
                return true;
            }

            if (IsProtectedVanillaLair(coord, out reason))
            {
                return true;
            }

            if (IsProtectedSecretColumn(coord, "$oboroqorulair", "Oboroqoru", 10, 19, out reason))
            {
                return true;
            }

            if (IsProtectedSecretColumn(coord, "$qasqonlair", "Qas/Qon", 10, 14, out reason))
            {
                return true;
            }

            if (IsProtectedSecretColumn(coord, "$rermadonlair", "Rermadon", 10, 14, out reason))
            {
                return true;
            }

            if (IsProtectedSecretParasang(coord, "$shugruithmouth", "Shug'ruith mouth parasang", 10, 60, out reason))
            {
                return true;
            }

            if (IsProtectedSecretParasang(coord, "$shugruithlair", "Shug'ruith lair parasang", 10, 60, out reason))
            {
                return true;
            }

            reason = "";
            return false;
        }

        private static bool IsProtectedVanillaLair(
            SubterraneanZoneCoord coord,
            out string reason
        )
        {
            foreach (ProtectedZoneColumn column in VanillaLairColumns)
            {
                if (column.Contains(coord))
                {
                    reason = column.Name;
                    return true;
                }
            }

            reason = "";
            return false;
        }

        internal static void CaptureVanillaLairsFromWorldInfo(JoppaWorldBuilder builder)
        {
            VanillaLairColumns.Clear();

            if (builder == null)
            {
                return;
            }

            if (builder.worldInfo == null)
            {
                return;
            }

            if (builder.worldInfo.lairs == null)
            {
                return;
            }

            foreach (GeneratedLocationInfo lair in builder.worldInfo.lairs)
            {
                if (lair == null)
                {
                    continue;
                }

                if (lair.zoneLocation == null)
                {
                    continue;
                }

                string surfaceZoneId =
                    Zone.XYToID(
                        "JoppaWorld",
                        lair.zoneLocation.X,
                        lair.zoneLocation.Y,
                        10
                    );

                SubterraneanZoneCoord lairCoord;

                try
                {
                    lairCoord = SubterraneanZoneCoord.Parse(surfaceZoneId);
                }
                catch
                {
                    continue;
                }

                string name = "vanilla lair";

                if (lair.name != null && lair.name != "")
                {
                    name = "vanilla lair: " + lair.name;
                }

                VanillaLairColumns.Add(
                    new ProtectedZoneColumn(
                        name,
                        lairCoord.World,
                        lairCoord.ParasangX,
                        lairCoord.ParasangY,
                        lairCoord.ZoneX,
                        lairCoord.ZoneY,
                        10,
                        14
                    )
                );
            }
        }

        private static bool IsProtectedHistoricalSite(SubterraneanZoneCoord coord, out string reason)
        {
            for (int i = 0; i < 8; i++)
            {
                string regionName =
                    The.Game.GetStringGameState("SultanDungeonPlacementOrder_" + i.ToString());

                if (regionName == null || regionName == "")
                {
                    continue;
                }

                object position = null;

                try
                {
                    position = The.Game.GetObjectGameState("sultanRegionPosition_" + regionName);
                }
                catch
                {
                    continue;
                }

                int x;
                int y;

                if (!TryGetXY(position, out x, out y))
                {
                    continue;
                }

                if (coord.World == "JoppaWorld" &&
                    coord.ParasangX == x &&
                    coord.ParasangY == y &&
                    coord.ZoneX == 1 &&
                    coord.ZoneY == 1 &&
                    coord.Z >= 10 &&
                    coord.Z <= 19)
                {
                    reason = "historical site " + i.ToString();
                    return true;
                }
            }

            reason = "";
            return false;
        }

        private static bool IsProtectedSecretColumn(
            SubterraneanZoneCoord coord,
            string secretId,
            string name,
            int minZ,
            int maxZ,
            out string reason
        )
        {
            string zoneId = GetSecretZoneId(secretId);

            if (zoneId == null || zoneId == "")
            {
                reason = "";
                return false;
            }

            SubterraneanZoneCoord secretCoord;

            try
            {
                secretCoord = SubterraneanZoneCoord.Parse(zoneId);
            }
            catch
            {
                reason = "";
                return false;
            }

            if (coord.World == secretCoord.World &&
                coord.ParasangX == secretCoord.ParasangX &&
                coord.ParasangY == secretCoord.ParasangY &&
                coord.ZoneX == secretCoord.ZoneX &&
                coord.ZoneY == secretCoord.ZoneY &&
                coord.Z >= minZ &&
                coord.Z <= maxZ)
            {
                reason = name;
                return true;
            }

            reason = "";
            return false;
        }

        private static bool IsProtectedSecretParasang(
            SubterraneanZoneCoord coord,
            string secretId,
            string name,
            int minZ,
            int maxZ,
            out string reason
        )
        {
            string zoneId = GetSecretZoneId(secretId);

            if (zoneId == null || zoneId == "")
            {
                reason = "";
                return false;
            }

            SubterraneanZoneCoord secretCoord;

            try
            {
                secretCoord = SubterraneanZoneCoord.Parse(zoneId);
            }
            catch
            {
                reason = "";
                return false;
            }

            if (coord.World == secretCoord.World &&
                coord.ParasangX == secretCoord.ParasangX &&
                coord.ParasangY == secretCoord.ParasangY &&
                coord.Z >= minZ &&
                coord.Z <= maxZ)
            {
                reason = name;
                return true;
            }

            reason = "";
            return false;
        }

        private static string GetSecretZoneId(string secretId)
        {
            JournalMapNote note = null;

            try
            {
                note = JournalAPI.GetMapNote(secretId);
            }
            catch
            {
                return "";
            }

            if (note == null)
            {
                return "";
            }

            return note.ZoneID;
        }

        // Vanilla AddSultanHistoryLocations stores location2D.Vector2i in
        // sultanRegionPosition_[regionName]. Runtime inspection showed this object is
        // Vector2i from Assembly-CSharp with int fields x and y.
        private static bool TryGetXY(object position, out int x, out int y)
        {
            x = 0;
            y = 0;

            if (position == null)
            {
                return false;
            }

            Type type = position.GetType();

            FieldInfo xField = type.GetField(
                "x",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
            );

            FieldInfo yField = type.GetField(
                "y",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
            );

            if (xField == null || yField == null)
            {
                return false;
            }

            try
            {
                x = Convert.ToInt32(xField.GetValue(position));
                y = Convert.ToInt32(yField.GetValue(position));
                return true;
            }
            catch
            {
                return false;
            }
        }
        
    }

    [JoppaWorldBuilderExtension]
    public class UndergroundSiteJoppaWorldBuilderExtension : IJoppaWorldBuilderExtension
    {
        public override void OnAfterBuild(JoppaWorldBuilder builder)
        {
            SubterraneanDynamicProtectedLocations.CaptureVanillaLairsFromWorldInfo(builder);

            The.Game.RequireSystem<RuntimeZoneBuilderInjectionSystem>();
        }
    }

    public class RuntimeZoneBuilderInjectionSystem : IGameSystem
    {
        //Joppa is "JoppaWorld.11.22.1.1.10"
        //private const string TargetZoneId = "JoppaWorld.11.22.0.1.11"; // this is 1-down, 1-west from Joppa
        //private const string TargetZoneId = "JoppaWorld.11.22.0.1.16"; // this is 6-down, 1-west from Joppa
        //private const string TargetZoneId = "JoppaWorld.10.22.2.1.14"; // this is 4-down, 2-west from Joppa
        //private const string TargetZoneId = "JoppaWorld.11.22.1.1.13"; // this is 3-down from Joppa
        private const string TargetZoneId = "JoppaWorld.11.22.0.1.13"; // this is 13-down, 1-west from Joppa
        //private const string TargetZoneId = "JoppaWorld.10.22.2.1.13"; // this is 3-down, 2-west from Joppa
        private const string OwnerProperty = "SubterraneanSites_Owner";
        private const string InitFlag = "SubterraneanSites_TestSultanSiteRegistered";
        private const bool DebugNameVisitedZonesWithZoneId = false;
        private const bool DebugShowCriticalSecretCoordinates = false;
        private const string CriticalSecretProbeFlag = "SubterraneanSites_CriticalSecretProbeShown_2";

        private const bool DebugShowVanillaLairProtectionSample = false;
        private const string VanillaLairProbeFlag = "SubterraneanSites_VanillaLairProbeShown_1";

        private enum SiteKind
        {
            SultanHistoric,
            BasicLairChaos,
            ProperLair,
            MerchantHive,

        }

        public override void Register(XRLGame game, IEventRegistrar registrar)
        {
            registrar.Register(BeforeZoneBuiltEvent.ID);
            registrar.Register(ZoneActivatedEvent.ID);
        }

        public override bool HandleEvent(BeforeZoneBuiltEvent zoneBuildEvent)
        {

            if (DebugNameVisitedZonesWithZoneId && zoneBuildEvent != null && zoneBuildEvent.Zone != null)
            {
                The.ZoneManager.SetZoneName(
                    zoneBuildEvent.Zone.ZoneID,
                    "ZONE " + zoneBuildEvent.Zone.ZoneID,
                    Proper: false
                );
            }

            //this will become the entry to the system that generates sites as the 
            //player moves through the game.
            //Current plan is detect if player appears in a matrix, and just build. could be a problem
            //if player is a zone that is being built by a very small chance. most of the time this will happen 
            //when the player pops down from the map.
            //if a player is moving through zones, the code will detect if they are in the tile adjacent 
            //to another matrix and generate the sites in that adjacent matrix. if the player for some horrible reason
            //just happens to go to the very corner tile of a matrix then the system has to generate 3. Actually,
            //I think for the corners, if that happens we can fall back on the first system.
            if (The.Game.GetStringGameState(InitFlag) == "Yes")
            {
                return true;
            }

            /*
            //Debug code for lair saftey test
            string targetZoneId = TargetZoneId;

            string lairTestZoneId =
                SubterraneanDynamicProtectedLocations.GetVanillaLairTestZoneId(0, 13);

            if (lairTestZoneId != null && lairTestZoneId != "")
            {
                targetZoneId = lairTestZoneId;
            }*/






            The.Game.SetStringGameState(InitFlag, "Yes");

            int rawSeed = XRLCore.Core.Game.GetWorldSeed();
            int zoneSeed = XRLCore.Core.Game.GetWorldSeed(TargetZoneId + rawSeed);
            //int zoneSeed = XRLCore.Core.Game.GetWorldSeed(targetZoneId + rawSeed);
            System.Random rng = new System.Random(zoneSeed);

            int layers = rng.Next(3, 6); // 3-5 layers for now, but I may make it 3-7 at final

            List<string> siteZoneIds = BuildSiteZoneIds(TargetZoneId, layers);
            //List<string> siteZoneIds = BuildSiteZoneIds(targetZoneId, layers);

            //below checks if site zones are protected
            siteZoneIds = RemoveProtectedZones(siteZoneIds, "site zone");

            if (siteZoneIds.Count == 0)
            {
                The.ZoneManager.SetZoneName(
                    TargetZoneId,
                    //targetZoneId,
                    "SubterraneanSites skipped: all site zones protected",
                    Proper: false
                );

                return true;
            }

            //RegisterSultanDungeonSite(siteZoneIds);
            //RegisterSelectedSite(siteZoneIds, rng);
            SiteKind siteKind = RollSiteKind(rng);
            RegisterSelectedSite(siteZoneIds, siteKind, rng);

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

            pathInstructions = RemoveProtectedPathInstructions(pathInstructions);

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

        private void RegisterSelectedSite(
            List<string> siteZoneIds,
            SiteKind siteKind,
            System.Random rng
        )
        {
            switch (siteKind)
            {
            case SiteKind.SultanHistoric:
                new SultanHistoricSiteRegistrar(this).Register(siteZoneIds);
                break;

            case SiteKind.BasicLairChaos:
                new BasicLairChaosSiteRegistrar(this).Register(siteZoneIds);
                break;

            case SiteKind.ProperLair:
                new ProperLairSiteRegistrar(this).Register(siteZoneIds);
                break;

            case SiteKind.MerchantHive:
                new MerchantHiveSiteRegistrar(this).Register(siteZoneIds);
                break;

            default:
                new SultanHistoricSiteRegistrar(this).Register(siteZoneIds);
                break;
            }
        }

        private SiteKind RollSiteKind(System.Random rng)
        {
            if (rng == null)
            {
                return SiteKind.SultanHistoric;
            }

            // Weighted deterministic site archetype selection.
            //
            // Current weights:
            //   35 = SultanHistoric
            //   30 = ProperLair
            //   25 = BasicLairChaos
            //   10 = MerchantHive
            //
            // These sum to 90. We roll over the total weight rather than 100
            // so no archetype is accidentally assigned the unused 10%.
            int roll = rng.Next(100);

            if (roll < 35)
            {
                return SiteKind.SultanHistoric;
            }

            if (roll < 65)
            {
                return SiteKind.ProperLair;
            }

            if (roll < 90)
            {
                return SiteKind.BasicLairChaos;
            }

            return SiteKind.MerchantHive;
        }

        private List<SubterraneanPathCoordinateGenerator.PathZoneInstruction> RemoveProtectedPathInstructions(
            List<SubterraneanPathCoordinateGenerator.PathZoneInstruction> pathInstructions
        )
        {
            List<SubterraneanPathCoordinateGenerator.PathZoneInstruction> safeInstructions =
                new List<SubterraneanPathCoordinateGenerator.PathZoneInstruction>();

            if (pathInstructions == null)
            {
                return safeInstructions;
            }

            foreach (SubterraneanPathCoordinateGenerator.PathZoneInstruction instruction in pathInstructions)
            {
                string safetyReason;

                if (SubterraneanSafety.IsProtected(instruction.ZoneId, out safetyReason))
                {
                    The.ZoneManager.SetZoneName(
                        instruction.ZoneId,
                        "SubterraneanSites skipped path zone: " + safetyReason,
                        Proper: false
                    );

                    continue;
                }

                safeInstructions.Add(instruction);
            }

            return safeInstructions;
        }

        private List<string> RemoveProtectedZones(
            List<string> zoneIds,
            string debugLabel
        )
        {
            List<string> safeZoneIds = new List<string>();

            if (zoneIds == null)
            {
                return safeZoneIds;
            }

            foreach (string zoneId in zoneIds)
            {
                string safetyReason;

                if (SubterraneanSafety.IsProtected(zoneId, out safetyReason))
                {
                    //in release we should remove this s owe dont overwrite 
                    //the games native zone name of a protected site
                    The.ZoneManager.SetZoneName(
                        zoneId,
                        "SubterraneanSites skipped " + debugLabel + ": " + safetyReason,
                        Proper: false
                    );

                    continue;
                }

                safeZoneIds.Add(zoneId);
            }

            return safeZoneIds;
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

        internal int GetZFromZoneId(string id)
        {
            string[] parts = id.Split('.');
            return int.Parse(parts[5]);
        }

        internal int GetTierFromZ(int z)
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

        public override bool HandleEvent(ZoneActivatedEvent zoneActivatedEvent)
        {
            //debug popup for girsh lair test
            MaybeShowCriticalSecretCoordinateProbe();
            MaybeShowVanillaLairProtectionProbe();

            if (zoneActivatedEvent == null || zoneActivatedEvent.Zone == null)
            {
                return true;
            }

            string zoneId = zoneActivatedEvent.Zone.ZoneID;

            string isSiteOrigin =
                The.ZoneManager.GetZoneProperty(zoneId, "SubterraneanSites_IsSiteOrigin") as string;

            if (isSiteOrigin != "Yes")
            {
                return true;
            }

            string siteDisplayName =
                The.ZoneManager.GetZoneProperty(zoneId, "SubterraneanSites_SiteDisplayName") as string;

            if (siteDisplayName == null || siteDisplayName == "")
            {
                siteDisplayName = "a forgotten historical site";
            }

            string discoveryKey =
                The.ZoneManager.GetZoneProperty(zoneId, "SubterraneanSites_DiscoveryKey") as string;

            if (discoveryKey == null || discoveryKey == "")
            {
                discoveryKey = "SubterraneanSites_Discovered_" + zoneId;
            }

            if (The.Game.GetStringGameState(discoveryKey) == "Yes")
            {
                return true;
            }

            The.Game.SetStringGameState(discoveryKey, "Yes");

            Popup.Show("You have discovered " + siteDisplayName + ".");

            return true;
        }

        private void MaybeShowVanillaLairProtectionProbe()
        {
            if (!DebugShowVanillaLairProtectionSample)
            {
                return;
            }

            if (The.Game.GetStringGameState(VanillaLairProbeFlag) == "Yes")
            {
                return;
            }

            The.Game.SetStringGameState(VanillaLairProbeFlag, "Yes");

            Popup.Show(
                SubterraneanDynamicProtectedLocations
                    .DescribeVanillaLairProtectionSample(5)
            );
        }

        private void MaybeShowCriticalSecretCoordinateProbe()
        {
            if (!DebugShowCriticalSecretCoordinates)
            {
                return;
            }

            if (The.Game.GetStringGameState(CriticalSecretProbeFlag) == "Yes")
            {
                return;
            }

            The.Game.SetStringGameState(CriticalSecretProbeFlag, "Yes");

            string message =
                DescribeMapNoteSecret("$shugruithmouth") + ", " +
                DescribeMapNoteSecret("$shugruithlair") + ", " +
                DescribeMapNoteSecret("$qasqonlair") + ", " +
                DescribeMapNoteSecret("$rermadonlair") + ", " +
                DescribeMapNoteSecret("$oboroqorulair") + "\n\n" +
                DescribeHistoricalSiteCoordinates();

            Popup.Show(message);
        }
        private string DescribeMapNoteSecret(string secretId)
        {
            JournalMapNote note = null;

            try
            {
                note = JournalAPI.GetMapNote(secretId);
            }
            catch (Exception ex)
            {
                return secretId + "=ERR:" + ex.GetType().Name;
            }

            if (note == null)
            {
                return secretId + "=NULL";
            }

            if (note.ZoneID == null || note.ZoneID == "")
            {
                return secretId + "=FOUND_NO_ZONE";
            }

            return secretId + "=" + note.ZoneID;
        }
        private string DescribeHistoricalSiteCoordinates()
        {
            StringBuilder text = new StringBuilder();

            text.AppendLine("historical sites:");

            for (int i = 0; i < 8; i++)
            {
                string regionName =
                    The.Game.GetStringGameState("SultanDungeonPlacementOrder_" + i.ToString());

                if (regionName == null || regionName == "")
                {
                    text.AppendLine("hist" + i.ToString() + "=NO_REGION");
                    continue;
                }

                object position = null;

                try
                {
                    position = The.Game.GetObjectGameState("sultanRegionPosition_" + regionName);
                    
                    text.AppendLine(
                        "hist" + i.ToString() +
                        " " +
                        DescribeStoredVectorObject(position)
                    );
                }
                catch (Exception ex)
                {
                    text.AppendLine("hist" + i.ToString() + "=" + regionName + "=ERR:" + ex.GetType().Name);
                    continue;
                }

                string zoneId = TryBuildHistoricalSiteZoneIdFromStoredVector(position);

                if (zoneId == null || zoneId == "")
                {
                    text.AppendLine(
                        "hist" + i.ToString() +
                        "=" + regionName +
                        "=NO_POS:" +
                        (position == null ? "null" : position.GetType().FullName + ":" + position.ToString())
                    );

                    continue;
                }

                text.AppendLine(
                    "hist" + i.ToString() +
                    "=" + zoneId +
                    "-20"
                );
            }

            return text.ToString();
        }
        private string DescribeStoredVectorObject(object position)
        {
            if (position == null)
            {
                return "type=null";
            }

            Type type = position.GetType();

            StringBuilder text = new StringBuilder();

            text.Append("type=");
            text.Append(type.FullName);
            text.Append(" asm=");
            text.Append(type.Assembly.GetName().Name);

            FieldInfo[] fields =
                type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (FieldInfo field in fields)
            {
                object value = null;

                try
                {
                    value = field.GetValue(position);
                }
                catch
                {
                    value = "?";
                }

                text.Append(" field ");
                text.Append(field.Name);
                text.Append(":");
                text.Append(field.FieldType.Name);
                text.Append("=");
                text.Append(value == null ? "null" : value.ToString());
            }

            return text.ToString();
        }
        // Vanilla AddSultanHistoryLocations stores location2D.Vector2i in
        // sultanRegionPosition_[regionName]. At runtime this is Vector2i from
        // Assembly-CSharp with int fields x and y. These are parasang coordinates.
        // Historical sites use the center local zone: JoppaWorld.x.y.1.1.Z.
        private string TryBuildHistoricalSiteZoneIdFromStoredVector(object position)
        {
            if (position == null)
            {
                return "";
            }

            Type type = position.GetType();

            FieldInfo xField = type.GetField(
                "x",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
            );

            FieldInfo yField = type.GetField(
                "y",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
            );

            if (xField == null || yField == null)
            {
                return "";
            }

            try
            {
                int x = Convert.ToInt32(xField.GetValue(position));
                int y = Convert.ToInt32(yField.GetValue(position));

                return "JoppaWorld." + x.ToString() + "." + y.ToString() + ".1.1.10";
            }
            catch
            {
                return "";
            }
        }

        internal void RegisterLayeredSite(
            List<string> siteZoneIds,
            string siteDisplayName,
            string discoveryKey,
            Action<SiteLayerContext> registerLayer
        )
        {
            if (siteZoneIds == null || siteZoneIds.Count == 0)
            {
                return;
            }

            for (int i = 0; i < siteZoneIds.Count; i++)
            {
                string zoneId = siteZoneIds[i];

                string safetyReason;

                if (SubterraneanSafety.IsProtected(zoneId, out safetyReason))
                {
                    The.ZoneManager.SetZoneName(
                        zoneId,
                        "site out: " + safetyReason + " at " + zoneId,
                        Proper: false
                    );

                    continue;
                }

                string stairs = GetStairsForLayer(i, siteZoneIds.Count);
                int z = GetZFromZoneId(zoneId);
                int tier = GetTierFromZ(z);

                if (i != 0)
                {
                    The.ZoneManager.ClearZoneBuilders(zoneId);
                    The.ZoneManager.SetZoneProperty(zoneId, "SkipTerrainBuilders", true);
                }

                The.ZoneManager.SetZoneProperty(zoneId, OwnerProperty, "Yes");
                The.ZoneManager.SetZoneProperty(zoneId, "ZoneTierOverride", tier.ToString());

                SiteLayerContext context = new SiteLayerContext();
                context.ZoneId = zoneId;
                context.LayerIndex = i;
                context.LayerCount = siteZoneIds.Count;
                context.Z = z;
                context.Tier = tier;
                context.Stairs = stairs;
                context.IsOrigin = i == 0;
                context.IsBottom = i == siteZoneIds.Count - 1;

                registerLayer(context);

                if (context.IsOrigin)
                {
                    The.ZoneManager.SetZoneProperty(zoneId, "SubterraneanSites_IsSiteOrigin", "Yes");
                    The.ZoneManager.SetZoneProperty(zoneId, "SubterraneanSites_SiteDisplayName", siteDisplayName);
                    The.ZoneManager.SetZoneProperty(zoneId, "SubterraneanSites_DiscoveryKey", discoveryKey);
                }

                The.ZoneManager.SetZoneName(
                    zoneId,
                    siteDisplayName,
                    Proper: true
                );
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
            string safetyReason;

            if (SubterraneanSafety.IsProtected(instruction.ZoneId, out safetyReason))
            {
                The.ZoneManager.SetZoneName(
                    instruction.ZoneId,
                    "SubterraneanSites refused path builder: " + safetyReason,
                    Proper: false
                );

                return;
            }

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
                //"BlackMarbleWalkway",
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

            SubterraneanZoneCoord current = SubterraneanZoneCoord.Parse(originZoneId);

            PathHeading heading = PickPathHeading(rng);
            PathDirection? previousDirection = null;

            pathZoneIds.Add(current.ToZoneId());

            for (int i = 0; i < steps; i++)
            {
                // TEMPORARY TEST HACK:
                // Use forced east only for Waterlogged Tunnel collision testing.
                // Restore PickNextDirection before normal development.
                //PathDirection direction = PathDirection.East;

                // Normal behavior:
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

                //int z = ParseZoneId(currentZoneId).Z;
                int z = SubterraneanZoneCoord.Parse(currentZoneId).Z;

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
            SubterraneanZoneCoord current = SubterraneanZoneCoord.Parse(currentZoneId);
            SubterraneanZoneCoord other = SubterraneanZoneCoord.Parse(otherZoneId);


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

        private bool IsNorthOf(SubterraneanZoneCoord current, SubterraneanZoneCoord other)
        {
            if (other.ParasangX != current.ParasangX)
            {
                return false;
            }

            if (other.ParasangY == current.ParasangY &&
                other.ZoneX == current.ZoneX &&
                other.ZoneY == current.ZoneY - 1)
            {
                return true;
            }

            if (other.ParasangY == current.ParasangY - 1 &&
                other.ZoneX == current.ZoneX &&
                current.ZoneY == 0 &&
                other.ZoneY == 2)
            {
                return true;
            }

            return false;
        }

        private bool IsSouthOf(SubterraneanZoneCoord current, SubterraneanZoneCoord other)
        {
            if (other.ParasangX != current.ParasangX)
            {
                return false;
            }

            if (other.ParasangY == current.ParasangY &&
                other.ZoneX == current.ZoneX &&
                other.ZoneY == current.ZoneY + 1)
            {
                return true;
            }

            if (other.ParasangY == current.ParasangY + 1 &&
                other.ZoneX == current.ZoneX &&
                current.ZoneY == 2 &&
                other.ZoneY == 0)
            {
                return true;
            }

            return false;
        }

        private bool IsEastOf(SubterraneanZoneCoord current, SubterraneanZoneCoord other)
        {
            if (other.ParasangY != current.ParasangY)
            {
                return false;
            }

            if (other.ParasangX == current.ParasangX &&
                other.ZoneY == current.ZoneY &&
                other.ZoneX == current.ZoneX + 1)
            {
                return true;
            }

            if (other.ParasangX == current.ParasangX + 1 &&
                other.ZoneY == current.ZoneY &&
                current.ZoneX == 2 &&
                other.ZoneX == 0)
            {
                return true;
            }

            return false;
        }

        private bool IsWestOf(SubterraneanZoneCoord current, SubterraneanZoneCoord other)
        {
            if (other.ParasangY != current.ParasangY)
            {
                return false;
            }

            if (other.ParasangX == current.ParasangX &&
                other.ZoneY == current.ZoneY &&
                other.ZoneX == current.ZoneX - 1)
            {
                return true;
            }

            if (other.ParasangX == current.ParasangX - 1 &&
                other.ZoneY == current.ZoneY &&
                current.ZoneX == 0 &&
                other.ZoneX == 2)
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

        private SubterraneanZoneCoord Step(
            SubterraneanZoneCoord coord,
            PathDirection direction
        )
        {
            switch (direction)
            {
                case PathDirection.Up:
                    return coord.StepUp();

                case PathDirection.North:
                    return coord.StepNorth();

                case PathDirection.South:
                    return coord.StepSouth();

                case PathDirection.East:
                    return coord.StepEast();

                case PathDirection.West:
                    return coord.StepWest();
            }

            return coord;
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

        public string MarkerObject = "RandomPathStatue";
        public int MarkerChance = 100;

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

            if (!MarkerObject.IsNullOrEmpty() && Stat.Random(1, 100) <= MarkerChance)
            {
                PlaceExitMarker(Z);
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
        private void PlaceExitMarker(Zone Z)
        {
            Location2D markerPoint = GetPointForConnection(Z, Entry);

            if (markerPoint == null)
            {
                return;
            }

            Cell markerCell = FindMarkerCellNear(Z, markerPoint);

            if (markerCell == null)
            {
                return;
            }

            markerCell.ClearTerrain();

            string markerBlueprint = PickMarkerObject();

            GameObject marker = GameObjectFactory.Factory.CreateObject(markerBlueprint);

            if (marker != null)
            {
                markerCell.AddObject(marker);
            }
        }

        private Cell FindMarkerCellNear(Zone Z, Location2D point)
        {
            for (int radius = 1; radius <= 3; radius++)
            {
                foreach (Cell cell in Z.GetCell(point).GetLocalAdjacentCellsCircular(radius, true))
                {
                    if (cell == null)
                    {
                        continue;
                    }

                    if (cell.IsEmptyOfSolid())
                    {
                        return cell;
                    }
                }
            }

            return null;
        }
        private string PickMarkerObject()
        {
            if (MarkerObject != "RandomPathStatue")
            {
                return MarkerObject;
            }

            string[] markers =
            {
                //"Random Stone Statue",
                //"Random Marble Statue",
                "EaterStatue",
                "EaterStatueFlipped",
                "ImplantedEaterStatue"
            };

            return markers[Stat.Random(0, markers.Length - 1)];
        }
    }

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
                table = "SubterraneanSites_Tier" + Tier.ToString() + "_Mobs";
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