using System;
using System.Collections.Generic;
using Genkit;
using XRL;
using XRL.Core;
using XRL.Rules;
using XRL.World;
using XRL.World.Effects;
using XRL.World.Parts;


namespace SubterraneanSites
{

    internal sealed class SubterraneanSitesEPOozeTheme :
        ISubterraneanSitesEPCategoryProvider,
        ISubterraneanSitesEPPrimaryObjectProvider,
        ISubterraneanSitesEPAttunementProvider,
        ISubterraneanSitesEPDenizenAdaptationProvider,
        ISubterraneanSitesEPSignatureMutationProvider
    {
        public string ThemeKey
        {
            get { return "Ooze"; }
        }

        public string SignatureMutationClass
        {
            get { return "AcidSlimeGlands"; }
        }

        public SubterraneanSitesEPAttunementBuildResult
            TryCreateAttunement(
                GameObject actor,
                Zone zone,
                out XRL.World.Effects
                    .SubterraneanSitesEPAttunementEffect effect,
                out string successMessage
            )
        {
            effect = null;
            successMessage = "";

            if (
                actor == null ||
                zone == null
            )
            {
                return
                    SubterraneanSitesEPAttunementBuildResult
                        .Unavailable;
            }

            int mutationLevel =
                SubterraneanSitesEPAttunementSystem
                    .GetAttunementMutationLevel(
                        zone
                    );

            effect =
                new XRL.World.Effects
                    .SubterraneanSitesEPOozeAttunementEffect(
                        SubterraneanSitesEPAttunementSystem
                            .DefaultDuration,
                        mutationLevel
                    );

            successMessage =
                "Attunement grants:\n" +
                "Acid Slime Glands (level " +
                mutationLevel.ToString() +
                ")\n" +
                "+10 Toughness saves vs. disease\n" +
                "Ambient illness is suppressed.";

            return
                SubterraneanSitesEPAttunementBuildResult
                    .Success;
        }

        public void ApplyDenizenAdaptation(
            GameObject creature
        )
        {
            if (creature == null)
                return;


            SaveModifier saveModifier =
                new SaveModifier();


            saveModifier.Vs =
                "Disease";

            saveModifier.Amount =
                10;

            saveModifier.WorksOnEquipper =
                false;

            saveModifier.WorksOnSelf =
                true;


            creature.AddPart<
                SaveModifier
            >(
                saveModifier
            );
        }

        public int MinimumHoleSeparation
        {
            get { return 25; }
        }

                internal static SubterraneanSitesEPFloorSpec
            CreateFloorSpec()
        {
            return
                new SubterraneanSitesEPFloorSpec
                {
                    PaintCell =
                        PaintFloorCell
                };
        }


        private static void PaintFloorCell(
            Cell cell
        )
        {
            if (
                cell == null ||
                cell.ParentZone == null
            )
            {
                return;
            }

            //
            // Toxic waste-ground.
            //
            // CrystalGrassy uses this same vanilla simplex-noise helper
            // to create coherent terrain regions rather than independent
            // random cells. Use a zone-specific seed so each EP layer gets
            // its own stable contamination pattern.
            //
            double patch =
                global::XRL.World.Parts
                    .CrystalGrassy
                    .sampleSimplexNoise(
                        "SubterraneanSites:OozeFloor:" +
                        cell.ParentZone.ZoneID,
                        cell.X,
                        cell.Y,
                        cell.ParentZone.Z,
                        5,
                        0.75f
                    );

            //
            // Use the vanilla desert-ground family for dirty, irregular
            // graphical variation. Color is supplied independently below.
            //
            int tileVariant =
                Math.Abs(
                    cell.X * 5 +
                    cell.Y * 7
                ) % 12 + 1;

            cell.PaintTile =
                "Terrain/sw_ground_desert_" +
                tileVariant.ToString() +
                ".bmp";

            //
            // Leave a substantial portion of the toxic substrate nearly black.
            // This reduces the visible dot density without changing the larger
            // contamination-patch color pattern.
            //
            if (
                Stat.Random(
                    1,
                    100
                ) <= 45
            )
            {
                cell.PaintTileColor =
                    "&k";

                cell.PaintColorString =
                    "&k^k";

                cell.PaintDetailColor =
                    "k";

                return;
            }

            string tileColor;
            string detailColor;

            //
            // Large coherent contamination patches.
            //
            if (patch <= -2.0)
            {
                //
                // Muddy/brown waste.
                //
                tileColor =
                    "&w";

                detailColor =
                    "g";
            }
            else if (patch <= 0.0)
            {
                //
                // Dark toxic green.
                //
                tileColor =
                    "&g";

                detailColor =
                    "y";
            }
            else if (patch <= 2.0)
            {
                //
                // Sickly yellow.
                //
                tileColor =
                    "&y";

                detailColor =
                    "g";
            }
            else
            {
                //
                // Rusty/orange chemical waste.
                //
                tileColor =
                    "&o";

                detailColor =
                    "y";
            }

            cell.PaintTileColor =
                tileColor;

            cell.PaintColorString =
                tileColor + "^k";

            cell.PaintDetailColor =
                detailColor;
        }

        public void RegisterCategory1(
            SubterraneanSitesEPLayerContext context
        )
        {
            if (
                context == null ||
                The.Game == null
            )
            {
                return;
            }

            //
            // Ooze C1 is runtime environmental pressure rather than a static
            // zone-build transformation. This intentionally includes the origin.
            //
            The.Game.RequireSystem<
                SubterraneanSitesEPOozeEnvironmentSystem
            >();
        }

        public void RegisterCategory1Objects(
            SubterraneanSitesEPLayerContext context
        )
        {
            if (context == null)
                return;

            The.ZoneManager.AddZonePostBuilder(
                context.ZoneId,
                "SubterraneanSitesOozePrimaryPools"
            );
        }


        public void RegisterEntranceCategory1Objects(
            SubterraneanSitesEPEntranceContext context
        )
        {
            if (context == null)
                return;

            The.ZoneManager.AddZonePostBuilder(
                context.ZoneId,
                "SubterraneanSitesOozePrimaryPools",
                "EntranceOnly", "1"
            );
        }

        public void RegisterCategory2(
            SubterraneanSitesEPLayerContext context
        )
        {
            if (context == null)
                return;

            The.ZoneManager.AddZonePostBuilder(
                context.ZoneId,
                "SubterraneanSitesOozeAcidTraps"
            );
        }

        public void RegisterCategory3(
            SubterraneanSitesEPLayerContext context
        )
        {
            if (context == null)
                return;

            The.ZoneManager.AddZonePostBuilder(
                context.ZoneId,
                "SubterraneanSitesOozeMaterials"
            );
        }

        public void RegisterCategory4Layout(
            SubterraneanSitesEPLayerContext context
        )
        {
            if (context == null)
                return;

            The.ZoneManager.AddZonePostBuilder(
                context.ZoneId,
                "SubterraneanSitesOozeLayout"
            );
        }

        public void RegisterCategory4Floor(
            SubterraneanSitesEPLayerContext context
        )
        {
            if (context == null)
                return;

            The.ZoneManager.AddZonePostBuilder(
                context.ZoneId,
                "SubterraneanSitesOozeFloor"
            );
        }

        public void RegisterCategory5(
            SubterraneanSitesEPLayerContext context
        )
        {
            if (context == null)
                return;

            The.ZoneManager.AddZonePostBuilder(
                context.ZoneId,
                "SubterraneanSitesOozeDecorations"
            );
        }

        public void RegisterEntranceFloor(
            SubterraneanSitesEPEntranceContext context
        )
        {
            if (context == null)
                return;

            The.ZoneManager.AddZonePostBuilder(
                context.ZoneId,
                "SubterraneanSitesOozeFloor",
                "EntranceOnly", "1"
            );
        }

        public void RegisterEntranceDecorations(
            SubterraneanSitesEPEntranceContext context
        )
        {
            if (context == null)
                return;

            The.ZoneManager.AddZonePostBuilder(
                context.ZoneId,
                "SubterraneanSitesOozeDecorations",
                "EntranceOnly", "1"
            );
        }
    }

}


