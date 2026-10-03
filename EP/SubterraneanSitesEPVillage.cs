using System;
using System.Collections.Generic;
using Genkit;
using XRL;
using XRL.Core;
using XRL.World;
using XRL.Rules;
using System.Linq;
using XRL.World.ZoneBuilders.Utility;


namespace SubterraneanSites
{
    /// <summary>
    /// VILLAGE DIMENSION
    ///
    /// Category 1:
    ///     villagers / attunement friendliness -- deferred.
    ///
    /// Category 2:
    ///     farm / farmer / domesticated creatures -- deferred.
    ///
    /// Category 3:
    ///     paired strongly colored and exotic village structural materials.
    ///
    /// Category 4:
    ///     one of four village layouts selected per layer:
    ///     Ring, Comb, Shell, or Cave Cluster.
    ///     Shared Village floor treatment is applied separately.
    ///
    /// Category 5:
    ///     village furnishings / decoration -- deferred.
    /// </summary>
    internal sealed class SubterraneanSitesEPVillageTheme :
            ISubterraneanSitesEPCategoryProvider,
            ISubterraneanSitesEPAttunementProvider,
            ISubterraneanSitesEPDenizenAdaptationProvider,
            ISubterraneanSitesEPSignatureMutationProvider,
            ISubterraneanSitesEPPrimaryObjectProvider
    {
        public string ThemeKey
        {
            get { return "Village"; }
        }


        public int MinimumHoleSeparation
        {
            get { return 25; }
        }

        public string SignatureMutationClass
        {
            get { return "Beguiling"; }
        }


        public void ApplyDenizenAdaptation(
            GameObject creature
        )
        {
            //
            // Village denizens need no additional permanent adaptation.
            //
            // Their Village signature mutation, Beguile, is supplied by the
            // shared dimensional mutation package through
            // ISubterraneanSitesEPSignatureMutationProvider.
            //
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
            effect =
                null;

            successMessage =
                "";


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
                    .SubterraneanSitesEPVillageAttunementEffect(
                        SubterraneanSitesEPAttunementSystem
                            .DefaultDuration,
                        SignatureMutationClass,
                        mutationLevel
                    );


            successMessage =
                "Attunement grants:\n" +
                "Beguile (level " +
                mutationLevel.ToString() +
                ")\n" +
                "Village inhabitants become non-hostile.\n" +
                "Provoking a Village denizen into hostility ends attunement early.";


            return
                SubterraneanSitesEPAttunementBuildResult
                    .Success;
        }

        private static readonly string[]
            Category4LayoutBuilders =
            new string[]
            {
                "SubterraneanSitesVillageRingLayout",
                "SubterraneanSitesVillageCombLayout",
                "SubterraneanSitesVillageShellLayout",
                "SubterraneanSitesVillageCaveClusterLayout"
            };


        private static string PickCategory4LayoutBuilder(
            string zoneId
        )
        {
            int seed =
                XRLCore.Core.Game.GetWorldSeed(
                    "SubterraneanSites:VillageCategory4Layout:" +
                    zoneId
                );


            System.Random rng =
                new System.Random(
                    seed
                );


            return
                Category4LayoutBuilders[
                    rng.Next(
                        Category4LayoutBuilders.Length
                    )
                ];
        }

        public void RegisterCategory1Objects(
    SubterraneanSitesEPLayerContext context
        )
        {
            //
            // Village C1 has no separate early underground object pass.
            //
            // Its underground physical content is the Village population,
            // registered later through RegisterCategory1().
            //
        }

        public void RegisterEntranceCategory1Objects(
            SubterraneanSitesEPEntranceContext context
        )
        {
            //
            // Village C1 has no dedicated physical entrance-scar preview.
            //
            // Village furniture belongs to C5 and is registered through
            // RegisterEntranceDecorations().
            //
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
            // Village attunement changes the social state of Village inhabitants.
            // The persistent system keeps that state synchronized when the player
            // changes layers or when attunement expires.
            //
            The.Game.RequireSystem<
                SubterraneanSitesEPVillageSocialSystem
            >();


            //
            // C1 itself still owns the Village population.
            //
            The.ZoneManager.AddZonePostBuilder(
                context.ZoneId,
                "SubterraneanSitesVillagePopulation",
                "Tier", context.Tier.ToString()
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
                "SubterraneanSitesVillageFarm",
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
                "SubterraneanSitesVillageMaterials"
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
                PickCategory4LayoutBuilder(
                    context.ZoneId
                )
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
                "SubterraneanSitesVillageFloor"
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
                "SubterraneanSitesVillageDecorations",
                "Tier", context.Tier.ToString()
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
                "SubterraneanSitesVillageFloor",
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
                "SubterraneanSitesVillageDecorations",
                "Tier", context.Tier.ToString(),
                "EntranceOnly", "1"
            );
        }


        //
        // Village floor palettes.
        //
        // First color = main tile color.
        // Second color = highlight/detail color.
        //
        // Qud palette:
        //
        // B = blue
        // O = orange
        //
        // Y = white
        // W = yellow/gold
        //
        // K = dark grey
        // M = purple/magenta
        //
        // C = cyan
        // G = green
        //
        private static readonly string[]
            VillageFloorMainColors =
            new string[]
            {
                "B",
                "Y",
                "K",
                "C"
            };


        private static readonly string[]
            VillageFloorHighlightColors =
            new string[]
            {
                "O",
                "W",
                "M",
                "G"
            };


        internal static SubterraneanSitesEPFloorSpec
            CreateFloorSpec(
                Zone Z
            )
        {
            int paletteIndex =
                PickFloorPaletteIndex(
                    Z
                );


            string mainColor =
                VillageFloorMainColors[
                    paletteIndex
                ];


            string highlightColor =
                VillageFloorHighlightColors[
                    paletteIndex
                ];


            int patternSeed =
                0;


            if (
                Z != null &&
                !Z.ZoneID.IsNullOrEmpty()
            )
            {
                patternSeed =
                    XRLCore.Core.Game.GetWorldSeed(
                        "SubterraneanSites:VillageFloorPattern:" +
                        Z.ZoneID
                    );
            }


            return
                new SubterraneanSitesEPFloorSpec
                {
                    PaintCell =
                        delegate(Cell cell)
                        {
                            PaintFloorCell(
                                cell,
                                mainColor,
                                highlightColor,
                                patternSeed
                            );
                        }
                };
        }



       internal static int PickFloorPaletteIndex(
            Zone Z
        )
        {
            if (
                Z == null ||
                Z.ZoneID.IsNullOrEmpty()
            )
            {
                return 0;
            }


            int seed =
                XRLCore.Core.Game.GetWorldSeed(
                    "SubterraneanSites:VillageFloorPalette:" +
                    Z.ZoneID
                );


            System.Random rng =
                new System.Random(
                    seed
                );


            return
                rng.Next(
                    VillageFloorMainColors.Length
                );
        }

        private static void PaintFloorCell(
            Cell cell,
            string mainColor,
            string highlightColor,
            int patternSeed
        )
        {
            if (cell == null)
                return;


            uint hash =
                unchecked(
                    (uint)patternSeed
                );


            hash ^=
                unchecked(
                    (uint)(
                        cell.X *
                        374761393
                    )
                );


            hash ^=
                unchecked(
                    (uint)(
                        cell.Y *
                        668265263
                    )
                );


            hash ^=
                hash >>
                13;


            hash *=
                1274126177u;


            hash ^=
                hash >>
                16;


            //
            // About 30% visible grass.
            //
            bool hasGrass =
                hash %
                100u <
                30u;


            if (hasGrass)
            {
                global::XRL.World.Parts.Grassy
                    .PaintCell(
                        cell
                    );


                //
                // Defensive fallback:
                // every Village floor cell must have an actual tile.
                //
                if (
                    cell.PaintTile.IsNullOrEmpty()
                )
                {
                    cell.PaintTile =
                        "Tiles/tile-dirt1.png";
                }


                //
                // Most grass uses the primary color.
                // About 20% of the grass uses the secondary color.
                //
                bool useHighlight =
                    (
                        hash /
                        100u
                    ) %
                    100u <
                    20u;


                string grassColor =
                    useHighlight
                        ? highlightColor
                        : mainColor;


                cell.PaintTileColor =
                    "&" +
                    grassColor;


                cell.PaintColorString =
                    "&" +
                    grassColor +
                    "^k";


                cell.PaintDetailColor =
                    grassColor;
            }
            else
            {
                //
                // Bare Village ground still needs a real tile.
                //
                // Keep it very dark so visually this remains the quiet substrate
                // between the colored grass rather than reading as another terrain.
                //
                cell.PaintTile =
                    "Tiles/tile-dirt1.png";


                cell.PaintTileColor =
                    "&k";


                cell.PaintColorString =
                    "&k^k";


                cell.PaintDetailColor =
                    "k";
            }


            //
            // Intentionally no PaintRenderString.
            //
            // Shared FloorSystem cleared it before calling us. Leaving it null avoids
            // the stray '.', ',', '`', and '\'' fallback glyphs that were visible on
            // cells without a tile.
            //
        }
   
    }
}


namespace SubterraneanSites
{

    [Serializable]
    public class SubterraneanSitesEPVillageSocialSystem :
        IGameSystem
    {
        //
        // These mirror the exposure-state tracking used by Blood C1.
        //
        // They let us distinguish:
        //   - continuously remaining unattuned in Village
        // from
        //   - newly entering Village unattuned
        //   - becoming unattuned while still inside Village
        //
        public bool WasInVillageEnvironment =
            false;

        public bool WasVillageAttuned =
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
            //
            // A newly activated Village layer initially contains inhabitants
            // in their normal unattuned hostile state.
            //
            // Synchronize that new layer before we ever interpret hostility
            // as player provocation.
            //
            RefreshPlayerState(
                checkProvocation: false
            );

            return true;
        }


        public override bool HandleEvent(
            EndTurnEvent E
        )
        {
            //
            // Once the active layer has been synchronized, normal turns may
            // safely interpret newly-developed hostility as provocation.
            //
            RefreshPlayerState(
                checkProvocation: true
            );

            return true;
        }


        public void RefreshPlayerState(
            bool checkProvocation
        )
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


            bool villageEnvironment =
                string.Equals(
                    category1,
                    "Village",
                    StringComparison.Ordinal
                );
            
            XRL.World.Effects
                .SubterraneanSitesEPVillageHostilityEffect
                hostilityEffect =
                    player.GetEffectDescendedFrom<
                        XRL.World.Effects
                            .SubterraneanSitesEPVillageHostilityEffect
                    >();


            //
            // Village social friendliness belongs specifically to Village C1.
            //
            // If Village only supplied some secondary category, such as a farm
            // through C2, those inhabitants remain hostile to the player.
            //
            if (!villageEnvironment)
            {
                if (
                    hostilityEffect !=
                    null
                )
                {
                    player.RemoveEffect(
                        hostilityEffect
                    );
                }


                SynchronizeZone(
                    player.CurrentZone,
                    false
                );


                ResetExposureState();

                return;
            }


            bool villageAttuned =
                SubterraneanSitesEPAttunementSystem
                    .IsAttunedTo(
                        player,
                        "Village"
                    );

            bool attunementBrokenByProvocation =
                false;

            //
            // Let Qud decide what constitutes aggression.
            //
            // If any Village inhabitant has developed genuine hostility,
            // a hostile personal opinion, or an explicit kill target against
            // the player, the Village truce is over.
            //
            if (
                checkProvocation &&
                villageAttuned &&
                HasVillageDenizenBecomeHostile(
                    player.CurrentZone,
                    player
                )
            )
            {
                XRL.World.Effects
                    .SubterraneanSitesEPAttunementEffect
                    currentAttunement =
                        SubterraneanSitesEPAttunementSystem
                            .GetCurrentAttunement(
                                player
                            );


                if (
                    currentAttunement != null &&
                    string.Equals(
                        currentAttunement.ThemeKey,
                        "Village",
                        StringComparison.Ordinal
                    )
                )
                {
                    XRL.Messages.MessageQueue
                        .AddPlayerMessage(
                            "{{R|Your Village attunement breaks as the denizens turn against you.}}"
                        );


                    player.RemoveEffect(
                        currentAttunement
                    );
                }


                villageAttuned =
                    false;

                attunementBrokenByProvocation =
                    true;
            }
            
            //
            // The warning status exists exactly while Village C1 is active
            // and the player is not Village-attuned.
            //
            if (villageAttuned)
            {
                if (
                    hostilityEffect !=
                    null
                )
                {
                    player.RemoveEffect(
                        hostilityEffect
                    );
                }
            }
            else
            {
                if (
                    hostilityEffect ==
                    null
                )
                {
                    hostilityEffect =
                        new XRL.World.Effects
                            .SubterraneanSitesEPVillageHostilityEffect();


                    player.ApplyEffect(
                        hostilityEffect
                    );
                }
            }

            //
            // Same transition logic as Blood:
            //
            // Warn when the player is unattuned AND either:
            //
            // 1. they just entered Village C1, or
            // 2. they were attuned on the previous synchronization and
            //    have now become unattuned while still inside Village.
            //
            bool becameExposed =
                !attunementBrokenByProvocation &&
                !villageAttuned &&
                (
                    !WasInVillageEnvironment ||
                    WasVillageAttuned
                );

            WasInVillageEnvironment =
                true;

            WasVillageAttuned =
                villageAttuned;


            if (becameExposed)
            {
                XRL.UI.Popup.Show(
                    "You sense hostility."
                );
            }


            //
            // Keep villagers, farmer, and farm animals synchronized with
            // the current Village-attunement state.
            //
            SynchronizeZone(
                player.CurrentZone,
                villageAttuned
            );
        }

        private static bool HasVillageDenizenBecomeHostile(
            Zone zone,
            GameObject player
        )
        {
            if (
                zone == null ||
                player == null
            )
            {
                return false;
            }


            bool hostile =
                false;


            zone.ForeachObject(
                delegate(GameObject inhabitant)
                {
                    if (
                        hostile ||
                        inhabitant == null ||
                        inhabitant.Brain == null ||
                        !inhabitant.HasStringProperty(
                            "SubterraneanSitesEPVillageInhabitant"
                        )
                    )
                    {
                        return;
                    }


                    //
                    // Signal 1:
                    //
                    // Qud's final social judgment already says this creature
                    // is hostile to the player.
                    //
                    if (
                        inhabitant.IsHostileTowards(
                            player
                        )
                    )
                    {
                        hostile =
                            true;

                        return;
                    }


                    //
                    // Signal 2:
                    //
                    // Village attunement sets Calm. Brain.GetFeeling() can
                    // therefore suppress moderate negative feeling back to
                    // neutral.
                    //
                    // GetPersonalFeeling() lets us see the creature's direct
                    // grievance against the player underneath that artificial
                    // Village calm state.
                    //
                    int? personalFeeling =
                        inhabitant.Brain
                            .GetPersonalFeeling(
                                player
                            );


                    if (
                        personalFeeling.HasValue &&
                        personalFeeling.Value <
                            XRL.World.Parts.Brain
                                .FEELING_HOSTILE_THRESHOLD
                    )
                    {
                        hostile =
                            true;

                        return;
                    }


                    //
                    // Signal 3:
                    //
                    // An explicit Kill goal against the player is exposed
                    // through Brain.Target. Catch this even if some special
                    // mechanic bypassed ordinary feeling calculation.
                    //
                    if (
                        inhabitant.Target ==
                        player
                    )
                    {
                        hostile =
                            true;
                    }
                }
            );


            return hostile;
        }

        public static void SynchronizeZone(
            Zone zone,
            bool villageAttuned
        )
        {
            if (zone == null)
                return;


            zone.ForeachObject(
                delegate(GameObject inhabitant)
                {
                    if (
                        inhabitant == null ||
                        inhabitant.Brain == null ||
                        !inhabitant.HasStringProperty(
                            "SubterraneanSitesEPVillageInhabitant"
                        )
                    )
                    {
                        return;
                    }


                    XRL.World.ZoneBuilders
                        .SubterraneanSitesVillageInhabitant
                        .ApplySocialState(
                            inhabitant,
                            villageAttuned
                        );
                }
            );
        }

        public static void EstablishAttunedTruce(
            Zone zone,
            GameObject player
        )
        {
            if (
                zone == null ||
                player == null
            )
            {
                return;
            }


            zone.ForeachObject(
                delegate(GameObject inhabitant)
                {
                    if (
                        inhabitant == null ||
                        inhabitant.Brain == null ||
                        !inhabitant.HasStringProperty(
                            "SubterraneanSitesEPVillageInhabitant"
                        )
                    )
                    {
                        return;
                    }


                    //
                    // First install the attuned Village allegiance.
                    //
                    XRL.World.ZoneBuilders
                        .SubterraneanSitesVillageInhabitant
                        .ApplySocialState(
                            inhabitant,
                            true
                        );


                    //
                    // Re-attunement represents a fresh truce.
                    //
                    // Forgive only grievances against this player rather than
                    // globally deleting the creature's combat opinions.
                    //
                    inhabitant.Brain.Forgive(
                        player
                    );


                    //
                    // Reset accumulated accidental-friendly-fire incidents
                    // involving this player as part of the new truce.
                    //
                    if (
                        inhabitant.Brain.FriendlyFire !=
                        null
                    )
                    {
                        inhabitant.Brain.FriendlyFire
                            .Remove(
                                player
                            );
                    }


                    //
                    // Remove an existing Kill goal against the player.
                    //
                    if (
                        inhabitant.Target ==
                        player
                    )
                    {
                        inhabitant.Target =
                            null;
                    }
                }
            );
        }


        private void ResetExposureState()
        {
            WasInVillageEnvironment =
                false;

            WasVillageAttuned =
                false;
        }
    }


}


namespace XRL.World.ZoneBuilders
{


    internal static class SubterraneanSitesVillageInhabitant
    {
        internal const string ThemeKey =
            "Village";

        internal const string AttunedFactionKey =
            "SubterraneanSitesVillageInhabitants";

        internal const string UnattunedFactionKey =
            "Playerhater";


        internal static void Prepare(
            GameObject creature,
            int pocketTier
        )
        {
            if (
                creature == null ||
                creature.Brain == null
            )
            {
                return;
            }

            SubterraneanSites
                .SubterraneanSitesDimensionEngine
                .SanitizePocketInhabitantSpawnBehavior(
                    creature
                );


            //
            // Apply the true generated Village-dimensional identity first.
            //
            // ApplyDecorationDimensionIdentity records the actual generated
            // dimension faction in EP metadata before we replace the creature's
            // runtime allegiance below.
            //
            SubterraneanSites
                .SubterraneanSitesDimensionEngine
                .ApplyDecorationDimensionIdentity(
                    creature,
                    ThemeKey
                );

            SubterraneanSites
                .SubterraneanSitesDimensionEngine
                .ConfigureDenizenLoot(
                    creature
                );


            //
            // Village inhabitants deliberately trail the pocket by one tier.
            //
            int inhabitantTier =
                Math.Max(
                    1,
                    pocketTier - 1
                );


            int targetLevel =
                SubterraneanSites
                    .SubterraneanSitesDimensionEngine
                    .GetMinimumLevelForTier(
                        inhabitantTier
                    );


            SubterraneanSites
                .SubterraneanSitesDimensionEngine
                .ScaleCreatureUpToLevel(
                    creature,
                    targetLevel
                );

            //
            // All Village inhabitants begin in the unattuned state.
            //
            // ApplyDecorationDimensionIdentity above has already applied
            // the actual extradimensional identity and recorded its
            // generated dimensional faction. From this point onward,
            // Village controls only the creature's live social allegiance.
            //
            ApplySocialState(
                creature,
                false
            );



            creature.SetStringProperty(
                "SubterraneanSitesEPVillageInhabitant",
                "Yes"
            );
        }

        internal static void ApplySocialState(
            GameObject creature,
            bool villageAttuned
        )
        {
            if (
                creature == null ||
                creature.Brain == null
            )
            {
                return;
            }


            creature.Brain.Allegiance.Clear();


            if (villageAttuned)
            {
                //
                // Attuned Village inhabitants belong to our private,
                // player-neutral faction.
                //
                // Factions.xml makes this faction mutually friendly with
                // Playerhater, so ordinary EP denizens remain their allies.
                //
                creature.Brain.Allegiance.Add(
                    AttunedFactionKey,
                    100
                );

                creature.Brain.Allegiance.Hostile =
                    false;

                creature.Brain.Allegiance.Calm =
                    true;
            }
            else
            {
                //
                // Unattuned inhabitants are deliberately and reliably
                // hostile to the player.
                //
                creature.Brain.Allegiance.Add(
                    UnattunedFactionKey,
                    100
                );

                creature.Brain.Allegiance.Hostile =
                    true;

                creature.Brain.Allegiance.Calm =
                    false;
            }


            creature.Brain.Hibernating =
                false;



        }

    }



