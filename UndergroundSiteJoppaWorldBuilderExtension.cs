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
        private const string TargetZoneName = "subterranean test site";
        private const string ZoneBuilderName = "SnapjawStockadeMaker";

        public override void Register(XRLGame game, IEventRegistrar registrar)
        {
            // BeforeZoneBuiltEvent fires early enough to mutate the zone directly.
            // AddZoneBuilder is too late here because the zone builder list has already been consumed.
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

            // Apply the builder directly to the zone currently being built.
            ZoneManager.ApplyBuilderToZone(
                ZoneBuilderName,
                zoneBuildEvent.Zone
            );

            return true;
        }
    }
}