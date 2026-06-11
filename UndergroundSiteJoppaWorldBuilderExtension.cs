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
using System.Collections;


namespace SubterraneanSites
{

    //This is the bootstrab that allows runs 
    //after a game is loaded () and registers RuntimeZoneBuilderInjectionSystem
    //which is needed first to do anything. 
    //Also launches the saftey system and dynamic site detections/protections
    //HasCAllAfterGameliaded tags the class with that meta distinction
    [HasCallAfterGameLoaded]
    public static class SubterraneanSitesBootstrap
    {
        [CallAfterGameLoaded]
        public static void AfterGameLoaded()
        {
            if (The.Game == null)
            {
                return;
            }

            RuntimeZoneBuilderInjectionSystem system =
                The.Game.RequireSystem<RuntimeZoneBuilderInjectionSystem>();

            system.EnsureSafetyReady();
        }
    }

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
        // Convenience overload for callers that only have a zone ID string.
        // Converts the zone ID into SubterraneanZoneCoord, then delegates to
        // the coordinate-based protection check below.
        public static bool IsProtected(string zoneId, out string reason)
        {
            SubterraneanZoneCoord coord = SubterraneanZoneCoord.Parse(zoneId);
            return IsProtected(coord, out reason);
        }

        // Core protection check. Tests a parsed coordinate against static
        // protected zone columns, static protected parasang columns, and
        // dynamically recovered vanilla protected locations.
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

        internal static bool InitializeRuntimeProtection()
        {
            ClearRuntimeProtection();

            bool lairsOk = CaptureVanillaLairsFromRuntimeWorldInfo();
            bool historicalOk = CanReadHistoricalSiteProtection();
            bool specialsOk = CanReadSpecialProtectionSources();

            return lairsOk && historicalOk && specialsOk;
        }

        internal static bool HasRuntimeProtectionData()
        {
            return VanillaLairColumns != null && VanillaLairColumns.Count > 0;
        }

        private static void ClearRuntimeProtection()
        {
            if (VanillaLairColumns == null)
            {
                VanillaLairColumns = new List<ProtectedZoneColumn>();
            }

            VanillaLairColumns.Clear();
        }

        internal static bool CaptureVanillaLairsFromRuntimeWorldInfo()
        {
            if (VanillaLairColumns == null)
            {
                VanillaLairColumns = new List<ProtectedZoneColumn>();
            }

            VanillaLairColumns.Clear();

            if (The.Game == null)
            {
                return false;
            }

            object worldInfo = null;

            try
            {
                worldInfo = The.Game.GetObjectGameState("JoppaWorldInfo");
            }
            catch
            {
                return false;
            }

            if (worldInfo == null)
            {
                return false;
            }

            FieldInfo lairsField = worldInfo.GetType().GetField(
                "lairs",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
            );

            if (lairsField == null)
            {
                return false;
            }

            object lairsObject = null;

            try
            {
                lairsObject = lairsField.GetValue(worldInfo);
            }
            catch
            {
                return false;
            }

            IEnumerable lairs = lairsObject as IEnumerable;

            if (lairs == null)
            {
                return false;
            }

            foreach (object lair in lairs)
            {
                TryAddVanillaLairColumnFromGeneratedLocationInfo(lair);
            }

            return VanillaLairColumns.Count > 0;
        }

        private static bool TryAddVanillaLairColumnFromGeneratedLocationInfo(object lair)
        {
            if (lair == null)
            {
                return false;
            }

            Type type = lair.GetType();

            FieldInfo zoneLocationField = type.GetField(
                "zoneLocation",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
            );

            if (zoneLocationField == null)
            {
                return false;
            }

            object zoneLocation = null;

            try
            {
                zoneLocation = zoneLocationField.GetValue(lair);
            }
            catch
            {
                return false;
            }

            int x;
            int y;

            if (!TryGetLocationXY(zoneLocation, out x, out y))
            {
                return false;
            }

            string surfaceZoneId = Zone.XYToID("JoppaWorld", x, y, 10);

            SubterraneanZoneCoord lairCoord;

            try
            {
                lairCoord = SubterraneanZoneCoord.Parse(surfaceZoneId);
            }
            catch
            {
                return false;
            }

            string name = TryGetStringField(lair, "name");

            if (name == null || name == "")
            {
                name = "vanilla lair";
            }
            else
            {
                name = "vanilla lair: " + name;
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

            return true;
        }

        private static string TryGetStringField(object obj, string fieldName)
        {
            if (obj == null || fieldName == null || fieldName == "")
            {
                return "";
            }

            FieldInfo field = obj.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
            );

            if (field == null)
            {
                return "";
            }

            try
            {
                object value = field.GetValue(obj);

                if (value == null)
                {
                    return "";
                }

                return value.ToString();
            }
            catch
            {
                return "";
            }
        }

        private static bool TryGetLocationXY(object location, out int x, out int y)
        {
            x = 0;
            y = 0;

            if (location == null)
            {
                return false;
            }

            int foundX;
            int foundY;

            if (TryGetIntMember(location, "X", out foundX) &&
                TryGetIntMember(location, "Y", out foundY))
            {
                x = foundX;
                y = foundY;
                return true;
            }

            if (TryGetIntMember(location, "x", out foundX) &&
                TryGetIntMember(location, "y", out foundY))
            {
                x = foundX;
                y = foundY;
                return true;
            }

            return false;
        }

        private static bool TryGetIntMember(object obj, string name, out int value)
        {
            value = 0;

            if (obj == null || name == null || name == "")
            {
                return false;
            }

            Type type = obj.GetType();

            PropertyInfo property = type.GetProperty(
                name,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
            );

            if (property != null)
            {
                try
                {
                    object propertyValue = property.GetValue(obj, null);

                    if (propertyValue != null)
                    {
                        value = Convert.ToInt32(propertyValue);
                        return true;
                    }
                }
                catch
                {
                }
            }

            FieldInfo field = type.GetField(
                name,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
            );

            if (field != null)
            {
                try
                {
                    object fieldValue = field.GetValue(obj);

                    if (fieldValue != null)
                    {
                        value = Convert.ToInt32(fieldValue);
                        return true;
                    }
                }
                catch
                {
                }
            }

            return false;
        }

