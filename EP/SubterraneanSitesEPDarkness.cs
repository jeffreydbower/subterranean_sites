using System;
using System.Collections.Generic;
using XRL;
using XRL.Core;
using XRL.Rules;
using XRL.World;
using Genkit;
using XRL.World.Effects;
using XRL.UI;


namespace SubterraneanSites
{
    /// <summary>
    /// DARKNESS DIMENSION
    ///
    /// Category 1:
    ///     magical darkness environment with Night Vision attunement.
    ///
    /// Category 2:
    ///     directional blindness-gas wall vents.
    ///
    /// Category 3:
    ///     limestone structural mass with exposed
    ///     InnerSultanWall_Period6 interior walls.
    ///
    /// Category 4:
    ///     dense recursively subdivided room network over grey marble.
    ///
    /// Category 5:
    ///     habitation remnants, vanta flora, white spinefruit, and debris.
    /// </summary>
    internal sealed class SubterraneanSitesEPDarknessTheme :
        ISubterraneanSitesEPCategoryProvider,
        ISubterraneanSitesEPAttunementProvider,
        ISubterraneanSitesEPDenizenAdaptationProvider,
        ISubterraneanSitesEPSignatureMutationProvider
    {
        public string ThemeKey
        {
            get { return "Darkness"; }
        }

        public string SignatureMutationClass
        {
            get { return "NightVision"; }
        }


        public void ApplyDenizenAdaptation(
            GameObject creature
        )
        {
            if (creature == null)
                return;

            XRL.World.Effects
                .SubterraneanSitesEPDarknessDenizenAdaptationEffect existing =
                    creature.GetEffectDescendedFrom<
                        XRL.World.Effects
                            .SubterraneanSitesEPDarknessDenizenAdaptationEffect
                    >();

            if (existing == null)
            {
                creature.ApplyEffect(
                    new XRL.World.Effects
                        .SubterraneanSitesEPDarknessDenizenAdaptationEffect()
                );
            }
        }

        public int MinimumHoleSeparation
        {
            get { return 25; }
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
            // Darkness C1 is a runtime visual environment.
            //
            // One persistent system synchronizes the player-side render effect
            // whenever a Darkness-primary zone is entered or left.
            //
            The.Game.RequireSystem<
                SubterraneanSitesEPDarknessEnvironmentSystem
            >();
        }

        public SubterraneanSitesEPAttunementBuildResult TryCreateAttunement(
            GameObject actor,
            Zone zone,
            out XRL.World.Effects.SubterraneanSitesEPAttunementEffect effect,
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
                    .SubterraneanSitesEPDarknessAttunementEffect(
                        SubterraneanSitesEPAttunementSystem
                            .DefaultDuration,
                        mutationLevel
                    );

            successMessage =
                "Attunement grants:\n" +
                "Night Vision (level " +
                mutationLevel.ToString() +
                ")\n" +
                "Extradimensional darkness is banished\n" +
                "Blindness becomes limited darkness.";

            return
                SubterraneanSitesEPAttunementBuildResult
                    .Success;
        }


        public void RegisterCategory2(
            SubterraneanSitesEPLayerContext context
        )
        {
            if (context == null)
                return;

            //
            // Darkness C2:
            // relatively few wall-mounted vents, but each creates
            // a broad blindness-gas plume.
            //
            SubterraneanSitesEPBoundaryHazards
                .RegisterDirectional(
                    context,
                    "SubterraneanSitesDarknessBlindVentN",
                    "SubterraneanSitesDarknessBlindVentS",
                    "SubterraneanSitesDarknessBlindVentE",
                    "SubterraneanSitesDarknessBlindVentW",

                    // Darkness vent density is tuned independently from Cold.
                    10,
                    16,

                    // keep the vents meaningfully separated
                    6,

                    // don't crowd vertical transitions
                    5,

                    // roughly Cold-like trigger cadence
                    6
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
                "SubterraneanSitesDarknessMaterials"
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
                "SubterraneanSitesDarknessLayout"
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
                "SubterraneanSitesDarknessFloor"
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
                "SubterraneanSitesDarknessDecorations"
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
                "SubterraneanSitesDarknessFloor",
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
                "SubterraneanSitesDarknessDecorations",
                "EntranceOnly", "1"
            );
        }


        internal static SubterraneanSitesEPFloorSpec
        CreateFloorSpec()
        {
            return
                new SubterraneanSitesEPFloorSpec
                {
                    FloorBlueprint =
                        "GreyMarbleFloor",

                    ConfigureFloorObject =
                        ConfigureFloorObject
                };
        }


        private static void ConfigureFloorObject(
            GameObject floor
        )
        {
            if (
                floor == null ||
                floor.Render == null
            )
            {
                return;
            }

            //
            // Keep GreyMarbleFloor's native graphical tile.
            // We are only varying the shade of its existing tiny floor mark.
            //
            int roll =
                Stat.Random(
                    1,
                    100
                );

            if (roll <= 4)
            {
                //
                // Rare white fleck.
                //
                floor.Render.ColorString =
                    "&Y^k";

                floor.Render.TileColor =
                    "&Y";

                floor.Render.DetailColor =
                    "k";
            }
            else if (roll <= 16)
            {
                //
                // Light grey fleck.
                //
                floor.Render.ColorString =
                    "&y^k";

                floor.Render.TileColor =
                    "&y";

                floor.Render.DetailColor =
                    "k";
            }
            else if (roll <= 35)
            {
                //
                // Dark grey fleck.
                //
                floor.Render.ColorString =
                    "&K^k";

                floor.Render.TileColor =
                    "&K";

                floor.Render.DetailColor =
                    "k";
            }
            else
            {
                //
                // Quiet dark substrate.
                //
                floor.Render.ColorString =
                    "&k^k";

                floor.Render.TileColor =
                    "&k";

                floor.Render.DetailColor =
                    "k";
            }
        }
    }     
}

namespace SubterraneanSites
{
    /// <summary>
    /// Darkness Category-1 runtime environment.
    ///
    /// Underground Darkness-C1 layers are entirely blacked out.
    ///
    /// On the preserved origin zone, only the extradimensional entrance
    /// scar is blacked out.
    ///
    /// The actual render manipulation lives in a player effect so it runs
    /// at the correct BeforeRender timing.
    /// </summary>
    [Serializable]
    public class SubterraneanSitesEPDarknessEnvironmentSystem :
        IGameSystem
    {
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
            RefreshPlayerState();

            return true;
        }


        public override bool HandleEvent(
            EndTurnEvent E
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

            bool darknessEnvironment =
                string.Equals(
                    category1,
                    "Darkness",
                    StringComparison.Ordinal
                );

            XRL.World.Effects
                .SubterraneanSitesEPDarknessEnvironmentEffect
                existing =
                    player.GetEffectDescendedFrom<
                        XRL.World.Effects
                            .SubterraneanSitesEPDarknessEnvironmentEffect
                    >();


            if (darknessEnvironment)
            {
                if (existing == null)
                {
                    existing =
                        new XRL.World.Effects
                            .SubterraneanSitesEPDarknessEnvironmentEffect();

                    if (
                        !player.ApplyEffect(
                            existing
                        )
                    )
                    {
                        return;
                    }
                }

                bool darknessAttuned =
                    SubterraneanSitesEPAttunementSystem
                        .IsAttunedTo(
                            player,
                            "Darkness"
                        );

                existing.SynchronizeAttunement(
                    darknessAttuned
                );

                return;
            }


            if (existing != null)
            {
                player.RemoveEffect(
                    existing
                );
            }
        }
    }
}


