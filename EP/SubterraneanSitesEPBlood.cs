using System;
using System.Collections.Generic;
using Genkit;
using XRL;
using XRL.Core;
using XRL.World;
using XRL.World.AI.Pathfinding;
using XRL.World.Parts;
using XRL.Rules;
using XRL.World.Effects;
using XRL.World.Anatomy;
using XRL.World.Parts.Mutation;


namespace SubterraneanSites
{
    /// <summary>
    /// BLOOD DIMENSION
    ///
    /// Category 1:
    ///     bleeding pressure, conveyor/dismemberment systems,
    ///     and Regeneration attunement.
    ///
    /// Category 2:
    ///     blood-filled madpole hazard pools.
    ///
    /// Category 3:
    ///     fulcrete structural mass with exposed hot-tubing machine walls.
    ///
    /// Category 4:
    ///     institutional ward/procedure layout built around a long central spine,
    ///     over a dark industrial metal floor.
    ///
    /// Category 5:
    ///     medical furnishings, blood storage, regeneration equipment, and gore.
    /// </summary>
    internal sealed class SubterraneanSitesEPBloodTheme :
        ISubterraneanSitesEPCategoryProvider,
        ISubterraneanSitesEPPrimaryObjectProvider,
        ISubterraneanSitesEPAttunementProvider,
        ISubterraneanSitesEPDenizenAdaptationProvider,
        ISubterraneanSitesEPSignatureMutationProvider
    {
        public string ThemeKey
        {
            get { return "Blood"; }
        }

        public string SignatureMutationClass
        {
            get { return "Regeneration"; }
        }


        public void ApplyDenizenAdaptation(
            GameObject creature
        )
        {
            //
            // Blood's permanent denizen adaptation is entirely its
            // signature Regeneration mutation, supplied by the common
            // dimensional mutation package.
            //
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
            // Blood C1 is a runtime player environment.
            //
            // RequireSystem is idempotent, so every Blood-primary layer can
            // safely request the same persistent game system.
            //
            The.Game.RequireSystem<
                SubterraneanSitesEPBloodEnvironmentSystem
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
                "SubterraneanSitesBloodConveyors",
                "MinSystems", "2",
                "MaxSystems", "4",
                "PreferredMinLength", "15",
                "MinimumLength", "8",
                "MaximumLength", "30",
                "TransitionExclusionRadius", "5"
            );
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

                        "",
                        0,

                        "",
                        "",
                        0,

                        SignatureMutationClass,
                        mutationLevel
                    );

            successMessage =
                "Attunement grants:\n" +
                "Regeneration (level " +
                mutationLevel.ToString() +
                ")\n" +
                "Extradimensional bleeding is suppressed.";

            return
                SubterraneanSitesEPAttunementBuildResult
                    .Success;
        }


        public void RegisterEntranceCategory1Objects(
            SubterraneanSitesEPEntranceContext context
        )
        {
            if (context == null)
                return;

            The.ZoneManager.AddZonePostBuilder(
                context.ZoneId,
                "SubterraneanSitesBloodConveyors",
                "EntranceOnly", "1",
                "MinSystems", "1",
                "MaxSystems", "1",
                "PreferredMinLength", "6",
                "MinimumLength", "4",
                "MaximumLength", "10"
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
                "SubterraneanSitesBloodMadpolePools"
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
                "SubterraneanSitesBloodMaterials"
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
                "SubterraneanSitesBloodWardLayout"
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
                "SubterraneanSitesBloodFloor"
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
                "SubterraneanSitesBloodDecorations"
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
                "SubterraneanSitesBloodFloor",
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
                "SubterraneanSitesBloodDecorations",
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
                        "SmallHexFloor",

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
            // Preserve SmallHexFloor's existing graphical tile.
            // Recolor it into Blood's red-grey industrial palette.
            //
            floor.Render.ColorString =
                "&K^k";

            floor.Render.TileColor =
                "&K";

            floor.Render.DetailColor =
                "R";
        }
    }
}


namespace XRL.World.ZoneBuilders
{


