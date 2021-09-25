using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using BepInEx.Logging;
using HarmonyLib;
using Polus.Mods.Patching;
using TMPro;

namespace Polus.Patches.Permanent {
    [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
    public class StereotypicalClientModderVersionShowerPatch {
        public static (DateTime date, int? packageVersion) Ver => (GetBuildDate(typeof(StereotypicalClientModderVersionShowerPatch).Assembly), PogusPlugin.Revision);
        [PermanentPatch]
        [HarmonyPostfix]
        public static void Start(VersionShower __instance) {
            __instance.text.alignment = TextAlignmentOptions.TopLeft;
            __instance.text.text = "<color=#FF7E7E>Among Us</color> " + __instance.text.text;
            (DateTime date, int? packageVersion) = Ver;
            __instance.text.text +=
                $"\n<color=#B77EFF>Polus.gg</color> v{date.Year}.{date.Month}.{date.Day}:{(packageVersion.HasValue ? packageVersion.Value : "?")}";
            __instance.text.text += $"\n<size=75%>{(PogusPlugin.ModManager.AllPatched ? "<color=#0A9D34>Successfully loaded!" : "<color=#FF7E7E>Failed to load!")}</color></size>";
        }

        private static DateTime GetBuildDate(Assembly assembly) {
            const string BuildVersionMetadataPrefix = "+";

            AssemblyInformationalVersionAttribute attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            if (attribute?.InformationalVersion != null) {
                string value = attribute.InformationalVersion;
                int index = value.IndexOf(BuildVersionMetadataPrefix, StringComparison.Ordinal);
                if (index > 0) {
                    value = value.Substring(index + BuildVersionMetadataPrefix.Length);
                    if (DateTime.TryParseExact(value, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result)) {
                        return result;
                    }
                }
            }

            return default;
        }
    }
}