using HarmonyLib;
using Il2CppSystem.Text;

namespace Polus.Patches.Temporary {
    [HarmonyPatch(typeof(ImportantTextTask), nameof(ImportantTextTask.AppendTaskText))]
    public class ITTAppendLinePatch {
        [HarmonyPrefix]
        public static bool Prefix([HarmonyArgument(0)] StringBuilder sb) {
            sb.Clear();
            return false;
        }
    }
}