namespace XRL.World.ZoneBuilders
{

    /// <summary>
    /// Ooze Category-4 floor adapter.
    ///
    /// Ooze owns its contaminated-ground recipe. The shared EP floor
    /// system owns universal underground/scar scope and application.
    /// </summary>
    public class SubterraneanSitesOozeFloor :
        ZoneBuilderSandbox
    {
        public int EntranceOnly = 0;


        public bool BuildZone(
            Zone Z
        )
        {
            return
                SubterraneanSites
                    .SubterraneanSitesEPFloorSystem
                    .Apply(
                        Z,
                        SubterraneanSites
                            .SubterraneanSitesEPOozeTheme
                            .CreateFloorSpec(),
                        EntranceOnly != 0
                    );
        }
    }

    /// <summary>
    /// Ooze Category 3 materialization.
    ///
    /// Buried structural mass is ebon fulcrete, while exposed room-facing
    /// boundaries become ornate CryptWall. The shared geometry classifier
    /// determines which placeholder role each cell has before this builder runs.
    ///
    /// The forced outer shell remains bulk material rather than exposing
    /// CryptWall against the edge of the zone.
    /// </summary>
    public class SubterraneanSitesOozeMaterials :
        ZoneBuilderSandbox
    {
        public string BulkWallBlueprint =
            "EbonFulcrete";

        public string InnerWallBlueprint =
            "CryptWall";

        public int SealedBorderWidth = 1;

        public bool BuildZone(Zone Z)
        {
            if (Z == null)
                return true;

            if (SealedBorderWidth < 0)
                SealedBorderWidth = 0;

            for (int x = 0; x < Z.Width; x++)
            {
                for (int y = 0; y < Z.Height; y++)
                {
                    Cell cell =
                        Z.GetCell(
                            x,
                            y
                        );

                    if (cell == null)
                        continue;

                    bool isCore =
                        SubterraneanSites
                            .SubterraneanSitesEPGeometry
                            .IsSolidPlaceholder(cell);

                    bool isBoundary =
                        SubterraneanSites
                            .SubterraneanSitesEPGeometry
                            .IsBoundaryPlaceholder(cell);

                    if (
                        !isCore &&
                        !isBoundary
                    )
                    {
                        continue;
                    }

                    //
                    // Exposed interior surfaces are CryptWall.
                    // Everything buried behind them, plus the absolute outer
                    // shell, remains ebon fulcrete.
                    //
                    string material =
                        isBoundary &&
                        !IsForcedBorder(
                            Z,
                            x,
                            y
                        )
                            ? InnerWallBlueprint
                            : BulkWallBlueprint;

                    cell.ClearWalls();

                    if (!material.IsNullOrEmpty())
                    {
                        cell.AddObject(
                            material
                        );
                    }
                }
            }

            return true;
        }

        private bool IsForcedBorder(
            Zone Z,
            int x,
            int y
        )
        {
            return
                x < SealedBorderWidth ||
                y < SealedBorderWidth ||
                x >= Z.Width - SealedBorderWidth ||
                y >= Z.Height - SealedBorderWidth;
        }
    }



