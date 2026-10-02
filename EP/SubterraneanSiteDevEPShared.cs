using System;
using System.Collections.Generic;
using Genkit;
using XRL;
using XRL.Core;
using XRL.Rules;
using XRL.World;
using XRL.World.AI;
using XRL.World.Parts;
using XRL.UI;


namespace SubterraneanSiteDev
{
    /// <summary>
    /// Minimal site-layer context owned by the extradimensional-pocket system.
    /// The dev host constructs this from the same information that production
    /// Subterranean Sites exposes through RegisterLayeredSite(...).
    /// </summary>
    internal sealed class SubterraneanSiteDevEPLayerContext
    {
        public string ZoneId;
        public int LayerIndex;
        public int LayerCount;
        public int Z;
        public int Tier;
        public bool IsOrigin;
        public bool IsBottom;
    }

    /// <summary>
    /// Shared description of the special EP entrance scar on the origin layer.
    ///
    /// The scar composes the shuffled category winners explicitly:
    /// C1 supplies its physical preview objects,
    /// C5 supplies its decorations,
    /// and C4 supplies the scar floor.
    ///
    /// Attunement remains owned by C1.
    /// </summary>
    internal sealed class SubterraneanSiteDevEPEntranceContext
    {
        public string ZoneId;
        public int Tier;
        public int CenterX;
        public int CenterY;
        public int ScarRadius;
        public int HoleRadius;
    }

    /// <summary>
    /// Adapter boundary between the EP registrar and whichever system owns site
    /// registration. The dev harness implements this for a fixed test stack;
    /// production can adapt RuntimeZoneBuilderInjectionSystem.RegisterLayeredSite.
    /// </summary>
    internal interface ISubterraneanSiteDevEPHost
    {
        bool RegisterLayeredSite(
            List<string> siteZoneIds,
            string siteDisplayName,
            string discoveryKey,
            Action<SubterraneanSiteDevEPLayerContext> registerLayer
        );
    }

    /// <summary>
    /// Top-level contract for a complete EP site composition.
    ///
    /// Individual dimensional themes contribute category implementations through
    /// ISubterraneanSiteDevEPCategoryProvider. The shuffled-theme coordinator
    /// composes those contributions into one complete EP and exposes that
    /// composition through this interface to the site registrar.
    /// </summary>
    internal interface ISubterraneanSiteDevEPTheme
    {
        string ThemeKey { get; }
        string SiteDisplayName { get; }
        int MinimumHoleSeparation { get; }
        void RegisterLayer(SubterraneanSiteDevEPLayerContext context);
    }

    /// <summary>
    /// Category 4 owns both structural layout and floor substrate.
    ///
    /// Layout runs during structural generation. Floor is registered once later,
    /// after ordinary EP content, so every owned cell receives its substrate
    /// underneath walls, fixtures, decorations, and other objects.
    ///
    /// They remain one conceptual shuffled category even though their physical
    /// registration phases differ.
    /// </summary>
    internal interface ISubterraneanSiteDevEPCategoryProvider
    {
        string ThemeKey { get; }
        int MinimumHoleSeparation { get; }

        void RegisterCategory1(SubterraneanSiteDevEPLayerContext context);
        void RegisterCategory2(SubterraneanSiteDevEPLayerContext context);
        void RegisterCategory3(SubterraneanSiteDevEPLayerContext context);
        void RegisterCategory4Layout(SubterraneanSiteDevEPLayerContext context);
        void RegisterCategory4Floor(SubterraneanSiteDevEPLayerContext context);
        void RegisterCategory5(SubterraneanSiteDevEPLayerContext context);

        // Entrance preview contributions. These use the same visual/content
        // language as Categories 4-floor and 5, but are explicitly scoped to
        // the entrance scar so the rest of the pre-existing origin zone stays
        // intact.
        void RegisterEntranceFloor(SubterraneanSiteDevEPEntranceContext context);
        void RegisterEntranceDecorations(SubterraneanSiteDevEPEntranceContext context);
    }

    /// <summary>
    /// Optional Category-1 physical-content contract.
    ///
    /// Category 1 normally registers its pervasive/global environmental behavior
    /// late in the pipeline through RegisterCategory1(). Some themes also have
    /// physical objects whose presence is specifically coupled to Category 1 and
    /// therefore to the matching attunement.
    ///
    /// These objects must be registered while the shared abstract geometry still
    /// exists, before Category 2/5 content and before final reachability.
    ///
    /// Themes without Category-1 physical objects simply do not implement this
    /// interface.
    /// </summary>
    internal interface ISubterraneanSiteDevEPPrimaryObjectProvider
    {
        void RegisterCategory1Objects(
            SubterraneanSiteDevEPLayerContext context
        );

        void RegisterEntranceCategory1Objects(
            SubterraneanSiteDevEPEntranceContext context
        );
    }

    internal enum SubterraneanSiteDevEPAttunementBuildResult
    {
        Success,
        Cancelled,
        Unavailable
    }

    /// <summary>
    /// Theme-owned Category-1 attunement construction.
    ///
    /// Shared infrastructure owns:
    ///   - duration
    ///   - replacement
    ///   - refresh
    ///   - common resistance/save/mutation mechanics
    ///   - guaranteed removal lifecycle
    ///
    /// The theme owns:
    ///   - its attunement package
    ///   - any special player interaction
    ///   - the effect subclass it creates
    ///   - theme-specific success text
    ///   - special apply/remove/event behavior
    /// </summary>
    internal interface ISubterraneanSiteDevEPAttunementProvider
    {
        string ThemeKey { get; }

        SubterraneanSiteDevEPAttunementBuildResult TryCreateAttunement(
            GameObject actor,
            Zone zone,
            out XRL.World.Effects.SubterraneanSiteDevEPAttunementEffect effect,
            out string successMessage
        );
    }

    internal interface ISubterraneanSiteDevEPDenizenAdaptationProvider
    {
        string ThemeKey { get; }

        void ApplyDenizenAdaptation(
            GameObject creature
        );
    }

    internal interface ISubterraneanSiteDevEPSignatureMutationProvider
    {
        string ThemeKey { get; }

        string SignatureMutationClass { get; }
    }

    /// <summary>
    /// Shared control for EP merchant stock.
    ///
    /// Vanilla GenericInventoryRestocker remains responsible for choosing
    /// what merchandise a merchant receives.
    ///
    /// This helper reduces generated sale inventory to a controlled number
    /// of entries and dimensionalizes the survivors.
    ///
    /// SubterraneanSiteDevEPMerchantStockController repeats the same
    /// normalization after future vanilla StockedEvent restocks.
    /// </summary>
    internal static class SubterraneanSiteDevEPMerchantStockControl
    {
        internal const string ControlledStockProperty =
            "SubterraneanSiteDevEPControlledMerchantStock";


        //
        // Initial pass.
        //
        // GetInventory() is carried inventory only. Equipped gear is not
        // part of this list and is deliberately untouched.
        //
        internal static int TrimAndDimensionalizeStock(
            GameObject merchant,
            string dimensionThemeKey,
            int minimumStock,
            int maximumStock,
            System.Random rng
        )
        {
            if (
                merchant == null ||
                rng == null
            )
            {
                return 0;
            }


            List<GameObject> inventory =
                merchant.GetInventory();


            if (
                inventory == null ||
                inventory.Count == 0
            )
            {
                return 0;
            }


            List<GameObject> candidates =
                new List<GameObject>();


            foreach (
                GameObject item
                in inventory
            )
            {
                if (item == null)
                    continue;


                //
                // An explicit WontSell object is not merchandise.
                //
                if (
                    item.HasIntProperty(
                        "WontSell"
                    )
                )
                {
                    continue;
                }


                candidates.Add(
                    item
                );
            }


            return
                TrimCandidates(
                    merchant,
                    candidates,
                    dimensionThemeKey,
                    minimumStock,
                    maximumStock,
                    rng
                );
        }


        //
        // Live post-restock pass.
        //
        // Vanilla has just created its new _stock merchandise and sent
        // StockedEvent.
        //
        // At that point the merchant's carried SALE inventory should be
        // exactly our controlled stock. Old persistent/norestock merchandise,
        // native carried merchandise, and player-sold objects do not survive
        // a merchant restock unless they are explicitly WontSell.
        //
        internal static int TrimAndDimensionalizeRestockedStock(
            GameObject merchant,
            string dimensionThemeKey,
            int minimumStock,
            int maximumStock,
            System.Random rng
        )
        {
            if (
                merchant == null ||
                rng == null
            )
            {
                return 0;
            }


            List<GameObject> inventory =
                merchant.GetInventory();


            if (inventory == null)
                return 0;


            //
            // First remove every carried sale item that is NOT part of the
            // new vanilla _stock batch.
            //
            // This includes:
            //   - the controlled merchandise retained by our earlier pass
            //   - native/pre-existing carried merchandise
            //   - norestock merchandise
            //   - player-sold objects still present when restocking occurs
            //
            // Equipped gear is not returned by GetInventory() and is untouched.
            //
            List<GameObject> remove =
                new List<GameObject>();


            foreach (
                GameObject item
                in inventory
            )
            {
                if (item == null)
                    continue;


                if (
                    item.HasIntProperty(
                        "WontSell"
                    )
                )
                {
                    continue;
                }


                if (
                    !item.HasProperty(
                        "_stock"
                    )
                )
                {
                    remove.Add(
                        item
                    );
                }
            }


            foreach (
                GameObject item
                in remove
            )
            {
                RemoveInventoryObject(
                    merchant,
                    item
                );
            }


            inventory =
                merchant.GetInventory();


            if (
                inventory == null ||
                inventory.Count == 0
            )
            {
                return 0;
            }


            List<GameObject> candidates =
                new List<GameObject>();


            foreach (
                GameObject item
                in inventory
            )
            {
                if (item == null)
                    continue;


                if (
                    !item.HasProperty(
                        "_stock"
                    )
                )
                {
                    continue;
                }


                if (
                    item.HasIntProperty(
                        "WontSell"
                    )
                )
                {
                    continue;
                }


                candidates.Add(
                    item
                );
            }


            return
                TrimCandidates(
                    merchant,
                    candidates,
                    dimensionThemeKey,
                    minimumStock,
                    maximumStock,
                    rng
                );
        }


        private static int TrimCandidates(
            GameObject merchant,
            List<GameObject> candidates,
            string dimensionThemeKey,
            int minimumStock,
            int maximumStock,
            System.Random rng
        )
        {
            if (
                merchant == null ||
                candidates == null ||
                candidates.Count == 0 ||
                rng == null
            )
            {
                return 0;
            }


            if (minimumStock < 0)
                minimumStock = 0;


            if (maximumStock < minimumStock)
            {
                maximumStock =
                    minimumStock;
            }


            SubterraneanSiteDevDimensionBinding dimension =
                SubterraneanSiteDevDimensionEngine
                    .GetBindingByThemeKey(
                        dimensionThemeKey
                    );


            if (dimension == null)
                return 0;


            //
            // Rank the vanilla-generated merchandise by per-item value.
            //
            // ValueEach is deliberate: a large stack of cheap ammo, food,
            // or scrap does not outrank a valuable single item merely because
            // vanilla generated many copies.
            //
            candidates.Sort(
                delegate(
                    GameObject left,
                    GameObject right
                )
                {
                    if (
                        left == null &&
                        right == null
                    )
                    {
                        return 0;
                    }


                    if (left == null)
                        return 1;


                    if (right == null)
                        return -1;


                    int valueComparison =
                        right.ValueEach.CompareTo(
                            left.ValueEach
                        );


                    if (valueComparison != 0)
                        return valueComparison;


                    return
                        string.Compare(
                            left.Blueprint,
                            right.Blueprint,
                            StringComparison.Ordinal
                        );
                }
            );


            int targetStock;


            if (
                candidates.Count <=
                minimumStock
            )
            {
                targetStock =
                    candidates.Count;
            }
            else
            {
                targetStock =
                    rng.Next(
                        minimumStock,
                        maximumStock + 1
                    );


                targetStock =
                    Math.Min(
                        targetStock,
                        candidates.Count
                    );
            }


        //
        // Build the preferred selection order.
        //
        // First retained item:
        //     random item from the top third by value.
        //
        // Second retained item:
        //     random item from the middle third.
        //
        // Third retained item:
        //     random item from anywhere still remaining.
        //
        // Any merchandise after those preferred choices remains in the
        // list as fallback in case ModExtradimensional cannot be applied
        // to one of the preferred objects.
        //
        List<GameObject> selectionOrder =
            new List<GameObject>();


        HashSet<GameObject> selected =
            new HashSet<GameObject>();


        int topThirdEnd =
            Math.Min(
                candidates.Count,
                Math.Max(
                    1,
                    (candidates.Count + 2) / 3
                )
            );


        int middleThirdEnd =
            Math.Min(
                candidates.Count,
                Math.Max(
                    topThirdEnd,
                    (2 * candidates.Count + 2) / 3
                )
            );


        GameObject pick =
            null;


        // First pick: top third.
        if (
            targetStock >= 1 &&
            selected.Count <
            candidates.Count
        )
        {
            pick =
                candidates[
                    rng.Next(
                        0,
                        topThirdEnd
                    )
                ];


            selectionOrder.Add(
                pick
            );


            selected.Add(
                pick
            );
        }


        // Second pick: middle third.
        if (
            targetStock >= 2 &&
            selected.Count <
            candidates.Count
        )
        {
            if (
                middleThirdEnd >
                topThirdEnd
            )
            {
                pick =
                    candidates[
                        rng.Next(
                            topThirdEnd,
                            middleThirdEnd
                        )
                    ];
            }
            else
            {
                do
                {
                    pick =
                        candidates[
                            rng.Next(
                                candidates.Count
                            )
                        ];
                }
                while (
                    selected.Contains(
                        pick
                    )
                );
            }


            selectionOrder.Add(
                pick
            );


            selected.Add(
                pick
            );
        }


        // Third pick: completely random among everything not already chosen.
        if (
            targetStock >= 3 &&
            selected.Count <
            candidates.Count
        )
        {
            do
            {
                pick =
                    candidates[
                        rng.Next(
                            candidates.Count
                        )
                    ];
            }
            while (
                selected.Contains(
                    pick
                )
            );


            selectionOrder.Add(
                pick
            );


            selected.Add(
                pick
            );
        }


        //
        // Append everything else afterward.
        //
        // These are primarily fallback candidates. The existing loop below
        // will continue through them only if one of our preferred choices
        // cannot actually receive the extradimensional modification.
        //
        foreach (
            GameObject item
            in candidates
        )
        {
            if (
                item != null &&
                !selected.Contains(
                    item
                )
            )
            {
                selectionOrder.Add(
                    item
                );
            }
        }


            int retainedStock =
                0;


            foreach (
                GameObject item
                in selectionOrder
            )
            {
                if (item == null)
                    continue;


                bool dimensionalized =
                    false;


                if (
                    retainedStock <
                    targetStock
                )
                {
                    //
                    // One merchandise slot means ONE item, never a stack.
                    //
                    if (
                        item.Stacker != null &&
                        item.Stacker.StackCount > 1
                    )
                    {
                        item.Stacker.StackCount =
                            1;
                    }


                    dimensionalized =
                        SubterraneanSiteDevDimensionEngine
                            .ApplyDimensionToItem(
                                item,
                                dimension
                            );
                }


                if (dimensionalized)
                {
                    item.SetStringProperty(
                        ControlledStockProperty,
                        "Yes"
                    );


                    retainedStock++;

                    continue;
                }


                //
                // Excess merchandise, or merchandise that cannot accept the
                // dimensional modification, is actually removed.
                //
                // Do not hide it with WontSell.
                //
                RemoveInventoryObject(
                    merchant,
                    item
                );
            }


            return
                retainedStock;
        }


        private static void RemoveInventoryObject(
            GameObject merchant,
            GameObject item
        )
        {
            if (
                merchant == null ||
                item == null
            )
            {
                return;
            }


            if (
                merchant.Inventory != null
            )
            {
                merchant.Inventory.RemoveObject(
                    item
                );
            }


            item.Obliterate();
        }
    }

    /// <summary>
    /// Persistent live controller for EP merchants.
    ///
    /// Vanilla GenericInventoryRestocker remains fully responsible for
    /// choosing and generating merchandise.
    ///
    /// After vanilla sends StockedEvent, this part trims the newly-created
    /// _stock entries and dimensionalizes the surviving merchandise.
    /// </summary>
    [Serializable]
    public class SubterraneanSiteDevEPMerchantStockController :
        IPart
    {
        public string DimensionThemeKey =
            "";

        public int MinimumStock =
            2;

        public int MaximumStock =
            3;


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
                ID == StockedEvent.ID;
        }


