//v1.0.8
using System;
using System.Collections.Generic;
using System.Text;
using Genkit;
using XRL;
using XRL.Rules;
using XRL.World;
using XRL.World.Parts;
using XRL.World.ZoneBuilders.Utility;
using XRL.Core;
using XRL.UI;

namespace SubterraneanSites
{
    /// <summary>
    /// FIRE DIMENSION
    ///
    /// Category 1: hot ambient environment.
    /// Category 2: crematory fire vents that also heat open liquids.
    /// Category 3: lava exterior / lava boundary.
    /// Category 4: irregular islands and stochastic causeways over
    ///             dark-orange smoldering dirt.
    /// Category 5: hot asphalt/oil, bones, boulders.
    /// </summary>
    internal sealed class SubterraneanSitesEPFireTheme :
        ISubterraneanSitesEPCategoryProvider,
        ISubterraneanSitesEPAttunementProvider,
        ISubterraneanSitesEPDenizenAdaptationProvider,
        ISubterraneanSitesEPSignatureMutationProvider
    {
        public string ThemeKey
        {
            get { return "Fire"; }
        }

        public string SignatureMutationClass
        {
            get { return "FlamingRay"; }
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

                        "HeatResistance",
                        100,

                        "",
                        "",
                        0,

                        SignatureMutationClass,
                        mutationLevel
                    );

            successMessage =
                "Attunement grants:\n" +
                "Flaming Ray (level " +
                mutationLevel.ToString() +
                ")\n" +
                "+100 Heat Resistance";

            return
                SubterraneanSitesEPAttunementBuildResult
                    .Success;
        }

        internal static SubterraneanSitesEPFloorSpec
            CreateFloorSpec()
        {
            return
                new SubterraneanSitesEPFloorSpec
                {
                    //
                    // Salt dunes provide a pale dirt-like tile mask that
                    // recolors cleanly into the Fire theme's scorched
                    // substrate.
                    //
                    FloorBlueprint =
                        "SaltDune",

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
            // Smoldering dirt.
            //
            // Use the same dirt-tile family as vanilla DirtFloor,
            // but choose the variant once when the EP floor is built.
            //
            int dirtVariant =
                Stat.Random(
                    1,
                    4
                );

            floor.Render.Tile =
                "Terrain/sw_ground_dots" +
                dirtVariant.ToString() +
                ".png";

            //
            // Dark-orange scorched-earth palette.
            //
            floor.Render.ColorString =
                "&o^k";

            floor.Render.TileColor =
                "&o";

            floor.Render.DetailColor =
                "w";
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
                "Temperature", "200"
            );

            if (The.Game != null)
            {
                The.Game.RequireSystem<
                    SubterraneanSitesEPFireEnvironmentSystem
                >();
            }
        }

        public void RegisterCategory2(
            SubterraneanSitesEPLayerContext context
        )
        {
            SubterraneanSitesEPBoundaryHazards.Register(
                context,
                "SubterraneanSitesFireVent",
                40,
                3,
                7,
                8
            );
        }

        public void RegisterCategory3(
            SubterraneanSitesEPLayerContext context
        )
        {
            The.ZoneManager.AddZonePostBuilder(
                context.ZoneId,
                "SubterraneanSitesFireMaterials"
            );
        }

        public void RegisterCategory4Layout(
            SubterraneanSitesEPLayerContext context
        )
        {
            The.ZoneManager.AddZonePostBuilder(
                context.ZoneId,
                "SubterraneanSitesEPBlobLayout",
                "LandDepth", "4",
                "AnchorRadius", "4"
            );

            The.ZoneManager.AddZonePostBuilder(
                context.ZoneId,
                "SubterraneanSitesEPBlobConnector",
                "OptionalConnectionPercent", "80",
                "CausewayRadius", "1",
                "MinRegionSize", "6"
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
                "SubterraneanSitesFireFloor"
            );
        }

        public void RegisterCategory5(
            SubterraneanSitesEPLayerContext context
        )
        {
            The.ZoneManager.AddZonePostBuilder(
                context.ZoneId,
                "SubterraneanSitesFireDecorations",
                "AsphaltPercent", "25",
                "OilPercent", "20",
                "AsphaltPatchCount", "4"
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
                "SubterraneanSitesFireFloor",
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
                "SubterraneanSitesFireDecorations",
                "EntranceOnly", "1"
            );
        }

        public void ApplyDenizenAdaptation(
            GameObject creature
        )
        {
            if (creature == null)
                return;

            creature.AddStatBonus(
                "HeatResistance",
                100
            );
        }
    }
}

