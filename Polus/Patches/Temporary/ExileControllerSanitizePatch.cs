using HarmonyLib;

namespace Polus.Patches.Temporary {
    [HarmonyPatch(typeof(ExileController), nameof(ExileController.Begin))]
    public class ExileControllerSanitizePatch {
        [HarmonyPostfix]
        public static void Postfix(ExileController __instance) {
            var inTag = false;
            var finalString = "";

            foreach (char c in __instance.completeString)
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

            __instance.completeString = finalString;
        }
    }
}