    /// <summary>
    /// Blood Category 5.
    ///
    /// Medical / processing remnants:
    /// - grouped wall-side medical furniture
    /// - peptic and hyperbiotic chairs
    /// - occasional torture chair
    /// - medical lockers
    /// - medical signs
    /// - alchemist tables
    /// - techlights
    /// - heavy garbage and bone debris
    /// - one enormous permissive blood spill
    /// - one or two smaller permissive blood spills
    ///
    /// Regeneration tanks are intentionally added separately once their
    /// vanilla operational-state implementation has been inspected.
    /// </summary>
    public class SubterraneanSitesBloodDecorations :
        ZoneBuilderSandbox
    {


        public int EntranceOnly = 0;

        public int TransitionExclusionRadius = 4;

        //
        // Medical installations.
        //
        public int MinMedicalClusters = 3;
        public int MaxMedicalClusters = 5;

        public int MinObjectsPerCluster = 3;
        public int MaxObjectsPerCluster = 6;

        public int MedicalClusterRadius = 3;


        //
        // Rare / standalone objects.
        //
        public int MinTortureChairs = 0;
        public int MaxTortureChairs = 1;

        public int MinLights = 5;
        public int MaxLights = 9;

        public int MinLightSpacing = 4;


        //
        // Debris.
        //
        public int MinGarbage = 35;
        public int MaxGarbage = 60;

        public int MinBones = 25;
        public int MaxBones = 45;


        //
        // Blood spills.
        //
        public int MinLargeSpillPercent = 10;
        public int MaxLargeSpillPercent = 15;

        public int MinSmallSpills = 1;
        public int MaxSmallSpills = 2;

        public int MinSmallSpillCells = 10;
        public int MaxSmallSpillCells = 20;

        public int MinRegenTanks = 0;
        public int MaxRegenTanks = 2;

        public int OperationalRegenTankChance = 20;

        public int RegenTankMedicalClusterRadius = 5;


        private const string MedicalChairBlueprint =
            "Medical Chair";

        private const string HyperbioticChairBlueprint =
            "Hyperbiotic Chair";

        private const string TortureChairBlueprint =
            "Torture Chair";

        private const string MedLockerBlueprint =
            "MedLocker";

        private const string MedicalSignBlueprint =
            "YdMedicalSign";

        private const string AlchemistTableBlueprint =
            "Alchemist Table";

        private const string GarbageBlueprint =
            "Garbage";

        private const string BonesBlueprint =
            "Bones";

        private const string RegenTankBlueprint =
            "Regen Tank";


        private static readonly string[] LightBlueprints =
        {
            "Techlight1",
            "Techlight2",
            "Techlight3"
        };


        private sealed class MedicalDecoration
        {
            public string Blueprint;
            public int Weight;
        }


        private static readonly MedicalDecoration[] MedicalPool =
        {
            new MedicalDecoration
            {
                Blueprint = MedicalChairBlueprint,
                Weight = 7
            },

            new MedicalDecoration
            {
                Blueprint = HyperbioticChairBlueprint,
                Weight = 3
            },

            new MedicalDecoration
            {
                Blueprint = MedLockerBlueprint,
                Weight = 5
            },

            new MedicalDecoration
            {
                Blueprint = MedicalSignBlueprint,
                Weight = 2
            },

            new MedicalDecoration
            {
                Blueprint = AlchemistTableBlueprint,
                Weight = 4
            }
        };


        public bool BuildZone(
            Zone Z
        )
        {
            if (Z == null)
                return true;


            ClampSettings();


            int seed =
                XRLCore.Core.Game.GetWorldSeed(
                    "SubterraneanSites:BloodDecorations:" +
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
            // Discrete installations first.
            //
            PlaceMedicalClusters(
                Z,
                anchors,
                rng
            );

            PlaceRegenTanks(
                Z,
                anchors,
                rng
            );


            PlaceTortureChairs(
                Z,
                anchors,
                rng
            );


            PlaceLights(
                Z,
                anchors,
                rng
            );


            //
            // Then filthy loose debris.
            //
            PlaceLooseDebris(
                Z,
                anchors,
                rng,
                GarbageBlueprint,
                MinGarbage,
                MaxGarbage
            );


            PlaceLooseDebris(
                Z,
                anchors,
                rng,
                BonesBlueprint,
                MinBones,
                MaxBones
            );


            //
            // Blood comes LAST.
            //
            // It deliberately ignores reservation ownership and may spread
            // underneath chairs, lockers, garbage, bones, conveyors, etc.
            //
            PlaceBloodSpills(
                Z,
                anchors,
                rng
            );


            return true;
        }

        private void PlaceRegenTanks(
            Zone Z,
            List<Location2D> anchors,
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


            int desired =
                rng.Next(
                    MinRegenTanks,
                    MaxRegenTanks + 1
                );


            if (desired <= 0)
                return;


            List<Cell> medicalCells =
                GetExistingMedicalCells(
                    Z
                );


            for (
                int i = 0;
                i < desired;
                i++
            )
            {
                List<Cell> candidates =
                    GetWallSideDecorationCells(
                        Z,
                        anchors
                    );


                //
                // Prefer placing regeneration tanks near an already-created
                // medical cluster.
                //
                if (medicalCells.Count > 0)
                {
                    List<Cell> nearMedical =
                        candidates.FindAll(
                            cell =>
                                IsNearAnyCell(
                                    cell,
                                    medicalCells,
                                    RegenTankMedicalClusterRadius
                                )
                        );


                    if (nearMedical.Count > 0)
                    {
                        candidates =
                            nearMedical;
                    }
                }


                if (candidates.Count == 0)
                    return;


                Cell destination =
                    candidates[
                        rng.Next(
                            candidates.Count
                        )
                    ];


                GameObject tank =
                    GameObjectFactory
                        .Factory
                        .CreateObject(
                            RegenTankBlueprint
                        );


                if (tank == null)
                    continue;


                ConfigureRegenTank(
                    tank,
                    rng
                );


                destination.AddObject(
                    tank
                );


                SubterraneanSites
                    .SubterraneanSitesEPReservations
                    .ClaimCell(
                        Z,
                        destination
                    );


                //
                // Let a second tank naturally join this medical installation.
                //
                medicalCells.Add(
                    destination
                );
            }
        }

        private List<Cell> GetExistingMedicalCells(
            Zone Z
        )
        {
            List<Cell> result =
                new List<Cell>();


            if (Z == null)
                return result;


            foreach (
                Cell cell
                in Z.GetCells()
            )
            {
                if (cell == null)
                    continue;


                if (
                    cell.HasObjectWithBlueprint(
                        MedicalChairBlueprint
                    ) ||
                    cell.HasObjectWithBlueprint(
                        HyperbioticChairBlueprint
                    ) ||
                    cell.HasObjectWithBlueprint(
                        MedLockerBlueprint
                    ) ||
                    cell.HasObjectWithBlueprint(
                        MedicalSignBlueprint
                    ) ||
                    cell.HasObjectWithBlueprint(
                        AlchemistTableBlueprint
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

        private void ConfigureRegenTank(
            GameObject tank,
            System.Random rng
        )
        {
            if (
                tank == null ||
                rng == null
            )
            {
                return;
            }


            LiquidVolume liquid =
                tank.LiquidVolume;


            Capacitor capacitor =
                tank.GetPart<Capacitor>();


            bool operational =
                rng.Next(100) <
                OperationalRegenTankChance;


            if (operational)
            {
                //
                // FULLY FUNCTIONAL TANK.
                //
                // 63:1 convalessence/cloning is the vanilla functional mixture.
                // At 128 drams this contains about 2 drams of cloning draught,
                // comfortably exceeding RegenTank's 850-millidram requirement.
                //
                if (liquid != null)
                {
                    liquid.Empty();

                    liquid.InitialLiquid =
                        "convalessence-63,cloning-1";

                    liquid.Volume =
                        liquid.MaxVolume > 0
                            ? liquid.MaxVolume
                            : 128;

                    liquid.CheckImage();
                }


                //
                // The RegenTank part is powered and calls IsReady(), so liquid
                // alone is not sufficient. Give the surviving tank a full capacitor.
                //
                if (capacitor != null)
                {
                    capacitor.Charge =
                        capacitor.MaxCharge;
                }


                return;
            }


            //
            // NONFUNCTIONAL BUT INTACT TANK.
            //
            // Guarantee failure by keeping it below the vanilla 100-dram minimum.
            // Some are nearly empty; others look frustratingly close to usable.
            //
            if (liquid != null)
            {
                liquid.Empty();


                int maximum =
                    liquid.MaxVolume > 0
                        ? Math.Min(
                            99,
                            liquid.MaxVolume
                        )
                        : 99;


                if (
                    maximum > 0 &&
                    rng.Next(100) < 80
                )
                {
                    liquid.InitialLiquid =
                        "convalessence-1000";

                    liquid.Volume =
                        rng.Next(
                            Math.Min(
                                10,
                                maximum
                            ),
                            maximum + 1
                        );
                }
                else
                {
                    liquid.Volume =
                        0;
                }


                liquid.CheckImage();
            }


            //
            // Its remaining charge can be arbitrary; the underfilled liquid
            // already guarantees that the tank is not ready.
            //
            if (
                capacitor != null &&
                capacitor.MaxCharge > 0
            )
            {
                capacitor.Charge =
                    rng.Next(
                        capacitor.MaxCharge + 1
                    );
            }
        }


        private bool IsNearAnyCell(
            Cell candidate,
            List<Cell> others,
            int radius
        )
        {
            if (
                candidate == null ||
                others == null
            )
            {
                return false;
            }


            int radiusSquared =
                radius *
                radius;


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
                    dy * dy <=
                    radiusSquared
                )
                {
                    return true;
                }
            }


            return false;
        }

        private void ConfigureBloodMedicalLocker(
            GameObject locker,
            System.Random rng
        )
        {
            if (
                locker == null ||
                rng == null
            )
            {
                return;
            }


            //
            // Remove the ordinary vanilla medical inventory.
            //
            IList<GameObject> existing =
                locker.GetContents();


            if (
                existing != null &&
                existing.Count > 0
            )
            {
                List<GameObject> remove =
                    new List<GameObject>(
                        existing
                    );


                foreach (
                    GameObject item
                    in remove
                )
                {
                    if (item != null)
                    {
                        item.Release();
                    }
                }
            }


            SubterraneanSites
                .SubterraneanSitesDimensionBinding bloodDimension =
                    SubterraneanSites
                        .SubterraneanSitesDimensionEngine
                        .GetBindingByThemeKey(
                            "Blood"
                        );


            int count =
                rng.Next(
                    1,
                    4
                );


            Inventory inventory =
                locker.RequirePart<Inventory>();


            for (
                int i = 0;
                i < count;
                i++
            )
            {
                GameObject phial =
                    GameObject.Create(
                        "Phial"
                    );


                if (
                    phial == null ||
                    phial.LiquidVolume == null
                )
                {
                    if (phial != null)
                        phial.Destroy();

                    continue;
                }


                phial.LiquidVolume.InitialLiquid =
                    "blood-1000";

                phial.LiquidVolume.Volume =
                    1;

                phial.LiquidVolume.CheckImage();


                if (bloodDimension != null)
                {
                    SubterraneanSites
                        .SubterraneanSitesDimensionEngine
                        .ApplyDimensionToItem(
                            phial,
                            bloodDimension
                        );
                }


                inventory.AddObject(
                    phial
                );
            }
        }


        // ================================================================
        // MEDICAL CLUSTERS
        // ================================================================

        private void PlaceMedicalClusters(
            Zone Z,
            List<Location2D> anchors,
            System.Random rng
        )
        {
            int desired =
                rng.Next(
                    MinMedicalClusters,
                    MaxMedicalClusters + 1
                );


            List<Cell> usedAnchors =
                new List<Cell>();


            for (
                int clusterIndex = 0;
                clusterIndex < desired;
                clusterIndex++
            )
            {
                List<Cell> anchorsAvailable =
                    GetWallSideDecorationCells(
                        Z,
                        anchors
                    );


                //
                // Don't pile all medical stations into one corner.
                //
                anchorsAvailable.RemoveAll(
                    cell =>
                        !FarEnoughFromCells(
                            cell,
                            usedAnchors,
                            6
                        )
                );


                if (anchorsAvailable.Count == 0)
                    return;


                Cell anchor =
                    anchorsAvailable[
                        rng.Next(
                            anchorsAvailable.Count
                        )
                    ];


                usedAnchors.Add(
                    anchor
                );


                int objectCount =
                    rng.Next(
                        MinObjectsPerCluster,
                        MaxObjectsPerCluster + 1
                    );


                for (
                    int i = 0;
                    i < objectCount;
                    i++
                )
                {
                    List<Cell> local =
                        GetLocalMedicalCells(
                            Z,
                            anchors,
                            anchor,
                            MedicalClusterRadius
                        );


                    if (local.Count == 0)
                        break;


                    Cell cell =
                        local[
                            rng.Next(
                                local.Count
                            )
                        ];


                    string blueprint =
                        PickMedicalBlueprint(
                            rng
                        );


                    PlaceClaimedObject(
                        Z,
                        cell,
                        blueprint,
                        rng
                    );
                }
            }
        }


        private string PickMedicalBlueprint(
            System.Random rng
        )
        {
            int totalWeight =
                0;


            foreach (
                MedicalDecoration entry
                in MedicalPool
            )
            {
                totalWeight +=
                    Math.Max(
                        0,
                        entry.Weight
                    );
            }


            if (totalWeight <= 0)
                return MedicalChairBlueprint;


            int roll =
                rng.Next(
                    totalWeight
                );


            foreach (
                MedicalDecoration entry
                in MedicalPool
            )
            {
                int weight =
                    Math.Max(
                        0,
                        entry.Weight
                    );


                if (roll < weight)
                    return entry.Blueprint;


                roll -=
                    weight;
            }


            return MedicalChairBlueprint;
        }


        private List<Cell> GetLocalMedicalCells(
            Zone Z,
            List<Location2D> anchors,
            Cell center,
            int radius
        )
        {
            List<Cell> result =
                new List<Cell>();


            if (
                Z == null ||
                center == null
            )
            {
                return result;
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
                    Cell cell =
                        Z.GetCell(
                            center.X + dx,
                            center.Y + dy
                        );


                    if (
                        !IsDiscreteDecorationCell(
                            Z,
                            cell,
                            anchors
                        )
                    )
                    {
                        continue;
                    }


                    //
                    // Underground medical fixtures preferentially hug room walls.
                    //
                    // The entrance scar has been cleared and does not have Blood C4's
                    // institutional walls, so scar decoration may use any otherwise-valid
                    // scar cell.
                    //
                    if (
                        EntranceOnly == 0 &&
                        !TouchesSolidCardinal(
                            cell
                        )
                    )
                    {
                        continue;
                    }
                    
                    //
                    // Blood spills are permissive around ordinary objects, but keep
                    // conveyor machinery visually readable.
                    //
                    if (
                        cell.HasObjectWithPart(
                            "ConveyorPad"
                        )
                    )
                    {
                        continue;
                    }


                    result.Add(
                        cell
                    );
                }
            }


            return result;
        }


        // ================================================================
        // TORTURE CHAIRS
        // ================================================================

        private void PlaceTortureChairs(
            Zone Z,
            List<Location2D> anchors,
            System.Random rng
        )
        {
            int desired =
                rng.Next(
                    MinTortureChairs,
                    MaxTortureChairs + 1
                );


            for (
                int i = 0;
                i < desired;
                i++
            )
            {
                List<Cell> cells =
                    GetWallSideDecorationCells(
                        Z,
                        anchors
                    );


                if (cells.Count == 0)
                    return;


                Cell cell =
                    cells[
                        rng.Next(
                            cells.Count
                        )
                    ];


                PlaceClaimedObject(
                    Z,
                    cell,
                    TortureChairBlueprint
                );
            }
        }


        // ================================================================
        // LIGHTS
        // ================================================================

        private void PlaceLights(
            Zone Z,
            List<Location2D> anchors,
            System.Random rng
        )
        {
            int desired =
                rng.Next(
                    MinLights,
                    MaxLights + 1
                );


            List<Cell> selected =
                new List<Cell>();


            for (
                int i = 0;
                i < desired;
                i++
            )
            {
                List<Cell> candidates =
                    GetWallSideDecorationCells(
                        Z,
                        anchors
                    );


                candidates.RemoveAll(
                    cell =>
                        !FarEnoughFromCells(
                            cell,
                            selected,
                            MinLightSpacing
                        )
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
                    LightBlueprints[
                        rng.Next(
                            LightBlueprints.Length
                        )
                    ];


                if (
                    PlaceClaimedObject(
                        Z,
                        cell,
                        blueprint
                    )
                )
                {
                    selected.Add(
                        cell
                    );
                }
            }
        }


        // ================================================================
        // GARBAGE / BONES
        // ================================================================

        private void PlaceLooseDebris(
            Zone Z,
            List<Location2D> anchors,
            System.Random rng,
            string blueprint,
            int minimum,
            int maximum
        )
        {
            int desired =
                rng.Next(
                    minimum,
                    maximum + 1
                );


            List<Cell> candidates =
                GetLooseDebrisCells(
                    Z,
                    anchors
                );


            if (candidates.Count == 0)
                return;


            for (
                int i = 0;
                i < desired &&
                candidates.Count > 0;
                i++
            )
            {
                int index =
                    rng.Next(
                        candidates.Count
                    );


                Cell cell =
                    candidates[
                        index
                    ];


                //
                // Loose debris is still discrete decoration.
                // Do not deliberately stack several debris objects
                // onto the same tile.
                //
                candidates.RemoveAt(
                    index
                );


                if (cell == null)
                    continue;


                cell.AddObject(
                    blueprint
                );


                //
                // Protect this discrete decoration from later discrete
                // EP placement. Permissive liquids may still spread
                // underneath it.
                //
                SubterraneanSites
                    .SubterraneanSitesEPReservations
                    .ClaimCell(
                        Z,
                        cell
                    );
            }
        }


        // ================================================================
        // BLOOD SPILLS
        // ================================================================

        private void PlaceBloodSpills(
            Zone Z,
            List<Location2D> anchors,
            System.Random rng
        )
        {
            List<Cell> interior =
                GetSpillCells(
                    Z,
                    anchors
                );


            if (interior.Count == 0)
                return;


            //
            // One dominant contiguous catastrophic spill.
            //
            int percent =
                rng.Next(
                    MinLargeSpillPercent,
                    MaxLargeSpillPercent + 1
                );


            int largeTarget =
                Math.Max(
                    1,
                    interior.Count *
                        percent /
                        100
                );


            HashSet<Cell> large =
                SubterraneanSites
                    .SubterraneanSitesEPPlacement
                    .GrowBestPatch(
                        new List<Cell>(
                            interior
                        ),
                        largeTarget,
                        12,
                        null,
                        null
                    );


            foreach (
                Cell cell
                in large
            )
            {
                AddBlood(
                    cell
                );
            }


            //
            // Smaller spills should read as separate accidents rather than
            // simply thickening the edge of the giant spill.
            //
            List<Cell> remaining =
                new List<Cell>(
                    interior
                );


            remaining.RemoveAll(
                cell =>
                    large.Contains(
                        cell
                    )
            );


            int smallCount =
                rng.Next(
                    MinSmallSpills,
                    MaxSmallSpills + 1
                );


            for (
                int i = 0;
                i < smallCount;
                i++
            )
            {
                if (remaining.Count == 0)
                    break;


                int target =
                    rng.Next(
                        MinSmallSpillCells,
                        MaxSmallSpillCells + 1
                    );


                HashSet<Cell> patch =
                    SubterraneanSites
                        .SubterraneanSitesEPPlacement
                        .GrowBestPatch(
                            new List<Cell>(
                                remaining
                            ),
                            target,
                            6,
                            null,
                            null
                        );


                foreach (
                    Cell cell
                    in patch
                )
                {
                    AddBlood(
                        cell
                    );
                }


                remaining.RemoveAll(
                    cell =>
                        patch.Contains(
                            cell
                        )
                );
            }
        }


        private void AddBlood(
            Cell cell
        )
        {
            if (cell == null)
                return;

            //
            // Blood is a permissive spill:
            // - ignore EP reservations
            // - coexist with discrete objects
            // - mix into an existing open liquid
            //
            SubterraneanSites
                .SubterraneanSitesEPLiquids
                .AddOrMixLiquid(
                    cell,
                    "blood",
                    "BloodPool"
                );
        }


        // ================================================================
        // CELL COLLECTION
        // ================================================================

        private List<Cell> GetWallSideDecorationCells(
            Zone Z,
            List<Location2D> anchors
        )
        {
            List<Cell> result =
                new List<Cell>();


            if (Z == null)
                return result;


            foreach (
                Cell cell
                in Z.GetCells()
            )
            {
                if (
                    !IsDiscreteDecorationCell(
                        Z,
                        cell,
                        anchors
                    )
                )
                {
                    continue;
                }

                if (
                    EntranceOnly == 0 &&
                    !TouchesSolidCardinal(
                        cell
                    )
                )
                {
                    continue;
                }


                result.Add(
                    cell
                );
            }


            return result;
        }


        private List<Cell> GetLooseDebrisCells(
            Zone Z,
            List<Location2D> anchors
        )
        {
            List<Cell> result =
                new List<Cell>();


            if (Z == null)
                return result;


            foreach (
                Cell cell
                in Z.GetCells()
            )
            {
                if (
                    !IsDiscreteDecorationCell(
                        Z,
                        cell,
                        anchors
                    )
                )
                {
                    continue;
                }


                result.Add(
                    cell
                );
            }


            return result;
        }


       private List<Cell> GetSpillCells(
            Zone Z,
            List<Location2D> anchors
        )
        {
            List<Cell> result =
                new List<Cell>();


            if (Z == null)
                return result;


            foreach (
                Cell cell
                in Z.GetCells()
            )
            {
                if (cell == null)
                    continue;


                if (EntranceOnly != 0)
                {
                    //
                    // Entrance decoration is confined strictly to the EP scar.
                    //
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
                        continue;
                    }
                }
                else
                {
                    //
                    // Underground decoration uses whichever C4 geometry won.
                    //
                    if (
                        !SubterraneanSites
                            .SubterraneanSitesEPGeometry
                            .IsOpenGeometryCell(
                                cell
                            )
                    )
                    {
                        continue;
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
                        continue;
                    }
                }


                if (!cell.IsEmptyOfSolid())
                    continue;


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
                    continue;
                }


                result.Add(
                    cell
                );
            }


            return result;
        }

        private bool IsDiscreteDecorationCell(
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
            // Underground C5 uses the winning Category-4 open geometry.
            //
            // Entrance C5 instead uses only the localized entrance scar,
            // because the rest of the origin zone belongs to vanilla Qud.
            //
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


            //
            // Respect C1/C2 and earlier semantic ownership.
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


            if (cell.HasSpawnBlocker())
                return false;


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
                    .HasMeaningfulOccupant(
                        cell
                    )
            )
            {
                return false;
            }


            return true;
        }


       

        private bool TouchesSolidCardinal(
            Cell cell
        )
        {
            if (cell == null)
                return false;


            string[] directions =
            {
                "N",
                "S",
                "E",
                "W"
            };


            foreach (
                string direction
                in directions
            )
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


        private bool PlaceClaimedObject(
            Zone Z,
            Cell cell,
            string blueprint,
            System.Random rng = null
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


            GameObject obj =
                GameObjectFactory.Factory
                    .CreateObject(
                        blueprint
                    );


            if (obj == null)
                return false;

            if (
                blueprint ==
                    MedLockerBlueprint &&
                rng != null
            )
            {
                ConfigureBloodMedicalLocker(
                    obj,
                    rng
                );
            }


            cell.AddObject(
                obj
            );


            SubterraneanSites
                .SubterraneanSitesEPReservations
                .ClaimCell(
                    Z,
                    cell
                );


            return true;
        }


        private void ClampSettings()
        {
            MinMedicalClusters =
                Math.Max(
                    0,
                    MinMedicalClusters
                );

            MaxMedicalClusters =
                Math.Max(
                    MinMedicalClusters,
                    MaxMedicalClusters
                );


            MinObjectsPerCluster =
                Math.Max(
                    1,
                    MinObjectsPerCluster
                );

            MaxObjectsPerCluster =
                Math.Max(
                    MinObjectsPerCluster,
                    MaxObjectsPerCluster
                );


            MedicalClusterRadius =
                Math.Max(
                    1,
                    MedicalClusterRadius
                );


            MinTortureChairs =
                Math.Max(
                    0,
                    MinTortureChairs
                );

            MaxTortureChairs =
                Math.Max(
                    MinTortureChairs,
                    MaxTortureChairs
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


            MinLightSpacing =
                Math.Max(
                    1,
                    MinLightSpacing
                );


            MinGarbage =
                Math.Max(
                    0,
                    MinGarbage
                );

            MaxGarbage =
                Math.Max(
                    MinGarbage,
                    MaxGarbage
                );


            MinBones =
                Math.Max(
                    0,
                    MinBones
                );

            MaxBones =
                Math.Max(
                    MinBones,
                    MaxBones
                );


            MinLargeSpillPercent =
                Math.Max(
                    0,
                    MinLargeSpillPercent
                );

            MaxLargeSpillPercent =
                Math.Max(
                    MinLargeSpillPercent,
                    MaxLargeSpillPercent
                );


            MinSmallSpills =
                Math.Max(
                    0,
                    MinSmallSpills
                );

            MaxSmallSpills =
                Math.Max(
                    MinSmallSpills,
                    MaxSmallSpills
                );


            MinSmallSpillCells =
                Math.Max(
                    1,
                    MinSmallSpillCells
                );

            MaxSmallSpillCells =
                Math.Max(
                    MinSmallSpillCells,
                    MaxSmallSpillCells
                );


            TransitionExclusionRadius =
                Math.Max(
                    0,
                    TransitionExclusionRadius
                );
            MinRegenTanks =
                Math.Max(
                    0,
                    MinRegenTanks
                );

            MaxRegenTanks =
                Math.Max(
                    MinRegenTanks,
                    MaxRegenTanks
                );

            OperationalRegenTankChance =
                Math.Max(
                    0,
                    Math.Min(
                        100,
                        OperationalRegenTankChance
                    )
                );

            RegenTankMedicalClusterRadius =
                Math.Max(
                    1,
                    RegenTankMedicalClusterRadius
                );
        }
    }

    /// <summary>
    /// Blood Category 2.
    ///
    /// Constructs deliberately artificial blood tanks framed in black marble
    /// walkway and stocks them with extradimensional madpoles.
    ///
    /// Small tanks:
    ///     4x4 liquid interior
    ///     6x6 total footprint
    ///     1-3 madpoles
    ///
    /// Large tanks:
    ///     8x5 liquid interior
    ///     10x7 total footprint
    ///     10-20 madpoles
    ///
    /// Large tanks may rotate to 5x8 depending on available geometry.
    ///
    /// The entire installation is claimed as exclusive C2 space so later
    /// decorations and shared denizens do not occupy the tanks.
    /// </summary>
    public class SubterraneanSitesBloodMadpolePools :
        ZoneBuilderSandbox
    {
        public string BloodBlueprint =
            "BloodPool";

        public string BorderBlueprint =
            "BlackMarbleWalkway";

        public string MadpoleBlueprint =
            "Madpole";

        public string DecorationThemeKey =
            "Blood";


        public int MinSmallPools = 3;
        public int MaxSmallPools = 5;

        public int MinLargePools = 1;
        public int MaxLargePools = 2;


        public int SmallPoolWidth = 2;
        public int SmallPoolHeight = 2;

        public int LargePoolWidth = 4;
        public int LargePoolHeight = 2;


        public int MinSmallMadpoles = 1;
        public int MaxSmallMadpoles = 3;

        public int MinLargeMadpoles = 4;
        public int MaxLargeMadpoles = 6;


        public int TransitionExclusionRadius = 5;

        public int OuterBorderExclusion = 1;

        public int PlacementAttemptsPerPool = 250;

        public int MadpoleRegenerationLevel = 10;

                //
        // Small-pool restraint machinery.
        //
        public string GrabberArmBlueprint =
            "GrabberArm";


        //
        // Large-pool forced-movement machinery.
        //
        public string FanNorthBlueprint =
            "SubterraneanSitesBloodFanNorth";

        public string FanSouthBlueprint =
            "SubterraneanSitesBloodFanSouth";

        public int MinFanGap = 2;
        public int MaxFanGap = 5;


        private sealed class FanBankPlan
        {
            public int MinX;
            public int MaxX;

            public int FanY;

            public int ReserveMinY;
            public int ReserveMaxY;

            public string Blueprint;

            public int Gap;
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
                    "SubterraneanSites:BloodMadpolePools:" +
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
            // Large installations first.
            //
            // They are much harder to fit, so giving them first choice prevents
            // a field of small pools from consuming every suitable rectangle.
            //
            int largeCount =
                rng.Next(
                    MinLargePools,
                    MaxLargePools + 1
                );


            for (
                int i = 0;
                i < largeCount;
                i++
            )
            {
                TryPlacePool(
                    Z,
                    anchors,
                    rng,
                    LargePoolWidth,
                    LargePoolHeight,
                    MinLargeMadpoles,
                    MaxLargeMadpoles,
                    largePool: true
                );
            }


            int smallCount =
                rng.Next(
                    MinSmallPools,
                    MaxSmallPools + 1
                );


            for (
                int i = 0;
                i < smallCount;
                i++
            )
            {
                TryPlacePool(
                    Z,
                    anchors,
                    rng,
                    SmallPoolWidth,
                    SmallPoolHeight,
                    MinSmallMadpoles,
                    MaxSmallMadpoles,
                    largePool: false
                );
            }


            return true;
        }

        private void GiveMadpoleRegeneration(
            GameObject madpole
        )
        {
            if (madpole == null)
                return;


            //
            // Blood C2 special case.
            //
            // These are environmental hazard creatures rather than ordinary
            // EP denizens, so give them only the Blood-theme power we want here
            // instead of running the complete random denizen mutation package.
            //
            if (madpole.HasPart("Regeneration"))
                return;


            try
            {
                Mutations mutations =
                    madpole.RequirePart<Mutations>();


                mutations.AddMutation(
                    "Regeneration",
                    Math.Max(
                        1,
                        MadpoleRegenerationLevel
                    )
                );
            }
            catch (Exception ex)
            {
                MetricsManager.LogException(
                    "Subterranean Sites Blood madpole regeneration",
                    ex
                );
            }
        }

               private bool TryPlacePool(
            Zone Z,
            List<Location2D> anchors,
            System.Random rng,
            int interiorWidth,
            int interiorHeight,
            int minMadpoles,
            int maxMadpoles,
            bool largePool
        )
        {
            if (
                Z == null ||
                rng == null
            )
            {
                return false;
            }


            for (
                int attempt = 0;
                attempt < PlacementAttemptsPerPool;
                attempt++
            )
            {
                int width =
                    interiorWidth;

                int height =
                    interiorHeight;


                //
                // One-cell black-marble rim on all four sides.
                //
                int totalWidth =
                    width + 2;

                int totalHeight =
                    height + 2;


                int minimumX =
                    OuterBorderExclusion;

                int minimumY =
                    OuterBorderExclusion;

                int maximumXExclusive =
                    Z.Width -
                    totalWidth -
                    OuterBorderExclusion +
                    1;

                int maximumYExclusive =
                    Z.Height -
                    totalHeight -
                    OuterBorderExclusion +
                    1;


                if (
                    maximumXExclusive <= minimumX ||
                    maximumYExclusive <= minimumY
                )
                {
                    return false;
                }


                int x1 =
                    rng.Next(
                        minimumX,
                        maximumXExclusive
                    );

                int y1 =
                    rng.Next(
                        minimumY,
                        maximumYExclusive
                    );

                int x2 =
                    x1 +
                    totalWidth -
                    1;

                int y2 =
                    y1 +
                    totalHeight -
                    1;


                if (
                    !FootprintIsAvailable(
                        Z,
                        anchors,
                        x1,
                        y1,
                        x2,
                        y2
                    )
                )
                {
                    continue;
                }


                FanBankPlan fanPlan =
                    null;


                //
                // A large tank is one complete functional installation.
                //
                // Do not commit the pool unless we can also give it a
                // four-wide fan bank with an intact pre-existing blow lane.
                //
                if (largePool)
                {
                    fanPlan =
                        FindLargeFanBankPlan(
                            Z,
                            anchors,
                            rng,
                            x1,
                            y1,
                            x2,
                            y2
                        );


                    if (fanPlan == null)
                    {
                        continue;
                    }
                }


                PlacePool(
                    Z,
                    rng,
                    x1,
                    y1,
                    width,
                    height,
                    minMadpoles,
                    maxMadpoles
                );


                if (largePool)
                {
                    PlaceFanBank(
                        Z,
                        fanPlan
                    );
                }
                else
                {
                    PlaceSmallPoolGrabber(
                        Z,
                        rng,
                        x1,
                        y1,
                        x2,
                        y2
                    );
                }


                return true;
            }


            return false;
        }


        private FanBankPlan FindLargeFanBankPlan(
            Zone Z,
            List<Location2D> anchors,
            System.Random rng,
            int poolX1,
            int poolY1,
            int poolX2,
            int poolY2
        )
        {
            if (
                Z == null ||
                rng == null
            )
            {
                return null;
            }


            //
            // Randomize which side gets first choice.
            //
            bool northFirst =
                rng.Next(2) == 0;


            FanBankPlan plan;


            if (
                TryFindFanBankOnSide(
                    Z,
                    anchors,
                    poolX1,
                    poolY1,
                    poolX2,
                    poolY2,
                    northFirst,
                    out plan
                )
            )
            {
                return plan;
            }


            if (
                TryFindFanBankOnSide(
                    Z,
                    anchors,
                    poolX1,
                    poolY1,
                    poolX2,
                    poolY2,
                    !northFirst,
                    out plan
                )
            )
            {
                return plan;
            }


            return null;
        }


        private bool TryFindFanBankOnSide(
            Zone Z,
            List<Location2D> anchors,
            int poolX1,
            int poolY1,
            int poolX2,
            int poolY2,
            bool northSide,
            out FanBankPlan plan
        )
        {
            plan =
                null;


            if (Z == null)
                return false;


            //
            // Fans align exactly with the four liquid columns.
            //
            int minX =
                poolX1 + 1;

            int maxX =
                poolX2 - 1;


            //
            // Prefer the most distant valid bank.
            //
            // If five tiles of open lane are unavailable, progressively
            // bring the machinery closer until the two-tile minimum.
            //
            for (
                int gap = MaxFanGap;
                gap >= MinFanGap;
                gap--
            )
            {
                int fanY;


                if (northSide)
                {
                    fanY =
                        poolY1 -
                        gap -
                        1;
                }
                else
                {
                    fanY =
                        poolY2 +
                        gap +
                        1;
                }


                //
                // Preserve the normal absolute border exclusion.
                //
                if (
                    fanY < OuterBorderExclusion ||
                    fanY >=
                        Z.Height -
                        OuterBorderExclusion
                )
                {
                    continue;
                }


                int reserveMinY =
                    northSide
                        ? fanY
                        : poolY2 + 1;

                int reserveMaxY =
                    northSide
                        ? poolY1 - 1
                        : fanY;


                //
                // C2 owns the complete machinery corridor:
                // fan bank + clear blow lane.
                //
                if (
                    !SubterraneanSites
                        .SubterraneanSitesEPReservations
                        .RectangleIsAvailable(
                            Z,
                            minX,
                            reserveMinY,
                            maxX,
                            reserveMaxY
                        )
                )
                {
                    continue;
                }


                bool valid =
                    true;


                //
                // The mounting strip may be open space OR C4 wall material.
                // Only these four mounting cells are allowed to be carved.
                //
                for (
                    int x = minX;
                    x <= maxX;
                    x++
                )
                {
                    Cell mount =
                        Z.GetCell(
                            x,
                            fanY
                        );


                    if (
                        !IsFanMountCellAvailable(
                            Z,
                            mount,
                            anchors
                        )
                    )
                    {
                        valid =
                            false;

                        break;
                    }
                }


                if (!valid)
                    continue;


                //
                // Everything between the fans and the tank must ALREADY
                // be open geometry. Do not turn C2 into a layout carver.
                //
                int laneMinY =
                    northSide
                        ? fanY + 1
                        : poolY2 + 1;

                int laneMaxY =
                    northSide
                        ? poolY1 - 1
                        : fanY - 1;


                for (
                    int x = minX;
                    x <= maxX &&
                    valid;
                    x++
                )
                {
                    for (
                        int y = laneMinY;
                        y <= laneMaxY;
                        y++
                    )
                    {
                        Cell lane =
                            Z.GetCell(
                                x,
                                y
                            );


                        if (
                            !IsFanLaneCellAvailable(
                                Z,
                                lane,
                                anchors
                            )
                        )
                        {
                            valid =
                                false;

                            break;
                        }
                    }
                }


                if (!valid)
                    continue;


                plan =
                    new FanBankPlan
                    {
                        MinX =
                            minX,

                        MaxX =
                            maxX,

                        FanY =
                            fanY,

                        ReserveMinY =
                            reserveMinY,

                        ReserveMaxY =
                            reserveMaxY,

                        Blueprint =
                            northSide
                                ? FanSouthBlueprint
                                : FanNorthBlueprint,
                        
                        Gap =
                            gap
                    };


                return true;
            }


            return false;
        }


        private bool IsFanLaneCellAvailable(
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


            if (cell.HasSpawnBlocker())
                return false;


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


            if (
                SubterraneanSites
                    .SubterraneanSitesEPPlacement
                    .HasMeaningfulOccupant(
                        cell
                    )
            )
            {
                return false;
            }


            //
            // Liquids are deliberately allowed in the blow lane.
            // The fans care about open space, not whether the floor is wet.
            //
            return true;
        }


        private bool IsFanMountCellAvailable(
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


            if (cell.HasSpawnBlocker())
                return false;


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


            if (
                SubterraneanSites
                    .SubterraneanSitesEPPlacement
                    .HasMeaningfulOccupant(
                        cell
                    )
            )
            {
                return false;
            }


            if (HasOpenLiquid(cell))
                return false;


            //
            // Mounting cells may already be ordinary open geometry.
            //
            if (
                SubterraneanSites
                    .SubterraneanSitesEPGeometry
                    .IsOpenGeometryCell(
                        cell
                    )
            )
            {
                return
                    cell.IsEmptyOfSolid();
            }


            //
            // Or they may be one of C4's semantic wall placeholders.
            //
            // These are the ONLY wall cells this C2 installation is allowed
            // to carve away.
            //
            if (
                SubterraneanSites
                    .SubterraneanSitesEPGeometry
                    .IsAnyPlaceholder(
                        cell
                    )
            )
            {
                return true;
            }


            return false;
        }


        private void PlaceFanBank(
            Zone Z,
            FanBankPlan plan
        )
        {
            if (
                Z == null ||
                plan == null ||
                plan.Blueprint.IsNullOrEmpty()
            )
            {
                return;
            }


            List<GameObject> fans =
                new List<GameObject>();


            //
            // Construct all four before modifying geometry.
            //
            // If the custom blueprint is unavailable, fail cleanly rather
            // than leaving a partial bank.
            //
            for (
                int x = plan.MinX;
                x <= plan.MaxX;
                x++
            )
            {
                GameObject fan =
                    GameObjectFactory
                        .Factory
                        .CreateObject(
                            plan.Blueprint
                        );


                if (fan == null)
                {
                    foreach (
                        GameObject created
                        in fans
                    )
                    {
                        if (created != null)
                            created.Destroy();
                    }


                    return;
                }


                fans.Add(
                    fan
                );
            }


            int fanIndex =
                0;


            for (
                int x = plan.MinX;
                x <= plan.MaxX;
                x++
            )
            {
                Cell cell =
                    Z.GetCell(
                        x,
                        plan.FanY
                    );


                if (cell == null)
                    continue;


                //
                // Blood machinery may cut only its mounting recess.
                // The intervening blow lane was required to already be open.
                //
                cell.ClearWalls();


                GameObject fan =
                    fans[
                        fanIndex++
                    ];
                
                //
                // Tune the vanilla fan to the actual distance between
                // this bank and its madpole tank.
                //
                // Close banks use weaker, shorter pushes.
                // Distant banks use progressively stronger/longer pushes.
                //
                // The range cap is deliberately tight so the intended
                // destination is the blood tank rather than the far side
                // of the room.
                //
                Fan fanPart =
                    fan.GetPart<
                        Fan
                    >();


                if (fanPart != null)
                {
                    fanPart.BlowStrength =
                        55 +
                        plan.Gap * 10;

                    fanPart.BlowRange =
                        plan.Gap + 1;
                }


                cell.AddObject(
                    fan
                );


                //
                // Fan supplies TurnTick behavior.
                //
                fan.MakeActive();
            }


            //
            // Reserve the entire functional corridor so Category 5 cannot
            // later park solid furniture between the fan bank and the tank.
            //
            SubterraneanSites
                .SubterraneanSitesEPReservations
                .ClaimRectangle(
                    Z,
                    plan.MinX,
                    plan.ReserveMinY,
                    plan.MaxX,
                    plan.ReserveMaxY
                );
        }

        private void PlaceSmallPoolGrabber(
            Zone Z,
            System.Random rng,
            int poolX1,
            int poolY1,
            int poolX2,
            int poolY2
        )
        {
            if (
                Z == null ||
                rng == null
            )
            {
                return;
            }


            int x;
            int y;
            string direction;


            //
            // Pick one side of the small tank.
            //
            // The grabber itself occupies one of the two non-corner
            // rim cells and points ALONG the rim at the other one.
            //
            // That means the victim is restrained on the edge of the
            // tank rather than already standing in the blood.
            //
            switch (
                rng.Next(
                    4
                )
            )
            {
                case 0:
                    //
                    // North rim.
                    //
                    if (rng.Next(2) == 0)
                    {
                        x = poolX1 + 1;
                        y = poolY1;
                        direction = "E";
                    }
                    else
                    {
                        x = poolX2 - 1;
                        y = poolY1;
                        direction = "W";
                    }

                    break;


                case 1:
                    //
                    // South rim.
                    //
                    if (rng.Next(2) == 0)
                    {
                        x = poolX1 + 1;
                        y = poolY2;
                        direction = "E";
                    }
                    else
                    {
                        x = poolX2 - 1;
                        y = poolY2;
                        direction = "W";
                    }

                    break;


                case 2:
                    //
                    // West rim.
                    //
                    if (rng.Next(2) == 0)
                    {
                        x = poolX1;
                        y = poolY1 + 1;
                        direction = "S";
                    }
                    else
                    {
                        x = poolX1;
                        y = poolY2 - 1;
                        direction = "N";
                    }

                    break;


                default:
                    //
                    // East rim.
                    //
                    if (rng.Next(2) == 0)
                    {
                        x = poolX2;
                        y = poolY1 + 1;
                        direction = "S";
                    }
                    else
                    {
                        x = poolX2;
                        y = poolY2 - 1;
                        direction = "N";
                    }

                    break;
            }


            Cell cell =
                Z.GetCell(
                    x,
                    y
                );


            if (cell == null)
                return;


            GameObject arm =
                GameObjectFactory
                    .Factory
                    .CreateObject(
                        GrabberArmBlueprint
                    );


            if (arm == null)
                return;


            GrabberArm grabber =
                arm.GetPart<
                    GrabberArm
                >();


            if (grabber == null)
            {
                arm.Destroy();

                return;
            }


            grabber.Direction =
                direction;


            cell.AddObject(
                arm
            );


            arm.MakeActive();
        }


        

    
        private bool FootprintIsAvailable(
            Zone Z,
            List<Location2D> anchors,
            int x1,
            int y1,
            int x2,
            int y2
        )
        {
            if (Z == null)
                return false;


            //
            // Fast reservation test first.
            //
            if (
                !SubterraneanSites
                    .SubterraneanSitesEPReservations
                    .RectangleIsAvailable(
                        Z,
                        x1,
                        y1,
                        x2,
                        y2
                    )
            )
            {
                return false;
            }


            for (
                int x = x1;
                x <= x2;
                x++
            )
            {
                for (
                    int y = y1;
                    y <= y2;
                    y++
                )
                {
                    Cell cell =
                        Z.GetCell(
                            x,
                            y
                        );


                    if (cell == null)
                        return false;


                    //
                    // C2 must work against whichever Category-4 layout won
                    // the shuffle. Do not carve through its walls.
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


                    if (!cell.IsEmptyOfSolid())
                        return false;


                    if (cell.HasSpawnBlocker())
                        return false;


                    //
                    // Blood tanks are exclusive liquid installations.
                    // Do not stamp them across an earlier C1 pool/spill.
                    //
                    if (HasOpenLiquid(cell))
                        return false;


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
                }
            }


            return true;
        }


        private bool HasOpenLiquid(
            Cell cell
        )
        {
            if (cell == null)
                return false;


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
                    return true;
                }
            }


            return false;
        }


        private void PlacePool(
            Zone Z,
            System.Random rng,
            int x1,
            int y1,
            int interiorWidth,
            int interiorHeight,
            int minMadpoles,
            int maxMadpoles
        )
        {
            int x2 =
                x1 +
                interiorWidth +
                1;

            int y2 =
                y1 +
                interiorHeight +
                1;


            //
            // Once accepted, C2 owns the entire installation:
            // black rim + liquid + resident hazard creatures.
            //
            SubterraneanSites
                .SubterraneanSitesEPReservations
                .ClaimRectangle(
                    Z,
                    x1,
                    y1,
                    x2,
                    y2
                );


            List<Cell> poolCells =
                new List<Cell>();


            for (
                int x = x1;
                x <= x2;
                x++
            )
            {
                for (
                    int y = y1;
                    y <= y2;
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


                    bool border =
                        x == x1 ||
                        x == x2 ||
                        y == y1 ||
                        y == y2;


                    if (border)
                    {
                        //
                        // A deliberate constructed rim rather than a natural
                        // irregular puddle.
                        //
                        if (
                            !cell.HasObjectWithBlueprint(
                                BorderBlueprint
                            )
                        )
                        {
                            cell.AddObject(
                                BorderBlueprint
                            );
                        }


                        continue;
                    }

                    //
                    // Interior is primarily blood, with a small amount of
                    // water mixed in so aquatic madpoles can potentially
                    // move normally inside the constructed tank.
                    //
                    cell.AddObject(
                        BloodBlueprint
                    );


                   GameObject liquidObject =
                        cell.GetOpenLiquidVolume();


                    LiquidVolume liquid =
                        liquidObject != null
                            ? liquidObject.LiquidVolume
                            : null;


                    if (liquid != null)
                    {
                        //
                        // Aquatic creatures require swim-depth liquid to move normally.
                        //
                        // Guarantee a slightly-deep pool while retaining whatever amount of
                        // blood the BloodPool blueprint originally supplied.
                        //
                        const int TargetPoolVolume = 2200;


                        int waterAmount =
                            Math.Max(
                                0,
                                TargetPoolVolume -
                                liquid.Volume
                            );


                        if (waterAmount > 0)
                        {
                            liquid.MixWith(
                                new LiquidVolume(
                                    "water",
                                    waterAmount
                                )
                            );
                        }


                        liquid.CheckImage();
                    }


                    poolCells.Add(
                        cell
                    );
                }
            }


            SpawnMadpoles(
                Z,
                poolCells,
                rng,
                minMadpoles,
                maxMadpoles
            );
        }


        private void SpawnMadpoles(
            Zone Z,
            List<Cell> poolCells,
            System.Random rng,
            int minimum,
            int maximum
        )
        {
            if (
                Z == null ||
                poolCells == null ||
                poolCells.Count == 0 ||
                rng == null
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


            if (
                desired >
                poolCells.Count
            )
            {
                desired =
                    poolCells.Count;
            }


            //
            // Work from a copy so every initial madpole receives its own cell.
            //
            List<Cell> available =
                new List<Cell>(
                    poolCells
                );


            for (
                int i = 0;
                i < desired &&
                available.Count > 0;
                i++
            )
            {
                int index =
                    rng.Next(
                        available.Count
                    );


                Cell destination =
                    available[index];


                available.RemoveAt(
                    index
                );


                GameObject madpole =
                    GameObject.Create(
                        MadpoleBlueprint
                    );


                if (madpole == null)
                    continue;


                //
                // Keep the real generated Blood dimensional identity.
                //
                SubterraneanSites
                    .SubterraneanSitesDimensionEngine
                    .ApplyDecorationDimensionIdentity(
                        madpole,
                        DecorationThemeKey
                    );


                //
                // Blood C2 special package:
                // only Regeneration, not the complete random EP-denizen package.
                //
                GiveMadpoleRegeneration(
                    madpole
                );


                //
                // Ordinary EP denizens use Playerhater as their runtime allegiance.
                //
                // Do the same for Blood madpoles so the pocket's denizens recognize them
                // as fellow inhabitants rather than attacking them as an unrelated faction.
                //
                // ApplyDecorationDimensionIdentity() has already recorded the actual
                // extradimensional source faction as metadata before we replace runtime
                // allegiance here.
                //
                if (madpole.Brain != null)
                {
                    madpole.Brain.Allegiance.Clear();

                    madpole.Brain.Allegiance.Add(
                        "Playerhater",
                        100
                    );

                    madpole.Brain.Allegiance.Hostile =
                        true;

                    madpole.Brain.Allegiance.Calm =
                        false;
                }


                destination.AddObject(
                    madpole
                );


                madpole.MakeActive();
            }
        }


        private void ClampSettings()
        {
            if (MinSmallPools < 0)
                MinSmallPools = 0;

            if (MaxSmallPools < MinSmallPools)
                MaxSmallPools = MinSmallPools;


            if (MinLargePools < 0)
                MinLargePools = 0;

            if (MaxLargePools < MinLargePools)
                MaxLargePools = MinLargePools;


            if (SmallPoolWidth < 1)
                SmallPoolWidth = 1;

            if (SmallPoolHeight < 1)
                SmallPoolHeight = 1;


            if (LargePoolWidth < 1)
                LargePoolWidth = 1;

            if (LargePoolHeight < 1)
                LargePoolHeight = 1;


            if (MinSmallMadpoles < 0)
                MinSmallMadpoles = 0;

            if (MaxSmallMadpoles < MinSmallMadpoles)
                MaxSmallMadpoles = MinSmallMadpoles;


            if (MinLargeMadpoles < 0)
                MinLargeMadpoles = 0;

            if (MaxLargeMadpoles < MinLargeMadpoles)
                MaxLargeMadpoles = MinLargeMadpoles;


            if (TransitionExclusionRadius < 0)
                TransitionExclusionRadius = 0;

            if (OuterBorderExclusion < 0)
                OuterBorderExclusion = 0;

            if (PlacementAttemptsPerPool < 1)
                PlacementAttemptsPerPool = 1;

            if (MinFanGap < 1)
                MinFanGap = 1;

            if (MaxFanGap < MinFanGap)
                MaxFanGap = MinFanGap;
        }
    }
    /// <summary>
    /// Blood Category-1 physical machinery.
    ///
    /// Finds long cardinal routes through whatever Category-4 open geometry
    /// actually won the shuffle and installs vanilla ConveyorPads along them.
    ///
    /// Does not carve geometry.
    /// Does not assume Blood supplied Category 4.
    /// Does not require electrical power infrastructure.
    ///
    /// The cell immediately beyond each conveyor is reserved for the future
    /// dismemberment statue.
    /// </summary>
    public class SubterraneanSitesBloodConveyors :
        ZoneBuilderSandbox
    {
        public int EntranceOnly = 0;

        public int MinSystems = 2;
        public int MaxSystems = 4;

        public int PreferredMinLength = 15;
        public int MinimumLength = 8;
        public int MaximumLength = 30;

        public int TransitionExclusionRadius = 5;

        public int RouteAttemptsPerSystem = 300;

        private static readonly string[] DismembermentStatueBlueprints =
        {
            "SubterraneanSitesBloodDismembermentStatue",
            "SubterraneanSitesBloodDismembermentStatueFlipped",
            "SubterraneanSitesBloodDismembermentStatueImplanted"
        };

        //
        // Vanilla MakeSeveredBodyParts() will generate authentic severed
        // anatomy from creatures descended from these blueprint bases.
        //
        private static readonly string[] WarningLimbBases =
        {
            "Goatfolk",
            "Naphtaali",
            "Plant",
            "BaseFarmer",
            "BaseFarmer",
            "BaseFarmer",
            "BaseFarmer",
            "BaseFarmer",
            "Baboon",
            "BaseBat",
            "BaseCannibal",
            "BaseSpider",
            "BaseCat",
            "BaseReptile",
            "BaseDog",
            "BaseRobot",
            "Chromeling",
            "Chromeling",
            "Chromeling",
            "Chromeling",
            "BaseInsect",
            "Giant Beetle",
            "BaseBird",
            "BaseFish",
            "BaseGoat",
            "BaseTortoise",
            "Snapjaw",
            "Snapjaw",
            "Snapjaw",
            "Snapjaw",
            "Snapjaw",
            "BaseIssachari",
            "BaseIssachari",
            "BaseIssachari",
            "BaseIssachari",
            "BaseIssachari",
            "BaseCrab",
            "Mechanimist",
            "Templar",
            "Mechanimist",
            "Templar",
            "Mechanimist",
            "Templar",
            "Mechanimist",
            "Templar",
            "Mechanimist",
            "Templar",
            "Mechanimist",
            "Templar"
        };


        //
        // Deliberately restrict the scenery to actual limbs.
        // No heads/faces: Blood's machinery harvests pieces without decapitating.
        //
        private static readonly string[] WarningLimbTypes =
        {
            "Arm",
            "Arm",
            "Hand",
            "Hand",
            "Leg",
            "Leg",
            "Foot",
            "Foot",
            "Face"
        };


        private sealed class ConveyorPlan
        {
            public List<Cell> Path;

            public Cell DriveCell;

            public Cell HazardCell;
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
                    "SubterraneanSites:BloodConveyors:" +
                    Z.ZoneID +
                    ":" +
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


            int desired =
                rng.Next(
                    MinSystems,
                    MaxSystems + 1
                );


            for (
                int i = 0;
                i < desired;
                i++
            )
            {
                ConveyorPlan plan =
                    FindPlan(
                        Z,
                        anchors,
                        rng
                    );

                if (plan == null)
                    continue;

                PlacePlan(
                    Z,
                    plan,
                    rng
                );
            }


            return true;
        }


        private ConveyorPlan FindPlan(
            Zone Z,
            List<Location2D> anchors,
            System.Random rng
        )
        {
            List<Cell> candidates =
                CollectCandidates(
                    Z,
                    anchors
                );

            if (
                candidates == null ||
                candidates.Count < 2
            )
            {
                return null;
            }


            ConveyorPlan bestFallback =
                null;

            int bestFallbackLength =
                0;


            for (
                int attempt = 0;
                attempt < RouteAttemptsPerSystem;
                attempt++
            )
            {
                Cell start =
                    candidates[
                        rng.Next(
                            candidates.Count
                        )
                    ];

                Cell end =
                    candidates[
                        rng.Next(
                            candidates.Count
                        )
                    ];


                if (
                    start == null ||
                    end == null ||
                    start == end
                )
                {
                    continue;
                }


                //
                // Don't waste pathfinder calls on obviously tiny routes.
                //
                int manhattan =
                    Math.Abs(
                        start.X -
                        end.X
                    ) +
                    Math.Abs(
                        start.Y -
                        end.Y
                    );

                if (
                    manhattan <
                    MinimumLength - 1
                )
                {
                    continue;
                }


                FindPath findPath =
                    new FindPath(
                        Z,
                        start.X,
                        start.Y,
                        Z,
                        end.X,
                        end.Y,
                        PathGlobal: false,
                        PathUnlimited: true,
                        null,
                        AddNoise: false,
                        CardinalOnly: true
                    );


                if (
                    !findPath.Usable ||
                    findPath.Steps == null
                )
                {
                    continue;
                }


                List<Cell> path =
                    new List<Cell>(
                        findPath.Steps
                    );


                if (
                    path.Count <
                        MinimumLength ||
                    path.Count >
                        MaximumLength
                )
                {
                    continue;
                }


                if (
                    !PathIsUsable(
                        Z,
                        path,
                        anchors
                    )
                )
                {
                    continue;
                }


                Cell hazardCell;

                if (
                    !TryGetHazardCell(
                        Z,
                        path,
                        anchors,
                        out hazardCell
                    )
                )
                {
                    continue;
                }


                Cell driveCell;

                if (
                    !TryGetDriveCell(
                        Z,
                        path,
                        anchors,
                        out driveCell
                    )
                )
                {
                    continue;
                }


                ConveyorPlan plan =
                    new ConveyorPlan
                    {
                        Path =
                            path,

                        DriveCell =
                            driveCell,

                        HazardCell =
                            hazardCell
                    };


                if (
                    path.Count >=
                    PreferredMinLength
                )
                {
                    return plan;
                }


                if (
                    path.Count >
                    bestFallbackLength
                )
                {
                    bestFallback =
                        plan;

                    bestFallbackLength =
                        path.Count;
                }
            }


            return bestFallback;
        }


        private List<Cell> CollectCandidates(
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
                    IsAvailableConveyorCell(
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


        private bool PathIsUsable(
            Zone Z,
            List<Cell> path,
            List<Location2D> anchors
        )
        {
            if (
                Z == null ||
                path == null ||
                path.Count < MinimumLength
            )
            {
                return false;
            }


            foreach (
                Cell cell
                in path
            )
            {
                if (
                    !IsAvailableConveyorCell(
                        Z,
                        cell,
                        anchors
                    )
                )
                {
                    return false;
                }
            }


            return true;
        }


        private bool IsAvailableConveyorCell(
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
            // Entrance preview stays entirely inside the scar and outside
            // the central hole footprint.
            //
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
                //
                // C1 operates only on the abstract open geometry supplied by
                // whichever Category-4 theme won.
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
            // C1 runs before C2/C5, but other earlier C1 content or shared
            // infrastructure may already own a cell.
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
                cell.HasObjectWithPart(
                    "ConveyorPad"
                ) ||
                cell.HasObjectWithPart(
                    "ConveyorDriver"
                )
            )
            {
                return false;
            }


            return true;
        }


        private bool TryGetHazardCell(
            Zone Z,
            List<Cell> path,
            List<Location2D> anchors,
            out Cell hazardCell
        )
        {
            hazardCell =
                null;


            if (
                Z == null ||
                path == null ||
                path.Count < 2
            )
            {
                return false;
            }


            Cell previous =
                path[
                    path.Count - 2
                ];

            Cell last =
                path[
                    path.Count - 1
                ];


            string direction =
                previous.GetDirectionFromCell(
                    last
                );


            if (direction.IsNullOrEmpty())
                return false;


            //
            // Continue one tile beyond the final conveyor.
            //
            // This is where the dismemberment statue will eventually sit.
            //
            Cell candidate =
                last.GetCellFromDirection(
                    direction
                );


            if (
                !IsAvailableConveyorCell(
                    Z,
                    candidate,
                    anchors
                )
            )
            {
                return false;
            }


            if (
                path.Contains(
                    candidate
                )
            )
            {
                return false;
            }

            if (
                !HasStatueFootprint(
                    Z,
                    candidate,
                    anchors
                )
            )
            {
                return false;
            }


            hazardCell =
                candidate;

            return true;
        }

        private bool HasStatueFootprint(
            Zone Z,
            Cell center,
            List<Location2D> anchors
        )
        {
            if (
                Z == null ||
                center == null
            )
            {
                return false;
            }


            //
            // Require a complete 3x3 functional apron around the future
            // dismemberment statue.
            //
            // One of these cells will already be the final conveyor pad.
            // The remaining cells give us room for gore / severed limbs and
            // prevent the trap from appearing jammed into a narrow corridor.
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
                    Cell cell =
                        Z.GetCell(
                            center.X + dx,
                            center.Y + dy
                        );


                    if (cell == null)
                        return false;


                    if (
                        !IsAvailableConveyorCell(
                            Z,
                            cell,
                            anchors
                        )
                    )
                    {
                        return false;
                    }
                }
            }


            return true;
        }


        private bool TryGetDriveCell(
            Zone Z,
            List<Cell> path,
            List<Location2D> anchors,
            out Cell driveCell
        )
        {
            driveCell =
                null;


            if (
                Z == null ||
                path == null ||
                path.Count < 2
            )
            {
                return false;
            }


            Cell first =
                path[0];

            Cell second =
                path[1];


            string forward =
                first.GetDirectionFromCell(
                    second
                );

            string behind =
                GetOppositeDirection(
                    forward
                );


            //
            // Ideal position: directly behind the first belt tile.
            //
            if (!behind.IsNullOrEmpty())
            {
                Cell candidate =
                    first.GetCellFromDirection(
                        behind
                    );


                if (
                    candidate != null &&
                    !path.Contains(
                        candidate
                    ) &&
                    IsAvailableConveyorCell(
                        Z,
                        candidate,
                        anchors
                    )
                )
                {
                    driveCell =
                        candidate;

                    return true;
                }
            }


            //
            // Fallback: any clean cardinal neighbor.
            //
            string[] directions =
            {
                "N",
                "E",
                "S",
                "W"
            };


            foreach (
                string direction
                in directions
            )
            {
                Cell candidate =
                    first.GetCellFromDirection(
                        direction
                    );


                if (
                    candidate == null ||
                    path.Contains(
                        candidate
                    )
                )
                {
                    continue;
                }


                if (
                    IsAvailableConveyorCell(
                        Z,
                        candidate,
                        anchors
                    )
                )
                {
                    driveCell =
                        candidate;

                    return true;
                }
            }


            return false;
        }


        private string GetOppositeDirection(
            string direction
        )
        {
            switch (direction)
            {
                case "N":
                    return "S";

                case "S":
                    return "N";

                case "E":
                    return "W";

                case "W":
                    return "E";

                default:
                    return "";
            }
        }


        private void PlacePlan(
            Zone Z,
            ConveyorPlan plan,
            System.Random rng
        )
        {
            if (
                Z == null ||
                plan == null ||
                plan.Path == null ||
                plan.Path.Count < 2
            )
            {
                return;
            }


            string lastDirection =
                "N";


            for (
                int i = 0;
                i < plan.Path.Count;
                i++
            )
            {
                Cell cell =
                    plan.Path[i];


                if (cell == null)
                    continue;


                GameObject conveyor =
                    GameObjectFactory
                        .Factory
                        .CreateObject(
                            "SubterraneanSitesBloodConveyorPad"
                        );


                if (conveyor == null)
                    continue;


                ConveyorPad pad =
                    conveyor.GetPart<
                        ConveyorPad
                    >();


                if (pad != null)
                {
                    if (
                        i <
                        plan.Path.Count - 1
                    )
                    {
                        string direction =
                            cell.GetDirectionFromCell(
                                plan.Path[
                                    i + 1
                                ]
                            );


                        if (
                            !direction.IsNullOrEmpty()
                        )
                        {
                            pad.Direction =
                                direction;

                            pad.Connections =
                                direction;

                            lastDirection =
                                direction;
                        }
                    }
                    else
                    {
                        //
                        // The last belt continues toward the reserved hazard cell.
                        //
                        pad.Direction =
                            lastDirection;

                        pad.Connections =
                            lastDirection;
                    }
                }


                cell.AddObject(
                    conveyor
                );

                conveyor.MakeActive();


                SubterraneanSites
                    .SubterraneanSitesEPReservations
                    .ClaimCell(
                        Z,
                        cell
                    );
            }


            //
            // Vanilla ConveyorDrive is already self-operating machinery.
            // No generator or power-grid setup is required.
            //
            if (plan.DriveCell != null)
            {
                plan.DriveCell.AddObject(
                    "ConveyorDrive"
                );


                SubterraneanSites
                    .SubterraneanSitesEPReservations
                    .ClaimCell(
                        Z,
                        plan.DriveCell
                    );
            }


            PlaceHazardEndpoint(
                Z,
                plan.HazardCell,
                rng
            );
        }

        private void PlaceHazardEndpoint(
            Zone Z,
            Cell hazardCell,
            System.Random rng
        )
        {
            if (
                Z == null ||
                hazardCell == null ||
                rng == null
            )
            {
                return;
            }


            //
            // The endpoint is C1's exclusive functional footprint in either case.
            //
            SubterraneanSites
                .SubterraneanSitesEPReservations
                .ClaimCell(
                    Z,
                    hazardCell
                );


            if (EntranceOnly != 0)
            {
                //
                // IMPORTANT BLOOD-SCAR RULE:
                //
                // Never put the active dismemberment statue on the origin scar.
                // The conveyor and severed remains foreshadow what waits below,
                // but the player is not unexpectedly mutilated before they have
                // had a fair opportunity to understand and attune to the pocket.
                //
                PlaceWarningLimbs(
                    Z,
                    hazardCell,
                    rng,
                    5,
                    8,
                    includeCenter: true
                );

                return;
            }

            PlaceBloodApron(
                Z,
                hazardCell,
                rng
            );


            string statueBlueprint =
                DismembermentStatueBlueprints[
                    rng.Next(
                        DismembermentStatueBlueprints.Length
                    )
                ];


            GameObject statue =
                GameObjectFactory.Factory
                    .CreateObject(
                        statueBlueprint
                    );


            if (statue != null)
            {
                hazardCell.AddObject(
                    statue
                );


                //
                // Give the statue the visibly blood-defiled treatment.
                //
                statue.ForceApplyEffect(
                    new LiquidStained(
                        "blood",
                        4,
                        9999
                    )
                );


                //
                // This object has a TurnTick part, so make absolutely sure it
                // participates in active-zone processing.
                //
                statue.MakeActive();
            }


            //
            // Old remains advertise exactly what this machinery does.
            //
            PlaceWarningLimbs(
                Z,
                hazardCell,
                rng,
                8,
                14,
                includeCenter: false
            );
        }

        private void PlaceBloodApron(
            Zone Z,
            Cell center,
            System.Random rng
        )
        {
            if (
                Z == null ||
                center == null ||
                rng == null
            )
            {
                return;
            }


            //
            // Nearly complete 3x3 pool.
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
                    //
                    // Always keep the center bloody.
                    // Occasionally omit another tile so it isn't stamped
                    // as a perfect square every time.
                    //
                    if (
                        (dx != 0 || dy != 0) &&
                        rng.Next(100) < 12
                    )
                    {
                        continue;
                    }


                    PlaceBlood(
                        Z.GetCell(
                            center.X + dx,
                            center.Y + dy
                        )
                    );
                }
            }


