using System;
using System.Collections.Generic;
using Genkit;
using XRL;
using XRL.Core;
using XRL.World;
using XRL.Rules;
using XRL.World.Parts;
using XRL.UI;
using XRL.EditorFormats.Map;

namespace SubterraneanSites
{
    /// <summary>
    /// PORTAL DIMENSION
    ///
    /// C1:
    ///     deferred.
    ///
    /// C2:
    ///     deferred; later owns rifts / portal set pieces.
    ///
    /// C3:
    ///     Blueshifted Chrome structural mass with exposed
    ///     SultanWall_Period1 interior surfaces.
    ///
    /// C4:
    ///     large transit chamber + oval side chambers + narrow halls,
    ///     sparse dividers / doors, worn Fibonacci-rhythm transit floor.
    ///
    /// C5:
    ///     deferred.
    /// </summary>
    internal sealed class SubterraneanSitesEPPortalTheme :
        ISubterraneanSitesEPCategoryProvider,
        ISubterraneanSitesEPPrimaryObjectProvider,
        ISubterraneanSitesEPAttunementProvider,
        ISubterraneanSitesEPDenizenAdaptationProvider,
        ISubterraneanSitesEPSignatureMutationProvider
    {
        public string ThemeKey
        {
            get { return "Portal"; }
        }

        public string SignatureMutationClass
        {
            get { return "Teleportation"; }
        }


        public void ApplyDenizenAdaptation(
            GameObject creature
        )
        {
            if (creature == null)
                return;

            XRL.World.Effects
                .SubterraneanSitesEPPortalDenizenAdaptationEffect existing =
                    creature.GetEffectDescendedFrom<
                        XRL.World.Effects
                            .SubterraneanSitesEPPortalDenizenAdaptationEffect
                    >();

            if (existing == null)
            {
                creature.ApplyEffect(
                    new XRL.World.Effects
                        .SubterraneanSitesEPPortalDenizenAdaptationEffect()
                );
            }
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
            if (cell == null)
                return;

            int x =
                cell.X;

            int y =
                cell.Y;

            //
            // Fibonacci-ish transit pattern.
            //
            // Two crossing modular rhythms use successive Fibonacci
            // numbers:
            //
            //     8, 13, 21, 34, 55
            //
            // This produces repeating diagonal wayfinding / transit
            // traces without becoming a literal checkerboard.
            //
            // A deterministic wear mask removes parts of the pattern
            // so the installation feels old and partially abandoned.
            //
            int tileVariant =
                PositiveMod(
                    x * 3 +
                    y * 5,
                    4
                ) + 1;

            cell.PaintTile =
                "Terrain/sw_ground_dots" +
                tileVariant.ToString() +
                ".png";


            int primaryPhase =
                PositiveMod(
                    x * 13 +
                    y * 8,
                    21
                );

            int secondaryPhase =
                PositiveMod(
                    x * 8 -
                    y * 13,
                    34
                );


            bool primaryTrace =
                primaryPhase == 0 ||
                primaryPhase == 1;

            bool secondaryTrace =
                secondaryPhase == 0;


            bool transferNode =
                PositiveMod(
                    x * 5 +
                    y * 3,
                    13
                ) == 0 &&
                PositiveMod(
                    x * 3 -
                    y * 5,
                    8
                ) == 0;


            int wear =
                PositiveMod(
                    x * 97 +
                    y * 193 +
                    x * y * 11,
                    100
                );


            bool wornOut =
                wear < 18 ||
                PositiveMod(
                    x * 13 +
                    y * 21,
                    55
                ) == 0;


            if (wornOut)
            {
                //
                // Pattern has been scuffed away here.
                //
                cell.PaintTileColor =
                    "&k";

                cell.PaintColorString =
                    "&k^k";

                cell.PaintDetailColor =
                    "k";
            }
            else if (transferNode)
            {
                //
                // Rare bright transfer / wayfinding node.
                //
                cell.PaintTileColor =
                    "&W";

                cell.PaintColorString =
                    "&W^k";

                cell.PaintDetailColor =
                    "C";
            }
            else if (
                primaryTrace &&
                secondaryTrace
            )
            {
                //
                // Crossing transit traces.
                //
                cell.PaintTileColor =
                    "&C";

                cell.PaintColorString =
                    "&C^k";

                cell.PaintDetailColor =
                    "W";
            }
            else if (primaryTrace)
            {
                cell.PaintTileColor =
                    "&B";

                cell.PaintColorString =
                    "&B^k";

                cell.PaintDetailColor =
                    "C";
            }
            else if (secondaryTrace)
            {
                cell.PaintTileColor =
                    "&C";

                cell.PaintColorString =
                    "&C^k";

                cell.PaintDetailColor =
                    "B";
            }
            else
            {
                //
                // Quiet terminal flooring between marked routes.
                //
                cell.PaintTileColor =
                    "&K";

                cell.PaintColorString =
                    "&K^k";

                cell.PaintDetailColor =
                    "B";
            }
        }


        private static int PositiveMod(
            int value,
            int modulus
        )
        {
            if (modulus <= 0)
                return 0;

            int result =
                value %
                modulus;

            if (result < 0)
                result += modulus;

            return result;
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

            The.Game.RequireSystem<
                SubterraneanSitesEPPortalEnvironmentSystem
            >();

            The.Game.RequireSystem<
                SubterraneanSitesEPPortalInstabilitySystem
            >();
        }

        public SubterraneanSitesEPAttunementBuildResult TryCreateAttunement(
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
                    .SubterraneanSitesEPPortalAttunementEffect(
                        SubterraneanSitesEPAttunementSystem
                            .DefaultDuration,
                        mutationLevel
                    );

            successMessage =
                "Attunement grants:\n" +
                "Teleportation (level " +
                mutationLevel.ToString() +
                ")\n" +
                "Spatial instability is suppressed\n" +
                "You may refuse involuntary translocation.";

            return
                SubterraneanSitesEPAttunementBuildResult
                    .Success;
        }

        public void RegisterCategory1Objects(
            SubterraneanSitesEPLayerContext context
        )
        {
            if (context == null)
                return;

            The.ZoneManager.AddZonePostBuilder(
                context.ZoneId,
                "SubterraneanSitesPortalRifts",
                "MinRifts", "1",
                "MaxRifts", "3",
                "MinSpacing", "8",
                "TransitionExclusionRadius", "5"
            );
        }


        public void RegisterEntranceCategory1Objects(
            SubterraneanSitesEPEntranceContext context
        )
        {
            if (context == null)
                return;

            //
            // One stationary rift previews Portal C1 at the breach.
            //
            // Moving-vortex generation remains underground-only.
            //
            The.ZoneManager.AddZonePostBuilder(
                context.ZoneId,
                "SubterraneanSitesPortalRifts",
                "EntranceOnly", "1",
                "MinRifts", "1",
                "MaxRifts", "1"
            );
        }


        public void RegisterCategory2(
            SubterraneanSitesEPLayerContext context
        )
        {
            if (context == null)
                return;

            //
            // Large Portal transit infrastructure gets first priority.
            //
            The.ZoneManager.AddZonePostBuilder(
                context.ZoneId,
                "SubterraneanSitesPortalInfrastructure"
            );

            //
            // Aloe Porta transit network.
            //
            // Each generated pair receives its own GroupKey, so every
            // pair forms a private two-endpoint teleport connection rather
            // than joining one giant zone-wide Aloe Porta network.
            //
            The.ZoneManager.AddZonePostBuilder(
                context.ZoneId,
                "SubterraneanSitesPortalAloePortaPairs",
                "MinPairs", "8",
                "MaxPairs", "16",
                "MinPairSeparation", "8",
                "TransitionExclusionRadius", "4"
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
                "SubterraneanSitesPortalMaterials"
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
                "SubterraneanSitesPortalLayout"
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
                "SubterraneanSitesPortalFloor"
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
                "SubterraneanSitesPortalDecorations"
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
                "SubterraneanSitesPortalFloor",
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
                "SubterraneanSitesPortalDecorations",
                "EntranceOnly", "1"
            );
        }
    }

    /// <summary>
    /// Portal Category-1 runtime environment.
    ///
    /// WhileS the player is on an underground EP layer whose actual
    /// Category-1 winner is Portal, there is a small independent chance
    /// each turn for a normal Space-Time Vortex to appear somewhere
    /// in the playable zone.
    ///
    /// This mechanic is intentionally independent of the persistent
    /// Space-Time Rifts. The rifts retain completely vanilla behavior.
    /// </summary>
    [Serializable]
    public class SubterraneanSitesEPPortalEnvironmentSystem :
        IGameSystem
    {
        //
        // Match the vanilla Space-Time Rift/Vortex emergence base rate:
        // 5 per 1000 turns.
        //
        public const int VortexSpawnPermillage =
            15;

        public const int VortexDuration =
            10;

        public const int MinimumPortalSeparation =
            7;

        private const string EmittedVortexProperty =
            "SubterraneanSitesPortalEmittedVortex";


        public override void Register(
            XRLGame Game,
            IEventRegistrar Registrar
        )
        {
            Registrar.Register(
                EndTurnEvent.ID
            );
        }


        public override bool HandleEvent(
            EndTurnEvent E
        )
        {
            TrySpawnVortex();

            return true;
        }


        private static void TrySpawnVortex()
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


            Zone Z =
                player.CurrentZone;


            string category1 =
                SubterraneanSitesEPAttunementSystem
                    .GetCategory1Theme(
                        Z
                    );


            if (
                !string.Equals(
                    category1,
                    "Portal",
                    StringComparison.Ordinal
                )
            )
            {
                return;
            }


            //
            // Category1Theme is also stored on the origin zone for its
            // attunement stone. The random vortex environment belongs only
            // to the actual underground pocket, not the entrance scar.
            //
            Location2D entranceCenter;
            int scarRadius;
            int holeRadius;

            if (
                SubterraneanSitesEPEntrance
                    .TryGetSpec(
                        Z,
                        out entranceCenter,
                        out scarRadius,
                        out holeRadius
                    )
            )
            {
                return;
            }


            //
            // Independent zone-wide Portal roll.
            //
            // This is deliberately NOT tied to how many Space-Time Rifts
            // happened to be placed on this layer.
            //
            if (
                !VortexSpawnPermillage
                    .in1000()
            )
            {
                return;
            }


            List<Cell> candidates =
                GetVortexSpawnCandidates(
                    Z
                );


            if (
                candidates.Count == 0
            )
            {
                return;
            }


            Cell destination =
                candidates[
                    Stat.Random(
                        0,
                        candidates.Count - 1
                    )
                ];


            GameObject vortex =
                GameObject.Create(
                    "Space-Time Vortex"
                );


            if (vortex == null)
                return;


            XRL.World.Parts.Temporary temporary =
                vortex.GetPart<
                    XRL.World.Parts.Temporary
                >();


            if (temporary != null)
            {
                temporary.Duration =
                    VortexDuration;
            }


            //
            // Mark only vortices created by this EP environmental mechanic.
            // Future Portal spawns can keep away from one another without
            // interfering with vortices created by the player or vanilla.
            //
            vortex.SetIntProperty(
                EmittedVortexProperty,
                1
            );


            destination.AddObject(
                vortex
            );

            vortex.MakeActive();
        }


