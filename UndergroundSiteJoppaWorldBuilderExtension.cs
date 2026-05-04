using System;
using System.Collections.Generic;
using XRL;
using XRL.Core;
using XRL.World;
using XRL.World.WorldBuilders;

namespace SubterraneanSites
{
    [JoppaWorldBuilderExtension]
    public class UndergroundSiteJoppaWorldBuilderExtension : IJoppaWorldBuilderExtension
    {
        public override void OnAfterBuild(JoppaWorldBuilder builder)
        {
            The.Game.RequireSystem<RuntimeZoneBuilderInjectionSystem>();
        }
    }

    public class RuntimeZoneBuilderInjectionSystem : IGameSystem
    {
        private const string TargetZoneId = "JoppaWorld.11.22.0.1.11";

        public override void Register(XRLGame game, IEventRegistrar registrar)
        {
            registrar.Register(BeforeZoneBuiltEvent.ID);
        }

        public override bool HandleEvent(BeforeZoneBuiltEvent zoneBuildEvent)
        {
            var Z = zoneBuildEvent.Zone;
            var zId = Z.ZoneID;

            if (Z.Z <= 10)
            {
                return true;
            }

            int rawSeed = XRLCore.Core.Game.GetWorldSeed();
            int zoneSeed = XRLCore.Core.Game.GetWorldSeed(TargetZoneId + rawSeed);
            System.Random rng = new System.Random(zoneSeed);
            int layers = rng.Next(2, 7);

            List<string> siteZoneIds = BuildSiteZoneIds(layers);

            // Only the top layer is handled directly here.
            // Lower layers are registered with ZoneManager and should build later.
            if (zId != siteZoneIds[0])
            {
                return true;
            }

            RegisterLowerLayers(siteZoneIds);

            foreach (var cell in Z.GetCells())
            {
                cell.Clear();
            }

            var lair = new XRL.World.ZoneBuilders.BasicLair();
            lair.Table = "";
            lair.Adjectives = "";
            lair.Stairs = "D";
            lair.BuildZone(Z);

            The.ZoneManager.SetZoneName
            (
                Z.ZoneID,
                "Stacked Lair Test: Layer 1 of " + siteZoneIds.Count,
                Proper: false
            );

            return true;
        }

        private List<string> BuildSiteZoneIds(int layers)
        {
            List<string> siteZoneIds = new List<string>();

            string baseWorld = "JoppaWorld";
            int parasangX = 11;
            int parasangY = 22;
            int zoneX = 0;
            int zoneY = 1;
            int startZ = 11;

            for (int i = 0; i < layers; i++)
            {
                int z = startZ + i;

                string zoneId =
                    baseWorld + "." +
                    parasangX + "." +
                    parasangY + "." +
                    zoneX + "." +
                    zoneY + "." +
                    z;

                siteZoneIds.Add(zoneId);
            }

            return siteZoneIds;
        }

        private void RegisterLowerLayers(List<string> siteZoneIds)
        {
            for (int i = 1; i < siteZoneIds.Count; i++)
            {
                string childZoneId = siteZoneIds[i];

                The.ZoneManager.ClearZoneBuilders(childZoneId);
                The.ZoneManager.SetZoneProperty(childZoneId, "SkipTerrainBuilders", true);

                string stairs;

                if (i == siteZoneIds.Count - 1)
                {
                    stairs = "U";
                }
                else
                {
                    stairs = "UD";
                }

                The.ZoneManager.AddZoneBuilder(
                    childZoneId,
                    6000,
                    "BasicLair",
                    "Table", "",
                    "Adjectives", "",
                    "Stairs", stairs
                );

                The.ZoneManager.SetZoneName
                (
                    childZoneId,
                    "Stacked Lair Test: Layer " + (i + 1) + " of " + siteZoneIds.Count,
                    Proper: false
                );
            }
        }
    }
}