namespace XRL.World.ZoneBuilders
{
    /// <summary>
    /// Darkness Category-4 floor adapter.
    ///
    /// Darkness owns the GreyMarbleFloor recipe and sparse wear.
    /// The shared EP floor system owns universal substrate application.
    /// </summary>
    public class SubterraneanSitesDarknessFloor :
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
                            .SubterraneanSitesEPDarknessTheme
                            .CreateFloorSpec(),
                        EntranceOnly != 0
                    );
        }
    }

    /// <summary>
    /// Darkness Category-5 scenery.
    ///
    /// Remnants of inhabited interior space:
    /// - ornate benches, chairs, and tables
    /// - underground air wells and catch basins
    /// - oil vases and wine pitchers
    /// - vantablooms
    /// - garbage and bones
    /// - rubble and small grey boulders
    ///
    /// White spinefruit is intentionally deferred.
    /// </summary>
    public class SubterraneanSitesDarknessDecorations :
        ZoneBuilderSandbox
    {
        public int EntranceOnly = 0;

        public int TransitionExclusionRadius = 4;


        public string BenchBlueprint =
            "Ornate Bench";

        public string ChairBlueprint =
            "Ornate Chair";

        public string TableBlueprint =
            "Ornate Table";

        public string AirWellBlueprint =
            "Underground Air Well";

        public string CatchbasinBlueprint =
            "Underground Catchbasin";

        public string OilVaseBlueprint =
            "SubterraneanSitesDarknessOilVase";

        public string WinePitcherBlueprint =
            "Wine Pitcher";

        public string VantabloomBlueprint =
            "Vantabloom";

        public string GarbageBlueprint =
            "Garbage";

        public string BonesBlueprint =
            "Bones";

        public string RubbleBlueprint =
            "SubterraneanSitesColdRubble";

        public string SmallBoulderBlueprint =
            "SmallBoulder Grey";

        public string MarbleWalkwayBlueprint =
            "SubterraneanSitesDarknessWhiteMarbleWalkway";

        public string FractiBlueprint =
            "SubterraneanSitesDarknessWhiteFracti";

        public string DecorationThemeKey =
            "Darkness";

        public bool BuildZone(
            Zone Z
        )
        {
            if (Z == null)
                return true;

            int seed =
                XRLCore.Core.Game.GetWorldSeed(
                    "SubterraneanSites:DarknessDecorations:" +
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


            //
            // Establish the inhabited-looking furnishing clusters first.
            //
            List<Cell> benchCells =
                PlaceSittingAreas(
                    Z,
                    anchors,
                    rng
                );


            //
            // A couple pieces of old water infrastructure occur near
            // the sitting areas rather than randomly across the zone.
            //
            PlaceAirWellPairs(
                Z,
                anchors,
                benchCells,
                rng
            );


            //
            // Smaller movable/storage objects.
            //
            PlaceScatter(
                Z,
                anchors,
                rng,
                OilVaseBlueprint,
                EntranceOnly != 0 ? 1 : 5,
                EntranceOnly != 0 ? 2 : 8,
                requireBroadOpen: false
            );

            PlaceScatter(
                Z,
                anchors,
                rng,
                WinePitcherBlueprint,
                EntranceOnly != 0 ? 0 : 2,
                EntranceOnly != 0 ? 1 : 4,
                requireBroadOpen: false
            );


            //
            // Local magical-darkness flora.
            //
            PlaceVantabloomGardens(
                Z,
                anchors,
                rng
            );

            PlaceFractiHedges(
                Z,
                anchors,
                rng
            );


            //
            // Abandonment / deterioration.
            //
            PlaceScatter(
                Z,
                anchors,
                rng,
                GarbageBlueprint,
                EntranceOnly != 0 ? 2 : 10,
                EntranceOnly != 0 ? 4 : 18,
                requireBroadOpen: false
            );

            PlaceBoneRuins(
                Z,
                anchors,
                rng
            );


            PlaceDebrisClusters(
                Z,
                anchors,
                rng
            );

            return true;
        }

        private void PlaceBoneRuins(
            Zone Z,
            List<Location2D> anchors,
            System.Random rng
        )
        {
            int desired =
                EntranceOnly != 0
                    ? rng.Next(1, 3)
                    : rng.Next(6, 11);

            for (
                int i = 0;
                i < desired;
                i++
            )
            {
                Cell boneCell =
                    SubterraneanSites
                        .SubterraneanSitesEPPlacement
                        .PickRandomCell(
                            Z,
                            delegate(Cell cell)
                            {
                                return
                                    IsCandidate(
                                        Z,
                                        cell,
                                        anchors
                                    );
                            },
                            rng
                        );

                if (boneCell == null)
                    continue;

                //
                // Some remains lie in or at the edge of surviving masonry.
                //
                if (
                    rng.Next(100) <
                    70
                )
                {
                    int target =
                        rng.Next(
                            4,
                            10
                        );

                    HashSet<Cell> patch =
                        SubterraneanSites
                            .SubterraneanSitesEPPlacement
                            .GrowPatch(
                                boneCell,
                                target,
                                delegate(Cell cell)
                                {
                                    return
                                        cell != null &&
                                        (
                                            EntranceOnly != 0
                                                ? SubterraneanSites
                                                    .SubterraneanSitesEPEntrance
                                                    .IsInsideScar(
                                                        Z,
                                                        cell
                                                    )
                                                : SubterraneanSites
                                                    .SubterraneanSitesEPGeometry
                                                    .IsOpenGeometryCell(
                                                        cell
                                                    )
                                        );
                                },
                                rng
                            );

                    foreach (
                        Cell cell
                        in patch
                    )
                    {
                        if (
                            cell != null &&
                            !cell.HasObjectWithBlueprint(
                                MarbleWalkwayBlueprint
                            )
                        )
                        {
                            cell.AddObject(
                                MarbleWalkwayBlueprint
                            );
                        }
                    }
                }

                PlaceAndClaim(
                    Z,
                    boneCell,
                    BonesBlueprint
                );
            }
        }

        private void PlaceFractiHedges(
            Zone Z,
            List<Location2D> anchors,
            System.Random rng
        )
        {
            int hedgeCount =
                EntranceOnly != 0
                    ? rng.Next(2, 4)
                    : rng.Next(10, 16);

            for (
                int hedge = 0;
                hedge < hedgeCount;
                hedge++
            )
            {
                Cell seed =
                    SubterraneanSites
                        .SubterraneanSitesEPPlacement
                        .PickRandomCell(
                            Z,
                            delegate(Cell cell)
                            {
                                return
                                    IsCandidate(
                                        Z,
                                        cell,
                                        anchors
                                    );
                            },
                            rng
                        );

                if (seed == null)
                    continue;

                bool horizontal =
                    rng.Next(2) == 0;

                int direction =
                    rng.Next(2) == 0
                        ? -1
                        : 1;

                int length =
                    rng.Next(
                        5,
                        12
                    );

                int x =
                    seed.X;

                int y =
                    seed.Y;

                int placed =
                    0;

                for (
                    int step = 0;
                    step < length;
                    step++
                )
                {
                    Cell cell =
                        Z.GetCell(
                            x,
                            y
                        );

                    if (
                        cell == null ||
                        !IsCandidate(
                            Z,
                            cell,
                            anchors
                        )
                    )
                    {
                        break;
                    }

                    if (
                        PlaceAndClaim(
                            Z,
                            cell,
                            FractiBlueprint
                        )
                    )
                    {
                        placed++;
                    }

                    //
                    // A minority of hedges make one right-angle turn.
                    //
                    if (
                        step >= 2 &&
                        step < length - 2 &&
                        rng.Next(100) < 12
                    )
                    {
                        horizontal =
                            !horizontal;

                        direction =
                            rng.Next(2) == 0
                                ? -1
                                : 1;
                    }

                    if (horizontal)
                    {
                        x +=
                            direction;
                    }
                    else
                    {
                        y +=
                            direction;
                    }
                }
            }
        }

        private void PlaceVantabloomGardens(
            Zone Z,
            List<Location2D> anchors,
            System.Random rng
        )
        {
            int gardenCount =
                EntranceOnly != 0
                    ? 1
                    : rng.Next(
                        2,
                        4
                    );

            for (
                int garden = 0;
                garden < gardenCount;
                garden++
            )
            {
                Cell seed =
                    SubterraneanSites
                        .SubterraneanSitesEPPlacement
                        .PickRandomCell(
                            Z,
                            delegate(Cell cell)
                            {
                                return
                                    IsCandidate(
                                        Z,
                                        cell,
                                        anchors
                                    );
                            },
                            rng
                        );

                if (seed == null)
                    continue;

                int desired =
                    EntranceOnly != 0
                        ? rng.Next(1, 3)
                        : rng.Next(2, 5);

                HashSet<Cell> patch =
                    SubterraneanSites
                        .SubterraneanSitesEPPlacement
                        .GrowPatch(
                            seed,
                            desired,
                            delegate(Cell cell)
                            {
                                return
                                    IsCandidate(
                                        Z,
                                        cell,
                                        anchors
                                    );
                            },
                            rng
                        );

                foreach (
                    Cell cell
                    in patch
                )
                {
                    if (cell == null)
                        continue;

                    GameObject bloom =
                        GameObject.Create(
                            VantabloomBlueprint
                        );

                    if (bloom == null)
                        continue;

                    //
                    // Same dimensional-decoration contract used by Fungus living
                    // decorative creatures: extradimensional identity + assigned
                    // dimensional faction, but no denizen mutation/adaptation package.
                    //
                    SubterraneanSites
                        .SubterraneanSitesDimensionEngine
                        .ApplyDecorationDimensionIdentity(
                            bloom,
                            DecorationThemeKey
                        );

                    cell.AddObject(
                        bloom
                    );

                    bloom.MakeActive();

                    SubterraneanSites
                        .SubterraneanSitesEPReservations
                        .ClaimCell(
                            Z,
                            cell
                        );
                }
            }
        }


        private List<Cell> PlaceSittingAreas(
            Zone Z,
            List<Location2D> anchors,
            System.Random rng
        )
        {
            List<Cell> benches =
                new List<Cell>();

            int minimum =
                EntranceOnly != 0
                    ? 1
                    : 6;

            int maximum =
                EntranceOnly != 0
                    ? 2
                    : 8;

            int desired =
                rng.Next(
                    minimum,
                    maximum + 1
                );

            for (
                int i = 0;
                i < desired;
                i++
            )
            {
                Cell benchCell =
                    SubterraneanSites
                        .SubterraneanSitesEPPlacement
                        .PickRandomCell(
                            Z,
                            delegate(Cell cell)
                            {
                                return
                                    IsBroadCandidate(
                                        Z,
                                        cell,
                                        anchors
                                    );
                            },
                            rng
                        );

                if (benchCell == null)
                    break;

                if (
                    !PlaceAndClaim(
                        Z,
                        benchCell,
                        BenchBlueprint
                    )
                )
                {
                    continue;
                }

                benches.Add(
                    benchCell
                );

                HashSet<Cell> patch =
                    new HashSet<Cell>
                    {
                        benchCell
                    };

                PlaceSurvivingMarblePatch(
                    Z,
                    benchCell,
                    rng
                );


                //
                // Most sitting areas get a table.
                //
                if (
                    rng.Next(100) <
                    70
                )
                {
                    PlaceNearPatch(
                        Z,
                        anchors,
                        patch,
                        rng,
                        TableBlueprint,
                        1,
                        3,
                        requireBroadOpen: true
                    );
                }


                //
                // Chairs are common but not mandatory.
                //
                if (
                    rng.Next(100) <
                    65
                )
                {
                    PlaceNearPatch(
                        Z,
                        anchors,
                        patch,
                        rng,
                        ChairBlueprint,
                        1,
                        3,
                        requireBroadOpen: true
                    );
                }

                //
                // Some rooms get another chair.
                //
                if (
                    rng.Next(100) <
                    25
                )
                {
                    PlaceNearPatch(
                        Z,
                        anchors,
                        patch,
                        rng,
                        ChairBlueprint,
                        1,
                        3,
                        requireBroadOpen: true
                    );
                }
            }

            return benches;
        }

        private void PlaceSurvivingMarblePatch(
            Zone Z,
            Cell seed,
            System.Random rng
        )
        {
            if (
                Z == null ||
                seed == null ||
                rng == null
            )
            {
                return;
            }

            int target =
                rng.Next(
                    8,
                    17
                );

            HashSet<Cell> patch =
                SubterraneanSites
                    .SubterraneanSitesEPPlacement
                    .GrowPatch(
                        seed,
                        target,
                        delegate(Cell cell)
                        {
                            if (cell == null)
                                return false;

                            if (EntranceOnly != 0)
                            {
                                return
                                    SubterraneanSites
                                        .SubterraneanSitesEPEntrance
                                        .IsInsideScar(
                                            Z,
                                            cell
                                        );
                            }

                            return
                                SubterraneanSites
                                    .SubterraneanSitesEPGeometry
                                    .IsOpenGeometryCell(
                                        cell
                                    );
                        },
                        rng
                    );

            foreach (
                Cell cell
                in patch
            )
            {
                if (cell == null)
                    continue;

                //
                // This is substrate-like visual content.
                // Deliberately ignore reservations: the marble belongs underneath
                // benches, chairs, bones, vases, etc.
                //
                if (
                    !cell.HasObjectWithBlueprint(
                        MarbleWalkwayBlueprint
                    )
                )
                {
                    cell.AddObject(
                        MarbleWalkwayBlueprint
                    );
                }
            }
        }


        private void PlaceAirWellPairs(
            Zone Z,
            List<Location2D> anchors,
            List<Cell> benchCells,
            System.Random rng
        )
        {
            if (
                Z == null ||
                benchCells == null ||
                benchCells.Count == 0
            )
            {
                return;
            }

            List<Cell> availableBenches =
                new List<Cell>(
                    benchCells
                );

            int desiredPairs =
                EntranceOnly != 0
                    ? 1
                    : 2;

            for (
                int i = 0;
                i < desiredPairs &&
                availableBenches.Count > 0;
                i++
            )
            {
                int index =
                    rng.Next(
                        availableBenches.Count
                    );

                Cell bench =
                    availableBenches[
                        index
                    ];

                availableBenches.RemoveAt(
                    index
                );

                if (bench == null)
                    continue;

                HashSet<Cell> benchPatch =
                    new HashSet<Cell>
                    {
                        bench
                    };

                Cell wellCell =
                    SubterraneanSites
                        .SubterraneanSitesEPPlacement
                        .PickRandomCellNearPatch(
                            Z,
                            benchPatch,
                            1,
                            3,
                            delegate(Cell candidate)
                            {
                                return
                                    IsBroadCandidate(
                                        Z,
                                        candidate,
                                        anchors
                                    );
                            },
                            rng
                        );

                if (wellCell == null)
                    continue;

                if (
                    !PlaceAndClaim(
                        Z,
                        wellCell,
                        AirWellBlueprint
                    )
                )
                {
                    continue;
                }

                HashSet<Cell> wellPatch =
                    new HashSet<Cell>
                    {
                        wellCell
                    };

                Cell basinCell =
                    SubterraneanSites
                        .SubterraneanSitesEPPlacement
                        .PickRandomCellNearPatch(
                            Z,
                            wellPatch,
                            1,
                            1,
                            delegate(Cell candidate)
                            {
                                return
                                    IsCandidate(
                                        Z,
                                        candidate,
                                        anchors
                                    );
                            },
                            rng
                        );

                if (basinCell == null)
                    continue;

                PlaceAndClaim(
                    Z,
                    basinCell,
                    CatchbasinBlueprint
                );
            }
        }


        private void PlaceScatter(
            Zone Z,
            List<Location2D> anchors,
            System.Random rng,
            string blueprint,
            int minimum,
            int maximum,
            bool requireBroadOpen
        )
        {
            if (
                Z == null ||
                rng == null ||
                blueprint.IsNullOrEmpty()
            )
            {
                return;
            }

            if (minimum < 0)
                minimum = 0;

            if (maximum < minimum)
                maximum = minimum;

            int desired =
                rng.Next(
                    minimum,
                    maximum + 1
                );

            for (
                int i = 0;
                i < desired;
                i++
            )
            {
                Cell cell =
                    SubterraneanSites
                        .SubterraneanSitesEPPlacement
                        .PickRandomCell(
                            Z,
                            delegate(Cell candidate)
                            {
                                if (requireBroadOpen)
                                {
                                    return
                                        IsBroadCandidate(
                                            Z,
                                            candidate,
                                            anchors
                                        );
                                }

                                return
                                    IsCandidate(
                                        Z,
                                        candidate,
                                        anchors
                                    );
                            },
                            rng
                        );

                if (cell == null)
                    return;

                PlaceAndClaim(
                    Z,
                    cell,
                    blueprint
                );
            }
        }


        private void PlaceDebrisClusters(
            Zone Z,
            List<Location2D> anchors,
            System.Random rng
        )
        {
            int clusterCount =
                EntranceOnly != 0
                    ? 1
                    : rng.Next(
                        2,
                        4
                    );

            for (
                int i = 0;
                i < clusterCount;
                i++
            )
            {
                int target =
                    EntranceOnly != 0
                        ? rng.Next(
                            2,
                            4
                        )
                        : rng.Next(
                            3,
                            6
                        );

                Cell seed =
                    SubterraneanSites
                        .SubterraneanSitesEPPlacement
                        .PickRandomCell(
                            Z,
                            delegate(Cell candidate)
                            {
                                return
                                    IsBroadCandidate(
                                        Z,
                                        candidate,
                                        anchors
                                    );
                            },
                            rng
                        );

                if (seed == null)
                    continue;

                HashSet<Cell> patch =
                    SubterraneanSites
                        .SubterraneanSitesEPPlacement
                        .GrowPatch(
                            seed,
                            target,
                            delegate(Cell candidate)
                            {
                                return
                                    IsBroadCandidate(
                                        Z,
                                        candidate,
                                        anchors
                                    );
                            },
                            rng
                        );

                foreach (
                    Cell cell
                    in patch
                )
                {
                    if (cell == null)
                        continue;

                    string blueprint =
                        rng.Next(100) <
                        65
                            ? RubbleBlueprint
                            : SmallBoulderBlueprint;

                    PlaceAndClaim(
                        Z,
                        cell,
                        blueprint
                    );
                }
            }
        }


        private bool PlaceNearPatch(
            Zone Z,
            List<Location2D> anchors,
            HashSet<Cell> patch,
            System.Random rng,
            string blueprint,
            int minimumDistance,
            int maximumDistance,
            bool requireBroadOpen
        )
        {
            if (
                Z == null ||
                patch == null ||
                patch.Count == 0 ||
                rng == null
            )
            {
                return false;
            }

            Cell cell =
                SubterraneanSites
                    .SubterraneanSitesEPPlacement
                    .PickRandomCellNearPatch(
                        Z,
                        patch,
                        minimumDistance,
                        maximumDistance,
                        delegate(Cell candidate)
                        {
                            if (requireBroadOpen)
                            {
                                return
                                    IsBroadCandidate(
                                        Z,
                                        candidate,
                                        anchors
                                    );
                            }

                            return
                                IsCandidate(
                                    Z,
                                    candidate,
                                    anchors
                                );
                        },
                        rng
                    );

            if (cell == null)
                return false;

            return
                PlaceAndClaim(
                    Z,
                    cell,
                    blueprint
                );
        }


        private bool IsBroadCandidate(
            Zone Z,
            Cell cell,
            List<Location2D> anchors
        )
        {
            if (
                !IsCandidate(
                    Z,
                    cell,
                    anchors
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
                            return
                                IsCandidate(
                                    Z,
                                    neighbor,
                                    anchors
                                );
                        }
                    );
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

            if (EntranceOnly != 0)
            {
                if (
                    !SubterraneanSites
                        .SubterraneanSitesEPEntrance
                        .IsInsideScar(
                            Z,
                            cell
                        ) ||
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
                !cell.IsEmptyOfSolid()
            )
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


        private bool PlaceAndClaim(
            Zone Z,
            Cell cell,
            string blueprint
        )
        {
            if (
                Z == null ||
                cell == null ||
                blueprint.IsNullOrEmpty()
            )
            {
                return false;
            }

            GameObject placed =
                cell.AddObject(
                    blueprint
                );

            if (placed == null)
                return false;

            SubterraneanSites
                .SubterraneanSitesEPReservations
                .ClaimCell(
                    Z,
                    cell
                );

            return true;
        }
    }

    


    /// <summary>
    /// Darkness Category 4.
    ///
    /// Starts with the shared solid-placeholder field and recursively
    /// subdivides almost the entire zone into connected rectangular rooms.
    ///
    /// Each recursive split reserves one row or column of wall between its
    /// children, then carves a narrow opening through that divider.
    ///
    /// Unlike Electrical:
    ///     rooms are not separate boxes joined by long corridors.
    ///
    /// The entire Darkness interior is one densely partitioned architectural
    /// footprint.
    /// </summary>
    public class SubterraneanSitesDarknessLayout :
        ZoneBuilderSandbox
    {
        //
        // Absolute outer shell.
        //
        public int Border = 1;

        //
        // Minimum dimensions of a terminal room.
        //
        // A split needs room for:
        //     child + divider wall + child
        //
        public int MinRoomWidth = 6;
        public int MinRoomHeight = 4;

        //
        // Most connections are one cell wide.
        // Some are two cells wide to break up the regularity.
        //
        public int WideOpeningChance = 10;

        public int StubChance = 75;
        public int ExtraStubChance = 35;

        //
        // Keep transition landings deliberately open.
        //
        public int AnchorRadius = 2;

        //
        // Parent dividers wander around their nominal split line instead
        // of crossing the whole zone as one perfectly straight axis.
        //
        public int DividerMaxOffset = 2;

        public int DividerMinRun = 3;

        public int DividerMaxRun = 6;


        //
        // One or two local areas are rebuilt according to a different,
        // fragmentary structural rule. This breaks the sense that the
        // entire zone came from one global plan.
        //
        public int DisruptionStampCount = 2;

        public int DisruptionMinRadius = 5;

        public int DisruptionMaxRadius = 8;

        public int DisruptionMinSegments = 5;

public int DisruptionMaxSegments = 9;


        private sealed class RoomRect
        {
            public int X1;
            public int Y1;
            public int X2;
            public int Y2;


            public int Width
            {
                get
                {
                    return
                        X2 -
                        X1 +
                        1;
                }
            }


            public int Height
            {
                get
                {
                    return
                        Y2 -
                        Y1 +
                        1;
                }
            }
        }


        public bool BuildZone(
            Zone Z
        )
        {
            if (Z == null)
                return true;

            ClampSettings();

            int seed =
                XRLCore.Core.Game.GetWorldSeed(
                    "SubterraneanSites:DarknessLayout:" +
                    Z.ZoneID
                );

            System.Random rng =
                new System.Random(
                    seed
                );

            RoomRect root =
                new RoomRect
                {
                    X1 = Border,
                    Y1 = Border,
                    X2 = Z.Width - Border - 1,
                    Y2 = Z.Height - Border - 1
                };

            if (
                root.X2 < root.X1 ||
                root.Y2 < root.Y1
            )
            {
                return true;
            }

            //
            // Recursively carve terminal rooms while leaving each recursive
            // divider as shared semantic solid geometry.
            //
            PartitionAndCarve(
                Z,
                root,
                rng
            );

            //
            // Break the global BSP grammar in a few local regions.
            //
            ApplyDisruptionStamps(
                Z,
                rng
            );

            //
            // A recursive divider can legally cross an EP transition anchor.
            // Clear a modest landing area around every required transition.
            //
            List<Location2D> anchors =
                SubterraneanSites
                    .SubterraneanSitesEPVerticalTransitions
                    .GetVerticalAnchors(
                        Z.ZoneID
                    );

            if (anchors != null)
            {
                foreach (
                    Location2D anchor
                    in anchors
                )
                {
                    if (anchor == null)
                        continue;

                    CarveAnchorFootprint(
                        Z,
                        anchor.X,
                        anchor.Y
                    );
                }
            }


            //
            // Darkness should not leave incidental sealed empty rooms.
            //
            // Preserve:
            //   - the largest connected open region
            //   - any region containing a required vertical transition anchor
            //
            // Everything else returns to structural fill.
            //

            FillDisconnectedOpenRegions(
                Z,
                anchors
            );


            //
            // The absolute zone perimeter remains a hard dimensional shell.
            //
            ForceSealedBorder(
                Z
            );

            Z.ClearReachableMap();

            return true;
        }


        private void PartitionAndCarve(
            Zone Z,
            RoomRect room,
            System.Random rng
        )
        {
            if (
                Z == null ||
                room == null ||
                rng == null
            )
            {
                return;
            }

            bool canSplitVertical =
                room.Width >=
                    MinRoomWidth * 2 + 1;

            bool canSplitHorizontal =
                room.Height >=
                    MinRoomHeight * 2 + 1;

            //
            // Terminal partition:
            // this entire rectangle becomes one room.
            //
            if (
                !canSplitVertical &&
                !canSplitHorizontal
            )
            {
                CarveRoom(
                    Z,
                    room
                );

                AddTerminalSubdividers(
                    Z,
                    room,
                    rng
                );

                return;
            }

            bool splitVertical;

            if (
                canSplitVertical &&
                !canSplitHorizontal
            )
            {
                splitVertical =
                    true;
            }
            else if (
                canSplitHorizontal &&
                !canSplitVertical
            )
            {
                splitVertical =
                    false;
            }
            else
            {
                //
                // Very wide regions preferentially split vertically.
                // Tall/square-ish regions get horizontal opportunities.
                // Otherwise allow variation.
                //
                if (
                    room.Width >=
                    room.Height * 2
                )
                {
                    splitVertical =
                        true;
                }
                else if (
                    room.Height >=
                    room.Width
                )
                {
                    splitVertical =
                        false;
                }
                else
                {
                    splitVertical =
                        rng.Next(
                            2
                        ) == 0;
                }
            }

            if (splitVertical)
            {
                SplitVertical(
                    Z,
                    room,
                    rng
                );
            }
            else
            {
                SplitHorizontal(
                    Z,
                    room,
                    rng
                );
            }
        }


        private void SplitVertical(
            Zone Z,
            RoomRect room,
            System.Random rng
        )
        {
            int minimum =
                room.X1 +
                MinRoomWidth;

            int maximum =
                room.X2 -
                MinRoomWidth;

            if (maximum < minimum)
            {
                CarveRoom(
                    Z,
                    room
                );

                return;
            }

            int center =
                (
                    room.X1 +
                    room.X2
                ) / 2;

            int jitter =
                Math.Max(
                    1,
                    room.Width / 8
                );

            int splitX =
                center +
                rng.Next(
                    -jitter,
                    jitter + 1
                );

            if (splitX < minimum)
                splitX = minimum;

            if (splitX > maximum)
                splitX = maximum;

            //
            // splitX itself remains wall.
            //
            RoomRect left =
                new RoomRect
                {
                    X1 = room.X1,
                    Y1 = room.Y1,
                    X2 = splitX - 1,
                    Y2 = room.Y2
                };

            RoomRect right =
                new RoomRect
                {
                    X1 = splitX + 1,
                    Y1 = room.Y1,
                    X2 = room.X2,
                    Y2 = room.Y2
                };

            PartitionAndCarve(
                Z,
                left,
                rng
            );

            PartitionAndCarve(
                Z,
                right,
                rng
            );

            BuildJoggedVerticalDivider(
                Z,
                splitX,
                room.Y1,
                room.Y2,
                rng
            );

        }


        private void SplitHorizontal(
            Zone Z,
            RoomRect room,
            System.Random rng
        )
        {
            int minimum =
                room.Y1 +
                MinRoomHeight;

            int maximum =
                room.Y2 -
                MinRoomHeight;

            if (maximum < minimum)
            {
                CarveRoom(
                    Z,
                    room
                );

                return;
            }

            int center =
                (
                    room.Y1 +
                    room.Y2
                ) / 2;

            int jitter =
                Math.Max(
                    1,
                    room.Height / 8
                );

            int splitY =
                center +
                rng.Next(
                    -jitter,
                    jitter + 1
                );

            if (splitY < minimum)
                splitY = minimum;

            if (splitY > maximum)
                splitY = maximum;

            //
            // splitY itself remains wall.
            //
            RoomRect top =
                new RoomRect
                {
                    X1 = room.X1,
                    Y1 = room.Y1,
                    X2 = room.X2,
                    Y2 = splitY - 1
                };

            RoomRect bottom =
                new RoomRect
                {
                    X1 = room.X1,
                    Y1 = splitY + 1,
                    X2 = room.X2,
                    Y2 = room.Y2
                };

            PartitionAndCarve(
                Z,
                top,
                rng
            );

            PartitionAndCarve(
                Z,
                bottom,
                rng
            );

            BuildJoggedHorizontalDivider(
                Z,
                splitY,
                room.X1,
                room.X2,
                rng
            );
        }

        private void FillDisconnectedOpenRegions(
            Zone Z,
            List<Location2D> anchors
        )
        {
            if (Z == null)
                return;

            bool[,] visited =
                new bool[
                    Z.Width,
                    Z.Height
                ];

            List<List<Cell>> regions =
                new List<List<Cell>>();

            //
            // Find all cardinally connected open regions.
            //
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
                    if (
                        visited[x, y] ||
                        IsForcedBorder(
                            Z,
                            x,
                            y
                        )
                    )
                    {
                        continue;
                    }

                    Cell start =
                        Z.GetCell(
                            x,
                            y
                        );

                    if (
                        start == null ||
                        start.HasWall()
                    )
                    {
                        continue;
                    }

                    List<Cell> region =
                        new List<Cell>();

                    Queue<Cell> queue =
                        new Queue<Cell>();

                    visited[x, y] =
                        true;

                    queue.Enqueue(
                        start
                    );

                    while (
                        queue.Count > 0
                    )
                    {
                        Cell current =
                            queue.Dequeue();

                        region.Add(
                            current
                        );

                        TryQueueOpenCell(
                            Z,
                            current.X + 1,
                            current.Y,
                            visited,
                            queue
                        );

                        TryQueueOpenCell(
                            Z,
                            current.X - 1,
                            current.Y,
                            visited,
                            queue
                        );

                        TryQueueOpenCell(
                            Z,
                            current.X,
                            current.Y + 1,
                            visited,
                            queue
                        );

                        TryQueueOpenCell(
                            Z,
                            current.X,
                            current.Y - 1,
                            visited,
                            queue
                        );
                    }

                    regions.Add(
                        region
                    );
                }
            }

            if (
                regions.Count <= 1
            )
            {
                return;
            }

            //
            // Always preserve the largest region.
            //
            List<Cell> largest =
                null;

            foreach (
                List<Cell> region
                in regions
            )
            {
                if (
                    largest == null ||
                    region.Count >
                    largest.Count
                )
                {
                    largest =
                        region;
                }
            }


            foreach (
                List<Cell> region
                in regions
            )
            {
                if (
                    region ==
                    largest
                )
                {
                    continue;
                }

                bool containsAnchor =
                    false;

                if (anchors != null)
                {
                    foreach (
                        Location2D anchor
                        in anchors
                    )
                    {
                        if (anchor == null)
                            continue;

                        foreach (
                            Cell cell
                            in region
                        )
                        {
                            if (
                                cell.X ==
                                    anchor.X &&
                                cell.Y ==
                                    anchor.Y
                            )
                            {
                                containsAnchor =
                                    true;

                                break;
                            }
                        }

                        if (containsAnchor)
                            break;
                    }
                }

                if (containsAnchor)
                    continue;

                //
                // Isolated incidental chamber:
                // restore structural placeholder.
                //
                foreach (
                    Cell cell
                    in region
                )
                {
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

        private void TryQueueOpenCell(
            Zone Z,
            int x,
            int y,
            bool[,] visited,
            Queue<Cell> queue
        )
        {
            if (
                Z == null ||
                visited == null ||
                queue == null ||
                x < 0 ||
                y < 0 ||
                x >= Z.Width ||
                y >= Z.Height ||
                visited[x, y] ||
                IsForcedBorder(
                    Z,
                    x,
                    y
                )
            )
            {
                return;
            }

            visited[x, y] =
                true;

            Cell cell =
                Z.GetCell(
                    x,
                    y
                );

            if (
                cell == null ||
                cell.HasWall()
            )
            {
                return;
            }

            queue.Enqueue(
                cell
            );
        }

        private void ApplyDisruptionStamps(
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

            for (
                int stamp = 0;
                stamp < DisruptionStampCount;
                stamp++
            )
            {
                int radius =
                    rng.Next(
                        DisruptionMinRadius,
                        DisruptionMaxRadius + 1
                    );

                int minimumX =
                    Border +
                    radius +
                    1;

                int maximumX =
                    Z.Width -
                    Border -
                    radius -
                    2;

                int minimumY =
                    Border +
                    radius +
                    1;

                int maximumY =
                    Z.Height -
                    Border -
                    radius -
                    2;

                if (
                    maximumX < minimumX ||
                    maximumY < minimumY
                )
                {
                    continue;
                }

                int centerX =
                    rng.Next(
                        minimumX,
                        maximumX + 1
                    );

                int centerY =
                    rng.Next(
                        minimumY,
                        maximumY + 1
                    );

                //
                // First erase the existing structural grammar inside the
                // circular mask.
                //
                int radiusSquared =
                    radius *
                    radius;

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
                            radiusSquared
                        )
                        {
                            continue;
                        }

                        CarveCell(
                            Z,
                            centerX + dx,
                            centerY + dy
                        );
                    }
                }


                //
                // Then lay down an unrelated local collection of short
                // structural fragments.
                //
                int segmentCount =
                    rng.Next(
                        DisruptionMinSegments,
                        DisruptionMaxSegments + 1
                    );

                for (
                    int i = 0;
                    i < segmentCount;
                    i++
                )
                {
                    AddDisruptionSegment(
                        Z,
                        centerX,
                        centerY,
                        radius,
                        rng
                    );
                }
            }
        }


        private void AddDisruptionSegment(
            Zone Z,
            int centerX,
            int centerY,
            int radius,
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

            int radiusSquared =
                radius *
                radius;

            int startX =
                centerX;

            int startY =
                centerY;

            bool foundStart =
                false;

            //
            // Pick a point comfortably inside the circular mask.
            //
            for (
                int attempt = 0;
                attempt < 20;
                attempt++
            )
            {
                int candidateX =
                    centerX +
                    rng.Next(
                        -radius + 1,
                        radius
                    );

                int candidateY =
                    centerY +
                    rng.Next(
                        -radius + 1,
                        radius
                    );

                int dx =
                    candidateX -
                    centerX;

                int dy =
                    candidateY -
                    centerY;

                if (
                    dx * dx +
                    dy * dy <=
                    radiusSquared
                )
                {
                    startX =
                        candidateX;

                    startY =
                        candidateY;

                    foundStart =
                        true;

                    break;
                }
            }

            if (!foundStart)
                return;

            bool horizontal =
                rng.Next(
                    2
                ) == 0;

            int length =
                rng.Next(
                    3,
                    8
                );

            int direction =
                rng.Next(
                    2
                ) == 0
                    ? -1
                    : 1;


            int endX =
                startX;

            int endY =
                startY;

            for (
                int step = 0;
                step < length;
                step++
            )
            {
                int x =
                    startX +
                    (
                        horizontal
                            ? direction * step
                            : 0
                    );

                int y =
                    startY +
                    (
                        horizontal
                            ? 0
                            : direction * step
                    );

                int dx =
                    x -
                    centerX;

                int dy =
                    y -
                    centerY;

                if (
                    dx * dx +
                    dy * dy >
                    radiusSquared
                )
                {
                    break;
                }

                FillSolidCell(
                    Z,
                    x,
                    y
                );

                endX =
                    x;

                endY =
                    y;
            }


            //
            // About half the fragments get a short perpendicular branch,
            // yielding L/T-like pieces rather than only isolated bars.
            //
            if (
                rng.Next(
                    100
                ) >= 50
            )
            {
                return;
            }

            int branchLength =
                rng.Next(
                    2,
                    5
                );

            int branchDirection =
                rng.Next(
                    2
                ) == 0
                    ? -1
                    : 1;

            for (
                int step = 1;
                step <= branchLength;
                step++
            )
            {
                int x =
                    endX +
                    (
                        horizontal
                            ? 0
                            : branchDirection * step
                    );

                int y =
                    endY +
                    (
                        horizontal
                            ? branchDirection * step
                            : 0
                    );

                int dx =
                    x -
                    centerX;

                int dy =
                    y -
                    centerY;

                if (
                    dx * dx +
                    dy * dy >
                    radiusSquared
                )
                {
                    break;
                }

                FillSolidCell(
                    Z,
                    x,
                    y
                );
            }
        }


        private void CarveRoom(
            Zone Z,
            RoomRect room
        )
        {
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
                    CarveCell(
                        Z,
                        x,
                        y
                    );
                }
            }
        }

                private void AddTerminalSubdividers(
            Zone Z,
            RoomRect room,
            System.Random rng
        )
        {
            if (
                Z == null ||
                room == null ||
                rng == null
            )
            {
                return;
            }

            //
            // Only larger terminal rooms get secondary internal structure.
            //
            if (
                room.Width < 7 &&
                room.Height < 6
            )
            {
                return;
            }

            if (
                rng.Next(100) >= StubChance
            )
            {
                return;
            }

            int stubCount = 1;

            if (
                rng.Next(100) < ExtraStubChance &&
                (
                    room.Width >= 9 ||
                    room.Height >= 7
                )
            )
            {
                stubCount++;
            }

            for (
                int i = 0;
                i < stubCount;
                i++
            )
            {
                bool preferVertical;

                if (
                    room.Width > room.Height &&
                    room.Width >= 7
                )
                {
                    preferVertical =
                        true;
                }
                else if (
                    room.Height > room.Width &&
                    room.Height >= 6
                )
                {
                    preferVertical =
                        false;
                }
                else
                {
                    preferVertical =
                        rng.Next(2) == 0;
                }

                if (preferVertical)
                {
                    if (
                        !TryAddVerticalStub(
                            Z,
                            room,
                            rng
                        )
                    )
                    {
                        TryAddHorizontalStub(
                            Z,
                            room,
                            rng
                        );
                    }
                }
                else
                {
                    if (
                        !TryAddHorizontalStub(
                            Z,
                            room,
                            rng
                        )
                    )
                    {
                        TryAddVerticalStub(
                            Z,
                            room,
                            rng
                        );
                    }
                }
            }
        }


        private bool TryAddVerticalStub(
            Zone Z,
            RoomRect room,
            System.Random rng
        )
        {
            if (
                room == null ||
                room.Width < 7 ||
                room.Height < 5
            )
            {
                return false;
            }

            int minX =
                room.X1 + 2;

            int maxX =
                room.X2 - 2;

            if (maxX < minX)
            {
                return false;
            }

            int x =
                rng.Next(
                    minX,
                    maxX + 1
                );

            bool fromTop =
                rng.Next(2) == 0;

            int minLength =
                Math.Max(
                    2,
                    room.Height / 3
                );

            int maxLength =
                Math.Max(
                    minLength,
                    room.Height - 2
                );

            int length =
                rng.Next(
                    minLength,
                    maxLength + 1
                );

            int y1;
            int y2;

            if (fromTop)
            {
                y1 = room.Y1;
                y2 =
                    Math.Min(
                        room.Y2 - 1,
                        room.Y1 + length - 1
                    );
            }
            else
            {
                y1 =
                    Math.Max(
                        room.Y1 + 1,
                        room.Y2 - length + 1
                    );
                y2 = room.Y2;
            }

            if (y2 < y1)
            {
                return false;
            }

            for (
                int y = y1;
                y <= y2;
                y++
            )
            {
                FillSolidCell(
                    Z,
                    x,
                    y
                );
            }

            return true;
        }


        private bool TryAddHorizontalStub(
            Zone Z,
            RoomRect room,
            System.Random rng
        )
        {
            if (
                room == null ||
                room.Height < 6 ||
                room.Width < 6
            )
            {
                return false;
            }

            int minY =
                room.Y1 + 2;

            int maxY =
                room.Y2 - 2;

            if (maxY < minY)
            {
                return false;
            }

            int y =
                rng.Next(
                    minY,
                    maxY + 1
                );

            bool fromLeft =
                rng.Next(2) == 0;

            int minLength =
                Math.Max(
                    2,
                    room.Width / 3
                );

            int maxLength =
                Math.Max(
                    minLength,
                    room.Width - 2
                );

            int length =
                rng.Next(
                    minLength,
                    maxLength + 1
                );

            int x1;
            int x2;

            if (fromLeft)
            {
                x1 = room.X1;
                x2 =
                    Math.Min(
                        room.X2 - 1,
                        room.X1 + length - 1
                    );
            }
            else
            {
                x1 =
                    Math.Max(
                        room.X1 + 1,
                        room.X2 - length + 1
                    );
                x2 = room.X2;
            }

            if (x2 < x1)
            {
                return false;
            }

            for (
                int x = x1;
                x <= x2;
                x++
            )
            {
                FillSolidCell(
                    Z,
                    x,
                    y
                );
            }

            return true;
        }


        private void FillSolidCell(
            Zone Z,
            int x,
            int y
        )
        {
            if (
                Z == null ||
                x < 0 ||
                y < 0 ||
                x >= Z.Width ||
                y >= Z.Height ||
                IsForcedBorder(
                    Z,
                    x,
                    y
                )
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

            cell.Clear();

            cell.AddObject(
                SubterraneanSites
                    .SubterraneanSitesEPGeometry
                    .SolidPlaceholderBlueprint
            );
        }

        private void BuildJoggedVerticalDivider(
            Zone Z,
            int nominalX,
            int y1,
            int y2,
            System.Random rng
        )
        {
            if (
                Z == null ||
                rng == null ||
                y2 < y1
            )
            {
                return;
            }

            //
            // Remove the original perfectly straight BSP divider.
            //
            for (
                int y = y1;
                y <= y2;
                y++
            )
            {
                CarveCell(
                    Z,
                    nominalX,
                    y
                );
            }

            int span =
                y2 -
                y1 +
                1;

            int[] pathX =
                new int[
                    span
                ];

            int offset = 0;

            int currentX =
                nominalX;

            int runRemaining = 0;


            for (
                int y = y1;
                y <= y2;
                y++
            )
            {
                if (runRemaining <= 0)
                {
                    int previousX =
                        currentX;

                    offset +=
                        rng.Next(
                            -1,
                            2
                        );

                    offset =
                        Math.Max(
                            -DividerMaxOffset,
                            Math.Min(
                                DividerMaxOffset,
                                offset
                            )
                        );

                    currentX =
                        nominalX +
                        offset;

                    //
                    // Join the previous vertical run to the new one with
                    // a short horizontal elbow.
                    //
                    int connectorX1 =
                        Math.Min(
                            previousX,
                            currentX
                        );

                    int connectorX2 =
                        Math.Max(
                            previousX,
                            currentX
                        );

                    for (
                        int x = connectorX1;
                        x <= connectorX2;
                        x++
                    )
                    {
                        FillSolidCell(
                            Z,
                            x,
                            y
                        );
                    }

                    runRemaining =
                        rng.Next(
                            DividerMinRun,
                            DividerMaxRun + 1
                        );
                }

                pathX[
                    y -
                    y1
                ] =
                    currentX;

                FillSolidCell(
                    Z,
                    currentX,
                    y
                );

                runRemaining--;
            }


            //
            // Punch the passage through the actual wandered divider,
            // not through its old nominal position.
            //
            int openingY;

            if (
                y2 -
                y1 >= 2
            )
            {
                openingY =
                    rng.Next(
                        y1 + 1,
                        y2
                    );
            }
            else
            {
                openingY =
                    y1;
            }

            int openingX =
                pathX[
                    openingY -
                    y1
                ];

            for (
                int x = openingX - 1;
                x <= openingX + 1;
                x++
            )
            {
                CarveCell(
                    Z,
                    x,
                    openingY
                );
            }


            //
            // Occasional two-cell opening.
            //
            if (
                rng.Next(
                    100
                ) <
                WideOpeningChance &&
                openingY < y2
            )
            {
                int secondY =
                    openingY + 1;

                int secondX =
                    pathX[
                        secondY -
                        y1
                    ];

                for (
                    int x = secondX - 1;
                    x <= secondX + 1;
                    x++
                )
                {
                    CarveCell(
                        Z,
                        x,
                        secondY
                    );
                }
            }
        }


        private void BuildJoggedHorizontalDivider(
            Zone Z,
            int nominalY,
            int x1,
            int x2,
            System.Random rng
        )
        {
            if (
                Z == null ||
                rng == null ||
                x2 < x1
            )
            {
                return;
            }

            //
            // Remove the original perfectly straight BSP divider.
            //
            for (
                int x = x1;
                x <= x2;
                x++
            )
            {
                CarveCell(
                    Z,
                    x,
                    nominalY
                );
            }

            int span =
                x2 -
                x1 +
                1;

            int[] pathY =
                new int[
                    span
                ];

            int offset = 0;

            int currentY =
                nominalY;

            int runRemaining = 0;


            for (
                int x = x1;
                x <= x2;
                x++
            )
            {
                if (runRemaining <= 0)
                {
                    int previousY =
                        currentY;

                    offset +=
                        rng.Next(
                            -1,
                            2
                        );

                    offset =
                        Math.Max(
                            -DividerMaxOffset,
                            Math.Min(
                                DividerMaxOffset,
                                offset
                            )
                        );

                    currentY =
                        nominalY +
                        offset;

                    //
                    // Join horizontal runs with a short vertical elbow.
                    //
                    int connectorY1 =
                        Math.Min(
                            previousY,
                            currentY
                        );

                    int connectorY2 =
                        Math.Max(
                            previousY,
                            currentY
                        );

                    for (
                        int y = connectorY1;
                        y <= connectorY2;
                        y++
                    )
                    {
                        FillSolidCell(
                            Z,
                            x,
                            y
                        );
                    }

                    runRemaining =
                        rng.Next(
                            DividerMinRun,
                            DividerMaxRun + 1
                        );
                }

                pathY[
                    x -
                    x1
                ] =
                    currentY;

                FillSolidCell(
                    Z,
                    x,
                    currentY
                );

                runRemaining--;
            }


            int openingX;

            if (
                x2 -
                x1 >= 2
            )
            {
                openingX =
                    rng.Next(
                        x1 + 1,
                        x2
                    );
            }
            else
            {
                openingX =
                    x1;
            }

            int openingY =
                pathY[
                    openingX -
                    x1
                ];

            for (
                int y = openingY - 1;
                y <= openingY + 1;
                y++
            )
            {
                CarveCell(
                    Z,
                    openingX,
                    y
                );
            }


            if (
                rng.Next(
                    100
                ) <
                WideOpeningChance &&
                openingX < x2
            )
            {
                int secondX =
                    openingX + 1;

                int secondY =
                    pathY[
                        secondX -
                        x1
                    ];

                for (
                    int y = secondY - 1;
                    y <= secondY + 1;
                    y++
                )
                {
                    CarveCell(
                        Z,
                        secondX,
                        y
                    );
                }
            }
        }

        private void CarveAnchorFootprint(
            Zone Z,
            int centerX,
            int centerY
        )
        {
            int radiusSquared =
                AnchorRadius *
                AnchorRadius;

            for (
                int dx = -AnchorRadius;
                dx <= AnchorRadius;
                dx++
            )
            {
                for (
                    int dy = -AnchorRadius;
                    dy <= AnchorRadius;
                    dy++
                )
                {
                    if (
                        dx * dx +
                        dy * dy >
                        radiusSquared
                    )
                    {
                        continue;
                    }

                    CarveCell(
                        Z,
                        centerX + dx,
                        centerY + dy
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
                Z == null ||
                x < 0 ||
                y < 0 ||
                x >= Z.Width ||
                y >= Z.Height ||
                IsForcedBorder(
                    Z,
                    x,
                    y
                )
            )
            {
                return;
            }

            Cell cell =
                Z.GetCell(
                    x,
                    y
                );

            if (cell != null)
            {
                cell.ClearWalls();
            }
        }


        private void ForceSealedBorder(
            Zone Z
        )
        {
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
                    if (
                        !IsForcedBorder(
                            Z,
                            x,
                            y
                        )
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


        private bool IsForcedBorder(
            Zone Z,
            int x,
            int y
        )
        {
            return
                x < Border ||
                y < Border ||
                x >= Z.Width - Border ||
                y >= Z.Height - Border;
        }


        private void ClampSettings()
        {
            if (Border < 1)
                Border = 1;

            if (MinRoomWidth < 3)
                MinRoomWidth = 3;

            if (MinRoomHeight < 3)
                MinRoomHeight = 3;

            if (WideOpeningChance < 0)
                WideOpeningChance = 0;

            if (WideOpeningChance > 100)
                WideOpeningChance = 100;

            if (AnchorRadius < 1)
                AnchorRadius = 1;
            if (DividerMaxOffset < 0)
                DividerMaxOffset = 0;

            if (DividerMinRun < 1)
                DividerMinRun = 1;

            if (DividerMaxRun < DividerMinRun)
                DividerMaxRun = DividerMinRun;

            if (DisruptionStampCount < 0)
                DisruptionStampCount = 0;

            if (DisruptionMinRadius < 2)
                DisruptionMinRadius = 2;

            if (DisruptionMaxRadius < DisruptionMinRadius)
                DisruptionMaxRadius = DisruptionMinRadius;

            if (DisruptionMinSegments < 1)
                DisruptionMinSegments = 1;

            if (
                DisruptionMaxSegments <
                DisruptionMinSegments
            )
            {
                DisruptionMaxSegments =
                    DisruptionMinSegments;
            }
        }
    }


    /// <summary>
    /// Darkness Category-3 materialization.
    ///
    /// Exposed architectural surfaces become gilded white Sultan walls.
    /// Buried structural mass and the absolute shell become limestone.
    /// </summary>
    public class SubterraneanSitesDarknessMaterials :
        ZoneBuilderSandbox
    {
        public string BulkWallBlueprint =
            "Limestone";

        public string InnerWallBlueprint =
            "InnerSultanWall_Period6";

        public int SealedBorderWidth = 1;


        public bool BuildZone(
            Zone Z
        )
        {
            if (Z == null)
                return true;

            if (SealedBorderWidth < 0)
                SealedBorderWidth = 0;

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
                            .IsSolidPlaceholder(
                                cell
                            );

                    bool isBoundary =
                        SubterraneanSites
                            .SubterraneanSitesEPGeometry
                            .IsBoundaryPlaceholder(
                                cell
                            );

                    if (
                        !isCore &&
                        !isBoundary
                    )
                    {
                        continue;
                    }

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

                    if (
                        !material.IsNullOrEmpty()
                    )
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
}

namespace XRL.World.Parts
{

    [Serializable]
    public class SubterraneanSitesDarknessBlindGasBehavior :
        IObjectGasBehavior
    {
        //
        // Keep this short.
        //
        // While the creature remains in the gas, ApplyGas continually
        // refreshes the blindness. After leaving, it clears almost
        // immediately instead of producing a long lingering status.
        //
        public int BlindDuration = 5;


        public override bool ApplyGas(
            GameObject Object
        )
        {
            if (
                Object == null ||
                Object == ParentObject
            )
            {
                return false;
            }

            //
            // Only creatures need a blindness effect.
            //
            if (Object.Brain == null)
                return false;

            Gas gas =
                ParentObject.GetPart<Gas>();

            if (gas == null)
                return false;

            if (
                !CheckGasCanAffectEvent.Check(
                    Object,
                    ParentObject,
                    gas
                )
            )
            {
                return false;
            }

            if (
                !Object.PhaseMatches(
                    ParentObject
                )
            )
            {
                return false;
            }

            //
            // Tiny dissipating wisps don't blind.
            //
            if (gas.Density <= 10)
                return false;

            if (
                !CanApplyEffectEvent.Check<Blind>(
                    Object
                )
            )
            {
                return false;
            }

            Blind existing =
                Object.GetEffect<Blind>();

            if (existing != null)
            {
                if (
                    existing.Duration <
                    BlindDuration
                )
                {
                    existing.Duration =
                        BlindDuration;
                }

                return true;
            }

            //
            // Use vanilla Blind normally.
            //
            // Blind.Apply() itself fires ApplyBlind, which gives us the
            // exact interception point we will need for Darkness attunement.
            //
            return
                Object.ApplyEffect(
                    new Blind(
                        BlindDuration
                    )
                );
        }
    }

    [Serializable]
    public class SubterraneanSitesDarknessBlindVent :
        IPart
    {
        public string Direction =
            "N";

        public string GasBlueprint =
            "SubterraneanSitesDarknessBlindGas";

        //
        // Main plume.
        //
        public int GasDensity =
            100;

        public int ForwardLength =
            4;

        public int DensityFalloff =
            15;

        //
        // Darkness vents have a fatter mouth than Cold.
        //
        // First diagonal pair:
        //   85 density
        //
        // Second diagonal pair:
        //   65 density
        //
        public int NearDiagonalDensity =
            85;

        public int FarDiagonalDensity =
            65;


        public override bool SameAs(
            IPart p
        )
        {
            return false;
        }


        public override void Register(
            GameObject Object,
            IEventRegistrar Registrar
        )
        {
            Registrar.Register(
                "WalltrapTrigger"
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
            if (
                E.ID ==
                "WalltrapTrigger"
            )
            {
                Emit();
            }

            return true;
        }


        private void Emit()
        {
            Cell source =
                ParentObject.GetCurrentCell();

            if (
                source == null ||
                IsBroken() ||
                IsRusted() ||
                IsEMPed()
            )
            {
                return;
            }


            //
            // Main directional plume.
            //
            Cell current =
                source;

            Cell firstForward =
                null;

            for (
                int i = 0;
                i < ForwardLength;
                i++
            )
            {
                current =
                    current.GetCellFromDirection(
                        Direction
                    );

                if (
                    current == null ||
                    current.IsSolid(true)
                )
                {
                    break;
                }

                if (i == 0)
                {
                    firstForward =
                        current;
                }

                int density =
                    GasDensity -
                    (
                        i *
                        DensityFalloff
                    );

                if (density < 1)
                    density = 1;

                AddBlindGas(
                    current,
                    density
                );
            }


            //
            // Build the broad mouth.
            //
            // For a north-facing vent, for example:
            //
            //       x x
            //      x x x
            //        V
            //
            // where V is the wall vent.
            //
            // Directions.GetAdjacentDirections() is the same API
            // already used by the Cold cryovent for diagonal spill.
            //
            List<string> diagonalDirections =
                Directions.GetAdjacentDirections(
                    Direction,
                    1
                );

            foreach (
                string diagonalDirection
                in diagonalDirections
            )
            {
                //
                // Immediate diagonals from the vent.
                //
                Cell nearDiagonal =
                    source.GetCellFromDirection(
                        diagonalDirection
                    );

                AddBlindGas(
                    nearDiagonal,
                    NearDiagonalDensity
                );


                //
                // Continue the diagonal shoulder one row farther out.
                //
                if (firstForward != null)
                {
                    Cell farDiagonal =
                        firstForward
                            .GetCellFromDirection(
                                diagonalDirection
                            );

                    AddBlindGas(
                        farDiagonal,
                        FarDiagonalDensity
                    );
                }
            }
        }


        private void AddBlindGas(
            Cell cell,
            int density
        )
        {
            if (
                cell == null ||
                cell.IsSolid(true)
            )
            {
                return;
            }

            //
            // Match the Cold vent contract:
            //
            // if another gas already owns this cell, don't overwrite it.
            // If our own gas is already here, replenish it to the intended
            // plume strength rather than accumulating without limit.
            //
            foreach (
                GameObject obj
                in cell.GetObjects()
            )
            {
                if (obj == null)
                    continue;

                Gas existingGas =
                    obj.GetPart<Gas>();

                if (existingGas == null)
                    continue;

                if (
                    existingGas.GasType !=
                    "SubterraneanSitesDarknessBlindGas"
                )
                {
                    return;
                }

                if (
                    existingGas.Density <
                    density
                )
                {
                    existingGas.Density =
                        density;
                }

                return;
            }


            GameObject gasObject =
                GameObject.Create(
                    GasBlueprint
                );

            if (gasObject == null)
                return;

            Gas gas =
                gasObject.GetPart<Gas>();

            if (gas != null)
            {
                gas.Density =
                    density;

                gas.Creator =
                    ParentObject;
            }

            cell.AddObject(
                gasObject
            );
        }
    }

    


}

namespace XRL.World.Effects
{
    [Serializable]
    public class SubterraneanSitesEPDarknessDenizenAdaptationEffect :
        Effect
    {
        public SubterraneanSitesEPDarknessDenizenAdaptationEffect()
        {
            DisplayName = "";
            Duration = 1;
        }


        public override bool UseStandardDurationCountdown()
        {
            return false;
        }


        public override bool Apply(
            GameObject Object
        )
        {
            return
                Object != null;
        }


        public override void Register(
            GameObject Object,
            IEventRegistrar Registrar
        )
        {
            Registrar.Register(
                "ApplyBlind"
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
            if (
                E != null &&
                E.ID == "ApplyBlind"
            )
            {
                return false;
            }

            return base.FireEvent(E);
        }
    }
    /// <summary>
    /// Darkness-specific EP attunement.
    ///
    /// Shared attunement owns:
    ///   - lifetime
    ///   - refresh/replacement
    ///   - temporary Night Vision mutation
    ///
    /// Darkness owns:
    ///   - converting blindness into temporary suppression
    ///     of the Darkness vision aura while the actor is
    ///     actually inside the Darkness C1 environment.
    /// </summary>
    [Serializable]
    public class SubterraneanSitesEPDarknessAttunementEffect :
        SubterraneanSitesEPAttunementEffect
    {

        public int BlindnessVisionLimitTurns = 0;

        public SubterraneanSitesEPDarknessAttunementEffect()
            : base()
        {
            ThemeKey =
                "Darkness";
        }

        public SubterraneanSitesEPDarknessAttunementEffect(
            int duration,
            int mutationLevel
        )
            : base(
                duration,
                "Darkness",

                // no conventional resistance
                "",
                0,

                // no generic save bonus
                "",
                "",
                0,

                // signature mutation
                "NightVision",
                mutationLevel
            )
        {
        }

        public bool IsVisionLimitedByBlindness
        {
            get
            {
                return
                    BlindnessVisionLimitTurns > 0;
            }
        }

        protected override void RegisterThemeEvents(
            GameObject Object,
            IEventRegistrar Registrar
        )
        {
            //
            // Blind.Apply() asks the target whether blindness
            // may be applied through this legacy event.
            //
            Registrar.Register(
                "ApplyBlind"
            );

            //
            // Use the same ordinary per-turn theme hook already
            // used by the Fungus attunement.
            //
            Registrar.Register(
                "EndTurn"
            );
        }

        protected override bool FireThemeEvent(
            Event E
        )
        {
            if (E == null)
                return true;

            if (E.ID == "ApplyBlind")
            {
                //
                // Darkness attunement is NOT global blindness immunity.
                // Convert blindness only while the actor is actually
                // standing in the Darkness C1 environment.
                //
                if (!IsInsideDarknessEnvironment(base.Object))
                {
                    return true;
                }

                int duration =
                    E.GetIntParameter(
                        "Duration"
                    );

                if (duration < 1)
                {
                    duration = 1;
                }

                if (
                    duration >
                    BlindnessVisionLimitTurns
                )
                {
                    BlindnessVisionLimitTurns =
                        duration;
                }

                //
                // Returning false causes vanilla Blind.Apply()
                // to fail, so no Blind effect is installed.
                //
                return false;
            }

            if (E.ID == "EndTurn")
            {
                HandleBlindSuppressionTurn(
                    base.Object
                );
            }

            return true;
        }

        protected override bool ApplyTheme(
            GameObject Object
        )
        {
            if (Object == null)
                return false;

            //
            // Edge case:
            // if the actor attunes while already blind inside
            // Darkness C1, convert the remaining vanilla blindness
            // into aura suppression immediately.
            //
            CaptureExistingBlindness(
                Object
            );

            SubterraneanSitesEPDarknessEnvironmentEffect environment =
                Object.GetEffectDescendedFrom<
                    SubterraneanSitesEPDarknessEnvironmentEffect
                >();

            if (environment != null)
            {
                environment.SynchronizeAttunement(
                    true
                );
            }

            return true;
        }

        protected override void RemoveTheme(
            GameObject Object
        )
        {
            BlindnessVisionLimitTurns =
                0;

            if (
                Object == null ||
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
                    "Darkness",
                    StringComparison.Ordinal
                )
            )
            {
                return;
            }

            SubterraneanSitesEPDarknessEnvironmentEffect environment =
                Object.GetEffectDescendedFrom<
                    SubterraneanSitesEPDarknessEnvironmentEffect
                >();

            if (environment != null)
            {
                environment.SynchronizeAttunement(
                    false
                );
            }
        }

        protected override void AddThemeDetails(
            List<string> lines
        )
        {
            if (lines == null)
                return;

            lines.Add(
                "The darkness is banished."
            );

            lines.Add(
                "Blindness only reduces vision range."
            );
        }

        private void HandleBlindSuppressionTurn(
            GameObject Object
        )
        {
            if (Object == null)
                return;

            bool insideDarkness =
                IsInsideDarknessEnvironment(
                    Object
                );

            if (insideDarkness)
            {
                //
                // Also handles entering Darkness C1 while already
                // suffering ordinary blindness.
                //
                CaptureExistingBlindness(
                    Object
                );

                if (BlindnessVisionLimitTurns > 0)
                {
                    BlindnessVisionLimitTurns--;
                }

                return;
            }

            //
            // If blindness was converted inside Darkness but the
            // player leaves the Darkness environment before its
            // duration expires, restore ordinary vanilla Blind for
            // the remaining duration. This keeps the attunement from
            // acting as blindness immunity elsewhere.
            //
            if (BlindnessVisionLimitTurns > 0)
            {
                int remaining =
                    BlindnessVisionLimitTurns;

                BlindnessVisionLimitTurns =
                    0;

                Object.ApplyEffect(
                    new Blind(
                        remaining
                    )
                );
            }
        }

        private void CaptureExistingBlindness(
            GameObject Object
        )
        {
            if (
                Object == null ||
                !IsInsideDarknessEnvironment(
                    Object
                )
            )
            {
                return;
            }

            Blind blind =
                Object.GetEffectDescendedFrom<
                    Blind
                >();

            if (blind == null)
                return;

            if (
                blind.Duration >
                BlindnessVisionLimitTurns
            )
            {
                BlindnessVisionLimitTurns =
                    blind.Duration;
            }

            Object.RemoveEffect(
                blind
            );
        }

        private static bool IsInsideDarknessEnvironment(
            GameObject Object
        )
        {
            if (
                Object == null ||
                Object.CurrentZone == null
            )
            {
                return false;
            }

            return
                string.Equals(
                    SubterraneanSites
                        .SubterraneanSitesEPAttunementSystem
                        .GetCategory1Theme(
                            Object.CurrentZone
                        ),
                    "Darkness",
                    StringComparison.Ordinal
                );
        }

       
    }
    [Serializable]
    public class SubterraneanSitesEPDarknessEnvironmentEffect :
        Effect
    {
        public bool UnattunedExposureActive =
            false;

        //
        // Normal unattuned Darkness C1:
        // enough nearby information to navigate without making the
        // environment comfortable.
        //
        public const int UnattunedVisionRadius =
            2;

        //
        // Blindness while attuned reimposes a harsher form of the
        // environmental restriction.
        //
        public const int BlindedAttunedVisionRadius =
            1;

        public void SynchronizeAttunement(
            bool attuned
        )
        {
            if (attuned)
            {
                UnattunedExposureActive =
                    false;

                return;
            }

            if (UnattunedExposureActive)
                return;

            UnattunedExposureActive =
                true;

            GameObject actor =
                base.Object;

            if (
                actor != null &&
                actor.IsPlayer()
            )
            {
                Popup.Show(
                    "Supernatural darkness surrounds you."
                );
            }
        }


        public SubterraneanSitesEPDarknessEnvironmentEffect()
        {
            //
            // Visible environmental status.
            //
            DisplayName =
                "{{K|extradimensional darkness}}";

            //
            // This is environmental state, not a conventional timed status.
            // The Darkness environment system owns its lifetime.
            //
            Duration =
                1;
        }


        public override string GetDetails()
        {
            return
                "Ordinary light is swallowed by extradimensional darkness.";
        }


        public override bool UseStandardDurationCountdown()
        {
            return false;
        }

        public override string GetDescription()
        {
            GameObject player =
                base.Object;

            if (
                player != null &&
                SubterraneanSites
                    .SubterraneanSitesEPAttunementSystem
                    .IsAttunedTo(
                        player,
                        "Darkness"
                    )
            )
            {
                return null;
            }

            return base.GetDescription();
        }


        public override bool WantEvent(
            int ID,
            int cascade
        )
        {
            if (
                !base.WantEvent(
                    ID,
                    cascade
                )
            )
            {
                return
                    ID ==
                    BeforeRenderEvent.ID;
            }

            return true;
        }

        public override bool HandleEvent(
            BeforeRenderEvent E
        )
        {
            //
            // Run after ordinary lighting/vision sources have populated
            // the light map, just as vanilla Blackout does.
            //
            if (E.Pass == 1)
            {
                E.AfterHandlers.Add(
                    this
                );

                return
                    base.HandleEvent(
                        E
                    );
            }


            if (E.Pass != 2)
            {
                return
                    base.HandleEvent(
                        E
                    );
            }


            GameObject player =
                base.Object;

            if (
                player == null ||
                !player.IsPlayer() ||
                player.CurrentZone == null ||
                player.CurrentCell == null
            )
            {
                return
                    base.HandleEvent(
                        E
                    );
            }


            Zone Z =
                player.CurrentZone;


            string category1 =
                SubterraneanSites
                    .SubterraneanSitesEPAttunementSystem
                    .GetCategory1Theme(
                        Z
                    );

            if (
                !string.Equals(
                    category1,
                    "Darkness",
                    StringComparison.Ordinal
                )
            )
            {
                return
                    base.HandleEvent(
                        E
                    );
            }


            SubterraneanSitesEPDarknessAttunementEffect
                attunement =
                    player.GetEffectDescendedFrom<
                        SubterraneanSitesEPDarknessAttunementEffect
                    >();

            //
            // No valid Darkness attunement:
            // use the normal radius-2 environmental restriction.
            //
            if (
                attunement == null ||
                attunement.Duration <= 0
            )
            {
                ApplyVisionLimit(
                    player,
                    Z,
                    UnattunedVisionRadius
                );
            }
            //
            // Attuned + blindness:
            // blindness does not become vanilla Blind.
            // Instead it temporarily restores a harsher radius-1
            // version of the Darkness environmental restriction.
            //
            else if (
                attunement.IsVisionLimitedByBlindness
            )
            {
                ApplyVisionLimit(
                    player,
                    Z,
                    BlindedAttunedVisionRadius
                );
            }

            //
            // Otherwise the player is normally attuned.
            // Do nothing: their ordinary light sources and vision
            // systems work at their normal ranges.
            //


            return
                base.HandleEvent(
                    E
                );
        }

        private static void ApplyVisionLimit(
            GameObject player,
            Zone Z,
            int radius
        )
        {
            if (
                player == null ||
                Z == null ||
                player.CurrentCell == null
            )
            {
                return;
            }

            int playerX =
                player.CurrentCell.X;

            int playerY =
                player.CurrentCell.Y;


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
                    //
                    // Preserve the player's own cell and all eight
                    // immediately surrounding cells.
                    //
                    // Chebyshev distance is intentional here:
                    //
                    //     xxx
                    //     x@x
                    //     xxx
                    //
                    int dx =
                        Math.Abs(
                            x - playerX
                        );

                    int dy =
                        Math.Abs(
                            y - playerY
                        );

                    if (
                        dx <= radius &&
                        dy <= radius
                    )
                    {
                        continue;
                    }


                    LightLevel light =
                        Z.GetLight(
                            x,
                            y
                        );


                    //
                    // Preserve all vision/light systems at Interpolight
                    // strength or stronger.
                    //
                    // This deliberately allows:
                    //
                    //   Interpolight
                    //   Radar
                    //   LitRadar
                    //   Omniscient / Clairvoyance
                    //
                    // Ordinary Light, Safelight, Darkvision/Night Vision,
                    // Dimvision, etc. are suppressed outside radius 1.
                    //
                    if (
                        light >=
                        LightLevel.Interpolight
                    )
                    {
                        continue;
                    }


                    Z.SetLight(
                        x,
                        y,
                        LightLevel.Blackout
                    );
                }
            }
        }

    }

}