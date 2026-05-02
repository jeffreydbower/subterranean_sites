using System;
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
        //private const string TargetZoneId = "JoppaWorld.11.22.0.1.11";
        //private const string TargetZoneName = "basic lair default clean test";

        public override void Register(XRLGame game, IEventRegistrar registrar)
        {
            registrar.Register(BeforeZoneBuiltEvent.ID);
        }

        public override bool HandleEvent(BeforeZoneBuiltEvent zoneBuildEvent)
        {
            var Z = zoneBuildEvent.Zone;

            // Only test underground zones.
            if (Z.Z <= 10)
            {
                return true;
            }

            int zoneSeed = StableHash(Z.ZoneID);

            //int rawSeed = XRLCore.Core.Game.GetWorldSeed();
            //int hash = XRLCore.Core.Game.GetWorldSeed(Z.ZoneID + rawSeed);

            System.Random rng = new System.Random(zoneSeed);


            //Test for deterministic randon numbers
            //will repeat 2 new games 
            //if we use the zoneid as the rng seed
            //then the rolls and name should be the same in 
            // different games
            int rollA = rng.Next(0, 2);      // 0 or 1
            int rollB = rng.Next(1, 101);    // 1 to 100

            int bucket = Math.Abs(zoneSeed % 100);


            if (bucket < 50)
            {
                //return true;
                foreach (var cell in Z.GetCells())
                {
                    cell.Clear();
                }

                The.ZoneManager.SetZoneName
                (
                     Z.ZoneID,
                    "rng test zoneSeed " + zoneSeed + " A" + rollA + " B" + rollB,
                    Proper: false
                );


                var lair = new XRL.World.ZoneBuilders.BasicLair();
                 //lair.Table = "";        // default
                lair.Table = "DynamicInheritsTable:Creature:Tier5";        // test creature table 
                lair.Adjectives = "";   // test adjetive
                lair.Stairs = "D";       // down stairs stairs
                lair.BuildZone(Z);

                var factionEncounters = new XRL.World.ZoneBuilders.FactionEncounters();
                factionEncounters.Chance = 100;
                factionEncounters.Rolls = 1;
                factionEncounters.Population = "GenericFactionPopulation";
                factionEncounters.BuildZone(Z);
                return true;
            }

  

            /*
            if (zoneBuildEvent.Zone.ZoneID != TargetZoneId)
            {
                return true;
            }

            // Set test name
            The.ZoneManager.SetZoneName(
                TargetZoneId,
                TargetZoneName,
                Proper: false
            );
            */

            // --- PATTERN: String-based builder invocation ---
            // Use this for builders that:
            // - do not require parameters
            // - are designed to be invoked by name
            // Example: SnapjawStockadeMaker
            /*
            ZoneManager.ApplyBuilderToZone(
                "SnapjawStockadeMaker",
                zoneBuildEvent.Zone
            );
            */

            // --- TEST 1 (overlay BananaGrove) ---
            /*
            var builder = new XRL.World.ZoneBuilders.BananaGrove();
            builder.Underground = true;
            builder.BuildZone(zoneBuildEvent.Zone);
            
            // --- TEST 2: clear zone first, then apply BananaGrove ---
            var Z = zoneBuildEvent.Zone;
            // Clear all cells (terrain + objects)
            foreach (var cell in Z.GetCells())
            {
                cell.Clear();
            }
            var cleanBuilder = new XRL.World.ZoneBuilders.BananaGrove();
            cleanBuilder.Underground = true;
            cleanBuilder.BuildZone(Z);
            */
/*
            // --- Basic Lair Test Defaults Clear Screen ---
            var Z = zoneBuildEvent.Zone;
            // Clear all cells (terrain + objects)
            foreach (var cell in Z.GetCells())
            {
                cell.Clear();
            }
            var lair = new XRL.World.ZoneBuilders.BasicLair();
            //lair.Table = "";        // default
            lair.Table = "DynamicInheritsTable:Creature:Tier5";        // test creature table 
            lair.Adjectives = "workshop";   // test adjetive
            lair.Stairs = "D";       // down stairs stairs
            lair.BuildZone(Z);

            var factionEncounters = new XRL.World.ZoneBuilders.FactionEncounters();
            factionEncounters.Chance = 100;
            factionEncounters.Rolls = 1;
            factionEncounters.Population = "GenericFactionPopulation";
            factionEncounters.BuildZone(Z);
*/
            return true;
        }

        private static int StableHash(string text)
        {
            unchecked
            {
                int hash = 23;
                foreach (char c in text)
                {
                    hash = hash * 31 + c;
                }
                return hash & 0x7fffffff;
            }
        }

    }
}