using HarmonyLib;
using Hazel;
using Polus.Enums;

namespace Polus.Patches.Temporary {
    [HarmonyPatch(typeof(ResolutionManager), nameof(ResolutionManager.SetResolution))]
    public static class ResizeHandlerPatch {
        [HarmonyPrefix]
        public static void SetResolution([HarmonyArgument(0)] int width, [HarmonyArgument(1)] int height) {
            if (!AmongUsClient.Instance || !AmongUsClient.Instance.AmConnected) return;
            MessageWriter writer = MessageWriter.Get(SendOption.Reliable);
            writer.StartMessage((byte) PolusRootPackets.Resize);
            writer.WritePacked(width);
            writer.WritePacked(height);
            writer.EndMessage();
            PolusMod.AddDispatch(() => {
                AmongUsClient.Instance.SendOrDisconnect(writer);
                writer.Recycle();
            });
        }
    }
}