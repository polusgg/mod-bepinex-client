using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Polus.Extensions;
using UnhollowerRuntimeLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Polus.Behaviours {
    public class CosmeticManager : MonoBehaviour {
        static CosmeticManager() {
            ClassInjector.RegisterTypeInIl2Cpp<CosmeticManager>();
        }
        public CosmeticManager(IntPtr ptr) : base(ptr) {}
        private HatManager manager;
        private Dictionary<uint, HatBehaviour> Hats = new();
        private Dictionary<uint, PetBehaviour> Pets = new();
        private Dictionary<uint, SkinData> Skins = new();

        public static bool InstanceExists => HatManager.InstanceExists;
        // unreadable :)
        public static CosmeticManager Instance => InstanceExists ? HatManager.Instance.TryGetComponent(UnhollowerRuntimeLib.Il2CppType.Of<CosmeticManager>(), out Component component) ? component.Cast<CosmeticManager>() : HatManager.Instance.gameObject.AddComponent<CosmeticManager>().Initialize() : null;

        private CosmeticManager Initialize() {
            manager = GetComponent<HatManager>();

            for (int i = 0; i < manager.AllHats.Count; i++) {
                (Hats[(uint) i] = manager.AllHats[(Index) i].Cast<HatBehaviour>()).name.Log(comment: $"hat at {i}");
            }

            for (int i = 0; i < manager.AllPets.Count; i++) {
                (Pets[(uint) i] = manager.AllPets[(Index) i].Cast<PetBehaviour>()).name.Log(comment: $"pet at {i}");
            }

            for (int i = 0; i < manager.AllSkins.Count; i++) {
                (Skins[(uint) i] = manager.AllSkins[(Index) i].Cast<SkinData>()).name.Log(comment: $"skin at {i}");
            }

            return this;
        }

        public void Reset() {
            Hats = Hats.Where(kvp => kvp.Key < 10000000).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            Pets = Pets.Where(kvp => kvp.Key < 10000000).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            Skins = Skins.Where(kvp => kvp.Key < 10000000).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public HatBehaviour GetHatById(uint hatId) => Hats.TryGetValue(hatId, out HatBehaviour hat) ? hat : manager.NoneHat;
        public PetBehaviour GetPetById(uint petId) => Pets.TryGetValue(petId, out PetBehaviour pet) ? pet : Pets[0];
        public SkinData GetSkinById(uint skinId) => Skins.TryGetValue(skinId, out SkinData skin) ? skin : Skins[0];

        public uint GetIdByHat(HatBehaviour hat) {
            return Hats.All(x => x.Value.Pointer != hat.Pointer) ? 0 : Hats.First(x => x.Value.Pointer == hat.Pointer).Key;
        }

        public uint GetIdByPet(PetBehaviour pet) {
            if (Pets.All(x => x.Value.name != pet.name))
                return 0;
            return Pets.First(x => {
                $"test {x.Value.name} vs {pet.name}".Log();
                return x.Value.name == pet.name;
            }).Key.Log();
        }

        [HarmonyPatch(typeof(PetsTab), nameof(PetsTab.OnEnable))]
        public static class TestOnEnable {
            [HarmonyPrefix]
            public static bool Prefix(PetsTab __instance) {
                PlayerControl.SetPlayerMaterialColors(PlayerControl.LocalPlayer.Data.ColorId, __instance.DemoImage);
                __instance.HatImage.SetHat(SaveManager.LastHat, PlayerControl.LocalPlayer.Data.ColorId);
                PlayerControl.SetSkinImage(SaveManager.LastSkin, __instance.SkinImage);
                PlayerControl.SetPetImage(SaveManager.LastPet, PlayerControl.LocalPlayer.Data.ColorId, __instance.PetImage);
                PetBehaviour[] unlockedPets = DestroyableSingleton<HatManager>.Instance.GetUnlockedPets();
                for (int i = 0; i < unlockedPets.Length; i++) {
                    PetBehaviour pet = unlockedPets[i];
                    float num = __instance.XRange.Lerp((float) (i % __instance.NumPerRow) / ((float) __instance.NumPerRow - 1f));
                    float num2 = __instance.YStart - (float) (i / __instance.NumPerRow) * __instance.YOffset;
                    ColorChip chip = Instantiate(__instance.ColorTabPrefab, __instance.scroller.Inner);
                    chip.transform.localPosition = new Vector3(num, num2, -1f);
                    chip.InUseForeground.SetActive(DestroyableSingleton<HatManager>.Instance.GetIdFromPet(pet) == SaveManager.LastPet);
                    chip.Button.OnClick.AddListener((Action) (() => __instance.SelectPet(chip, pet)));
                    PlayerControl.SetPetImage(pet, PlayerControl.LocalPlayer.Data.ColorId, chip.Inner.FrontLayer);
                    __instance.ColorChips.Add(chip);
                }

                __instance.scroller.YBounds.max = -(__instance.YStart - unlockedPets.Length / __instance.NumPerRow * __instance.YOffset) - 3f;

                return false;
            }
        }

        public uint GetIdBySkin(SkinData skin) {
            return Skins.All(x => x.Value.Pointer != skin.Pointer) ? 0 : Skins.First(x => x.Value.Pointer == skin.Pointer).Key;
        }

        public HatBehaviour[] GetOwnedHats() => Hats.Select(kvp => kvp.Value).Where(h => !HatManager.IsMapStuff(h.ProdId) && h.LimitedMonth == 0 || SaveManager.GetPurchase(h.ProductId)).OrderByDescending(o => o.Order).ThenBy(o => o.name).ToArray();
        public PetBehaviour[] GetOwnedPets() => Pets.Select(kvp => kvp.Value).Where(h => h.Free || SaveManager.GetPurchase(h.ProductId)).ToArray();
        public SkinData[] GetOwnedSkins() => Skins.Select(kvp => kvp.Value).Where(s => !HatManager.IsMapStuff(s.ProdId) || SaveManager.GetPurchase(s.ProdId)).OrderByDescending(o => o.Order).ThenBy(o => o.name).ToArray();

        public void SetHat(uint id, HatBehaviour behaviour) {
            $"{behaviour.name} hat at {id}".Log();
            Hats[id] = behaviour;
        }

        public void SetPet(uint id, PetBehaviour behaviour) {
            $"{behaviour.name} pet at {id}".Log();
            Pets[id] = behaviour;
        }

        public void SetSkin(uint id, SkinData behaviour) {
            $"{behaviour.name} skin at {id}".Log();
            Skins[id] = behaviour;
        }

        public void AddAllOwnedVanillaCosmetics() {
            DateTime utcNow = DateTime.UtcNow;
            for (uint i = 0; i < Hats.Count; i++) {
                HatBehaviour hatBehaviour = Hats[i].Cast<HatBehaviour>();
                if (!hatBehaviour.ProdId.StartsWith("pet_") && (hatBehaviour.LimitedMonth == utcNow.Month || hatBehaviour.LimitedMonth == 0) && (hatBehaviour.LimitedYear == utcNow.Year || hatBehaviour.LimitedYear == 0) && !hatBehaviour.NotInStore && !HatManager.IsMapStuff(hatBehaviour.ProdId) && !SaveManager.GetPurchase(hatBehaviour.ProductId)) {
                    SaveManager.SetPurchased(hatBehaviour.ProductId);
                }
            }
        }
    }
}