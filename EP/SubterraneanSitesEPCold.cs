using System;
using System.Collections.Generic;
using Genkit;
using XRL;
using XRL.Core;
using XRL.Rules;
using XRL.World;
using XRL.World.Parts;
using XRL.World.ZoneBuilders.Utility;
using XRL.UI;

namespace SubterraneanSites
{
    /// <summary>
    /// COLD DIMENSION
    ///
    /// Category 1: -85 ambient environment.
    /// Category 2: directional cryogas wall vents.
    /// Category 3: Marble structural core with Burnished Azzurum boundary.
    /// Category 4: sealed organic cave with cyan snow-grass and
    ///             white/blue frozen ground.
    /// Category 5: cold-convalessence, cryochambers, stillvines,
    ///             brightshrooms, rubble and small grey boulders.
    /// </summary>
    internal sealed class SubterraneanSitesEPColdTheme :
        ISubterraneanSitesEPCategoryProvider,
        ISubterraneanSitesEPAttunementProvider,
        ISubterraneanSitesEPDenizenAdaptationProvider,
        ISubterraneanSitesEPSignatureMutationProvider
    {
        public string ThemeKey
        {
            get { return "Cold"; }
        }

        public string SignatureMutationClass
        {
            get { return "FreezingRay"; }
        }

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

                        "ColdResistance",
                        100,

                        "",
                        "",
                        0,

                        SignatureMutationClass,
                        mutationLevel
                    );

            successMessage =
                "Attunement grants:\n" +
                "Freezing Ray (level " +
                mutationLevel.ToString() +
                ")\n" +
                "+100 Cold Resistance";

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
                "ColdResistance",
                100
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

            int floorRoll =
                Stat.Random(
                    1,
                    8
                );

            //
            // 50% snow-grass:
            //   1/4 dark cyan grass
            //   1/4 bright cyan grass
            //   1/4 white snowy dirt
            //   1/4 blue icy dirt
            //
            if (floorRoll <= 2)
            {
                cell.PaintColorString =
                    "&c";

                cell.PaintTile =
                    DirtPicker
                        .GetRandomGrassTile();
            }
            else if (floorRoll <= 4)
            {
                cell.PaintColorString =
                    "&C";

                cell.PaintTile =
                    DirtPicker
                        .GetRandomGrassTile();
            }
            else if (floorRoll <= 6)
            {
                cell.PaintColorString =
                    "&Y";

                cell.PaintDetailColor =
                    "C";

                cell.PaintTile =
                    "Tiles/tile-dirt1.png";
            }
            else
            {
                cell.PaintColorString =
                    "&B";

                cell.PaintDetailColor =
                    "Y";

                cell.PaintTile =
                    "Tiles/tile-dirt1.png";
            }

            //
            // Same subtle ASCII-ground variation used by several vanilla
            // terrain painters.
            //
            int renderRoll =
                Stat.Random(
                    1,
                    5
                );

            if (renderRoll == 1)
            {
                cell.PaintRenderString =
                    ".";
            }
            else if (renderRoll == 2)
            {
                cell.PaintRenderString =
                    ",";
            }
            else if (renderRoll == 3)
            {
                cell.PaintRenderString =
                    "`";
            }
            else if (renderRoll == 4)
            {
                cell.PaintRenderString =
                    "'";
            }
        }

        public void RegisterCategory1(
            SubterraneanSitesEPLayerContext context
        )
        {
            if (context == null)
                return;

            The.ZoneManager.AddZonePostBuilder(
                context.ZoneId,
                "SubterraneanSitesEPTemperature",
                "Temperature", "-85"
            );

            if (The.Game != null)
            {
                The.Game.RequireSystem<
                    SubterraneanSitesEPColdEnvironmentSystem
                >();
            }
        }

        public void RegisterCategory2(
            SubterraneanSitesEPLayerContext context
        )
        {
            SubterraneanSitesEPBoundaryHazards
                .RegisterDirectional(
                    context,
                    "SubterraneanSitesColdVentN",
                    "SubterraneanSitesColdVentS",
                    "SubterraneanSitesColdVentE",
                    "SubterraneanSitesColdVentW",
                    6,
                    9,
                    2,
                    5,
                    6
                );
        }

        public void RegisterCategory3(
            SubterraneanSitesEPLayerContext context
        )
        {
            The.ZoneManager.AddZonePostBuilder(
                context.ZoneId,
                "SubterraneanSitesColdMaterials"
            );
        }

        public void RegisterCategory4Layout(
            SubterraneanSitesEPLayerContext context
        )
        {
            The.ZoneManager.AddZonePostBuilder(
                context.ZoneId,
                "SubterraneanSitesColdCaveLayout"
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
                "SubterraneanSitesColdFloor"
            );
        }

        public void RegisterCategory5(
            SubterraneanSitesEPLayerContext context
        )
        {
            The.ZoneManager.AddZonePostBuilder(
                context.ZoneId,
                "SubterraneanSitesColdDecorations"
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
                "SubterraneanSitesColdFloor",
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
                "SubterraneanSitesColdDecorations",
                "EntranceOnly", "1"
            );
        }
    }
}

namespace SubterraneanSites
{
    [Serializable]
    public class SubterraneanSitesEPColdEnvironmentSystem :
        IGameSystem
    {
        public bool WasInColdEnvironment =
            false;

        public bool WasColdAttuned =
            false;


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
            SynchronizePlayerState();

            return true;
        }


