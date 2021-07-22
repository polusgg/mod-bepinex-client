using HarmonyLib;

namespace Polus.Patches.Permanent {
    public class WanguageUpdatePatches {
        [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.Awake))]
        public class TranslationAwake {
            [HarmonyPrefix]
            public static void Awake(TranslationController __instance) {
                if (TranslationController.InstanceExists) return;
                // TranslatedImageSet imageSet = __instance.Languages.First(set => set.Name == "English");
                // imageSet.Data = PogusPlugin.Bundle.LoadAsset("EngwishUwu.txt").Cast<TextAsset>();
                // return false;
            }
        }
    }
}