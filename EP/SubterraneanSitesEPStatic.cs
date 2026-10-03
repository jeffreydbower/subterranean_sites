using System;
using System.Collections.Generic;
using Genkit;
using XRL;
using XRL.Core;
using XRL.Rules;
using XRL.World;
using XRL.World.Parts;
using System.Reflection;
using XRL.UI;
using XRL.World.Capabilities;


namespace SubterraneanSites
{
    /// <summary>
    /// STATIC DIMENSION
    ///
    /// C1:
    ///     warm-static environment with Precognition attunement.
    ///
    /// C2:
    ///     warm-static spills.
    ///
    /// C3:
    ///     extradimensional structural wall materials.
    ///
    /// C4:
    ///     directly-connected rectangular rooms with no dedicated halls,
    ///     Rocky floor, and runtime wall redistribution.
    ///
    /// C5:
    ///     Static-themed decorations.
    /// </summary>
    internal sealed class SubterraneanSitesEPStaticTheme :
        ISubterraneanSitesEPCategoryProvider,
        ISubterraneanSitesEPPrimaryObjectProvider,
        ISubterraneanSitesEPAttunementProvider,
        ISubterraneanSitesEPDenizenAdaptationProvider,
        ISubterraneanSitesEPSignatureMutationProvider
    {
        public string ThemeKey
        {
            get { return "Static"; }
        }

        public string SignatureMutationClass
        {
            get { return "Precognition"; }
        }


        public void ApplyDenizenAdaptation(
            GameObject creature
        )
        {
            if (creature == null)
                return;

            creature.RequirePart<
                XRL.World.Parts
                    .SubterraneanSitesEPStaticWarmStaticImmunity
            >();
        }


        public int MinimumHoleSeparation
        {
            get { return 25; }
        }

        internal const string StaticC3WallProperty =
            "SubterraneanSites_EP_StaticC3Wall";

        internal const string StaticC3InitialWallAProperty =
            "SubterraneanSites_EP_StaticC3InitialWallA";

        internal const string StaticC3InitialWallBProperty =
            "SubterraneanSites_EP_StaticC3InitialWallB";


        internal static readonly string[] WallBlueprints =
        {
            "Shale",
            "Sandstone",
            "Limestone",
            "Marl",
            //"Halite",
            "Gypsum",
            "Black Shale",
            "Coral Rag",
            "Slate",
            "Quartzite",
            "Marble",
            "Black Marble",
            "Granite",
            "Basalt",
            "Fulcrete",
            "EbonFulcrete",
            "MetalWall",
            "RustedMetalWall",
            "CrysteelPlatedWall",
            "Foamcrete",
            "Concrete",
            "BrickWall",
            "MachineWallEmptyTubing",
            "Burnished Azzurum"
        };


        internal static List<string> PickInitialWallSubset(
            System.Random rng,
            int count
        )
        {
            List<string> available =
                new List<string>(
                    WallBlueprints
                );


            List<string> result =
                new List<string>();


            if (
                rng == null ||
                available.Count == 0
            )
            {
                result.Add(
                    "Fulcrete"
                );

                return result;
            }


            count =
                Math.Max(
                    1,
                    Math.Min(
                        count,
                        available.Count
                    )
                );


            while (
                result.Count < count &&
                available.Count > 0
            )
            {
                int index =
                    rng.Next(
                        available.Count
                    );


                result.Add(
                    available[index]
                );


                available.RemoveAt(
                    index
                );
            }


            return result;
        }


       internal static string PickDifferentWallBlueprint(
            string currentBlueprint,
            string initialWallA,
            string initialWallB
        )
        {
            if (
                WallBlueprints == null ||
                WallBlueprints.Length == 0
            )
            {
                return "Fulcrete";
            }


            //
            // Half of all substitutions come from the layer's original
            // two-material palette.
            //
            // The other half can come from the entire Static wall vocabulary.
            //
            for (
                int attempt = 0;
                attempt < 30;
                attempt++
            )
            {
                string selected;


                if (
                    Stat.Random(
                        1,
                        100
                    ) <= 50 &&
                    !initialWallA.IsNullOrEmpty()
                )
                {
                    if (
                        !initialWallB.IsNullOrEmpty() &&
                        !string.Equals(
                            initialWallA,
                            initialWallB,
                            StringComparison.Ordinal
                        )
                    )
                    {
                        selected =
                            Stat.Random(
                                0,
                                1
                            ) == 0
                                ? initialWallA
                                : initialWallB;
                    }
                    else
                    {
                        selected =
                            initialWallA;
                    }
                }
                else
                {
                    selected =
                        WallBlueprints[
                            Stat.Random(
                                0,
                                WallBlueprints.Length - 1
                            )
                        ];
                }


                if (
                    !selected.IsNullOrEmpty() &&
                    !string.Equals(
                        selected,
                        currentBlueprint,
                        StringComparison.Ordinal
                    )
                )
                {
                    return selected;
                }
            }


            //
            // Extremely unlikely fallback: find any different valid wall.
            //
            foreach (
                string blueprint
                in WallBlueprints
            )
            {
                if (
                    !string.Equals(
                        blueprint,
                        currentBlueprint,
                        StringComparison.Ordinal
                    )
                )
                {
                    return blueprint;
                }
            }


            return currentBlueprint;
        }


        internal const string StaticC4InitialFloorAProperty =
            "SubterraneanSites_EP_StaticC4InitialFloorA";

        internal const string StaticC4InitialFloorBProperty =
            "SubterraneanSites_EP_StaticC4InitialFloorB";


        internal static readonly string[] FloorPainters =
        {
            "Rocky",
            "Dirty",
            "PaleDirty",
            "BlueTile",
            "ConcreteFloor",
            "CrystalDirty",
            "CrystalGrassy",
            "Mushroomy",
            "KelpDirty",
            "UndergroundGrassy",
            "Flowery",
            "Grassy"
        };


        private static readonly Dictionary<string, MethodInfo>
            FloorPainterMethodCache =
                new Dictionary<string, MethodInfo>(
                    StringComparer.Ordinal
                );


        internal static List<string> PickInitialFloorSubset(
            System.Random rng,
            int count
        )
        {
            List<string> available =
                new List<string>(
                    FloorPainters
                );


            List<string> result =
                new List<string>();


            if (
                rng == null ||
                available.Count == 0
            )
            {
                result.Add(
                    "Rocky"
                );

                return result;
            }


            count =
                Math.Max(
                    1,
                    Math.Min(
                        count,
                        available.Count
                    )
                );


            while (
                result.Count < count &&
                available.Count > 0
            )
            {
                int index =
                    rng.Next(
                        available.Count
                    );


                result.Add(
                    available[index]
                );


                available.RemoveAt(
                    index
                );
            }


            return result;
        }


        internal static string PickRuntimeFloorPainter(
            string initialFloorA,
            string initialFloorB
        )
        {
            if (
                FloorPainters == null ||
                FloorPainters.Length == 0
            )
            {
                return "Rocky";
            }


            //
            // Half of all entropy substitutions favor the two materials
            // the level originally started with.
            //
            if (
                Stat.Random(
                    1,
                    100
                ) <= 50 &&
                !initialFloorA.IsNullOrEmpty()
            )
            {
                if (
                    !initialFloorB.IsNullOrEmpty() &&
                    !string.Equals(
                        initialFloorA,
                        initialFloorB,
                        StringComparison.Ordinal
                    )
                )
                {
                    return
                        Stat.Random(
                            0,
                            1
                        ) == 0
                            ? initialFloorA
                            : initialFloorB;
                }


                return initialFloorA;
            }


            //
            // Otherwise reality can decay into any Static floor treatment.
            //
            return
                FloorPainters[
                    Stat.Random(
                        0,
                        FloorPainters.Length - 1
                    )
                ];
        }


        internal static void ApplyFloorPainter(
            Cell cell,
            string painterName
        )
        {
            if (cell == null)
                return;


            if (painterName.IsNullOrEmpty())
            {
                painterName =
                    "Rocky";
            }


            //
            // The new Static floor completely replaces the old painted
            // substrate on this cell.
            //
            cell.PaintTile =
                null;

            cell.PaintTileColor =
                null;

            cell.PaintColorString =
                null;

            cell.PaintDetailColor =
                null;

            cell.PaintRenderString =
                null;


            MethodInfo method =
                ResolveFloorPainterMethod(
                    painterName
                );


            if (method != null)
            {
                try
                {
                    method.Invoke(
                        null,
                        new object[]
                        {
                            cell
                        }
                    );

                    return;
                }
                catch
                {
                    //
                    // Fail soft below. A bad optional painter should never
                    // leave the cell without a valid Static floor.
                    //
                }
            }


            //
            // Known-safe fallback.
            //
            global::XRL.World.Parts
                .Rocky
                .Paint(
                    cell
                );
        }


        private static MethodInfo ResolveFloorPainterMethod(
            string painterName
        )
        {
            if (painterName.IsNullOrEmpty())
                return null;


            MethodInfo cached;


            if (
                FloorPainterMethodCache.TryGetValue(
                    painterName,
                    out cached
                )
            )
            {
                return cached;
            }


            Type painterType =
                typeof(
                    global::XRL.World.Parts.Rocky
                )
                .Assembly
                .GetType(
                    "XRL.World.Parts." +
                    painterName
                );


            if (painterType == null)
                return null;


            BindingFlags flags =
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Static;


            MethodInfo method =
                painterType.GetMethod(
                    "PaintCell",
                    flags,
                    null,
                    new Type[]
                    {
                        typeof(Cell)
                    },
                    null
                );


            if (method == null)
            {
                method =
                    painterType.GetMethod(
                        "Paint",
                        flags,
                        null,
                        new Type[]
                        {
                            typeof(Cell)
                        },
                        null
                    );
            }


            if (method != null)
            {
                FloorPainterMethodCache[
                    painterName
                ] =
                    method;
            }


            return method;
        }

        private static void PaintFloorCell(
            Cell cell
        )
        {
            if (cell == null)
                return;


            //
            // Rocky.Paint preserves existing paint.
            // Static owns C4 here, so clear the old paint first.
            //
            cell.PaintTile =
                null;

            cell.PaintColorString =
                null;

            cell.PaintTileColor =
                null;

            cell.PaintDetailColor =
                null;

            cell.PaintRenderString =
                null;


            global::XRL.World.Parts
                .Rocky
                .Paint(
                    cell
                );
        }


        public void RegisterCategory1(
            SubterraneanSitesEPLayerContext context
        )
        {
            if (context == null)
                return;


            if (The.Game != null)
            {
                The.Game.RequireSystem<
                    SubterraneanSitesEPStaticEnvironmentSystem
                >();
            }
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
                    .SubterraneanSitesEPStaticAttunementEffect(
                        SubterraneanSitesEPAttunementSystem
                            .DefaultDuration,
                        SignatureMutationClass,
                        mutationLevel
                    );


            successMessage =
                "Attunement grants:\n" +
                "Precognition (level " +
                mutationLevel.ToString() +
                ")\n" +
                "Protection from environmental random effects\n" +
                "Protection from dilute warm static.";


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
                "SubterraneanSitesStaticReefs"
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
                "SubterraneanSitesStaticReefs",
                "EntranceOnly", "1",
                "MinPatches", "1",
                "MaxPatches", "1",
                "MinCellsPerPatch", "3",
                "MaxCellsPerPatch", "5",
                "PlaceCyst", "0"
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
                "SubterraneanSitesStaticWarmStaticSpills"
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
                "SubterraneanSitesStaticMaterials"
            );


            //
            // Static C3 owns its own independent runtime material instability.
            //
            // It does not care who owns C1 and attunement does not stop it.
            //
            if (The.Game != null)
            {
                The.Game.RequireSystem<
                    SubterraneanSitesEPStaticMaterialSystem
                >();
            }
        }


        public void RegisterCategory4Layout(
            SubterraneanSitesEPLayerContext context
        )
        {
            if (context == null)
                return;


            The.ZoneManager.AddZonePostBuilder(
                context.ZoneId,
                "SubterraneanSitesStaticRectLayout"
            );


            //
            // This runtime system belongs to Static C4, not Static C1.
            //
            // Therefore it must be installed when Static actually wins
            // Category 4, including when Static is the secondary dimension.
            //
            if (The.Game != null)
            {
                The.Game.RequireSystem<
                    SubterraneanSitesEPStaticTopologySystem
                >();
            }
        }


        public void RegisterCategory4Floor(
            SubterraneanSitesEPLayerContext context
        )
        {
            if (context == null)
                return;


            The.ZoneManager.AddZonePostBuilder(
                context.ZoneId,
                "SubterraneanSitesStaticFloor"
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
                "SubterraneanSitesStaticDecorations"
            );


            if (The.Game != null)
            {
                The.Game.RequireSystem<
                    SubterraneanSitesEPStaticDecorationSystem
                >();
            }
        }


        public void RegisterEntranceFloor(
            SubterraneanSitesEPEntranceContext context
        )
        {
            if (context == null)
                return;


            The.ZoneManager.AddZonePostBuilder(
                context.ZoneId,
                "SubterraneanSitesStaticFloor",
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
                "SubterraneanSitesStaticDecorations",
                "EntranceOnly", "1"
            );
        }
    }
}

namespace SubterraneanSites
{
    /// <summary>
    /// Static Category-1 runtime environment.
    ///
    /// While the player is in an EP zone whose actual
    /// Category-1 owner is Static:
    ///
    /// - an unattuned player receives one allowed random effect on entry
    /// - further random effects occur every 50-100 turns
    /// - Static attunement suppresses these effects
    ///
    /// The effect pool deliberately follows vanilla dilute warm static's
    /// own random-effect machinery, minus the effects explicitly excluded
    /// from the Static EP environment.
    /// </summary>
    [Serializable]
    public class SubterraneanSitesEPStaticEnvironmentSystem :
        IGameSystem
    {
        public const int MinimumEffectDelay =
            50;

        public const int MaximumEffectDelay =
            100;

        public int TurnsUntilEffect =
            0;

        public bool WasInStaticEnvironment =
            false;

        public bool WasStaticAttuned =
            false;


        //
        // Explicit vanilla effect CLR type names allowed for Static C1.
        // These are matched against LiquidWarmStatic.RandomEffects.
        //
        private static readonly string[] AllowedEffectTypeNames =
        {
            "BlinkingTicSickness",
            "Asleep",
            "Berserk",
            "Blaze_Tonic",
            "Bleeding",
            "Blind",
            "Broken",
            "Budding",
            "Burning",
            "AshPoison",
            "ShatterArmor",
            "CoatedInPlasma",
            "Confused",
            "Spectacles",
            "ShatteredArmor",
            "Dazed",
            "Disoriented",
            "Emboldened",
            "Enclosed",
            "Exhausted",
            "Flying",
            "Frenzied",
            "Frozen",
            "Gleaming",
            "Greased",
            "Grounded",
            "Hobbled",
            "AxonsInflated",
            "Ill",
            "Inspired",
            "Interdicted",
            "LatchedOnto",
            "Lost",
            "Lovesick",
            "Omniphase",
            "Overburdened",
            "Paralyzed",
            "PhasePoisoned",
            "Phased",
            "Luminous",
            "Poisoned",
            "PoisonGasPoison",
            "Prone",
            "Proselytized",
            "ShatterMentalArmor",
            "ElectromagneticPulsed",
            "Rusted",
            "Shaken",
            "Shamed",
            "Sitting",
            "AxonsDeflated",
            "Springing",
            "Running",
            "LiquidStained",
            "BasiliskPoison",
            "Stuck",
            "Stun",
            "StunGasStun",
            "Submerged",
            "LifeDrain",
            "Terrified",
            "Wakeful",
            "WarTrance"
        };


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
            Synchronize(
                advanceTimer: false
            );

            return true;
        }


