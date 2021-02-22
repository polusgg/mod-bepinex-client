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
            
            MessageReader reader = MessageReader.GetSized(0);
            
            {
				AmongUsClient.Instance.Method_16(reader, 0);
            }
            
            hasStarted = true;
            ServerManager._instance.CurrentRegion = PggConstants.Region;
            ServerManager._instance.SaveServers();
            return true;
        }
    }
}