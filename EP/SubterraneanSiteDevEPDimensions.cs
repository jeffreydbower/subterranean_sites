//v1.0.8
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Genkit;
using Qud.API;
using XRL;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.Wish;
using XRL.World;
using XRL.World.Encounters;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;
using HistoryKit;

namespace SubterraneanSiteDev
{
    /// <summary>
    /// A stable Sub-Sites-facing reference to one of vanilla Qud's generated
    /// dimensions.
    ///
    /// Vanilla stores the eleven relevant dimensional identities in two
    /// different collections:
    ///   - 8 ExtraDimension records
    ///   - 3 PsychicFaction records, each with its own associated dimension
    ///
    /// The EP engine hides that split behind this one record. The vanilla
    /// objects themselves remain the authority for names, item indices,
    /// training, faction identity, etc.; this record only points at them and
    /// carries the Sub-Sites theme-slot assignment.
    /// </summary>
    internal sealed class SubterraneanSiteDevDimensionBinding
    {
        public int ReportIndex;
        public int ThemeSlot;

        public string SecretID;
        public string DimensionName;

        // VanillaFactionName is populated only for the three PsychicFaction
        // dimensions. FactionName is always the faction Sub-Sites actually
        // uses for EP denizens. Normally those are identical; if vanilla chose
        // a faction with no usable creature members (for example Resheph),
        // Sub-Sites preserves the vanilla lore identity but assigns a usable
        // replacement denizen faction.
        public string VanillaFactionName;
        public string FactionName;
        public bool FactionWasAssignedBySubSites;
        public bool FactionReplacesUnusableVanillaFaction;

        public ExtraDimension ExtraDimension;
        public PsychicFaction PsychicFaction;

        public bool IsPsychicFactionDimension
        {
            get { return PsychicFaction != null; }
        }

        public string ThemeLabel
        {
            get
            {
                return SubterraneanSiteDevDimensionEngine
                    .GetThemeSlotLabel(ThemeSlot);
            }
        }

        public string KindLabel
        {
            get
            {
                return IsPsychicFactionDimension
                    ? "PsychicFaction dimension"
                    : "ExtraDimension";
            }
        }

        public string FactionSourceLabel
        {
            get
            {
                if (FactionReplacesUnusableVanillaFaction)
                    return "REPLACEMENT";

                return FactionWasAssignedBySubSites
                    ? "ASSIGNED"
                    : "VANILLA";
            }
        }
    }

    internal sealed class SubterraneanSiteDevDimensionEligibility
    {
        public SubterraneanSiteDevDimensionBinding Dimension;
        public int TargetTier;

        public int EligibleMemberCount;
        public int ExactTierCount;
        public int HighestLowerTier = -1;
        public int HighestLowerTierCount;
        public int LowestAvailableTier = int.MaxValue;
        public int LowestAvailableLevel = int.MaxValue;
        public int LowestAvailableLevelCount;

        public bool Eligible;
        public bool Tier8Override;
        public string Rule = "";

        // Used only as the emergency site-generation fallback when there are
        // too few normally eligible dimensions. It measures how far the
        // faction's lowest creature sits above the top of the requested tier's
        // ordinary level band. Exact/lower-tier dimensions have distance 0.
        public int FallbackDistance = int.MaxValue;

        public string AvailableTiers = "";
    }

    internal sealed class SubterraneanSiteDevDimensionPairSelection
    {
        public SubterraneanSiteDevDimensionBinding Primary;
        public SubterraneanSiteDevDimensionBinding Secondary;
        public bool PrimaryUsedFallback;
        public bool SecondaryUsedFallback;
        public int EligibleThemeCount;
    }

    internal static class SubterraneanSiteDevEPNameGenerator
    {
        private static readonly string[] RelationshipWords =
        {
            "Superior",
            "Inferior",
            "Dorsal",
            "Ventral",
            "Cranial",
            "Caudal",
            "Proximal",
            "Distal",
            "Major",
            "Minor"
        };

        private static readonly string[] JunctionWords =
        {
            "Anastomosis",
            "Fistula",
            "Intussusception",
            "Invagination",
            "Adhesion",
            "Prolapse",
            "Volvulus",
            "Herniation",
            "Impaction",
            "Suture",
            "Syzygy",
            "Conjunction",
            "Occlusion",
            "Fold",
            "Shear",
            "Interpenetration",
            "Confluence"
        };

        private static readonly string[] Templates =
        {
            "the {0} {1}-{2} {3}",
            "the {1}-{2} {3}",
            "the {0} {1} of {2}",
            "the {1} upon the {0} {2}",
            "the {1} of the {0} {2}"
        };

        internal static string Generate(
            string siteKey,
            string primaryThemeKey,
            string secondaryThemeKey
        )
        {
            if (
                primaryThemeKey.IsNullOrEmpty() ||
                secondaryThemeKey.IsNullOrEmpty() ||
                XRLCore.Core.Game == null
            )
            {
                return "";
            }

            string primaryName =
                SubterraneanSiteDevDimensionEngine
                    .GetDimensionNameByThemeKey(
                        primaryThemeKey
                    );

            string secondaryName =
                SubterraneanSiteDevDimensionEngine
                    .GetDimensionNameByThemeKey(
                        secondaryThemeKey
                    );

            string primaryIdentity =
                GetTerminalIdentity(
                    primaryName
                );

            string secondaryIdentity =
                GetTerminalIdentity(
                    secondaryName
                );

            if (
                primaryIdentity.IsNullOrEmpty() ||
                secondaryIdentity.IsNullOrEmpty()
            )
            {
                return "";
            }

            string stableSiteKey =
                siteKey.IsNullOrEmpty()
                    ? "UnknownSite"
                    : siteKey;

            int seed =
                XRLCore.Core.Game.GetWorldSeed(
                    "SubterraneanSiteDev:EPName:v1:" +
                    stableSiteKey + ":" +
                    primaryThemeKey + ":" +
                    secondaryThemeKey
                );

            System.Random rng =
                new System.Random(
                    seed
                );

            string relationship =
                RelationshipWords[
                    rng.Next(
                        RelationshipWords.Length
                    )
                ];

            string junction =
                JunctionWords[
                    rng.Next(
                        JunctionWords.Length
                    )
                ];

            string template =
                Templates[
                    rng.Next(
                        Templates.Length
                    )
                ];

            return string.Format(
                template,
                relationship,
                primaryIdentity,
                secondaryIdentity,
                junction
            );
        }

        private static string GetTerminalIdentity(
            string name
        )
        {
            if (name.IsNullOrEmpty())
                return "";

            name =
                name.Trim();

            int lastSpace =
                name.LastIndexOf(
                    ' '
                );

            if (
                lastSpace >= 0 &&
                lastSpace < name.Length - 1
            )
            {
                name =
                    name.Substring(
                        lastSpace + 1
                    );
            }

            return name.Trim();
        }
    }

    /// <summary>
    /// Shared dimensional engine for extradimensional pockets.
    ///
    /// Owns:
    /// - the persistent one-to-one mapping between Qud's eleven generated
    ///   extradimensions and the eleven EP themes;
    /// - dimension/faction lookup and generated dimension-name resolution;
    /// - tier eligibility, denizen selection, and upward scaling;
    /// - extradimensional identity for creatures and items;
    /// - signature/random mutation packages and theme-native adaptation;
    /// - EP denizen hostility, merchant normalization, and loot behavior;
    /// - dimension-development diagnostic wishes.
    /// </summary>
    internal static class SubterraneanSiteDevDimensionEngine
    {
        private const string InitializationFlag =
            "SubterraneanSiteDev_DimensionAssignments_v1";

        private const string SlotStatePrefix =
            "SubterraneanSiteDev_DimensionSlot_v1_";

        private const string ThemeDimensionNameStatePrefix =
            "SubterraneanSiteDev_DimensionNameByTheme_v1_";

        private const string AssignmentSeedKey =
            "SubterraneanSiteDev:DimensionAssignments:v1";

        // v2 rejects faction assignments with no usable creature members and
        // allows a replacement denizen faction for an unusable vanilla
        // PsychicFaction assignment while preserving the vanilla faction as
        // lore. Bumping the state version intentionally rebuilds old test-save
        // faction assignments once.
        private const string FactionInitializationFlag =
            "SubterraneanSiteDev_DimensionFactions_v2";

        private const string FactionStatePrefix =
            "SubterraneanSiteDev_DimensionFaction_v2_";

        private const string LegacyFactionStatePrefix =
            "SubterraneanSiteDev_DimensionFaction_v1_";

        // The eleven vanilla dimensional identities map one-to-one onto the
        // eleven implemented EP themes. Slot numbers are persistent save data
        // and must remain stable so existing saves retain their assignments.
        private static readonly string[] ThemeSlotLabels =
        {
            "Fire",
            "Cold",
            "Electrical",
            "Ooze",
            "Fungus",
            "Light",
            "Darkness",
            "Blood",
            "Portal",
            "Static",
            "Village"
        };

        internal static string GetThemeSlotLabel(int slot)
        {
            if (slot < 0 || slot >= ThemeSlotLabels.Length)
                return "UNASSIGNED";

            return ThemeSlotLabels[slot];
        }

        internal static List<ISubterraneanSiteDevEPCategoryProvider>
            CreateThemeProviderPool()
        {
            return new List<ISubterraneanSiteDevEPCategoryProvider>
            {
                new SubterraneanSiteDevEPFireTheme(),

                // Cold
                null,

                // Electrical
                null,

                // Ooze
                null,

                // Fungus
                null,

                // Light
                null,

                // Darkness
                null,

                // Blood
                null,

                // Portal
                null,

                // Static
                null,

                // Village
                null
            };
        }



        internal static bool EnsureAssignments()
        {
            if (The.Game == null)
                return false;

            DimensionManager manager = GetDimensionManager();
            if (manager == null)
                return false;

            List<SubterraneanSiteDevDimensionBinding> dimensions =
                EnumerateVanillaDimensions(manager, readAssignedSlots: false);

            // Current vanilla source establishes exactly 8 ExtraDimensions and
            // 3 PsychicFaction-linked dimensions. Do not mark initialization
            // complete if that expected set is not available yet.
            if (dimensions.Count != 11)
                return false;

            if (The.Game.GetStringGameState(InitializationFlag) != "Yes")
            {
                List<int> slots = new List<int>();
                for (int i = 0; i < dimensions.Count; i++)
                    slots.Add(i);

                int seed = XRLCore.Core.Game.GetWorldSeed(AssignmentSeedKey);
                System.Random rng = new System.Random(seed);

                // Fisher-Yates. The assignment is deterministic for the world seed,
                // but is still persisted so later implementation changes cannot
                // silently reshuffle an existing save.
                for (int i = slots.Count - 1; i > 0; i--)
                {
                    int j = rng.Next(i + 1);
                    int tmp = slots[i];
                    slots[i] = slots[j];
                    slots[j] = tmp;
                }

                for (int i = 0; i < dimensions.Count; i++)
                {
                    string stateKey = GetSlotStateKey(dimensions[i]);
                    The.Game.SetStringGameState(
                        stateKey,
                        slots[i].ToString()
                    );
                }

                The.Game.SetStringGameState(
                    InitializationFlag,
                    "Yes"
                );
            }

            PersistThemeDimensionNames(
                dimensions
            );

            // Pass 2: use each vanilla PsychicFaction when it has usable
            // creature members; otherwise preserve that faction as lore and
            // assign a usable EP denizen faction. The eight ordinary
            // ExtraDimensions also receive usable factions from the same vanilla
            // potentially-extradimensional pool. These assignments persist
            // independently from theme slots.
            return EnsureFactionAssignments(dimensions);
        }

        private static bool EnsureFactionAssignments(
            List<SubterraneanSiteDevDimensionBinding> dimensions
        )
        {
            if (The.Game == null || dimensions == null || dimensions.Count != 11)
                return false;

            if (The.Game.GetStringGameState(FactionInitializationFlag) == "Yes")
                return true;

            HashSet<string> usedFactionNames =
                new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Preserve each vanilla PsychicFaction as the denizen faction only
            // when it actually has at least one usable creature member. This
            // keeps the vanilla identity whenever possible while preventing a
            // zero-member faction such as Resheph from making an EP dimension
            // unpopulatable.
            foreach (SubterraneanSiteDevDimensionBinding dimension in dimensions)
            {
                if (!dimension.IsPsychicFactionDimension)
                    continue;

                string vanillaFaction = dimension.VanillaFactionName;
                if (!vanillaFaction.IsNullOrEmpty() &&
                    FactionHasUsableMembers(vanillaFaction))
                {
                    usedFactionNames.Add(vanillaFaction);
                    The.Game.SetStringGameState(
                        GetFactionStateKey(dimension),
                        vanillaFaction
                    );
                }
            }

            // Preserve usable v1 test assignments for the eight ordinary
            // ExtraDimensions when possible. This avoids reshuffling a test
            // world's already-observed dimension identities just because the
            // validation rules became stricter. Unusable or duplicate legacy
            // factions are deliberately discarded and rerolled below.
            foreach (SubterraneanSiteDevDimensionBinding dimension in dimensions)
            {
                if (dimension.IsPsychicFactionDimension)
                    continue;

                string legacyFaction = The.Game.GetStringGameState(
                    GetLegacyFactionStateKey(dimension)
                );

                if (legacyFaction.IsNullOrEmpty() ||
                    usedFactionNames.Contains(legacyFaction) ||
                    !FactionHasUsableMembers(legacyFaction))
                {
                    continue;
                }

                usedFactionNames.Add(legacyFaction);
                The.Game.SetStringGameState(
                    GetFactionStateKey(dimension),
                    legacyFaction
                );
            }

            // Fill every remaining dimension with a usable faction from the
            // same vanilla potentially-extradimensional source. Prefer unique
            // factions across all eleven dimensions; if the vanilla pool cannot
            // provide eleven unique usable factions, permit a duplicate rather
            // than leave a dimension without denizens.
            foreach (SubterraneanSiteDevDimensionBinding dimension in dimensions)
            {
                string existing = The.Game.GetStringGameState(
                    GetFactionStateKey(dimension)
                );

                if (!existing.IsNullOrEmpty() && FactionHasUsableMembers(existing))
                    continue;

                string chosen = PickUsablePotentiallyExtradimensionalFaction(
                    usedFactionNames,
                    requireUnique: true
                );

                if (chosen.IsNullOrEmpty())
                {
                    chosen = PickUsablePotentiallyExtradimensionalFaction(
                        usedFactionNames,
                        requireUnique: false
                    );
                }

                if (chosen.IsNullOrEmpty())
                    return false;

                usedFactionNames.Add(chosen);
                The.Game.SetStringGameState(
                    GetFactionStateKey(dimension),
                    chosen
                );
            }

            The.Game.SetStringGameState(FactionInitializationFlag, "Yes");
            return true;
        }

