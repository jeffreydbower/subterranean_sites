using System;
using System.Collections.Generic;
using Genkit;
using XRL;
using XRL.Core;
using XRL.World;
using XRL.World.Effects;
using XRL.World.Parts;
using XRL.UI;

namespace SubterraneanSites
{
    /// <summary>
    /// ELECTRICAL DIMENSION
    ///
    /// Category 1: extradimensional power-cell drain plus connected
    ///             electrical infrastructure.
    /// Category 2: wall-mounted electrical shock traps.
    /// Category 3: MetalWall structural core, square-wave boundary,
    ///             and mainframe wall installations.
    /// Category 4: rectangular rooms and orthogonal corridors over a
    ///             dark electrical substrate with sparse blue/yellow nodes.
    /// Category 5: disconnected technological machinery and decoration.
    /// </summary>
    internal sealed class SubterraneanSitesEPElectricalTheme :
        ISubterraneanSitesEPCategoryProvider,
        ISubterraneanSitesEPPrimaryObjectProvider,
        ISubterraneanSitesEPAttunementProvider,
        ISubterraneanSitesEPDenizenAdaptationProvider,
        ISubterraneanSitesEPSignatureMutationProvider
    {
        public string ThemeKey
        {
            get { return "Electrical"; }
        }

        public string SignatureMutationClass
        {
            get { return "ElectricalGeneration"; }
        }

        internal const string DrainProtectionProperty =
            "SubterraneanSites_ElectricalDrainProtected";

        public int MinimumHoleSeparation
        {
            get { return 25; }
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
                    .SubterraneanSitesEPAttunementEffect(
                        SubterraneanSitesEPAttunementSystem
                            .DefaultDuration,
                        ThemeKey,

                        "ElectricResistance",
                        100,

                        "",
                        "",
                        0,

                        SignatureMutationClass,
                        mutationLevel
                    );

            successMessage =
                "Attunement grants:\n" +
                "Electrical Generation (level " +
                mutationLevel.ToString() +
                ")\n" +
                "+100 Electric Resistance\n" +
                "Power cells in your possession are protected from " +
                "extradimensional drain.";

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


            creature.AddStatBonus(
                "ElectricResistance",
                100
            );

            creature.SetIntProperty(
                DrainProtectionProperty,
                1
            );
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
            if (cell == null)
                return;

            //
            // Electrical substrate:
            // mostly dark ground with sparse blue/yellow circuit-like dots.
            //
            int variant =
                (
                    cell.X * 3 +
                    cell.Y * 5
                ) % 4 + 1;

            cell.PaintTile =
                "Terrain/sw_ground_dots" +
                variant.ToString() +
                ".png";

            //
            // Most cells are nearly black/dark gray.
            // Regular coordinate spacing produces sparse colored nodes without
            // independent random speckling.
            //
            bool blueNode =
                cell.X % 7 == 0 &&
                cell.Y % 4 == 0;

            bool yellowNode =
                cell.X % 7 == 3 &&
                cell.Y % 4 == 2;

            if (blueNode)
            {
                cell.PaintTileColor =
                    "&B";

                cell.PaintColorString =
                    "&B^k";
            }
            else if (yellowNode)
            {
                cell.PaintTileColor =
                    "&W";

                cell.PaintColorString =
                    "&W^k";
            }
            else
            {
                cell.PaintTileColor =
                    "&K";

                cell.PaintColorString =
                    "&K^k";
            }

            cell.PaintDetailColor =
                "k";
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
            // Electrical Category 1 is a runtime environmental effect rather
            // than a zone-builder operation. RequireSystem is idempotent, so
            // multiple Electrical layers can safely request the same system.
            //
            The.Game.RequireSystem<
                SubterraneanSitesEPElectricalEnvironmentSystem
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
                "SubterraneanSitesElectricalInfrastructure"
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
                "SubterraneanSitesElectricalInfrastructure",
                "EntranceOnly", "1"
            );
        }

        public void RegisterCategory2(
            SubterraneanSitesEPLayerContext context
        )
        {
            SubterraneanSitesEPBoundaryHazards.Register(
                context,
                "WalltrapShock",
                25,
                4,
                6,
                5
            );
        }

        public void RegisterCategory3(
            SubterraneanSitesEPLayerContext context
        )
        {
            The.ZoneManager.AddZonePostBuilder(
                context.ZoneId,
                "SubterraneanSitesElectricalMaterials"
            );

            The.ZoneManager.AddZonePostBuilder(
                context.ZoneId,
                "SubterraneanSitesElectricalMainframeRuns"
            );
        }

        public void RegisterCategory4Layout(
            SubterraneanSitesEPLayerContext context
        )
        {
            The.ZoneManager.AddZonePostBuilder(
                context.ZoneId,
                "SubterraneanSitesElectricalLayout"
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
                "SubterraneanSitesElectricalFloor"
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
                "SubterraneanSitesElectricalDecorations"
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
                "SubterraneanSitesElectricalFloor",
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
                "SubterraneanSitesElectricalDecorations",
                "EntranceOnly", "1"
            );
        }
    }
}

namespace SubterraneanSites
{
    /// <summary>
    /// Electrical Category 1 runtime environment.
    ///
    /// On an underground EP layer whose actual Category-1 winner is
    /// Electrical, concrete carried power cells lose charge every turn
    /// unless the player currently has Electrical attunement.
    ///
    /// Deliberately affects concrete EnergyCell parts only:
    ///   - loose carried power cells
    ///   - power cells installed in carried/equipped devices
    ///
    /// Deliberately does NOT drain:
    ///   - ElectricalGeneration
    ///   - Capacitor
    ///   - IntegratedPowerSystems
    ///   - arbitrary IEnergyCell implementations
    ///   - the site's electrical infrastructure
    ///
    /// </summary>
    [Serializable]
    public class SubterraneanSitesEPElectricalEnvironmentSystem :
        IGameSystem
    {
        //
        //
        // Current Electrical C1 drain tuning.
        //
        public const int CellDrainPerTurn = 10;

        public override void Register(
            XRLGame Game,
            IEventRegistrar Registrar
        )
        {
            Registrar.Register(
                ZoneActivatedEvent.ID
            );

            Registrar.Register(
                EndTurnEvent.ID
            );
        }


        public override bool HandleEvent(
            ZoneActivatedEvent E
        )
        {
            SynchronizePlayerExposure();

            return true;
        }


        public override bool HandleEvent(
            EndTurnEvent E
        )
        {
            GameObject player =
                The.Player;

            if (
                player == null ||
                player.CurrentZone == null
            )
            {
                return true;
            }

            Zone zone =
                player.CurrentZone;

            bool electricalEnvironment =
                IsElectricalEnvironment(
                    zone
                );

            bool electricallyAttuned =
                SubterraneanSitesEPAttunementSystem
                    .IsAttunedTo(
                        player,
                        "Electrical"
                    );


            //
            // Keep the player's visible exposure state synchronized separately
            // from the actual per-turn cell drain.
            //
            SynchronizePlayerExposure(
                player,
                electricalEnvironment,
                electricallyAttuned
            );


            if (!electricalEnvironment)
                return true;


            HashSet<EnergyCell> drained =
                new HashSet<EnergyCell>();

            HashSet<GameObject> visitedObjects =
                new HashSet<GameObject>();


            //
            // Physical cells throughout the environment drain regardless of
            // player attunement. Electrical-native denizens are excluded by
            // DrainZoneEnergyCells().
            //
            DrainZoneEnergyCells(
                zone,
                player,
                drained,
                visitedObjects
            );


            //
            // Electrical attunement protects physical cells while they remain
            // in the player's possession.
            //
            if (electricallyAttuned)
                return true;


            DrainPlayerEnergyCells(
                player,
                drained,
                visitedObjects
            );

            return true;
        }


        private static bool IsElectricalEnvironment(
            Zone zone
        )
        {
            if (zone == null)
                return false;

            string category1 =
                SubterraneanSitesEPAttunementSystem
                    .GetCategory1Theme(
                        zone
                    );

            return
                string.Equals(
                    category1,
                    "Electrical",
                    StringComparison.Ordinal
                );
        }


        private static void SynchronizePlayerExposure()
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

            bool electricalEnvironment =
                IsElectricalEnvironment(
                    player.CurrentZone
                );

            bool electricallyAttuned =
                SubterraneanSitesEPAttunementSystem
                    .IsAttunedTo(
                        player,
                        "Electrical"
                    );

            SynchronizePlayerExposure(
                player,
                electricalEnvironment,
                electricallyAttuned
            );
        }


        private static void SynchronizePlayerExposure(
            GameObject player,
            bool electricalEnvironment,
            bool electricallyAttuned
        )
        {
            if (player == null)
                return;

            XRL.World.Effects
                .SubterraneanSitesEPElectricalDrainEffect
                drainEffect;

            bool hasDrainEffect =
                player.TryGetEffect<
                    XRL.World.Effects
                        .SubterraneanSitesEPElectricalDrainEffect
                >(out drainEffect);


            if (
                !electricalEnvironment ||
                electricallyAttuned
            )
            {
                if (
                    hasDrainEffect &&
                    drainEffect != null
                )
                {
                    player.RemoveEffect(
                        drainEffect
                    );
                }

                return;
            }


            if (!hasDrainEffect)
            {
                player.ApplyEffect(
                    new XRL.World.Effects
                        .SubterraneanSitesEPElectricalDrainEffect()
                );
            }
        }



        private static void DrainZoneEnergyCells(
            Zone zone,
            GameObject player,
            HashSet<EnergyCell> drained,
            HashSet<GameObject> visitedObjects
        )
        {
            if (
                zone == null ||
                drained == null ||
                visitedObjects == null
            )
            {
                return;
            }

            foreach (
                Cell cell
                in zone.GetCells()
            )
            {
                if (cell == null)
                    continue;

                foreach (
                    GameObject root
                    in cell.GetObjects()
                )
                {
                    if (
                        root == null ||
                        ReferenceEquals(
                            root,
                            player
                        )
                    )
                    {
                        continue;
                    }

                    if (
                        root.GetIntProperty(
                            SubterraneanSitesEPElectricalTheme
                                .DrainProtectionProperty
                        ) > 0
                    )
                    {
                        continue;
                    }

                    //
                    // Recurse from every physical root in the zone.
                    //
                    // This catches:
                    //   - loose power cells
                    //   - cells installed in ground devices
                    //   - cells inside chests and other containers
                    //   - deeper nested containers
                    //   - cells carried/equipped by NPCs
                    //
                    DrainObjectAndContents(
                        root,
                        drained,
                        visitedObjects
                    );
                }
            }
        }

        private static void DrainPlayerEnergyCells(
            GameObject player,
            HashSet<EnergyCell> drained,
            HashSet<GameObject> visitedObjects
        )
        {
            if (
                player == null ||
                drained == null ||
                visitedObjects == null
            )
            {
                return;
            }

            List<GameObject> roots =
                player.GetWholeInventoryReadonly();

            if (roots == null)
                return;

            foreach (
                GameObject root
                in roots
            )
            {
                if (root == null)
                    continue;

                //
                // Use the same recursive traversal as the zone pass.
                //
                // This matters for an unattuned player carrying a
                // container that itself contains cells or powered devices.
                //
                DrainObjectAndContents(
                    root,
                    drained,
                    visitedObjects
                );
            }
        }


        private static void DrainObjectAndContents(
            GameObject root,
            HashSet<EnergyCell> drained,
            HashSet<GameObject> visitedObjects
        )
        {
            if (
                root == null ||
                drained == null ||
                visitedObjects == null
            )
            {
                return;
            }

            //
            // A normal Qud inventory traversal may expose an object through
            // more than one route. It may also already flatten some inventory
            // relationships. Visit every GameObject at most once regardless.
            //
            if (!visitedObjects.Add(root))
                return;

            //
            // The object itself may either be a physical cell or contain an
            // explicitly socketed physical cell.
            //
            DrainObjectAndSocket(
                root,
                drained
            );

            //
            // Qud's whole-inventory API supplies normal inventory contents and,
            // for creatures, equipped objects and installed cybernetics.
            //
            // Recurse explicitly so nested containers are guaranteed even if
            // the underlying Inventory.GetObjects implementation is not itself
            // recursively flattened.
            //
            List<GameObject> contents =
                root.GetWholeInventoryReadonly();

            if (contents == null)
                return;

            foreach (
                GameObject obj
                in contents
            )
            {
                if (obj == null)
                    continue;

                DrainObjectAndContents(
                    obj,
                    drained,
                    visitedObjects
                );
            }
        }

        private static void DrainObjectAndSocket(
            GameObject obj,
            HashSet<EnergyCell> drained
        )
        {
            if (
                obj == null ||
                drained == null
            )
            {
                return;
            }

            //
            // Loose physical EnergyCell represented by this object.
            //
            DrainActualCells(
                obj,
                drained
            );

            //
            // Physical cell installed in a device.
            //
            EnergyCellSocket socket =
                obj.GetPart<EnergyCellSocket>();

            if (
                socket != null &&
                socket.Cell != null
            )
            {
                DrainActualCells(
                    socket.Cell,
                    drained
                );
            }
        }


        private static void DrainActualCells(
            GameObject cellObject,
            HashSet<EnergyCell> drained
        )
        {
            if (
                cellObject == null ||
                drained == null
            )
            {
                return;
            }

            List<EnergyCell> cells =
                cellObject.GetPartsDescendedFrom<EnergyCell>();

            if (cells == null)
                return;

            foreach (
                EnergyCell cell
                in cells
            )
            {
                if (
                    cell == null ||
                    !drained.Add(cell)
                )
                {
                    continue;
                }

                cell.UseCharge(
                    CellDrainPerTurn
                );
            }
        }
    }
}

namespace XRL.World.Effects
{
    /// <summary>
    /// Visible indicator for Electrical Category 1.
    ///
    /// The actual battery drain is performed by
    /// SubterraneanSitesEPElectricalEnvironmentSystem.
    /// </summary>
    [Serializable]
    public class SubterraneanSitesEPElectricalDrainEffect :
        Effect
    {
        public SubterraneanSitesEPElectricalDrainEffect()
        {
            DisplayName =
                "{{C|extradimensional power drain}}";

            Duration = 1;
        }


        public override bool UseStandardDurationCountdown()
        {
            return false;
        }


        public override string GetDetails()
        {
            return
                "The local extradimensional field is siphoning " +
                "charge from physical power cells throughout the area.\n\n" +
                "Electrical attunement protects power cells while they remain " +
                "in your possession.";
        }


        public override bool Apply(
            GameObject Object
        )
        {
            if (Object == null)
                return false;

            if (Object.IsPlayer())
            {
                Popup.Show(
                    "Your power cells start depleting."
                );
            }

            return true;
        }
    }
}

