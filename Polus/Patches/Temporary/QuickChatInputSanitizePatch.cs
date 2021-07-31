using System;
using HarmonyLib;

namespace Polus.Patches.Temporary {
    [HarmonyPatch(typeof(QuickChatMenu), nameof(QuickChatMenu.QuickChat), new Type[] {typeof(QuickChatMenuItem)})]
    public class QuickChatInputSanitizePatch {
        [HarmonyPostfix]
        public static void Postfix(QuickChatMenu __instance) {
            if (__instance.targetTextBox.text.Contains("<") || __instance.targetTextBox.text.Contains(">"))
            {
                var inTag = false;
                var finalString = "";

                foreach (char c in __instance.targetTextBox.text)
                {
                    if (c == '>' && inTag)
                    {
                        inTag = false;
                        continue;
                    }
                    if (c == '<' || inTag)
                    {
                        inTag = true;
                        continue;
                    }
                    finalString = finalString.Insert(finalString.Length, c.ToString());
                }

                __instance.targetTextBox.text = finalString;
            }
        }
    }
}