    /// <summary>
    /// Ooze Category 4.
    ///
    /// Builds a set of overlapping rounded/elliptical chambers and guarantees
    /// a connected passage network between them. Transition anchors receive
    /// dedicated chambers so incoming and outgoing EP transitions always land
    /// in intentional interior space.
    /// </summary>
    public class SubterraneanSitesOozeLayout :
        ZoneBuilderSandbox
    {
        public int MinRooms = 9;
        public int MaxRooms = 12;

        public int MinRadius = 3;
        public int MaxRadius = 5;

        public int AnchorRadius = 4;

        public int MinLargeRooms = 1;
        public int MaxLargeRooms = 1;

        public int MinLargeRadius = 7;
        public int MaxLargeRadius = 9;

        public int Border = 2;

        //
        // Desired open-wall gap between independently generated rooms.
        // Corridors deliberately cut through this separation afterward.
        //
        public int RoomGap = 2;

        public int ExtraConnections = 2;

        private class Room
        {
            public int X;
            public int Y;
            public int RX;
            public int RY;
        }

        public bool BuildZone(Zone Z)
        {
            if (Z == null)
                return true;

            ClampSettings();

            int seed =
                XRLCore.Core.Game.GetWorldSeed(
                    "SubterraneanSites:OozeLayout:" +
                    Z.ZoneID
                );

            System.Random rng =
                new System.Random(seed);

            List<Room> rooms =
                new List<Room>();

            //
            // First establish intentional chambers around all required
            // vertical transition anchors.
            //
            List<Location2D> anchors =
                SubterraneanSites
                    .SubterraneanSitesEPVerticalTransitions
                    .GetVerticalAnchors(
                        Z.ZoneID
                    );

            if (anchors != null)
            {
                foreach (Location2D anchor in anchors)
                {
                    if (anchor == null)
                        continue;

                    Room anchorRoom =
                        new Room
                        {
                            X = anchor.X,
                            Y = anchor.Y,
                            RX = AnchorRadius,
                            RY = AnchorRadius
                        };

                    rooms.Add(anchorRoom);

                    CarveEllipse(
                        Z,
                        anchorRoom
                    );
                }
            }

            int attempts = 0;
            int maxAttempts = 700;

            //
            // Add 1-2 deliberately larger chambers first so the layout has some
            // broader open spaces before the ordinary room pass fills in around them.
            //
            int desiredLargeRooms =
                rng.Next(
                    MinLargeRooms,
                    MaxLargeRooms + 1
                );

            int largePlaced = 0;

            while (
                largePlaced < desiredLargeRooms &&
                attempts < maxAttempts
            )
            {
                attempts++;

                int rx =
                    rng.Next(
                        MinLargeRadius,
                        MaxLargeRadius + 1
                    );

                int ry =
                    rng.Next(
                        MinLargeRadius,
                        MaxLargeRadius + 1
                    );

                int minX =
                    Border + rx;

                int maxX =
                    Z.Width - Border - rx - 1;

                int minY =
                    Border + ry;

                int maxY =
                    Z.Height - Border - ry - 1;

                if (
                    maxX < minX ||
                    maxY < minY
                )
                {
                    break;
                }

                Room candidate =
                    new Room
                    {
                        X = rng.Next(minX, maxX + 1),
                        Y = rng.Next(minY, maxY + 1),
                        RX = rx,
                        RY = ry
                    };

                if (
                    !IsFarEnoughFromExistingCenters(
                        candidate,
                        rooms
                    )
                )
                {
                    continue;
                }

                rooms.Add(candidate);
                CarveEllipse(Z, candidate);
                largePlaced++;
            }

            int desiredRooms =
                rng.Next(
                    MinRooms,
                    MaxRooms + 1
                );

            while (
                rooms.Count < desiredRooms &&
                attempts < maxAttempts
            )
            {
                attempts++;

                int rx =
                    rng.Next(
                        MinRadius,
                        MaxRadius + 1
                    );

                int ry =
                    rng.Next(
                        MinRadius,
                        MaxRadius + 1
                    );

                if (rng.Next(100) < 55)
                {
                    if (rng.Next(2) == 0)
                        rx = Math.Max(MinRadius, rx - 1);
                    else
                        ry = Math.Max(MinRadius, ry - 1);
                }

                int minX =
                    Border + rx;

                int maxX =
                    Z.Width - Border - rx - 1;

                int minY =
                    Border + ry;

                int maxY =
                    Z.Height - Border - ry - 1;

                if (
                    maxX < minX ||
                    maxY < minY
                )
                {
                    break;
                }

                Room candidate =
                    new Room
                    {
                        X = rng.Next(minX, maxX + 1),
                        Y = rng.Next(minY, maxY + 1),
                        RX = rx,
                        RY = ry
                    };

                if (
                    !IsFarEnoughFromExistingCenters(
                        candidate,
                        rooms
                    )
                )
                {
                    continue;
                }

                rooms.Add(candidate);
                CarveEllipse(Z, candidate);
            }


            //
            // Connect every chamber into one guaranteed spanning network.
            // Each room after the first connects to its nearest earlier room.
            //
            for (int i = 1; i < rooms.Count; i++)
            {
                int nearestIndex = 0;
                int bestDistanceSquared =
                    int.MaxValue;

                for (int j = 0; j < i; j++)
                {
                    int dx =
                        rooms[i].X -
                        rooms[j].X;

                    int dy =
                        rooms[i].Y -
                        rooms[j].Y;

                    int distanceSquared =
                        dx * dx +
                        dy * dy;

                    if (
                        distanceSquared <
                        bestDistanceSquared
                    )
                    {
                        bestDistanceSquared =
                            distanceSquared;

                        nearestIndex = j;
                    }
                }

                CarveCorridor(
                    Z,
                    rooms[i],
                    rooms[nearestIndex],
                    rng
                );
            }

            //
            // A few redundant connections keep the map from feeling like a
            // simple branching tree and create alternate movement routes.
            //
            if (rooms.Count >= 3)
            {
                for (
                    int i = 0;
                    i < ExtraConnections;
                    i++
                )
                {
                    Room a =
                        rooms[
                            rng.Next(
                                rooms.Count
                            )
                        ];

                    Room b =
                        rooms[
                            rng.Next(
                                rooms.Count
                            )
                        ];

                    if (a == b)
                    {
                        i--;
                        continue;
                    }

                    CarveCorridor(
                        Z,
                        a,
                        b,
                        rng
                    );
                }
            }

            //
            // Reassert an unbroken abstract shell after carving.
            //
            ForceSealedBorder(Z);

            Z.ClearReachableMap();

            return true;
        }

        private void ClampSettings()
        {

            if (MinLargeRooms < 0)
                MinLargeRooms = 0;

            if (MaxLargeRooms < MinLargeRooms)
                MaxLargeRooms = MinLargeRooms;

            if (MinLargeRadius < 3)
                MinLargeRadius = 3;

            if (MaxLargeRadius < MinLargeRadius)
                MaxLargeRadius = MinLargeRadius;

            if (MinRooms < 1)
                MinRooms = 1;

            if (MaxRooms < MinRooms)
                MaxRooms = MinRooms;

            if (MinRadius < 2)
                MinRadius = 2;

            if (MaxRadius < MinRadius)
                MaxRadius = MinRadius;

            if (AnchorRadius < 2)
                AnchorRadius = 2;

            if (Border < 1)
                Border = 1;

            if (RoomGap < 0)
                RoomGap = 0;

            if (ExtraConnections < 0)
                ExtraConnections = 0;
        }

        private bool IsFarEnoughFromExistingCenters(
            Room candidate,
            List<Room> rooms
        )
        {
            if (candidate == null)
                return false;

            foreach (Room existing in rooms)
            {
                if (existing == null)
                    continue;

                int dx =
                    candidate.X -
                    existing.X;

                int dy =
                    candidate.Y -
                    existing.Y;

                double centerDistance =
                    Math.Sqrt(
                        (double)dx * dx +
                        (double)dy * dy
                    );

                //
                // Use each chamber's approximate outer radius rather than a fixed
                // center-to-center distance. This keeps independently generated rooms
                // from simply merging together and leaves real solid material for the
                // corridor pass to cut through.
                //
                int candidateRadius =
                    Math.Max(
                        candidate.RX,
                        candidate.RY
                    );

                int existingRadius =
                    Math.Max(
                        existing.RX,
                        existing.RY
                    );

                int requiredDistance =
                    candidateRadius +
                    existingRadius +
                    RoomGap;

                if (
                    centerDistance <
                    requiredDistance
                )
                {
                    return false;
                }
            }

            return true;
        }



        private void CarveEllipse(
            Zone Z,
            Room room
        )
        {
            if (
                Z == null ||
                room == null
            )
            {
                return;
            }

            long rxSquared =
                (long)room.RX *
                room.RX;

            long rySquared =
                (long)room.RY *
                room.RY;

            long threshold =
                rxSquared *
                rySquared;

            for (
                int x = room.X - room.RX;
                x <= room.X + room.RX;
                x++
            )
            {
                for (
                    int y = room.Y - room.RY;
                    y <= room.Y + room.RY;
                    y++
                )
                {
                    Cell cell =
                        Z.GetCell(x, y);

                    if (cell == null)
                        continue;

                    int dx =
                        x - room.X;

                    int dy =
                        y - room.Y;

                    long value =
                        (long)dx * dx *
                            rySquared +
                        (long)dy * dy *
                            rxSquared;

                    if (value > threshold)
                        continue;

                    CarveCell(
                        Z,
                        x,
                        y
                    );
                }
            }
        }

        private void CarveCorridor(
            Zone Z,
            Room a,
            Room b,
            System.Random rng
        )
        {
            if (
                Z == null ||
                a == null ||
                b == null
            )
            {
                return;
            }

            int x = a.X;
            int y = a.Y;

            bool horizontalFirst =
                rng.Next(2) == 0;

            if (horizontalFirst)
            {
                while (x != b.X)
                {
                    x +=
                        b.X > x
                            ? 1
                            : -1;

                    CarveCell(
                        Z,
                        x,
                        y
                    );
                }

                while (y != b.Y)
                {
                    y +=
                        b.Y > y
                            ? 1
                            : -1;

                    CarveCell(
                        Z,
                        x,
                        y
                    );
                }
            }
            else
            {
                while (y != b.Y)
                {
                    y +=
                        b.Y > y
                            ? 1
                            : -1;

                    CarveCell(
                        Z,
                        x,
                        y
                    );
                }

                while (x != b.X)
                {
                    x +=
                        b.X > x
                            ? 1
                            : -1;

                    CarveCell(
                        Z,
                        x,
                        y
                    );
                }
            }
        }

        private void CarveCell(
            Zone Z,
            int x,
            int y
        )
        {
            if (
                x < Border ||
                y < Border ||
                x >= Z.Width - Border ||
                y >= Z.Height - Border
            )
            {
                return;
            }

            Cell cell =
                Z.GetCell(
                    x,
                    y
                );

            if (cell == null)
                return;

            cell.ClearWalls();
        }

        private void ForceSealedBorder(
            Zone Z
        )
        {
            for (int x = 0; x < Z.Width; x++)
            {
                for (int y = 0; y < Z.Height; y++)
                {
                    if (
                        x >= Border &&
                        y >= Border &&
                        x < Z.Width - Border &&
                        y < Z.Height - Border
                    )
                    {
                        continue;
                    }

                    Cell cell =
                        Z.GetCell(
                            x,
                            y
                        );

                    if (cell == null)
                        continue;

                    cell.Clear();

                    cell.AddObject(
                        SubterraneanSites
                            .SubterraneanSitesEPGeometry
                            .SolidPlaceholderBlueprint
                    );
                }
            }
        }
    }