        public override bool HandleEvent(
            EndTurnEvent E
        )
        {
            Synchronize(
                advanceTimer: true
            );

            return true;
        }

        private void Synchronize(
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


            Zone Z =
                player.CurrentZone;


            string category1 =
                SubterraneanSitesEPAttunementSystem
                    .GetCategory1Theme(
                        Z
                    );
            
            bool staticEnvironment =
                string.Equals(
                    category1,
                    "Static",
                    StringComparison.Ordinal
                );

            if (!staticEnvironment)
            {
                ResetTimer();

                return;
            }

            bool staticAttuned =
                SubterraneanSitesEPAttunementSystem
                    .IsAttunedTo(
                        player,
                        "Static"
                    );


            bool justEntered =
                !WasInStaticEnvironment;

            bool justBecameUnattuned =
                WasInStaticEnvironment &&
                WasStaticAttuned &&
                !staticAttuned;


            WasInStaticEnvironment =
                true;

            WasStaticAttuned =
                staticAttuned;


            //
            // Static attunement suppresses C1.
            //
            if (staticAttuned)
                return;


            //
            // Immediate effect:
            //
            // - first entering the underground Static C1 environment unattuned
            // - Static attunement expiring while still inside it
            //
            // Moving between layers of the same EP is not a new exposure.
            //
            if (
                justEntered ||
                justBecameUnattuned
            )
            {
                if (
                    TryApplyRandomEffect(
                        player,
                        Z
                    ) != null
                )
                {
                    ScheduleNextEffect();
                }

                return;
            }


            //
            // Zone activation synchronizes state.
            // Only turns advance the recurring timer.
            //
            if (!advanceTimer)
                return;


            if (TurnsUntilEffect > 0)
            {
                TurnsUntilEffect--;
            }


            if (TurnsUntilEffect > 0)
                return;


            if (
                TryApplyRandomEffect(
                    player,
                    Z
                ) != null
            )
            {
                ScheduleNextEffect();
            }
        }

        private void ScheduleNextEffect()
        {
            TurnsUntilEffect =
                Stat.Random(
                    MinimumEffectDelay,
                    MaximumEffectDelay
                );
        }

        private void ResetTimer()
        {
            TurnsUntilEffect =
                0;

            WasInStaticEnvironment =
                false;

            WasStaticAttuned =
                false;
        }

        private static Effect TryApplyRandomEffect(
            GameObject target,
            Zone Z
        )
        {
            if (
                target == null ||
                Z == null
            )
            {
                return null;
            }


            //
            // Vanilla LiquidWarmStatic.ApplyRandomEffectTo() retries
            // application several times because an otherwise-applicable
            // effect can still refuse ApplyEffect().
            //
            // Use the same retry pattern with our restricted effect pool.
            //
            for (
                int attempt = 0;
                attempt < 10;
                attempt++
            )
            {
                Effect effect =
                    CreateRandomAllowedEffect(
                        target,
                        Z
                    );


                if (effect == null)
                    return null;


                if (
                    target.ApplyEffect(
                        effect
                    )
                )
                {
                    Popup.Show(
                        "Chaos has its way."
                    );

                    return effect;
                }
            }


            return null;
        }


       


       private static Effect CreateRandomAllowedEffect(
            GameObject target,
            Zone Z
        )
        {
            if (
                target == null ||
                Z == null ||
                AllowedEffectTypeNames == null ||
                AllowedEffectTypeNames.Length == 0
            )
            {
                return null;
            }

            List<Type> candidates =
                new List<Type>();

            foreach (
                Type effectType
                in XRL.Liquids.LiquidWarmStatic.RandomEffects
            )
            {
                if (effectType == null)
                    continue;

                for (
                    int i = 0;
                    i < AllowedEffectTypeNames.Length;
                    i++
                )
                {
                    if (
                        string.Equals(
                            effectType.Name,
                            AllowedEffectTypeNames[i],
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                    {
                        candidates.Add(
                            effectType
                        );

                        break;
                    }
                }
            }

            //
            // Nothing in our whitelist exists in vanilla's warm-static
            // random-effect pool.
            //
            if (candidates.Count == 0)
                return null;

            //
            // Try each confirmed candidate at most once.
            //
            // Random removal means the result remains random, while guaranteeing
            // that one temporarily-inapplicable effect cannot monopolize the rolls.
            //
            while (candidates.Count > 0)
            {
                int index =
                    Stat.Random(
                        0,
                        candidates.Count - 1
                    );

                Type effectType =
                    candidates[index];

                candidates.RemoveAt(
                    index
                );

                try
                {
                    Effect effect =
                        Activator.CreateInstance(
                            effectType
                        ) as Effect;

                    if (effect == null)
                        continue;

                    if (
                        !effect.CanBeAppliedTo(
                            target
                        )
                    )
                    {
                        continue;
                    }

                    ITierInitialized tierInitialized =
                        effect as ITierInitialized;

                    if (tierInitialized == null)
                        continue;

                    tierInitialized.Initialize(
                        Z.NewTier
                    );

                    return effect;
                }
                catch
                {
                    //
                    // One unusual effect must not prevent Static C1 from
                    // trying another allowed candidate.
                    //
                    continue;
                }
            }

            return null;
        }
    }

    internal static class
    SubterraneanSitesEPStaticWarmStaticProtection
    {
        internal static bool ShouldBlockDiluteWarmStaticEffect(
            GameObject target,
            Effect effect
        )
        {
            if (
                target == null ||
                effect == null ||
                target.CurrentCell == null
            )
            {
                return false;
            }


            //
            // Use Qud's normal open-liquid lookup.
            //
            GameObject liquidObject =
                target.CurrentCell.GetOpenLiquidVolume();


            if (
                liquidObject == null ||
                liquidObject.LiquidVolume == null
            )
            {
                return false;
            }


            LiquidVolume liquid =
                liquidObject.LiquidVolume;


            //
            // This protection is specifically from dilute/impure
            // warm static in the environment.
            //
            // Pure warm static remains completely vanilla.
            //
            if (
                liquid.Amount(
                    "warmstatic"
                ) <= 0 ||
                liquid.IsPure()
            )
            {
                return false;
            }


            //
            // Do not block arbitrary effects merely because the actor is
            // standing in warm static. Only block an effect that belongs
            // to vanilla LiquidWarmStatic.RandomEffects.
            //
            Type incomingType =
                effect.GetType();


            foreach (
                Type warmStaticEffectType
                in XRL.Liquids.LiquidWarmStatic.RandomEffects
            )
            {
                if (
                    warmStaticEffectType ==
                    incomingType
                )
                {
                    return true;
                }
            }


            return false;
        }
    }




}


namespace XRL.World.ZoneBuilders
{