        public override bool HandleEvent(
            EndTurnEvent E
        )
        {
            SynchronizePlayerState();

            return true;
        }


        private void SynchronizePlayerState()
        {
            GameObject player =
                The.Player;

            if (
                player == null ||
                player.CurrentZone == null
            )
            {
                ResetExposureState();

                return;
            }


            string category1 =
                SubterraneanSitesEPAttunementSystem
                    .GetCategory1Theme(
                        player.CurrentZone
                    );


            bool coldEnvironment =
                string.Equals(
                    category1,
                    "Cold",
                    StringComparison.Ordinal
                );


            if (!coldEnvironment)
            {
                ResetExposureState();

                return;
            }


            bool coldAttuned =
                SubterraneanSitesEPAttunementSystem
                    .IsAttunedTo(
                        player,
                        "Cold"
                    );


            bool becameExposed =
                !coldAttuned &&
                (
                    !WasInColdEnvironment ||
                    WasColdAttuned
                );


            WasInColdEnvironment =
                true;

            WasColdAttuned =
                coldAttuned;


            if (becameExposed)
            {
                Popup.Show(
                    "You feel cold."
                );
            }
        }


        private void ResetExposureState()
        {
            WasInColdEnvironment =
                false;

            WasColdAttuned =
                false;
        }
    }
}