namespace SubterraneanSites
{
    /// <summary>
    /// Player-facing Fire Category-1 exposure tracking.
    ///
    /// Fire's actual hazard is the zone's ambient temperature. This system
    /// does not create a duplicate player effect; it only announces the
    /// transition into an unprotected Fire environment.
    ///
    /// The warning occurs:
    /// - when the player enters Fire C1 while unattuned;
    /// - when Fire attunement expires while the player remains in Fire C1.
    ///
    /// Moving between layers of the same Fire-primary pocket is not a new
    /// exposure and does not repeat the warning.
    /// </summary>
    [Serializable]
    public class SubterraneanSitesEPFireEnvironmentSystem :
        IGameSystem
    {
        public bool WasInFireEnvironment =
            false;

        public bool WasFireAttuned =
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


            bool fireEnvironment =
                string.Equals(
                    category1,
                    "Fire",
                    StringComparison.Ordinal
                );


            if (!fireEnvironment)
            {
                ResetExposureState();

                return;
            }


            bool fireAttuned =
                SubterraneanSitesEPAttunementSystem
                    .IsAttunedTo(
                        player,
                        "Fire"
                    );


            bool becameExposed =
                !fireAttuned &&
                (
                    !WasInFireEnvironment ||
                    WasFireAttuned
                );


            WasInFireEnvironment =
                true;

            WasFireAttuned =
                fireAttuned;


            if (becameExposed)
            {
                Popup.Show(
                    "You feel hot."
                );
            }
        }


        private void ResetExposureState()
        {
            WasInFireEnvironment =
                false;

            WasFireAttuned =
                false;
        }
    }
}

namespace XRL.World.ZoneBuilders
{
    /// <summary>
    /// Fire Category-4 floor adapter.
    ///
    /// Fire owns its floor appearance. The shared EP floor system owns
    /// universal underground/scar scope and application mechanics.
    /// </summary>
    public class SubterraneanSitesFireFloor :
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
                            .SubterraneanSitesEPFireTheme
                            .CreateFloorSpec(),
                        EntranceOnly != 0
                    );
        }
    }

    public class SubterraneanSitesFireMaterials : ZoneBuilderSandbox
    {
        public string ExteriorBlueprint = "LavaPuddle";

        public bool BuildZone(Zone Z)
        {
            if (Z == null || ExteriorBlueprint.IsNullOrEmpty())
                return true;

            foreach (Cell cell in Z.GetCells())
            {
                if (cell == null)
                    continue;

                if (!SubterraneanSites.SubterraneanSitesEPGeometry
                    .IsAnyPlaceholder(cell))
                {
                    continue;
                }

                cell.ClearWalls();

                GameObject exterior =
                    GameObjectFactory.Factory.CreateObject(
                        ExteriorBlueprint
                    );

                if (exterior != null)
                    cell.AddObject(exterior);
            }

            return true;
        }
    }

    public class SubterraneanSitesEPBlobLayout : ZoneBuilderSandbox
    {
        public int LandDepth = 4;
        public int AnchorRadius = 4;

        public bool BuildZone(Zone Z)
        {
            if (Z == null)
            {
                return true;
            }

            ClampSettings();

            //
            // EP vertical-transition coordinates are mandatory
            // dry-space anchors for this level.
            //
            List<Location2D> verticalAnchors =
                SubterraneanSites
                    .SubterraneanSitesEPVerticalTransitions
                    .GetVerticalAnchors(
                        Z.ZoneID
                    );

            Location2D center =
                Location2D.Get(
                    Z.Width / 2,
                    Z.Height / 2
                );

            //
            // Give NoiseMap important locations as extra nodes.
            //
            List<NoiseMapNode> extraNodes =
                new List<NoiseMapNode>();

            foreach (
                Location2D anchor
                in verticalAnchors
            )
            {
                extraNodes.Add(
                    new NoiseMapNode(
                        anchor.X,
                        anchor.Y
                    )
                );
            }

            extraNodes.Add(
                new NoiseMapNode(
                    center.X,
                    center.Y
                )
            );

            //
            // Same basic NoiseMap parameters used by Mines/Mines2.
            //
            NoiseMap noiseMap =
                new NoiseMap(
                    Z.Width,
                    Z.Height,
                    20,
                    3,
                    3,
                    5,
                    50,
                    60,
                    6,
                    2,
                    1,
                    1,
                    extraNodes
                );

            foreach (
                List<NoiseMapNode> area
                in noiseMap.AreaNodes.Values
            )
            {
                foreach (
                    NoiseMapNode node
                    in area
                )
                {
                    //
                    // Main island body.
                    //
                    if (node.depth >= LandDepth)
                    {
                        CarveLand(
                            Z,
                            node.x,
                            node.y
                        );

                        continue;
                    }

                    //
                    // Admit some cells from the immediately
                    // shallower noise band to broaden the islands.
                    //
                    if (
                        node.depth == LandDepth - 1 &&
                        50.in100()
                    )
                    {
                        CarveLand(
                            Z,
                            node.x,
                            node.y
                        );
                    }
                }
            }

            //
            // Vertical transitions need reliable dry footprints.
            //
            // Unlike ordinary islands, these do not get a
            // randomized outer edge.
            //
            foreach (
                Location2D anchor
                in verticalAnchors
            )
            {
                CarveMandatoryIsland(
                    Z,
                    anchor.X,
                    anchor.Y,
                    AnchorRadius
                );
            }

            //
            // Keep the ordinary central island.
            //
            CarveIsland(
                Z,
                center.X,
                center.Y,
                AnchorRadius
            );

            //
            // Connectivity is handled by the separate
            // EP blob connector after all land exists.
            //
            Z.ClearReachableMap();

            return true;
        }

        private void ClampSettings()
        {
            if (LandDepth < 0)
            {
                LandDepth = 0;
            }

            if (AnchorRadius < 1)
            {
                AnchorRadius = 1;
            }
        }

        private void CarveMandatoryIsland(
            Zone Z,
            int centerX,
            int centerY,
            int radius
        )
        {
            int radiusSquared =
                radius * radius;

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
                    int distanceSquared =
                        dx * dx +
                        dy * dy;

                    if (
                        distanceSquared >
                        radiusSquared
                    )
                    {
                        continue;
                    }

                    CarveLand(
                        Z,
                        centerX + dx,
                        centerY + dy
                    );
                }
            }
        }

        private void CarveLand(
            Zone Z,
            int x,
            int y
        )
        {
            Cell cell =
                Z.GetCell(x, y);

            if (cell == null)
            {
                return;
            }

            cell.Clear();
        }

        private void CarveIsland(
            Zone Z,
            int centerX,
            int centerY,
            int radius
        )
        {
            int radiusSquared =
                radius * radius;

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
                    int distanceSquared =
                        dx * dx + dy * dy;

                    if (
                        distanceSquared >
                        radiusSquared
                    )
                    {
                        continue;
                    }

                    //
                    // Keep the center reliable, but make the
                    // outer edge somewhat irregular.
                    //
                    if (
                        distanceSquared >
                        radiusSquared * 2 / 3 &&
                        !70.in100()
                    )
                    {
                        continue;
                    }

                    CarveLand(
                        Z,
                        centerX + dx,
                        centerY + dy
                    );
                }
            }
        }
    }
}

