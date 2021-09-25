using HarmonyLib;
using Polus.Mods.Patching;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Polus.Patches.Permanent {
    [HarmonyPatch(typeof(ChatBubble), nameof(ChatBubble.SetText))]
    public class ChatBubblePatch {
        [PermanentPatch]
        [HarmonyPostfix]
        public static void Update(ChatBubble __instance) {
            __instance.TextArea.richText = true;
        }
    }
}