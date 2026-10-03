using System;
using System.Collections.Generic;
using Genkit;
using XRL;
using XRL.Rules;
using XRL.UI;
using XRL.World;
using XRL.World.Parts;
using XRL.World.Anatomy;

namespace SubterraneanSites
{
    /// <summary>
    /// Fungus extradimensional-pocket category provider.
    ///
    /// Current category implementation:
    ///   C1 - minor shimmering plus primary brooding puffers
    ///   C2 - periodic sleep-gas vents
    ///   C3 - red mushroom-flesh bulk with orange mushroom-flesh facing
    ///   C4 - mostly-open field with sparse chunky divider structures
    ///        and Rainbow-Wood-style Mushroomy floor paint
    ///   C5 - colored fungal colonies, primordial soup,
    ///        brightshrooms, drowsing urchins, and addling urchins
    /// </summary>
    internal sealed class SubterraneanSitesEPFungusTheme :
        ISubterraneanSitesEPCategoryProvider,
        ISubterraneanSitesEPPrimaryObjectProvider,
        ISubterraneanSitesEPAttunementProvider,
        ISubterraneanSitesEPDenizenAdaptationProvider,
        ISubterraneanSitesEPSignatureMutationProvider
    {
        public string ThemeKey
        {
            get { return "Fungus"; }
        }

        public string SignatureMutationClass
        {
            get { return "SleepGasGeneration"; }
        }

        public void ApplyDenizenAdaptation(
            GameObject creature
        )
        {
            if (creature == null)
                return;


            //
            // Native Fungus denizens are permanently adapted to their
            // extradimensional fungal environment.
            //
            // Vanilla fungal infection code checks ImmuneToFungus directly,
            // so native denizens do not need the temporary ApplySpores event
            // interception used by the player's attunement effect.
            //
            creature.SetIntProperty(
                "ImmuneToFungus",
                1
            );
        }

        private static readonly string[] AttunementInfectionBlueprints =
        {
            "LuminousInfection",
            "PuffInfection",
            "WaxInfection",
            "MumblesInfection"
        };

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

            //
            // Use vanilla Rainbow Wood's exact floor recipe.
            //
            global::XRL.World.Parts
                .Mushroomy
                .Paint(
                    cell
                );
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
            // Fungus C1 is a runtime player environment.
            //
            // RequireSystem is idempotent, so every Fungus-primary layer can
            // safely request the same persistent game system.
            //
            The.Game.RequireSystem<
                SubterraneanSitesEPFungusEnvironmentSystem
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
                "SubterraneanSitesFungusPrimaryPuffers",
                "DecorationThemeKey", "Fungus",
                "MinPuffers", "12",
                "MaxPuffers", "18",
                "TransitionExclusionRadius", "4"
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
                "SubterraneanSitesFungusPrimaryPuffers",
                "DecorationThemeKey", "Fungus",
                "EntranceOnly", "1",
                "MinPuffers", "1",
                "MaxPuffers", "2",
                "TransitionExclusionRadius", "0"
            );
        }

        public SubterraneanSitesEPAttunementBuildResult TryCreateAttunement
        (
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
                zone == null ||
                actor.Body == null
            )
            {
                return
                    SubterraneanSitesEPAttunementBuildResult
                        .Unavailable;
            }

            List<BodyPart> parts =
                actor.Body.GetParts();

            if (
                parts == null ||
                parts.Count == 0
            )
            {
                return
                    SubterraneanSitesEPAttunementBuildResult
                        .Unavailable;
            }

            Popup.Show(
                "Fungus attunement requires a temporary fungal infection. " +
                "Choose a body part to host the growth. " +
                "It will fall away when the attunement ends."
            );

            //
            // Distinguish "no valid anatomy" from the player pressing Escape.
            //
            // Vanilla ChooseLimbForInfection() returns false for both cases, so
            // perform the availability check first.
            //
            bool hasInfectablePart =
                false;

            foreach (
                BodyPart part
                in parts
            )
            {
                if (
                    part != null &&
                    XRL.World.Effects
                        .FungalSporeInfection
                        .BodyPartSuitableForFungalInfection(
                            part
                        )
                )
                {
                    hasInfectablePart =
                        true;

                    break;
                }
            }

            if (!hasInfectablePart)
            {
                Popup.Show(
                    "You have no infectable body parts."
                );

                return
                    SubterraneanSitesEPAttunementBuildResult
                        .Unavailable;
            }

            BodyPart selectedPart;
            string selectedPartName;

            //
            // Use vanilla's exact fungal-infection body-part selector.
            //
            // Ordinary restrictions remain enabled. We intentionally do NOT use
            // IgnoreBodyPartCategory=true as the quest-specific Pax infection does.
            //
            if (
                !XRL.World.Effects
                    .FungalSporeInfection
                    .ChooseLimbForInfection(
                        parts,
                        "an extradimensional fungus",
                        out selectedPart,
                        out selectedPartName
                    )
            )
            {
                //
                // We already proved at least one valid body part existed.
                // Therefore false here means the player cancelled the chooser.
                //
                return
                    SubterraneanSitesEPAttunementBuildResult
                        .Cancelled;
            }

            string infectionBlueprint =
                AttunementInfectionBlueprints[
                    Stat.Random(
                        0,
                        AttunementInfectionBlueprints.Length - 1
                    )
                ];

            int mutationLevel =
                SubterraneanSitesEPAttunementSystem
                    .GetAttunementMutationLevel(
                        zone
                    );

            effect =
                new XRL.World.Effects
                    .SubterraneanSitesEPFungusAttunementEffect(
                        SubterraneanSitesEPAttunementSystem
                            .DefaultDuration,
                        mutationLevel,
                        infectionBlueprint,
                        selectedPart
                    );

            successMessage =
                "Attunement grants:\n" +
                "Sleep Gas Generation (level " +
                mutationLevel.ToString() +
                ")\n" +
                "Immunity to fungal spores\n" +
                "Temporary fungal infection: " +
                selectedPartName;

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

            The.ZoneManager.AddZonePostBuilder(
                context.ZoneId,
                "SubterraneanSitesFungusSleepVents"
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
                "SubterraneanSitesFungusMaterials"
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
                "SubterraneanSitesFungusLayout"
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
                "SubterraneanSitesFungusFloor"
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
                "SubterraneanSitesFungusDecorations",
                "DecorationThemeKey", "Fungus"
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
                "SubterraneanSitesFungusFloor",
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
                "SubterraneanSitesFungusDecorations",
                "DecorationThemeKey", "Fungus",
                "EntranceOnly", "1"
            );
        }
    }
}

namespace SubterraneanSites
{
    /// <summary>
    /// Fungus Category-1 runtime environment.
    ///
    /// An unattuned player in a Fungus-primary EP experiences minor
    /// shimmering. Fungus attunement suppresses the effect.
    /// </summary>
    [Serializable]
    public class SubterraneanSitesEPFungusEnvironmentSystem :
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

            bool fungusEnvironment =
                string.Equals(
                    category1,
                    "Fungus",
                    StringComparison.Ordinal
                );

            bool fungusAttuned =
                SubterraneanSitesEPAttunementSystem
                    .IsAttunedTo(
                        player,
                        "Fungus"
                    );

            XRL.World.Effects
                .SubterraneanSitesEPFungusShimmeringEffect shimmering =
                    player.GetEffectDescendedFrom<
                        XRL.World.Effects
                            .SubterraneanSitesEPFungusShimmeringEffect
                    >();

            bool shouldShimmer =
                fungusEnvironment &&
                !fungusAttuned;

            if (shouldShimmer)
            {
                if (shimmering == null)
                {
                    player.ApplyEffect(
                        new XRL.World.Effects
                            .SubterraneanSitesEPFungusShimmeringEffect()
                    );
                }

                //
                // VisionLevel is static vanilla state. Reassert it every
                // synchronization so save/load cannot leave our effect active
                // while the fullscreen renderer remains disabled.
                //
                XRL.World.Effects
                    .FungalVisionary
                    .VisionLevel = 1;

                return;
            }

            if (shimmering != null)
            {
                player.RemoveEffect(
                    shimmering
                );

                return;
            }

            //
            // Defensive cleanup of our static visual state.
            // Never disable genuine vanilla Shimmering.
            //
            XRL.World.Effects.FungalVisionary vanilla =
                player.GetEffectDescendedFrom<
                    XRL.World.Effects.FungalVisionary
                >();