namespace XRL.World.ZoneBuilders
{
    public class SubterraneanSitesEPBlobConnector :
        ZoneBuilderSandbox
    {
        public int OptionalConnectionPercent = 80;

        public int CausewayRadius = 1;

        public int MinRegionSize = 6;

        private class LandRegion
        {
            public List<Location2D> Cells =
                new List<Location2D>();

            public Location2D Anchor;

            public bool HasIncomingLanding;
            public bool HasOutgoingHole;

            public bool IsVerticalRegion
            {
                get
                {
                    return
                        HasIncomingLanding ||
                        HasOutgoingHole;
                }
            }
        }

        public bool BuildZone(Zone Z)
        {
            if (Z == null)
            {
                return true;
            }

            ClampSettings();

            Location2D incoming = null;
            Location2D outgoing = null;

            SubterraneanSites
                .SubterraneanSitesEPVerticalTransitions
                .TryGetCoordinate(
                    Z.ZoneID,
                    SubterraneanSites
                        .SubterraneanSitesEPVerticalTransitions
                        .IncomingLandingProperty,
                    out incoming
                );

            SubterraneanSites
                .SubterraneanSitesEPVerticalTransitions
                .TryGetCoordinate(
                    Z.ZoneID,
                    SubterraneanSites
                        .SubterraneanSitesEPVerticalTransitions
                        .OutgoingHoleProperty,
                    out outgoing
                );

            List<LandRegion> regions =
                FindLandRegions(
                    Z,
                    incoming,
                    outgoing
                );

            if (regions.Count == 0)
            {
                Z.ClearReachableMap();
                return true;
            }

            regions.Sort(
                delegate(
                    LandRegion a,
                    LandRegion b
                )
                {
                    return
                        b.Cells.Count.CompareTo(
                            a.Cells.Count
                        );
                }
            );

            LandRegion root =
                FindRootRegion(regions);

            if (root == null)
            {
                Z.ClearReachableMap();
                return true;
            }

            List<LandRegion> connectedNetwork =
                new List<LandRegion>();

            connectedNetwork.Add(root);

            //
            // Incoming/outgoing vertical-transition regions are mandatory.
            // This keeps Fire geometry usable when a mixed Category 3 turns
            // the surrounding exterior into hard solid material.
            //
            foreach (
                LandRegion region
                in regions
            )
            {
                if (
                    region == root ||
                    connectedNetwork.Contains(region) ||
                    !region.IsVerticalRegion
                )
                {
                    continue;
                }

                ConnectRegion(
                    Z,
                    region,
                    connectedNetwork
                );
            }

            //
            // Connect most remaining substantial regions while preserving a
            // little occasional isolation for visual variety.
            //
            foreach (
                LandRegion region
                in regions
            )
            {
                if (
                    region == root ||
                    connectedNetwork.Contains(region)
                )
                {
                    continue;
                }

                if (
                    !OptionalConnectionPercent
                        .in100()
                )
                {
                    continue;
                }

                ConnectRegion(
                    Z,
                    region,
                    connectedNetwork
                );
            }

            RebuildInteriorReachability(
                Z,
                root.Anchor
            );

            return true;
        }

        private void ConnectRegion(
            Zone Z,
            LandRegion region,
            List<LandRegion> connectedNetwork
        )
        {
            LandRegion target =
                FindNearestConnectedRegion(
                    region,
                    connectedNetwork
                );

            if (target == null)
            {
                return;
            }

            CarveCauseway(
                Z,
                region.Anchor,
                target.Anchor
            );

            connectedNetwork.Add(region);
        }

        private void ClampSettings()
        {
            if (
                OptionalConnectionPercent < 0
            )
            {
                OptionalConnectionPercent = 0;
            }

            if (
                OptionalConnectionPercent > 100
            )
            {
                OptionalConnectionPercent = 100;
            }

            if (CausewayRadius < 0)
            {
                CausewayRadius = 0;
            }

            if (MinRegionSize < 1)
            {
                MinRegionSize = 1;
            }
        }

        private List<LandRegion> FindLandRegions(
            Zone Z,
            Location2D incoming,
            Location2D outgoing
        )
        {
            List<LandRegion> result =
                new List<LandRegion>();

            bool[,] visited =
                new bool[
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
                    if (visited[x, y])
                    {
                        continue;
                    }

                    Cell start =
                        Z.GetCell(x, y);

                    if (!IsInterior(start))
                    {
                        visited[x, y] = true;
                        continue;
                    }

                    LandRegion region =
                        FloodRegion(
                            Z,
                            x,
                            y,
                            visited,
                            incoming,
                            outgoing
                        );

                    if (
                        region != null &&
                        region.Cells.Count >=
                        MinRegionSize
                    )
                    {
                        region.Anchor =
                            FindRegionAnchor(
                                region
                            );

                        result.Add(region);
                    }
                }
            }

            return result;
        }