        /// <summary>
        /// Village Category 1 population.
        ///
        /// C1 owns only the inhabitants themselves.
        ///
        /// Population is drawn from the faction assigned to the Village
        /// extradimensional identity. The pocket does not need Village C2-C5
        /// architecture in order for these inhabitants to appear.
        ///
        /// Social/attunement behavior is deliberately handled separately.
        /// </summary>
        public class SubterraneanSitesVillagePopulation :
            ZoneBuilderSandbox
        {
            public int Tier = 1;


            private const int MinimumPopulation =
                4;

            private const int MaximumPopulation =
                8;

            private const int TransitionExclusionRadius =
                4;


            public bool BuildZone(
                Zone Z
            )
            {
                if (Z == null)
                    return true;


                Tier =
                    Math.Max(
                        1,
                        Math.Min(
                            8,
                            Tier
                        )
                    );


                SubterraneanSites
                    .SubterraneanSitesDimensionBinding
                    dimension =
                        SubterraneanSites
                            .SubterraneanSitesDimensionEngine
                            .GetBindingByThemeKey(
                                "Village"
                            );


                if (dimension == null)
                    return true;


                int inhabitantTier =
                    Math.Max(
                        1,
                        Tier - 1
                    );


                int seed =
                    XRLCore.Core.Game.GetWorldSeed(
                        "SubterraneanSites:VillagePopulation:" +
                        Z.ZoneID
                    );


                System.Random rng =
                    new System.Random(
                        seed
                    );


                int populationCount =
                    rng.Next(
                        MinimumPopulation,
                        MaximumPopulation + 1
                    );


                //
                // ------------------------------------------------------------
                // BUILD THE VILLAGE ROSTER
                // ------------------------------------------------------------
                //
                // Warden, Mayor, and legendary villager are part of the total
                // population rather than bonus creatures added on top.
                //
                List<string> roles =
                    new List<string>
                    {
                        "Warden",
                        "Mayor",
                        "Legendary"
                    };


                //
                // Vanilla-style optional specialist roles.
                //
                // These consume ordinary population slots rather than increasing
                // the final population size.
                //
                List<string> optionalRoles =
                    new List<string>();


                if (
                    rng.Next(
                        100
                    ) <
                    30
                )
                {
                    optionalRoles.Add(
                        "Merchant"
                    );
                }


                if (
                    rng.Next(
                        100
                    ) <
                    25
                )
                {
                    optionalRoles.Add(
                        "Tinker"
                    );
                }


                if (
                    rng.Next(
                        100
                    ) <
                    25
                )
                {
                    optionalRoles.Add(
                        "Apothecary"
                    );
                }


                //
                // Randomize specialist priority if the population roll was small
                // enough that not every rolled specialist can fit.
                //
                for (
                    int i =
                        optionalRoles.Count - 1;
                    i > 0;
                    i--
                )
                {
                    int j =
                        rng.Next(
                            i + 1
                        );


                    string temp =
                        optionalRoles[i];


                    optionalRoles[i] =
                        optionalRoles[j];


                    optionalRoles[j] =
                        temp;
                }


                foreach (
                    string optionalRole
                    in optionalRoles
                )
                {
                    if (
                        roles.Count >=
                        populationCount
                    )
                    {
                        break;
                    }


                    roles.Add(
                        optionalRole
                    );
                }


                while (
                    roles.Count <
                    populationCount
                )
                {
                    roles.Add(
                        "Villager"
                    );
                }


                //
                // ------------------------------------------------------------
                // CREATE AND PLACE THE INHABITANTS
                // ------------------------------------------------------------
                //
                foreach (
                    string role
                    in roles
                )
                {
                    string sourceDescription;


                    GameObject inhabitant =
                        SubterraneanSites
                            .SubterraneanSitesDimensionEngine
                            .CreateDenizenForSite(
                                dimension,
                                inhabitantTier,
                                out sourceDescription
                            );


                    if (inhabitant == null)
                        continue;


                    inhabitant =
                        ApplyVillageRole(
                            inhabitant,
                            role,
                            inhabitantTier
                        );


                    if (inhabitant == null)
                        continue;


                    //
                    // Apply dimensional identity AFTER HeroMaker / role conversion.
                    //
                    // This ensures the generated Village dimension owns the final
                    // visual identity and faction assignment.
                    //
                    SubterraneanSitesVillageInhabitant
                        .Prepare(
                            inhabitant,
                            Tier
                        );


                    inhabitant.SetIntProperty(
                        "Villager",
                        1
                    );


                    inhabitant.SetIntProperty(
                        "ParticipantVillager",
                        1
                    );


                    inhabitant.SetStringProperty(
                        "SubterraneanSitesEPVillageInhabitant",
                        "Yes"
                    );


                    inhabitant.SetStringProperty(
                        "SubterraneanSitesEPVillageRole",
                        role
                    );

                    ConfigureVillageInventoryControl(
                        inhabitant,
                        role,
                        rng
                    );



                    Cell destination =
                        PickPlacementCell(
                            Z,
                            inhabitant,
                            rng
                        );


                    if (destination == null)
                    {
                        inhabitant.Release(
                            RemoveFromContext: false
                        );

                        continue;
                    }


                    destination.AddObject(
                        inhabitant
                    );


                    inhabitant.MakeActive();


                    SubterraneanSites
                        .SubterraneanSitesEPReservations
                        .ClaimCell(
                            Z,
                            destination
                        );
                }


                return true;
            }



            private GameObject ApplyVillageRole(
                GameObject creature,
                string role,
                int inhabitantTier
            )
            {
                if (creature == null)
                    return null;

                //
                // HeroMaker must know before promotion that these are not
                // ordinary water-ritual faction heroes.
                //
                creature.SetStringProperty(
                    "HeroNoWaterRitual",
                    "true"
                );


                switch (role)
                {
                    case "Warden":
                        return MakeWarden(
                            creature
                        );


                    case "Mayor":
                        return MakeMayor(
                            creature
                        );


                    case "Legendary":
                        return MakeLegendaryVillager(
                            creature
                        );


                    case "Merchant":
                        return MakeMerchant(
                            creature,
                            inhabitantTier
                        );


                    case "Tinker":
                        return MakeTinker(
                            creature,
                            inhabitantTier
                        );


                    case "Apothecary":
                        return MakeApothecary(
                            creature,
                            inhabitantTier
                        );


                    default:
                        return creature;
                }
            }



            private GameObject MakeWarden(
                GameObject creature
            )
            {
                GameObject promoted =
                    HeroMaker.MakeHero(
                        creature,
                        "SpecialVillagerHeroTemplate_Warden",
                        -1,
                        "Warden"
                    );


                if (promoted == null)
                    return creature;


                promoted
                    .RequirePart<
                        XRL.World.Parts.Interesting
                    >();


                promoted.SetIntProperty(
                    "VillageWarden",
                    1
                );


                promoted.SetIntProperty(
                    "NamedVillager",
                    1
                );


                TakeOnRoleEvent.Send(
                    promoted,
                    "Warden"
                );


                return promoted;
            }



            private GameObject MakeMayor(
                GameObject creature
            )
            {
                GameObject promoted =
                    HeroMaker.MakeHero(
                        creature,
                        "SpecialVillagerHeroTemplate_Mayor",
                        -1,
                        "Mayor"
                    );


                if (promoted == null)
                    return creature;


                promoted
                    .RequirePart<
                        XRL.World.Parts.Interesting
                    >();


                promoted.SetIntProperty(
                    "VillageMayor",
                    1
                );


                promoted.SetIntProperty(
                    "NamedVillager",
                    1
                );


                TakeOnRoleEvent.Send(
                    promoted,
                    "Mayor"
                );


                return promoted;
            }



            private GameObject MakeLegendaryVillager(
                GameObject creature
            )
            {
                GameObject promoted =
                    HeroMaker.MakeHero(
                        creature,
                        (string)null,
                        "SpecialVillagerHeroTemplate_Villager"
                    );


                if (promoted == null)
                    return creature;


                promoted.SetIntProperty(
                    "NamedVillager",
                    1
                );


                promoted
                    .RequirePart<
                        XRL.World.Parts.Interesting
                    >();


                return promoted;
            }



            private GameObject MakeMerchant(
                GameObject creature,
                int inhabitantTier
            )
            {
                string template =
                    creature
                        .GetBlueprint()
                        .DescendsFrom(
                            "Dromad"
                        )
                        ? "SpecialVillagerHeroTemplate_DromadMerchant"
                        : "SpecialVillagerHeroTemplate_Merchant";


                GameObject promoted =
                    HeroMaker.MakeHero(
                        creature,
                        template,
                        -1,
                        "Merchant"
                    );


                if (promoted == null)
                    promoted =
                        creature;

                promoted
                    .RequirePart<
                        XRL.World.Parts.Interesting
                    >();


                promoted
                    .RequirePart<
                        XRL.World.Parts.Inventory
                    >();


                XRL.World.Parts
                    .GenericInventoryRestocker
                    restocker =
                        promoted
                            .RequirePart<
                                XRL.World.Parts
                                    .GenericInventoryRestocker
                            >();


                restocker.Clear();


                for (
                    int i = 0;
                    i <= 2 &&
                    inhabitantTier > i;
                    i++
                )
                {
                    restocker.AddTable(
                        "Tier" +
                        (
                            inhabitantTier -
                            i
                        ).ToString() +
                        "Wares"
                    );
                }


                restocker.PerformRestock(
                    Silent: true
                );


                promoted.SetIntProperty(
                    "VillageMerchant",
                    1
                );


                promoted.SetIntProperty(
                    "NamedVillager",
                    1
                );


                TakeOnRoleEvent.Send(
                    promoted,
                    "Merchant"
                );


                return promoted;
            }



            private GameObject MakeTinker(
                GameObject creature,
                int inhabitantTier
            )
            {
                GameObject template =
                    GameObjectFactory.Factory
                        .Blueprints[
                            "HumanTinker" +
                            inhabitantTier.ToString()
                        ]
                        .createOne();


                if (template != null)
                {
                    XRL.World.Parts.Skills
                        skills =
                            template.GetPart<
                                XRL.World.Parts.Skills
                            >();


                    if (skills != null)
                    {
                        foreach (
                            XRL.World.Parts.Skill
                                .BaseSkill skill
                            in skills.SkillList
                        )
                        {
                            creature.AddSkill(
                                skill.Name
                            );
                        }
                    }


                    XRL.World.Parts
                        .GenericInventoryRestocker
                        sourceRestocker =
                            template.GetPart<
                                XRL.World.Parts
                                    .GenericInventoryRestocker
                            >();

                    creature
                        .RequirePart<
                            XRL.World.Parts.Inventory
                        >();

                    XRL.World.Parts
                        .GenericInventoryRestocker
                        restocker =
                            creature
                                .RequirePart<
                                    XRL.World.Parts
                                        .GenericInventoryRestocker
                                >();


                    restocker.Table =
                        sourceRestocker ==
                        null
                            ? "Village Tinker 1"
                            : sourceRestocker.Table;


                    restocker.Chance =
                        100;


                    restocker.PerformRestock(
                        Silent: true
                    );


                    template.Release(
                        RemoveFromContext: false
                    );
                }


                GameObject promoted =
                    HeroMaker.MakeHero(
                        creature,
                        "SpecialVillagerHeroTemplate_Tinker",
                        -1,
                        "Tinker"
                    );


                if (promoted == null)
                    promoted =
                        creature;


                promoted
                    .RequirePart<
                        XRL.World.Parts.Interesting
                    >();


                promoted.SetIntProperty(
                    "VillageTinker",
                    1
                );


                promoted.SetIntProperty(
                    "NamedVillager",
                    1
                );


                TakeOnRoleEvent.Send(
                    promoted,
                    "Tinker"
                );


                return promoted;
            }



            private GameObject MakeApothecary(
                GameObject creature,
                int inhabitantTier
            )
            {
                GameObject template =
                    GameObjectFactory.Factory
                        .Blueprints[
                            "HumanApothecary" +
                            inhabitantTier.ToString()
                        ]
                        .createOne();


                if (template != null)
                {
                    XRL.World.Parts.Skills
                        skills =
                            template.GetPart<
                                XRL.World.Parts.Skills
                            >();


                    if (skills != null)
                    {
                        foreach (
                            XRL.World.Parts.Skill
                                .BaseSkill skill
                            in skills.SkillList
                        )
                        {
                            creature.AddSkill(
                                skill.Name
                            );
                        }
                    }


                    XRL.World.Parts
                        .GenericInventoryRestocker
                        sourceRestocker =
                            template.GetPart<
                                XRL.World.Parts
                                    .GenericInventoryRestocker
                            >();
                    
                    creature
                        .RequirePart<
                            XRL.World.Parts.Inventory
                        >();


                    XRL.World.Parts
                        .GenericInventoryRestocker
                        restocker =
                            creature
                                .RequirePart<
                                    XRL.World.Parts
                                        .GenericInventoryRestocker
                                >();


                    restocker.Table =
                        sourceRestocker ==
                        null
                            ? "Village Apothecary 1"
                            : sourceRestocker.Table;


                    restocker.Chance =
                        100;


                    restocker.PerformRestock(
                        Silent: true
                    );


                    template.Release(
                        RemoveFromContext: false
                    );
                }


                GameObject promoted =
                    HeroMaker.MakeHero(
                        creature,
                        "SpecialVillagerHeroTemplate_Apothecary",
                        -1,
                        "Apothecary"
                    );


                if (promoted == null)
                    promoted =
                        creature;


                promoted
                    .RequirePart<
                        XRL.World.Parts.Interesting
                    >();


                promoted.SetIntProperty(
                    "VillageApothecary",
                    1
                );


                promoted.SetIntProperty(
                    "NamedVillager",
                    1
                );


                TakeOnRoleEvent.Send(
                    promoted,
                    "Apothecary"
                );


                return promoted;
            }

            private void ConfigureVillageInventoryControl(
                GameObject inhabitant,
                string role,
                System.Random rng
            )
            {
                if (
                    inhabitant == null ||
                    rng == null
                )
                {
                    return;
                }


                bool fullMerchant =
                    string.Equals(
                        role,
                        "Merchant",
                        StringComparison.Ordinal
                    ) ||
                    string.Equals(
                        role,
                        "Tinker",
                        StringComparison.Ordinal
                    ) ||
                    string.Equals(
                        role,
                        "Apothecary",
                        StringComparison.Ordinal
                    );


                int minimumStock =
                    fullMerchant
                        ? 2
                        : 0;


                int maximumStock =
                    fullMerchant
                        ? 3
                        : 1;


                XRL.World.Parts
                    .GenericInventoryRestocker
                    restocker =
                        inhabitant.GetPart<
                            XRL.World.Parts
                                .GenericInventoryRestocker
                        >();


                //
                // Any inhabitant with a vanilla restocker needs the persistent
                // EP controller so future vanilla restocks are constrained too.
                //
                if (restocker != null)
                {
                    SubterraneanSites
                        .SubterraneanSitesEPMerchantStockController
                        controller =
                            inhabitant.RequirePart<
                                SubterraneanSites
                                    .SubterraneanSitesEPMerchantStockController
                            >();


                    controller.DimensionThemeKey =
                        "Village";

                    controller.MinimumStock =
                        minimumStock;

                    controller.MaximumStock =
                        maximumStock;
                }


                //
                // Merchant / Tinker / Apothecary role construction normally
                // performs a real vanilla stock operation before reaching here.
                //
                // Detect that by looking for _stock.
                //
                bool hasGeneratedStock =
                    false;


                List<GameObject> inventory =
                    inhabitant.GetInventory();


                if (inventory != null)
                {
                    foreach (
                        GameObject item
                        in inventory
                    )
                    {
                        if (
                            item != null &&
                            item.HasProperty(
                                "_stock"
                            )
                        )
                        {
                            hasGeneratedStock =
                                true;

                            break;
                        }
                    }
                }


                if (hasGeneratedStock)
                {
                    //
                    // Vanilla-generated merchandise exists.
                    //
                    // The shared controller now removes all other carried
                    // sale inventory, retains only the requested number of
                    // _stock entries, forces retained stacks to one item,
                    // and dimensionalizes the survivors.
                    //
                    SubterraneanSites
                        .SubterraneanSitesEPMerchantStockControl
                        .TrimAndDimensionalizeRestockedStock(
                            inhabitant,
                            "Village",
                            minimumStock,
                            maximumStock,
                            rng
                        );
                }
                else
                {
                    //
                    // Ordinary villagers, or a merchant-like source creature
                    // whose stock has not yet generated, use whatever carried
                    // inventory presently exists.
                    //
                    // Equipped gear is not part of GetInventory() and is
                    // therefore untouched.
                    //
                    SubterraneanSites
                        .SubterraneanSitesEPMerchantStockControl
                        .TrimAndDimensionalizeStock(
                            inhabitant,
                            "Village",
                            minimumStock,
                            maximumStock,
                            rng
                        );
                }
            }



            private Cell PickPlacementCell(
                Zone Z,
                GameObject creature,
                System.Random rng
            )
            {
                List<Cell> candidates =
                    new List<Cell>();


                List<Location2D> anchors =
                    SubterraneanSites
                        .SubterraneanSitesEPVerticalTransitions
                        .GetVerticalAnchors(
                            Z.ZoneID
                        );


                foreach (
                    Cell cell
                    in Z.GetCells()
                )
                {
                    if (
                        cell == null ||
                        !cell.IsReachable() ||
                        !cell.IsSpawnable() ||
                        !cell.IsEmptyOfSolid() ||
                        cell.HasSpawnBlocker()
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


                    if (
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


                    if (
                        creature != null &&
                        cell.GetNavigationWeightFor(
                            creature
                        ) >=
                        30
                    )
                    {
                        continue;
                    }


                    candidates.Add(
                        cell
                    );
                }


                if (
                    candidates.Count ==
                    0
                )
                {
                    return null;
                }


                return
                    candidates[
                        rng.Next(
                            candidates.Count
                        )
                    ];
            }
        }




    /// <summary>
    /// Village Category-2 farm.
    ///
    /// Stamps one enclosed animal pen and attached hut into the existing
    /// EP geometry. Like Portal C2 installations, the farm may excavate
    /// abstract C4 structure, but it will not overwrite earlier claimed
    /// content, meaningful occupants, or vertical transitions.
    /// </summary>
    public class SubterraneanSitesVillageFarm :
        ZoneBuilderSandbox
    {
        public int Tier = 1;


        private const int FarmWidth =
            18;

        private const int FarmHeight =
            10;

        private const int BorderClearance =
            1;

        private const int TransitionExclusionRadius =
            4;
        
        private const int MinimumFarmWidth = 14;
        private const int MinimumFarmHeight = 8;

        private enum FarmShape
        {
            Rectangle,
            ShortSide,
            NotchedCorner,
            OffsetEnd
        }

        public bool BuildZone(
            Zone Z
        )
        {
            if (Z == null)
                return true;


            Tier =
                Math.Max(
                    1,
                    Math.Min(
                        8,
                        Tier
                    )
                );


            List<Location2D> anchors =
                SubterraneanSites
                    .SubterraneanSitesEPVerticalTransitions
                    .GetVerticalAnchors(
                        Z.ZoneID
                    );


            int seed =
                XRLCore.Core.Game.GetWorldSeed(
                    "SubterraneanSites:VillageFarm:" +
                    Z.ZoneID
                );


            System.Random rng =
                new System.Random(
                    seed
                );

            FarmShape farmShape =
                (FarmShape)rng.Next(
                    4
                );

            int actualWidth =
                rng.Next(
                    MinimumFarmWidth,
                    FarmWidth + 1
                );

            int actualHeight =
                rng.Next(
                    MinimumFarmHeight,
                    FarmHeight + 1
                );

            List<Cell> candidates = 
                new List<Cell>(); 


            int maxX = 
                Z.Width - 
                BorderClearance - 
                actualWidth; 

            int maxY = 
                Z.Height - 
                BorderClearance - 
                actualHeight; 


            for ( 
                int x = BorderClearance; 
                x <= maxX; 
                x++ 
            ) 
            { 
                for ( 
                    int y = BorderClearance; 
                    y <= maxY; 
                    y++ 
                ) 
                { 
                    if ( 
                        FarmFootprintAvailable( 
                            Z, 
                            x, 
                            y, 
                            actualWidth,
                            actualHeight,
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
                return true; 


            //
            // Randomize the candidate order first.
            //
            // The stable edge-distance sort below will then preserve random
            // ordering among candidates equally far from an outside edge.
            //
            for ( 
                int i = candidates.Count - 1; 
                i > 0; 
                i-- 
            ) 
            { 
                int j = 
                    rng.Next( 
                        i + 1 
                    ); 


                Cell temp = 
                    candidates[i]; 


                candidates[i] = 
                    candidates[j]; 


                candidates[j] = 
                    temp; 
            }


            //
            // Prefer farms near an outside edge.
            //
            // BorderClearance is now zero, so a farm is allowed to touch
            // the actual edge of the zone.
            //
            candidates = 
                candidates
                    .OrderBy( 
                        cell => 
                            Math.Min( 
                                Math.Min( 
                                    cell.X, 
                                    Z.Width - 
                                    ( 
                                        cell.X + 
                                        actualWidth 
                                    ) 
                                ), 
                                Math.Min( 
                                    cell.Y, 
                                    Z.Height - 
                                    ( 
                                        cell.Y + 
                                        actualHeight 
                                    ) 
                                ) 
                            ) 
                    )
                    .ToList(); 


            Cell chosen = 
                candidates[0]; 


            BuildFarm( 
                Z, 
                chosen.X, 
                chosen.Y, 
                actualWidth,
                actualHeight,
                farmShape,
                rng 
            );
           


            //
            // C2 has changed the geometry.
            //
            Z.ClearReachableMap();


            return true;
        }



        private bool FarmFootprintAvailable(
            Zone Z,
            int x1,
            int y1,
            int farmWidth,
            int farmHeight,
            List<Location2D> anchors
        )
        {
            int x2 =
                x1 +
                farmWidth -
                1;

            int y2 =
                y1 +
                farmHeight -
                1;


            //
            // Respect earlier C1/C2 semantic ownership.
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


            bool touchesOpenGeometry =
                false;


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
                    // Exactly like Portal C2:
                    // abstract C4 structure may be excavated,
                    // but real solid content survives.
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
                                x,
                                y,
                                anchors,
                                TransitionExclusionRadius
                            )
                    )
                    {
                        return false;
                    }


                    if (
                        SubterraneanSites
                            .SubterraneanSitesEPGeometry
                            .IsOpenGeometryCell(
                                cell
                            )
                    )
                    {
                        touchesOpenGeometry =
                            true;
                    }
                }
            }


            //
            // Do not create a completely buried farm.
            //
            return touchesOpenGeometry;
        }

        private bool IsFarmCell(
            int x,
            int y,
            int x1,
            int y1,
            int x2,
            int y2,
            FarmShape farmShape
        )
        {

            if (
                x < x1 ||
                x > x2 ||
                y < y1 ||
                y > y2
            )
            {
                return false;
            }

            switch (farmShape)
            {
                case FarmShape.ShortSide:
                {
                    //
                    // Trim the lower-right end of the farm.
                    //
                    int cutX =
                        x2 - 3;

                    int cutY =
                        y2 - 2;

                    return !(
                        x >= cutX &&
                        y >= cutY
                    );
                }

                case FarmShape.NotchedCorner:
                {
                    //
                    // Remove a small upper-right corner.
                    //
                    int notchX =
                        x2 - 3;

                    int notchY =
                        y1 + 2;

                    return !(
                        x >= notchX &&
                        y <= notchY
                    );
                }

                case FarmShape.OffsetEnd:
                {
                    //
                    // Narrow the right-hand end, producing
                    // a simple stepped footprint.
                    //
                    int offsetStart =
                        x2 - 4;

                    return
                        x < offsetStart ||
                        (
                            y >= y1 + 2 &&
                            y <= y2 - 2
                        );
                }

                default:
                    return true;
            }
        }


        private bool IsFarmBoundaryCell(
            int x,
            int y,
            int x1,
            int y1,
            int x2,
            int y2,
            FarmShape farmShape
        )
        {
            if (
                !IsFarmCell(
                    x,
                    y,
                    x1,
                    y1,
                    x2,
                    y2,
                    farmShape
                )
            )
            {
                return false;
            }


            //
            // A farm cell is boundary if any cardinal neighbor
            // falls outside the farm footprint.
            //
            return
                !IsFarmCell(
                    x - 1,
                    y,
                    x1,
                    y1,
                    x2,
                    y2,
                    farmShape
                ) ||
                !IsFarmCell(
                    x + 1,
                    y,
                    x1,
                    y1,
                    x2,
                    y2,
                    farmShape
                ) ||
                !IsFarmCell(
                    x,
                    y - 1,
                    x1,
                    y1,
                    x2,
                    y2,
                    farmShape
                ) ||
                !IsFarmCell(
                    x,
                    y + 1,
                    x1,
                    y1,
                    x2,
                    y2,
                    farmShape
                );
        }   

        private void BuildFarm(
            Zone Z,
            int x1,
            int y1,
            int farmWidth,
            int farmHeight,
            FarmShape farmShape,
            System.Random rng
        )
        {
            int x2 =
                x1 +
                farmWidth -
                1;

            int y2 =
                y1 +
                farmHeight -
                1;


            //
            // Claim the whole installation before constructing it.
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


            //
            // Stamp through the abstract C4 structure,
            // but only inside the selected farm shape.
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
                    if (
                        !IsFarmCell(
                            x,
                            y,
                            x1,
                            y1,
                            x2,
                            y2,
                            farmShape
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


            //
            // ------------------------------------------------------------
            // OUTER ANIMAL PEN
            // ------------------------------------------------------------
            //
            // Fence every boundary cell of the actual farm shape.
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
                    if (
                        !IsFarmBoundaryCell(
                            x,
                            y,
                            x1,
                            y1,
                            x2,
                            y2,
                            farmShape
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


                    cell.ClearWalls();

                    cell.AddObject(
                        "SubterraneanSitesVillageFarmFence"
                    );
                }
            }


            //
            // Put the gate on the bottom edge, near the center.
            //
            int gateX =
                x1 +
                farmWidth / 2;

            int gateY =
                y2;

            //
            // ------------------------------------------------------------
            // FARM GATE
            // ------------------------------------------------------------
            //
            // Prefer a boundary cell whose outside neighbor is already
            // open Village geometry. This makes the gate face usable space
            // instead of opening directly toward a wall.
            //
            List<Cell> gateCandidates =
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
                    if (
                        !IsFarmBoundaryCell(
                            x,
                            y,
                            x1,
                            y1,
                            x2,
                            y2,
                            farmShape
                        )
                    )
                    {
                        continue;
                    }


                    //
                    // Determine which cardinal neighbor lies outside
                    // the farm at this boundary cell.
                    //
                    int outsideX =
                        x;

                    int outsideY =
                        y;

                    bool hasOutsideNeighbor =
                        false;


                    if (
                        !IsFarmCell(
                            x - 1,
                            y,
                            x1,
                            y1,
                            x2,
                            y2,
                            farmShape
                        )
                    )
                    {
                        outsideX =
                            x - 1;

                        outsideY =
                            y;

                        hasOutsideNeighbor =
                            true;
                    }
                    else if (
                        !IsFarmCell(
                            x + 1,
                            y,
                            x1,
                            y1,
                            x2,
                            y2,
                            farmShape
                        )
                    )
                    {
                        outsideX =
                            x + 1;

                        outsideY =
                            y;

                        hasOutsideNeighbor =
                            true;
                    }
                    else if (
                        !IsFarmCell(
                            x,
                            y - 1,
                            x1,
                            y1,
                            x2,
                            y2,
                            farmShape
                        )
                    )
                    {
                        outsideX =
                            x;

                        outsideY =
                            y - 1;

                        hasOutsideNeighbor =
                            true;
                    }
                    else if (
                        !IsFarmCell(
                            x,
                            y + 1,
                            x1,
                            y1,
                            x2,
                            y2,
                            farmShape
                        )
                    )
                    {
                        outsideX =
                            x;

                        outsideY =
                            y + 1;

                        hasOutsideNeighbor =
                            true;
                    }


                    if (!hasOutsideNeighbor)
                        continue;


                    Cell outsideCell =
                        Z.GetCell(
                            outsideX,
                            outsideY
                        );


                    if (
                        outsideCell != null &&
                        SubterraneanSites
                            .SubterraneanSitesEPGeometry
                            .IsOpenGeometryCell(
                                outsideCell
                            )
                    )
                    {
                        Cell candidate =
                            Z.GetCell(
                                x,
                                y
                            );


                        if (candidate != null)
                        {
                            gateCandidates.Add(
                                candidate
                            );
                        }
                    }
                }
            }


            //
            // Pick randomly among boundary positions that actually
            // face existing open geometry.
            //
            Cell gateCell =
                null;


            if (
                gateCandidates.Count >
                0
            )
            {
                gateCell =
                    gateCandidates[
                        rng.Next(
                            gateCandidates.Count
                        )
                    ];
            }
            else
            {
                //
                // Fallback: retain the old near-center bottom gate
                // if no boundary cell faces existing open geometry.
                //
                int preferredGateX =
                    x1 +
                    farmWidth / 2;


                for (
                    int distance = 0;
                    distance < farmWidth;
                    distance++
                )
                {
                    int leftX =
                        preferredGateX -
                        distance;

                    int rightX =
                        preferredGateX +
                        distance;


                    if (
                        leftX >= x1 &&
                        leftX <= x2 &&
                        IsFarmBoundaryCell(
                            leftX,
                            y2,
                            x1,
                            y1,
                            x2,
                            y2,
                            farmShape
                        )
                    )
                    {
                        gateCell =
                            Z.GetCell(
                                leftX,
                                y2
                            );

                        break;
                    }


                    if (
                        rightX >= x1 &&
                        rightX <= x2 &&
                        IsFarmBoundaryCell(
                            rightX,
                            y2,
                            x1,
                            y1,
                            x2,
                            y2,
                            farmShape
                        )
                    )
                    {
                        gateCell =
                            Z.GetCell(
                                rightX,
                                y2
                            );

                        break;
                    }
                }
            }


            if (gateCell != null)
            {
                gateCell.ClearWalls();

                gateCell.AddObject(
                    "SubterraneanSitesVillageBrinestalkGate"
                );
            }
 


            //
            // ------------------------------------------------------------
            // FARM HUT
            // ------------------------------------------------------------
            //
            // Six by five, tucked into the upper-left portion of the pen.
            //
            int hutX1 =
                x1 + 2;

            int hutY1 =
                y1 + 2;

            int hutX2 =
                hutX1 + 5;

            int hutY2 =
                hutY1 + 4;


            int hutDoorX =
                hutX2;

            int hutDoorY =
                hutY1 +
                (hutY2 - hutY1) / 2;


            for (
                int x = hutX1;
                x <= hutX2;
                x++
            )
            {
                for (
                    int y = hutY1;
                    y <= hutY2;
                    y++
                )
                {
                    bool border =
                        x == hutX1 ||
                        x == hutX2 ||
                        y == hutY1 ||
                        y == hutY2;


                    if (!border)
                        continue;


                    if (
                        x == hutDoorX &&
                        y == hutDoorY
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


                    cell.ClearWalls();

                    cell.AddObject(
                        "SubterraneanSitesVillageFarmWall"
                    );
                }
            }


            Cell hutDoor =
                Z.GetCell(
                    hutDoorX,
                    hutDoorY
                );


            if (hutDoor != null)
            {
                hutDoor.ClearWalls();

                hutDoor.AddObject(
                    "Door"
                );
            }


            //
            // ------------------------------------------------------------
            // VANILLA FARM POPULATION
            // ------------------------------------------------------------
            //
            List<Location2D> penLocations =
                new List<Location2D>();


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
                    //
                    // Only populate actual farm cells.
                    //
                    if (
                        !IsFarmCell(
                            x,
                            y,
                            x1,
                            y1,
                            x2,
                            y2,
                            farmShape
                        )
                    )
                    {
                        continue;
                    }


                    //
                    // Do not populate fence cells.
                    //
                    if (
                        IsFarmBoundaryCell(
                            x,
                            y,
                            x1,
                            y1,
                            x2,
                            y2,
                            farmShape
                        )
                    )
                    {
                        continue;
                    }


                    //
                    // Keep creatures out of the hut footprint.
                    //
                    if (
                        x >= hutX1 &&
                        x <= hutX2 &&
                        y >= hutY1 &&
                        y <= hutY2
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
                        cell != null &&
                        cell.IsEmptyOfSolid() &&
                        !cell.HasSpawnBlocker()
                    )
                    {
                        penLocations.Add(
                            cell.Location
                        );
                    }
                }
            }


            if (
                penLocations.Count >
                0
            )
            {
                LocationList penArea =
                    new LocationList(
                        penLocations
                    );


                List<GameObject> farmPopulation =
                    PopulationManager.Expand(
                        PopulationManager.Generate(
                            "StiltAnimalPen",
                            "zonetier",
                            Tier.ToString()
                        )
                    );


                if (farmPopulation != null)
                {
                    int placementIndex =
                        0;


                    foreach (
                        GameObject obj
                        in farmPopulation
                    )
                    {
                        if (obj == null)
                            continue;


                        //
                        // Farmer and livestock belong to the Village dimension,
                        // but remain otherwise ordinary vanilla creatures.
                        //
                        if (obj.Brain != null)
                        {
                            SubterraneanSitesVillageInhabitant
                                .Prepare(
                                    obj,
                                    Tier
                                );
                        }


                        ZoneBuilderSandbox
                            .PlaceObjectInArea(
                                Z,
                                penArea,
                                obj,
                                placementIndex,
                                0,
                                null,
                                null,
                                true
                            );

                        //
                        // First merchant-stock-control test:
                        //
                        // StiltAnimalPen creates exactly one farmer/herder
                        // merchant. Livestock do not have
                        // GenericInventoryRestocker, so this naturally selects
                        // the farmer without knowing which farm family rolled.
                        //
                        if (
                            obj.HasPart<
                                XRL.World.Parts.GenericInventoryRestocker
                            >()
                        )
                        {
                            //
                            // Attach the persistent controller BEFORE doing
                            // the initial pass.
                            //
                            // GenericInventoryRestocker may not perform its
                            // first real stock operation until StartTradeEvent.
                            //
                            SubterraneanSites
                                .SubterraneanSitesEPMerchantStockController
                                stockController =
                                    obj.RequirePart<
                                        SubterraneanSites
                                            .SubterraneanSitesEPMerchantStockController
                                    >();


                            stockController.DimensionThemeKey =
                                "Village";

                            stockController.MinimumStock =
                                2;

                            stockController.MaximumStock =
                                3;


                            //
                            // Preserve the useful pre-trade behavior from the
                            // first test: whatever sale inventory already
                            // exists on the farmer is reduced immediately.
                            //
                            SubterraneanSites
                                .SubterraneanSitesEPMerchantStockControl
                                .TrimAndDimensionalizeStock(
                                    obj,
                                    "Village",
                                    2,
                                    3,
                                    rng
                                );
                        }


                        placementIndex++;
                    }
                }
            }


            //
            // ------------------------------------------------------------
            // EMPTY FARM-HUT BASKET
            // ------------------------------------------------------------
            //
            Cell basketCell =
                Z.GetCell(
                    hutX1 + 2,
                    hutY1 + 2
                );


            if (basketCell != null)
            {
                GameObject basket =
                    GameObjectFactory.Factory
                        .CreateObject(
                            "Woven Basket"
                        );


                if (basket != null)
                {
                    basketCell.AddObject(
                        basket
                    );


                    SubterraneanSitesVillageDecorations
                        .EmptyDecorativeInventory(
                            basket
                        );
                }
            }
        }

    }

    /// <summary>
    /// Village Category-5 decoration.
    ///
    /// Each layer chooses one coherent decorative profile:
    ///
    /// - one vanilla Village dwelling/furniture theme
    /// - one walkway material
    /// - two extradimensional tree types
    /// - several Noisegrass patches
    /// - optional small brick-rimmed liquid pool
    ///
    /// Placement is deliberately independent of the Category-4 layout.
    /// The same builder therefore works when Village C5 is combined with
    /// another theme's geometry.
    /// </summary>
    public class SubterraneanSitesVillageDecorations :
        ZoneBuilderSandbox
    {
        public int Tier = 1;

        public int EntranceOnly = 0;


        private const int AnchorExclusionRadius =
            4;


        private static readonly string[]
            WalkwayBlueprints =
            new string[]
            {
                "BrickWalkway",
                "MarbleWalkway",
                "CyanMarbleWalkway",
                "BlackMarbleWalkway",
                "GreyMarbleWalkway"
            };


        private static readonly string[]
            TreeBlueprints =
            new string[]
            {
                "SubterraneanSitesVillageMangrove",
                "SubterraneanSitesVillageDogthorn",
                "SubterraneanSitesVillageZivBough",
                "SubterraneanSitesVillageStarPalm",
                "SubterraneanSitesVillageGlitchwood",
                "SubterraneanSitesVillageIcosahedar"
            };


        //
        // Ordinary, non-rare liquids only.
        //
        private static readonly string[]
            PoolLiquids =
            new string[]
            {
                "blood-1000",
                "sludge-1000",
                "goo-1000",
                "ooze-1000",
                "putrid-1000",
                "convalessence-1000"
            };


        public bool BuildZone(
            Zone Z
        )
        {
            if (Z == null)
                return true;


            Tier =
                Math.Max(
                    1,
                    Math.Min(
                        8,
                        Tier
                    )
                );


            int seed =
                XRLCore.Core.Game.GetWorldSeed(
                    "SubterraneanSites:VillageDecorations:" +
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


            //
            // One intentional-looking profile for this entire layer.
            //
            string dwellingTheme =
                PickDwellingTheme();

            //
            // Village C1 entrance preview:
            //
            // Furniture only. The scar deliberately does not inherit the
            // full Village C5 package of pool, patios, trees, or Noisegrass.
            //
            if (EntranceOnly != 0)
            {
                PlaceFurnitureClusters(
                    Z,
                    anchors,
                    dwellingTheme,
                    null,
                    rng
                );


                return true;
            }

            string walkwayBlueprint =
                WalkwayBlueprints[
                    rng.Next(
                        WalkwayBlueprints.Length
                    )
                ];


            int firstTreeIndex =
                rng.Next(
                    TreeBlueprints.Length
                );


            int secondTreeIndex =
                rng.Next(
                    TreeBlueprints.Length - 1
                );


            if (
                secondTreeIndex >=
                firstTreeIndex
            )
            {
                secondTreeIndex++;
            }


            string firstTreeBlueprint =
                TreeBlueprints[
                    firstTreeIndex
                ];


            string secondTreeBlueprint =
                TreeBlueprints[
                    secondTreeIndex
                ];


            //
            // Large C5 features go first.
            //
            // The pool claims its complete footprint so furnishings and plants
            // cannot later invade it.
            //
            TryPlacePool(
                Z,
                anchors,
                rng
            );


            //
            // Furniture is the main Village identity.
            //
            // All clusters on this layer use the same vanilla dwelling theme.
            //
            PlaceFurnitureClusters(
                Z,
                anchors,
                dwellingTheme,
                walkwayBlueprint,
                rng
            );


            //
            // Build tree groves first.
            //
            // Each grove also grows some Noisegrass around itself, making the
            // vegetation read as a planted/growing cluster rather than unrelated
            // random objects.
            //
            PlaceTreeClusters(
                Z,
                anchors,
                firstTreeBlueprint,
                secondTreeBlueprint,
                rng
            );


            //
            // Then add several independent Noisegrass patches elsewhere in the
            // settlement so Noisegrass is still a recurring Village signature.
            //
            PlaceNoisegrassPatches(
                Z,
                anchors,
                rng
            );


            return true;
        }



        /// <summary>
        /// Use Qud's own Standard village-building distribution, but reject
        /// Torture for this deliberately friendlier extradimensional Village.
        /// </summary>
        private string PickDwellingTheme()
        {
            for (
                int attempt = 0;
                attempt < 6;
                attempt++
            )
            {
                PopulationResult result =
                    PopulationManager.RollOneFrom(
                        "Villages_BuildingTheme_Standard_*Default"
                    );


                if (
                    result == null ||
                    result.Blueprint.IsNullOrEmpty()
                )
                {
                    continue;
                }

                return result.Blueprint;
            }


            return "House";
        }



        private void PlaceFurnitureClusters(
            Zone Z,
            List<Location2D> anchors,
            string dwellingTheme,
            string walkwayBlueprint,
            System.Random rng
        )
        {
            int clusterCount =
                EntranceOnly != 0
                    ? rng.Next(
                        3,
                        6
                    )
                    : rng.Next(
                        6,
                        9
                    );


            HashSet<Cell> usedClusterCells =
                new HashSet<Cell>();


            for (
                int clusterIndex = 0;
                clusterIndex < clusterCount;
                clusterIndex++
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
                                    CanUseAsClusterCell(
                                        Z,
                                        cell,
                                        anchors
                                    ) &&
                                    !usedClusterCells.Contains(
                                        cell
                                    );
                            }
                        );


                if (
                    candidates.Count ==
                    0
                )
                {
                    break;
                }


                int targetSize =
                    rng.Next(
                        16,
                        29
                    );


                HashSet<Cell> cluster =
                    SubterraneanSites
                        .SubterraneanSitesEPPlacement
                        .GrowBestPatch(
                            candidates,
                            targetSize,
                            10,
                            delegate(Cell cell)
                            {
                                return
                                    CanUseAsClusterCell(
                                        Z,
                                        cell,
                                        anchors
                                    ) &&
                                    !usedClusterCells.Contains(
                                        cell
                                    );
                            },
                            rng
                        );


                if (
                    cluster == null ||
                    cluster.Count <
                    6
                )
                {
                    continue;
                }


                foreach (
                    Cell cell
                    in cluster
                )
                {
                    usedClusterCells.Add(
                        cell
                    );
                }


                PlaceFurniturePacket(
                    Z,
                    cluster,
                    anchors,
                    dwellingTheme,
                    rng
                );


                //
                // Most occupied clusters receive a small local patio or work
                // surface. It does not attempt to connect to other clusters.
                //
                if (
                    rng.Next(100) <
                    75
                )
                {
                    PlacePatio(
                        Z,
                        cluster,
                        walkwayBlueprint,
                        rng
                    );
                }
            }
        }



        private void PlaceFurniturePacket(
            Zone Z,
            HashSet<Cell> cluster,
            List<Location2D> anchors,
            string dwellingTheme,
            System.Random rng
        )
        {
            if (
                cluster == null ||
                cluster.Count == 0 ||
                dwellingTheme.IsNullOrEmpty()
            )
            {
                return;
            }


            string table =
                "Villages_BuildingContents_Dwelling_" +
                dwellingTheme +
                "_*Default";


            List<PopulationResult> results =
                null;


            try
            {
                results =
                    PopulationManager.Generate(
                        table,
                        "zonetier",
                        Tier.ToString()
                    );
            }
            catch
            {
                //
                // A missing/changed vanilla table should not abort zonebuild.
                //
                return;
            }


            if (results == null)
                return;


            foreach (
                PopulationResult result
                in results
            )
            {
                if (
                    result == null ||
                    result.Blueprint.IsNullOrEmpty() ||
                    result.Number <= 0
                )
                {
                    continue;
                }


                for (
                    int i = 0;
                    i < result.Number;
                    i++
                )
                {
                    string blueprint =
                        ResolveFurnitureBlueprint(
                            result.Blueprint
                        );


                    if (
                        blueprint.IsNullOrEmpty()
                    )
                    {
                        continue;
                    }


                    //
                    // Reward containers belong to the EP reward system, not
                    // random Village decoration.
                    //
                    if (
                        blueprint.IndexOf(
                            "Chest",
                            StringComparison.OrdinalIgnoreCase
                        ) >=
                        0
                    )
                    {
                        continue;
                    }


                    PlaceFurnitureObject(
                        Z,
                        cluster,
                        anchors,
                        blueprint,
                        result.Hint,
                        rng
                    );
                }
            }
        }



        /// <summary>
        /// Vanilla Village dwelling tables contain semantic placeholders such as
        /// "*Storage,*Furniture". Resolve those through the same dynamic semantic
        /// population mechanism used by VillageOutskirts.
        /// </summary>
        private string ResolveFurnitureBlueprint(
            string blueprint
        )
        {
            if (
                blueprint.IsNullOrEmpty()
            )
            {
                return null;
            }


            if (
                !blueprint.StartsWith(
                    "*"
                )
            )
            {
                return blueprint;
            }


            string semanticTags =
                blueprint.Replace(
                    "*",
                    ""
                );


            PopulationResult resolved =
                null;


            try
            {
                resolved =
                    PopulationManager.RollOneFrom(
                        "DynamicSemanticTable:" +
                        semanticTags +
                        "::" +
                        Tier.ToString()
                    );
            }
            catch
            {
                return null;
            }


            if (
                resolved == null ||
                resolved.Blueprint.IsNullOrEmpty()
            )
            {
                return null;
            }


            return resolved.Blueprint;
        }



        private void PlaceFurnitureObject(
            Zone Z,
            HashSet<Cell> cluster,
            List<Location2D> anchors,
            string blueprint,
            string hint,
            System.Random rng
        )
        {
            Cell destination =
                PickFurnitureCell(
                    Z,
                    cluster,
                    anchors,
                    hint,
                    rng
                );


            if (destination == null)
                return;


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
                return;
            }


            if (obj == null)
                return;


            destination.AddObject(
                obj
            );


            //
            // Vanilla Village furniture may generate ordinary container loot.
            //
            // Village C5 is decorative. Keep the container/furniture object but
            // strip generated contents. Merchant inventories are handled later
            // by the C1/C2 Village population systems, not here.
            //
            EmptyDecorativeInventory(
                obj
            );


            SubterraneanSites
                .SubterraneanSitesEPReservations
                .ClaimCell(
                    Z,
                    destination
                );
        }