        private static List<Cell>
            GetVortexSpawnCandidates(
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
                if (
                    IsValidVortexSpawnCell(
                        Z,
                        cell
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


        private static bool IsValidVortexSpawnCell(
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


            //
            // Materialize somewhere in the playable architecture.
            // After that the vanilla vortex may wander wherever it likes.
            //
            if (
                !SubterraneanSitesEPGeometry
                    .IsOpenGeometryCell(
                        cell
                    )
            )
            {
                return false;
            }


            if (
                cell.IsSolid() ||
                cell.HasSpawnBlocker()
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


            if (
                SubterraneanSitesEPPlacement
                    .HasMeaningfulOccupant(
                        cell
                    )
            )
            {
                return false;
            }


            if (
                !IsFarEnoughFromPortalHazards(
                    Z,
                    cell
                )
            )
            {
                return false;
            }


            return true;
        }


        private static bool IsFarEnoughFromPortalHazards(
            Zone Z,
            Cell candidate
        )
        {
            foreach (
                Cell cell
                in Z.GetCells()
            )
            {
                if (
                    cell == null ||
                    !ContainsPortalHazard(
                        cell
                    )
                )
                {
                    continue;
                }


                int dx =
                    Math.Abs(
                        candidate.X -
                        cell.X
                    );

                int dy =
                    Math.Abs(
                        candidate.Y -
                        cell.Y
                    );


                //
                // Vortices move diagonally, so this is the minimum number
                // of movement steps between the two cells.
                //
                int distance =
                    Math.Max(
                        dx,
                        dy
                    );


                if (
                    distance <
                    MinimumPortalSeparation
                )
                {
                    return false;
                }
            }


            return true;
        }


        private static bool ContainsPortalHazard(
            Cell cell
        )
        {
            if (cell == null)
                return false;


            //
            // C1 static rifts.
            //
            if (
                cell.HasObjectWithBlueprint(
                    "Space-Time Rift"
                )
            )
            {
                return true;
            }


            //
            // Other vortices created by this zone-wide Portal effect.
            //
            foreach (
                GameObject obj
                in cell.GetObjectsInCell()
            )
            {
                if (
                    obj != null &&
                    obj.GetIntProperty(
                        EmittedVortexProperty
                    ) > 0
                )
                {
                    return true;
                }
            }


            //
            // When we add C2 portal stands, add their actual functional
            // portal blueprints here. Nothing else needs to change.
            //

            return false;
        }
    }
}

namespace SubterraneanSites
{
    /// <summary>
    /// Portal Category-1 player instability.
    ///
    /// While the player is inside a Portal-C1 environment and is not
    /// Portal-attuned, spatial instability involuntarily teleports them
    /// once every 15-50 turns.
    ///
    /// Attunement suppresses the instability entirely.
    /// Leaving Portal C1 resets the timer.
    /// </summary>
    [Serializable]
    public class SubterraneanSitesEPPortalInstabilitySystem :
        IGameSystem
    {
        public const int MinimumBlinkDelay =
            50;

        public const int MaximumBlinkDelay =
            100;

        public int TurnsUntilBlink =
            0;

        public string ScheduledZoneID =
            "";


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
            SynchronizePlayerState(
                advanceTimer: false
            );

            return true;
        }


        public override bool HandleEvent(
            EndTurnEvent E
        )
        {
            SynchronizePlayerState(
                advanceTimer: true
            );

            return true;
        }


        private void SynchronizePlayerState(
            bool advanceTimer
        )
        {
            GameObject player =
                The.Player;

            if (
                player == null ||
                player.CurrentZone == null
            )
            {
                ResetTimer();

                return;
            }

            string category1 =
                SubterraneanSitesEPAttunementSystem
                    .GetCategory1Theme(
                        player.CurrentZone
                    );

            bool portalEnvironment =
                string.Equals(
                    category1,
                    "Portal",
                    StringComparison.Ordinal
                );


            XRL.World.Effects
                .SubterraneanSitesEPPortalSpatialInstabilityEffect
                instability =
                    player.GetEffectDescendedFrom<
                        XRL.World.Effects
                            .SubterraneanSitesEPPortalSpatialInstabilityEffect
                    >();


            if (!portalEnvironment)
            {
                if (instability != null)
                {
                    player.RemoveEffect(
                        instability
                    );
                }

                ResetTimer();

                return;
            }


            bool portalAttuned =
                SubterraneanSitesEPAttunementSystem
                    .IsAttunedTo(
                        player,
                        "Portal"
                    );


            if (portalAttuned)
            {
                if (instability != null)
                {
                    player.RemoveEffect(
                        instability
                    );
                }

                ResetTimer();

                return;
            }


            //
            // Unattuned Portal exposure owns a visible, clickable status effect.
            // The actual blink timing remains in this environment system.
            //
            if (instability == null)
            {
                player.ApplyEffect(
                    new XRL.World.Effects
                        .SubterraneanSitesEPPortalSpatialInstabilityEffect()
                );
            }



            string zoneID =
                player.CurrentZone.ZoneID;

            //
            // First turn in this Portal layer, or newly returned to an
            // unattuned state: start a fresh 15-50 turn countdown.
            //
            if (
                !string.Equals(
                    ScheduledZoneID,
                    zoneID,
                    StringComparison.Ordinal
                ) ||
                TurnsUntilBlink <= 0
            )
            {
                ScheduledZoneID =
                    zoneID;

                ScheduleNextBlink();

                return;
            }

            if (!advanceTimer)
                return;

            TurnsUntilBlink--;

            if (TurnsUntilBlink > 0)
                return;

            //
            // Use Qud's normal reality-distortion teleport machinery.
            //
            // MaxDistance is deliberately left at zero, so the instability
            // may relocate the player anywhere valid in the current zone.
            //
            player.RandomTeleport(
                Swirl: true,
                Forced: true,
                Voluntary: false
            );

            ScheduleNextBlink();
        }


        private void ScheduleNextBlink()
        {
            TurnsUntilBlink =
                Stat.Random(
                    MinimumBlinkDelay,
                    MaximumBlinkDelay
                );
        }


        private void ResetTimer()
        {
            TurnsUntilBlink =
                0;

            ScheduledZoneID =
                "";
        }
    }
}


namespace XRL.World.ZoneBuilders
{

    /// <summary>
    /// Portal Category-1 physical anomalies.
    ///
    /// Places 1-3 persistent stationary Space-Time Rifts in broad open
    /// portions of whatever Category-4 geometry actually won.
    ///
    /// The custom Portal rift inherits all vanilla Space-Time Rift behavior.
    /// Its only additional behavior is an independent chance to emit an
    /// ordinary moving Space-Time Vortex.
    ///
    /// Does not carve geometry.
    /// </summary>
    public class SubterraneanSitesPortalRifts :
        ZoneBuilderSandbox
    {
        public int EntranceOnly =
            0;
        public string RiftBlueprint =
            "Space-Time Rift";

        public int MinRifts =
            1;

        public int MaxRifts =
            3;

        public int MinSpacing =
            8;

        public int TransitionExclusionRadius =
            5;


        public bool BuildZone(
            Zone Z
        )
        {
            if (Z == null)
                return true;


            MinRifts =
                Math.Max(
                    0,
                    MinRifts
                );

            MaxRifts =
                Math.Max(
                    MinRifts,
                    MaxRifts
                );

            MinSpacing =
                Math.Max(
                    0,
                    MinSpacing
                );

            TransitionExclusionRadius =
                Math.Max(
                    0,
                    TransitionExclusionRadius
                );


            List<Location2D> anchors =
                SubterraneanSites
                    .SubterraneanSitesEPVerticalTransitions
                    .GetVerticalAnchors(
                        Z.ZoneID
                    );


            int seed =
                XRLCore.Core.Game.GetWorldSeed(
                    "SubterraneanSites:PortalRifts:" +
                    Z.ZoneID
                );

            System.Random rng =
                new System.Random(
                    seed
                );


            int desired =
                rng.Next(
                    MinRifts,
                    MaxRifts + 1
                );


            List<Cell> placed =
                new List<Cell>();


            for (
                int i = 0;
                i < desired;
                i++
            )
            {
                List<Cell> candidates =
                    CollectCandidates(
                        Z,
                        anchors,
                        placed
                    );


                if (candidates.Count == 0)
                    break;


                Cell cell =
                    candidates[
                        rng.Next(
                            candidates.Count
                        )
                    ];


                GameObject rift =
                    GameObject.Create(
                        RiftBlueprint
                    );


                if (rift == null)
                    continue;


                cell.AddObject(
                    rift
                );

                rift.MakeActive();


                placed.Add(
                    cell
                );


                //
                // The rift itself occupies only one cell, but its adjacent
                // cells are its functional emergence area.
                //
                // Protect the 3x3 footprint from later C2/C5 placement.
                //
                SubterraneanSites
                    .SubterraneanSitesEPReservations
                    .ClaimRectangle(
                        Z,
                        cell.X - 1,
                        cell.Y - 1,
                        cell.X + 1,
                        cell.Y + 1
                    );
            }


            return true;
        }


        private List<Cell> CollectCandidates(
            Zone Z,
            List<Location2D> anchors,
            List<Cell> placed
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
                    CellAvailable(
                        Z,
                        anchors,
                        placed,
                        cell
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


        private bool CellAvailable(
            Zone Z,
            List<Location2D> anchors,
            List<Cell> placed,
            Cell cell
        )
        {
            if (cell == null)
                return false;


            if (EntranceOnly != 0)
            {
                //
                // Entrance preview is confined to the dimensional scar
                // and kept well clear of the central descent.
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
                            2
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
                    .HasMeaningfulOccupant(
                        cell
                    )
            )
            {
                return false;
            }


            if (
                EntranceOnly == 0 &&
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


            //
            // Rifts generate things immediately around themselves, so keep
            // them out of one-cell corridors and cramped corners.
            //
            if (
                !SubterraneanSites
                    .SubterraneanSitesEPPlacement
                    .HasBroadOpenClearance(
                        Z,
                        cell,
                        delegate(Cell neighbor)
                        {
                            return
                                IsClearRiftNeighborhoodCell(
                                    Z,
                                    neighbor
                                );
                        },
                        1
                    )
            )
            {
                return false;
            }


            foreach (
                Cell existing
                in placed
            )
            {
                if (existing == null)
                    continue;


                int dx =
                    Math.Abs(
                        cell.X -
                        existing.X
                    );

                int dy =
                    Math.Abs(
                        cell.Y -
                        existing.Y
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


        private bool IsClearRiftNeighborhoodCell(
            Zone Z,
            Cell cell
        )
        {
            if (cell == null)
                return false;


            if (EntranceOnly != 0)
            {
                //
                // Keep the entire 3x3 functional neighborhood of an
                // entrance rift inside the scar and outside the hole.
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
            }


            if (!cell.IsEmptyOfSolid())
                return false;


            if (cell.HasSpawnBlocker())
                return false;


            return true;
        }
    }

    /// <summary>
    /// Portal Category-2 transit infrastructure.
    ///
    /// Placement priority:
    ///   1. one randomly selected portal-gate prefab
    ///   2. large Quantum Rippler installation
    ///   3. small Quantum Rippler installation when a second rippler
    ///      is requested, or as fallback if the large one cannot place
    ///
    /// Prefabs may excavate abstract structural walls. They may not
    /// deliberately overwrite earlier reserved EP content or meaningful
    /// discrete objects.
    /// </summary>
    public class SubterraneanSitesPortalInfrastructure :
        ZoneBuilderSandbox
    {
        //
        // Preserve the absolute dimensional shell.
        //
        private const int BorderClearance =
            1;


        //
        // The later EP hole builder is destructive, so keep prefab
        // footprints away from preplanned vertical-transition anchors.
        //
        public int TransitionExclusionRadius =
            4;


        //
        // Equal weighting for the first Portal C2 pass.
        //
        private static readonly string[] PortalGatePrefabs =
            new string[]
            {
                "preset_tile_chunks/HydraulicTeleportGate1.rpm",
                "preset_tile_chunks/HydraulicTeleportGate2.rpm",
                "preset_tile_chunks/HydraulicTeleportGate3.rpm",
                "preset_tile_chunks/RuinedTeleportGate.rpm",
                "preset_tile_chunks/TeleportGate1.rpm",
                "preset_tile_chunks/TeleportGate2.rpm",
                "preset_tile_chunks/TeleportGate3.rpm"//,
                //"preset_tile_chunks/Gate_5x5.rpm"
            };


        private const string LargeRipplerPrefab =
            "preset_tile_chunks/SpaceTimeRift_11x7.rpm";


        private const string SmallRipplerPrefab =
            "preset_tile_chunks/SpaceTimeRift_5x5.rpm";


        public bool BuildZone(
            Zone Z
        )
        {
            if (Z == null)
                return true;


            TransitionExclusionRadius =
                Math.Max(
                    0,
                    TransitionExclusionRadius
                );


            List<Location2D> anchors =
                SubterraneanSites
                    .SubterraneanSitesEPVerticalTransitions
                    .GetVerticalAnchors(
                        Z.ZoneID
                    );


            int seed =
                XRLCore.Core.Game.GetWorldSeed(
                    "SubterraneanSites:PortalInfrastructure:" +
                    Z.ZoneID
                );


            System.Random rng =
                new System.Random(
                    seed
                );


            //
            // ============================================================
            // PORTAL GATE
            // ============================================================
            //
            // Pick the visual/mechanical variant first. All seven currently
            // have equal standing.
            //
            string gatePrefab =
                PortalGatePrefabs[
                    rng.Next(
                        PortalGatePrefabs.Length
                    )
                ];


            TryPlacePrefab(
                Z,
                gatePrefab,
                9,
                7,
                anchors,
                rng
            );


            //
            // ============================================================
            // QUANTUM RIPPLERS
            // ============================================================
            //
            // One or two installations per layer.
            //
            // Prefer the large installation. If only one was requested
            // and the large one cannot place, fall back to the small one.
            //
            // When two were requested, attempt both.
            //
            int desiredRipplers =
                rng.Next(
                    1,
                    3
                );


            bool largePlaced =
                TryPlacePrefab(
                    Z,
                    LargeRipplerPrefab,
                    11,
                    7,
                    anchors,
                    rng
                );


            if (desiredRipplers >= 2)
            {
                TryPlacePrefab(
                    Z,
                    SmallRipplerPrefab,
                    5,
                    5,
                    anchors,
                    rng
                );
            }
            else if (!largePlaced)
            {
                TryPlacePrefab(
                    Z,
                    SmallRipplerPrefab,
                    5,
                    5,
                    anchors,
                    rng
                );
            }


            //
            // C2 may have excavated new geometry. The shared pipeline will
            // rebuild actual reachability after C5, but discard any stale
            // map now.
            //
            Z.ClearReachableMap();

            return true;
        }


        private bool TryPlacePrefab(
            Zone Z,
            string prefabName,
            int width,
            int height,
            List<Location2D> anchors,
            System.Random rng
        )
        {
            if (
                Z == null ||
                prefabName.IsNullOrEmpty() ||
                rng == null
            )
            {
                return false;
            }


            MapFile mapFile =
                MapFile.Resolve(
                    prefabName
                );


            if (mapFile == null)
                return false;


            if (
                width <= 0 ||
                height <= 0
            )
            {
                return false;
            }


            List<Cell> candidates =
                new List<Cell>();


            int minX =
                BorderClearance;

            int minY =
                BorderClearance;


            int maxX =
                Z.Width -
                BorderClearance -
                width;

            int maxY =
                Z.Height -
                BorderClearance -
                height;


            if (
                maxX < minX ||
                maxY < minY
            )
            {
                return false;
            }


            for (
                int x = minX;
                x <= maxX;
                x++
            )
            {
                for (
                    int y = minY;
                    y <= maxY;
                    y++
                )
                {
                    if (
                        FootprintAvailable(
                            Z,
                            x,
                            y,
                            width,
                            height,
                            anchors
                        )
                    )
                    {
                        Cell upperLeft =
                            Z.GetCell(
                                x,
                                y
                            );


                        if (upperLeft != null)
                        {
                            candidates.Add(
                                upperLeft
                            );
                        }
                    }
                }
            }


            if (candidates.Count == 0)
                return false;


            Cell chosen =
                candidates[
                    rng.Next(
                        candidates.Count
                    )
                ];


            int x1 =
                chosen.X;

            int y1 =
                chosen.Y;

            int x2 =
                x1 +
                width -
                1;

            int y2 =
                y1 +
                height -
                1;


            //
            // Do not use ZoneBuilderSandbox.PlacePrefab() here.
            //
            // Vanilla PlacePrefab derives its application rectangle from
            // RightmostObject()/BottommostObject(). That can truncate painted-only
            // cells lying beyond the furthest actual object in an RPM.
            //
            // Portal knows the complete intended dimensions of these prefabs, so
            // apply every map cell in that rectangle directly.
            //
            for (
                int localY = 0;
                localY < height;
                localY++
            )
            {
                for (
                    int localX = 0;
                    localX < width;
                    localX++
                )
                {
                    Cell target =
                        Z.GetCell(
                            chosen.X + localX,
                            chosen.Y + localY
                        );


                    if (target == null)
                        continue;


                    mapFile.Cells[
                        localX,
                        localY
                    ].ApplyTo(
                        target,
                        false,
                        PreAction:
                            cell =>
                            {
                                if (cell != null)
                                {
                                    cell.ClearWalls();
                                }
                            }
                    );
                }
            }


            SubterraneanSites
                .SubterraneanSitesEPReservations
                .ClaimRectangle(
                    Z,
                    x1,
                    y1,
                    x2,
                    y2
                );


            //
            // Teleport-gate prefabs receive their own small local service
            // installation. Quantum-rippler prefabs deliberately do not.
            //
            if (
                IsPortalGatePrefab(
                    prefabName
                )
            )
            {
                DressPortalGate(
                    Z,
                    prefabName,
                    x1,
                    y1,
                    x2,
                    y2,
                    anchors,
                    rng
                );
            }


            return true;
        }

                // ================================================================
        // PORTAL GATE SERVICE DRESSING
        // ================================================================

        private sealed class GateServicePlan
        {
            public Cell AnchorCell;

            public Cell MachineCell;

            public List<Cell> ConduitCells =
                new List<Cell>();

            public bool MachineInsidePrefab;
        }


        private bool IsPortalGatePrefab(
            string prefabName
        )
        {
            if (prefabName.IsNullOrEmpty())
                return false;

            return
                prefabName.IndexOf(
                    "TeleportGate",
                    StringComparison.OrdinalIgnoreCase
                ) >= 0;
        }


        private void DressPortalGate(
            Zone Z,
            string prefabName,
            int x1,
            int y1,
            int x2,
            int y2,
            List<Location2D> anchors,
            System.Random rng
        )
        {
            if (
                Z == null ||
                prefabName.IsNullOrEmpty() ||
                rng == null
            )
            {
                return;
            }


            bool hydraulic =
                prefabName.IndexOf(
                    "HydraulicTeleportGate",
                    StringComparison.OrdinalIgnoreCase
                ) >= 0;


            bool ruined =
                prefabName.IndexOf(
                    "RuinedTeleportGate",
                    StringComparison.OrdinalIgnoreCase
                ) >= 0;


            if (ruined)
            {
                DressRuinedGate(
                    Z,
                    x1,
                    y1,
                    x2,
                    y2,
                    anchors,
                    rng
                );

                return;
            }


            if (hydraulic)
            {
                DressHydraulicGate(
                    Z,
                    x1,
                    y1,
                    x2,
                    y2,
                    anchors,
                    rng
                );

                return;
            }


            DressElectricalGate(
                Z,
                x1,
                y1,
                x2,
                y2,
                anchors,
                rng
            );
        }


        // ================================================================
        // HYDRAULIC GATE
        // ================================================================

        private void DressHydraulicGate(
            Zone Z,
            int x1,
            int y1,
            int x2,
            int y2,
            List<Location2D> anchors,
            System.Random rng
        )
        {
            List<Cell> networkCells =
                new List<Cell>();


            string primaryMachine;

            int roll =
                rng.Next(100);


            if (roll < 70)
            {
                primaryMachine =
                    "Fusion Pumping Station";
            }
            else if (roll < 90)
            {
                primaryMachine =
                    "Hydraulic Turbine";
            }
            else
            {
                primaryMachine =
                    "Hydraulic Bubblething";
            }


            if (
                !TryBuildGateServiceSpine(
                    Z,
                    x1,
                    y1,
                    x2,
                    y2,
                    anchors,
                    rng,
                    "GlassHydraulicPipe",
                    "HydraulicPowerTransmission",
                    primaryMachine,
                    networkCells
                )
            )
            {
                return;
            }


            //
            // Secondary equipment attaches to the same service spine.
            //
            if (
                rng.Next(100) <
                45
            )
            {
                TryPlaceMachineAdjacentToNetwork(
                    Z,
                    x1,
                    y1,
                    x2,
                    y2,
                    anchors,
                    rng,
                    networkCells,
                    "Hydraulic Turbine"
                );
            }


            if (
                rng.Next(100) <
                30
            )
            {
                TryPlaceMachineAdjacentToNetwork(
                    Z,
                    x1,
                    y1,
                    x2,
                    y2,
                    anchors,
                    rng,
                    networkCells,
                    "Hydraulic Bubblething"
                );
            }
        }


        // ================================================================
        // ELECTRICAL GATE
        // ================================================================

        private void DressElectricalGate(
            Zone Z,
            int x1,
            int y1,
            int x2,
            int y2,
            List<Location2D> anchors,
            System.Random rng
        )
        {
            List<Cell> networkCells =
                new List<Cell>();


            string primaryMachine;

            int roll =
                rng.Next(100);


            if (roll < 70)
            {
                primaryMachine =
                    "Fusion Power Station";
            }
            else if (roll < 90)
            {
                primaryMachine =
                    "Broadcast Power Station";
            }
            else
            {
                primaryMachine =
                    "Electrothing";
            }


            string wireBlueprint =
                rng.Next(100) < 30
                    ? "HeavyPowerLine"
                    : "PowerLine";


            if (
                !TryBuildGateServiceSpine(
                    Z,
                    x1,
                    y1,
                    x2,
                    y2,
                    anchors,
                    rng,
                    wireBlueprint,
                    "ElectricalPowerTransmission",
                    primaryMachine,
                    networkCells
                )
            )
            {
                return;
            }


            if (
                rng.Next(100) <
                45
            )
            {
                TryPlaceMachineAdjacentToNetwork(
                    Z,
                    x1,
                    y1,
                    x2,
                    y2,
                    anchors,
                    rng,
                    networkCells,
                    "Electrothing"
                );
            }


            if (
                rng.Next(100) <
                30
            )
            {
                TryPlaceMachineAdjacentToNetwork(
                    Z,
                    x1,
                    y1,
                    x2,
                    y2,
                    anchors,
                    rng,
                    networkCells,
                    "Broadcast Power Station"
                );
            }
        }


        // ================================================================
        // RUINED GATE
        // ================================================================

        private void DressRuinedGate(
            Zone Z,
            int x1,
            int y1,
            int x2,
            int y2,
            List<Location2D> anchors,
            System.Random rng
        )
        {
            List<Cell> networkCells =
                new List<Cell>();


            bool hydraulic =
                rng.Next(2) == 0;


            string conduitBlueprint =
                hydraulic
                    ? "GlassHydraulicPipe"
                    : (
                        rng.Next(100) < 25
                            ? "HeavyPowerLine"
                            : "PowerLine"
                    );


            string transmissionPart =
                hydraulic
                    ? "HydraulicPowerTransmission"
                    : "ElectricalPowerTransmission";


            string machineBlueprint =
                hydraulic
                    ? "Hydraulic Bubblething"
                    : "Electrothing";


            TryBuildGateServiceSpine(
                Z,
                x1,
                y1,
                x2,
                y2,
                anchors,
                rng,
                conduitBlueprint,
                transmissionPart,
                machineBlueprint,
                networkCells,
                2,
                4
            );
        }

                // ================================================================
        // GATE-ATTACHED SERVICE SPINE
        // ================================================================

        private bool TryBuildGateServiceSpine(
            Zone Z,
            int x1,
            int y1,
            int x2,
            int y2,
            List<Location2D> anchors,
            System.Random rng,
            string conduitBlueprint,
            string transmissionPart,
            string machineBlueprint,
            List<Cell> networkCells,
            int minDistance = 2,
            int maxDistance = 6
        )
        {
            if (
                Z == null ||
                rng == null ||
                conduitBlueprint.IsNullOrEmpty() ||
                machineBlueprint.IsNullOrEmpty() ||
                networkCells == null
            )
            {
                return false;
            }


            Cell anchor =
                FindGateServiceAnchor(
                    Z,
                    x1,
                    y1,
                    x2,
                    y2,
                    transmissionPart,
                    rng
                );


            if (anchor == null)
                return false;


            minDistance =
                Math.Max(
                    2,
                    minDistance
                );


            maxDistance =
                Math.Max(
                    minDistance,
                    maxDistance
                );


            int[][] directions =
            {
                new int[] {  1,  0 },
                new int[] { -1,  0 },
                new int[] {  0,  1 },
                new int[] {  0, -1 }
            };


            List<GateServicePlan> plans =
                new List<GateServicePlan>();


            for (
                int directionIndex = 0;
                directionIndex <
                    directions.Length;
                directionIndex++
            )
            {
                int dx =
                    directions[
                        directionIndex
                    ][0];

                int dy =
                    directions[
                        directionIndex
                    ][1];


                for (
                    int distance =
                        minDistance;
                    distance <=
                        maxDistance;
                    distance++
                )
                {
                    Cell machineCell =
                        Z.GetCell(
                            anchor.X +
                                dx *
                                distance,

                            anchor.Y +
                                dy *
                                distance
                        );


                    if (
                        !IsGateDressingCellAvailable(
                            Z,
                            machineCell,
                            x1,
                            y1,
                            x2,
                            y2,
                            anchors
                        )
                    )
                    {
                        continue;
                    }


                    GateServicePlan plan =
                        new GateServicePlan
                        {
                            AnchorCell =
                                anchor,

                            MachineCell =
                                machineCell,

                            MachineInsidePrefab =
                                CellInsideRectangle(
                                    machineCell,
                                    x1,
                                    y1,
                                    x2,
                                    y2
                                )
                        };


                    bool clear =
                        true;


                    //
                    // Every intervening square becomes conduit.
                    //
                    // The first square is directly adjacent to the gate,
                    // which is the visual/mechanical relationship we want.
                    //
                    for (
                        int step = 1;
                        step < distance;
                        step++
                    )
                    {
                        Cell conduitCell =
                            Z.GetCell(
                                anchor.X +
                                    dx *
                                    step,

                                anchor.Y +
                                    dy *
                                    step
                            );


                        if (
                            !IsGateDressingCellAvailable(
                                Z,
                                conduitCell,
                                x1,
                                y1,
                                x2,
                                y2,
                                anchors
                            )
                        )
                        {
                            clear =
                                false;

                            break;
                        }


                        plan.ConduitCells.Add(
                            conduitCell
                        );
                    }


                    if (
                        clear &&
                        plan.ConduitCells.Count > 0
                    )
                    {
                        plans.Add(
                            plan
                        );
                    }
                }
            }


            if (plans.Count == 0)
            {
                //
                // Straight service runs are preferred because they read
                // cleanly as intentional gate infrastructure.
                //
                // If the surrounding architecture does not permit one,
                // fall back to a short bent route through any legal
                // prefab/open cells. Portal infrastructure is allowed to
                // look somewhat improvised and disorderly.
                //
                return
                    TryBuildFlexibleGateServiceSpine(
                        Z,
                        anchor,
                        x1,
                        y1,
                        x2,
                        y2,
                        anchors,
                        rng,
                        conduitBlueprint,
                        machineBlueprint,
                        networkCells
                    );
            }


            //
            // Prefer putting the service equipment on the actual painted
            // gate platform when a valid position exists.
            //
            // This makes the machinery read as part of the installation
            // rather than as unrelated clutter outside it.
            //
            List<GateServicePlan> insidePlans =
                new List<GateServicePlan>();


            foreach (
                GateServicePlan plan
                in plans
            )
            {
                if (
                    plan != null &&
                    plan.MachineInsidePrefab
                )
                {
                    insidePlans.Add(
                        plan
                    );
                }
            }


            List<GateServicePlan> choicePool =
                insidePlans.Count > 0 &&
                rng.Next(100) < 75
                    ? insidePlans
                    : plans;


            GateServicePlan chosen =
                choicePool[
                    rng.Next(
                        choicePool.Count
                    )
                ];


            //
            // Place the endpoint machine first. If its blueprint somehow
            // fails, do not leave an orphan conduit run.
            //
            if (
                !TryPlaceGateDressingObject(
                    chosen.MachineCell,
                    machineBlueprint
                )
            )
            {
                return false;
            }


            foreach (
                Cell conduitCell
                in chosen.ConduitCells
            )
            {
                if (
                    !TryPlaceGateDressingObject(
                        conduitCell,
                        conduitBlueprint
                    )
                )
                {
                    continue;
                }


                networkCells.Add(
                    conduitCell
                );
            }


            //
            // Let later support machinery cluster beside both the conduit
            // and the main endpoint machine.
            //
            networkCells.Add(
                chosen.MachineCell
            );


            return true;
        }

                private bool TryBuildFlexibleGateServiceSpine(
            Zone Z,
            Cell anchor,
            int x1,
            int y1,
            int x2,
            int y2,
            List<Location2D> anchors,
            System.Random rng,
            string conduitBlueprint,
            string machineBlueprint,
            List<Cell> networkCells
        )
        {
            if (
                Z == null ||
                anchor == null ||
                rng == null ||
                conduitBlueprint.IsNullOrEmpty() ||
                machineBlueprint.IsNullOrEmpty() ||
                networkCells == null
            )
            {
                return false;
            }


            //
            // This is deliberately local. We are dressing one gate,
            // not building another zone-wide utility network.
            //
            const int MaxRouteLength =
                10;


            bool[,] visited =
                new bool[
                    Z.Width,
                    Z.Height
                ];


            int[,] distance =
                new int[
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
                    distance[
                        x,
                        y
                    ] = -1;


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


            Queue<Cell> queue =
                new Queue<Cell>();


            int[][] directions =
            {
                new int[] {  1,  0 },
                new int[] { -1,  0 },
                new int[] {  0,  1 },
                new int[] {  0, -1 }
            };


            //
            // Seed from legal cells immediately beside the actual gate
            // connection. The anchor itself remains untouched.
            //
            foreach (
                int[] direction
                in directions
            )
            {
                int nx =
                    anchor.X +
                    direction[0];

                int ny =
                    anchor.Y +
                    direction[1];


                Cell neighbor =
                    Z.GetCell(
                        nx,
                        ny
                    );


                if (
                    !IsGateDressingCellAvailable(
                        Z,
                        neighbor,
                        x1,
                        y1,
                        x2,
                        y2,
                        anchors
                    )
                )
                {
                    continue;
                }


                visited[
                    nx,
                    ny
                ] = true;


                distance[
                    nx,
                    ny
                ] = 1;


                parentX[
                    nx,
                    ny
                ] =
                    anchor.X;


                parentY[
                    nx,
                    ny
                ] =
                    anchor.Y;


                queue.Enqueue(
                    neighbor
                );
            }


            while (
                queue.Count > 0
            )
            {
                Cell current =
                    queue.Dequeue();


                if (current == null)
                    continue;


                int currentDistance =
                    distance[
                        current.X,
                        current.Y
                    ];


                if (
                    currentDistance >=
                    MaxRouteLength
                )
                {
                    continue;
                }


                //
                // Shuffle direction order so equally valid fallback routes
                // do not all bend in the same deterministic way.
                //
                int[][] shuffled =
                {
                    new int[] {  1,  0 },
                    new int[] { -1,  0 },
                    new int[] {  0,  1 },
                    new int[] {  0, -1 }
                };


                for (
                    int i =
                        shuffled.Length - 1;
                    i > 0;
                    i--
                )
                {
                    int swapIndex =
                        rng.Next(
                            i + 1
                        );


                    int[] temp =
                        shuffled[
                            i
                        ];


                    shuffled[
                        i
                    ] =
                        shuffled[
                            swapIndex
                        ];


                    shuffled[
                        swapIndex
                    ] =
                        temp;
                }


                foreach (
                    int[] direction
                    in shuffled
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
                        !IsGateDressingCellAvailable(
                            Z,
                            next,
                            x1,
                            y1,
                            x2,
                            y2,
                            anchors
                        )
                    )
                    {
                        continue;
                    }


                    visited[
                        nx,
                        ny
                    ] = true;


                    distance[
                        nx,
                        ny
                    ] =
                        currentDistance +
                        1;


                    parentX[
                        nx,
                        ny
                    ] =
                        current.X;


                    parentY[
                        nx,
                        ny
                    ] =
                        current.Y;


                    queue.Enqueue(
                        next
                    );
                }
            }


            //
            // Any reachable cell at least two steps from the gate can hold
            // the machine. Two steps guarantees at least one visible conduit
            // cell between the gate and its service machinery.
            //
            List<Cell> insideCandidates =
                new List<Cell>();


            List<Cell> outsideCandidates =
                new List<Cell>();


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
                    int d =
                        distance[
                            x,
                            y
                        ];


                    if (
                        d < 2 ||
                        d >
                            MaxRouteLength
                    )
                    {
                        continue;
                    }


                    Cell candidate =
                        Z.GetCell(
                            x,
                            y
                        );


                    if (candidate == null)
                        continue;


                    if (
                        CellInsideRectangle(
                            candidate,
                            x1,
                            y1,
                            x2,
                            y2
                        )
                    )
                    {
                        insideCandidates.Add(
                            candidate
                        );
                    }
                    else
                    {
                        outsideCandidates.Add(
                            candidate
                        );
                    }
                }
            }


            if (
                insideCandidates.Count == 0 &&
                outsideCandidates.Count == 0
            )
            {
                return false;
            }


            //
            // Prefer machinery on the painted gate platform when possible,
            // but allow it outside when the prefab is crowded.
            //
            List<Cell> choicePool;


            if (
                insideCandidates.Count > 0 &&
                (
                    outsideCandidates.Count == 0 ||
                    rng.Next(100) < 75
                )
            )
            {
                choicePool =
                    insideCandidates;
            }
            else
            {
                choicePool =
                    outsideCandidates;
            }


            Cell machineCell =
                choicePool[
                    rng.Next(
                        choicePool.Count
                    )
                ];


            //
            // Reconstruct the route backward from the machine toward the
            // gate. The machine cell itself is not conduit.
            //
            List<Cell> route =
                new List<Cell>();


            int px =
                machineCell.X;

            int py =
                machineCell.Y;


            while (true)
            {
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
                    return false;
                }


                //
                // Reached the actual gate connection.
                //
                if (
                    nextX ==
                        anchor.X &&
                    nextY ==
                        anchor.Y
                )
                {
                    break;
                }


                Cell conduitCell =
                    Z.GetCell(
                        nextX,
                        nextY
                    );


                if (conduitCell == null)
                    return false;


                route.Add(
                    conduitCell
                );


                px =
                    nextX;

                py =
                    nextY;
            }


            if (route.Count == 0)
                return false;


            //
            // Place the machine first so a failed object creation cannot
            // leave an orphaned cable or pipe.
            //
            if (
                !TryPlaceGateDressingObject(
                    machineCell,
                    machineBlueprint
                )
            )
            {
                return false;
            }


            foreach (
                Cell conduitCell
                in route
            )
            {
                if (
                    TryPlaceGateDressingObject(
                        conduitCell,
                        conduitBlueprint
                    )
                )
                {
                    networkCells.Add(
                        conduitCell
                    );
                }
            }


            networkCells.Add(
                machineCell
            );


            return true;
        }


        private Cell FindGateServiceAnchor(
            Zone Z,
            int x1,
            int y1,
            int x2,
            int y2,
            string transmissionPart,
            System.Random rng
        )
        {
            if (
                Z == null ||
                rng == null
            )
            {
                return null;
            }


            List<Cell> poweredGateCells =
                new List<Cell>();


            List<Cell> transmissionCells =
                new List<Cell>();


            List<Cell> gateCells =
                new List<Cell>();


            //
            // Final visual fallback for ruined/dead gate prefabs that no
            // longer contain a working TeleportGate or transmission part.
            //
            // These cells contain some actual structural/object content from
            // the prefab, rather than merely painted floor.
            //
            List<Cell> structuralCells =
                new List<Cell>();


            int centerX =
                (
                    x1 +
                    x2
                ) / 2;


            int centerY =
                (
                    y1 +
                    y2
                ) / 2;


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


                    bool hasGate =
                        cell.GetFirstObjectWithPart(
                            "TeleportGate"
                        ) != null;


                    bool hasTransmission =
                        !transmissionPart
                            .IsNullOrEmpty() &&
                        cell.GetFirstObjectWithPart(
                            transmissionPart
                        ) != null;


                    bool hasStructure =
                        cell.IsSolid() ||
                        SubterraneanSites
                            .SubterraneanSitesEPPlacement
                            .HasMeaningfulOccupant(
                                cell
                            );


                    //
                    // Best case: this is the actual powered gate.
                    //
                    if (
                        hasGate &&
                        hasTransmission
                    )
                    {
                        poweredGateCells.Add(
                            cell
                        );
                    }
                    else
                    {
                        if (hasTransmission)
                        {
                            transmissionCells.Add(
                                cell
                            );
                        }


                        if (hasGate)
                        {
                            gateCells.Add(
                                cell
                            );
                        }
                    }


                    if (hasStructure)
                    {
                        structuralCells.Add(
                            cell
                        );
                    }
                }
            }


            if (
                poweredGateCells.Count > 0
            )
            {
                return
                    poweredGateCells[
                        rng.Next(
                            poweredGateCells.Count
                        )
                    ];
            }


            if (
                transmissionCells.Count > 0
            )
            {
                return
                    transmissionCells[
                        rng.Next(
                            transmissionCells.Count
                        )
                    ];
            }


            if (
                gateCells.Count > 0
            )
            {
                return
                    gateCells[
                        rng.Next(
                            gateCells.Count
                        )
                    ];
            }


            //
            // Ruined gates may no longer contain either a live TeleportGate
            // part or a working transmission component.
            //
            // In that case, attach the remaining service infrastructure to
            // actual ruin structure. Prefer something near the center so the
            // pipe/wire visually reads as belonging to the gate installation.
            //
            if (
                structuralCells.Count > 0
            )
            {
                int bestDistance =
                    int.MaxValue;


                List<Cell> best =
                    new List<Cell>();


                foreach (
                    Cell cell
                    in structuralCells
                )
                {
                    if (cell == null)
                        continue;


                    int distance =
                        Math.Abs(
                            cell.X -
                            centerX
                        ) +
                        Math.Abs(
                            cell.Y -
                            centerY
                        );


                    if (
                        distance <
                        bestDistance
                    )
                    {
                        bestDistance =
                            distance;


                        best.Clear();


                        best.Add(
                            cell
                        );
                    }
                    else if (
                        distance ==
                        bestDistance
                    )
                    {
                        best.Add(
                            cell
                        );
                    }
                }


                if (
                    best.Count > 0
                )
                {
                    return
                        best[
                            rng.Next(
                                best.Count
                            )
                        ];
                }
            }


            //
            // Extreme fallback: even an RPM made entirely from painted tiles
            // can still receive a little ruined service infrastructure.
            //
            return
                Z.GetCell(
                    centerX,
                    centerY
                );
        }


        private bool CellInsideRectangle(
            Cell cell,
            int x1,
            int y1,
            int x2,
            int y2
        )
        {
            if (cell == null)
                return false;


            return
                cell.X >= x1 &&
                cell.X <= x2 &&
                cell.Y >= y1 &&
                cell.Y <= y2;
        }

        // ================================================================
        // MACHINERY
        // ================================================================

        private bool TryPlaceMachineAdjacentToNetwork(
            Zone Z,
            int x1,
            int y1,
            int x2,
            int y2,
            List<Location2D> anchors,
            System.Random rng,
            List<Cell> networkCells,
            string blueprint
        )
        {
            if (
                Z == null ||
                rng == null ||
                networkCells == null ||
                networkCells.Count == 0 ||
                blueprint.IsNullOrEmpty()
            )
            {
                return false;
            }


            List<Cell> candidates =
                new List<Cell>();


            int[][] directions =
            {
                new int[] {  1,  0 },
                new int[] { -1,  0 },
                new int[] {  0,  1 },
                new int[] {  0, -1 }
            };


            foreach (
                Cell networkCell
                in networkCells
            )
            {
                if (networkCell == null)
                    continue;


                foreach (
                    int[] direction
                    in directions
                )
                {
                    Cell candidate =
                        Z.GetCell(
                            networkCell.X +
                                direction[0],

                            networkCell.Y +
                                direction[1]
                        );


                    if (
                        !IsGateDressingCellAvailable(
                            Z,
                            candidate,
                            x1,
                            y1,
                            x2,
                            y2,
                            anchors
                        )
                    )
                    {
                        continue;
                    }


                    if (
                        !candidates.Contains(
                            candidate
                        )
                    )
                    {
                        candidates.Add(
                            candidate
                        );
                    }
                }
            }


            if (candidates.Count == 0)
                return false;


            Cell chosen =
                candidates[
                    rng.Next(
                        candidates.Count
                    )
                ];


            if (
                !TryPlaceGateDressingObject(
                    chosen,
                    blueprint
                )
            )
            {
                return false;
            }


            //
            // Later machinery can cluster against this machine as well.
            // Since all selected machines contain the appropriate power
            // transmission part, this can extend the little service network.
            //
            networkCells.Add(
                chosen
            );


            return true;
        }


        // ================================================================
        // COMMON PLACEMENT
        // ================================================================

        private bool TryPlaceGateDressingObject(
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
                GameObjectFactory
                    .Factory
                    .CreateObject(
                        blueprint
                    );


            if (obj == null)
                return false;


            cell.AddObject(
                obj
            );


            SubterraneanSites
                .SubterraneanSitesEPReservations
                .ClaimCell(
                    cell.ParentZone,
                    cell
                );


            return true;
        }

        private bool IsGateDressingCellAvailable(
            Zone Z,
            Cell cell,
            int x1,
            int y1,
            int x2,
            int y2,
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


            bool insideGatePrefab =
                CellInsideRectangle(
                    cell,
                    x1,
                    y1,
                    x2,
                    y2
                );


            //
            // Preserve the absolute zone shell.
            //
            if (
                cell.X < BorderClearance ||
                cell.Y < BorderClearance ||
                cell.X >=
                    Z.Width -
                    BorderClearance ||
                cell.Y >=
                    Z.Height -
                    BorderClearance
            )
            {
                return false;
            }


            //
            // Outside the gate's own rectangle, obey normal semantic
            // reservations and shared open geometry.
            //
            // Inside its own 9x7 footprint, the reservation belongs to THIS
            // Portal installation, so service machinery is explicitly allowed
            // to use otherwise-empty painted prefab cells.
            //
            if (!insideGatePrefab)
            {
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
            }


            //
            // Never cover an actual solid piece of the gate or another wall.
            //
            if (
                !cell.IsEmptyOfSolid()
            )
            {
                return false;
            }


            //
            // Painted prefab floor is fine. Actual furniture, machinery,
            // creatures, containers, etc. are not.
            //
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


            if (
                cell.HasSpawnBlocker()
            )
            {
                return false;
            }


            if (
                cell.GetFirstObjectWithPart(
                    "Door"
                ) != null
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


            return true;
        }

        private bool FootprintAvailable(
            Zone Z,
            int x1,
            int y1,
            int width,
            int height,
            List<Location2D> anchors
        )
        {
            if (
                Z == null ||
                width <= 0 ||
                height <= 0
            )
            {
                return false;
            }


            int x2 =
                x1 +
                width -
                1;

            int y2 =
                y1 +
                height -
                1;


            //
            // First respect semantic ownership from earlier builders.
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
                    // C4 structural placeholders are explicitly allowed:
                    // the prefab is permitted to excavate them.
                    //
                    // A solid object that is NOT one of our abstract
                    // placeholders is real content and should survive.
                    //
                    if (
                        cell.IsSolid() &&
                        !SubterraneanSites
                            .SubterraneanSitesEPGeometry
                            .IsAnyPlaceholder(
                                cell
                            )
                    )
                    {
                        return false;
                    }


                    //
                    // Preserve creatures, furniture, containers, etc.
                    //
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
                    // Doors are C4 architecture and are not necessarily
                    // caught by the generic meaningful-occupant test.
                    //
                    if (
                        cell.GetFirstObjectWithPart(
                            "Door"
                        ) != null
                    )
                    {
                        return false;
                    }


                    //
                    // Defensive protection for explicit transition objects.
                    //
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
                    // Do not let the later destructive vertical hole eat
                    // part of this installation.
                    //
                    if (
                        SubterraneanSites
                            .SubterraneanSitesEPPlacement
                            .IsNearAnyAnchor(
                                x,
                                y,
                                anchors,
                                TransitionExclusionRadius
                            )
                    )
                    {
                        return false;
                    }
                }
            }


            //
            // A prefab may carve deeply into structural mass, but it must
            // overlap or touch the already-open architecture so it does not
            // become an isolated buried installation.
            //
            return
                HasOpenConnection(
                    Z,
                    x1,
                    y1,
                    x2,
                    y2
                );
        }
        


        private bool HasOpenConnection(
            Zone Z,
            int x1,
            int y1,
            int x2,
            int y2
        )
        {
            //
            // Best case: some part of the footprint already overlaps
            // traversable/open geometry.
            //
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


                    if (
                        IsExistingOpenCell(
                            cell
                        )
                    )
                    {
                        return true;
                    }
                }
            }


            //
            // Otherwise allow excavation immediately beside existing
            // architecture. ClearWalls() across the footprint will join
            // the new cavity to that open edge.
            //
            for (
                int x = x1 - 1;
                x <= x2 + 1;
                x++
            )
            {
                for (
                    int y = y1 - 1;
                    y <= y2 + 1;
                    y++
                )
                {
                    //
                    // Only inspect the one-cell ring outside the footprint.
                    //
                    if (
                        x >= x1 &&
                        x <= x2 &&
                        y >= y1 &&
                        y <= y2
                    )
                    {
                        continue;
                    }


                    if (
                        x < 0 ||
                        y < 0 ||
                        x >= Z.Width ||
                        y >= Z.Height
                    )
                    {
                        continue;
                    }


                    Cell cell =
                        Z.GetCell(
                            x,
                            y
                        );


                    if (
                        IsExistingOpenCell(
                            cell
                        )
                    )
                    {
                        return true;
                    }
                }
            }


            return false;
        }


        private bool IsExistingOpenCell(
            Cell cell
        )
        {
            return
                cell != null &&
                SubterraneanSites
                    .SubterraneanSitesEPGeometry
                    .IsOpenGeometryCell(
                        cell
                    ) &&
                !cell.IsSolid();
        }
    }