    /// <summary>
    /// Shared Ooze-theme liquid creation/mixing.
    ///
    /// Both C1 environmental pools and the small putrid stain beneath
    /// C5 desecrated statues use the same liquid behavior.
    /// </summary>
    internal static class SubterraneanSitesOozeLiquidUtility
    {
        internal const int WadingPoolVolume = 200;

        internal static void AddOrMixLiquid(
            Cell cell,
            string liquidID
        )
        {
            if (
                cell == null ||
                liquidID.IsNullOrEmpty()
            )
            {
                return;
            }

            LiquidVolume existing = null;

            foreach (
                GameObject obj
                in cell.GetObjectsInCell()
            )
            {
                if (obj == null)
                    continue;

                LiquidVolume liquid =
                    obj.LiquidVolume;

                if (
                    liquid != null &&
                    liquid.IsOpenVolume()
                )
                {
                    existing = liquid;
                    break;
                }
            }

            if (existing != null)
            {
                int amount =
                    Math.Max(
                        WadingPoolVolume,
                        existing.Volume
                    );

                existing.MixWith(
                    new LiquidVolume(
                        liquidID,
                        amount
                    )
                );

                existing.CheckImage();

                return;
            }

            GameObject pool =
                GameObject.Create(
                    "Water"
                );

            if (
                pool == null ||
                pool.LiquidVolume == null
            )
            {
                return;
            }

            pool.LiquidVolume.InitialLiquid =
                liquidID + "-1000";

            pool.LiquidVolume.Volume =
                WadingPoolVolume;

            cell.AddObject(
                pool
            );

            pool.LiquidVolume.CheckImage();
        }
    }


    /// <summary>
    /// Ooze Category 1 physical environment.
    ///
    /// Creates the large overlapping sludge/goo/ooze/putrescence pools
    /// associated with the dominant Ooze environment.
    ///
    /// Each liquid grows independently so overlaps deliberately produce
    /// mixed pools.
    ///
    /// The entrance uses the previous reduced scar-scale pool tuning.
    /// </summary>
    public class SubterraneanSitesOozePrimaryPools :
        ZoneBuilderSandbox
    {
        public int EntranceOnly = 0;

        public int NormalPatchesPerLiquid = 6;
        public int EntrancePatchesPerLiquid = 2;

        public int MinNormalPatchCells = 24;
        public int MaxNormalPatchCells = 42;

        public int MinEntrancePatchCells = 12;
        public int MaxEntrancePatchCells = 20;

        private const int TransitionExclusionRadius = 5;

        private static readonly string[] LiquidIDs =
        {
            "sludge",
            "goo",
            "ooze",
            "putrid"
        };


        public bool BuildZone(
            Zone Z
        )
        {
            if (Z == null)
                return true;

            int seed =
                XRLCore.Core.Game.GetWorldSeed(
                    "SubterraneanSites:OozePrimaryPools:" +
                    Z.ZoneID +
                    ":Entrance=" +
                    EntranceOnly.ToString()
                );

            System.Random rng =
                new System.Random(
                    seed
                );

            List<Location2D> anchors =
                SubterraneanSites
                    .SubterraneanSitesEPVerticalTransitions
                    .GetVerticalAnchors(
                        Z.ZoneID
                    );

            int patchesPerLiquid =
                EntranceOnly != 0
                    ? EntrancePatchesPerLiquid
                    : NormalPatchesPerLiquid;

            int minPatchCells =
                EntranceOnly != 0
                    ? MinEntrancePatchCells
                    : MinNormalPatchCells;

            int maxPatchCells =
                EntranceOnly != 0
                    ? MaxEntrancePatchCells
                    : MaxNormalPatchCells;

            foreach (
                string liquidID
                in LiquidIDs
            )
            {
                for (
                    int patchIndex = 0;
                    patchIndex < patchesPerLiquid;
                    patchIndex++
                )
                {
                    int targetCells =
                        rng.Next(
                            minPatchCells,
                            maxPatchCells + 1
                        );

                    HashSet<Cell> patch =
                        GrowLiquidPatch(
                            Z,
                            anchors,
                            targetCells,
                            rng
                        );

                    foreach (
                        Cell cell
                        in patch
                    )
                    {
                        SubterraneanSitesOozeLiquidUtility
                            .AddOrMixLiquid(
                                cell,
                                liquidID
                            );
                    }
                }
            }

            return true;
        }


        private HashSet<Cell> GrowLiquidPatch(
            Zone Z,
            List<Location2D> anchors,
            int targetCells,
            System.Random rng
        )
        {
            HashSet<Cell> result =
                new HashSet<Cell>();

            List<Cell> seeds =
                GetPoolCells(
                    Z,
                    anchors
                );

            if (seeds.Count == 0)
                return result;

            Cell seed =
                seeds[
                    rng.Next(
                        seeds.Count
                    )
                ];

            List<Cell> frontier =
                new List<Cell>();

            HashSet<int> queued =
                new HashSet<int>();

            frontier.Add(
                seed
            );

            queued.Add(
                CellKey(
                    Z,
                    seed
                )
            );

            while (
                frontier.Count > 0 &&
                result.Count < targetCells
            )
            {
                int index =
                    rng.Next(
                        frontier.Count
                    );

                Cell current =
                    frontier[index];

                frontier.RemoveAt(
                    index
                );

                if (
                    !IsPoolCell(
                        Z,
                        current,
                        anchors
                    )
                )
                {
                    continue;
                }

                result.Add(
                    current
                );

                //
                // Preserve the existing broad, ragged eight-neighbor
                // pool growth.
                //
                for (
                    int dx = -1;
                    dx <= 1;
                    dx++
                )
                {
                    for (
                        int dy = -1;
                        dy <= 1;
                        dy++
                    )
                    {
                        if (
                            dx == 0 &&
                            dy == 0
                        )
                        {
                            continue;
                        }

                        Cell next =
                            Z.GetCell(
                                current.X + dx,
                                current.Y + dy
                            );

                        if (
                            !IsPoolCell(
                                Z,
                                next,
                                anchors
                            )
                        )
                        {
                            continue;
                        }

                        int key =
                            CellKey(
                                Z,
                                next
                            );

                        if (!queued.Add(key))
                            continue;

                        frontier.Add(
                            next
                        );
                    }
                }
            }

            return result;
        }


        private List<Cell> GetPoolCells(
            Zone Z,
            List<Location2D> anchors
        )
        {
            List<Cell> result =
                new List<Cell>();

            foreach (
                Cell cell
                in Z.GetCells()
            )
            {
                if (
                    IsPoolCell(
                        Z,
                        cell,
                        anchors
                    )
                )
                {
                    result.Add(
                        cell
                    );
                }
            }

            return result;
        }


        private bool IsPoolCell(
            Zone Z,
            Cell cell,
            List<Location2D> anchors
        )
        {
            if (
                Z == null ||
                cell == null
            )
            {
                return false;
            }

            if (EntranceOnly != 0)
            {
                if (
                    !SubterraneanSites
                        .SubterraneanSitesEPEntrance
                        .IsInsideScar(
                            Z,
                            cell
                        )
                )
                {
                    return false;
                }

                if (
                    SubterraneanSites
                        .SubterraneanSitesEPEntrance
                        .IsInsideHoleExclusion(
                            Z,
                            cell,
                            1
                        )
                )
                {
                    return false;
                }
            }
            else
            {
                if (
                    !SubterraneanSites
                        .SubterraneanSitesEPGeometry
                        .IsOpenGeometryCell(
                            cell
                        )
                )
                {
                    return false;
                }

                if (
                    SubterraneanSites
                        .SubterraneanSitesEPPlacement
                        .IsNearAnyAnchor(
                            cell.X,
                            cell.Y,
                            anchors,
                            TransitionExclusionRadius
                        )
                )
                {
                    return false;
                }
            }

            //
            // Ooze C1 is a spill.
            //
            // Existing liquid and discrete objects are deliberately allowed.
            // The spill may mix beneath or through machinery, doors, creatures,
            // decorations, and other content as long as the cell belongs to valid
            // open EP geometry.
            //
            // Pools and reservations do not constrain spill painting.
            //
            return true;
        }


        private int CellKey(
            Zone Z,
            Cell cell
        )
        {
            return
                cell.Y *
                Z.Width +
                cell.X;
        }
    }


