//v1.0.3
using System;
using System.Reflection;
using XRL;
using XRL.World;
using XRL.World.WorldBuilders;
using XRL.UI;

/*
Subterranean Sites external exclusion template

Use this file if your Caves of Qud mod adds a fixed or semi-fixed underground site
and you want Subterranean Sites to avoid generating paths or sites through it.

How to use:
1. Copy this .cs file into your mod's Scripting folder.
2. Edit RegisterExclusions() below.
3. Add one or more protected locations using the examples.
4. Ship this file with your mod.

This file is safe to include even when Subterranean Sites is not installed.
It looks for the Subterranean Sites compatibility API at runtime; if the API is not
found, it quietly does nothing and your mod continues normally.

Use RegisterProtectedZoneColumn when your site occupies one specific zone column.

Use RegisterProtectedParasangColumn when your site may occupy multiple zones in a
parasang, or when you want a broader safety exclusion.

Coordinate format:
- world: usually "JoppaWorld"
- parasangX, parasangY: the world-map parasang coordinate
- zoneX, zoneY: the local zone inside the parasang, from 0 to 2
- minZ, maxZ: depth range, where surface is Z10 and deeper underground increases Z

Leave the example registrations commented out unless you want them active.
*/
namespace SubterraneanSitesExternalExclusions
{
    [JoppaWorldBuilderExtension]
    public class ExternalSubterraneanSitesExclusionsWorldBuilder :
        IJoppaWorldBuilderExtension
    {
        public override void OnAfterBuild(JoppaWorldBuilder builder)
        {
            ExternalSubterraneanSitesExclusions.RegisterExclusions();
        }
    }

    [HasCallAfterGameLoaded]
    public static class ExternalSubterraneanSitesExclusions
    {
        [CallAfterGameLoaded]
        public static void AfterGameLoaded()
        {
            RegisterExclusions();
        }

        public static void RegisterExclusions()
        {
            Type compatType = FindSubterraneanSitesCompatType();

            if (compatType == null)
            {
                return;
            }

            // Example: protect one exact zone column.
            // Use this when your site is centered on one zone inside a parasang.
            //RegisterProtectedZoneColumn(
            //    compatType,
            //    "External exclusion test: zone 1 west of Joppa",
            //    "JoppaWorld",
            //    11, 22,
            //    0, 1,
            //    10, 110
            //);

            // Example: protect a whole parasang column.
            // Use this when your site may occupy several zones in the parasang.
            //RegisterProtectedParasangColumn(
            //    compatType,
            //    "External exclusion test: parasang north of Joppa's parasang",
            //    "JoppaWorld",
            //    11, 21,
            //    10, 110
            //);

            //Popup.Show("External Subterranean Sites exclusions registered.");

        }

        private static Type FindSubterraneanSitesCompatType()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly assembly in assemblies)
            {
                Type type = assembly.GetType(
                    "SubterraneanSites.SubterraneanSitesCompat",
                    false
                );

                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }

        private static void RegisterProtectedZoneColumn(
            Type compatType,
            string name,
            string world,
            int parasangX,
            int parasangY,
            int zoneX,
            int zoneY,
            int minZ,
            int maxZ
        )
        {
            MethodInfo method = compatType.GetMethod(
                "RegisterProtectedZoneColumn",
                BindingFlags.Public | BindingFlags.Static
            );

            if (method == null)
            {
                return;
            }

            method.Invoke(
                null,
                new object[]
                {
                    name,
                    world,
                    parasangX,
                    parasangY,
                    zoneX,
                    zoneY,
                    minZ,
                    maxZ
                }
            );
        }

        private static void RegisterProtectedParasangColumn(
            Type compatType,
            string name,
            string world,
            int parasangX,
            int parasangY,
            int minZ,
            int maxZ
        )
        {
            MethodInfo method = compatType.GetMethod(
                "RegisterProtectedParasangColumn",
                BindingFlags.Public | BindingFlags.Static
            );

            if (method == null)
            {
                return;
            }

            method.Invoke(
                null,
                new object[]
                {
                    name,
                    world,
                    parasangX,
                    parasangY,
                    minZ,
                    maxZ
                }
            );
        }
    }
}