        private static string PickUsablePotentiallyExtradimensionalFaction(
            HashSet<string> usedFactionNames,
            bool requireUnique
        )
        {
            for (int attempt = 0; attempt < 4000; attempt++)
            {
                var faction = Factions.GetRandomPotentiallyExtradimensionalFaction();
                if (faction == null || faction.Name.IsNullOrEmpty())
                    continue;

                if (requireUnique &&
                    usedFactionNames != null &&
                    usedFactionNames.Contains(faction.Name))
                {
                    continue;
                }

                if (!FactionHasUsableMembers(faction.Name))
                    continue;

                return faction.Name;
            }

            return null;
        }

        private static bool FactionHasUsableMembers(string factionName)
        {
            return GetEligibleFactionMembersByName(factionName).Count > 0;
        }

        internal static List<SubterraneanSiteDevDimensionBinding>
            GetBindings()
        {
            EnsureAssignments();

            DimensionManager manager = GetDimensionManager();
            if (manager == null)
                return new List<SubterraneanSiteDevDimensionBinding>();

            return EnumerateVanillaDimensions(
                manager,
                readAssignedSlots: true
            );
        }

        internal static string GetDimensionNameByThemeKey(
            string themeKey
        )
        {
            if (
                The.Game == null ||
                themeKey.IsNullOrEmpty()
            )
            {
                return "";
            }

            string stateKey =
                ThemeDimensionNameStatePrefix +
                themeKey;

            string name =
                The.Game.GetStringGameState(
                    stateKey
                );

            if (!name.IsNullOrEmpty())
                return name;

            //
            // Backfill support.
            //
            // Older saves may already have the persistent dimension -> theme-slot
            // assignment without the newer direct theme -> dimension-name entry.
            //
            DimensionManager manager =
                GetDimensionManager();

            if (manager == null)
                return "";

            List<SubterraneanSiteDevDimensionBinding> dimensions =
                EnumerateVanillaDimensions(
                    manager,
                    readAssignedSlots: false
                );

            PersistThemeDimensionNames(
                dimensions
            );

            return
                The.Game.GetStringGameState(
                    stateKey
                ) ?? "";
        }

        private static void PersistThemeDimensionNames(
            List<SubterraneanSiteDevDimensionBinding> dimensions
        )
        {
            if (
                The.Game == null ||
                dimensions == null
            )
            {
                return;
            }

            foreach (
                SubterraneanSiteDevDimensionBinding dimension
                in dimensions
            )
            {
                if (dimension == null)
                    continue;

                int slot =
                    ReadAssignedSlot(
                        dimension
                    );

                if (
                    slot < 0 ||
                    slot >= ThemeSlotLabels.Length
                )
                {
                    continue;
                }

                string themeKey =
                    GetThemeSlotLabel(
                        slot
                    );

                string dimensionName =
                    ResolveDimensionDisplayName(
                        dimension
                    );

                if (
                    themeKey.IsNullOrEmpty() ||
                    dimensionName.IsNullOrEmpty()
                )
                {
                    continue;
                }

                The.Game.SetStringGameState(
                    ThemeDimensionNameStatePrefix +
                    themeKey,
                    dimensionName
                );
            }
        }

        private static string ResolveDimensionDisplayName(
            SubterraneanSiteDevDimensionBinding dimension
        )
        {
            if (
                dimension == null ||
                dimension.DimensionName.IsNullOrEmpty()
            )
            {
                return "";
            }

            string name =
                dimension.DimensionName;

            if (dimension.ExtraDimension != null)
            {
                string symbol =
                    ((char)dimension.ExtraDimension.Symbol)
                        .ToString();

                name =
                    name.Replace(
                        "*DimensionSymbol*",
                        symbol
                    ).Replace(
                        "*dimensionSymbol*",
                        symbol
                    );
            }
            else if (dimension.PsychicFaction != null)
            {
                string symbol =
                    ((char)dimension.PsychicFaction.dimensionSymbol)
                        .ToString();

                name =
                    name.Replace(
                        "*DimensionSymbol*",
                        symbol
                    ).Replace(
                        "*dimensionSymbol*",
                        symbol
                    );
            }

            return name;
        }

        internal static SubterraneanSiteDevDimensionBinding
            GetBindingByReportIndex(int reportIndex)
        {
            List<SubterraneanSiteDevDimensionBinding> bindings =
                GetBindings();

            foreach (SubterraneanSiteDevDimensionBinding binding in bindings)
            {
                if (binding.ReportIndex == reportIndex)
                    return binding;
            }

            return null;
        }

        internal static SubterraneanSiteDevDimensionBinding
            GetBindingByThemeKey(string themeKey)
        {
            if (themeKey.IsNullOrEmpty())
                return null;

            foreach (SubterraneanSiteDevDimensionBinding binding in GetBindings())
            {
                if (string.Equals(
                    binding.ThemeLabel,
                    themeKey,
                    StringComparison.OrdinalIgnoreCase
                ))
                {
                    return binding;
                }
            }

            return null;
        }

        private static List<GameObjectBlueprint> GetEligibleFactionMembersByName(
            string factionName
        )
        {
            List<GameObjectBlueprint> result = new List<GameObjectBlueprint>();

            if (factionName.IsNullOrEmpty())
                return result;

            List<GameObjectBlueprint> factionMembers =
                GameObjectFactory.Factory.GetFactionMembers(factionName);

            if (factionMembers == null)
                return result;

            foreach (GameObjectBlueprint blueprint in factionMembers)
            {
                if (blueprint == null ||
                    !blueprint.HasStat("Level") ||
                    !EncountersAPI.IsEligibleForDynamicEncounters(blueprint) ||
                    !EncountersAPI.IsLegendaryEligible(blueprint))
                {
                    continue;
                }

                result.Add(blueprint);
            }

            return result;
        }

        internal static List<GameObjectBlueprint> GetEligibleFactionMembers(
            SubterraneanSiteDevDimensionBinding dimension
        )
        {
            return GetEligibleFactionMembersByName(
                dimension == null ? null : dimension.FactionName
            );
        }

        /// <summary>
        /// Development EP selection rule: choose randomly from eligible members
        /// of the dimension's assigned faction whose blueprint Tier exactly
        /// matches targetTier. No fallback is used in this test pass; an empty
        /// tier is information we want to see.
        /// </summary>
        internal static GameObject CreateFactionCreatureAtTier(
            SubterraneanSiteDevDimensionBinding dimension,
            int targetTier,
            out string sourceDescription
        )
        {
            List<GameObjectBlueprint> all = GetEligibleFactionMembers(dimension);
            List<GameObjectBlueprint> candidates = new List<GameObjectBlueprint>();

            foreach (GameObjectBlueprint blueprint in all)
            {
                if (blueprint.Tier == targetTier)
                    candidates.Add(blueprint);
            }

            sourceDescription =
                "faction " + (dimension == null ? "UNKNOWN" : dimension.FactionName) +
                "; exact tier " + targetTier.ToString() +
                "; candidates " + candidates.Count.ToString();

            if (candidates.Count == 0)
                return null;

            GameObjectBlueprint selected =
                candidates[Stat.Random(0, candidates.Count - 1)];

            return GameObject.Create(selected.Name);
        }

        /// <summary>
        /// Vanilla PsychicFaction-style level selection, except the requested
        /// level is supplied by the wish rather than read from the player. This
        /// lets us reproduce the observed "same creature until a level boundary
        /// is crossed" behavior without repeatedly rebuilding test characters.
        /// </summary>
        internal static GameObject CreateFactionCreatureAtLevel(
            SubterraneanSiteDevDimensionBinding dimension,
            int targetLevel,
            out string sourceDescription
        )
        {
            List<GameObjectBlueprint> factionMembers =
                GetEligibleFactionMembers(dimension);

            if (factionMembers.Count == 0)
            {
                sourceDescription =
                    "faction " + (dimension == null ? "UNKNOWN" : dimension.FactionName) +
                    "; vanilla-style target level " + targetLevel.ToString() +
                    "; eligible members 0";
                return null;
            }

            factionMembers.ShuffleInPlace<GameObjectBlueprint>();

            GameObjectBlueprint bestBelowTarget = null;
            GameObjectBlueprint lowestEligible = null;
            int lowestLevel = int.MaxValue;
            int bestBelowLevel = 0;

            foreach (GameObjectBlueprint blueprint in factionMembers)
            {
                int level = blueprint.GetStat("Level").Value;

                if (bestBelowTarget == null && level < targetLevel)
                {
                    bestBelowTarget = blueprint;
                    bestBelowLevel = level;
                }
                else if (level < targetLevel && level > bestBelowLevel)
                {
                    bestBelowTarget = blueprint;
                    bestBelowLevel = level;
                }
                else if (level < targetLevel &&
                         level == bestBelowLevel &&
                         50.in100())
                {
                    bestBelowTarget = blueprint;
                    bestBelowLevel = level;
                }

                if (lowestEligible == null || level < lowestLevel)
                {
                    lowestEligible = blueprint;
                    lowestLevel = level;
                }
                else if (level == lowestLevel && 50.in100())
                {
                    lowestEligible = blueprint;
                    lowestLevel = level;
                }
            }

            GameObjectBlueprint selected = bestBelowTarget ?? lowestEligible;

            sourceDescription =
                "faction " + dimension.FactionName +
                "; vanilla-style target level " + targetLevel.ToString() +
                "; eligible members " + factionMembers.Count.ToString() +
                "; selected level " +
                (selected == null ? "NONE" : selected.GetStat("Level").Value.ToString());

            if (selected == null)
                return null;

            return GameObject.Create(selected.Name);
        }

        internal const int MaximumNativeOverlevel = 10;

        internal static int GetMinimumLevelForTier(int tier)
        {
            tier = Math.Max(1, Math.Min(8, tier));
            return 1 + (tier - 1) * 5;
        }

        internal static int GetMaximumLevelForTier(int tier)
        {
            tier = Math.Max(1, Math.Min(8, tier));
            return tier * 5;
        }

        /// <summary>
        /// Evaluate whether a dimension can support an EP using only the EP
        /// tier as the design input. No player level participates.
        ///
        /// Rule order:
        ///   1. exact blueprint tier -> eligible;
        ///   2. otherwise nearest LOWER blueprint tier -> eligible and its
        ///      selected creature can be leveled upward to the minimum level of
        ///      the EP target tier;
        ///   3. if the faction has no creature at or below the target tier, its
        ///      lowest-level native creature is allowed only when it is within
        ///      +10 levels of the TOP of the target tier's ordinary level band;
        ///   4. Tier 8 makes every dimension with at least one usable faction
        ///      member available. In ordinary data a lower-tier member will hit
        ///      rule 2 before this override is needed;
        ///   5. otherwise the dimension/theme is unavailable and the site roll
        ///      chooses another theme.
        /// </summary>
        internal static SubterraneanSiteDevDimensionEligibility
            EvaluateDimensionForSite(
                SubterraneanSiteDevDimensionBinding dimension,
                int targetTier
            )
        {
            targetTier = Math.Max(1, Math.Min(8, targetTier));

            SubterraneanSiteDevDimensionEligibility result =
                new SubterraneanSiteDevDimensionEligibility
                {
                    Dimension = dimension,
                    TargetTier = targetTier
                };

            List<GameObjectBlueprint> members =
                GetEligibleFactionMembers(dimension);

            result.EligibleMemberCount = members.Count;

            if (members.Count == 0)
            {
                result.Eligible = false;
                result.Rule = "NO ELIGIBLE FACTION MEMBERS";
                return result;
            }

            HashSet<int> tiers = new HashSet<int>();

            foreach (GameObjectBlueprint blueprint in members)
            {
                int tier = blueprint.Tier;
                int level = blueprint.GetStat("Level").Value;
                tiers.Add(tier);

                if (tier == targetTier)
                    result.ExactTierCount++;

                if (tier < targetTier)
                {
                    if (tier > result.HighestLowerTier)
                    {
                        result.HighestLowerTier = tier;
                        result.HighestLowerTierCount = 1;
                    }
                    else if (tier == result.HighestLowerTier)
                    {
                        result.HighestLowerTierCount++;
                    }
                }

                if (tier < result.LowestAvailableTier)
                    result.LowestAvailableTier = tier;

                if (level < result.LowestAvailableLevel)
                {
                    result.LowestAvailableLevel = level;
                    result.LowestAvailableLevelCount = 1;
                }
                else if (level == result.LowestAvailableLevel)
                {
                    result.LowestAvailableLevelCount++;
                }
            }

            result.AvailableTiers = FormatTierSet(tiers);

            if (result.ExactTierCount > 0)
            {
                result.Eligible = true;
                result.Rule = "EXACT TIER";
                result.FallbackDistance = 0;
                return result;
            }

            if (result.HighestLowerTier >= 1)
            {
                result.Eligible = true;
                result.Rule =
                    "LOWER TIER T" + result.HighestLowerTier.ToString() +
                    " -> SCALE TO T" + targetTier.ToString();
                result.FallbackDistance = 0;
                return result;
            }

            int targetTierTopLevel = GetMaximumLevelForTier(targetTier);
            int overLevel = result.LowestAvailableLevel - targetTierTopLevel;
            result.FallbackDistance = Math.Max(0, overLevel);

            if (targetTier >= 8)
            {
                result.Eligible = true;
                result.Tier8Override = true;
                result.Rule = "TIER 8 ALL-THEMES OVERRIDE";
                return result;
            }

            if (overLevel <= MaximumNativeOverlevel)
            {
                result.Eligible = true;
                result.Rule =
                    "NATIVE HIGHER CREATURE WITHIN +" +
                    MaximumNativeOverlevel.ToString() +
                    " LEVELS OF TIER CEILING";
                return result;
            }

            result.Eligible = false;
            result.Rule =
                "UNAVAILABLE: LOWEST CREATURE IS +" +
                overLevel.ToString() +
                " LEVELS ABOVE T" + targetTier.ToString() +
                " CEILING";

            return result;
        }