namespace XRL.World.ZoneBuilders
{
    /// <summary>
    /// Electrical Category-4 floor adapter.
    ///
    /// Electrical owns its circuit-pattern floor recipe. The shared EP floor
    /// system owns universal underground/scar scope and application.
    /// </summary>
    public class SubterraneanSitesElectricalFloor :
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
                            .SubterraneanSitesEPElectricalTheme
                            .CreateFloorSpec(),
                        EntranceOnly != 0
                    );
        }
    }
    
    /// <summary>
    /// Electrical Category 4.
    ///
    /// Produces a deliberately artificial plan:
    /// rectangular rooms joined by broad orthogonal corridors.
    ///
    /// Mandatory EP vertical-transition anchors receive rooms first.
    /// Additional rooms are distributed around the remaining map.
    /// A nearest-neighbor spanning network then guarantees that every
    /// room, including all transition rooms, belongs to one connected
    /// traversable complex.
    /// </summary>
    public class SubterraneanSitesElectricalLayout :
        ZoneBuilderSandbox
    {
        public int MinRooms = 15;
        public int MaxRooms = 18;

        public int MinRoomWidth = 6;
        public int MaxRoomWidth = 14;

        public int MinRoomHeight = 3;
        public int MaxRoomHeight = 7;

        public int AnchorRoomWidth = 9;
        public int AnchorRoomHeight = 5;

        public int Border = 2;
        public int RoomPadding = 1;

        // Radius 0 produces one-cell-wide hallways.
        public int CorridorRadius = 0;

        // A couple of extra links keep the layout from feeling
        // like a single branching tree.
        public int ExtraConnections = 10;

        // Temporary vanilla door while the Electrical visual set is developed.
        public string DoorBlueprint = "Door";

        // Door policy is selected per room, not per doorway.
        // Rooms that fail this roll receive no doors at all.
        public int RoomDoorChance = 70;

        private sealed class RoomSpec
        {
            public int X1;
            public int Y1;
            public int X2;
            public int Y2;

            public int CenterX
            {
                get { return (X1 + X2) / 2; }
            }

            public int CenterY
            {
                get { return (Y1 + Y2) / 2; }
            }
        }

        private sealed class DoorCandidate
        {
            public int X;
            public int Y;

            // 0 = travel north/south through the door.
            // 1 = travel east/west through the door.
            public int Axis;
        }

        public bool BuildZone(Zone Z)
        {
            if (Z == null)
                return true;

            ClampSettings();

            int seed =
                XRLCore.Core.Game.GetWorldSeed(
                    "SubterraneanSites:ElectricalLayout:" +
                    Z.ZoneID
                );

            System.Random rng =
                new System.Random(seed);

            List<RoomSpec> rooms =
                new List<RoomSpec>();

            //
            // Mandatory transition rooms first.
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

                    RoomSpec anchorRoom =
                        MakeCenteredRoom(
                            Z,
                            anchor.X,
                            anchor.Y,
                            AnchorRoomWidth,
                            AnchorRoomHeight
                        );

                    if (anchorRoom != null)
                        rooms.Add(anchorRoom);
                }
            }

            //
            // Add ordinary rectangular rooms.
            //
            int desiredRooms =
                rng.Next(
                    MinRooms,
                    MaxRooms + 1
                );

            int attempts = 300;

            while (
                rooms.Count < desiredRooms &&
                attempts-- > 0
            )
            {
                RoomSpec candidate =
                    MakeRandomRoom(
                        Z,
                        rng
                    );

                if (
                    candidate == null ||
                    OverlapsExisting(
                        candidate,
                        rooms
                    )
                )
                {
                    continue;
                }

                rooms.Add(candidate);
            }

            //
            // Defensive fallback. We should normally have many rooms,
            // but never permit an empty Category-4 plan.
            //
            if (rooms.Count == 0)
            {
                RoomSpec center =
                    MakeCenteredRoom(
                        Z,
                        Z.Width / 2,
                        Z.Height / 2,
                        12,
                        7
                    );

                if (center != null)
                    rooms.Add(center);
            }

            //
            // Carve all rooms out of the neutral solid substrate.
            //
            foreach (RoomSpec room in rooms)
                CarveRoom(Z, room);

            //
            // Connect every room into one network.
            //
            ConnectAllRooms(
                Z,
                rooms,
                rng
            );

            AddExtraConnections(
                Z,
                rooms,
                rng
            );

            //
            // The complete final corridor network now exists.
            // Detect clean crossings through each room's surrounding
            // structural shell and place doors at those crossings.
            //
            PlaceDoors(
                Z,
                rooms,
                rng
            );

            Z.ClearReachableMap();

            return true;
        }

        private void ClampSettings()
        {
            if (MinRooms < 1)
                MinRooms = 1;

            if (MaxRooms < MinRooms)
                MaxRooms = MinRooms;

            if (MinRoomWidth < 3)
                MinRoomWidth = 3;

            if (MaxRoomWidth < MinRoomWidth)
                MaxRoomWidth = MinRoomWidth;

            if (MinRoomHeight < 3)
                MinRoomHeight = 3;

            if (MaxRoomHeight < MinRoomHeight)
                MaxRoomHeight = MinRoomHeight;

            if (AnchorRoomWidth < 3)
                AnchorRoomWidth = 3;

            if (AnchorRoomHeight < 3)
                AnchorRoomHeight = 3;

            if (Border < 1)
                Border = 1;

            if (RoomPadding < 0)
                RoomPadding = 0;

            if (CorridorRadius < 0)
                CorridorRadius = 0;

            if (ExtraConnections < 0)
                ExtraConnections = 0;

            if (RoomDoorChance < 0)
                RoomDoorChance = 0;

            if (RoomDoorChance > 100)
                RoomDoorChance = 100;
        }

        private RoomSpec MakeCenteredRoom(
            Zone Z,
            int centerX,
            int centerY,
            int width,
            int height
        )
        {
            if (Z == null)
                return null;

            width =
                Math.Max(
                    3,
                    Math.Min(
                        width,
                        Z.Width - Border * 2
                    )
                );

            height =
                Math.Max(
                    3,
                    Math.Min(
                        height,
                        Z.Height - Border * 2
                    )
                );

            int x1 =
                centerX - width / 2;

            int y1 =
                centerY - height / 2;

            int maxX1 =
                Z.Width -
                Border -
                width;

            int maxY1 =
                Z.Height -
                Border -
                height;

            x1 =
                Math.Max(
                    Border,
                    Math.Min(
                        x1,
                        maxX1
                    )
                );

            y1 =
                Math.Max(
                    Border,
                    Math.Min(
                        y1,
                        maxY1
                    )
                );

            return new RoomSpec
            {
                X1 = x1,
                Y1 = y1,
                X2 = x1 + width - 1,
                Y2 = y1 + height - 1
            };
        }

        private RoomSpec MakeRandomRoom(
            Zone Z,
            System.Random rng
        )
        {
            int width =
                rng.Next(
                    MinRoomWidth,
                    MaxRoomWidth + 1
                );

            int height =
                rng.Next(
                    MinRoomHeight,
                    MaxRoomHeight + 1
                );

            int maxX1 =
                Z.Width -
                Border -
                width;

            int maxY1 =
                Z.Height -
                Border -
                height;

            if (
                maxX1 < Border ||
                maxY1 < Border
            )
            {
                return null;
            }

            int x1 =
                rng.Next(
                    Border,
                    maxX1 + 1
                );

            int y1 =
                rng.Next(
                    Border,
                    maxY1 + 1
                );

            return new RoomSpec
            {
                X1 = x1,
                Y1 = y1,
                X2 = x1 + width - 1,
                Y2 = y1 + height - 1
            };
        }

        private bool OverlapsExisting(
            RoomSpec candidate,
            List<RoomSpec> rooms
        )
        {
            foreach (RoomSpec room in rooms)
            {
                if (room == null)
                    continue;

                bool separated =
                    candidate.X2 + RoomPadding < room.X1 ||
                    candidate.X1 - RoomPadding > room.X2 ||
                    candidate.Y2 + RoomPadding < room.Y1 ||
                    candidate.Y1 - RoomPadding > room.Y2;

                if (!separated)
                    return true;
            }

            return false;
        }

        private void CarveRoom(
            Zone Z,
            RoomSpec room
        )
        {
            if (Z == null || room == null)
                return;

            for (
                int x = room.X1;
                x <= room.X2;
                x++
            )
            {
                for (
                    int y = room.Y1;
                    y <= room.Y2;
                    y++
                )
                {
                    Cell cell =
                        Z.GetCell(x, y);

                    if (cell != null)
                        cell.Clear();
                }
            }
        }

        private void ConnectAllRooms(
            Zone Z,
            List<RoomSpec> rooms,
            System.Random rng
        )
        {
            if (
                rooms == null ||
                rooms.Count <= 1
            )
            {
                return;
            }

            List<RoomSpec> connected =
                new List<RoomSpec>();

            List<RoomSpec> remaining =
                new List<RoomSpec>(rooms);

            connected.Add(remaining[0]);
            remaining.RemoveAt(0);

            while (remaining.Count > 0)
            {
                RoomSpec bestFrom = null;
                RoomSpec bestTo = null;
                int bestDistance = int.MaxValue;

                foreach (RoomSpec from in connected)
                {
                    foreach (RoomSpec to in remaining)
                    {
                        int distance =
                            Math.Abs(
                                from.CenterX -
                                to.CenterX
                            ) +
                            Math.Abs(
                                from.CenterY -
                                to.CenterY
                            );

                        if (distance < bestDistance)
                        {
                            bestDistance = distance;
                            bestFrom = from;
                            bestTo = to;
                        }
                    }
                }

                if (
                    bestFrom == null ||
                    bestTo == null
                )
                {
                    break;
                }

                CarveCorridor(
                    Z,
                    bestFrom.CenterX,
                    bestFrom.CenterY,
                    bestTo.CenterX,
                    bestTo.CenterY,
                    rng
                );

                connected.Add(bestTo);
                remaining.Remove(bestTo);
            }
        }

        private void AddExtraConnections(
            Zone Z,
            List<RoomSpec> rooms,
            System.Random rng
        )
        {
            if (
                rooms == null ||
                rooms.Count < 2
            )
            {
                return;
            }

            for (
                int i = 0;
                i < ExtraConnections;
                i++
            )
            {
                RoomSpec a =
                    rooms[
                        rng.Next(
                            rooms.Count
                        )
                    ];

                RoomSpec b =
                    rooms[
                        rng.Next(
                            rooms.Count
                        )
                    ];

                if (a == b)
                    continue;

                CarveCorridor(
                    Z,
                    a.CenterX,
                    a.CenterY,
                    b.CenterX,
                    b.CenterY,
                    rng
                );
            }
        }

        private void CarveCorridor(
            Zone Z,
            int fromX,
            int fromY,
            int toX,
            int toY,
            System.Random rng
        )
        {
            bool horizontalFirst =
                rng.Next(2) == 0;

            if (horizontalFirst)
            {
                CarveHorizontal(
                    Z,
                    fromX,
                    toX,
                    fromY
                );

                CarveVertical(
                    Z,
                    fromY,
                    toY,
                    toX
                );
            }
            else
            {
                CarveVertical(
                    Z,
                    fromY,
                    toY,
                    fromX
                );

                CarveHorizontal(
                    Z,
                    fromX,
                    toX,
                    toY
                );
            }
        }

        private void PlaceDoors(
            Zone Z,
            List<RoomSpec> rooms,
            System.Random rng
        )
        {
            if (
                Z == null ||
                rooms == null ||
                rooms.Count == 0 ||
                rng == null
            )
            {
                return;
            }

            Dictionary<int, DoorCandidate> candidates =
                new Dictionary<int, DoorCandidate>();

            foreach (RoomSpec room in rooms)
            {
                if (room == null)
                    continue;

                //
                // Door policy is per ROOM.
                //
                // Some rooms deliberately remain completely open to
                // their corridors. This prevents every hallway from
                // terminating in a door.
                //
                if (
                    rng.Next(100) >=
                    RoomDoorChance
                )
                {
                    continue;
                }

                //
                // NORTH + SOUTH
                //
                for (
                    int x = room.X1 + 1;
                    x <= room.X2 - 1;
                    x++
                )
                {
                    AddDoorCandidateIfValid(
                        Z,
                        candidates,
                        x,
                        room.Y1 - 1,
                        0,
                        -1
                    );

                    AddDoorCandidateIfValid(
                        Z,
                        candidates,
                        x,
                        room.Y2 + 1,
                        0,
                        1
                    );
                }

                //
                // WEST + EAST
                //
                for (
                    int y = room.Y1 + 1;
                    y <= room.Y2 - 1;
                    y++
                )
                {
                    AddDoorCandidateIfValid(
                        Z,
                        candidates,
                        room.X1 - 1,
                        y,
                        -1,
                        0
                    );

                    AddDoorCandidateIfValid(
                        Z,
                        candidates,
                        room.X2 + 1,
                        y,
                        1,
                        0
                    );
                }
            }

            //
            // Get rid of immediately adjacent doors that lie one
            // after another ALONG a corridor.
            //
            // Adjacent doors perpendicular to travel are preserved,
            // allowing attractive two-cell-wide double doorways.
            //
            RemoveSequentialDoorPairs(
                Z,
                candidates,
                rng
            );

            foreach (
                DoorCandidate candidate
                in candidates.Values
            )
            {
                Cell cell =
                    Z.GetCell(
                        candidate.X,
                        candidate.Y
                    );

                if (cell == null)
                    continue;

                if (
                    cell.GetFirstObjectWithPart(
                        "Door"
                    ) != null
                )
                {
                    continue;
                }

                if (!DoorBlueprint.IsNullOrEmpty())
                    cell.AddObject(DoorBlueprint);
            }
        }

        private void RemoveSequentialDoorPairs(
            Zone Z,
            Dictionary<int, DoorCandidate> candidates,
            System.Random rng
        )
        {
            if (
                Z == null ||
                candidates == null ||
                candidates.Count < 2 ||
                rng == null
            )
            {
                return;
            }

            HashSet<int> remove =
                new HashSet<int>();

            List<int> keys =
                new List<int>(
                    candidates.Keys
                );

            foreach (int key in keys)
            {
                if (remove.Contains(key))
                    continue;

                DoorCandidate candidate;

                if (
                    !candidates.TryGetValue(
                        key,
                        out candidate
                    ) ||
                    candidate == null
                )
                {
                    continue;
                }

                //
                // Only look in the positive direction so each pair
                // is evaluated once.
                //
                int neighborX =
                    candidate.X;

                int neighborY =
                    candidate.Y;

                if (candidate.Axis == 0)
                {
                    // Door travel is north/south.
                    neighborY++;
                }
                else
                {
                    // Door travel is east/west.
                    neighborX++;
                }

                if (
                    neighborX < 0 ||
                    neighborY < 0 ||
                    neighborX >= Z.Width ||
                    neighborY >= Z.Height
                )
                {
                    continue;
                }

                int neighborKey =
                    neighborX +
                    neighborY * Z.Width;

                if (remove.Contains(neighborKey))
                    continue;

                DoorCandidate neighbor;

                if (
                    !candidates.TryGetValue(
                        neighborKey,
                        out neighbor
                    ) ||
                    neighbor == null
                )
                {
                    continue;
                }

                //
                // Same axis + adjacency along that axis means:
                //
                //        D D
                //
                // encountered sequentially while moving through
                // the corridor. Keep exactly one.
                //
                if (
                    neighbor.Axis !=
                    candidate.Axis
                )
                {
                    continue;
                }

                if (rng.Next(2) == 0)
                    remove.Add(key);
                else
                    remove.Add(neighborKey);
            }

            foreach (int key in remove)
                candidates.Remove(key);
        }

        private void AddDoorCandidateIfValid(
            Zone Z,
            Dictionary<int, DoorCandidate> candidates,
            int doorX,
            int doorY,
            int outwardX,
            int outwardY
        )
        {
            if (
                Z == null ||
                candidates == null
            )
            {
                return;
            }

            //
            // doorX/Y is the one-cell structural ring immediately
            // outside the RoomSpec rectangle.
            //
            int insideX =
                doorX - outwardX;

            int insideY =
                doorY - outwardY;

            int outsideX =
                doorX + outwardX;

            int outsideY =
                doorY + outwardY;

            //
            // The two cells perpendicular to travel should still be
            // intact structural placeholder walls.
            //
            int sideAX =
                doorX - outwardY;

            int sideAY =
                doorY + outwardX;

            int sideBX =
                doorX + outwardY;

            int sideBY =
                doorY - outwardX;

            if (
                !IsInsideZone(Z, doorX, doorY) ||
                !IsInsideZone(Z, insideX, insideY) ||
                !IsInsideZone(Z, outsideX, outsideY) ||
                !IsInsideZone(Z, sideAX, sideAY) ||
                !IsInsideZone(Z, sideBX, sideBY)
            )
            {
                return;
            }

            Cell doorCell =
                Z.GetCell(
                    doorX,
                    doorY
                );

            Cell insideCell =
                Z.GetCell(
                    insideX,
                    insideY
                );

            Cell outsideCell =
                Z.GetCell(
                    outsideX,
                    outsideY
                );

            Cell sideA =
                Z.GetCell(
                    sideAX,
                    sideAY
                );

            Cell sideB =
                Z.GetCell(
                    sideBX,
                    sideBY
                );

            //
            // A real doorway must be a straight three-cell traversal:
            //
            //     room floor
            //         |
            //       door
            //         |
            //      corridor
            //
            // The corridor has already carved all three cells open.
            //
            if (
                !IsCarvedOpenCell(doorCell) ||
                !IsCarvedOpenCell(insideCell) ||
                !IsCarvedOpenCell(outsideCell)
            )
            {
                return;
            }

            //
            // And the door must still be flanked by structural material:
            //
            //       #
            //   . - D - .
            //       #
            //
            // or the rotated equivalent.
            //
            if (
                !SubterraneanSites
                    .SubterraneanSitesEPGeometry
                    .IsSolidPlaceholder(sideA) ||
                !SubterraneanSites
                    .SubterraneanSitesEPGeometry
                    .IsSolidPlaceholder(sideB)
            )
            {
                return;
            }

            int key =
                doorX +
                doorY * Z.Width;

            int axis =
                outwardX == 0
                    ? 0
                    : 1;

            DoorCandidate existing;

            if (
                candidates.TryGetValue(
                    key,
                    out existing
                )
            )
            {
                //
                // Same physical opening found from another room.
                // If both interpretations agree on orientation,
                // there is nothing else to do.
                //
                return;
            }

            candidates.Add(
                key,
                new DoorCandidate
                {
                    X = doorX,
                    Y = doorY,
                    Axis = axis
                }
            );
        }

        private bool IsCarvedOpenCell(
            Cell cell
        )
        {
            return
                cell != null &&
                SubterraneanSites
                    .SubterraneanSitesEPGeometry
                    .IsOpenGeometryCell(cell) &&
                !cell.IsSolid();
        }

        private bool IsInsideZone(
            Zone Z,
            int x,
            int y
        )
        {
            return
                Z != null &&
                x >= 0 &&
                y >= 0 &&
                x < Z.Width &&
                y < Z.Height;
        }

        private void CarveHorizontal(
            Zone Z,
            int x1,
            int x2,
            int y
        )
        {
            int start =
                Math.Min(x1, x2);

            int end =
                Math.Max(x1, x2);

            for (
                int x = start;
                x <= end;
                x++
            )
            {
                CarveCorridorPoint(
                    Z,
                    x,
                    y
                );
            }
        }

        private void CarveVertical(
            Zone Z,
            int y1,
            int y2,
            int x
        )
        {
            int start =
                Math.Min(y1, y2);

            int end =
                Math.Max(y1, y2);

            for (
                int y = start;
                y <= end;
                y++
            )
            {
                CarveCorridorPoint(
                    Z,
                    x,
                    y
                );
            }
        }

        private void CarveCorridorPoint(
            Zone Z,
            int centerX,
            int centerY
        )
        {
            for (
                int dx = -CorridorRadius;
                dx <= CorridorRadius;
                dx++
            )
            {
                for (
                    int dy = -CorridorRadius;
                    dy <= CorridorRadius;
                    dy++
                )
                {
                    int x =
                        centerX + dx;

                    int y =
                        centerY + dy;

                    if (
                        x < Border ||
                        y < Border ||
                        x >= Z.Width - Border ||
                        y >= Z.Height - Border
                    )
                    {
                        continue;
                    }

                    Cell cell =
                        Z.GetCell(x, y);

                    if (cell != null)
                        cell.Clear();
                }
            }
        }
    }

    /// <summary>
    /// Electrical Category 3, first structural pass.
    ///
    /// For now both buried structural mass and exposed interior walls
    /// become ordinary Fullcrete. Once the geometry is approved, the
    /// boundary material can be replaced with wired sqauare wave walls without
    /// changing Category 4 at all.
    /// </summary>
    public class SubterraneanSitesElectricalMaterials :
        ZoneBuilderSandbox
    {
        public string BulkWallBlueprint =
            "MetalWall";

        public string BoundaryWallBlueprint =
            "SubterraneanSitesElectricalSquareWave_CW";

        public bool BuildZone(Zone Z)
        {
            if (Z == null)
                return true;

            foreach (Cell cell in Z.GetCells())
            {
                if (cell == null)
                    continue;

                bool core =
                    SubterraneanSites
                        .SubterraneanSitesEPGeometry
                        .IsSolidPlaceholder(cell);

                bool boundary =
                    SubterraneanSites
                        .SubterraneanSitesEPGeometry
                        .IsBoundaryPlaceholder(cell);

                if (!core && !boundary)
                    continue;

                string material =
                    boundary
                        ? BoundaryWallBlueprint
                        : BulkWallBlueprint;

                cell.ClearWalls();

                if (!material.IsNullOrEmpty())
                    cell.AddObject(material);
            }

            return true;
        }
    }

    public class SubterraneanSitesElectricalMainframeRuns :
        ZoneBuilderSandbox
    {
        public string BoundaryBlueprint =
            "SubterraneanSitesElectricalSquareWave_CW";

        public string MonitorBlueprint =
            "SubterraneanSitesElectricalMainframeMonitor";

        public string StatusBlueprint =
            "SubterraneanSitesElectricalMainframeStatus";

        public int MinRuns = 8;
        public int MaxRuns = 14;

        public int MinRunLength = 2;
        public int MaxRunLength = 4;

        private sealed class WallRun
        {
            public List<Location2D> Cells =
                new List<Location2D>();
        }

        public bool BuildZone(Zone Z)
        {
            if (Z == null)
                return true;

            ClampSettings();

            int seed =
                XRLCore.Core.Game.GetWorldSeed(
                    "SubterraneanSites:ElectricalMainframes:" +
                    Z.ZoneID
                );

            System.Random rng =
                new System.Random(seed);

            //
            // Find straight continuous runs of the actual finished
            // Electrical boundary wall. Doors naturally break these
            // runs, which is exactly what we want.
            //
            List<WallRun> candidates =
                CollectStraightRuns(Z);

            if (candidates.Count == 0)
                return true;

            int desiredRuns =
                rng.Next(
                    MinRuns,
                    MaxRuns + 1
                );

            //
            // Horizontal and vertical candidates can intersect at
            // corners. Never replace the same cell twice.
            //
            HashSet<int> used =
                new HashSet<int>();

            int placedRuns = 0;
            int attempts = 0;

            while (
                placedRuns < desiredRuns &&
                attempts++ < 100
            )
            {
                WallRun candidate =
                    candidates[
                        rng.Next(candidates.Count)
                    ];

                if (
                    candidate == null ||
                    candidate.Cells == null ||
                    candidate.Cells.Count < MinRunLength
                )
                {
                    continue;
                }

                int maximumLength =
                    Math.Min(
                        MaxRunLength,
                        candidate.Cells.Count
                    );

                if (maximumLength < MinRunLength)
                    continue;

                int length =
                    rng.Next(
                        MinRunLength,
                        maximumLength + 1
                    );

                int start =
                    rng.Next(
                        0,
                        candidate.Cells.Count -
                        length +
                        1
                    );

                bool valid = true;

                for (
                    int i = 0;
                    i < length;
                    i++
                )
                {
                    Location2D location =
                        candidate.Cells[
                            start + i
                        ];

                    if (location == null)
                    {
                        valid = false;
                        break;
                    }

                    int key =
                        location.X +
                        location.Y * Z.Width;

                    if (used.Contains(key))
                    {
                        valid = false;
                        break;
                    }

                    Cell cell =
                        Z.GetCell(
                            location.X,
                            location.Y
                        );

                    if (
                        cell == null ||
                        !cell.HasObjectWithBlueprint(
                            BoundaryBlueprint
                        )
                    )
                    {
                        valid = false;
                        break;
                    }
                }

                if (!valid)
                    continue;

                //
                // One visual type for the whole installation.
                // This makes the run read as one equipment bank
                // instead of randomized wall confetti.
                //
                string replacementBlueprint =
                    rng.Next(2) == 0
                        ? MonitorBlueprint
                        : StatusBlueprint;

                bool replacedAny = false;

                for (
                    int i = 0;
                    i < length;
                    i++
                )
                {
                    Location2D location =
                        candidate.Cells[
                            start + i
                        ];

                    Cell cell =
                        Z.GetCell(
                            location.X,
                            location.Y
                        );

                    if (
                        ReplaceBoundaryWall(
                            cell,
                            replacementBlueprint
                        )
                    )
                    {
                        int key =
                            location.X +
                            location.Y * Z.Width;

                        used.Add(key);
                        replacedAny = true;
                    }
                }

                if (replacedAny)
                    placedRuns++;
            }

            return true;
        }

        private void ClampSettings()
        {
            if (MinRuns < 0)
                MinRuns = 0;

            if (MaxRuns < MinRuns)
                MaxRuns = MinRuns;

            if (MinRunLength < 1)
                MinRunLength = 1;

            if (MaxRunLength < MinRunLength)
                MaxRunLength = MinRunLength;
        }

       private List<WallRun> CollectStraightRuns(
            Zone Z
        )
        {
            List<WallRun> runs =
                new List<WallRun>();

            //
            // Mainframe artwork is most valuable on a top-facing
            // horizontal wall: solid structure behind it, with
            // traversable room/hall space immediately below it.
            //
            // We deliberately ignore:
            //   - vertical walls
            //   - bottom-facing horizontal walls
            //   - freestanding/thin divider walls
            //
            for (
                int y = 1;
                y < Z.Height - 1;
                y++
            )
            {
                int x = 0;

                while (x < Z.Width)
                {
                    while (
                        x < Z.Width &&
                        !IsTopFacingBoundaryWall(
                            Z,
                            x,
                            y
                        )
                    )
                    {
                        x++;
                    }

                    if (x >= Z.Width)
                        break;

                    WallRun run =
                        new WallRun();

                    while (
                        x < Z.Width &&
                        IsTopFacingBoundaryWall(
                            Z,
                            x,
                            y
                        )
                    )
                    {
                        run.Cells.Add(
                            Location2D.Get(
                                x,
                                y
                            )
                        );

                        x++;
                    }

                    if (
                        run.Cells.Count >=
                        MinRunLength
                    )
                    {
                        runs.Add(run);
                    }
                }
            }

            return runs;
        }

        private bool IsTopFacingBoundaryWall(
            Zone Z,
            int x,
            int y
        )
        {
            if (
                Z == null ||
                x < 0 ||
                x >= Z.Width ||
                y <= 0 ||
                y >= Z.Height - 1
            )
            {
                return false;
            }

            Cell wallCell =
                Z.GetCell(
                    x,
                    y
                );

            if (!IsBoundaryWall(wallCell))
                return false;

            Cell above =
                Z.GetCell(
                    x,
                    y - 1
                );

            Cell below =
                Z.GetCell(
                    x,
                    y + 1
                );

            if (
                above == null ||
                below == null
            )
            {
                return false;
            }

            //
            // We want the visible/front face of a structural wall:
            //
            //       solid backing
            //       MAINFRAME
            //       open interior
            //
            // Requiring solid backing also avoids using thin divider
            // walls where there is open space on both sides.
            //
            return
                above.IsSolid() &&
                !below.IsSolid();
        }

        private bool IsBoundaryWall(
            Cell cell
        )
        {
            return
                cell != null &&
                !BoundaryBlueprint.IsNullOrEmpty() &&
                cell.HasObjectWithBlueprint(
                    BoundaryBlueprint
                );
        }

        private bool ReplaceBoundaryWall(
            Cell cell,
            string replacementBlueprint
        )
        {
            if (
                cell == null ||
                replacementBlueprint.IsNullOrEmpty()
            )
            {
                return false;
            }

            List<GameObject> walls =
                cell.GetObjects(
                    BoundaryBlueprint
                );

            if (
                walls == null ||
                walls.Count == 0
            )
            {
                return false;
            }

            GameObject oldWall =
                walls[0];

            if (oldWall == null)
                return false;

            //
            // Remove exactly the square-wave wall we're replacing;
            // don't ClearWalls(), because there's no reason to
            // disturb anything else occupying this cell.
            //
            cell.RemoveObject(
                oldWall,
                true,
                true,
                Repaint: false
            );

            oldWall.Obliterate();

            cell.AddObject(
                replacementBlueprint
            );

            return true;
        }
    }