        private Cell PickFurnitureCell(
            Zone Z,
            HashSet<Cell> cluster,
            List<Location2D> anchors,
            string hint,
            System.Random rng
        )
        {
            List<Cell> preferred =
                new List<Cell>();


            List<Cell> fallback =
                new List<Cell>();


            foreach (
                Cell cell
                in cluster
            )
            {
                if (
                    !CanPlaceDiscreteDecoration(
                        Z,
                        cell,
                        anchors
                    )
                )
                {
                    continue;
                }


                fallback.Add(
                    cell
                );


                if (
                    MatchesFurnitureHint(
                        Z,
                        cell,
                        hint
                    )
                )
                {
                    preferred.Add(
                        cell
                    );
                }
            }


            if (
                preferred.Count >
                0
            )
            {
                return
                    preferred[
                        rng.Next(
                            preferred.Count
                        )
                    ];
            }


            if (
                fallback.Count >
                0
            )
            {
                return
                    fallback[
                        rng.Next(
                            fallback.Count
                        )
                    ];
            }


            return null;
        }



        /// <summary>
        /// We do not have a full historical-Village PopulationLayout here.
        ///
        /// Preserve the useful semantic portion of the vanilla hints:
        /// wall furniture goes near a wall, corner furniture goes in corners,
        /// and ordinary interior furniture can use any legal cluster cell.
        /// </summary>
        private bool MatchesFurnitureHint(
            Zone Z,
            Cell cell,
            string hint
        )
        {
            if (
                cell == null ||
                hint.IsNullOrEmpty()
            )
            {
                return true;
            }


            int walls =
                CountCardinalStructuralNeighbors(
                    Z,
                    cell
                );


            if (
                hint.IndexOf(
                    "Corner",
                    StringComparison.OrdinalIgnoreCase
                ) >=
                0
            )
            {
                return
                    walls >=
                    2;
            }


            if (
                hint.IndexOf(
                    "Wall",
                    StringComparison.OrdinalIgnoreCase
                ) >=
                0
            )
            {
                return
                    walls >=
                    1;
            }


            return true;
        }



        internal static void EmptyDecorativeInventory(
            GameObject obj
        )
        {
            if (
                obj == null ||
                obj.Inventory == null
            )
            {
                return;
            }


            List<GameObject> contents =
                new List<GameObject>();


            obj.GetInventoryDirect(
                contents
            );


            foreach (
                GameObject item
                in contents
            )
            {
                if (item == null)
                    continue;


                obj.Inventory.RemoveObject(
                    item
                );


                item.Destroy();
            }
        }



        private void PlacePatio(
            Zone Z,
            HashSet<Cell> cluster,
            string walkwayBlueprint,
            System.Random rng
        )
        {
            if (
                cluster == null ||
                cluster.Count == 0 ||
                walkwayBlueprint.IsNullOrEmpty()
            )
            {
                return;
            }


            List<Cell> seeds =
                new List<Cell>(
                    cluster
                );


            if (
                seeds.Count ==
                0
            )
            {
                return;
            }


            Cell seed =
                seeds[
                    rng.Next(
                        seeds.Count
                    )
                ];


            int target =
                rng.Next(
                    5,
                    12
                );


            HashSet<Cell> patio =
                SubterraneanSites
                    .SubterraneanSitesEPPlacement
                    .GrowPatch(
                        seed,
                        target,
                        delegate(Cell cell)
                        {
                            return
                                cell != null &&
                                cluster.Contains(
                                    cell
                                ) &&
                                SubterraneanSites
                                    .SubterraneanSitesEPGeometry
                                    .IsOpenGeometryCell(
                                        cell
                                    ) &&
                                !cell.IsSolid() &&
                                !cell.HasOpenLiquidVolume();
                        },
                        rng
                    );


            foreach (
                Cell cell
                in patio
            )
            {
                if (
                    cell == null ||
                    cell.HasObjectWithBlueprint(
                        walkwayBlueprint
                    )
                )
                {
                    continue;
                }


                cell.AddObject(
                    walkwayBlueprint
                );
            }
        }

        private void PlaceNoisegrassPatches(
            Zone Z,
            List<Location2D> anchors,
            System.Random rng
        )
        {
            //
            // Additional free-standing Noisegrass patches.
            //
            // Tree groves already receive their own local Noisegrass, so these are
            // the independent patches scattered through the rest of the Village.
            //
            int patchCount =
                rng.Next(
                    4,
                    7
                );


            for (
                int patchIndex = 0;
                patchIndex < patchCount;
                patchIndex++
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
                                    CanPlacePlant(
                                        Z,
                                        cell,
                                        anchors,
                                        false
                                    );
                            }
                        );


                if (
                    candidates.Count ==
                    0
                )
                {
                    return;
                }


                int target =
                    rng.Next(
                        4,
                        8
                    );