        internal static List<SubterraneanSiteDevDimensionEligibility>
            GetDimensionEligibilityTable(int targetTier)
        {
            List<SubterraneanSiteDevDimensionEligibility> result =
                new List<SubterraneanSiteDevDimensionEligibility>();

            foreach (SubterraneanSiteDevDimensionBinding binding in GetBindings())
            {
                result.Add(EvaluateDimensionForSite(binding, targetTier));
            }

            return result;
        }

        /// <summary>
        /// Choose two distinct dimensions from the themes that can actually
        /// supply denizens at this EP tier. Choosing directly from the eligible
        /// pool is equivalent to rerolling an unavailable theme until one works,
        /// without the possibility of an infinite reroll loop.
        ///
        /// If fewer than two dimensions are normally eligible, fill the missing
        /// slot with the dimension whose lowest creature is closest above the
        /// target tier ceiling. This is only the requested generation safety net.
        /// </summary>
        internal static SubterraneanSiteDevDimensionPairSelection
            SelectDimensionPairForSite(
                string siteKey,
                int targetTier
            )
        {
            targetTier = Math.Max(1, Math.Min(8, targetTier));

            List<SubterraneanSiteDevDimensionEligibility> evaluations =
                GetDimensionEligibilityTable(targetTier);

            List<SubterraneanSiteDevDimensionEligibility> eligible =
                new List<SubterraneanSiteDevDimensionEligibility>();

            foreach (SubterraneanSiteDevDimensionEligibility evaluation in evaluations)
            {
                if (evaluation.Eligible)
                    eligible.Add(evaluation);
            }

            string stableSiteKey = siteKey.IsNullOrEmpty()
                ? "UnknownSite"
                : siteKey;

            int seed = XRLCore.Core.Game.GetWorldSeed(
                "SubterraneanSiteDev:EPDimensionPair:v2:" +
                stableSiteKey + ":T" + targetTier.ToString()
            );

            System.Random rng = new System.Random(seed);

            SubterraneanSiteDevDimensionPairSelection result =
                new SubterraneanSiteDevDimensionPairSelection();

            result.EligibleThemeCount = eligible.Count;

            SubterraneanSiteDevDimensionEligibility primaryEval = null;
            SubterraneanSiteDevDimensionEligibility secondaryEval = null;

            if (eligible.Count >= 2)
            {
                int primaryIndex = rng.Next(eligible.Count);
                primaryEval = eligible[primaryIndex];

                List<SubterraneanSiteDevDimensionEligibility> remaining =
                    new List<SubterraneanSiteDevDimensionEligibility>(eligible);
                remaining.RemoveAt(primaryIndex);

                secondaryEval = remaining[rng.Next(remaining.Count)];
            }
            else if (eligible.Count == 1)
            {
                primaryEval = eligible[0];
                secondaryEval = PickClosestFallback(
                    evaluations,
                    primaryEval.Dimension,
                    rng
                );
                result.SecondaryUsedFallback = secondaryEval != null;
            }
            else
            {
                primaryEval = PickClosestFallback(evaluations, null, rng);
                result.PrimaryUsedFallback = primaryEval != null;

                secondaryEval = PickClosestFallback(
                    evaluations,
                    primaryEval == null ? null : primaryEval.Dimension,
                    rng
                );
                result.SecondaryUsedFallback = secondaryEval != null;
            }

            result.Primary = primaryEval == null ? null : primaryEval.Dimension;
            result.Secondary = secondaryEval == null ? null : secondaryEval.Dimension;

            return result;
        }

        private static SubterraneanSiteDevDimensionEligibility
            PickClosestFallback(
                List<SubterraneanSiteDevDimensionEligibility> evaluations,
                SubterraneanSiteDevDimensionBinding exclude,
                System.Random rng
            )
        {
            int bestDistance = int.MaxValue;
            List<SubterraneanSiteDevDimensionEligibility> best =
                new List<SubterraneanSiteDevDimensionEligibility>();

            foreach (SubterraneanSiteDevDimensionEligibility evaluation in evaluations)
            {
                if (evaluation == null ||
                    evaluation.Dimension == null ||
                    evaluation.EligibleMemberCount <= 0)
                {
                    continue;
                }

                if (exclude != null &&
                    evaluation.Dimension.ReportIndex == exclude.ReportIndex)
                {
                    continue;
                }

                int distance = evaluation.FallbackDistance;

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best.Clear();
                    best.Add(evaluation);
                }
                else if (distance == bestDistance)
                {
                    best.Add(evaluation);
                }
            }

            if (best.Count == 0)
                return null;

            return best[rng.Next(best.Count)];
        }

        /// <summary>
        /// Create a base denizen for a specific assigned dimension using the
        /// tier-only rules. The returned object is not yet dressed with the
        /// vanilla Extradimensional part, EP resistance/mutations, hostility,
        /// equipment replacement, or placement.
        /// </summary>
        internal static GameObject CreateDenizenForSite(
            SubterraneanSiteDevDimensionBinding dimension,
            int targetTier,
            out string sourceDescription
        )
        {
            targetTier = Math.Max(1, Math.Min(8, targetTier));

            SubterraneanSiteDevDimensionEligibility evaluation =
                EvaluateDimensionForSite(dimension, targetTier);

            List<GameObjectBlueprint> members =
                GetEligibleFactionMembers(dimension);

            if (members.Count == 0)
            {
                sourceDescription = evaluation.Rule;
                return null;
            }

            List<GameObjectBlueprint> candidates =
                new List<GameObjectBlueprint>();

            string selectionRule;
            bool scaleUp = false;
            int targetLevelForScaling = GetMinimumLevelForTier(targetTier);

            foreach (GameObjectBlueprint blueprint in members)
            {
                if (blueprint.Tier == targetTier)
                    candidates.Add(blueprint);
            }

            if (candidates.Count > 0)
            {
                selectionRule = "exact T" + targetTier.ToString();
            }
            else if (evaluation.HighestLowerTier >= 1)
            {
                foreach (GameObjectBlueprint blueprint in members)
                {
                    if (blueprint.Tier == evaluation.HighestLowerTier)
                        candidates.Add(blueprint);
                }

                selectionRule =
                    "nearest lower T" +
                    evaluation.HighestLowerTier.ToString() +
                    " -> scale to minimum L" +
                    targetLevelForScaling.ToString() +
                    " for T" + targetTier.ToString();
                scaleUp = true;
            }
            else
            {
                if (!evaluation.Eligible)
                {
                    sourceDescription = evaluation.Rule;
                    return null;
                }

                foreach (GameObjectBlueprint blueprint in members)
                {
                    if (blueprint.GetStat("Level").Value ==
                        evaluation.LowestAvailableLevel)
                    {
                        candidates.Add(blueprint);
                    }
                }

                selectionRule = evaluation.Tier8Override
                    ? "T8 all-themes native fallback L" +
                      evaluation.LowestAvailableLevel.ToString()
                    : "native higher within tolerance L" +
                      evaluation.LowestAvailableLevel.ToString();
            }

            if (candidates.Count == 0)
            {
                sourceDescription =
                    "NO CANDIDATE AFTER RULE EVALUATION: " + evaluation.Rule;
                return null;
            }

            GameObjectBlueprint selected =
                candidates[Stat.Random(0, candidates.Count - 1)];

            GameObject creature = GameObject.Create(selected.Name);

            if (creature == null)
            {
                sourceDescription = "FAILED TO CREATE " + selected.Name;
                return null;
            }

            int originalLevel = creature.Stat("Level");

            creature.SetIntProperty(
                "SubterraneanSiteDevEPOriginalTier",
                selected.Tier
            );
            creature.SetIntProperty(
                "SubterraneanSiteDevEPOriginalLevel",
                originalLevel
            );
            creature.SetStringProperty(
                "SubterraneanSiteDevEPSelectionRule",
                selectionRule
            );

            if (scaleUp && originalLevel < targetLevelForScaling)
                ScaleCreatureUpToLevel(creature, targetLevelForScaling);

            sourceDescription =
                selectionRule +
                "; blueprint " + selected.Name +
                " T" + selected.Tier.ToString() +
                " L" + originalLevel.ToString() +
                "; final L" + creature.Stat("Level").ToString();

            return creature;
        }

        internal static void ScaleCreatureUpToLevel(
            GameObject creature,
            int targetLevel
        )
        {
            if (creature == null || targetLevel <= creature.Stat("Level"))
                return;

            if (!creature.HasStat("XP") || !creature.HasStat("Level"))
                return;

            int targetXP = Leveler.GetXPForLevel(targetLevel);
            int currentXP = creature.Stat("XP");
            int neededXP = targetXP - currentXP;

            if (neededXP > 0)
                creature.AwardXP(neededXP);
        }

        // EP denizens receive roughly half of the player-like mutation-point
        // progression for their FINAL level after any tier repair.
        //
        // Their original creature blueprint already supplies its normal stats,
        // abilities and mutations, while the dimensional signature mutation is
        // guaranteed separately below. The reduced budget adds extradimensional
        // development without giving every denizen a player-sized mutation kit.
        internal static int GetMutationPointBudgetForLevel(
            int level
        )
        {
            level =
                Math.Max(
                    1,
                    level
                );

            int fullBudget =
                level;

            if (level >= 5)
                fullBudget += 3;

            if (level >= 15)
                fullBudget += 3;

            if (level >= 25)
                fullBudget += 3;

            if (level >= 35)
                fullBudget += 3;

            //
            // EP denizens receive about 55% of the player-like mutation
            // progression, capped at 34 points. This keeps their mutation
            // package meaningfully below a player's while giving high-tier
            // denizens a little more development than the 50% test pass.
            //
            int reducedBudget =
                fullBudget * 11 / 20;

            return
                Math.Max(
                    1,
                    Math.Min(
                        34,
                        reducedBudget
                    )
                );
        }

        private static bool IsExcludedRandomDenizenMutation(
            BaseMutation mutation
        )
        {
            if (mutation == null)
                return false;

            string className =
                mutation.GetType().Name;

            string mutationName =
                mutation.Name ?? "";


            return
                string.Equals(
                    className,
                    "SunderMind",
                    StringComparison.Ordinal
                ) ||
                string.Equals(
                    mutationName,
                    "SunderMind",
                    StringComparison.Ordinal
                ) ||
                string.Equals(
                    mutationName,
                    "Sunder Mind",
                    StringComparison.Ordinal
                ) ||
                string.Equals(
                    className,
                    "Disintegration",
                    StringComparison.Ordinal
                ) ||
                string.Equals(
                    className,
                    "Disintegrate",
                    StringComparison.Ordinal
                ) ||
                string.Equals(
                    mutationName,
                    "Disintegration",
                    StringComparison.Ordinal
                ) ||
                string.Equals(
                    mutationName,
                    "Disintegrate",
                    StringComparison.Ordinal
                );
        }