            if (vanilla == null)
            {
                XRL.World.Effects
                    .FungalVisionary
                    .VisionLevel = 0;
            }
        }
    }
}


namespace XRL.World.Effects
{
    [Serializable]
    public class SubterraneanSitesEPFungusShimmeringEffect :
        Effect
    {
        public const int QuicknessPenalty =
            15;

        public SubterraneanSitesEPFungusShimmeringEffect()
        {
            DisplayName =
                "{{O|minor shimmering}}";

            //
            // Environment system owns the lifetime.
            //
            Duration = 1;
        }

        public override bool UseStandardDurationCountdown()
        {
            return false;
        }

        public override string GetDetails()
        {
            return
                "-15 Quickness\n" +
                "Your vision shimmers across extradimensional boundaries.";
        }

        public override bool Apply(
            GameObject Object
        )
        {
            if (
                Object == null ||
                !Object.HasStat(
                    "Speed"
                )
            )
            {
                return false;
            }

            //
            // Qud's Quickness is represented internally by Speed.
            //
            StatShifter.SetStatShift(
                "Speed",
                -QuicknessPenalty
            );

            if (Object.IsPlayer())
            {
                FungalVisionary.VisionLevel =
                    1;
                    Popup.Show(
                    "You start shimmering."
                );
            }

            return true;
        }

        public override void Remove(
            GameObject Object
        )
        {
            StatShifter.RemoveStatShifts();

            if (
                Object == null ||
                !Object.IsPlayer()
            )
            {
                return;
            }

            //
            // If the player genuinely has vanilla Shimmering, preserve its
            // fullscreen visual when our weaker EP version is removed.
            //
            FungalVisionary vanilla =
                Object.GetEffectDescendedFrom<
                    FungalVisionary
                >();

            FungalVisionary.VisionLevel =
                vanilla != null
                    ? 1
                    : 0;
        }
    }
    