    /// <summary>
    /// Ooze Category 2.
    ///
    /// Places a small number of vanilla radial acid walltraps directly
    /// on open floor rather than mounting them into the structural boundary.
    /// WalltrapAcid already owns the radial acid behavior; this builder owns
    /// only EP placement, spacing, and transition protection.
    /// </summary>
    public class SubterraneanSitesOozeAcidTraps :
        ZoneBuilderSandbox
    {
        public string HazardBlueprint =
            "WalltrapAcid";

        public int MinTraps = 1;
        public int MaxTraps = 4;

        public int MinSpacing = 10;

        public int TransitionExclusionRadius = 5;

        public int MinTurnInterval = 4;
        public int MaxTurnInterval = 8;


        public bool BuildZone(
            Zone Z
        )
        {
            if (
                Z == null ||
                HazardBlueprint.IsNullOrEmpty()
            )
            {
                return true;
            }

            ClampSettings();

            int seed =
                XRLCore.Core.Game.GetWorldSeed(
                    "SubterraneanSites:OozeAcidTraps:" +
                    Z.ZoneID
                );

            System.Random rng =
                new System.Random(
                    seed
                );

            List<Location2D> anchors =
                SubterraneanSites
                    .SubterraneanSitesEPVerticalTransitions
                    .GetVerticalAnchors(
                        Z.ZoneID
                    );

            List<Cell> candidates =
                new List<Cell>();

            foreach (
                Cell cell
                in Z.GetCells()
            )
            {
                if (
                    IsCandidate(
                        Z,
                        cell,
                        anchors
                    )
                )
                {
                    candidates.Add(
                        cell
                    );
                }
            }

            if (candidates.Count == 0)
                return true;

            int desired =
                rng.Next(
                    MinTraps,
                    MaxTraps + 1
                );

            List<Cell> selected =
                new List<Cell>();

            while (
                candidates.Count > 0 &&
                selected.Count < desired
            )
            {
                int index =
                    rng.Next(
                        candidates.Count
                    );

                Cell candidate =
                    candidates[index];

                candidates.RemoveAt(
                    index
                );

                if (
                    !FarEnoughFromSelected(
                        candidate,
                        selected
                    )
                )
                {
                    continue;
                }

                selected.Add(
                    candidate
                );
            }

            foreach (
                Cell cell
                in selected
            )
            {
                PlaceTrap(
                    cell,
                    rng
                );
            }

            return true;
        }


        private void ClampSettings()
        {
            if (MinTraps < 0)
                MinTraps = 0;

            if (MaxTraps < MinTraps)
                MaxTraps = MinTraps;

            if (MinSpacing < 0)
                MinSpacing = 0;

            if (TransitionExclusionRadius < 0)
                TransitionExclusionRadius = 0;

            if (MinTurnInterval < 1)
                MinTurnInterval = 1;

            if (MaxTurnInterval < MinTurnInterval)
                MaxTurnInterval = MinTurnInterval;
        }


        private bool IsCandidate(
            Zone Z,
            Cell cell,
            List<Location2D> anchors
        )
        {
            if (
                Z == null ||
                cell == null
            )
            {
                return false;
            }

            //
            // The radial acid trap needs a small amount of functional clearance.
            //
            // Reject this candidate if any part of its 3x3 operating footprint is
            // already owned by higher-priority EP content.
            //
            if (
                !SubterraneanSites
                    .SubterraneanSitesEPReservations
                    .RectangleIsAvailable(
                        Z,
                        cell.X - 1,
                        cell.Y - 1,
                        cell.X + 1,
                        cell.Y + 1
                    )
            )
            {
                return false;
            }

            if (
                !SubterraneanSites
                    .SubterraneanSitesEPGeometry
                    .IsOpenGeometryCell(
                        cell
                    )
            )
            {
                return false;
            }

            if (!cell.IsEmptyOfSolid())
                return false;

            //
            // Spills are valid hazard space.
            //
            // The acid trap may sit directly in Ooze, oil, asphalt, or another
            // permissive liquid. Object-like pools exclude it through claims instead.
            //
            if (cell.HasSpawnBlocker())
            {
                return false;
            }

            if (
                cell.HasObjectWithBlueprint(
                    "StairsUp"
                ) ||
                cell.HasObjectWithBlueprint(
                    "StairsDown"
                ) ||
                cell.HasObjectWithBlueprint(
                    "Pit"
                )
            )
            {
                return false;
            }

            if (
                SubterraneanSites
                    .SubterraneanSitesEPPlacement
                    .IsNearAnyAnchor(
                        cell.X,
                        cell.Y,
                        anchors,
                        TransitionExclusionRadius
                    )
            )
            {
                return false;
            }

            // Generic meaningful-occupant protection.
            //
            // Shared reservations already protect cells explicitly owned by
            // higher-priority EP content. This additional check prevents the
            // radial hazard from occupying living, inventory-bearing, or
            // furniture-like objects.
            foreach (
                GameObject obj
                in cell.GetObjects()
            )
            {
                if (obj == null)
                    continue;

                if (
                    obj.Brain != null ||
                    obj.Inventory != null ||
                    obj.HasTagOrProperty(
                        "Furniture"
                    )
                )
                {
                    return false;
                }
            }

            return true;
        }


        private bool FarEnoughFromSelected(
            Cell candidate,
            List<Cell> selected
        )
        {
            if (candidate == null)
                return false;

            foreach (
                Cell other
                in selected
            )
            {
                if (other == null)
                    continue;

                int dx =
                    Math.Abs(
                        candidate.X -
                        other.X
                    );

                int dy =
                    Math.Abs(
                        candidate.Y -
                        other.Y
                    );

                if (
                    Math.Max(
                        dx,
                        dy
                    ) <
                    MinSpacing
                )
                {
                    return false;
                }
            }

            return true;
        }


        private void PlaceTrap(
            Cell cell,
            System.Random rng
        )
        {
            if (
                cell == null ||
                rng == null
            )
            {
                return;
            }

            GameObject trap =
                GameObjectFactory.Factory
                    .CreateObject(
                        HazardBlueprint
                    );

            if (trap == null)
                return;

            Walltrap walltrap =
                trap.GetPart<Walltrap>();

            if (walltrap != null)
            {
                walltrap.TurnInterval =
                    rng.Next(
                        MinTurnInterval,
                        MaxTurnInterval + 1
                    );

                walltrap.CurrentTurn =
                    rng.Next(
                        0,
                        walltrap.TurnInterval
                    );
            }

            cell.AddObject(
                trap
            );

            //
            // Preserve a one-cell operating area around the radial hazard.
            //
            // This protects the trap from later discrete C5 content, while spill
            // painters remain free to ignore the reservation and cover the area.
            //
            SubterraneanSites
                .SubterraneanSitesEPReservations
                .ClaimRectangle(
                    cell.ParentZone,
                    cell.X - 1,
                    cell.Y - 1,
                    cell.X + 1,
                    cell.Y + 1
                );
        }
    }