    /// <summary>
    /// Static Category-1 physical content.
    ///
    /// Places several small colonies of extradimensional Static reef.
    /// Exactly one underground colony contains a buzzing cyst filled
    /// with pure warm static.
    ///
    /// Reef cells claim their locations against later discrete EP
    /// placement. Permissive liquids may still spread through them.
    /// </summary>
    public class SubterraneanSitesStaticReefs :
        ZoneBuilderSandbox
    {
        public int EntranceOnly =
            0;


        public int MinPatches =
            3;

        public int MaxPatches =
            5;


        public int MinCellsPerPatch =
            4;

        public int MaxCellsPerPatch =
            9;


        public int PatchSeparationRadius =
            2;


        public int TransitionExclusionRadius =
            5;


        public int PlaceCyst =
            1;


        public string ReefBlueprint =
            "SubterraneanSitesStaticReef";

        public string CacheReefBlueprint =
            "SubterraneanSitesStaticReefCache";


        public bool BuildZone(
            Zone Z
        )
        {
            if (Z == null)
                return true;


            ClampSettings();


            int seed =
                XRLCore.Core.Game.GetWorldSeed(
                    "SubterraneanSites:StaticReefs:" +
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


            int desiredPatches =
                rng.Next(
                    MinPatches,
                    MaxPatches + 1
                );


            List<HashSet<Cell>> patches =
                new List<HashSet<Cell>>();


            HashSet<Cell> forbidden =
                new HashSet<Cell>();


            for (
                int patchIndex = 0;
                patchIndex < desiredPatches;
                patchIndex++
            )
            {
                List<Cell> candidates =
                    CollectCandidates(
                        Z,
                        anchors,
                        forbidden
                    );


                if (candidates.Count == 0)
                    break;


                int target =
                    rng.Next(
                        MinCellsPerPatch,
                        MaxCellsPerPatch + 1
                    );


                HashSet<Cell> patch =
                    SubterraneanSites
                        .SubterraneanSitesEPPlacement
                        .GrowBestPatch(
                            candidates,
                            target,
                            Math.Min(
                                12,
                                candidates.Count
                            ),
                            null,
                            rng
                        );


                if (patch.Count == 0)
                    continue;


                patches.Add(
                    patch
                );


                AddPatchAndHaloToForbidden(
                    Z,
                    patch,
                    forbidden
                );
            }


            if (patches.Count == 0)
                return true;


            //
            // Exactly one successfully-created underground patch
            // gets the warm-static cyst.
            //
            int cystPatchIndex =
                -1;


            if (PlaceCyst != 0)
            {
                cystPatchIndex =
                    rng.Next(
                        patches.Count
                    );
            }


            for (
                int patchIndex = 0;
                patchIndex < patches.Count;
                patchIndex++
            )
            {
                HashSet<Cell> patch =
                    patches[patchIndex];


                Cell cystCell =
                    null;


                if (
                    patchIndex ==
                    cystPatchIndex
                )
                {
                    List<Cell> cells =
                        new List<Cell>(
                            patch
                        );


                    if (cells.Count > 0)
                    {
                        cystCell =
                            cells[
                                rng.Next(
                                    cells.Count
                                )
                            ];
                    }
                }


                foreach (
                    Cell cell
                    in patch
                )
                {
                    if (cell == null)
                        continue;


                    string blueprint =
                        cell == cystCell
                            ? CacheReefBlueprint
                            : ReefBlueprint;


                    GameObject reef =
                        GameObjectFactory
                            .Factory
                            .CreateObject(
                                blueprint
                            );


                    if (reef == null)
                        continue;


                    cell.AddObject(
                        reef
                    );


                    //
                    // Static reef is C1 physical content.
                    // Later ordinary EP objects should respect it.
                    //
                    SubterraneanSites
                        .SubterraneanSitesEPReservations
                        .ClaimCell(
                            Z,
                            cell
                        );
                }
            }


            return true;
        }


        private List<Cell> CollectCandidates(
            Zone Z,
            List<Location2D> anchors,
            HashSet<Cell> forbidden
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
                    !CellAvailable(
                        Z,
                        cell,
                        anchors,
                        forbidden
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


        private bool CellAvailable(
            Zone Z,
            Cell cell,
            List<Location2D> anchors,
            HashSet<Cell> forbidden
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
                forbidden != null &&
                forbidden.Contains(
                    cell
                )
            )
            {
                return false;
            }


            //
            // Respect earlier semantic ownership.
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


        private void AddPatchAndHaloToForbidden(
            Zone Z,
            HashSet<Cell> patch,
            HashSet<Cell> forbidden
        )
        {
            if (
                Z == null ||
                patch == null ||
                forbidden == null
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


                for (
                    int dx =
                        -PatchSeparationRadius;
                    dx <=
                        PatchSeparationRadius;
                    dx++
                )
                {
                    for (
                        int dy =
                            -PatchSeparationRadius;
                        dy <=
                            PatchSeparationRadius;
                        dy++
                    )
                    {
                        Cell nearby =
                            Z.GetCell(
                                cell.X + dx,
                                cell.Y + dy
                            );


                        if (nearby != null)
                        {
                            forbidden.Add(
                                nearby
                            );
                        }
                    }
                }
            }
        }


        private void ClampSettings()
        {
            MinPatches =
                Math.Max(
                    0,
                    MinPatches
                );


            MaxPatches =
                Math.Max(
                    MinPatches,
                    MaxPatches
                );


            MinCellsPerPatch =
                Math.Max(
                    1,
                    MinCellsPerPatch
                );


            MaxCellsPerPatch =
                Math.Max(
                    MinCellsPerPatch,
                    MaxCellsPerPatch
                );


            PatchSeparationRadius =
                Math.Max(
                    0,
                    PatchSeparationRadius
                );


            TransitionExclusionRadius =
                Math.Max(
                    0,
                    TransitionExclusionRadius
                );
        }
    }

    public class SubterraneanSitesStaticWarmStaticSpills :
    ZoneBuilderSandbox
    {
        public int MinLargeSpillPercent =
            4;

        public int MaxLargeSpillPercent =
            7;


        public int MinSmallSpills =
            4;

        public int MaxSmallSpills =
            6;


        public int MinSmallSpillCells =
            8;

        public int MaxSmallSpillCells =
            14;


        public int TransitionExclusionRadius =
            5;


        private const string PoolBlueprint =
            "SubterraneanSitesStaticDilutedWarmStaticPuddle";


        public bool BuildZone(
            Zone Z
        )
        {
            if (Z == null)
                return true;


            ClampSettings();


            int seed =
                XRLCore.Core.Game.GetWorldSeed(
                    "SubterraneanSites:StaticWarmStaticSpills:" +
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


            List<Cell> interior =
                GetSpillCells(
                    Z,
                    anchors
                );


            if (interior.Count == 0)
                return true;


            //
            // One large contiguous spill.
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
                AddWarmStatic(
                    cell
                );
            }


            //
            // Smaller separate spills.
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
                    AddWarmStatic(
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


            return true;
        }


        private void AddWarmStatic(
            Cell cell
        )
        {
            if (cell == null)
                return;


            //
            // Dilute warm static is a permissive spill.
            //
            // Deliberately:
            // - does not check EP reservations
            // - does not claim its cells
            // - may mix with existing open liquid
            //
            SubterraneanSites
                .SubterraneanSitesEPLiquids
                .AddOrMixLiquid(
                    cell,
                    "warmstatic-1000,water-500",
                    PoolBlueprint
                );
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


                //
                // Static C2 uses whichever Category-4 geometry won.
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


                //
                // A spill does not care about reservation ownership.
                // It only needs somewhere liquid can physically exist.
                //
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


        private void ClampSettings()
        {
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
        }
    }
    /// <summary>
    /// Static Category-4 initial geometry.
    ///
    /// The starting shape is a connected collection of rectangular rooms.
    /// There are no dedicated halls. New rectangles either overlap the
    /// existing complex modestly or touch it directly.
    ///
    /// Excessive overlap is rejected so the result does not collapse into
    /// one giant open rectangle.
    /// </summary>
    /// 
    /// 
    /// 
    /// 
    public class SubterraneanSitesStaticRectLayout :
    ZoneBuilderSandbox
    {
        public int MinRooms = 16;
        public int MaxRooms = 22;

        public int WideMinWidth = 9;
        public int WideMaxWidth = 16;

        public int WideMinHeight = 4;
        public int WideMaxHeight = 6;

        public int TallMinWidth = 5;
        public int TallMaxWidth = 8;

        public int TallMinHeight = 8;
        public int TallMaxHeight = 11;

        public int BorderWidth = 2;


        //
        // Roughly one third of room attachments merge shallowly
        // into the parent instead of leaving a separating wall.
        //
        public int OverlapAttachmentChancePercent = 35;

        //
        // Even an overlap-mode room must contribute mostly new space.
        //
        public int MaximumOverlapPercent = 25;

        public int PlacementAttempts = 600;


        private enum AttachDirection
        {
            North,
            South,
            West,
            East
        }


        private sealed class RoomBox
        {
            public int X1;
            public int Y1;
            public int X2;
            public int Y2;

            public bool Wide;


            public int Width
            {
                get
                {
                    return X2 - X1 + 1;
                }
            }


            public int Height
            {
                get
                {
                    return Y2 - Y1 + 1;
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
                    "SubterraneanSites:StaticLayout:" +
                    Z.ZoneID
                );


            System.Random rng =
                new System.Random(
                    seed
                );


            bool[,] carved =
                new bool[
                    Z.Width,
                    Z.Height
                ];


            List<RoomBox> rooms =
                new List<RoomBox>();


            List<Location2D> anchors =
                SubterraneanSites
                    .SubterraneanSitesEPVerticalTransitions
                    .GetVerticalAnchors(
                        Z.ZoneID
                    );


            Location2D start =
                null;


            if (
                anchors != null &&
                anchors.Count > 0
            )
            {
                start =
                    anchors[0];
            }


            if (start == null)
            {
                start =
                    Location2D.Get(
                        Z.Width / 2,
                        Z.Height / 2
                    );
            }


            //
            // Start with either orientation.
            //
            bool firstWide =
                rng.Next(2) == 0;


            RoomBox firstRoom =
                MakeCenteredRoom(
                    Z,
                    start.X,
                    start.Y,
                    RollWidth(
                        firstWide,
                        rng
                    ),
                    RollHeight(
                        firstWide,
                        rng
                    ),
                    firstWide
                );


            CarveRoom(
                Z,
                firstRoom,
                carved
            );


            rooms.Add(
                firstRoom
            );


            int desiredRooms =
                rng.Next(
                    MinRooms,
                    MaxRooms + 1
                );


            int attempts =
                0;


            while (
                rooms.Count < desiredRooms &&
                attempts < PlacementAttempts
            )
            {
                attempts++;


                //
                // Branch from any existing room rather than only extending
                // the newest room.
                //
                RoomBox parent =
                    rooms[
                        rng.Next(
                            rooms.Count
                        )
                    ];


                //
                // Alternate room proportions along each branch.
                //
                bool childWide =
                    !parent.Wide;


                //
                // Most connections retain one wall cell between rooms.
                // Some instead use a shallow direct overlap.
                //
                bool overlapMode =
                    rng.Next(100) <
                    OverlapAttachmentChancePercent;


                AttachDirection direction;


                RoomBox candidate =
                    CreateAttachedCandidate(
                        Z,
                        parent,
                        childWide,
                        overlapMode,
                        rng,
                        out direction
                    );


                if (
                    candidate == null ||
                    !CanAddRoom(
                        candidate,
                        carved,
                        overlapMode
                    )
                )
                {
                    continue;
                }


                CarveRoom(
                    Z,
                    candidate,
                    carved
                );


                //
                // Separated rooms deliberately retain a one-cell wall.
                // Open only a small doorway through that separator.
                //
                if (!overlapMode)
                {
                    CarveDoorway(
                        Z,
                        parent,
                        candidate,
                        direction,
                        carved,
                        rng
                    );
                }


                rooms.Add(
                    candidate
                );
            }


            //
            // Random branching is not required to happen to reach every
            // planned vertical transition.
            //
            // If an anchor is still outside the room complex, connect it
            // using a short chain of alternating rectangular spaces rather
            // than cutting a conventional hallway.
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


                    EnsureAnchorConnected(
                        Z,
                        anchor,
                        carved
                    );
                }
            }


            Z.ClearReachableMap();

            return true;
        }


        private RoomBox CreateAttachedCandidate(
            Zone Z,
            RoomBox parent,
            bool childWide,
            bool overlapMode,
            System.Random rng,
            out AttachDirection direction
        )
        {
            direction =
                (AttachDirection)rng.Next(4);


            int width =
                RollWidth(
                    childWide,
                    rng
                );


            int height =
                RollHeight(
                    childWide,
                    rng
                );


            int x1 = 0;
            int y1 = 0;


            //
            // Keep a meaningful perpendicular overlap between the two
            // rectangles so a wall-separated pair can receive a doorway
            // and an overlap pair actually reads as connected rooms.
            //
            const int MinimumAttachmentSpan = 3;


            if (
                direction == AttachDirection.North ||
                direction == AttachDirection.South
            )
            {
                int minimumX1 =
                    parent.X1 -
                    width +
                    MinimumAttachmentSpan;


                int maximumX1 =
                    parent.X2 -
                    MinimumAttachmentSpan +
                    1;


                x1 =
                    NextInclusive(
                        rng,
                        minimumX1,
                        maximumX1
                    );


                if (overlapMode)
                {
                    int depth =
                        rng.Next(
                            1,
                            4
                        );


                    depth =
                        Math.Min(
                            depth,
                            Math.Min(
                                parent.Height,
                                height
                            )
                        );


                    if (
                        direction ==
                        AttachDirection.North
                    )
                    {
                        int y2 =
                            parent.Y1 +
                            depth -
                            1;


                        y1 =
                            y2 -
                            height +
                            1;
                    }
                    else
                    {
                        y1 =
                            parent.Y2 -
                            depth +
                            1;
                    }
                }
                else
                {
                    //
                    // Exactly one solid row remains between rooms.
                    //
                    if (
                        direction ==
                        AttachDirection.North
                    )
                    {
                        int y2 =
                            parent.Y1 -
                            2;


                        y1 =
                            y2 -
                            height +
                            1;
                    }
                    else
                    {
                        y1 =
                            parent.Y2 +
                            2;
                    }
                }
            }
            else
            {
                int minimumY1 =
                    parent.Y1 -
                    height +
                    MinimumAttachmentSpan;


                int maximumY1 =
                    parent.Y2 -
                    MinimumAttachmentSpan +
                    1;


                y1 =
                    NextInclusive(
                        rng,
                        minimumY1,
                        maximumY1
                    );


                if (overlapMode)
                {
                    int depth =
                        rng.Next(
                            1,
                            4
                        );


                    depth =
                        Math.Min(
                            depth,
                            Math.Min(
                                parent.Width,
                                width
                            )
                        );


                    if (
                        direction ==
                        AttachDirection.West
                    )
                    {
                        int x2 =
                            parent.X1 +
                            depth -
                            1;


                        x1 =
                            x2 -
                            width +
                            1;
                    }
                    else
                    {
                        x1 =
                            parent.X2 -
                            depth +
                            1;
                    }
                }
                else
                {
                    //
                    // Exactly one solid column remains between rooms.
                    //
                    if (
                        direction ==
                        AttachDirection.West
                    )
                    {
                        int x2 =
                            parent.X1 -
                            2;


                        x1 =
                            x2 -
                            width +
                            1;
                    }
                    else
                    {
                        x1 =
                            parent.X2 +
                            2;
                    }
                }
            }


            RoomBox result =
                new RoomBox
                {
                    X1 = x1,
                    Y1 = y1,
                    X2 = x1 + width - 1,
                    Y2 = y1 + height - 1,
                    Wide = childWide
                };


            if (
                result.X1 < BorderWidth ||
                result.Y1 < BorderWidth ||
                result.X2 >=
                    Z.Width -
                    BorderWidth ||
                result.Y2 >=
                    Z.Height -
                    BorderWidth
            )
            {
                return null;
            }


            return result;
        }


        private bool CanAddRoom(
            RoomBox room,
            bool[,] carved,
            bool overlapMode
        )
        {
            if (
                room == null ||
                carved == null
            )
            {
                return false;
            }


            int area =
                room.Width *
                room.Height;


            if (area <= 0)
                return false;


            int overlap =
                0;


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
                    if (carved[x, y])
                    {
                        overlap++;
                    }
                }
            }


            if (overlapMode)
            {
                //
                // This mode deliberately merges shallowly with existing
                // room space, so require some actual overlap.
                //
                if (overlap == 0)
                    return false;


                //
                // But most of the candidate must still be new space.
                //
                if (
                    overlap * 100 >
                    area *
                    MaximumOverlapPercent
                )
                {
                    return false;
                }
            }
            else
            {
                //
                // A wall-separated attachment should not erase an
                // already-existing room elsewhere in the complex.
                //
                if (overlap != 0)
                    return false;
            }


            return true;
        }


        private void CarveDoorway(
            Zone Z,
            RoomBox parent,
            RoomBox child,
            AttachDirection direction,
            bool[,] carved,
            System.Random rng
        )
        {
            if (
                Z == null ||
                parent == null ||
                child == null ||
                carved == null ||
                rng == null
            )
            {
                return;
            }


            if (
                direction == AttachDirection.North ||
                direction == AttachDirection.South
            )
            {
                int start =
                    Math.Max(
                        parent.X1,
                        child.X1
                    );


                int end =
                    Math.Min(
                        parent.X2,
                        child.X2
                    );


                if (end < start)
                    return;


                int span =
                    end -
                    start +
                    1;


                int doorwayWidth =
                    Math.Min(
                        span,
                        rng.Next(
                            1,
                            3
                        )
                    );


                int doorwayStart =
                    NextInclusive(
                        rng,
                        start,
                        end -
                        doorwayWidth +
                        1
                    );


                int wallY =
                    direction ==
                    AttachDirection.North
                        ? parent.Y1 - 1
                        : parent.Y2 + 1;


                for (
                    int i = 0;
                    i < doorwayWidth;
                    i++
                )
                {
                    CarveCell(
                        Z,
                        doorwayStart + i,
                        wallY,
                        carved
                    );
                }
            }
            else
            {
                int start =
                    Math.Max(
                        parent.Y1,
                        child.Y1
                    );


                int end =
                    Math.Min(
                        parent.Y2,
                        child.Y2
                    );


                if (end < start)
                    return;


                int span =
                    end -
                    start +
                    1;


                int doorwayWidth =
                    Math.Min(
                        span,
                        rng.Next(
                            1,
                            3
                        )
                    );


                int doorwayStart =
                    NextInclusive(
                        rng,
                        start,
                        end -
                        doorwayWidth +
                        1
                    );


                int wallX =
                    direction ==
                    AttachDirection.West
                        ? parent.X1 - 1
                        : parent.X2 + 1;


                for (
                    int i = 0;
                    i < doorwayWidth;
                    i++
                )
                {
                    CarveCell(
                        Z,
                        wallX,
                        doorwayStart + i,
                        carved
                    );
                }
            }
        }


        private void EnsureAnchorConnected(
            Zone Z,
            Location2D anchor,
            bool[,] carved
        )
        {
            if (
                Z == null ||
                anchor == null ||
                carved == null
            )
            {
                return;
            }


            if (
                anchor.X >= 0 &&
                anchor.Y >= 0 &&
                anchor.X < Z.Width &&
                anchor.Y < Z.Height &&
                carved[
                    anchor.X,
                    anchor.Y
                ]
            )
            {
                return;
            }


            Location2D nearest =
                FindNearestCarvedCell(
                    Z,
                    anchor,
                    carved
                );


            if (nearest == null)
            {
                RoomBox anchorRoom =
                    MakeCenteredRoom(
                        Z,
                        anchor.X,
                        anchor.Y,
                        7,
                        5,
                        true
                    );


                CarveRoom(
                    Z,
                    anchorRoom,
                    carved
                );

                return;
            }


            int currentX =
                nearest.X;

            int currentY =
                nearest.Y;


            int safety =
                0;


            bool wide =
                Math.Abs(
                    anchor.X -
                    currentX
                ) >=
                Math.Abs(
                    anchor.Y -
                    currentY
                );


            while (
                (
                    Math.Abs(
                        anchor.X -
                        currentX
                    ) > 2 ||
                    Math.Abs(
                        anchor.Y -
                        currentY
                    ) > 2
                ) &&
                safety < 30
            )
            {
                safety++;


                int dx =
                    anchor.X -
                    currentX;


                int dy =
                    anchor.Y -
                    currentY;


                if (wide)
                {
                    currentX +=
                        ClampStep(
                            dx,
                            4
                        );


                    currentY +=
                        ClampStep(
                            dy,
                            2
                        );
                }
                else
                {
                    currentX +=
                        ClampStep(
                            dx,
                            2
                        );


                    currentY +=
                        ClampStep(
                            dy,
                            4
                        );
                }


                RoomBox connector =
                    MakeCenteredRoom(
                        Z,
                        currentX,
                        currentY,
                        wide ? 7 : 5,
                        wide ? 4 : 7,
                        wide
                    );


                CarveRoom(
                    Z,
                    connector,
                    carved
                );


                wide =
                    !wide;
            }


            RoomBox finalRoom =
                MakeCenteredRoom(
                    Z,
                    anchor.X,
                    anchor.Y,
                    7,
                    5,
                    true
                );


            CarveRoom(
                Z,
                finalRoom,
                carved
            );
        }


        private Location2D FindNearestCarvedCell(
            Zone Z,
            Location2D target,
            bool[,] carved
        )
        {
            Location2D best =
                null;


            int bestDistance =
                int.MaxValue;


            for (
                int x = BorderWidth;
                x <
                    Z.Width -
                    BorderWidth;
                x++
            )
            {
                for (
                    int y = BorderWidth;
                    y <
                        Z.Height -
                        BorderWidth;
                    y++
                )
                {
                    if (!carved[x, y])
                        continue;


                    int distance =
                        Math.Abs(
                            target.X -
                            x
                        ) +
                        Math.Abs(
                            target.Y -
                            y
                        );


                    if (
                        distance <
                        bestDistance
                    )
                    {
                        bestDistance =
                            distance;


                        best =
                            Location2D.Get(
                                x,
                                y
                            );
                    }
                }
            }


            return best;
        }


        private int RollWidth(
            bool wide,
            System.Random rng
        )
        {
            return
                wide
                    ? rng.Next(
                        WideMinWidth,
                        WideMaxWidth + 1
                    )
                    : rng.Next(
                        TallMinWidth,
                        TallMaxWidth + 1
                    );
        }


        private int RollHeight(
            bool wide,
            System.Random rng
        )
        {
            return
                wide
                    ? rng.Next(
                        WideMinHeight,
                        WideMaxHeight + 1
                    )
                    : rng.Next(
                        TallMinHeight,
                        TallMaxHeight + 1
                    );
        }


        private RoomBox MakeCenteredRoom(
            Zone Z,
            int centerX,
            int centerY,
            int width,
            int height,
            bool wide
        )
        {
            width =
                Math.Max(
                    3,
                    Math.Min(
                        width,
                        Z.Width -
                        BorderWidth * 2
                    )
                );


            height =
                Math.Max(
                    3,
                    Math.Min(
                        height,
                        Z.Height -
                        BorderWidth * 2
                    )
                );


            int x1 =
                centerX -
                width / 2;


            int y1 =
                centerY -
                height / 2;


            int maximumX1 =
                Z.Width -
                BorderWidth -
                width;


            int maximumY1 =
                Z.Height -
                BorderWidth -
                height;


            x1 =
                Math.Max(
                    BorderWidth,
                    Math.Min(
                        maximumX1,
                        x1
                    )
                );


            y1 =
                Math.Max(
                    BorderWidth,
                    Math.Min(
                        maximumY1,
                        y1
                    )
                );


            return
                new RoomBox
                {
                    X1 = x1,
                    Y1 = y1,
                    X2 = x1 + width - 1,
                    Y2 = y1 + height - 1,
                    Wide = wide
                };
        }


        private void CarveRoom(
            Zone Z,
            RoomBox room,
            bool[,] carved
        )
        {
            if (
                Z == null ||
                room == null ||
                carved == null
            )
            {
                return;
            }


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
                        y,
                        carved
                    );
                }
            }
        }