        public override bool HandleEvent(
            StockedEvent E
        )
        {
            if (
                ParentObject != null &&
                !string.IsNullOrEmpty(
                    DimensionThemeKey
                ) &&
                The.Game != null
            )
            {
                //
                // Restocking itself is already dynamic vanilla behavior.
                //
                // We only need a stable local shuffle for the merchandise
                // that vanilla just produced.
                //
                int seed =
                    XRLCore.Core.Game.GetWorldSeed(
                        "SubterraneanSiteDev:EPMerchantStock:" +
                        ParentObject.ID +
                        ":" +
                        The.Game.TimeTicks.ToString()
                    );


                System.Random rng =
                    new System.Random(
                        seed
                    );


                SubterraneanSiteDevEPMerchantStockControl
                    .TrimAndDimensionalizeRestockedStock(
                        ParentObject,
                        DimensionThemeKey,
                        MinimumStock,
                        MaximumStock,
                        rng
                    );
            }


            return
                base.HandleEvent(E);
        }
    }

    internal static class SubterraneanSiteDevEPProviderFactory
    {
        internal static bool TryCreate<TProvider>(
            string typeName,
            out TProvider provider
        )
            where TProvider : class
        {
            provider = null;

            if (typeName.IsNullOrEmpty())
                return false;


            Type providerType =
                Type.GetType(
                    typeName,
                    throwOnError: false
                );


            if (providerType == null)
                return false;


            object instance;

            try
            {
                instance =
                    Activator.CreateInstance(
                        providerType,
                        nonPublic: true
                    );
            }
            catch
            {
                return false;
            }


            provider =
                instance as TProvider;


            return
                provider != null;
        }
    }

    /// <summary>
    /// Shared semantic geometry used while a layer is being built.
    ///
    /// Category 4 creates the abstract open/closed shape using a neutral solid
    /// placeholder. After the shape is complete, the shared classifier upgrades
    /// cave-facing solid cells to a distinct boundary placeholder. Categories 2
    /// and 5 can therefore reason about walls/interior without knowing which
    /// Category-3 material theme will eventually be selected.
    ///
    /// Category 3 is the final consumer of the placeholders and replaces them
    /// with its own exterior/core and inner-boundary materials.
    /// </summary>
    internal static class SubterraneanSiteDevEPGeometry
    {
        internal const string SolidPlaceholderBlueprint =
            "SubterraneanSiteDevEPSolidPlaceholder";

        internal const string BoundaryPlaceholderBlueprint =
            "SubterraneanSiteDevEPBoundaryPlaceholder";

        internal static bool IsSolidPlaceholder(Cell cell)
        {
            return
                cell != null &&
                cell.HasObjectWithBlueprint(
                    SolidPlaceholderBlueprint
                );
        }

        internal static bool IsBoundaryPlaceholder(Cell cell)
        {
            return
                cell != null &&
                cell.HasObjectWithBlueprint(
                    BoundaryPlaceholderBlueprint
                );
        }

        internal static bool IsAnyPlaceholder(Cell cell)
        {
            return
                IsSolidPlaceholder(cell) ||
                IsBoundaryPlaceholder(cell);
        }

        internal static bool IsOpenGeometryCell(Cell cell)
        {
            return
                cell != null &&
                !IsAnyPlaceholder(cell);
        }
    }

    /// <summary>
    /// Temporary semantic ownership for cells during one EP zone build.
    ///
    /// EP builder order supplies priority:
    /// earlier content claims cells or footprints, and later content checks
    /// whether those locations have already been claimed.
    ///
    /// Claims deliberately do not know which theme or category created them.
    /// They exist only while the zone is being built and are not saved as
    /// permanent per-cell game state.
    /// </summary>
    internal static class SubterraneanSiteDevEPReservations
    {
        private static readonly Dictionary<string, bool[,]> ClaimsByZone =
            new Dictionary<string, bool[,]>();

        internal static void Reset(Zone Z)
        {
            if (
                Z == null ||
                Z.ZoneID.IsNullOrEmpty()
            )
            {
                return;
            }

            ClaimsByZone[Z.ZoneID] =
                new bool[
                    Z.Width,
                    Z.Height
                ];
        }

        internal static void Clear(Zone Z)
        {
            if (
                Z == null ||
                Z.ZoneID.IsNullOrEmpty()
            )
            {
                return;
            }

            ClaimsByZone.Remove(
                Z.ZoneID
            );
        }

        internal static bool IsClaimed(
            Zone Z,
            Cell cell
        )
        {
            if (cell == null)
                return false;

            return IsClaimed(
                Z,
                cell.X,
                cell.Y
            );
        }

        internal static bool IsClaimed(
            Zone Z,
            int x,
            int y
        )
        {
            bool[,] claims =
                GetClaims(
                    Z,
                    create: false
                );

            if (claims == null)
                return false;

            if (
                x < 0 ||
                y < 0 ||
                x >= Z.Width ||
                y >= Z.Height
            )
            {
                return true;
            }

            return claims[x, y];
        }

        internal static bool RectangleIsAvailable(
            Zone Z,
            int x1,
            int y1,
            int x2,
            int y2
        )
        {
            if (Z == null)
                return false;

            if (x1 > x2)
            {
                int swap = x1;
                x1 = x2;
                x2 = swap;
            }

            if (y1 > y2)
            {
                int swap = y1;
                y1 = y2;
                y2 = swap;
            }

            //
            // A requested functional footprint must actually fit inside the zone.
            //
            if (
                x1 < 0 ||
                y1 < 0 ||
                x2 >= Z.Width ||
                y2 >= Z.Height
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
                    if (
                        IsClaimed(
                            Z,
                            x,
                            y
                        )
                    )
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        internal static void ClaimCell(
            Zone Z,
            Cell cell
        )
        {
            if (cell == null)
                return;

            ClaimCell(
                Z,
                cell.X,
                cell.Y
            );
        }

        internal static void ClaimCell(
            Zone Z,
            int x,
            int y
        )
        {
            bool[,] claims =
                GetClaims(
                    Z,
                    create: true
                );

            if (claims == null)
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

            claims[x, y] = true;
        }

        internal static void ClaimRectangle(
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
                int swap = x1;
                x1 = x2;
                x2 = swap;
            }

            if (y1 > y2)
            {
                int swap = y1;
                y1 = y2;
                y2 = swap;
            }

            x1 = Math.Max(
                0,
                x1
            );

            y1 = Math.Max(
                0,
                y1
            );

            x2 = Math.Min(
                Z.Width - 1,
                x2
            );

            y2 = Math.Min(
                Z.Height - 1,
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
                    ClaimCell(
                        Z,
                        x,
                        y
                    );
                }
            }
        }

        private static bool[,] GetClaims(
            Zone Z,
            bool create
        )
        {
            if (
                Z == null ||
                Z.ZoneID.IsNullOrEmpty()
            )
            {
                return null;
            }

            bool[,] claims;

            if (
                ClaimsByZone.TryGetValue(
                    Z.ZoneID,
                    out claims
                ) &&
                claims != null &&
                claims.GetLength(0) == Z.Width &&
                claims.GetLength(1) == Z.Height
            )
            {
                return claims;
            }

            if (!create)
                return null;

            claims =
                new bool[
                    Z.Width,
                    Z.Height
                ];

            ClaimsByZone[Z.ZoneID] =
                claims;

            return claims;
        }
    }

    /// <summary>
    /// Shared EP liquid placement.
    ///
    /// If the cell already contains an open liquid volume, mix the requested
    /// liquid into that existing volume instead of creating a second independent
    /// liquid object.
    ///
    /// If the cell is dry, use the supplied pool blueprint so each theme keeps
    /// its intended native pool object, depth, rendering, and other properties.
    /// </summary>
    internal static class SubterraneanSiteDevEPLiquids
    {





        internal static bool AddOrMixLiquid(
            Cell cell,
            string liquidID,
            string dryCellPoolBlueprint
        )
        {
            if (
                cell == null ||
                liquidID.IsNullOrEmpty() ||
                dryCellPoolBlueprint.IsNullOrEmpty()
            )
            {
                return false;
            }

            LiquidVolume existing =
                null;

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
                    existing =
                        liquid;

                    break;
                }
            }

            //
            // Wet cell:
            // genuinely mix the new liquid into the existing open volume.
            //
            if (existing != null)
            {
                int amount =
                    Math.Max(
                        1,
                        existing.Volume
                    );

                existing.MixWith(
                    new LiquidVolume(
                        liquidID,
                        amount
                    )
                );

                existing.CheckImage();

                return true;
            }

            //
            // Dry cell:
            // preserve the theme's actual pool blueprint.
            //
            GameObject pool =
                GameObjectFactory
                    .Factory
                    .CreateObject(
                        dryCellPoolBlueprint
                    );

            if (
                pool == null ||
                pool.LiquidVolume == null
            )
            {
                if (pool != null)
                {
                    pool.Destroy();
                }

                return false;
            }

            cell.AddObject(
                pool
            );

            pool.LiquidVolume.CheckImage();

            return true;
        }
    }

        /// <summary>
    /// Theme-owned description of one Category-4 floor treatment.
    ///
    /// A floor uses exactly one representation:
    ///
    ///   1. a concrete Floor GameObject blueprint, optionally customized
    ///      after creation; or
    ///
    ///   2. a cell-paint operation that writes PaintTile / paint colors /
    ///      PaintRenderString directly onto the Cell.
    ///
    /// The shared floor system knows nothing about individual themes.
    /// Theme files construct these specifications.
    /// </summary>
    internal sealed class SubterraneanSiteDevEPFloorSpec
    {
        public string FloorBlueprint = "";

        public Action<Cell> PaintCell = null;

        public Action<GameObject> ConfigureFloorObject = null;
    }


    /// <summary>
    /// Shared Category-4 floor application.
    ///
    /// Underground EP layers:
    ///     the selected C4 floor is applied to every cell in the zone.
    ///
    /// Origin layer:
    ///     the selected entrance floor is applied to every cell inside
    ///     the dimensional scar.
    ///
    /// Floor is substrate. Geometry, solidity, reservations, decorations,
    /// creatures, walls, liquids, and other content do not affect whether
    /// a cell receives its floor.
    /// </summary>
    internal static class SubterraneanSiteDevEPFloorSystem
    {
        internal static bool Apply(
            Zone Z,
            SubterraneanSiteDevEPFloorSpec spec,
            bool entranceOnly
        )
        {
            if (
                Z == null ||
                spec == null
            )
            {
                return true;
            }

            bool usesBlueprint =
                !spec.FloorBlueprint.IsNullOrEmpty();

            bool usesPaint =
                spec.PaintCell != null;

            //
            // A floor specification must choose exactly one representation.
            //
            if (usesBlueprint == usesPaint)
            {
                return true;
            }

            //
            // Match the behavior of vanilla zone-wide floor-painter parts
            // such as Mushroomy and Rocky.
            //
            if (
                usesPaint &&
                Options.DisableFloorTextureObjects
            )
            {
                return true;
            }

            foreach (
                Cell cell
                in Z.GetCells()
            )
            {
                if (cell == null)
                    continue;

                //
                // Origin preview owns only the dimensional scar.
                //
                if (
                    entranceOnly &&
                    !SubterraneanSiteDevEPEntrance
                        .IsInsideScar(
                            Z,
                            cell
                        )
                )
                {
                    continue;
                }

                //
                // Blueprint-backed floor.
                //
                if (usesBlueprint)
                {
                    GameObject floor =
                        cell.FindObject(
                            spec.FloorBlueprint
                        );

                    if (floor == null)
                    {
                        floor =
                            cell.AddObject(
                                spec.FloorBlueprint
                            );
                    }

                    if (
                        floor != null &&
                        spec.ConfigureFloorObject != null
                    )
                    {
                        spec.ConfigureFloorObject(
                            floor
                        );
                    }

                    continue;
                }

                //
                // Cell-painted floor.
                //
                // The selected C4 treatment completely owns the paint
                // channels so no stale values survive from earlier terrain.
                //
                cell.PaintTile = null;
                cell.PaintTileColor = null;
                cell.PaintColorString = null;
                cell.PaintDetailColor = null;
                cell.PaintRenderString = null;

                spec.PaintCell(
                    cell
                );
            }

            return true;
        }
    }

    /// <summary>
    /// Shared entrance-scar properties and registration. The origin layer keeps
    /// its ordinary zone build everywhere except for a harsh, clipped circular
    /// overwrite around the planned outgoing EP hole.
    /// </summary>
    internal static class SubterraneanSiteDevEPEntrance
    {
        internal const string CenterProperty =
            "SubterraneanSiteDev_EPEntranceCenter";

        internal const string ScarRadiusProperty =
            "SubterraneanSiteDev_EPEntranceScarRadius";

        internal const string HoleRadiusProperty =
            "SubterraneanSiteDev_EPEntranceHoleRadius";

        internal const int DefaultScarRadius = 11;
        internal const int DefaultHoleRadius = 5;

        internal static void Register(
            SubterraneanSiteDevEPLayerContext layer,
            ISubterraneanSiteDevEPCategoryProvider category1Theme,
            ISubterraneanSiteDevEPCategoryProvider decorationTheme,
            ISubterraneanSiteDevEPCategoryProvider floorTheme
        )
        {
            if (
                layer == null ||
                category1Theme == null ||
                decorationTheme == null ||
                floorTheme == null
            )
            {
                return;
            }

            Location2D center = null;

            SubterraneanSiteDevEPVerticalTransitions.TryGetCoordinate(
                layer.ZoneId,
                SubterraneanSiteDevEPVerticalTransitions.OutgoingHoleProperty,
                out center
            );

            if (center == null)
            {
                center = Location2D.Get(40, 12);
            }

            SubterraneanSiteDevEPEntranceContext entrance =
                new SubterraneanSiteDevEPEntranceContext
                {
                    ZoneId = layer.ZoneId,
                    Tier = layer.Tier,
                    CenterX = center.X,
                    CenterY = center.Y,
                    ScarRadius = DefaultScarRadius,
                    HoleRadius = DefaultHoleRadius
                };

            The.ZoneManager.SetZoneProperty(
                layer.ZoneId,
                CenterProperty,
                center.X.ToString() + "," + center.Y.ToString()
            );

            The.ZoneManager.SetZoneProperty(
                layer.ZoneId,
                ScarRadiusProperty,
                entrance.ScarRadius.ToString()
            );

            The.ZoneManager.SetZoneProperty(
                layer.ZoneId,
                HoleRadiusProperty,
                entrance.HoleRadius.ToString()
            );

            // Preserve the ordinary origin zone outside this scar.
            The.ZoneManager.AddZonePostBuilder(
                layer.ZoneId,
                "SubterraneanSiteDevEPEntranceScar",
                "CenterX", entrance.CenterX.ToString(),
                "CenterY", entrance.CenterY.ToString(),
                "Radius", entrance.ScarRadius.ToString()
            );


            //
            // Category-1 physical preview.
            //
            // This is deliberately independent of entrance Category-5 decoration.
            // Fungus puffers, and future attunement-coupled C1 objects, belong here.
            //
            ISubterraneanSiteDevEPPrimaryObjectProvider
                primaryObjectProvider =
                    category1Theme
                        as ISubterraneanSiteDevEPPrimaryObjectProvider;

            if (primaryObjectProvider != null)
            {
                primaryObjectProvider
                    .RegisterEntranceCategory1Objects(
                        entrance
                    );
            }

            //
            // The attunement stone is essential shared EP infrastructure.
            // Give it priority over entrance Category-5 decoration so dense
            // themes cannot consume or visually cover its two-cell footprint.
            //
            The.ZoneManager.AddZonePostBuilder(
                layer.ZoneId,
                "SubterraneanSiteDevEPAttunementStoneBuilder",
                "EntranceMode", "1"
            );

            decorationTheme.RegisterEntranceDecorations(entrance);

            //
            // Category-4 floor is substrate, not placement geometry.
            // Apply it once after all scar content has been registered so
            // every scar cell receives the selected floor underneath whatever
            // objects were placed there.
            //

            floorTheme.RegisterEntranceFloor(entrance);
        }

        internal static bool TryGetSpec(
            Zone Z,
            out Location2D center,
            out int scarRadius,
            out int holeRadius
        )
        {
            center = null;
            scarRadius = DefaultScarRadius;
            holeRadius = DefaultHoleRadius;

            if (Z == null)
                return false;

            string centerText =
                The.ZoneManager.GetZoneProperty(
                    Z.ZoneID,
                    CenterProperty
                ) as string;

            if (centerText.IsNullOrEmpty())
                return false;

            string[] parts = centerText.Split(',');
            if (parts.Length != 2)
                return false;

            int x;
            int y;
            if (!int.TryParse(parts[0], out x) ||
                !int.TryParse(parts[1], out y))
            {
                return false;
            }

            center = Location2D.Get(x, y);

            string scarText =
                The.ZoneManager.GetZoneProperty(
                    Z.ZoneID,
                    ScarRadiusProperty
                ) as string;

            int parsed;
            if (int.TryParse(scarText, out parsed) && parsed > 0)
                scarRadius = parsed;

            string holeText =
                The.ZoneManager.GetZoneProperty(
                    Z.ZoneID,
                    HoleRadiusProperty
                ) as string;

            if (int.TryParse(holeText, out parsed) && parsed > 0)
                holeRadius = parsed;

            return true;
        }

        internal static bool IsInsideScar(Zone Z, Cell cell)
        {
            if (cell == null)
                return false;

            Location2D center;
            int scarRadius;
            int holeRadius;

            if (!TryGetSpec(Z, out center, out scarRadius, out holeRadius))
                return false;

            int dx = cell.X - center.X;
            int dy = cell.Y - center.Y;

            return dx * dx + dy * dy <= scarRadius * scarRadius;
        }

        internal static bool IsInsideHoleExclusion(
            Zone Z,
            Cell cell,
            int padding
        )
        {
            if (cell == null)
                return false;

            Location2D center;
            int scarRadius;
            int holeRadius;

            if (!TryGetSpec(Z, out center, out scarRadius, out holeRadius))
                return false;

            int radius = holeRadius + Math.Max(0, padding);
            int dx = cell.X - center.X;
            int dy = cell.Y - center.Y;

            return dx * dx + dy * dy <= radius * radius;
        }

        internal static int GetHoleRadius(string zoneId, int fallback)
        {
            if (zoneId.IsNullOrEmpty())
                return fallback;

            string text =
                The.ZoneManager.GetZoneProperty(
                    zoneId,
                    HoleRadiusProperty
                ) as string;

            int parsed;
            if (int.TryParse(text, out parsed) && parsed > 0)
                return parsed;

            return fallback;
        }
    }

    /// <summary>
    /// Registration order for independently swappable EP categories.
    ///
    /// The conceptual category numbers are not the physical build order:
    ///
    ///  1. neutral solid fill
    ///  2. Category 4 layout/shape
    ///  3. classify core vs active boundary
    ///  4. optional Category 1 physical content
    ///  5. Category 2 hazards/effects
    ///  6. Category 5 decorations/fixtures
    ///  7. rebuild reachable geometry
    ///  8. shared attunement stone
    ///  9. Category 3 materials
    /// 10. Category 1 runtime environment
    ///
    /// Category-4 floor is no longer part of the structural build sequence.
    /// It is substrate and is registered once later by the shuffled-theme
    /// coordinator after shared treasure/population content.
    /// </summary>
    internal static class SubterraneanSiteDevEPCategoryPipeline
    {

        internal static void RegisterMixedLayer(
            SubterraneanSiteDevEPLayerContext context,
            ISubterraneanSiteDevEPCategoryProvider category1,
            ISubterraneanSiteDevEPCategoryProvider category2,
            ISubterraneanSiteDevEPCategoryProvider category3,
            ISubterraneanSiteDevEPCategoryProvider category4,
            ISubterraneanSiteDevEPCategoryProvider category5
        )
        {
            if (
                context == null ||
                category1 == null ||
                category2 == null ||
                category3 == null ||
                category4 == null ||
                category5 == null
            )
            {
                return;
            }

            if (context.IsOrigin)
            {
                // Mixed themes register their special entrance explicitly so
                // primary/secondary semantics are available. Never ClearAll the
                // origin layer here; most of the pre-existing zone is preserved.
                return;
            }

            The.ZoneManager.AddZonePostBuilder(
                context.ZoneId,
                "SubterraneanSiteDevEPSolidPlaceholderFill"
            );

            category4.RegisterCategory4Layout(context);

            The.ZoneManager.AddZonePostBuilder(
                context.ZoneId,
                "SubterraneanSiteDevEPBoundaryClassifier"
            );

            //
            // Vertical transitions are mandatory shared EP infrastructure.
            //
            // Theme layouts are expected to incorporate their anchors naturally,
            // but this pass is the final geometric guarantee that no incoming
            // landing or outgoing hole is stranded in an isolated pocket.
            //
            The.ZoneManager.AddZonePostBuilder(
                context.ZoneId,
                "SubterraneanSiteDevEPAnchorConnectivity"
            );

            //
            // Category-1 physical content is optional. Only the actual Category-1
            // provider is queried, so secondary themes cannot accidentally contribute
            // attunement-coupled physical hazards.
            //
            // This executes while abstract geometry still exists and before C2/C5.
            //
            ISubterraneanSiteDevEPPrimaryObjectProvider primaryObjectProvider =
                category1 as ISubterraneanSiteDevEPPrimaryObjectProvider;

            if (primaryObjectProvider != null)
            {
                primaryObjectProvider.RegisterCategory1Objects(
                    context
                );
            }

            category2.RegisterCategory2(context);

            //
            // Build reachability before placing the shared attunement stone.
            //
            // The stone is essential EP infrastructure, so it gets priority
            // over Category-5 decoration. This prevents dense decoration themes
            // from exhausting every legal two-cell stone location.
            //
            The.ZoneManager.AddZonePostBuilder(
                context.ZoneId,
                "SubterraneanSiteDevEPGeometryReachability"
            );

            //
            // Shared site system, not one of the five shuffled categories.
            //
            // Place and reserve the stone before Category 5 so later discrete
            // decoration respects its two-cell footprint.
            //
            The.ZoneManager.AddZonePostBuilder(
                context.ZoneId,
                "SubterraneanSiteDevEPAttunementStoneBuilder"
            );

            category5.RegisterCategory5(context);

            //
            // Category 5 may add solid furniture, colonies, or other obstacles.
            // Rebuild reachability afterward so later shared content sees the
            // final decorated traversable space.
            //
            The.ZoneManager.AddZonePostBuilder(
                context.ZoneId,
                "SubterraneanSiteDevEPGeometryReachability"
            );

            category3.RegisterCategory3(context);

            category1.RegisterCategory1(context);

        }
    }

    /// <summary>
    /// Shared EP attunement infrastructure.
    ///
    /// Category 1 owns the pocket's attunement target. Theme-specific
    /// attunement construction is delegated to that theme's
    /// ISubterraneanSiteDevEPAttunementProvider.
    ///
    /// Shared code owns duration, tier scaling, provider lookup,
    /// current-attunement lookup, and common effect lifecycle machinery.
    /// </summary>
    internal static class SubterraneanSiteDevEPAttunementSystem
    {
        internal const string StoneLeftBlueprint =
            "SubterraneanSiteDevEPAttunementStoneLeft";

        internal const string StoneRightBlueprint =
            "SubterraneanSiteDevEPAttunementStoneRight";

        internal const int DefaultDuration = 300;
        internal const int CountdownInterval = 20;

        internal const string ProviderTypeProperty =
            "SubterraneanSiteDev_EP_AttunementProviderType";

        internal const string TierProperty =
            "SubterraneanSiteDev_EP_Tier";


        internal static int GetAttunementMutationLevel(
            Zone Z
        )
        {
            int tier = 1;

            if (Z != null)
            {
                string tierText =
                    The.ZoneManager.GetZoneProperty(
                        Z.ZoneID,
                        TierProperty
                    ) as string;

                int parsed;

                if (
                    int.TryParse(tierText, out parsed) &&
                    parsed >= 1
                )
                {
                    tier = parsed;
                }
                else
                {
                    tier = Z.NewTier;
                }
            }

            tier = Math.Max(1, Math.Min(8, tier));

            return tier * 2;
        }

        internal static bool TryGetAttunementProvider(
            Zone Z,
            out ISubterraneanSiteDevEPAttunementProvider provider
        )
        {
            provider = null;

            if (
                Z == null ||
                The.ZoneManager == null
            )
            {
                return false;
            }

            string typeName =
                The.ZoneManager.GetZoneProperty(
                    Z.ZoneID,
                    ProviderTypeProperty
                ) as string;

            return
                SubterraneanSiteDevEPProviderFactory
                    .TryCreate(
                        typeName,
                        out provider
                    );
        }

        internal static string GetCategory1Theme(Zone Z)
        {
            if (Z == null)
                return null;

            //
            // Every valid EP composition explicitly persists its actual
            // Category-1 owner. Category 1 is currently constrained to equal
            // the primary dimensional theme, but this property remains the
            // authoritative environmental/attunement lookup.
            //
            return
                The.ZoneManager.GetZoneProperty(
                    Z.ZoneID,
                    SubterraneanSiteDevEPShuffledTheme
                        .Category1ThemeProperty
                ) as string;
        }

        internal static XRL.World.Effects
            .SubterraneanSiteDevEPAttunementEffect
            GetCurrentAttunement(
                GameObject actor
            )
        {
            if (actor == null)
                return null;

            //
            // IMPORTANT:
            // GameObject.TryGetEffect<T>() matches the exact runtime type.
            //
            // Theme-specific attunements derive from the shared EP effect, so use
            // Qud's inheritance-aware lookup instead.
            //
            return actor.GetEffectDescendedFrom<
                XRL.World.Effects
                    .SubterraneanSiteDevEPAttunementEffect
            >();
        }

            internal static bool IsAttunedTo(
                GameObject actor,
                string themeKey
            )
            {
                if (
                    actor == null ||
                    themeKey.IsNullOrEmpty()
                )
                {
                    return false;
                }

                XRL.World.Effects
                    .SubterraneanSiteDevEPAttunementEffect effect =
                        GetCurrentAttunement(
                            actor
                        );

                return
                    effect != null &&
                    string.Equals(
                        effect.ThemeKey,
                        themeKey,
                        StringComparison.Ordinal
                    );
            }



    }

    /// <summary>
    /// Site-wide dimensional category coordinator.
    ///
    /// The caller supplies the pocket's primary and secondary dimensional
    /// themes. Category 1 always belongs to the primary theme because it defines
    /// the pocket's dominant environmental influence and attunement target.
    ///
    /// Categories 2-5 independently make deterministic 50/50 choices between
    /// the primary and secondary themes. The choices are made once when this
    /// object is constructed and remain constant across every layer of the EP.
    /// </summary>
    internal sealed class SubterraneanSiteDevEPShuffledTheme :
        ISubterraneanSiteDevEPTheme
    {
        internal const string PrimaryThemeProperty =
            "SubterraneanSiteDev_EP_PrimaryTheme";

        internal const string SecondaryThemeProperty =
            "SubterraneanSiteDev_EP_SecondaryTheme";

        internal const string PrimaryDenizenProviderTypeProperty =
            "SubterraneanSiteDev_EP_PrimaryDenizenProviderType";

        internal const string SecondaryDenizenProviderTypeProperty =
            "SubterraneanSiteDev_EP_SecondaryDenizenProviderType";

        internal const string Category1ThemeProperty =
            "SubterraneanSiteDev_EP_Category1Theme";
        internal const string Category2ThemeProperty =
            "SubterraneanSiteDev_EP_Category2Theme";
        internal const string Category3ThemeProperty =
            "SubterraneanSiteDev_EP_Category3Theme";
        internal const string Category4ThemeProperty =
            "SubterraneanSiteDev_EP_Category4Theme";
        internal const string Category5ThemeProperty =
            "SubterraneanSiteDev_EP_Category5Theme";

        // DEV/DIAGNOSTIC NAME MODE.
        //
        // false = use the player-facing extradimensional collision name.
        // true  = restore the old [C1/C2/C3/C4/C5] category signature.
        //
        // The category signature is intentionally retained because it is useful
        // while testing individual theme/category behavior.
        internal const bool UseCategoryDebugSiteName = false;

        private readonly ISubterraneanSiteDevEPCategoryProvider primaryTheme;
        private readonly ISubterraneanSiteDevEPCategoryProvider secondaryTheme;

        private readonly ISubterraneanSiteDevEPCategoryProvider category1;
        private readonly ISubterraneanSiteDevEPCategoryProvider category2;
        private readonly ISubterraneanSiteDevEPCategoryProvider category3;
        private readonly ISubterraneanSiteDevEPCategoryProvider category4;
        private readonly ISubterraneanSiteDevEPCategoryProvider category5;

        private readonly string siteDisplayName;

        public SubterraneanSiteDevEPShuffledTheme(
            string siteKey,
            ISubterraneanSiteDevEPCategoryProvider primaryTheme,
            ISubterraneanSiteDevEPCategoryProvider secondaryTheme
        )
        {
            this.primaryTheme = primaryTheme;
            this.secondaryTheme = secondaryTheme;

            if (this.primaryTheme == null || this.secondaryTheme == null)
            {
                throw new ArgumentNullException(
                    "Both shuffled EP themes must be supplied."
                );
            }

            string stableSiteKey =
                siteKey.IsNullOrEmpty()
                    ? "UnknownSite"
                    : siteKey;

            int seed =
                XRLCore.Core.Game.GetWorldSeed(
                    "SubterraneanSiteDev:EPCategoryShuffle:" +
                    stableSiteKey + ":" +
                    this.primaryTheme.ThemeKey + ":" +
                    this.secondaryTheme.ThemeKey
                );

            System.Random rng =
                new System.Random(seed);

            //
            // Category 1 is the dominant dimensional influence.
            // It is always supplied by the primary theme and is therefore
            // also the theme used by attunement.
            //
            category1 =
                this.primaryTheme;

            //
            // Categories 2-5 independently express either of the pocket's
            // two underlying dimensional themes.
            //
            category2 =
                PickTheme(rng);

            category3 =
                PickTheme(rng);

            category4 =
                PickTheme(rng);

            category5 =
                PickTheme(rng);

            string categoryDebugName =
                "An Extradimensional Pocket [" +
                ShortKey(category1.ThemeKey) + "/" +
                ShortKey(category2.ThemeKey) + "/" +
                ShortKey(category3.ThemeKey) + "/" +
                ShortKey(category4.ThemeKey) + "/" +
                ShortKey(category5.ThemeKey) + "]";

            if (UseCategoryDebugSiteName)
            {
                siteDisplayName =
                    categoryDebugName;
            }
            else
            {
                string generatedName =
                    SubterraneanSiteDevEPNameGenerator
                        .Generate(
                            stableSiteKey,
                            this.primaryTheme.ThemeKey,
                            this.secondaryTheme.ThemeKey
                        );
                siteDisplayName =
                    generatedName.IsNullOrEmpty()
                        ? categoryDebugName
                        : generatedName;
            }
        }

        public string ThemeKey
        {
            get
            {
                return
                    "Shuffle(" +
                    primaryTheme.ThemeKey + "," +
                    secondaryTheme.ThemeKey + ")";
            }
        }

        public string SiteDisplayName
        {
            get { return siteDisplayName; }
        }

        public int MinimumHoleSeparation
        {
            get { return category4.MinimumHoleSeparation; }
        }

        public void RegisterLayer(
            SubterraneanSiteDevEPLayerContext context
        )
        {
            if (context == null)
                return;

            The.ZoneManager.SetZoneProperty(
                context.ZoneId,
                SubterraneanSiteDevEPAttunementSystem.TierProperty,
                context.Tier.ToString()
            );

            // Persist both the dimensional pair and the five category winners.
            // Attunement/denizen systems can use the pair; category-specific
            // systems can read the actual expression independently.
            The.ZoneManager.SetZoneProperty(
                context.ZoneId,
                PrimaryThemeProperty,
                primaryTheme.ThemeKey
            );
            The.ZoneManager.SetZoneProperty(
                context.ZoneId,
                SecondaryThemeProperty,
                secondaryTheme.ThemeKey
            );

            ISubterraneanSiteDevEPDenizenAdaptationProvider
                primaryDenizenProvider =
                    primaryTheme
                        as ISubterraneanSiteDevEPDenizenAdaptationProvider;


            ISubterraneanSiteDevEPDenizenAdaptationProvider
                secondaryDenizenProvider =
                    secondaryTheme
                        as ISubterraneanSiteDevEPDenizenAdaptationProvider;


            The.ZoneManager.SetZoneProperty(
                context.ZoneId,
                PrimaryDenizenProviderTypeProperty,
                primaryDenizenProvider == null
                    ? ""
                    : primaryTheme
                        .GetType()
                        .AssemblyQualifiedName
            );


            The.ZoneManager.SetZoneProperty(
                context.ZoneId,
                SecondaryDenizenProviderTypeProperty,
                secondaryDenizenProvider == null
                    ? ""
                    : secondaryTheme
                        .GetType()
                        .AssemblyQualifiedName
            );

            The.ZoneManager.SetZoneProperty(
                context.ZoneId,
                Category1ThemeProperty,
                category1.ThemeKey
            );

            ISubterraneanSiteDevEPAttunementProvider
                attunementProvider =
                    category1
                        as ISubterraneanSiteDevEPAttunementProvider;

            The.ZoneManager.SetZoneProperty(
                context.ZoneId,
                SubterraneanSiteDevEPAttunementSystem
                    .ProviderTypeProperty,
                attunementProvider == null
                    ? ""
                    : category1
                        .GetType()
                        .AssemblyQualifiedName
            );

            The.ZoneManager.SetZoneProperty(
                context.ZoneId,
                Category2ThemeProperty,
                category2.ThemeKey
            );
            The.ZoneManager.SetZoneProperty(
                context.ZoneId,
                Category3ThemeProperty,
                category3.ThemeKey
            );
            The.ZoneManager.SetZoneProperty(
                context.ZoneId,
                Category4ThemeProperty,
                category4.ThemeKey
            );
            The.ZoneManager.SetZoneProperty(
                context.ZoneId,
                Category5ThemeProperty,
                category5.ThemeKey
            );

            //
            // The extradimensional pocket has one musical identity from the
            // entrance scar through every underground layer.
            //
            The.ZoneManager.AddZoneBuilder(
                context.ZoneId,
                6000,
                "Music",
                "Track", "Music/Substrate"
            );

            if (context.IsOrigin)
            {
                //
                // Build the localized scar first.
                //
                SubterraneanSiteDevEPEntrance.Register(
                    context,
                    category1,
                    category5,
                    category4
                );

                //
                // Category 1 is the global/dominant dimensional environment.
                // Unlike C2-C5, it applies across the whole origin zone too.
                //
                category1.RegisterCategory1(
                    context
                );

                return;
            }

            SubterraneanSiteDevEPCategoryPipeline.RegisterMixedLayer(
                context,
                category1,
                category2,
                category3,
                category4,
                category5
            );

            // Shared EP treasure pass. Every underground layer gets one
            // tier-appropriate vanilla container. Qud generates its normal
            // contents; the chest builder only assigns those contents to the
            // pocket's two dimensional identities.
            The.ZoneManager.AddZonePostBuilder(
                context.ZoneId,
                "SubterraneanSiteDevEPChestBuilder",
                "Tier", context.Tier.ToString(),
                "PrimaryThemeKey", primaryTheme.ThemeKey,
                "SecondaryThemeKey", secondaryTheme.ThemeKey
            );
            

            if (context.IsBottom)
            {
                The.ZoneManager.AddZonePostBuilder(
                    context.ZoneId,
                    "SubterraneanSiteDevEPHeroBuilder",
                    "Tier", context.Tier.ToString(),
                    "PrimaryThemeKey", primaryTheme.ThemeKey,
                    "SecondaryThemeKey", secondaryTheme.ThemeKey
                );

                The.ZoneManager.AddZonePostBuilder(
                    context.ZoneId,
                    "SubterraneanSiteDevEPRelicBuilder",
                    "Tier", context.Tier.ToString(),
                    "PrimaryThemeKey", primaryTheme.ThemeKey,
                    "SecondaryThemeKey", secondaryTheme.ThemeKey
                );

            }

            The.ZoneManager.AddZonePostBuilder(
                context.ZoneId,
                "SubterraneanSiteDevEPDenizenBuilder",
                "Tier", context.Tier.ToString(),
                "PrimaryThemeKey", primaryTheme.ThemeKey,
                "SecondaryThemeKey", secondaryTheme.ThemeKey,
                "MinDenizensPerTheme", "1",
                "MaxDenizensPerTheme", "3"
            );


            //
            // Category-4 floor is universal substrate.
            //
            // Register it once after theme content, treasure, bosses/relics,
            // and denizens. The selected Category-4 provider still owns the
            // floor recipe; the shuffled coordinator only controls when that
            // recipe is applied.
            //
            category4.RegisterCategory4Floor(
                context
            );

        }

        private ISubterraneanSiteDevEPCategoryProvider PickTheme(
            System.Random rng
        )
        {
            return
                rng.Next(2) == 0
                    ? primaryTheme
                    : secondaryTheme;
        }

        private static string ShortKey(string key)
        {
            if (key.IsNullOrEmpty())
                return "?";

            return key.Substring(0, 1).ToUpperInvariant();
        }
    }

    /// <summary>
    /// Shared EP site registrar. Owns only site-wide mechanics: deterministic
    /// vertical-transition planning, layered-site registration, and final pit
    /// materialization. Theme builders remain in the selected theme file.
    /// </summary>
    internal sealed class ExtradimensionalPocketSiteRegistrar
    {
        private readonly ISubterraneanSiteDevEPHost host;
        private readonly ISubterraneanSiteDevEPTheme theme;

        public ExtradimensionalPocketSiteRegistrar(
            ISubterraneanSiteDevEPHost host,
            ISubterraneanSiteDevEPTheme theme
        )
        {
            this.host = host;
            this.theme = theme;
        }

        public bool Register(List<string> siteZoneIds)
        {
            if (host == null || theme == null ||
                siteZoneIds == null || siteZoneIds.Count == 0)
            {
                return false;
            }

            if (!SubterraneanSiteDevEPVerticalTransitions.Prepare(
                siteZoneIds,
                theme.MinimumHoleSeparation
            ))
            {
                return false;
            }

            string discoveryKey =
                "SubterraneanSiteDev_Discovered_ExtradimensionalPocket_" +
                siteZoneIds[0];

            if (!host.RegisterLayeredSite(
                siteZoneIds,
                theme.SiteDisplayName,
                discoveryKey,
                theme.RegisterLayer
            ))
            {
                return false;
            }

            // Shared EP bottom exit. This is deliberately registered after all theme,
            // treasure, hero, and denizen builders so nothing later overwrites it.
            if (siteZoneIds.Count > 1)
            {
                string originZoneId =
                    siteZoneIds[0];

                string bottomZoneId =
                    siteZoneIds[siteZoneIds.Count - 1];

                The.ZoneManager.AddZonePostBuilder(
                    bottomZoneId,
                    "SubterraneanSiteDevEPExitTeleporterBuilder",
                    "TargetZone", originZoneId
                );
            }

            // Must run after theme builders so layout/decorations cannot erase pits.
            SubterraneanSiteDevEPVerticalTransitions.RegisterHoleBuilders(
                siteZoneIds
            );

            return true;
        }
    }

    /// <summary>
    /// Shared registration helpers for Category-2 hazards that mount directly
    /// into the semantic EP boundary created by Category 4.
    ///
    /// Nondirectional mode is currently used by Fire and Electrical.
    /// Directional mode is currently used by Cold.
    /// </summary>
    internal static class SubterraneanSiteDevEPBoundaryHazards
    {
        internal static void Register(
            SubterraneanSiteDevEPLayerContext context,
            string hazardBlueprint,
            int boundaryCellsPerMount,
            int minMounts,
            int maxMounts,
            int minSpacing,
            int outerBorderExclusion = 0
        )
        {
            if (
                context == null ||
                hazardBlueprint.IsNullOrEmpty()
            )
            {
                return;
            }

            The.ZoneManager.AddZonePostBuilder(
                context.ZoneId,
                "SubterraneanSiteDevEPBoundaryHazardBuilder",
                "HazardBlueprint", hazardBlueprint,
                "BoundaryCellsPerMount",
                    boundaryCellsPerMount.ToString(),
                "MinMounts", minMounts.ToString(),
                "MaxMounts", maxMounts.ToString(),
                "MinSpacing", minSpacing.ToString(),
                "OuterBorderExclusion", outerBorderExclusion.ToString()
            );
        }


        internal static void RegisterDirectional(
            SubterraneanSiteDevEPLayerContext context,
            string northBlueprint,
            string southBlueprint,
            string eastBlueprint,
            string westBlueprint,
            int minMounts,
            int maxMounts,
            int minSpacing,
            int anchorExclusionRadius,
            int maxTurnInterval
        )
        {
            if (
                context == null ||
                northBlueprint.IsNullOrEmpty() ||
                southBlueprint.IsNullOrEmpty() ||
                eastBlueprint.IsNullOrEmpty() ||
                westBlueprint.IsNullOrEmpty()
            )
            {
                return;
            }


            string directionalBlueprints =
                northBlueprint + "|" +
                southBlueprint + "|" +
                eastBlueprint + "|" +
                westBlueprint;

            The.ZoneManager.AddZonePostBuilder(
                context.ZoneId,
                "SubterraneanSiteDevEPBoundaryHazardBuilder",
                "DirectionalBlueprints", directionalBlueprints,
                "MinMounts", minMounts.ToString(),
                "MaxMounts", maxMounts.ToString(),
                "MinSpacing", minSpacing.ToString(),
                "AnchorExclusionRadius",
                    anchorExclusionRadius.ToString(),
                "MaxTurnInterval", maxTurnInterval.ToString()
            );
        }
    }


}

