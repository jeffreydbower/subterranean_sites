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
            // Register the runtime system after world generation.
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

            // Only test underground zones.
            if (Z.Z <= 10)
            {
                return true;
            }

            
            int rawSeed = XRLCore.Core.Game.GetWorldSeed();
            int zoneSeed = XRLCore.Core.Game.GetWorldSeed(TargetZoneId + rawSeed);
            System.Random rng = new System.Random(zoneSeed);
            int layers = rng.Next(1, 8); // 1 to 7 addtional layers

            List<string> siteZoneIds = new List<string>();

            string baseWorld = "JoppaWorld";
            int parasangX = 11;
            int parasangY = 22;
            int zoneX = 0;
            int zoneY = 1;
            int startZ = 11;

            siteZoneIds.Add(TargetZoneId);
            for (int i = 1; i < layers; i++)
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

            //foreach (string zoneId in siteZoneIds)
            for (int layer_num = 0; layer_num < siteZoneIds.Count; layer_num++)
            {
                string zoneId = siteZoneIds[layer_num];

                if (zId == zoneId)
                {
                    // this zone is one layer of the test site
                    foreach (var cell in Z.GetCells())
                    {
                        cell.Clear();
                    }

                    var lair = new XRL.World.ZoneBuilders.BasicLair();
                    lair.Table = "";        // default
                    //lair.Table = "DynamicInheritsTable:Creature:Tier5";        // test creature table 
                    lair.Adjectives = "";   // test adjetive
                    lair.Stairs = "D";       // down stairs stairs
                    lair.BuildZone(Z);

                    if(zId == siteZoneIds[siteZoneIds.Count - 1])
                    {
                        var factionEncounters = new XRL.World.ZoneBuilders.FactionEncounters();
                        factionEncounters.Chance = 100;
                        factionEncounters.Rolls = 1;
                        factionEncounters.Population = "GenericFactionPopulation";
                        factionEncounters.BuildZone(Z);
                    }

                    The.ZoneManager.SetZoneName
                    (
                    Z.ZoneID,
                    "Stacked Lair Test: Layer: " + (layer_num + 1) + " of " + siteZoneIds.Count,
                    Proper: false
                    );

                    return true;

                }
            }
            

            //int zoneSeed = StableHash(Z.ZoneID);
            //Tests that world seed is generated and constand succeeded
            //below is the corect call
            //int rawSeed = XRLCore.Core.Game.GetWorldSeed();
            //int hash = XRLCore.Core.Game.GetWorldSeed(Z.ZoneID + rawSeed);
            //Test for deterministic randon numbers
            //will repeat 2 new games 
            //if we use the zoneid as the rng seed
            //then the rolls and name should be the same in 
            // different games
            //System.Random rng = new System.Random(zoneSeed);
           //int rollA = rng.Next(0, 2);      // 0 or 1
            //int rollB = rng.Next(1, 101);    // 1 to 100
            //int bucket = Math.Abs(zoneSeed % 100);
            //if (bucket < 50)
            //{
            //    //inject
            //}


            return true;
        }
    }
    
}