    /// <summary>
    /// Fungus-specific EP attunement.
    ///
    /// Shared base owns:
    ///   - 250-turn lifetime
    ///   - mutation modifier
    ///   - generic removal lifecycle
    ///
    /// Fungus owns:
    ///   - mandatory temporary fungal infection
    ///   - fungal-spore immunity
    ///   - validation that the infection remains attached
    ///   - exact cleanup of the infection
    ///
    /// </summary>
    [Serializable]
    public class SubterraneanSitesEPFungusAttunementEffect :
        SubterraneanSitesEPAttunementEffect
    {
        public const string TemporaryInfectionProperty =
            "SubterraneanSitesEPTemporaryFungalInfection";

        public string InfectionBlueprint =
            "";

        //
        // Keep an exact reference to the infection owned by this effect.
        //
        // Qud effects can persist GameObject references; vanilla
        // FungalSporeInfection itself does this with its Owner field.
        //
        public GameObject TemporaryInfectionObject;

        //
        // This is needed only during the synchronous ApplyEffect() transaction.
        // Once the infection exists we keep the GameObject reference instead.
        //
        [NonSerialized]
        public BodyPart PendingBodyPart;

        public SubterraneanSitesEPFungusAttunementEffect()
            : base()
        {
            ThemeKey =
                "Fungus";
        }

        public SubterraneanSitesEPFungusAttunementEffect(
            int duration,
            int mutationLevel,
            string infectionBlueprint,
            BodyPart pendingBodyPart
        )
            : base(
                duration,
                "Fungus",

                // no conventional resistance
                "",
                0,

                // no generic save bonus
                "",
                "",
                0,

                // signature mutation
                "SleepGasGeneration",
                mutationLevel
            )
        {
            InfectionBlueprint =
                infectionBlueprint ?? "";

            PendingBodyPart =
                pendingBodyPart;
        }

        protected override void RegisterThemeEvents(
            GameObject Object,
            IEventRegistrar Registrar
        )
        {
            Registrar.Register(
                "ApplySpores"
            );

            Registrar.Register(
                "EndTurn"
            );
        }

        protected override bool FireThemeEvent(
            Event E
        )
        {
            //
            // Native fungal-spore immunity.
            //
            // GasFungalSpores and the itchy-skin precursor use ApplySpores as
            // an allow/deny hook. We deliberately do NOT set ImmuneToFungus,
            // because vanilla ApplyFungalInfection() checks that property and
            // our mandatory infection must still be installable.
            //
            if (E.ID == "ApplySpores")
            {
                return false;
            }

            //
            // The fungal infection is the cost/physical anchor of this
            // attunement. If it somehow disappears early, do not let the
            // remaining attunement benefits continue indefinitely.
            //
            if (
                E.ID == "EndTurn" &&
                Duration > 0 &&
                !HasActiveTemporaryInfection(
                    base.Object
                )
            )
            {
                Duration = 0;
            }

            return true;
        }

        protected override bool ApplyTheme(
            GameObject Object
        )
        {
            if (
                Object == null ||
                Object.Body == null ||
                PendingBodyPart == null ||
                InfectionBlueprint.IsNullOrEmpty()
            )
            {
                return false;
            }

            //
            // Revalidate immediately before modifying the actor.
            //
            if (
                !FungalSporeInfection
                    .BodyPartSuitableForFungalInfection(
                        PendingBodyPart
                    )
            )
            {
                return false;
            }

            //
            // Use vanilla's actual fungal infection installer.
            //
            // It handles:
            //   - removing displaced equipment
            //   - UsesSlots/dependent slots
            //   - infection-specific AV/DV/melee adjustments
            //   - equipping the fungal object correctly
            //   - normal fungal lifecycle setup
            //
            if (
                !FungalSporeInfection
                    .ApplyFungalInfection(
                        Object,
                        InfectionBlueprint,
                        PendingBodyPart
                    )
            )
            {
                return false;
            }

            //
            // Because we supplied an exact SelectedPart, vanilla installs the
            // newly-created infection onto that body part.
            //
            GameObject infection =
                PendingBodyPart.Equipped;

            if (infection == null)
            {
                return false;
            }

            infection.SetIntProperty(
                TemporaryInfectionProperty,
                1
            );

            TemporaryInfectionObject =
                infection;

            //
            // Do not retain anatomy references after the initial transaction.
            //
            PendingBodyPart =
                null;

            //
            // Successful Fungus attunement immediately suppresses minor shimmering.
            // Do this directly rather than waiting for the next EndTurn synchronization.
            //
            SubterraneanSitesEPFungusShimmeringEffect
                shimmering =
                    Object.GetEffectDescendedFrom<
                        SubterraneanSitesEPFungusShimmeringEffect
                    >();

            if (shimmering != null)
            {
                Object.RemoveEffect(
                    shimmering
                );
            }

            return true;
        }

        protected override void RemoveTheme(
            GameObject Object
        )
        {
            bool removedInfection =
                false;

            //
            // Normal path: remove the exact infection owned by this effect.
            //
            GameObject infection =
                TemporaryInfectionObject;

            GameObject.Validate(
                ref infection
            );

            if (infection != null)
            {
                removedInfection =
                    DestroyTemporaryInfection(
                        Object,
                        infection
                    ) ||
                    removedInfection;
            }

            TemporaryInfectionObject =
                null;

            //
            // Defensive fallback if the saved direct reference was ever lost.
            //
            removedInfection =
                RemoveMarkedTemporaryInfections(
                    Object
                ) ||
                removedInfection;

            //
            // Only announce a real Fungus infection being removed.
            // This avoids a bogus popup if ApplyTheme failed before infection.
            //
            if (
                removedInfection &&
                Object != null &&
                Object.IsPlayer()
            )
            {
                Popup.Show(
                    "The extradimensional fungus crumbles and sloughs away from your body."
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
                "Immune to fungal spores"
            );

            lines.Add(
                "Temporary fungal infection"
            );
        }

        public override bool CanRefresh(
            GameObject Object,
            Zone Z
        )
        {
            return
                base.CanRefresh(
                    Object,
                    Z
                ) &&
                HasActiveTemporaryInfection(
                    Object
                );
        }

        private bool HasActiveTemporaryInfection(
            GameObject Object
        )
        {
            if (
                Object == null ||
                Object.Body == null
            )
            {
                return false;
            }

            GameObject infection =
                TemporaryInfectionObject;

            GameObject.Validate(
                ref infection
            );

            TemporaryInfectionObject =
                infection;

            if (
                infection == null ||
                infection.GetIntProperty(
                    TemporaryInfectionProperty
                ) <= 0
            )
            {
                return false;
            }

            foreach (
                BodyPart part
                in Object.Body.GetParts()
            )
            {
                if (
                    part != null &&
                    ReferenceEquals(
                        part.Equipped,
                        infection
                    )
                )
                {
                    return true;
                }
            }

            return false;
        }
        private static bool DestroyTemporaryInfection(
            GameObject actor,
            GameObject infection
        )
        {
            if (infection == null)
                return false;

            bool owned =
                infection.GetIntProperty(
                    TemporaryInfectionProperty
                ) > 0;

            if (
                actor != null &&
                actor.Body != null
            )
            {
                foreach (
                    BodyPart part
                    in actor.Body.GetParts()
                )
                {
                    if (
                        part == null ||
                        !ReferenceEquals(
                            part.Equipped,
                            infection
                        )
                    )
                    {
                        continue;
                    }

                    part.TryUnequip(
                        Silent: true,
                        SemiForced: true
                    );

                    break;
                }
            }

            GameObject target =
                infection;

            GameObject.Validate(
                ref target
            );

            if (target != null)
            {
                target.Destroy();
            }

            return owned;
        }

        private static bool RemoveMarkedTemporaryInfections(
            GameObject actor
        )
        {
            if (
                actor == null ||
                actor.Body == null
            )
            {
                return false;
            }

            HashSet<GameObject> found =
                new HashSet<GameObject>();

            foreach (
                BodyPart part
                in actor.Body.GetParts()
            )
            {
                if (
                    part == null ||
                    part.Equipped == null
                )
                {
                    continue;
                }

                if (
                    part.Equipped.GetIntProperty(
                        TemporaryInfectionProperty
                    ) > 0
                )
                {
                    found.Add(
                        part.Equipped
                    );
                }
            }

            bool removedAny =
                false;

            foreach (
                GameObject infection
                in found
            )
            {
                removedAny =
                    DestroyTemporaryInfection(
                        actor,
                        infection
                    ) ||
                    removedAny;
            }

            return removedAny;
        }


        
    }
}

namespace XRL.World.ZoneBuilders
{
    /// <summary>
    /// Fungus Category 3.
    ///
    /// Buried/core mass is red mushroom flesh.
    /// Exposed playable-facing walls use the base/orange mushroom flesh.
    /// The sealed zone perimeter remains bulk material.
    /// </summary>
    public class SubterraneanSitesFungusMaterials :
        ZoneBuilderSandbox
    {
        public string BulkWallBlueprint =
            "MushroomWall-Red";

        public string InnerWallBlueprint =
            "BaseMushroomWall";

        public int SealedBorderWidth = 1;

        public bool BuildZone(Zone Z)
        {
            if (Z == null)
                return true;

            foreach (Cell cell in Z.GetCells())
            {
                if (cell == null)
                    continue;

                bool isSolid =
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

                if (!isSolid && !isBoundary)
                    continue;

                string material =
                    isBoundary &&
                    !IsForcedBorder(
                        Z,
                        cell.X,
                        cell.Y
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
    /// Fungus Category 4.
    ///
    /// Unlike the room/cave themes, Fungus begins nearly open.
    /// Several short, thick dividers are reintroduced after opening the field.
    ///
    /// C5's destructible mushrooms later form the fine-grained navigation
    /// challenge; C4 itself remains broadly traversable.
    /// </summary>
    public class SubterraneanSitesFungusLayout :
        ZoneBuilderSandbox
    {
        public int MinDividers = 5;
        public int MaxDividers = 8;

        //
        // Actual dividing walls should be thin.
        // Every cell of these will become exposed BaseMushroomWall.
        //
        public int DividerThickness = 1;

        public int MinHorizontalLength = 10;
        public int MaxHorizontalLength = 24;

        public int MinVerticalLength = 5;
        public int MaxVerticalLength = 11;

        public int BendChance = 55;
        public int MinTailLength = 3;
        public int MaxTailLength = 8;

        //
        // Separate compact fungal masses provide true Category-3 bulk away from
        // the perimeter. Their outer cells become BaseMushroomWall while their
        // protected cores remain MushroomWall-Red.
        //
        public int MinBulkMasses = 3;
        public int MaxBulkMasses = 5;

        public int MinBulkRadiusX = 3;
        public int MaxBulkRadiusX = 6;

        public int MinBulkRadiusY = 2;
        public int MaxBulkRadiusY = 4;

        public int AnchorRadius = 4;
        public int TransitionExclusionRadius = 5;

        public int SealedBorderWidth = 1;

        public bool BuildZone(Zone Z)
        {
            if (Z == null)
                return true;

            ClampSettings();

            List<Location2D> anchors =
                SubterraneanSites
                    .SubterraneanSitesEPVerticalTransitions
                    .GetVerticalAnchors(
                        Z.ZoneID
                    );

            bool[,] reserved =
                new bool[
                    Z.Width,
                    Z.Height
                ];

            SubterraneanSites
                .SubterraneanSitesEPPlacement
                .ReserveAroundAnchors(
                    reserved,
                    Z,
                    anchors,
                    TransitionExclusionRadius
                );

            //
            // Begin with a broad open field.
            //
            for (int x = 0; x < Z.Width; x++)
            {
                for (int y = 0; y < Z.Height; y++)
                {
                    if (IsForcedBorder(Z, x, y))
                        continue;

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
            }

            System.Random rng =
                new System.Random(
                    Stat.Random(
                        0,
                        2147483646
                    )
                );

            int dividerCount =
                rng.Next(
                    MinDividers,
                    MaxDividers + 1
                );

            for (
                int i = 0;
                i < dividerCount;
                i++
            )
            {
                PlaceRandomDivider(
                    Z,
                    reserved,
                    rng
                );
            }

            PlaceBulkMasses(
                Z,
                reserved,
                rng
            );

            //
            // Transition areas are hard gameplay invariants.
            // Re-open them after all divider placement.
            //
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

            ForceSealedBorder(Z);

            Z.ClearReachableMap();

            return true;
        }

        private void ClampSettings()
        {
            if (MinDividers < 0)
                MinDividers = 0;

            if (MaxDividers < MinDividers)
                MaxDividers = MinDividers;

            if (DividerThickness < 1)
                DividerThickness = 1;

            if (MinHorizontalLength < 2)
                MinHorizontalLength = 2;

            if (
                MaxHorizontalLength <
                MinHorizontalLength
            )
            {
                MaxHorizontalLength =
                    MinHorizontalLength;
            }

            if (MinVerticalLength < 2)
                MinVerticalLength = 2;

            if (
                MaxVerticalLength <
                MinVerticalLength
            )
            {
                MaxVerticalLength =
                    MinVerticalLength;
            }

            if (BendChance < 0)
                BendChance = 0;

            if (BendChance > 100)
                BendChance = 100;

            if (MinTailLength < 1)
                MinTailLength = 1;

            if (
                MaxTailLength <
                MinTailLength
            )
            {
                MaxTailLength =
                    MinTailLength;
            }

            if (AnchorRadius < 1)
                AnchorRadius = 1;

            if (
                TransitionExclusionRadius <
                AnchorRadius
            )
            {
                TransitionExclusionRadius =
                    AnchorRadius;
            }

            if (SealedBorderWidth < 1)
                SealedBorderWidth = 1;
        }

        private void PlaceRandomDivider(
            Zone Z,
            bool[,] reserved,
            System.Random rng
        )
        {
            bool horizontal =
                rng.Next(2) == 0;

            if (horizontal)
            {
                PlaceHorizontalDivider(
                    Z,
                    reserved,
                    rng
                );
            }
            else
            {
                PlaceVerticalDivider(
                    Z,
                    reserved,
                    rng
                );
            }
        }

        private void PlaceHorizontalDivider(
            Zone Z,
            bool[,] reserved,
            System.Random rng
        )
        {
            int length =
                rng.Next(
                    MinHorizontalLength,
                    MaxHorizontalLength + 1
                );

            int safeLeft =
                SealedBorderWidth + 4;

            int safeRight =
                Z.Width -
                SealedBorderWidth -
                4;

            if (
                safeRight -
                safeLeft <=
                length
            )
            {
                return;
            }

            int x =
                rng.Next(
                    safeLeft,
                    safeRight - length
                );

            int y =
                rng.Next(
                    SealedBorderWidth + 3,
                    Z.Height -
                    SealedBorderWidth -
                    3
                );

            for (
                int step = 0;
                step < length;
                step++
            )
            {
                PlaceHorizontalThickness(
                    Z,
                    reserved,
                    x + step,
                    y
                );
            }

            if (rng.Next(100) >= BendChance)
                return;

            int tailLength =
                rng.Next(
                    MinTailLength,
                    MaxTailLength + 1
                );

            int bendX =
                x +
                rng.Next(
                    Math.Max(
                        2,
                        length / 3
                    ),
                    Math.Max(
                        3,
                        length - 2
                    )
                );

            int direction =
                rng.Next(2) == 0
                    ? -1
                    : 1;

            for (
                int step = 1;
                step <= tailLength;
                step++
            )
            {
                PlaceVerticalThickness(
                    Z,
                    reserved,
                    bendX,
                    y +
                    direction * step
                );
            }
        }

        private void PlaceVerticalDivider(
            Zone Z,
            bool[,] reserved,
            System.Random rng
        )
        {
            int length =
                rng.Next(
                    MinVerticalLength,
                    MaxVerticalLength + 1
                );

            int safeTop =
                SealedBorderWidth + 3;

            int safeBottom =
                Z.Height -
                SealedBorderWidth -
                3;

            if (
                safeBottom -
                safeTop <=
                length
            )
            {
                return;
            }

            int y =
                rng.Next(
                    safeTop,
                    safeBottom - length
                );

            int x =
                rng.Next(
                    SealedBorderWidth + 5,
                    Z.Width -
                    SealedBorderWidth -
                    5
                );

            for (
                int step = 0;
                step < length;
                step++
            )
            {
                PlaceVerticalThickness(
                    Z,
                    reserved,
                    x,
                    y + step
                );
            }

            if (rng.Next(100) >= BendChance)
                return;

            int tailLength =
                rng.Next(
                    MinTailLength,
                    MaxTailLength + 1
                );

            int bendY =
                y +
                rng.Next(
                    Math.Max(
                        2,
                        length / 3
                    ),
                    Math.Max(
                        3,
                        length - 2
                    )
                );

            int direction =
                rng.Next(2) == 0
                    ? -1
                    : 1;

            for (
                int step = 1;
                step <= tailLength;
                step++
            )
            {
                PlaceHorizontalThickness(
                    Z,
                    reserved,
                    x +
                    direction * step,
                    bendY
                );
            }
        }

        private void PlaceBulkMasses(
            Zone Z,
            bool[,] reserved,
            System.Random rng
        )
        {
            int min =
                Math.Max(
                    0,
                    MinBulkMasses
                );

            int max =
                Math.Max(
                    min,
                    MaxBulkMasses
                );

            int desired =
                rng.Next(
                    min,
                    max + 1
                );

            int placed = 0;

            for (
                int attempt = 0;
                attempt < 80 &&
                placed < desired;
                attempt++
            )
            {
                int radiusX =
                    rng.Next(
                        Math.Max(
                            2,
                            MinBulkRadiusX
                        ),
                        Math.Max(
                            Math.Max(
                                2,
                                MinBulkRadiusX
                            ),
                            MaxBulkRadiusX
                        ) + 1
                    );

                int radiusY =
                    rng.Next(
                        Math.Max(
                            2,
                            MinBulkRadiusY
                        ),
                        Math.Max(
                            Math.Max(
                                2,
                                MinBulkRadiusY
                            ),
                            MaxBulkRadiusY
                        ) + 1
                    );

                int minX =
                    SealedBorderWidth +
                    radiusX +
                    2;

                int maxX =
                    Z.Width -
                    SealedBorderWidth -
                    radiusX -
                    3;

                int minY =
                    SealedBorderWidth +
                    radiusY +
                    2;

                int maxY =
                    Z.Height -
                    SealedBorderWidth -
                    radiusY -
                    3;

                if (
                    maxX < minX ||
                    maxY < minY
                )
                {
                    return;
                }

                int centerX =
                    rng.Next(
                        minX,
                        maxX + 1
                    );

                int centerY =
                    rng.Next(
                        minY,
                        maxY + 1
                    );

                if (
                    !CanPlaceBulkMass(
                        Z,
                        reserved,
                        centerX,
                        centerY,
                        radiusX,
                        radiusY
                    )
                )
                {
                    continue;
                }

                CarveBulkMass(
                    Z,
                    reserved,
                    centerX,
                    centerY,
                    radiusX,
                    radiusY
                );

                placed++;
            }
        }

        private bool CanPlaceBulkMass(
            Zone Z,
            bool[,] reserved,
            int centerX,
            int centerY,
            int radiusX,
            int radiusY
        )
        {
            for (
                int dx = -radiusX;
                dx <= radiusX;
                dx++
            )
            {
                for (
                    int dy = -radiusY;
                    dy <= radiusY;
                    dy++
                )
                {
                    double nx =
                        (double)dx /
                        radiusX;

                    double ny =
                        (double)dy /
                        radiusY;

                    if (
                        nx * nx +
                        ny * ny >
                        1.0
                    )
                    {
                        continue;
                    }

                    int x =
                        centerX + dx;

                    int y =
                        centerY + dy;

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
                        IsForcedBorder(
                            Z,
                            x,
                            y
                        ) ||
                        reserved[x, y]
                    )
                    {
                        return false;
                    }

                    Cell cell =
                        Z.GetCell(
                            x,
                            y
                        );

                    if (
                        cell == null ||
                        SubterraneanSites
                            .SubterraneanSitesEPGeometry
                            .IsAnyPlaceholder(
                                cell
                            )
                    )
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private void CarveBulkMass(
            Zone Z,
            bool[,] reserved,
            int centerX,
            int centerY,
            int radiusX,
            int radiusY
        )
        {
            for (
                int dx = -radiusX;
                dx <= radiusX;
                dx++
            )
            {
                for (
                    int dy = -radiusY;
                    dy <= radiusY;
                    dy++
                )
                {
                    double nx =
                        (double)dx /
                        radiusX;

                    double ny =
                        (double)dy /
                        radiusY;

                    if (
                        nx * nx +
                        ny * ny >
                        1.0
                    )
                    {
                        continue;
                    }

                    SetDividerCell(
                        Z,
                        reserved,
                        centerX + dx,
                        centerY + dy
                    );
                }
            }
        }


        private void PlaceHorizontalThickness(
            Zone Z,
            bool[,] reserved,
            int centerX,
            int centerY
        )
        {
            int half =
                DividerThickness / 2;

            for (
                int dy = -half;
                dy <= half;
                dy++
            )
            {
                SetDividerCell(
                    Z,
                    reserved,
                    centerX,
                    centerY + dy
                );
            }
        }

        private void PlaceVerticalThickness(
            Zone Z,
            bool[,] reserved,
            int centerX,
            int centerY
        )
        {
            int half =
                DividerThickness / 2;

            for (
                int dx = -half;
                dx <= half;
                dx++
            )
            {
                SetDividerCell(
                    Z,
                    reserved,
                    centerX + dx,
                    centerY
                );
            }
        }

        private void SetDividerCell(
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
                return;
            }

            if (IsForcedBorder(Z, x, y))
                return;

            if (reserved[x, y])
                return;

            Cell cell =
                Z.GetCell(
                    x,
                    y
                );

            if (cell == null)
                return;

            if (
                SubterraneanSites
                    .SubterraneanSitesEPGeometry
                    .IsAnyPlaceholder(
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

                    int x =
                        centerX + dx;

                    int y =
                        centerY + dy;

                    if (
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
                        continue;
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
            }
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
    }

    /// <summary>
    /// Fungus Category-4 floor adapter.
    ///
    /// Fungus owns the vanilla Mushroomy floor recipe. The shared EP floor
    /// system owns universal underground/scar scope and application.
    /// </summary>
    public class SubterraneanSitesFungusFloor :
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
                            .SubterraneanSitesEPFungusTheme
                            .CreateFloorSpec(),
                        EntranceOnly != 0
                    );
        }
    }


   

    /// <summary>
    /// Fungus Category-1 physical content.
    ///
    /// Brooding puffers are deliberately coupled to Fungus Category 1 because
    /// fungal-spore immunity is part of the matching Fungus attunement package.
    ///
    /// These are extradimensional environmental hazards, not EP denizens:
    /// they receive the Fungus dimension identity/faction but no denizen mutation
    /// package, scaling, adaptation, loot rules, or Playerhater.
    /// </summary>
    public class SubterraneanSitesFungusPrimaryPuffers :
        ZoneBuilderSandbox
    {
        public int EntranceOnly = 0;

        public string DecorationThemeKey =
            "Fungus";

        public int MinPuffers = 12;
        public int MaxPuffers = 18;

        public int TransitionExclusionRadius = 4;

        private static readonly string[] PufferBlueprints =
        {
            "FungusPuffer1",
            "FungusPuffer2",
            "FungusPuffer3",
            "FungusPuffer4"
        };

        public bool BuildZone(
            Zone Z
        )
        {
            if (Z == null)
                return true;

            int min =
                Math.Max(
                    0,
                    MinPuffers
                );

            int max =
                Math.Max(
                    min,
                    MaxPuffers
                );

            List<Location2D> anchors =
                SubterraneanSites
                    .SubterraneanSitesEPVerticalTransitions
                    .GetVerticalAnchors(
                        Z.ZoneID
                    );

            System.Random rng =
                new System.Random(
                    Stat.Random(
                        0,
                        2147483646
                    )
                );

            int desired =
                rng.Next(
                    min,
                    max + 1
                );

            for (
                int i = 0;
                i < desired;
                i++
            )
            {
                List<Cell> candidates =
                    CollectCandidates(
                        Z,
                        anchors
                    );

                if (candidates.Count == 0)
                    break;

                Cell cell =
                    candidates[
                        rng.Next(
                            candidates.Count
                        )
                    ];

                string blueprint =
                    PufferBlueprints[
                        rng.Next(
                            PufferBlueprints.Length
                        )
                    ];

                GameObject creature =
                    GameObject.Create(
                        blueprint
                    );

                if (creature == null)
                    continue;

                //
                // Give the puffer the actual generated Fungus extradimensional
                // faction/identity, but not the ordinary EP-denizen package.
                //
                SubterraneanSites
                    .SubterraneanSitesDimensionEngine
                    .ApplyDecorationDimensionIdentity(
                        creature,
                        DecorationThemeKey
                    );

                //
                // Vanilla puffers react according to alliance relationships.
                // These are intended to be active environmental hazards.
                //
                if (creature.Brain != null)
                {
                    creature.Brain.Hostile =
                        true;
                }

                cell.AddObject(
                    creature
                );

                creature.MakeActive();

                //
                // Fungus C1 now owns this location for the remainder of EP construction.
                //
                // Later C2/C5 builders do not need to know that the claimant is a puffer,
                // or even that it came from Fungus. They only need to respect the shared
                // semantic ownership.
                //
                SubterraneanSites
                    .SubterraneanSitesEPReservations
                    .ClaimCell(
                        Z,
                        cell
                    );
            }

            return true;
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
                    CellAvailable(
                        Z,
                        anchors,
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
            Cell cell
        )
        {
            if (cell == null)
                return false;

        //
        // Respect any earlier semantic ownership.
        //
        // Fungus C1 is normally the first theme-owned physical-content pass, so this
        // grid will usually still be empty here. Keeping the check in the C1 builder
        // makes the placement contract complete and prevents it from colliding with
        // any future shared invariant that may legitimately claim space earlier.
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
        // Underground P1 content uses the abstract Category-4 geometry.
        //
        // Entrance P1 content instead uses the localized entrance scar because
        // the origin zone deliberately retains its ordinary geometry outside it.
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
            else if (
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

            foreach (
                GameObject obj
                in cell.GetObjects()
            )
            {
                if (
                    obj != null &&
                    obj.Brain != null
                )
                {
                    return false;
                }
            }

            return true;
        }
    }

    /// <summary>
    /// Fungus Category 2.
    ///
    /// Places passable sleep-gas vents in reasonably open Category-4 floor
    /// space. Each vent continuously maintains SleepGas on its own cell and
    /// allows the normal gas simulation to spread it through the surrounding
    /// terrain.
    ///
    /// C5 solid fungal growth deliberately leaves a small clearance around these
    /// vents so later decoration cannot completely choke them off.
    /// </summary>
    public class SubterraneanSitesFungusSleepVents :
        ZoneBuilderSandbox
    {
        public string VentBlueprint =
            "SubterraneanSitesFungusSleepVent";

        public int MinVents = 8;
        public int MaxVents = 12;

        public int MinSpacing = 5;

        public int TransitionExclusionRadius = 5;

        //
        // A vent can tolerate irregular terrain, but it shouldn't be deliberately
        // placed in a one-cell closet.
        //
        public int MinOpenCardinalNeighbors = 3;
        public int MinOpenNeighborsIn3x3 = 7;

        private class VentCandidate
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

            ClampSettings();

            List<Location2D> anchors =
                SubterraneanSites
                    .SubterraneanSitesEPVerticalTransitions
                    .GetVerticalAnchors(
                        Z.ZoneID
                    );

            List<VentCandidate> candidates =
                FindCandidates(
                    Z,
                    anchors
                );

            if (candidates.Count == 0)
                return true;

            int desired =
                Stat.Random(
                    MinVents,
                    MaxVents
                );

            if (desired > candidates.Count)
                desired = candidates.Count;

            List<VentCandidate> selected =
                SelectSeparatedCandidates(
                    candidates,
                    desired
                );

            foreach (
                VentCandidate candidate
                in selected
            )
            {
                PlaceVent(
                    Z,
                    candidate
                );
            }

            return true;
        }

        private void ClampSettings()
        {
            if (MinVents < 0)
                MinVents = 0;

            if (MaxVents < MinVents)
                MaxVents = MinVents;

            if (MinSpacing < 0)
                MinSpacing = 0;

            if (TransitionExclusionRadius < 0)
                TransitionExclusionRadius = 0;

            if (MinOpenCardinalNeighbors < 0)
                MinOpenCardinalNeighbors = 0;

            if (MinOpenCardinalNeighbors > 4)
                MinOpenCardinalNeighbors = 4;

            if (MinOpenNeighborsIn3x3 < 1)
                MinOpenNeighborsIn3x3 = 1;

            if (MinOpenNeighborsIn3x3 > 9)
                MinOpenNeighborsIn3x3 = 9;
        }

        private List<VentCandidate> FindCandidates(
            Zone Z,
            List<Location2D> anchors
        )
        {
            List<VentCandidate> result =
                new List<VentCandidate>();

            foreach (
                Cell cell
                in Z.GetCells()
            )
            {
                if (
                    !CellAvailable(
                        Z,
                        anchors,
                        cell
                    )
                )
                {
                    continue;
                }

                result.Add(
                    new VentCandidate
                    {
                        X = cell.X,
                        Y = cell.Y
                    }
                );
            }

            return result;
        }

        private bool CellAvailable(
            Zone Z,
            List<Location2D> anchors,
            Cell cell
        )
        {
            if (cell == null)
                return false;

            //
            // C2 runs after C1 physical content.
            //
            // The vent needs more than its own tile: its surrounding 3x3 area is
            // functional clearance for gas dispersal. Reject the candidate if any
            // part of that footprint is already owned by earlier EP content.
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

            if (cell.HasSpawnBlocker())
                return false;

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

            foreach (
                GameObject obj
                in cell.GetObjects()
            )
            {
                if (
                    obj != null &&
                    obj.Brain != null
                )
                {
                    return false;
                }
            }

            //
            // First requirement:
            // at least three immediately-open cardinal exits.
            //
            if (
                CountOpenCardinalNeighbors(
                    Z,
                    cell
                ) <
                MinOpenCardinalNeighbors
            )
            {
                return false;
            }

            //
            // Second requirement:
            // most of the surrounding 3x3 neighborhood must also be open.
            //
            // This rejects little pockets even when they happen to have several
            // narrow exits.
            //
            if (
                CountOpenCellsIn3x3(
                    Z,
                    cell
                ) <
                MinOpenNeighborsIn3x3
            )
            {
                return false;
            }

            return true;
        }

        private int CountOpenCardinalNeighbors(
            Zone Z,
            Cell center
        )
        {
            if (
                Z == null ||
                center == null
            )
            {
                return 0;
            }

            int count = 0;

            CountOpen(
                Z.GetCell(
                    center.X,
                    center.Y - 1
                ),
                ref count
            );

            CountOpen(
                Z.GetCell(
                    center.X + 1,
                    center.Y
                ),
                ref count
            );

            CountOpen(
                Z.GetCell(
                    center.X,
                    center.Y + 1
                ),
                ref count
            );

            CountOpen(
                Z.GetCell(
                    center.X - 1,
                    center.Y
                ),
                ref count
            );

            return count;
        }

        private int CountOpenCellsIn3x3(
            Zone Z,
            Cell center
        )
        {
            if (
                Z == null ||
                center == null
            )
            {
                return 0;
            }

            int count = 0;

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

                    if (
                        cell != null &&
                        SubterraneanSites
                            .SubterraneanSitesEPGeometry
                            .IsOpenGeometryCell(
                                cell
                            ) &&
                        !cell.IsSolid(true)
                    )
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        private void CountOpen(
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
                !cell.IsSolid(true)
            )
            {
                count++;
            }
        }

        private List<VentCandidate>
            SelectSeparatedCandidates(
                List<VentCandidate> candidates,
                int desired
            )
        {
            List<VentCandidate> pool =
                new List<VentCandidate>(
                    candidates
                );

            List<VentCandidate> selected =
                new List<VentCandidate>();

            while (
                pool.Count > 0 &&
                selected.Count < desired
            )
            {
                int index =
                    Stat.Random(
                        0,
                        pool.Count - 1
                    );

                VentCandidate candidate =
                    pool[index];

                pool.RemoveAt(
                    index
                );

                if (
                    IsFarEnough(
                        candidate,
                        selected
                    )
                )
                {
                    selected.Add(
                        candidate
                    );
                }
            }

            return selected;
        }

        private bool IsFarEnough(
            VentCandidate candidate,
            List<VentCandidate> selected
        )
        {
            foreach (
                VentCandidate other
                in selected
            )
            {
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

        private void PlaceVent(
            Zone Z,
            VentCandidate candidate
        )
        {
            if (
                Z == null ||
                candidate == null
            )
            {
                return;
            }

            Cell cell =
                Z.GetCell(
                    candidate.X,
                    candidate.Y
                );

            if (
                cell == null ||
                !cell.IsEmptyOfSolid()
            )
            {
                return;
            }

            GameObject vent =
                GameObjectFactory
                    .Factory
                    .CreateObject(
                        VentBlueprint
                    );

            if (vent == null)
                return;

           cell.AddObject(
                vent
            );

            vent.MakeActive();

            SubterraneanSitesFungusSleepVent part =
                vent.GetPart<
                    SubterraneanSitesFungusSleepVent
                >();

            if (part != null)
            {
                part.Emit();
            }

            //
            // The physical vent occupies only the center cell, but its surrounding
            // open space is part of the Category-2 hazard's functional footprint.
            //
            // Later C5 content may decorate around this area, but it must not occupy
            // these cells and deliberately choke off the sleep-gas source.
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
    }




    /// <summary>
    /// Fungus Category 5.
    ///
    /// Physical navigation comes from destructible solid mushroom colonies
    /// layered over C4's broadly-open structural geometry.
    /// </summary>
    public class SubterraneanSitesFungusDecorations :
        ZoneBuilderSandbox
    {
        public int EntranceOnly = 0;

        public string DecorationThemeKey =
            "Fungus";

        public int TransitionExclusionRadius = 4;

        //
        // Underground density.
        //
        public int MinColonies = 10;
        public int MaxColonies = 16;

        public int MinColonyCells = 35;
        public int MaxColonyCells = 60;

        public int MushroomPlacementPercent = 85;

        public int MinSoupPatches = 4;
        public int MaxSoupPatches = 6;

        public int MinSoupCells = 14;
        public int MaxSoupCells = 24;

        public int MinDrowsingUrchins = 2;
        public int MaxDrowsingUrchins = 4;

        public int MinAddlingUrchins = 2;
        public int MaxAddlingUrchins = 4;

        //
        // Entrance preview density.
        //
        public int MinEntranceColonies = 2;
        public int MaxEntranceColonies = 3;

        public int MinEntranceColonyCells = 7;
        public int MaxEntranceColonyCells = 14;

        public int MinEntranceSoupPatches = 1;
        public int MaxEntranceSoupPatches = 1;

        public int MinEntranceSoupCells = 5;
        public int MaxEntranceSoupCells = 9;

        public int MinEntranceDrowsingUrchins = 0;
        public int MaxEntranceDrowsingUrchins = 1;

        public int MinEntranceAddlingUrchins = 0;
        public int MaxEntranceAddlingUrchins = 1;

        public string BrightshroomBlueprint =
            "Brightshroom";

        public int MinBrightshroomPatches = 3;
        public int MaxBrightshroomPatches = 5;

        public int MinBrightshroomsPerPatch = 5;
        public int MaxBrightshroomsPerPatch = 12;

        public int MinEntranceBrightshroomPatches = 1;
        public int MaxEntranceBrightshroomPatches = 2;

        public int MinEntranceBrightshroomsPerPatch = 3;
        public int MaxEntranceBrightshroomsPerPatch = 6;

        private static readonly string[][] ColonyPalettes =
        {
            new string[] { "C", "c" },
            new string[] { "G", "g" },
            new string[] { "Y", "y" },
            new string[] { "R", "r" },
            new string[] { "M", "m" },
            new string[] { "B", "b" }
        };

        public bool BuildZone(
            Zone Z
        )
        {
            if (Z == null)
                return true;

            ClampSettings();

            System.Random rng =
                new System.Random(
                    Stat.Random(
                        0,
                        2147483646
                    )
                );

            List<Location2D> anchors =
                SubterraneanSites
                    .SubterraneanSitesEPVerticalTransitions
                    .GetVerticalAnchors(
                        Z.ZoneID
                    );

            bool[,] colonyReserved =
                new bool[
                    Z.Width,
                    Z.Height
                ];

            SubterraneanSites
                .SubterraneanSitesEPPlacement
                .ReserveAroundAnchors(
                    colonyReserved,
                    Z,
                    anchors,
                    TransitionExclusionRadius
                );

            //
            // Pools first.
            //
            // Primordial soup may mix with other liquids, but its cells are exclusive
            // to discrete objects. Claim those cells so later C5 decorations leave the
            // pools themselves unobstructed.
            //
            PlaceSoupPatches(
                Z,
                anchors,
                rng
            );

            PlaceFungalColonies(
                Z,
                anchors,
                colonyReserved,
                rng
            );

            PlaceBrightshroomPatches(
                Z,
                anchors,
                rng
            );

            PlaceCreatureGroup(
                Z,
                anchors,
                new string[]
                {
                    "Drowsing Urchin"
                },
                EntranceOnly != 0
                    ? MinEntranceDrowsingUrchins
                    : MinDrowsingUrchins,
                EntranceOnly != 0
                    ? MaxEntranceDrowsingUrchins
                    : MaxDrowsingUrchins,
                rng
            );

            PlaceCreatureGroup(
                Z,
                anchors,
                new string[]
                {
                    "Addling Urchin"
                },
                EntranceOnly != 0
                    ? MinEntranceAddlingUrchins
                    : MinAddlingUrchins,
                EntranceOnly != 0
                    ? MaxEntranceAddlingUrchins
                    : MaxAddlingUrchins,
                rng
            );

            return true;
        }

        private void ClampSettings()
        {
            ClampRange(
                ref MinColonies,
                ref MaxColonies
            );

            ClampRange(
                ref MinEntranceColonies,
                ref MaxEntranceColonies
            );

            ClampRange(
                ref MinSoupPatches,
                ref MaxSoupPatches
            );

            ClampRange(
                ref MinEntranceSoupPatches,
                ref MaxEntranceSoupPatches
            );

            ClampRange(
                ref MinDrowsingUrchins,
                ref MaxDrowsingUrchins
            );

            ClampRange(
                ref MinEntranceDrowsingUrchins,
                ref MaxEntranceDrowsingUrchins
            );

            ClampRange(
                ref MinAddlingUrchins,
                ref MaxAddlingUrchins
            );

            ClampRange(
                ref MinEntranceAddlingUrchins,
                ref MaxEntranceAddlingUrchins
            );
            ClampRange(
                ref MinBrightshroomPatches,
                ref MaxBrightshroomPatches
            );

            ClampRange(
                ref MinEntranceBrightshroomPatches,
                ref MaxEntranceBrightshroomPatches
            );

            if (MinColonyCells < 1)
                MinColonyCells = 1;

            if (MaxColonyCells < MinColonyCells)
                MaxColonyCells = MinColonyCells;

            if (MinEntranceColonyCells < 1)
                MinEntranceColonyCells = 1;

            if (
                MaxEntranceColonyCells <
                MinEntranceColonyCells
            )
            {
                MaxEntranceColonyCells =
                    MinEntranceColonyCells;
            }

            if (MinSoupCells < 1)
                MinSoupCells = 1;

            if (MaxSoupCells < MinSoupCells)
                MaxSoupCells = MinSoupCells;

            if (MinEntranceSoupCells < 1)
                MinEntranceSoupCells = 1;

            if (
                MaxEntranceSoupCells <
                MinEntranceSoupCells
            )
            {
                MaxEntranceSoupCells =
                    MinEntranceSoupCells;
            }

            if (MushroomPlacementPercent < 0)
                MushroomPlacementPercent = 0;

            if (MushroomPlacementPercent > 100)
                MushroomPlacementPercent = 100;

            if (MinBrightshroomsPerPatch < 1)
                MinBrightshroomsPerPatch = 1;

            if (
                MaxBrightshroomsPerPatch <
                MinBrightshroomsPerPatch
            )
            {
                MaxBrightshroomsPerPatch =
                    MinBrightshroomsPerPatch;
            }

            if (MinEntranceBrightshroomsPerPatch < 1)
                MinEntranceBrightshroomsPerPatch = 1;

            if (
                MaxEntranceBrightshroomsPerPatch <
                MinEntranceBrightshroomsPerPatch
            )
            {
                MaxEntranceBrightshroomsPerPatch =
                    MinEntranceBrightshroomsPerPatch;
            }
        }

        private void ClampRange(
            ref int min,
            ref int max
        )
        {
            if (min < 0)
                min = 0;

            if (max < min)
                max = min;
        }

        private void PlaceSoupPatches(
            Zone Z,
            List<Location2D> anchors,
            System.Random rng
        )
        {
            int minPatches =
                EntranceOnly != 0
                    ? MinEntranceSoupPatches
                    : MinSoupPatches;

            int maxPatches =
                EntranceOnly != 0
                    ? MaxEntranceSoupPatches
                    : MaxSoupPatches;

            int minCells =
                EntranceOnly != 0
                    ? MinEntranceSoupCells
                    : MinSoupCells;

            int maxCells =
                EntranceOnly != 0
                    ? MaxEntranceSoupCells
                    : MaxSoupCells;

            int count =
                rng.Next(
                    minPatches,
                    maxPatches + 1
                );

            for (
                int i = 0;
                i < count;
                i++
            )
            {
                List<Cell> seeds =
                    CollectSoupCandidates(
                        Z,
                        anchors
                    );

                if (seeds.Count == 0)
                    return;

                int target =
                    rng.Next(
                        minCells,
                        maxCells + 1
                    );

                HashSet<Cell> patch =
                    SubterraneanSites
                        .SubterraneanSitesEPPlacement
                        .GrowBestPatch(
                            seeds,
                            target,
                            Math.Min(
                                12,
                                seeds.Count
                            ),
                            delegate(Cell cell)
                            {
                                return
                                    CellAvailableForSoup(
                                        Z,
                                        anchors,
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

                    bool placed =
                        SubterraneanSites
                            .SubterraneanSitesEPLiquids
                            .AddOrMixLiquid(
                                cell,
                                "proteangunk",
                                "ProteanDeepPool"
                            );

                    if (!placed)
                        continue;

                    //
                    // Primordial soup is exclusive to discrete objects, but not to liquids.
                    //
                    // Later object placement must stay out of this cell. Permissive liquid
                    // painters such as Ooze and Fire deliberately ignore this claim.
                    //
                    SubterraneanSites
                        .SubterraneanSitesEPReservations
                        .ClaimCell(
                            Z,
                            cell
                        );
                }
            }
        }

        private List<Cell> CollectSoupCandidates(
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
                    CellAvailableForSoup(
                        Z,
                        anchors,
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

        private bool CellAvailableForSoup(
            Zone Z,
            List<Location2D> anchors,
            Cell cell
        )
        {
            if (
                !CellInDecorationScope(
                    Z,
                    cell
                )
            )
            {
                return false;
            }

            //
            // Primordial soup is C5 liquid content, but its eventual cells are
            // exclusive to discrete objects.
            //
            // Respect higher-priority C1/C2 ownership. Do NOT reject existing liquid:
            // Ooze, Fire, and other compatible liquids are allowed to mix with it.
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
                IsNearTransition(
                    cell,
                    anchors
                )
            )
            {
                return false;
            }

            if (IsTransitionObjectCell(cell))
                return false;

            return true;
        }

       
       
    
        private void PlaceFungalColonies(
            Zone Z,
            List<Location2D> anchors,
            bool[,] reserved,
            System.Random rng
        )
        {
            int minColonies =
                EntranceOnly != 0
                    ? MinEntranceColonies
                    : MinColonies;

            int maxColonies =
                EntranceOnly != 0
                    ? MaxEntranceColonies
                    : MaxColonies;

            int minCells =
                EntranceOnly != 0
                    ? MinEntranceColonyCells
                    : MinColonyCells;

            int maxCells =
                EntranceOnly != 0
                    ? MaxEntranceColonyCells
                    : MaxColonyCells;

            int count =
                rng.Next(
                    minColonies,
                    maxColonies + 1
                );

            for (
                int i = 0;
                i < count;
                i++
            )
            {
                List<Cell> seeds =
                    CollectColonyCandidates(
                        Z,
                        anchors,
                        reserved
                    );

                if (seeds.Count == 0)
                    return;

                int target =
                    rng.Next(
                        minCells,
                        maxCells + 1
                    );

                HashSet<Cell> patch =
                    SubterraneanSites
                        .SubterraneanSitesEPPlacement
                        .GrowBestPatch(
                            seeds,
                            target,
                            Math.Min(
                                12,
                                seeds.Count
                            ),
                            delegate(Cell cell)
                            {
                                return
                                    CellAvailableForColony(
                                        Z,
                                        anchors,
                                        reserved,
                                        cell
                                    );
                            },
                            rng
                        );

                if (patch.Count == 0)
                    continue;

                string[] palette =
                    ColonyPalettes[
                        rng.Next(
                            ColonyPalettes.Length
                        )
                    ];

                foreach (
                    Cell cell
                    in patch
                )
                {
                    if (cell == null)
                        continue;

                    reserved[
                        cell.X,
                        cell.Y
                    ] = true;

                    if (
                        rng.Next(100) >=
                        MushroomPlacementPercent
                    )
                    {
                        continue;
                    }

                    if (!cell.IsEmptyOfSolid())
                        continue;

                    string blueprint =
                        rng.Next(100) < 60
                            ? "Spotted Shagspook"
                            : "Dandy Cap";

                    GameObject mushroom =
                        GameObject.Create(
                            blueprint
                        );

                    if (mushroom == null)
                        continue;

                    if (mushroom.Render != null)
                    {
                        mushroom.Render
                            .SetForegroundColor(
                                palette[0]
                            );

                        mushroom.Render.DetailColor =
                            palette[1];
                    }

                    cell.AddObject(
                        mushroom
                    );

                    //
                    // An actual discrete fungal object now occupies this cell.
                    //
                    // Do not globally claim the entire colony patch: some patch cells are
                    // intentionally empty because MushroomPlacementPercent is less than 100.
                    //
                    SubterraneanSites
                        .SubterraneanSitesEPReservations
                        .ClaimCell(
                            Z,
                            cell
                        );
                }

                ReservePatchOnly(
                    reserved,
                    patch
                );
            }
        }

        private List<Cell> CollectColonyCandidates(
            Zone Z,
            List<Location2D> anchors,
            bool[,] reserved
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
                    CellAvailableForColony(
                        Z,
                        anchors,
                        reserved,
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

        private bool CellAvailableForColony(
            Zone Z,
            List<Location2D> anchors,
            bool[,] reserved,
            Cell cell
        )
        {
            if (
                !CellInDecorationScope(
                    Z,
                    cell
                )
            )
            {
                return false;
            }

            //
            // Respect all earlier semantic ownership:
            // C1 objects, C2 functional footprints, and exclusive C5 pools.
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

            //
            // This remains Fungus C5's own local patch-spacing map.
            // It is separate from the cross-theme semantic reservation system.
            //
            if (reserved[x, y])
                return false;

            if (!cell.IsEmptyOfSolid())
                return false;

            if (
                IsNearTransition(
                    cell,
                    anchors
                )
            )
            {
                return false;
            }

            if (IsTransitionObjectCell(cell))
                return false;

            //
            // Do not reject liquid generically.
            //
            // Fungal growth may occupy permissive Ooze/Fire liquid cells.
            // Exclusive Fungus/Cold pools are excluded through shared claims instead.
            //
            return true;
        }

        private void ReservePatchOnly(
            bool[,] reserved,
            HashSet<Cell> patch
        )
        {
            if (
                reserved == null ||
                patch == null
            )
            {
                return;
            }

            foreach (
                Cell cell
                in patch
            )
            {
                if (cell == null)
                    continue;

                reserved[
                    cell.X,
                    cell.Y
                ] = true;
            }
        }

        private void PlaceBrightshroomPatches(
            Zone Z,
            List<Location2D> anchors,
            System.Random rng
        )
        {
            int minPatches =
                EntranceOnly != 0
                    ? MinEntranceBrightshroomPatches
                    : MinBrightshroomPatches;

            int maxPatches =
                EntranceOnly != 0
                    ? MaxEntranceBrightshroomPatches
                    : MaxBrightshroomPatches;

            int minPerPatch =
                EntranceOnly != 0
                    ? MinEntranceBrightshroomsPerPatch
                    : MinBrightshroomsPerPatch;

            int maxPerPatch =
                EntranceOnly != 0
                    ? MaxEntranceBrightshroomsPerPatch
                    : MaxBrightshroomsPerPatch;

            int patchCount =
                rng.Next(
                    minPatches,
                    maxPatches + 1
                );

            for (
                int i = 0;
                i < patchCount;
                i++
            )
            {
                List<Cell> seeds =
                    CollectBrightshroomCandidates(
                        Z,
                        anchors
                    );

                if (seeds.Count == 0)
                    return;

                int target =
                    rng.Next(
                        minPerPatch,
                        maxPerPatch + 1
                    );

                HashSet<Cell> patch =
                    SubterraneanSites
                        .SubterraneanSitesEPPlacement
                        .GrowBestPatch(
                            seeds,
                            target,
                            Math.Min(
                                12,
                                seeds.Count
                            ),
                            delegate(Cell cell)
                            {
                                return
                                    CellAvailableForBrightshroom(
                                        Z,
                                        anchors,
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
                    if (
                        cell == null ||
                        !CellAvailableForBrightshroom(
                            Z,
                            anchors,
                            cell
                        )
                    )
                    {
                        continue;
                    }

                    cell.AddObject(
                        BrightshroomBlueprint
                    );

                    SubterraneanSites
                        .SubterraneanSitesEPReservations
                        .ClaimCell(
                            Z,
                            cell
                        );
                }
            }
        }
        private List<Cell> CollectBrightshroomCandidates(
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
                    CellAvailableForBrightshroom(
                        Z,
                        anchors,
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

        private bool CellAvailableForBrightshroom(
            Zone Z,
            List<Location2D> anchors,
            Cell cell
        )
        {
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

            if (!cell.IsEmptyOfSolid())
                return false;

            if (
                IsNearTransition(
                    cell,
                    anchors
                )
            )
            {
                return false;
            }

            if (IsTransitionObjectCell(cell))
                return false;

            return true;
        }

       

        private void PlaceCreatureGroup(
            Zone Z,
            List<Location2D> anchors,
            string[] blueprints,
            int minCount,
            int maxCount,
            System.Random rng
        )
        {
            if (
                blueprints == null ||
                blueprints.Length == 0
            )
            {
                return;
            }

            if (minCount < 0)
                minCount = 0;

            if (maxCount < minCount)
                maxCount = minCount;

            int desired =
                rng.Next(
                    minCount,
                    maxCount + 1
                );

            for (
                int i = 0;
                i < desired;
                i++
            )
            {
                List<Cell> candidates =
                    CollectCreatureCells(
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
                    blueprints[
                        rng.Next(
                            blueprints.Length
                        )
                    ];

                GameObject creature =
                    GameObject.Create(
                        blueprint
                    );

                if (creature == null)
                    continue;

                SubterraneanSites
                    .SubterraneanSitesDimensionEngine
                    .ApplyDecorationDimensionIdentity(
                        creature,
                        DecorationThemeKey
                    );
                //
                // Fungus C5's living decorations are environmental hazards.
                // Preserve their extradimensional faction identity, but make them
                // innately hostile so their vanilla proximity-triggered gas/spore
                // behavior actually functions.
                //
                // This is deliberately NOT Playerhater and does not give them any
                // EP-denizen adaptation or mutation package.
                //
                if (creature.Brain != null)
                {
                    creature.Brain.Hostile = true;
                }

                cell.AddObject(
                    creature
                );

                creature.MakeActive();

                SubterraneanSites
                    .SubterraneanSitesEPReservations
                    .ClaimCell(
                        Z,
                        cell
                    );
            }
        }

        private List<Cell> CollectCreatureCells(
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
                    !CellInDecorationScope(
                        Z,
                        cell
                    )
                )
                {
                    continue;
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
                    continue;
                }

                if (!cell.IsEmptyOfSolid())
                    continue;

                if (cell.HasSpawnBlocker())
                    continue;

                if (
                    IsNearTransition(
                        cell,
                        anchors
                    )
                )
                {
                    continue;
                }

                if (IsTransitionObjectCell(cell))
                    continue;

                bool hasCreature =
                    false;

                foreach (
                    GameObject obj
                    in cell.GetObjects()
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

                result.Add(
                    cell
                );
            }

            return result;
        }

        private bool CellInDecorationScope(
            Zone Z,
            Cell cell
        )
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

        private bool IsNearTransition(
            Cell cell,
            List<Location2D> anchors
        )
        {
            if (cell == null)
                return true;

            return
                SubterraneanSites
                    .SubterraneanSitesEPPlacement
                    .IsNearAnyAnchor(
                        cell.X,
                        cell.Y,
                        anchors,
                        TransitionExclusionRadius
                    );
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
    }
}


namespace XRL.World.Parts
{

    [Serializable]
    public class SubterraneanSitesFungusSleepVent :
        IPart
    {
        public string GasBlueprint =
            "SleepGas80";

        public int GasDensity = 80;

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
                "EndTurn"
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
            if (E.ID == "EndTurn")
            {
                Emit();
            }

            return base.FireEvent(E);
        }

        public void Emit()
        {
            Cell cell =
                ParentObject == null
                    ? null
                    : ParentObject.GetCurrentCell();

            if (cell == null)
                return;

            MaintainSleepGas(
                cell
            );
        }

        private void MaintainSleepGas(
            Cell cell
        )
        {
            if (cell == null)
                return;

            foreach (
                GameObject obj
                in cell.GetObjects()
            )
            {
                if (
                    obj == null ||
                    obj == ParentObject
                )
                {
                    continue;
                }

                Gas gas =
                    obj.GetPart<Gas>();

                if (gas == null)
                    continue;

                //
                // Don't overwrite another gas occupying the vent.
                //
                if (
                    gas.GasType !=
                    "SleepGas"
                )
                {
                    return;
                }

                //
                // Continually replenish the source cell.
                // Qud handles diffusion away from here.
                //
                if (
                    gas.Density <
                    GasDensity
                )
                {
                    gas.Density =
                        GasDensity;
                }

                return;
            }

            GameObject gasObject =
                GameObjectFactory
                    .Factory
                    .CreateObject(
                        GasBlueprint
                    );

            if (gasObject == null)
                return;

            Gas gasPart =
                gasObject.GetPart<Gas>();

            if (gasPart != null)
            {
                gasPart.Density =
                    GasDensity;

                gasPart.Creator =
                    ParentObject;
            }

            cell.AddObject(
                gasObject
            );
        }
    }
  
   
}