        private LandRegion FloodRegion(
            Zone Z,
            int startX,
            int startY,
            bool[,] visited,
            Location2D incoming,
            Location2D outgoing
        )
        {
            LandRegion region =
                new LandRegion();

            Queue<Location2D> queue =
                new Queue<Location2D>();

            queue.Enqueue(
                Location2D.Get(
                    startX,
                    startY
                )
            );

            visited[
                startX,
                startY
            ] = true;

            while (queue.Count > 0)
            {
                Location2D current =
                    queue.Dequeue();

                Cell cell =
                    Z.GetCell(
                        current.X,
                        current.Y
                    );

                if (!IsInterior(cell))
                {
                    continue;
                }

                region.Cells.Add(current);

                if (
                    SameLocation(
                        current,
                        incoming
                    )
                )
                {
                    region.HasIncomingLanding =
                        true;
                }

                if (
                    SameLocation(
                        current,
                        outgoing
                    )
                )
                {
                    region.HasOutgoingHole =
                        true;
                }

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
                            current.X + dx;

                        int ny =
                            current.Y + dy;

                        if (
                            nx < 0 ||
                            ny < 0 ||
                            nx >= Z.Width ||
                            ny >= Z.Height
                        )
                        {
                            continue;
                        }

                        if (visited[nx, ny])
                        {
                            continue;
                        }

                        Cell next =
                            Z.GetCell(
                                nx,
                                ny
                            );

                        if (!IsInterior(next))
                        {
                            continue;
                        }

                        visited[nx, ny] = true;

                        queue.Enqueue(
                            Location2D.Get(
                                nx,
                                ny
                            )
                        );
                    }
                }
            }

            return region;
        }

        private bool SameLocation(
            Location2D a,
            Location2D b
        )
        {
            return
                a != null &&
                b != null &&
                a.X == b.X &&
                a.Y == b.Y;
        }

        private bool IsInterior(
            Cell cell
        )
        {
            return
                SubterraneanSites
                    .SubterraneanSitesEPGeometry
                    .IsOpenGeometryCell(cell);
        }

        private Location2D FindRegionAnchor(
            LandRegion region
        )
        {
            if (
                region == null ||
                region.Cells.Count == 0
            )
            {
                return null;
            }

            int sumX = 0;
            int sumY = 0;

            foreach (
                Location2D location
                in region.Cells
            )
            {
                sumX += location.X;
                sumY += location.Y;
            }

            int centerX =
                sumX /
                region.Cells.Count;

            int centerY =
                sumY /
                region.Cells.Count;

            Location2D best =
                region.Cells[0];

            int bestDistance =
                int.MaxValue;

            foreach (
                Location2D location
                in region.Cells
            )
            {
                int distance =
                    Math.Abs(
                        location.X -
                        centerX
                    ) +
                    Math.Abs(
                        location.Y -
                        centerY
                    );

                if (
                    distance <
                    bestDistance
                )
                {
                    bestDistance =
                        distance;

                    best = location;
                }
            }

            return best;
        }

        private LandRegion FindRootRegion(
            List<LandRegion> regions
        )
        {
            //
            // Player arrives here.
            //
            foreach (
                LandRegion region
                in regions
            )
            {
                if (
                    region.HasIncomingLanding
                )
                {
                    return region;
                }
            }

            //
            // Origin-style fallback.
            //
            foreach (
                LandRegion region
                in regions
            )
            {
                if (
                    region.HasOutgoingHole
                )
                {
                    return region;
                }
            }

            //
            // regions were sorted largest-first.
            //
            return regions[0];
        }

        private LandRegion
            FindNearestConnectedRegion(
                LandRegion source,
                List<LandRegion> connected
            )
        {
            if (
                source == null ||
                source.Anchor == null ||
                connected == null ||
                connected.Count == 0
            )
            {
                return null;
            }

            LandRegion best = null;

            int bestDistance =
                int.MaxValue;

            foreach (
                LandRegion candidate
                in connected
            )
            {
                if (
                    candidate == null ||
                    candidate.Anchor == null
                )
                {
                    continue;
                }

                int distance =
                    Math.Abs(
                        source.Anchor.X -
                        candidate.Anchor.X
                    ) +
                    Math.Abs(
                        source.Anchor.Y -
                        candidate.Anchor.Y
                    );

                if (
                    distance <
                    bestDistance
                )
                {
                    bestDistance =
                        distance;

                    best = candidate;
                }
            }

            return best;
        }

        private void CarveCauseway(
            Zone Z,
            Location2D from,
            Location2D to
        )
        {
            if (
                from == null ||
                to == null
            )
            {
                return;
            }

            int x = from.X;
            int y = from.Y;

            int safety =
                Z.Width *
                Z.Height *
                2;

            while (
                (
                    x != to.X ||
                    y != to.Y
                ) &&
                safety-- > 0
            )
            {
                CarveCausewayCell(
                    Z,
                    x,
                    y
                );

                int dx =
                    to.X - x;

                int dy =
                    to.Y - y;

                bool moveX;

                if (dx == 0)
                {
                    moveX = false;
                }
                else if (dy == 0)
                {
                    moveX = true;
                }
                else
                {
                    int xWeight =
                        Math.Abs(dx);

                    int yWeight =
                        Math.Abs(dy);

                    moveX =
                        Stat.Random(
                            1,
                            xWeight +
                            yWeight
                        ) <= xWeight;
                }

                if (moveX)
                {
                    x += Math.Sign(dx);
                }
                else
                {
                    y += Math.Sign(dy);
                }
            }

            CarveCausewayCell(
                Z,
                to.X,
                to.Y
            );
        }

        private void CarveCausewayCell(
            Zone Z,
            int centerX,
            int centerY
        )
        {
            for (
                int dx = -CausewayRadius;
                dx <= CausewayRadius;
                dx++
            )
            {
                for (
                    int dy = -CausewayRadius;
                    dy <= CausewayRadius;
                    dy++
                )
                {
                    if (
                        Math.Abs(dx) +
                        Math.Abs(dy) >
                        CausewayRadius + 1
                    )
                    {
                        continue;
                    }

                    Cell cell =
                        Z.GetCell(
                            centerX + dx,
                            centerY + dy
                        );

                    if (cell == null)
                    {
                        continue;
                    }

                    cell.Clear();
                }
            }
        }

        private void RebuildInteriorReachability(
            Zone Z,
            Location2D start
        )
        {
            Z.ClearReachableMap();

            if (start == null)
            {
                return;
            }

            bool[,] visited =
                new bool[
                    Z.Width,
                    Z.Height
                ];

            Queue<Location2D> queue =
                new Queue<Location2D>();

            queue.Enqueue(start);

            visited[
                start.X,
                start.Y
            ] = true;

            while (queue.Count > 0)
            {
                Location2D current =
                    queue.Dequeue();

                Cell cell =
                    Z.GetCell(
                        current.X,
                        current.Y
                    );

                if (!IsInterior(cell))
                {
                    continue;
                }

                Z.ReachableMap[
                    current.X,
                    current.Y
                ] = true;

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
                            current.X + dx;

                        int ny =
                            current.Y + dy;

                        if (
                            nx < 0 ||
                            ny < 0 ||
                            nx >= Z.Width ||
                            ny >= Z.Height
                        )
                        {
                            continue;
                        }

                        if (visited[nx, ny])
                        {
                            continue;
                        }

                        Cell next =
                            Z.GetCell(
                                nx,
                                ny
                            );

                        if (!IsInterior(next))
                        {
                            continue;
                        }

                        visited[nx, ny] = true;

                        queue.Enqueue(
                            Location2D.Get(
                                nx,
                                ny
                            )
                        );
                    }
                }
            }
        }
    }
}

