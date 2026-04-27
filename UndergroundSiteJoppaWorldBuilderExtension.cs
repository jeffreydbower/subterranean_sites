using XRL;
using XRL.World;
using XRL.World.WorldBuilders;

namespace UndergroundSiteDev
{
    [JoppaWorldBuilderExtension]
    public class UndergroundSiteJoppaWorldBuilderExtension : IJoppaWorldBuilderExtension
    {
        public override void OnAfterBuild(JoppaWorldBuilder builder)
        {
            string zoneID = "JoppaWorld.11.22.0.1.11";

            The.ZoneManager.AddZoneBuilder(zoneID, 6000, "Mines2");

            The.ZoneManager.SetZoneName(
                zoneID,
                "test sultan dungeon",
                Proper: false
            );
        }
    }
}