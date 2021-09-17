using HarmonyLib;
using Polus.Mods.Patching;

namespace Polus.Patches.Permanent {
    [HarmonyPatch(typeof(DisconnectPopup), nameof(DisconnectPopup.DoShow))]
    public class DisconnectDisplayAddressPatch {
        [PermanentPatch]
        [HarmonyPostfix]
        public static void DoShow(DisconnectPopup __instance) {
            __instance.TextArea.text += $"\n(Server location: {AmongUsClient.Instance.networkAddress}:{AmongUsClient.Instance.networkPort})";
        }
    }
}