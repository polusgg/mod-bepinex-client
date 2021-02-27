using System.Linq;
using HarmonyLib;
using Hazel;

namespace PolusGGMod.Patches {
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    public class MainMenuStartPatch {
        private static bool hasStarted = false;
        [PermanentPatch]
        [HarmonyPrefix]
        public static bool Start() {
            if (hasStarted) return true;
            hasStarted = true;
            ServerManager.DefaultRegions = ServerManager.DefaultRegions.Append(PggConstants.Region).ToArray();
            ServerManager._instance.CurrentRegion = PggConstants.Region;
            ServerManager._instance.SaveServers();
            return true;
        }
    }
}