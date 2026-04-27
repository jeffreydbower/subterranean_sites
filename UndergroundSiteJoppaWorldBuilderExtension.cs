using XRL;
using XRL.UI;
using XRL.World;
using XRL.World.WorldBuilders;

namespace SubterraneanSites
{
    [JoppaWorldBuilderExtension]
    public class UndergroundSiteJoppaWorldBuilderExtension : IJoppaWorldBuilderExtension
    {
        public override void OnAfterBuild(JoppaWorldBuilder builder)
        {
            
            Popup.Show("Subterranean Sites loaded.");

            string targetZoneId = "JoppaWorld.11.22.0.1.11";

            The.ZoneManager.SetZoneName(
                targetZoneId,
                "test runtime underground site",
                Proper: false
            );

            The.Game.RequireSystem<ZoneEntryBuilderInjectionSystem>();
        }
    }

    public class ZoneEntryBuilderInjectionSystem : IGameSystem
    {
        public override void Register(XRLGame game, IEventRegistrar registrar)
        {
            registrar.Register(BeforeZoneBuiltEvent.ID);
        }

        public override bool HandleEvent(BeforeZoneBuiltEvent zoneEvent)
        {
            string targetZoneId = "JoppaWorld.11.22.0.1.11";

            if (zoneEvent.Zone.ZoneID == targetZoneId)
            {
                The.ZoneManager.SetZoneName(
                    targetZoneId,
                    "before zone built hook fired",
                    Proper: false
                );

                ZoneManager.ApplyBuilderToZone(
                    "SnapjawStockadeMaker",
                    zoneEvent.Zone
                );
            }

            return true;
        }
    }
}