        private void CarveCell(
            Zone Z,
            int x,
            int y,
            bool[,] carved
        )
        {
            if (
                Z == null ||
                carved == null ||
                x < BorderWidth ||
                y < BorderWidth ||
                x >=
                    Z.Width -
                    BorderWidth ||
                y >=
                    Z.Height -
                    BorderWidth
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


            carved[
                x,
                y
            ] = true;
        }


        private int ClampStep(
            int difference,
            int maximum
        )
        {
            if (difference > maximum)
                return maximum;


            if (difference < -maximum)
                return -maximum;


            return difference;
        }


        private int NextInclusive(
            System.Random rng,
            int minimum,
            int maximum
        )
        {
            if (maximum < minimum)
            {
                int swap =
                    minimum;


                minimum =
                    maximum;


                maximum =
                    swap;
            }


            return
                rng.Next(
                    minimum,
                    maximum + 1
                );
        }


        private void ClampSettings()
        {
            if (MinRooms < 1)
                MinRooms = 1;


            if (MaxRooms < MinRooms)
                MaxRooms = MinRooms;


            if (WideMinWidth < 3)
                WideMinWidth = 3;


            if (WideMaxWidth < WideMinWidth)
                WideMaxWidth = WideMinWidth;


            if (WideMinHeight < 3)
                WideMinHeight = 3;


            if (WideMaxHeight < WideMinHeight)
                WideMaxHeight = WideMinHeight;


            if (TallMinWidth < 3)
                TallMinWidth = 3;


            if (TallMaxWidth < TallMinWidth)
                TallMaxWidth = TallMinWidth;


            if (TallMinHeight < 3)
                TallMinHeight = 3;


            if (TallMaxHeight < TallMinHeight)
                TallMaxHeight = TallMinHeight;


            if (BorderWidth < 1)
                BorderWidth = 1;


            if (OverlapAttachmentChancePercent < 0)
                OverlapAttachmentChancePercent = 0;


            if (OverlapAttachmentChancePercent > 100)
                OverlapAttachmentChancePercent = 100;


            if (MaximumOverlapPercent < 1)
                MaximumOverlapPercent = 1;


            if (MaximumOverlapPercent > 50)
                MaximumOverlapPercent = 50;


            if (PlacementAttempts < 1)
                PlacementAttempts = 1;
        }
    }

    /// <summary>
    /// Static Category-3 materialization.
    ///
    /// First pass deliberately uses only two closely related wall types.
    /// Later Static C3 can gain a larger material vocabulary and runtime
    /// material shifting without changing C4 topology behavior.
    /// </summary>
    public class SubterraneanSitesStaticMaterials :
        ZoneBuilderSandbox
    {
        public int InitialWallTypeCount =
            2;

        public int MinimumGroupSize =
            2;

        public int MaximumGroupSize =
            5;


        public bool BuildZone(
            Zone Z
        )
        {
            if (Z == null)
                return true;


            int seed =
                XRLCore.Core.Game.GetWorldSeed(
                    "SubterraneanSites:StaticMaterials:" +
                    Z.ZoneID
                );


            System.Random rng =
                new System.Random(
                    seed
                );


            List<string> initialPalette =
                SubterraneanSites
                    .SubterraneanSitesEPStaticTheme
                    .PickInitialWallSubset(
                        rng,
                        InitialWallTypeCount
                    );

            string initialWallA =
                initialPalette[0];

            string initialWallB =
                initialPalette.Count > 1
                    ? initialPalette[1]
                    : initialPalette[0];


            The.ZoneManager.SetZoneProperty(
                Z.ZoneID,
                SubterraneanSites
                    .SubterraneanSitesEPStaticTheme
                    .StaticC3InitialWallAProperty,
                initialWallA
            );


            The.ZoneManager.SetZoneProperty(
                Z.ZoneID,
                SubterraneanSites
                    .SubterraneanSitesEPStaticTheme
                    .StaticC3InitialWallBProperty,
                initialWallB
            );


            if (initialPalette.Count == 0)
            {
                initialPalette.Add(
                    "Fulcrete"
                );
            }


            //
            // Snapshot every placeholder before materializing any of them.
            //
            // The bool map lets us grow small contiguous material patches
            // without depending on placeholders still existing after a
            // neighboring cell has already been converted.
            //
            bool[,] eligible =
                new bool[
                    Z.Width,
                    Z.Height
                ];


            bool[,] assigned =
                new bool[
                    Z.Width,
                    Z.Height
                ];


            List<Cell> remaining =
                new List<Cell>();


            foreach (
                Cell cell
                in Z.GetCells()
            )
            {
                if (
                    cell == null ||
                    !SubterraneanSites
                        .SubterraneanSitesEPGeometry
                        .IsAnyPlaceholder(
                            cell
                        )
                )
                {
                    continue;
                }


                eligible[
                    cell.X,
                    cell.Y
                ] = true;


                remaining.Add(
                    cell
                );
            }


            while (
                remaining.Count > 0
            )
            {
                int seedIndex =
                    rng.Next(
                        remaining.Count
                    );


                Cell groupSeed =
                    remaining[
                        seedIndex
                    ];


                remaining.RemoveAt(
                    seedIndex
                );


                if (
                    groupSeed == null ||
                    assigned[
                        groupSeed.X,
                        groupSeed.Y
                    ]
                )
                {
                    continue;
                }


                string wallBlueprint =
                    initialPalette[
                        rng.Next(
                            initialPalette.Count
                        )
                    ];


                int desiredGroupSize =
                    rng.Next(
                        MinimumGroupSize,
                        MaximumGroupSize + 1
                    );


                List<Cell> frontier =
                    new List<Cell>();


                frontier.Add(
                    groupSeed
                );


                int groupSize =
                    0;


                while (
                    frontier.Count > 0 &&
                    groupSize < desiredGroupSize
                )
                {
                    int frontierIndex =
                        rng.Next(
                            frontier.Count
                        );


                    Cell cell =
                        frontier[
                            frontierIndex
                        ];


                    frontier.RemoveAt(
                        frontierIndex
                    );


                    if (
                        cell == null ||
                        !eligible[
                            cell.X,
                            cell.Y
                        ] ||
                        assigned[
                            cell.X,
                            cell.Y
                        ]
                    )
                    {
                        continue;
                    }


                    assigned[
                        cell.X,
                        cell.Y
                    ] = true;


                    RemoveFromRemaining(
                        remaining,
                        cell
                    );


                    PlaceWall(
                        cell,
                        wallBlueprint
                    );


                    groupSize++;


                    //
                    // Grow only cardinally.
                    //
                    // This makes each material form small connected
                    // sequences/blobs rather than diagonal confetti.
                    //
                    AddFrontierCell(
                        Z,
                        cell.X - 1,
                        cell.Y,
                        eligible,
                        assigned,
                        frontier
                    );


                    AddFrontierCell(
                        Z,
                        cell.X + 1,
                        cell.Y,
                        eligible,
                        assigned,
                        frontier
                    );


                    AddFrontierCell(
                        Z,
                        cell.X,
                        cell.Y - 1,
                        eligible,
                        assigned,
                        frontier
                    );


                    AddFrontierCell(
                        Z,
                        cell.X,
                        cell.Y + 1,
                        eligible,
                        assigned,
                        frontier
                    );
                }
            }


            return true;
        }


        private void PlaceWall(
            Cell cell,
            string wallBlueprint
        )
        {
            if (
                cell == null ||
                wallBlueprint.IsNullOrEmpty()
            )
            {
                return;
            }


            GameObject wall =
                GameObjectFactory.Factory
                    .CreateObject(
                        wallBlueprint
                    );


            if (wall == null)
                return;


            wall.SetIntProperty(
                SubterraneanSites
                    .SubterraneanSitesEPStaticTheme
                    .StaticC3WallProperty,
                1
            );


            cell.ClearWalls();


            cell.AddObject(
                wall
            );
        }


        private void AddFrontierCell(
            Zone Z,
            int x,
            int y,
            bool[,] eligible,
            bool[,] assigned,
            List<Cell> frontier
        )
        {
            if (
                Z == null ||
                eligible == null ||
                assigned == null ||
                frontier == null ||
                x < 0 ||
                y < 0 ||
                x >= Z.Width ||
                y >= Z.Height ||
                !eligible[x, y] ||
                assigned[x, y]
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
                cell != null &&
                !frontier.Contains(
                    cell
                )
            )
            {
                frontier.Add(
                    cell
                );
            }
        }


        private void RemoveFromRemaining(
            List<Cell> remaining,
            Cell cell
        )
        {
            if (
                remaining == null ||
                cell == null
            )
            {
                return;
            }


            for (
                int i =
                    remaining.Count - 1;
                i >= 0;
                i--
            )
            {
                if (
                    ReferenceEquals(
                        remaining[i],
                        cell
                    )
                )
                {
                    remaining.RemoveAt(
                        i
                    );

                    return;
                }
            }
        }
    }
    