namespace XRL.World.ZoneBuilders
{
    /// <summary>
    /// Cold Category-4 floor adapter.
    ///
    /// Cold owns its procedural snow-grass recipe. The shared EP floor
    /// system owns universal underground/scar scope and application.
    /// </summary>
    public class SubterraneanSitesColdFloor :
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
                            .SubterraneanSitesEPColdTheme
                            .CreateFloorSpec(),
                        EntranceOnly != 0
                    );
        }
    }

    /// <summary>
    /// Cold Categories 3 + 4.
    ///
    /// The layout is intentionally closer to vanilla Cave than to a room
    /// generator. Marble first fills the zone. A CellularGrid and NoiseMap
    /// independently nominate open cells; their union becomes the cave void.
    /// NoiseMap ExtraNodes bias the mandatory EP transition anchors toward
    /// naturally broad open regions. Qud's own EnsureAllVoidsConnected pass
    /// then joins the generated voids.
    ///
    /// Materialization happens after the geometry is final:
    ///   solid cell touching open space    -> Burnished Azzurum
    ///   buried solid cell                 -> Marble
    ///   absolute sealed outer border      -> Marble
    ///
    /// This makes Burnished Azzurum the normal active inner-wall interface,
    /// while large enough wall masses naturally reveal a Marble core.
    /// </summary>
    public class SubterraneanSitesColdCaveLayout : ZoneBuilderSandbox
    {
        // Vanilla Cave-inspired cellular component.
        public int CellularPasses = 2;
        public int CellularSeedChance = 60;
        public int CellularBorderDepth = 2;

        // NoiseMap component. These are intentionally close to the public
        // NoiseMap demo values, but spread over the 80x25 Qud zone shape.
        public int NoiseSectorsWide = 3;
        public int NoiseSectorsHigh = 2;
        public int NoiseSeedsPerSector = 2;
        public int NoiseMinSeedDepth = 80;
        public int NoiseMaxSeedDepth = 80;
        public int NoiseBaseNoise = 4;
        public int NoiseFilterPasses = 5;
        public int NoiseBorderWidth = -3;
        public int NoiseCutoffDepth = 1;
        public int NoiseOpenThreshold = 2;

        // Guaranteed open footprint around incoming/outgoing EP transitions.
        public int AnchorRadius = 3;

        // The true zone edge is always structural fill. A value of 1 means
        // only the absolute perimeter is forced Marble.
        public int SealedBorderWidth = 1;

        public bool BuildZone(Zone Z)
        {
            if (Z == null)
                return true;

            ClampSettings();

            List<Location2D> anchors =
                SubterraneanSites
                    .SubterraneanSitesEPVerticalTransitions
                    .GetVerticalAnchors(Z.ZoneID);

            List<NoiseMapNode> extraNodes =
                new List<NoiseMapNode>();

            if (anchors != null)
            {
                foreach (Location2D anchor in anchors)
                {
                    if (anchor == null)
                        continue;

                    extraNodes.Add(
                        new NoiseMapNode(
                            anchor.X,
                            anchor.Y
                        )
                    );
                }
            }

            NoiseMap noiseMap = new NoiseMap(
                Z.Width,
                Z.Height,
                10,
                NoiseSectorsWide,
                NoiseSectorsHigh,
                NoiseSeedsPerSector,
                NoiseMinSeedDepth,
                NoiseMaxSeedDepth,
                NoiseBaseNoise,
                NoiseFilterPasses,
                NoiseBorderWidth,
                NoiseCutoffDepth,
                extraNodes
            );

            CellularGrid cellularGrid = new CellularGrid();
            cellularGrid.Passes = CellularPasses;
            cellularGrid.SeedChance = CellularSeedChance;
            cellularGrid.SeedBorders = true;
            cellularGrid.BorderDepth = CellularBorderDepth;

            int seed = XRLCore.Core.Game.GetWorldSeed(
                "SubterraneanSites:ColdCaveCellular:" + Z.ZoneID
            );

            cellularGrid.Generate(
                new System.Random(seed),
                Z.Width,
                Z.Height
            );

            // Vanilla Cave uses a union of two independent signals to decide
            // where to clear wall. We do the same: cellular voids provide the
            // irregular cave texture; NoiseMap produces broader chambers.
            for (int x = 0; x < Z.Width; x++)
            {
                for (int y = 0; y < Z.Height; y++)
                {
                    if (IsForcedBorder(Z, x, y))
                        continue;

                    bool openFromCellular =
                        cellularGrid.cells[x, y] == 0;

                    bool openFromNoise =
                        noiseMap.Noise[x, y] > NoiseOpenThreshold;

                    if (openFromCellular || openFromNoise)
                    {
                        Z.GetCell(x, y).ClearWalls();
                    }
                }
            }

            // ExtraNodes bias the field but are not a hard gameplay guarantee.
            // Give every transition a small reliable open landing/hole footprint.
            if (anchors != null)
            {
                foreach (Location2D anchor in anchors)
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

            ForceSealedBorder(Z);

            // This is the same utility used in the public NoiseMap example.
            // It lets the organic generators decide the shapes, then handles
            // the gameplay requirement that the cave voids actually join up.
            EnsureAllVoidsConnected(
                Z,
                pathWithNoise: true
            );

            // Connectivity should have no reason to use the absolute edge, but
            // reassert the dimensional shell as a hard invariant.
            ForceSealedBorder(Z);

            // Floor treatment, reachable-map rebuild, and final materials are
            // separate category-pipeline passes so they can be shuffled.
            Z.ClearReachableMap();

            return true;
        }

        private void ClampSettings()
        {
            if (CellularPasses < 0)
                CellularPasses = 0;

            if (CellularSeedChance < 0)
                CellularSeedChance = 0;
            if (CellularSeedChance > 100)
                CellularSeedChance = 100;

            if (CellularBorderDepth < 0)
                CellularBorderDepth = 0;

            if (NoiseSectorsWide < 1)
                NoiseSectorsWide = 1;
            if (NoiseSectorsHigh < 1)
                NoiseSectorsHigh = 1;
            if (NoiseSeedsPerSector < 0)
                NoiseSeedsPerSector = 0;

            if (NoiseMaxSeedDepth < NoiseMinSeedDepth)
                NoiseMaxSeedDepth = NoiseMinSeedDepth;

            if (NoiseBaseNoise < 0)
                NoiseBaseNoise = 0;

            if (NoiseFilterPasses < 0)
                NoiseFilterPasses = 0;

            if (AnchorRadius < 1)
                AnchorRadius = 1;

            if (SealedBorderWidth < 1)
                SealedBorderWidth = 1;
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

        private void ForceSealedBorder(Zone Z)
        {
            for (int x = 0; x < Z.Width; x++)
            {
                for (int y = 0; y < Z.Height; y++)
                {
                    if (!IsForcedBorder(Z, x, y))
                        continue;

                    Cell cell = Z.GetCell(x, y);
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

        private void CarveAnchorFootprint(
            Zone Z,
            int centerX,
            int centerY
        )
        {
            int radiusSquared =
                AnchorRadius * AnchorRadius;

            for (int dx = -AnchorRadius; dx <= AnchorRadius; dx++)
            {
                for (int dy = -AnchorRadius; dy <= AnchorRadius; dy++)
                {
                    if (dx * dx + dy * dy > radiusSquared)
                        continue;

                    int x = centerX + dx;
                    int y = centerY + dy;

                    if (
                        x < 0 ||
                        y < 0 ||
                        x >= Z.Width ||
                        y >= Z.Height ||
                        IsForcedBorder(Z, x, y)
                    )
                    {
                        continue;
                    }

                    Cell cell = Z.GetCell(x, y);
                    if (cell != null)
                        cell.ClearWalls();
                }
            }
        }


    }
}

namespace XRL.World.ZoneBuilders
{


    /// <summary>
    /// Cold Category 3 materialization. Core and boundary roles are supplied by
    /// the shared geometry classifier, so this builder does not know which
    /// Category-4 shape generated them.
    /// </summary>
    public class SubterraneanSitesColdMaterials : ZoneBuilderSandbox
    {
        public string BulkWallBlueprint = "Marble";
        public string InnerWallBlueprint = "Burnished Azzurum";
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
                    Cell cell = Z.GetCell(x, y);
                    if (cell == null)
                        continue;

                    bool isCore =
                        SubterraneanSites.SubterraneanSitesEPGeometry
                            .IsSolidPlaceholder(cell);

                    bool isBoundary =
                        SubterraneanSites.SubterraneanSitesEPGeometry
                            .IsBoundaryPlaceholder(cell);

                    if (!isCore && !isBoundary)
                        continue;

                    string material =
                        isBoundary && !IsForcedBorder(Z, x, y)
                            ? InnerWallBlueprint
                            : BulkWallBlueprint;

                    cell.ClearWalls();

                    if (!material.IsNullOrEmpty())
                        cell.AddObject(material);
                }
            }

            return true;
        }

        private bool IsForcedBorder(Zone Z, int x, int y)
        {
            return
                x < SealedBorderWidth ||
                y < SealedBorderWidth ||
                x >= Z.Width - SealedBorderWidth ||
                y >= Z.Height - SealedBorderWidth;
        }
    }
}



namespace XRL.Liquids
{
    /// <summary>
    /// Cold-EP convalessence.
    ///
    /// The liquid keeps the vanilla convalessence implementation but has its
    /// own registry ID so the Cold theme can give it the same intrinsic
    /// temperature as the dimension without changing vanilla convalessence.
    ///
    /// First test target: -85T. The inherited freeze threshold remains vanilla.
    /// </summary>
    [IsLiquid]
    [Serializable]
    public class SubterraneanSitesColdConvalessence : LiquidConvalessence
    {
        public const string LiquidID =
            "SubterraneanSitesColdConvalessence";

        public SubterraneanSitesColdConvalessence()
            : base()
        {
            ((BaseLiquid)this).ID = LiquidID;
            Temperature = -85;
        }
    }
}

namespace XRL.World.ZoneBuilders
{
    /// <summary>
    /// Cold Category 5.
    ///
    /// Cold Category 5 decorations:
    /// - 6-10 cold-convalessence puddle patches
    /// - usually 1-2 cryochambers with curated tiered occupants
    /// - one dominant stillvine growth with satellite patches/singles
    /// - independently-sized brightshroom colonies
    /// - sparse mixed rubble/small-grey-boulder debris clusters
    ///
    /// Organic patch geometry is delegated to shared EP placement helpers;
    /// this builder retains Cold-specific legality and object choices.
    /// </summary>
    public class SubterraneanSitesColdDecorations : ZoneBuilderSandbox
    {
        public int EntranceOnly = 0;

        public string PuddleBlueprint =
            "SubterraneanSitesColdConvalessencePuddle";

        public int MinPuddlePatches = 6;
        public int MaxPuddlePatches = 10;

        public int MinPuddleCells = 8;
        public int MaxPuddleCells = 16;

        public int MinCryochambers = 1;
        public int MaxCryochambers = 2;

        public string VineBlueprint =
            "Stillvine";

        public string BrightshroomBlueprint =
            "Brightshroom";

        public string RubbleBlueprint =
            "SubterraneanSitesColdRubble";
        //
        // Stillvines:
        // choose one total population, then distribute it
        // across one dominant growth, satellites, and singles.
        //
        public int MinVines = 45;
        public int MaxVines = 65;
        public int MainVinePercent = 60;
        public int MinVineSatellitePatches = 2;
        public int MaxVineSatellitePatches = 4;
        public int MinVineSingles = 3;
        public int MaxVineSingles = 7;

        //
        // Brightshrooms:
        // independently sized colonies.
        //
        public int MinShroomPatches = 2;
        public int MaxShroomPatches = 4;

        public int MinShroomsPerPatch = 5;
        public int MaxShroomsPerPatch = 15;

        //
        // Rubble + small grey boulders:
        // sparse mixed debris piles.
        //
        public int MinDebrisClusters = 2;
        public int MaxDebrisClusters = 3;

        public int MinDebrisCells = 3;
        public int MaxDebrisCells = 6;

        public int DebrisRubblePercent = 65;

        private const int TransitionExclusionRadius = 6;

        public string SmallBoulderBlueprint =
            "SmallBoulder Grey";

        public bool BuildZone(Zone Z)
        {
            if (Z == null)
                return true;

            ClampSettings();

            int seed = XRLCore.Core.Game.GetWorldSeed(
                "SubterraneanSites:ColdDecorations:" + Z.ZoneID
            );

            System.Random rng = new System.Random(seed);

            bool[,] reserved = new bool[Z.Width, Z.Height];

            List<Location2D> verticalAnchors =
                SubterraneanSites
                    .SubterraneanSitesEPVerticalTransitions
                    .GetVerticalAnchors(Z.ZoneID);

            if (EntranceOnly == 0)
            {
                ReserveTransitionAreas(
                    reserved,
                    Z,
                    verticalAnchors
                );
            }

            //
            // Cryochambers get first choice of the larger open spaces.
            // Most levels have them, but an occasional level has none.
            //
            int cryoCount = 0;

            if (rng.Next(100) < 85)
            {
                cryoCount =
                    rng.Next(
                        MinCryochambers,
                        MaxCryochambers + 1
                    );
            }

            for (int i = 0; i < cryoCount; i++)
            {
                TryPlaceCryochamber(
                    Z,
                    reserved,
                    rng
                );
            }

            int puddleCount =
                rng.Next(
                    MinPuddlePatches,
                    MaxPuddlePatches + 1
                );

            for (int i = 0; i < puddleCount; i++)
            {
                int targetCells =
                    rng.Next(
                        MinPuddleCells,
                        MaxPuddleCells + 1
                    );

                HashSet<Cell> patch =
                    GrowPuddlePatch(
                        Z,
                        reserved,
                        targetCells,
                        rng
                    );

                if (patch.Count == 0)
                    continue;

                foreach (Cell cell in patch)
                {
                    if (cell == null)
                        continue;

                    bool placed =
                        SubterraneanSites
                            .SubterraneanSitesEPLiquids
                            .AddOrMixLiquid(
                                cell,
                                XRL.Liquids
                                    .SubterraneanSitesColdConvalessence
                                    .LiquidID,
                                PuddleBlueprint
                            );

                    if (!placed)
                        continue;

                    //
                    // Cold convalessence is exclusive to discrete objects, but compatible
                    // with other liquids. Claim the pool cell against later object placement.
                    //
                    SubterraneanSites
                        .SubterraneanSitesEPReservations
                        .ClaimCell(
                            Z,
                            cell
                        );
                }

                ReservePatchAndHalo(
                    reserved,
                    Z,
                    patch
                );
            }

            PlaceVineGrowths(
                Z,
                reserved,
                rng
            );

            PlaceShroomPatches(
                Z,
                reserved,
                rng
            );

            PlaceDebrisClusters(
                Z,
                reserved,
                rng
            );

            return true;
        }

        private void ClampSettings()
        {
            if (MinPuddlePatches < 0)
                MinPuddlePatches = 0;
            if (MaxPuddlePatches < MinPuddlePatches)
                MaxPuddlePatches = MinPuddlePatches;

            if (MinPuddleCells < 1)
                MinPuddleCells = 1;
            if (MaxPuddleCells < MinPuddleCells)
                MaxPuddleCells = MinPuddleCells;

            if (MinCryochambers < 0)
                MinCryochambers = 0;
            if (MaxCryochambers < MinCryochambers)
                MaxCryochambers = MinCryochambers;
        }

        private void ReserveTransitionAreas(
            bool[,] reserved,
            Zone Z,
            List<Location2D> anchors
        )
        {
            SubterraneanSites
                .SubterraneanSitesEPPlacement
                .ReserveAroundAnchors(
                    reserved,
                    Z,
                    anchors,
                    TransitionExclusionRadius
                );
        }

        private HashSet<Cell> GrowPuddlePatch(
            Zone Z,
            bool[,] reserved,
            int targetCells,
            System.Random rng
        )
        {
            List<Cell> seeds =
                SubterraneanSites
                    .SubterraneanSitesEPPlacement
                    .CollectCells(
                        Z,
                        delegate (Cell cell)
                        {
                            return CellIsAvailableForPuddle(
                                Z,
                                reserved,
                                cell
                            );
                        }
                    );

            int attempts = Math.Min(12, seeds.Count);

            return
                SubterraneanSites
                    .SubterraneanSitesEPPlacement
                    .GrowBestPatch(
                        seeds,
                        targetCells,
                        attempts,
                        delegate (Cell cell)
                        {
                            return CellIsAvailableForPuddle(
                                Z,
                                reserved,
                                cell
                            );
                        },
                        rng
                    );
        }

        private bool CellIsInsideDecorationScope(
            Zone Z,
            Cell cell
        )
        {
            if (cell == null)
                return false;

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



        private bool CellIsAvailableForPuddle(
            Zone Z,
            bool[,] reserved,
            Cell cell
        )
        {
            if (cell == null)
                return false;

            if (!CellIsInsideDecorationScope(Z, cell))
                return false;

            int x = cell.X;
            int y = cell.Y;

            if (
                x < 0 ||
                y < 0 ||
                x >= Z.Width ||
                y >= Z.Height
            )
            {
                return false;
            }

            if (reserved[x, y])
                return false;

            //
            // Cold convalessence pools are exclusive to discrete objects.
            //
            // Respect any higher-priority semantic ownership, but do not reject a cell
            // merely because another liquid is already present. Ooze/Fire liquids are
            // allowed to mix with Cold convalessence.
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

            return true;
        }

        private void ReservePatchAndHalo(
            bool[,] reserved,
            Zone Z,
            HashSet<Cell> patch
        )
        {
            foreach (Cell cell in patch)
            {
                if (cell == null)
                    continue;

                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        int x = cell.X + dx;
                        int y = cell.Y + dy;

                        if (
                            x < 0 ||
                            y < 0 ||
                            x >= Z.Width ||
                            y >= Z.Height
                        )
                        {
                            continue;
                        }

                        reserved[x, y] = true;
                    }
                }
            }
        }

        private string RollCryochamberContents(
            Zone Z
        )
        {
            if (Z == null)
                return null;

            int tier = Z.NewTier;

            if (tier < 1)
                tier = 1;

            if (tier > 8)
                tier = 8;

            string table =
                "SubterraneanSites_CryochamberTier" +
                tier.ToString() +
                "_Mobs";

            PopulationResult result =
                PopulationManager.RollOneFrom(
                    table
                );

            if (
                result == null ||
                result.Blueprint.IsNullOrEmpty()
            )
            {
                return null;
            }

            return result.Blueprint;
        }

        private bool TryPlaceCryochamber(
            Zone Z,
            bool[,] reserved,
            System.Random rng
        )
        {
            List<Location2D> candidates =
                new List<Location2D>();

            //
            // MakeCryochamber itself uses a 5x5 footprint:
            // center +/- 2.
            //
            // Search the whole cave for every genuinely valid
            // location instead of randomly hoping to hit one.
            //
            for (int x = 3; x < Z.Width - 3; x++)
            {
                for (int y = 3; y < Z.Height - 3; y++)
                {
                    if (
                        RectangleAreaIsAvailable(
                            Z,
                            reserved,
                            x - 2,
                            y - 2,
                            x + 2,
                            y + 2
                        )
                    )
                    {
                        candidates.Add(
                            Location2D.Get(
                                x,
                                y
                            )
                        );
                    }
                }
            }

            if (candidates.Count == 0)
                return false;

            Location2D chosen =
                candidates[
                    rng.Next(candidates.Count)
                ];

            int chamberX = chosen.X;
            int chamberY = chosen.Y;

            string contents =
                RollCryochamberContents(Z);

            Cryobarrio2.MakeCryochamber(
                chamberX,
                chamberY,
                Z,
                contents
            );

            //
            // The actual cryochamber installation occupies center +/- 2.
            //
            // Claim only that real 5x5 footprint globally. The larger 7x7 reservation
            // below remains a Cold-specific spacing rule rather than cross-theme
            // semantic ownership.
            //
            SubterraneanSites
                .SubterraneanSitesEPReservations
                .ClaimRectangle(
                    Z,
                    chamberX - 2,
                    chamberY - 2,
                    chamberX + 2,
                    chamberY + 2
                );

            //
            // The chamber occupies 5x5, but reserve one extra
            // cell around it so subsequent puddles/chambers
            // don't crowd right against the installation.
            //
            ReserveRectangle(
                reserved,
                Z,
                chamberX - 3,
                chamberY - 3,
                chamberX + 3,
                chamberY + 3
            );

            return true;
        }



        private bool RectangleAreaIsAvailable(
            Zone Z,
            bool[,] reserved,
            int x1,
            int y1,
            int x2,
            int y2
        )
        {
            for (int x = x1; x <= x2; x++)
            {
                for (int y = y1; y <= y2; y++)
                {
                    if (!CellIsAvailable(
                        Z,
                        reserved,
                        x,
                        y
                    ))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool CellIsAvailable(
            Zone Z,
            bool[,] reserved,
            int x,
            int y
        )
        {
            if (
                x < 0 ||
                y < 0 ||
                x >= Z.Width ||
                y >= Z.Height
            )
            {
                return false;
            }

            if (reserved[x, y])
                return false;

            Cell cell = Z.GetCell(x, y);
            if (cell == null)
                return false;

            if (!CellIsInsideDecorationScope(Z, cell))
                return false;

            //
            // A cryochamber is destructive across its 5x5 installation footprint.
            // Every cell must therefore be free of earlier semantic ownership.
            //
            // Notice that there is deliberately NO liquid test here. Cryochambers may
            // be installed in permissive Ooze/Fire liquid.
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

            return true;
        }

        private void ReserveRectangle(
            bool[,] reserved,
            Zone Z,
            int x1,
            int y1,
            int x2,
            int y2
        )
        {
            SubterraneanSites
                .SubterraneanSitesEPPlacement
                .ReserveRectangle(
                    reserved,
                    Z,
                    x1,
                    y1,
                    x2,
                    y2
                );
        }

        private Cell PickDecorationSeed(
            Zone Z,
            bool[,] reserved,
            System.Random rng,
            bool wallAdjacent,
            bool broadOpen
        )
        {
            return
                SubterraneanSites
                    .SubterraneanSitesEPPlacement
                    .PickRandomCell(
                        Z,
                        delegate (Cell cell)
                        {
                            if (
                                !CellIsAvailableForDecoration(
                                    Z,
                                    reserved,
                                    cell
                                )
                            )
                            {
                                return false;
                            }

                            if (
                                wallAdjacent &&
                                !TouchesSolidCardinal(cell)
                            )
                            {
                                return false;
                            }

                            if (
                                broadOpen &&
                                !SubterraneanSites
                                    .SubterraneanSitesEPPlacement
                                    .HasBroadOpenClearance(
                                        Z,
                                        cell,
                                        delegate (Cell neighbor)
                                        {
                                            return CellIsAvailableForDecoration(
                                                Z,
                                                reserved,
                                                neighbor
                                            );
                                        }
                                    )
                            )
                            {
                                return false;
                            }

                            return true;
                        },
                        rng
                    );
        }

        private Cell PickDecorationSeedNearPatch(
            Zone Z,
            bool[,] reserved,
            HashSet<Cell> patch,
            System.Random rng,
            int minDistance,
            int maxDistance
        )
        {
            return
                SubterraneanSites
                    .SubterraneanSitesEPPlacement
                    .PickRandomCellNearPatch(
                        Z,
                        patch,
                        minDistance,
                        maxDistance,
                        delegate (Cell cell)
                        {
                            return CellIsAvailableForDecoration(
                                Z,
                                reserved,
                                cell
                            );
                        },
                        rng
                    );
        }

        private HashSet<Cell> GrowDecorationPatch(
            Zone Z,
            bool[,] reserved,
            Cell seed,
            int targetCells,
            System.Random rng,
            bool broadOpen
        )
        {
            return
                SubterraneanSites
                    .SubterraneanSitesEPPlacement
                    .GrowPatch(
                        seed,
                        targetCells,
                        delegate (Cell cell)
                        {
                            return DecorationPatchCellIsAvailable(
                                Z,
                                reserved,
                                cell,
                                broadOpen
                            );
                        },
                        rng
                    );
        }

        private bool DecorationPatchCellIsAvailable(
            Zone Z,
            bool[,] reserved,
            Cell cell,
            bool broadOpen
        )
        {
            if (
                !CellIsAvailableForDecoration(
                    Z,
                    reserved,
                    cell
                )
            )
            {
                return false;
            }

            if (
                broadOpen &&
                !SubterraneanSites
                    .SubterraneanSitesEPPlacement
                    .HasBroadOpenClearance(
                        Z,
                        cell,
                        delegate (Cell neighbor)
                        {
                            return CellIsAvailableForDecoration(
                                Z,
                                reserved,
                                neighbor
                            );
                        }
                    )
            )
            {
                return false;
            }

            return true;
        }

        private void PlaceDecorationPatch(
            Zone Z,
            HashSet<Cell> patch,
            string blueprint,
            bool[,] reserved
        )
        {
            if (
                Z == null ||
                patch == null
            )
            {
                return;
            }

            foreach (Cell cell in patch)
            {
                if (cell == null)
                    continue;

                cell.AddObject(
                    blueprint
                );

                reserved[
                    cell.X,
                    cell.Y
                ] = true;

                //
                // Actual discrete C5 content owns this cell.
                //
                SubterraneanSites
                    .SubterraneanSitesEPReservations
                    .ClaimCell(
                        Z,
                        cell
                    );
            }
        }

        private void PlaceVineGrowths(
            Zone Z,
            bool[,] reserved,
            System.Random rng
        )
        {
            int totalTarget =
                rng.Next(
                    MinVines,
                    MaxVines + 1
                );

            int singlesTarget =
                rng.Next(
                    MinVineSingles,
                    MaxVineSingles + 1
                );

            if (singlesTarget > totalTarget)
                singlesTarget = totalTarget;

            int clusteredTarget =
                totalTarget - singlesTarget;

            if (clusteredTarget <= 0)
                return;

            //
            // The dominant patch gets roughly 60% of all
            // clustered vines.
            //
            int mainTarget =
                clusteredTarget *
                MainVinePercent /
                100;

            if (mainTarget < 1)
                mainTarget = 1;

            Cell mainSeed =
                PickDecorationSeed(
                    Z,
                    reserved,
                    rng,
                    wallAdjacent: true,
                    broadOpen: false
                );

            if (mainSeed == null)
                return;

            HashSet<Cell> mainPatch =
                GrowDecorationPatch(
                    Z,
                    reserved,
                    mainSeed,
                    mainTarget,
                    rng,
                    broadOpen: false
                );

            PlaceDecorationPatch(
                Z,
                mainPatch,
                VineBlueprint,
                reserved
            );

            int remaining =
                clusteredTarget -
                mainPatch.Count;

            int satelliteCount =
                rng.Next(
                    MinVineSatellitePatches,
                    MaxVineSatellitePatches + 1
                );

            for (
                int i = 0;
                i < satelliteCount &&
                remaining > 0;
                i++
            )
            {
                int patchesLeft =
                    satelliteCount - i;

                int target =
                    remaining /
                    patchesLeft;

                //
                // Make satellite sizes uneven.
                //
                target +=
                    rng.Next(
                        -2,
                        3
                    );

                if (target < 2)
                    target = 2;

                if (target > remaining)
                    target = remaining;

                Cell satelliteSeed =
                    PickDecorationSeedNearPatch(
                        Z,
                        reserved,
                        mainPatch,
                        rng,
                        2,
                        8
                    );

                if (satelliteSeed == null)
                    continue;

                HashSet<Cell> satellite =
                    GrowDecorationPatch(
                        Z,
                        reserved,
                        satelliteSeed,
                        target,
                        rng,
                        broadOpen: false
                    );

                PlaceDecorationPatch(
                    Z,
                    satellite,
                    VineBlueprint,
                    reserved
                );

                remaining -=
                    satellite.Count;
            }

            //
            // Singles occur in the loose perimeter around
            // the principal growth.
            //
            for (int i = 0; i < singlesTarget; i++)
            {
                Cell single =
                    PickDecorationSeedNearPatch(
                        Z,
                        reserved,
                        mainPatch,
                        rng,
                        1,
                        10
                    );

                if (single == null)
                    break;

                single.AddObject(
                    VineBlueprint
                );

                reserved[
                    single.X,
                    single.Y
                ] = true;

                SubterraneanSites
                    .SubterraneanSitesEPReservations
                    .ClaimCell(
                        Z,
                        single
                    );
            }
        }

        private void PlaceShroomPatches(
            Zone Z,
            bool[,] reserved,
            System.Random rng
        )
        {
            int patchCount =
                rng.Next(
                    MinShroomPatches,
                    MaxShroomPatches + 1
                );

            for (int i = 0; i < patchCount; i++)
            {
                int target =
                    rng.Next(
                        MinShroomsPerPatch,
                        MaxShroomsPerPatch + 1
                    );

                Cell seed =
                    PickDecorationSeed(
                        Z,
                        reserved,
                        rng,
                        wallAdjacent: false,
                        broadOpen: false
                    );

                if (seed == null)
                    continue;

                HashSet<Cell> patch =
                    GrowDecorationPatch(
                        Z,
                        reserved,
                        seed,
                        target,
                        rng,
                        broadOpen: false
                    );

                PlaceDecorationPatch(
                    Z,
                    patch,
                    BrightshroomBlueprint,
                    reserved
                );
            }
        }

        private void PlaceDebrisClusters(
            Zone Z,
            bool[,] reserved,
            System.Random rng
        )
        {
            int clusterCount =
                rng.Next(
                    MinDebrisClusters,
                    MaxDebrisClusters + 1
                );

            for (int i = 0; i < clusterCount; i++)
            {
                int target =
                    rng.Next(
                        MinDebrisCells,
                        MaxDebrisCells + 1
                    );

                Cell seed =
                    PickDecorationSeed(
                        Z,
                        reserved,
                        rng,
                        wallAdjacent: false,
                        broadOpen: true
                    );

                if (seed == null)
                    continue;

                HashSet<Cell> patch =
                    GrowDecorationPatch(
                        Z,
                        reserved,
                        seed,
                        target,
                        rng,
                        broadOpen: true
                    );

                foreach (Cell cell in patch)
                {
                    if (cell == null)
                        continue;

                    string blueprint =
                        rng.Next(100) <
                        DebrisRubblePercent
                            ? RubbleBlueprint
                            : SmallBoulderBlueprint;

                    cell.AddObject(
                        blueprint
                    );

                    reserved[
                        cell.X,
                        cell.Y
                    ] = true;

                    SubterraneanSites
                        .SubterraneanSitesEPReservations
                        .ClaimCell(
                            Z,
                            cell
                        );
                }
            }
        }

        private bool CellIsAvailableForDecoration(
            Zone Z,
            bool[,] reserved,
            Cell cell
        )
        {
            if (cell == null)
                return false;

            if (!CellIsInsideDecorationScope(Z, cell))
                return false;

            int x = cell.X;
            int y = cell.Y;

            if (
                x < 0 ||
                y < 0 ||
                x >= Z.Width ||
                y >= Z.Height
            )
            {
                return false;
            }

            if (reserved[x, y])
                return false;

            //
            // Respect semantic ownership from earlier EP content.
            //
            // This catches C1 objects, C2 functional footprints, cryochambers,
            // and Cold/Fungus pools without treating liquid itself as occupied.
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
            // No generic liquid exclusion.
            //
            // Stillvines, brightshrooms, rubble, and boulders may all be placed
            // in permissive spills such as Ooze or Fire liquids. Object-like pools
            // exclude them through reservation claims instead.
            //
            return true;
        }

        private bool TouchesSolidCardinal(
            Cell cell
        )
        {
            string[] directions =
            {
                "N",
                "S",
                "E",
                "W"
            };

            foreach (string direction in directions)
            {
                Cell neighbor =
                    cell.GetCellFromDirection(
                        direction
                    );

                if (
                    neighbor != null &&
                    neighbor.IsSolid()
                )
                {
                    return true;
                }
            }

            return false;
        }

    }
}



namespace XRL.World.Parts
{
    /// <summary>
    /// Directional cryogenic vent for the Cold EP.
    ///
    /// Produces a strong plume immediately in front of the vent,
    /// with density falling off over four cells. Existing CryoGas
    /// is reinforced back up to the intended density rather than
    /// accumulated without limit.
    /// </summary>
    [Serializable]
    public class SubterraneanSitesColdVent : IPart
    {
        public string Direction = "N";

        //
        // Tuning knobs.
        public int GasDensity = 48;
        public int ForwardLength = 4;
        public int DensityFalloff = 12;

        public int SideSprayChance = 60;
        public int SideGasDensity = 36;

        public override bool SameAs(IPart p)
        {
            return false;
        }

        public override void Register(
            GameObject Object,
            IEventRegistrar Registrar
        )
        {
            Registrar.Register("WalltrapTrigger");
            base.Register(Object, Registrar);
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "WalltrapTrigger")
            {
                Emit();
            }

            return true;
        }

        private void Emit()
        {
            Cell source = ParentObject.GetCurrentCell();

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
            Cell current = source;

            for (int i = 0; i < ForwardLength; i++)
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

                int density =
                    GasDensity -
                    (i * DensityFalloff);

                if (density < 1)
                    density = 1;

                AddCryoGas(
                    current,
                    density
                );
            }

            //
            // Occasional diagonal spill immediately beside
            // the mouth of the vent.
            //
            List<string> sideDirections =
                Directions.GetAdjacentDirections(
                    Direction,
                    1
                );

            foreach (string sideDirection in sideDirections)
            {
                if (!SideSprayChance.in100())
                    continue;

                Cell side =
                    source.GetCellFromDirection(
                        sideDirection
                    );

                if (
                    side == null ||
                    side.IsSolid(true)
                )
                {
                    continue;
                }

                AddCryoGas(
                    side,
                    SideGasDensity
                );
            }
        }

        private void AddCryoGas(
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
            // If gas already occupies the cell, only interact
            // with it if it is CryoGas.
            //
            foreach (GameObject obj in cell.GetObjects())
            {
                if (obj == null)
                    continue;

                Gas existingGas =
                    obj.GetPart<Gas>();

                if (existingGas == null)
                    continue;

                //
                // Don't overwrite another gas type.
                //
                if (obj.Blueprint != "CryoGas")
                    return;

                //
                // Maintain the designed plume strength.
                // Do NOT keep adding density every firing cycle.
                //
                if (existingGas.Density < density)
                {
                    existingGas.Density = density;
                }

                return;
            }

            //
            // No gas exists here yet.
            //
            GameObject gasObject =
                GameObject.Create(
                    "CryoGas"
                );

            if (gasObject == null)
                return;

            Gas gas =
                gasObject.GetPart<Gas>();

            if (gas != null)
            {
                gas.Density = density;
                gas.Creator = ParentObject;
            }

            cell.AddObject(gasObject);
        }
    }
}