        /// <summary>
        /// Give a denizen its dimensional signature mutation, then distribute
        /// its reduced level-based mutation-point budget.
        ///
        /// When both actions are available, there is a 10% chance to buy a new
        /// random mutation and a 90% chance to increase an existing mutation
        /// rank. Buying a new mutation costs four points; raising an owned
        /// mutation one rank costs one point.
        ///
        /// If no owned mutation can still be increased, acquisition is allowed
        /// so that remaining budget can still be spent. No mutation may be
        /// intentionally raised above floor(final level / 2).
        ///
        /// Existing blueprint mutations are legitimate recipients of rank
        /// increases. The dimensional signature mutation is guaranteed even
        /// when a low-level denizen's reduced budget is smaller than its normal
        /// four-point acquisition cost.
        /// </summary>
        internal static void ApplyRandomMutationPackage(
            GameObject creature,
            string signatureClass
        )
        {
            if (creature == null)
                return;

            int finalLevel = Math.Max(1, creature.Stat("Level"));
            int rankCap = Math.Max(1, finalLevel / 2);
            int initialBudget = GetMutationPointBudgetForLevel(finalLevel);
            int remaining = initialBudget;

            
            Mutations mutations = creature.RequirePart<Mutations>();
            


            if (!signatureClass.IsNullOrEmpty() &&
                !creature.HasPart(signatureClass))
            {
                try
                {
                    mutations.AddMutation(signatureClass, 1);
                    remaining = Math.Max(0, remaining - 4);
                }
                catch (Exception ex)
                {
                    MetricsManager.LogException(
                        "Subterranean Sites EP signature mutation " +
                        signatureClass,
                        ex
                    );
                }
            }

            // Once random acquisition has proved impossible, stop trying to
            // buy and spend the remainder on legal rank increases.
            bool canTryBuying = true;
            int safety = 512;

            while (remaining > 0 && safety-- > 0)
            {
                List<BaseMutation> levelable = new List<BaseMutation>();
                foreach (BaseMutation mutation in mutations.MutationList)
                {
                    if (
                        mutation != null &&
                        !IsExcludedRandomDenizenMutation(mutation) &&
                        mutation.BaseLevel < rankCap &&
                        mutation.CanIncreaseLevel()
                    )
                    {
                        levelable.Add(mutation);
                    }
                }

                bool canLevel = levelable.Count > 0;
                bool canBuy = canTryBuying && remaining >= 4;

                if (!canLevel && !canBuy)
                    break;

                bool buyMutation;
                if (!canLevel)
                    buyMutation = true;
                else if (!canBuy)
                    buyMutation = false;
                else
                    //
                    // Strongly prefer developing the denizen's existing kit.
                    // New random powers should be occasional additions rather
                    // than the defining feature of every EP inhabitant.
                    //
                    // If all currently owned mutations have reached their
                    // legal rank cap, the branch above still allows a new
                    // mutation to be purchased.
                    //
                    buyMutation =
                        Stat.Random(
                            1,
                            10
                        ) == 1;

                if (buyMutation)
                {
                    try
                    {
                        //
                        // Snapshot the owned mutations so we can identify
                        // exactly what RandomlyMutate added.
                        //
                        List<BaseMutation> beforePurchase =
                            new List<BaseMutation>(
                                mutations.MutationList
                            );


                        var added =
                            MutationsAPI.RandomlyMutate(
                                creature
                            );


                        if (added == null)
                        {
                            canTryBuying = false;
                            continue;
                        }


                        BaseMutation purchasedMutation =
                            null;


                        foreach (
                            BaseMutation mutation
                            in mutations.MutationList
                        )
                        {
                            if (
                                mutation != null &&
                                !beforePurchase.Contains(
                                    mutation
                                )
                            )
                            {
                                purchasedMutation =
                                    mutation;

                                break;
                            }
                        }


                        //
                        // Sunder Mind is particularly inappropriate for
                        // confined EP combat, and Disintegration is also
                        // excluded from the random denizen package.
                        //
                        // Remove a forbidden roll and retry without charging
                        // mutation points.
                        //
                        if (
                            IsExcludedRandomDenizenMutation(
                                purchasedMutation
                            )
                        )
                        {
                            mutations.RemoveMutation(
                                purchasedMutation
                            );

                            continue;
                        }


                        remaining -= 4;
                    }
                    catch (Exception ex)
                    {
                        canTryBuying = false;

                        MetricsManager.LogException(
                            "Subterranean Sites EP random mutation purchase",
                            ex
                        );
                    }
                }
                else
                {
                    BaseMutation selected =
                        levelable[Stat.Random(0, levelable.Count - 1)];

                    mutations.LevelMutation(
                        selected,
                        selected.BaseLevel + 1
                    );
                    remaining -= 1;
                }
            }

            creature.SetIntProperty(
                "SubterraneanSiteDevEPMutationBudget",
                initialBudget
            );
            creature.SetIntProperty(
                "SubterraneanSiteDevEPMutationSpent",
                initialBudget - remaining
            );
            creature.SetIntProperty(
                "SubterraneanSiteDevEPMutationRankCap",
                rankCap
            );
            creature.SetStringProperty(
                "SubterraneanSiteDevEPSignatureMutation",
                signatureClass ?? ""
            );
        }

        private static void EnsureDenizenLootCandidate(GameObject creature)
        {
            if (creature == null || creature.GetMostValuableItem() != null)
                return;

            GameObject randomItem = EncountersAPI.GetAnItem();
            if (randomItem == null)
                return;

            creature.RequirePart<Inventory>().AddObject(randomItem);
        }

        internal static void ConfigureDenizenLoot(
            GameObject creature
        )
        {
            if (creature == null)
                return;


            //
            // ApplyExtradimensionalIdentity installs vanilla
            // ExtradimensionalLoot as part of the creature's dimensional
            // identity.
            //
            // True EP denizens replace it with our very small wrapper.
            // Decorative creatures retain ordinary vanilla behavior.
            //
            creature.RemovePart<
                XRL.World.Parts.ExtradimensionalLoot
            >();


            creature.RequirePart<
                XRL.World.Parts
                    .SubterraneanSiteDevEPDenizenLoot
            >();


            //
            // Preserve the existing EP guarantee that even a denizen
            // generated without ordinary equipment has something that
            // can participate in the vanilla 5% loot roll.
            //
            EnsureDenizenLootCandidate(
                creature
            );


            //
            // Bind the inventory that exists right now.
            //
            // The wrapper repeats this immediately before death so stock
            // acquired later is covered too.
            //
            BindDenizenLootToExistence(
                creature
            );
        }

        internal static void BindDenizenLootToExistence(GameObject creature)
        {
            if (creature == null)
                return;

            List<GameObject> items = creature.GetInventoryAndEquipment();

            foreach (GameObject item in items)
            {
                if (item == null ||
                    item.IsNatural() ||
                    item.Physics == null ||
                    !item.Physics.IsReal)
                {
                    continue;
                }

                MakeTemporaryEvent.Send(
                    item,
                    Duration: -1,
                    TurnInto: null,
                    DependsOn: creature,
                    RootObjectValidateEveryTurn: false
                );
            }
        }

        /// <summary>
        /// Apply one specific vanilla generated dimension to an item.
        ///
        /// This is shared EP infrastructure for treasure/relic systems.
        /// It deliberately supplies the selected dimension's exact vanilla
        /// indices instead of using ModExtradimensional's parameterless
        /// constructor, which would choose a random dimension.
        /// </summary>
        internal static bool ApplyDimensionToItem(
            GameObject item,
            SubterraneanSiteDevDimensionBinding dimension
        )
        {
            if (item == null || dimension == null)
                return false;

            if (item.GetPart<ModExtradimensional>() != null)
                return true;

            string dimensionalName =
                ResolveDimensionDisplayName(
                    dimension
                );

            if (dimensionalName.IsNullOrEmpty())
            {
                dimensionalName =
                    "an unknown dimension";
            }

            string secretID =
                dimension.SecretID ?? "";

            int weaponIndex = 0;
            int missileWeaponIndex = 0;
            int armorIndex = 0;
            int shieldIndex = 0;
            int miscIndex = 0;
            string training = null;

            if (dimension.ExtraDimension != null)
            {
                ExtraDimension d = dimension.ExtraDimension;

                secretID = d.SecretID;
                weaponIndex = d.WeaponIndex;
                missileWeaponIndex = d.MissileWeaponIndex;
                armorIndex = d.ArmorIndex;
                shieldIndex = d.ShieldIndex;
                miscIndex = d.MiscIndex;
                training = d.Training;
            }
            else if (dimension.PsychicFaction != null)
            {
                PsychicFaction d = dimension.PsychicFaction;

                secretID = d.dimensionSecretID;
                weaponIndex = d.dimensionalWeaponIndex;
                missileWeaponIndex =
                    d.dimensionalMissileWeaponIndex;
                armorIndex = d.dimensionalArmorIndex;
                shieldIndex = d.dimensionalShieldIndex;
                miscIndex = d.dimensionalMiscIndex;
                training = d.dimensionalTraining;
            }

            return item.ApplyModification(
                new ModExtradimensional(
                    weaponIndex,
                    missileWeaponIndex,
                    armorIndex,
                    shieldIndex,
                    miscIndex,
                    training,
                    dimensionalName,
                    secretID
                )
            );
        }

        /// <summary>
        /// Apply the generated vanilla extradimensional identity shared by EP
        /// denizens and living Category-5 decorations.
        ///
        /// This applies only extradimensional identity:
        /// - dimensional color/name treatment
        /// - Extradimensional part
        /// - vanilla ExtradimensionalLoot behavior
        /// - dimension-secret reveal behavior
        /// - common EP dimension/theme metadata
        ///
        /// It deliberately does NOT apply EP-denizen adaptation, mutation packages,
        /// Playerhater allegiance, forced mobility, aquatic changes, or denizen
        /// inventory rules.
        /// </summary>
        internal static void ApplyExtradimensionalIdentity(
            GameObject creature,
            SubterraneanSiteDevDimensionBinding dimension
        )
        {
            if (
                creature == null ||
                dimension == null
            )
            {
                return;
            }

            string dimensionalName =
                ResolveDimensionDisplayName(
                    dimension
                );

            if (dimensionalName.IsNullOrEmpty())
            {
                dimensionalName =
                    "an unknown dimension";
            }

            string secretID =
                dimension.SecretID ?? "";

            string mainColor =
                "O";

            int weaponIndex = 0;
            int missileWeaponIndex = 0;
            int armorIndex = 0;
            int shieldIndex = 0;
            int miscIndex = 0;

            string training = null;

            if (dimension.ExtraDimension != null)
            {
                ExtraDimension d =
                    dimension.ExtraDimension;

                mainColor =
                    d.mainColor;

                secretID =
                    d.SecretID;

                weaponIndex =
                    d.WeaponIndex;

                missileWeaponIndex =
                    d.MissileWeaponIndex;

                armorIndex =
                    d.ArmorIndex;

                shieldIndex =
                    d.ShieldIndex;

                miscIndex =
                    d.MiscIndex;

                training =
                    d.Training;
            }
            else if (dimension.PsychicFaction != null)
            {
                PsychicFaction d =
                    dimension.PsychicFaction;

                mainColor =
                    d.mainColor;

                secretID =
                    d.dimensionSecretID;

                weaponIndex =
                    d.dimensionalWeaponIndex;

                missileWeaponIndex =
                    d.dimensionalMissileWeaponIndex;

                armorIndex =
                    d.dimensionalArmorIndex;

                shieldIndex =
                    d.dimensionalShieldIndex;

                miscIndex =
                    d.dimensionalMiscIndex;

                training =
                    d.dimensionalTraining;
            }

            if (
                creature.Render != null &&
                !mainColor.IsNullOrEmpty()
            )
            {
                creature.Render
                    .SetForegroundColor(
                        mainColor
                    );

                creature.Render.DetailColor =
                    "O";
            }

            if (!mainColor.IsNullOrEmpty())
            {
                creature
                    .RequirePart<DisplayNameColor>()
                    .SetColorAndPriority(
                        "O",
                        30
                    );
            }

            if (
                creature.GetPart<Extradimensional>() ==
                null
            )
            {
                creature.AddPart<Extradimensional>(
                    new Extradimensional(
                        "{{O|" +
                        dimensionalName +
                        "}}",
                        weaponIndex,
                        missileWeaponIndex,
                        armorIndex,
                        shieldIndex,
                        miscIndex,
                        training,
                        secretID
                    )
                );
            }

            //
            // This is part of vanilla extradimensional identity, not an EP-denizen
            // mutation/adaptation rule. Decorative creatures normally have little or
            // no loot, but retaining the vanilla part keeps the identity complete.
            //
            creature.RequirePart<
                ExtradimensionalLoot
            >();

            if (
                !secretID.IsNullOrEmpty() &&
                creature.GetPart<
                    RevealObservationOnLook
                >() == null
            )
            {
                creature.AddPart<
                    RevealObservationOnLook
                >(
                    new RevealObservationOnLook(
                        secretID
                    )
                );
            }

            creature.SetStringProperty(
                "SubterraneanSiteDevEPDimensionSecretID",
                secretID
            );

            creature.SetStringProperty(
                "SubterraneanSiteDevEPTheme",
                dimension.ThemeLabel
            );
        }

        /// <summary>
        /// Apply a Category-5 theme's extradimensional identity and assigned faction
        /// to a living decorative object.
        ///
        /// Decorative creatures belong to the dimension represented by the Category-5
        /// theme but remain otherwise ordinary versions of their vanilla blueprint.
        /// They do not receive EP-denizen resistances, mutation packages, forced
        /// hostility, movement changes, or denizen loot treatment.
        /// </summary>
        internal static void ApplyDecorationDimensionIdentity(
            GameObject creature,
            string themeKey
        )
        {
            if (
                creature == null ||
                themeKey.IsNullOrEmpty()
            )
            {
                return;
            }

            SubterraneanSiteDevDimensionBinding dimension =
                GetBindingByThemeKey(
                    themeKey
                );

            if (dimension == null)
                return;

            ApplyExtradimensionalIdentity(
                creature,
                dimension
            );

            //
            // Replace the blueprint's ordinary faction allegiance with the faction
            // assigned to this generated extradimensional identity.
            //
            // Do NOT force Hostile/Calm/Mobile/Aquatic here. Those properties remain
            // whatever the decorative creature's vanilla blueprint specifies.
            //
            if (
                creature.Brain != null &&
                !dimension.FactionName.IsNullOrEmpty()
            )
            {
                creature.Brain.Allegiance.Clear();

                creature.Brain.Allegiance.Add(
                    dimension.FactionName,
                    100
                );
            }

            creature.SetStringProperty(
                "SubterraneanSiteDevEPDecorationFaction",
                dimension.FactionName ?? ""
            );
        }

        /// <summary>
        /// Remove vanilla behaviors that create uncontrolled secondary content
        /// from generated EP inhabitants.
        ///
        /// This is intentionally NOT called by ApplyDecorationDimensionIdentity.
        /// Theme-owned living decorations such as Ooze weeps keep their normal
        /// behavior; only creatures recruited into the EP inhabitant/denizen
        /// population are sanitized.
        ///
        /// LiquidProducer is the first confirmed case. A liquid-weep faction
        /// member should be allowed to appear as a denizen without manufacturing
        /// ordinary liquid-pool objects inside the EP.
        /// </summary>
        internal static void SanitizePocketInhabitantSpawnBehavior(
            GameObject creature
        )
        {
            if (creature == null)
                return;


            //
            // Environmental producer behavior.
            //
            // A liquid-weep faction member may be selected as an EP denizen,
            // but it should not manufacture ordinary liquid pools inside the
            // extradimensional pocket.
            //
            if (
                creature.HasPart<
                    LiquidProducer
                >()
            )
            {
                creature.RemovePart<
                    LiquidProducer
                >();
            }


            //
            // Vanilla entourage behavior.
            //
            // EP population generation deliberately selects the denizen itself.
            // Do not allow that selected creature to subsequently manufacture
            // ordinary non-dimensional followers when it enters a cell.
            //
            // DromadCaravan creates saltbacks and caravan guards.
            //
            if (
                creature.HasPart<
                    DromadCaravan
                >()
            )
            {
                creature.RemovePart<
                    DromadCaravan
                >();
            }


            //
            // GoatfolkClan1 creates a goatfolk clan around the selected
            // hero/shaman on its first EnteredCell event.
            //
            if (
                creature.HasPart<
                    GoatfolkClan1
                >()
            )
            {
                creature.RemovePart<
                    GoatfolkClan1
                >();
            }

                        //
            // Snapjaw heroes can manufacture an ordinary snapjaw pack after
            // being selected as a single EP denizen.
            //
            if (
                creature.HasPart<
                    SnapjawPack1
                >()
            )
            {
                creature.RemovePart<
                    SnapjawPack1
                >();
            }


            //
            // Baboon heroes can manufacture an ordinary baboon pack after
            // being selected as a single EP denizen.
            //
            if (
                creature.HasPart<
                    BaboonHero1Pack
                >()
            )
            {
                creature.RemovePart<
                    BaboonHero1Pack
                >();
            }


            //
            // Eyeless king crabs carry their own skuttle-generation behavior.
            // A crab selected as an EP denizen should remain the deliberately
            // selected crab rather than producing ordinary crab companions.
            //
            if (
                creature.HasPart<
                    EyelessKingCrabSkuttle1
                >()
            )
            {
                creature.RemovePart<
                    EyelessKingCrabSkuttle1
                >();
            }

            //
            // HeroMaker can add vanilla bodyguard behavior to promoted
            // creatures through HeroHasGuards. EP inhabitants are selected
            // individually, so they should not manufacture ordinary guards
            // after promotion.
            //
            if (
                creature.HasPart<
                    HasGuards
                >()
            )
            {
                creature.RemovePart<
                    HasGuards
                >();
            }
        }