namespace XRL.World.ZoneBuilders
{
    public class SubterraneanSiteDevEPSolidPlaceholderFill : ZoneBuilderSandbox
    {
        public bool BuildZone(Zone Z)
        {
            if (Z == null)
                return true;
            //
            // This underground EP completely owns its connection topology.
            //
            // The vanilla zone that would ordinarily exist here may already have
            // ZoneConnection metadata even though its builders/content are being replaced.
            // Qud's final zone-build pass uses those connections to force paths through
            // walls after our EP builders have finished.
            //
            // Disable that generic repair and discard both persistent and build-local
            // vanilla connection points. EP vertical travel is handled entirely by our
            // own planned incoming/outgoing anchors and pit/teleporter system.
            //
            The.ZoneManager.SetZoneProperty(
                Z.ZoneID,
                "DisableForcedConnections",
                "Yes"
            );

            The.ZoneManager
                .GetZoneConnections(
                    Z.ZoneID
                )
                .Clear();

            Z.ClearZoneConnectionCache();

            //
            // Start this EP layer with a fresh semantic reservation map.
            //
            // Later C1/C2/C5 builders all consult the same per-zone grid.
            // Resetting here guarantees that no temporary claims from an earlier
            // build of this ZoneID can survive into the new construction pass.
            //
            SubterraneanSiteDev
                .SubterraneanSiteDevEPReservations
                .Reset(Z);

            foreach (Cell cell in Z.GetCells())
            {
                if (cell == null)
                    continue;

                cell.Clear();
                cell.AddObject(
                    SubterraneanSiteDev.SubterraneanSiteDevEPGeometry
                        .SolidPlaceholderBlueprint
                );
            }

            Z.ClearReachableMap();
            return true;
        }
    }