            //
            // Add 1-3 irregular spill cells just outside the 3x3 core.
            //
            int fringe =
                rng.Next(
                    1,
                    4
                );


            for (
                int i = 0;
                i < fringe;
                i++
            )
            {
                int dx;
                int dy;


                do
                {
                    dx =
                        rng.Next(
                            -2,
                            3
                        );

                    dy =
                        rng.Next(
                            -2,
                            3
                        );
                }
                while (
                    Math.Max(
                        Math.Abs(dx),
                        Math.Abs(dy)
                    ) != 2
                );


                PlaceBlood(
                    Z.GetCell(
                        center.X + dx,
                        center.Y + dy
                    )
                );
            }
        }

        private void PlaceBlood(
            Cell cell
        )
        {
            if (
                cell == null ||
                !cell.IsEmptyOfSolid()
            )
            {
                return;
            }


            //
            // Blood is a permissive liquid.
            //
            // If another open liquid already exists here, mix Blood into it.
            // Otherwise create Blood's normal pool object.
            //
            SubterraneanSites
                .SubterraneanSitesEPLiquids
                .AddOrMixLiquid(
                    cell,
                    "blood",
                    "BloodPool"
                );
        }


        private void PlaceWarningLimbs(
            Zone Z,
            Cell center,
            System.Random rng,
            int minimum,
            int maximum,
            bool includeCenter
        )
        {
            if (
                Z == null ||
                center == null ||
                rng == null
            )
            {
                return;
            }


            List<Cell> candidates =
                new List<Cell>();


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
                        dy == 0 &&
                        !includeCenter
                    )
                    {
                        continue;
                    }


                    Cell cell =
                        Z.GetCell(
                            center.X + dx,
                            center.Y + dy
                        );


                    if (
                        cell == null ||
                        !cell.IsEmptyOfSolid()
                    )
                    {
                        continue;
                    }


                    //
                    // Don't put the warning remains directly on the machinery.
                    // Otherwise the live conveyor can immediately carry the
                    // decorative evidence away.
                    //
                    if (
                        cell.HasObjectWithPart(
                            "ConveyorPad"
                        ) ||
                        cell.HasObjectWithPart(
                            "ConveyorDriver"
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


            if (candidates.Count == 0)
                return;


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
                    candidates[
                        rng.Next(
                            candidates.Count
                        )
                    ];


                PlaceAuthenticLimb(
                    cell,
                    rng
                );

                // cell.AddObject(
                //     blueprint
                // );
            }
        }

        private void PlaceAuthenticLimb(
            Cell destination,
            System.Random rng
        )
        {
            if (
                destination == null ||
                rng == null
            )
            {
                return;
            }


            string creatureBase =
                WarningLimbBases[
                    rng.Next(
                        WarningLimbBases.Length
                    )
                ];


            string partType =
                WarningLimbTypes[
                    rng.Next(
                        WarningLimbTypes.Length
                    )
                ];


            List<GameObject> generated =
                new List<GameObject>();


            BodyPart.MakeSeveredBodyParts(
                1,

                // Let vanilla choose an actual creature blueprint descended
                // from the requested base.
                Blueprint: null,

                // Explicitly request a non-head anatomical part.
                Type: partType,

                Tag: null,

                Base: creatureBase,

                Filter: null,

                // We want the objects returned to us, not placed into inventory.
                PutIn: null,

                Return: generated
            );


            foreach (
                GameObject limb
                in generated
            )
            {
                if (limb == null)
                    continue;

                destination.AddObject(
                    limb
                );
            }
        }


        private void ClampSettings()
        {
            if (MinSystems < 0)
                MinSystems = 0;

            if (MaxSystems < MinSystems)
                MaxSystems = MinSystems;

            if (MinimumLength < 3)
                MinimumLength = 3;

            if (
                PreferredMinLength <
                MinimumLength
            )
            {
                PreferredMinLength =
                    MinimumLength;
            }

            if (
                MaximumLength <
                PreferredMinLength
            )
            {
                MaximumLength =
                    PreferredMinLength;
            }

            if (TransitionExclusionRadius < 0)
                TransitionExclusionRadius = 0;

            if (RouteAttemptsPerSystem < 1)
                RouteAttemptsPerSystem = 1;
        }
    }


    /// <summary>
    /// Blood Category-4 floor adapter.
    ///
    /// Blood owns the floor recipe.
    /// The shared EP floor system owns substrate application.
    /// </summary>
    public class SubterraneanSitesBloodFloor :
        ZoneBuilderSandbox
    {
        public int EntranceOnly = 0;


        public bool BuildZone(
            Zone Z
        )
        {
            if (Z == null)
                return true;

            return
                SubterraneanSites
                    .SubterraneanSitesEPFloorSystem
                    .Apply(
                        Z,
                        SubterraneanSites
                            .SubterraneanSitesEPBloodTheme
                            .CreateFloorSpec(),
                        EntranceOnly != 0
                    );
        }
    }



    /// <summary>
    /// Blood Category-3 materialization.
    ///
    /// Buried structural mass and the absolute outer shell are ordinary
    /// fulcrete.
    ///
    /// Exposed room-facing boundaries become MachineWallHotTubing.
    ///
    /// All of the machine wall's native behavior remains intact, including
    /// its red glow, light source and destruction behavior.
    /// </summary>
    public class SubterraneanSitesBloodMaterials :
        ZoneBuilderSandbox
    {
        public string BulkWallBlueprint =
            "Fulcrete";

        public string InnerWallBlueprint =
            "MachineWallHotTubing";

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



    /// <summary>
    /// Blood Category 4.
    ///
    /// Creates an institutional ward/procedure complex centered on one long
    /// east-west circulation spine.
    ///
    /// Rectangular treatment bays grow from the north and south sides of that
    /// spine, with intentional solid gaps between groups of bays.
    ///
    /// This is deliberately NOT conveyor-specific. Blood Category 2 must later
    /// be capable of placing conveyors into any shuffled Category-4 geometry.
    ///
    /// Vertical-transition anchors receive small landing chambers and a
    /// two-cell-wide connection back to the central spine.
    /// </summary>
    public class SubterraneanSitesBloodWardLayout :
        ZoneBuilderSandbox
    {
        public int Border = 1;

        public int SpineHalfHeight = 1;

        public int AnchorRadius = 2;
        public int MinBayWidth = 6;
        public int MaxBayWidth = 10;

        public int MinBayDepth = 5;
        public int MaxBayDepth = 10;

        public int MinBayGap = 1;
        public int MaxBayGap = 2;

        public int OppositeBayChance = 80;

        public int LargeProcedureChance = 20;

        public int DoorWidth = 2;

        //
        // Blood is organized around several staggered institutional halls
        // rather than one zone-wide central axis.
        //
        public int MinHallSegments = 2;
        public int MaxHallSegments = 3;

        //
        // Adjacent hall segments overlap slightly in X so a short vertical
        // dogleg can join them cleanly.
        //
        public int HallOverlap = 2;

        //
        // Each hall shifts a little above or below the zone center.
        //
        public int MinHallOffset = 2;
        public int MaxHallOffset = 4;


        //
        // After the ward bays are carved, look for small masses of unused
        // structural fill lying directly between two already-open spaces.
        // Cutting through some of these produces Bethesda-like secondary
        // service connections.
        //
        public int MinServicePassages = 2;
        public int MaxServicePassages = 4;

        public int MinServicePassageLength = 3;
        public int MaxServicePassageLength = 10;

        private sealed class HallSegment
        {
            public int X1;
            public int X2;
            public int Y;

            public int Y1
            {
                get
                {
                    return
                        Y - 1;
                }
            }

            public int Y2
            {
                get
                {
                    return
                        Y + 1;
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
                    "SubterraneanSites:BloodWardLayout:" +
                    Z.ZoneID
                );

            System.Random rng =
                new System.Random(
                    seed
                );


            //
            // Instead of one enormous central hallway, build several
            // overlapping horizontal ward halls at slightly different heights.
            //
            List<HallSegment> halls =
                BuildHallSegments(
                    Z,
                    rng
                );


            foreach (
                HallSegment hall
                in halls
            )
            {
                CarveRectangle(
                    Z,
                    hall.X1,
                    hall.Y1,
                    hall.X2,
                    hall.Y2
                );
            }


            //
            // Join neighboring hall sections with short vertical doglegs.
            //
            ConnectHallSegments(
                Z,
                halls,
                rng
            );


            //
            // Ward / procedure rooms grow independently from each hall segment.
            //
            foreach (
                HallSegment hall
                in halls
            )
            {
                BuildWardBays(
                    Z,
                    rng,
                    hall.X1,
                    hall.X2,
                    hall.Y1,
                    hall.Y2
                );
            }


            //
            // Some otherwise-unused structural fill becomes secondary service
            // passages between already-open rooms/halls.
            //
            AddServicePassages(
                Z,
                rng
            );


            //
            // Preserve intentional accessible landing space around all required
            // vertical transitions, then connect each transition to whichever
            // ward hall is actually nearest.
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

                    ConnectAnchorToNearestHall(
                        Z,
                        anchor,
                        halls
                    );
                }
            }


            ForceSealedBorder(
                Z
            );

            Z.ClearReachableMap();

            return true;
        }
        


        private void BuildWardBays(
            Zone Z,
            System.Random rng,
            int spineX1,
            int spineX2,
            int spineY1,
            int spineY2
        )
        {
            int x =
                spineX1 +
                2 +
                rng.Next(
                    0,
                    3
                );

            bool topNext =
                rng.Next(2) == 0;


            while (
                x +
                MinBayWidth -
                1 <=
                spineX2 -
                1
            )
            {
                bool large =
                    rng.Next(100) <
                    LargeProcedureChance;

                int width =
                    rng.Next(
                        MinBayWidth,
                        MaxBayWidth + 1
                    );

                if (large)
                    width += 2;


                int x2 =
                    Math.Min(
                        spineX2 - 1,
                        x + width - 1
                    );

                if (
                    x2 -
                    x +
                    1 <
                    MinBayWidth
                )
                {
                    break;
                }


                //
                // Primary bay for this section of the spine.
                //
                CarveBay(
                    Z,
                    rng,
                    x,
                    x2,
                    spineY1,
                    spineY2,
                    topNext,
                    large
                );


                //
                // Some sections become paired wards or large treatment suites
                // on both sides of the central gallery.
                //
                if (
                    rng.Next(100) <
                    OppositeBayChance
                )
                {
                    bool oppositeLarge =
                        large &&
                        rng.Next(2) == 0;

                    CarveBay(
                        Z,
                        rng,
                        x,
                        x2,
                        spineY1,
                        spineY2,
                        !topNext,
                        oppositeLarge
                    );
                }


                topNext =
                    !topNext;


                //
                // Leave real structural mass between adjacent ward groups.
                //
                x =
                    x2 +
                    1 +
                    rng.Next(
                        MinBayGap,
                        MaxBayGap + 1
                    );
            }
        }

        private void ConnectHallSegments(
            Zone Z,
            List<HallSegment> halls,
            System.Random rng
        )
        {
            if (
                Z == null ||
                halls == null ||
                halls.Count < 2 ||
                rng == null
            )
            {
                return;
            }


            for (
                int i = 0;
                i <
                halls.Count - 1;
                i++
            )
            {
                HallSegment left =
                    halls[i];

                HallSegment right =
                    halls[
                        i + 1
                    ];


                int overlapX1 =
                    Math.Max(
                        left.X1,
                        right.X1
                    );

                int overlapX2 =
                    Math.Min(
                        left.X2,
                        right.X2
                    );


                int connectorX;


                if (
                    overlapX2 >=
                    overlapX1
                )
                {
                    connectorX =
                        rng.Next(
                            overlapX1,
                            overlapX2 + 1
                        );
                }
                else
                {
                    //
                    // Defensive fallback. Normally HallOverlap ensures this
                    // branch is unnecessary.
                    //
                    connectorX =
                        (
                            left.X2 +
                            right.X1
                        ) /
                        2;
                }


                CarveVerticalCorridor(
                    Z,
                    connectorX,
                    left.Y,
                    right.Y,
                    2
                );
            }
        }

        private List<HallSegment> BuildHallSegments(
            Zone Z,
            System.Random rng
        )
        {
            List<HallSegment> result =
                new List<HallSegment>();

            if (
                Z == null ||
                rng == null
            )
            {
                return result;
            }


            int hallCount =
                rng.Next(
                    MinHallSegments,
                    MaxHallSegments + 1
                );


            int overallX1 =
                Border +
                1;

            int overallX2 =
                Z.Width -
                Border -
                2;

            int totalWidth =
                overallX2 -
                overallX1 +
                1;


            if (
                hallCount < 1 ||
                totalWidth < 1
            )
            {
                return result;
            }


            //
            // Keep enough vertical room for wards on either side.
            //
            int minimumY =
                Border +
                MinBayDepth +
                3 +
                SpineHalfHeight;

            int maximumY =
                Z.Height -
                Border -
                MinBayDepth -
                4 -
                SpineHalfHeight;

            int centerY =
                Z.Height /
                2;


            if (maximumY < minimumY)
            {
                minimumY =
                    Border +
                    SpineHalfHeight +
                    1;

                maximumY =
                    Z.Height -
                    Border -
                    SpineHalfHeight -
                    2;
            }


            int firstSide =
                rng.Next(2) == 0
                    ? -1
                    : 1;


            for (
                int i = 0;
                i < hallCount;
                i++
            )
            {
                int rawX1 =
                    overallX1 +
                    (
                        totalWidth *
                        i /
                        hallCount
                    );

                int rawX2 =
                    overallX1 +
                    (
                        totalWidth *
                        (
                            i +
                            1
                        ) /
                        hallCount
                    ) -
                    1;


                //
                // Give neighboring segments a small shared X range.
                // Their Y positions differ, so the overlap becomes a natural
                // location for a connecting dogleg rather than a continuous hall.
                //
                int x1 =
                    rawX1;

                int x2 =
                    rawX2;


                if (i > 0)
                {
                    x1 -=
                        HallOverlap;
                }

                if (
                    i <
                    hallCount -
                    1
                )
                {
                    x2 +=
                        HallOverlap;
                }


                x1 =
                    ClampInt(
                        x1,
                        overallX1,
                        overallX2
                    );

                x2 =
                    ClampInt(
                        x2,
                        overallX1,
                        overallX2
                    );


                int side =
                    i % 2 == 0
                        ? firstSide
                        : -firstSide;

                int offset =
                    rng.Next(
                        MinHallOffset,
                        MaxHallOffset + 1
                    );


                int y =
                    centerY +
                    (
                        side *
                        offset
                    );


                y =
                    ClampInt(
                        y,
                        minimumY,
                        maximumY
                    );


                result.Add(
                    new HallSegment
                    {
                        X1 =
                            x1,

                        X2 =
                            x2,

                        Y =
                            y
                    }
                );
            }


            return result;
        }



        private void CarveBay(
            Zone Z,
            System.Random rng,
            int x1,
            int x2,
            int spineY1,
            int spineY2,
            bool top,
            bool large
        )
        {
            int depth =
                rng.Next(
                    MinBayDepth,
                    MaxBayDepth + 1
                );

            if (large)
            {
                depth +=
                    rng.Next(
                        1,
                        3
                    );
            }


            int roomY1;
            int roomY2;
            int doorY;


            if (top)
            {
                //
                // Leave one solid row between the room and the spine.
                // Only the doorway cuts through that row.
                //
                roomY2 =
                    spineY1 -
                    2;

                roomY1 =
                    Math.Max(
                        Border + 1,
                        roomY2 -
                        depth +
                        1
                    );

                doorY =
                    spineY1 -
                    1;
            }
            else
            {
                roomY1 =
                    spineY2 +
                    2;

                roomY2 =
                    Math.Min(
                        Z.Height -
                            Border -
                            2,
                        roomY1 +
                            depth -
                            1
                    );

                doorY =
                    spineY2 +
                    1;
            }


            if (
                roomY2 <
                roomY1
            )
            {
                return;
            }


            CarveRectangle(
                Z,
                x1,
                roomY1,
                x2,
                roomY2
            );


            //
            // A broad clinical doorway rather than the single-cell doors used
            // by many ordinary dungeon room layouts.
            //
            int roomWidth =
                x2 -
                x1 +
                1;

            int spareWidth =
                roomWidth -
                DoorWidth -
                2;

            int doorX1 =
                x1 +
                1;

            if (spareWidth > 0)
            {
                doorX1 +=
                    rng.Next(
                        spareWidth + 1
                    );
            }

            int doorX2 =
                Math.Min(
                    x2 - 1,
                    doorX1 +
                        DoorWidth -
                        1
                );


            CarveRectangle(
                Z,
                doorX1,
                doorY,
                doorX2,
                doorY
            );
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

        private void ConnectAnchorToNearestHall(
            Zone Z,
            Location2D anchor,
            List<HallSegment> halls
        )
        {
            if (
                Z == null ||
                anchor == null ||
                halls == null ||
                halls.Count == 0
            )
            {
                return;
            }


            HallSegment bestHall =
                null;

            int bestTargetX =
                anchor.X;

            int bestDistance =
                int.MaxValue;


            foreach (
                HallSegment hall
                in halls
            )
            {
                if (hall == null)
                    continue;


                int targetX =
                    ClampInt(
                        anchor.X,
                        hall.X1,
                        hall.X2
                    );


                int distance =
                    Math.Abs(
                        anchor.X -
                        targetX
                    ) +
                    Math.Abs(
                        anchor.Y -
                        hall.Y
                    );


                if (
                    distance <
                    bestDistance
                )
                {
                    bestDistance =
                        distance;

                    bestHall =
                        hall;

                    bestTargetX =
                        targetX;
                }
            }


            if (bestHall == null)
                return;


            //
            // Two-cell-wide service connection to the nearest institutional hall.
            //
            CarveHorizontalCorridor(
                Z,
                anchor.X,
                bestTargetX,
                anchor.Y,
                2
            );

            CarveVerticalCorridor(
                Z,
                bestTargetX,
                anchor.Y,
                bestHall.Y,
                2
            );
        }

        private void AddServicePassages(
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


            int desired =
                rng.Next(
                    MinServicePassages,
                    MaxServicePassages + 1
                );

            int placed =
                0;


            //
            // Don't search forever if this particular generated layout simply
            // doesn't have suitable chunks of fill between open regions.
            //
            for (
                int attempt = 0;
                attempt < 300 &&
                placed < desired;
                attempt++
            )
            {
                int x =
                    rng.Next(
                        Border + 2,
                        Z.Width -
                        Border -
                        2
                    );

                int y =
                    rng.Next(
                        Border + 2,
                        Z.Height -
                        Border -
                        2
                    );


                Cell start =
                    Z.GetCell(
                        x,
                        y
                    );


                if (
                    !IsOpenLayoutCell(
                        start
                    )
                )
                {
                    continue;
                }


                bool horizontal =
                    rng.Next(2) == 0;

                int direction =
                    rng.Next(2) == 0
                        ? -1
                        : 1;

                int dx =
                    horizontal
                        ? direction
                        : 0;

                int dy =
                    horizontal
                        ? 0
                        : direction;


                if (
                    TryCarveServicePassage(
                        Z,
                        x,
                        y,
                        dx,
                        dy
                    )
                )
                {
                    placed++;
                }
            }
        }



        private bool TryCarveServicePassage(
            Zone Z,
            int startX,
            int startY,
            int dx,
            int dy
        )
        {
            if (Z == null)
                return false;


            int solidLength =
                0;


            for (
                int step = 1;
                step <=
                    MaxServicePassageLength +
                    1;
                step++
            )
            {
                int x =
                    startX +
                    (
                        dx *
                        step
                    );

                int y =
                    startY +
                    (
                        dy *
                        step
                    );


                if (
                    x < Border ||
                    y < Border ||
                    x >=
                        Z.Width -
                        Border ||
                    y >=
                        Z.Height -
                        Border
                )
                {
                    return false;
                }


                Cell cell =
                    Z.GetCell(
                        x,
                        y
                    );


                if (cell == null)
                    return false;


                //
                // We're specifically looking for untouched semantic fill.
                //
                if (
                    SubterraneanSites
                        .SubterraneanSitesEPGeometry
                        .IsSolidPlaceholder(
                            cell
                        )
                )
                {
                    solidLength++;

                    continue;
                }


                //
                // Once we hit existing open geometry, we've found the other end.
                //
                if (
                    IsOpenLayoutCell(
                        cell
                    )
                )
                {
                    if (
                        solidLength <
                            MinServicePassageLength ||
                        solidLength >
                            MaxServicePassageLength
                    )
                    {
                        return false;
                    }


                    //
                    // Cut only the intervening structural mass.
                    //
                    for (
                        int carveStep = 1;
                        carveStep <= solidLength;
                        carveStep++
                    )
                    {
                        CarveCell(
                            Z,
                            startX +
                                (
                                    dx *
                                    carveStep
                                ),
                            startY +
                                (
                                    dy *
                                    carveStep
                                )
                        );
                    }


                    return true;
                }


                //
                // Something other than clean fill/open geometry interrupted the run.
                //
                return false;
            }


            return false;
        }



        private bool IsOpenLayoutCell(
            Cell cell
        )
        {
            if (cell == null)
                return false;

            return
                !cell.HasWall();
        }

       


        private void CarveHorizontalCorridor(
            Zone Z,
            int x1,
            int x2,
            int y,
            int width
        )
        {
            if (width < 1)
                width = 1;

            int minX =
                Math.Min(
                    x1,
                    x2
                );

            int maxX =
                Math.Max(
                    x1,
                    x2
                );

            int offsetStart =
                -(width / 2);

            int offsetEnd =
                offsetStart +
                width -
                1;


            for (
                int x = minX;
                x <= maxX;
                x++
            )
            {
                for (
                    int offset = offsetStart;
                    offset <= offsetEnd;
                    offset++
                )
                {
                    CarveCell(
                        Z,
                        x,
                        y + offset
                    );
                }
            }
        }



        private void CarveVerticalCorridor(
            Zone Z,
            int x,
            int y1,
            int y2,
            int width
        )
        {
            if (width < 1)
                width = 1;

            int minY =
                Math.Min(
                    y1,
                    y2
                );

            int maxY =
                Math.Max(
                    y1,
                    y2
                );

            int offsetStart =
                -(width / 2);

            int offsetEnd =
                offsetStart +
                width -
                1;


            for (
                int y = minY;
                y <= maxY;
                y++
            )
            {
                for (
                    int offset = offsetStart;
                    offset <= offsetEnd;
                    offset++
                )
                {
                    CarveCell(
                        Z,
                        x + offset,
                        y
                    );
                }
            }
        }



        private void CarveRectangle(
            Zone Z,
            int x1,
            int y1,
            int x2,
            int y2
        )
        {
            if (Z == null)
                return;


            if (x1 > x2)
            {
                int swap =
                    x1;

                x1 =
                    x2;

                x2 =
                    swap;
            }


            if (y1 > y2)
            {
                int swap =
                    y1;

                y1 =
                    y2;

                y2 =
                    swap;
            }


            x1 =
                Math.Max(
                    Border,
                    x1
                );

            y1 =
                Math.Max(
                    Border,
                    y1
                );

            x2 =
                Math.Min(
                    Z.Width -
                        Border -
                        1,
                    x2
                );

            y2 =
                Math.Min(
                    Z.Height -
                        Border -
                        1,
                    y2
                );


            for (
                int x = x1;
                x <= x2;
                x++
            )
            {
                for (
                    int y = y1;
                    y <= y2;
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



        private void CarveCell(
            Zone Z,
            int x,
            int y
        )
        {
            if (Z == null)
                return;

            if (
                x < 0 ||
                y < 0 ||
                x >= Z.Width ||
                y >= Z.Height
            )
            {
                return;
            }

            if (
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

            cell.ClearWalls();
        }



        private void ForceSealedBorder(
            Zone Z
        )
        {
            if (Z == null)
                return;

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



        private int ClampInt(
            int value,
            int minimum,
            int maximum
        )
        {
            if (value < minimum)
                return minimum;

            if (value > maximum)
                return maximum;

            return value;
        }



        private void ClampSettings()
        {
            if (Border < 1)
                Border = 1;

            if (SpineHalfHeight < 1)
                SpineHalfHeight = 1;

            if (AnchorRadius < 1)
                AnchorRadius = 1;

            if (MinBayWidth < 5)
                MinBayWidth = 5;

            if (MaxBayWidth < MinBayWidth)
                MaxBayWidth = MinBayWidth;

            if (MinBayDepth < 3)
                MinBayDepth = 3;

            if (MaxBayDepth < MinBayDepth)
                MaxBayDepth = MinBayDepth;

            if (MinBayGap < 1)
                MinBayGap = 1;

            if (MaxBayGap < MinBayGap)
                MaxBayGap = MinBayGap;

            if (OppositeBayChance < 0)
                OppositeBayChance = 0;

            if (OppositeBayChance > 100)
                OppositeBayChance = 100;

            if (LargeProcedureChance < 0)
                LargeProcedureChance = 0;

            if (LargeProcedureChance > 100)
                LargeProcedureChance = 100;

            if (DoorWidth < 1)
                DoorWidth = 1;

            if (
                DoorWidth >
                MinBayWidth - 2
            )
            {
                DoorWidth =
                    MinBayWidth - 2;
            }

            if (MinHallSegments < 2)
                MinHallSegments = 2;

            if (MaxHallSegments < MinHallSegments)
                MaxHallSegments = MinHallSegments;

            if (HallOverlap < 1)
                HallOverlap = 1;

            if (MinHallOffset < 1)
                MinHallOffset = 1;

            if (MaxHallOffset < MinHallOffset)
                MaxHallOffset = MinHallOffset;


            if (MinServicePassages < 0)
                MinServicePassages = 0;

            if (MaxServicePassages < MinServicePassages)
                MaxServicePassages = MinServicePassages;

            if (MinServicePassageLength < 1)
                MinServicePassageLength = 1;

            if (
                MaxServicePassageLength <
                MinServicePassageLength
            )
            {
                MaxServicePassageLength =
                    MinServicePassageLength;
            }

        }
    }
}

namespace SubterraneanSites
{
    /// <summary>
    /// Blood Category-1 runtime environment.
    ///
    /// While the player is inside a Blood-primary EP and is not attuned,
    /// extradimensional blood continuously leaks from their body.
    ///
    /// This is intentionally harmless:
    /// the player becomes genuinely covered in blood, but takes no damage.
    /// Blood attunement stops the ongoing bleeding.
    /// </summary>
    [Serializable]
    public class SubterraneanSitesEPBloodEnvironmentSystem :
        IGameSystem
    {

        public bool WasInBloodEnvironment =
            false;

        public bool WasBloodAttuned =
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

        public void RefreshPlayerState()
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


            bool bloodEnvironment =
                string.Equals(
                    category1,
                    "Blood",
                    StringComparison.Ordinal
                );


            if (!bloodEnvironment)
            {
                ResetExposureState();

                return;
            }


            bool bloodAttuned =
                SubterraneanSitesEPAttunementSystem
                    .IsAttunedTo(
                        player,
                        "Blood"
                    );


            bool becameExposed =
                !bloodAttuned &&
                (
                    !WasInBloodEnvironment ||
                    WasBloodAttuned
                );


            WasInBloodEnvironment =
                true;

            WasBloodAttuned =
                bloodAttuned;


            if (becameExposed)
            {
                XRL.UI.Popup.Show(
                    "Blood drips from your openings."
                );
            }


            if (bloodAttuned)
                return;


            //
            // Actual LiquidCovered blood, not damage or vanilla Bleeding.
            //
            player.MakeBloody(
                "blood",
                1,
                2
            );
        }


        private void ResetExposureState()
        {
            WasInBloodEnvironment =
                false;

            WasBloodAttuned =
                false;
        }



    }
}

namespace XRL.World.Parts
{

    [Serializable]
    public class SubterraneanSitesConveyorLiquidDrain :
        IPart
    {
        public override bool SameAs(
            IPart p
        )
        {
            return false;
        }


        public override bool WantTurnTick()
        {
            return true;
        }


        public override void TurnTick(
            long TimeTick,
            int Amount
        )
        {
            DrainOpenLiquid();
        }


        private void DrainOpenLiquid()
        {
            Cell cell =
                ParentObject.GetCurrentCell();


            if (cell == null)
                return;


            List<GameObject> liquids =
                new List<GameObject>();


            foreach (
                GameObject obj
                in cell.GetObjectsInCell()
            )
            {
                if (
                    obj == null ||
                    obj == ParentObject
                )
                {
                    continue;
                }


                LiquidVolume liquid =
                    obj.LiquidVolume;


                //
                // Only remove open terrain liquid.
                //
                // Phials, tanks, canteens and other contained liquids are not
                // open volumes and are therefore untouched.
                //
                if (
                    liquid != null &&
                    liquid.IsOpenVolume() &&
                    liquid.Volume > 0
                )
                {
                    liquids.Add(
                        obj
                    );
                }
            }


            //
            // Don't mutate the cell object list while enumerating it.
            //
            foreach (
                GameObject liquidObject
                in liquids
            )
            {
                if (liquidObject != null)
                {
                    liquidObject.Obliterate();
                }
            }
        }
    }

    /// <summary>
    /// Blood C1 meat-processing hazard.
    ///
    /// Every turn, the statue checks its own cell and all locally adjacent
    /// cells. Any creature with valid severable anatomy has a chance to lose
    /// one non-decapitating body part.
    ///
    /// This is intentionally not powered machinery.
    /// </summary>
    [Serializable]
    public class SubterraneanSitesBloodDismemberer :
        IPart
    {
        public int ChancePerCreature =
            25;


        public override bool SameAs(
            IPart p
        )
        {
            return false;
        }


        public override bool WantTurnTick()
        {
            return true;
        }


        public override void TurnTick(
            long TimeTick,
            int Amount
        )
        {
            CheckDismemberment();
        }


        private void CheckDismemberment()
        {
            Cell cell =
                ParentObject.GetCurrentCell();


            if (
                cell == null ||
                cell.OnWorldMap()
            )
            {
                return;
            }


            //
            // Preserve the same spatial behavior as the chiral-rings
            // implementation: own cell plus all local adjacent cells.
            //
            CheckCell(
                cell
            );


            foreach (
                Cell adjacent
                in cell.GetLocalAdjacentCells()
            )
            {
                CheckCell(
                    adjacent
                );
            }
        }


        private void CheckCell(
            Cell C
        )
        {
            if (C == null)
                return;


            //
            // Dismemberment can create a severed-part object in the cell,
            // so use the same mutation-safe iteration pattern as vanilla.
            //
            int i = 0;

            for (
                int count = C.Objects.Count;
                i < count;
                i++
            )
            {
                GameObject target =
                    C.Objects[i];


                if (
                    target != null &&
                    target != ParentObject &&
                    target.Brain != null
                )
                {
                    TryDismember(
                        target
                    );
                }


                if (
                    count !=
                    C.Objects.Count
                )
                {
                    count =
                        C.Objects.Count;


                    if (
                        i < count &&
                        C.Objects[i] !=
                        target
                    )
                    {
                        i--;
                    }
                }
            }
        }


        private void TryDismember(
            GameObject Target
        )
        {
            if (
                Target == null ||
                Target.Body == null
            )
            {
                return;
            }


            if (Target.IsInStasis())
                return;


            //
            // Keep the physical compatibility checks used by the vanilla
            // chiral-rings behavior.
            //
            if (
                !Target.PhaseMatches(
                    ParentObject
                )
            )
            {
                return;
            }


            if (
                !ParentObject.FlightMatches(
                    Target
                )
            )
            {
                return;
            }


            if (
                !ChancePerCreature.in100()
            )
            {
                return;
            }


            //
            // Use Qud's actual anatomy/dismemberment machinery.
            //
            // suppressDecapitate:true is the key Blood-theme rule:
            // the machine takes limbs, never heads.
            //
            XRL.World.Parts.Skill
                .Axe_Dismember
                .Dismember(
                    ParentObject,
                    Target,
                    null,
                    null,
                    null,
                    null,
                    "sfx_characterTrigger_dismember",
                    assumeDecapitate: false,
                    suppressDecapitate: true,
                    weaponActing: false,
                    UsePopups: false
                );
        }
    }
}