        /// <summary>
        /// Apply the generated dimensional identity, dimensional mutation
        /// package, theme-owned native adaptation, hostility, and denizen loot
        /// behavior.
        ///
        /// The denizen provider owns the theme's signature mutation and
        /// permanent native adaptation. Shared dimensional code owns the
        /// common mutation-budget and denizen machinery.
        /// </summary>
        internal static void ApplyDimensionIdentityAndAdaptation(
            GameObject creature,
            SubterraneanSiteDevDimensionBinding dimension,
            string denizenProviderType
        )
        {
            if (
                creature == null ||
                dimension == null
            )
            {
                return;
            }

            SanitizePocketInhabitantSpawnBehavior(
                creature
            );



            ApplyExtradimensionalIdentity(
                creature,
                dimension
            );


            ISubterraneanSiteDevEPDenizenAdaptationProvider
                denizenProvider =
                    null;


            SubterraneanSiteDevEPProviderFactory
                .TryCreate(
                    denizenProviderType,
                    out denizenProvider
                );


            ISubterraneanSiteDevEPSignatureMutationProvider
                signatureProvider =
                    denizenProvider
                        as ISubterraneanSiteDevEPSignatureMutationProvider;


            string signatureClass =
                signatureProvider == null
                    ? ""
                    : signatureProvider.SignatureMutationClass;


            //
            // Signature + random mutation package uses the denizen's final
            // level after any upward tier repair / hero promotion.
            //
            ApplyRandomMutationPackage(
                creature,
                signatureClass
            );


            //
            // Permanent native adaptations belong to the dimensional theme.
            //
            if (denizenProvider != null)
            {
                denizenProvider
                    .ApplyDenizenAdaptation(
                        creature
                    );
            }


            //
            // EP denizens are inhabitants of the pocket rather than members of
            // the source faction's normal Qud ecology.
            //
            if (creature.Brain != null)
            {
                creature.Brain.Allegiance.Clear();

                creature.Brain.Allegiance.Add(
                    "Playerhater",
                    100
                );

                creature.Brain.Allegiance.Hostile = true;
                creature.Brain.Allegiance.Calm = false;
                creature.Brain.Hibernating = false;
                creature.Brain.Aquatic = false;
                creature.Brain.Mobile = true;
            }

            ConfigureMerchantStock(
                creature,
                dimension
            );


            ConfigureDenizenLoot(
                creature
            );


            creature.SetStringProperty(
                "SubterraneanSiteDevEPTheme",
                dimension.ThemeLabel
            );

            creature.SetStringProperty(
                "SubterraneanSiteDevEPDenizenFaction",
                dimension.FactionName ?? ""
            );
        }

        private static void ConfigureMerchantStock(
            GameObject creature,
            SubterraneanSiteDevDimensionBinding dimension
        )
        {
            if (
                creature == null ||
                dimension == null ||
                The.Game == null
            )
            {
                return;
            }


            GenericInventoryRestocker restocker =
                creature.GetPart<
                    GenericInventoryRestocker
                >();


            //
            // GenericInventoryRestocker is our functional marker that this
            // denizen carries merchant merchandise rather than ordinary
            // creature inventory.
            //
            if (restocker == null)
                return;


            //
            // Keep all future vanilla restocks constrained too.
            //
            SubterraneanSiteDevEPMerchantStockController controller =
                creature.RequirePart<
                    SubterraneanSiteDevEPMerchantStockController
                >();


            controller.DimensionThemeKey =
                dimension.ThemeLabel;

            controller.MinimumStock =
                2;

            controller.MaximumStock =
                3;


            //
            // Normalize whatever merchandise already exists now.
            //
            // If vanilla has not stocked the merchant yet, this may do little
            // or nothing; the persistent controller will catch the later
            // StockedEvent.
            //
            int seed =
                XRLCore.Core.Game.GetWorldSeed(
                    "SubterraneanSiteDev:EPMerchantInitialStock:" +
                    creature.ID + ":" +
                    dimension.ThemeLabel
                );


            System.Random rng =
                new System.Random(
                    seed
                );


            SubterraneanSiteDevEPMerchantStockControl
                .TrimAndDimensionalizeStock(
                    creature,
                    dimension.ThemeLabel,
                    2,
                    3,
                    rng
                );
        }


       

        private static string FormatTierSet(HashSet<int> tiers)
        {
            if (tiers == null || tiers.Count == 0)
                return "-";

            List<int> ordered = new List<int>(tiers);
            ordered.Sort();

            StringBuilder text = new StringBuilder();
            for (int i = 0; i < ordered.Count; i++)
            {
                if (i > 0)
                    text.Append(',');
                text.Append(ordered[i].ToString());
            }

            return text.ToString();
        }

        private static DimensionManager GetDimensionManager()
        {
            if (The.Game == null)
                return null;

            return The.Game.GetObjectGameState("DimensionManager")
                as DimensionManager;
        }

        private static List<SubterraneanSiteDevDimensionBinding>
            EnumerateVanillaDimensions(
                DimensionManager manager,
                bool readAssignedSlots
            )
        {
            List<SubterraneanSiteDevDimensionBinding> result =
                new List<SubterraneanSiteDevDimensionBinding>();

            if (manager == null)
                return result;

            int reportIndex = 1;

            // Report the eight ordinary dimensions first because that mirrors
            // DimensionManager.ExtraDimensions and makes indices 1-8 easy to
            // compare against vanilla solo extradimensional hunters.
            if (manager.ExtraDimensions != null)
            {
                for (int i = 0; i < manager.ExtraDimensions.Count; i++)
                {
                    ExtraDimension extra = manager.ExtraDimensions[i];
                    if (extra == null)
                        continue;

                    SubterraneanSiteDevDimensionBinding binding =
                        new SubterraneanSiteDevDimensionBinding
                        {
                            ReportIndex = reportIndex++,
                            SecretID = extra.SecretID,
                            DimensionName = extra.Name,
                            VanillaFactionName = "",
                            FactionName = "",
                            FactionWasAssignedBySubSites = true,
                            FactionReplacesUnusableVanillaFaction = false,
                            ExtraDimension = extra,
                            PsychicFaction = null,
                            ThemeSlot = -1
                        };

                    if (readAssignedSlots)
                    {
                        binding.ThemeSlot = ReadAssignedSlot(binding);
                        binding.FactionName = ReadAssignedFaction(binding);
                    }

                    result.Add(binding);
                }
            }

            // The remaining three are vanilla PsychicFaction objects, each of
            // which carries a separate generated dimension plus the name of the
            // existing faction used as that cult's creature pool.
            if (manager.PsychicFactions != null)
            {
                for (int i = 0; i < manager.PsychicFactions.Count; i++)
                {
                    PsychicFaction psychic = manager.PsychicFactions[i];
                    if (psychic == null)
                        continue;

                    SubterraneanSiteDevDimensionBinding binding =
                        new SubterraneanSiteDevDimensionBinding
                        {
                            ReportIndex = reportIndex++,
                            SecretID = psychic.dimensionSecretID,
                            DimensionName = psychic.dimensionName,
                            VanillaFactionName = psychic.factionName,
                            FactionName = psychic.factionName,
                            FactionWasAssignedBySubSites = false,
                            FactionReplacesUnusableVanillaFaction = false,
                            ExtraDimension = null,
                            PsychicFaction = psychic,
                            ThemeSlot = -1
                        };

                    if (readAssignedSlots)
                    {
                        binding.ThemeSlot = ReadAssignedSlot(binding);

                        string assignedFaction = ReadAssignedFaction(binding);
                        if (!assignedFaction.IsNullOrEmpty())
                        {
                            binding.FactionName = assignedFaction;
                            binding.FactionReplacesUnusableVanillaFaction =
                                !binding.VanillaFactionName.IsNullOrEmpty() &&
                                !string.Equals(
                                    binding.VanillaFactionName,
                                    assignedFaction,
                                    StringComparison.OrdinalIgnoreCase
                                );
                            binding.FactionWasAssignedBySubSites =
                                binding.FactionReplacesUnusableVanillaFaction;
                        }
                    }

                    result.Add(binding);
                }
            }

            return result;
        }

        private static int ReadAssignedSlot(
            SubterraneanSiteDevDimensionBinding binding
        )
        {
            if (The.Game == null || binding == null)
                return -1;

            string text = The.Game.GetStringGameState(
                GetSlotStateKey(binding)
            );

            int slot;
            if (!int.TryParse(text, out slot))
                return -1;

            if (slot < 0 || slot >= ThemeSlotLabels.Length)
                return -1;

            return slot;
        }

        private static string ReadAssignedFaction(
            SubterraneanSiteDevDimensionBinding binding
        )
        {
            if (The.Game == null || binding == null)
                return "";

            return The.Game.GetStringGameState(GetFactionStateKey(binding));
        }

        private static string GetFactionStateKey(
            SubterraneanSiteDevDimensionBinding binding
        )
        {
            string identity = binding == null
                ? "UNKNOWN"
                : binding.SecretID;

            if (identity.IsNullOrEmpty() && binding != null)
            {
                identity =
                    binding.KindLabel + "_" +
                    binding.ReportIndex.ToString() + "_" +
                    binding.DimensionName;
            }

            return FactionStatePrefix + identity;
        }

        private static string GetLegacyFactionStateKey(
            SubterraneanSiteDevDimensionBinding binding
        )
        {
            string identity = binding == null
                ? "UNKNOWN"
                : binding.SecretID;

            if (identity.IsNullOrEmpty() && binding != null)
            {
                identity =
                    binding.KindLabel + "_" +
                    binding.ReportIndex.ToString() + "_" +
                    binding.DimensionName;
            }

            return LegacyFactionStatePrefix + identity;
        }