    /// <summary>
    /// Classifies cave-facing/exposed abstract solid cells as boundary cells.
    /// Category 3 later decides what core and boundary materials actually are.
    /// </summary>
    public class SubterraneanSiteDevEPBoundaryClassifier : ZoneBuilderSandbox
    {
        public bool BuildZone(Zone Z)
        {
            if (Z == null)
                return true;

            bool[,] exposed = new bool[Z.Width, Z.Height];

            for (int x = 0; x < Z.Width; x++)
            {
                for (int y = 0; y < Z.Height; y++)
                {
                    Cell cell = Z.GetCell(x, y);

                    if (!SubterraneanSiteDev.SubterraneanSiteDevEPGeometry
                        .IsSolidPlaceholder(cell))
                    {
                        continue;
                    }

                    exposed[x, y] = TouchesOpenGeometry8(Z, x, y);
                }
            }

            for (int x = 0; x < Z.Width; x++)
            {
                for (int y = 0; y < Z.Height; y++)
                {
                    if (!exposed[x, y])
                        continue;

                    Cell cell = Z.GetCell(x, y);
                    if (cell == null)
                        continue;

                    cell.ClearWalls();
                    cell.AddObject(
                        SubterraneanSiteDev.SubterraneanSiteDevEPGeometry
                            .BoundaryPlaceholderBlueprint
                    );
                }
            }

            return true;
        }

        private bool TouchesOpenGeometry8(Zone Z, int x, int y)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0)
                        continue;

                    int nx = x + dx;
                    int ny = y + dy;

                    if (
                        nx < 0 || ny < 0 ||
                        nx >= Z.Width || ny >= Z.Height
                    )
                    {
                        continue;
                    }

                    Cell neighbor = Z.GetCell(nx, ny);

                    if (SubterraneanSiteDev.SubterraneanSiteDevEPGeometry
                        .IsOpenGeometryCell(neighbor))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }

    public class SubterraneanSiteDevEPAnchorConnectivity :
    ZoneBuilderSandbox
    {
        public int AnchorClearRadius = 1;


        public bool BuildZone(
            Zone Z
        )
        {
            if (Z == null)
                return true;


            List<Location2D> anchors =
                SubterraneanSiteDev
                    .SubterraneanSiteDevEPVerticalTransitions
                    .GetVerticalAnchors(
                        Z.ZoneID
                    );


            if (
                anchors == null ||
                anchors.Count == 0
            )
            {
                return true;
            }


            //
            // First guarantee that every transition center and its immediate
            // landing space are geometrically open.
            //
            foreach (
                Location2D anchor
                in anchors
            )
            {
                if (anchor == null)
                    continue;


                OpenAnchorFootprint(
                    Z,
                    anchor
                );
            }


            //
            // Process anchors independently.
            //
            // Recalculate the main region after each repair so the next anchor
            // sees any connection we just added.
            //
            foreach (
                Location2D anchor
                in anchors
            )
            {
                if (anchor == null)
                    continue;


                List<Location2D> mainRegion =
                    FindLargestOpenRegion(
                        Z
                    );


                if (
                    mainRegion == null ||
                    mainRegion.Count == 0
                )
                {
                    continue;
                }


                if (
                    RegionContains(
                        mainRegion,
                        anchor.X,
                        anchor.Y
                    )
                )
                {
                    continue;
                }


                Location2D target =
                    FindNearestPoint(
                        mainRegion,
                        anchor.X,
                        anchor.Y
                    );


                if (target == null)
                    continue;


                CarveConnection(
                    Z,
                    anchor.X,
                    anchor.Y,
                    target.X,
                    target.Y
                );
            }


            return true;
        }



        private void OpenAnchorFootprint(
            Zone Z,
            Location2D anchor
        )
        {
            int radius =
                Math.Max(
                    0,
                    AnchorClearRadius
                );


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
                    int x =
                        anchor.X +
                        dx;


                    int y =
                        anchor.Y +
                        dy;


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


                    if (cell == null)
                        continue;


                    cell.ClearWalls();
                }
            }
        }



        private List<Location2D> FindLargestOpenRegion(
            Zone Z
        )
        {
            bool[,] visited =
                new bool[
                    Z.Width,
                    Z.Height
                ];


            List<Location2D> largest =
                new List<Location2D>();


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
                        !IsTraversable(
                            Z.GetCell(
                                x,
                                y
                            )
                        )
                    )
                    {
                        continue;
                    }


                    List<Location2D> region =
                        FloodRegion(
                            Z,
                            x,
                            y,
                            visited
                        );


                    if (
                        region.Count >
                        largest.Count
                    )
                    {
                        largest =
                            region;
                    }
                }
            }


            return largest;
        }



        private List<Location2D> FloodRegion(
            Zone Z,
            int startX,
            int startY,
            bool[,] visited
        )
        {
            List<Location2D> result =
                new List<Location2D>();


            Queue<Location2D> queue =
                new Queue<Location2D>();


            Location2D start =
                Location2D.Get(
                    startX,
                    startY
                );


            if (start == null)
                return result;


            queue.Enqueue(
                start
            );


            visited[
                startX,
                startY
            ] = true;


            while (
                queue.Count >
                0
            )
            {
                Location2D current =
                    queue.Dequeue();


                result.Add(
                    current
                );


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


                        if (
                            !IsTraversable(
                                Z.GetCell(
                                    nx,
                                    ny
                                )
                            )
                        )
                        {
                            continue;
                        }


                        Location2D next =
                            Location2D.Get(
                                nx,
                                ny
                            );


                        if (next == null)
                            continue;


                        visited[
                            nx,
                            ny
                        ] = true;


                        queue.Enqueue(
                            next
                        );
                    }
                }
            }


            return result;
        }



        private bool IsTraversable(
            Cell cell
        )
        {
            if (
                cell == null ||
                !SubterraneanSiteDev
                    .SubterraneanSiteDevEPGeometry
                    .IsOpenGeometryCell(
                        cell
                    )
            )
            {
                return false;
            }


            if (
                !cell.IsSolid()
            )
            {
                return true;
            }


            //
            // Closed doors are still valid traversable architecture.
            //
            return
                cell.GetFirstObjectWithPart(
                    "Door"
                ) != null;
        }



        private bool RegionContains(
            List<Location2D> region,
            int x,
            int y
        )
        {
            foreach (
                Location2D point
                in region
            )
            {
                if (
                    point != null &&
                    point.X == x &&
                    point.Y == y
                )
                {
                    return true;
                }
            }


            return false;
        }



        private Location2D FindNearestPoint(
            List<Location2D> region,
            int x,
            int y
        )
        {
            Location2D nearest =
                null;


            int nearestDistance =
                int.MaxValue;


            foreach (
                Location2D point
                in region
            )
            {
                if (point == null)
                    continue;


                int dx =
                    point.X -
                    x;


                int dy =
                    point.Y -
                    y;


                int distance =
                    dx * dx +
                    dy * dy;


                if (
                    distance <
                    nearestDistance
                )
                {
                    nearest =
                        point;


                    nearestDistance =
                        distance;
                }
            }


            return nearest;
        }



        private void CarveConnection(
            Zone Z,
            int x1,
            int y1,
            int x2,
            int y2
        )
        {
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
                CarveCell(
                    Z,
                    x,
                    y
                );


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
        }



        private void CarveCell(
            Zone Z,
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


            Cell cell =
                Z.GetCell(
                    x,
                    y
                );


            if (cell == null)
                return;


            cell.ClearWalls();
        }
    }

    /// <summary>
    /// Rebuild reachability before Category 3 turns abstract exterior into its
    /// final material. This keeps passable exteriors such as lava out of the
    /// intended traversable/reachable interior.
    /// </summary>
    public class SubterraneanSiteDevEPGeometryReachability : ZoneBuilderSandbox
    {
        public bool BuildZone(Zone Z)
        {
            if (Z == null)
                return true;

            Z.ClearReachableMap();

            Cell start = FindStartCell(Z);
            if (start == null)
                return true;

            bool[,] visited = new bool[Z.Width, Z.Height];
            Queue<Location2D> queue = new Queue<Location2D>();

            queue.Enqueue(Location2D.Get(start.X, start.Y));
            visited[start.X, start.Y] = true;

            while (queue.Count > 0)
            {
                Location2D current = queue.Dequeue();
                Cell cell = Z.GetCell(current.X, current.Y);

                if (!IsTraversableGeometry(cell))
                    continue;

                Z.ReachableMap[current.X, current.Y] = true;

                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0)
                            continue;

                        int nx = current.X + dx;
                        int ny = current.Y + dy;

                        if (
                            nx < 0 || ny < 0 ||
                            nx >= Z.Width || ny >= Z.Height ||
                            visited[nx, ny]
                        )
                        {
                            continue;
                        }

                        visited[nx, ny] = true;

                        Cell next = Z.GetCell(nx, ny);
                        if (IsTraversableGeometry(next))
                            queue.Enqueue(Location2D.Get(nx, ny));
                    }
                }
            }

            return true;
        }

        private Cell FindStartCell(Zone Z)
        {
            Location2D anchor = null;

            if (SubterraneanSiteDev.SubterraneanSiteDevEPVerticalTransitions
                .TryGetCoordinate(
                    Z.ZoneID,
                    SubterraneanSiteDev.SubterraneanSiteDevEPVerticalTransitions
                        .IncomingLandingProperty,
                    out anchor
                ) && anchor != null)
            {
                Cell incoming = Z.GetCell(anchor.X, anchor.Y);
                if (IsTraversableGeometry(incoming))
                    return incoming;
            }

            anchor = null;

            if (SubterraneanSiteDev.SubterraneanSiteDevEPVerticalTransitions
                .TryGetCoordinate(
                    Z.ZoneID,
                    SubterraneanSiteDev.SubterraneanSiteDevEPVerticalTransitions
                        .OutgoingHoleProperty,
                    out anchor
                ) && anchor != null)
            {
                Cell outgoing = Z.GetCell(anchor.X, anchor.Y);
                if (IsTraversableGeometry(outgoing))
                    return outgoing;
            }

            foreach (Cell cell in Z.GetCells())
            {
                if (IsTraversableGeometry(cell))
                    return cell;
            }

            return null;
        }

        private bool IsTraversableGeometry(
            Cell cell
        )
        {
            if (
                !SubterraneanSiteDev
                    .SubterraneanSiteDevEPGeometry
                    .IsOpenGeometryCell(cell)
            )
            {
                return false;
            }

            if (!cell.IsSolid())
                return true;

            //
            // Closed doors are intentionally traversable architecture.
            // Treat them as connected geometry even though the closed
            // door object itself may currently make the cell solid.
            //
            return
                cell.GetFirstObjectWithPart(
                    "Door"
                ) != null;
        }
    }

    /// <summary>
    /// Shared Category-1 ambient-temperature builder. Denizen resistance is
    /// intentionally handled later by the denizen/attunement engine.
    /// </summary>
    public class SubterraneanSiteDevEPTemperature :
        ZoneBuilderSandbox
    {
        public int Temperature = 25;

        public bool BuildZone(
            Zone Z
        )
        {
            if (Z == null)
                return true;

            Z.BaseTemperature =
                Temperature;

            return true;
        }
    }

    /// <summary>
    /// Dev-only regular faction-team builder.
    ///
    /// Intentionally mirrors the useful core of vanilla FactionEncounters:
    /// - faction chosen from a population table
    /// - members restricted to +/- 10 levels from the requested zone level
    /// - members closer to the zone level are weighted more heavily
    /// - party size comes from FactionEncounterNumber_<Faction> or *Default
    /// - party members share an AllyRetinue leader
    /// - faction member inventory and party/zone flavor objects are preserved
    ///
    /// Deliberate difference:
    /// - NO HeroMaker.MakeHero(...)
    /// - the first creature is an ordinary party leader
    /// - the ordinary leader uses member inventory, not hero/leader inventory
    /// </summary>
    public class SubterraneanSiteDevFactionTeam : ZoneBuilderSandbox
    {
        public string Population = "";
        public int Chance = 100;
        public int Rolls = 1;

        public bool BuildZone(Zone Z)
        {
            if (Z == null || Population.IsNullOrEmpty())
                return true;

            for (int roll = 0; roll < Rolls; roll++)
            {
                if (!Chance.in100())
                    continue;

                PopulationResult factionResult =
                    PopulationManager.RollOneFrom(Population);

                if (factionResult == null ||
                    factionResult.Blueprint.IsNullOrEmpty())
                    continue;

                BuildRegularFactionTeam(
                    factionResult.Blueprint,
                    Z,
                    Z.Level,
                    Z.NewTier
                );
            }

            return true;
        }

        public static bool BuildRegularFactionTeam(
            string faction,
            Zone Z,
            int zoneLevel,
            int zoneTier
        )
        {
            if (Z == null || faction.IsNullOrEmpty())
                return false;

            BallBag<string> candidates = new BallBag<string>();

            foreach (GameObjectBlueprint member
                in GameObjectFactory.Factory.GetFactionMembers(faction))
            {
                if (member == null)
                    continue;

                int memberLevel =
                    ZoneBuilderSandbox.GetLevelOfObject(member.Name);

                int difference = Math.Abs(zoneLevel - memberLevel);

                if (difference <= 10)
                {
                    candidates.Add(
                        member.Name,
                        Math.Max(5, 25 - difference)
                    );
                }
            }

            if (candidates.Count == 0)
                return false;

            string numberText;

            int partySize =
                !XRL.Data.TryGetText(
                    "FactionEncounterNumber_" + faction,
                    out numberText
                )
                ? XRL.Data.GetText(
                    "FactionEncounterNumber_*Default"
                ).RollCached()
                : numberText.RollCached();

            if (partySize < 1)
                partySize = 1;

            List<GameObject> party = Event.NewGameObjectList();
            GameObject leader = null;

            for (int i = 0; i < partySize; i++)
            {
                string blueprint = candidates.PeekOne();

                if (blueprint.IsNullOrEmpty())
                    continue;

                GameObject member = GameObject.Create(blueprint);

                if (member == null)
                    continue;

                party.Add(member);

                if (leader == null)
                    leader = member;
            }

            if (leader == null || party.Count == 0)
                return false;

            string memberInventory =
                ZoneBuilderSandbox.PopulationOr(
                    "FactionEncounterMemberInventory_" + faction,
                    "FactionEncounterMemberInventory_*Default"
                );

            string partyObjects =
                ZoneBuilderSandbox.PopulationOr(
                    "FactionEncounterPartyObjects_" + faction,
                    "FactionEncounterPartyObjects_*Default"
                );

            string zoneObjects =
                ZoneBuilderSandbox.PopulationOr(
                    "FactionEncounterZoneObjects_" + faction,
                    "FactionEncounterZoneObjects_*Default"
                );

            foreach (GameObject member in party)
            {
                if (member == null)
                    continue;

                if (member != leader)
                {
                    member.SetAlliedLeader<AllyRetinue>(leader);
                }

                member.EquipFromPopulationTable(
                    memberInventory,
                    zoneTier
                );
            }

            foreach (PopulationResult result
                in PopulationManager.Generate(
                    partyObjects,
                    "zonetier",
                    zoneTier.ToString()
                ))
            {
                if (result == null)
                    continue;

                for (int i = 0; i < result.Number; i++)
                {
                    GameObject obj =
                        GameObject.Create(result.Blueprint);

                    if (obj == null)
                        continue;

                    if (!string.IsNullOrEmpty(result.Hint))
                    {
                        obj.SetStringProperty(
                            "PlacementHint",
                            result.Hint
                        );
                    }

                    party.Add(obj);
                }
            }

            ZoneBuilderSandbox.PlaceParty(party, Z);

            foreach (PopulationResult result
                in PopulationManager.Generate(
                    zoneObjects,
                    "zonetier",
                    zoneTier.ToString()
                ))
            {
                if (result == null)
                    continue;

                for (int i = 0; i < result.Number; i++)
                {
                    GameObject obj =
                        GameObject.Create(result.Blueprint);

                    if (obj == null)
                        continue;

                    ZoneBuilderSandbox.PlaceObjectInArea(
                        Z,
                        (ILocationArea) Z.area,
                        obj,
                        Hints: result.Hint
                    );
                }
            }

            return true;
        }
    }
}