    /// <summary>
    /// Portal Category-2 Aloe Porta network.
    ///
    /// Vanilla Aloe Porta finds another Aloe Porta in the same zone with
    /// the same GroupKey. Giving every generated pair its own GroupKey
    /// therefore turns each pair into an independent two-endpoint portal.
    ///
    /// Pair selection is Portal-specific, but ordinary cell selection,
    /// spacing, vertical-anchor avoidance and semantic reservations remain
    /// owned by the shared EP placement systems.
    /// </summary>
    public class SubterraneanSitesPortalAloePortaPairs :
        ZoneBuilderSandbox
    {
        public int MinPairs =
            8;

        public int MaxPairs =
            16;

        //
        // Chebyshev distance between the two members of one pair.
        //
        public int MinPairSeparation =
            8;

        //
        // Protect the later EP vertical-transition holes.
        //
        public int TransitionExclusionRadius =
            4;


        private const string AloeBlueprint =
            "Aloe Porta";

        private const string ThemeKey =
            "Portal";


        public bool BuildZone(
            Zone Z
        )
        {
            if (Z == null)
                return true;


            MinPairs =
                Math.Max(
                    0,
                    MinPairs
                );

            MaxPairs =
                Math.Max(
                    MinPairs,
                    MaxPairs
                );

            MinPairSeparation =
                Math.Max(
                    1,
                    MinPairSeparation
                );

            TransitionExclusionRadius =
                Math.Max(
                    0,
                    TransitionExclusionRadius
                );


            List<Location2D> anchors =
                SubterraneanSites
                    .SubterraneanSitesEPVerticalTransitions
                    .GetVerticalAnchors(
                        Z.ZoneID
                    );


            int seed =
                XRLCore.Core.Game.GetWorldSeed(
                    "SubterraneanSites:PortalAloePorta:" +
                    Z.ZoneID
                );


            System.Random rng =
                new System.Random(
                    seed
                );


            int desiredPairs =
                rng.Next(
                    MinPairs,
                    MaxPairs + 1
                );


            for (
                int pairIndex = 0;
                pairIndex < desiredPairs;
                pairIndex++
            )
            {
                //
                // Once we can no longer find one legal complete pair,
                // later pairs will not have more space than this one did.
                //
                if (
                    !TryPlacePair(
                        Z,
                        anchors,
                        rng,
                        pairIndex
                    )
                )
                {
                    break;
                }
            }


            return true;
        }


        private bool TryPlacePair(
            Zone Z,
            List<Location2D> anchors,
            System.Random rng,
            int pairIndex
        )
        {
            if (
                Z == null ||
                rng == null
            )
            {
                return false;
            }


            //
            // Collect currently legal first endpoints.
            //
            // Previous Portal infrastructure and previously placed Aloe Porta
            // pairs are already represented in the shared reservation map.
            //
            List<Cell> firstCandidates =
                SubterraneanSites
                    .SubterraneanSitesEPPlacement
                    .CollectCells(
                        Z,
                        delegate(Cell candidate)
                        {
                            return
                                IsCandidate(
                                    Z,
                                    candidate,
                                    anchors
                                );
                        }
                    );


            //
            // Try candidate first endpoints in random order.
            //
            // Removing rejected candidates avoids repeatedly picking a cell
            // that cannot form a sufficiently separated pair.
            //
            while (
                firstCandidates.Count > 0
            )
            {
                int firstIndex =
                    rng.Next(
                        firstCandidates.Count
                    );


                Cell firstCell =
                    firstCandidates[
                        firstIndex
                    ];


                firstCandidates.RemoveAt(
                    firstIndex
                );


                if (firstCell == null)
                    continue;


                HashSet<Cell> firstPatch =
                    new HashSet<Cell>
                    {
                        firstCell
                    };


                //
                // The shared near-patch helper already measures Chebyshev
                // distance, so a one-cell "patch" gives us exactly the
                // endpoint-separation behavior required here.
                //
                Cell secondCell =
                    SubterraneanSites
                        .SubterraneanSitesEPPlacement
                        .PickRandomCellNearPatch(
                            Z,
                            firstPatch,
                            MinPairSeparation,
                            Math.Max(
                                Z.Width,
                                Z.Height
                            ),
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


                if (secondCell == null)
                    continue;


                return
                    PlacePair(
                        Z,
                        firstCell,
                        secondCell,
                        pairIndex
                    );
            }


            return false;
        }


        private bool PlacePair(
            Zone Z,
            Cell firstCell,
            Cell secondCell,
            int pairIndex
        )
        {
            if (
                Z == null ||
                firstCell == null ||
                secondCell == null
            )
            {
                return false;
            }


            GameObject firstAloe =
                GameObjectFactory
                    .Factory
                    .CreateObject(
                        AloeBlueprint
                    );


            GameObject secondAloe =
                GameObjectFactory
                    .Factory
                    .CreateObject(
                        AloeBlueprint
                    );


            if (
                firstAloe == null ||
                secondAloe == null
            )
            {
                return false;
            }


            XRL.World.Parts.AloePorta firstPart =
                firstAloe.GetPart<
                    XRL.World.Parts.AloePorta
                >();


            XRL.World.Parts.AloePorta secondPart =
                secondAloe.GetPart<
                    XRL.World.Parts.AloePorta
                >();


            //
            // Do not ever place a broken half-pair.
            //
            if (
                firstPart == null ||
                secondPart == null
            )
            {
                return false;
            }


            //
            // Vanilla Aloe Porta searches by GroupKey.
            //
            // Zone ID + pair index makes this pair unique from every
            // other generated Aloe Porta pair.
            //
            string groupKey =
                "SubterraneanSitesPortalPorta_" +
                Z.ZoneID +
                "_" +
                pairIndex.ToString();


            firstPart.GroupKey =
                groupKey;

            secondPart.GroupKey =
                groupKey;


            //
            // These are extradimensional Portal flora, but they are not
            // ordinary EP denizens and should not receive denizen combat
            // adaptation.
            //
            SubterraneanSites
                .SubterraneanSitesDimensionEngine
                .ApplyDecorationDimensionIdentity(
                    firstAloe,
                    ThemeKey
                );


            SubterraneanSites
                .SubterraneanSitesDimensionEngine
                .ApplyDecorationDimensionIdentity(
                    secondAloe,
                    ThemeKey
                );


            firstCell.AddObject(
                firstAloe
            );

            secondCell.AddObject(
                secondAloe
            );


            //
            // Each Aloe Porta is discrete functional C2 content.
            //
            // Reserve only the actual cells, not a corridor or bounding box
            // between the pair.
            //
            SubterraneanSites
                .SubterraneanSitesEPReservations
                .ClaimCell(
                    Z,
                    firstCell
                );


            SubterraneanSites
                .SubterraneanSitesEPReservations
                .ClaimCell(
                    Z,
                    secondCell
                );


            return true;
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
            // Aloe Porta belongs in already-open C4 architecture.
            // Unlike the gate/rippler prefabs, it does not excavate.
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


            //
            // Respect earlier C1/C2 semantic ownership.
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
                !cell.IsEmptyOfSolid()
            )
            {
                return false;
            }


            //
            // Preserve creatures, furniture, containers and other
            // meaningful discrete content.
            //
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
            // Do not plant a portal directly in an architectural doorway.
            //
            if (
                cell.GetFirstObjectWithPart(
                    "Door"
                ) != null
            )
            {
                return false;
            }


            //
            // Defensive protection for explicit transition objects.
            //
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
            // The actual holes are installed later, but their positions
            // are already planned.
            //
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


            return true;
        }
    }