/// <summary>
/// Electrical Category 1 physical infrastructure.
///
/// Places the active technological installation:
///   - clustered electrical power producers
///   - broadcast-power installations
///   - powered machinery and chargers
///   - HeavyPowerLine backbone
///   - PowerLine machinery branches
///
/// This builder uses shared EP geometry rather than Electrical's own
/// Category-4 layout, so Electrical C1 remains compatible with any
/// shuffled topology.
///
/// The separate Electrical Category-5 builder owns disconnected
/// technological decoration and does not place power lines.
/// </summary>
    public class SubterraneanSitesElectricalInfrastructure :
        ZoneBuilderSandbox
    {

        //
        // When nonzero, this builder is being used as the origin-layer
        // C5 infastructure preview. All placement and electrical-network operations
        // must remain inside the entrance scar and outside the hole.
        //
        public int EntranceOnly = 0;

        public int MinSourceClusters = 2;
        public int MaxSourceClusters = 4;

        public int MinBroadcastStations = 1;
        public int MaxBroadcastStations = 2;

        public int MinLooseMachinery = 8;
        public int MaxLooseMachinery = 16;

        public int MinBroadcastExtras = 2;
        public int MaxBroadcastExtras = 5;

        public int ChargerClusterChance = 60;
        public int MinChargerExtras = 1;
        public int MaxChargerExtras = 2;

        public int TransitionExclusionRadius = 4;

        public int SourceClusterRadius = 2;
        public int BroadcastClusterRadius = 2;
        public int ChargerClusterRadius = 2;

        public int MajorClusterSeparationRadius = 4;

        public int ExtraNetworkConnections = 8;

        public int BranchSecondConnectionChance = 30;

        public int MaxWireRouteLength = 250;

        private const string ElectricGeneratorBlueprint =
            "Electric Generator";

        private const string FusionPowerStationBlueprint =
            "Fusion Power Station";

        private const string BroadcastPowerStationBlueprint =
            "Broadcast Power Station";

        private const string InductionChargingStationBlueprint =
            "Induction Charging Station";

        private const string UniversalChargingStationBlueprint =
            "Universal Charging Station";

        private const string PowerLineBlueprint =
            "PowerLine";

        private const string HeavyPowerLineBlueprint =
            "HeavyPowerLine";


        private sealed class WeightedBlueprint
        {
            public string Blueprint;
            public int Weight;
            public bool PreferWall;
            public bool Charger;
        }


        private readonly WeightedBlueprint[] MachineryPool =
        {
            new WeightedBlueprint
            {
                Blueprint = "Unicomputer",
                Weight = 6,
                PreferWall = true
            },

            new WeightedBlueprint
            {
                Blueprint = "Electrothing",
                Weight = 3,
                PreferWall = false
            },

            new WeightedBlueprint
            {
                Blueprint = "Fluxthing",
                Weight = 3,
                PreferWall = false
            },

            new WeightedBlueprint
            {
                Blueprint = "Induction Charging Station",
                Weight = 2,
                PreferWall = true,
                Charger = true
            },

            new WeightedBlueprint
            {
                Blueprint = "Universal Charging Station",
                Weight = 2,
                PreferWall = true,
                Charger = true
            },

            new WeightedBlueprint
            {
                Blueprint = "Loudspeaker",
                Weight = 2,
                PreferWall = true
            },

            new WeightedBlueprint
            {
                Blueprint = "Wire Extruder",
                Weight = 1,
                PreferWall = false
            }
        };


        public bool BuildZone(Zone Z)
        {
            if (Z == null)
                return true;

            if (EntranceOnly != 0)
            {
                ApplyEntranceDefaults();
            }

            ClampSettings();

            int seed =
                XRLCore.Core.Game.GetWorldSeed(
                    "SubterraneanSites:ElectricalInfrastructure:" +
                    Z.ZoneID
                );

            System.Random rng =
                new System.Random(seed);

            bool[,] reserved =
                new bool[Z.Width, Z.Height];

            List<Location2D> anchors =
                SubterraneanSites
                    .SubterraneanSitesEPVerticalTransitions
                    .GetVerticalAnchors(
                        Z.ZoneID
                    );

            SubterraneanSites
                .SubterraneanSitesEPPlacement
                .ReserveAroundAnchors(
                    reserved,
                    Z,
                    anchors,
                    TransitionExclusionRadius
                );

            //
            // Major machinery gets first choice of the broadest available
            // interior spaces.
            //
            PlaceSourceClusters(
                Z,
                reserved,
                rng
            );

            PlaceBroadcastClusters(
                Z,
                reserved,
                rng
            );

            //
            // Then distribute miscellaneous consumers/infrastructure.
            //
            PlaceLooseMachinery(
                Z,
                reserved,
                rng
            );

            //
            // Stage 2: connect the successfully placed electrical machinery
            // with Qud's native conduit objects.
            //
            PlaceWireNetwork(
                Z,
                rng
            );

            return true;
        }

        private void ApplyEntranceDefaults()
        {
            //
            // The entrance scar is much smaller than a full EP layer.
            // Give it a recognizable Electrical C5 installation without
            // trying to squeeze full-level machinery density into the scar.
            //
            MinSourceClusters = 2;
            MaxSourceClusters = 3;

            MinBroadcastStations = 1;
            MaxBroadcastStations = 1;

            MinLooseMachinery = 6;
            MaxLooseMachinery = 10;

            MinBroadcastExtras = 1;
            MaxBroadcastExtras = 2;

            ExtraNetworkConnections = 2;
        }


        private void ClampSettings()
        {
            MinSourceClusters =
                Math.Max(
                    0,
                    MinSourceClusters
                );

            MaxSourceClusters =
                Math.Max(
                    MinSourceClusters,
                    MaxSourceClusters
                );

            MinBroadcastStations =
                Math.Max(
                    0,
                    MinBroadcastStations
                );

            MaxBroadcastStations =
                Math.Max(
                    MinBroadcastStations,
                    MaxBroadcastStations
                );

            MinLooseMachinery =
                Math.Max(
                    0,
                    MinLooseMachinery
                );

            MaxLooseMachinery =
                Math.Max(
                    MinLooseMachinery,
                    MaxLooseMachinery
                );

            MinBroadcastExtras =
                Math.Max(
                    0,
                    MinBroadcastExtras
                );

            MaxBroadcastExtras =
                Math.Max(
                    MinBroadcastExtras,
                    MaxBroadcastExtras
                );

            ChargerClusterChance =
                Math.Max(
                    0,
                    Math.Min(
                        100,
                        ChargerClusterChance
                    )
                );

            MinChargerExtras =
                Math.Max(
                    0,
                    MinChargerExtras
                );

            MaxChargerExtras =
                Math.Max(
                    MinChargerExtras,
                    MaxChargerExtras
                );

            TransitionExclusionRadius =
                Math.Max(
                    0,
                    TransitionExclusionRadius
                );

            SourceClusterRadius =
                Math.Max(
                    1,
                    SourceClusterRadius
                );

            BroadcastClusterRadius =
                Math.Max(
                    1,
                    BroadcastClusterRadius
                );

            ChargerClusterRadius =
                Math.Max(
                    1,
                    ChargerClusterRadius
                );

            MajorClusterSeparationRadius =
                Math.Max(
                    0,
                    MajorClusterSeparationRadius
                );

        }


        // ================================================================
        // POWER-SOURCE CLUSTERS
        // ================================================================

        private void PlaceSourceClusters(
            Zone Z,
            bool[,] reserved,
            System.Random rng
        )
        {
            int clusterCount =
                rng.Next(
                    MinSourceClusters,
                    MaxSourceClusters + 1
                );

            for (
                int clusterIndex = 0;
                clusterIndex < clusterCount;
                clusterIndex++
            )
            {
                Cell center =
                    PickBroadInteriorCell(
                        Z,
                        reserved,
                        rng
                    );

                if (center == null)
                    break;

                bool wantsFour =
                    rng.Next(100) >= 60;

                List<Cell> pattern =
                    null;

                //
                // Four-source installations must be a coherent 2x2 block.
                //
                if (wantsFour)
                {
                    pattern =
                        FindSourceSquare(
                            Z,
                            reserved,
                            center,
                            rng
                        );
                }

                //
                // Either this was intended as a two-source bank,
                // or the four-source square could not fit.
                //
                if (
                    pattern == null ||
                    pattern.Count == 0
                )
                {
                    pattern =
                        FindSourceLine(
                            Z,
                            reserved,
                            center,
                            rng
                        );
                }

                if (
                    pattern == null ||
                    pattern.Count == 0
                )
                {
                    //
                    // Do not reserve the failed seed area. Another cluster
                    // or later machinery may still be able to use it.
                    //
                    continue;
                }

                foreach (
                    Cell sourceCell
                    in pattern
                )
                {
                    if (
                        sourceCell == null
                    )
                    {
                        continue;
                    }

                    if (
                        TryPlacePowerSource(
                            sourceCell,
                            rng
                        )
                    )
                    {
                        reserved[
                            sourceCell.X,
                            sourceCell.Y
                        ] = true;
                    }
                }

                //
                // Reserve around the installation as a whole so another
                // major cluster does not immediately crowd it.
                //
                Cell reserveCenter =
                    pattern[
                        rng.Next(
                            pattern.Count
                        )
                    ];

                ReserveAroundCell(
                    reserved,
                    Z,
                    reserveCenter,
                    MajorClusterSeparationRadius
                );
            }
        }

        private List<Cell> FindSourceLine(
            Zone Z,
            bool[,] reserved,
            Cell center,
            System.Random rng
        )
        {
            if (
                Z == null ||
                reserved == null ||
                center == null ||
                rng == null
            )
            {
                return null;
            }

            //
            // Four possible two-cell lines containing the selected seed.
            // Shuffle the order so horizontal and vertical installations
            // both naturally occur.
            //
            int[][] directions =
            {
                new int[] {  1,  0 },
                new int[] { -1,  0 },
                new int[] {  0,  1 },
                new int[] {  0, -1 }
            };

            ShuffleDirections(
                directions,
                rng
            );

            foreach (
                int[] direction
                in directions
            )
            {
                Cell second =
                    Z.GetCell(
                        center.X +
                            direction[0],
                        center.Y +
                            direction[1]
                    );

                if (
                    IsAvailableOpenCell(
                        Z,
                        reserved,
                        center
                    ) &&
                    IsAvailableOpenCell(
                        Z,
                        reserved,
                        second
                    )
                )
                {
                    return
                        new List<Cell>
                        {
                            center,
                            second
                        };
                }
            }

            return null;
        }


        private List<Cell> FindSourceSquare(
            Zone Z,
            bool[,] reserved,
            Cell center,
            System.Random rng
        )
        {
            if (
                Z == null ||
                reserved == null ||
                center == null ||
                rng == null
            )
            {
                return null;
            }

            //
            // A seed cell can occupy any of the four corners of a 2x2 block.
            //
            int[][] offsets =
            {
                new int[] {  0,  0 },
                new int[] { -1,  0 },
                new int[] {  0, -1 },
                new int[] { -1, -1 }
            };

            ShuffleDirections(
                offsets,
                rng
            );

            foreach (
                int[] offset
                in offsets
            )
            {
                int x =
                    center.X +
                    offset[0];

                int y =
                    center.Y +
                    offset[1];

                Cell c00 =
                    Z.GetCell(
                        x,
                        y
                    );

                Cell c10 =
                    Z.GetCell(
                        x + 1,
                        y
                    );

                Cell c01 =
                    Z.GetCell(
                        x,
                        y + 1
                    );

                Cell c11 =
                    Z.GetCell(
                        x + 1,
                        y + 1
                    );

                if (
                    IsAvailableOpenCell(
                        Z,
                        reserved,
                        c00
                    ) &&
                    IsAvailableOpenCell(
                        Z,
                        reserved,
                        c10
                    ) &&
                    IsAvailableOpenCell(
                        Z,
                        reserved,
                        c01
                    ) &&
                    IsAvailableOpenCell(
                        Z,
                        reserved,
                        c11
                    )
                )
                {
                    return
                        new List<Cell>
                        {
                            c00,
                            c10,
                            c01,
                            c11
                        };
                }
            }

            return null;
        }

        private void ShuffleDirections(
            int[][] values,
            System.Random rng
        )
        {
            if (
                values == null ||
                rng == null
            )
            {
                return;
            }

            for (
                int i =
                    values.Length - 1;
                i > 0;
                i--
            )
            {
                int j =
                    rng.Next(
                        i + 1
                    );

                int[] temp =
                    values[i];

                values[i] =
                    values[j];

                values[j] =
                    temp;
            }
        }


        private bool TryPlacePowerSource(
            Cell cell,
            System.Random rng
        )
        {
            if (
                cell == null ||
                rng == null
            )
            {
                return false;
            }

            string blueprint =
                rng.Next(2) == 0
                    ? ElectricGeneratorBlueprint
                    : FusionPowerStationBlueprint;

            return
                TryAddObject(
                    cell,
                    blueprint
                );
        }


        // ================================================================
        // BROADCAST CLUSTERS
        // ================================================================

        private void PlaceBroadcastClusters(
            Zone Z,
            bool[,] reserved,
            System.Random rng
        )
        {
            int broadcastCount =
                rng.Next(
                    MinBroadcastStations,
                    MaxBroadcastStations + 1
                );

            for (
                int i = 0;
                i < broadcastCount;
                i++
            )
            {
                Cell center =
                    PickBroadInteriorCell(
                        Z,
                        reserved,
                        rng
                    );

                if (center == null)
                    break;

                if (
                    !TryAddObject(
                        center,
                        BroadcastPowerStationBlueprint
                    )
                )
                {
                    reserved[
                        center.X,
                        center.Y
                    ] = true;

                    continue;
                }

                reserved[
                    center.X,
                    center.Y
                ] = true;

                int extraCount =
                    rng.Next(
                        MinBroadcastExtras,
                        MaxBroadcastExtras + 1
                    );

                for (
                    int extra = 0;
                    extra < extraCount;
                    extra++
                )
                {
                    WeightedBlueprint spec =
                        RollMachinery(
                            rng
                        );

                    if (spec == null)
                        continue;

                    Cell destination =
                        PickNearbyAvailableCell(
                            Z,
                            reserved,
                            center,
                            BroadcastClusterRadius,
                            rng,
                            spec.PreferWall
                        );

                    if (destination == null)
                        continue;

                    if (
                        TryAddObject(
                            destination,
                            spec.Blueprint
                        )
                    )
                    {
                        reserved[
                            destination.X,
                            destination.Y
                        ] = true;
                    }
                }

                ReserveAroundCell(
                    reserved,
                    Z,
                    center,
                    MajorClusterSeparationRadius
                );
            }
        }


        // ================================================================
        // LOOSE MACHINERY + CHARGER MINI-CLUSTERS
        // ================================================================

        private void PlaceLooseMachinery(
            Zone Z,
            bool[,] reserved,
            System.Random rng
        )
        {
            int count =
                rng.Next(
                    MinLooseMachinery,
                    MaxLooseMachinery + 1
                );

            for (
                int i = 0;
                i < count;
                i++
            )
            {
                WeightedBlueprint spec =
                    RollMachinery(
                        rng
                    );

                if (spec == null)
                    continue;

                Cell destination =
                    PickMachineryCell(
                        Z,
                        reserved,
                        rng,
                        spec.PreferWall
                    );

                if (destination == null)
                    continue;

                if (
                    !TryAddObject(
                        destination,
                        spec.Blueprint
                    )
                )
                {
                    continue;
                }

                reserved[
                    destination.X,
                    destination.Y
                ] = true;

                //
                // Chargers remain part of the general random pool, but a
                // subset become local equipment centers with a few nearby
                // miscellaneous devices.
                //
                if (
                    spec.Charger &&
                    rng.Next(100) <
                        ChargerClusterChance
                )
                {
                    PlaceChargerExtras(
                        Z,
                        reserved,
                        destination,
                        rng
                    );
                }
            }
        }


        private void PlaceChargerExtras(
            Zone Z,
            bool[,] reserved,
            Cell charger,
            System.Random rng
        )
        {
            if (
                Z == null ||
                charger == null ||
                rng == null
            )
            {
                return;
            }

            int extraCount =
                rng.Next(
                    MinChargerExtras,
                    MaxChargerExtras + 1
                );

            for (
                int i = 0;
                i < extraCount;
                i++
            )
            {
                WeightedBlueprint spec =
                    RollMachinery(
                        rng
                    );

                if (spec == null)
                    continue;

                //
                // Do not recursively turn a charger cluster into another
                // charger cluster. The object can still happen to be a charger;
                // it just does not trigger another expansion here.
                //
                Cell destination =
                    PickNearbyAvailableCell(
                        Z,
                        reserved,
                        charger,
                        ChargerClusterRadius,
                        rng,
                        spec.PreferWall
                    );

                if (destination == null)
                    continue;

                if (
                    TryAddObject(
                        destination,
                        spec.Blueprint
                    )
                )
                {
                    reserved[
                        destination.X,
                        destination.Y
                    ] = true;
                }
            }
        }


        private WeightedBlueprint RollMachinery(
            System.Random rng
        )
        {
            if (
                rng == null ||
                MachineryPool == null ||
                MachineryPool.Length == 0
            )
            {
                return null;
            }

            int totalWeight = 0;

            foreach (
                WeightedBlueprint entry
                in MachineryPool
            )
            {
                if (
                    entry != null &&
                    entry.Weight > 0
                )
                {
                    totalWeight +=
                        entry.Weight;
                }
            }

            if (totalWeight <= 0)
                return null;

            int roll =
                rng.Next(
                    totalWeight
                );

            foreach (
                WeightedBlueprint entry
                in MachineryPool
            )
            {
                if (
                    entry == null ||
                    entry.Weight <= 0
                )
                {
                    continue;
                }

                if (roll < entry.Weight)
                    return entry;

                roll -=
                    entry.Weight;
            }

            return MachineryPool[0];
        }

    

        // ================================================================
        // ELECTRICAL NETWORK
        // ================================================================

        private sealed class ElectricalNode
        {
            public int X;
            public int Y;
            public bool Major;
        }


        private void PlaceWireNetwork(
            Zone Z,
            System.Random rng
        )
        {
            if (
                Z == null ||
                rng == null
            )
            {
                return;
            }

            List<ElectricalNode> majorNodes =
                new List<ElectricalNode>();

            List<ElectricalNode> branchNodes =
                new List<ElectricalNode>();

            CollectElectricalNodes(
                Z,
                majorNodes,
                branchNodes
            );

            if (
                majorNodes.Count == 0 &&
                branchNodes.Count == 0
            )
            {
                return;
            }

            //
            // The heavy backbone is built first. Later normal branches can
            // naturally reuse these cells instead of painting over them.
            //
            BuildMajorBackbone(
                Z,
                majorNodes,
                rng
            );

            //
            // Grow ordinary machinery outward from the already connected
            // network. Shuffling the machinery first prevents one consistent
            // spatial direction from dominating the result.
            //
            ShuffleNodes(
                branchNodes,
                rng
            );

            List<ElectricalNode> connected =
                new List<ElectricalNode>();

            connected.AddRange(
                majorNodes
            );

            if (
                connected.Count == 0 &&
                branchNodes.Count > 0
            )
            {
                connected.Add(
                    branchNodes[0]
                );

                branchNodes.RemoveAt(0);
            }

            foreach (
                ElectricalNode node
                in branchNodes
            )
            {
                if (node == null)
                    continue;

                ElectricalNode nearest =
                    FindNearestNode(
                        node,
                        connected
                    );

                if (nearest != null)
                {
                    ConnectElectricalNodes(
                        Z,
                        node,
                        nearest,
                        PowerLineBlueprint,
                        rng
                    );
                }

                //
                // Some machinery receives a second feed. This creates loops,
                // irregular branches, and redundant paths without requiring
                // an actual circuit-planning system.
                //
                if (
                    connected.Count > 1 &&
                    rng.Next(100) <
                        BranchSecondConnectionChance
                )
                {
                    ElectricalNode second =
                        FindNearestNode(
                            node,
                            connected,
                            nearest
                        );

                    if (second != null)
                    {
                        ConnectElectricalNodes(
                            Z,
                            node,
                            second,
                            PowerLineBlueprint,
                            rng
                        );
                    }
                }

                connected.Add(
                    node
                );
            }

            //
            // Finally make the whole thing less tree-like.
            //
            AddExtraNetworkConnections(
                Z,
                connected,
                rng
            );
        }


        private void CollectElectricalNodes(
            Zone Z,
            List<ElectricalNode> majorNodes,
            List<ElectricalNode> branchNodes
        )
        {
            if (
                Z == null ||
                majorNodes == null ||
                branchNodes == null
            )
            {
                return;
            }

            foreach (
                Cell cell
                in Z.GetCells()
            )
            {
                if (cell == null)
                    continue;

                if (
                    !CellIsInsideDecorationScope(
                        Z,
                        cell
                    )
                )
                {
                    continue;
                }

                //
                // Power producers and broadcast stations are the major
                // infrastructure that forms the HeavyPowerLine backbone.
                //
                if (
                    cell.HasObjectWithBlueprint(
                        ElectricGeneratorBlueprint
                    ) ||
                    cell.HasObjectWithBlueprint(
                        FusionPowerStationBlueprint
                    ) ||
                    cell.HasObjectWithBlueprint(
                        BroadcastPowerStationBlueprint
                    )
                )
                {
                    majorNodes.Add(
                        new ElectricalNode
                        {
                            X = cell.X,
                            Y = cell.Y,
                            Major = true
                        }
                    );

                    continue;
                }

                //
                // Only actual powered machinery belongs to the network.
                // Clockthing is intentionally absent because the base vanilla
                // Clockthing does not consume electrical power.
                //
                if (
                    cell.HasObjectWithBlueprint(
                        "Unicomputer"
                    ) ||
                    cell.HasObjectWithBlueprint(
                        "Electrothing"
                    ) ||
                    cell.HasObjectWithBlueprint(
                        "Fluxthing"
                    ) ||
                    cell.HasObjectWithBlueprint(
                        InductionChargingStationBlueprint
                    ) ||
                    cell.HasObjectWithBlueprint(
                        UniversalChargingStationBlueprint
                    ) ||
                    cell.HasObjectWithBlueprint(
                        "Loudspeaker"
                    ) ||
                    cell.HasObjectWithBlueprint(
                        "Wire Extruder"
                    )
                )
                {
                    branchNodes.Add(
                        new ElectricalNode
                        {
                            X = cell.X,
                            Y = cell.Y,
                            Major = false
                        }
                    );
                }
            }
        }


        // ================================================================
        // HEAVY BACKBONE
        // ================================================================

        private void BuildMajorBackbone(
            Zone Z,
            List<ElectricalNode> majorNodes,
            System.Random rng
        )
        {
            if (
                Z == null ||
                majorNodes == null ||
                majorNodes.Count < 2 ||
                rng == null
            )
            {
                return;
            }

            List<ElectricalNode> connected =
                new List<ElectricalNode>();

            List<ElectricalNode> remaining =
                new List<ElectricalNode>(
                    majorNodes
                );

            int firstIndex =
                rng.Next(
                    remaining.Count
                );

            connected.Add(
                remaining[
                    firstIndex
                ]
            );

            remaining.RemoveAt(
                firstIndex
            );

            //
            // Simple nearest-neighbor spanning tree.
            //
            while (
                remaining.Count > 0
            )
            {
                ElectricalNode bestConnected =
                    null;

                ElectricalNode bestRemaining =
                    null;

                int bestDistance =
                    int.MaxValue;

                foreach (
                    ElectricalNode from
                    in connected
                )
                {
                    foreach (
                        ElectricalNode to
                        in remaining
                    )
                    {
                        int distance =
                            DistanceSquared(
                                from,
                                to
                            );

                        if (
                            distance <
                            bestDistance
                        )
                        {
                            bestDistance =
                                distance;

                            bestConnected =
                                from;

                            bestRemaining =
                                to;
                        }
                    }
                }

                if (
                    bestConnected == null ||
                    bestRemaining == null
                )
                {
                    break;
                }

                ConnectElectricalNodes(
                    Z,
                    bestConnected,
                    bestRemaining,
                    HeavyPowerLineBlueprint,
                    rng
                );

                connected.Add(
                    bestRemaining
                );

                remaining.Remove(
                    bestRemaining
                );
            }
        }


        // ================================================================
        // EXTRA CONNECTIONS
        // ================================================================

        private void AddExtraNetworkConnections(
            Zone Z,
            List<ElectricalNode> nodes,
            System.Random rng
        )
        {
            if (
                Z == null ||
                nodes == null ||
                nodes.Count < 2 ||
                rng == null
            )
            {
                return;
            }

            int desired =
                Math.Max(
                    0,
                    ExtraNetworkConnections
                );

            int attempts =
                0;

            int placed =
                0;

            while (
                placed < desired &&
                attempts++ <
                    desired * 10 + 20
            )
            {
                ElectricalNode a =
                    nodes[
                        rng.Next(
                            nodes.Count
                        )
                    ];

                ElectricalNode b =
                    nodes[
                        rng.Next(
                            nodes.Count
                        )
                    ];

                if (
                    a == null ||
                    b == null ||
                    ReferenceEquals(a, b)
                )
                {
                    continue;
                }

                //
                // Major-to-major redundant connections become heavy trunks.
                // Everything else uses ordinary PowerLine.
                //
                string blueprint =
                    a.Major &&
                    b.Major
                        ? HeavyPowerLineBlueprint
                        : PowerLineBlueprint;

                if (
                    ConnectElectricalNodes(
                        Z,
                        a,
                        b,
                        blueprint,
                        rng
                    )
                )
                {
                    placed++;
                }
            }
        }


        // ================================================================
        // NODE CONNECTION
        // ================================================================

        private bool ConnectElectricalNodes(
            Zone Z,
            ElectricalNode a,
            ElectricalNode b,
            string wireBlueprint,
            System.Random rng
        )
        {
            if (
                Z == null ||
                a == null ||
                b == null ||
                wireBlueprint.IsNullOrEmpty() ||
                rng == null
            )
            {
                return false;
            }

            Location2D start =
                null;

            Location2D end =
                null;

            if (
                !FindBestEndpointPair(
                    Z,
                    a,
                    b,
                    out start,
                    out end,
                    rng
                )
            )
            {
                return false;
            }

            List<Location2D> path =
                FindWirePath(
                    Z,
                    start,
                    end,
                    rng
                );

            if (
                path == null ||
                path.Count == 0
            )
            {
                return false;
            }

            if (
                MaxWireRouteLength > 0 &&
                path.Count >
                    MaxWireRouteLength
            )
            {
                return false;
            }

            bool placedAny =
                false;

            foreach (
                Location2D location
                in path
            )
            {
                if (location == null)
                    continue;

                Cell cell =
                    Z.GetCell(
                        location.X,
                        location.Y
                    );

                if (
                    TryPlaceWire(
                        cell,
                        wireBlueprint
                    )
                )
                {
                    placedAny =
                        true;
                }
            }

            return placedAny;
        }


        // ================================================================
        // CONNECTION ENDPOINTS
        // ================================================================

        private bool FindBestEndpointPair(
            Zone Z,
            ElectricalNode a,
            ElectricalNode b,
            out Location2D bestStart,
            out Location2D bestEnd,
            System.Random rng
        )
        {
            bestStart =
                null;

            bestEnd =
                null;

            if (
                Z == null ||
                a == null ||
                b == null
            )
            {
                return false;
            }

            List<Location2D> aNeighbors =
                CollectWireEndpointNeighbors(
                    Z,
                    a.X,
                    a.Y
                );

            List<Location2D> bNeighbors =
                CollectWireEndpointNeighbors(
                    Z,
                    b.X,
                    b.Y
                );

            if (
                aNeighbors.Count == 0 ||
                bNeighbors.Count == 0
            )
            {
                return false;
            }

            int bestDistance =
                int.MaxValue;

            List<Location2D[]> bestPairs =
                new List<Location2D[]>();

            foreach (
                Location2D start
                in aNeighbors
            )
            {
                foreach (
                    Location2D end
                    in bNeighbors
                )
                {
                    int distance =
                        Math.Abs(
                            start.X -
                            end.X
                        ) +
                        Math.Abs(
                            start.Y -
                            end.Y
                        );

                    if (
                        distance <
                        bestDistance
                    )
                    {
                        bestDistance =
                            distance;

                        bestPairs.Clear();

                        bestPairs.Add(
                            new Location2D[]
                            {
                                start,
                                end
                            }
                        );
                    }
                    else if (
                        distance ==
                            bestDistance
                    )
                    {
                        bestPairs.Add(
                            new Location2D[]
                            {
                                start,
                                end
                            }
                        );
                    }
                }
            }

            if (
                bestPairs.Count == 0
            )
            {
                return false;
            }

            Location2D[] chosen =
                bestPairs[
                    rng.Next(
                        bestPairs.Count
                    )
                ];

            bestStart =
                chosen[0];

            bestEnd =
                chosen[1];

            return true;
        }


        private List<Location2D> CollectWireEndpointNeighbors(
            Zone Z,
            int x,
            int y
        )
        {
            List<Location2D> result =
                new List<Location2D>();

            TryAddWireEndpoint(
                Z,
                result,
                x,
                y - 1
            );

            TryAddWireEndpoint(
                Z,
                result,
                x + 1,
                y
            );

            TryAddWireEndpoint(
                Z,
                result,
                x,
                y + 1
            );

            TryAddWireEndpoint(
                Z,
                result,
                x - 1,
                y
            );

            return result;
        }


        private void TryAddWireEndpoint(
            Zone Z,
            List<Location2D> result,
            int x,
            int y
        )
        {
            if (
                Z == null ||
                result == null
            )
            {
                return;
            }

            Cell cell =
                Z.GetCell(
                    x,
                    y
                );

            if (
                IsWirePathCell(
                    Z,
                    cell
                )
            )
            {
                result.Add(
                    Location2D.Get(
                        x,
                        y
                    )
                );
            }
        }


        // ================================================================
        // PATHFINDING
        // ================================================================

        private List<Location2D> FindWirePath(
            Zone Z,
            Location2D start,
            Location2D end,
            System.Random rng
        )
        {
            if (
                Z == null ||
                start == null ||
                end == null ||
                rng == null
            )
            {
                return null;
            }

            bool[,] visited =
                new bool[
                    Z.Width,
                    Z.Height
                ];

            int[,] parentX =
                new int[
                    Z.Width,
                    Z.Height
                ];

            int[,] parentY =
                new int[
                    Z.Width,
                    Z.Height
                ];

            for (
                int x = 0;
                x < Z.Width;
                x++
            )
            {
                for (
                    int y = 0;
                    y < Z.Height;
                    y++
                )
                {
                    parentX[
                        x,
                        y
                    ] = -1;

                    parentY[
                        x,
                        y
                    ] = -1;
                }
            }

            Queue<Location2D> queue =
                new Queue<Location2D>();

            queue.Enqueue(
                start
            );

            visited[
                start.X,
                start.Y
            ] = true;

            bool found =
                false;

            while (
                queue.Count > 0
            )
            {
                Location2D current =
                    queue.Dequeue();

                if (
                    current.X == end.X &&
                    current.Y == end.Y
                )
                {
                    found =
                        true;

                    break;
                }

                //
                // The order changes per expansion. BFS still favors short
                // routes, but equally short alternatives do not all resolve
                // into the same repeated geometric pattern.
                //
                int[][] directions =
                {
                    new int[] {  1,  0 },
                    new int[] { -1,  0 },
                    new int[] {  0,  1 },
                    new int[] {  0, -1 }
                };

                ShuffleDirections(
                    directions,
                    rng
                );

                foreach (
                    int[] direction
                    in directions
                )
                {
                    int nx =
                        current.X +
                        direction[0];

                    int ny =
                        current.Y +
                        direction[1];

                    if (
                        nx < 0 ||
                        ny < 0 ||
                        nx >= Z.Width ||
                        ny >= Z.Height
                    )
                    {
                        continue;
                    }

                    if (
                        visited[
                            nx,
                            ny
                        ]
                    )
                    {
                        continue;
                    }

                    Cell next =
                        Z.GetCell(
                            nx,
                            ny
                        );

                    if (
                        !IsWirePathCell(
                            Z,
                            next
                        )
                    )
                    {
                        continue;
                    }

                    visited[
                        nx,
                        ny
                    ] = true;

                    parentX[
                        nx,
                        ny
                    ] = current.X;

                    parentY[
                        nx,
                        ny
                    ] = current.Y;

                    queue.Enqueue(
                        Location2D.Get(
                            nx,
                            ny
                        )
                    );
                }
            }

            if (!found)
                return null;

            List<Location2D> path =
                new List<Location2D>();

            int px =
                end.X;

            int py =
                end.Y;

            while (true)
            {
                path.Add(
                    Location2D.Get(
                        px,
                        py
                    )
                );

                if (
                    px == start.X &&
                    py == start.Y
                )
                {
                    break;
                }

                int nextX =
                    parentX[
                        px,
                        py
                    ];

                int nextY =
                    parentY[
                        px,
                        py
                    ];

                if (
                    nextX < 0 ||
                    nextY < 0
                )
                {
                    return null;
                }

                px =
                    nextX;

                py =
                    nextY;
            }

            path.Reverse();

            return path;
        }


        private bool IsWirePathCell(
            Zone Z,
            Cell cell
        )
        {
            if (
                Z == null ||
                cell == null
            )
            {
                return false;
            }

            if (
                !CellIsInsideDecorationScope(
                    Z,
                    cell
                )
            )
            {
                return false;
            }

            //
            // Existing wire is always valid routing space. This is what lets
            // later branches reuse trunks and causes the network to coalesce
            // into a lattice.
            //
            if (
                HasAnyPowerLine(
                    cell
                )
            )
            {
                return true;
            }

            //
            // Existing C1 conduit was handled above and remains reusable.
            //
            // Otherwise, do not route new conduit through space already owned by
            // higher-priority EP content.
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

            //
            // C5 operates against the shared semantic geometry, so this remains
            // compatible with whatever theme supplied Category 4.
            //
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
                !cell.IsEmptyOfSolid()
            )
            {
                return false;
            }

            //
            // Conduit may run directly through spills.
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

            //
            // Never route directly through another electrical machine.
            // The path should terminate beside it instead.
            //
            if (
                IsElectricalMachineCell(
                    cell
                )
            )
            {
                return false;
            }

            return true;
        }


        // ================================================================
        // WIRE PLACEMENT
        // ================================================================

        private bool TryPlaceWire(
            Cell cell,
            string wireBlueprint
        )
        {
            if (
                cell == null ||
                wireBlueprint.IsNullOrEmpty()
            )
            {
                return false;
            }

            if (
                HasAnyPowerLine(
                    cell
                )
            )
            {
                //
                // The network may reuse an existing trunk, but reused C1 conduit is
                // still part of the C1 installation and therefore owns this cell.
                //
                SubterraneanSites
                    .SubterraneanSitesEPReservations
                    .ClaimCell(
                        cell.ParentZone,
                        cell
                    );

                return true;
            }

            GameObject wire =
                GameObjectFactory.Factory
                    .CreateObject(
                        wireBlueprint
                    );

            if (wire == null)
                return false;

            cell.AddObject(
                wire
            );

            SubterraneanSites
                .SubterraneanSitesEPReservations
                .ClaimCell(
                    cell.ParentZone,
                    cell
                );

            return true;
        }


        private bool HasAnyPowerLine(
            Cell cell
        )
        {
            if (cell == null)
                return false;

            return
                cell.HasObjectWithBlueprint(
                    PowerLineBlueprint
                ) ||
                cell.HasObjectWithBlueprint(
                    HeavyPowerLineBlueprint
                );
        }


        // ================================================================
        // MACHINE IDENTIFICATION
        // ================================================================

        private bool IsElectricalMachineCell(
            Cell cell
        )
        {
            if (cell == null)
                return false;

            return
                cell.HasObjectWithBlueprint(
                    ElectricGeneratorBlueprint
                ) ||
                cell.HasObjectWithBlueprint(
                    FusionPowerStationBlueprint
                ) ||
                cell.HasObjectWithBlueprint(
                    BroadcastPowerStationBlueprint
                ) ||
                cell.HasObjectWithBlueprint(
                    "Unicomputer"
                ) ||
                cell.HasObjectWithBlueprint(
                    "Electrothing"
                ) ||
                cell.HasObjectWithBlueprint(
                    "Fluxthing"
                ) ||
                cell.HasObjectWithBlueprint(
                    InductionChargingStationBlueprint
                ) ||
                cell.HasObjectWithBlueprint(
                    UniversalChargingStationBlueprint
                ) ||
                cell.HasObjectWithBlueprint(
                    "Loudspeaker"
                ) ||
                cell.HasObjectWithBlueprint(
                    "Wire Extruder"
                );
        }


        // ================================================================
        // NODE HELPERS
        // ================================================================

        private ElectricalNode FindNearestNode(
            ElectricalNode source,
            List<ElectricalNode> candidates,
            ElectricalNode exclude = null
        )
        {
            if (
                source == null ||
                candidates == null ||
                candidates.Count == 0
            )
            {
                return null;
            }

            ElectricalNode best =
                null;

            int bestDistance =
                int.MaxValue;

            foreach (
                ElectricalNode candidate
                in candidates
            )
            {
                if (
                    candidate == null ||
                    ReferenceEquals(
                        candidate,
                        source
                    ) ||
                    ReferenceEquals(
                        candidate,
                        exclude
                    )
                )
                {
                    continue;
                }

                int distance =
                    DistanceSquared(
                        source,
                        candidate
                    );

                if (
                    distance <
                    bestDistance
                )
                {
                    bestDistance =
                        distance;

                    best =
                        candidate;
                }
            }

            return best;
        }


        private int DistanceSquared(
            ElectricalNode a,
            ElectricalNode b
        )
        {
            if (
                a == null ||
                b == null
            )
            {
                return int.MaxValue;
            }

            int dx =
                a.X -
                b.X;

            int dy =
                a.Y -
                b.Y;

            return
                dx * dx +
                dy * dy;
        }


        private void ShuffleNodes(
            List<ElectricalNode> nodes,
            System.Random rng
        )
        {
            if (
                nodes == null ||
                rng == null
            )
            {
                return;
            }

            for (
                int i =
                    nodes.Count - 1;
                i > 0;
                i--
            )
            {
                int j =
                    rng.Next(
                        i + 1
                    );

                ElectricalNode temp =
                    nodes[i];

                nodes[i] =
                    nodes[j];

                nodes[j] =
                    temp;
            }
        }

        // ================================================================
        // CELL SELECTION
        // ================================================================

        private bool CellIsInsideDecorationScope(
            Zone Z,
            Cell cell
        )
        {
            if (cell == null)
                return false;

            if (EntranceOnly == 0)
                return true;

            return
                SubterraneanSites
                    .SubterraneanSitesEPEntrance
                    .IsInsideScar(
                        Z,
                        cell
                    ) &&
                !SubterraneanSites
                    .SubterraneanSitesEPEntrance
                    .IsInsideHoleExclusion(
                        Z,
                        cell,
                        1
                    );
        }

        private Cell PickBroadInteriorCell(
            Zone Z,
            bool[,] reserved,
            System.Random rng
        )
        {
            List<Cell> candidates =
                new List<Cell>();

            int bestScore =
                -1;

            foreach (
                Cell cell
                in Z.GetCells()
            )
            {
                if (
                    !IsAvailableOpenCell(
                        Z,
                        reserved,
                        cell
                    )
                )
                {
                    continue;
                }

                int score =
                    GetOpenNeighborhoodScore(
                        Z,
                        cell,
                        2
                    );

                //
                // A 5x5 area has a maximum score of 25.
                // Requiring at least 16 strongly favors actual chambers,
                // broad caves, and large islands over one-cell corridors.
                //
                if (score < 16)
                    continue;

                if (score > bestScore)
                {
                    bestScore =
                        score;

                    candidates.Clear();

                    candidates.Add(
                        cell
                    );
                }
                else if (
                    score >=
                        bestScore - 2
                )
                {
                    candidates.Add(
                        cell
                    );
                }
            }

            //
            // Some mixed Category-4 geometries may simply not contain a
            // 5x5-ish open region. Fall back gracefully rather than making
            // Electrical C5 dependent on one shape family.
            //
            if (candidates.Count == 0)
            {
                foreach (
                    Cell cell
                    in Z.GetCells()
                )
                {
                    if (
                        IsAvailableOpenCell(
                            Z,
                            reserved,
                            cell
                        ) &&
                        CountOpenCardinalNeighbors(
                            Z,
                            cell
                        ) >= 3
                    )
                    {
                        candidates.Add(
                            cell
                        );
                    }
                }
            }

            if (candidates.Count == 0)
                return null;

            return
                candidates[
                    rng.Next(
                        candidates.Count
                    )
                ];
        }


        private Cell PickMachineryCell(
            Zone Z,
            bool[,] reserved,
            System.Random rng,
            bool preferWall
        )
        {
            List<Cell> candidates =
                new List<Cell>();

            foreach (
                Cell cell
                in Z.GetCells()
            )
            {
                if (
                    !IsAvailableOpenCell(
                        Z,
                        reserved,
                        cell
                    )
                )
                {
                    continue;
                }

                //
                // Don't deliberately put large solid machinery into a
                // one-cell choke point.
                //
                if (
                    CountOpenCardinalNeighbors(
                        Z,
                        cell
                    ) < 2
                )
                {
                    continue;
                }

                if (
                    preferWall &&
                    !IsAdjacentToWallLike(
                        Z,
                        cell
                    )
                )
                {
                    continue;
                }

                candidates.Add(
                    cell
                );
            }

            //
            // Wall-preferring objects get a fallback rather than simply
            // disappearing on layouts with very little conventional wall.
            //
            if (
                candidates.Count == 0 &&
                preferWall
            )
            {
                return
                    PickMachineryCell(
                        Z,
                        reserved,
                        rng,
                        false
                    );
            }

            if (candidates.Count == 0)
                return null;

            return
                candidates[
                    rng.Next(
                        candidates.Count
                    )
                ];
        }


        private Cell PickNearbyAvailableCell(
            Zone Z,
            bool[,] reserved,
            Cell center,
            int radius,
            System.Random rng,
            bool preferWall
        )
        {
            if (
                Z == null ||
                center == null ||
                rng == null
            )
            {
                return null;
            }

            List<Cell> candidates =
                new List<Cell>();

            for (
                int x =
                    center.X - radius;
                x <=
                    center.X + radius;
                x++
            )
            {
                for (
                    int y =
                        center.Y - radius;
                    y <=
                        center.Y + radius;
                    y++
                )
                {
                    Cell cell =
                        Z.GetCell(
                            x,
                            y
                        );

                    if (
                        !IsAvailableOpenCell(
                            Z,
                            reserved,
                            cell
                        )
                    )
                    {
                        continue;
                    }

                    int dx =
                        x - center.X;

                    int dy =
                        y - center.Y;

                    if (
                        dx == 0 &&
                        dy == 0
                    )
                    {
                        continue;
                    }

                    if (
                        dx * dx +
                        dy * dy >
                        radius * radius
                    )
                    {
                        continue;
                    }

                    if (
                        CountOpenCardinalNeighbors(
                            Z,
                            cell
                        ) < 2
                    )
                    {
                        continue;
                    }

                    if (
                        preferWall &&
                        !IsAdjacentToWallLike(
                            Z,
                            cell
                        )
                    )
                    {
                        continue;
                    }

                    candidates.Add(
                        cell
                    );
                }
            }

            //
            // As above, preference is not an absolute requirement.
            //
            if (
                candidates.Count == 0 &&
                preferWall
            )
            {
                return
                    PickNearbyAvailableCell(
                        Z,
                        reserved,
                        center,
                        radius,
                        rng,
                        false
                    );
            }

            if (candidates.Count == 0)
                return null;

            return
                candidates[
                    rng.Next(
                        candidates.Count
                    )
                ];
        }


        private bool IsAvailableOpenCell(
            Zone Z,
            bool[,] reserved,
            Cell cell
        )
        {
            if (
                Z == null ||
                reserved == null ||
                cell == null
            )
            {
                return false;
            }

            if (
                !CellIsInsideDecorationScope(
                    Z,
                    cell
                )
            )
            {
                return false;
            }

            int x =
                cell.X;

            int y =
                cell.Y;

            if (
                x < 0 ||
                y < 0 ||
                x >= Z.Width ||
                y >= Z.Height
            )
            {
                return false;
            }

            if (
                reserved[
                    x,
                    y
                ]
            )
            {
                return false;
            }

            //
            // Respect semantic ownership from anything legitimately placed before
            // Electrical C1 physical infrastructure.
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
                !cell.IsEmptyOfSolid()
            )
            {
                return false;
            }

            //
            // Spills are not occupied space.
            //
            // Electrical machinery may be installed directly in Ooze, oil, asphalt,
            // or other permissive liquid. Pools exclude machinery through claims instead.
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

            //
            // Avoid deliberately stacking infrastructure on creatures,
            // containers, existing furniture, or other meaningful fixtures.
            //
            foreach (
                GameObject obj
                in cell.GetObjects()
            )
            {
                if (obj == null)
                    continue;

                if (
                    obj.IsCombatObject() ||
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


        private int GetOpenNeighborhoodScore(
            Zone Z,
            Cell center,
            int radius
        )
        {
            if (
                Z == null ||
                center == null
            )
            {
                return 0;
            }

            int score =
                0;

            for (
                int dx = -radius;
                dx <= radius;
                dx++
            )
            {
                for (
                    int dy = -radius;
                    dy <= radius;
                    dy++
                )
                {
                    Cell cell =
                        Z.GetCell(
                            center.X + dx,
                            center.Y + dy
                        );

                    if (
                        cell != null &&
                        SubterraneanSites
                            .SubterraneanSitesEPGeometry
                            .IsOpenGeometryCell(
                                cell
                            )
                    )
                    {
                        score++;
                    }
                }
            }

            return score;
        }


        private int CountOpenCardinalNeighbors(
            Zone Z,
            Cell cell
        )
        {
            if (
                Z == null ||
                cell == null
            )
            {
                return 0;
            }

            int count =
                0;

            CountOpenNeighbor(
                Z.GetCell(
                    cell.X,
                    cell.Y - 1
                ),
                ref count
            );

            CountOpenNeighbor(
                Z.GetCell(
                    cell.X + 1,
                    cell.Y
                ),
                ref count
            );

            CountOpenNeighbor(
                Z.GetCell(
                    cell.X,
                    cell.Y + 1
                ),
                ref count
            );

            CountOpenNeighbor(
                Z.GetCell(
                    cell.X - 1,
                    cell.Y
                ),
                ref count
            );

            return count;
        }


        private void CountOpenNeighbor(
            Cell cell,
            ref int count
        )
        {
            if (
                cell != null &&
                SubterraneanSites
                    .SubterraneanSitesEPGeometry
                    .IsOpenGeometryCell(
                        cell
                    ) &&
                !cell.IsSolid()
            )
            {
                count++;
            }
        }


        private bool IsAdjacentToWallLike(
            Zone Z,
            Cell cell
        )
        {
            if (
                Z == null ||
                cell == null
            )
            {
                return false;
            }

            return
                IsWallLike(
                    Z.GetCell(
                        cell.X,
                        cell.Y - 1
                    )
                ) ||
                IsWallLike(
                    Z.GetCell(
                        cell.X + 1,
                        cell.Y
                    )
                ) ||
                IsWallLike(
                    Z.GetCell(
                        cell.X,
                        cell.Y + 1
                    )
                ) ||
                IsWallLike(
                    Z.GetCell(
                        cell.X - 1,
                        cell.Y
                    )
                );
        }


        private bool IsWallLike(
            Cell cell
        )
        {
            if (cell == null)
                return false;

            //
            // At this point most structural cells are still the shared
            // semantic placeholders. A Category-2 hazard may already have
            // replaced a boundary placeholder, however, so actual solidity
            // is accepted as well.
            //
            return
                SubterraneanSites
                    .SubterraneanSitesEPGeometry
                    .IsAnyPlaceholder(
                        cell
                    ) ||
                cell.IsSolid();
        }


        // ================================================================
        // RESERVATION / SPACING
        // ================================================================

        private void ReserveAroundCell(
            bool[,] reserved,
            Zone Z,
            Cell center,
            int radius
        )
        {
            if (
                reserved == null ||
                Z == null ||
                center == null
            )
            {
                return;
            }

            for (
                int dx = -radius;
                dx <= radius;
                dx++
            )
            {
                for (
                    int dy = -radius;
                    dy <= radius;
                    dy++
                )
                {
                    if (
                        dx * dx +
                        dy * dy >
                        radius * radius
                    )
                    {
                        continue;
                    }

                    int x =
                        center.X + dx;

                    int y =
                        center.Y + dy;

                    if (
                        x < 0 ||
                        y < 0 ||
                        x >= Z.Width ||
                        y >= Z.Height
                    )
                    {
                        continue;
                    }

                    reserved[
                        x,
                        y
                    ] = true;
                }
            }
        }


        private bool FarEnoughFromCells(
            Cell candidate,
            List<Cell> others,
            int minimumDistance
        )
        {
            if (
                candidate == null ||
                others == null
            )
            {
                return false;
            }

            foreach (
                Cell other
                in others
            )
            {
                if (other == null)
                    continue;

                int dx =
                    candidate.X -
                    other.X;

                int dy =
                    candidate.Y -
                    other.Y;

                if (
                    dx * dx +
                    dy * dy <
                    minimumDistance *
                    minimumDistance
                )
                {
                    return false;
                }
            }

            return true;
        }


        // ================================================================
        // OBJECT CREATION
        // ================================================================

        private bool TryAddObject(
            Cell cell,
            string blueprint
        )
        {
            if (
                cell == null ||
                blueprint.IsNullOrEmpty()
            )
            {
                return false;
            }

            GameObject obj =
                GameObjectFactory.Factory
                    .CreateObject(
                        blueprint
                    );

            if (obj == null)
                return false;

            cell.AddObject(
                obj
            );

            //
            // Every successfully placed Electrical C1 machine is discrete
            // higher-priority infrastructure.
            //
            SubterraneanSites
                .SubterraneanSitesEPReservations
                .ClaimCell(
                    cell.ParentZone,
                    cell
                );

            return true;
        }
    }

}