namespace XRL.World.ZoneBuilders
{
// Namespaced copy of the production custom population-placement builder.
    public class SubterraneanSiteDevMobs : ZoneBuilderSandbox
    {
        public int Rolls = 1;
        public int Tier = 1;
        public string Table = "";

        public bool BuildZone(Zone Z)
        {
            if (Tier < 1) Tier = 1;
            if (Tier > 8) Tier = 8;

            string table = Table;
            if (table.IsNullOrEmpty())
                table = "SubterraneanSiteDev_Tier" + Tier.ToString() + "_Mobs";

            List<Location2D> locations = new List<Location2D>();
            foreach (Cell cell in Z.GetCells())
            {
                if (cell.IsReachable() && cell.IsEmptyOfSolid() && !cell.HasSpawnBlocker())
                    locations.Add(cell.Location);
            }

            if (locations.Count == 0)
                return true;

            LocationList area = new LocationList(locations);

            for (int roll = 0; roll < Rolls; roll++)
            {
                List<GameObject> objects = PopulationManager.Expand(
                    PopulationManager.Generate(table, "zonetier", Tier.ToString())
                );

                if (objects == null)
                    continue;

                int placementIndex = 0;
                foreach (GameObject obj in objects)
                {
                    if (obj == null)
                        continue;

                    ZoneBuilderSandbox.PlaceObjectInArea(
                        Z,
                        area,
                        obj,
                        placementIndex,
                        0,
                        null,
                        null,
                        true
                    );

                    placementIndex++;
                }
            }

            return true;
        }
    }
}

namespace SubterraneanSiteDev
{
    /// <summary>
    /// Shared extradimensional-pocket vertical transition planning.
    ///
    /// This class decides where transitions are located and stores those
    /// coordinates as zone properties.
    ///
    /// Theme-specific code decides what placement parameters to use.
    /// Fire currently requests a 25-tile minimum separation.
    /// </summary>
    internal static class SubterraneanSiteDevEPVerticalTransitions
    {
        internal const string IncomingLandingProperty =
            "SubterraneanSiteDev_EPIncomingLanding";

        internal const string OutgoingHoleProperty =
            "SubterraneanSiteDev_EPOutgoingHole";

        //
        // Keep transition centers away from the absolute
        // zone edge, but otherwise use the available map.
        //
        // The hole itself has radius 3, so a 4-cell margin
        // keeps the complete pit footprint inside the zone.
        //
        private const int MinX = 4;
        private const int MaxX = 75;

        private const int MinY = 4;
        private const int MaxY = 20;

        public static bool Prepare(
            List<string> siteZoneIds,
            int minHoleSeparation
        )
        {
            if (
                siteZoneIds == null ||
                siteZoneIds.Count == 0
            )
            {
                return false;
            }

            if (minHoleSeparation < 0)
            {
                minHoleSeparation = 0;
            }

            int transitionCount =
                siteZoneIds.Count - 1;

            if (transitionCount <= 0)
            {
                return true;
            }

            int seed =
                XRLCore.Core.Game.GetWorldSeed(
                    "SubterraneanSiteDev:EPVertical:" +
                    siteZoneIds[0] +
                    ":MinSeparation:" +
                    minHoleSeparation.ToString()
                );

            System.Random rng =
                new System.Random(seed);

            List<Location2D> transitions =
                new List<Location2D>();

            Location2D previous = null;

            for (
                int i = 0;
                i < transitionCount;
                i++
            )
            {
                Location2D next =
                    PickTransition(
                        rng,
                        previous,
                        minHoleSeparation
                    );

                if (next == null)
                {
                    return false;
                }

                transitions.Add(next);
                previous = next;
            }

            //
            // Write the transition coordinates onto the site layers.
            //
            // Transition i:
            //   outgoing hole on layer i
            //   incoming landing on layer i + 1
            //
            for (
                int layerIndex = 0;
                layerIndex < siteZoneIds.Count;
                layerIndex++
            )
            {
                string zoneId =
                    siteZoneIds[layerIndex];

                string incoming = "";
                string outgoing = "";

                if (layerIndex > 0)
                {
                    incoming =
                        FormatCoordinate(
                            transitions[
                                layerIndex - 1
                            ]
                        );
                }

                if (
                    layerIndex <
                    siteZoneIds.Count - 1
                )
                {
                    outgoing =
                        FormatCoordinate(
                            transitions[
                                layerIndex
                            ]
                        );
                }

                The.ZoneManager.SetZoneProperty(
                    zoneId,
                    IncomingLandingProperty,
                    incoming
                );

                The.ZoneManager.SetZoneProperty(
                    zoneId,
                    OutgoingHoleProperty,
                    outgoing
                );
            }

            return true;
        }



        /// <summary>
        /// Register the actual pit builders.
        ///
        /// Call this AFTER the theme registrar so the pits are materialized
        /// after the theme finishes building/decorating the zone.
        /// </summary>
        public static void RegisterHoleBuilders(
            List<string> siteZoneIds
        )
        {
            if (siteZoneIds == null)
            {
                return;
            }

            //
            // Bottom layer has no outgoing hole.
            //
            for (
                int i = 0;
                i < siteZoneIds.Count - 1;
                i++
            )
            {
                string zoneId =
                    siteZoneIds[i];

                Location2D hole;

                if (
                    !TryGetCoordinate(
                        zoneId,
                        OutgoingHoleProperty,
                        out hole
                    )
                )
                {
                    continue;
                }

                int holeRadius =
                    SubterraneanSiteDevEPEntrance.GetHoleRadius(
                        zoneId,
                        3
                    );

                The.ZoneManager.AddZonePostBuilder(
                    zoneId,
                    "SubterraneanSiteDevEPHoleBuilder",
                    "CenterX", hole.X.ToString(),
                    "CenterY", hole.Y.ToString(),
                    "Radius", holeRadius.ToString(),
                    "HoleObject", "Pit"
                );
            }


            //
            // Reservations are build-time coordination only.
            //
            // Register cleanup on EVERY layer after all final hole builders have been
            // registered. On layers with an outgoing hole this runs after the destructive
            // hole pass. On the bottom layer there is no hole, so it runs after the
            // previously-registered exit teleporter.
            //
            foreach (
                string zoneId
                in siteZoneIds
            )
            {
                if (zoneId.IsNullOrEmpty())
                    continue;

                The.ZoneManager.AddZonePostBuilder(
                    zoneId,
                    "SubterraneanSiteDevEPReservationCleanup"
                );
            }
        }

        internal static List<Location2D> GetVerticalAnchors(
            string zoneId
        )
        {
            List<Location2D> result =
                new List<Location2D>();

            Location2D incoming;
            Location2D outgoing;

            if (
                TryGetCoordinate(
                    zoneId,
                    IncomingLandingProperty,
                    out incoming
                )
            )
            {
                result.Add(incoming);
            }

            if (
                TryGetCoordinate(
                    zoneId,
                    OutgoingHoleProperty,
                    out outgoing
                )
            )
            {
                if (
                    incoming == null ||
                    incoming.X != outgoing.X ||
                    incoming.Y != outgoing.Y
                )
                {
                    result.Add(outgoing);
                }
            }

            return result;
        }

        internal static bool TryGetCoordinate(
            string zoneId,
            string propertyName,
            out Location2D location
        )
        {
            location = null;

            if (
                zoneId == null ||
                zoneId == "" ||
                propertyName == null ||
                propertyName == ""
            )
            {
                return false;
            }

            string value =
                The.ZoneManager.GetZoneProperty(
                    zoneId,
                    propertyName
                ) as string;

            if (
                value == null ||
                value == ""
            )
            {
                return false;
            }

            string[] parts =
                value.Split(',');

            if (parts.Length != 2)
            {
                return false;
            }

            int x;
            int y;

            if (
                !int.TryParse(
                    parts[0],
                    out x
                ) ||
                !int.TryParse(
                    parts[1],
                    out y
                )
            )
            {
                return false;
            }

            location =
                Location2D.Get(x, y);

            return true;
        }

        private static Location2D PickTransition(
            System.Random rng,
            Location2D previous,
            int minHoleSeparation
        )
        {
            List<Location2D> preferred =
                new List<Location2D>();

            List<Location2D> farthest =
                new List<Location2D>();

            int farthestDistanceSquared = -1;

            int minimumSquared =
                minHoleSeparation *
                minHoleSeparation;

            for (
                int x = MinX;
                x <= MaxX;
                x++
            )
            {
                for (
                    int y = MinY;
                    y <= MaxY;
                    y++
                )
                {
                    Location2D candidate =
                        Location2D.Get(x, y);

                    //
                    // First transition has no previous
                    // transition to avoid.
                    //
                    if (previous == null)
                    {
                        preferred.Add(candidate);
                        continue;
                    }

                    int dx =
                        candidate.X -
                        previous.X;

                    int dy =
                        candidate.Y -
                        previous.Y;

                    int distanceSquared =
                        dx * dx +
                        dy * dy;

                    //
                    // Never stack exactly on the
                    // previous transition.
                    //
                    if (distanceSquared == 0)
                    {
                        continue;
                    }

                    if (
                        distanceSquared >=
                        minimumSquared
                    )
                    {
                        preferred.Add(candidate);
                    }

                    if (
                        distanceSquared >
                        farthestDistanceSquared
                    )
                    {
                        farthestDistanceSquared =
                            distanceSquared;

                        farthest.Clear();
                        farthest.Add(candidate);
                    }
                    else if (
                        distanceSquared ==
                        farthestDistanceSquared
                    )
                    {
                        farthest.Add(candidate);
                    }
                }
            }

            //
            // Normal Fire case.
            //
            if (preferred.Count > 0)
            {
                return preferred[
                    rng.Next(
                        preferred.Count
                    )
                ];
            }

            //
            // If the requested distance is impossible,
            // use the greatest available distance.
            //
            if (farthest.Count > 0)
            {
                return farthest[
                    rng.Next(
                        farthest.Count
                    )
                ];
            }

            return null;
        }

        private static string FormatCoordinate(
            Location2D location
        )
        {
            if (location == null)
            {
                return "";
            }

            return
                location.X.ToString() +
                "," +
                location.Y.ToString();
        }
    }
}


namespace XRL.World.Effects
{
    /// <summary>
    /// Shared base effect for temporary EP dimensional attunement.
    ///
    /// Owns common duration, resistance/save modifiers, temporary mutation
    /// lifecycle, and the player countdown. Theme-specific attunement effects
    /// extend this behavior through the shared hooks.
    /// </summary>
    [Serializable]
    public class SubterraneanSiteDevEPAttunementEffect : Effect
    {
        public string ThemeKey = "";
        public string ResistanceStat = "";
        public int ResistanceShift = 0;

        public string SaveStat = "";
        public string SaveVs = "";
        public int SaveShift = 0;

        public string MutationClass = "";
        public int MutationLevel = 0;

        // ID of the vanilla mutation modifier created specifically by this
        // attunement effect. It persists with the effect/save and lets Qud remove
        // exactly our contribution when the attunement ends.
        public Guid MutationModID = Guid.Empty;

        public SubterraneanSiteDevEPAttunementEffect()
        {
            DisplayName = "{{C|extradimensionally attuned}}";
        }

        public SubterraneanSiteDevEPAttunementEffect(
            int duration,
            string themeKey,
            string resistanceStat,
            int resistanceShift,
            string saveStat,
            string saveVs,
            int saveShift,
            string mutationClass,
            int mutationLevel
        ) : this()
        {
            Duration = duration;

            ThemeKey =
                themeKey ?? "";

            ResistanceStat =
                resistanceStat ?? "";

            ResistanceShift =
                resistanceShift;

            SaveStat =
                saveStat ?? "";

            SaveVs =
                saveVs ?? "";

            SaveShift =
                saveShift;

            MutationClass =
                mutationClass ?? "";

            MutationLevel =
                mutationLevel;
        }

        public override void Register(
            GameObject Object,
            IEventRegistrar Registrar
        )
        {
            Registrar.Register(
                "ModifyDefendingSave"
            );

            //
            // Shared attunement countdown.
            //
            // Every EP attunement inherits this effect, so the player receives
            // one consistent message-log warning regardless of theme.
            //
            Registrar.Register(
                "EndTurn"
            );

            //
            // Specialized attunement effects register their own events here.
            // The shared effect does not need to know what those events mean.
            //
            RegisterThemeEvents(
                Object,
                Registrar
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
                E.ID == "ModifyDefendingSave" &&
                SaveShift != 0
            )
            {
                string stat =
                    E.GetStringParameter(
                        "Stat"
                    );

                string vs =
                    E.GetStringParameter(
                        "Vs"
                    );

                if (
                    SaveStatMatches(stat) &&
                    SaveVsMatches(vs)
                )
                {
                    E.SetParameter(
                        "Roll",
                        E.GetIntParameter("Roll") +
                        SaveShift
                    );
                }
            }

            if (!FireThemeEvent(E))
            {
                return false;
            }

            //
            // Give the player a conspicuous message-log countdown every
            // 20 turns after attunement.
            //
            // Do not print the initial 300-turn value; the first countdown
            // message appears at 280 turns remaining.
            //
            if (
                E.ID == "EndTurn" &&
                base.Object != null &&
                base.Object.IsPlayer() &&
                Duration > 0 &&
                Duration <
                    SubterraneanSiteDev
                        .SubterraneanSiteDevEPAttunementSystem
                        .DefaultDuration &&
                Duration %
                    SubterraneanSiteDev
                        .SubterraneanSiteDevEPAttunementSystem
                        .CountdownInterval ==
                    0
            )
            {
                ShowAttunementCountdown();
            }

            return base.FireEvent(E);
        }

        private void ShowAttunementCountdown()
        {
            string[] flare =
            {
                ".",
                "...",
                ".....",
                "--...--",
                "---=---",
                "--===--",
                "-=====-",
                "==<*>==",
                "=<***>=",
                "<*****>"
            };

            foreach (string line in flare)
            {
                XRL.Messages.MessageQueue
                    .AddPlayerMessage(
                        "{{M|" +
                        line +
                        "}}"
                    );
            }

            XRL.Messages.MessageQueue
                .AddPlayerMessage(
                    "{{M|<<< ATTUNEMENT: " +
                    Duration.ToString() +
                    " TURNS REMAINING >>>}}"
                );

            for (
                int i = flare.Length - 1;
                i >= 0;
                i--
            )
            {
                XRL.Messages.MessageQueue
                    .AddPlayerMessage(
                        "{{M|" +
                        flare[i] +
                        "}}"
                    );
            }
        }

