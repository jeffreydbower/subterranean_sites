using System;
using System.Collections.Generic;
using Genkit;
using XRL;
using XRL.Core;
using XRL.World;
using XRL.World.Parts;
using ConsoleLib.Console;
using Qud.UI;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.ZoneBuilders.Utility;

namespace SubterraneanSites
{
    /// <summary>
    /// LIGHT DIMENSION
    ///
    /// FIRST STATIC/VISUAL PASS
    ///
    /// Category 1:
    ///   scattered crystalline roots.
    ///
    /// Category 2:
    ///   additional crystalline-root networks plus reflecting laser emitter.
    ///
    /// Category 3:
    ///   Black Marble sealed/core material;
    ///   exposed walls become Chavvah Taproot.
    ///
    /// Category 4:
    ///   mostly-open field divided by overlapping asymmetric
    ///   rectangular crystalline wall structures.
    ///
    /// Category 5:
    ///   sparse crystals, psychal rhythm rocks and Chavvah leaves.
    ///
    /// Pending:
    ///   - zone-wide illumination / clairvoyance
    ///   - pre-attunement Agility penalty
    ///   - Light Manipulation attunement
    ///   - reflected beam Category-2 hazard
    /// </summary>
    internal sealed class SubterraneanSitesEPLightTheme :
        ISubterraneanSitesEPCategoryProvider,
        ISubterraneanSitesEPPrimaryObjectProvider,
        ISubterraneanSitesEPAttunementProvider,
        ISubterraneanSitesEPDenizenAdaptationProvider,
        ISubterraneanSitesEPSignatureMutationProvider
        {
        public string ThemeKey
        {
            get { return "Light"; }
        }

        public string SignatureMutationClass
        {
            get { return "LightManipulation"; }
        }

        internal const int ReflectionBonus =
            50;

        public int MinimumHoleSeparation
        {
            get { return 25; }
        }

        public void ApplyDenizenAdaptation(
            GameObject creature
        )
        {
            if (creature == null)
                return;

            XRL.World.Effects
                .SubterraneanSitesEPLightDenizenAdaptationEffect existing =
                    creature.GetEffectDescendedFrom<
                        XRL.World.Effects
                            .SubterraneanSitesEPLightDenizenAdaptationEffect
                    >();

            if (existing == null)
            {
                creature.ApplyEffect(
                    new XRL.World.Effects
                        .SubterraneanSitesEPLightDenizenAdaptationEffect()
                );
            }
        }

        internal static bool HandleReflectionBonus(
            GameObject actor,
            Event E
        )
        {
            if (
                actor == null ||
                E == null ||
                E.ID != "RefractLight"
            )
            {
                return true;
            }

            XRL.World.Parts.Mutation.LightManipulation mutation =
                actor.GetPart(
                    "LightManipulation"
                ) as XRL.World.Parts.Mutation.LightManipulation;

            if (mutation == null)
                return true;


            //
            // Vanilla Light Manipulation already receives RefractLight first.
            // This supplemental roll operates over the remaining failures so the
            // combined chance is vanilla chance + 50 percentage points, capped
            // at 100%.
            //
            int baseChance =
                Math.Max(
                    0,
                    Math.Min(
                        100,
                        mutation.GetReflectChance()
                    )
                );

            int targetChance =
                Math.Min(
                    100,
                    baseChance +
                    ReflectionBonus
                );

            int additionalChance =
                targetChance -
                baseChance;

            int remainingChance =
                100 -
                baseChance;

            if (
                additionalChance <= 0 ||
                remainingChance <= 0
            )
            {
                return true;
            }

            if (
                Stat.Random(
                    1,
                    remainingChance
                ) >
                additionalChance
            )
            {
                return true;
            }


            //
            // Match vanilla LightManipulation's successful RefractLight handling.
            //
            E.SetParameter(
                "By",
                actor
            );

            E.SetParameter(
                "Direction",
                (int)(float)E.GetParameter(
                    "Angle"
                ) + 180
            );

            E.SetParameter(
                "Verb",
                "reflect"
            );

            return false;
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

            floor.Render.Tile =
                "Terrain/sw_hex_dotted_1.bmp";

            floor.Render.ColorString =
                "&K";

            floor.Render.DetailColor =
                "w";
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
            // Light C1 is a persistent runtime player environment.
            //
            // RequireSystem is idempotent. The system itself checks the actual
            // Category-1 owner whenever the player's zone/state changes.
            //
            The.Game.RequireSystem<
                SubterraneanSitesEPLightEnvironmentSystem
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
                    .SubterraneanSitesEPLightAttunementEffect(
                        SubterraneanSitesEPAttunementSystem
                            .DefaultDuration,
                        mutationLevel
                    );
                    
            successMessage =
                "Attunement grants:\n" +
                "Light Manipulation (level " +
                mutationLevel.ToString() +
                ")\n" +
                "+50 percentage points to light reflection chance\n" +
                "Sensory overload is suppressed.";

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
                "SubterraneanSitesLightRoots",
                "SeedVariant", "C1",
                "MinNetworks", "8",
                "MaxNetworks", "11"
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
                "SubterraneanSitesLightRoots",
                "EntranceOnly", "1",
                "SeedVariant", "C1"
            );
        }


        public void RegisterCategory2(
            SubterraneanSitesEPLayerContext context
        )
        {
            if (context == null)
                return;

            //
            // C2 adds a second full crystalline-root pass.
            // It uses the same Light-specific root mechanic as C1 but a
            // separate deterministic seed stream, so the networks are independent.
            //
            The.ZoneManager.AddZonePostBuilder(
                context.ZoneId,
                "SubterraneanSitesLightRoots",
                "SeedVariant", "C2",
                "MinNetworks", "8",
                "MaxNetworks", "11"
            );

            The.ZoneManager.AddZonePostBuilder(
                context.ZoneId,
                "SubterraneanSitesLightEmitterBuilder",
                "Tier", context.Tier.ToString()
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
                "SubterraneanSitesLightMaterials"
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
                "SubterraneanSitesLightLayout"
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
                "SubterraneanSitesLightFloor"
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
                "SubterraneanSitesLightDecorations"
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
                "SubterraneanSitesLightFloor",
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
                "SubterraneanSitesLightDecorations",
                "EntranceOnly", "1"
            );
        }
    }
}

namespace SubterraneanSites
{
    /// <summary>
    /// Light Category-1 runtime environment.
    ///
    /// Light as the actual Category-1 owner floods the player's senses with
    /// whole-zone illumination and visibility.
    ///
    /// Unattuned:
    ///   -3 Agility.
    ///
    /// Attuned:
    ///   the Agility penalty is suppressed, while whole-zone vision remains.
    /// </summary>
    [Serializable]
    public class SubterraneanSitesEPLightEnvironmentSystem :
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

            bool lightEnvironment =
                string.Equals(
                    category1,
                    "Light",
                    StringComparison.Ordinal
                );

            XRL.World.Effects
                .SubterraneanSitesEPLightEnvironmentEffect environment =
                    player.GetEffectDescendedFrom<
                        XRL.World.Effects
                            .SubterraneanSitesEPLightEnvironmentEffect
                    >();

            //
            // Outside a Light-primary EP, none of the Light C1 environment
            // should remain on the player.
            //
            if (!lightEnvironment)
            {
                if (environment != null)
                {
                    player.RemoveEffect(
                        environment
                    );
                }

                return;
            }

            bool lightAttuned =
                SubterraneanSitesEPAttunementSystem
                    .IsAttunedTo(
                        player,
                        "Light"
                    );

            //
            // Unlike Fungus shimmering, the Light environment itself remains
            // active while attuned because whole-zone vision is intrinsic C1
            // behavior rather than the pre-attunement penalty.
            //
            if (environment == null)
            {
                environment =
                    new XRL.World.Effects
                        .SubterraneanSitesEPLightEnvironmentEffect();

                if (
                    !player.ApplyEffect(
                        environment
                    )
                )
                {
                    return;
                }
            }

            environment.SynchronizeAttunement(
                lightAttuned
            );
        }
    }
}


namespace XRL.World.Effects
{
    /// <summary>
    /// Persistent Light Category-1 sensory environment.
    ///
    /// The environment system owns this effect's lifetime.
    /// Whole-zone light/visibility remains active regardless of attunement;
    /// attunement only suppresses the Agility penalty.
    /// </summary>
    [Serializable]
    public class SubterraneanSitesEPLightEnvironmentEffect :
        Effect
    {
        public const int AgilityPenalty =
            3;

        public bool PenaltyApplied =
            false;


        public SubterraneanSitesEPLightEnvironmentEffect()
        {
            DisplayName =
                "{{Y|senses overloaded}}";

            //
            // Environment system owns lifetime.
            //
            Duration = 1;
        }


        public override bool UseStandardDurationCountdown()
        {
            return false;
        }


        public override string GetDetails()
        {
            if (PenaltyApplied)
            {
                return
                    "-3 Agility\n" +
                    "Your senses are flooded with extradimensional light.";
            }

            return
                "Your senses are flooded with extradimensional light.";
        }


        public override bool Apply(
            GameObject Object
        )
        {
            if (
                Object == null ||
                !Object.HasStat(
                    "Agility"
                )
            )
            {
                return false;
            }

            //
            // Do not decide attunement state here.
            // The owning environment system synchronizes it immediately after
            // application and again on zone activation/end turn.
            //
            return true;
        }

        public void SynchronizeAttunement(
            bool attuned
        )
        {
            if (attuned)
            {
                //
                // The Light environment remains active so whole-zone vision
                // continues, but attunement prevents the sensory overload.
                //
                DisplayName =
                    "";

                if (PenaltyApplied)
                {
                    StatShifter.RemoveStatShifts();

                    PenaltyApplied =
                        false;
                }

                return;
            }

            //
            // Unattuned Light C1 overloads the player's senses.
            //
            DisplayName =
                "{{Y|senses overloaded}}";

            if (!PenaltyApplied)
            {
                if (!PenaltyApplied)
                {
                    StatShifter.SetStatShift(
                        "Agility",
                        -AgilityPenalty
                    );

                    PenaltyApplied =
                        true;

                    GameObject actor =
                        base.Object;

                    if (
                        actor != null &&
                        actor.IsPlayer()
                    )
                    {
                        Popup.Show(
                            "Your senses overload."
                        );
                    }
                }
            }
        }





        public override bool WantEvent(
            int ID,
            int cascade
        )
        {
            return
                base.WantEvent(
                    ID,
                    cascade
                ) ||
                ID == BeforeRenderEvent.ID;
        }


        public override bool HandleEvent(
            BeforeRenderEvent E
        )
        {
            GameObject actor =
                base.Object;

            if (
                actor != null &&
                actor.IsPlayer() &&
                actor.CurrentZone != null
            )
            {
                string category1 =
                    SubterraneanSites
                        .SubterraneanSitesEPAttunementSystem
                        .GetCategory1Theme(
                            actor.CurrentZone
                        );

                if (
                    string.Equals(
                        category1,
                        "Light",
                        StringComparison.Ordinal
                    )
                )
                {
                    Zone zone =
                        actor.CurrentZone;

                    //
                    // Light C1 overloads the player's senses with the entire zone.
                    //
                    // Qud tracks exploration separately from visibility and illumination.
                    // VisAll() alone does not cause previously unexplored cells to render.
                    //
                    for (int x = 0; x < zone.Width; x++)
                    {
                        for (int y = 0; y < zone.Height; y++)
                        {
                            zone.SetExplored(
                                x,
                                y,
                                true
                            );
                        }
                    }

                    zone.LightAll();
                    zone.VisAll();
                }
            }

            return
                base.HandleEvent(E);
        }


        public override void Remove(
            GameObject Object
        )
        {
            StatShifter.RemoveStatShifts();

            PenaltyApplied =
                false;
        }
    }