    /// <summary>
    /// Ooze Category 5.
    ///
    /// Filthy extradimensional ecology and debris:
    /// desecrated statues, grey boulders, rubble, garbage, bones,
    /// and extradimensional giant weeps.
    ///
    /// The major sludge/goo/ooze/putrescence pool field belongs to
    /// Category 1 and is generated separately.
    /// </summary>
    public class SubterraneanSitesOozeDecorations :
        ZoneBuilderSandbox
    {

        public string DecorationThemeKey =
            "Ooze";
        public int MinWeeps = 6;
        public int MaxWeeps = 9;

        public int MinEntranceWeeps = 1;
        public int MaxEntranceWeeps = 2;
        public int EntranceOnly = 0;

        public int MinStatues = 3;
        public int MaxStatues = 5;

        public int MinEntranceStatues = 1;
        public int MaxEntranceStatues = 2;

        public int MinBoulders = 10;
        public int MaxBoulders = 16;

        public int MinEntranceBoulders = 2;
        public int MaxEntranceBoulders = 5;

        public int MinRubble = 10;
        public int MaxRubble = 18;

        public int MinEntranceRubble = 2;
        public int MaxEntranceRubble = 5;

        public int MinTrash = 52;
        public int MaxTrash = 100;

        public int MinEntranceTrash = 16;
        public int MaxEntranceTrash = 32;

        public int MinBones = 4;
        public int MaxBones = 10;

        public int MinEntranceBones = 1;
        public int MaxEntranceBones = 3;

        private const int TransitionExclusionRadius = 5;

        private static readonly string[] EaterStatueBlueprints =
        {
            "EaterStatue",
            "EaterStatueFlipped",
            "ImplantedEaterStatue"
        };

        private const string BoulderBlueprint =
            "SmallBoulder Grey";

        private const string RubbleBlueprint =
            "SubterraneanSitesColdRubble";

        private const string TrashBlueprint =
            "Garbage";

        private const string BonesBlueprint =
            "Bones";

        private static readonly string[] GiantWeepBlueprints =
        {
            "honeyLichen",
            "waxLichen",
            "inkLichen",
            "sapLichen",
            "lavaLichen",
            "acidLichen",
            "wineLichen",
            "slimeLichen",
            "ciderLichen",
            "waterLichen",
            "gelLichen",
            "asphaltLichen",
            "saltLichen",
            "oilLichen"
        };

        public bool BuildZone(Zone Z)
        {
            if (Z == null)
                return true;

            int seed =
                XRLCore.Core.Game.GetWorldSeed(
                    "SubterraneanSites:OozeDecorations:" +
                    Z.ZoneID +
                    ":Entrance=" +
                    EntranceOnly.ToString()
                );

            System.Random rng =
                new System.Random(seed);

            List<Location2D> anchors =
                SubterraneanSites
                    .SubterraneanSitesEPVerticalTransitions
                    .GetVerticalAnchors(
                        Z.ZoneID
                    );

            PlaceDesecratedStatues(
                Z,
                anchors,
                rng
            );

            PlaceBoulders(
                Z,
                anchors,
                rng
            );

            PlaceRubble(
                Z,
                anchors,
                rng
            );

            PlaceTrash(
                Z,
                anchors,
                rng
            );

            PlaceBones(
                Z,
                anchors,
                rng
            );

            PlaceGiantWeeps(
                Z,
                anchors,
                rng
            );



            return true;
        }

        private void PlaceGiantWeeps(
            Zone Z,
            List<Location2D> anchors,
            System.Random rng
        )
        {
            if (
                Z == null ||
                rng == null ||
                GiantWeepBlueprints.Length == 0
            )
            {
                return;
            }

            int min =
                EntranceOnly != 0
                    ? MinEntranceWeeps
                    : MinWeeps;

            int max =
                EntranceOnly != 0
                    ? MaxEntranceWeeps
                    : MaxWeeps;

            if (min < 0)
                min = 0;

            if (max < min)
                max = min;

            int desired =
                rng.Next(
                    min,
                    max + 1
                );

            for (int i = 0; i < desired; i++)
            {
                List<Cell> candidates =
                    GetWeepCells(
                        Z,
                        anchors
                    );

                if (candidates.Count == 0)
                    return;

                Cell cell =
                    candidates[
                        rng.Next(
                            candidates.Count
                        )
                    ];

                string blueprint =
                    GiantWeepBlueprints[
                        rng.Next(
                            GiantWeepBlueprints.Length
                        )
                    ];

                GameObject weep =
                    GameObject.Create(
                        blueprint
                    );

                if (weep == null)
                    continue;

                SubterraneanSites
                    .SubterraneanSitesDimensionEngine
                    .ApplyDecorationDimensionIdentity(
                        weep,
                        DecorationThemeKey
                    );

                cell.AddObject(
                    weep
                );

                weep.MakeActive();
                
                //
                // Match vanilla liquid-weep placement.
                //
                // Do not force the weep active during zone construction. Giant weeps own
                // ongoing liquid behavior and should enter the normal Qud activation
                // lifecycle after the finished zone is activated.
                //

                //
                // Only the giant-weep source object is exclusive.
                //
                // Any liquid produced by the weep remains a spill and is free to spread,
                // mix, and overlap other EP content according to the normal liquid system.
                //
                SubterraneanSites
                    .SubterraneanSitesEPReservations
                    .ClaimCell(
                        Z,
                        cell
                    );
            }
        }

        private List<Cell> GetWeepCells(
            Zone Z,
            List<Location2D> anchors
        )
        {
            List<Cell> result =
                new List<Cell>();

            if (Z == null)
                return result;

            foreach (Cell cell in Z.GetCells())
            {
                if (
                    !IsDecorationCell(
                        Z,
                        cell,
                        anchors,
                        requireEmptySolid: true
                    )
                )
                {
                    continue;
                }

                //
                // Existing liquid is explicitly allowed: the giant-weep source may stand
                // in a spill.
                //
                // Earlier discrete C5 content is excluded through shared reservations.
                // The liquid subsequently emitted by the weep remains overlap-permissive.
                //
                bool hasCreature =
                    false;

                foreach (
                    GameObject obj
                    in cell.GetObjectsInCell()
                )
                {
                    if (
                        obj != null &&
                        obj.Brain != null
                    )
                    {
                        hasCreature =
                            true;

                        break;
                    }
                }

                if (hasCreature)
                    continue;

                result.Add(cell);
            }

            return result;
        }

        private void PlaceDesecratedStatues(
            Zone Z,
            List<Location2D> anchors,
            System.Random rng
        )
        {
            int min =
                EntranceOnly != 0
                    ? MinEntranceStatues
                    : MinStatues;

            int max =
                EntranceOnly != 0
                    ? MaxEntranceStatues
                    : MaxStatues;

            if (min < 0)
                min = 0;

            if (max < min)
                max = min;

            int desired =
                rng.Next(
                    min,
                    max + 1
                );

            for (int i = 0; i < desired; i++)
            {
                List<Cell> candidates =
                    GetBroadOpenDecorationCells(
                        Z,
                        anchors
                    );

                if (candidates.Count == 0)
                    return;

                Cell cell =
                    candidates[
                        rng.Next(
                            candidates.Count
                        )
                    ];

                string blueprint =
                    EaterStatueBlueprints[
                        rng.Next(
                            EaterStatueBlueprints.Length
                        )
                    ];

                GameObject statue =
                    GameObjectFactory.Factory
                        .CreateObject(
                            blueprint
                        );

                if (statue == null)
                    continue;

                cell.AddObject(statue);

                //
                // The statue is discrete C5 content.
                //
                SubterraneanSites
                    .SubterraneanSitesEPReservations
                    .ClaimCell(
                        Z,
                        cell
                    );

                //
                // Visibly desecrate the statue itself...
                //
                statue.ForceApplyEffect(
                    new LiquidStained(
                        "putrid",
                        4,
                        9999
                    )
                );

                //
                // ...and foul the ground around its base with putrescence.
                //
                SubterraneanSitesOozeLiquidUtility
                    .AddOrMixLiquid(
                        cell,
                        "putrid"
                    );
            }
        }

        private void PlaceBoulders(
            Zone Z,
            List<Location2D> anchors,
            System.Random rng
        )
        {
            int min =
                EntranceOnly != 0
                    ? MinEntranceBoulders
                    : MinBoulders;

            int max =
                EntranceOnly != 0
                    ? MaxEntranceBoulders
                    : MaxBoulders;

            if (min < 0)
                min = 0;

            if (max < min)
                max = min;

            int desired =
                rng.Next(
                    min,
                    max + 1
                );

            for (int i = 0; i < desired; i++)
            {
                List<Cell> candidates =
                    GetBroadOpenDecorationCells(
                        Z,
                        anchors
                    );

                if (candidates.Count == 0)
                    return;

                Cell cell =
                    candidates[
                        rng.Next(
                            candidates.Count
                        )
                    ];

                cell.AddObject(
                    BoulderBlueprint
                );

                SubterraneanSites
                    .SubterraneanSitesEPReservations
                    .ClaimCell(
                        Z,
                        cell
                    );
            }
        }

        private void PlaceRubble(
            Zone Z,
            List<Location2D> anchors,
            System.Random rng
        )
        {
            int min =
                EntranceOnly != 0
                    ? MinEntranceRubble
                    : MinRubble;

            int max =
                EntranceOnly != 0
                    ? MaxEntranceRubble
                    : MaxRubble;

            if (min < 0)
                min = 0;

            if (max < min)
                max = min;

            int desired =
                rng.Next(
                    min,
                    max + 1
                );

            for (int i = 0; i < desired; i++)
            {
                List<Cell> candidates =
                    GetBroadOpenDecorationCells(
                        Z,
                        anchors
                    );

                if (candidates.Count == 0)
                    return;

                Cell cell =
                    candidates[
                        rng.Next(
                            candidates.Count
                        )
                    ];

                cell.AddObject(
                    RubbleBlueprint
                );

                SubterraneanSites
                    .SubterraneanSitesEPReservations
                    .ClaimCell(
                        Z,
                        cell
                    );
            }
        }

        private void PlaceTrash(
            Zone Z,
            List<Location2D> anchors,
            System.Random rng
        )
        {
            int min =
                EntranceOnly != 0
                    ? MinEntranceTrash
                    : MinTrash;

            int max =
                EntranceOnly != 0
                    ? MaxEntranceTrash
                    : MaxTrash;

            PlaceScatterObjects(
                Z,
                anchors,
                rng,
                TrashBlueprint,
                min,
                max
            );
        }

        private void PlaceBones(
            Zone Z,
            List<Location2D> anchors,
            System.Random rng
        )
        {
            int min =
                EntranceOnly != 0
                    ? MinEntranceBones
                    : MinBones;

            int max =
                EntranceOnly != 0
                    ? MaxEntranceBones
                    : MaxBones;

            PlaceScatterObjects(
                Z,
                anchors,
                rng,
                BonesBlueprint,
                min,
                max
            );
        }

        private void PlaceScatterObjects(
            Zone Z,
            List<Location2D> anchors,
            System.Random rng,
            string blueprint,
            int min,
            int max
        )
        {
            if (min < 0)
                min = 0;

            if (max < min)
                max = min;

            int desired =
                rng.Next(
                    min,
                    max + 1
                );

            for (int i = 0; i < desired; i++)
            {
                List<Cell> candidates =
                    GetDecorationCells(
                        Z,
                        anchors,
                        requireEmptySolid: true
                    );

                if (candidates.Count == 0)
                    return;

                Cell cell =
                    candidates[
                        rng.Next(
                            candidates.Count
                        )
                    ];

                cell.AddObject(
                    blueprint
                );

                SubterraneanSites
                    .SubterraneanSitesEPReservations
                    .ClaimCell(
                        Z,
                        cell
                    );
            }
        }

        private bool IsBroadOpenDecorationCell(
            Zone Z,
            Cell cell,
            List<Location2D> anchors
        )
        {
            if (
                !IsDecorationCell(
                    Z,
                    cell,
                    anchors,
                    requireEmptySolid: true
                )
            )
            {
                return false;
            }

            return
                SubterraneanSites
                    .SubterraneanSitesEPPlacement
                    .HasBroadOpenClearance(
                        Z,
                        cell,
                        delegate(Cell neighbor)
                        {
                            return IsDecorationCell(
                                Z,
                                neighbor,
                                anchors,
                                requireEmptySolid: true
                            );
                        }
                    );
        }

        private List<Cell> GetDecorationCells(
            Zone Z,
            List<Location2D> anchors,
            bool requireEmptySolid
        )
        {
            List<Cell> result =
                new List<Cell>();

            foreach (Cell cell in Z.GetCells())
            {
                if (
                    IsDecorationCell(
                        Z,
                        cell,
                        anchors,
                        requireEmptySolid
                    )
                )
                {
                    result.Add(cell);
                }
            }

            return result;
        }

        private List<Cell> GetBroadOpenDecorationCells(
            Zone Z,
            List<Location2D> anchors
        )
        {
            List<Cell> result =
                new List<Cell>();

            foreach (Cell cell in Z.GetCells())
            {
                if (
                    IsBroadOpenDecorationCell(
                        Z,
                        cell,
                        anchors
                    )
                )
                {
                    result.Add(cell);
                }
            }

            return result;
        }

        private bool IsDecorationCell(
            Zone Z,
            Cell cell,
            List<Location2D> anchors,
            bool requireEmptySolid
        )
        {
            if (
                Z == null ||
                cell == null
            )
            {
                return false;
            }

            if (EntranceOnly != 0)
            {
                if (
                    !SubterraneanSites
                        .SubterraneanSitesEPEntrance
                        .IsInsideScar(
                            Z,
                            cell
                        )
                )
                {
                    return false;
                }

                //
                // Keep C5 material away from the central entrance hole and
                // its immediate interaction space.
                //
                if (
                    SubterraneanSites
                        .SubterraneanSitesEPEntrance
                        .IsInsideHoleExclusion(
                            Z,
                            cell,
                            1
                        )
                )
                {
                    return false;
                }
            }
            else
            {
                if (
                    !SubterraneanSites
                        .SubterraneanSitesEPGeometry
                        .IsOpenGeometryCell(cell)
                )
                {
                    return false;
                }

                if (
                    SubterraneanSites
                        .SubterraneanSitesEPPlacement
                        .IsNearAnyAnchor(
                            cell.X,
                            cell.Y,
                            anchors,
                            TransitionExclusionRadius
                        )
                )
                {
                    return false;
                }
            }

            //
            // Ooze C5 objects are discrete content even though the surrounding Ooze
            // liquids are permissive spills.
            //
            // Respect C1/C2 ownership, exclusive pools, and other claimed EP content.
            // Do NOT treat liquid itself as occupied space.
            //
            if (
                SubterraneanSites
                    .SubterraneanSitesEPReservations
                    .IsClaimed(
                        Z,
                        cell
                    )
            )
            {
                return false;
            }

            if (
                requireEmptySolid &&
                !cell.IsEmptyOfSolid()
            )
            {
                return false;
            }

            return true;
        }
    }
}

