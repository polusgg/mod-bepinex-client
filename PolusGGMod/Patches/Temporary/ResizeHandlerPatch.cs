using System;
using System.Threading;
using HarmonyLib;
using Hazel;
using PolusGG.Enums;
using UnityEngine;

namespace PolusGG {
    [HarmonyPatch(typeof(ResolutionManager), nameof(ResolutionManager.SetResolution))]
    public static class ResizeHandlerPatch {
        [HarmonyPrefix]
        public static void SetResolution([HarmonyArgument(0)] int width, [HarmonyArgument(1)] int height) {
            MessageWriter writer = MessageWriter.Get(SendOption.Reliable);
            writer.StartMessage((byte) PolusRootPackets.Resize);
            writer.WritePacked(width);
            writer.WritePacked(height);
            writer.EndMessage();
            AmongUsClient.Instance.SendOrDisconnect(writer);
        }
    }
}