    /// <summary>
    /// Light-specific EP attunement.
    ///
    /// Shared base owns:
    ///   - 200-turn lifetime
    ///   - tier-scaled temporary Light Manipulation
    ///   - generic mutation cleanup
    ///
    /// Light owns:
    ///   - immediate suppression/restoration of the C1 Agility penalty
    ///   - +50 percentage points of effective light-reflection chance
    /// </summary>
    [Serializable]
    public class SubterraneanSitesEPLightAttunementEffect :
        SubterraneanSitesEPAttunementEffect
    {
        public SubterraneanSitesEPLightAttunementEffect()
            : base()
        {
            ThemeKey =
                "Light";
        }


        public SubterraneanSitesEPLightAttunementEffect(
            int duration,
            int mutationLevel
        )
            : base(
                duration,
                "Light",

                // no conventional resistance
                "",
                0,

                // no generic save bonus
                "",
                "",
                0,

                // signature mutation
                "LightManipulation",
                mutationLevel
            )
        {
        }


        protected override void RegisterThemeEvents(
            GameObject Object,
            IEventRegistrar Registrar
        )
        {
            Registrar.Register(
                "RefractLight"
            );
        }


        protected override bool ApplyTheme(
            GameObject Object
        )
        {
            if (Object == null)
                return false;

            //
            // Remove the pre-attunement penalty immediately rather than waiting
            // for the next EndTurn synchronization.
            //
            SubterraneanSitesEPLightEnvironmentEffect environment =
                Object.GetEffectDescendedFrom<
                    SubterraneanSitesEPLightEnvironmentEffect
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

            //
            // If attunement expires or is replaced while the player remains
            // inside Light C1, restore the sensory penalty immediately.
            //
            if (
                !string.Equals(
                    category1,
                    "Light",
                    StringComparison.Ordinal
                )
            )
            {
                return;
            }

            SubterraneanSitesEPLightEnvironmentEffect environment =
                Object.GetEffectDescendedFrom<
                    SubterraneanSitesEPLightEnvironmentEffect
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
                "+50% chance to reflect light-based damage"
            );
        }

        protected override bool FireThemeEvent(
            Event E
        )
        {
            if (
                E == null ||
                E.ID != "RefractLight"
            )
            {
                return true;
            }

            return
                SubterraneanSites
                    .SubterraneanSitesEPLightTheme
                    .HandleReflectionBonus(
                        base.Object,
                        E
                    );
        }
    }

   


}