        private static string GetSlotStateKey(
            SubterraneanSiteDevDimensionBinding binding
        )
        {
            string identity = binding == null
                ? "UNKNOWN"
                : binding.SecretID;

            if (identity.IsNullOrEmpty() && binding != null)
            {
                identity =
                    binding.KindLabel + "_" +
                    binding.ReportIndex.ToString() + "_" +
                    binding.DimensionName;
            }

            return SlotStatePrefix + identity;
        }
    }

    /// <summary>
    /// Development wishes for the dimension/faction assignment experiment.
    ///
    /// Commands:
    ///   subsites:dimensions
    ///   subsites:invader:DIMENSION:tier:TIER[:COUNT]
    ///   subsites:invader:DIMENSION:t:TIER[:COUNT]
    ///   subsites:invader:DIMENSION:level:LEVEL[:COUNT]
    ///   subsites:invader:DIMENSION:l:LEVEL[:COUNT]
    ///
    /// DIMENSION is the 1-11 index printed by subsites:dimensions.
    /// Tier mode draws only exact-tier eligible members of the dimension's
    /// assigned faction. Level mode reproduces vanilla PsychicFaction selection
    /// against the supplied level instead of the player's actual level.
    /// Generated test objects are reported and immediately destroyed.
    /// </summary>
    [HasWishCommand]
    public static class SubterraneanSiteDevDimensionWishes
    {
        [WishCommand("subsites:dimensions", null)]
        public static void ShowDimensions()
        {
            List<SubterraneanSiteDevDimensionBinding> bindings =
                SubterraneanSiteDevDimensionEngine.GetBindings();

            if (bindings.Count == 0)
            {
                Popup.Show(
                    "Subterranean Site Dev\n\n" +
                    "DimensionManager is not available yet."
                );
                return;
            }

            StringBuilder text = new StringBuilder();

            text.AppendLine("Subterranean Site Dev - dimensions");
            text.AppendLine();
            text.AppendLine(
                "Index | theme slot | vanilla kind | dimension | denizen faction"
            );
            text.AppendLine();

            foreach (SubterraneanSiteDevDimensionBinding binding in bindings)
            {
                text.Append(binding.ReportIndex.ToString("00"));
                text.Append(" | ");
                text.Append(binding.ThemeLabel);
                text.Append(" | ");
                text.Append(
                    binding.IsPsychicFactionDimension
                        ? "PSYCHIC"
                        : "EXTRA"
                );
                text.Append(" | ");
                text.Append(binding.DimensionName);
                text.Append(" | faction: ");
                text.Append(binding.FactionName.IsNullOrEmpty()
                    ? "UNASSIGNED"
                    : binding.FactionName);
                text.Append(" [");
                text.Append(binding.FactionSourceLabel);
                text.Append("]");
                if (binding.FactionReplacesUnusableVanillaFaction)
                {
                    text.Append(" (vanilla psychic faction: ");
                    text.Append(binding.VanillaFactionName);
                    text.Append(")");
                }
                text.AppendLine();
            }

            text.AppendLine();
            text.AppendLine(
                "VANILLA = usable faction from one of Qud's three PsychicFaction assignments."
            );
            text.AppendLine(
                "ASSIGNED = Sub-Sites drew a usable faction for an ordinary " +
                "ExtraDimension from Qud's same potentially-extradimensional source."
            );
            text.AppendLine(
                "REPLACEMENT = vanilla PsychicFaction had no usable creature " +
                "members, so its lore faction is preserved but EP denizens use " +
                "a separate usable faction."
            );

            Popup.Show(text.ToString());
        }

        [WishCommand(
            null,
            null,
            Regex = @"^subsites:invader:\s*(1[01]|[1-9]):\s*(tier|t|level|l):\s*(\d+)(?::(\d+))?$"
        )]
        public static bool RollInvaderFromDimension(Match match)
        {
            int dimensionIndex = 1;
            int requestedValue = 1;
            int count = 10;
            string mode = "tier";

            if (match != null && match.Groups.Count > 1)
            {
                int parsed;
                if (int.TryParse(match.Groups[1].Value, out parsed))
                    dimensionIndex = parsed;
            }

            if (match != null && match.Groups.Count > 2)
                mode = match.Groups[2].Value.ToLowerInvariant();

            if (match != null && match.Groups.Count > 3)
            {
                int parsed;
                if (int.TryParse(match.Groups[3].Value, out parsed))
                    requestedValue = parsed;
            }

            if (match != null &&
                match.Groups.Count > 4 &&
                match.Groups[4].Success)
            {
                int parsed;
                if (int.TryParse(match.Groups[4].Value, out parsed))
                    count = parsed;
            }

            bool tierMode = mode == "tier" || mode == "t";

            if (tierMode)
            {
                if (requestedValue < 1)
                    requestedValue = 1;
                if (requestedValue > 8)
                    requestedValue = 8;
            }
            else
            {
                if (requestedValue < 1)
                    requestedValue = 1;
                if (requestedValue > 100)
                    requestedValue = 100;
            }

            if (count < 1)
                count = 1;
            if (count > 50)
                count = 50;

            SubterraneanSiteDevDimensionBinding binding =
                SubterraneanSiteDevDimensionEngine
                    .GetBindingByReportIndex(dimensionIndex);

            if (binding == null)
            {
                Popup.Show(
                    "Subterranean Site Dev\n\n" +
                    "No vanilla dimension exists at index " +
                    dimensionIndex.ToString() + ".\n\n" +
                    "Use subsites:dimensions first."
                );
                return true;
            }

            StringBuilder text = new StringBuilder();

            text.AppendLine(
                "Subterranean Site Dev - dimension faction pull"
            );
            text.AppendLine();
            text.AppendLine(
                "Dimension " + binding.ReportIndex.ToString() +
                ": " + binding.DimensionName
            );
            text.AppendLine("Vanilla kind: " + binding.KindLabel);
            text.AppendLine("Theme slot: " + binding.ThemeLabel);
            text.AppendLine(
                "Faction: " + binding.FactionName +
                " [" + binding.FactionSourceLabel + "]"
            );
            text.AppendLine(
                tierMode
                    ? "Selection: exact blueprint Tier " + requestedValue.ToString()
                    : "Selection: vanilla-style target Level " + requestedValue.ToString()
            );
            text.AppendLine();
            text.AppendLine("Base-creature rolls:");

            for (int i = 0; i < count; i++)
            {
                string source;
                GameObject creature;

                if (tierMode)
                {
                    creature = SubterraneanSiteDevDimensionEngine
                        .CreateFactionCreatureAtTier(
                            binding,
                            requestedValue,
                            out source
                        );
                }
                else
                {
                    creature = SubterraneanSiteDevDimensionEngine
                        .CreateFactionCreatureAtLevel(
                            binding,
                            requestedValue,
                            out source
                        );
                }

                text.Append((i + 1).ToString());
                text.Append(". ");

                if (creature == null)
                {
                    text.Append("NO CREATURE");
                }
                else
                {
                    text.Append(
                        creature.Blueprint.IsNullOrEmpty()
                            ? creature.DisplayName
                            : creature.Blueprint
                    );

                    GameObjectBlueprint blueprint =
                        GameObjectFactory.Factory.GetBlueprint(creature.Blueprint);

                    if (blueprint != null)
                    {
                        text.Append(" | T");
                        text.Append(blueprint.Tier.ToString());

                        if (blueprint.HasStat("Level"))
                        {
                            text.Append(" L");
                            text.Append(
                                blueprint.GetStat("Level").Value.ToString()
                            );
                        }
                    }
                }

                text.Append(" | ");
                text.AppendLine(source);

                if (creature != null)
                {
                    try
                    {
                        creature.Obliterate();
                    }
                    catch
                    {
                        // Debug cleanup failure should not hide the roll report.
                    }
                }
            }

            text.AppendLine();
            text.AppendLine(
                tierMode
                    ? "Exact-tier mode intentionally has NO fallback. If a " +
                      "faction has no eligible creature at that tier, the report " +
                      "says NO CREATURE so we can see coverage gaps."
                    : "Level mode mirrors the vanilla PsychicFaction preference: " +
                      "highest eligible level below the supplied target, with " +
                      "lowest eligible member as fallback."
            );

            Popup.Show(text.ToString());
            return true;
        }

        [WishCommand(
            null,
            null,
            Regex = @"^subsites:eligibility:\s*([1-8])$"
        )]
        public static bool ShowEligibility(Match match)
        {
            int targetTier = int.Parse(match.Groups[1].Value);

            List<SubterraneanSiteDevDimensionEligibility> evaluations =
                SubterraneanSiteDevDimensionEngine
                    .GetDimensionEligibilityTable(targetTier);

            StringBuilder text = new StringBuilder();
            text.AppendLine(
                "EP dimension eligibility - T" + targetTier.ToString()
            );
            text.AppendLine(
                "Tier level band: L" +
                SubterraneanSiteDevDimensionEngine
                    .GetMinimumLevelForTier(targetTier).ToString() +
                "-" +
                SubterraneanSiteDevDimensionEngine
                    .GetMaximumLevelForTier(targetTier).ToString()
            );
            text.AppendLine();

            int eligibleCount = 0;

            foreach (SubterraneanSiteDevDimensionEligibility evaluation in evaluations)
            {
                if (evaluation.Eligible)
                    eligibleCount++;

                SubterraneanSiteDevDimensionBinding d = evaluation.Dimension;

                text.Append(d.ReportIndex.ToString("00"));
                text.Append(" | ");
                text.Append(d.ThemeLabel);
                text.Append(" | ");
                text.Append(d.FactionName);
                text.Append(" [");
                text.Append(d.FactionSourceLabel);
                text.Append("] | tiers ");
                text.Append(evaluation.AvailableTiers);
                text.Append(" | lowest L");
                text.Append(
                    evaluation.LowestAvailableLevel == int.MaxValue
                        ? "-"
                        : evaluation.LowestAvailableLevel.ToString()
                );
                text.Append(" | ");
                text.Append(evaluation.Eligible ? "YES" : "NO");
                text.Append(" | ");
                text.AppendLine(evaluation.Rule);
            }

            text.AppendLine();
            text.AppendLine(
                "Eligible pool: " + eligibleCount.ToString() +
                "/" + evaluations.Count.ToString() +
                " dimensions"
            );
            text.AppendLine(
                "Rule: exact tier; else nearest lower tier and scale upward; " +
                "if no creature exists at/below the target tier, allow the " +
                "faction's lowest native creature only when it is within +" +
                SubterraneanSiteDevDimensionEngine.MaximumNativeOverlevel.ToString() +
                " levels of the target tier ceiling. T8 makes every dimension " +
                "with a usable denizen faction available."
            );

            Popup.Show(text.ToString());
            return true;
        }

        [WishCommand(
            null,
            null,
            Regex = @"^subsites:pair:\s*([1-8])(?::(\d+))?$"
        )]
        public static bool RollDimensionPairs(Match match)
        {
            int targetTier = int.Parse(match.Groups[1].Value);
            int count = 10;

            if (match.Groups[2].Success)
                int.TryParse(match.Groups[2].Value, out count);

            count = Math.Max(1, Math.Min(30, count));

            StringBuilder text = new StringBuilder();
            text.AppendLine(
                "EP dimension-pair test - T" + targetTier.ToString()
            );
            text.AppendLine();

            for (int i = 0; i < count; i++)
            {
                SubterraneanSiteDevDimensionPairSelection pair =
                    SubterraneanSiteDevDimensionEngine
                        .SelectDimensionPairForSite(
                            "WishPair:" + i.ToString(),
                            targetTier
                        );

                text.Append((i + 1).ToString());
                text.Append(". ");

                if (pair.Primary == null || pair.Secondary == null)
                {
                    text.AppendLine("FAILED TO SELECT TWO DIMENSIONS");
                    continue;
                }

                text.Append(
                    pair.Primary.ThemeLabel +
                    " [" + pair.Primary.FactionName + "]"
                );
                if (pair.PrimaryUsedFallback)
                    text.Append(" {FALLBACK}");

                text.Append(" + ");

                text.Append(
                    pair.Secondary.ThemeLabel +
                    " [" + pair.Secondary.FactionName + "]"
                );
                if (pair.SecondaryUsedFallback)
                    text.Append(" {FALLBACK}");

                text.Append(" | eligible pool ");
                text.AppendLine(pair.EligibleThemeCount.ToString());
            }

            text.AppendLine();
            text.AppendLine(
                "Eligible pool = how many of the 11 dimensions can legally " +
                "supply denizens at this EP tier before the Primary/Secondary " +
                "pair is rolled."
            );

            Popup.Show(text.ToString());
            return true;
        }

        [WishCommand(
            null,
            null,
            Regex = @"^subsites:denizen:\s*(1[01]|[1-9]):\s*([1-8])(?::(\d+))?$"
        )]
        public static bool RollScaledDenizens(Match match)
        {
            int dimensionIndex = int.Parse(match.Groups[1].Value);
            int targetTier = int.Parse(match.Groups[2].Value);
            int count = 10;

            if (match.Groups[3].Success)
                int.TryParse(match.Groups[3].Value, out count);

            count = Math.Max(1, Math.Min(30, count));

            SubterraneanSiteDevDimensionBinding binding =
                SubterraneanSiteDevDimensionEngine
                    .GetBindingByReportIndex(dimensionIndex);

            if (binding == null)
            {
                Popup.Show("No dimension at index " + dimensionIndex.ToString());
                return true;
            }

            SubterraneanSiteDevDimensionEligibility evaluation =
                SubterraneanSiteDevDimensionEngine
                    .EvaluateDimensionForSite(binding, targetTier);

            StringBuilder text = new StringBuilder();
            text.AppendLine("EP base-denizen tier/scaling test");
            text.AppendLine();
            text.AppendLine(
                "Dimension " + binding.ReportIndex.ToString() +
                " | " + binding.ThemeLabel +
                " | " + binding.DimensionName
            );
            text.AppendLine(
                "Denizen faction: " + binding.FactionName +
                " [" + binding.FactionSourceLabel + "]"
            );
            if (binding.FactionReplacesUnusableVanillaFaction)
            {
                text.AppendLine(
                    "Vanilla psychic faction preserved as lore: " +
                    binding.VanillaFactionName
                );
            }
            text.AppendLine("Target: T" + targetTier.ToString());
            text.AppendLine(
                "Eligibility: " +
                (evaluation.Eligible ? "YES" : "NO") +
                " | " + evaluation.Rule
            );
            text.AppendLine();

            for (int i = 0; i < count; i++)
            {
                string source;
                GameObject creature =
                    SubterraneanSiteDevDimensionEngine
                        .CreateDenizenForSite(
                            binding,
                            targetTier,
                            out source
                        );

                text.Append((i + 1).ToString());
                text.Append(". ");

                if (creature == null)
                {
                    text.Append("NO CREATURE");
                }
                else
                {
                    text.Append(creature.Blueprint);
                    text.Append(" | final L");
                    text.Append(creature.Stat("Level").ToString());
                }

                text.Append(" | ");
                text.AppendLine(source);

                if (creature != null)
                {
                    try
                    {
                        creature.Obliterate();
                    }
                    catch
                    {
                    }
                }
            }

            text.AppendLine();
            text.AppendLine(
                "Tier-only test. No player level is consulted. Lower-tier " +
                "fallback creatures are leveled only to the minimum ordinary " +
                "level for the requested tier. Exact-tier creatures retain " +
                "their native level."
            );

            Popup.Show(text.ToString());
            return true;
        }

        private static string DescribeDenizenMutations(GameObject obj)
        {
            if (obj == null)
                return "-";

            Mutations mutations = obj.GetPart<Mutations>();
            if (mutations == null || mutations.MutationList.Count == 0)
                return "-";

            StringBuilder result = new StringBuilder();
            foreach (BaseMutation mutation in mutations.MutationList)
            {
                if (mutation == null)
                    continue;

                if (result.Length > 0)
                    result.Append(", ");

                result.Append(mutation.GetDisplayName());
                result.Append(" ");
                result.Append(mutation.BaseLevel.ToString());
            }

            return result.Length == 0 ? "-" : result.ToString();
        }

        [WishCommand("subsites:denizens", null)]
        public static void ShowCurrentZoneDenizens()
        {
            Zone Z = The.ActiveZone;
            if (Z == null)
            {
                Popup.Show("No active zone.");
                return;
            }

            StringBuilder text = new StringBuilder();
            text.AppendLine("EP denizens in " + Z.ZoneID);
            text.AppendLine();

            int count = 0;
            foreach (Cell cell in Z.GetCells())
            {
                foreach (GameObject obj in cell.GetObjects())
                {
                    if (obj == null ||
                        !obj.HasStringProperty("SubterraneanSiteDevEPTheme"))
                    {
                        continue;
                    }

                    count++;
                    string theme = obj.GetStringProperty(
                        "SubterraneanSiteDevEPTheme",
                        "?"
                    );
                    string faction = obj.GetStringProperty(
                        "SubterraneanSiteDevEPDenizenFaction",
                        "?"
                    );

                    text.Append(count.ToString());
                    text.Append(". ");
                    text.Append(obj.Blueprint);
                    text.Append(" | ");
                    text.Append(theme);
                    text.Append(" | ");
                    text.Append(faction);
                    text.Append(" | T");
                    text.Append(
                        obj.GetIntProperty(
                            "SubterraneanSiteDevEPOriginalTier",
                            obj.GetBlueprint().Tier
                        ).ToString()
                    );
                    text.Append(" L");
                    int originalLevel = obj.GetIntProperty(
                        "SubterraneanSiteDevEPOriginalLevel",
                        obj.Stat("Level")
                    );
                    text.Append(originalLevel.ToString());
                    if (obj.Stat("Level") != originalLevel)
                    {
                        text.Append("->");
                        text.Append(obj.Stat("Level").ToString());
                    }
                    text.Append(" | HR ");
                    text.Append(obj.Stat("HeatResistance").ToString());
                    text.Append(" | CR ");
                    text.Append(obj.Stat("ColdResistance").ToString());
                    text.Append(" | MP ");
                    text.Append(
                        obj.GetIntProperty(
                            "SubterraneanSiteDevEPMutationSpent",
                            0
                        ).ToString()
                    );
                    text.Append("/");
                    text.Append(
                        obj.GetIntProperty(
                            "SubterraneanSiteDevEPMutationBudget",
                            0
                        ).ToString()
                    );
                    text.Append(" cap ");
                    text.Append(
                        obj.GetIntProperty(
                            "SubterraneanSiteDevEPMutationRankCap",
                            0
                        ).ToString()
                    );
                    text.Append(" | Mobile ");
                    text.Append(
                        obj.Brain != null && obj.Brain.Mobile
                            ? "Y"
                            : "N"
                    );
                    text.Append(" Aquatic ");
                    text.Append(
                        obj.Brain != null && obj.Brain.Aquatic
                            ? "Y"
                            : "N"
                    );
                    text.AppendLine();
                    text.Append("    mutations: ");
                    text.AppendLine(DescribeDenizenMutations(obj));
                }
            }

            if (count == 0)
                text.AppendLine("No EP denizens found in this zone.");

            Popup.Show(text.ToString());
        }

    }
}