        private bool SaveStatMatches(
            string eventStat
        )
        {
            if (SaveStat.IsNullOrEmpty())
                return true;

            if (eventStat.IsNullOrEmpty())
                return false;

            string[] stats =
                eventStat.Split(',');

            foreach (string stat in stats)
            {
                if (
                    string.Equals(
                        stat.Trim(),
                        SaveStat,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    return true;
                }
            }

            return false;
        }

        private bool SaveVsMatches(
            string eventVs
        )
        {
            if (SaveVs.IsNullOrEmpty())
                return true;

            if (eventVs.IsNullOrEmpty())
                return false;

            return
                eventVs.IndexOf(
                    SaveVs,
                    StringComparison.OrdinalIgnoreCase
                ) >= 0;
        }

        public override bool UseStandardDurationCountdown()
        {
            return true;
        }

        public override string GetDetails()
        {
            List<string> lines =
                new List<string>();

            if (
                !ResistanceStat.IsNullOrEmpty() &&
                ResistanceShift != 0
            )
            {
                string readable =
                    ResistanceStat;

                if (ResistanceStat == "HeatResistance")
                    readable = "Heat Resistance";
                else if (ResistanceStat == "ColdResistance")
                    readable = "Cold Resistance";
                else if (ResistanceStat == "ElectricResistance")
                    readable = "Electric Resistance";

                string sign =
                    ResistanceShift > 0
                        ? "+"
                        : "";

                lines.Add(
                    sign +
                    ResistanceShift.ToString() +
                    " " +
                    readable
                );
            }

            if (SaveShift != 0)
            {
                string sign =
                    SaveShift > 0
                        ? "+"
                        : "";

                string line =
                    sign +
                    SaveShift.ToString() +
                    " " +
                    (
                        SaveStat.IsNullOrEmpty()
                            ? ""
                            : SaveStat + " "
                    ) +
                    "saves";

                if (!SaveVs.IsNullOrEmpty())
                {
                    line +=
                        " vs. " +
                        SaveVs.ToLowerInvariant();
                }

                lines.Add(line);
            }

            AddThemeDetails(
                lines
            );

            lines.Add(
                "Attuned to: " +
                ThemeKey
            );

            return string.Join(
                "\n",
                lines.ToArray()
            );
        }

        public override bool Apply(
            GameObject Object
        )
        {
            if (Object == null)
                return false;

            //
            // Validate common requirements before allowing a specialized theme
            // to modify the actor.
            //
            if (
                !ResistanceStat.IsNullOrEmpty() &&
                ResistanceShift != 0 &&
                !Object.HasStat(ResistanceStat)
            )
            {
                return false;
            }

            //
            // Theme-specific setup happens through the effect subclass.
            //
            // Fungus, for example, will install and mark its temporary infection
            // here. Most themes simply inherit the default true result.
            //
            if (!ApplyTheme(Object))
            {
                //
                // Require specialized implementations to be removable even after
                // a partial failure. Since RemoveTheme must remove only content
                // owned by this effect, calling it defensively is safe.
                //
                RemoveTheme(
                    Object
                );

                return false;
            }

            //
            // Common resistance package.
            //
            if (
                !ResistanceStat.IsNullOrEmpty() &&
                ResistanceShift != 0
            )
            {
                StatShifter.SetStatShift(
                    ResistanceStat,
                    ResistanceShift
                );
            }

            //
            // Common temporary signature-mutation package.
            //
            if (!ApplyTemporaryMutation(Object))
            {
                //
                // Roll back everything already applied by this attunement.
                //
                RemoveTemporaryMutation(
                    Object
                );

                StatShifter.RemoveStatShifts();

                RemoveTheme(
                    Object
                );

                return false;
            }

            return true;
        }

        public override void Remove(
            GameObject Object
        )
        {
            //
            // Every specialized attunement gets one guaranteed removal stage.
            //
            // Standard duration expiry, switching attunements, explicit removal,
            // etc. all eventually arrive here.
            //
            RemoveTheme(
                Object
            );

            RemoveTemporaryMutation(
                Object
            );

            StatShifter.RemoveStatShifts();
        }

        /// <summary>
        /// Register events required only by a specialized attunement.
        ///
        /// Default: none.
        /// </summary>
        protected virtual void RegisterThemeEvents(
            GameObject Object,
            IEventRegistrar Registrar
        )
        {
        }

        /// <summary>
        /// Handle an event owned by a specialized attunement.
        ///
        /// Returning false propagates the normal Qud event cancellation behavior.
        ///
        /// Default: do nothing.
        /// </summary>
        protected virtual bool FireThemeEvent(
            Event E
        )
        {
            return true;
        }

        /// <summary>
        /// Apply theme-specific state.
        ///
        /// This is called before the shared resistance/mutation package is applied.
        /// Any object or effect created here must be identifiable as owned by this
        /// attunement so RemoveTheme() can safely undo it.
        ///
        /// Default: success with no action.
        /// </summary>
        protected virtual bool ApplyTheme(
            GameObject Object
        )
        {
            return true;
        }

        /// <summary>
        /// Remove everything temporary that belongs specifically to this theme's
        /// attunement.
        ///
        /// This may be called defensively after a failed ApplyTheme(), so
        /// implementations must remove only objects/state explicitly owned by this
        /// attunement.
        ///
        /// Default: no action.
        /// </summary>
        protected virtual void RemoveTheme(
            GameObject Object
        )
        {
        }

        /// <summary>
        /// Add theme-specific lines to the attunement status description.
        ///
        /// Default: none.
        /// </summary>
        protected virtual void AddThemeDetails(
            List<string> lines
        )
        {
        }

        /// <summary>
        /// Determines whether touching another stone for the same Category-1 theme
        /// may simply refresh this effect instead of constructing a replacement.
        ///
        /// Specialized effects can override this when refresh depends on additional
        /// owned state. Fungus will use this to confirm that its temporary infection
        /// still exists.
        ///
        /// The generic tier-scaled mutation must match the current layer.
        /// </summary>
        public virtual bool CanRefresh(
            GameObject Object,
            Zone Z
        )
        {
            if (
                Object == null ||
                Z == null
            )
            {
                return false;
            }

            string currentTheme =
                SubterraneanSiteDev
                    .SubterraneanSiteDevEPAttunementSystem
                    .GetCategory1Theme(
                        Z
                    );

            if (
                !string.Equals(
                    ThemeKey,
                    currentTheme,
                    StringComparison.Ordinal
                )
            )
            {
                return false;
            }

            int expectedMutationLevel =
                SubterraneanSiteDev
                    .SubterraneanSiteDevEPAttunementSystem
                    .GetAttunementMutationLevel(
                        Z
                    );

            return
                MutationLevel ==
                expectedMutationLevel;
        }

        protected void RemoveTemporaryMutation(
            GameObject Object
        )
        {
            if (
                Object == null ||
                MutationModID == Guid.Empty
            )
            {
                return;
            }

            Mutations mutations =
                Object.GetPart<Mutations>();

            if (mutations != null)
            {
                mutations.RemoveMutationMod(
                    MutationModID
                );
            }

            MutationModID =
                Guid.Empty;
        }

       


        protected bool ApplyTemporaryMutation(
            GameObject Object
        )
        {
            if (
                Object == null ||
                MutationClass.IsNullOrEmpty() ||
                MutationLevel <= 0
            )
            {
                return true;
            }

            Mutations mutations =
                Object.RequirePart<Mutations>();

            XRL.World.Parts.Mutation.BaseMutation existing =
                Object.GetPart(MutationClass)
                    as XRL.World.Parts.Mutation.BaseMutation;

            int currentLevel =
                existing == null
                    ? 0
                    : existing.Level;

            // Attunement never weakens an existing mutation.
            int bonus =
                MutationLevel - currentLevel;

            if (bonus <= 0)
                return true;

            //
            // This deliberately reproduces the useful core of
            // Mutations.AddMutationMod(), but WITHOUT its CompatibleWith()
            // gate. EP attunement is explicitly allowed to grant its
            // extradimensional mutation to Mutants, True Kin, and robots.
            //
            Mutations.MutationModifierTracker tracker =
                new Mutations.MutationModifierTracker
                {
                    id = Guid.NewGuid(),
                    mutationName = MutationClass,
                    sourceType =
                        Mutations.MutationModifierTracker
                            .SourceType.External,
                    sourceName =
                        "Subterranean Sites EP attunement",
                    bonus = bonus
                };

            mutations.MutationMods.Add(
                tracker
            );

            MutationModID =
                tracker.id;

            //
            // If the actor did not already possess the mutation, create
            // the mutation part the same way vanilla AddMutationMod does.
            //
            if (existing == null)
            {
                XRL.World.Parts.Mutation.BaseMutation mutation =
                    XRL.World.Parts.Mutation.BaseMutation.Create(
                        MutationClass
                    );

                if (mutation == null)
                {
                    mutations.MutationMods.Remove(
                        tracker
                    );

                    MutationModID =
                        Guid.Empty;

                    return false;
                }

                mutation.ParentObject =
                    Object;

                if (!mutation.Mutate(Object, 0))
                {
                    mutations.MutationMods.Remove(
                        tracker
                    );

                    MutationModID =
                        Guid.Empty;

                    return false;
                }

                Object.AddPart<
                    XRL.World.Parts.Mutation.BaseMutation
                >(
                    mutation
                );

                mutation.AfterMutate();
            }

            Object.SyncMutationLevelAndGlimmer();

            return true;
        }
    }

}

namespace XRL.World.Conversations.Parts
{
    /// <summary>
    /// Shared EP attunement conversation action.
    ///
    /// Chavvah's attunement architecture is preserved: the object only hosts a
    /// ConversationScript; selecting the attune choice invokes this conversation
    /// part. The listener is the creature doing the interacting (normally the
    /// player). Re-attuning refreshes the duration. Attuning to a different
    /// Category-1 theme replaces the old EP attunement cleanly.
    /// </summary>
    public class SubterraneanSiteDevEPAttune : IConversationPart
    {
        public string FailTarget = "Unavailable";

        public string CancelTarget = "End";

        public override bool WantEvent(int ID, int Propagation)
        {
            return
                base.WantEvent(ID, Propagation) ||
                ID == GetTargetElementEvent.ID;
        }

        public override bool HandleEvent(
            GetTargetElementEvent E
        )
        {
            GameObject listener =
                The.Listener ??
                The.Player;


            if (
                listener == null ||
                listener.CurrentZone == null
            )
            {
                E.Target =
                    FailTarget;

                return base.HandleEvent(E);
            }


            string themeKey =
                SubterraneanSiteDev
                    .SubterraneanSiteDevEPAttunementSystem
                    .GetCategory1Theme(
                        listener.CurrentZone
                    );


            SubterraneanSiteDev
                .ISubterraneanSiteDevEPAttunementProvider provider;


            if (
                !SubterraneanSiteDev
                    .SubterraneanSiteDevEPAttunementSystem
                    .TryGetAttunementProvider(
                        listener.CurrentZone,
                        out provider
                    )
            )
            {
                E.Target =
                    FailTarget;

                return base.HandleEvent(E);
            }


            HandleProviderAttunement(
                listener,
                listener.CurrentZone,
                themeKey,
                provider,
                E
            );


            return base.HandleEvent(E);
        }

        private void HandleProviderAttunement(
            GameObject listener,
            Zone zone,
            string themeKey,
            SubterraneanSiteDev
                .ISubterraneanSiteDevEPAttunementProvider provider,
            GetTargetElementEvent E
        )
        {
            if (
                listener == null ||
                zone == null ||
                provider == null ||
                themeKey.IsNullOrEmpty()
            )
            {
                E.Target =
                    FailTarget;

                return;
            }

            XRL.World.Effects
                .SubterraneanSiteDevEPAttunementEffect existing =
                    SubterraneanSiteDev
                        .SubterraneanSiteDevEPAttunementSystem
                        .GetCurrentAttunement(
                            listener
                        );

            // Specialized effects may require additional owned state to permit
            // a simple refresh. Fungus, for example, verifies that its temporary
            // infection still exists before allowing the shared duration refresh.
            if (
                existing != null &&
                string.Equals(
                    existing.ThemeKey,
                    themeKey,
                    StringComparison.Ordinal
                ) &&
                existing.CanRefresh(
                    listener,
                    zone
                )
            )
            {
                existing.Duration =
                    SubterraneanSiteDev
                        .SubterraneanSiteDevEPAttunementSystem
                        .DefaultDuration;

                return;
            }

            XRL.World.Effects
                .SubterraneanSiteDevEPAttunementEffect candidate;

            string successMessage;

            SubterraneanSiteDev
                .SubterraneanSiteDevEPAttunementBuildResult result =
                    provider.TryCreateAttunement(
                        listener,
                        zone,
                        out candidate,
                        out successMessage
                    );

            //
            // A deliberate player cancellation is not an attunement failure.
            //
            if (
                result ==
                SubterraneanSiteDev
                    .SubterraneanSiteDevEPAttunementBuildResult
                    .Cancelled
            )
            {
                E.Target =
                    CancelTarget;

                return;
            }

            if (
                result !=
                    SubterraneanSiteDev
                        .SubterraneanSiteDevEPAttunementBuildResult
                        .Success ||
                candidate == null
            )
            {
                E.Target =
                    FailTarget;

                return;
            }

            //
            // A provider may only construct the attunement belonging to the actual
            // Category-1 winner. Treat a mismatch as a programming/configuration error
            // rather than allowing two theme identities to become mixed.
            //
            if (
                !string.Equals(
                    candidate.ThemeKey,
                    themeKey,
                    StringComparison.Ordinal
                )
            )
            {
                E.Target =
                    FailTarget;

                return;
            }

            //
            // Duration remains shared policy.
            //
            // Individual themes are not allowed to accidentally invent permanent
            // attunements by forgetting to set their duration.
            //
            candidate.Duration =
                SubterraneanSiteDev
                    .SubterraneanSiteDevEPAttunementSystem
                    .DefaultDuration;

            //
            // Do not remove the old attunement until the new provider has successfully
            // completed any player interaction required to construct its candidate.
            //
            // Therefore cancelling Fungus limb selection leaves the player's previous
            // attunement completely untouched.
            //
            if (existing != null)
            {
                listener.RemoveEffect(
                    existing
                );
            }

            //
            // GameObject.ApplyEffect returns whether Effect.Apply() actually succeeded.
            //
            if (!listener.ApplyEffect(candidate))
            {
                E.Target =
                    FailTarget;

                return;
            }

            //
            // Optional theme-owned explanatory text.
            //
            // Most themes can return an empty string. Fungus will use this to make the
            // temporary nature of its infection unmistakable.
            //
            if (!successMessage.IsNullOrEmpty())
            {
                XRL.UI.Popup.Show(
                    successMessage
                );
            }
        }



    }
}

namespace XRL.World.ZoneBuilders
{
    /// <summary>
    /// Shared two-cell EP attunement-stone placement.
    ///
    /// Entrance: place the pair near, but not inside, the oversized entrance
    /// hole and keep it inside the dimensional scar.
    ///
    /// Underground: choose a deterministic random reachable/open pair away from
    /// both vertical transition anchors. The stone pair respects existing semantic
    /// ownership, may coexist with permissive spills, and claims its two cells
    /// against later discrete EP content.
    /// </summary>
    public class SubterraneanSiteDevEPAttunementStoneBuilder :
        ZoneBuilderSandbox
    {
        public int EntranceMode = 0;
        public int TransitionExclusionRadius = 6;
        public int EntranceMinHolePadding = 2;
        public int EntranceMaxHolePadding = 6;

        public bool BuildZone(Zone Z)
        {
            if (Z == null)
                return true;

            List<Cell> candidates = new List<Cell>();

            foreach (Cell left in Z.GetCells())
            {
                if (left == null)
                    continue;

                Cell right = left.GetCellFromDirection("E");

                if (right == null)
                    continue;

                if (
                    EntranceMode != 0
                        ? IsValidEntrancePair(Z, left, right)
                        : IsValidUndergroundPair(Z, left, right)
                )
                {
                    candidates.Add(left);
                }
            }

            if (candidates.Count == 0)
                return true;

            int seed =
                XRLCore.Core.Game.GetWorldSeed(
                    "SubterraneanSiteDev:EPAttunementStone:" +
                    Z.ZoneID + ":Entrance:" + EntranceMode.ToString()
                );

            System.Random rng = new System.Random(seed);

            Cell chosenLeft =
                candidates[rng.Next(candidates.Count)];

            Cell chosenRight =
                chosenLeft.GetCellFromDirection("E");

            if (chosenRight == null)
                return true;

            GameObject leftStone =
                GameObjectFactory.Factory.CreateObject(
                    SubterraneanSiteDev
                        .SubterraneanSiteDevEPAttunementSystem
                        .StoneLeftBlueprint
                );

            GameObject rightStone =
                GameObjectFactory.Factory.CreateObject(
                    SubterraneanSiteDev
                        .SubterraneanSiteDevEPAttunementSystem
                        .StoneRightBlueprint
                );

            if (leftStone != null)
            {
                chosenLeft.AddObject(
                    leftStone
                );

                SubterraneanSiteDev
                    .SubterraneanSiteDevEPReservations
                    .ClaimCell(
                        Z,
                        chosenLeft
                    );
            }

            if (rightStone != null)
            {
                chosenRight.AddObject(
                    rightStone
                );

                SubterraneanSiteDev
                    .SubterraneanSiteDevEPReservations
                    .ClaimCell(
                        Z,
                        chosenRight
                    );
            }

            return true;
        }

        private bool IsValidEntrancePair(
            Zone Z,
            Cell left,
            Cell right
        )
        {
            Location2D center;
            int scarRadius;
            int holeRadius;

            if (
                !SubterraneanSiteDev
                    .SubterraneanSiteDevEPEntrance
                    .TryGetSpec(
                        Z,
                        out center,
                        out scarRadius,
                        out holeRadius
                    )
            )
            {
                return false;
            }

            if (
                !BasicCellAvailable(
                    Z,
                    left
                ) ||
                !BasicCellAvailable(
                    Z,
                    right
                )
            )
            {
                return false;
            }

            int minRadius =
                holeRadius + Math.Max(1, EntranceMinHolePadding);

            int maxRadius =
                holeRadius + Math.Max(
                    EntranceMinHolePadding,
                    EntranceMaxHolePadding
                );

            return
                IsInEntranceAnnulus(left, center, minRadius, maxRadius, scarRadius) &&
                IsInEntranceAnnulus(right, center, minRadius, maxRadius, scarRadius);
        }

        private bool IsInEntranceAnnulus(
            Cell cell,
            Location2D center,
            int minRadius,
            int maxRadius,
            int scarRadius
        )
        {
            if (cell == null || center == null)
                return false;

            int dx = cell.X - center.X;
            int dy = cell.Y - center.Y;
            int distanceSquared = dx * dx + dy * dy;

            return
                distanceSquared >= minRadius * minRadius &&
                distanceSquared <= maxRadius * maxRadius &&
                distanceSquared <= scarRadius * scarRadius;
        }

        private bool IsValidUndergroundPair(
            Zone Z,
            Cell left,
            Cell right
        )
        {
            if (
                !BasicCellAvailable(
                    Z,
                    left
                ) ||
                !BasicCellAvailable(
                    Z,
                    right
                )
            )
            {
                return false;
            }

            if (
                !SubterraneanSiteDev
                    .SubterraneanSiteDevEPGeometry
                    .IsOpenGeometryCell(left) ||
                !SubterraneanSiteDev
                    .SubterraneanSiteDevEPGeometry
                    .IsOpenGeometryCell(right)
            )
            {
                return false;
            }

            if (!left.IsReachable() || !right.IsReachable())
                return false;

            List<Location2D> anchors =
                SubterraneanSiteDev
                    .SubterraneanSiteDevEPVerticalTransitions
                    .GetVerticalAnchors(Z.ZoneID);

            if (
                SubterraneanSiteDev
                    .SubterraneanSiteDevEPPlacement
                    .IsNearAnyAnchor(
                        left.X,
                        left.Y,
                        anchors,
                        TransitionExclusionRadius
                    ) ||
                SubterraneanSiteDev
                    .SubterraneanSiteDevEPPlacement
                    .IsNearAnyAnchor(
                        right.X,
                        right.Y,
                        anchors,
                        TransitionExclusionRadius
                    )
            )
            {
                return false;
            }

            return true;
        }

        private bool BasicCellAvailable(
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
            // The attunement stone is shared discrete EP content.
            //
            // Respect everything already claimed by the theme/category pipeline:
            // C1 objects, C2 functional footprints, C5 objects, and object-like pools.
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
                return false;
            }

            if (!cell.IsEmptyOfSolid())
                return false;

            //
            // No generic liquid rejection.
            //
            // The stone may stand directly in a permissive Ooze/Fire spill.
            //
            return true;
        }
    }
}