namespace XRL.World.ZoneBuilders
{
    /// <summary>
    /// Light Category 4.
    ///
    /// This is intentionally OPEN-FIRST.
    ///
    /// Almost the entire zone interior is traversable floor. We then lay
    /// asymmetric rectangular one-cell wall loops over that field.
    ///
    /// Rectangle intersections are opened rather than thickened, several
    /// openings are punched into each structure, and a final connectivity
    /// pass removes individual divider cells until every open region joins.
    ///
    /// The only substantial solid fill is the outer shell.
    /// </summary>
    public class SubterraneanSitesLightLayout :
        ZoneBuilderSandbox
    {
        public int OuterShellThickness = 2;

        public int LargeRectangles = 3;
        public int MediumRectangles = 2;
        public int SingleEntranceRooms = 2;

        public int MinLargeWidth = 28;
        public int MaxLargeWidth = 46;

        public int MinLargeHeight = 8;
        public int MaxLargeHeight = 14;

        public int MinMediumWidth = 18;
        public int MaxMediumWidth = 30;

        public int MinMediumHeight = 6;
        public int MaxMediumHeight = 10;

        public int AnchorClearRadius = 4;


        private sealed class RectSpec
        {
            public int X1;
            public int Y1;
            public int X2;
            public int Y2;

            public int Side;

            public bool SingleEntrance;
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
                    "SubterraneanSites:LightLayout:" +
                    Z.ZoneID
                );

            System.Random rng =
                new System.Random(
                    seed
                );

            bool[,] wallMap =
                new bool[
                    Z.Width,
                    Z.Height
                ];

            int[,] wallHits =
                new int[
                    Z.Width,
                    Z.Height
                ];

            List<RectSpec> rectangles =
                new List<RectSpec>();

            HashSet<int> singleEntranceOrdinals =
                new HashSet<int>();

            int totalRooms =
                LargeRectangles +
                MediumRectangles;

            int protectedCount =
                Math.Min(
                    Math.Max(
                        0,
                        SingleEntranceRooms
                    ),
                    totalRooms
                );

            while (
                singleEntranceOrdinals.Count <
                protectedCount
            )
            {
                singleEntranceOrdinals.Add(
                    rng.Next(
                        totalRooms
                    )
                );
            }

            //
            // Thin sealed outer shell.
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
                        IsOuterShell(
                            Z,
                            x,
                            y
                        )
                    )
                    {
                        wallMap[
                            x,
                            y
                        ] = true;
                    }
                }
            }

            //
            // Every room grows inward from the perimeter.
            //
            // Rotate the starting side randomly, then distribute structures
            // around the four sides rather than allowing all of them to pile
            // up along one edge.
            //
            int sideOffset =
            rng.Next(4);

            int roomOrdinal = 0;

            for (int i = 0; i < LargeRectangles; i++)
            {
                RectSpec rectangle =
                    RollRectangle(
                        Z,
                        rng,
                        MinLargeWidth,
                        MaxLargeWidth,
                        MinLargeHeight,
                        MaxLargeHeight,
                        (sideOffset + roomOrdinal) % 4
                    );

                 roomOrdinal++;

                if (rectangle == null)
                    continue;

                rectangle.SingleEntrance =
                    singleEntranceOrdinals.Contains(
                        roomOrdinal - 1
                    );

                rectangles.Add(rectangle);

                AddRectangleHits(
                    wallHits,
                    rectangle
                );
            }

            for (int i = 0; i < MediumRectangles; i++)
            {
                RectSpec rectangle =
                    RollRectangle(
                        Z,
                        rng,
                        MinMediumWidth,
                        MaxMediumWidth,
                        MinMediumHeight,
                        MaxMediumHeight,
                        (sideOffset + roomOrdinal) % 4
                    );

                roomOrdinal++;

                if (rectangle == null)
                    continue;

                rectangles.Add(rectangle);

                AddRectangleHits(
                    wallHits,
                    rectangle
                );
            }

            bool[,] protectedWalls =
                new bool[
                    Z.Width,
                    Z.Height
                ];

            foreach (RectSpec rectangle in rectangles)
            {
                if (
                    rectangle == null ||
                    !rectangle.SingleEntrance
                )
                {
                    continue;
                }

                MarkRectangleWalls(
                    protectedWalls,
                    rectangle
                );
            }

            //
            // A wall segment belonging to exactly one rectangle survives.
            //
            // When two or more rectangle perimeters cross or exactly overlap,
            // erase that crossing section. This makes the structures merge
            // rather than forming dense knots of wall.
            //
            for (
                int x = OuterShellThickness;
                x < Z.Width - OuterShellThickness;
                x++
            )
            {
                for (
                    int y = OuterShellThickness;
                    y < Z.Height - OuterShellThickness;
                    y++
                )
                {
                    if (
                        protectedWalls[
                            x,
                            y
                        ]
                    )
                    {
                        wallMap[
                            x,
                            y
                        ] = true;
                    }
                    else if (
                        wallHits[
                            x,
                            y
                        ] == 1
                    )
                    {
                        wallMap[
                            x,
                            y
                        ] = true;
                    }
                    else if (
                        wallHits[
                            x,
                            y
                        ] >= 2
                    )
                    {
                        wallMap[
                            x,
                            y
                        ] = false;
                    }
                }
            }

            //
            // These are open breaches, not doors.
            //
            // Each perimeter room gets one broad opening on the side facing
            // the central field. Rectangle intersections can create additional
            // irregular connections naturally.
            //
            foreach (RectSpec rectangle in rectangles)
            {
                PunchOpening(
                    wallMap,
                    rectangle,
                    rng
                );
            }

            //
            // Incoming/outgoing transition footprints must remain broad open
            // areas regardless of the rectangular pattern.
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

                    ClearAroundAnchor(
                        Z,
                        wallMap,
                        anchor
                    );
                }
            }

            //
            // Materialize the abstract geometry exactly.
            //
            // SolidPlaceholderFill already intentionally erased the original
            // underground zone. At this point no C1/C2/C5 content exists yet.
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
                    Cell cell =
                        Z.GetCell(
                            x,
                            y
                        );

                    if (cell == null)
                        continue;

                    cell.Clear();

                    if (
                        wallMap[
                            x,
                            y
                        ]
                    )
                    {
                        cell.AddObject(
                            SubterraneanSites
                                .SubterraneanSitesEPGeometry
                                .SolidPlaceholderBlueprint
                        );
                    }
                }
            }

            //
            // Stronger than merely making the transition anchors reachable:
            // make every ordinary open region cardinally connected.
            //
            // Because internal rectangle walls are one cell thick, opening one
            // divider cell at a time preserves the room/divider language instead
            // of cutting artificial corridors.
            //
            EnsureOpenRegionsConnected(
                Z,
                rng,
                protectedWalls
            );

            Z.ClearReachableMap();

            return true;
        }


        private void ClampSettings(
            Zone Z
        )
        {
            if (OuterShellThickness < 1)
                OuterShellThickness = 1;

            int maximumShell =
                Math.Max(
                    1,
                    Math.Min(
                        Z.Width,
                        Z.Height
                    ) / 4
                );

            if (
                OuterShellThickness >
                maximumShell
            )
            {
                OuterShellThickness =
                    maximumShell;
            }

            if (LargeRectangles < 0)
                LargeRectangles = 0;

            if (MediumRectangles < 0)
                MediumRectangles = 0;

            if (AnchorClearRadius < 1)
                AnchorClearRadius = 1;
        }


        private bool IsOuterShell(
            Zone Z,
            int x,
            int y
        )
        {
            return
                x < OuterShellThickness ||
                y < OuterShellThickness ||
                x >=
                    Z.Width -
                    OuterShellThickness ||
                y >=
                    Z.Height -
                    OuterShellThickness;
        }


        private RectSpec RollRectangle(
            Zone Z,
            System.Random rng,
            int requestedMinWidth,
            int requestedMaxWidth,
            int requestedMinHeight,
            int requestedMaxHeight,
            int side
        )
        {
            int shell =
                OuterShellThickness;

            int freeMargin =
                shell + 2;

            int maxWidth =
                Math.Min(
                    requestedMaxWidth,
                    Z.Width - freeMargin * 2
                );

            int maxHeight =
                Math.Min(
                    requestedMaxHeight,
                    Z.Height - freeMargin * 2
                );

            int minWidth =
                Math.Min(
                    Math.Max(5, requestedMinWidth),
                    maxWidth
                );

            int minHeight =
                Math.Min(
                    Math.Max(5, requestedMinHeight),
                    maxHeight
                );

            if (
                maxWidth < 5 ||
                maxHeight < 5
            )
            {
                return null;
            }

            int width =
                rng.Next(
                    minWidth,
                    maxWidth + 1
                );

            int height =
                rng.Next(
                    minHeight,
                    maxHeight + 1
                );

            RectSpec result =
                new RectSpec();

            result.Side = side;

            if (side == 0)
            {
                //
                // NORTH:
                // the outer north shell is the missing fourth wall.
                //
                int latestX1 =
                    Z.Width -
                    freeMargin -
                    width;

                if (latestX1 < freeMargin)
                    return null;

                result.X1 =
                    rng.Next(
                        freeMargin,
                        latestX1 + 1
                    );

                result.X2 =
                    result.X1 +
                    width -
                    1;

                result.Y1 =
                    shell;

                result.Y2 =
                    Math.Min(
                        Z.Height - shell - 2,
                        result.Y1 + height - 1
                    );
            }
            else if (side == 1)
            {
                //
                // SOUTH
                //
                int latestX1 =
                    Z.Width -
                    freeMargin -
                    width;

                if (latestX1 < freeMargin)
                    return null;

                result.X1 =
                    rng.Next(
                        freeMargin,
                        latestX1 + 1
                    );

                result.X2 =
                    result.X1 +
                    width -
                    1;

                result.Y2 =
                    Z.Height -
                    shell -
                    1;

                result.Y1 =
                    Math.Max(
                        shell + 1,
                        result.Y2 - height + 1
                    );
            }
            else if (side == 2)
            {
                //
                // WEST
                //
                int latestY1 =
                    Z.Height -
                    freeMargin -
                    height;

                if (latestY1 < freeMargin)
                    return null;

                result.Y1 =
                    rng.Next(
                        freeMargin,
                        latestY1 + 1
                    );

                result.Y2 =
                    result.Y1 +
                    height -
                    1;

                result.X1 =
                    shell;

                result.X2 =
                    Math.Min(
                        Z.Width - shell - 2,
                        result.X1 + width - 1
                    );
            }
            else
            {
                //
                // EAST
                //
                int latestY1 =
                    Z.Height -
                    freeMargin -
                    height;

                if (latestY1 < freeMargin)
                    return null;

                result.Y1 =
                    rng.Next(
                        freeMargin,
                        latestY1 + 1
                    );

                result.Y2 =
                    result.Y1 +
                    height -
                    1;

                result.X2 =
                    Z.Width -
                    shell -
                    1;

                result.X1 =
                    Math.Max(
                        shell + 1,
                        result.X2 - width + 1
                    );
            }

            return result;
        }

        private void MarkRectangleWalls(
            bool[,] wallMap,
            RectSpec rectangle
        )
        {
            if (
                wallMap == null ||
                rectangle == null
            )
            {
                return;
            }

            if (rectangle.Side == 0)
            {
                for (int y = rectangle.Y1; y <= rectangle.Y2; y++)
                {
                    wallMap[
                        rectangle.X1,
                        y
                    ] = true;

                    wallMap[
                        rectangle.X2,
                        y
                    ] = true;
                }

                for (int x = rectangle.X1 + 1; x < rectangle.X2; x++)
                {
                    wallMap[
                        x,
                        rectangle.Y2
                    ] = true;
                }
            }
            else if (rectangle.Side == 1)
            {
                for (int y = rectangle.Y1; y <= rectangle.Y2; y++)
                {
                    wallMap[
                        rectangle.X1,
                        y
                    ] = true;

                    wallMap[
                        rectangle.X2,
                        y
                    ] = true;
                }

                for (int x = rectangle.X1 + 1; x < rectangle.X2; x++)
                {
                    wallMap[
                        x,
                        rectangle.Y1
                    ] = true;
                }
            }
            else if (rectangle.Side == 2)
            {
                for (int x = rectangle.X1; x <= rectangle.X2; x++)
                {
                    wallMap[
                        x,
                        rectangle.Y1
                    ] = true;

                    wallMap[
                        x,
                        rectangle.Y2
                    ] = true;
                }

                for (int y = rectangle.Y1 + 1; y < rectangle.Y2; y++)
                {
                    wallMap[
                        rectangle.X2,
                        y
                    ] = true;
                }
            }
            else
            {
                for (int x = rectangle.X1; x <= rectangle.X2; x++)
                {
                    wallMap[
                        x,
                        rectangle.Y1
                    ] = true;

                    wallMap[
                        x,
                        rectangle.Y2
                    ] = true;
                }

                for (int y = rectangle.Y1 + 1; y < rectangle.Y2; y++)
                {
                    wallMap[
                        rectangle.X1,
                        y
                    ] = true;
                }
            }
        }


        private void AddRectangleHits(
            int[,] wallHits,
            RectSpec rectangle
        )
        {
            if (
                wallHits == null ||
                rectangle == null
            )
            {
                return;
            }

            if (rectangle.Side == 0)
            {
                //
                // NORTH:
                // two side walls + inward/south wall.
                //
                for (int y = rectangle.Y1; y <= rectangle.Y2; y++)
                {
                    wallHits[rectangle.X1, y]++;
                    wallHits[rectangle.X2, y]++;
                }

                for (int x = rectangle.X1 + 1; x < rectangle.X2; x++)
                {
                    wallHits[x, rectangle.Y2]++;
                }
            }
            else if (rectangle.Side == 1)
            {
                //
                // SOUTH:
                // two side walls + inward/north wall.
                //
                for (int y = rectangle.Y1; y <= rectangle.Y2; y++)
                {
                    wallHits[rectangle.X1, y]++;
                    wallHits[rectangle.X2, y]++;
                }

                for (int x = rectangle.X1 + 1; x < rectangle.X2; x++)
                {
                    wallHits[x, rectangle.Y1]++;
                }
            }
            else if (rectangle.Side == 2)
            {
                //
                // WEST:
                // top/bottom + inward/east wall.
                //
                for (int x = rectangle.X1; x <= rectangle.X2; x++)
                {
                    wallHits[x, rectangle.Y1]++;
                    wallHits[x, rectangle.Y2]++;
                }

                for (int y = rectangle.Y1 + 1; y < rectangle.Y2; y++)
                {
                    wallHits[rectangle.X2, y]++;
                }
            }
            else
            {
                //
                // EAST:
                // top/bottom + inward/west wall.
                //
                for (int x = rectangle.X1; x <= rectangle.X2; x++)
                {
                    wallHits[x, rectangle.Y1]++;
                    wallHits[x, rectangle.Y2]++;
                }

                for (int y = rectangle.Y1 + 1; y < rectangle.Y2; y++)
                {
                    wallHits[rectangle.X1, y]++;
                }
            }
        }


        private void PunchOpening(
            bool[,] wallMap,
            RectSpec rectangle,
            System.Random rng
        )
        {
            if (
                wallMap == null ||
                rectangle == null ||
                rng == null
            )
            {
                return;
            }

            int openingWidth =
                rng.Next(
                    2,
                    4
                );

            if (
                rectangle.Side == 0 ||
                rectangle.Side == 1
            )
            {
                if (
                    rectangle.X2 -
                    rectangle.X1 <
                    6
                )
                {
                    return;
                }

                int center =
                    rng.Next(
                        rectangle.X1 + 2,
                        rectangle.X2 - 1
                    );

                int y =
                    rectangle.Side == 0
                        ? rectangle.Y2
                        : rectangle.Y1;

                int start =
                    center -
                    openingWidth / 2;

                for (int i = 0; i < openingWidth; i++)
                {
                    int x =
                        start + i;

                    if (
                        x <= rectangle.X1 ||
                        x >= rectangle.X2
                    )
                    {
                        continue;
                    }

                    wallMap[x, y] = false;
                }
            }
            else
            {
                if (
                    rectangle.Y2 -
                    rectangle.Y1 <
                    6
                )
                {
                    return;
                }

                int center =
                    rng.Next(
                        rectangle.Y1 + 2,
                        rectangle.Y2 - 1
                    );

                int x =
                    rectangle.Side == 2
                        ? rectangle.X2
                        : rectangle.X1;

                int start =
                    center -
                    openingWidth / 2;

                for (int i = 0; i < openingWidth; i++)
                {
                    int y =
                        start + i;

                    if (
                        y <= rectangle.Y1 ||
                        y >= rectangle.Y2
                    )
                    {
                        continue;
                    }

                    wallMap[x, y] = false;
                }
            }
        }


        private void ClearAroundAnchor(
            Zone Z,
            bool[,] wallMap,
            Location2D anchor
        )
        {
            int radiusSquared =
                AnchorClearRadius *
                AnchorClearRadius;

            for (
                int dx = -AnchorClearRadius;
                dx <= AnchorClearRadius;
                dx++
            )
            {
                for (
                    int dy = -AnchorClearRadius;
                    dy <= AnchorClearRadius;
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
                        anchor.X +
                        dx;

                    int y =
                        anchor.Y +
                        dy;

                    if (
                        x < OuterShellThickness ||
                        y < OuterShellThickness ||
                        x >=
                            Z.Width -
                            OuterShellThickness ||
                        y >=
                            Z.Height -
                            OuterShellThickness
                    )
                    {
                        continue;
                    }

                    wallMap[
                        x,
                        y
                    ] = false;
                }
            }
        }


        private void EnsureOpenRegionsConnected(
            Zone Z,
            System.Random rng,
            bool[,] protectedWalls
        )
        {
            int safety = 256;

            while (
                safety-- > 0
            )
            {
                int[,] components;
                int componentCount =
                    BuildOpenComponentMap(
                        Z,
                        out components
                    );

                if (
                    componentCount <= 1
                )
                {
                    return;
                }

                List<Cell> bridgeCandidates =
                    new List<Cell>();

                //
                // Search only the interior wall structures.
                // Never punch through the sealed map perimeter.
                //
                for (
                    int x = OuterShellThickness;
                    x <
                        Z.Width -
                        OuterShellThickness;
                    x++
                )
                {
                    for (
                        int y = OuterShellThickness;
                        y <
                            Z.Height -
                            OuterShellThickness;
                        y++
                    )
                    {

                        if (
                            protectedWalls != null &&
                            protectedWalls[
                                x,
                                y
                            ]
                        )
                        {
                            continue;
                        }

                        Cell wall =
                            Z.GetCell(
                                x,
                                y
                            );

                        if (
                            wall == null ||
                            !SubterraneanSites
                                .SubterraneanSitesEPGeometry
                                .IsAnyPlaceholder(
                                    wall
                                )
                        )
                        {
                            continue;
                        }

                        if (
                            SeparatesDifferentComponents(
                                Z,
                                components,
                                x,
                                y
                            )
                        )
                        {
                            bridgeCandidates.Add(
                                wall
                            );
                        }
                    }
                }

                if (
                    bridgeCandidates.Count == 0
                )
                {
                    //
                    // With one-cell rectangular divider walls this should be
                    // extremely unusual. Do not start carving long fallback
                    // corridors; preserve the map for diagnosis instead.
                    //
                    return;
                }

                Cell chosen =
                    bridgeCandidates[
                        rng.Next(
                            bridgeCandidates.Count
                        )
                    ];

                chosen.Clear();
            }
        }


        private int BuildOpenComponentMap(
            Zone Z,
            out int[,] components
        )
        {
            components =
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
                    components[
                        x,
                        y
                    ] = -1;
                }
            }

            int nextComponent = 0;

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
                        components[
                            x,
                            y
                        ] >= 0
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
                        !IsOpenCell(
                            start
                        )
                    )
                    {
                        continue;
                    }

                    FloodComponent(
                        Z,
                        components,
                        start,
                        nextComponent
                    );

                    nextComponent++;
                }
            }

            return nextComponent;
        }


        private void FloodComponent(
            Zone Z,
            int[,] components,
            Cell start,
            int component
        )
        {
            Queue<Cell> queue =
                new Queue<Cell>();

            queue.Enqueue(
                start
            );

            components[
                start.X,
                start.Y
            ] = component;

            int[] dx =
            {
                1,
                -1,
                0,
                0
            };

            int[] dy =
            {
                0,
                0,
                1,
                -1
            };

            while (
                queue.Count > 0
            )
            {
                Cell current =
                    queue.Dequeue();

                for (
                    int i = 0;
                    i < 4;
                    i++
                )
                {
                    int nx =
                        current.X +
                        dx[i];

                    int ny =
                        current.Y +
                        dy[i];

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
                        components[
                            nx,
                            ny
                        ] >= 0
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
                        !IsOpenCell(
                            next
                        )
                    )
                    {
                        continue;
                    }

                    components[
                        nx,
                        ny
                    ] = component;

                    queue.Enqueue(
                        next
                    );
                }
            }
        }


        private bool SeparatesDifferentComponents(
            Zone Z,
            int[,] components,
            int x,
            int y
        )
        {
            int first =
                -1;

            int[] dx =
            {
                1,
                -1,
                0,
                0
            };

            int[] dy =
            {
                0,
                0,
                1,
                -1
            };

            for (
                int i = 0;
                i < 4;
                i++
            )
            {
                int nx =
                    x +
                    dx[i];

                int ny =
                    y +
                    dy[i];

                if (
                    nx < 0 ||
                    ny < 0 ||
                    nx >= Z.Width ||
                    ny >= Z.Height
                )
                {
                    continue;
                }

                int component =
                    components[
                        nx,
                        ny
                    ];

                if (
                    component < 0
                )
                {
                    continue;
                }

                if (
                    first < 0
                )
                {
                    first =
                        component;
                }
                else if (
                    component != first
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
                !SubterraneanSites
                    .SubterraneanSitesEPGeometry
                    .IsAnyPlaceholder(
                        cell
                    );
        }
    }
}

