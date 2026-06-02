using System.Collections.Generic;

namespace SubterraneanSites
{
    internal static class SubterraneanProtectedLocations
    {
        public static readonly List<ProtectedZoneColumn> Columns =
            new List<ProtectedZoneColumn>
            {
                // Joppa / Waterlogged Tunnel column.
                // Z 10 = Joppa surface.
                // Z 11-14 = Waterlogged Tunnel, -1 through -4.
                new ProtectedZoneColumn(
                    "Joppa / Waterlogged Tunnel column",
                    "JoppaWorld",
                    11,
                    22,
                    1,
                    1,
                    10,
                    14
                ),

                new ProtectedZoneColumn(
                    "Grit Gate",
                    "JoppaWorld",
                    22,
                    14,
                    1,
                    0,
                    10,
                    30
                ),

                new ProtectedZoneColumn(
                    "Ezra",
                    "JoppaWorld",
                    53,
                    4,
                    0,
                    0,
                    10,
                    11
                ),

                new ProtectedZoneColumn(
                    "Spindle",
                    "JoppaWorld",
                    53,
                    3,
                    1,
                    1,
                    10,
                    11
                ),

                new ProtectedZoneColumn(
                    "Golgotha / Agolgot",
                    "JoppaWorld",
                    23,
                    9,
                    1,
                    1,
                    10,
                    46
                ),

                new ProtectedZoneColumn(
                    "Bethesda Susa / Temple Rock / Bethsaida",
                    "JoppaWorld",
                    25,
                    3,
                    1,
                    1,
                    10,
                    46
                ),

                new ProtectedZoneColumn(
                    "Court of the Sultans",
                    "JoppaWorld",
                    53,
                    4,
                    1,
                    1,
                    10,
                    11
                ),

                new ProtectedZoneColumn(
                    "Redrock",
                    "JoppaWorld",
                    11,
                    20,
                    1,
                    1,
                    10,
                    14
                ),

                new ProtectedZoneColumn(
                    "Rustwell 1",
                    "JoppaWorld",
                    16,
                    22,
                    1,
                    1,
                    10,
                    13
                ),

                new ProtectedZoneColumn(
                    "Rustwell 2",
                    "JoppaWorld",
                    17,
                    23,
                    1,
                    1,
                    10,
                    13
                ),

                new ProtectedZoneColumn(
                    "Rustwell 3",
                    "JoppaWorld",
                    16,
                    24,
                    1,
                    1,
                    10,
                    13
                ),

                new ProtectedZoneColumn(
                    "Rusted Archway",
                    "JoppaWorld",
                    16,
                    24,
                    1,
                    1,
                    10,
                    14
                ),

                new ProtectedZoneColumn(
                    "Kyakukya",
                    "JoppaWorld",
                    27,
                    20,
                    1,
                    1,
                    10,
                    11
                ),

                new ProtectedZoneColumn(
                    "YD",
                    "JoppaWorld",
                    67,
                    17,
                    1,
                    1,
                    10,
                    11
                ),

                new ProtectedZoneColumn(
                    "Eyn Roj",
                    "JoppaWorld",
                    76,
                    5,
                    1,
                    1,
                    10,
                    50
                )
            };

        public static readonly List<ProtectedParasangColumn> ParasangColumns =
            new List<ProtectedParasangColumn>
            {
                new ProtectedParasangColumn(
                    "Stiltgrounds",
                    "JoppaWorld",
                    5,
                    2,
                    10,
                    11
                ),

                new ProtectedParasangColumn(
                    "Grit Gate Entrance - Center",
                    "JoppaWorld",
                    22,
                    14,
                    10,
                    30
                ),

                new ProtectedParasangColumn(
                    "Tomb of the Eaters",
                    "JoppaWorld",
                    53,
                    3,
                    10,
                    15
                ),

                new ProtectedParasangColumn(
                    "Asphalt Mines",
                    "JoppaWorld",
                    11,
                    0,
                    10,
                    60
                )
            };
    }
}