namespace XRL.World.ZoneBuilders
{
    public class SubterraneanSitesFireDecorations :
        ZoneBuilderSandbox
    {
        public int EntranceOnly = 0;
        public int TransitionExclusionRadius = 4;
        public int AsphaltPercent = 25;
        public int OilPercent = 20;

        public int AsphaltPatchCount = 4;

        public int BoneCellsPerObject = 8;
        public int MinBones = 30;
        public int MaxBones = 120;

        private const string AsphaltBlueprint =
            "SubterraneanSitesFireAsphaltPuddle";

        private const string BonesBlueprint =
            "Bones";

        private const string SmallBoulderBlueprint =
            "SmallBoulder";
        private const string MediumBoulderBlueprint =
            "MediumBoulder";
        private const string LargeBoulderBlueprint =
            "LargeBoulder";
        public int MinBoulders = 5;
        public int MaxBoulders = 12;

        private const string StairsUpBlueprint =
            "StairsUp";

        private const string StairsDownBlueprint =
            "StairsDown";

        public bool BuildZone(Zone Z)
        {
            if (Z == null)
            {
                return true;
            }

            List<Location2D> anchors =
                SubterraneanSites
                    .SubterraneanSitesEPVerticalTransitions
                    .GetVerticalAnchors(
                        Z.ZoneID
                    );

            List<Cell> interior =
                GetInteriorCells(
                    Z,
                    anchors
                );

            if (interior.Count == 0)
            {
                return true;
            }

            //
            // Asphalt:
            // several separate patches totaling about
            // 25% of the usable interior.
            //
            int asphaltTarget =
                interior.Count * AsphaltPercent / 100;

            HashSet<Cell> asphaltCells =
                SubterraneanSites
                    .SubterraneanSitesEPPlacement
                    .GrowDistributedPatches(
                        interior,
                        asphaltTarget,
                        AsphaltPatchCount,
                        null,
                        null
                    );

            foreach (Cell cell in asphaltCells)
            {
                if (cell == null)
                {
                    continue;
                }

                //
                // Fire asphalt is a spill:
                // ignore reservations and mix with any existing open liquid.
                //
                SubterraneanSites
                    .SubterraneanSitesEPLiquids
                    .AddOrMixLiquid(
                        cell,
                        XRL.Liquids
                            .SubterraneanSitesFireAsphalt
                            .LiquidID,
                        AsphaltBlueprint
                    );
            }

            //
            // Oil:
            // one contiguous spill totaling about
            // 20% of the interior.
            //
            // Fire spills are deliberately allowed to overlap and mix, so oil may
            // grow through asphalt as well as any other compatible liquid/content.
            //
            int oilTarget =
                interior.Count * OilPercent / 100;

            List<Cell> oilCandidates =
                new List<Cell>(
                    interior
                );

            HashSet<Cell> oilCells =
                SubterraneanSites
                    .SubterraneanSitesEPPlacement
                    .GrowBestPatch(
                        oilCandidates,
                        oilTarget,
                        12,
                        null,
                        null
                    );

            foreach (Cell cell in oilCells)
            {
                AddOil(cell);
            }

            PlaceBoulders(
                interior
            );

            //
            // Bones:
            // intentionally heavy scattering.
            //
            PlaceBones(
                interior
            );

            return true;
        }

        private List<Cell> GetInteriorCells(
            Zone Z,
            List<Location2D> anchors
        )
        {
            List<Cell> result =
                new List<Cell>();

            foreach (Cell cell in Z.GetCells())
            {
                if (cell == null)
                {
                    continue;
                }

                if (EntranceOnly != 0)
                {
                    if (!SubterraneanSites
                        .SubterraneanSitesEPEntrance
                        .IsInsideScar(Z, cell) ||
                        SubterraneanSites
                            .SubterraneanSitesEPEntrance
                            .IsInsideHoleExclusion(Z, cell, 1))
                    {
                        continue;
                    }
                }
                else if (!SubterraneanSites
                    .SubterraneanSitesEPGeometry
                    .IsOpenGeometryCell(cell))
                {
                    continue;
                }

                //
                // Underground decorations should not occupy the player's planned
                // incoming/outgoing transition footprints.
                //
                // Entrance mode already has the much larger hole exclusion.
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
                    continue;
                }

                if (
                    cell.HasObjectWithBlueprint(
                        StairsUpBlueprint
                    ) ||
                    cell.HasObjectWithBlueprint(
                        StairsDownBlueprint
                    )
                )
                {
                    continue;
                }

                result.Add(cell);
            }

            return result;
        }