namespace XRL.World.Effects
{
    /// <summary>
    /// Permanent Light-dimensional adaptation for native denizens.
    ///
    /// Light Manipulation itself is supplied by the shared dimensional
    /// signature-mutation package. This effect supplies Light's additional
    /// +50 percentage-point reflection adaptation.
    /// </summary>
    [Serializable]
    public class SubterraneanSitesEPLightDenizenAdaptationEffect :
        Effect
    {
        public SubterraneanSitesEPLightDenizenAdaptationEffect()
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
                "RefractLight"
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
                E.ID == "RefractLight"
            )
            {
                if (
                    !SubterraneanSites
                        .SubterraneanSitesEPLightTheme
                        .HandleReflectionBonus(
                            base.Object,
                            E
                        )
                )
                {
                    return false;
                }
            }

            return base.FireEvent(E);
        }
    }
}

namespace XRL.World.ZoneBuilders
{
    /// <summary>
    /// Light Category-4 floor adapter.
    ///
    /// Light owns the floor specification. The shared floor system owns
    /// universal EP/scar scope and application mechanics.
    /// </summary>
    public class SubterraneanSitesLightFloor :
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
                            .SubterraneanSitesEPLightTheme
                            .CreateFloorSpec(),
                        EntranceOnly != 0
                    );
        }
    }

}


namespace XRL.World.ZoneBuilders
{
    /// <summary>
    /// Light Category 3.
    ///
    /// Thick/core placeholder:
    ///     Black Marble
    ///
    /// Cave-facing / room-facing boundary placeholder:
    ///     Chavvah Taproot
    ///
    /// Internal Light C4 divider walls are only one cell thick, so they
    /// naturally classify entirely as exposed taproot.
    /// </summary>
    public class SubterraneanSitesLightMaterials :
        ZoneBuilderSandbox
    {
        public string BulkWallBlueprint =
            "Black Marble";

        public string InnerWallBlueprint =
            "Chavvah Taproot";


        public bool BuildZone(
            Zone Z
        )
        {
            if (Z == null)
                return true;

            foreach (
                Cell cell
                in Z.GetCells()
            )
            {
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
                    isBoundary
                        ? InnerWallBlueprint
                        : BulkWallBlueprint;

                if (
                    blueprint.IsNullOrEmpty()
                )
                {
                    continue;
                }

                //
                // Create first so a failed blueprint construction cannot
                // leave a white/blank cell after destructive wall clearing.
                //
                GameObject replacement =
                    GameObjectFactory
                        .Factory
                        .CreateObject(
                            blueprint
                        );

                if (
                    replacement == null
                )
                {
                    continue;
                }

                cell.ClearWalls();

                cell.AddObject(
                    replacement
                );
            }

            return true;
        }
    }
}

