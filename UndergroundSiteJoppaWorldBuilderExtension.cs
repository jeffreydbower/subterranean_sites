using XRL;
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
        private const string TargetZoneName = "basic lair default clean test";

        public override void Register(XRLGame game, IEventRegistrar registrar)
        {
            registrar.Register(BeforeZoneBuiltEvent.ID);
        }

        public override bool HandleEvent(BeforeZoneBuiltEvent zoneBuildEvent)
        {
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

            // --- Basic Lair Test Defaults Clear Screen ---
            var Z = zoneBuildEvent.Zone;
            // Clear all cells (terrain + objects)
            foreach (var cell in Z.GetCells())
            {
                cell.Clear();
            }
            var lair = new XRL.World.ZoneBuilders.BasicLair();
            //lair.Table = "";        // default
            lair.Table = "GenericLairOwner";        // test arguments
            lair.Adjectives = "";   // no modifiers
            lair.Stairs = "";       // no stairs
            lair.BuildZone(Z);

            return true;
        }
    }
}