                HashSet<Cell> patch =
                    SubterraneanSites
                        .SubterraneanSitesEPPlacement
                        .GrowBestPatch(
                            candidates,
                            target,
                            Math.Min(
                                10,
                                candidates.Count
                            ),
                            delegate(Cell cell)
                            {
                                return
                                    CanPlacePlant(
                                        Z,
                                        cell,
                                        anchors,
                                        false
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
                        !CanPlacePlant(
                            Z,
                            cell,
                            anchors,
                            false
                        )
                    )
                    {
                        continue;
                    }


                    cell.AddObject(
                        "Noisegrass"
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



        


        private void PlaceTreeClusters(
            Zone Z,
            List<Location2D> anchors,
            string firstTreeBlueprint,
            string secondTreeBlueprint,
            System.Random rng
        )
        {
            //
            // Earlier tuning used only 2-3 very small groves.
            //
            // Village vegetation should now be visually substantial:
            // 6-8 groves, each containing 1-5 trees.
            //
            int clusterCount =
                rng.Next(
                    6,
                    9
                );


            for (
                int clusterIndex = 0;
                clusterIndex < clusterCount;
                clusterIndex++
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
                                    CanPlacePlant(
                                        Z,
                                        cell,
                                        anchors,
                                        true
                                    );
                            }
                        );


                if (
                    candidates.Count ==
                    0
                )
                {
                    return;
                }


                int targetTrees =
                    rng.Next(
                        1,
                        6
                    );


                //
                // Use the existing shared EP patch-growth helper.
                //
                // This gives us actual adjacent groves instead of independent tree
                // coordinates.
                //
                HashSet<Cell> cluster =
                    SubterraneanSites
                        .SubterraneanSitesEPPlacement
                        .GrowBestPatch(
                            candidates,
                            targetTrees,
                            Math.Min(
                                12,
                                candidates.Count
                            ),
                            delegate(Cell cell)
                            {
                                return
                                    CanPlacePlant(
                                        Z,
                                        cell,
                                        anchors,
                                        true
                                    );
                            },
                            rng
                        );


                if (
                    cluster == null ||
                    cluster.Count == 0
                )
                {
                    continue;
                }


                //
                // Each grove has a dominant tree species.
                //
                // The second selected Village species occasionally appears as an
                // oddball inside that grove.
                //
                bool firstIsDominant =
                    rng.Next(2) ==
                    0;


                string dominantBlueprint =
                    firstIsDominant
                        ? firstTreeBlueprint
                        : secondTreeBlueprint;


                string oddballBlueprint =
                    firstIsDominant
                        ? secondTreeBlueprint
                        : firstTreeBlueprint;


                HashSet<Cell> placedTrees =
                    new HashSet<Cell>();


                foreach (
                    Cell cell
                    in cluster
                )
                {
                    if (
                        cell == null ||
                        !CanPlacePlant(
                            Z,
                            cell,
                            anchors,
                            true
                        )
                    )
                    {
                        continue;
                    }


                    //
                    // Most members of a grove match.
                    // About one in five is the other selected species.
                    //
                    string blueprint =
                        rng.Next(100) <
                        20
                            ? oddballBlueprint
                            : dominantBlueprint;


                    cell.AddObject(
                        blueprint
                    );


                    placedTrees.Add(
                        cell
                    );


                    SubterraneanSites
                        .SubterraneanSitesEPReservations
                        .ClaimCell(
                            Z,
                            cell
                        );
                }


                //
                // Noisegrass likes to gather around the trees.
                //
                // This uses the same shared patch-growth machinery rather than
                // independently sprinkling cells.
                //
                PlaceNoisegrassAroundTrees(
                    Z,
                    anchors,
                    placedTrees,
                    rng
                );
            }
        }


        private void PlaceNoisegrassAroundTrees(
            Zone Z,
            List<Location2D> anchors,
            HashSet<Cell> treeCells,
            System.Random rng
        )
        {
            if (
                treeCells == null ||
                treeCells.Count == 0
            )
            {
                return;
            }


            HashSet<Cell> candidateSet =
                new HashSet<Cell>();


            //
            // Build a local halo around the grove.
            //
            // Radius two gives Noisegrass somewhere to spread without making the
            // tree itself the center of a perfectly regular ring.
            //
            foreach (
                Cell treeCell
                in treeCells
            )
            {
                if (treeCell == null)
                    continue;


                foreach (
                    Cell nearby
                    in treeCell
                        .GetLocalAdjacentCellsCircular(
                            2,
                            true
                        )
                )
                {
                    if (
                        nearby == null ||
                        treeCells.Contains(
                            nearby
                        ) ||
                        !CanPlacePlant(
                            Z,
                            nearby,
                            anchors,
                            false
                        )
                    )
                    {
                        continue;
                    }


                    candidateSet.Add(
                        nearby
                    );
                }
            }


            if (
                candidateSet.Count ==
                0
            )
            {
                return;
            }


            List<Cell> candidates =
                new List<Cell>(
                    candidateSet
                );


            int target =
                rng.Next(
                    2,
                    6
                );


            HashSet<Cell> patch =
                SubterraneanSites
                    .SubterraneanSitesEPPlacement
                    .GrowBestPatch(
                        candidates,
                        target,
                        Math.Min(
                            8,
                            candidates.Count
                        ),
                        delegate(Cell cell)
                        {
                            return
                                candidateSet.Contains(
                                    cell
                                ) &&
                                CanPlacePlant(
                                    Z,
                                    cell,
                                    anchors,
                                    false
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
                    !CanPlacePlant(
                        Z,
                        cell,
                        anchors,
                        false
                    )
                )
                {
                    continue;
                }


                cell.AddObject(
                    "Noisegrass"
                );


                SubterraneanSites
                    .SubterraneanSitesEPReservations
                    .ClaimCell(
                        Z,
                        cell
                    );
            }
        }



        



        private bool CanPlacePlant(
            Zone Z,
            Cell cell,
            List<Location2D> anchors,
            bool requireBroadClearance
        )
        {
            if (
                !CanPlaceDiscreteDecoration(
                    Z,
                    cell,
                    anchors
                )
            )
            {
                return false;
            }


            if (
                !requireBroadClearance
            )
            {
                return true;
            }


            //
            // Trees can be solid/occluding. Only grow them in genuinely broad
            // spaces, not in the one-cell circulation network.
            //
            return
                SubterraneanSites
                    .SubterraneanSitesEPPlacement
                    .HasBroadOpenClearance(
                        Z,
                        cell,
                        delegate(Cell nearby)
                        {
                            return
                                nearby != null &&
                                nearby.IsReachable() &&
                                SubterraneanSites
                                    .SubterraneanSitesEPGeometry
                                    .IsOpenGeometryCell(
                                        nearby
                                    ) &&
                                !nearby.IsSolid() &&
                                !nearby.HasSpawnBlocker() &&
                                !SubterraneanSites
                                    .SubterraneanSitesEPReservations
                                    .IsClaimed(
                                        Z,
                                        nearby
                                    );
                        },
                        1
                    );
        }

        private bool CanUseAsClusterCell(
            Zone Z,
            Cell cell,
            List<Location2D> anchors
        )
        {
            if (
                !CanPlaceDiscreteDecoration(
                    Z,
                    cell,
                    anchors
                )
            )
            {
                return false;
            }


            //
            // The entrance scar has no underground C4 geometry to test.
            // Being a legal scar cell is sufficient.
            //
            if (EntranceOnly != 0)
                return true;


            //
            // Underground, do not treat narrow one-cell corridors as rooms.
            //
            return
                CountCardinalOpenNeighbors(
                    Z,
                    cell
                ) >=
                2;
        }

        private bool CanPlaceDiscreteDecoration(
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
                //
                // Entrance furniture belongs strictly to the dimensional scar,
                // outside the protected central hole footprint.
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
                //
                // Normal C5 furniture uses whichever underground C4 geometry won.
                //
                if (
                    !cell.IsReachable() ||
                    !cell.IsSpawnable() ||
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
                            AnchorExclusionRadius
                        )
                )
                {
                    return false;
                }
            }


            //
            // Shared placement restrictions.
            //
            if (
                cell.IsSolid() ||
                cell.HasSpawnBlocker() ||
                cell.HasOpenLiquidVolume() ||
                SubterraneanSites
                    .SubterraneanSitesEPReservations
                    .IsClaimed(
                        Z,
                        cell
                    ) ||
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



        private int CountCardinalOpenNeighbors(
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


            CountOpenNeighbor(
                Z,
                cell.X - 1,
                cell.Y,
                ref count
            );


            CountOpenNeighbor(
                Z,
                cell.X + 1,
                cell.Y,
                ref count
            );


            CountOpenNeighbor(
                Z,
                cell.X,
                cell.Y - 1,
                ref count
            );


            CountOpenNeighbor(
                Z,
                cell.X,
                cell.Y + 1,
                ref count
            );


            return count;
        }



        private void CountOpenNeighbor(
            Zone Z,
            int x,
            int y,
            ref int count
        )
        {
            Cell neighbor =
                Z.GetCell(
                    x,
                    y
                );


            if (
                neighbor != null &&
                SubterraneanSites
                    .SubterraneanSitesEPGeometry
                    .IsOpenGeometryCell(
                        neighbor
                    ) &&
                !neighbor.IsSolid()
            )
            {
                count++;
            }
        }



        private int CountCardinalStructuralNeighbors(
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
                IsStructuralCell(
                    Z.GetCell(
                        cell.X - 1,
                        cell.Y
                    )
                )
            )
            {
                count++;
            }


            if (
                IsStructuralCell(
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
                IsStructuralCell(
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
                IsStructuralCell(
                    Z.GetCell(
                        cell.X,
                        cell.Y + 1
                    )
                )
            )
            {
                count++;
            }


            return count;
        }



        private bool IsStructuralCell(
            Cell cell
        )
        {
            return
                SubterraneanSites
                    .SubterraneanSitesEPGeometry
                    .IsAnyPlaceholder(
                        cell
                    );
        }



        private void TryPlacePool(
            Zone Z,
            List<Location2D> anchors,
            System.Random rng
        )
        {
            if (
                rng.Next(100) >=
                40
            )
            {
                return;
            }


            List<Cell> centers =
                new List<Cell>();


            for (
                int x = 2;
                x < Z.Width - 2;
                x++
            )
            {
                for (
                    int y = 2;
                    y < Z.Height - 2;
                    y++
                )
                {
                    if (
                        PoolFootprintIsAvailable(
                            Z,
                            x,
                            y,
                            anchors
                        )
                    )
                    {
                        centers.Add(
                            Z.GetCell(
                                x,
                                y
                            )
                        );
                    }
                }
            }


            if (
                centers.Count ==
                0
            )
            {
                return;
            }


            Cell center =
                centers[
                    rng.Next(
                        centers.Count
                    )
                ];


            if (center == null)
                return;


            string liquid =
                PoolLiquids[
                    rng.Next(
                        PoolLiquids.Length
                    )
                ];


            //
            // Reserve the complete five-by-five feature first.
            //
            SubterraneanSites
                .SubterraneanSitesEPReservations
                .ClaimRectangle(
                    Z,
                    center.X - 2,
                    center.Y - 2,
                    center.X + 2,
                    center.Y + 2
                );


            for (
                int dx = -2;
                dx <= 2;
                dx++
            )
            {
                for (
                    int dy = -2;
                    dy <= 2;
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


                    bool rim =
                        Math.Abs(dx) ==
                            2 ||
                        Math.Abs(dy) ==
                            2;


                    if (rim)
                    {
                        if (
                            !cell.HasObjectWithBlueprint(
                                "BrickWalkway"
                            )
                        )
                        {
                            cell.AddObject(
                                "BrickWalkway"
                            );
                        }


                        continue;
                    }


                    GameObject pool =
                        GameObjectFactory.Factory
                            .CreateObject(
                                "WaterPool"
                            );


                    if (
                        pool == null ||
                        pool.LiquidVolume == null
                    )
                    {
                        if (pool != null)
                            pool.Destroy();


                        continue;
                    }


                    cell.AddObject(
                        pool
                    );


                    pool.LiquidVolume
                        .InitialLiquid =
                            liquid;


                    pool.LiquidVolume.Volume =
                        1000;


                    pool.LiquidVolume.Update();
                }
            }
        }



        private bool PoolFootprintIsAvailable(
            Zone Z,
            int centerX,
            int centerY,
            List<Location2D> anchors
        )
        {
            int x1 =
                centerX - 2;


            int y1 =
                centerY - 2;


            int x2 =
                centerX + 2;


            int y2 =
                centerY + 2;


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


                    if (
                        cell == null ||
                        !cell.IsReachable() ||
                        !SubterraneanSites
                            .SubterraneanSitesEPGeometry
                            .IsOpenGeometryCell(
                                cell
                            ) ||
                        cell.IsSolid() ||
                        cell.HasSpawnBlocker() ||
                        cell.HasOpenLiquidVolume() ||
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
                        SubterraneanSites
                            .SubterraneanSitesEPPlacement
                            .IsNearAnyAnchor(
                                x,
                                y,
                                anchors,
                                AnchorExclusionRadius
                            )
                    )
                    {
                        return false;
                    }
                }
            }


            return true;
        }
    }

    
    /// <summary>
    /// Village Category-4 floor.
    ///
    /// Shared Village floor treatment applied across all four layout types.
    /// </summary>
    public class SubterraneanSitesVillageFloor :
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
                            .SubterraneanSitesEPVillageTheme
                            .CreateFloorSpec(Z),
                        EntranceOnly != 0
                    );
        }
    }



    /// <summary>
    /// Village Category-3 materialization.
    ///
    /// Each zone selects one paired exotic material set.
    /// Solid structural mass receives the bulk material and exposed internal
    /// boundaries receive the corresponding accent material.
    ///
    /// Material selection is deterministic per zone.
    /// </summary>
    public class SubterraneanSitesVillageMaterials :
        ZoneBuilderSandbox
    {

        public int SealedBorderWidth = 1;

        private static readonly string[]
            BulkWallBlueprints =
            new string[]
            {
                "SubterraneanSitesVillageJasperWall",
                "SubterraneanSitesVillageAmethystWall",
                "SubterraneanSitesVillageSapphireWall",
                "SubterraneanSitesVillageEmeraldWall",
                "SubterraneanSitesVillageTopazWall"
            };


        private static readonly string[]
            InnerWallBlueprints =
            new string[]
            {
                "SubterraneanSitesMushroomWallOrange",
                "Star Orchid Marble",
                "SubterraneanSitesVillagePeridotWall",
                "Coral Rag",
                "Oolite"
            };


        public bool BuildZone(
            Zone Z
        )
        {
            if (Z == null)
                return true;


            if (SealedBorderWidth < 0)
                SealedBorderWidth = 0;


            int seed =
                XRLCore.Core.Game.GetWorldSeed(
                    "SubterraneanSites:VillageMaterials:" +
                    Z.ZoneID
                );


            System.Random rng =
                new System.Random(
                    seed
                );


            int materialIndex =
                rng.Next(
                    InnerWallBlueprints.Length
                );


            string bulkWallBlueprint =
                BulkWallBlueprints[
                    materialIndex
                ];


            string innerWallBlueprint =
                InnerWallBlueprints[
                    materialIndex
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
                            ? innerWallBlueprint
                            : bulkWallBlueprint;


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
                x >=
                    Z.Width -
                    SealedBorderWidth ||
                y >=
                    Z.Height -
                    SealedBorderWidth;
        }
    }

        /// <summary>
    /// Shared doorway pass for all Village Category-4 layouts.
    ///
    /// The four Village layouts build their geometry differently, so this
    /// pass deliberately ignores layout-specific room metadata.
    ///
    /// Instead it examines the final abstract geometry and recognizes:
    ///
    ///     open space
    ///         |
    ///       doorway
    ///         |
    ///     open space
    ///
    /// with structural walls flanking the doorway.
    ///
    /// A candidate must also widen into a larger space on at least one side.
    /// This prevents long one-cell cave corridors from being mistaken for a
    /// sequence of doorway locations.
    ///
    /// Both one-cell and two-cell-wide openings are supported.
    /// </summary>
    internal static class SubterraneanSitesVillageDoorPlacement
    {
        private const string DoorBlueprint =
            "Door";


        //
        // Village doors should be noticeable but sparse.
        //
        // These numbers count actual door objects, so one double doorway
        // consumes two of the available door objects.
        //
        private const int MinimumDoorObjects =
            6;

        private const int MaximumDoorObjects =
            10;


        //
        // Keep separate doorways from clustering together.
        //
        private const int MinimumOpeningSpacing =
            5;


        //
        // Do not clutter stair / vertical-transition landings.
        //
        private const int AnchorClearance =
            2;


        private sealed class DoorOpeningCandidate
        {
            public int X1;
            public int Y1;

            public int X2 = -1;
            public int Y2 = -1;


            public bool IsDouble
            {
                get
                {
                    return
                        X2 >= 0 &&
                        Y2 >= 0;
                }
            }


            public int DoorCount
            {
                get
                {
                    return
                        IsDouble
                            ? 2
                            : 1;
                }
            }


            public int CenterX
            {
                get
                {
                    return
                        IsDouble
                            ? (
                                X1 +
                                X2
                              ) / 2
                            : X1;
                }
            }


            public int CenterY
            {
                get
                {
                    return
                        IsDouble
                            ? (
                                Y1 +
                                Y2
                              ) / 2
                            : Y1;
                }
            }
        }



        internal static void PlaceDoors(
            Zone Z
        )
        {
            if (Z == null)
                return;


            int seed =
                XRLCore.Core.Game.GetWorldSeed(
                    "SubterraneanSites:VillageDoors:" +
                    Z.ZoneID
                );


            System.Random rng =
                new System.Random(
                    seed
                );


            List<DoorOpeningCandidate> candidates =
                new List<DoorOpeningCandidate>();


            //
            // Geometry is complete when this runs.
            //
            // Scan only the interior because candidate validation needs
            // neighboring cells in every direction.
            //
            for (
                int x = 1;
                x < Z.Width - 1;
                x++
            )
            {
                for (
                    int y = 1;
                    y < Z.Height - 1;
                    y++
                )
                {
                    if (
                        !IsOpen(
                            Z,
                            x,
                            y
                        )
                    )
                    {
                        continue;
                    }


                    //
                    // One-cell doorway with north/south travel.
                    //
                    if (
                        IsSingleVerticalOpening(
                            Z,
                            x,
                            y
                        )
                    )
                    {
                        candidates.Add(
                            new DoorOpeningCandidate
                            {
                                X1 = x,
                                Y1 = y
                            }
                        );
                    }


                    //
                    // One-cell doorway with east/west travel.
                    //
                    if (
                        IsSingleHorizontalOpening(
                            Z,
                            x,
                            y
                        )
                    )
                    {
                        candidates.Add(
                            new DoorOpeningCandidate
                            {
                                X1 = x,
                                Y1 = y
                            }
                        );
                    }


                    //
                    // Two-cell-wide opening with north/south travel.
                    //
                    //
                    //      ####
                    //       DD
                    //      ....
                    //
                    // Scan only toward +X so the same opening is not
                    // discovered twice.
                    //
                    if (
                        x + 2 <
                            Z.Width &&
                        IsDoubleVerticalOpening(
                            Z,
                            x,
                            y
                        )
                    )
                    {
                        candidates.Add(
                            new DoorOpeningCandidate
                            {
                                X1 = x,
                                Y1 = y,
                                X2 = x + 1,
                                Y2 = y
                            }
                        );
                    }


                    //
                    // Two-cell-wide opening with east/west travel.
                    //
                    // Scan only toward +Y.
                    //
                    if (
                        y + 2 <
                            Z.Height &&
                        IsDoubleHorizontalOpening(
                            Z,
                            x,
                            y
                        )
                    )
                    {
                        candidates.Add(
                            new DoorOpeningCandidate
                            {
                                X1 = x,
                                Y1 = y,
                                X2 = x,
                                Y2 = y + 1
                            }
                        );
                    }
                }
            }


            if (
                candidates.Count ==
                0
            )
            {
                return;
            }


            Shuffle(
                candidates,
                rng
            );


            List<DoorOpeningCandidate> selected =
                new List<DoorOpeningCandidate>();


            List<Location2D> anchors =
                SubterraneanSites
                    .SubterraneanSitesEPVerticalTransitions
                    .GetVerticalAnchors(
                        Z.ZoneID
                    );


            int targetDoorObjects =
                rng.Next(
                    MinimumDoorObjects,
                    MaximumDoorObjects + 1
                );


            int placedDoorObjects =
                0;


            foreach (
                DoorOpeningCandidate candidate
                in candidates
            )
            {
                if (
                    placedDoorObjects >=
                    targetDoorObjects
                )
                {
                    break;
                }


                if (
                    placedDoorObjects +
                    candidate.DoorCount >
                    targetDoorObjects
                )
                {
                    continue;
                }


                if (
                    IsNearVerticalAnchor(
                        candidate,
                        anchors
                    )
                )
                {
                    continue;
                }


                if (
                    IsTooCloseToSelectedOpening(
                        candidate,
                        selected
                    )
                )
                {
                    continue;
                }


                if (
                    !PlaceCandidate(
                        Z,
                        candidate
                    )
                )
                {
                    continue;
                }


                selected.Add(
                    candidate
                );


                placedDoorObjects +=
                    candidate.DoorCount;
            }
        }



        private static bool IsSingleVerticalOpening(
            Zone Z,
            int x,
            int y
        )
        {
            if (
                !IsOpen(Z, x, y - 1) ||
                !IsOpen(Z, x, y + 1) ||
                !IsStructuralWall(Z, x - 1, y) ||
                !IsStructuralWall(Z, x + 1, y)
            )
            {
                return false;
            }


            //
            // At least one end must widen into a room / plaza.
            //
            // Without this condition, every cell of a one-wide vertical
            // tunnel would look like a doorway.
            //
            return
                IsOpen(Z, x - 1, y - 1) ||
                IsOpen(Z, x + 1, y - 1) ||
                IsOpen(Z, x - 1, y + 1) ||
                IsOpen(Z, x + 1, y + 1);
        }



        private static bool IsSingleHorizontalOpening(
            Zone Z,
            int x,
            int y
        )
        {
            if (
                !IsOpen(Z, x - 1, y) ||
                !IsOpen(Z, x + 1, y) ||
                !IsStructuralWall(Z, x, y - 1) ||
                !IsStructuralWall(Z, x, y + 1)
            )
            {
                return false;
            }


            //
            // Again, require widening on at least one side so a long
            // horizontal one-cell hallway does not become a row of doors.
            //
            return
                IsOpen(Z, x - 1, y - 1) ||
                IsOpen(Z, x - 1, y + 1) ||
                IsOpen(Z, x + 1, y - 1) ||
                IsOpen(Z, x + 1, y + 1);
        }



        private static bool IsDoubleVerticalOpening(
            Zone Z,
            int x,
            int y
        )
        {
            if (
                !IsOpen(Z, x, y) ||
                !IsOpen(Z, x + 1, y) ||

                !IsOpen(Z, x, y - 1) ||
                !IsOpen(Z, x + 1, y - 1) ||

                !IsOpen(Z, x, y + 1) ||
                !IsOpen(Z, x + 1, y + 1) ||

                !IsStructuralWall(Z, x - 1, y) ||
                !IsStructuralWall(Z, x + 2, y)
            )
            {
                return false;
            }


            //
            // A genuine two-wide doorway widens beyond the two-cell strip
            // on at least one side.
            //
            // A straight two-wide corridor does not.
            //
            return
                IsOpen(Z, x - 1, y - 1) ||
                IsOpen(Z, x + 2, y - 1) ||
                IsOpen(Z, x - 1, y + 1) ||
                IsOpen(Z, x + 2, y + 1);
        }



        private static bool IsDoubleHorizontalOpening(
            Zone Z,
            int x,
            int y
        )
        {
            if (
                !IsOpen(Z, x, y) ||
                !IsOpen(Z, x, y + 1) ||

                !IsOpen(Z, x - 1, y) ||
                !IsOpen(Z, x - 1, y + 1) ||

                !IsOpen(Z, x + 1, y) ||
                !IsOpen(Z, x + 1, y + 1) ||

                !IsStructuralWall(Z, x, y - 1) ||
                !IsStructuralWall(Z, x, y + 2)
            )
            {
                return false;
            }


            return
                IsOpen(Z, x - 1, y - 1) ||
                IsOpen(Z, x - 1, y + 2) ||
                IsOpen(Z, x + 1, y - 1) ||
                IsOpen(Z, x + 1, y + 2);
        }



        private static bool PlaceCandidate(
            Zone Z,
            DoorOpeningCandidate candidate
        )
        {
            if (
                Z == null ||
                candidate == null
            )
            {
                return false;
            }


            Cell first =
                Z.GetCell(
                    candidate.X1,
                    candidate.Y1
                );


            if (
                !CanReceiveDoor(
                    first
                )
            )
            {
                return false;
            }


            Cell second =
                null;


            if (candidate.IsDouble)
            {
                second =
                    Z.GetCell(
                        candidate.X2,
                        candidate.Y2
                    );


                if (
                    !CanReceiveDoor(
                        second
                    )
                )
                {
                    return false;
                }
            }


            first.AddObject(
                DoorBlueprint
            );


            if (
                second !=
                null
            )
            {
                second.AddObject(
                    DoorBlueprint
                );
            }


            return true;
        }



        private static bool CanReceiveDoor(
            Cell cell
        )
        {
            return
                IsOpen(cell) &&
                cell.GetFirstObjectWithPart(
                    "Door"
                ) ==
                null;
        }



        private static bool IsOpen(
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
                y >= Z.Height
            )
            {
                return false;
            }


            return
                IsOpen(
                    Z.GetCell(
                        x,
                        y
                    )
                );
        }



        private static bool IsOpen(
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



        private static bool IsStructuralWall(
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
                y >= Z.Height
            )
            {
                return false;
            }


            Cell cell =
                Z.GetCell(
                    x,
                    y
                );


            return
                SubterraneanSites
                    .SubterraneanSitesEPGeometry
                    .IsSolidPlaceholder(
                        cell
                    );
        }



        private static bool IsNearVerticalAnchor(
            DoorOpeningCandidate candidate,
            List<Location2D> anchors
        )
        {
            if (
                candidate == null ||
                anchors == null
            )
            {
                return false;
            }


            foreach (
                Location2D anchor
                in anchors
            )
            {
                if (anchor == null)
                    continue;


                if (
                    IsNear(
                        candidate.X1,
                        candidate.Y1,
                        anchor.X,
                        anchor.Y,
                        AnchorClearance
                    )
                )
                {
                    return true;
                }


                if (
                    candidate.IsDouble &&
                    IsNear(
                        candidate.X2,
                        candidate.Y2,
                        anchor.X,
                        anchor.Y,
                        AnchorClearance
                    )
                )
                {
                    return true;
                }
            }


            return false;
        }



        private static bool IsTooCloseToSelectedOpening(
            DoorOpeningCandidate candidate,
            List<DoorOpeningCandidate> selected
        )
        {
            if (
                candidate == null ||
                selected == null
            )
            {
                return false;
            }


            foreach (
                DoorOpeningCandidate existing
                in selected
            )
            {
                if (existing == null)
                    continue;


                int dx =
                    Math.Abs(
                        candidate.CenterX -
                        existing.CenterX
                    );


                int dy =
                    Math.Abs(
                        candidate.CenterY -
                        existing.CenterY
                    );


                if (
                    Math.Max(
                        dx,
                        dy
                    ) <
                    MinimumOpeningSpacing
                )
                {
                    return true;
                }
            }


            return false;
        }



        private static bool IsNear(
            int x1,
            int y1,
            int x2,
            int y2,
            int radius
        )
        {
            return
                Math.Max(
                    Math.Abs(
                        x1 -
                        x2
                    ),
                    Math.Abs(
                        y1 -
                        y2
                    )
                ) <=
                radius;
        }



        private static void Shuffle(
            List<DoorOpeningCandidate> candidates,
            System.Random rng
        )
        {
            if (
                candidates == null ||
                rng == null
            )
            {
                return;
            }


            for (
                int i =
                    candidates.Count - 1;
                i > 0;
                i--
            )
            {
                int j =
                    rng.Next(
                        i + 1
                    );


                DoorOpeningCandidate temporary =
                    candidates[i];


                candidates[i] =
                    candidates[j];


                candidates[j] =
                    temporary;
            }
        }
    }



    /// <summary>
    /// Village Category-4 prototype: courtyard-ring settlement.
    ///
    /// Four large inhabited structures surround a central open court.
    /// Each structure has a broad opening facing the court.
    ///
    /// The surrounding zone remains open settlement space, while the absolute
    /// zone perimeter remains sealed structural mass.
    ///
    /// This is intentionally the simplest Village layout prototype. Actual
    /// doors, furniture, villagers, farms and secondary structures are added
    /// in later passes.
    /// </summary>
    public class SubterraneanSitesVillageRingLayout :
        ZoneBuilderSandbox
    {
        public int SealedBorderWidth = 1;

        public int AnchorRadius = 2;

        public int DoorWidth = 2;


        public bool BuildZone(
            Zone Z
        )
        {
            if (Z == null)
                return true;


            ClampSettings();


            int seed =
                XRLCore.Core.Game.GetWorldSeed(
                    "SubterraneanSites:VillageRingLayout:" +
                    Z.ZoneID
                );


            System.Random rng =
                new System.Random(
                    seed
                );


            //
            // Start with a broad open settlement field.
            //
            CarveRectangle(
                Z,
                SealedBorderWidth,
                SealedBorderWidth,
                Z.Width - SealedBorderWidth - 1,
                Z.Height - SealedBorderWidth - 1
            );


            //
            // Slightly off-center communal court.
            //
            int centerX =
                Z.Width / 2 +
                rng.Next(-4, 5);

            int centerY =
                Z.Height / 2 +
                rng.Next(-2, 3);


            int courtHalfWidth =
                rng.Next(9, 13);

            int courtHalfHeight =
                rng.Next(3, 5);


            int courtX1 =
                centerX - courtHalfWidth;

            int courtX2 =
                centerX + courtHalfWidth;

            int courtY1 =
                centerY - courtHalfHeight;

            int courtY2 =
                centerY + courtHalfHeight;


            Rect2D court =
                new Rect2D(
                    courtX1,
                    courtY1,
                    courtX2,
                    courtY2
                );


            bool northTouchesEdge =
                rng.Next(100) < 30;

            bool southTouchesEdge =
                !northTouchesEdge &&
                rng.Next(100) < 25;

            bool westTouchesEdge =
                !northTouchesEdge &&
                !southTouchesEdge &&
                rng.Next(100) < 20;

            bool eastTouchesEdge =
                !northTouchesEdge &&
                !southTouchesEdge &&
                !westTouchesEdge &&
                rng.Next(100) < 20;


            //
            // Four main structures around the central court.
            //
            Rect2D north =
                CreateNorthBuilding(
                    Z,
                    court,
                    rng,
                    northTouchesEdge
                );

            Rect2D south =
                CreateSouthBuilding(
                    Z,
                    court,
                    rng,
                    southTouchesEdge
                );

            Rect2D west =
                CreateWestBuilding(
                    Z,
                    court,
                    rng,
                    westTouchesEdge
                );

            Rect2D east =
                CreateEastBuilding(
                    Z,
                    court,
                    rng,
                    eastTouchesEdge
                );


            List<Rect2D> buildings =
                new List<Rect2D>();


            if (north != null)
                buildings.Add(north);

            if (south != null)
                buildings.Add(south);

            if (west != null)
                buildings.Add(west);

            if (east != null)
                buildings.Add(east);


            //
            // Optional detached outbuildings.
            //
            int outbuildingCount =
                rng.Next(0, 3);

            for (
                int i = 0;
                i < outbuildingCount;
                i++
            )
            {
                Rect2D? outbuilding =
                    TryCreateOutbuilding(
                        Z,
                        court,
                        buildings,
                        rng
                    );

                if (outbuilding.HasValue)
                {
                    buildings.Add(
                        outbuilding.Value
                    );
                }
            }


            foreach (
                Rect2D building
                in buildings
            )
            {
                BuildHollowBuilding(
                    Z,
                    building
                );

                MaybeAddSubdivision(
                    Z,
                    building,
                    rng
                );
            }


            //
            // Main inward-facing entrances for the four core buildings.
            //
            if (north != null)
            {
                PunchHorizontalDoor(
                    Z,
                    north.y2,
                    Clamp(
                        centerX + rng.Next(-3, 4),
                        north.x1 + 1,
                        north.x2 - 1
                    )
                );
            }

            if (south != null)
            {
                PunchHorizontalDoor(
                    Z,
                    south.y1,
                    Clamp(
                        centerX + rng.Next(-3, 4),
                        south.x1 + 1,
                        south.x2 - 1
                    )
                );
            }

            if (west != null)
            {
                PunchVerticalDoor(
                    Z,
                    west.x2,
                    Clamp(
                        centerY + rng.Next(-2, 3),
                        west.y1 + 1,
                        west.y2 - 1
                    )
                );
            }

            if (east != null)
            {
                PunchVerticalDoor(
                    Z,
                    east.x1,
                    Clamp(
                        centerY + rng.Next(-2, 3),
                        east.y1 + 1,
                        east.y2 - 1
                    )
                );
            }


            //
            // Give detached outbuildings a simple door facing roughly inward.
            //
            for (
                int i = 4;
                i < buildings.Count;
                i++
            )
            {
                AddOutbuildingDoorTowardCourt(
                    Z,
                    buildings[i],
                    centerX,
                    centerY
                );
            }


            //
            // 1-2 additional exits from the inner court to the outside.
            // These deliberately create less planned-looking circulation.
            //
            List<string> exitCandidates =
                new List<string>();

            if (north != null)
                exitCandidates.Add("North");

            if (south != null)
                exitCandidates.Add("South");

            if (west != null)
                exitCandidates.Add("West");

            if (east != null)
                exitCandidates.Add("East");


            int extraExitCount =
                Math.Min(
                    exitCandidates.Count,
                    rng.Next(1, 3)
                );


            for (
                int i = 0;
                i < extraExitCount;
                i++
            )
            {
                int index =
                    rng.Next(
                        exitCandidates.Count
                    );

                string side =
                    exitCandidates[index];

                exitCandidates.RemoveAt(
                    index
                );

                if (side == "North")
                {
                    AddNorthThroughExit(
                        Z,
                        north,
                        centerX,
                        rng
                    );
                }
                else if (side == "South")
                {
                    AddSouthThroughExit(
                        Z,
                        south,
                        centerX,
                        rng
                    );
                }
                else if (side == "West")
                {
                    AddWestThroughExit(
                        Z,
                        west,
                        centerY,
                        rng
                    );
                }
                else if (side == "East")
                {
                    AddEastThroughExit(
                        Z,
                        east,
                        centerY,
                        rng
                    );
                }
            }


            //
            // Vertical transition anchors always get a footprint and route.
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

                    CarveOrthogonalConnection(
                        Z,
                        anchor.X,
                        anchor.Y,
                        centerX,
                        centerY
                    );
                }
            }


            ForceSealedBorder(
                Z
            );

            SubterraneanSitesVillageDoorPlacement
            .PlaceDoors(
                Z
            );

            Z.ClearReachableMap();


            return true;
        }


        private void ClampSettings()
        {
            if (SealedBorderWidth < 1)
                SealedBorderWidth = 1;


            if (AnchorRadius < 1)
                AnchorRadius = 1;


            if (DoorWidth < 1)
                DoorWidth = 1;
        }


        private Rect2D CreateNorthBuilding(
            Zone Z,
            Rect2D court,
            System.Random rng,
            bool touchEdge
        )
        {
            int width =
                rng.Next(18, 28);

            int height =
                rng.Next(4, 7);

            int x1 =
                Clamp(
                    court.Center.x - width / 2 + rng.Next(-5, 6),
                    SealedBorderWidth + 1,
                    Z.Width - SealedBorderWidth - width - 2
                );

            int x2 =
                x1 + width - 1;

            int y2 =
                court.y1 - rng.Next(2, 5);

            int y1 =
                y2 - height + 1;

            if (touchEdge)
            {
                y1 = SealedBorderWidth;
                y2 = y1 + height - 1;
            }

            return
                NormalizeRect(
                    Z,
                    x1,
                    y1,
                    x2,
                    y2
                );
        }


        private Rect2D CreateSouthBuilding(
            Zone Z,
            Rect2D court,
            System.Random rng,
            bool touchEdge
        )
        {
            int width =
                rng.Next(18, 28);

            int height =
                rng.Next(4, 7);

            int x1 =
                Clamp(
                    court.Center.x - width / 2 + rng.Next(-5, 6),
                    SealedBorderWidth + 1,
                    Z.Width - SealedBorderWidth - width - 2
                );

            int x2 =
                x1 + width - 1;

            int y1 =
                court.y2 + rng.Next(2, 5);

            int y2 =
                y1 + height - 1;

            if (touchEdge)
            {
                y2 =
                    Z.Height -
                    SealedBorderWidth -
                    1;

                y1 =
                    y2 - height + 1;
            }

            return
                NormalizeRect(
                    Z,
                    x1,
                    y1,
                    x2,
                    y2
                );
        }


        private Rect2D CreateWestBuilding(
            Zone Z,
            Rect2D court,
            System.Random rng,
            bool touchEdge
        )
        {
            int width =
                rng.Next(7, 12);

            int height =
                rng.Next(8, 14);

            int y1 =
                Clamp(
                    court.Center.y - height / 2 + rng.Next(-3, 4),
                    SealedBorderWidth + 1,
                    Z.Height - SealedBorderWidth - height - 2
                );

            int y2 =
                y1 + height - 1;

            int x2 =
                court.x1 - rng.Next(2, 5);

            int x1 =
                x2 - width + 1;

            if (touchEdge)
            {
                x1 = SealedBorderWidth;
                x2 = x1 + width - 1;
            }

            return
                NormalizeRect(
                    Z,
                    x1,
                    y1,
                    x2,
                    y2
                );
        }


        private Rect2D CreateEastBuilding(
            Zone Z,
            Rect2D court,
            System.Random rng,
            bool touchEdge
        )
        {
            int width =
                rng.Next(7, 12);

            int height =
                rng.Next(8, 14);

            int y1 =
                Clamp(
                    court.Center.y - height / 2 + rng.Next(-3, 4),
                    SealedBorderWidth + 1,
                    Z.Height - SealedBorderWidth - height - 2
                );

            int y2 =
                y1 + height - 1;

            int x1 =
                court.x2 + rng.Next(2, 5);

            int x2 =
                x1 + width - 1;

            if (touchEdge)
            {
                x2 =
                    Z.Width -
                    SealedBorderWidth -
                    1;

                x1 =
                    x2 - width + 1;
            }

            return
                NormalizeRect(
                    Z,
                    x1,
                    y1,
                    x2,
                    y2
                );
        }


        private Rect2D? TryCreateOutbuilding(
            Zone Z,
            Rect2D court,
            List<Rect2D> existing,
            System.Random rng
        )
        {
            for (
                int tries = 0;
                tries < 30;
                tries++
            )
            {
                int width =
                    rng.Next(6, 11);

                int height =
                    rng.Next(4, 7);

                int x1 =
                    rng.Next(
                        SealedBorderWidth + 1,
                        Z.Width - SealedBorderWidth - width - 1
                    );

                int y1 =
                    rng.Next(
                        SealedBorderWidth + 1,
                        Z.Height - SealedBorderWidth - height - 1
                    );

                Rect2D candidate =
                    NormalizeRect(
                        Z,
                        x1,
                        y1,
                        x1 + width - 1,
                        y1 + height - 1
                    );

                if (
                    IntersectsExpanded(
                        candidate,
                        court,
                        3
                    )
                )
                {
                    continue;
                }

                bool overlaps =
                    false;

                foreach (
                    Rect2D other
                    in existing
                )
                {
                    if (
                        IntersectsExpanded(
                            candidate,
                            other,
                            2
                        )
                    )
                    {
                        overlaps = true;
                        break;
                    }
                }

                if (overlaps)
                    continue;

                return candidate;
            }

            return null;
        }


        private bool IntersectsExpanded(
            Rect2D a,
            Rect2D b,
            int pad
        )
        {
            if (
                a == null ||
                b == null
            )
            {
                return false;
            }

            return !(
                a.x2 < b.x1 - pad ||
                a.x1 > b.x2 + pad ||
                a.y2 < b.y1 - pad ||
                a.y1 > b.y2 + pad
            );
        }


        private void BuildHollowBuilding(
            Zone Z,
            Rect2D rectangle
        )
        {
            if (
                Z == null ||
                rectangle == null ||
                rectangle.Width < 3 ||
                rectangle.Height < 3
            )
            {
                return;
            }

            for (
                int x = rectangle.x1;
                x <= rectangle.x2;
                x++
            )
            {
                SetStructuralWall(
                    Z,
                    x,
                    rectangle.y1
                );

                SetStructuralWall(
                    Z,
                    x,
                    rectangle.y2
                );
            }

            for (
                int y = rectangle.y1 + 1;
                y < rectangle.y2;
                y++
            )
            {
                SetStructuralWall(
                    Z,
                    rectangle.x1,
                    y
                );

                SetStructuralWall(
                    Z,
                    rectangle.x2,
                    y
                );
            }
        }


        private void MaybeAddSubdivision(
            Zone Z,
            Rect2D rectangle,
            System.Random rng
        )
        {
            if (
                rectangle == null ||
                rng.Next(100) >= 60
            )
            {
                return;
            }

            bool vertical =
                rectangle.Width >= rectangle.Height;

            if (
                vertical &&
                rectangle.Width >= 9
            )
            {
                int x =
                    rng.Next(
                        rectangle.x1 + 2,
                        rectangle.x2 - 1
                    );

                int doorY =
                    rng.Next(
                        rectangle.y1 + 1,
                        rectangle.y2
                    );

                for (
                    int y = rectangle.y1 + 1;
                    y < rectangle.y2;
                    y++
                )
                {
                    if (
                        y == doorY ||
                        y == doorY + 1
                    )
                    {
                        continue;
                    }

                    SetStructuralWall(
                        Z,
                        x,
                        y
                    );
                }
            }
            else if (
                rectangle.Height >= 7
            )
            {
                int y =
                    rng.Next(
                        rectangle.y1 + 2,
                        rectangle.y2 - 1
                    );

                int doorX =
                    rng.Next(
                        rectangle.x1 + 1,
                        rectangle.x2
                    );

                for (
                    int x = rectangle.x1 + 1;
                    x < rectangle.x2;
                    x++
                )
                {
                    if (
                        x == doorX ||
                        x == doorX + 1
                    )
                    {
                        continue;
                    }

                    SetStructuralWall(
                        Z,
                        x,
                        y
                    );
                }
            }
        }


        private void AddOutbuildingDoorTowardCourt(
            Zone Z,
            Rect2D building,
            int courtX,
            int courtY
        )
        {
            if (building == null)
                return;

            int dx =
                building.Center.x - courtX;

            int dy =
                building.Center.y - courtY;

            if (
                Math.Abs(dx) >=
                Math.Abs(dy)
            )
            {
                if (dx < 0)
                {
                    PunchVerticalDoor(
                        Z,
                        building.x2,
                        building.Center.y
                    );
                }
                else
                {
                    PunchVerticalDoor(
                        Z,
                        building.x1,
                        building.Center.y
                    );
                }
            }
            else
            {
                if (dy < 0)
                {
                    PunchHorizontalDoor(
                        Z,
                        building.y2,
                        building.Center.x
                    );
                }
                else
                {
                    PunchHorizontalDoor(
                        Z,
                        building.y1,
                        building.Center.x
                    );
                }
            }
        }


        private void AddNorthThroughExit(
            Zone Z,
            Rect2D north,
            int courtX,
            System.Random rng
        )
        {
            if (north == null)
                return;

            int x =
                Clamp(
                    courtX + rng.Next(-4, 5),
                    north.x1 + 1,
                    north.x2 - 1
                );

            PunchHorizontalDoor(
                Z,
                north.y2,
                x
            );

            for (
                int y = north.y1 + 1;
                y < north.y2;
                y++
            )
            {
                CarveCell(
                    Z,
                    x,
                    y
                );
            }

            PunchHorizontalDoor(
                Z,
                north.y1,
                x
            );
        }


        private void AddSouthThroughExit(
            Zone Z,
            Rect2D south,
            int courtX,
            System.Random rng
        )
        {
            if (south == null)
                return;

            int x =
                Clamp(
                    courtX + rng.Next(-4, 5),
                    south.x1 + 1,
                    south.x2 - 1
                );

            PunchHorizontalDoor(
                Z,
                south.y1,
                x
            );

            for (
                int y = south.y1 + 1;
                y < south.y2;
                y++
            )
            {
                CarveCell(
                    Z,
                    x,
                    y
                );
            }

            PunchHorizontalDoor(
                Z,
                south.y2,
                x
            );
        }


        private void AddWestThroughExit(
            Zone Z,
            Rect2D west,
            int courtY,
            System.Random rng
        )
        {
            if (west == null)
                return;

            int y =
                Clamp(
                    courtY + rng.Next(-3, 4),
                    west.y1 + 1,
                    west.y2 - 1
                );

            PunchVerticalDoor(
                Z,
                west.x2,
                y
            );

            for (
                int x = west.x1 + 1;
                x < west.x2;
                x++
            )
            {
                CarveCell(
                    Z,
                    x,
                    y
                );
            }

            PunchVerticalDoor(
                Z,
                west.x1,
                y
            );
        }


        private void AddEastThroughExit(
            Zone Z,
            Rect2D east,
            int courtY,
            System.Random rng
        )
        {
            if (east == null)
                return;

            int y =
                Clamp(
                    courtY + rng.Next(-3, 4),
                    east.y1 + 1,
                    east.y2 - 1
                );

            PunchVerticalDoor(
                Z,
                east.x1,
                y
            );

            for (
                int x = east.x1 + 1;
                x < east.x2;
                x++
            )
            {
                CarveCell(
                    Z,
                    x,
                    y
                );
            }

            PunchVerticalDoor(
                Z,
                east.x2,
                y
            );
        }


        private Rect2D NormalizeRect(
            Zone Z,
            int x1,
            int y1,
            int x2,
            int y2
        )
        {
            x1 =
                Math.Max(
                    SealedBorderWidth,
                    x1
                );

            y1 =
                Math.Max(
                    SealedBorderWidth,
                    y1
                );

            x2 =
                Math.Min(
                    Z.Width -
                    SealedBorderWidth -
                    1,
                    x2
                );

            y2 =
                Math.Min(
                    Z.Height -
                    SealedBorderWidth -
                    1,
                    y2
                );


            return
                new Rect2D(
                    x1,
                    y1,
                    x2,
                    y2
                );
        }


        private void SetStructuralWall(
            Zone Z,
            int x,
            int y
        )
        {
            if (
                x < SealedBorderWidth ||
                y < SealedBorderWidth ||
                x >=
                    Z.Width -
                    SealedBorderWidth ||
                y >=
                    Z.Height -
                    SealedBorderWidth
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

            cell.AddObject(
                SubterraneanSites
                    .SubterraneanSitesEPGeometry
                    .SolidPlaceholderBlueprint
            );
        }


        private void PunchHorizontalDoor(
            Zone Z,
            int y,
            int centerX
        )
        {
            int start =
                centerX -
                DoorWidth / 2;

            for (
                int i = 0;
                i < DoorWidth;
                i++
            )
            {
                CarveCell(
                    Z,
                    start + i,
                    y
                );
            }
        }


        private void PunchVerticalDoor(
            Zone Z,
            int x,
            int centerY
        )
        {
            int start =
                centerY -
                DoorWidth / 2;

            for (
                int i = 0;
                i < DoorWidth;
                i++
            )
            {
                CarveCell(
                    Z,
                    x,
                    start + i
                );
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


        private void CarveOrthogonalConnection(
            Zone Z,
            int x1,
            int y1,
            int x2,
            int y2
        )
        {
            int x =
                x1;

            int y =
                y1;

            while (x != x2)
            {
                CarveCell(
                    Z,
                    x,
                    y
                );

                x +=
                    x < x2
                        ? 1
                        : -1;
            }

            while (y != y2)
            {
                CarveCell(
                    Z,
                    x,
                    y
                );

                y +=
                    y < y2
                        ? 1
                        : -1;
            }

            CarveCell(
                Z,
                x2,
                y2
            );
        }


        private void CarveRectangle(
            Zone Z,
            int x1,
            int y1,
            int x2,
            int y2
        )
        {
            x1 =
                Math.Max(
                    SealedBorderWidth,
                    x1
                );

            y1 =
                Math.Max(
                    SealedBorderWidth,
                    y1
                );

            x2 =
                Math.Min(
                    Z.Width -
                    SealedBorderWidth -
                    1,
                    x2
                );

            y2 =
                Math.Min(
                    Z.Height -
                    SealedBorderWidth -
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
            if (
                x < SealedBorderWidth ||
                y < SealedBorderWidth ||
                x >=
                    Z.Width -
                    SealedBorderWidth ||
                y >=
                    Z.Height -
                    SealedBorderWidth
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
                    bool forced =
                        x < SealedBorderWidth ||
                        y < SealedBorderWidth ||
                        x >=
                            Z.Width -
                            SealedBorderWidth ||
                        y >=
                            Z.Height -
                            SealedBorderWidth;

                    if (!forced)
                        continue;

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


        private int Clamp(
            int value,
            int min,
            int max
        )
        {
            if (value < min)
                return min;

            if (value > max)
                return max;

            return value;
        }
    }

    public class SubterraneanSitesVillageCombLayout :
    ZoneBuilderSandbox
    {
        public int SealedBorderWidth = 1;

        public int AnchorRadius = 2;

        public int SpineHalfWidth = 1;


        public bool BuildZone(
            Zone Z
        )
        {
            if (Z == null)
                return true;


            ClampSettings();


            int seed =
                XRLCore.Core.Game.GetWorldSeed(
                    "SubterraneanSites:VillageCombLayout:" +
                    Z.ZoneID
                );


            System.Random rng =
                new System.Random(
                    seed
                );


            int centerY =
                Z.Height / 2 +
                rng.Next(
                    -2,
                    3
                );


            //
            // Unlike Ring, Comb does not begin by opening almost the whole
            // settlement field.
            //
            // Category 4 receives a solid placeholder zone. Comb directly
            // carves its streets, rooms and passages out of that mass, leaving
            // considerably more structural density behind.
            //
            int[] spineYByX =
                BuildCrookedSpine(
                    Z,
                    rng,
                    centerY
                );


            //
            // Grow the primary dwellings from the communal spine.
            //
            // Placement deliberately allows the architecture to feel accumulated
            // rather than mathematically planned.
            //
            int cursor =
                rng.Next(
                    6,
                    10
                );


            bool lastNorth =
                rng.Next(2) == 0;


            while (
                cursor <
                Z.Width - 7
            )
            {
                int spineY =
                    spineYByX[
                        cursor
                    ];


                if (spineY <= 0)
                {
                    spineY =
                        centerY;
                }


                //
                // Usually alternate north/south to use the available space,
                // but occasionally place two successive structures on the
                // same side.
                //
                bool north;


                if (
                    rng.Next(100) <
                    70
                )
                {
                    north =
                        !lastNorth;
                }
                else
                {
                    north =
                        rng.Next(2) == 0;
                }


                BuildTooth(
                    Z,
                    rng,
                    cursor,
                    spineY,
                    north
                );

                //
                // Frequently grow another habitation area from the opposite side of the
                // same stretch of street. Comb is intentionally one of Village's denser
                // settlement forms.
                //
                if (
                    rng.Next(100) <
                    40
                )
                {
                    BuildTooth(
                        Z,
                        rng,
                        Clamp(
                            cursor + rng.Next(-2, 3),
                            5,
                            Z.Width - 6
                        ),
                        spineY,
                        !north
                    );
                }


                lastNorth =
                    north;


                cursor +=
                    rng.Next(
                        4,
                        7
                    );
            }


            //
            // Smaller pockets break up stretches of the spine and use additional
            // otherwise-unused settlement space.
            //
            int extraRoomCount =
                rng.Next(
                    4,
                    8
                );


            for (
                int i = 0;
                i < extraRoomCount;
                i++
            )
            {
                int x =
                    rng.Next(
                        7,
                        Z.Width - 7
                    );


                int spineY =
                    spineYByX[
                        x
                    ];


                if (spineY <= 0)
                {
                    spineY =
                        centerY;
                }


                AddSpinePocket(
                    Z,
                    rng,
                    x,
                    spineY
                );
            }


            //
            // Vertical-transition anchors remain gameplay requirements.
            //
            // Give each one an open landing and connect it to the nearest
            // corresponding point on the settlement spine.
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


                    int targetX =
                        Clamp(
                            anchor.X,
                            3,
                            Z.Width - 4
                        );


                    int targetY =
                        spineYByX[
                            targetX
                        ];


                    if (targetY <= 0)
                    {
                        targetY =
                            centerY;
                    }


                    CarveOrthogonalConnection(
                        Z,
                        anchor.X,
                        anchor.Y,
                        targetX,
                        targetY
                    );
                }
            }


            ForceSealedBorder(
                Z
            );

            SubterraneanSitesVillageDoorPlacement
            .PlaceDoors(
                Z
            );


            Z.ClearReachableMap();


            return true;
        }



        /// <summary>
        /// Build the main east-west communal lane.
        ///
        /// It proceeds in medium-length horizontal runs with small vertical
        /// shifts between them, giving the settlement a crooked but readable
        /// organizing feature.
        /// </summary>
        private int[] BuildCrookedSpine(
            Zone Z,
            System.Random rng,
            int startingY
        )
        {
            int[] result =
                new int[
                    Z.Width
                ];


            int startX =
                3;


            int endX =
                Z.Width - 4;


            int x =
                startX;


            int currentY =
                Clamp(
                    startingY,
                    5,
                    Z.Height - 6
                );


            while (
                x <= endX
            )
            {
                int runLength =
                    rng.Next(
                        7,
                        14
                    );


                int nextX =
                    Math.Min(
                        endX,
                        x + runLength
                    );


                CarveRectangle(
                    Z,
                    x,
                    currentY - SpineHalfWidth,
                    nextX,
                    currentY + SpineHalfWidth
                );


                for (
                    int markX = x;
                    markX <= nextX;
                    markX++
                )
                {
                    result[
                        markX
                    ] =
                        currentY;
                }


                if (
                    nextX >=
                    endX
                )
                {
                    break;
                }


                int nextY =
                    Clamp(
                        currentY +
                        rng.Next(
                            -2,
                            3
                        ),
                        5,
                        Z.Height - 6
                    );


                //
                // Join successive horizontal stretches with a short crooked jog.
                //
                CarveRectangle(
                    Z,
                    nextX - 1,
                    Math.Min(
                        currentY,
                        nextY
                    ) - SpineHalfWidth,
                    nextX + 1,
                    Math.Max(
                        currentY,
                        nextY
                    ) + SpineHalfWidth
                );


                result[
                    nextX
                ] =
                    nextY;


                x =
                    nextX;


                currentY =
                    nextY;
            }


            return result;
        }



        /// <summary>
        /// Carve one of the primary branching habitation areas.
        ///
        /// A tooth consists of:
        ///   - one substantial room/compound,
        ///   - a narrow connection to the spine,
        ///   - often an internal partition,
        ///   - sometimes a small annex.
        /// </summary>
        private void BuildTooth(
            Zone Z,
            System.Random rng,
            int branchX,
            int spineY,
            bool north
        )
        {
            int roomWidth =
                rng.Next(
                    6,
                    12
                );

            int roomHeight =
                rng.Next(
                    4,
                    8
                );


            int roomX1 =
                branchX -
                roomWidth / 2 +
                rng.Next(
                    -2,
                    3
                );


            roomX1 =
                Clamp(
                    roomX1,
                    2,
                    Z.Width -
                    roomWidth -
                    2
                );


            int roomX2 =
                roomX1 +
                roomWidth -
                1;


            int roomY1;
            int roomY2;


            //
            // Occasionally push a structure all the way toward the outer rim.
            //
            // This keeps Comb from forming a perfectly centered strip of buildings.
            //
            bool pushTowardOuterEdge =
                rng.Next(100) <
                35;


            if (north)
            {
                if (pushTowardOuterEdge)
                {
                    roomY1 =
                        SealedBorderWidth;


                    roomY2 =
                        roomY1 +
                        roomHeight -
                        1;
                }
                else
                {
                    roomY2 =
                        spineY -
                        rng.Next(
                            2,
                            5
                        );


                    roomY1 =
                        roomY2 -
                        roomHeight +
                        1;


                    if (
                        roomY1 <
                        SealedBorderWidth
                    )
                    {
                        roomY1 =
                            SealedBorderWidth;


                        roomY2 =
                            roomY1 +
                            roomHeight -
                            1;
                    }
                }
            }
            else
            {
                if (pushTowardOuterEdge)
                {
                    roomY2 =
                        Z.Height -
                        SealedBorderWidth -
                        1;


                    roomY1 =
                        roomY2 -
                        roomHeight +
                        1;
                }
                else
                {
                    roomY1 =
                        spineY +
                        rng.Next(
                            2,
                            5
                        );


                    roomY2 =
                        roomY1 +
                        roomHeight -
                        1;


                    if (
                        roomY2 >=
                        Z.Height -
                        SealedBorderWidth
                    )
                    {
                        roomY2 =
                            Z.Height -
                            SealedBorderWidth -
                            1;


                        roomY1 =
                            roomY2 -
                            roomHeight +
                            1;
                    }
                }
            }


            //
            // The room itself is carved from the solid mass.
            //
            // Therefore the remaining solid cells surrounding the void become
            // its structural walls after the shared boundary classifier runs.
            //
            CarveRectangle(
                Z,
                roomX1,
                roomY1,
                roomX2,
                roomY2
            );


            //
            // Connect this dwelling to the settlement spine.
            //
            int laneWidth =
                rng.Next(100) < 30
                    ? 2
                    : 1;


            int laneX =
                Clamp(
                    branchX +
                    rng.Next(
                        -1,
                        2
                    ),
                    roomX1 + 1,
                    roomX2 - laneWidth
                );


            if (north)
            {
                CarveRectangle(
                    Z,
                    laneX,
                    roomY2,
                    laneX + laneWidth - 1,
                    spineY
                );
            }
            else
            {
                CarveRectangle(
                    Z,
                    laneX,
                    spineY,
                    laneX + laneWidth - 1,
                    roomY1
                );
            }


            //
            // Most substantial dwellings receive some internal structure.
            //
            if (
                rng.Next(100) <
                65
            )
            {
                AddSubdivision(
                    Z,
                    rng,
                    roomX1,
                    roomY1,
                    roomX2,
                    roomY2
                );
            }

            //
            // Some dwellings have a crooked little annex.
            //
            if (
                rng.Next(100) <
                55
            )
            {
                AddAnnex(
                    Z,
                    rng,
                    roomX1,
                    roomY1,
                    roomX2,
                    roomY2
                );
            }
        }



        private void AddSubdivision(
            Zone Z,
            System.Random rng,
            int x1,
            int y1,
            int x2,
            int y2
        )
        {
            int width =
                x2 -
                x1 +
                1;


            int height =
                y2 -
                y1 +
                1;


            bool vertical =
                width >=
                height;


            if (
                vertical &&
                width >= 7
            )
            {
                int wallX =
                    rng.Next(
                        x1 + 2,
                        x2 - 1
                    );


                int openingY =
                    rng.Next(
                        y1 + 1,
                        y2
                    );


                bool wideOpening =
                    rng.Next(100) <
                    25;


                for (
                    int y = y1 + 1;
                    y < y2;
                    y++
                )
                {
                    if (
                        y == openingY ||
                        (
                            wideOpening &&
                            y ==
                                openingY + 1
                        )
                    )
                    {
                        continue;
                    }


                    SetStructuralWall(
                        Z,
                        wallX,
                        y
                    );
                }
            }
            else if (
                height >= 5
            )
            {
                int wallY =
                    rng.Next(
                        y1 + 2,
                        y2
                    );


                int openingX =
                    rng.Next(
                        x1 + 1,
                        x2
                    );


                bool wideOpening =
                    rng.Next(100) <
                    25;


                for (
                    int x = x1 + 1;
                    x < x2;
                    x++
                )
                {
                    if (
                        x == openingX ||
                        (
                            wideOpening &&
                            x ==
                                openingX + 1
                        )
                    )
                    {
                        continue;
                    }


                    SetStructuralWall(
                        Z,
                        x,
                        wallY
                    );
                }
            }
        }



        private void AddAnnex(
            Zone Z,
            System.Random rng,
            int roomX1,
            int roomY1,
            int roomX2,
            int roomY2
        )
        {
            int annexWidth =
                rng.Next(
                    3,
                    6
                );


            int annexHeight =
                rng.Next(
                    3,
                    5
                );


            bool left =
                rng.Next(2) ==
                0;


            int annexX1;
            int annexX2;


            if (left)
            {
                annexX2 =
                    roomX1 - 2;


                annexX1 =
                    annexX2 -
                    annexWidth +
                    1;
            }
            else
            {
                annexX1 =
                    roomX2 + 2;


                annexX2 =
                    annexX1 +
                    annexWidth -
                    1;
            }


            int centerY =
                (
                    roomY1 +
                    roomY2
                ) / 2;


            int annexY1 =
                centerY -
                annexHeight / 2 +
                rng.Next(
                    -1,
                    2
                );


            int annexY2 =
                annexY1 +
                annexHeight -
                1;


            annexX1 =
                Clamp(
                    annexX1,
                    SealedBorderWidth,
                    Z.Width -
                    SealedBorderWidth -
                    1
                );


            annexX2 =
                Clamp(
                    annexX2,
                    SealedBorderWidth,
                    Z.Width -
                    SealedBorderWidth -
                    1
                );


            annexY1 =
                Clamp(
                    annexY1,
                    SealedBorderWidth,
                    Z.Height -
                    SealedBorderWidth -
                    annexHeight
                );


            annexY2 =
                annexY1 +
                annexHeight -
                1;


            if (
                annexX2 <=
                annexX1
            )
            {
                return;
            }


            CarveRectangle(
                Z,
                annexX1,
                annexY1,
                annexX2,
                annexY2
            );


            //
            // Join the annex to its parent room.
            //
            if (left)
            {
                CarveRectangle(
                    Z,
                    annexX2,
                    centerY,
                    roomX1,
                    centerY + 1
                );
            }
            else
            {
                CarveRectangle(
                    Z,
                    roomX2,
                    centerY,
                    annexX1,
                    centerY + 1
                );
            }
        }



        /// <summary>
        /// Add a smaller room immediately adjacent to the main street.
        ///
        /// These are deliberately less formal than the primary dwellings.
        /// </summary>
        private void AddSpinePocket(
            Zone Z,
            System.Random rng,
            int x,
            int spineY
        )
        {
            bool north =
                rng.Next(2) ==
                0;


            int width =
                rng.Next(
                    3,
                    6
                );


            int height =
                rng.Next(
                    3,
                    5
                );


            int x1 =
                Clamp(
                    x -
                    width / 2,
                    SealedBorderWidth,
                    Z.Width -
                    SealedBorderWidth -
                    width
                );


            int x2 =
                x1 +
                width -
                1;


            int y1;
            int y2;


            if (north)
            {
                y2 =
                    spineY -
                    SpineHalfWidth -
                    1;


                y1 =
                    y2 -
                    height +
                    1;
            }
            else
            {
                y1 =
                    spineY +
                    SpineHalfWidth +
                    1;


                y2 =
                    y1 +
                    height -
                    1;
            }


            y1 =
                Clamp(
                    y1,
                    SealedBorderWidth,
                    Z.Height -
                    SealedBorderWidth -
                    height
                );


            y2 =
                y1 +
                height -
                1;


            CarveRectangle(
                Z,
                x1,
                y1,
                x2,
                y2
            );


            //
            // Small open neck leading back to the spine.
            //
            CarveRectangle(
                Z,
                x,
                Math.Min(
                    spineY,
                    y1
                ),
                x + 1,
                Math.Max(
                    spineY,
                    y2
                )
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



        private void CarveOrthogonalConnection(
            Zone Z,
            int x1,
            int y1,
            int x2,
            int y2
        )
        {
            int x =
                x1;


            int y =
                y1;


            int seed =
                XRLCore.Core.Game.GetWorldSeed(
                    "SubterraneanSites:VillageCombAnchor:" +
                    Z.ZoneID + ":" +
                    x1.ToString() + ":" +
                    y1.ToString()
                );


            System.Random rng =
                new System.Random(
                    seed
                );


            //
            // Half the anchor routes travel horizontally first; the others
            // vertically first. This avoids identical L-shaped routes.
            //
            if (
                rng.Next(2) ==
                0
            )
            {
                while (
                    x != x2
                )
                {
                    CarveCell(
                        Z,
                        x,
                        y
                    );


                    x +=
                        x < x2
                            ? 1
                            : -1;
                }


                while (
                    y != y2
                )
                {
                    CarveCell(
                        Z,
                        x,
                        y
                    );


                    y +=
                        y < y2
                            ? 1
                            : -1;
                }
            }
            else
            {
                while (
                    y != y2
                )
                {
                    CarveCell(
                        Z,
                        x,
                        y
                    );


                    y +=
                        y < y2
                            ? 1
                            : -1;
                }


                while (
                    x != x2
                )
                {
                    CarveCell(
                        Z,
                        x,
                        y
                    );


                    x +=
                        x < x2
                            ? 1
                            : -1;
                }
            }


            CarveCell(
                Z,
                x2,
                y2
            );
        }



        private void CarveRectangle(
            Zone Z,
            int x1,
            int y1,
            int x2,
            int y2
        )
        {
            x1 =
                Math.Max(
                    SealedBorderWidth,
                    x1
                );


            y1 =
                Math.Max(
                    SealedBorderWidth,
                    y1
                );


            x2 =
                Math.Min(
                    Z.Width -
                    SealedBorderWidth -
                    1,
                    x2
                );


            y2 =
                Math.Min(
                    Z.Height -
                    SealedBorderWidth -
                    1,
                    y2
                );


            if (
                x2 < x1 ||
                y2 < y1
            )
            {
                return;
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
                x < SealedBorderWidth ||
                y < SealedBorderWidth ||
                x >=
                    Z.Width -
                    SealedBorderWidth ||
                y >=
                    Z.Height -
                    SealedBorderWidth
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



        /// <summary>
        /// Reintroduce an abstract structural wall inside an already-carved room.
        /// Shared Category-3 processing later gives this wall the Village material.
        /// </summary>
        private void SetStructuralWall(
            Zone Z,
            int x,
            int y
        )
        {
            if (
                x < SealedBorderWidth ||
                y < SealedBorderWidth ||
                x >=
                    Z.Width -
                    SealedBorderWidth ||
                y >=
                    Z.Height -
                    SealedBorderWidth
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


            cell.AddObject(
                SubterraneanSites
                    .SubterraneanSitesEPGeometry
                    .SolidPlaceholderBlueprint
            );
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
                    bool forced =
                        x < SealedBorderWidth ||
                        y < SealedBorderWidth ||
                        x >=
                            Z.Width -
                            SealedBorderWidth ||
                        y >=
                            Z.Height -
                            SealedBorderWidth;


                    if (!forced)
                        continue;


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



        private void ClampSettings()
        {
            if (
                SealedBorderWidth < 1
            )
            {
                SealedBorderWidth =
                    1;
            }


            if (
                AnchorRadius < 1
            )
            {
                AnchorRadius =
                    1;
            }


            if (
                SpineHalfWidth < 1
            )
            {
                SpineHalfWidth =
                    1;
            }
        }



        private int Clamp(
            int value,
            int min,
            int max
        )
        {
            if (
                value < min
            )
            {
                return min;
            }


            if (
                value > max
            )
            {
                return max;
            }


            return value;
        }
    }

    public class SubterraneanSitesVillageShellLayout :
        ZoneBuilderSandbox
    {
        public int SealedBorderWidth = 1;

        public int AnchorRadius = 2;


        public bool BuildZone(
            Zone Z
        )
        {
            if (Z == null)
                return true;


            ClampSettings();


            int seed =
                XRLCore.Core.Game.GetWorldSeed(
                    "SubterraneanSites:VillageShellLayout:" +
                    Z.ZoneID
                );


            System.Random rng =
                new System.Random(
                    seed
                );


            //
            // The apparent center wanders.
            //
            int centerX =
                Z.Width / 2 +
                rng.Next(
                    -5,
                    6
                );


            int centerY =
                Z.Height / 2 +
                rng.Next(
                    -2,
                    3
                );


            //
            // Push the outer shell close to the usable zone perimeter.
            //
            int outerRadiusX =
                Math.Max(
                    32,
                    Z.Width / 2 -
                    rng.Next(
                        3,
                        6
                    )
                );


            int outerRadiusY =
                Math.Max(
                    9,
                    Z.Height / 2 -
                    rng.Next(
                        1,
                        3
                    )
                );


            int middleRadiusX =
                Math.Max(
                    22,
                    outerRadiusX -
                    rng.Next(
                        8,
                        12
                    )
                );


            int middleRadiusY =
                Math.Max(
                    6,
                    outerRadiusY -
                    rng.Next(
                        2,
                        4
                    )
                );


            int innerRadiusX =
                Math.Max(
                    12,
                    middleRadiusX -
                    rng.Next(
                        8,
                        12
                    )
                );


            int innerRadiusY =
                Math.Max(
                    3,
                    middleRadiusY -
                    rng.Next(
                        2,
                        4
                    )
                );


            //
            // Three major nested streets/shells.
            //
            CarveEllipseBand(
                Z,
                centerX,
                centerY,
                outerRadiusX,
                outerRadiusY,
                1
            );


            CarveEllipseBand(
                Z,
                centerX + rng.Next(-2, 3),
                centerY + rng.Next(-1, 2),
                middleRadiusX,
                middleRadiusY,
                1
            );


            CarveEllipseBand(
                Z,
                centerX + rng.Next(-2, 3),
                centerY + rng.Next(-1, 2),
                innerRadiusX,
                innerRadiusY,
                1
            );


            //
            // Lots of radial movement between shells.
            //
            int connectorCount =
                rng.Next(
                    9,
                    15
                );


            for (
                int i = 0;
                i < connectorCount;
                i++
            )
            {
                double angle =
                    rng.NextDouble() *
                    Math.PI *
                    2.0;


                int connectorWidth =
                    rng.Next(100) < 45
                        ? 2
                        : 1;


                CarveRadialConnector(
                    Z,
                    centerX,
                    centerY,
                    angle,
                    innerRadiusX,
                    innerRadiusY,
                    outerRadiusX,
                    outerRadiusY,
                    connectorWidth
                );
            }


            //
            // Actual dwellings and compounds.
            //
            // Most sit directly on one of the shell streets, so their entrances
            // naturally become part of the village circulation.
            //
            int roomCount =
                rng.Next(
                    16,
                    25
                );


            for (
                int i = 0;
                i < roomCount;
                i++
            )
            {
                int whichShell =
                    rng.Next(
                        3
                    );


                int radiusX;
                int radiusY;


                if (whichShell == 0)
                {
                    radiusX =
                        outerRadiusX;

                    radiusY =
                        outerRadiusY;
                }
                else if (whichShell == 1)
                {
                    radiusX =
                        middleRadiusX;

                    radiusY =
                        middleRadiusY;
                }
                else
                {
                    radiusX =
                        innerRadiusX;

                    radiusY =
                        innerRadiusY;
                }


                double angle =
                    rng.NextDouble() *
                    Math.PI *
                    2.0;


                int roomCenterX =
                    centerX +
                    (int)Math.Round(
                        Math.Cos(angle) *
                        radiusX
                    );


                int roomCenterY =
                    centerY +
                    (int)Math.Round(
                        Math.Sin(angle) *
                        radiusY
                    );


                //
                // A minority become open squares/commons rather than houses.
                //
                bool openCommons =
                    rng.Next(100) <
                    18;


                if (openCommons)
                {
                    int width =
                        rng.Next(
                            10,
                            18
                        );


                    int height =
                        rng.Next(
                            5,
                            9
                        );


                    CarveRectangle(
                        Z,
                        roomCenterX - width / 2,
                        roomCenterY - height / 2,
                        roomCenterX + width / 2,
                        roomCenterY + height / 2
                    );
                }
                else
                {
                    int width =
                        rng.Next(
                            5,
                            12
                        );


                    int height =
                        rng.Next(
                            4,
                            8
                        );


                    BuildShellRoom(
                        Z,
                        rng,
                        roomCenterX,
                        roomCenterY,
                        width,
                        height,
                        angle
                    );
                }
            }


            //
            // Large district-dividing chords.
            //
            // These are intentionally substantial. They break the giant open
            // interior into neighborhoods but contain several openings, so the
            // map remains traversable.
            //
            int chordCount =
                rng.Next(
                    7,
                    12
                );


            for (
                int i = 0;
                i < chordCount;
                i++
            )
            {
                double angle =
                    rng.NextDouble() *
                    Math.PI *
                    2.0;


                double oppositeAngle =
                    angle +
                    Math.PI +
                    (
                        rng.NextDouble() -
                        0.5
                    ) *
                    0.55;


                double scale1 =
                    0.60 +
                    rng.NextDouble() *
                    0.32;


                double scale2 =
                    0.60 +
                    rng.NextDouble() *
                    0.32;


                int x1 =
                    centerX +
                    (int)Math.Round(
                        Math.Cos(angle) *
                        outerRadiusX *
                        scale1
                    );


                int y1 =
                    centerY +
                    (int)Math.Round(
                        Math.Sin(angle) *
                        outerRadiusY *
                        scale1
                    );


                int x2 =
                    centerX +
                    (int)Math.Round(
                        Math.Cos(oppositeAngle) *
                        outerRadiusX *
                        scale2
                    );


                int y2 =
                    centerY +
                    (int)Math.Round(
                        Math.Sin(oppositeAngle) *
                        outerRadiusY *
                        scale2
                    );


                BuildWallLineWithGaps(
                    Z,
                    rng,
                    x1,
                    y1,
                    x2,
                    y2,
                    rng.Next(
                        2,
                        5
                    )
                );
            }


            //
            // Additional partial shells.
            //
            // More than the first version: these should contribute meaningful
            // local structure rather than merely decorative fragments.
            //
            int fragmentCount =
                rng.Next(
                    7,
                    12
                );


            for (
                int i = 0;
                i < fragmentCount;
                i++
            )
            {
                double startAngle =
                    rng.NextDouble() *
                    Math.PI *
                    2.0;


                double arcLength =
                    0.45 +
                    rng.NextDouble() *
                    1.05;


                int fragmentRadiusX =
                    rng.Next(
                        innerRadiusX,
                        outerRadiusX + 1
                    );


                int fragmentRadiusY =
                    Math.Max(
                        3,
                        (int)Math.Round(
                            fragmentRadiusX *
                            (
                                (double)outerRadiusY /
                                Math.Max(
                                    1,
                                    outerRadiusX
                                )
                            )
                        )
                    );


                CarveEllipseArc(
                    Z,
                    centerX + rng.Next(-5, 6),
                    centerY + rng.Next(-2, 3),
                    fragmentRadiusX,
                    fragmentRadiusY,
                    startAngle,
                    startAngle + arcLength,
                    1
                );
            }


            //
            // Smaller detached rooms and pockets.
            //
            // These consume leftover space between major shell structures.
            //
            int pocketCount =
                rng.Next(
                    6,
                    11
                );


            for (
                int i = 0;
                i < pocketCount;
                i++
            )
            {
                int pocketX =
                    rng.Next(
                        5,
                        Z.Width - 5
                    );


                int pocketY =
                    rng.Next(
                        4,
                        Z.Height - 4
                    );


                int width =
                    rng.Next(
                        4,
                        8
                    );


                int height =
                    rng.Next(
                        3,
                        6
                    );


                BuildDetachedRoom(
                    Z,
                    rng,
                    pocketX,
                    pocketY,
                    width,
                    height
                );


                //
                // Tie it into approximately the nearest shell.
                //
                double dx =
                    pocketX -
                    centerX;


                double dy =
                    pocketY -
                    centerY;


                double angle =
                    Math.Atan2(
                        dy,
                        dx
                    );


                int targetX =
                    centerX +
                    (int)Math.Round(
                        Math.Cos(angle) *
                        middleRadiusX
                    );


                int targetY =
                    centerY +
                    (int)Math.Round(
                        Math.Sin(angle) *
                        middleRadiusY
                    );


                CarveCrookedConnection(
                    Z,
                    pocketX,
                    pocketY,
                    targetX,
                    targetY,
                    rng
                );
            }


            //
            // Deliberately erase a few structures to create larger open areas.
            //
            // Density should vary within the same map rather than becoming
            // uniformly cramped.
            //
            int lateCommonsCount =
                rng.Next(
                    3,
                    6
                );


            for (
                int i = 0;
                i < lateCommonsCount;
                i++
            )
            {
                int commonsX =
                    centerX +
                    rng.Next(
                        -outerRadiusX / 2,
                        outerRadiusX / 2 + 1
                    );


                int commonsY =
                    centerY +
                    rng.Next(
                        -Math.Max(
                            2,
                            outerRadiusY / 2
                        ),
                        Math.Max(
                            3,
                            outerRadiusY / 2 + 1
                        )
                    );


                int width =
                    rng.Next(
                        8,
                        15
                    );


                int height =
                    rng.Next(
                        4,
                        8
                    );


                CarveRectangle(
                    Z,
                    commonsX - width / 2,
                    commonsY - height / 2,
                    commonsX + width / 2,
                    commonsY + height / 2
                );
            }


            //
            // Transition anchors are repaired last.
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


                    double dx =
                        anchor.X -
                        centerX;


                    double dy =
                        anchor.Y -
                        centerY;


                    double angle =
                        Math.Atan2(
                            dy,
                            dx
                        );


                    int targetX =
                        centerX +
                        (int)Math.Round(
                            Math.Cos(angle) *
                            outerRadiusX
                        );


                    int targetY =
                        centerY +
                        (int)Math.Round(
                            Math.Sin(angle) *
                            outerRadiusY
                        );


                    CarveCrookedConnection(
                        Z,
                        anchor.X,
                        anchor.Y,
                        targetX,
                        targetY,
                        rng
                    );
                }
            }


            ForceSealedBorder(
                Z
            );

            SubterraneanSitesVillageDoorPlacement
            .PlaceDoors(
                Z
            );


            Z.ClearReachableMap();


            return true;
        }



        private void BuildShellRoom(
            Zone Z,
            System.Random rng,
            int centerX,
            int centerY,
            int width,
            int height,
            double shellAngle
        )
        {
            int x1 =
                centerX -
                width / 2;


            int y1 =
                centerY -
                height / 2;


            int x2 =
                x1 +
                width -
                1;


            int y2 =
                y1 +
                height -
                1;


            BuildRoomShell(
                Z,
                x1,
                y1,
                x2,
                y2
            );


            //
            // Open the room along the tangent of the shell.
            //
            // This makes houses feel attached to the shell street instead of
            // randomly sealed boxes.
            //
            double tangentX =
                -Math.Sin(
                    shellAngle
                );


            double tangentY =
                Math.Cos(
                    shellAngle
                );


            if (
                Math.Abs(tangentX) >=
                Math.Abs(tangentY)
            )
            {
                int openingY =
                    Clamp(
                        centerY +
                        rng.Next(-1, 2),
                        y1 + 1,
                        y2 - 1
                    );


                CarveCell(
                    Z,
                    x1,
                    openingY
                );


                CarveCell(
                    Z,
                    x2,
                    openingY
                );
            }
            else
            {
                int openingX =
                    Clamp(
                        centerX +
                        rng.Next(-1, 2),
                        x1 + 1,
                        x2 - 1
                    );


                CarveCell(
                    Z,
                    openingX,
                    y1
                );


                CarveCell(
                    Z,
                    openingX,
                    y2
                );
            }


            //
            // Strong subdivision bias.
            //
            if (
                rng.Next(100) <
                78
            )
            {
                AddRoomSubdivision(
                    Z,
                    rng,
                    x1,
                    y1,
                    x2,
                    y2
                );
            }
        }



        private void BuildDetachedRoom(
            Zone Z,
            System.Random rng,
            int centerX,
            int centerY,
            int width,
            int height
        )
        {
            int x1 =
                centerX -
                width / 2;


            int y1 =
                centerY -
                height / 2;


            int x2 =
                x1 +
                width -
                1;


            int y2 =
                y1 +
                height -
                1;


            BuildRoomShell(
                Z,
                x1,
                y1,
                x2,
                y2
            );


            int side =
                rng.Next(
                    4
                );


            if (side == 0)
            {
                CarveCell(
                    Z,
                    centerX,
                    y1
                );
            }
            else if (side == 1)
            {
                CarveCell(
                    Z,
                    centerX,
                    y2
                );
            }
            else if (side == 2)
            {
                CarveCell(
                    Z,
                    x1,
                    centerY
                );
            }
            else
            {
                CarveCell(
                    Z,
                    x2,
                    centerY
                );
            }


            if (
                rng.Next(100) <
                70
            )
            {
                AddRoomSubdivision(
                    Z,
                    rng,
                    x1,
                    y1,
                    x2,
                    y2
                );
            }
        }



        private void BuildRoomShell(
            Zone Z,
            int x1,
            int y1,
            int x2,
            int y2
        )
        {
            x1 =
                Clamp(
                    x1,
                    SealedBorderWidth + 1,
                    Z.Width -
                    SealedBorderWidth -
                    3
                );


            y1 =
                Clamp(
                    y1,
                    SealedBorderWidth + 1,
                    Z.Height -
                    SealedBorderWidth -
                    3
                );


            x2 =
                Clamp(
                    x2,
                    x1 + 2,
                    Z.Width -
                    SealedBorderWidth -
                    2
                );


            y2 =
                Clamp(
                    y2,
                    y1 + 2,
                    Z.Height -
                    SealedBorderWidth -
                    2
                );


            //
            // Guarantee an open interior first.
            //
            CarveRectangle(
                Z,
                x1,
                y1,
                x2,
                y2
            );


            for (
                int x = x1;
                x <= x2;
                x++
            )
            {
                SetStructuralWall(
                    Z,
                    x,
                    y1
                );


                SetStructuralWall(
                    Z,
                    x,
                    y2
                );
            }


            for (
                int y = y1 + 1;
                y < y2;
                y++
            )
            {
                SetStructuralWall(
                    Z,
                    x1,
                    y
                );


                SetStructuralWall(
                    Z,
                    x2,
                    y
                );
            }
        }



        private void AddRoomSubdivision(
            Zone Z,
            System.Random rng,
            int x1,
            int y1,
            int x2,
            int y2
        )
        {
            int width =
                x2 -
                x1 +
                1;


            int height =
                y2 -
                y1 +
                1;


            if (
                width < 5 ||
                height < 4
            )
            {
                return;
            }


            bool vertical =
                width >=
                height;


            if (
                vertical &&
                width >= 6
            )
            {
                int wallX =
                    rng.Next(
                        x1 + 2,
                        x2
                    );


                int openingY =
                    rng.Next(
                        y1 + 1,
                        y2
                    );


                int openingWidth =
                    rng.Next(100) < 35
                        ? 2
                        : 1;


                for (
                    int y = y1 + 1;
                    y < y2;
                    y++
                )
                {
                    if (
                        y >= openingY &&
                        y <
                            openingY +
                            openingWidth
                    )
                    {
                        continue;
                    }


                    SetStructuralWall(
                        Z,
                        wallX,
                        y
                    );
                }
            }
            else if (
                height >= 5
            )
            {
                int wallY =
                    rng.Next(
                        y1 + 2,
                        y2
                    );


                int openingX =
                    rng.Next(
                        x1 + 1,
                        x2
                    );


                int openingWidth =
                    rng.Next(100) < 35
                        ? 2
                        : 1;


                for (
                    int x = x1 + 1;
                    x < x2;
                    x++
                )
                {
                    if (
                        x >= openingX &&
                        x <
                            openingX +
                            openingWidth
                    )
                    {
                        continue;
                    }


                    SetStructuralWall(
                        Z,
                        x,
                        wallY
                    );
                }
            }
        }



        /// <summary>
        /// Build one long district wall with several deliberate breaks.
        /// </summary>
        private void BuildWallLineWithGaps(
            Zone Z,
            System.Random rng,
            int x1,
            int y1,
            int x2,
            int y2,
            int gapCount
        )
        {
            List<Location2D> points =
                GetLinePoints(
                    x1,
                    y1,
                    x2,
                    y2
                );


            if (
                points.Count <
                6
            )
            {
                return;
            }


            HashSet<int> openings =
                new HashSet<int>();


            for (
                int i = 0;
                i < gapCount;
                i++
            )
            {
                int center =
                    rng.Next(
                        2,
                        points.Count - 2
                    );


                int width =
                    rng.Next(100) < 35
                        ? 2
                        : 1;


                for (
                    int j = 0;
                    j < width;
                    j++
                )
                {
                    if (
                        center + j <
                        points.Count
                    )
                    {
                        openings.Add(
                            center + j
                        );
                    }
                }
            }


            for (
                int i = 0;
                i < points.Count;
                i++
            )
            {
                Location2D point =
                    points[i];


                if (
                    openings.Contains(
                        i
                    )
                )
                {
                    CarveCell(
                        Z,
                        point.X,
                        point.Y
                    );

                    continue;
                }


                SetStructuralWall(
                    Z,
                    point.X,
                    point.Y
                );
            }
        }



        private List<Location2D> GetLinePoints(
            int x1,
            int y1,
            int x2,
            int y2
        )
        {
            List<Location2D> result =
                new List<Location2D>();


            int dx =
                Math.Abs(
                    x2 -
                    x1
                );


            int dy =
                Math.Abs(
                    y2 -
                    y1
                );


            int sx =
                x1 <
                x2
                    ? 1
                    : -1;


            int sy =
                y1 <
                y2
                    ? 1
                    : -1;


            int err =
                dx -
                dy;


            int x =
                x1;


            int y =
                y1;

            while (true)
            {
                Location2D point =
                    Location2D.Get(
                        x,
                        y
                    );


                if (
                    point !=
                    null
                )
                {
                    result.Add(
                        point
                    );
                }


                if (
                    x == x2 &&
                    y == y2
                )
                {
                    break;
                }


                int e2 =
                    2 *
                    err;


                if (
                    e2 >
                    -dy
                )
                {
                    err -=
                        dy;


                    x +=
                        sx;
                }


                if (
                    e2 <
                    dx
                )
                {
                    err +=
                        dx;


                    y +=
                        sy;
                }
            }


            return result;
        }



        private void CarveEllipseBand(
            Zone Z,
            int centerX,
            int centerY,
            int radiusX,
            int radiusY,
            int brushRadius
        )
        {
            int steps =
                300;


            for (
                int i = 0;
                i < steps;
                i++
            )
            {
                double angle =
                    (
                        Math.PI *
                        2.0 *
                        i
                    ) /
                    steps;


                int x =
                    centerX +
                    (int)Math.Round(
                        Math.Cos(angle) *
                        radiusX
                    );


                int y =
                    centerY +
                    (int)Math.Round(
                        Math.Sin(angle) *
                        radiusY
                    );


                CarveBrush(
                    Z,
                    x,
                    y,
                    brushRadius
                );
            }
        }



        private void CarveEllipseArc(
            Zone Z,
            int centerX,
            int centerY,
            int radiusX,
            int radiusY,
            double startAngle,
            double endAngle,
            int brushRadius
        )
        {
            int steps =
                90;


            for (
                int i = 0;
                i <= steps;
                i++
            )
            {
                double fraction =
                    (double)i /
                    steps;


                double angle =
                    startAngle +
                    (
                        endAngle -
                        startAngle
                    ) *
                    fraction;


                int x =
                    centerX +
                    (int)Math.Round(
                        Math.Cos(angle) *
                        radiusX
                    );


                int y =
                    centerY +
                    (int)Math.Round(
                        Math.Sin(angle) *
                        radiusY
                    );


                CarveBrush(
                    Z,
                    x,
                    y,
                    brushRadius
                );
            }
        }



        private void CarveRadialConnector(
            Zone Z,
            int centerX,
            int centerY,
            double angle,
            int innerRadiusX,
            int innerRadiusY,
            int outerRadiusX,
            int outerRadiusY,
            int width
        )
        {
            int startX =
                centerX +
                (int)Math.Round(
                    Math.Cos(angle) *
                    innerRadiusX
                );


            int startY =
                centerY +
                (int)Math.Round(
                    Math.Sin(angle) *
                    innerRadiusY
                );


            int endX =
                centerX +
                (int)Math.Round(
                    Math.Cos(angle) *
                    outerRadiusX
                );


            int endY =
                centerY +
                (int)Math.Round(
                    Math.Sin(angle) *
                    outerRadiusY
                );


            CarveLine(
                Z,
                startX,
                startY,
                endX,
                endY,
                width
            );
        }



        private void CarveCrookedConnection(
            Zone Z,
            int x1,
            int y1,
            int x2,
            int y2,
            System.Random rng
        )
        {
            int bendX =
                Clamp(
                    x1 +
                    (
                        x2 -
                        x1
                    ) /
                    2 +
                    rng.Next(
                        -4,
                        5
                    ),
                    SealedBorderWidth + 1,
                    Z.Width - SealedBorderWidth - 2
                );


            int bendY =
                Clamp(
                    y1 +
                    (
                        y2 -
                        y1
                    ) /
                    2 +
                    rng.Next(
                        -3,
                        4
                    ),
                    SealedBorderWidth + 1,
                    Z.Height - SealedBorderWidth - 2
                );


            if (
                rng.Next(2) ==
                0
            )
            {
                CarveLine(
                    Z,
                    x1,
                    y1,
                    bendX,
                    y1,
                    1
                );


                CarveLine(
                    Z,
                    bendX,
                    y1,
                    bendX,
                    bendY,
                    1
                );


                CarveLine(
                    Z,
                    bendX,
                    bendY,
                    x2,
                    y2,
                    1
                );
            }
            else
            {
                CarveLine(
                    Z,
                    x1,
                    y1,
                    x1,
                    bendY,
                    1
                );


                CarveLine(
                    Z,
                    x1,
                    bendY,
                    bendX,
                    bendY,
                    1
                );


                CarveLine(
                    Z,
                    bendX,
                    bendY,
                    x2,
                    y2,
                    1
                );
            }
        }



        private void CarveLine(
            Zone Z,
            int x1,
            int y1,
            int x2,
            int y2,
            int width
        )
        {
            List<Location2D> points =
                GetLinePoints(
                    x1,
                    y1,
                    x2,
                    y2
                );


            foreach (
                Location2D point
                in points
            )
            {
                CarveBrush(
                    Z,
                    point.X,
                    point.Y,
                    width
                );
            }
        }



        private void CarveBrush(
            Zone Z,
            int centerX,
            int centerY,
            int radius
        )
        {
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
                        Math.Abs(dx) +
                        Math.Abs(dy) >
                        radius + 1
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



        private void CarveRectangle(
            Zone Z,
            int x1,
            int y1,
            int x2,
            int y2
        )
        {
            x1 =
                Math.Max(
                    SealedBorderWidth,
                    x1
                );


            y1 =
                Math.Max(
                    SealedBorderWidth,
                    y1
                );


            x2 =
                Math.Min(
                    Z.Width -
                    SealedBorderWidth -
                    1,
                    x2
                );


            y2 =
                Math.Min(
                    Z.Height -
                    SealedBorderWidth -
                    1,
                    y2
                );


            if (
                x2 < x1 ||
                y2 < y1
            )
            {
                return;
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
                x < SealedBorderWidth ||
                y < SealedBorderWidth ||
                x >=
                    Z.Width -
                    SealedBorderWidth ||
                y >=
                    Z.Height -
                    SealedBorderWidth
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



        private void SetStructuralWall(
            Zone Z,
            int x,
            int y
        )
        {
            if (
                x < SealedBorderWidth ||
                y < SealedBorderWidth ||
                x >=
                    Z.Width -
                    SealedBorderWidth ||
                y >=
                    Z.Height -
                    SealedBorderWidth
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


            cell.AddObject(
                SubterraneanSites
                    .SubterraneanSitesEPGeometry
                    .SolidPlaceholderBlueprint
            );
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
                    bool forced =
                        x < SealedBorderWidth ||
                        y < SealedBorderWidth ||
                        x >=
                            Z.Width -
                            SealedBorderWidth ||
                        y >=
                            Z.Height -
                            SealedBorderWidth;


                    if (!forced)
                        continue;


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



        private void ClampSettings()
        {
            if (
                SealedBorderWidth < 1
            )
            {
                SealedBorderWidth =
                    1;
            }


            if (
                AnchorRadius < 1
            )
            {
                AnchorRadius =
                    1;
            }
        }



        private int Clamp(
            int value,
            int min,
            int max
        )
        {
            if (
                value < min
            )
            {
                return min;
            }


            if (
                value > max
            )
            {
                return max;
            }


            return value;
        }
    }


    public class SubterraneanSitesVillageCaveClusterLayout :
        ZoneBuilderSandbox
    {
        public int SealedBorderWidth = 1;

        public int AnchorRadius = 2;


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


        private sealed class PartitionNode
        {
            public int X1;
            public int Y1;
            public int X2;
            public int Y2;

            public PartitionNode A;
            public PartitionNode B;

            public RoomSpec Room;

            public int Width
            {
                get { return X2 - X1 + 1; }
            }

            public int Height
            {
                get { return Y2 - Y1 + 1; }
            }

            public int Area
            {
                get { return Width * Height; }
            }
        }


        private sealed class DistrictSpec
        {
            public int X1;
            public int Y1;
            public int X2;
            public int Y2;

            public PartitionNode Root;

            public List<RoomSpec> Rooms =
                new List<RoomSpec>();

            public int CenterX
            {
                get { return (X1 + X2) / 2; }
            }

            public int CenterY
            {
                get { return (Y1 + Y2) / 2; }
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
                    "SubterraneanSites:VillageCaveRooms3:" +
                    Z.ZoneID
                );


            System.Random rng =
                new System.Random(
                    seed
                );


            //
            // IMPORTANT:
            //
            // The zone begins solid and stays mostly solid.
            //
            // We build three or four dense carved districts. Rooms are created
            // first with real rock walls separating them. Only afterward do we
            // cut halls and selectively merge spaces.
            //
            List<DistrictSpec> districts =
                CreateDistricts(
                    Z,
                    rng
                );


            foreach (
                DistrictSpec district
                in districts
            )
            {
                BuildDistrict(
                    Z,
                    district,
                    rng
                );
            }


            //
            // Connect districts through the untouched rock.
            //
            // Each district connects to whichever previously-built district is
            // spatially closest, giving us a guaranteed settlement network
            // without turning the whole zone into open terrain.
            //
            for (
                int i = 1;
                i < districts.Count;
                i++
            )
            {
                DistrictSpec current =
                    districts[i];

                DistrictSpec nearest =
                    districts[0];

                int nearestDistance =
                    DistanceSquared(
                        current.CenterX,
                        current.CenterY,
                        nearest.CenterX,
                        nearest.CenterY
                    );


                for (
                    int j = 1;
                    j < i;
                    j++
                )
                {
                    DistrictSpec candidate =
                        districts[j];


                    int distance =
                        DistanceSquared(
                            current.CenterX,
                            current.CenterY,
                            candidate.CenterX,
                            candidate.CenterY
                        );


                    if (
                        distance <
                        nearestDistance
                    )
                    {
                        nearest =
                            candidate;

                        nearestDistance =
                            distance;
                    }
                }


                RoomSpec first;
                RoomSpec second;


                FindClosestRoomPair(
                    current.Rooms,
                    nearest.Rooms,
                    out first,
                    out second
                );


                if (
                    first != null &&
                    second != null
                )
                {
                    CarveHall(
                        Z,
                        first.CenterX,
                        first.CenterY,
                        second.CenterX,
                        second.CenterY,
                        rng,
                        rng.Next(100) < 15
                    );
                }
            }


            //
            // One to three genuinely open communal spaces.
            //
            // These deliberately erase some of the room structure locally,
            // producing plazas / farm-compatible chambers amid the denser
            // neighborhoods.
            //
            int commonsCount =
                rng.Next(
                    1,
                    Math.Min(
                        4,
                        districts.Count + 1
                    )
                );


            HashSet<int> commonsDistricts =
                new HashSet<int>();


            int commonsSafety =
                0;


            while (
                commonsDistricts.Count <
                    commonsCount &&
                commonsSafety++ <
                    30
            )
            {
                commonsDistricts.Add(
                    rng.Next(
                        districts.Count
                    )
                );
            }


            foreach (
                int districtIndex
                in commonsDistricts
            )
            {
                DistrictSpec district =
                    districts[
                        districtIndex
                    ];


                if (
                    district.Rooms.Count ==
                    0
                )
                {
                    continue;
                }


                RoomSpec room =
                    district.Rooms[
                        rng.Next(
                            district.Rooms.Count
                        )
                    ];


                int width =
                    rng.Next(
                        9,
                        15
                    );


                int height =
                    rng.Next(
                        4,
                        8
                    );


                CarveRectangle(
                    Z,
                    room.CenterX - width / 2,
                    room.CenterY - height / 2,
                    room.CenterX + width / 2,
                    room.CenterY + height / 2
                );
            }


            //
            // Transition anchors always win.
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


                    RoomSpec nearest =
                        FindNearestRoom(
                            districts,
                            anchor.X,
                            anchor.Y
                        );


                    if (nearest != null)
                    {
                        CarveHall(
                            Z,
                            anchor.X,
                            anchor.Y,
                            nearest.CenterX,
                            nearest.CenterY,
                            rng,
                            false
                        );
                    }
                }
            }


            ForceSealedBorder(
                Z
            );


            Z.ClearReachableMap();

            SubterraneanSitesVillageDoorPlacement
            .PlaceDoors(
                Z
            );


            return true;
        }



        private List<DistrictSpec> CreateDistricts(
            Zone Z,
            System.Random rng
        )
        {
            List<DistrictSpec> candidates =
                new List<DistrictSpec>();


            //
            // Four broad settlement regions.
            //
            // We usually use all four, but sometimes leave one quadrant almost
            // completely solid.
            //
            candidates.Add(
                CreateDistrictFromSlot(
                    Z,
                    rng,
                    3,
                    2,
                    38,
                    11
                )
            );


            candidates.Add(
                CreateDistrictFromSlot(
                    Z,
                    rng,
                    41,
                    2,
                    Z.Width - 4,
                    11
                )
            );


            candidates.Add(
                CreateDistrictFromSlot(
                    Z,
                    rng,
                    3,
                    13,
                    38,
                    Z.Height - 3
                )
            );


            candidates.Add(
                CreateDistrictFromSlot(
                    Z,
                    rng,
                    41,
                    13,
                    Z.Width - 4,
                    Z.Height - 3
                )
            );


            //
            // About one map in four has one whole district missing.
            //
            if (
                rng.Next(100) <
                25
            )
            {
                candidates.RemoveAt(
                    rng.Next(
                        candidates.Count
                    )
                );
            }


            return candidates;
        }



        private DistrictSpec CreateDistrictFromSlot(
            Zone Z,
            System.Random rng,
            int slotX1,
            int slotY1,
            int slotX2,
            int slotY2
        )
        {
            DistrictSpec result =
                new DistrictSpec();


            //
            // Random margins ensure the neighborhoods do not all occupy the same
            // exact rectangle.
            //
            result.X1 =
                Clamp(
                    slotX1 +
                    rng.Next(
                        0,
                        5
                    ),
                    SealedBorderWidth + 1,
                    Z.Width - 8
                );


            result.X2 =
                Clamp(
                    slotX2 -
                    rng.Next(
                        0,
                        5
                    ),
                    result.X1 + 16,
                    Z.Width -
                        SealedBorderWidth -
                        2
                );


            result.Y1 =
                Clamp(
                    slotY1 +
                    rng.Next(
                        0,
                        3
                    ),
                    SealedBorderWidth + 1,
                    Z.Height - 6
                );


            result.Y2 =
                Clamp(
                    slotY2 -
                    rng.Next(
                        0,
                        3
                    ),
                    result.Y1 + 5,
                    Z.Height -
                        SealedBorderWidth -
                        2
                );


            return result;
        }



        private void BuildDistrict(
            Zone Z,
            DistrictSpec district,
            System.Random rng
        )
        {
            PartitionNode root =
                new PartitionNode();


            root.X1 =
                district.X1;

            root.Y1 =
                district.Y1;

            root.X2 =
                district.X2;

            root.Y2 =
                district.Y2;


            district.Root =
                root;


            List<PartitionNode> leaves =
                new List<PartitionNode>();


            leaves.Add(
                root
            );


            int targetRoomCount =
                rng.Next(
                    5,
                    9
                );


            //
            // Repeatedly split the largest available partition.
            //
            while (
                leaves.Count <
                targetRoomCount
            )
            {
                PartitionNode chosen =
                    FindLargestSplittableLeaf(
                        leaves
                    );


                if (
                    chosen ==
                    null
                )
                {
                    break;
                }


                PartitionNode first;
                PartitionNode second;


                if (
                    !TrySplit(
                        chosen,
                        rng,
                        out first,
                        out second
                    )
                )
                {
                    break;
                }


                chosen.A =
                    first;

                chosen.B =
                    second;


                leaves.Remove(
                    chosen
                );


                leaves.Add(
                    first
                );

                leaves.Add(
                    second
                );
            }


            //
            // Every final partition becomes a recognizable room.
            //
            foreach (
                PartitionNode leaf
                in leaves
            )
            {
                RoomSpec room =
                    CarvePartitionRoom(
                        Z,
                        leaf,
                        rng
                    );


                leaf.Room =
                    room;


                if (
                    room != null
                )
                {
                    district.Rooms.Add(
                        room
                    );


                    //
                    // Cave-like nooks roughen otherwise rectangular room edges.
                    //
                    AddRoomNooks(
                        Z,
                        room,
                        rng
                    );
                }
            }


            //
            // Connect BSP siblings.
            //
            // Because their rooms were separated by solid partition lines, these
            // become actual doors/halls through real walls.
            //
            ConnectPartitionTree(
                Z,
                root,
                rng
            );


            //
            // Add several redundant local routes. These give neighborhoods loops
            // rather than making each district a simple tree.
            //
            int extraConnections =
                rng.Next(
                    2,
                    6
                );


            for (
                int i = 0;
                i < extraConnections;
                i++
            )
            {
                if (
                    district.Rooms.Count <
                    2
                )
                {
                    break;
                }


                RoomSpec first =
                    district.Rooms[
                        rng.Next(
                            district.Rooms.Count
                        )
                    ];


                RoomSpec second =
                    district.Rooms[
                        rng.Next(
                            district.Rooms.Count
                        )
                    ];


                if (
                    first ==
                    second
                )
                {
                    continue;
                }


                int distance =
                    DistanceSquared(
                        first.CenterX,
                        first.CenterY,
                        second.CenterX,
                        second.CenterY
                    );


                //
                // Avoid cutting absurdly long new routes through the entire
                // district. Long travel belongs to inter-district tunnels.
                //
                if (
                    distance >
                    400
                )
                {
                    continue;
                }


                CarveHall(
                    Z,
                    first.CenterX,
                    first.CenterY,
                    second.CenterX,
                    second.CenterY,
                    rng,
                    rng.Next(100) <
                    20
                );
            }
        }



        private PartitionNode FindLargestSplittableLeaf(
            List<PartitionNode> leaves
        )
        {
            PartitionNode result =
                null;


            int largestArea =
                -1;


            foreach (
                PartitionNode leaf
                in leaves
            )
            {
                bool canSplitVertical =
                    leaf.Width >=
                    11;


                bool canSplitHorizontal =
                    leaf.Height >=
                    8;


                if (
                    !canSplitVertical &&
                    !canSplitHorizontal
                )
                {
                    continue;
                }


                if (
                    leaf.Area >
                    largestArea
                )
                {
                    result =
                        leaf;

                    largestArea =
                        leaf.Area;
                }
            }


            return result;
        }



        private bool TrySplit(
            PartitionNode source,
            System.Random rng,
            out PartitionNode first,
            out PartitionNode second
        )
        {
            first =
                null;

            second =
                null;


            bool canVertical =
                source.Width >=
                11;


            bool canHorizontal =
                source.Height >=
                8;


            if (
                !canVertical &&
                !canHorizontal
            )
            {
                return false;
            }


            bool vertical;


            if (
                canVertical &&
                !canHorizontal
            )
            {
                vertical =
                    true;
            }
            else if (
                canHorizontal &&
                !canVertical
            )
            {
                vertical =
                    false;
            }
            else if (
                source.Width >
                source.Height *
                2
            )
            {
                vertical =
                    true;
            }
            else if (
                source.Height >
                source.Width
            )
            {
                vertical =
                    false;
            }
            else
            {
                vertical =
                    rng.Next(2) ==
                    0;
            }


            if (vertical)
            {
                int minimumSplit =
                    source.X1 +
                    4;


                int maximumSplit =
                    source.X2 -
                    4;


                if (
                    maximumSplit <
                    minimumSplit
                )
                {
                    return false;
                }


                int splitX =
                    rng.Next(
                        minimumSplit,
                        maximumSplit + 1
                    );


                //
                // splitX itself remains untouched solid rock: the partition wall.
                //
                first =
                    new PartitionNode
                    {
                        X1 = source.X1,
                        Y1 = source.Y1,
                        X2 = splitX - 1,
                        Y2 = source.Y2
                    };


                second =
                    new PartitionNode
                    {
                        X1 = splitX + 1,
                        Y1 = source.Y1,
                        X2 = source.X2,
                        Y2 = source.Y2
                    };
            }
            else
            {
                int minimumSplit =
                    source.Y1 +
                    3;


                int maximumSplit =
                    source.Y2 -
                    3;


                if (
                    maximumSplit <
                    minimumSplit
                )
                {
                    return false;
                }


                int splitY =
                    rng.Next(
                        minimumSplit,
                        maximumSplit + 1
                    );


                //
                // splitY itself remains untouched solid rock.
                //
                first =
                    new PartitionNode
                    {
                        X1 = source.X1,
                        Y1 = source.Y1,
                        X2 = source.X2,
                        Y2 = splitY - 1
                    };


                second =
                    new PartitionNode
                    {
                        X1 = source.X1,
                        Y1 = splitY + 1,
                        X2 = source.X2,
                        Y2 = source.Y2
                    };
            }


            return true;
        }



        private RoomSpec CarvePartitionRoom(
            Zone Z,
            PartitionNode leaf,
            System.Random rng
        )
        {
            int insetLeft =
                rng.Next(100) <
                20
                    ? 1
                    : 0;


            int insetRight =
                rng.Next(100) <
                20
                    ? 1
                    : 0;


            int insetTop =
                rng.Next(100) <
                20
                    ? 1
                    : 0;


            int insetBottom =
                rng.Next(100) <
                20
                    ? 1
                    : 0;


            int x1 =
                leaf.X1 +
                insetLeft;


            int y1 =
                leaf.Y1 +
                insetTop;


            int x2 =
                leaf.X2 -
                insetRight;


            int y2 =
                leaf.Y2 -
                insetBottom;


            if (
                x2 - x1 <
                2
            )
            {
                x1 =
                    leaf.X1;

                x2 =
                    leaf.X2;
            }


            if (
                y2 - y1 <
                1
            )
            {
                y1 =
                    leaf.Y1;

                y2 =
                    leaf.Y2;
            }


            CarveRectangle(
                Z,
                x1,
                y1,
                x2,
                y2
            );


            return
                new RoomSpec
                {
                    X1 = x1,
                    Y1 = y1,
                    X2 = x2,
                    Y2 = y2
                };
        }



        /// <summary>
        /// Add small cave-like bulges to otherwise rectangular rooms.
        ///
        /// These are deliberately modest. The recognizable room-and-wall
        /// structure remains dominant.
        /// </summary>
        private void AddRoomNooks(
            Zone Z,
            RoomSpec room,
            System.Random rng
        )
        {
            if (
                room ==
                null ||
                rng.Next(100) >=
                70
            )
            {
                return;
            }


            int nookCount =
                rng.Next(100) <
                25
                    ? 2
                    : 1;


            for (
                int i = 0;
                i < nookCount;
                i++
            )
            {
                int side =
                    rng.Next(
                        4
                    );


                if (
                    side == 0
                )
                {
                    int width =
                        rng.Next(
                            2,
                            5
                        );


                    int centerX =
                        Clamp(
                            room.CenterX +
                            rng.Next(
                                -2,
                                3
                            ),
                            room.X1,
                            room.X2
                        );


                    CarveRectangle(
                        Z,
                        centerX - width / 2,
                        room.Y1 - rng.Next(1, 3),
                        centerX + width / 2,
                        room.Y1
                    );
                }
                else if (
                    side == 1
                )
                {
                    int width =
                        rng.Next(
                            2,
                            5
                        );


                    int centerX =
                        Clamp(
                            room.CenterX +
                            rng.Next(
                                -2,
                                3
                            ),
                            room.X1,
                            room.X2
                        );


                    CarveRectangle(
                        Z,
                        centerX - width / 2,
                        room.Y2,
                        centerX + width / 2,
                        room.Y2 + rng.Next(1, 3)
                    );
                }
                else if (
                    side == 2
                )
                {
                    int height =
                        rng.Next(
                            2,
                            4
                        );


                    int centerY =
                        Clamp(
                            room.CenterY +
                            rng.Next(
                                -1,
                                2
                            ),
                            room.Y1,
                            room.Y2
                        );


                    CarveRectangle(
                        Z,
                        room.X1 - rng.Next(1, 4),
                        centerY - height / 2,
                        room.X1,
                        centerY + height / 2
                    );
                }
                else
                {
                    int height =
                        rng.Next(
                            2,
                            4
                        );


                    int centerY =
                        Clamp(
                            room.CenterY +
                            rng.Next(
                                -1,
                                2
                            ),
                            room.Y1,
                            room.Y2
                        );


                    CarveRectangle(
                        Z,
                        room.X2,
                        centerY - height / 2,
                        room.X2 + rng.Next(1, 4),
                        centerY + height / 2
                    );
                }
            }
        }



        private void ConnectPartitionTree(
            Zone Z,
            PartitionNode node,
            System.Random rng
        )
        {
            if (
                node ==
                null ||
                node.A ==
                null ||
                node.B ==
                null
            )
            {
                return;
            }


            ConnectPartitionTree(
                Z,
                node.A,
                rng
            );


            ConnectPartitionTree(
                Z,
                node.B,
                rng
            );


            List<RoomSpec> firstRooms =
                new List<RoomSpec>();


            List<RoomSpec> secondRooms =
                new List<RoomSpec>();


            CollectRooms(
                node.A,
                firstRooms
            );


            CollectRooms(
                node.B,
                secondRooms
            );


            RoomSpec first;
            RoomSpec second;


            FindClosestRoomPair(
                firstRooms,
                secondRooms,
                out first,
                out second
            );


            if (
                first != null &&
                second != null
            )
            {
                CarveHall(
                    Z,
                    first.CenterX,
                    first.CenterY,
                    second.CenterX,
                    second.CenterY,
                    rng,
                    rng.Next(100) <
                    12
                );
            }
        }



        private void CollectRooms(
            PartitionNode node,
            List<RoomSpec> rooms
        )
        {
            if (
                node ==
                null
            )
            {
                return;
            }


            if (
                node.Room !=
                null
            )
            {
                rooms.Add(
                    node.Room
                );


                return;
            }


            CollectRooms(
                node.A,
                rooms
            );


            CollectRooms(
                node.B,
                rooms
            );
        }



        private void FindClosestRoomPair(
            List<RoomSpec> firstRooms,
            List<RoomSpec> secondRooms,
            out RoomSpec first,
            out RoomSpec second
        )
        {
            first =
                null;

            second =
                null;


            int nearestDistance =
                int.MaxValue;


            if (
                firstRooms ==
                null ||
                secondRooms ==
                null
            )
            {
                return;
            }


            foreach (
                RoomSpec firstCandidate
                in firstRooms
            )
            {
                foreach (
                    RoomSpec secondCandidate
                    in secondRooms
                )
                {
                    int distance =
                        DistanceSquared(
                            firstCandidate.CenterX,
                            firstCandidate.CenterY,
                            secondCandidate.CenterX,
                            secondCandidate.CenterY
                        );


                    if (
                        distance <
                        nearestDistance
                    )
                    {
                        nearestDistance =
                            distance;

                        first =
                            firstCandidate;

                        second =
                            secondCandidate;
                    }
                }
            }
        }



        /// <summary>
        /// Carve an actual hallway rather than a broad cave connection.
        ///
        /// Most are one tile wide. A small minority are two tiles wide.
        /// </summary>
        private void CarveHall(
            Zone Z,
            int x1,
            int y1,
            int x2,
            int y2,
            System.Random rng,
            bool wide
        )
        {
            bool horizontalFirst =
                rng.Next(2) ==
                0;


            if (horizontalFirst)
            {
                int bendX =
                    Clamp(
                        (
                            x1 +
                            x2
                        ) /
                        2 +
                        rng.Next(
                            -2,
                            3
                        ),
                        Math.Min(
                            x1,
                            x2
                        ),
                        Math.Max(
                            x1,
                            x2
                        )
                    );


                CarveAxisHall(
                    Z,
                    x1,
                    y1,
                    bendX,
                    y1,
                    wide
                );


                CarveAxisHall(
                    Z,
                    bendX,
                    y1,
                    bendX,
                    y2,
                    wide
                );


                CarveAxisHall(
                    Z,
                    bendX,
                    y2,
                    x2,
                    y2,
                    wide
                );
            }
            else
            {
                int bendY =
                    Clamp(
                        (
                            y1 +
                            y2
                        ) /
                        2 +
                        rng.Next(
                            -2,
                            3
                        ),
                        Math.Min(
                            y1,
                            y2
                        ),
                        Math.Max(
                            y1,
                            y2
                        )
                    );


                CarveAxisHall(
                    Z,
                    x1,
                    y1,
                    x1,
                    bendY,
                    wide
                );


                CarveAxisHall(
                    Z,
                    x1,
                    bendY,
                    x2,
                    bendY,
                    wide
                );


                CarveAxisHall(
                    Z,
                    x2,
                    bendY,
                    x2,
                    y2,
                    wide
                );
            }
        }



        private void CarveAxisHall(
            Zone Z,
            int x1,
            int y1,
            int x2,
            int y2,
            bool wide
        )
        {
            if (
                x1 ==
                x2
            )
            {
                int minimumY =
                    Math.Min(
                        y1,
                        y2
                    );


                int maximumY =
                    Math.Max(
                        y1,
                        y2
                    );


                for (
                    int y = minimumY;
                    y <= maximumY;
                    y++
                )
                {
                    CarveCell(
                        Z,
                        x1,
                        y
                    );


                    if (wide)
                    {
                        CarveCell(
                            Z,
                            x1 + 1,
                            y
                        );
                    }
                }
            }
            else if (
                y1 ==
                y2
            )
            {
                int minimumX =
                    Math.Min(
                        x1,
                        x2
                    );


                int maximumX =
                    Math.Max(
                        x1,
                        x2
                    );


                for (
                    int x = minimumX;
                    x <= maximumX;
                    x++
                )
                {
                    CarveCell(
                        Z,
                        x,
                        y1
                    );


                    if (wide)
                    {
                        CarveCell(
                            Z,
                            x,
                            y1 + 1
                        );
                    }
                }
            }
        }



        private RoomSpec FindNearestRoom(
            List<DistrictSpec> districts,
            int x,
            int y
        )
        {
            RoomSpec nearest =
                null;


            int nearestDistance =
                int.MaxValue;


            foreach (
                DistrictSpec district
                in districts
            )
            {
                foreach (
                    RoomSpec room
                    in district.Rooms
                )
                {
                    int distance =
                        DistanceSquared(
                            x,
                            y,
                            room.CenterX,
                            room.CenterY
                        );


                    if (
                        distance <
                        nearestDistance
                    )
                    {
                        nearest =
                            room;

                        nearestDistance =
                            distance;
                    }
                }
            }


            return nearest;
        }



        private int DistanceSquared(
            int x1,
            int y1,
            int x2,
            int y2
        )
        {
            int dx =
                x2 -
                x1;


            int dy =
                y2 -
                y1;


            return
                dx *
                dx +
                dy *
                dy;
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



        private void CarveRectangle(
            Zone Z,
            int x1,
            int y1,
            int x2,
            int y2
        )
        {
            x1 =
                Math.Max(
                    SealedBorderWidth,
                    x1
                );


            y1 =
                Math.Max(
                    SealedBorderWidth,
                    y1
                );


            x2 =
                Math.Min(
                    Z.Width -
                    SealedBorderWidth -
                    1,
                    x2
                );


            y2 =
                Math.Min(
                    Z.Height -
                    SealedBorderWidth -
                    1,
                    y2
                );


            if (
                x2 <
                x1 ||
                y2 <
                y1
            )
            {
                return;
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
                x < SealedBorderWidth ||
                y < SealedBorderWidth ||
                x >=
                    Z.Width -
                    SealedBorderWidth ||
                y >=
                    Z.Height -
                    SealedBorderWidth
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
                cell ==
                null
            )
            {
                return;
            }


            cell.ClearWalls();
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
                    bool forced =
                        x < SealedBorderWidth ||
                        y < SealedBorderWidth ||
                        x >=
                            Z.Width -
                            SealedBorderWidth ||
                        y >=
                            Z.Height -
                            SealedBorderWidth;


                    if (!forced)
                        continue;


                    Cell cell =
                        Z.GetCell(
                            x,
                            y
                        );


                    if (
                        cell ==
                        null
                    )
                    {
                        continue;
                    }


                    cell.Clear();


                    cell.AddObject(
                        SubterraneanSites
                            .SubterraneanSitesEPGeometry
                            .SolidPlaceholderBlueprint
                    );
                }
            }
        }



        private void ClampSettings()
        {
            if (
                SealedBorderWidth <
                1
            )
            {
                SealedBorderWidth =
                    1;
            }


            if (
                AnchorRadius <
                1
            )
            {
                AnchorRadius =
                    1;
            }
        }



        private int Clamp(
            int value,
            int min,
            int max
        )
        {
            if (
                value <
                min
            )
            {
                return min;
            }


            if (
                value >
                max
            )
            {
                return max;
            }


            return value;
        }
    }



}

namespace XRL.World.Effects
{

    /// <summary>
    /// Player-facing warning while standing in a Village-C1
    /// extradimensional pocket without Village attunement.
    ///
    /// The Village social system owns this effect's lifetime.
    /// </summary>
    [Serializable]
    public class SubterraneanSitesEPVillageHostilityEffect :
        Effect
    {
        public SubterraneanSitesEPVillageHostilityEffect()
        {
            DisplayName =
                "{{R|hostile villager denizens}}";

            //
            // Runtime Village social system owns lifetime.
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
                "Extradimensional villagers will be hostile and will not trade unless you are attuned.";
        }


        public override bool Apply(
            GameObject Object
        )
        {
            return
                Object != null;
        }
    }





    /// <summary>
    /// Village attunement:
    ///
    /// - grants temporary Beguile through the shared attunement mutation system;
    /// - makes tagged Village inhabitants non-hostile while active;
    /// - restores their hostile state when the attunement ends.
    /// </summary>
    [Serializable]
    public class SubterraneanSitesEPVillageAttunementEffect :
        SubterraneanSitesEPAttunementEffect
    {
        public SubterraneanSitesEPVillageAttunementEffect()
            : base()
        {
            ThemeKey =
                "Village";
        }


        public SubterraneanSitesEPVillageAttunementEffect(
            int duration,
            string mutationClass,
            int mutationLevel
        )
            : base(
                duration,
                "Village",

                "",
                0,

                "",
                "",
                0,

                mutationClass,
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


            if (
                Object.CurrentZone != null
            )
            {
                SubterraneanSites
                    .SubterraneanSitesEPVillageSocialSystem
                    .EstablishAttunedTruce(
                        Object.CurrentZone,
                        Object
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


            SubterraneanSites
                .SubterraneanSitesEPVillageSocialSystem
                .SynchronizeZone(
                    Object.CurrentZone,
                    false
                );
        }


        protected override void AddThemeDetails(
            List<string> lines
        )
        {
            if (lines == null)
                return;


            lines.Add(
                "Village inhabitants are non-hostile"
            );


            lines.Add(
                "Provoking a Village denizen into hostility ends attunement early"
            );
        }


        protected override void RegisterThemeEvents(
            GameObject Object,
            IEventRegistrar Registrar
        )
        {
            //
            // Qud sends this to the attacker before resolving a normal
            // melee attack. Attacker and Defender are available on the event,
            // so a miss still counts as choosing to attack the villager.
            //
            Registrar.Register(
                "PerformMeleeAttack"
            );
        }


        protected override bool FireThemeEvent(
            Event E
        )
        {
            if (
                E == null ||
                E.ID !=
                    "PerformMeleeAttack"
            )
            {
                return true;
            }


            GameObject attacker =
                E.GetGameObjectParameter(
                    "Attacker"
                );


            GameObject defender =
                E.GetGameObjectParameter(
                    "Defender"
                );


            if (
                attacker == null ||
                defender == null ||
                !attacker.IsPlayer()
            )
            {
                return true;
            }


            //
            // VillageInhabitant.Prepare() marks every actual Village
            // inhabitant with this property. That includes merchants,
            // specialists, leaders, and the farmer while excluding
            // unrelated creatures on the level.
            //
            if (
                !defender.HasStringProperty(
                    "SubterraneanSitesEPVillageInhabitant"
                )
            )
            {
                return true;
            }


            XRL.Messages.MessageQueue
                .AddPlayerMessage(
                    "{{R|Your Village attunement ends as you attack a Village denizen.}}"
                );


            //
            // Removing this effect invokes RemoveTheme(), which already
            // resynchronizes the current Village level to hostile.
            //
            attacker.RemoveEffect(
                this
            );


            return true;
        }
    }
}