namespace XRL.World.ZoneBuilders
{
    /// <summary>
    /// Light crystalline-root network used by Categories 1 and 2.
    ///
    /// Adapted from vanilla ChavvahRoots:
    /// several noisy paths grow and branch through open geometry.
    ///
    /// Unlike Chavvah's builder, these paths are forbidden from entering
    /// shared EP placeholders. TunnelTo therefore cannot use the root pass
    /// to carve through Category-4 walls.
    ///
    /// Crystalline Root is passable ground-like dimensional content and
    /// deliberately does NOT claim EP reservation cells.
    /// </summary>
    public class SubterraneanSitesLightRoots :
        ZoneBuilderSandbox
    {
        public int EntranceOnly = 0;

            //
            // Multiple Light categories can use the same root-network mechanic.
            // Give each use case its own deterministic RNG stream so C1 and C2
            // generate independent networks instead of reproducing one another.
            //
            public string SeedVariant = "Default";

        public int MinNetworks = 8;
        public int MaxNetworks = 11;

        public int MinSegmentsPerNetwork = 3;
        public int MaxSegmentsPerNetwork = 5;

        public int MinSegmentDistance = 4;
        public int MaxSegmentDistance = 9;

        public int BranchChancePercent = 35;

        public int TransitionExclusionRadius = 4;

        public string RootBlueprint =
            "Crystalline Root";

        public bool BuildZone(
            Zone Z
        )
        {
            if (
                Z == null ||
                RootBlueprint.IsNullOrEmpty()
            )
            {
                return true;
            }

            if (EntranceOnly != 0)
            {
                MinNetworks = 2;
                MaxNetworks = 2;

                MinSegmentsPerNetwork = 2;
                MaxSegmentsPerNetwork = 3;

                MinSegmentDistance = 3;
                MaxSegmentDistance = 6;
            }

            ClampSettings();

            string seedVariant =
                string.IsNullOrEmpty(
                    SeedVariant
                )
                    ? "Default"
                    : SeedVariant;

            int seed =
                XRLCore.Core.Game.GetWorldSeed(
                    "SubterraneanSites:LightRootNetwork:" +
                    Z.ZoneID +
                    ":Entrance:" +
                    EntranceOnly.ToString() +
                    ":Variant:" +
                    seedVariant
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
                CollectRootCells(
                    Z,
                    anchors
                );

            if (candidates.Count == 0)
                return true;

            int networkCount =
                rng.Next(
                    MinNetworks,
                    MaxNetworks + 1
                );

            for (int network = 0; network < networkCount; network++)
            {
                Cell start =
                    candidates[
                        rng.Next(
                            candidates.Count
                        )
                    ];

                if (start == null)
                    continue;

                Cell current =
                    start;

                List<Cell> branchPoints =
                    new List<Cell>();

                int segmentCount =
                    rng.Next(
                        MinSegmentsPerNetwork,
                        MaxSegmentsPerNetwork + 1
                    );

                for (int segment = 0; segment < segmentCount; segment++)
                {
                    if (
                        branchPoints.Count > 0 &&
                        rng.Next(100) <
                            BranchChancePercent
                    )
                    {
                        current =
                            branchPoints[
                                rng.Next(
                                    branchPoints.Count
                                )
                            ];
                    }

                    Cell endpoint =
                        PickEndpoint(
                            Z,
                            current,
                            candidates,
                            rng
                        );

                    if (endpoint == null)
                        break;

                    Cell branchSource =
                        current;

                    ZoneBuilderSandbox.TunnelTo(
                        Z,
                        current.Location,
                        endpoint.Location,
                        pathWithNoise: true,
                        0.2f,
                        0,
                        delegate(Cell cell)
                        {
                            if (
                                !IsRootPathCell(
                                    Z,
                                    cell,
                                    anchors
                                )
                            )
                            {
                                return;
                            }

                            if (
                                !cell.HasObjectWithBlueprint(
                                    RootBlueprint
                                )
                            )
                            {
                                cell.AddObject(
                                    RootBlueprint
                                );
                            }
                        },
                        delegate(
                            int x,
                            int y,
                            int cost
                        )
                        {
                            Cell cell =
                                Z.GetCell(
                                    x,
                                    y
                                );

                            if (
                                !IsRootPathCell(
                                    Z,
                                    cell,
                                    anchors
                                )
                            )
                            {
                                return int.MaxValue;
                            }

                            return 0;
                        }
                    );

                    if (
                        rng.Next(100) <
                        BranchChancePercent
                    )
                    {
                        branchPoints.Add(
                            branchSource
                        );
                    }

                    current =
                        endpoint;
                }
            }

            return true;
        }


        private void ClampSettings()
        {
            if (MinNetworks < 0)
                MinNetworks = 0;

            if (MaxNetworks < MinNetworks)
                MaxNetworks = MinNetworks;

            if (MinSegmentsPerNetwork < 1)
                MinSegmentsPerNetwork = 1;

            if (
                MaxSegmentsPerNetwork <
                MinSegmentsPerNetwork
            )
            {
                MaxSegmentsPerNetwork =
                    MinSegmentsPerNetwork;
            }

            if (MinSegmentDistance < 2)
                MinSegmentDistance = 2;

            if (
                MaxSegmentDistance <
                MinSegmentDistance
            )
            {
                MaxSegmentDistance =
                    MinSegmentDistance;
            }

            if (BranchChancePercent < 0)
                BranchChancePercent = 0;

            if (BranchChancePercent > 100)
                BranchChancePercent = 100;
        }


        private List<Cell> CollectRootCells(
            Zone Z,
            List<Location2D> anchors
        )
        {
            List<Cell> result =
                new List<Cell>();

            foreach (Cell cell in Z.GetCells())
            {
                if (
                    IsRootPathCell(
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


        private Cell PickEndpoint(
            Zone Z,
            Cell current,
            List<Cell> candidates,
            System.Random rng
        )
        {
            if (
                Z == null ||
                current == null ||
                candidates == null ||
                candidates.Count == 0
            )
            {
                return null;
            }

            List<Cell> choices =
                new List<Cell>();

            foreach (Cell cell in candidates)
            {
                if (cell == null)
                    continue;

                int distance =
                    Math.Abs(
                        cell.X -
                        current.X
                    ) +
                    Math.Abs(
                        cell.Y -
                        current.Y
                    );

                if (
                    distance <
                        MinSegmentDistance ||
                    distance >
                        MaxSegmentDistance
                )
                {
                    continue;
                }

                choices.Add(
                    cell
                );
            }

            if (choices.Count == 0)
                return null;

            return
                choices[
                    rng.Next(
                        choices.Count
                    )
                ];
        }


        private bool IsRootPathCell(
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
                //
                // This is what prevents TunnelTo from turning the
                // root network into another wall-carving system.
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
    }
}
namespace XRL.World.ZoneBuilders
{
    /// <summary>
    /// Light Category-5 scenery.
    ///
    /// - occasional full 3x2 Moon-Stair-style hex crystals
    /// - scattered small hex crystals
    /// - psychal rhythm rocks
    /// - grouped dense/sparse Chavvah crystal leaves
    ///
    /// Crystalline roots belong to Category 1 and may coexist under
    /// these objects.
    /// </summary>
    public class SubterraneanSitesLightDecorations :
        ZoneBuilderSandbox
    {
        public int EntranceOnly = 0;

        public int MinLargeHexCrystals = 2;
        public int MaxLargeHexCrystals = 4;

        public int MinSmallHexCrystals = 6;
        public int MaxSmallHexCrystals = 12;

        public int MinPsychalRocks = 4;
        public int MaxPsychalRocks = 9;

        public int MinLeafClusters = 3;
        public int MaxLeafClusters = 5;

        public int MinLeafCellsPerCluster = 8;
        public int MaxLeafCellsPerCluster = 16;

        public int TransitionExclusionRadius = 4;

        public string LargeCrystalBlueprint =
            "CrystalWall";

        public string SmallCrystalBlueprint =
            "SmallHexCrystal";

        public string PsychalRockBlueprint =
            "Psychal Rhythm Rock";

        public string DenseLeavesBlueprint =
            "Chavvah Dense Leaves";

        public string SparseLeavesBlueprint =
            "Chavvah Sparse Leaves";
        
        public int MinTreesPerType = 1;
        public int MaxTreesPerType = 3;

        public int TreeMinDistanceFromLeaves = 2;
        public int TreeMaxDistanceFromLeaves = 5;

        public string NAryTreeBlueprint =
            "n-Ary Tree";

        public string GlitchwoodTreeBlueprint =
            "Glitchwood Tree";

        public string IcosahedarBlueprint =
            "Icosahedar";

        public string CrystalFlowersBlueprint =
            "Crystal Flowers";


        public bool BuildZone(
            Zone Z
        )
        {
            if (Z == null)
                return true;

            if (EntranceOnly != 0)
            {
                MinLargeHexCrystals = 0;
                MaxLargeHexCrystals = 1;

                MinSmallHexCrystals = 1;
                MaxSmallHexCrystals = 2;

                MinPsychalRocks = 1;
                MaxPsychalRocks = 2;

                MinLeafClusters = 3;
                MaxLeafClusters = 3;

                MinLeafCellsPerCluster = 12;
                MaxLeafCellsPerCluster = 20;

                MinTreesPerType = 1;
                MaxTreesPerType = 3;
            }

            int seed =
                XRLCore.Core.Game.GetWorldSeed(
                    "SubterraneanSites:LightDecorations:" +
                    Z.ZoneID +
                    ":Entrance:" +
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

            PlaceLargeHexCrystals(
                Z,
                anchors,
                rng
            );

            PlaceObjects(
                Z,
                anchors,
                rng,
                SmallCrystalBlueprint,
                MinSmallHexCrystals,
                MaxSmallHexCrystals,
                true
            );

            PlaceObjects(
                Z,
                anchors,
                rng,
                PsychalRockBlueprint,
                MinPsychalRocks,
                MaxPsychalRocks,
                false
            );

            //
            // Actual Moon Stair crystal-flower vegetation.
            // These remain clustered rather than scattered.
            //
            PlaceCrystalFlowerPatches(
                Z,
                anchors,
                rng
            );

            //
            // Leaves first, then put their associated trees nearby.
            //
            List<HashSet<Cell>> leafPatches =
                PlaceLeafClusters(
                    Z,
                    anchors,
                    rng
                );

            PlaceTreesNearLeaves(
                Z,
                anchors,
                rng,
                leafPatches
            );

            return true;
        }


        private void PlaceLargeHexCrystals(
            Zone Z,
            List<Location2D> anchors,
            System.Random rng
        )
        {
            int desired =
                rng.Next(
                    Math.Max(
                        0,
                        MinLargeHexCrystals
                    ),
                    Math.Max(
                        MinLargeHexCrystals,
                        MaxLargeHexCrystals
                    ) + 1
                );

            for (int placed = 0; placed < desired; placed++)
            {
                List<Location2D> candidates =
                    new List<Location2D>();

                //
                // Match the exact staggered 3x2 lattice used by Moon Stair.
                //
                for (
                    int column = 0;
                    column < Z.Width / 3;
                    column++
                )
                {
                    int x =
                        column * 3;

                    for (
                        int row = 0;
                        row < Z.Height / 2;
                        row++
                    )
                    {
                        int y =
                            row * 2 +
                            column % 2;

                        if (
                            LargeCrystalFootprintAvailable(
                                Z,
                                anchors,
                                x,
                                y
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
                    return;

                Location2D origin =
                    candidates[
                        rng.Next(
                            candidates.Count
                        )
                    ];

                for (int dx = 0; dx < 3; dx++)
                {
                    for (int dy = 0; dy < 2; dy++)
                    {
                        Cell cell =
                            Z.GetCell(
                                origin.X + dx,
                                origin.Y + dy
                            );

                        if (cell == null)
                            continue;

                        GameObject crystal =
                            cell.AddObject(
                                LargeCrystalBlueprint
                            );

                        if (crystal == null)
                            continue;

                        SubterraneanSites
                            .SubterraneanSitesEPReservations
                            .ClaimCell(
                                Z,
                                cell
                            );
                    }
                }
            }
        }


        private bool LargeCrystalFootprintAvailable(
            Zone Z,
            List<Location2D> anchors,
            int x,
            int y
        )
        {
            //
            // Actual six cells.
            //
            for (int dx = 0; dx < 3; dx++)
            {
                for (int dy = 0; dy < 2; dy++)
                {
                    Cell cell =
                        Z.GetCell(
                            x + dx,
                            y + dy
                        );

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
                }
            }

            //
            // One-cell halo. This keeps the six-cell solid object out
            // of narrow door-like breaches and one-cell passages.
            //
            for (int px = x - 1; px <= x + 3; px++)
            {
                for (int py = y - 1; py <= y + 2; py++)
                {
                    Cell cell =
                        Z.GetCell(
                            px,
                            py
                        );

                    if (cell == null)
                        return false;

                    if (
                        EntranceOnly == 0 &&
                        !SubterraneanSites
                            .SubterraneanSitesEPGeometry
                            .IsOpenGeometryCell(
                                cell
                            )
                    )
                    {
                        return false;
                    }

                    if (cell.IsSolid())
                        return false;

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
                }
            }

            return true;
        }

        private List<HashSet<Cell>> PlaceLeafClusters(
            Zone Z,
            List<Location2D> anchors,
            System.Random rng
        )
        {
            List<HashSet<Cell>> result =
                new List<HashSet<Cell>>();

            int clusterCount =
                rng.Next(
                    MinLeafClusters,
                    MaxLeafClusters + 1
                );

            for (
                int cluster = 0;
                cluster < clusterCount;
                cluster++
            )
            {
                List<Cell> seeds =
                    CollectCandidates(
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

                int targetCells =
                    rng.Next(
                        Math.Max(
                            1,
                            MinLeafCellsPerCluster
                        ),
                        Math.Max(
                            MinLeafCellsPerCluster,
                            MaxLeafCellsPerCluster
                        ) + 1
                    );

                HashSet<Cell> patch =
                    SubterraneanSites
                        .SubterraneanSitesEPPlacement
                        .GrowPatch(
                            seed,
                            targetCells,
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

                if (
                    patch == null ||
                    patch.Count == 0
                )
                {
                    continue;
                }

                //
                // Keep the cells that actually received leaves as this
                // specific patch's footprint.
                //
                HashSet<Cell> placedLeafCells =
                    new HashSet<Cell>();

                foreach (Cell cell in patch)
                {
                    if (cell == null)
                        continue;

                    int neighbors =
                        CountPatchCardinalNeighbors(
                            patch,
                            cell
                        );

                    bool dense;

                    if (neighbors >= 3)
                    {
                        dense =
                            rng.Next(100) < 75;
                    }
                    else if (neighbors == 2)
                    {
                        dense =
                            rng.Next(100) < 40;
                    }
                    else
                    {
                        dense =
                            rng.Next(100) < 15;
                    }

                    string blueprint =
                        dense
                            ? DenseLeavesBlueprint
                            : SparseLeavesBlueprint;

                    GameObject leaves =
                        cell.AddObject(
                            blueprint
                        );

                    if (leaves == null)
                        continue;

                    placedLeafCells.Add(
                        cell
                    );

                    SubterraneanSites
                        .SubterraneanSitesEPReservations
                        .ClaimCell(
                            Z,
                            cell
                        );
                }

                if (
                    placedLeafCells.Count > 0
                )
                {
                    result.Add(
                        placedLeafCells
                    );
                }
            }

            return result;
        }

        private void PlaceTreesNearLeaves(
            Zone Z,
            List<Location2D> anchors,
            System.Random rng,
            List<HashSet<Cell>> leafPatches
        )
        {
            if (
                Z == null ||
                rng == null ||
                leafPatches == null ||
                leafPatches.Count == 0
            )
            {
                return;
            }

            string[] treeBlueprints =
            {
                NAryTreeBlueprint,
                GlitchwoodTreeBlueprint,
                IcosahedarBlueprint
            };

            //
            // Important:
            // each leaf patch independently receives 1-3 of EACH
            // tree species.
            //
            foreach (
                HashSet<Cell> leafPatch
                in leafPatches
            )
            {
                if (
                    leafPatch == null ||
                    leafPatch.Count == 0
                )
                {
                    continue;
                }

                foreach (
                    string blueprint
                    in treeBlueprints
                )
                {
                    if (blueprint.IsNullOrEmpty())
                        continue;

                    int desired =
                        rng.Next(
                            Math.Max(
                                0,
                                MinTreesPerType
                            ),
                            Math.Max(
                                MinTreesPerType,
                                MaxTreesPerType
                            ) + 1
                        );

                    for (
                        int i = 0;
                        i < desired;
                        i++
                    )
                    {
                        Cell chosen =
                            SubterraneanSites
                                .SubterraneanSitesEPPlacement
                                .PickRandomCellNearPatch(
                                    Z,
                                    leafPatch,
                                    TreeMinDistanceFromLeaves,
                                    TreeMaxDistanceFromLeaves,
                                    delegate(Cell candidate)
                                    {
                                        if (
                                            !IsCandidate(
                                                Z,
                                                candidate,
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
                                                    candidate,
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
                                    },
                                    rng
                                );

                        if (chosen == null)
                            break;

                        GameObject tree =
                            chosen.AddObject(
                                blueprint
                            );

                        if (tree == null)
                            continue;

                        SubterraneanSites
                            .SubterraneanSitesEPReservations
                            .ClaimCell(
                                Z,
                                chosen
                            );
                    }
                }
            }
        }


       


        private void PlaceCrystalFlowerPatches(
            Zone Z,
            List<Location2D> anchors,
            System.Random rng
        )
        {
            if (
                Z == null ||
                rng == null ||
                CrystalFlowersBlueprint.IsNullOrEmpty()
            )
            {
                return;
            }

            //
            // Vanilla Moon Stair uses four possible adjacent Crystal Flowers
            // groups:
            //
            //     100%
            //      75%
            //      75%
            //      25%
            //
            // with 12-20 objects per group.
            //
            // Preserve that characteristic underground distribution.
            //
            int[] chances;

            int minPatch;
            int maxPatch;

            if (EntranceOnly != 0)
            {
                //
                // Keep the much smaller entrance readable.
                //
                chances =
                    new int[]
                    {
                        100
                    };

                minPatch = 8;
                maxPatch = 12;
            }
            else
            {
                chances =
                    new int[]
                    {
                        100,
                        75,
                        75,
                        25
                    };

                minPatch = 12;
                maxPatch = 20;
            }

            foreach (int chance in chances)
            {
                if (
                    rng.Next(100) >=
                    chance
                )
                {
                    continue;
                }

                List<Cell> seeds =
                    CollectCandidates(
                        Z,
                        anchors
                    );

                if (seeds.Count == 0)
                    return;

                Cell seed =
                    seeds[
                        rng.Next(
                            seeds.Count
                        )
                    ];

                int target =
                    rng.Next(
                        minPatch,
                        maxPatch + 1
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
                                    IsCandidate(
                                        Z,
                                        cell,
                                        anchors
                                    );
                            },
                            rng
                        );

                foreach (Cell cell in patch)
                {
                    if (cell == null)
                        continue;

                    GameObject flowers =
                        cell.AddObject(
                            CrystalFlowersBlueprint
                        );

                    if (flowers == null)
                        continue;

                    SubterraneanSites
                        .SubterraneanSitesEPReservations
                        .ClaimCell(
                            Z,
                            cell
                        );
                }
            }
        }


       


        private int CountPatchCardinalNeighbors(
            HashSet<Cell> patch,
            Cell cell
        )
        {
            if (
                patch == null ||
                cell == null
            )
            {
                return 0;
            }

            int count = 0;

            Cell north =
                cell.GetCellFromDirection(
                    "N"
                );

            Cell south =
                cell.GetCellFromDirection(
                    "S"
                );

            Cell east =
                cell.GetCellFromDirection(
                    "E"
                );

            Cell west =
                cell.GetCellFromDirection(
                    "W"
                );

            if (
                north != null &&
                patch.Contains(north)
            )
            {
                count++;
            }

            if (
                south != null &&
                patch.Contains(south)
            )
            {
                count++;
            }

            if (
                east != null &&
                patch.Contains(east)
            )
            {
                count++;
            }

            if (
                west != null &&
                patch.Contains(west)
            )
            {
                count++;
            }

            return count;
        }


        private void PlaceObjects(
            Zone Z,
            List<Location2D> anchors,
            System.Random rng,
            string blueprint,
            int min,
            int max,
            bool requireBroadOpen
        )
        {
            min =
                Math.Max(
                    0,
                    min
                );

            max =
                Math.Max(
                    min,
                    max
                );

            int desired =
                rng.Next(
                    min,
                    max + 1
                );

            for (int i = 0; i < desired; i++)
            {
                List<Cell> candidates =
                    new List<Cell>();

                foreach (Cell cell in Z.GetCells())
                {
                    if (
                        !IsCandidate(
                            Z,
                            cell,
                            anchors
                        )
                    )
                    {
                        continue;
                    }

                    if (
                        requireBroadOpen &&
                        !SubterraneanSites
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
                            )
                    )
                    {
                        continue;
                    }

                    candidates.Add(
                        cell
                    );
                }

                if (candidates.Count == 0)
                    return;

                Cell chosen =
                    candidates[
                        rng.Next(
                            candidates.Count
                        )
                    ];

                GameObject decoration =
                    chosen.AddObject(
                        blueprint
                    );

                if (decoration == null)
                    continue;

                SubterraneanSites
                    .SubterraneanSitesEPReservations
                    .ClaimCell(
                        Z,
                        chosen
                    );
            }
        }


        private List<Cell> CollectCandidates(
            Zone Z,
            List<Location2D> anchors
        )
        {
            List<Cell> result =
                new List<Cell>();

            foreach (Cell cell in Z.GetCells())
            {
                if (
                    IsCandidate(
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

            //
            // Deliberately no HasSpawnBlocker() test here.
            //
            // Light's floor and root network are compatible substrate.
            // Earlier genuinely exclusive EP content protects itself through
            // shared reservations instead.
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

            return true;
        }
    }
}

namespace XRL.World.ZoneBuilders
{
    /// <summary>
    /// Light Category-2 active beam emitters.
    ///
    /// These resemble ordinary Psychal Rhythm Rocks, but periodically
    /// telegraph and fire extradimensional light beams.
    ///
    /// The first implementation checkpoint intentionally owns only:
    /// - placement
    /// - activation
    /// - timing
    /// - targeting
    /// - warning line
    /// - Irisdual beam VFX
    ///
    /// Reflection and damage are added after those behaviors are proven.
    /// </summary>
    public class SubterraneanSitesLightEmitterBuilder :
        ZoneBuilderSandbox
    {
        public int Tier = 1;

        public int MinEmitters = 1;
        public int MaxEmitters = 3;

        public int MinSpacing = 10;

        public int TransitionExclusionRadius = 5;

        public string EmitterBlueprint =
            "SubterraneanSitesLightEmitter";


        public bool BuildZone(
            Zone Z
        )
        {
            if (Z == null)
                return true;

            ClampSettings();

            int seed =
                XRLCore.Core.Game.GetWorldSeed(
                    "SubterraneanSites:LightEmitter:" +
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
                CollectCandidates(
                    Z,
                    anchors
                );

            if (candidates.Count == 0)
                return true;

            int desired =
                rng.Next(
                    MinEmitters,
                    MaxEmitters + 1
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
                PlaceEmitter(
                    Z,
                    cell,
                    rng
                );
            }

            return true;
        }


        private void ClampSettings()
        {
            if (Tier < 1)
                Tier = 1;

            if (Tier > 8)
                Tier = 8;

            if (MinEmitters < 0)
                MinEmitters = 0;

            if (MaxEmitters < MinEmitters)
                MaxEmitters = MinEmitters;

            if (MinSpacing < 1)
                MinSpacing = 1;

            if (TransitionExclusionRadius < 0)
                TransitionExclusionRadius = 0;
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
                    !BasicCandidate(
                        Z,
                        cell,
                        anchors
                    )
                )
                {
                    continue;
                }

                //
                // The emitter itself is solid. Require the same broad-open
                // clearance used for other static impassable decorations so
                // it does not casually seal a room opening.
                //
                if (
                    !SubterraneanSites
                        .SubterraneanSitesEPPlacement
                        .HasBroadOpenClearance(
                            Z,
                            cell,
                            delegate(Cell nearby)
                            {
                                return
                                    BasicClearanceCell(
                                        Z,
                                        nearby
                                    );
                            },
                            1
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


        private bool BasicCandidate(
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

            return true;
        }


        private bool BasicClearanceCell(
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

                //
                // Chebyshev distance fits the rectangular room geometry
                // better than a Manhattan diamond here.
                //
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


        private void PlaceEmitter(
            Zone Z,
            Cell cell,
            System.Random rng
        )
        {
            if (
                Z == null ||
                cell == null ||
                rng == null
            )
            {
                return;
            }

            GameObject emitter =
                GameObject.Create(
                    EmitterBlueprint
                );

            if (emitter == null)
                return;

            XRL.World.Parts
                .SubterraneanSitesLightEmitter part =
                    emitter.GetPart<
                        XRL.World.Parts
                            .SubterraneanSitesLightEmitter
                    >();

            if (part != null)
            {
                part.Tier =
                    Tier;

                int maximum =
                    Math.Max(
                        1,
                        part.MaxCooldown
                    );

                //
                // Stagger the initial phase so several emitters do not
                // synchronize immediately on zone entry.
                //
                part.Cooldown =
                    rng.Next(
                        1,
                        maximum + 1
                    );
            }

            cell.AddObject(
                emitter
            );

            SubterraneanSites
                .SubterraneanSitesEPReservations
                .ClaimCell(
                    Z,
                    cell
                );

            emitter.MakeActive();
        }
    }
}

namespace XRL.World.Parts
{
    /// <summary>
    /// Periodic Light-EP beam emitter.
    ///
    /// State machine:
    ///
    /// COOLDOWN
    ///     |
    ///     v
    /// LOCK + TELEGRAPH
    ///     |
    ///     | one full turn
    ///     v
    /// FIRE
    ///     |
    ///     v
    /// NEW RANDOM COOLDOWN
    ///
    /// The locked endpoint never follows a moving target.
    /// </summary>
    [Serializable]
    public class SubterraneanSitesLightEmitter :
        IPart
    {
        public int Tier = 1;

        public int MinCooldown = 6;
        public int MaxCooldown = 12;

        public int Cooldown = 1;

        public bool HasLockedShot = false;

        public int LockedX = -1;
        public int LockedY = -1;

        public int MaxReflections = 5;

        [NonSerialized]
        private List<List<Cell>> LockedBeamLines;

        [NonSerialized]
        private HashSet<Cell> LockedRefractedCells;

        [NonSerialized]
        private bool AllowPaint;

        public int HighTierThreshold = 5;

        public int LowTierPenetration = 5;
        public string LowTierDamage = "1d12";

        public int HighTierPenetration = 7;
        public string HighTierDamage = "2d8";


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
            if (
                E.ID ==
                "EndTurn"
            )
            {
                TickEmitter();
            }

            return base.FireEvent(
                E
            );
        }


        private void TickEmitter()
        {
            if (
                ParentObject == null ||
                ParentObject.CurrentCell == null ||
                ParentObject.CurrentZone == null
            )
            {
                return;
            }

            //
            // Only run while this is actually the active gameplay zone.
            //
            if (!ParentObject.IsInActiveZone())
                return;

            ClampSettings();

            //
            // A shot locked on the previous turn fires now.
            //
            if (HasLockedShot)
            {
                FireLockedShot();

                ClearLockedShot();

                Cooldown =
                    Stat.Random(
                        MinCooldown,
                        MaxCooldown
                    );

                return;
            }

            //
            // Normal recharge.
            //
            if (Cooldown > 0)
            {
                Cooldown--;

                if (Cooldown > 0)
                    return;
            }

            //
            // Cooldown has matured. Choose an endpoint and expose the
            // warning line for the next full turn.
            //
            Cell target =
                ChooseTargetCell();

            if (target == null)
            {
                Cooldown = 1;
                return;
            }

            LockedX =
                target.X;

            LockedY =
                target.Y;

            HasLockedShot =
                true;

            LockedBeamLines =
                BuildReflectedBeamLines();
                
            WarnPlayerIfThreatened();
        }


        private void ClampSettings()
        {
            if (Tier < 1)
                Tier = 1;

            if (Tier > 8)
                Tier = 8;

            if (MinCooldown < 1)
                MinCooldown = 1;

            if (MaxCooldown < MinCooldown)
                MaxCooldown = MinCooldown;
        }

        private Cell ChooseTargetCell()
        {
            Zone Z =
                ParentObject.CurrentZone;

            if (Z == null)
                return null;

            GameObject player =
                The.Player;

            //
            // First preference:
            // the player, but ONLY when the cooldown has actually matured
            // and the player is visible right now.
            //
            if (
                player != null &&
                player.CurrentZone == Z &&
                player.CurrentCell != null &&
                CanSeeTarget(
                    player
                )
            )
            {
                return player.CurrentCell;
            }

            //
            // Second preference:
            // visible hostile creatures, but never creatures belonging to
            // Light's own extradimensional theme.
            //
            List<GameObject> targets =
                new List<GameObject>();

            Zone.ObjectEnumerator enumerator =
                Z.IterateObjects()
                    .GetEnumerator();

            while (
                enumerator.MoveNext()
            )
            {
                GameObject obj =
                    enumerator.Current;

                if (
                    obj == null ||
                    obj == ParentObject ||
                    obj == player ||
                    obj.CurrentCell == null ||
                    !obj.IsCombatObject()
                )
                {
                    continue;
                }

                //
                // Never deliberately target native Light denizens.
                //
                if (
                    IsLightNative(
                        obj
                    )
                )
                {
                    continue;
                }

                //
                // If the player is present, use the game's ordinary hostility
                // relationship to decide whether this is something worth targeting.
                //
                if (
                    player != null &&
                    player.CurrentZone == Z
                )
                {
                    if (
                        !obj.IsHostileTowards(
                            player
                        ) &&
                        !player.IsHostileTowards(
                            obj
                        )
                    )
                    {
                        continue;
                    }
                }
                else
                {
                    //
                    // Defensive fallback if no player object is available.
                    //
                    if (
                        obj.Brain == null ||
                        !obj.Brain.Allegiance.Hostile
                    )
                    {
                        continue;
                    }
                }

                if (
                    CanSeeTarget(
                        obj
                    )
                )
                {
                    targets.Add(
                        obj
                    );
                }
            }

            if (targets.Count > 0)
            {
                GameObject chosen =
                    targets[
                        Stat.Random(
                            0,
                            targets.Count - 1
                        )
                    ];

                if (
                    chosen != null &&
                    chosen.CurrentCell != null
                )
                {
                    return chosen.CurrentCell;
                }
            }

            //
            // No valid target:
            // fire a purposeless beam across the zone anyway.
            //
            return
                PickRandomPerimeterCell(
                    Z
                );
        }

        private bool IsLightNative(
            GameObject obj
        )
        {
            if (obj == null)
                return false;

            return
                string.Equals(
                    obj.GetStringProperty(
                        "SubterraneanSitesEPTheme",
                        ""
                    ),
                    "Light",
                    StringComparison.OrdinalIgnoreCase
                );
        }

        private bool CanSeeTarget(
            GameObject target
        )
        {
            if (
                target == null ||
                target.CurrentCell == null ||
                ParentObject == null
            )
            {
                return false;
            }

            return
                ParentObject.HasLOSTo(
                    target,
                    IncludeSolid: true,
                    BlackoutStops: false,
                    UseTargetability: true
                );
        }


        private Cell PickRandomPerimeterCell(
            Zone Z
        )
        {
            if (
                Z == null ||
                Z.Width < 2 ||
                Z.Height < 2
            )
            {
                return null;
            }

            //
            // Choose one of four map edges, then a random position along it.
            // This normally gives much longer and more interesting idle shots
            // than selecting one of eight compass directions.
            //
            int edge =
                Stat.Random(
                    0,
                    3
                );

            int x;
            int y;

            if (edge == 0)
            {
                x =
                    Stat.Random(
                        0,
                        Z.Width - 1
                    );

                y = 0;
            }
            else if (edge == 1)
            {
                x =
                    Stat.Random(
                        0,
                        Z.Width - 1
                    );

                y =
                    Z.Height - 1;
            }
            else if (edge == 2)
            {
                x = 0;

                y =
                    Stat.Random(
                        0,
                        Z.Height - 1
                    );
            }
            else
            {
                x =
                    Z.Width - 1;

                y =
                    Stat.Random(
                        0,
                        Z.Height - 1
                    );
            }

            return
                Z.GetCell(
                    x,
                    y
                );
        }


        private void ClearLockedShot()
        {
            HasLockedShot =
                false;

            LockedX = -1;
            LockedY = -1;

            if (LockedBeamLines != null)
            {
                LockedBeamLines.Clear();
                LockedBeamLines = null;
            }

            if (LockedRefractedCells != null)
            {
                LockedRefractedCells.Clear();
                LockedRefractedCells = null;
            }

            AllowPaint =
                false;
        }


        private List<Cell> BuildLockedLine()
        {
            List<Cell> result =
                new List<Cell>();

            if (
                ParentObject == null ||
                ParentObject.CurrentCell == null ||
                ParentObject.CurrentZone == null ||
                LockedX < 0 ||
                LockedY < 0
            )
            {
                return result;
            }

            Zone Z =
                ParentObject.CurrentZone;

            int x0 =
                ParentObject.CurrentCell.X;

            int y0 =
                ParentObject.CurrentCell.Y;

            int x1 =
                LockedX;

            int y1 =
                LockedY;

            int dx =
                Math.Abs(
                    x1 - x0
                );

            int sx =
                x0 < x1
                    ? 1
                    : -1;

            int dy =
                -Math.Abs(
                    y1 - y0
                );

            int sy =
                y0 < y1
                    ? 1
                    : -1;

            int error =
                dx + dy;

            int safety =
                Z.Width *
                Z.Height *
                2;

            bool first =
                true;

            while (
                safety-- > 0
            )
            {
                Cell cell =
                    Z.GetCell(
                        x0,
                        y0
                    );

                if (cell == null)
                    break;

                result.Add(
                    cell
                );

                //
                // Include the first solid obstruction as the point where
                // the beam strikes, then stop.
                //
                if (
                    !first &&
                    cell.IsSolid(true)
                )
                {
                    break;
                }

                first =
                    false;

                if (
                    x0 == x1 &&
                    y0 == y1
                )
                {
                    break;
                }

                int doubled =
                    2 * error;

                if (doubled >= dy)
                {
                    error +=
                        dy;

                    x0 +=
                        sx;
                }

                if (doubled <= dx)
                {
                    error +=
                        dx;

                    y0 +=
                        sy;
                }
            }

            return result;
        }

        private List<List<Cell>> GetLockedBeamLines()
        {
            if (
                LockedBeamLines == null &&
                HasLockedShot
            )
            {
                LockedBeamLines =
                    BuildReflectedBeamLines();
            }

            return LockedBeamLines;
        }


        private List<List<Cell>> BuildReflectedBeamLines()
        {
            List<List<Cell>> lines =
                new List<List<Cell>>();

            List<Cell> initial =
                BuildLockedLine();

            if (
                initial == null ||
                initial.Count <= 1
            )
            {
                return lines;
            }

            lines.Add(
                initial
            );

            HashSet<GameObject> usedRefractors =
                new HashSet<GameObject>();
            
            LockedRefractedCells =
                new HashSet<Cell>();

            int reflections =
                0;

            //
            // This is intentionally a growing loop.
            //
            // A successful refraction adds another segment to 'lines',
            // which will subsequently be examined for another refractor.
            //
            for (
                int segmentIndex = 0;
                segmentIndex < lines.Count &&
                reflections < MaxReflections;
                segmentIndex++
            )
            {
                List<Cell> segment =
                    lines[
                        segmentIndex
                    ];

                if (
                    segment == null ||
                    segment.Count <= 1
                )
                {
                    continue;
                }

                float incomingAngle =
                    GetBeamAngle(
                        segment
                    );

                    for (
                        int i = 1;
                        i < segment.Count;
                        i++
                    )
                    {
                        Cell cell =
                            segment[i];

                        if (cell == null)
                            continue;

                        bool canReflect =
                            cell.HasObjectWithRegisteredEvent(
                                "RefractLight"
                            ) ||
                            cell.HasObjectWithRegisteredEvent(
                                "ReflectProjectile"
                            );

                        //
                        // Reflection gets first chance.
                        //
                        // This is important for the player: if Light Manipulation or another
                        // reflection effect succeeds, the beam redirects instead of striking them.
                        //
                        if (canReflect)
                        {
                            GameObject refractor;
                            int outgoingDirection;
                            string sound;
                            string verb;

                            if (
                                TryRefractBeam(
                                    cell,
                                    incomingAngle,
                                    out refractor,
                                    out outgoingDirection,
                                    out sound,
                                    out verb
                                ) &&
                                refractor != null &&
                                !usedRefractors.Contains(
                                    refractor
                                )
                            )
                            {
                                usedRefractors.Add(
                                    refractor
                                );

                                LockedRefractedCells.Add(
                                    cell
                                );

                                reflections++;

                                //
                                // Current beam ends on the successful refractor.
                                //
                                if (
                                    i + 1 <
                                    segment.Count
                                )
                                {
                                    segment.RemoveRange(
                                        i + 1,
                                        segment.Count -
                                        i -
                                        1
                                    );
                                }

                                List<Cell> continuation =
                                    BuildReflectedContinuation(
                                        cell,
                                        outgoingDirection
                                    );

                                if (
                                    continuation != null &&
                                    continuation.Count > 1
                                )
                                {
                                    lines.Add(
                                        continuation
                                    );
                                }

                                break;
                            }
                        }

                        //
                        // If the player occupies this cell and did NOT successfully reflect
                        // the beam, the ray ends here.
                        //
                        GameObject player =
                            The.Player;

                        if (
                            player != null &&
                            player.CurrentZone ==
                                ParentObject.CurrentZone &&
                            player.CurrentCell ==
                                cell
                        )
                        {
                            if (
                                i + 1 <
                                segment.Count
                            )
                            {
                                segment.RemoveRange(
                                    i + 1,
                                    segment.Count -
                                    i -
                                    1
                                );
                            }

                            break;
                        }
                    }


            }

            return lines;
        }


        private float GetBeamAngle(
            List<Cell> segment
        )
        {
            if (
                segment == null ||
                segment.Count <= 1
            )
            {
                return 0f;
            }

            Cell first =
                segment[0];

            Cell last =
                segment[
                    segment.Count - 1
                ];

            if (
                first == null ||
                last == null
            )
            {
                return 0f;
            }

            return
                (float)Math.Atan2(
                    last.X -
                    first.X,
                    last.Y -
                    first.Y
                ).toDegrees();
        }


        private bool TryRefractBeam(
            Cell cell,
            float incomingAngle,
            out GameObject refractor,
            out int outgoingDirection,
            out string sound,
            out string verb
        )
        {
            refractor =
                null;

            outgoingDirection =
                -1;

            sound =
                "sfx_light_refract";

            verb =
                "refract";

            if (cell == null)
                return false;

            bool continueBeam =
                true;

            //
            // This intentionally mirrors LightManipulation's RefractLight
            // event contract.
            //
            if (
                cell.HasObjectWithRegisteredEvent(
                    "RefractLight"
                )
            )
            {
                Event refractEvent =
                    Event.New(
                        "RefractLight"
                    );

                refractEvent.SetParameter(
                    "Projectile",
                    (object)null
                );

                refractEvent.SetParameter(
                    "Attacker",
                    ParentObject
                );

                refractEvent.SetParameter(
                    "Cell",
                    cell
                );

                refractEvent.SetParameter(
                    "Angle",
                    incomingAngle
                );

                refractEvent.SetParameter(
                    "Direction",
                    Stat.Random(
                        0,
                        359
                    )
                );

                refractEvent.SetParameter(
                    "Verb",
                    null
                );

                refractEvent.SetParameter(
                    "Sound",
                    "sfx_light_refract"
                );

                refractEvent.SetParameter(
                    "By",
                    (object)null
                );

                continueBeam =
                    cell.FireEvent(
                        refractEvent
                    );

                if (!continueBeam)
                {
                    refractor =
                        refractEvent
                            .GetGameObjectParameter(
                                "By"
                            );

                    sound =
                        refractEvent
                            .GetStringParameter(
                                "Sound"
                            );

                    verb =
                        refractEvent
                            .GetStringParameter(
                                "Verb"
                            ) ??
                        "refract";

                    outgoingDirection =
                        refractEvent
                            .GetIntParameter(
                                "Direction"
                            )
                            .normalizeDegrees();
                }
            }

            //
            // Vanilla Light Manipulation also allows ordinary projectile
            // reflection systems to redirect a laser. Preserve that behavior.
            //
            if (
                continueBeam &&
                cell.HasObjectWithRegisteredEvent(
                    "ReflectProjectile"
                )
            )
            {
                Event reflectEvent =
                    Event.New(
                        "ReflectProjectile"
                    );

                reflectEvent.SetParameter(
                    "Projectile",
                    (object)null
                );

                reflectEvent.SetParameter(
                    "Attacker",
                    ParentObject
                );

                reflectEvent.SetParameter(
                    "Cell",
                    cell
                );

                reflectEvent.SetParameter(
                    "Angle",
                    incomingAngle
                );

                reflectEvent.SetParameter(
                    "Direction",
                    Stat.Random(
                        0,
                        359
                    )
                );

                reflectEvent.SetParameter(
                    "Verb",
                    null
                );

                reflectEvent.SetParameter(
                    "Sound",
                    "sfx_light_refract"
                );

                reflectEvent.SetParameter(
                    "By",
                    (object)null
                );

                continueBeam =
                    cell.FireEvent(
                        reflectEvent
                    );

                if (!continueBeam)
                {
                    refractor =
                        reflectEvent
                            .GetGameObjectParameter(
                                "By"
                            );

                    sound =
                        reflectEvent
                            .GetStringParameter(
                                "Sound"
                            );

                    verb =
                        reflectEvent
                            .GetStringParameter(
                                "Verb"
                            ) ??
                        "refract";

                    outgoingDirection =
                        reflectEvent
                            .GetIntParameter(
                                "Direction"
                            )
                            .normalizeDegrees();
                }
            }

            if (continueBeam)
                return false;

            if (
                !GameObject.Validate(
                    ref refractor
                )
            )
            {
                return false;
            }

            return
                outgoingDirection >= 0;
        }


        private List<Cell> BuildReflectedContinuation(
            Cell origin,
            int direction
        )
        {
            List<Cell> result =
                new List<Cell>();

            if (
                origin == null ||
                origin.ParentZone == null
            )
            {
                return result;
            }

            Zone Z =
                origin.ParentZone;

            result.Add(
                origin
            );

            float x =
                origin.X;

            float y =
                origin.Y;

            float radians =
                (float)direction *
                (
                    MathF.PI /
                    180f
                );

            float stepX =
                (float)Math.Sin(
                    radians
                );

            float stepY =
                (float)Math.Cos(
                    radians
                );

            Cell previous =
                origin;

            int safety =
                Math.Min(
                    400,
                    Z.Width *
                    Z.Height
                );

            while (
                safety-- > 0
            )
            {
                x +=
                    stepX;

                y +=
                    stepY;

                Cell cell =
                    Z.GetCell(
                        (int)x,
                        (int)y
                    );

                if (cell == null)
                    break;

                if (cell == previous)
                    continue;

                result.Add(
                    cell
                );

                previous =
                    cell;

                //
                // Include the impact cell in the visible beam, then terminate.
                //
                if (
                    cell.IsSolid(
                        true
                    )
                )
                {
                    break;
                }
            }

            return result;
        }

        private void WarnPlayerIfThreatened()
        {
            GameObject player =
                The.Player;

            if (
                player == null ||
                player.CurrentCell == null ||
                player.CurrentZone !=
                    ParentObject.CurrentZone
            )
            {
                return;
            }

            List<List<Cell>> lines =
                GetLockedBeamLines();

            if (lines == null)
                return;

            foreach (
                List<Cell> line
                in lines
            )
            {
                if (line == null)
                    continue;

                foreach (
                    Cell cell
                    in line
                )
                {
                    if (
                        cell ==
                        player.CurrentCell
                    )
                    {
                        AutoAct.Interrupt(
                            "you are in the path of a focused light beam",
                            cell,
                            null,
                            IsThreat: true
                        );

                        return;
                    }
                }
            }
        }


        private void FireLockedShot()
        {
            List<List<Cell>> lines =
                GetLockedBeamLines();

            if (
                lines == null ||
                lines.Count == 0
            )
            {
                return;
            }

            ParentObject.PlayWorldSound(
                "Sounds/Abilities/sfx_ability_mutation_lightManipulation_laser_fire"
            );

            MissileWeaponVFXConfiguration config =
                MissileWeaponVFXConfiguration
                    .next();

            int pathIndex =
                0;

            foreach (
                List<Cell> line
                in lines
            )
            {
                if (
                    line == null ||
                    line.Count <= 1
                )
                {
                    continue;
                }

                config.addStep(
                    pathIndex,
                    line[0].Location
                );

                config.addStep(
                    pathIndex,
                    line[
                        line.Count - 1
                    ].Location
                );

                config.setPathProjectileVFX(
                    pathIndex,
                    "MissileWeaponsEffects/gradient_laser",
                    "duration::2"
                );

                //
                // Every continuation after the first begins at a refractor.
                //
                if (
                    pathIndex > 0 &&
                    line[0] != null
                )
                {
                    line[0].PlayWorldSound(
                        "sfx_light_refract",
                        0.5f,
                        0f,
                        Combat: true
                    );
                }

                pathIndex++;
            }

            if (pathIndex > 0)
            {
                CombatJuice
                    .missileWeaponVFX(
                        config,
                        Async: true
                    );
            }

            ApplyBeamDamage(
                lines
            );
        }

        private void ApplyBeamDamage(
            List<List<Cell>> lines
        )
        {
            if (
                lines == null ||
                lines.Count == 0
            )
            {
                return;
            }

            string damageRoll =
                Tier >= HighTierThreshold
                    ? HighTierDamage
                    : LowTierDamage;

            int penetration =
                Tier >= HighTierThreshold
                    ? HighTierPenetration
                    : LowTierPenetration;

            //
            // Even if a reflected path crosses the same creature twice,
            // one emitter discharge damages it only once.
            //
            HashSet<GameObject> alreadyHit =
                new HashSet<GameObject>();

            foreach (
                List<Cell> line
                in lines
            )
            {
                if (
                    line == null ||
                    line.Count <= 1
                )
                {
                    continue;
                }

                //
                // Index zero is the emitter/refractor cell that begins this segment.
                //
                for (
                    int i = 1;
                    i < line.Count;
                    i++
                )
                {
                    Cell cell =
                        line[i];

                    if (cell == null)
                        continue;

                    //
                    // Anything occupying a cell that successfully refracted this shot
                    // is protected from this discharge.
                    //
                    // This is principally what prevents an attuned player from both
                    // reflecting the laser and taking its damage.
                    //
                    if (
                        LockedRefractedCells != null &&
                        LockedRefractedCells.Contains(
                            cell
                        )
                    )
                    {
                        continue;
                    }

                    foreach (
                        GameObject obj
                        in cell.GetObjects()
                    )
                    {
                        if (
                            obj == null ||
                            !obj.IsReal ||
                            !obj.IsCombatObject()
                        )
                        {
                            continue;
                        }

                        if (
                            !ParentObject.PhaseMatches(
                                obj
                            )
                        )
                        {
                            continue;
                        }

                        if (
                            alreadyHit.Contains(
                                obj
                            )
                        )
                        {
                            continue;
                        }

                        alreadyHit.Add(
                            obj
                        );

                        InflictBeamDamage(
                            obj,
                            damageRoll,
                            penetration
                        );
                    }
                }
            }
        }

        private void InflictBeamDamage(
            GameObject target,
            string damageRoll,
            int penetration
        )
        {
            if (
                target == null ||
                damageRoll.IsNullOrEmpty()
            )
            {
                return;
            }

            int penetrations =
                Stat.RollDamagePenetrations(
                    target.Stat(
                        "AV"
                    ),
                    penetration,
                    penetration
                );

            if (penetrations <= 0)
                return;

            int amount = 0;

            for (
                int i = 0;
                i < penetrations;
                i++
            )
            {
                amount +=
                    damageRoll.RollCached();
            }

            if (amount <= 0)
                return;

            string resultColor =
                Stat.GetResultColor(
                    penetrations
                );

            target.TakeDamage(
                amount,
                Owner: ParentObject,
                Attacker: ParentObject,
                Message:
                    "from %t focused light beam! {{" +
                    resultColor +
                    "|(x" +
                    penetrations.ToString() +
                    ")}}",
                Attributes: "Light Laser",
                ShowForInanimate: true
            );
        }



      


        public override bool FinalRender(
            RenderEvent E
        )
        {
            AllowPaint =
                HasLockedShot &&
                ParentObject != null &&
                ParentObject.IsInActiveZone();

            E.WantsToPaint =
                E.WantsToPaint ||
                AllowPaint;

            return true;
        }

        public override void OnPaint(
            ScreenBuffer SB
        )
        {
            if (
                !AllowPaint ||
                !HasLockedShot ||
                ParentObject == null
            )
            {
                return;
            }

            List<List<Cell>> lines =
                GetLockedBeamLines();

            if (
                lines == null ||
                lines.Count == 0
            )
            {
                return;
            }

            foreach (
                List<Cell> line
                in lines
            )
            {
                if (
                    line == null ||
                    line.Count <= 1
                )
                {
                    continue;
                }

                int denominator =
                    Math.Max(
                        1,
                        500 /
                        line.Count
                    );

                int pulse =
                    (int)(
                        IComponent<GameObject>
                            .frameTimerMS %
                        500 /
                        denominator
                    );

                for (
                    int i = 1;
                    i < line.Count;
                    i++
                )
                {
                    Cell cell =
                        line[i];

                    if (
                        cell == null ||
                        !cell.IsVisible() ||
                        cell.ParentZone !=
                            ParentObject.CurrentZone
                    )
                    {
                        continue;
                    }

                    ConsoleChar consoleChar =
                        SB[cell];

                    if (i == pulse)
                    {
                        consoleChar.Background =
                            The.Color.DarkRed;

                        consoleChar.TileBackground =
                            The.Color.DarkRed;

                        consoleChar.Detail =
                            The.Color.Red;

                        consoleChar.TileForeground =
                            The.Color.Red;
                    }
                    else
                    {
                        consoleChar.Background =
                            The.Color.Red;

                        consoleChar.TileBackground =
                            The.Color.Red;

                        consoleChar.Detail =
                            The.Color.DarkRed;

                        consoleChar.TileForeground =
                            The.Color.DarkRed;
                    }

                    consoleChar.SetForeground(
                        'r'
                    );
                }
            }
        }


        
    }
}