    /// <summary>
    /// Portal Category-4 floor adapter.
    ///
    /// Portal owns the paint recipe.
    /// The shared EP floor system owns universal underground/scar
    /// scope and late substrate application.
    /// </summary>
    public class SubterraneanSitesPortalFloor :
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
                            .SubterraneanSitesEPPortalTheme
                            .CreateFloorSpec(),
                        EntranceOnly != 0
                    );
        }
    }

        /// <summary>
    /// Portal Category 5.
    ///
    /// Abandoned extradimensional transit-station furnishings:
    /// recognizable furniture scenes plus irregular ruin debris.
    ///
    /// All furniture scenes use the same small irregular spatial pattern.
    /// Their identity comes from the objects selected for the scene rather
    /// than from separate miniature layout generators.
    /// </summary>
    public class SubterraneanSitesPortalDecorations :
        ZoneBuilderSandbox
    {

        public int EntranceOnly =
            0;

        public int MinSceneClusters =
            5;

        public int MaxSceneClusters =
            9;

        public int MinScenePatchCells =
            7;

        public int MaxScenePatchCells =
            11;

        public int MinObjectsPerScene =
            3;

        public int MaxObjectsPerScene =
            6;

        public int MinSceneSeparation =
            6;


        public int MinDebrisPatches =
            2;

        public int MaxDebrisPatches =
            4;

        public int MinDebrisCells =
            4;

        public int MaxDebrisCells =
            8;


        private const int
            TransitionExclusionRadius =
                4;


        private const string
            EmptyMulticabinetBlueprint =
                "SubterraneanSitesPortalEmptyMulticabinet";


        private static readonly string[]
            WaitingPool =
        {
            "Bench",
            "Bench",
            "Bench",
            "Stool",
            "Eater Sign 1",
            "Eater Sign 2",
            "Clockthing",
            "Full-Spectrum Techlight"
        };


        private static readonly string[]
            InformationPool =
        {
            "SubterraneanSitesPortalEmptyDesk",
            "SubterraneanSitesPortalEmptyDesk",
            "SubterraneanSitesPortalEmptyTable",
            "Book Table",
            "Stool",
            "SubterraneanSitesPortalEmptyUndergroundBookshelf",
            EmptyMulticabinetBlueprint,
            "Eater Sign 1",
            "Eater Sign 2",
            "Clockthing",
            "Full-Spectrum Techlight"
        };


        private static readonly string[]
            MaintenancePool =
        {
            "SubterraneanSitesPortalEmptyWorkbench",
            "SubterraneanSitesPortalEmptyWorkbench",
            "SubterraneanSitesPortalEmptyWorkbench",
            "SubterraneanSitesPortalEmptyTable",
            "Stool",
            EmptyMulticabinetBlueprint,
            EmptyMulticabinetBlueprint,
            "Phasic Screw"
        };


        private static readonly string[]
            LoungePool =
        {
            "Wood-Carved Chair",
            "Wood-Carved Chair",
            "SubterraneanSitesPortalEmptyWoodCarvedBookshelf",
            "Book Table",
            "Light Sculpture",
            "Hookah",
            "Full-Spectrum Techlight"
        };


        private static readonly string[]
            CampPool =
        {
            "Bedroll",
            "Bedroll",
            "Bedroll",
            "Woven Basket",
            "Stool",
            "Bench",
            "SubterraneanSitesPortalEmptyTable",
            "Hookah"
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
                    "SubterraneanSites:PortalDecorations:" +
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


            List<HashSet<Cell>> scenes =
                PlaceSceneClusters(
                    Z,
                    anchors,
                    rng
                );


            PlaceDebrisPatches(
                Z,
                anchors,
                scenes,
                rng
            );


            return true;
        }


        // ================================================================
        // FURNITURE SCENES
        // ================================================================

        private List<HashSet<Cell>>
            PlaceSceneClusters(
                Zone Z,
                List<Location2D> anchors,
                System.Random rng
            )
        {
            List<HashSet<Cell>> result =
                new List<HashSet<Cell>>();


            List<Cell> usedAnchors =
                new List<Cell>();


            int desired =
                EntranceOnly != 0
                    ? rng.Next(
                        3,
                        4
                    )
                    : rng.Next(
                        MinSceneClusters,
                        MaxSceneClusters + 1
                    );


            for (
                int sceneIndex = 0;
                sceneIndex < desired;
                sceneIndex++
            )
            {
                List<Cell> candidates =
                    SubterraneanSites
                        .SubterraneanSitesEPPlacement
                        .CollectCells(
                            Z,
                            delegate(Cell cell)
                            {
                                return
                                    IsBroadSceneCell(
                                        Z,
                                        cell,
                                        anchors
                                    ) &&
                                    FarEnoughFromCells(
                                        cell,
                                        usedAnchors,
                                        MinSceneSeparation
                                    );
                            }
                        );


                if (candidates.Count == 0)
                    break;


                Cell seed =
                    candidates[
                        rng.Next(
                            candidates.Count
                        )
                    ];


                int patchTarget =
                    rng.Next(
                        MinScenePatchCells,
                        MaxScenePatchCells + 1
                    );


                HashSet<Cell> patch =
                    SubterraneanSites
                        .SubterraneanSitesEPPlacement
                        .GrowPatch(
                            seed,
                            patchTarget,
                            delegate(Cell cell)
                            {
                                return
                                    IsBroadSceneCell(
                                        Z,
                                        cell,
                                        anchors
                                    );
                            },
                            rng
                        );


                if (patch.Count == 0)
                    continue;


                List<Cell> cells =
                    new List<Cell>(
                        patch
                    );


                ShuffleCells(
                    cells,
                    rng
                );


                int sceneType =
                    PickSceneType(
                        rng
                    );


                int objectCount =
                    rng.Next(
                        MinObjectsPerScene,
                        MaxObjectsPerScene + 1
                    );


                objectCount =
                    Math.Min(
                        objectCount,
                        cells.Count
                    );


                int placed =
                    0;


                for (
                    int i = 0;
                    i < objectCount;
                    i++
                )
                {
                    string blueprint =
                        i == 0
                            ? GetPrimarySceneBlueprint(
                                sceneType,
                                rng
                            )
                            : PickSceneBlueprint(
                                sceneType,
                                rng
                            );


                    if (
                        PlaceAndClaim(
                            Z,
                            cells[i],
                            blueprint
                        )
                    )
                    {
                        placed++;
                    }
                }


                if (placed > 0)
                {
                    usedAnchors.Add(
                        seed
                    );


                    //
                    // Keep the entire intended scene footprint.
                    //
                    // Debris can later select unused cells in or immediately
                    // around this patch and make the station look damaged
                    // without overwriting the furniture itself.
                    //
                    result.Add(
                        patch
                    );
                }
            }


            return result;
        }


        private int PickSceneType(
            System.Random rng
        )
        {
            int roll =
                rng.Next(
                    100
                );


            //
            // Weight the ordinary station functions most strongly.
            //
            if (roll < 35)
                return 0; // waiting

            if (roll < 60)
                return 1; // information / office

            if (roll < 78)
                return 2; // maintenance

            if (roll < 90)
                return 3; // lounge

            return 4;     // stranded-traveler camp
        }


        private string GetPrimarySceneBlueprint(
            int sceneType,
            System.Random rng
        )
        {
            switch (sceneType)
            {
                case 0:
                    return "Bench";

                case 1:
                    return
                        rng.Next(2) == 0
                            ? "SubterraneanSitesPortalEmptyDesk"
                            : "Book Table";

                case 2:
                    return
                        "SubterraneanSitesPortalEmptyWorkbench";

                case 3:
                    return "Wood-Carved Chair";

                case 4:
                    return "Bedroll";

                default:
                    return "Bench";
            }
        }


        private string PickSceneBlueprint(
            int sceneType,
            System.Random rng
        )
        {
            string[] pool;


            switch (sceneType)
            {
                case 0:
                    pool =
                        WaitingPool;
                    break;

                case 1:
                    pool =
                        InformationPool;
                    break;

                case 2:
                    pool =
                        MaintenancePool;
                    break;

                case 3:
                    pool =
                        LoungePool;
                    break;

                case 4:
                    pool =
                        CampPool;
                    break;

                default:
                    pool =
                        WaitingPool;
                    break;
            }


            return
                pool[
                    rng.Next(
                        pool.Length
                    )
                ];
        }


        // ================================================================
        // RUIN DEBRIS
        // ================================================================

        private void PlaceDebrisPatches(
            Zone Z,
            List<Location2D> anchors,
            List<HashSet<Cell>> scenes,
            System.Random rng
        )
        {
            int desired =
                EntranceOnly != 0
                    ? rng.Next(
                        1,
                        3
                    )
                    : rng.Next(
                        MinDebrisPatches,
                        MaxDebrisPatches + 1
                    );


            for (
                int patchIndex = 0;
                patchIndex < desired;
                patchIndex++
            )
            {
                Cell seed =
                    null;


                //
                // Usually let a ruin patch intrude into or immediately beside
                // an existing furniture scene.
                //
                // Sometimes put it elsewhere in the station instead.
                //
                if (
                    scenes != null &&
                    scenes.Count > 0 &&
                    rng.Next(100) < 65
                )
                {
                    HashSet<Cell> scene =
                        scenes[
                            rng.Next(
                                scenes.Count
                            )
                        ];


                    seed =
                        SubterraneanSites
                            .SubterraneanSitesEPPlacement
                            .PickRandomCellNearPatch(
                                Z,
                                scene,
                                0,
                                2,
                                delegate(Cell cell)
                                {
                                    return
                                        IsDebrisCell(
                                            Z,
                                            cell,
                                            anchors
                                        );
                                },
                                rng
                            );
                }


                if (seed == null)
                {
                    seed =
                        SubterraneanSites
                            .SubterraneanSitesEPPlacement
                            .PickRandomCell(
                                Z,
                                delegate(Cell cell)
                                {
                                    return
                                        IsDebrisCell(
                                            Z,
                                            cell,
                                            anchors
                                        );
                                },
                                rng
                            );
                }


                if (seed == null)
                    continue;


                int target =
                    EntranceOnly != 0
                        ? rng.Next(
                            3,
                            6
                        )
                        : rng.Next(
                            MinDebrisCells,
                            MaxDebrisCells + 1
                        );


                HashSet<Cell> patch =
                    SubterraneanSites
                        .SubterraneanSitesEPPlacement
                        .GrowPatch(
                            seed,
                            target,
                            delegate(Cell cell)
                            {
                                return
                                    IsDebrisCell(
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


                    int roll =
                        rng.Next(
                            100
                        );


                    string blueprint;


                    if (roll < 45)
                    {
                        blueprint =
                            "Rubble Grey";
                    }
                    else if (roll < 65)
                    {
                        blueprint =
                            "SmallBoulder Grey";
                    }
                    else
                    {
                        blueprint =
                            "Garbage";
                    }


                    //
                    // Loose ruin material deliberately does not claim the cell.
                    // This matches the lighter-weight debris behavior used by
                    // other EP themes.
                    //
                    cell.AddObject(
                        blueprint
                    );
                }
            }
        }


        // ================================================================
        // PLACEMENT RULES
        // ================================================================

        private bool CellInDecorationScope(
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


            if (EntranceOnly != 0)
            {
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


            return
                SubterraneanSites
                    .SubterraneanSitesEPGeometry
                    .IsOpenGeometryCell(
                        cell
                    );
        }

        private bool IsBroadSceneCell(
            Zone Z,
            Cell cell,
            List<Location2D> anchors
        )
        {
            if (
                !IsSceneCell(
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
                                IsSceneCell(
                                    Z,
                                    neighbor,
                                    anchors
                                );
                        },
                        1
                    );
        }


        private bool IsSceneCell(
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
                !CellInDecorationScope(
                    Z,
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


            if (
                !cell.IsEmptyOfSolid() ||
                cell.HasSpawnBlocker()
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


            if (
                IsTransitionObjectCell(
                    cell
                )
            )
            {
                return false;
            }


            if (
                cell.GetFirstObjectWithPart(
                    "Door"
                ) != null
            )
            {
                return false;
            }


            //
            // Underground content respects the planned vertical-transition
            // anchors. Entrance mode instead uses the scar's much larger
            // explicit hole exclusion in CellInDecorationScope().
            //
            if (
                EntranceOnly == 0 &&
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


            return true;
        }


        private bool IsDebrisCell(
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
                !CellInDecorationScope(
                    Z,
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


            if (
                !cell.IsEmptyOfSolid() ||
                cell.HasSpawnBlocker()
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


            if (
                IsTransitionObjectCell(
                    cell
                )
            )
            {
                return false;
            }


            //
            // Underground debris respects transition anchors.
            // Entrance debris is instead bounded by the scar/hole rules.
            //
            if (
                EntranceOnly == 0 &&
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


            return true;
        }


        private bool IsTransitionObjectCell(
            Cell cell
        )
        {
            if (cell == null)
                return true;


            return
                cell.HasObjectWithBlueprint(
                    "StairsUp"
                ) ||
                cell.HasObjectWithBlueprint(
                    "StairsDown"
                ) ||
                cell.HasObjectWithBlueprint(
                    "Pit"
                );
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


            SubterraneanSites
                .SubterraneanSitesEPReservations
                .ClaimCell(
                    Z,
                    cell
                );


            return true;
        }


        private bool FarEnoughFromCells(
            Cell candidate,
            List<Cell> others,
            int minimumDistance
        )
        {
            if (candidate == null)
                return false;


            if (
                others == null ||
                others.Count == 0
            )
            {
                return true;
            }


            foreach (
                Cell other
                in others
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
                    minimumDistance
                )
                {
                    return false;
                }
            }


            return true;
        }


        private void ShuffleCells(
            List<Cell> cells,
            System.Random rng
        )
        {
            if (
                cells == null ||
                rng == null
            )
            {
                return;
            }


            for (
                int i =
                    cells.Count - 1;
                i > 0;
                i--
            )
            {
                int j =
                    rng.Next(
                        i + 1
                    );


                Cell temp =
                    cells[i];

                cells[i] =
                    cells[j];

                cells[j] =
                    temp;
            }
        }


        private void ClampSettings()
        {
            if (MinSceneClusters < 0)
                MinSceneClusters = 0;

            if (MaxSceneClusters < MinSceneClusters)
                MaxSceneClusters = MinSceneClusters;


            if (MinScenePatchCells < 1)
                MinScenePatchCells = 1;

            if (MaxScenePatchCells < MinScenePatchCells)
                MaxScenePatchCells = MinScenePatchCells;


            if (MinObjectsPerScene < 1)
                MinObjectsPerScene = 1;

            if (MaxObjectsPerScene < MinObjectsPerScene)
                MaxObjectsPerScene = MinObjectsPerScene;


            if (MinSceneSeparation < 0)
                MinSceneSeparation = 0;


            if (MinDebrisPatches < 0)
                MinDebrisPatches = 0;

            if (MaxDebrisPatches < MinDebrisPatches)
                MaxDebrisPatches = MinDebrisPatches;


            if (MinDebrisCells < 1)
                MinDebrisCells = 1;

            if (MaxDebrisCells < MinDebrisCells)
                MaxDebrisCells = MinDebrisCells;
        }
    }

    


    /// <summary>
    /// Portal Category-3 materialization.
    ///
    /// Buried structural mass and the absolute outer edge are
    /// Blueshifted Chrome.
    ///
    /// Exposed room-facing walls and divider walls become
    /// SultanWall_Period1.
    /// </summary>
    public class SubterraneanSitesPortalMaterials :
        ZoneBuilderSandbox
    {
        public string BulkWallBlueprint =
            "Blueshifted Chrome";

        public string InnerWallBlueprint =
            "CrysteelBraidWall";

        public int SealedBorderWidth =
            1;


        public bool BuildZone(
            Zone Z
        )
        {
            if (Z == null)
                return true;

            SealedBorderWidth =
                Math.Max(
                    0,
                    SealedBorderWidth
                );


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


                    string blueprint =
                        isBoundary &&
                        !IsForcedBorder(
                            Z,
                            x,
                            y
                        )
                            ? InnerWallBlueprint
                            : BulkWallBlueprint;


                    if (blueprint.IsNullOrEmpty())
                        continue;


                    //
                    // Create first so an invalid blueprint cannot
                    // produce a blank structural cell after clearing
                    // the abstract wall.
                    //
                    GameObject replacement =
                        GameObjectFactory
                            .Factory
                            .CreateObject(
                                blueprint
                            );

                    if (replacement == null)
                        continue;


                    cell.ClearWalls();

                    cell.AddObject(
                        replacement
                    );
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
    /// Portal Category 4.
    ///
    /// Architectural idea:
    ///
    ///   - one broad main transit / gate chamber
    ///   - several oval waiting-room / terminal chambers
    ///   - mostly one-cell orthogonal halls
    ///   - occasional widened transfer junctions
    ///   - sparse internal divider walls
    ///   - inconsistent doors at clean one-cell chokepoints
    ///
    /// The main chamber deliberately keeps a broad central patch
    /// free of divider walls so later Portal C2 can fit a 9x7
    /// teleport-gate prefab or comparable installation.
    /// </summary>
    public class SubterraneanSitesPortalLayout :
        ZoneBuilderSandbox
    {

        public int Border =
            1;

        //
        // Main transit hall.
        //
        // Keep this approximately where it is. It already gives us
        // the broad central concourse we want for later portal sets.
        //
        public int MainRadiusX =
            14;

        public int MainRadiusY =
            6;


        //
        // Use more of the surrounding zone without turning the
        // whole level into one enormous room.
        //
        public int MinRooms =
            12;

        public int MaxRooms =
            15;


        //
        // Ordinary oval terminal / waiting chambers.
        //
        // Preserve some small chambers so the narrow vertical space
        // above and below the main concourse can still be populated,
        // but allow somewhat broader rooms overall.
        //
        public int MinRoomRadiusX =
            4;

        public int MaxRoomRadiusX =
            9;

        public int MinRoomRadiusY =
            2;

        public int MaxRoomRadiusY =
            4;


        //
        // Dedicated vertical-transition room when an anchor
        // falls outside the main chamber.
        //
        public int AnchorRadiusX =
            4;

        public int AnchorRadiusY =
            3;


        //
        // Rooms may approach one another more closely.
        // Their ellipse shapes still leave irregular structural mass
        // between them, while corridors provide the actual connections.
        //
        public int RoomGap =
            0;


        //
        // More cross-links help consume otherwise-unused structural
        // space and reinforce the transit-complex feel.
        //
        public int ExtraConnections =
            9;


        //
        // Some corridor elbows broaden into small transfer spaces.
        //
        public int JunctionPocketChance =
            45;


        //
        // Large secondary rooms may receive one divider.
        //
        public int SecondaryDividerChance =
            50;


        //
        // Doors are uncommon and now restricted to actual room
        // thresholds rather than arbitrary hallway chokepoints.
        //
        public string DoorBlueprint =
            "Door";

        public int DoorChance =
            35;

        public int MaxDoors =
            5;

        public int DoorMinSeparation =
            6;

        public int DoorAnchorClearRadius =
            3;



        private sealed class Room
        {
            public int X;
            public int Y;

            public int RX;
            public int RY;

            public bool IsMain;
            public bool IsAnchor;
        }


        private sealed class DoorCandidate
        {
            public int X;
            public int Y;
        }


        public bool BuildZone(
            Zone Z
        )
        {
            if (Z == null)
                return true;


            ClampSettings(
                Z
            );


            int seed =
                XRLCore.Core.Game.GetWorldSeed(
                    "SubterraneanSites:PortalLayout:" +
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


            List<Room> rooms =
                new List<Room>();


            //
            // ============================================================
            // MAIN TRANSIT CHAMBER
            // ============================================================
            //
            Room main =
                new Room
                {
                    X =
                        Z.Width / 2,

                    Y =
                        Z.Height / 2,

                    RX =
                        MainRadiusX,

                    RY =
                        MainRadiusY,

                    IsMain =
                        true
                };


            rooms.Add(
                main
            );

            CarveEllipse(
                Z,
                main
            );


            //
            // ============================================================
            // VERTICAL-TRANSITION CHAMBERS
            // ============================================================
            //
            // If an anchor already falls inside the large main chamber,
            // no extra chamber is needed.
            //
            if (anchors != null)
            {
                foreach (
                    Location2D anchor
                    in anchors
                )
                {
                    if (
                        anchor == null ||
                        IsPointInsideAnyRoom(
                            anchor.X,
                            anchor.Y,
                            rooms
                        )
                    )
                    {
                        continue;
                    }


                    Room room =
                        new Room
                        {
                            X =
                                anchor.X,

                            Y =
                                anchor.Y,

                            RX =
                                AnchorRadiusX,

                            RY =
                                AnchorRadiusY,

                            IsAnchor =
                                true
                        };


                    rooms.Add(
                        room
                    );

                    CarveEllipse(
                        Z,
                        room
                    );
                }
            }


            //
            // ============================================================
            // ORDINARY TERMINAL / WAITING CHAMBERS
            // ============================================================
            //
            int desiredRooms =
                rng.Next(
                    MinRooms,
                    MaxRooms + 1
                );


            int attempts =
                1200;


            while (
                rooms.Count < desiredRooms &&
                attempts-- > 0
            )
            {
                Room candidate =
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


                rooms.Add(
                    candidate
                );

                CarveEllipse(
                    Z,
                    candidate
                );
            }


            //
            // ============================================================
            // HALL NETWORK
            // ============================================================
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
            // ============================================================
            // INTERNAL ARCHITECTURE
            // ============================================================
            //
            // Divider cells are reintroduced as ordinary solid
            // placeholders. The shared boundary classifier that runs
            // immediately after C4 will classify their exposed faces
            // normally, so C3 later turns them into SultanWall_Period1.
            //
            AddRoomDividers(
                Z,
                rooms,
                anchors,
                rng
            );


            //
            // Doors are part of C4 structural architecture, not C5
            // decoration.
            //
            PlaceSparseDoors(
                Z,
                rooms,
                anchors,
                rng
            );


            //
            // No carving operation may damage the dimensional shell.
            //
            ForceSealedBorder(
                Z
            );


            Z.ClearReachableMap();

            return true;
        }


        private void ClampSettings(
            Zone Z
        )
        {
            Border =
                Math.Max(
                    1,
                    Border
                );


            int maxRX =
                Math.Max(
                    6,
                    (
                        Z.Width -
                        Border * 2 -
                        1
                    ) / 2
                );

            int maxRY =
                Math.Max(
                    4,
                    (
                        Z.Height -
                        Border * 2 -
                        1
                    ) / 2
                );


            MainRadiusX =
                Math.Max(
                    6,
                    Math.Min(
                        MainRadiusX,
                        maxRX
                    )
                );

            MainRadiusY =
                Math.Max(
                    4,
                    Math.Min(
                        MainRadiusY,
                        maxRY
                    )
                );


            MinRooms =
                Math.Max(
                    2,
                    MinRooms
                );

            MaxRooms =
                Math.Max(
                    MinRooms,
                    MaxRooms
                );


            MinRoomRadiusX =
                Math.Max(
                    3,
                    MinRoomRadiusX
                );

            MaxRoomRadiusX =
                Math.Max(
                    MinRoomRadiusX,
                    MaxRoomRadiusX
                );


            MinRoomRadiusY =
                Math.Max(
                    2,
                    MinRoomRadiusY
                );

            MaxRoomRadiusY =
                Math.Max(
                    MinRoomRadiusY,
                    MaxRoomRadiusY
                );


            AnchorRadiusX =
                Math.Max(
                    2,
                    AnchorRadiusX
                );

            AnchorRadiusY =
                Math.Max(
                    2,
                    AnchorRadiusY
                );


            RoomGap =
                Math.Max(
                    0,
                    RoomGap
                );

            ExtraConnections =
                Math.Max(
                    0,
                    ExtraConnections
                );


            JunctionPocketChance =
                Math.Max(
                    0,
                    Math.Min(
                        100,
                        JunctionPocketChance
                    )
                );


            SecondaryDividerChance =
                Math.Max(
                    0,
                    Math.Min(
                        100,
                        SecondaryDividerChance
                    )
                );


            DoorChance =
                Math.Max(
                    0,
                    Math.Min(
                        100,
                        DoorChance
                    )
                );


            MaxDoors =
                Math.Max(
                    0,
                    MaxDoors
                );

            DoorMinSeparation =
                Math.Max(
                    0,
                    DoorMinSeparation
                );

            DoorAnchorClearRadius =
                Math.Max(
                    0,
                    DoorAnchorClearRadius
                );
        }


        private Room MakeRandomRoom(
            Zone Z,
            System.Random rng
        )
        {
            int rx =
                rng.Next(
                    MinRoomRadiusX,
                    MaxRoomRadiusX + 1
                );

            int ry =
                rng.Next(
                    MinRoomRadiusY,
                    MaxRoomRadiusY + 1
                );


            //
            // Favor long waiting-room / concourse shapes without
            // making every chamber identical.
            //
            if (
                rng.Next(100) <
                65
            )
            {
                rx =
                    Math.Max(
                        rx,
                        ry + 2
                    );
            }


            int minX =
                Border +
                rx;

            int maxX =
                Z.Width -
                Border -
                rx -
                1;

            int minY =
                Border +
                ry;

            int maxY =
                Z.Height -
                Border -
                ry -
                1;


            if (
                maxX < minX ||
                maxY < minY
            )
            {
                return null;
            }


            return
                new Room
                {
                    X =
                        rng.Next(
                            minX,
                            maxX + 1
                        ),

                    Y =
                        rng.Next(
                            minY,
                            maxY + 1
                        ),

                    RX =
                        rx,

                    RY =
                        ry
                };
        }


        private bool OverlapsExisting(
            Room candidate,
            List<Room> rooms
        )
        {
            int candidateX1 =
                candidate.X -
                candidate.RX;

            int candidateX2 =
                candidate.X +
                candidate.RX;

            int candidateY1 =
                candidate.Y -
                candidate.RY;

            int candidateY2 =
                candidate.Y +
                candidate.RY;


            foreach (
                Room room
                in rooms
            )
            {
                int roomX1 =
                    room.X -
                    room.RX;

                int roomX2 =
                    room.X +
                    room.RX;

                int roomY1 =
                    room.Y -
                    room.RY;

                int roomY2 =
                    room.Y +
                    room.RY;


                bool separated =
                    candidateX2 +
                        RoomGap <
                        roomX1 ||

                    candidateX1 -
                        RoomGap >
                        roomX2 ||

                    candidateY2 +
                        RoomGap <
                        roomY1 ||

                    candidateY1 -
                        RoomGap >
                        roomY2;


                if (!separated)
                    return true;
            }


            return false;
        }


        private bool IsPointInsideAnyRoom(
            int x,
            int y,
            List<Room> rooms
        )
        {
            foreach (
                Room room
                in rooms
            )
            {
                if (
                    IsPointInsideEllipse(
                        room,
                        x,
                        y,
                        0
                    )
                )
                {
                    return true;
                }
            }


            return false;
        }


        private bool IsPointInsideEllipse(
            Room room,
            int x,
            int y,
            int inset
        )
        {
            int rx =
                Math.Max(
                    1,
                    room.RX -
                    inset
                );

            int ry =
                Math.Max(
                    1,
                    room.RY -
                    inset
                );


            long rxSquared =
                (long)rx *
                rx;

            long rySquared =
                (long)ry *
                ry;


            int dx =
                x -
                room.X;

            int dy =
                y -
                room.Y;


            return
                (long)dx *
                    dx *
                    rySquared +

                (long)dy *
                    dy *
                    rxSquared

                <=

                rxSquared *
                rySquared;
        }


        private void CarveEllipse(
            Zone Z,
            Room room
        )
        {
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
                    if (
                        IsPointInsideEllipse(
                            room,
                            x,
                            y,
                            0
                        )
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
        }


        private void ConnectAllRooms(
            Zone Z,
            List<Room> rooms,
            System.Random rng
        )
        {
            if (rooms.Count <= 1)
                return;


            List<Room> connected =
                new List<Room>
                {
                    rooms[0]
                };


            List<Room> remaining =
                new List<Room>(
                    rooms
                );

            remaining.RemoveAt(
                0
            );


            while (
                remaining.Count >
                0
            )
            {
                Room bestFrom =
                    null;

                Room bestTo =
                    null;

                int bestDistance =
                    int.MaxValue;


                foreach (
                    Room from
                    in connected
                )
                {
                    foreach (
                        Room to
                        in remaining
                    )
                    {
                        int distance =
                            Math.Abs(
                                from.X -
                                to.X
                            ) +
                            Math.Abs(
                                from.Y -
                                to.Y
                            );


                        if (
                            distance <
                            bestDistance
                        )
                        {
                            bestDistance =
                                distance;

                            bestFrom =
                                from;

                            bestTo =
                                to;
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
                    bestFrom.X,
                    bestFrom.Y,
                    bestTo.X,
                    bestTo.Y,
                    rng
                );


                connected.Add(
                    bestTo
                );

                remaining.Remove(
                    bestTo
                );
            }
        }


        private void AddExtraConnections(
            Zone Z,
            List<Room> rooms,
            System.Random rng
        )
        {
            if (rooms.Count < 2)
                return;


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
                    continue;


                CarveCorridor(
                    Z,
                    a.X,
                    a.Y,
                    b.X,
                    b.Y,
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
                rng.Next(2) ==
                0;


            int elbowX;
            int elbowY;


            if (horizontalFirst)
            {
                CarveHorizontal(
                    Z,
                    fromX,
                    toX,
                    fromY
                );


                elbowX =
                    toX;

                elbowY =
                    fromY;


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


                elbowX =
                    fromX;

                elbowY =
                    toY;


                CarveHorizontal(
                    Z,
                    fromX,
                    toX,
                    toY
                );
            }


            if (
                rng.Next(100) <
                JunctionPocketChance
            )
            {
                CarveJunctionPocket(
                    Z,
                    elbowX,
                    elbowY,
                    rng
                );
            }
        }


        private void CarveHorizontal(
            Zone Z,
            int x1,
            int x2,
            int y
        )
        {
            int start =
                Math.Min(
                    x1,
                    x2
                );

            int end =
                Math.Max(
                    x1,
                    x2
                );


            for (
                int x = start;
                x <= end;
                x++
            )
            {
                CarveCell(
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
                Math.Min(
                    y1,
                    y2
                );

            int end =
                Math.Max(
                    y1,
                    y2
                );


            for (
                int y = start;
                y <= end;
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


        private void CarveJunctionPocket(
            Zone Z,
            int centerX,
            int centerY,
            System.Random rng
        )
        {
            int rx =
                rng.Next(2) == 0
                    ? 2
                    : 1;

            int ry =
                rx == 2
                    ? 1
                    : 2;


            for (
                int dx = -rx;
                dx <= rx;
                dx++
            )
            {
                for (
                    int dy = -ry;
                    dy <= ry;
                    dy++
                )
                {
                    if (
                        dx *
                            dx *
                            ry *
                            ry +

                        dy *
                            dy *
                            rx *
                            rx

                        <=

                        rx *
                            rx *
                            ry *
                            ry
                    )
                    {
                        CarveCell(
                            Z,
                            centerX + dx,
                            centerY + dy
                        );
                    }
                }
            }
        }


        private void AddRoomDividers(
            Zone Z,
            List<Room> rooms,
            List<Location2D> anchors,
            System.Random rng
        )
        {
            foreach (
                Room room
                in rooms
            )
            {
                if (room.IsAnchor)
                    continue;


                int count =
                    0;


                if (room.IsMain)
                {
                    count =
                        rng.Next(
                            1,
                            3
                        );
                }
                else if (
                    room.RX >= 6 &&
                    rng.Next(100) <
                        SecondaryDividerChance
                )
                {
                    count =
                        1;
                }


                for (
                    int i = 0;
                    i < count;
                    i++
                )
                {
                    AddOneDivider(
                        Z,
                        room,
                        anchors,
                        rng
                    );
                }
            }
        }


        private void AddOneDivider(
            Zone Z,
            Room room,
            List<Location2D> anchors,
            System.Random rng
        )
        {
            bool vertical =
                room.RX >
                room.RY
                    ? rng.Next(100) < 65
                    : rng.Next(2) == 0;


            if (vertical)
            {
                int offset;


                if (room.IsMain)
                {
                    int magnitude =
                        Math.Max(
                            6,
                            room.RX / 2
                        );

                    offset =
                        rng.Next(2) == 0
                            ? -magnitude
                            : magnitude;
                }
                else
                {
                    int span =
                        Math.Max(
                            1,
                            room.RX / 3
                        );

                    offset =
                        rng.Next(
                            -span,
                            span + 1
                        );
                }


                int x =
                    room.X +
                    offset;


                int gap1 =
                    room.Y +
                    rng.Next(
                        -1,
                        2
                    );


                int gap2 =
                    room.Y +
                    (
                        rng.Next(2) == 0
                            ? -Math.Max(
                                2,
                                room.RY / 2
                            )
                            : Math.Max(
                                2,
                                room.RY / 2
                            )
                    );


                bool secondGap =
                    rng.Next(100) <
                    40;


                for (
                    int y =
                        room.Y -
                        room.RY +
                        1;

                    y <=
                        room.Y +
                        room.RY -
                        1;

                    y++
                )
                {
                    if (
                        y == gap1 ||
                        (
                            secondGap &&
                            y == gap2
                        )
                    )
                    {
                        continue;
                    }


                    TryAddDividerCell(
                        Z,
                        room,
                        anchors,
                        x,
                        y
                    );
                }
            }
            else
            {
                int offset;


                if (room.IsMain)
                {
                    int magnitude =
                        Math.Max(
                            3,
                            room.RY / 2
                        );

                    offset =
                        rng.Next(2) == 0
                            ? -magnitude
                            : magnitude;
                }
                else
                {
                    int span =
                        Math.Max(
                            1,
                            room.RY / 3
                        );

                    offset =
                        rng.Next(
                            -span,
                            span + 1
                        );
                }


                int y =
                    room.Y +
                    offset;


                int gap1 =
                    room.X +
                    rng.Next(
                        -1,
                        2
                    );


                int gap2 =
                    room.X +
                    (
                        rng.Next(2) == 0
                            ? -Math.Max(
                                3,
                                room.RX / 2
                            )
                            : Math.Max(
                                3,
                                room.RX / 2
                            )
                    );


                bool secondGap =
                    rng.Next(100) <
                    40;


                for (
                    int x =
                        room.X -
                        room.RX +
                        1;

                    x <=
                        room.X +
                        room.RX -
                        1;

                    x++
                )
                {
                    if (
                        x == gap1 ||
                        (
                            secondGap &&
                            x == gap2
                        )
                    )
                    {
                        continue;
                    }


                    TryAddDividerCell(
                        Z,
                        room,
                        anchors,
                        x,
                        y
                    );
                }
            }
        }


        private void TryAddDividerCell(
            Zone Z,
            Room room,
            List<Location2D> anchors,
            int x,
            int y
        )
        {
            if (
                x < Border ||
                y < Border ||
                x >= Z.Width - Border ||
                y >= Z.Height - Border ||
                !IsPointInsideEllipse(
                    room,
                    x,
                    y,
                    1
                )
            )
            {
                return;
            }


            //
            // Keep an 11x9 clean staging region in the center of
            // the main chamber.
            //
            // Our later 9x7 teleport-gate prefab can fit here
            // with one cell of breathing room around it.
            //
            if (
                room.IsMain &&
                Math.Abs(
                    x -
                    room.X
                ) <= 5 &&
                Math.Abs(
                    y -
                    room.Y
                ) <= 4
            )
            {
                return;
            }


            if (
                SubterraneanSites
                    .SubterraneanSitesEPPlacement
                    .IsNearAnyAnchor(
                        x,
                        y,
                        anchors,
                        2
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


            if (
                cell == null ||
                !SubterraneanSites
                    .SubterraneanSitesEPGeometry
                    .IsOpenGeometryCell(
                        cell
                    )
            )
            {
                return;
            }


            cell.ClearWalls();

            cell.AddObject(
                SubterraneanSites
                    .SubterraneanSitesEPGeometry
                    .SolidPlaceholderBlueprint
            );
        }


        private void PlaceSparseDoors(
            Zone Z,
            List<Room> rooms,
            List<Location2D> anchors,
            System.Random rng
        )
        {
            if (
                DoorBlueprint.IsNullOrEmpty() ||
                MaxDoors <= 0
            )
            {
                return;
            }


            List<DoorCandidate> candidates =
                new List<DoorCandidate>();


            for (
                int x = Border + 1;
                x < Z.Width - Border - 1;
                x++
            )
            {
                for (
                    int y = Border + 1;
                    y < Z.Height - Border - 1;
                    y++
                )
                {
                    Cell cell =
                        Z.GetCell(
                            x,
                            y
                        );


                    if (
                        !IsOpenCell(
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
                                x,
                                y,
                                anchors,
                                DoorAnchorClearRadius
                            )
                    )
                    {
                        continue;
                    }


                    Cell north =
                        Z.GetCell(
                            x,
                            y - 1
                        );

                    Cell south =
                        Z.GetCell(
                            x,
                            y + 1
                        );

                    Cell west =
                        Z.GetCell(
                            x - 1,
                            y
                        );

                    Cell east =
                        Z.GetCell(
                            x + 1,
                            y
                        );

                    bool northSouth =
                        IsOpenCell(
                            north
                        ) &&
                        IsOpenCell(
                            south
                        ) &&
                        IsStructuralCell(
                            west
                        ) &&
                        IsStructuralCell(
                            east
                        );


                    bool eastWest =
                        IsOpenCell(
                            west
                        ) &&
                        IsOpenCell(
                            east
                        ) &&
                        IsStructuralCell(
                            north
                        ) &&
                        IsStructuralCell(
                            south
                        );


                    if (
                        !northSouth &&
                        !eastWest
                    )
                    {
                        continue;
                    }


                    //
                    // A geometrically valid chokepoint is not automatically
                    // a doorway.
                    //
                    // In the first Portal pass this test was missing, so long
                    // one-cell halls accumulated doors anywhere the surrounding
                    // structure happened to make a clean choke.
                    //
                    // Require the opening to actually cross the boundary of one
                    // of our generated chambers.
                    //
                    if (
                        !IsRoomThresholdCandidate(
                            x,
                            y,
                            rooms,
                            northSouth,
                            eastWest
                        )
                    )
                    {
                        continue;
                    }


                    candidates.Add(
                        new DoorCandidate
                        {
                            X = x,
                            Y = y
                        }
                    );



                }
            }


            List<DoorCandidate> placed =
                new List<DoorCandidate>();


            while (
                candidates.Count > 0 &&
                placed.Count < MaxDoors
            )
            {
                int index =
                    rng.Next(
                        candidates.Count
                    );


                DoorCandidate candidate =
                    candidates[
                        index
                    ];


                candidates.RemoveAt(
                    index
                );


                if (
                    rng.Next(100) >=
                    DoorChance
                )
                {
                    continue;
                }


                if (
                    IsTooCloseToDoor(
                        candidate,
                        placed
                    )
                )
                {
                    continue;
                }


                Cell cell =
                    Z.GetCell(
                        candidate.X,
                        candidate.Y
                    );


                if (
                    !IsOpenCell(
                        cell
                    ) ||
                    cell.GetFirstObjectWithPart(
                        "Door"
                    ) != null
                )
                {
                    continue;
                }


                cell.AddObject(
                    DoorBlueprint
                );


                placed.Add(
                    candidate
                );
            }
        }

        private bool IsRoomThresholdCandidate(
            int x,
            int y,
            List<Room> rooms,
            bool northSouth,
            bool eastWest
        )
        {
            if (
                rooms == null ||
                rooms.Count == 0
            )
            {
                return false;
            }


            foreach (
                Room room
                in rooms
            )
            {
                if (room == null)
                    continue;


                //
                // For north/south travel, exactly one side of the
                // candidate should belong to the chamber.
                //
                // This means we are crossing the room boundary:
                //
                //       room
                //        .
                //        D
                //        .
                //       hall
                //
                if (northSouth)
                {
                    bool northInside =
                        IsPointInsideEllipse(
                            room,
                            x,
                            y - 1,
                            0
                        );

                    bool southInside =
                        IsPointInsideEllipse(
                            room,
                            x,
                            y + 1,
                            0
                        );


                    if (
                        northInside !=
                        southInside
                    )
                    {
                        return true;
                    }
                }


                //
                // Same test for an east/west threshold.
                //
                if (eastWest)
                {
                    bool westInside =
                        IsPointInsideEllipse(
                            room,
                            x - 1,
                            y,
                            0
                        );

                    bool eastInside =
                        IsPointInsideEllipse(
                            room,
                            x + 1,
                            y,
                            0
                        );


                    if (
                        westInside !=
                        eastInside
                    )
                    {
                        return true;
                    }
                }
            }


            return false;
        }


        private bool IsTooCloseToDoor(
            DoorCandidate candidate,
            List<DoorCandidate> placed
        )
        {
            foreach (
                DoorCandidate existing
                in placed
            )
            {
                int distance =
                    Math.Abs(
                        candidate.X -
                        existing.X
                    ) +
                    Math.Abs(
                        candidate.Y -
                        existing.Y
                    );


                if (
                    distance <
                    DoorMinSeparation
                )
                {
                    return true;
                }
            }


            return false;
        }


        private bool IsOpenCell(
            Cell cell
        )
        {
            return
                cell != null &&
                SubterraneanSites
                    .SubterraneanSitesEPGeometry
                    .IsOpenGeometryCell(
                        cell
                    ) &&
                !cell.IsSolid();
        }


        private bool IsStructuralCell(
            Cell cell
        )
        {
            return
                cell != null &&
                SubterraneanSites
                    .SubterraneanSitesEPGeometry
                    .IsAnyPlaceholder(
                        cell
                    );
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
}

namespace XRL.World.Effects
{
    /// <summary>
    /// PORTAL DIMENSION
    ///
    /// C1:
    ///     dimensional instability, static space-time rifts,
    ///     and Teleportation attunement.
    ///
    /// C2:
    ///     portal transit infrastructure and paired Aloe Porta networks.
    ///
    /// C3:
    ///     Blueshifted Chrome structural mass with exposed
    ///     SultanWall_Period1 interior surfaces.
    ///
    /// C4:
    ///     large transit chamber + oval side chambers + narrow halls,
    ///     sparse dividers / doors, worn Fibonacci-rhythm transit floor.
    ///
    /// C5:
    ///     portal-themed decorations.
    /// </summary>
    /// 
    [Serializable]
    public class SubterraneanSitesEPPortalAttunementEffect :
        SubterraneanSitesEPAttunementEffect
    {
        protected override bool ApplyTheme(
            GameObject Object
        )
        {
            if (Object == null)
                return false;

            SubterraneanSitesEPPortalSpatialInstabilityEffect
                instability =
                    Object.GetEffectDescendedFrom<
                        SubterraneanSitesEPPortalSpatialInstabilityEffect
                    >();

            if (instability != null)
            {
                Object.RemoveEffect(
                    instability
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
                    "Portal",
                    StringComparison.Ordinal
                )
            )
            {
                return;
            }


            SubterraneanSitesEPPortalSpatialInstabilityEffect
                instability =
                    Object.GetEffectDescendedFrom<
                        SubterraneanSitesEPPortalSpatialInstabilityEffect
                    >();

            if (instability == null)
            {
                Object.ApplyEffect(
                    new SubterraneanSitesEPPortalSpatialInstabilityEffect()
                );
            }
        }


        public SubterraneanSitesEPPortalAttunementEffect()
            : base()
        {
            ThemeKey =
                "Portal";
        }


        public SubterraneanSitesEPPortalAttunementEffect(
            int duration,
            int mutationLevel
        )
            : base(
                duration,
                "Portal",

                // no conventional resistance
                "",
                0,

                // no generic save bonus
                "",
                "",
                0,

                // signature mutation
                "Teleportation",
                mutationLevel
            )
        {
        }

        //
        // One-shot bridge between InitiateRealityDistortionTransit and
        // TeleportTo's later BeforeTeleport event.
        //
        // Normal reality-distortion transit is approved at the first hook,
        // then allowed through the second hook without asking twice.
        //
        // Direct TeleportTo callers such as Aloe Porta never set this flag,
        // so BeforeTeleport remains able to intercept them.
        //
        [NonSerialized]
        private bool AllowNextTeleport;


        protected override void RegisterThemeEvents(
            GameObject Object,
            IEventRegistrar Registrar
        )
        {
            //
            // Normal teleport/reality-distortion transit.
            //
            Registrar.Register(
                "InitiateRealityDistortionTransit"
            );


            //
            // TeleportTo() itself fires this.
            //
            // This catches direct teleport callers such as Aloe Porta that
            // bypass InitiateRealityDistortionTransit entirely.
            //
            Registrar.Register(
                "BeforeTeleport"
            );


            //
            // Space-Time Vortex transport deliberately uses its own
            // veto hook instead of the normal transit event.
            //
            Registrar.Register(
                "SpaceTimeVortexContact"
            );
        }


        protected override bool FireThemeEvent(
            Event E
        )
        {
            if (E == null)
                return true;

            if (
                E.ID ==
                "InitiateRealityDistortionTransit"
            )
            {
                //
                // This event can also be fired on a mutation owner, device,
                // or device operator even when some OTHER object is the thing
                // actually being translocated.
                //
                // Portal attunement only protects its own bearer.
                //
                GameObject transitObject =
                    E.GetGameObjectParameter(
                        "Object"
                    );


                if (
                    !ReferenceEquals(
                        transitObject,
                        base.Object
                    )
                )
                {
                    return true;
                }


                //
                // Deliberate use of Teleportation is the one transit that
                // Portal attunement should allow without asking.
                //
                // Qud passes the initiating mutation through this event.
                //
                IPart mutation =
                    E.GetParameter(
                        "Mutation"
                    ) as IPart;


                if (
                    mutation != null &&
                    string.Equals(
                        mutation.GetType().Name,
                        "Teleportation",
                        StringComparison.Ordinal
                    )
                )
                {
                    //
                    // RandomTeleport() will immediately proceed to
                    // TeleportTo(), which fires BeforeTeleport.
                    //
                    AllowNextTeleport =
                        true;

                    return true;
                }


                bool allow =
                    ConfirmTranslocation(
                        "Your extradimensional attunement resists an attempted translocation."
                    );


                if (allow)
                {
                    //
                    // The approved transit will now continue into TeleportTo().
                    //
                    AllowNextTeleport =
                        true;
                }


                return allow;
            }

            if (
                E.ID ==
                "BeforeTeleport"
            )
            {
                //
                // Normal reality-distortion transit was already considered
                // by InitiateRealityDistortionTransit. Consume its one-shot
                // approval rather than prompting twice.
                //
                if (AllowNextTeleport)
                {
                    AllowNextTeleport =
                        false;

                    return true;
                }


                //
                // No normal reality-distortion initiation preceded this
                // TeleportTo(). Aloe Porta is the important Portal example.
                //
                return ConfirmTranslocation(
                    "Your extradimensional attunement resists an attempted translocation."
                );
            }



            if (
                E.ID ==
                "SpaceTimeVortexContact"
            )
            {
                return ConfirmTranslocation(
                    "A space-time vortex attempts to translocate you."
                );
            }


            return true;
        }


        private bool ConfirmTranslocation(
            string message
        )
        {
            //
            // This effect is intended for the player. Be defensive if it
            // somehow winds up on something else.
            //
            if (
                base.Object == null ||
                !base.Object.IsPlayer()
            )
            {
                return true;
            }


            //
            // Default to aborting the transport.
            //
            // Qud's native confirmation control labels these Yes / No:
            //
            //     Yes = Allow
            //     No  = Abort
            //
            return
                Popup.ShowYesNo(
                    message +
                    "\n\nAllow this translocation?",
                    defaultResult:
                        DialogResult.No
                ) ==
                DialogResult.Yes;
        }
    }

    [Serializable]
    public class SubterraneanSitesEPPortalDenizenAdaptationEffect :
        Effect
    {
        [NonSerialized]
        private bool AllowNextTeleport;


        public SubterraneanSitesEPPortalDenizenAdaptationEffect()
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
                "InitiateRealityDistortionTransit"
            );

            Registrar.Register(
                "BeforeTeleport"
            );

            Registrar.Register(
                "SpaceTimeVortexContact"
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
            if (E == null)
                return true;


            if (
                E.ID ==
                "InitiateRealityDistortionTransit"
            )
            {
                GameObject transitObject =
                    E.GetGameObjectParameter(
                        "Object"
                    );

                if (
                    !ReferenceEquals(
                        transitObject,
                        base.Object
                    )
                )
                {
                    return true;
                }


                //
                // Allow the denizen's own Teleportation mutation.
                //
                IPart mutation =
                    E.GetParameter(
                        "Mutation"
                    ) as IPart;

                if (
                    mutation != null &&
                    string.Equals(
                        mutation.GetType().Name,
                        "Teleportation",
                        StringComparison.Ordinal
                    )
                )
                {
                    AllowNextTeleport =
                        true;

                    return true;
                }


                //
                // Native Portal denizens automatically reject external
                // reality-distortion translocation.
                //
                return false;
            }


            if (
                E.ID ==
                "BeforeTeleport"
            )
            {
                //
                // Consume the one-shot approval from the denizen's own
                // Teleportation mutation.
                //
                if (AllowNextTeleport)
                {
                    AllowNextTeleport =
                        false;

                    return true;
                }


                //
                // Direct TeleportTo callers such as Aloe Porta are rejected.
                //
                return false;
            }


            if (
                E.ID ==
                "SpaceTimeVortexContact"
            )
            {
                return false;
            }


            return base.FireEvent(E);
        }



    }

    /// <summary>
    /// Visible Portal Category-1 instability status.
    ///
    /// The Portal environment system owns this effect's lifetime.
    /// It exists while the player is exposed to Portal C1 and
    /// unattuned, and is removed while Portal attunement is active.
    /// </summary>
    [Serializable]
    public class SubterraneanSitesEPPortalSpatialInstabilityEffect :
        Effect
    {
        public SubterraneanSitesEPPortalSpatialInstabilityEffect()
        {
            DisplayName =
                "{{M|spatially unstable}}";

            //
            // Lifetime is controlled by the Portal environment system,
            // not by the normal effect-duration countdown.
            //
            Duration =
                1;
        }


        public override bool UseStandardDurationCountdown()
        {
            return false;
        }


        public override string GetDetails()
        {
            return
                "Your position in space is unstable.\n" +
                "You may suddenly blink to another location.";
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
                    "You feel spatially unstable."
                );
            }


            return true;
        }
    }




}