namespace XRL.World.ZoneBuilders
{
    /// <summary>
    /// Shared Category-2 EP boundary-hazard placer.
    ///
    /// Category 4 creates abstract geometry and the shared classifier marks
    /// solid cells exposed to the interior as semantic boundary placeholders.
    /// This builder replaces selected boundary cells with theme-supplied
    /// hazards before Category 3 materializes the remaining walls.
    ///
    /// Two modes are currently supported:
    ///
    /// Nondirectional:
    ///   - one hazard blueprint
    ///   - clean wall faces with exactly one open cardinal side
    ///   - count derived from available boundary length
    ///   - Manhattan spacing
    ///
    /// Directional:
    ///   - explicit N/S/E/W blueprints
    ///   - may use boundary cells facing multiple open sides
    ///   - one valid facing is chosen randomly
    ///   - random MinMounts..MaxMounts count
    ///   - Chebyshev spacing
    ///
    /// Fire and Electrical currently use nondirectional mode.
    /// Cold currently uses directional mode.
    /// </summary>
    public class SubterraneanSiteDevEPBoundaryHazardBuilder :
        ZoneBuilderSandbox
    {
        public int OuterBorderExclusion = 0;
        //
        // Nondirectional hazard.
        //
        public string HazardBlueprint = "";

        //
        // Directional hazard set, packed as:
        //
        // north|south|east|west
        //
        // Keeping all four actual blueprint names explicit avoids imposing a
        // naming convention on future themes.
        //
        public string DirectionalBlueprints = "";

        //
        // Nondirectional density model.
        //
        public int BoundaryCellsPerMount = 50;

        public int MinMounts = 2;
        public int MaxMounts = 6;

        public int MinSpacing = 8;

        //
        // Directional hazards such as Cold cryovents can reserve a larger
        // footprint around incoming/outgoing vertical transitions.
        //
        public int AnchorExclusionRadius = 0;

        //
        // Walltrap timing. Cold historically uses 4-6; Fire/Electrical retain
        // the shared default 4-8 unless their theme explicitly overrides it.
        //
        public int MinTurnInterval = 4;
        public int MaxTurnInterval = 8;

        private const string StairsUpBlueprint =
            "StairsUp";

        private const string StairsDownBlueprint =
            "StairsDown";


        private class FacingChoice
        {
            public Cell InteriorCell;
            public string Blueprint;
        }


        private class MountCandidate
        {
            public int X;
            public int Y;

            //
            // First open cell directly in front of the mounted hazard.
            // This is part of the hazard's minimal functional footprint.
            //
            public int InteriorX;
            public int InteriorY;

            public string Blueprint;
        }


        public bool BuildZone(Zone Z)
        {
            if (Z == null)
                return true;

            string northBlueprint;
            string southBlueprint;
            string eastBlueprint;
            string westBlueprint;

            bool directional =
                TryGetDirectionalBlueprints(
                    out northBlueprint,
                    out southBlueprint,
                    out eastBlueprint,
                    out westBlueprint
                );

            if (
                !directional &&
                HazardBlueprint.IsNullOrEmpty()
            )
            {
                return true;
            }

            ClampSettings();

            List<Location2D> anchors =
                SubterraneanSiteDev
                    .SubterraneanSiteDevEPVerticalTransitions
                    .GetVerticalAnchors(
                        Z.ZoneID
                    );

            List<MountCandidate> candidates =
                FindCandidates(
                    Z,
                    anchors,
                    directional,
                    northBlueprint,
                    southBlueprint,
                    eastBlueprint,
                    westBlueprint
                );

            if (candidates.Count == 0)
                return true;

            int desired;

            if (directional)
            {
                //
                // Preserve Cold's original behavior: choose a random absolute
                // number of vents rather than deriving density from boundary
                // length.
                //
                desired =
                    Stat.Random(
                        MinMounts,
                        MaxMounts
                    );
            }
            else
            {
                desired =
                    candidates.Count /
                    BoundaryCellsPerMount;

                if (desired < MinMounts)
                    desired = MinMounts;

                if (desired > MaxMounts)
                    desired = MaxMounts;
            }

            if (desired > candidates.Count)
                desired = candidates.Count;

            List<MountCandidate> selected =
                SelectSeparatedCandidates(
                    candidates,
                    desired,
                    directional
                );

            foreach (
                MountCandidate candidate
                in selected
            )
            {
                PlaceMount(
                    Z,
                    candidate,
                    directional
                );
            }

            return true;
        }


        private bool TryGetDirectionalBlueprints(
            out string northBlueprint,
            out string southBlueprint,
            out string eastBlueprint,
            out string westBlueprint
        )
        {
            northBlueprint = null;
            southBlueprint = null;
            eastBlueprint = null;
            westBlueprint = null;

            if (DirectionalBlueprints.IsNullOrEmpty())
                return false;

            string[] parts =
                DirectionalBlueprints.Split('|');

            if (parts.Length != 4)
                return false;

            if (
                parts[0].IsNullOrEmpty() ||
                parts[1].IsNullOrEmpty() ||
                parts[2].IsNullOrEmpty() ||
                parts[3].IsNullOrEmpty()
            )
            {
                return false;
            }

            northBlueprint = parts[0];
            southBlueprint = parts[1];
            eastBlueprint = parts[2];
            westBlueprint = parts[3];

            return true;
        }


        private void ClampSettings()
        {
            if (BoundaryCellsPerMount < 1)
                BoundaryCellsPerMount = 1;

            if (MinMounts < 0)
                MinMounts = 0;

            if (MaxMounts < MinMounts)
                MaxMounts = MinMounts;

            if (MinSpacing < 0)
                MinSpacing = 0;

            if (AnchorExclusionRadius < 0)
                AnchorExclusionRadius = 0;

            if (MinTurnInterval < 1)
                MinTurnInterval = 1;

            if (MaxTurnInterval < MinTurnInterval)
                MaxTurnInterval = MinTurnInterval;
        }


        private List<MountCandidate> FindCandidates(
            Zone Z,
            List<Location2D> anchors,
            bool directional,
            string northBlueprint,
            string southBlueprint,
            string eastBlueprint,
            string westBlueprint
        )
        {
            List<MountCandidate> result =
                new List<MountCandidate>();

            foreach (
                Cell cell
                in Z.GetCells()
            )
            {
                if (cell == null)
                    continue;

                //
                // Some C2 themes may use exposed interior boundary cells but should
                // never replace the sealed outer shell.
                //
                if (
                    OuterBorderExclusion > 0 &&
                    (
                        cell.X < OuterBorderExclusion ||
                        cell.Y < OuterBorderExclusion ||
                        cell.X >= Z.Width - OuterBorderExclusion ||
                        cell.Y >= Z.Height - OuterBorderExclusion
                    )
                )
                {
                    continue;
                }

                if (
                    !SubterraneanSiteDev
                        .SubterraneanSiteDevEPGeometry
                        .IsBoundaryPlaceholder(
                            cell
                        )
                )
                {
                    continue;
                }

                //
                // The mounted hazard itself is discrete Category-2 content.
                // Never replace a boundary cell already owned by earlier EP content.
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
                    AnchorExclusionRadius > 0 &&
                    SubterraneanSiteDev
                        .SubterraneanSiteDevEPPlacement
                        .IsNearAnyAnchor(
                            cell.X,
                            cell.Y,
                            anchors,
                            AnchorExclusionRadius
                        )
                )
                {
                    continue;
                }

                List<FacingChoice> choices =
                    new List<FacingChoice>();

                AddOpenFacing(
                    Z,
                    cell.X,
                    cell.Y - 1,
                    directional
                        ? northBlueprint
                        : HazardBlueprint,
                    choices
                );

                AddOpenFacing(
                    Z,
                    cell.X,
                    cell.Y + 1,
                    directional
                        ? southBlueprint
                        : HazardBlueprint,
                    choices
                );

                AddOpenFacing(
                    Z,
                    cell.X + 1,
                    cell.Y,
                    directional
                        ? eastBlueprint
                        : HazardBlueprint,
                    choices
                );

                AddOpenFacing(
                    Z,
                    cell.X - 1,
                    cell.Y,
                    directional
                        ? westBlueprint
                        : HazardBlueprint,
                    choices
                );

                if (choices.Count == 0)
                    continue;

                //
                // Fire/Electrical preserve their clean-wall requirement.
                // Cold preserves its old behavior of allowing corners and
                // choosing one valid direction randomly.
                //
                if (
                    !directional &&
                    choices.Count != 1
                )
                {
                    continue;
                }

                FacingChoice chosen =
                    choices[
                        directional
                            ? Stat.Random(
                                0,
                                choices.Count - 1
                            )
                            : 0
                    ];

                if (
                    chosen == null ||
                    chosen.InteriorCell == null
                )
                {
                    continue;
                }

                //
                // Preserve the original Fire/Electrical immediate stair
                // protection. Cold uses the stronger anchor-radius exclusion.
                //
                if (
                    !directional &&
                    (
                        chosen.InteriorCell
                            .HasObjectWithBlueprint(
                                StairsUpBlueprint
                            ) ||
                        chosen.InteriorCell
                            .HasObjectWithBlueprint(
                                StairsDownBlueprint
                            )
                    )
                )
                {
                    continue;
                }

                result.Add(
                    new MountCandidate
                    {
                        X = cell.X,
                        Y = cell.Y,
                        InteriorX = chosen.InteriorCell.X,
                        InteriorY = chosen.InteriorCell.Y,
                        Blueprint = chosen.Blueprint
                    }
                );
            }

            return result;
        }


        private void AddOpenFacing(
            Zone Z,
            int x,
            int y,
            string blueprint,
            List<FacingChoice> choices
        )
        {
            if (
                Z == null ||
                choices == null ||
                blueprint.IsNullOrEmpty()
            )
            {
                return;
            }

            Cell target =
                Z.GetCell(
                    x,
                    y
                );

            if (target == null)
                return;

            if (
                !SubterraneanSiteDev
                    .SubterraneanSiteDevEPGeometry
                    .IsOpenGeometryCell(
                        target
                    )
            )
            {
                return;
            }

            if (target.IsSolid())
                return;

            //
            // The first cell in front of the hazard must remain available.
            //
            // This is deliberately NOT a liquid test. A cryovent, fire vent, or
            // electrical walltrap may face directly into Ooze/Fire liquid or any other
            // compatible liquid environment.
            //
            if (
                SubterraneanSiteDev
                    .SubterraneanSiteDevEPReservations
                    .IsClaimed(
                        Z,
                        target
                    )
            )
            {
                return;
            }

            choices.Add(
                new FacingChoice
                {
                    InteriorCell = target,
                    Blueprint = blueprint
                }
            );
        }


        private List<MountCandidate>
            SelectSeparatedCandidates(
                List<MountCandidate> candidates,
                int desired,
                bool directional
            )
        {
            List<MountCandidate> pool =
                new List<MountCandidate>(
                    candidates
                );

            List<MountCandidate> selected =
                new List<MountCandidate>();

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

                MountCandidate candidate =
                    pool[index];

                pool.RemoveAt(index);

                if (
                    IsFarEnough(
                        candidate,
                        selected,
                        directional
                    )
                )
                {
                    selected.Add(candidate);
                }
            }

            return selected;
        }


        private bool IsFarEnough(
            MountCandidate candidate,
            List<MountCandidate> selected,
            bool directional
        )
        {
            foreach (
                MountCandidate other
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

                int distance;

                if (directional)
                {
                    //
                    // Preserve Cold's Chebyshev spacing. With MinSpacing = 2,
                    // vents cannot be orthogonally or diagonally adjacent.
                    //
                    distance =
                        Math.Max(
                            dx,
                            dy
                        );
                }
                else
                {
                    //
                    // Preserve Fire/Electrical Manhattan spacing.
                    //
                    distance =
                        dx + dy;
                }

                if (distance < MinSpacing)
                    return false;
            }

            return true;
        }


        private void PlaceMount(
            Zone Z,
            MountCandidate candidate,
            bool directional
        )
        {
            Cell hazardCell =
                Z.GetCell(
                    candidate.X,
                    candidate.Y
                );

            if (hazardCell == null)
                return;

            //
            // Construct the replacement first. If blueprint construction fails,
            // leave the original boundary untouched.
            //
            GameObject hazard =
                GameObjectFactory.Factory.CreateObject(
                    candidate.Blueprint
                );

            if (hazard == null)
                return;

            XRL.World.Parts.Walltrap walltrap =
                hazard.GetPart<
                    XRL.World.Parts.Walltrap
                >();

            if (walltrap != null)
            {
                walltrap.TurnInterval =
                    Stat.Random(
                        MinTurnInterval,
                        MaxTurnInterval
                    );

                if (walltrap.TurnInterval <= 1)
                {
                    walltrap.CurrentTurn = 0;
                }
                else if (directional)
                {
                    //
                    // Preserve Cold's original full phase range.
                    //
                    walltrap.CurrentTurn =
                        Stat.Random(
                            0,
                            walltrap.TurnInterval - 1
                        );
                }
                else
                {
                    //
                    // Preserve the previous Fire/Electrical timing behavior.
                    //
                    walltrap.CurrentTurn =
                        Stat.Random(
                            0,
                            walltrap.TurnInterval - 2
                        );
                }
            }

            //
            // This is a wall replacement, not a whole-cell replacement.
            //
            // Cold already used ClearWalls(). It is also the safer shared
            // semantic operation: don't destroy unrelated non-wall objects
            // merely because the hazard occupies the wall layer.
            //
            hazardCell.ClearWalls();
            hazardCell.AddObject(
                hazard
            );

            //
            // Category 2 now owns the actual wall mount.
            //
            SubterraneanSiteDev
                .SubterraneanSiteDevEPReservations
                .ClaimCell(
                    Z,
                    hazardCell
                );

            //
            // Preserve one immediately-open cell in front of the hazard.
            //
            // We deliberately do not reserve the entire jet/plume. Other systems are
            // still free to interact with the hazard farther out, while later discrete
            // decoration cannot completely plug the hazard at its mouth.
            //
            SubterraneanSiteDev
                .SubterraneanSiteDevEPReservations
                .ClaimCell(
                    Z,
                    candidate.InteriorX,
                    candidate.InteriorY
                );
        }
    }
}


namespace XRL.World.Parts
{

    
    /// <summary>
    /// EP bottom-layer return teleporter.
    ///
    /// Uses the same step-on behavior as vanilla CatacombsExitTeleporter,
    /// but returns specifically to this EP's origin zone.
    ///
    /// Deliberately player-only so EP denizens cannot escape through it.
    /// Combat does not prevent use.
    /// </summary>
    [Serializable]
    public class SubterraneanSiteDevEPExitTeleporter : IPoweredPart
    {
        public string TargetZone = "";

        public SubterraneanSiteDevEPExitTeleporter()
        {
            ChargeUse = 0;
            WorksOnCellContents = true;
            NameForStatus = "MatterRecompositionSystem";
        }

        public override bool WantEvent(
            int ID,
            int cascade
        )
        {
            return
                base.WantEvent(ID, cascade) ||
                ID == GetAdjacentNavigationWeightEvent.ID ||
                ID == GetNavigationWeightEvent.ID ||
                ID == ObjectEnteredCellEvent.ID;
        }

        public override bool HandleEvent(
            GetNavigationWeightEvent E
        )
        {
            E.MinWeight(60);
            return base.HandleEvent(E);
        }

        public override bool HandleEvent(
            GetAdjacentNavigationWeightEvent E
        )
        {
            E.MinWeight(2);
            return base.HandleEvent(E);
        }

        public override bool HandleEvent(
            ObjectEnteredCellEvent E
        )
        {
            if (
                E.Object == null ||
                E.Object == ParentObject ||
                !E.Object.IsPlayer() ||
                !IsObjectActivePartSubject(E.Object) ||
                TargetZone.IsNullOrEmpty()
            )
            {
                return base.HandleEvent(E);
            }

            Zone targetZone =
                The.ZoneManager.GetZone(TargetZone);

            if (targetZone == null)
                return base.HandleEvent(E);

            Cell destination =
                FindOriginDestination(targetZone);

            if (destination == null)
                return base.HandleEvent(E);

            //
            // Treat the EP return teleporter as reality-distortion transit.
            //
            // This allows theme-specific protections such as Portal
            // attunement to approve or veto the translocation before the
            // ordinary TeleportTo() movement occurs.
            //
            Event transit =
                Event.New(
                    "InitiateRealityDistortionTransit"
                );

            transit.SetParameter(
                "Object",
                E.Object
            );

            transit.SetParameter(
                "Cell",
                destination
            );

            transit.SetParameter(
                "Device",
                ParentObject
            );


            if (
                !E.Object.FireEvent(
                    transit
                )
            )
            {
                return base.HandleEvent(E);
            }

            Cell oldCell =
                E.Object.CurrentCell;

            if (E.Object.TeleportTo(destination))
            {
                if (E.Object.CurrentCell != oldCell)
                {
                    targetZone.SetActive();

                    IComponent<GameObject>.AddPlayerMessage(
                        "You are teleported back to the extradimensional breach."
                    );
                }

                E.Object.TeleportSwirl();
            }

            return base.HandleEvent(E);
        }

