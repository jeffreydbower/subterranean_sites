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
            // The system will apply the target zone builder when the zone is first built.
            The.Game.RequireSystem<RuntimeZoneBuilderInjectionSystem>();
        }
    }

    public class RuntimeZoneBuilderInjectionSystem : IGameSystem
    {
        private const string TargetZoneId = "JoppaWorld.11.22.0.1.11";
        private const string TargetZoneName = "banana grove test";
        //private const string ZoneBuilderName = "SnapjawStockadeMaker";

        public override void Register(XRLGame game, IEventRegistrar registrar)
        {
            // BeforeZoneBuiltEvent fires early enough to mutate the zone directly.
            registrar.Register(BeforeZoneBuiltEvent.ID);
        }

        public override bool HandleEvent(BeforeZoneBuiltEvent zoneBuildEvent)
        {
            if (zoneBuildEvent.Zone.ZoneID != TargetZoneId)
            {
                return true;
            }

            // Set a readable test name so the target zone is easy to confirm in-game.
            The.ZoneManager.SetZoneName(
                TargetZoneId,
                TargetZoneName,
                Proper: false
            );

            // --- ORIGINAL (commented out for this test) ---
            /*
            ZoneManager.ApplyBuilderToZone(
                ZoneBuilderName,
                zoneBuildEvent.Zone
            );
            */

            // --- TEST 1: Direct BananaGrove builder ---
            var builder = new XRL.World.ZoneBuilders.BananaGrove();
            builder.Underground = true; // enable underground behavior
            builder.BuildZone(zoneBuildEvent.Zone);

            return true;
        }
    }
}