        private void AddOil(
            Cell cell
        )
        {
            if (cell == null)
            {
                return;
            }

            //
            // Fire oil is a spill:
            // mix into an existing open liquid or create its native puddle on dry cells.
            //
            SubterraneanSites
                .SubterraneanSitesEPLiquids
                .AddOrMixLiquid(
                    cell,
                    XRL.Liquids
                        .SubterraneanSitesFireOil
                        .LiquidID,
                    "SubterraneanSitesFireOilPuddle"
                );
        }

        private void PlaceBoulders(
            List<Cell> interior
        )
        {
            if (
                interior == null ||
                interior.Count == 0
            )
            {
                return;
            }

            HashSet<Cell> interiorSet =
                new HashSet<Cell>(
                    interior
                );

            int minBoulders = MinBoulders;
            int maxBoulders = MaxBoulders;

            if (minBoulders < 0)
            {
                minBoulders = 0;
            }

            if (maxBoulders < minBoulders)
            {
                maxBoulders = minBoulders;
            }

            int desired =
                Stat.Random(
                    minBoulders,
                    maxBoulders
                );

            List<Cell> candidates =
                new List<Cell>();

            foreach (Cell cell in interior)
            {
                if (cell == null)
                {
                    continue;
                }

                //
                // Don't block stairs.
                //
                if (
                    cell.HasObjectWithBlueprint(
                        StairsUpBlueprint
                    ) ||
                    cell.HasObjectWithBlueprint(
                        StairsDownBlueprint
                    )
                )
                {
                    continue;
                }

                //
                // Boulders are discrete C5 objects.
                //
                // Unlike the Fire spills above, they must respect earlier semantic ownership.
                //
                if (
                    SubterraneanSites
                        .SubterraneanSitesEPReservations
                        .IsClaimed(
                            cell.ParentZone,
                            cell
                        )
                )
                {
                    continue;
                }

                //
                // Only use cells that do not already
                // contain another solid object.
                //
                if (!cell.IsEmptyOfSolid())
                {
                    continue;
                }

                if (
                    !SubterraneanSites
                        .SubterraneanSitesEPPlacement
                        .HasBroadOpenClearance(
                            cell.ParentZone,
                            cell,
                            delegate(Cell neighbor)
                            {
                                if (
                                    neighbor == null ||
                                    !interiorSet.Contains(
                                        neighbor
                                    )
                                )
                                {
                                    return false;
                                }

                                if (
                                    SubterraneanSites
                                        .SubterraneanSitesEPReservations
                                        .IsClaimed(
                                            cell.ParentZone,
                                            neighbor
                                        )
                                )
                                {
                                    return false;
                                }

                                return
                                    neighbor.IsEmptyOfSolid();
                            }
                        )
                )
                {
                    continue;
                }

                candidates.Add(cell);
            }

            for (
                int i = 0;
                i < desired &&
                candidates.Count > 0;
                i++
            )
            {
                int index =
                    Stat.Random(
                        0,
                        candidates.Count - 1
                    );

                Cell cell =
                    candidates[index];

                candidates.RemoveAt(index);

                if(50.in100()){

                    cell.AddObject(
                        SmallBoulderBlueprint
                    );

                }else{
                    if(50.in100()){

                        cell.AddObject(
                            MediumBoulderBlueprint
                        );

                    }else{

                        cell.AddObject(
                            LargeBoulderBlueprint
                        );

                    }
                }

                SubterraneanSites
                    .SubterraneanSitesEPReservations
                    .ClaimCell(
                        cell.ParentZone,
                        cell
                    );
            }
        }