        private Cell FindOriginDestination(
            Zone targetZone
        )
        {
            if (targetZone == null)
                return null;

            // Preferred destination: beside the guaranteed entrance
            // attunement stone, which is already outside the large pit.
            GameObject stone =
                targetZone.FindFirstObject(
                    SubterraneanSiteDev
                        .SubterraneanSiteDevEPAttunementSystem
                        .StoneLeftBlueprint
                );

            if (
                stone != null &&
                stone.CurrentCell != null
            )
            {
                Cell connected =
                    stone.CurrentCell
                        .GetConnectedSpawnLocation();

                if (connected != null)
                    return connected;
            }

            // Defensive fallback if the entrance stone has somehow been
            // destroyed or cannot be found.
            List<Cell> candidates =
                targetZone.GetEmptyReachableCells();

            if (candidates == null ||
                candidates.Count == 0)
            {
                return null;
            }

            return candidates[
                Stat.Random(
                    0,
                    candidates.Count - 1
                )
            ];
        }
    }
}

namespace XRL.World.ZoneBuilders
{
    /// <summary>
    /// Final cleanup for EP build-time semantic reservations.
    ///
    /// Reservations coordinate builders during construction only. They are not
    /// permanent zone state and should not survive after the EP has finished
    /// materializing.
    /// </summary>
    public class SubterraneanSiteDevEPReservationCleanup :
        ZoneBuilderSandbox
    {
        public bool BuildZone(
            Zone Z
        )
        {
            SubterraneanSiteDev
                .SubterraneanSiteDevEPReservations
                .Clear(Z);

            return true;
        }
    }
}

namespace XRL.World.ZoneBuilders
{
    public class SubterraneanSiteDevEPExitTeleporterBuilder :
    ZoneBuilderSandbox
    {
        public string TargetZone = "";

        private const int TransitionExclusionRadius = 4;

        public bool BuildZone(Zone Z)
        {
            if (
                Z == null ||
                TargetZone.IsNullOrEmpty()
            )
            {
                return true;
            }

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
                // The exit teleporter is shared discrete EP content.
                //
                // Respect everything already claimed by themes and shared content,
                // including object-like pools. Permissive spills remain valid terrain.
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
                    ) ||
                    cell.HasObjectWithBlueprint("RelicChest")
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

                bool occupied = false;

                foreach (GameObject obj in cell.GetObjects())
                {
                    if (obj == null)
                        continue;

                    if (
                        obj.IsCombatObject() ||
                        obj.Inventory != null ||
                        obj.HasTagOrProperty("Furniture")
                    )
                    {
                        occupied = true;
                        break;
                    }
                }

                if (occupied)
                    continue;

                candidates.Add(cell);
            }

            if (candidates.Count == 0)
                return true;

            Cell destination =
                candidates[
                    Stat.Random(
                        0,
                        candidates.Count - 1
                    )
                ];

            GameObject teleporter =
                GameObject.Create(
                    "Exit Teleporter"
                );

            if (teleporter == null)
                return true;

            teleporter.RemovePart<CatacombsExitTeleporter>();

            teleporter.AddPart<
                SubterraneanSiteDevEPExitTeleporter
            >(
                new SubterraneanSiteDevEPExitTeleporter
                {
                    TargetZone = TargetZone
                }
            );

            destination.AddObject(
                teleporter
            );

            SubterraneanSiteDev
                .SubterraneanSiteDevEPReservations
                .ClaimCell(
                    Z,
                    destination
                );

            return true;
        }
    }

   
    /// <summary>
    /// Harsh origin-layer dimensional scar. Only cells inside the clipped,
    /// mostly circular footprint are overwritten; everything else in the
    /// pre-existing zone is intentionally left alone.
        /// </summary>
    public class SubterraneanSiteDevEPEntranceScar : ZoneBuilderSandbox
    {
        public int CenterX = 40;
        public int CenterY = 12;
        public int Radius = 11;

        public bool BuildZone(Zone Z)
        {
            if (Z == null)
                return true;

            //
            // The entrance scar is the beginning of the origin-layer EP build.
            //
            // Unlike underground layers, the origin preserves the ordinary zone
            // outside the scar, but its C1/C5 preview content still needs the same
            // shared reservation system.
            //
            SubterraneanSiteDev
                .SubterraneanSiteDevEPReservations
                .Reset(Z);

            if (Radius < 1)
                Radius = 1;

            int radiusSquared = Radius * Radius;

            for (int x = CenterX - Radius; x <= CenterX + Radius; x++)
            {
                for (int y = CenterY - Radius; y <= CenterY + Radius; y++)
                {
                    Cell cell = Z.GetCell(x, y);
                    if (cell == null)
                        continue;

                    int dx = x - CenterX;
                    int dy = y - CenterY;
                    int distanceSquared = dx * dx + dy * dy;

                    if (distanceSquared > radiusSquared)
                        continue;

                    // Deliberately destructive inside the intrusion. Surface
                    // terrain, objects, walls, plants, etc. are simply replaced.
                    cell.Clear();
                }
            }

            Z.ClearReachableMap();
            return true;
        }
    }



    /// <summary>
    /// Generic EP hole renderer.
    ///
    /// Ported from the proven Subterranean Sites path-hole implementation,
    /// but deliberately does NOT draw a path to the hole.
    /// </summary>
    public class SubterraneanSiteDevEPHoleBuilder :
        ZoneBuilderSandbox
    {
        public int CenterX = 40;
        public int CenterY = 12;

        public int Radius = 3;

        public string HoleObject = "Pit";

        public bool BuildZone(Zone Z)
        {
            if (Z == null)
            {
                return true;
            }

            if (Radius < 1)
            {
                Radius = 1;
            }

            for (
                int dx = -Radius;
                dx <= Radius;
                dx++
            )
            {
                for (
                    int dy = -Radius;
                    dy <= Radius;
                    dy++
                )
                {
                    int x =
                        CenterX + dx;

                    int y =
                        CenterY + dy;

                    Cell cell =
                        Z.GetCell(x, y);

                    if (cell == null)
                    {
                        continue;
                    }

                    int distance =
                        Math.Abs(dx) +
                        Math.Abs(dy);

                    //
                    // Preserve the production Sub-Sites
                    // irregular hole shape.
                    //
                    if (
                        distance > Radius &&
                        !50.in100()
                    )
                    {
                        continue;
                    }

                    if (
                        distance >
                        Radius + 1
                    )
                    {
                        continue;
                    }

                    cell.Clear();

                    GameObject hole =
                        GameObjectFactory.Factory
                            .CreateObject(
                                HoleObject
                            );

                    if (hole == null)
                    {
                        continue;
                    }

                    XRL.World.Parts.StairsDown
                        stairsDown =
                            hole.GetPart<
                                XRL.World.Parts
                                    .StairsDown
                            >();

                    if (stairsDown != null)
                    {
                        stairsDown.ConnectLanding =
                            false;
                    }

                    cell.AddObject(
                        "FlyingWhitelistArea"
                    );

                    cell.AddObject(hole);

                    cell.AddObject(
                        "StairBlocker"
                    );

                    cell.AddObject(
                        "InfluenceMapBlocker"
                    );
                }
            }

            return true;
        }
    }
}

namespace SubterraneanSiteDev
{
    /// <summary>
    /// Shared, theme-agnostic placement geometry for extradimensional pockets.
    ///
    /// This class deliberately knows nothing about Fire, Cold, liquids, walls,
    /// floors, or decoration blueprints. Theme code supplies the legal cells;
    /// these helpers only choose/grow spatial patterns and reserve shared areas.
    /// </summary>
    internal static class SubterraneanSiteDevEPPlacement
    {

        /// <summary>
        /// Returns true when a cell already contains a meaningful discrete occupant
        /// that later EP placement should not deliberately stack another object onto.
        ///
        /// This is separate from EPReservations:
        /// - Reservations track semantic ownership by EP builders.
        /// - This catches pre-existing/unclaimed creatures, containers, and furniture.
        ///
        /// Floors, liquids, decals, and other compatible substrate do not count.
        /// </summary>
        public static bool HasMeaningfulOccupant(
            Cell cell
        )
        {
            if (cell == null)
                return false;


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
                    return true;
                }
            }


            return false;
        }

        /// <summary>
        /// Require a square neighborhood around a candidate cell to satisfy the
        /// caller's placement rules.
        ///
        /// Radius 1 is the standard EP "broad open" test: the full 3x3 neighborhood
        /// must remain legal. This keeps impassable decorations out of one-cell halls
        /// and narrow chokepoints without teaching the shared placement system anything
        /// about themes, liquids, reservations, or specific object types.
        /// </summary>
        internal static bool HasBroadOpenClearance(
            Zone Z,
            Cell center,
            Func<Cell, bool> isAllowed,
            int radius = 1
        )
        {
            if (
                Z == null ||
                center == null ||
                isAllowed == null
            )
            {
                return false;
            }

            if (radius < 0)
                radius = 0;

            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
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
                        return false;
                    }

                    Cell cell =
                        Z.GetCell(
                            x,
                            y
                        );

                    if (
                        cell == null ||
                        !isAllowed(cell)
                    )
                    {
                        return false;
                    }
                }
            }

            return true;
        }



        internal static List<Cell> CollectCells(
            Zone Z,
            Func<Cell, bool> isAllowed
        )
        {
            List<Cell> result = new List<Cell>();

            if (Z == null || isAllowed == null)
                return result;

            foreach (Cell cell in Z.GetCells())
            {
                if (cell != null && isAllowed(cell))
                    result.Add(cell);
            }

            return result;
        }

        /// <summary>
        /// Grow one irregular cardinally-connected patch from a seed.
        /// Frontier selection is randomized, so patches are not forced into
        /// circles or rectangles.
        /// </summary>
        internal static HashSet<Cell> GrowPatch(
            Cell seed,
            int targetCount,
            Func<Cell, bool> isAllowed,
            System.Random rng
        )
        {
            HashSet<Cell> result = new HashSet<Cell>();
            List<Cell> frontier = new List<Cell>();

            if (
                seed == null ||
                targetCount <= 0 ||
                isAllowed == null ||
                !isAllowed(seed)
            )
            {
                return result;
            }

            frontier.Add(seed);

            while (
                frontier.Count > 0 &&
                result.Count < targetCount
            )
            {
                int index = NextIndex(frontier.Count, rng);
                Cell cell = frontier[index];
                frontier.RemoveAt(index);

                if (
                    cell == null ||
                    result.Contains(cell) ||
                    !isAllowed(cell)
                )
                {
                    continue;
                }

                result.Add(cell);

                AddFrontierCell(
                    cell.GetCellFromDirection("N"),
                    result,
                    frontier,
                    isAllowed
                );

                AddFrontierCell(
                    cell.GetCellFromDirection("S"),
                    result,
                    frontier,
                    isAllowed
                );

                AddFrontierCell(
                    cell.GetCellFromDirection("E"),
                    result,
                    frontier,
                    isAllowed
                );

                AddFrontierCell(
                    cell.GetCellFromDirection("W"),
                    result,
                    frontier,
                    isAllowed
                );
            }

            return result;
        }

        /// <summary>
        /// Try several candidate seeds and keep the largest patch. Useful when
        /// legal terrain is fragmented into islands or small cave pockets.
        /// </summary>
        internal static HashSet<Cell> GrowBestPatch(
            List<Cell> candidates,
            int targetCount,
            int attempts,
            Func<Cell, bool> isAllowed,
            System.Random rng
        )
        {
            HashSet<Cell> best = new HashSet<Cell>();

            if (
                candidates == null ||
                candidates.Count == 0 ||
                targetCount <= 0
            )
            {
                return best;
            }

            if (attempts < 1)
                attempts = 1;

            Func<Cell, bool> allowed = isAllowed;

            if (allowed == null)
            {
                HashSet<Cell> candidateSet =
                    new HashSet<Cell>(candidates);

                allowed = delegate(Cell cell)
                {
                    return
                        cell != null &&
                        candidateSet.Contains(cell);
                };
            }

            List<Cell> seeds = new List<Cell>();

            foreach (Cell cell in candidates)
            {
                if (cell != null && allowed(cell))
                    seeds.Add(cell);
            }

            if (seeds.Count == 0)
                return best;

            for (int i = 0; i < attempts; i++)
            {
                Cell seed = seeds[NextIndex(seeds.Count, rng)];

                HashSet<Cell> attempt =
                    GrowPatch(
                        seed,
                        targetCount,
                        allowed,
                        rng
                    );

                if (attempt.Count > best.Count)
                    best = attempt;

                if (best.Count >= targetCount)
                    break;
            }

            return best;
        }

        /// <summary>
        /// Grow several patches that share one total target count. The remaining
        /// target is divided among the remaining patches at each step. This is
        /// the behavior used by Fire's asphalt fields.
        /// </summary>
        internal static HashSet<Cell> GrowDistributedPatches(
            List<Cell> candidates,
            int targetCount,
            int patchCount,
            HashSet<Cell> forbidden,
            System.Random rng
        )
        {
            HashSet<Cell> result = new HashSet<Cell>();

            if (
                candidates == null ||
                candidates.Count == 0 ||
                targetCount <= 0
            )
            {
                return result;
            }

            if (patchCount < 1)
                patchCount = 1;

            HashSet<Cell> candidateSet =
                new HashSet<Cell>(candidates);

            int remaining = targetCount;

            for (
                int patch = 0;
                patch < patchCount && remaining > 0;
                patch++
            )
            {
                List<Cell> available = new List<Cell>();

                foreach (Cell cell in candidates)
                {
                    if (
                        cell == null ||
                        result.Contains(cell) ||
                        (forbidden != null && forbidden.Contains(cell))
                    )
                    {
                        continue;
                    }

                    available.Add(cell);
                }

                if (available.Count == 0)
                    break;

                int patchesLeft = patchCount - patch;
                int patchTarget = remaining / patchesLeft;

                if (patchTarget < 1)
                    patchTarget = 1;

                Cell seed =
                    available[NextIndex(available.Count, rng)];

                HashSet<Cell> patchCells =
                    GrowPatch(
                        seed,
                        patchTarget,
                        delegate(Cell cell)
                        {
                            return
                                cell != null &&
                                candidateSet.Contains(cell) &&
                                !result.Contains(cell) &&
                                (forbidden == null || !forbidden.Contains(cell));
                        },
                        rng
                    );

                foreach (Cell cell in patchCells)
                    result.Add(cell);

                remaining = targetCount - result.Count;
            }

            return result;
        }

        internal static Cell PickRandomCell(
            Zone Z,
            Func<Cell, bool> isAllowed,
            System.Random rng
        )
        {
            List<Cell> candidates =
                CollectCells(Z, isAllowed);

            if (candidates.Count == 0)
                return null;

            return candidates[NextIndex(candidates.Count, rng)];
        }

        internal static Cell PickRandomCellNearPatch(
            Zone Z,
            HashSet<Cell> patch,
            int minDistance,
            int maxDistance,
            Func<Cell, bool> isAllowed,
            System.Random rng
        )
        {
            if (
                Z == null ||
                patch == null ||
                patch.Count == 0 ||
                isAllowed == null
            )
            {
                return null;
            }

            if (minDistance < 0)
                minDistance = 0;

            if (maxDistance < minDistance)
                maxDistance = minDistance;

            List<Cell> candidates = new List<Cell>();

            foreach (Cell cell in Z.GetCells())
            {
                if (cell == null || !isAllowed(cell))
                    continue;

                int distance =
                    DistanceToPatch(cell, patch);

                if (
                    distance < minDistance ||
                    distance > maxDistance
                )
                {
                    continue;
                }

                candidates.Add(cell);
            }

            if (candidates.Count == 0)
                return null;

            return candidates[NextIndex(candidates.Count, rng)];
        }

        internal static int DistanceToPatch(
            Cell cell,
            HashSet<Cell> patch
        )
        {
            if (
                cell == null ||
                patch == null ||
                patch.Count == 0
            )
            {
                return int.MaxValue;
            }

            int best = int.MaxValue;

            foreach (Cell other in patch)
            {
                if (other == null)
                    continue;

                int dx = Math.Abs(cell.X - other.X);
                int dy = Math.Abs(cell.Y - other.Y);

                int distance = Math.Max(dx, dy);

                if (distance < best)
                    best = distance;
            }

            return best;
        }

        internal static void ReserveRectangle(
            bool[,] reserved,
            Zone Z,
            int x1,
            int y1,
            int x2,
            int y2
        )
        {
            if (reserved == null || Z == null)
                return;

            if (x1 < 0)
                x1 = 0;
            if (y1 < 0)
                y1 = 0;
            if (x2 >= Z.Width)
                x2 = Z.Width - 1;
            if (y2 >= Z.Height)
                y2 = Z.Height - 1;

            for (int x = x1; x <= x2; x++)
            {
                for (int y = y1; y <= y2; y++)
                    reserved[x, y] = true;
            }
        }

        internal static void ReserveAroundAnchors(
            bool[,] reserved,
            Zone Z,
            List<Location2D> anchors,
            int radius
        )
        {
            if (
                reserved == null ||
                Z == null ||
                anchors == null
            )
            {
                return;
            }

            if (radius < 0)
                radius = 0;

            foreach (Location2D anchor in anchors)
            {
                if (anchor == null)
                    continue;

                ReserveRectangle(
                    reserved,
                    Z,
                    anchor.X - radius,
                    anchor.Y - radius,
                    anchor.X + radius,
                    anchor.Y + radius
                );
            }
        }

        internal static bool IsNearAnyAnchor(
            int x,
            int y,
            List<Location2D> anchors,
            int radius
        )
        {
            if (anchors == null)
                return false;

            if (radius < 0)
                radius = 0;

            foreach (Location2D anchor in anchors)
            {
                if (anchor == null)
                    continue;

                int dx = Math.Abs(x - anchor.X);
                int dy = Math.Abs(y - anchor.Y);

                if (Math.Max(dx, dy) <= radius)
                    return true;
            }

            return false;
        }

        private static void AddFrontierCell(
            Cell cell,
            HashSet<Cell> result,
            List<Cell> frontier,
            Func<Cell, bool> isAllowed
        )
        {
            if (
                cell == null ||
                result.Contains(cell) ||
                !isAllowed(cell) ||
                frontier.Contains(cell)
            )
            {
                return;
            }

            frontier.Add(cell);
        }

        private static int NextIndex(
            int count,
            System.Random rng
        )
        {
            if (count <= 1)
                return 0;

            if (rng != null)
                return rng.Next(count);

            return Stat.Random(0, count - 1);
        }
    }
}