namespace XRL.World.ZoneBuilders
{
    /// <summary>
    /// Electrical Category 5.
    ///
    /// Disconnected technological decoration.
    ///
    /// Unlike Electrical Category 1, this builder deliberately creates
    /// no PowerLine or HeavyPowerLine network and no major power-source
    /// installation. Its job is to make Electrical recognizable when it
    /// appears only as the shuffled decoration theme.
    ///
    /// Contents:
    ///   - loose technological machinery
    ///   - small equipment clusters
    ///   - clockthings and other disconnected devices
    ///   - Techlight1 / Techlight2 / Techlight3
    ///
    /// The same builder supplies a reduced entrance-scar preview.
    /// </summary>
    public class SubterraneanSitesElectricalDecorations :
        ZoneBuilderSandbox
    {
        public int EntranceOnly = 0;

        //
        // Full-layer tuning.
        //
        public int MinClusters = 2;
        public int MaxClusters = 4;

        public int MinClusterObjects = 2;
        public int MaxClusterObjects = 4;

        public int MinLooseObjects = 6;
        public int MaxLooseObjects = 12;

        public int MinStrandedGenerators = 1;
        public int MaxStrandedGenerators = 3;

        public int MinCabinets = 2;
        public int MaxCabinets = 4;

        public int MinLightSculptures = 0;
        public int MaxLightSculptures = 2;

        public int MinLights = 4;
        public int MaxLights = 8;

        public int MinWireRuns = 7;
        public int MaxWireRuns = 11;

        public int MinWireRunLength = 3;
        public int MaxWireRunLength = 7;

        public int HeavyWirePercent = 20;

        public int ClusterRadius = 2;
        public int ClusterSeparationRadius = 3;

        public int TransitionExclusionRadius = 4;

        public int MinLightSpacing = 4;
       


        private sealed class WeightedBlueprint
        {
            public string Blueprint;
            public int Weight;
            public bool PreferWall;
        }


        //
        // These are intentionally not divided into a functional
        // electrical circuit. Some are powered machinery in vanilla;
        // here they simply occur as isolated pieces of strange technology.
        //
        private readonly WeightedBlueprint[] DecorationPool =
        {
            new WeightedBlueprint
            {
                Blueprint = "Display Breadboard",
                Weight = 7,
                PreferWall = true
            },

            new WeightedBlueprint
            {
                Blueprint = "Clockthing",
                Weight = 4,
                PreferWall = false
            },

            new WeightedBlueprint
            {
                Blueprint = "Unicomputer",
                Weight = 4,
                PreferWall = true
            },

            new WeightedBlueprint
            {
                Blueprint = "Loudspeaker",
                Weight = 3,
                PreferWall = true
            },

            new WeightedBlueprint
            {
                Blueprint = "Electrothing",
                Weight = 2,
                PreferWall = false
            },

            new WeightedBlueprint
            {
                Blueprint = "Fluxthing",
                Weight = 2,
                PreferWall = false
            },

            new WeightedBlueprint
            {
                Blueprint = "Wire Extruder",
                Weight = 2,
                PreferWall = false
            },

            new WeightedBlueprint
            {
                Blueprint = "Induction Charging Station",
                Weight = 1,
                PreferWall = true
            },

            new WeightedBlueprint
            {
                Blueprint = "Universal Charging Station",
                Weight = 1,
                PreferWall = true
            }
        };


        private const string TechlightBlueprint =
            "Techlight3";

        private const string StrandedGeneratorBlueprint =
            "Electric Generator";

        private const string CabinetBlueprint =
            "Multicabinet";

        private const string LightSculptureBlueprint =
            "Light Sculpture";

        private const string PowerLineBlueprint =
            "PowerLine";

        private const string HeavyPowerLineBlueprint =
            "HeavyPowerLine";



        public bool BuildZone(
            Zone Z
        )
        {
            if (Z == null)
                return true;

            if (EntranceOnly != 0)
            {
                ApplyEntranceDefaults();
            }

            ClampSettings();

            int seed =
                XRLCore.Core.Game.GetWorldSeed(
                    "SubterraneanSites:ElectricalDisconnectedDecorations:" +
                    Z.ZoneID
                );

            System.Random rng =
                new System.Random(
                    seed
                );

            bool[,] reserved =
                new bool[
                    Z.Width,
                    Z.Height
                ];

            //
            // Underground levels reserve their planned vertical
            // transition footprints.
            //
            // The entrance uses its explicit scar/hole scope instead.
            //
            if (EntranceOnly == 0)
            {
                List<Location2D> anchors =
                    SubterraneanSites
                        .SubterraneanSitesEPVerticalTransitions
                        .GetVerticalAnchors(
                            Z.ZoneID
                        );

                SubterraneanSites
                    .SubterraneanSitesEPPlacement
                    .ReserveAroundAnchors(
                        reserved,
                        Z,
                        anchors,
                        TransitionExclusionRadius
                    );
            }

           //
            // Larger solid equipment gets first choice of safe,
            // reasonably broad floor space.
            //
            PlaceStrandedGenerators(
                Z,
                reserved,
                rng
            );

            PlaceCabinets(
                Z,
                reserved,
                rng
            );

            //
            // Then layer in the ordinary technological clutter.
            //
            PlaceClusters(
                Z,
                reserved,
                rng
            );

            PlaceLooseObjects(
                Z,
                reserved,
                rng
            );

            PlaceLightSculptures(
                Z,
                reserved,
                rng
            );

            PlaceLights(
                Z,
                reserved,
                rng
            );

            //
            // Finally scatter short conduit remnants.
            //
            // These are deliberately not routed between machinery.
            // They may nevertheless intersect one another or an Electrical
            // C1 network emergently, which is intentional.
            //
            PlaceDisconnectedWireRuns(
                Z,
                rng
            );

            return true;
        }


        private void ApplyEntranceDefaults()
        {
            MinClusters = 1;
            MaxClusters = 1;

            MinClusterObjects = 2;
            MaxClusterObjects = 3;

            MinLooseObjects = 2;
            MaxLooseObjects = 4;

            MinStrandedGenerators = 1;
            MaxStrandedGenerators = 1;

            MinCabinets = 0;
            MaxCabinets = 1;

            MinLightSculptures = 0;
            MaxLightSculptures = 1;

            MinLights = 1;
            MaxLights = 2;

            MinWireRuns = 2;
            MaxWireRuns = 4;

            MinWireRunLength = 2;
            MaxWireRunLength = 4;

            ClusterSeparationRadius = 2;
            MinLightSpacing = 3;
        }


        private void ClampSettings()
        {
            MinClusters =
                Math.Max(
                    0,
                    MinClusters
                );

            MaxClusters =
                Math.Max(
                    MinClusters,
                    MaxClusters
                );

            MinClusterObjects =
                Math.Max(
                    1,
                    MinClusterObjects
                );

            MaxClusterObjects =
                Math.Max(
                    MinClusterObjects,
                    MaxClusterObjects
                );

            MinLooseObjects =
                Math.Max(
                    0,
                    MinLooseObjects
                );

            MaxLooseObjects =
                Math.Max(
                    MinLooseObjects,
                    MaxLooseObjects
                );

            MinStrandedGenerators =
                Math.Max(
                    0,
                    MinStrandedGenerators
                );

            MaxStrandedGenerators =
                Math.Max(
                    MinStrandedGenerators,
                    MaxStrandedGenerators
                );

            MinCabinets =
                Math.Max(
                    0,
                    MinCabinets
                );

            MaxCabinets =
                Math.Max(
                    MinCabinets,
                    MaxCabinets
                );

            MinLightSculptures =
                Math.Max(
                    0,
                    MinLightSculptures
                );

            MaxLightSculptures =
                Math.Max(
                    MinLightSculptures,
                    MaxLightSculptures
                );

            MinLights =
                Math.Max(
                    0,
                    MinLights
                );

            MaxLights =
                Math.Max(
                    MinLights,
                    MaxLights
                );

            MinWireRuns =
                Math.Max(
                    0,
                    MinWireRuns
                );

            MaxWireRuns =
                Math.Max(
                    MinWireRuns,
                    MaxWireRuns
                );

            MinWireRunLength =
                Math.Max(
                    1,
                    MinWireRunLength
                );

            MaxWireRunLength =
                Math.Max(
                    MinWireRunLength,
                    MaxWireRunLength
                );

            HeavyWirePercent =
                Math.Max(
                    0,
                    Math.Min(
                        100,
                        HeavyWirePercent
                    )
                );

            ClusterRadius =
                Math.Max(
                    1,
                    ClusterRadius
                );

            ClusterSeparationRadius =
                Math.Max(
                    0,
                    ClusterSeparationRadius
                );

            TransitionExclusionRadius =
                Math.Max(
                    0,
                    TransitionExclusionRadius
                );

            MinLightSpacing =
                Math.Max(
                    0,
                    MinLightSpacing
                );
        }


        // ================================================================
        // EQUIPMENT CLUSTERS
        // ================================================================

        private void PlaceClusters(
            Zone Z,
            bool[,] reserved,
            System.Random rng
        )
        {
            int clusterCount =
                rng.Next(
                    MinClusters,
                    MaxClusters + 1
                );

            for (
                int clusterIndex = 0;
                clusterIndex < clusterCount;
                clusterIndex++
            )
            {
                WeightedBlueprint centerSpec =
                    RollDecoration(
                        rng
                    );

                if (centerSpec == null)
                    continue;

                Cell center =
                    PickDecorationCell(
                        Z,
                        reserved,
                        rng,
                        centerSpec.PreferWall
                    );

                if (center == null)
                    continue;

                if (
                    !TryAddObject(
                        center,
                        centerSpec.Blueprint
                    )
                )
                {
                    continue;
                }

                reserved[
                    center.X,
                    center.Y
                ] = true;

                int targetCount =
                    rng.Next(
                        MinClusterObjects,
                        MaxClusterObjects + 1
                    );

                for (
                    int objectIndex = 1;
                    objectIndex < targetCount;
                    objectIndex++
                )
                {
                    WeightedBlueprint spec =
                        RollDecoration(
                            rng
                        );

                    if (spec == null)
                        continue;

                    Cell destination =
                        PickNearbyDecorationCell(
                            Z,
                            reserved,
                            center,
                            ClusterRadius,
                            rng,
                            spec.PreferWall
                        );

                    if (destination == null)
                        continue;

                    if (
                        TryAddObject(
                            destination,
                            spec.Blueprint
                        )
                    )
                    {
                        reserved[
                            destination.X,
                            destination.Y
                        ] = true;
                    }
                }

                //
                // Keep the small installations visually distinct.
                // This reservation happens after the cluster itself is placed.
                //
                ReserveAroundCell(
                    reserved,
                    Z,
                    center,
                    ClusterSeparationRadius
                );
            }
        }


        // ================================================================
        // LOOSE TECHNOLOGY
        // ================================================================

        private void PlaceLooseObjects(
            Zone Z,
            bool[,] reserved,
            System.Random rng
        )
        {
            int count =
                rng.Next(
                    MinLooseObjects,
                    MaxLooseObjects + 1
                );

            for (
                int i = 0;
                i < count;
                i++
            )
            {
                WeightedBlueprint spec =
                    RollDecoration(
                        rng
                    );

                if (spec == null)
                    continue;

                Cell destination =
                    PickDecorationCell(
                        Z,
                        reserved,
                        rng,
                        spec.PreferWall
                    );

                if (destination == null)
                    continue;

                if (
                    TryAddObject(
                        destination,
                        spec.Blueprint
                    )
                )
                {
                    reserved[
                        destination.X,
                        destination.Y
                    ] = true;
                }
            }
        }

        // ================================================================
        // STRANDED GENERATORS
        // ================================================================

        private void PlaceStrandedGenerators(
            Zone Z,
            bool[,] reserved,
            System.Random rng
        )
        {
            int count =
                rng.Next(
                    MinStrandedGenerators,
                    MaxStrandedGenerators + 1
                );

            for (int i = 0; i < count; i++)
            {
                Cell destination =
                    PickBroadDecorationCell(
                        Z,
                        reserved,
                        rng,
                        false
                    );

                if (destination == null)
                    continue;

                if (
                    TryAddObject(
                        destination,
                        StrandedGeneratorBlueprint
                    )
                )
                {
                    ReserveAroundCell(
                        reserved,
                        Z,
                        destination,
                        1
                    );
                }
            }
        }


        // ================================================================
        // CABINETS
        // ================================================================

        private void PlaceCabinets(
            Zone Z,
            bool[,] reserved,
            System.Random rng
        )
        {
            int count =
                rng.Next(
                    MinCabinets,
                    MaxCabinets + 1
                );

            for (int i = 0; i < count; i++)
            {
                Cell destination =
                    PickBroadDecorationCell(
                        Z,
                        reserved,
                        rng,
                        true
                    );

                if (destination == null)
                    continue;

                if (
                    TryAddObject(
                        destination,
                        CabinetBlueprint
                    )
                )
                {
                    ReserveAroundCell(
                        reserved,
                        Z,
                        destination,
                        1
                    );
                }
            }
        }


        // ================================================================
        // LIGHT SCULPTURES
        // ================================================================

        private void PlaceLightSculptures(
            Zone Z,
            bool[,] reserved,
            System.Random rng
        )
        {
            int count =
                rng.Next(
                    MinLightSculptures,
                    MaxLightSculptures + 1
                );

            for (int i = 0; i < count; i++)
            {
                Cell destination =
                    PickDecorationCell(
                        Z,
                        reserved,
                        rng,
                        false
                    );

                if (destination == null)
                    continue;

                if (
                    TryAddObject(
                        destination,
                        LightSculptureBlueprint
                    )
                )
                {
                    reserved[
                        destination.X,
                        destination.Y
                    ] = true;
                }
            }
        }


        // ================================================================
        // LIGHTS
        // ================================================================

        private void PlaceLights(
            Zone Z,
            bool[,] reserved,
            System.Random rng
        )
        {
            int desired =
                rng.Next(
                    MinLights,
                    MaxLights + 1
                );

            List<Cell> placed =
                new List<Cell>();

            int attempts =
                desired * 12 + 20;

            while (
                placed.Count < desired &&
                attempts-- > 0
            )
            {
                Cell destination =
                    PickDecorationCell(
                        Z,
                        reserved,
                        rng,
                        true
                    );

                if (destination == null)
                    break;

                if (
                    !FarEnoughFromCells(
                        destination,
                        placed,
                        MinLightSpacing
                    )
                )
                {
                    continue;
                }

                if (
                    TryAddObject(
                        destination,
                        TechlightBlueprint
                    )
                )
                {
                    reserved[
                        destination.X,
                        destination.Y
                    ] = true;

                    placed.Add(
                        destination
                    );
                }
            }
        }

        // ================================================================
        // DISCONNECTED CONDUIT REMNANTS
        // ================================================================

        private void PlaceDisconnectedWireRuns(
            Zone Z,
            System.Random rng
        )
        {
            if (
                Z == null ||
                rng == null
            )
            {
                return;
            }

            int runCount =
                rng.Next(
                    MinWireRuns,
                    MaxWireRuns + 1
                );

            for (
                int runIndex = 0;
                runIndex < runCount;
                runIndex++
            )
            {
                Cell start =
                    PickWireStartCell(
                        Z,
                        rng
                    );

                if (start == null)
                    continue;

                int targetLength =
                    rng.Next(
                        MinWireRunLength,
                        MaxWireRunLength + 1
                    );

                string blueprint =
                    rng.Next(100) <
                        HeavyWirePercent
                            ? HeavyPowerLineBlueprint
                            : PowerLineBlueprint;

                int[][] directions =
                {
                    new int[] {  1,  0 },
                    new int[] { -1,  0 },
                    new int[] {  0,  1 },
                    new int[] {  0, -1 }
                };

                int firstDirection =
                    rng.Next(
                        directions.Length
                    );

                int[] chosenDirection =
                    null;

                //
                // Prefer a direction with at least one legal continuation
                // so most generated remnants read as actual short runs.
                //
                for (
                    int attempt = 0;
                    attempt < directions.Length;
                    attempt++
                )
                {
                    int directionIndex =
                        (
                            firstDirection +
                            attempt
                        ) %
                        directions.Length;

                    int[] candidateDirection =
                        directions[
                            directionIndex
                        ];

                    Cell next =
                        Z.GetCell(
                            start.X +
                                candidateDirection[0],
                            start.Y +
                                candidateDirection[1]
                        );

                    if (
                        IsWireDecorationCell(
                            Z,
                            next
                        )
                    )
                    {
                        chosenDirection =
                            candidateDirection;

                        break;
                    }
                }

                if (chosenDirection == null)
                    continue;

                int x =
                    start.X;

                int y =
                    start.Y;

                for (
                    int step = 0;
                    step < targetLength;
                    step++
                )
                {
                    Cell cell =
                        Z.GetCell(
                            x,
                            y
                        );

                    if (
                        !IsWireDecorationCell(
                            Z,
                            cell
                        )
                    )
                    {
                        break;
                    }

                    TryPlaceDecorationWire(
                        cell,
                        blueprint
                    );

                    x +=
                        chosenDirection[0];

                    y +=
                        chosenDirection[1];
                }
            }
        }


        private Cell PickWireStartCell(
            Zone Z,
            System.Random rng
        )
        {
            return
                SubterraneanSites
                    .SubterraneanSitesEPPlacement
                    .PickRandomCell(
                        Z,
                        delegate(Cell cell)
                        {
                            return
                                IsWireDecorationCell(
                                    Z,
                                    cell
                                ) &&
                                !HasAnyPowerLine(
                                    cell
                                );
                        },
                        rng
                    );
        }


        private bool IsWireDecorationCell(
            Zone Z,
            Cell cell
        )
        {
            if (
                Z == null ||
                cell == null
            )
            {
                return false;
            }

            if (
                !CellIsInsideDecorationScope(
                    Z,
                    cell
                )
            )
            {
                return false;
            }

            //
            // Existing conduit remains legal. This permits the independent
            // C5 remnants to intersect one another or an Electrical C1
            // network naturally.
            //
            if (
                HasAnyPowerLine(
                    cell
                )
            )
            {
                return true;
            }

            //
            // Wire may merge with wire, handled above.
            //
            // For every other kind of claimed space, however, the disconnected
            // remnant must respect the same semantic ownership as other C5 content.
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

            if (!cell.IsEmptyOfSolid())
                return false;

            //
            // Spills do not block conduit.
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

            return true;
        }


        private bool TryPlaceDecorationWire(
            Cell cell,
            string blueprint
        )
        {
            if (
                cell == null ||
                blueprint.IsNullOrEmpty()
            )
            {
                return false;
            }

            if (
                HasAnyPowerLine(
                    cell
                )
            )
            {
                //
                // Existing conduit remains reusable by later wire runs, but the
                // cell is still semantic Electrical infrastructure for non-wire content.
                //
                SubterraneanSites
                    .SubterraneanSitesEPReservations
                    .ClaimCell(
                        cell.ParentZone,
                        cell
                    );

                return true;
            }

            GameObject wire =
                GameObjectFactory.Factory
                    .CreateObject(
                        blueprint
                    );

            if (wire == null)
                return false;

            cell.AddObject(
                wire
            );

            SubterraneanSites
                .SubterraneanSitesEPReservations
                .ClaimCell(
                    cell.ParentZone,
                    cell
                );

            return true;
        }


        private bool HasAnyPowerLine(
            Cell cell
        )
        {
            if (cell == null)
                return false;

            return
                cell.HasObjectWithBlueprint(
                    PowerLineBlueprint
                ) ||
                cell.HasObjectWithBlueprint(
                    HeavyPowerLineBlueprint
                );
        }


        // ================================================================
        // WEIGHTED CONTENT
        // ================================================================

        private WeightedBlueprint RollDecoration(
            System.Random rng
        )
        {
            if (
                rng == null ||
                DecorationPool == null ||
                DecorationPool.Length == 0
            )
            {
                return null;
            }

            int totalWeight = 0;

            foreach (
                WeightedBlueprint entry
                in DecorationPool
            )
            {
                if (
                    entry != null &&
                    entry.Weight > 0
                )
                {
                    totalWeight +=
                        entry.Weight;
                }
            }

            if (totalWeight <= 0)
                return null;

            int roll =
                rng.Next(
                    totalWeight
                );

            foreach (
                WeightedBlueprint entry
                in DecorationPool
            )
            {
                if (
                    entry == null ||
                    entry.Weight <= 0
                )
                {
                    continue;
                }

                if (roll < entry.Weight)
                    return entry;

                roll -=
                    entry.Weight;
            }

            return DecorationPool[0];
        }


        // ================================================================
        // CELL SELECTION
        // ================================================================

        private Cell PickBroadDecorationCell(
            Zone Z,
            bool[,] reserved,
            System.Random rng,
            bool preferWall
        )
        {
            Cell result =
                SubterraneanSites
                    .SubterraneanSitesEPPlacement
                    .PickRandomCell(
                        Z,
                        delegate(Cell cell)
                        {
                            if (
                                !CellIsAvailable(
                                    Z,
                                    reserved,
                                    cell
                                )
                            )
                            {
                                return false;
                            }

                            //
                            // Solid furniture and generators should go in
                            // chambers/open areas, not one-cell corridors.
                            //
                            if (
                                CountOpenCardinalNeighbors(
                                    Z,
                                    cell
                                ) < 3
                            )
                            {
                                return false;
                            }

                            if (
                                preferWall &&
                                !IsAdjacentToWallLike(
                                    Z,
                                    cell
                                )
                            )
                            {
                                return false;
                            }

                            return true;
                        },
                        rng
                    );

            if (
                result == null &&
                preferWall
            )
            {
                return
                    PickBroadDecorationCell(
                        Z,
                        reserved,
                        rng,
                        false
                    );
            }

            return result;
        }


        private Cell PickDecorationCell(
            Zone Z,
            bool[,] reserved,
            System.Random rng,
            bool preferWall
        )
        {
            Cell result =
                SubterraneanSites
                    .SubterraneanSitesEPPlacement
                    .PickRandomCell(
                        Z,
                        delegate(Cell cell)
                        {
                            if (
                                !CellIsAvailable(
                                    Z,
                                    reserved,
                                    cell
                                )
                            )
                            {
                                return false;
                            }

                            if (
                                preferWall &&
                                !IsAdjacentToWallLike(
                                    Z,
                                    cell
                                )
                            )
                            {
                                return false;
                            }

                            return true;
                        },
                        rng
                    );

            //
            // Wall adjacency is a preference, not a requirement.
            // This keeps Electrical C5 portable to highly open
            // shuffled geometries such as Fire or Fungus.
            //
            if (
                result == null &&
                preferWall
            )
            {
                return
                    PickDecorationCell(
                        Z,
                        reserved,
                        rng,
                        false
                    );
            }

            return result;
        }


        private Cell PickNearbyDecorationCell(
            Zone Z,
            bool[,] reserved,
            Cell center,
            int radius,
            System.Random rng,
            bool preferWall
        )
        {
            if (
                Z == null ||
                reserved == null ||
                center == null ||
                rng == null
            )
            {
                return null;
            }

            List<Cell> candidates =
                new List<Cell>();

            for (
                int x = center.X - radius;
                x <= center.X + radius;
                x++
            )
            {
                for (
                    int y = center.Y - radius;
                    y <= center.Y + radius;
                    y++
                )
                {
                    Cell cell =
                        Z.GetCell(
                            x,
                            y
                        );

                    if (
                        !CellIsAvailable(
                            Z,
                            reserved,
                            cell
                        )
                    )
                    {
                        continue;
                    }

                    int dx =
                        x - center.X;

                    int dy =
                        y - center.Y;

                    if (
                        dx == 0 &&
                        dy == 0
                    )
                    {
                        continue;
                    }

                    if (
                        dx * dx +
                        dy * dy >
                        radius * radius
                    )
                    {
                        continue;
                    }

                    if (
                        preferWall &&
                        !IsAdjacentToWallLike(
                            Z,
                            cell
                        )
                    )
                    {
                        continue;
                    }

                    candidates.Add(
                        cell
                    );
                }
            }

            if (
                candidates.Count == 0 &&
                preferWall
            )
            {
                return
                    PickNearbyDecorationCell(
                        Z,
                        reserved,
                        center,
                        radius,
                        rng,
                        false
                    );
            }

            if (candidates.Count == 0)
                return null;

            return
                candidates[
                    rng.Next(
                        candidates.Count
                    )
                ];
        }


        private bool CellIsAvailable(
            Zone Z,
            bool[,] reserved,
            Cell cell
        )
        {
            if (
                Z == null ||
                reserved == null ||
                cell == null
            )
            {
                return false;
            }

            if (
                !CellIsInsideDecorationScope(
                    Z,
                    cell
                )
            )
            {
                return false;
            }

            int x =
                cell.X;

            int y =
                cell.Y;

            if (
                x < 0 ||
                y < 0 ||
                x >= Z.Width ||
                y >= Z.Height
            )
            {
                return false;
            }

            if (
                reserved[
                    x,
                    y
                ]
            )
            {
                return false;
            }

            //
            // Shared semantic ownership replaces the old Electrical-specific
            // infrastructure blacklist.
            //
            // This catches C1 machinery and conduit, C2 functional clearance,
            // exclusive pools, and earlier claimed content from either EP theme.
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

            if (!cell.IsEmptyOfSolid())
                return false;

            //
            // Spills are valid installation space.
            //
            // Electrical decoration can sit directly in Ooze, oil, asphalt, etc.
            // Object-like pools keep machinery out through shared claims instead.
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

            //
            // Don't deliberately place technological furniture on top
            // of creatures, containers, or existing furniture.
            //
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


        private bool CellIsInsideDecorationScope(
            Zone Z,
            Cell cell
        )
        {
            if (
                Z == null ||
                cell == null
            )
            {
                return false;
            }

            if (EntranceOnly == 0)
            {
                return
                    SubterraneanSites
                        .SubterraneanSitesEPGeometry
                        .IsOpenGeometryCell(
                            cell
                        );
            }

            return
                SubterraneanSites
                    .SubterraneanSitesEPEntrance
                    .IsInsideScar(
                        Z,
                        cell
                    ) &&
                !SubterraneanSites
                    .SubterraneanSitesEPEntrance
                    .IsInsideHoleExclusion(
                        Z,
                        cell,
                        1
                    );
        }

        private int CountOpenCardinalNeighbors(
            Zone Z,
            Cell cell
        )
        {
            if (
                Z == null ||
                cell == null
            )
            {
                return 0;
            }

            int count = 0;

            if (
                IsOpenDecorationNeighbor(
                    Z,
                    Z.GetCell(
                        cell.X,
                        cell.Y - 1
                    )
                )
            )
            {
                count++;
            }

            if (
                IsOpenDecorationNeighbor(
                    Z,
                    Z.GetCell(
                        cell.X + 1,
                        cell.Y
                    )
                )
            )
            {
                count++;
            }

            if (
                IsOpenDecorationNeighbor(
                    Z,
                    Z.GetCell(
                        cell.X,
                        cell.Y + 1
                    )
                )
            )
            {
                count++;
            }

            if (
                IsOpenDecorationNeighbor(
                    Z,
                    Z.GetCell(
                        cell.X - 1,
                        cell.Y
                    )
                )
            )
            {
                count++;
            }

            return count;
        }


        private bool IsOpenDecorationNeighbor(
            Zone Z,
            Cell cell
        )
        {
            return
                cell != null &&
                CellIsInsideDecorationScope(
                    Z,
                    cell
                ) &&
                !cell.IsSolid();
        }


        private bool IsAdjacentToWallLike(
            Zone Z,
            Cell cell
        )
        {
            if (
                Z == null ||
                cell == null
            )
            {
                return false;
            }

            return
                IsWallLike(
                    Z.GetCell(
                        cell.X,
                        cell.Y - 1
                    )
                ) ||
                IsWallLike(
                    Z.GetCell(
                        cell.X + 1,
                        cell.Y
                    )
                ) ||
                IsWallLike(
                    Z.GetCell(
                        cell.X,
                        cell.Y + 1
                    )
                ) ||
                IsWallLike(
                    Z.GetCell(
                        cell.X - 1,
                        cell.Y
                    )
                );
        }


        private bool IsWallLike(
            Cell cell
        )
        {
            if (cell == null)
                return false;

            return
                SubterraneanSites
                    .SubterraneanSitesEPGeometry
                    .IsAnyPlaceholder(
                        cell
                    ) ||
                cell.IsSolid();
        }


        // ================================================================
        // SPACING
        // ================================================================

        private void ReserveAroundCell(
            bool[,] reserved,
            Zone Z,
            Cell center,
            int radius
        )
        {
            if (
                reserved == null ||
                Z == null ||
                center == null
            )
            {
                return;
            }

            SubterraneanSites
                .SubterraneanSitesEPPlacement
                .ReserveRectangle(
                    reserved,
                    Z,
                    center.X - radius,
                    center.Y - radius,
                    center.X + radius,
                    center.Y + radius
                );
        }


        private bool FarEnoughFromCells(
            Cell candidate,
            List<Cell> others,
            int minimumDistance
        )
        {
            if (
                candidate == null ||
                others == null
            )
            {
                return false;
            }

            foreach (
                Cell other
                in others
            )
            {
                if (other == null)
                    continue;

                int dx =
                    candidate.X -
                    other.X;

                int dy =
                    candidate.Y -
                    other.Y;

                if (
                    dx * dx +
                    dy * dy <
                    minimumDistance *
                    minimumDistance
                )
                {
                    return false;
                }
            }

            return true;
        }


        // ================================================================
        // OBJECT CREATION
        // ================================================================

        private bool TryAddObject(
            Cell cell,
            string blueprint
        )
        {
            if (
                cell == null ||
                blueprint.IsNullOrEmpty()
            )
            {
                return false;
            }

            GameObject obj =
                GameObjectFactory.Factory
                    .CreateObject(
                        blueprint
                    );

            if (obj == null)
                return false;

            cell.AddObject(
                obj
            );

            //
            // Actual Electrical C5 machinery/decor is discrete content.
            // Later placement should treat this cell as occupied.
            //
            SubterraneanSites
                .SubterraneanSitesEPReservations
                .ClaimCell(
                    cell.ParentZone,
                    cell
                );

            return true;
        }
    }
}