namespace XRL.World.Parts
{
    /// <summary>
    /// EP-denizen version of vanilla ExtradimensionalLoot.
    ///
    /// Vanilla already owns the actual 5% extradimensional-loot rule.
    ///
    /// EP adds only one requirement:
    /// immediately before vanilla chooses its possible survivor,
    /// make sure every currently-carried/equipped real item is
    /// existence-bound to the denizen.
    ///
    /// This catches inventory generated or acquired after the denizen's
    /// initial EP conversion, including merchant restocks.
    /// </summary>
    [Serializable]
    public class SubterraneanSiteDevEPDenizenLoot :
        ExtradimensionalLoot
    {
        public override bool FireEvent(
            Event E
        )
        {
            if (
                E != null &&
                E.ID ==
                    "BeforeDeathRemoval"
            )
            {
                try
                {
                    SubterraneanSiteDev
                        .SubterraneanSiteDevDimensionEngine
                        .BindDenizenLootToExistence(
                            ParentObject
                        );
                }
                catch (Exception ex)
                {
                    MetricsManager.LogException(
                        "Subterranean Sites EP denizen death loot binding",
                        ex
                    );
                }
            }


            //
            // Vanilla now performs its normal behavior:
            //
            // 5% chance
            // -> GetMostValuableItem()
            // -> SplitStack(1)
            // -> remove Temporary / ExistenceSupport
            // -> apply ModExtradimensional
            // -> drop the rescued item
            //
            return base.FireEvent(
                E
            );
        }
    }
}

namespace XRL.World.ZoneBuilders
{

    /// <summary>
    /// Shared EP treasure container pass.
    ///
    /// Every underground EP layer receives one container drawn directly from
    /// Qud's vanilla tier-appropriate dynamic chest table. The selected
    /// blueprint is allowed to populate its inventory normally.
    ///
    /// One of those normally generated inventory entries is then selected
    /// completely at random. All other generated contents are discarded,
    /// and the surviving item/stack is assigned to one of the pocket's two
    /// underlying dimensions.
    ///
    /// This deliberately does not prefer equipment, valuable items, or useful
    /// items. The ordinary vanilla chest roll remains responsible for deciding
    /// what the player might receive.
    /// </summary>
    public class SubterraneanSiteDevEPChestBuilder : ZoneBuilderSandbox
    {
        public int Tier = 1;

        public string PrimaryThemeKey = "";
        public string SecondaryThemeKey = "";

        private const int TransitionExclusionRadius = 4;

        public bool BuildZone(Zone Z)
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

            SubterraneanSiteDev
                .SubterraneanSiteDevDimensionBinding primary =
                    SubterraneanSiteDev
                        .SubterraneanSiteDevDimensionEngine
                        .GetBindingByThemeKey(
                            PrimaryThemeKey
                        );

            SubterraneanSiteDev
                .SubterraneanSiteDevDimensionBinding secondary =
                    SubterraneanSiteDev
                        .SubterraneanSiteDevDimensionEngine
                        .GetBindingByThemeKey(
                            SecondaryThemeKey
                        );

            if (
                primary == null &&
                secondary == null
            )
            {
                return true;
            }

            Cell destination =
                PickPlacementCell(
                    Z
                );

            if (destination == null)
                return true;

            GameObject container =
                null;

            //
            // Some vanilla chest blueprints can legitimately finish creation with
            // no inventory. Do not leave an empty EP reward chest behind.
            //
            // Roll from the ordinary dynamic chest table repeatedly until one of the
            // actual vanilla containers produces at least one item/stack.
            //
            const int DynamicChestAttempts =
                12;

            for (
                int attempt = 0;
                attempt < DynamicChestAttempts;
                attempt++
            )
            {
                PopulationResult result =
                    PopulationManager.RollOneFrom(
                        "DynamicObjectsTable:Chests:Tier" +
                        Tier.ToString()
                    );

                string blueprint =
                    result == null ||
                    result.Blueprint.IsNullOrEmpty()
                        ? "Chest" + Tier.ToString()
                        : result.Blueprint;

                GameObject candidate =
                    GameObject.Create(
                        blueprint
                    );

                if (candidate == null)
                    continue;

                //
                // Place first. Some vanilla inventory behavior may complete as part
                // of entering the zone context.
                //
                destination.AddObject(
                    candidate
                );

                if (
                    KeepOneAndDimensionalizeContents(
                        candidate,
                        primary,
                        secondary
                    )
                )
                {
                    container =
                        candidate;

                    break;
                }

                //
                // Empty candidate. Remove it completely and try another vanilla roll.
                //
                candidate.Release();
            }

            //
            // Extremely conservative fallback:
            // if the dynamic table failed repeatedly, try the ordinary tier chest
            // directly. It still uses Qud's normal tier chest inventory builder.
            //
            if (container == null)
            {
                const int StandardChestAttempts =
                    8;

                string fallbackBlueprint =
                    "Chest" +
                    Tier.ToString();

                for (
                    int attempt = 0;
                    attempt < StandardChestAttempts;
                    attempt++
                )
                {
                    GameObject candidate =
                        GameObject.Create(
                            fallbackBlueprint
                        );

                    if (candidate == null)
                        continue;

                    destination.AddObject(
                        candidate
                    );

                    if (
                        KeepOneAndDimensionalizeContents(
                            candidate,
                            primary,
                            secondary
                        )
                    )
                    {
                        container =
                            candidate;

                        break;
                    }

                    candidate.Release();
                }
            }

            //
            // Better to omit the reward entirely after twenty failed vanilla
            // population attempts than deliberately leave an empty chest.
            //
            if (container == null)
                return true;

            SubterraneanSiteDev
                .SubterraneanSiteDevEPReservations
                .ClaimCell(
                    Z,
                    destination
                );

            return true;
        }

        private bool KeepOneAndDimensionalizeContents(
            GameObject container,
            SubterraneanSiteDev
                .SubterraneanSiteDevDimensionBinding primary,
            SubterraneanSiteDev
                .SubterraneanSiteDevDimensionBinding secondary
        )
        {
            if (container == null)
                return false;

            IList<GameObject> contents =
                container.GetContents();

            if (
                contents == null ||
                contents.Count == 0
            )
            {
                return false;
            }

            //
            // Snapshot the generated inventory before removing anything.
            // Do not filter by value, equipment type, usefulness, etc.
            //
            List<GameObject> candidates =
                new List<GameObject>();

            foreach (
                GameObject item
                in contents
            )
            {
                if (item != null)
                {
                    candidates.Add(
                        item
                    );
                }
            }

            if (candidates.Count == 0)
                return false;

            //
            // Every normally generated chest entry has equal standing.
            // A weapon can win; so can food, ammunition, or junk.
            //
            GameObject kept =
                candidates[
                    Stat.Random(
                        0,
                        candidates.Count - 1
                    )
                ];

            //
            // The chest is intentionally reduced to the one randomly
            // selected entry. Release removes the discarded generated
            // objects from their inventory context as part of cleanup.
            //
            foreach (
                GameObject item
                in candidates
            )
            {
                if (
                    ReferenceEquals(
                        item,
                        kept
                    )
                )
                {
                    continue;
                }

                item.Release();
            }

            SubterraneanSiteDev
                .SubterraneanSiteDevDimensionBinding chosen;

            if (primary == null)
            {
                chosen =
                    secondary;
            }
            else if (
                secondary == null ||
                secondary.ReportIndex ==
                    primary.ReportIndex
            )
            {
                chosen =
                    primary;
            }
            else
            {
                chosen =
                    50.in100()
                        ? primary
                        : secondary;
            }

            SubterraneanSiteDev
                .SubterraneanSiteDevDimensionEngine
                .ApplyDimensionToItem(
                    kept,
                    chosen
                );
            return true;
        }

        

