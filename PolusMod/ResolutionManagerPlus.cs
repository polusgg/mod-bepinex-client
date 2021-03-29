using System;
using System.Threading;
using Hazel;
using PolusMod.Enums;
using UnityEngine;

namespace PolusMod.Patches {
    public class ResolutionManagerPlus {
        public static void Resolution() {
            Action<float> resolutionChanged = f => global::ResolutionManager.ResolutionChanged.Invoke(f);
            Action<float> action2;
            Action<float> value = f => {
                MessageWriter writer = MessageWriter.Get(SendOption.Reliable);
                writer.StartMessage((byte) PolusRootPackets.Resize);
                writer.WritePacked(Screen.width);
                writer.WritePacked(Screen.height);
                writer.EndMessage();
                AmongUsClient.Instance.SendOrDisconnect(writer);
            };
            do {
                action2 = resolutionChanged;
                Action<float> value2 = (Action<float>)Delegate.Combine(action2, value);
                resolutionChanged = Interlocked.CompareExchange(ref resolutionChanged, value2, action2);
            }
            while (resolutionChanged != action2);
        }
    }
}