    /// <summary>
    /// Static Category-4 floor adapter.
    ///
    /// Underground:
    ///     applies Static's selected C4 substrate zone-wide.
    ///
    /// Entrance:
    ///     applies the same substrate only inside the dimensional scar.
    /// </summary>
    public class SubterraneanSitesStaticFloor :
        ZoneBuilderSandbox
    {
        public int EntranceOnly =
            0;

        public int InitialFloorTypeCount =
            2;

        //
        // Floor patches are larger than wall-material runs because
        // they occupy a 2D field rather than a one-cell-wide boundary.
        //
        public int MinimumGroupSize =
            8;

        public int MaximumGroupSize =
            20;


        public bool BuildZone(
            Zone Z
        )
        {
            if (
                Z == null ||
                Options.DisableFloorTextureObjects
            )
            {
                return true;
            }


            int seed =
                XRLCore.Core.Game.GetWorldSeed(
                    "SubterraneanSites:StaticFloor:" +
                    Z.ZoneID +
                    ":" +
                    EntranceOnly.ToString()
                );


            System.Random rng =
                new System.Random(
                    seed
                );


            List<string> initialPalette =
                SubterraneanSites
                    .SubterraneanSitesEPStaticTheme
                    .PickInitialFloorSubset(
                        rng,
                        InitialFloorTypeCount
                    );


            if (initialPalette.Count == 0)
            {
                initialPalette.Add(
                    "Rocky"
                );
            }


            string initialFloorA =
                initialPalette[0];


            string initialFloorB =
                initialPalette.Count > 1
                    ? initialPalette[1]
                    : initialPalette[0];


            The.ZoneManager.SetZoneProperty(
                Z.ZoneID,
                SubterraneanSites
                    .SubterraneanSitesEPStaticTheme
                    .StaticC4InitialFloorAProperty,
                initialFloorA
            );


            The.ZoneManager.SetZoneProperty(
                Z.ZoneID,
                SubterraneanSites
                    .SubterraneanSitesEPStaticTheme
                    .StaticC4InitialFloorBProperty,
                initialFloorB
            );


            bool[,] eligible =
                new bool[
                    Z.Width,
                    Z.Height
                ];


            bool[,] assigned =
                new bool[
                    Z.Width,
                    Z.Height
                ];


            List<Cell> remaining =
                new List<Cell>();


            foreach (
                Cell cell
                in Z.GetCells()
            )
            {
                if (cell == null)
                    continue;


                //
                // Underground Static C4 paints every cell, including cells
                // currently occupied by walls. The floor remains substrate
                // underneath them and is revealed if topology later moves.
                //
                if (
                    EntranceOnly != 0 &&
                    !SubterraneanSites
                        .SubterraneanSitesEPEntrance
                        .IsInsideScar(
                            Z,
                            cell
                        )
                )
                {
                    continue;
                }


                eligible[
                    cell.X,
                    cell.Y
                ] = true;


                remaining.Add(
                    cell
                );
            }


            while (
                remaining.Count > 0
            )
            {
                int seedIndex =
                    rng.Next(
                        remaining.Count
                    );


                Cell groupSeed =
                    remaining[
                        seedIndex
                    ];


                remaining.RemoveAt(
                    seedIndex
                );


                if (
                    groupSeed == null ||
                    assigned[
                        groupSeed.X,
                        groupSeed.Y
                    ]
                )
                {
                    continue;
                }


                string painter =
                    initialPalette[
                        rng.Next(
                            initialPalette.Count
                        )
                    ];


                int desiredGroupSize =
                    rng.Next(
                        MinimumGroupSize,
                        MaximumGroupSize + 1
                    );


                List<Cell> frontier =
                    new List<Cell>();


                frontier.Add(
                    groupSeed
                );


                int groupSize =
                    0;


                while (
                    frontier.Count > 0 &&
                    groupSize < desiredGroupSize
                )
                {
                    int frontierIndex =
                        rng.Next(
                            frontier.Count
                        );


                    Cell cell =
                        frontier[
                            frontierIndex
                        ];


                    frontier.RemoveAt(
                        frontierIndex
                    );


                    if (
                        cell == null ||
                        !eligible[
                            cell.X,
                            cell.Y
                        ] ||
                        assigned[
                            cell.X,
                            cell.Y
                        ]
                    )
                    {
                        continue;
                    }


                    assigned[
                        cell.X,
                        cell.Y
                    ] = true;


                    RemoveFromRemaining(
                        remaining,
                        cell
                    );


                    SubterraneanSites
                        .SubterraneanSitesEPStaticTheme
                        .ApplyFloorPainter(
                            cell,
                            painter
                        );


                    groupSize++;


                    AddFrontierCell(
                        Z,
                        cell.X - 1,
                        cell.Y,
                        eligible,
                        assigned,
                        frontier
                    );


                    AddFrontierCell(
                        Z,
                        cell.X + 1,
                        cell.Y,
                        eligible,
                        assigned,
                        frontier
                    );


                    AddFrontierCell(
                        Z,
                        cell.X,
                        cell.Y - 1,
                        eligible,
                        assigned,
                        frontier
                    );


                    AddFrontierCell(
                        Z,
                        cell.X,
                        cell.Y + 1,
                        eligible,
                        assigned,
                        frontier
                    );
                }
            }


            return true;
        }


        private void AddFrontierCell(
            Zone Z,
            int x,
            int y,
            bool[,] eligible,
            bool[,] assigned,
            List<Cell> frontier
        )
        {
            if (
                Z == null ||
                eligible == null ||
                assigned == null ||
                frontier == null ||
                x < 0 ||
                y < 0 ||
                x >= Z.Width ||
                y >= Z.Height ||
                !eligible[x, y] ||
                assigned[x, y]
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
                cell != null &&
                !frontier.Contains(
                    cell
                )
            )
            {
                frontier.Add(
                    cell
                );
            }
        }


        private void RemoveFromRemaining(
            List<Cell> remaining,
            Cell cell
        )
        {
            if (
                remaining == null ||
                cell == null
            )
            {
                return;
            }


            for (
                int i =
                    remaining.Count - 1;
                i >= 0;
                i--
            )
            {
                if (
                    ReferenceEquals(
                        remaining[i],
                        cell
                    )
                )
                {
                    remaining.RemoveAt(
                        i
                    );

                    return;
                }
            }
        }
    }
    
   
    public class SubterraneanSitesStaticDecorations :
        ZoneBuilderSandbox
    {
        public int EntranceOnly =
            0;


        public int MinClusters =
            6;

        public int MaxClusters =
            9;


        public int MinObjectsPerCluster =
            2;

        public int MaxObjectsPerCluster =
            5;


        public int ClusterRadius =
            2;


        public int MinScatter =
            10;

        public int MaxScatter =
            18;


        public int TransitionExclusionRadius =
            4;


        public bool BuildZone(
            Zone Z
        )
        {
            if (Z == null)
                return true;

            //
            // The entrance scar is deliberately much lighter than
            // a full underground Static-C5 layer.
            //
            if (EntranceOnly != 0)
            {
                MinClusters =
                    3;

                MaxClusters =
                    7;

                MinObjectsPerCluster =
                    2;

                MaxObjectsPerCluster =
                    3;

                MinScatter =
                    3;

                MaxScatter =
                    5;
            }


            ClampSettings();


            int seed =
                XRLCore.Core.Game.GetWorldSeed(
                    "SubterraneanSites:StaticDecorations:" +
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


            PlaceFurnitureClusters(
                Z,
                anchors,
                rng
            );


            PlaceScatter(
                Z,
                anchors,
                rng
            );


            return true;
        }


        private void PlaceFurnitureClusters(
            Zone Z,
            List<Location2D> anchors,
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
                List<Cell> centers =
                    CollectAvailableCells(
                        Z,
                        anchors,
                        requireBroadOpen: true
                    );


                if (centers.Count == 0)
                    return;


                Cell center =
                    centers[
                        rng.Next(
                            centers.Count
                        )
                    ];


                string family =
                    SubterraneanSites
                        .SubterraneanSitesEPStaticDecorationCatalog
                        .RollClusterFamily(
                            rng
                        );


                int objectCount =
                    rng.Next(
                        MinObjectsPerCluster,
                        MaxObjectsPerCluster + 1
                    );


                for (
                    int objectIndex = 0;
                    objectIndex < objectCount;
                    objectIndex++
                )
                {
                    Cell target;


                    if (objectIndex == 0)
                    {
                        target =
                            center;
                    }
                    else
                    {
                        target =
                            FindClusterCellNear(
                                Z,
                                center,
                                anchors,
                                rng
                            );
                    }


                    if (target == null)
                        continue;


                    string blueprint;


                    //
                    // The first object establishes the cluster identity.
                    //
                    // After that, most objects respect that family,
                    // while a minority are already anomalous.
                    //
                    if (
                        objectIndex == 0 ||
                        rng.Next(
                            1,
                            101
                        ) <= 70
                    )
                    {
                        blueprint =
                            SubterraneanSites
                                .SubterraneanSitesEPStaticDecorationCatalog
                                .PickBlueprintForFamily(
                                    family,
                                    rng
                                );
                    }
                    else
                    {
                        blueprint =
                            SubterraneanSites
                                .SubterraneanSitesEPStaticDecorationCatalog
                                .PickAnyFurnitureBlueprint(
                                    rng
                                );
                    }


                    PlaceDecoration(
                        Z,
                        target,
                        blueprint,
                        family
                    );
                }
            }
        }


        private void PlaceScatter(
            Zone Z,
            List<Location2D> anchors,
            System.Random rng
        )
        {
            int desired =
                rng.Next(
                    MinScatter,
                    MaxScatter + 1
                );


            for (
                int i = 0;
                i < desired;
                i++
            )
            {
                List<Cell> candidates =
                    CollectAvailableCells(
                        Z,
                        anchors,
                        requireBroadOpen: false
                    );


                if (candidates.Count == 0)
                    return;


                Cell target =
                    candidates[
                        rng.Next(
                            candidates.Count
                        )
                    ];


                string blueprint =
                    SubterraneanSites
                        .SubterraneanSitesEPStaticDecorationCatalog
                        .PickScatterBlueprint(
                            rng
                        );


                PlaceDecoration(
                    Z,
                    target,
                    blueprint,
                    SubterraneanSites
                        .SubterraneanSitesEPStaticDecorationCatalog
                        .ScatterFamily
                );
            }
        }


        private List<Cell> CollectAvailableCells(
            Zone Z,
            List<Location2D> anchors,
            bool requireBroadOpen
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
                    CellIsAvailable(
                        Z,
                        cell,
                        anchors,
                        requireBroadOpen
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


        private Cell FindClusterCellNear(
            Zone Z,
            Cell center,
            List<Location2D> anchors,
            System.Random rng
        )
        {
            if (
                Z == null ||
                center == null
            )
            {
                return null;
            }


            List<Cell> candidates =
                new List<Cell>();


            for (
                int dx = -ClusterRadius;
                dx <= ClusterRadius;
                dx++
            )
            {
                for (
                    int dy = -ClusterRadius;
                    dy <= ClusterRadius;
                    dy++
                )
                {
                    Cell cell =
                        Z.GetCell(
                            center.X + dx,
                            center.Y + dy
                        );


                    if (
                        CellIsAvailable(
                            Z,
                            cell,
                            anchors,
                            requireBroadOpen: true
                        )
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


        private bool CellIsAvailable(
            Zone Z,
            Cell cell,
            List<Location2D> anchors,
            bool requireBroadOpen
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
                ) ||
                cell.HasObjectWithBlueprint(
                    SubterraneanSites
                        .SubterraneanSitesEPAttunementSystem
                        .StoneLeftBlueprint
                ) ||
                cell.HasObjectWithBlueprint(
                    SubterraneanSites
                        .SubterraneanSitesEPAttunementSystem
                        .StoneRightBlueprint
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
                requireBroadOpen &&
                !SubterraneanSites
                    .SubterraneanSitesEPPlacement
                    .HasBroadOpenClearance(
                        Z,
                        cell,
                        delegate(Cell nearby)
                        {
                            return
                                NeighborhoodCellIsOpenForCluster(
                                    Z,
                                    nearby,
                                    anchors
                                );
                        },
                        1
                    )
            )
            {
                return false;
            }


            return true;
        }


        private bool NeighborhoodCellIsOpenForCluster(
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
                !CellIsInsideDecorationScope(
                    Z,
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
                ) ||
                cell.HasObjectWithBlueprint(
                    SubterraneanSites
                        .SubterraneanSitesEPAttunementSystem
                        .StoneLeftBlueprint
                ) ||
                cell.HasObjectWithBlueprint(
                    SubterraneanSites
                        .SubterraneanSitesEPAttunementSystem
                        .StoneRightBlueprint
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
                if (obj == null)
                    continue;


                //
                // Furniture already placed by this same Static cluster
                // may occupy the neighborhood. That is how clusters are
                // allowed to actually cluster.
                //
                if (
                    obj.GetIntProperty(
                        SubterraneanSites
                            .SubterraneanSitesEPStaticDecorationCatalog
                            .DecorationMarkerProperty
                    ) >
                    0
                )
                {
                    continue;
                }


                if (
                    obj.IsWall() ||
                    obj.IsCombatObject() ||
                    obj.IsSpawnBlocker() ||
                    obj.Inventory != null ||
                    obj.HasTagOrProperty(
                        "Furniture"
                    ) ||
                    (
                        obj.Physics != null &&
                        obj.Physics.Solid
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


        private void PlaceDecoration(
            Zone Z,
            Cell cell,
            string blueprint,
            string originalFamily
        )
        {
            if (
                Z == null ||
                cell == null ||
                blueprint.IsNullOrEmpty()
            )
            {
                return;
            }


            GameObject obj =
                SubterraneanSites
                    .SubterraneanSitesEPStaticDecorationCatalog
                    .CreateMarkedDecoration(
                        blueprint,
                        originalFamily
                    );


            if (obj == null)
                return;


            cell.AddObject(
                obj
            );


            //
            // Run the emptying pass again after placement.
            //
            // This is intentionally redundant: if any vanilla initialization
            // populated a container during placement, Static still leaves it empty.
            //
            SubterraneanSites
                .SubterraneanSitesEPStaticDecorationCatalog
                .EmptyGeneratedContents(
                    obj
                );


            SubterraneanSites
                .SubterraneanSitesEPReservations
                .ClaimCell(
                    Z,
                    cell
                );
        }


        private void ClampSettings()
        {
            if (MinClusters < 0)
                MinClusters = 0;

            if (MaxClusters < MinClusters)
                MaxClusters = MinClusters;


            if (MinObjectsPerCluster < 1)
                MinObjectsPerCluster = 1;

            if (
                MaxObjectsPerCluster <
                MinObjectsPerCluster
            )
            {
                MaxObjectsPerCluster =
                    MinObjectsPerCluster;
            }


            if (ClusterRadius < 1)
                ClusterRadius = 1;


            if (MinScatter < 0)
                MinScatter = 0;

            if (MaxScatter < MinScatter)
                MaxScatter = MinScatter;


            if (TransitionExclusionRadius < 0)
                TransitionExclusionRadius = 0;
        }
    }
    
    
}


namespace SubterraneanSites
{

    internal static class SubterraneanSitesEPStaticDecorationCatalog
    {
        internal const string DecorationMarkerProperty =
            "SubterraneanSites_EP_StaticC5Decoration";

        internal const string OriginalFamilyProperty =
            "SubterraneanSites_EP_StaticC5OriginalFamily";


        internal const string ScatterFamily =
            "Scatter";

        internal const string SeatingFamily =
            "Seating";

        internal const string SleepingFamily =
            "Sleeping";

        internal const string TablesFamily =
            "TablesStorage";

        internal const string VesselsFamily =
            "Vessels";

        internal const string LightingFamily =
            "Lighting";

        internal const string MiscFamily =
            "Misc";


        private static readonly string[] ScatterBlueprints =
        {
            "Garbage",
            "Bones",
            "Rubble"
        };


        private static readonly string[] SeatingBlueprints =
        {
            "Chair",
            "Floor Cushion",
            "Kline",
            "Bench",
            "Stool",
            "Throne",
            "Armchair",
            "Medical Chair",
            "Massage Chair",
            "Work Chair",
            "Ornate Chair",
            "Ornate Bench",
            "Scrapasan"
        };


        private static readonly string[] SleepingBlueprints =
        {
            "Bed",
            "Bedroll",
            "Hammock",
            "Crib",
            "Four-Poster Bed",
            "Medical Bed"
        };


        private static readonly string[] TablesBlueprints =
        {
            "Table",
            "Metal Table",
            "Workbench",
            "Metal Workbench",
            "Desk",
            "Sleek Table",
            "Low Table",
            "Octagonal Table",
            "Dresser",
            "Book Table",
            "Ornate Table",
            "Alchemist Table",
            "Bookshelf",
            "Woven Basket",
            "Multicabinet"
        };


        private static readonly string[] VesselBlueprints =
        {
            "Vase",
            "Vase Unhandled",
            "Empty Urn",
            "TombUrn"
        };


        private static readonly string[] LightingBlueprints =
        {
            "Brazier",
            "Tall Brazier",
            "Unlit Torchpost",
            "Techlight1",
            "Techlight2",
            "Techlight3",
            "Campfire",
            "Oven"
        };


        private static readonly string[] MiscBlueprints =
        {
            "Globe",
            "Orrery",
            "Clockthing",
            "Telescope",
            "Bust of K4K5",
            "Bust of Mehmet I",

            //
            // One logical entry despite four visual variants.
            //
            "@OrnatePottedPlant",

            "Chalkboard",
            "Display Breadboard",
            "Sewing Machine",
            "Mannequin",
            "Light Sculpture",
            "Brain Sculpture",
            "Unfinished Sculpture",
            "Painting",
            "Harp",
            "Rectangular Bells",
            "Hammered Dulcimer",
            "Anvil"
        };


        private static readonly string[] OrnatePottedPlants =
        {
            "Ornate Potted Plant 1",
            "Ornate Potted Plant 2",
            "Ornate Potted Plant 3",
            "Ornate Potted Plant 4"
        };


        internal static string RollClusterFamily(
            System.Random rng
        )
        {
            int roll =
                Next(
                    rng,
                    1,
                    100
                );


            //
            // Family weighting prevents categories with many blueprint
            // variants from dominating merely because their lists are longer.
            //
            // 20% seating
            // 12% sleeping
            // 23% tables/storage
            // 10% vessels
            // 10% lighting/hearth
            // 25% miscellaneous
            //
            if (roll <= 20)
                return SeatingFamily;

            if (roll <= 32)
                return SleepingFamily;

            if (roll <= 55)
                return TablesFamily;

            if (roll <= 65)
                return VesselsFamily;

            if (roll <= 75)
                return LightingFamily;


            return MiscFamily;
        }


        internal static string PickBlueprintForFamily(
            string family,
            System.Random rng
        )
        {
            string[] pool =
                GetPoolForFamily(
                    family
                );


            if (
                pool == null ||
                pool.Length == 0
            )
            {
                return "";
            }


            string blueprint =
                pool[
                    Next(
                        rng,
                        0,
                        pool.Length - 1
                    )
                ];


            if (
                string.Equals(
                    blueprint,
                    "@OrnatePottedPlant",
                    StringComparison.Ordinal
                )
            )
            {
                blueprint =
                    OrnatePottedPlants[
                        Next(
                            rng,
                            0,
                            OrnatePottedPlants.Length - 1
                        )
                    ];
            }


            return blueprint;
        }


        internal static string PickAnyFurnitureBlueprint(
            System.Random rng
        )
        {
            return
                PickBlueprintForFamily(
                    RollClusterFamily(
                        rng
                    ),
                    rng
                );
        }


        internal static GameObject CreateMarkedDecoration(
            string blueprint,
            string originalFamily
        )
        {
            if (blueprint.IsNullOrEmpty())
                return null;


            GameObject obj =
                null;


            try
            {
                obj =
                    GameObjectFactory.Factory
                        .CreateObject(
                            blueprint
                        );
            }
            catch
            {
                return null;
            }


            if (obj == null)
                return null;


            //
            // Static C5 is scenery, not a source of generated loot.
            //
            EmptyGeneratedContents(
                obj
            );


            obj.SetIntProperty(
                DecorationMarkerProperty,
                1
            );


            obj.SetStringProperty(
                OriginalFamilyProperty,
                originalFamily
            );


            return obj;
        }


        internal static void EmptyGeneratedContents(
            GameObject obj
        )
        {
            if (obj == null)
                return;


            //
            // Containers such as tables, dressers, cabinets, baskets,
            // and bookshelves may receive generated inventory.
            //
            if (obj.Inventory != null)
            {
                for (
                    int i =
                        obj.Inventory.Objects.Count - 1;
                    i >= 0;
                    i--
                )
                {
                    GameObject item =
                        obj.Inventory.Objects[i];


                    if (item == null)
                        continue;


                    obj.Inventory.RemoveObject(
                        item
                    );


                    item.Obliterate();
                }
            }


            //
            // Vessels may populate themselves with vanilla liquids.
            // Static C5 vessels always begin dry.
            //
            if (obj.LiquidVolume != null)
            {
                obj.LiquidVolume.Empty(
                    true
                );
            }
        }


        internal static string PickScatterBlueprint(
            System.Random rng
        )
        {
            if (
                ScatterBlueprints == null ||
                ScatterBlueprints.Length == 0
            )
            {
                return "";
            }


            return
                ScatterBlueprints[
                    Next(
                        rng,
                        0,
                        ScatterBlueprints.Length - 1
                    )
                ];
        }


        private static string[] GetPoolForFamily(
            string family
        )
        {
            if (
                string.Equals(
                    family,
                    SeatingFamily,
                    StringComparison.Ordinal
                )
            )
            {
                return SeatingBlueprints;
            }


            if (
                string.Equals(
                    family,
                    SleepingFamily,
                    StringComparison.Ordinal
                )
            )
            {
                return SleepingBlueprints;
            }


            if (
                string.Equals(
                    family,
                    TablesFamily,
                    StringComparison.Ordinal
                )
            )
            {
                return TablesBlueprints;
            }


            if (
                string.Equals(
                    family,
                    VesselsFamily,
                    StringComparison.Ordinal
                )
            )
            {
                return VesselBlueprints;
            }


            if (
                string.Equals(
                    family,
                    LightingFamily,
                    StringComparison.Ordinal
                )
            )
            {
                return LightingBlueprints;
            }


            if (
                string.Equals(
                    family,
                    ScatterFamily,
                    StringComparison.Ordinal
                )
            )
            {
                return ScatterBlueprints;
            }


            return MiscBlueprints;
        }


        private static int Next(
            System.Random rng,
            int minInclusive,
            int maxInclusive
        )
        {
            if (maxInclusive < minInclusive)
                maxInclusive = minInclusive;


            if (rng != null)
            {
                return
                    rng.Next(
                        minInclusive,
                        maxInclusive + 1
                    );
            }


            return
                Stat.Random(
                    minInclusive,
                    maxInclusive
                );
        }
    }

    [Serializable]
    public class SubterraneanSitesEPStaticDecorationSystem :
        IGameSystem
    {
        public const int MinimumInitialShiftDelay =
            0;

        public const int MaximumInitialShiftDelay =
            50;


        public const int MinimumShiftDelay =
            50;

        public const int MaximumShiftDelay =
            100;


        public const int MinimumReplacementsPerShift =
            6;

        public const int MaximumReplacementsPerShift =
            12;


        public int TurnsUntilShift =
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
            Synchronize(
                advanceTimer: false
            );

            return true;
        }


        public override bool HandleEvent(
            EndTurnEvent E
        )
        {
            Synchronize(
                advanceTimer: true
            );

            return true;
        }

        private void Synchronize(
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


            Zone Z =
                player.CurrentZone;


            string category5 =
                The.ZoneManager.GetZoneProperty(
                    Z.ZoneID,
                    SubterraneanSitesEPShuffledTheme
                        .Category5ThemeProperty
                ) as string;


            if (
                !string.Equals(
                    category5,
                    "Static",
                    StringComparison.Ordinal
                )
            )
            {
                ResetTimer();

                return;
            }


            string zoneID =
                Z.ZoneID;


            if (
                !string.Equals(
                    ScheduledZoneID,
                    zoneID,
                    StringComparison.Ordinal
                )
            )
            {
                ScheduledZoneID =
                    zoneID;


                ScheduleInitialShift();


                //
                // Zero means reality is already changing
                // when the player arrives.
                //
                if (TurnsUntilShift <= 0)
                {
                    ShiftDecorations(
                        Z
                    );


                    ScheduleNextShift();
                }


                return;
            }


            if (!advanceTimer)
                return;


            if (TurnsUntilShift > 0)
            {
                TurnsUntilShift--;
            }


            if (TurnsUntilShift > 0)
                return;


            ShiftDecorations(
                Z
            );


            ScheduleNextShift();
        }


       

        private void ScheduleInitialShift()
        {
            TurnsUntilShift =
                Stat.Random(
                    MinimumInitialShiftDelay,
                    MaximumInitialShiftDelay
                );
        }


        private void ScheduleNextShift()
        {
            TurnsUntilShift =
                Stat.Random(
                    MinimumShiftDelay,
                    MaximumShiftDelay
                );
        }


        private void ResetTimer()
        {
            TurnsUntilShift =
                0;

            ScheduledZoneID =
                "";
        }


        private void ShiftDecorations(
            Zone Z
        )
        {
            if (Z == null)
                return;


            List<GameObject> candidates =
                new List<GameObject>();


            foreach (
                Cell cell
                in Z.GetCells()
            )
            {
                if (cell == null)
                    continue;


                foreach (
                    GameObject obj
                    in cell.GetObjects()
                )
                {
                    if (
                        obj == null ||
                        obj.GetIntProperty(
                            SubterraneanSitesEPStaticDecorationCatalog
                                .DecorationMarkerProperty
                        ) <=
                        0
                    )
                    {
                        continue;
                    }


                    //
                    // Once the player puts something into a Static
                    // container, leave that object alone permanently
                    // unless it later becomes empty again.
                    //
                    if (
                        HasStoredContents(
                            obj
                        )
                    )
                    {
                        continue;
                    }


                    candidates.Add(
                        obj
                    );
                }
            }


            if (candidates.Count == 0)
                return;


            int desired =
                Stat.Random(
                    MinimumReplacementsPerShift,
                    MaximumReplacementsPerShift
                );


            desired =
                Math.Min(
                    desired,
                    candidates.Count
                );


            for (
                int i = 0;
                i < desired;
                i++
            )
            {
                GameObject oldObject =
                    TakeRandom(
                        candidates
                    );


                if (oldObject == null)
                    continue;


                ReplaceDecoration(
                    oldObject
                );
            }
        }


        private bool HasStoredContents(
            GameObject obj
        )
        {
            if (obj == null)
                return false;


            if (
                obj.Inventory != null &&
                obj.Inventory.Objects != null &&
                obj.Inventory.Objects.Count > 0
            )
            {
                return true;
            }


            //
            // Also respect liquids the player has put into a vessel.
            //
            if (
                obj.LiquidVolume != null &&
                obj.LiquidVolume.Volume > 0
            )
            {
                return true;
            }


            return false;
        }


        private void ReplaceDecoration(
            GameObject oldObject
        )
        {
            if (
                oldObject == null ||
                oldObject.CurrentCell == null
            )
            {
                return;
            }


            Cell cell =
                oldObject.CurrentCell;


            string originalFamily =
                oldObject.GetStringProperty(
                    SubterraneanSitesEPStaticDecorationCatalog
                        .OriginalFamilyProperty,
                    SubterraneanSitesEPStaticDecorationCatalog
                        .MiscFamily
                );


            string replacementBlueprint =
                PickDifferentBlueprint(
                    oldObject.Blueprint,
                    originalFamily
                );


            if (replacementBlueprint.IsNullOrEmpty())
                return;


            //
            // Construct the replacement before touching the old object.
            //
            GameObject replacement =
                SubterraneanSitesEPStaticDecorationCatalog
                    .CreateMarkedDecoration(
                        replacementBlueprint,
                        originalFamily
                    );


            if (replacement == null)
                return;


            try
            {
                cell.RemoveObject(
                    oldObject,
                    true,
                    true,
                    Repaint: false
                );


                cell.AddObject(
                    replacement
                );


                oldObject.Obliterate();
            }
            catch
            {
                //
                // Fail soft. If removal happened but placement failed,
                // restore the original object.
                //
                if (
                    oldObject.CurrentCell == null
                )
                {
                    cell.AddObject(
                        oldObject
                    );
                }


                if (
                    replacement.CurrentCell !=
                    cell
                )
                {
                    replacement.Obliterate();
                }
            }
        }


        private string PickDifferentBlueprint(
            string currentBlueprint,
            string originalFamily
        )
        {
            bool scatter =
                string.Equals(
                    originalFamily,
                    SubterraneanSitesEPStaticDecorationCatalog
                        .ScatterFamily,
                    StringComparison.Ordinal
                );


            for (
                int attempt = 0;
                attempt < 30;
                attempt++
            )
            {
                string selected;


                if (scatter)
                {
                    //
                    // Debris remains debris.
                    //
                    selected =
                        SubterraneanSitesEPStaticDecorationCatalog
                            .PickScatterBlueprint(
                                null
                            );
                }
                else if (
                    Stat.Random(
                        1,
                        100
                    ) <= 50
                )
                {
                    //
                    // Half of substitutions still remember what kind
                    // of cluster originally occupied this location.
                    //
                    selected =
                        SubterraneanSitesEPStaticDecorationCatalog
                            .PickBlueprintForFamily(
                                originalFamily,
                                null
                            );
                }
                else
                {
                    //
                    // The other half can become any clustered furniture.
                    //
                    selected =
                        SubterraneanSitesEPStaticDecorationCatalog
                            .PickAnyFurnitureBlueprint(
                                null
                            );
                }


                if (
                    !selected.IsNullOrEmpty() &&
                    !string.Equals(
                        selected,
                        currentBlueprint,
                        StringComparison.Ordinal
                    )
                )
                {
                    return selected;
                }
            }


            return "";
        }


        private T TakeRandom<T>(
            List<T> list
        )
        {
            if (
                list == null ||
                list.Count == 0
            )
            {
                return default(T);
            }


            int index =
                Stat.Random(
                    0,
                    list.Count - 1
                );


            T result =
                list[index];


            list.RemoveAt(
                index
            );


            return result;
        }
    }
    
    /// <summary>
    /// Static Category-3 runtime material instability.
    ///
    /// Whenever Static owns C3, structural walls created by Static C3
    /// periodically change into other members of the Static wall palette.
    ///
    /// This timer is completely independent from Static C4 topology
    /// shifting and later Static C5 decoration shifting.
    ///
    /// Attunement does NOT suppress this behavior.
    /// </summary>
    [Serializable]
    public class SubterraneanSitesEPStaticMaterialSystem :
        IGameSystem
    {

        public const int MinimumInitialShiftDelay =
            0;

        public const int MaximumInitialShiftDelay =
            50;

        public const int MinimumShiftDelay =
            50;

        public const int MaximumShiftDelay =
            100;


        //
        // Each material pulse changes a substantial but incomplete
        // fraction of the eligible structural walls.
        //
        // Leaving some walls unchanged makes each pulse visually readable
        // rather than simply replacing one complete random field with another.
        //
        public const int MinimumShiftPercent =
            20;

        public const int MaximumShiftPercent =
            30;


        public int TurnsUntilShift =
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
            Synchronize(
                advanceTimer: false
            );

            return true;
        }


        public override bool HandleEvent(
            EndTurnEvent E
        )
        {
            Synchronize(
                advanceTimer: true
            );

            return true;
        }


        private void Synchronize(
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


            Zone Z =
                player.CurrentZone;


            //
            // Do not transform ordinary world-zone walls around the
            // localized entrance scar.
            //
            // Static floor/decor instability may later explicitly operate
            // inside the scar, but C3 wall transformation belongs only to
            // the actual underground EP layers.
            //
            Location2D scarCenter;
            int scarRadius;
            int holeRadius;


            if (
                SubterraneanSitesEPEntrance
                    .TryGetSpec(
                        Z,
                        out scarCenter,
                        out scarRadius,
                        out holeRadius
                    )
            )
            {
                ResetTimer();

                return;
            }


            string category3 =
                The.ZoneManager.GetZoneProperty(
                    Z.ZoneID,
                    SubterraneanSitesEPShuffledTheme
                        .Category3ThemeProperty
                ) as string;


            if (
                !string.Equals(
                    category3,
                    "Static",
                    StringComparison.Ordinal
                )
            )
            {
                ResetTimer();

                return;
            }


            string zoneID =
                Z.ZoneID;


            if (
                !string.Equals(
                    ScheduledZoneID,
                    zoneID,
                    StringComparison.Ordinal
                )
            )
            {
                ScheduledZoneID =
                    zoneID;


                ScheduleInitialShift();


                if (TurnsUntilShift <= 0)
                {
                    ShiftMaterials(
                        Z
                    );


                    ScheduleNextShift();
                }


                return;
            }


            if (!advanceTimer)
                return;


            if (TurnsUntilShift > 0)
            {
                TurnsUntilShift--;
            }


            if (TurnsUntilShift > 0)
                return;


            ShiftMaterials(
                Z
            );


            ScheduleNextShift();
        }

        private void ScheduleInitialShift()
        {
            TurnsUntilShift =
                Stat.Random(
                    MinimumInitialShiftDelay,
                    MaximumInitialShiftDelay
                );
        }


        private void ScheduleNextShift()
        {
            TurnsUntilShift =
                Stat.Random(
                    MinimumShiftDelay,
                    MaximumShiftDelay
                );
        }


        private void ResetTimer()
        {
            TurnsUntilShift =
                0;

            ScheduledZoneID =
                "";
        }


        private void ShiftMaterials(
            Zone Z
        )
        {
            if (Z == null)
                return;


            //
            // Remember the two materials this layer originally started with.
            //
            string initialWallA =
                The.ZoneManager.GetZoneProperty(
                    Z.ZoneID,
                    SubterraneanSitesEPStaticTheme
                        .StaticC3InitialWallAProperty
                ) as string;


            string initialWallB =
                The.ZoneManager.GetZoneProperty(
                    Z.ZoneID,
                    SubterraneanSitesEPStaticTheme
                        .StaticC3InitialWallBProperty
                ) as string;


            List<Cell> candidates =
                new List<Cell>();


            foreach (
                Cell cell
                in Z.GetCells()
            )
            {
                if (
                    cell != null &&
                    HasStaticStructuralWall(
                        cell
                    )
                )
                {
                    candidates.Add(
                        cell
                    );
                }
            }


            if (candidates.Count == 0)
                return;


            int shiftPercent =
                Stat.Random(
                    MinimumShiftPercent,
                    MaximumShiftPercent
                );


            foreach (
                Cell cell
                in candidates
            )
            {
                if (
                    Stat.Random(
                        1,
                        100
                    ) >
                    shiftPercent
                )
                {
                    continue;
                }


                ReplaceStaticWall(
                    cell,
                    initialWallA,
                    initialWallB
                );
            }
        }


        private bool HasStaticStructuralWall(
            Cell cell
        )
        {
            if (cell == null)
                return false;


            GameObject markedWall =
                null;

            int wallCount =
                0;


            foreach (
                GameObject obj
                in cell.GetObjects()
            )
            {
                if (
                    obj == null ||
                    !obj.IsWall()
                )
                {
                    continue;
                }


                wallCount++;


                if (
                    obj.GetIntProperty(
                        SubterraneanSitesEPStaticTheme
                            .StaticC3WallProperty
                    ) >
                    0
                )
                {
                    markedWall =
                        obj;
                }
            }


            //
            // Be conservative if some later system somehow caused multiple
            // wall objects to share the same cell.
            //
            return
                wallCount == 1 &&
                markedWall != null;
        }

        private void ReplaceStaticWall(
            Cell cell,
            string initialWallA,
            string initialWallB
        )
        {
            if (cell == null)
                return;


            GameObject oldWall =
                null;


            foreach (
                GameObject obj
                in cell.GetObjects()
            )
            {
                if (
                    obj != null &&
                    obj.IsWall() &&
                    obj.GetIntProperty(
                        SubterraneanSitesEPStaticTheme
                            .StaticC3WallProperty
                    ) >
                    0
                )
                {
                    oldWall =
                        obj;

                    break;
                }
            }


            if (oldWall == null)
                return;


            string replacementBlueprint =
                SubterraneanSitesEPStaticTheme
                    .PickDifferentWallBlueprint(
                        oldWall.Blueprint,
                        initialWallA,
                        initialWallB
                    );


            if (replacementBlueprint.IsNullOrEmpty())
                return;


            GameObject replacement =
                GameObjectFactory.Factory
                    .CreateObject(
                        replacementBlueprint
                    );


            if (replacement == null)
                return;


            replacement.SetIntProperty(
                SubterraneanSitesEPStaticTheme
                    .StaticC3WallProperty,
                1
            );


            cell.ClearWalls();


            cell.AddObject(
                replacement
            );
        }


       
    }



    /// <summary>
    /// Runtime Static Category-4 topology mutation.
    ///
    /// This does not regenerate a dungeon.
    ///
    /// Instead, it redistributes existing wall objects:
    ///
    ///     existing wall touching open space -> becomes open
    ///     eligible open cell                -> receives that wall
    ///
    /// The number of walls remains approximately constant, while their
    /// positions drift over time.
    ///
    /// Static C4 shifts whenever Static owns Category 4.
    ///
    /// The ONLY suppression rule is:
    ///
    ///     Static owns C1 AND the player is Static-attuned.
    ///
    /// If another theme owns C1, Static C4 cannot be stabilized.
    /// </summary>
    [Serializable]
    public class SubterraneanSitesEPStaticTopologySystem :
        IGameSystem
    {

        public const int MinimumInitialShiftDelay =
            0;
        public const int MaximumInitialShiftDelay =
            50;
        public const int MinimumShiftDelay =
            50;

        public const int MaximumShiftDelay =
            100;

        public const int MinimumWallsPerShift =
            8;

        public const int MaximumWallsPerShift =
            14;

        public const int BorderWidth =
            2;

        public const int AnchorExclusionRadius =
            4;

        public const int PlacementProtectionRadius =
            1;


        public int TurnsUntilShift =
            0;

        public string ScheduledZoneID =
            "";

        public const int MinimumFloorShiftPercent =
            6;

        public const int MaximumFloorShiftPercent =
            13;


        private sealed class WallCandidate
        {
            public Cell Cell;
            public GameObject Wall;
        }


        private sealed class WallMove
        {
            public Cell Source;
            public Cell Destination;
            public GameObject Wall;
        }


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
            Synchronize(
                advanceTimer: false
            );

            return true;
        }


        public override bool HandleEvent(
            EndTurnEvent E
        )
        {
            Synchronize(
                advanceTimer: true
            );

            return true;
        }


        private void Synchronize(
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


            Zone Z =
                player.CurrentZone;


            //
            // Never mutate the entrance scar's surrounding ordinary zone.
            // Floor/decor shifting can later be explicitly scar-scoped.
            //
            Location2D scarCenter;
            int scarRadius;
            int holeRadius;


            if (
                SubterraneanSitesEPEntrance
                    .TryGetSpec(
                        Z,
                        out scarCenter,
                        out scarRadius,
                        out holeRadius
                    )
            )
            {
                ResetTimer();
                return;
            }


            string category4 =
                The.ZoneManager.GetZoneProperty(
                    Z.ZoneID,
                    SubterraneanSitesEPShuffledTheme
                        .Category4ThemeProperty
                ) as string;


            if (
                !string.Equals(
                    category4,
                    "Static",
                    StringComparison.Ordinal
                )
            )
            {
                ResetTimer();
                return;
            }


            string zoneID =
                Z.ZoneID;


            if (
                !string.Equals(
                    ScheduledZoneID,
                    zoneID,
                    StringComparison.Ordinal
                )
            )
            {
                ScheduledZoneID =
                    zoneID;


                ScheduleInitialShift();


                //
                // A zero roll means C4 begins changing immediately.
                //
                if (TurnsUntilShift <= 0)
                {
                    TryShiftZone(
                        Z,
                        player
                    );


                    ShiftFloors(
                        Z
                    );


                    ScheduleNextShift();
                }


                return;
            }


            if (!advanceTimer)
                return;


            if (TurnsUntilShift > 0)
            {
                TurnsUntilShift--;
            }


            if (TurnsUntilShift > 0)
                return;


            TryShiftZone(
                Z,
                player
            );

            ShiftFloors(
                Z
            );

            ScheduleNextShift();
        }

        private void ScheduleInitialShift()
        {
            TurnsUntilShift =
                Stat.Random(
                    MinimumInitialShiftDelay,
                    MaximumInitialShiftDelay
                );
        }


        private void ScheduleNextShift()
        {
            TurnsUntilShift =
                Stat.Random(
                    MinimumShiftDelay,
                    MaximumShiftDelay
                );
        }


        private void ResetTimer()
        {
            TurnsUntilShift =
                0;

            ScheduledZoneID =
                "";
        }

        private void ShiftFloors(
            Zone Z
        )
        {
            if (
                Z == null ||
                Options.DisableFloorTextureObjects
            )
            {
                return;
            }


            string initialFloorA =
                The.ZoneManager.GetZoneProperty(
                    Z.ZoneID,
                    SubterraneanSitesEPStaticTheme
                        .StaticC4InitialFloorAProperty
                ) as string;


            string initialFloorB =
                The.ZoneManager.GetZoneProperty(
                    Z.ZoneID,
                    SubterraneanSitesEPStaticTheme
                        .StaticC4InitialFloorBProperty
                ) as string;


            int shiftPercent =
                Stat.Random(
                    MinimumFloorShiftPercent,
                    MaximumFloorShiftPercent
                );


            foreach (
                Cell cell
                in Z.GetCells()
            )
            {
                if (cell == null)
                    continue;


                if (
                    Stat.Random(
                        1,
                        100
                    ) >
                    shiftPercent
                )
                {
                    continue;
                }


                string painter =
                    SubterraneanSitesEPStaticTheme
                        .PickRuntimeFloorPainter(
                            initialFloorA,
                            initialFloorB
                        );


                SubterraneanSitesEPStaticTheme
                    .ApplyFloorPainter(
                        cell,
                        painter
                    );
            }
        }


        private void TryShiftZone(
            Zone Z,
            GameObject player
        )
        {
            if (
                Z == null ||
                player == null ||
                player.CurrentCell == null
            )
            {
                return;
            }


            List<Location2D> anchors =
                SubterraneanSitesEPVerticalTransitions
                    .GetVerticalAnchors(
                        Z.ZoneID
                    );


            List<WallCandidate> sourceWalls =
                CollectSourceWalls(
                    Z,
                    anchors
                );


            List<Cell> destinations =
                CollectDestinations(
                    Z,
                    anchors
                );


            if (
                sourceWalls.Count == 0 ||
                destinations.Count == 0
            )
            {
                return;
            }


            int maximumPossible =
                Math.Min(
                    sourceWalls.Count,
                    destinations.Count
                );


            int desired =
                Stat.Random(
                    MinimumWallsPerShift,
                    MaximumWallsPerShift
                );


            desired =
                Math.Min(
                    desired,
                    maximumPossible
                );


            if (desired <= 0)
                return;


            //
            // Try several different redistributions before giving up.
            //
            // A failure is harmless: the attempted wall movement is rolled
            // back exactly and the existing level remains unchanged.
            //
            for (
                int attempt = 0;
                attempt < 8;
                attempt++
            )
            {
                List<WallCandidate> sourcePool =
                    new List<WallCandidate>(
                        sourceWalls
                    );


                List<Cell> destinationPool =
                    new List<Cell>(
                        destinations
                    );


                List<WallMove> moves =
                    new List<WallMove>();


                bool applied =
                    true;


                for (
                    int i = 0;
                    i < desired;
                    i++
                )
                {
                    if (
                        sourcePool.Count == 0 ||
                        destinationPool.Count == 0
                    )
                    {
                        applied =
                            false;

                        break;
                    }


                    WallCandidate source =
                        TakeRandom(
                            sourcePool
                        );


                    Cell destination =
                        TakeRandom(
                            destinationPool
                        );


                    if (
                        source == null ||
                        source.Cell == null ||
                        source.Wall == null ||
                        destination == null
                    )
                    {
                        applied =
                            false;

                        break;
                    }


                    if (
                        !ApplyMove(
                            source,
                            destination,
                            moves
                        )
                    )
                    {
                        applied =
                            false;

                        break;
                    }
                }


                if (
                    applied &&
                    CriticalRouteRemainsReachable(
                        Z,
                        player,
                        anchors
                    )
                )
                {
                    //
                    // Commit.
                    //
                    // Shared reachability metadata was generated before these
                    // walls moved, so rebuild it after the successful mutation.
                    //
                    new XRL.World.ZoneBuilders
                        .SubterraneanSitesEPGeometryReachability()
                        .BuildZone(
                            Z
                        );

                    return;
                }


                RollBack(
                    moves
                );
            }
        }


        private List<WallCandidate> CollectSourceWalls(
            Zone Z,
            List<Location2D> anchors
        )
        {
            List<WallCandidate> result =
                new List<WallCandidate>();


            foreach (
                Cell cell
                in Z.GetCells()
            )
            {
                if (
                    cell == null ||
                    IsOuterProtectedCell(
                        Z,
                        cell
                    ) ||
                    IsNearAnchor(
                        cell,
                        anchors
                    )
                )
                {
                    continue;
                }


                GameObject wall;


                if (
                    !TryGetSingleMovableWall(
                        cell,
                        out wall
                    )
                )
                {
                    continue;
                }


                //
                // IMPORTANT STATIC RULE:
                //
                // Only relocate walls that already touch open space.
                //
                // Removing one of these walls necessarily exposes a newly
                // usable cell to the existing open geometry. We therefore do
                // not slowly consume the playable map by moving useless buried
                // walls into good open cells.
                //
                if (
                    !TouchesCardinalOpenSpace(
                        Z,
                        cell
                    )
                )
                {
                    continue;
                }


                result.Add(
                    new WallCandidate
                    {
                        Cell = cell,
                        Wall = wall
                    }
                );
            }


            return result;
        }


        private List<Cell> CollectDestinations(
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
                    cell == null ||
                    IsOuterProtectedCell(
                        Z,
                        cell
                    ) ||
                    IsNearAnchor(
                        cell,
                        anchors
                    )
                )
                {
                    continue;
                }


                if (cell.HasWall())
                    continue;


                if (cell.IsSolid())
                    continue;


                if (cell.HasOpenLiquidVolume())
                    continue;


                if (cell.HasSpawnBlocker())
                    continue;


                //
                // A prospective new wall gets a one-cell safety halo around
                // ANY non-wall object.
                //
                // This is intentionally category-agnostic:
                // conveyors, furniture, hazards, treasure, denizens,
                // teleporters, pools, etc. protect themselves simply by
                // existing in the live zone.
                //
                if (
                    HasNonWallObjectNearby(
                        Z,
                        cell,
                        PlacementProtectionRadius
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


        private bool TryGetSingleMovableWall(
            Cell cell,
            out GameObject wall
        )
        {
            wall =
                null;


            if (cell == null)
                return false;


            int wallCount =
                0;


            foreach (
                GameObject obj
                in cell.GetObjects()
            )
            {
                if (obj == null)
                    continue;


                if (obj.IsWall())
                {
                    wallCount++;


                    //
                    // Wall traps and other functional wall hazards must stay
                    // where their own category put them.
                    //
                    if (
                        obj.GetPart<Walltrap>() !=
                        null
                    )
                    {
                        return false;
                    }


                    wall =
                        obj;
                }
                else
                {
                    //
                    // Do not pull a structural wall out from underneath some
                    // other object sharing its cell.
                    //
                    return false;
                }
            }


            return
                wallCount == 1 &&
                wall != null;
        }


        private bool TouchesCardinalOpenSpace(
            Zone Z,
            Cell wallCell
        )
        {
            if (
                Z == null ||
                wallCell == null
            )
            {
                return false;
            }


            Cell north =
                Z.GetCell(
                    wallCell.X,
                    wallCell.Y - 1
                );

            Cell south =
                Z.GetCell(
                    wallCell.X,
                    wallCell.Y + 1
                );

            Cell west =
                Z.GetCell(
                    wallCell.X - 1,
                    wallCell.Y
                );

            Cell east =
                Z.GetCell(
                    wallCell.X + 1,
                    wallCell.Y
                );


            return
                IsOpenSpace(
                    north
                ) ||
                IsOpenSpace(
                    south
                ) ||
                IsOpenSpace(
                    west
                ) ||
                IsOpenSpace(
                    east
                );
        }


        private bool IsOpenSpace(
            Cell cell
        )
        {
            return
                cell != null &&
                !cell.HasWall() &&
                !cell.IsSolid();
        }


        private bool IsOuterProtectedCell(
            Zone Z,
            Cell cell
        )
        {
            if (
                Z == null ||
                cell == null
            )
            {
                return true;
            }


            return
                cell.X < BorderWidth ||
                cell.Y < BorderWidth ||
                cell.X >=
                    Z.Width -
                    BorderWidth ||
                cell.Y >=
                    Z.Height -
                    BorderWidth;
        }


        private bool IsNearAnchor(
            Cell cell,
            List<Location2D> anchors
        )
        {
            if (cell == null)
                return true;


            return
                SubterraneanSitesEPPlacement
                    .IsNearAnyAnchor(
                        cell.X,
                        cell.Y,
                        anchors,
                        AnchorExclusionRadius
                    );
        }


        private bool HasNonWallObjectNearby(
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
                return true;
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


                    if (cell == null)
                        continue;


                    foreach (
                        GameObject obj
                        in cell.GetObjects()
                    )
                    {
                        if (obj == null)
                            continue;


                        if (!obj.IsWall())
                            return true;
                    }
                }
            }


            return false;
        }


        private bool ApplyMove(
            WallCandidate source,
            Cell destination,
            List<WallMove> moves
        )
        {
            if (
                source == null ||
                source.Cell == null ||
                source.Wall == null ||
                destination == null ||
                moves == null
            )
            {
                return false;
            }


            try
            {
                source.Cell.RemoveObject(
                    source.Wall,
                    true,
                    true,
                    Repaint: false
                );


                destination.AddObject(
                    source.Wall
                );


                moves.Add(
                    new WallMove
                    {
                        Source =
                            source.Cell,

                        Destination =
                            destination,

                        Wall =
                            source.Wall
                    }
                );


                return true;
            }
            catch
            {
                //
                // If removal succeeded but placement failed, put the wall
                // back where it came from before reporting failure.
                //
                if (
                    source.Wall.CurrentCell ==
                    null
                )
                {
                    source.Cell.AddObject(
                        source.Wall
                    );
                }


                return false;
            }
        }


        private bool CriticalRouteRemainsReachable(
            Zone Z,
            GameObject player,
            List<Location2D> anchors
        )
        {
            if (
                Z == null ||
                player == null ||
                player.CurrentCell == null
            )
            {
                return false;
            }


            List<Location2D> targets =
                new List<Location2D>();


            if (anchors != null)
            {
                foreach (
                    Location2D anchor
                    in anchors
                )
                {
                    if (anchor != null)
                    {
                        targets.Add(
                            anchor
                        );
                    }
                }
            }


            //
            // The bottom-layer return teleporter is not necessarily one of
            // the planned vertical anchors, so include it explicitly.
            //
            foreach (
                Cell cell
                in Z.GetCells()
            )
            {
                if (
                    cell != null &&
                    cell.HasObjectWithBlueprint(
                        "Exit Teleporter"
                    )
                )
                {
                    targets.Add(
                        Location2D.Get(
                            cell.X,
                            cell.Y
                        )
                    );
                }
            }


            if (targets.Count == 0)
                return true;


            bool[,] visited =
                new bool[
                    Z.Width,
                    Z.Height
                ];


            Queue<Location2D> queue =
                new Queue<Location2D>();


            Cell start =
                player.CurrentCell;


            queue.Enqueue(
                Location2D.Get(
                    start.X,
                    start.Y
                )
            );


            visited[
                start.X,
                start.Y
            ] = true;


            while (
                queue.Count > 0
            )
            {
                Location2D current =
                    queue.Dequeue();


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


                        int nx =
                            current.X +
                            dx;

                        int ny =
                            current.Y +
                            dy;


                        if (
                            nx < 0 ||
                            ny < 0 ||
                            nx >= Z.Width ||
                            ny >= Z.Height ||
                            visited[nx, ny]
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
                            next == null ||
                            next.HasWall()
                        )
                        {
                            continue;
                        }


                        //
                        // Respect other solid objects when judging real player
                        // movement, but allow transition/target cells
                        // themselves because stair/hole infrastructure may
                        // deliberately contain blockers.
                        //
                        if (
                            next.IsSolid() &&
                            !IsTargetCell(
                                nx,
                                ny,
                                targets
                            )
                        )
                        {
                            continue;
                        }


                        visited[
                            nx,
                            ny
                        ] = true;


                        queue.Enqueue(
                            Location2D.Get(
                                nx,
                                ny
                            )
                        );
                    }
                }
            }


            foreach (
                Location2D target
                in targets
            )
            {
                if (target == null)
                    continue;


                if (
                    target.X < 0 ||
                    target.Y < 0 ||
                    target.X >= Z.Width ||
                    target.Y >= Z.Height
                )
                {
                    continue;
                }


                if (
                    !visited[
                        target.X,
                        target.Y
                    ]
                )
                {
                    return false;
                }
            }


            return true;
        }


        private bool IsTargetCell(
            int x,
            int y,
            List<Location2D> targets
        )
        {
            if (targets == null)
                return false;


            foreach (
                Location2D target
                in targets
            )
            {
                if (
                    target != null &&
                    target.X == x &&
                    target.Y == y
                )
                {
                    return true;
                }
            }


            return false;
        }


        private void RollBack(
            List<WallMove> moves
        )
        {
            if (moves == null)
                return;


            for (
                int i =
                    moves.Count - 1;
                i >= 0;
                i--
            )
            {
                WallMove move =
                    moves[i];


                if (
                    move == null ||
                    move.Source == null ||
                    move.Destination == null ||
                    move.Wall == null
                )
                {
                    continue;
                }


                if (
                    ReferenceEquals(
                        move.Wall.CurrentCell,
                        move.Destination
                    )
                )
                {
                    move.Destination.RemoveObject(
                        move.Wall,
                        true,
                        true,
                        Repaint: false
                    );
                }


                if (
                    move.Wall.CurrentCell ==
                    null
                )
                {
                    move.Source.AddObject(
                        move.Wall
                    );
                }
            }
        }


        private T TakeRandom<T>(
            List<T> list
        )
        {
            if (
                list == null ||
                list.Count == 0
            )
            {
                return default(T);
            }


            int index =
                Stat.Random(
                    0,
                    list.Count - 1
                );


            T result =
                list[index];


            list.RemoveAt(
                index
            );


            return result;
        }
    }
}

//
// IMPORTANT:
// This namespace is OUTSIDE namespace SubterraneanSites.
//
namespace XRL.World.Effects
{
    [Serializable]
    public class SubterraneanSitesEPStaticAttunementEffect :
        SubterraneanSitesEPAttunementEffect
    {
        public SubterraneanSitesEPStaticAttunementEffect()
            : base()
        {
            ThemeKey =
                "Static";
        }


        public SubterraneanSitesEPStaticAttunementEffect(
            int duration,
            string mutationClass,
            int mutationLevel
        )
            : base(
                duration,
                "Static",

                // no conventional resistance
                "",
                0,

                // no generic save bonus
                "",
                "",
                0,

                mutationClass,
                mutationLevel
            )
        {
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
                ID ==
                    ApplyEffectEvent.ID;
        }


        public override bool HandleEvent(
            ApplyEffectEvent E
        )
        {
            if (
                E != null &&
                SubterraneanSites
                    .SubterraneanSitesEPStaticWarmStaticProtection
                    .ShouldBlockDiluteWarmStaticEffect(
                        base.Object,
                        E.Effect
                    )
            )
            {
                return false;
            }


            return
                base.HandleEvent(
                    E
                );
        }
    }
}


namespace XRL.World.Parts
{
    /// <summary>
    /// Static-reef analogue of vanilla PluckablePolyp.
    ///
    /// Vanilla PluckablePolyp cannot operate underground because it
    /// explicitly rejects zones with Z > 10.
    ///
    /// This version is deliberately one-shot. A cache-bearing Static Reef
    /// reveals its buzzing cyst when the player walks across it, manually
    /// plucks it, or reaches it through autoexplore.
    /// </summary>
    [Serializable]
    public class SubterraneanSitesStaticPluckablePolyp :
        IPart
    {
        public static readonly string COMMAND_NAME =
            "SubterraneanSitesPluckStaticPolyp";


        public bool Plucked =
            false;


        public string RevealObject =
            "SubterraneanSitesStaticBuzzingCyst";

        public int RevealChancePercent =
            0;


        public string PluckSounds =
            "pluck1,pluck2";


        public override bool SameAs(
            IPart p
        )
        {
            return false;
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
                ) &&
                ID != AutoexploreObjectEvent.ID &&
                ID != GetInventoryActionsEvent.ID &&
                ID != InventoryActionEvent.ID
            )
            {
                return
                    ID ==
                    ObjectEnteredCellEvent.ID;
            }


            return true;
        }


        public override bool HandleEvent(
            GetInventoryActionsEvent E
        )
        {
            if (
                !Plucked &&
                CanBePlucked(
                    E.Actor
                )
            )
            {
                E.AddAction(
                    "Pluck",
                    "pluck",
                    COMMAND_NAME,
                    null,
                    'p',
                    FireOnActor: false,
                    50
                );
            }


            return base.HandleEvent(
                E
            );
        }


        public override bool HandleEvent(
            InventoryActionEvent E
        )
        {
            if (
                E.Command ==
                COMMAND_NAME
            )
            {
                Pluck(
                    E.Actor
                );
            }


            return base.HandleEvent(
                E
            );
        }


        public override bool HandleEvent(
            ObjectEnteredCellEvent E
        )
        {
            //
            // Unlike vanilla reef, don't let wandering EP denizens
            // accidentally discover the hidden cache before the player.
            //
            if (
                !Plucked &&
                E.Object != null &&
                E.Object ==
                    IComponent<GameObject>.ThePlayer &&
                CanBePlucked(
                    E.Object
                )
            )
            {
                Pluck(
                    E.Object
                );
            }


            return base.HandleEvent(
                E
            );
        }


        public override bool HandleEvent(
            AutoexploreObjectEvent E
        )
        {
            if (
                !E.AutogetOnlyMode &&
                E.Command == null &&
                !Plucked &&
                CanBePlucked()
            )
            {
                E.Command =
                    COMMAND_NAME;

                E.AllowRetry =
                    true;
            }


            return base.HandleEvent(
                E
            );
        }


        public void Pluck(
            GameObject Actor
        )
        {
            if (
                Plucked ||
                Actor == null ||
                !CanBePlucked(
                    Actor
                )
            )
            {
                return;
            }


            Cell cell =
                ParentObject
                    .GetCurrentCell();


            if (cell == null)
                return;


            PlayWorldSound(
                "Sounds/Interact/sfx_interact_coralpolyp_pluck"
            );


            IComponent<GameObject>
                .XDidY(
                    Actor,
                    "pluck",
                    ParentObject.an() +
                    " free and " +
                    Actor.GetVerb(
                        "toss"
                    ) +
                    " it aside"
                );


            Plucked =
                true;


            if (
                !PluckSounds.IsNullOrEmpty() &&
                ParentObject.IsAudible(
                    IComponent<GameObject>.ThePlayer
                )
            )
            {
                PlayWorldSound(
                    PluckSounds
                        .CachedCommaExpansion()
                        .GetRandomElement(),
                    0.3f
                );
            }


            bool reveal =
                false;


            if (
                !RevealObject.IsNullOrEmpty()
            )
            {
                if (
                    RevealChancePercent >= 100
                )
                {
                    reveal =
                        true;
                }
                else if (
                    RevealChancePercent > 0 &&
                    Stat.Random(
                        1,
                        100
                    ) <=
                    RevealChancePercent
                )
                {
                    reveal =
                        true;
                }
            }


            if (!reveal)
            {
                if (
                    IComponent<GameObject>
                        .Visible(
                            Actor
                        )
                )
                {
                    CombatJuice
                        .playPrefabAnimation(
                            Actor,
                            "Particles/CoralPluck"
                        );
                }


                return;
            }


            if (
                IComponent<GameObject>
                    .Visible(
                        Actor
                    ) &&
                AutoAct.IsInterruptable()
            )
            {
                AutoAct.Interrupt();
            }


            if (
                IComponent<GameObject>
                    .Visible(
                        Actor
                    )
            )
            {
                CombatJuice
                    .playPrefabAnimation(
                        Actor,
                        "Particles/CoralCachePluck"
                    );
            }


            GameObject revealed =
                cell.AddObject(
                    RevealObject
                );
            
            


            if (revealed == null)
                return;


            IComponent<GameObject>
                .XDidYToZ(
                    Actor,
                    "reveal",
                    revealed,
                    null,
                    null,
                    null,
                    null,
                    Actor,
                    null,
                    UseFullNames: false,
                    IndefiniteSubject: false,
                    IndefiniteObject: true,
                    IndefiniteObjectForOthers: false,
                    PossessiveObject: false,
                    null,
                    null,
                    null,
                    DescribeSubjectDirection: false,
                    DescribeSubjectDirectionLate: false,
                    AlwaysVisible: false,
                    FromDialog: false,
                    Actor.IsPlayer()
                );
        }


        public bool CanBePlucked(
            GameObject Actor = null
        )
        {
            Zone currentZone =
                ParentObject.CurrentZone;


            if (currentZone == null)
                return false;


            //
            // Deliberately NO vanilla:
            //
            // if (currentZone.Z > 10)
            //     return false;
            //
            // These reefs are specifically underground.
            //
            return
                AllowPolypPluckingEvent
                    .Check(
                        currentZone,
                        ParentObject,
                        Actor
                    );
        }
    }
}

namespace XRL.World.Parts
{
    [Serializable]
    public class
        SubterraneanSitesEPStaticWarmStaticImmunity :
        IPart
    {
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
                ID ==
                    ApplyEffectEvent.ID;
        }


        public override bool HandleEvent(
            ApplyEffectEvent E
        )
        {
            if (
                E != null &&
                SubterraneanSites
                    .SubterraneanSitesEPStaticWarmStaticProtection
                    .ShouldBlockDiluteWarmStaticEffect(
                        ParentObject,
                        E.Effect
                    )
            )
            {
                return false;
            }


            return
                base.HandleEvent(
                    E
                );
        }
    }
}