        private Cell PickPlacementCell(
            Zone Z
        )
        {
            List<Cell> candidates =
                new List<Cell>();

            List<Location2D> anchors =
                SubterraneanSiteDev
                    .SubterraneanSiteDevEPVerticalTransitions
                    .GetVerticalAnchors(
                        Z.ZoneID
                    );

            foreach (Cell cell in Z.GetCells())
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

                //
                // The chest is shared discrete EP content.
                //
                // Respect all earlier category/theme ownership, including object-like
                // Cold/Fungus pools. Permissive spills remain legal because liquid itself
                // is not an ownership test.
                //
                if (
                    SubterraneanSiteDev
                        .SubterraneanSiteDevEPReservations
                        .IsClaimed(
                            Z,
                            cell
                        )
                )
                {
                    continue;
                }

                if (
                    cell.HasObjectWithBlueprint("Pit") ||
                    cell.HasObjectWithBlueprint(
                        SubterraneanSiteDev
                            .SubterraneanSiteDevEPAttunementSystem
                            .StoneLeftBlueprint
                    ) ||
                    cell.HasObjectWithBlueprint(
                        SubterraneanSiteDev
                            .SubterraneanSiteDevEPAttunementSystem
                            .StoneRightBlueprint
                    )
                )
                {
                    continue;
                }

                if (
                    SubterraneanSiteDev
                        .SubterraneanSiteDevEPPlacement
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

                candidates.Add(cell);
            }

            if (candidates.Count == 0)
                return null;

            return candidates[
                Stat.Random(
                    0,
                    candidates.Count - 1
                )
            ];
        }
    }

    public class SubterraneanSiteDevEPRelicBuilder : ZoneBuilderSandbox
    {
        private const int TransitionExclusionRadius = 4;
        public int Tier = 1;

        public string PrimaryThemeKey = "";
        public string SecondaryThemeKey = "";

        public bool BuildZone(Zone Z)
        {
            if (Z == null)
                return true;

            Tier = Math.Max(1, Math.Min(8, Tier));

            SubterraneanSiteDev.SubterraneanSiteDevDimensionBinding primary =
                SubterraneanSiteDev.SubterraneanSiteDevDimensionEngine
                    .GetBindingByThemeKey(PrimaryThemeKey);

            SubterraneanSiteDev.SubterraneanSiteDevDimensionBinding secondary =
                SubterraneanSiteDev.SubterraneanSiteDevDimensionEngine
                    .GetBindingByThemeKey(SecondaryThemeKey);

            if (primary == null && secondary == null)
                return true;

            int relicTier =
                Math.Min(8, Tier + 1);

            HistoricEntitySnapshot snapshot =
                new HistoricEntitySnapshot(
                    (HistoricEntity)null
                );

            snapshot.properties.Clear();

            GameObject relic =
                RelicGenerator.GenerateRelic(
                    snapshot,
                    relicTier,
                    RandomName: true
                );

            if (relic == null)
                return true;

            SubterraneanSiteDev.SubterraneanSiteDevDimensionBinding chosen;

            if (primary == null)
            {
                chosen = secondary;
            }
            else if (secondary == null)
            {
                chosen = primary;
            }
            else
            {
                chosen =
                    50.in100()
                        ? primary
                        : secondary;
            }

            List<Cell> candidates =
                new List<Cell>();

            List<Location2D> anchors =
                SubterraneanSiteDev
                    .SubterraneanSiteDevEPVerticalTransitions
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

                //
                // The relic chest is discrete shared content.
                //
                if (
                    SubterraneanSiteDev
                        .SubterraneanSiteDevEPReservations
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
                    )
                )
                {
                    continue;
                }

                if (
                    SubterraneanSiteDev
                        .SubterraneanSiteDevEPPlacement
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

                candidates.Add(
                    cell
                );
            }

            if (candidates.Count == 0)
            {
                relic.Release(
                    RemoveFromContext: false
                );

                return true;
            }

            if (candidates == null ||
                candidates.Count == 0)
            {
                return true;
            }

            Cell destination =
                candidates[
                    Stat.Random(
                        0,
                        candidates.Count - 1
                    )
                ];

            GameObject relicChest =
                GameObject.Create("RelicChest");

            if (relicChest == null)
                return true;

            destination.AddObject(
                relicChest
            );

            SubterraneanSiteDev
                .SubterraneanSiteDevEPReservations
                .ClaimCell(
                    Z,
                    destination
                );

            PlaceRelicBuilder placer =
                new PlaceRelicBuilder();

            //
            // The guaranteed bottom relic chest is protected from
            // spacetime vortices in every EP theme.
            //
            // Ordinary reward chests remain fair game.
            //
            relicChest.SetIntProperty(
                "IgnoreSpaceTimeVortex",
                1
            );

            placer.BuildZoneWithRelic(
                Z,
                relic
            );

            // PlaceRelicBuilder adds its normal cybernetics credit wedges after
            // inserting the relic. Dimensionalize those additional reward items too.
            IList<GameObject> rewardContents =
                relicChest.GetContents();

            if (rewardContents != null)
            {
                foreach (GameObject item in rewardContents)
                {
                    if (item == null ||
                        item.GetPart<ModExtradimensional>() != null)
                    {
                        continue;
                    }

                    SubterraneanSiteDev.SubterraneanSiteDevDimensionBinding itemDimension;

                    if (primary == null)
                    {
                        itemDimension = secondary;
                    }
                    else if (secondary == null)
                    {
                        itemDimension = primary;
                    }
                    else
                    {
                        itemDimension =
                            50.in100()
                                ? primary
                                : secondary;
                    }

                    SubterraneanSiteDev.SubterraneanSiteDevDimensionEngine
                        .ApplyDimensionToItem(
                            item,
                            itemDimension
                        );
                }
            }

            return true;
        }
    }


    public class SubterraneanSiteDevEPHeroBuilder : ZoneBuilderSandbox
    {
        public int Tier = 1;

        public string PrimaryThemeKey = "";
        public string SecondaryThemeKey = "";

        private const int TransitionExclusionRadius = 4;

        public bool BuildZone(Zone Z)
        {
            if (Z == null)
                return true;

            Tier = Math.Max(1, Math.Min(8, Tier));

            SubterraneanSiteDev.SubterraneanSiteDevDimensionBinding primary =
                SubterraneanSiteDev.SubterraneanSiteDevDimensionEngine
                    .GetBindingByThemeKey(PrimaryThemeKey);

            SubterraneanSiteDev.SubterraneanSiteDevDimensionBinding secondary =
                SubterraneanSiteDev.SubterraneanSiteDevDimensionEngine
                    .GetBindingByThemeKey(SecondaryThemeKey);

            if (primary == null && secondary == null)
                return true;

            SubterraneanSiteDev.SubterraneanSiteDevDimensionBinding chosen;
            SubterraneanSiteDev.SubterraneanSiteDevDimensionBinding alternate;

            if (primary == null)
            {
                chosen = secondary;
                alternate = null;
            }
            else if (secondary == null ||
                    secondary.ReportIndex == primary.ReportIndex)
            {
                chosen = primary;
                alternate = null;
            }
            else if (50.in100())
            {
                chosen = primary;
                alternate = secondary;
            }
            else
            {
                chosen = secondary;
                alternate = primary;
            }

            string sourceDescription;

            GameObject hero =
                SubterraneanSiteDev.SubterraneanSiteDevDimensionEngine
                    .CreateDenizenForSite(
                        chosen,
                        Tier,
                        out sourceDescription
                    );

            // Mainly useful in the current two-theme development harness:
            // if the randomly selected side cannot supply a creature at this tier,
            // use the other dimensional population rather than omit the boss.
            if (hero == null && alternate != null)
            {
                chosen = alternate;

                hero =
                    SubterraneanSiteDev.SubterraneanSiteDevDimensionEngine
                        .CreateDenizenForSite(
                            chosen,
                            Tier,
                            out sourceDescription
                        );
            }

            if (hero == null)
                return true;

            // EP faction assignments select a creature population; the dimensional
            // boss is not intended to become a normal water-ritual faction hero.
            hero.SetStringProperty(
                "HeroNoWaterRitual",
                "true"
            );

            GameObject promoted =
                HeroMaker.MakeHero(
                    hero,
                    Array.Empty<string>(),
                    Array.Empty<string>(),
                    Tier,
                    "Hero"
                );

            if (promoted == null)
            {
                hero.Release(RemoveFromContext: false);
                return true;
            }

            hero = promoted;

            string denizenProviderType =
                "";


            if (
                string.Equals(
                    chosen.ThemeLabel,
                    PrimaryThemeKey,
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                denizenProviderType =
                    The.ZoneManager.GetZoneProperty(
                        Z.ZoneID,
                        SubterraneanSiteDev
                            .SubterraneanSiteDevEPShuffledTheme
                            .PrimaryDenizenProviderTypeProperty
                    ) as string ?? "";
            }
            else if (
                string.Equals(
                    chosen.ThemeLabel,
                    SecondaryThemeKey,
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                denizenProviderType =
                    The.ZoneManager.GetZoneProperty(
                        Z.ZoneID,
                        SubterraneanSiteDev
                            .SubterraneanSiteDevEPShuffledTheme
                            .SecondaryDenizenProviderTypeProperty
                    ) as string ?? "";
            }

            // Run this AFTER HeroMaker so the mutation budget sees the promoted
            // level, hero-added inventory participates in dimensional loot rules,
            // and EP hostility/adaptation is the final behavioral pass.
            SubterraneanSiteDev.SubterraneanSiteDevDimensionEngine
                .ApplyDimensionIdentityAndAdaptation(
                    hero,
                    chosen,
                    denizenProviderType
                );
                
            hero.SetIntProperty(
                "SubterraneanSiteDevEPHero",
                1
            );

            Cell destination =
                PickPlacementCell(
                    Z,
                    hero
                );

            if (destination == null)
            {
                hero.Release(RemoveFromContext: false);
                return true;
            }

            destination.AddObject(
                hero
            );

            hero.MakeActive();

            SubterraneanSiteDev
                .SubterraneanSiteDevEPReservations
                .ClaimCell(
                    Z,
                    destination
                );

            return true;
        }

        private Cell PickPlacementCell(
            Zone Z,
            GameObject creature
        )
        {
            List<Cell> candidates =
                new List<Cell>();

            List<Location2D> anchors =
                SubterraneanSiteDev
                    .SubterraneanSiteDevEPVerticalTransitions
                    .GetVerticalAnchors(
                        Z.ZoneID
                    );

            foreach (Cell cell in Z.GetCells())
            {
                if (cell == null ||
                    !cell.IsReachable() ||
                    !cell.IsSpawnable() ||
                    !cell.IsEmptyOfSolid() ||
                    cell.HasSpawnBlocker())
                {
                    continue;
                }

                if (
                    SubterraneanSiteDev
                        .SubterraneanSiteDevEPReservations
                        .IsClaimed(
                            Z,
                            cell
                        )
                )
                {
                    continue;
                }

                if (cell.HasObjectWithBlueprint("Pit") ||
                    cell.HasObjectWithBlueprint(
                        SubterraneanSiteDev
                            .SubterraneanSiteDevEPAttunementSystem
                            .StoneLeftBlueprint
                    ) ||
                    cell.HasObjectWithBlueprint(
                        SubterraneanSiteDev
                            .SubterraneanSiteDevEPAttunementSystem
                            .StoneRightBlueprint
                    ))
                {
                    continue;
                }

                if (SubterraneanSiteDev
                    .SubterraneanSiteDevEPPlacement
                    .IsNearAnyAnchor(
                        cell.X,
                        cell.Y,
                        anchors,
                        TransitionExclusionRadius
                    ))
                {
                    continue;
                }

                if (creature != null &&
                    cell.GetNavigationWeightFor(creature) >= 30)
                {
                    continue;
                }

                candidates.Add(cell);
            }

            if (candidates.Count == 0)
                return null;

            return candidates[
                Stat.Random(
                    0,
                    candidates.Count - 1
                )
            ];
        }
    }

    /// <summary>
    /// Denizens come from both underlying extradimensional identities regardless
    /// of how Categories 1-5 resolve. Even if every shuffled category resolves to
    /// the primary theme, the secondary dimension remains part of the pocket's
    /// denizen population.
    /// </summary>
    public class SubterraneanSiteDevEPDenizenBuilder : ZoneBuilderSandbox
    {
        public int Tier = 1;
        public int MinDenizensPerTheme = 2;
        public int MaxDenizensPerTheme = 5;

        public string PrimaryThemeKey = "";
        public string SecondaryThemeKey = "";

        private const int TransitionExclusionRadius = 4;

        public bool BuildZone(
            Zone Z
        )
        {
            if (Z == null)
                return true;

            string primaryDenizenProviderType =
                The.ZoneManager.GetZoneProperty(
                    Z.ZoneID,
                    SubterraneanSiteDev
                        .SubterraneanSiteDevEPShuffledTheme
                        .PrimaryDenizenProviderTypeProperty
                ) as string ?? "";


            string secondaryDenizenProviderType =
                The.ZoneManager.GetZoneProperty(
                    Z.ZoneID,
                    SubterraneanSiteDev
                        .SubterraneanSiteDevEPShuffledTheme
                        .SecondaryDenizenProviderTypeProperty
                ) as string ?? "";


            Tier =
                Math.Max(
                    1,
                    Math.Min(
                        8,
                        Tier
                    )
                );


            MinDenizensPerTheme =
                Math.Max(
                    0,
                    MinDenizensPerTheme
                );


            MaxDenizensPerTheme =
                Math.Max(
                    MinDenizensPerTheme,
                    MaxDenizensPerTheme
                );


            SubterraneanSiteDev
                .SubterraneanSiteDevDimensionBinding primary =
                    SubterraneanSiteDev
                        .SubterraneanSiteDevDimensionEngine
                        .GetBindingByThemeKey(
                            PrimaryThemeKey
                        );


            SubterraneanSiteDev
                .SubterraneanSiteDevDimensionBinding secondary =
                    SubterraneanSiteDev
                        .SubterraneanSiteDevDimensionEngine
                        .GetBindingByThemeKey(
                            SecondaryThemeKey
                        );


            if (
                primary == null &&
                secondary == null
            )
            {
                return true;
            }


            if (primary != null)
            {
                int primaryCount =
                    Stat.Random(
                        MinDenizensPerTheme,
                        MaxDenizensPerTheme
                    );


                SpawnForDimension(
                    Z,
                    primary,
                    primaryDenizenProviderType,
                    primaryCount
                );
            }


            if (
                secondary != null &&
                (
                    primary == null ||
                    secondary.ReportIndex !=
                        primary.ReportIndex
                )
            )
            {
                int secondaryCount =
                    Stat.Random(
                        MinDenizensPerTheme,
                        MaxDenizensPerTheme
                    );

                    SpawnForDimension(
                        Z,
                        secondary,
                        secondaryDenizenProviderType,
                        secondaryCount
                    );
            }


            return true;
        }
        

        private void SpawnForDimension(
            Zone Z,
            SubterraneanSiteDev.SubterraneanSiteDevDimensionBinding dimension,
            string denizenProviderType,
            int count
        )
        {
            if (Z == null || dimension == null || count <= 0)
                return;

            for (int i = 0; i < count; i++)
            {
                string sourceDescription;
                GameObject creature =
                    SubterraneanSiteDev.SubterraneanSiteDevDimensionEngine
                        .CreateDenizenForSite(
                            dimension,
                            Tier,
                            out sourceDescription
                        );

                if (creature == null)
                    continue;

                SubterraneanSiteDev
                    .SubterraneanSiteDevDimensionEngine
                    .ApplyDimensionIdentityAndAdaptation(
                        creature,
                        dimension,
                        denizenProviderType
                    );

                Cell destination = PickPlacementCell(Z, creature);
                if (destination == null)
                {
                    creature.Release(RemoveFromContext: false);
                    continue;
                }

                destination.AddObject(
                    creature
                );

                creature.MakeActive();

                SubterraneanSiteDev
                    .SubterraneanSiteDevEPReservations
                    .ClaimCell(
                        Z,
                        destination
                    );
            }
        }

        private Cell PickPlacementCell(Zone Z, GameObject creature)
        {
            List<Cell> candidates = new List<Cell>();
            List<Location2D> anchors =
                SubterraneanSiteDev.SubterraneanSiteDevEPVerticalTransitions
                    .GetVerticalAnchors(Z.ZoneID);

            foreach (Cell cell in Z.GetCells())
            {
                if (cell == null ||
                    !cell.IsReachable() ||
                    !cell.IsSpawnable() ||
                    !cell.IsEmptyOfSolid() ||
                    cell.HasSpawnBlocker())
                {
                    continue;
                }

                //
                // Denizens may occupy spills, but not previously claimed discrete
                // or object-like pool space.
                //
                if (
                    SubterraneanSiteDev
                        .SubterraneanSiteDevEPReservations
                        .IsClaimed(
                            Z,
                            cell
                        )
                )
                {
                    continue;
                }

                if (cell.HasObjectWithBlueprint("Pit") ||
                    cell.HasObjectWithBlueprint(
                        SubterraneanSiteDev
                            .SubterraneanSiteDevEPAttunementSystem
                            .StoneLeftBlueprint
                    ) ||
                    cell.HasObjectWithBlueprint(
                        SubterraneanSiteDev
                            .SubterraneanSiteDevEPAttunementSystem
                            .StoneRightBlueprint
                    ))
                {
                    continue;
                }

                if (SubterraneanSiteDev.SubterraneanSiteDevEPPlacement
                    .IsNearAnyAnchor(
                        cell.X,
                        cell.Y,
                        anchors,
                        TransitionExclusionRadius
                    ))
                {
                    continue;
                }

                if (creature != null &&
                    cell.GetNavigationWeightFor(creature) >= 30)
                {
                    continue;
                }

                candidates.Add(cell);
            }

            if (candidates.Count == 0)
                return null;

            return candidates[Stat.Random(0, candidates.Count - 1)];
        }
    }
}