        private static bool CanReadHistoricalSiteProtection()
        {
            if (The.Game == null)
            {
                return false;
            }

            int found = 0;

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

                if (TryGetXY(position, out x, out y))
                {
                    found++;
                }
            }

            return found == 8;
        }

        private static bool CanReadSpecialProtectionSources()
        {
            return
                HasSecretZoneId("$oboroqorulair") &&
                HasSecretZoneId("$qasqonlair") &&
                HasSecretZoneId("$rermadonlair") &&
                HasSecretZoneId("$shugruithmouth") &&
                HasSecretZoneId("$shugruithlair");
        }

        private static bool HasSecretZoneId(string secretId)
        {
            string zoneId = GetSecretZoneId(secretId);
            return zoneId != null && zoneId != "";
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

            //reflection ---- reflection 
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

    // Marks this class as a Joppa world-builder extension.
    // Qud discovers this class through the attribute during Joppa worldgen
    // and calls OnAfterBuild(...) after the world has been generated.
    // We use this as the genesis bootstrap path for new worlds.
    [JoppaWorldBuilderExtension]
    public class UndergroundSiteJoppaWorldBuilderExtension : IJoppaWorldBuilderExtension
    {
        //OnAfterBuild runs after World Generation
        //It is a hook we can and do use to register RuntimeZoneBuilderInjectionSystem
        //Also to kick off the saftey system
        public override void OnAfterBuild(JoppaWorldBuilder builder)
        {
            RuntimeZoneBuilderInjectionSystem system =
                The.Game.RequireSystem<RuntimeZoneBuilderInjectionSystem>();

            system.EnsureSafetyReady();
        }
    }

    public class RuntimeZoneBuilderInjectionSystem : IGameSystem
    {

        private const string OwnerProperty = "SubterraneanSites_Owner";
        //##private const string InitFlag = "SubterraneanSites_TestSultanSiteRegistered";

        //This flat replaces zone names with zone coordinates
        private const bool DebugNameVisitedZonesWithZoneId = true;
        private const string SafetyFailureReportedFlag = "SubterraneanSites_SafetyFailureReported_v1";
        private const int MatrixParasangWidth = 4; // 4 is normal
        private const int MatrixParasangHeight = 5; // 5 is normal
        private const int MatrixDepth = 5;
        private const int SurfaceZ = 10;

        // Release candidate density: each slot attempts a site; safety/protection checks may still reject it.
        private const int MatrixSiteChancePercent = 100; 
        private const int MinSurfaceMatrixOriginZ = 11;
        private const int MinPathSteps = 30;
        private const int MaxPathStepsExclusive = 41;
        private const bool DebugShowMatrixGenerationPopup = false;

        private bool safetyReadyThisSession;

        private struct SubterraneanMatrixCoord
        {
            public string World;
            public int X;
            public int Y;
            public int Z;

            public string ToId()
            {
                return World + ":" + X.ToString() + ":" + Y.ToString() + ":" + Z.ToString();
            }

            public string ToGameStateKey()
            {
                return "SubterraneanSites_MatrixStatus_" + ToId();
            }
        }

        private SubterraneanMatrixCoord GetMatrixForZone(SubterraneanZoneCoord coord)
        {
            SubterraneanMatrixCoord matrix = new SubterraneanMatrixCoord();

            matrix.World = coord.World;
            matrix.X = coord.ParasangX / MatrixParasangWidth;
            matrix.Y = coord.ParasangY / MatrixParasangHeight;
            matrix.Z = (coord.Z - SurfaceZ) / MatrixDepth;

            if (matrix.Z < 0)
            {
                matrix.Z = 0;
            }

            return matrix;
        }

        private string GetMatrixStatus(SubterraneanMatrixCoord matrix)
        {
            return The.Game.GetStringGameState(matrix.ToGameStateKey());
        }

        // Stores the matrix processing result in game state.
        // Any non-empty matrix status means this matrix has already been processed
        // and should not generate again. Status strings are useful for debugging
        // why a matrix did or did not create content.
        private void SetMatrixStatus(SubterraneanMatrixCoord matrix, string status)
        {
            The.Game.SetStringGameState(matrix.ToGameStateKey(), status);
        }

        private bool IsMatrixProcessed(SubterraneanMatrixCoord matrix)
        {
            string status = GetMatrixStatus(matrix);
            return status != null && status != "";
        }

        private enum MatrixSiteSlot
        {
            A,
            B,
            C,
            D
        }

        private bool GetOriginBoundsForSlot(
            SubterraneanMatrixCoord matrix,
            MatrixSiteSlot slot,
            out int minGlobalZoneX,
            out int maxGlobalZoneX,
            out int minGlobalZoneY,
            out int maxGlobalZoneY
        )
        {
            int minPX = matrix.X * MatrixParasangWidth;
            int maxPX = minPX + MatrixParasangWidth - 1;

            int minPY = matrix.Y * MatrixParasangHeight;
            int maxPY = minPY + MatrixParasangHeight - 1;

            int slotMinPX = minPX;
            int slotMaxPX = maxPX;
            int slotMinPY = minPY;
            int slotMaxPY = maxPY;

            switch (slot)
            {
                case MatrixSiteSlot.A:
                    slotMinPX = minPX;
                    slotMaxPX = minPX + 1;
                    slotMinPY = minPY;
                    slotMaxPY = minPY + 2;
                    break;

                case MatrixSiteSlot.B:
                    slotMinPX = minPX + 2;
                    slotMaxPX = maxPX;
                    slotMinPY = minPY;
                    slotMaxPY = minPY + 2;
                    break;

                case MatrixSiteSlot.C:
                    slotMinPX = minPX;
                    slotMaxPX = minPX + 1;
                    slotMinPY = minPY + 2;
                    slotMaxPY = maxPY;
                    break;

                case MatrixSiteSlot.D:
                    slotMinPX = minPX + 2;
                    slotMaxPX = maxPX;
                    slotMinPY = minPY + 2;
                    slotMaxPY = maxPY;
                    break;
            }

            // Convert slot parasang bounds to global zone bounds.
            minGlobalZoneX = slotMinPX * 3;
            maxGlobalZoneX = slotMaxPX * 3 + 2;

            minGlobalZoneY = slotMinPY * 3;
            maxGlobalZoneY = slotMaxPY * 3 + 2;

            // Exclude only the outermost zone border of the whole top matrix.
            int matrixMinGlobalZoneX = minPX * 3 + 1;
            int matrixMaxGlobalZoneX = maxPX * 3 + 1;

            int matrixMinGlobalZoneY = minPY * 3 + 1;
            int matrixMaxGlobalZoneY = maxPY * 3 + 1;

            minGlobalZoneX = Math.Max(minGlobalZoneX, matrixMinGlobalZoneX);
            maxGlobalZoneX = Math.Min(maxGlobalZoneX, matrixMaxGlobalZoneX);

            minGlobalZoneY = Math.Max(minGlobalZoneY, matrixMinGlobalZoneY);
            maxGlobalZoneY = Math.Min(maxGlobalZoneY, matrixMaxGlobalZoneY);

            return
                maxGlobalZoneX >= minGlobalZoneX &&
                maxGlobalZoneY >= minGlobalZoneY;
        }

        private string GetSlotName(MatrixSiteSlot slot)
        {
            return slot.ToString();
        }

        private enum SiteKind
        {
            SultanHistoric,
            BasicLairChaos,
            ProperLair,
            MerchantHive,
        }

        // Called by Qud's IGameSystem infrastructure when this system is added
        // or loaded. We do not call this directly. Registering these event IDs
        // tells the game to route BeforeZoneBuiltEvent and ZoneActivatedEvent
        // to this system's HandleEvent(...) methods.
        public override void Register(XRLGame game, IEventRegistrar registrar)
        {
            //mod will not work if below are not registered
            registrar.Register(BeforeZoneBuiltEvent.ID);
            registrar.Register(ZoneActivatedEvent.ID);
        }

        // Fires while a zone is being built. AddZoneBuilder(...) is too late
        // to affect this same zone once it is already in the build pipeline,
        // but this event is still useful for lightweight diagnostics such as
        // debug zone naming.
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

            return true;
        }

        // Fires when a zone becomes the active/current zone. This is the main
        // matrix trigger: use the player's active zone to determine the current
        // matrix, process that matrix once, and register future site/path zones.
        // Also handles site discovery when the activated zone is a site origin.
        public override bool HandleEvent(ZoneActivatedEvent zoneActivatedEvent)
        {
             if (zoneActivatedEvent == null || zoneActivatedEvent.Zone == null)
            {
                return true;
            }

            //Saftey first!
            //Skip if not safe... no site for you!
            //uninstall mad and play Animal Croissing if this happens
            if (!EnsureSafetyReady())
            {
                return true;
            }

            //Entry for matrix system
            ProcessMatrixForZone(zoneActivatedEvent.Zone.ZoneID);

            //checks if site is discovered and adds a popup if not
            HandleSiteDiscovery(zoneActivatedEvent.Zone.ZoneID);

            return true;
        }

        //checks if site is discovered and adds a popup if not
        private void HandleSiteDiscovery(string zoneId)
        {
            string isSiteLayer =
                The.ZoneManager.GetZoneProperty(zoneId, "SubterraneanSites_IsSiteLayer") as string;

            if (isSiteLayer != "Yes")
            {
                return;
            }

            string siteDisplayName =
                The.ZoneManager.GetZoneProperty(zoneId, "SubterraneanSites_SiteDisplayName") as string;

            if (siteDisplayName == null || siteDisplayName == "")
            {
                siteDisplayName = "A forgotten historical site";
            }

            string discoveryKey =
                The.ZoneManager.GetZoneProperty(zoneId, "SubterraneanSites_DiscoveryKey") as string;

            if (discoveryKey == null || discoveryKey == "")
            {
                discoveryKey = "SubterraneanSites_Discovered_" + zoneId;
            }

            if (The.Game.GetStringGameState(discoveryKey) == "Yes")
            {
                return;
            }

            The.Game.SetStringGameState(discoveryKey, "Yes");

            Popup.Show("You have discovered " + siteDisplayName + ".");
        }

        private void ProcessMatrixForZone(string zoneId)
        {
            if (zoneId == null || zoneId == "")
            {
                return;
            }

            SubterraneanZoneCoord current;

            try
            {
                current = SubterraneanZoneCoord.Parse(zoneId);
            }
            catch
            {
                return;
            }

            if (current.World != "JoppaWorld")
            {
                return;
            }

            SubterraneanMatrixCoord matrix = GetMatrixForZone(current);

            List<SubterraneanMatrixCoord> matrices =
                GetCurrentAndNeighborMatrices(matrix);

            foreach (SubterraneanMatrixCoord candidateMatrix in matrices)
            {
                if (!IsValidMatrix(candidateMatrix))
                {
                    continue;
                }
                ProcessMatrix(candidateMatrix, current);
            }
        }

        private List<SubterraneanMatrixCoord> GetCurrentAndNeighborMatrices(
            SubterraneanMatrixCoord matrix
        )
        {
            List<SubterraneanMatrixCoord> matrices =
                new List<SubterraneanMatrixCoord>();

            HashSet<string> seen = new HashSet<string>();

            for (int dz = -1; dz <= 1; dz++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        SubterraneanMatrixCoord candidate =
                            GetOffsetMatrix(matrix, dx, dy, dz);

                        if (!IsValidMatrix(candidate))
                        {
                            continue;
                        }

                        string key = candidate.ToId();

                        if (seen.Contains(key))
                        {
                            continue;
                        }

                        seen.Add(key);
                        matrices.Add(candidate);
                    }
                }
            }

            return matrices;
        }

        private SubterraneanMatrixCoord GetOffsetMatrix(
            SubterraneanMatrixCoord matrix,
            int dx,
            int dy,
            int dz
        )
        {
            SubterraneanMatrixCoord offset = matrix;

            offset.X += dx;
            offset.Y += dy;
            offset.Z += dz;

            return offset;
        }

        // Assumes MatrixParasangWidth and MatrixParasangHeight evenly divide the 80x25 Joppa world map.
        // If matrix dimensions change, update this to use ceiling division or explicit edge handling.
        private bool IsValidMatrix(SubterraneanMatrixCoord matrix)
        {
            if (matrix.World != "JoppaWorld")
            {
                return false;
            }

            if (matrix.X < 0 || matrix.Y < 0 || matrix.Z < 0)
            {
                return false;
            }

            int matrixCountX = 80 / MatrixParasangWidth;
            int matrixCountY = 25 / MatrixParasangHeight;

            if (matrix.X >= matrixCountX)
            {
                return false;
            }

            if (matrix.Y >= matrixCountY)
            {
                return false;
            }

            return true;
        }

        private void ProcessMatrix(
            SubterraneanMatrixCoord matrix,
            SubterraneanZoneCoord current
        )
        {
            if (!EnsureSafetyReady())
            {
                return;
            }

            if (IsMatrixProcessed(matrix))
            {
                return;
            }

            bool anyGenerated = false;

            List<string> slotReports = new List<string>();

            MatrixSiteSlot[] slots =
            {
                MatrixSiteSlot.A,
                MatrixSiteSlot.B,
                MatrixSiteSlot.C,
                MatrixSiteSlot.D
            };

            foreach (MatrixSiteSlot slot in slots)
            {
                string slotReport;

                bool generated = TryProcessMatrixSlot(
                    matrix,
                    current,
                    slot,
                    out slotReport
                );

                if (generated)
                {
                    anyGenerated = true;
                }

                if (slotReport != null && slotReport != "")
                {
                    slotReports.Add(slotReport);
                }
            }

            if (anyGenerated)
            {
                SetMatrixStatus(matrix, "Generated");
            }
            else
            {
                SetMatrixStatus(matrix, "NoSitesGenerated");
            }

            if (DebugShowMatrixGenerationPopup)
            {
                StringBuilder text = new StringBuilder();

                text.AppendLine("SubterraneanSites matrix quads");
                text.AppendLine("matrix=" + matrix.ToId());
                text.AppendLine("status=" + GetMatrixStatus(matrix));

                foreach (string slotReport in slotReports)
                {
                    text.AppendLine("");
                    text.Append(slotReport);
                }

                Popup.Show(text.ToString());
            }
        }

        private bool TryProcessMatrixSlot(
            SubterraneanMatrixCoord matrix,
            SubterraneanZoneCoord current,
            MatrixSiteSlot slot,
            out string report
        )
        {
            StringBuilder slotReport = new StringBuilder();

            string slotName = GetSlotName(slot);

            slotReport.AppendLine("slot=" + slotName);

            report = "";

            string matrixId = matrix.ToId();

            int rawSeed = XRLCore.Core.Game.GetWorldSeed();

            int slotSeed = XRLCore.Core.Game.GetWorldSeed(
                "SubterraneanSites:Matrix:" +
                matrixId +
                ":Slot:" +
                slotName +
                ":" +
                rawSeed.ToString()
            );

            System.Random rng = new System.Random(slotSeed);

            if (rng.Next(1, 101) > MatrixSiteChancePercent)
            {
                slotReport.AppendLine("status=NoSite");
                report = slotReport.ToString();
                return false;
            }

            int minGX;
            int maxGX;
            int minGY;
            int maxGY;

            if (!GetOriginBoundsForSlot(
                matrix,
                slot,
                out minGX,
                out maxGX,
                out minGY,
                out maxGY
            ))
            {
                slotReport.AppendLine("status=NoBounds");
                report = slotReport.ToString();
                return false;
            }

            int layers = rng.Next(3, 7);

            string originZoneId = PickSiteOriginZoneId(
                matrix,
                current,
                layers,
                rng,
                minGX,
                maxGX,
                minGY,
                maxGY
            );

            if (originZoneId == null || originZoneId == "")
            {
                slotReport.AppendLine("status=FailedNoOrigin");
                slotReport.AppendLine("layers=" + layers.ToString());
                report = slotReport.ToString();
                return false;
            }

            List<string> siteZoneIds = BuildSiteZoneIds(originZoneId, layers);

            List<string> rejectedSiteZones = new List<string>();

            string blockedSiteZoneId;
            string blockedSiteReason;

            if (AnyBlockedForSubterraneanGeneration(
                siteZoneIds,
                out blockedSiteZoneId,
                out blockedSiteReason
            ))
            {
                rejectedSiteZones.Add(blockedSiteZoneId + " : " + blockedSiteReason);

                slotReport.AppendLine("status=SkippedProtected");
                slotReport.AppendLine("origin=" + originZoneId);
                slotReport.AppendLine("layers=" + layers.ToString());
                AppendRejectedList(slotReport, "rejectedSiteZones", rejectedSiteZones);

                report = slotReport.ToString();
                return false;
            }

            SiteKind siteKind = RollSiteKind(rng);

            bool siteRegistered = RegisterSelectedSite(siteZoneIds, siteKind, rng);

            if (!siteRegistered)
            {
                slotReport.AppendLine("status=FailedSiteRegistration");
                slotReport.AppendLine("siteKind=" + siteKind.ToString());
                slotReport.AppendLine("origin=" + originZoneId);
                slotReport.AppendLine("layers=" + layers.ToString());
                AppendRejectedList(slotReport, "rejectedSiteZones", rejectedSiteZones);

                report = slotReport.ToString();
                return false;
            }

            int originZ = GetZFromZoneId(originZoneId);
            int pathUpWeight = GetPathUpWeightForOriginZ(originZ);

            SubterraneanPathCoordinateGenerator pathGenerator =
                new SubterraneanPathCoordinateGenerator(pathUpWeight);

            int steps = rng.Next(MinPathSteps, MaxPathStepsExclusive);

            List<string> rejectedPathCandidates = new List<string>();

            List<string> pathZoneIds = pathGenerator.BuildPathZoneIds(
                siteZoneIds[0],
                steps,
                rng,
                delegate(string candidateZoneId)
                {
                    string safetyReason;

                    bool blocked = IsBlockedForSubterraneanGeneration(
                        candidateZoneId,
                        out safetyReason,
                        originZoneId
                    );

                    if (blocked)
                    {
                        rejectedPathCandidates.Add(
                            candidateZoneId + " : " + safetyReason
                        );

                        return false;
                    }

                    return true;
                }
            );

            List<SubterraneanPathCoordinateGenerator.PathZoneInstruction> pathInstructions =
                pathGenerator.BuildPathInstructions(pathZoneIds);

            List<string> rejectedPathZones = new List<string>();

            pathInstructions = RemoveProtectedPathInstructionsWithReport(
                pathInstructions,
                rejectedPathZones,
                originZoneId
            );

            string pathMaterial = PickPathMaterial(rng);

            RegisterHorizontalRoadPath(
                pathInstructions,
                pathMaterial,
                originZoneId
            );

            string pathMouth = "";

            if (pathInstructions != null && pathInstructions.Count > 0)
            {
                pathMouth = pathInstructions[pathInstructions.Count - 1].ZoneId;
            }

            slotReport.AppendLine("status=Generated");
            slotReport.AppendLine("siteKind=" + siteKind.ToString());
            slotReport.AppendLine("origin=" + originZoneId);
            slotReport.AppendLine("layers=" + layers.ToString());
            slotReport.AppendLine("pathUpWeight=" + pathUpWeight.ToString());
            slotReport.AppendLine("pathStepsRequested=" + steps.ToString());
            slotReport.AppendLine("pathZoneIds=" + pathZoneIds.Count.ToString());
            slotReport.AppendLine("pathInstructions=" + pathInstructions.Count.ToString());

            if (pathMouth != null && pathMouth != "")
            {
                slotReport.AppendLine("pathMouth=" + pathMouth);
            }

            AppendRejectedList(slotReport, "rejectedSiteZones", rejectedSiteZones);
            AppendRejectedList(slotReport, "rejectedPathZones", rejectedPathZones);
            AppendRejectedList(slotReport, "rejectedPathCandidates", rejectedPathCandidates);

            report = slotReport.ToString();
            return true;
        }

        private string PickSiteOriginZoneId(
            SubterraneanMatrixCoord matrix,
            SubterraneanZoneCoord current,
            int layers,
            System.Random rng,
            int minGlobalZoneX,
            int maxGlobalZoneX,
            int minGlobalZoneY,
            int maxGlobalZoneY
        )
        {

            int minZ = SurfaceZ + matrix.Z * MatrixDepth;
            int maxZ = minZ + MatrixDepth - 1;

            if (matrix.Z == 0 && minZ < MinSurfaceMatrixOriginZ)
            {
                minZ = MinSurfaceMatrixOriginZ;
            }

            if (maxZ < minZ)
            {
                return "";
            }

            //1. Pick a possible origin.
            //2. Build the full proposed site stack from that origin.
            //Example: origin Z 11, 4 layers → Z 11, 12, 13, 14.
            //3. Check every layer in that proposed site stack.
            //4. If any layer is blocked, reject this origin attempt.
            //5. Try another random origin.
            //6. If no layer is blocked, accept this origin and return it.
            for (int attempt = 0; attempt < 30; attempt++)
            {
                int globalZoneX = rng.Next(minGlobalZoneX, maxGlobalZoneX + 1);
                int globalZoneY = rng.Next(minGlobalZoneY, maxGlobalZoneY + 1);

                int px = globalZoneX / 3;
                int zoneX = globalZoneX % 3;

                int py = globalZoneY / 3;
                int zoneY = globalZoneY % 3;

                int z = rng.Next(minZ, maxZ + 1);

                SubterraneanZoneCoord origin = new SubterraneanZoneCoord(
                    matrix.World,
                    px,
                    py,
                    zoneX,
                    zoneY,
                    z
                );

                if (IsSameZone(origin, current))
                {
                    continue;
                }

                List<string> siteZoneIds = BuildSiteZoneIds(origin.ToZoneId(), layers);

                string reason;
                bool protectedHit = false;

                foreach (string siteZoneId in siteZoneIds)
                {
                    if (IsBlockedForSubterraneanGeneration(siteZoneId, out reason))
                    {
                        protectedHit = true;
                        break;
                    }
                }

                if (protectedHit)
                {
                    continue;
                }

                return origin.ToZoneId();
            }

            return "";
        }


        private bool IsOwnedBySubterraneanSites(string zoneId)
        {
            string owner =
                The.ZoneManager.GetZoneProperty(zoneId, OwnerProperty) as string;

            return owner == "Yes";
        }

        private bool IsBlockedForSubterraneanGeneration(
            string zoneId,
            out string reason,
            string allowedOwnedZoneId = ""
        )
        {
            if (IsOwnedBySubterraneanSites(zoneId) && zoneId != allowedOwnedZoneId)
            {
                reason = "already claimed by Subterranean Sites";
                return true;
            }

            if (SubterraneanSafety.IsProtected(zoneId, out reason))
            {
                return true;
            }

            reason = "";
            return false;
        }

        private bool AnyBlockedForSubterraneanGeneration(
            List<string> zoneIds,
            out string blockedZoneId,
            out string reason
        )
        {
            blockedZoneId = "";
            reason = "";

            if (zoneIds == null || zoneIds.Count == 0)
            {
                reason = "empty site zone list";
                return true;
            }

            foreach (string zoneId in zoneIds)
            {
                if (IsBlockedForSubterraneanGeneration(zoneId, out reason))
                {
                    blockedZoneId = zoneId;
                    return true;
                }
            }

            return false;
        }

        private bool IsSameZone(SubterraneanZoneCoord a, SubterraneanZoneCoord b)
        {
            return
                a.World == b.World &&
                a.ParasangX == b.ParasangX &&
                a.ParasangY == b.ParasangY &&
                a.ZoneX == b.ZoneX &&
                a.ZoneY == b.ZoneY &&
                a.Z == b.Z;
        }

        //Sets stacked site lower layer ids
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

        private bool RegisterSelectedSite(
            List<string> siteZoneIds,
            SiteKind siteKind,
            System.Random rng
        )
        {
            switch (siteKind)
            {
            case SiteKind.SultanHistoric:
                return new SultanHistoricSiteRegistrar(this).Register(siteZoneIds);

            case SiteKind.BasicLairChaos:
                return new BasicLairChaosSiteRegistrar(this).Register(siteZoneIds);

            case SiteKind.ProperLair:
                return new ProperLairSiteRegistrar(this).Register(siteZoneIds);

            case SiteKind.MerchantHive:
                return new MerchantHiveSiteRegistrar(this).Register(siteZoneIds);

            default:
                return new SultanHistoricSiteRegistrar(this).Register(siteZoneIds);
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

        internal bool EnsureSafetyReady()
        {
            if (safetyReadyThisSession &&
                SubterraneanDynamicProtectedLocations.HasRuntimeProtectionData())
            {
                return true;
            }

            bool ok =
                SubterraneanDynamicProtectedLocations
                    .InitializeRuntimeProtection();

            if (!ok)
            {
                safetyReadyThisSession = false;
                MaybeReportSafetyFailure();
                return false;
            }

            safetyReadyThisSession = true;
            return true;
        }

        private void MaybeReportSafetyFailure()
        {
            if (The.Game == null)
            {
                return;
            }

            if (The.Game.GetStringGameState(SafetyFailureReportedFlag) == "Yes")
            {
                return;
            }

            The.Game.SetStringGameState(SafetyFailureReportedFlag, "Yes");

            Popup.Show(
                "Subterranean Sites failed to initialize its protected-location safety system.\n\n" +
                "To avoid overwriting or damaging vanilla generated locations, the mod has disabled subterranean site generation for this save.\n\n" +
                "Existing game content is unchanged."
            );
        }

        private string BuildMatrixDebugMessage(
            SubterraneanMatrixCoord matrix,
            string status,
            string originZoneId,
            string siteKind,
            string pathMouth,
            int layers,
            List<string> rejectedSiteZones,
            List<string> rejectedPathZones
        )
        {
            StringBuilder text = new StringBuilder();

            text.AppendLine("SubterraneanSites matrix");
            text.AppendLine("matrix=" + matrix.ToId());
            text.AppendLine("status=" + status);

            if (siteKind != null && siteKind != "")
            {
                text.AppendLine("siteKind=" + siteKind);
            }

            if (originZoneId != null && originZoneId != "")
            {
                text.AppendLine("origin=" + originZoneId);
            }

            text.AppendLine("layers=" + layers.ToString());

            if (pathMouth != null && pathMouth != "")
            {
                text.AppendLine("pathMouth=" + pathMouth);
            }

            AppendRejectedList(text, "rejectedSiteZones", rejectedSiteZones);
            AppendRejectedList(text, "rejectedPathZones", rejectedPathZones);

            return text.ToString();
        }

        private void AppendRejectedList(
            StringBuilder text,
            string label,
            List<string> rejected
        )
        {
            if (rejected == null || rejected.Count == 0)
            {
                text.AppendLine(label + "=none");
                return;
            }

            text.AppendLine(label + "=" + rejected.Count.ToString());

            int max = Math.Min(rejected.Count, 6);

            for (int i = 0; i < max; i++)
            {
                text.AppendLine("  " + rejected[i]);
            }

            if (rejected.Count > max)
            {
                text.AppendLine("  ...");
            }
        }

        //Paths now try to redirect
        private List<SubterraneanPathCoordinateGenerator.PathZoneInstruction> RemoveProtectedPathInstructionsWithReport(
            List<SubterraneanPathCoordinateGenerator.PathZoneInstruction> pathInstructions,
            List<string> rejected,
            string allowedOwnedZoneId
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

                if (IsBlockedForSubterraneanGeneration(instruction.ZoneId,out safetyReason,allowedOwnedZoneId))
                {
                    if (rejected != null)
                    {
                        rejected.Add(instruction.ZoneId + " : " + safetyReason);
                    }

                    continue;
                }

                safeInstructions.Add(instruction);
            }

            return safeInstructions;
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

        private int GetPathUpWeightForOriginZ(int originZ)
        {
            if (originZ <= 15)
            {
                return 2;
            }

            return 9;
        }

      internal int GetTierForZoneId(string zoneId)
        {
            SubterraneanZoneCoord coord;

            try
            {
                coord = SubterraneanZoneCoord.Parse(zoneId);
            }
            catch
            {
                return 1;
            }

            int depthTier = GetTierFromZ(coord.Z);
            int surfaceTier = GetSurfaceRegionTierFloor(coord);

            int tier = Math.Max(depthTier, surfaceTier);

            if (tier < 1)
            {
                tier = 1;
            }

            if (tier > 8)
            {
                tier = 8;
            }

            return tier;
        }
        private int GetSurfaceRegionTierFloor(SubterraneanZoneCoord coord)
        {
            XRL.World.GameObject terrainObject = null;

            try
            {
                terrainObject = ZoneManager.GetTerrainObjectForZone(
                    coord.ParasangX,
                    coord.ParasangY,
                    coord.World
                );
            }
            catch
            {
                return 1;
            }

            if (terrainObject == null)
            {
                return 1;
            }

            int tier;

            if (!int.TryParse(terrainObject.GetTag("RegionTier", "1"), out tier))
            {
                tier = 1;
            }

            if (tier < 1)
            {
                tier = 1;
            }

            if (tier > 8)
            {
                tier = 8;
            }

            return tier;
        }
        internal bool RegisterLayeredSite(
            List<string> siteZoneIds,
            string siteDisplayName,
            string discoveryKey,
            Action<SiteLayerContext> registerLayer
        )
        {
            if (siteZoneIds == null || siteZoneIds.Count == 0)
            {
                return false;
            }

            string blockedZoneId;
            string blockedReason;

            if (AnyBlockedForSubterraneanGeneration(
                siteZoneIds,
                out blockedZoneId,
                out blockedReason
            ))
            {
                The.ZoneManager.SetZoneName(
                    blockedZoneId,
                    "site out: " + blockedReason + " at " + blockedZoneId,
                    Proper: false
                );

                return false;
            }

            for (int i = 0; i < siteZoneIds.Count; i++)
            {
                string zoneId = siteZoneIds[i];

                string safetyReason;

                if (IsBlockedForSubterraneanGeneration(zoneId, out safetyReason))
                {
                    The.ZoneManager.SetZoneName(
                        zoneId,
                        "site out: " + safetyReason + " at " + zoneId,
                        Proper: false
                    );

                    return false;
                }

                string stairs = GetStairsForLayer(i, siteZoneIds.Count);
                int z = GetZFromZoneId(zoneId);
                int tier = GetTierForZoneId(zoneId);

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

                The.ZoneManager.SetZoneProperty(zoneId, "SubterraneanSites_IsSiteLayer", "Yes");
                The.ZoneManager.SetZoneProperty(zoneId, "SubterraneanSites_SiteDisplayName", siteDisplayName);
                The.ZoneManager.SetZoneProperty(zoneId, "SubterraneanSites_DiscoveryKey", discoveryKey);

                if (context.IsOrigin)
                {
                    The.ZoneManager.SetZoneProperty(zoneId, "SubterraneanSites_IsSiteOrigin", "Yes");
                }

                The.ZoneManager.SetZoneName(
                    zoneId,
                    siteDisplayName,
                    Proper: true
                );
            }

            return true;
        }
        private void RegisterHorizontalRoadPath(
            List<SubterraneanPathCoordinateGenerator.PathZoneInstruction> pathInstructions,
            string pathMaterial,
            string pathSeedBase
        )
        {
            if (pathInstructions == null || pathInstructions.Count == 0)
            {
                return;
            }

            foreach (SubterraneanPathCoordinateGenerator.PathZoneInstruction instruction in pathInstructions)
            {
                RegisterRoadPathZone(instruction, pathMaterial, pathSeedBase);
            }
        }

        private void RegisterRoadPathZone(
            SubterraneanPathCoordinateGenerator.PathZoneInstruction instruction,
            string pathMaterial,
            string pathSeedBase
        )
        {
            string safetyReason;

            if (IsBlockedForSubterraneanGeneration(instruction.ZoneId,out safetyReason,pathSeedBase))
            {
                The.ZoneManager.SetZoneName(
                    instruction.ZoneId,
                    "SubterraneanSites refused path builder: " + safetyReason,
                    Proper: false
                );

                return;
            }

            int entryHoleX = GetEntryHoleXForInstruction(instruction, pathSeedBase);
            int entryHoleY = GetEntryHoleYForInstruction(instruction, pathSeedBase);
            int exitHoleX = GetExitHoleXForInstruction(instruction, pathSeedBase);
            int exitHoleY = GetExitHoleYForInstruction(instruction, pathSeedBase);

            string entryHole = entryHoleX.ToString() + "," + entryHoleY.ToString();
            string exitHole = exitHoleX.ToString() + "," + exitHoleY.ToString();

            The.ZoneManager.SetZoneProperty(instruction.ZoneId, OwnerProperty, "Yes");
            The.ZoneManager.SetZoneProperty(instruction.ZoneId, "SubterraneanSites_IsPath", "Yes");

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
                //"FungalTrailBrick",
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
            SubterraneanPathCoordinateGenerator.PathZoneInstruction instruction,
            string pathSeedBase
        )
        {
            if (instruction.Entry == "Down")
            {
                return GetHoleXForVerticalTransition(instruction.Index - 1, pathSeedBase);
            }

            return GetHoleXForVerticalTransition(instruction.Index, pathSeedBase);
        }

        private int GetEntryHoleYForInstruction(
            SubterraneanPathCoordinateGenerator.PathZoneInstruction instruction,
            string pathSeedBase
        )
        {
            if (instruction.Entry == "Down")
            {
                return GetHoleYForVerticalTransition(instruction.Index - 1, pathSeedBase);
            }

            return GetHoleYForVerticalTransition(instruction.Index, pathSeedBase);
        }

        private int GetExitHoleXForInstruction(
            SubterraneanPathCoordinateGenerator.PathZoneInstruction instruction,
            string pathSeedBase
        )
        {
            return GetHoleXForVerticalTransition(instruction.Index, pathSeedBase);
        }

        private int GetExitHoleYForInstruction(
            SubterraneanPathCoordinateGenerator.PathZoneInstruction instruction,
            string pathSeedBase
        )
        {
            return GetHoleYForVerticalTransition(instruction.Index, pathSeedBase);
        }

        private int GetHoleXForVerticalTransition(int verticalIndex, string pathSeedBase)
        {
            if (pathSeedBase == null || pathSeedBase == "")
            {
                pathSeedBase = "SubterraneanSites:Path";
            }

            int seed = XRLCore.Core.Game.GetWorldSeed(
                pathSeedBase + ":HoleX:" + verticalIndex.ToString()
            );

            System.Random rng = new System.Random(seed);

            return 40 + rng.Next(-16, 17);
        }

        private int GetHoleYForVerticalTransition(int verticalIndex, string pathSeedBase)
        {
            if (pathSeedBase == null || pathSeedBase == "")
            {
                pathSeedBase = "SubterraneanSites:Path";
            }

            int seed = XRLCore.Core.Game.GetWorldSeed(
                pathSeedBase + ":HoleY:" + verticalIndex.ToString()
            );

            System.Random rng = new System.Random(seed);

            return 12 + rng.Next(-8, 9);
        }
    }

    internal delegate bool PathCandidateValidator(string zoneId);
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
        private int upWeight = 2;

        public SubterraneanPathCoordinateGenerator()
        {
            upWeight = 2;
        }

        public SubterraneanPathCoordinateGenerator(int upWeight)
        {
            this.upWeight = upWeight;
        }

        public List<string> BuildPathZoneIds(
            string originZoneId,
            int steps,
            System.Random rng
        )
        {
            return BuildPathZoneIds(originZoneId, steps, rng, null);
        }

        public List<string> BuildPathZoneIds(
            string originZoneId,
            int steps,
            System.Random rng,
            PathCandidateValidator isCandidateAllowed
        )
        {
            List<string> pathZoneIds = new List<string>();
            HashSet<string> usedZoneIds = new HashSet<string>();

            if (steps <= 0)
            {
                return pathZoneIds;
            }

            SubterraneanZoneCoord current = SubterraneanZoneCoord.Parse(originZoneId);

            PathHeading heading = PickPathHeading(rng);
            PathDirection? previousDirection = null;

            pathZoneIds.Add(current.ToZoneId());
            usedZoneIds.Add(current.ToZoneId());

            for (int i = 0; i < steps; i++)
            {
                List<PathDirection> candidateDirections =
                    BuildCandidateDirections(rng, previousDirection, heading);

                bool moved = false;

                foreach (PathDirection direction in candidateDirections)
                {
                    SubterraneanZoneCoord candidate = Step(current, direction);
                    string candidateZoneId = candidate.ToZoneId();

                    if (usedZoneIds.Contains(candidateZoneId))
                    {
                        continue;
                    }

                    if (isCandidateAllowed != null &&
                        !isCandidateAllowed(candidateZoneId))
                    {
                        continue;
                    }

                    current = candidate;
                    pathZoneIds.Add(candidateZoneId);
                    usedZoneIds.Add(candidateZoneId);

                    previousDirection = direction;
                    moved = true;
                    break;
                }

                if (!moved)
                {
                    break;
                }

                if (current.Z <= 10)
                {
                    break;
                }
            }

            return pathZoneIds;
        }

        private List<PathDirection> BuildCandidateDirections(
            System.Random rng,
            PathDirection? previousDirection,
            PathHeading heading
        )
        {
            List<PathDirection> directions = new List<PathDirection>();

            PathDirection preferred =
                PickNextDirection(rng, previousDirection, heading);

            directions.Add(preferred);

            AddDirectionIfMissing(directions, PathDirection.Up);
            AddDirectionIfMissing(directions, PathDirection.North);
            AddDirectionIfMissing(directions, PathDirection.South);
            AddDirectionIfMissing(directions, PathDirection.East);
            AddDirectionIfMissing(directions, PathDirection.West);

            // Keep the preferred direction first, but shuffle the fallback directions.
            for (int i = directions.Count - 1; i > 1; i--)
            {
                int j = rng.Next(1, i + 1);

                PathDirection temp = directions[i];
                directions[i] = directions[j];
                directions[j] = temp;
            }

            return directions;
        }

        private void AddDirectionIfMissing(
            List<PathDirection> directions,
            PathDirection direction
        )
        {
            if (!directions.Contains(direction))
            {
                directions.Add(direction);
            }
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
                    directions.Add(new WeightedDirection(PathDirection.Up, upWeight));
                    directions.Add(new WeightedDirection(PathDirection.East, SideWeight));
                    directions.Add(new WeightedDirection(PathDirection.West, SideWeight));
                    break;

                case PathHeading.South:
                    directions.Add(new WeightedDirection(PathDirection.South, MainWeight));
                    directions.Add(new WeightedDirection(PathDirection.Up, upWeight));
                    directions.Add(new WeightedDirection(PathDirection.East, SideWeight));
                    directions.Add(new WeightedDirection(PathDirection.West, SideWeight));
                    break;

                case PathHeading.East:
                    directions.Add(new WeightedDirection(PathDirection.East, MainWeight));
                    directions.Add(new WeightedDirection(PathDirection.Up, upWeight));
                    directions.Add(new WeightedDirection(PathDirection.North, SideWeight));
                    directions.Add(new WeightedDirection(PathDirection.South, SideWeight));
                    break;

                case PathHeading.West:
                    directions.Add(new WeightedDirection(PathDirection.West, MainWeight));
                    directions.Add(new WeightedDirection(PathDirection.Up, upWeight));
                    directions.Add(new WeightedDirection(PathDirection.North, SideWeight));
                    directions.Add(new WeightedDirection(PathDirection.South, SideWeight));
                    break;

                case PathHeading.NorthEast:
                    directions.Add(new WeightedDirection(PathDirection.North, DiagonalComponentWeight));
                    directions.Add(new WeightedDirection(PathDirection.East, DiagonalComponentWeight));
                    directions.Add(new WeightedDirection(PathDirection.Up, upWeight));
                    break;

                case PathHeading.NorthWest:
                    directions.Add(new WeightedDirection(PathDirection.North, DiagonalComponentWeight));
                    directions.Add(new WeightedDirection(PathDirection.West, DiagonalComponentWeight));
                    directions.Add(new WeightedDirection(PathDirection.Up, upWeight));
                    break;

                case PathHeading.SouthEast:
                    directions.Add(new WeightedDirection(PathDirection.South, DiagonalComponentWeight));
                    directions.Add(new WeightedDirection(PathDirection.East, DiagonalComponentWeight));
                    directions.Add(new WeightedDirection(PathDirection.Up, upWeight));
                    break;
                case PathHeading.SouthWest:
                    directions.Add(new WeightedDirection(PathDirection.South, DiagonalComponentWeight));
                    directions.Add(new WeightedDirection(PathDirection.West, DiagonalComponentWeight));
                    directions.Add(new WeightedDirection(PathDirection.Up, upWeight));
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