namespace XRL.World.Effects
{
    [Serializable]
    public class SubterraneanSitesEPOozeAttunementEffect :
        SubterraneanSitesEPAttunementEffect
    {
        public SubterraneanSitesEPOozeAttunementEffect()
        {
        }


        public SubterraneanSitesEPOozeAttunementEffect(
            int duration,
            int mutationLevel
        ) : base(
            duration,
            "Ooze",

            "",
            0,

            "Toughness",
            "Disease",
            10,

            "AcidSlimeGlands",
            mutationLevel
        )
        {
        }


        protected override bool ApplyTheme(
            GameObject Object
        )
        {
            if (Object == null)
                return false;

            SubterraneanSitesEPOozeIllnessEffect ambientIllness;

            if (
                Object.TryGetEffect<
                    SubterraneanSitesEPOozeIllnessEffect
                >(out ambientIllness) &&
                ambientIllness != null
            )
            {
                Object.RemoveEffect(
                    ambientIllness
                );
            }

            return true;
        }


        protected override void RemoveTheme(
            GameObject Object
        )
        {
            if (
                Object == null ||
                !Object.IsPlayer() ||
                Object.CurrentZone == null
            )
            {
                return;
            }


            string category1 =
                SubterraneanSites
                    .SubterraneanSitesEPAttunementSystem
                    .GetCategory1Theme(
                        Object.CurrentZone
                    );


            if (
                !string.Equals(
                    category1,
                    "Ooze",
                    StringComparison.Ordinal
                )
            )
            {
                return;
            }


            //
            // A real vanilla illness remains authoritative. Do not stack
            // the EP-owned equivalent over it.
            //
            if (
                Object.HasEffect<
                    Ill
                >()
            )
            {
                return;
            }


            SubterraneanSitesEPOozeIllnessEffect ambientIllness;

            if (
                !Object.TryGetEffect<
                    SubterraneanSitesEPOozeIllnessEffect
                >(out ambientIllness)
            )
            {
                Object.ApplyEffect(
                    new SubterraneanSitesEPOozeIllnessEffect()
                );
            }
        }
    }
    /// <summary>
    /// Ambient illness caused specifically by Ooze Category 1.
    ///
    /// Kept separate from vanilla Ill so attunement can remove the
    /// extradimensional environmental illness without curing an unrelated
    /// illness the player acquired elsewhere.
    ///
    /// It reproduces the relevant vanilla Ill penalties:
    /// - no natural hit-point regeneration
    /// - external healing is halved
    ///
    /// Lifetime is controlled by the Ooze environment system rather than a
    /// normal effect countdown.
    /// </summary>
    [Serializable]
    public class SubterraneanSitesEPOozeIllnessEffect :
        Effect
    {
        public SubterraneanSitesEPOozeIllnessEffect()
        {
            DisplayName =
                "{{g|ill}}";

            Duration = 1;
        }

        public override bool UseStandardDurationCountdown()
        {
            return false;
        }

        public override string GetDescription()
        {
            return "{{g|ill}}";
        }

        public override string GetDetails()
        {
            return
                "Doesn't heal hit points naturally.\n" +
                "External healing is only half as effective.";
        }

        public override bool Apply(
            GameObject Object
        )
        {
            if (Object == null)
                return false;

            //
            // Respect vanilla systems that explicitly prevent Ill.
            //
            if (!Object.FireEvent("ApplyIll"))
                return false;

            if (Object.IsPlayer())
            {
                XRL.UI.Popup.Show(
                    "You feel ill."
                );
            }

            return true;
        }

        public override void Register(
            GameObject Object,
            IEventRegistrar Registrar
        )
        {
            Registrar.Register(
                "Healing"
            );

            Registrar.Register(
                "Regenerating"
            );

            base.Register(
                Object,
                Registrar
            );
        }

        public override bool FireEvent(
            Event E
        )
        {
            if (E.ID == "Healing")
            {
                E.SetParameter(
                    "Amount",
                    E.GetIntParameter("Amount") /
                    2
                );
            }
            else if (E.ID == "Regenerating")
            {
                E.SetParameter(
                    "Amount",
                    0
                );

                return false;
            }

            return base.FireEvent(E);
        }
    }
}