        private void PlaceBones(
            List<Cell> interior
        )
        {
            if (
                interior == null ||
                interior.Count == 0
            )
            {
                return;
            }

            int desired =
                interior.Count /
                BoneCellsPerObject;

            if (desired < MinBones)
            {
                desired = MinBones;
            }

            if (desired > MaxBones)
            {
                desired = MaxBones;
            }

            List<Cell> choices =
                new List<Cell>();

            foreach (Cell cell in interior)
            {
                if (cell == null)
                    continue;

                //
                // Bones are discrete decoration. Respect earlier claims, including
                // boulders placed immediately above.
                //
                if (
                    SubterraneanSites
                        .SubterraneanSitesEPReservations
                        .IsClaimed(
                            cell.ParentZone,
                            cell
                        )
                )
                {
                    continue;
                }

                if (!cell.IsEmptyOfSolid())
                    continue;

                choices.Add(
                    cell
                );
            }

            for (
                int i = 0;
                i < desired &&
                choices.Count > 0;
                i++
            )
            {
                int index =
                    Stat.Random(
                        0,
                        choices.Count - 1
                    );

                Cell cell =
                    choices[index];

                choices.RemoveAt(index);

                if (cell != null)
                {
                    cell.AddObject(
                        BonesBlueprint
                    );

                    SubterraneanSites
                        .SubterraneanSitesEPReservations
                        .ClaimCell(
                            cell.ParentZone,
                            cell
                        );
                }
            }
        }
    }
}

