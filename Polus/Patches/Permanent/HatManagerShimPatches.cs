using HarmonyLib;
using Polus.Behaviours;
using Polus.Extensions;
using Polus.Mods.Patching;
using UnhollowerBaseLib;

namespace Polus.Patches.Permanent {
    //patches to funnel HatManager calls into Polus.Behaviours.CosmeticManager
    public class HatManagerShimPatches {
        [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.CheckAddOns))]
        public static class MMCheckAddons {
            [PermanentPatch]
            [HarmonyPrefix]
            public static bool CheckAddons() {
                CosmeticManager.Instance.AddAllOwnedVanillaCosmetics();
                return false;
            }
        }

        public static class OverwritingHatManagerPatches {
            [HarmonyPatch(typeof(HatManager), nameof(HatManager.GetUnlockedHats))]
            public static class GetUnlockedHatsPatch {
                [PermanentPatch]
                [HarmonyPrefix]
                public static bool GetUnlockedHats(out Il2CppReferenceArray<HatBehaviour> __result) {
                    __result = CosmeticManager.Instance.GetOwnedHats();
                    return false;
                } 
            }

            [HarmonyPatch(typeof(HatManager), nameof(HatManager.GetUnlockedPets))]
            public static class GetUnlockedPetsPatch {
                [PermanentPatch]
                [HarmonyPrefix]
                public static bool GetUnlockedPets(out Il2CppReferenceArray<PetBehaviour> __result) {
                    __result = CosmeticManager.Instance.GetOwnedPets();
                    return false;
                } 
            }

            [HarmonyPatch(typeof(HatManager), nameof(HatManager.GetUnlockedSkins))]
            public static class GetUnlockedSkinsPatch {
                [PermanentPatch]
                [HarmonyPrefix]
                public static bool GetUnlockedSkins(out Il2CppReferenceArray<SkinData> __result) {
                    __result = CosmeticManager.Instance.GetOwnedSkins();
                    return false;
                } 
            }
            [HarmonyPatch(typeof(HatManager), nameof(HatManager.GetHatById))]
            public static class GetHatByIdPatch {
                [PermanentPatch]
                [HarmonyPrefix]
                public static bool GetHatById([HarmonyArgument(0)] uint hatId, out HatBehaviour __result) {
                    __result = CosmeticManager.Instance.GetHatById(hatId);
                    return false;
                } 
            }

            [HarmonyPatch(typeof(HatManager), nameof(HatManager.GetPetById))]
            public static class GetPetByIdPatch {
                [PermanentPatch]
                [HarmonyPrefix]
                public static bool GetPetById([HarmonyArgument(0)] uint petId, out PetBehaviour __result) {
                    __result = CosmeticManager.Instance.GetPetById(petId);
                    return false;
                } 
            }

            [HarmonyPatch(typeof(HatManager), nameof(HatManager.GetSkinById))]
            public static class GetSkinByIdPatch {
                [PermanentPatch]
                [HarmonyPrefix]
                public static bool GetSkinById([HarmonyArgument(0)] uint skinId, out SkinData __result) {
                    __result = CosmeticManager.Instance.GetSkinById(skinId);
                    return false;
                } 
            }
            [HarmonyPatch(typeof(HatManager), nameof(HatManager.GetIdFromHat))]
            public static class GetIdFromHatPatch {
                [PermanentPatch]
                [HarmonyPrefix]
                public static bool GetIdFromHat([HarmonyArgument(0)] HatBehaviour data, out uint __result) {
                    __result = CosmeticManager.Instance.GetIdByHat(data);
                    return false;
                } 
            }

            [HarmonyPatch(typeof(HatManager), nameof(HatManager.GetIdFromPet))]
            public static class GetIdFromPetPatch {
                [PermanentPatch]
                [HarmonyPrefix]
                public static bool GetIdFromPet([HarmonyArgument(0)] PetBehaviour data, out uint __result) {
                    __result = CosmeticManager.Instance.GetIdByPet(data);
                    return false;
                } 
            }

            [HarmonyPatch(typeof(HatManager), nameof(HatManager.GetIdFromSkin))]
            public static class GetIdFromSkinPatch {
                [PermanentPatch]
                [HarmonyPrefix]
                public static bool GetIdFromSkin([HarmonyArgument(0)] SkinData data, out uint __result) {
                    __result = CosmeticManager.Instance.GetIdBySkin(data);
                    return false;
                } 
            }
        }
    }
}