using HarmonyLib;
using Hazel;
using InnerNet;
using UnityEngine;

namespace Polus.Patches.Temporary {
    public class ServerMigrationPatches {
        [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.HostGame))]
        public static class HostGamePacketChange {
            public static bool Migrated = false;
            [HarmonyPrefix]
            public static bool HostGame(InnerNetClient __instance, [HarmonyArgument(0)] GameOptionsData options) {
                __instance.IsGamePublic = false;
                MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable);
                messageWriter.StartMessage(Tags.HostGame);
                messageWriter.WriteBytesAndSize(options.ToBytes(2));
                messageWriter.Write((byte)SaveManager.ChatModeType);
                messageWriter.Write(Migrated);
                messageWriter.EndMessage();
                __instance.SendOrDisconnect(messageWriter);
                messageWriter.Recycle();
                Debug.Log("Client requesting new game.");
                return false;
            }
        }
    }
}