namespace XRL.World.Parts
{
    /// <summary>
    /// Vanilla WalltrapFire only applies TemperatureChange to its narrow
    /// ShouldFlame target set. Fire EP vents add the missing environmental
    /// interaction: open liquids in the visible jet path are heated too.
    /// </summary>
    [Serializable]
    public class SubterraneanSitesFireVentLiquidHeat : IPart
    {
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

        private void HeatLiquidsInCell(
            Cell cell,
            WalltrapFire firePart
        )
        {
            if (cell == null)
                return;

            foreach (GameObject obj in cell.GetObjectsInCell())
            {
                if (!ParentObject.PhaseMatches(obj))
                    continue;

                LiquidVolume liquid = obj.LiquidVolume;

                if (liquid == null || !liquid.IsOpenVolume())
                    continue;

                obj.TemperatureChange(
                    firePart.JetTemperature.RollCached(),
                    ParentObject
                );
            }
        }

        private void HeatLiquidJet(
            string direction,
            WalltrapFire firePart
        )
        {
            Cell cell = ParentObject.CurrentCell;

            for (int i = 0; i < firePart.JetLength; i++)
            {
                cell = cell.GetCellFromDirection(direction);

                if (cell == null)
                    break;

                HeatLiquidsInCell(cell, firePart);

                // Match WalltrapFire.FireJet stopping behavior.
                if (cell.IsSolid(true))
                    break;
            }
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "WalltrapTrigger")
            {
                Cell currentCell = ParentObject.CurrentCell;

                if (currentCell != null &&
                    !IsBroken() &&
                    !IsRusted() &&
                    !IsEMPed())
                {
                    WalltrapFire firePart =
                        ParentObject.GetPart<WalltrapFire>();

                    if (firePart != null)
                    {
                        foreach (
                            string direction
                            in Directions.CardinalDirectionList
                        )
                        {
                            Cell adjacent =
                                currentCell.GetCellFromDirection(direction);

                            if (adjacent != null &&
                                !adjacent.IsSolid(true))
                            {
                                HeatLiquidJet(direction, firePart);
                            }
                        }
                    }
                }
            }

            return base.FireEvent(E);
        }
    }
}

namespace XRL.Liquids
{
    [IsLiquid]
    [Serializable]
    public class SubterraneanSitesFireAsphalt : LiquidAsphalt
    {
        public const string LiquidID = "SubterraneanSitesFireAsphalt";

        public SubterraneanSitesFireAsphalt()
            : base()
        {
            // LiquidAsphalt's constructor creates ordinary asphalt,
            // whose BaseLiquid temperature otherwise remains the
            // default 25 degrees.
            //
            // Give this subclass its own liquid ID so vanilla asphalt
            // is completely untouched.
            ((BaseLiquid)this).ID = LiquidID;

            // Fire EP ambient temperature.
            Temperature = 200;
        }

        public override bool Drank(
            LiquidVolume Liquid,
            int Volume,
            GameObject Target,
            StringBuilder Message,
            ref bool ExitInterface)
        {
            Message.Compound("{{K|It burns!}}");
            Target.TemperatureChange(500, Target);

            // Vanilla LiquidAsphalt hard-codes "asphalt" here.
            // Our custom liquid has a different ID, so reproduce the
            // vanilla behavior using our own liquid ID.
            Damage damage = new Damage(
                Stat.Roll(
                    (Liquid.Proportion(LiquidID) / 100 + 1).ToString() + "d6"
                )
            );

            Event E = Event.New("TakeDamage");
            E.AddParameter("Damage", damage);
            E.AddParameter("Owner", Liquid);
            E.AddParameter("Attacker", Liquid);
            E.AddParameter("Message", "from {{K|drinking asphalt}}!");

            Target.FireEvent(E);

            ExitInterface = true;
            return true;
        }
    }


    [IsLiquid]
    [Serializable]
    public class SubterraneanSitesFireOil : LiquidOil
    {
        public const string LiquidID = "SubterraneanSitesFireOil";

        public SubterraneanSitesFireOil()
            : base()
        {
            // Give this subclass its own liquid ID.
            ((BaseLiquid)this).ID = LiquidID;

            //Fire EP ambient temperature.
            Temperature = 200;
        }
    }
}