namespace SubterraneanSites
{
    /// <summary>
    /// Runtime Category-1 environment for Ooze.
    ///
    /// While the player is in a zone whose actual C1 winner is Ooze,
    /// maintain the EP-owned illness unless the player is attuned.
    ///
    /// A pre-existing vanilla Ill effect is left completely alone and prevents
    /// our equivalent ambient illness from stacking a second healing penalty.
    /// </summary>
    [Serializable]
    public class SubterraneanSitesEPOozeEnvironmentSystem :
        IGameSystem
    {
        public override void Register(
            XRLGame Game,
            IEventRegistrar Registrar
        )
        {
            Registrar.Register(
                EndTurnEvent.ID
            );

            Registrar.Register(
                ZoneActivatedEvent.ID
            );
        }

        public override bool HandleEvent(
            EndTurnEvent E
        )
        {
            RefreshPlayerState();

            return true;
        }

        public override bool HandleEvent(
            ZoneActivatedEvent E
        )
        {
            RefreshPlayerState();

            return true;
        }

        public static void RefreshPlayerState()
        {
            GameObject player =
                The.Player;

            if (
                player == null ||
                player.CurrentZone == null
            )
            {
                return;
            }

            string category1 =
                SubterraneanSitesEPAttunementSystem
                    .GetCategory1Theme(
                        player.CurrentZone
                    );

            bool oozeEnvironment =
                category1 == "Ooze";

            bool oozeAttuned =
                SubterraneanSitesEPAttunementSystem
                    .IsAttunedTo(
                        player,
                        "Ooze"
                    );

            XRL.World.Effects
                .SubterraneanSitesEPOozeIllnessEffect
                ambientIllness;

            bool hasAmbientIllness =
                player.TryGetEffect<
                    XRL.World.Effects
                        .SubterraneanSitesEPOozeIllnessEffect
                >(out ambientIllness);

            //
            // Outside Ooze C1, or while protected by Ooze attunement,
            // our environmental illness immediately disappears.
            //
            if (
                !oozeEnvironment ||
                oozeAttuned
            )
            {
                if (
                    hasAmbientIllness &&
                    ambientIllness != null
                )
                {
                    player.RemoveEffect(
                        ambientIllness
                    );
                }

                return;
            }

            //
            // Do not stack our Ill-equivalent on top of a real vanilla Ill.
            // Attunement therefore cannot accidentally cure a pre-existing
            // ordinary illness.
            //
            if (
                player.HasEffect<
                    XRL.World.Effects.Ill
                >()
            )
            {
                if (
                    hasAmbientIllness &&
                    ambientIllness != null
                )
                {
                    player.RemoveEffect(
                        ambientIllness
                    );
                }

                return;
            }

            if (!hasAmbientIllness)
            {
                player.ApplyEffect(
                    new XRL.World.Effects
                        .SubterraneanSitesEPOozeIllnessEffect()
                );
            }
        }
    }
}