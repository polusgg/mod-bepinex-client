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
        private Dictionary<uint, bool> Owned = new();

        public static bool InstanceExists => HatManager.InstanceExists;
        // unreadable :)
        public static CosmeticManager Instance => InstanceExists ? HatManager.Instance.TryGetComponent(UnhollowerRuntimeLib.Il2CppType.Of<CosmeticManager>(), out Component component) ? component.Cast<CosmeticManager>() : HatManager.Instance.gameObject.AddComponent<CosmeticManager>().Initialize() : null;

        private CosmeticManager Initialize() {
            manager = GetComponent<HatManager>();

            for (int i = 0; i < manager.AllHats.Count; i++) {
                Hats[(uint) i] = manager.AllHats[(Index) i].Cast<HatBehaviour>();
            }

            for (int i = 0; i < manager.AllPets.Count; i++) {
                Pets[(uint) i] = manager.AllPets[(Index) i].Cast<PetBehaviour>();
            }

            for (int i = 0; i < manager.AllSkins.Count; i++) {
                Skins[(uint) i] = manager.AllSkins[(Index) i].Cast<SkinData>();
            }

            return this;
        }

        public void Reset() {
            Hats = Hats.Where(kvp => kvp.Key < 10000000).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            Pets = Pets.Where(kvp => kvp.Key < 10000000).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            Skins = Skins.Where(kvp => kvp.Key < 10000000).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public bool IsOwned(uint id) {
            if (Owned.ContainsKey(id)) {
                return Owned[id];
            } else {
                return Owned[id] = false;
            }
        }

        public HatBehaviour GetHatById(uint hatId) => Hats.TryGetValue(hatId, out HatBehaviour hat) ? hat : manager.NoneHat;
        public PetBehaviour GetPetById(uint petId) => Pets.TryGetValue(petId, out PetBehaviour pet) ? pet : Pets[0];
        public SkinData GetSkinById(uint skinId) => Skins.TryGetValue(skinId, out SkinData skin) ? skin : Skins[0];

        public uint GetIdByHat(HatBehaviour hat) {
            return Hats.All(x => x.Value.Pointer != hat.Pointer) ? 0 : Hats.First(x => x.Value.Pointer == hat.Pointer).Key;
        }

        public uint GetIdByPet(PetBehaviour pet) {
            return Pets.All(x => x.Value.name != pet.name) ? 0 : Pets.First(x => x.Value.name == pet.name).Key;
        }

        public uint GetIdBySkin(SkinData skin) {
            return Skins.All(x => x.Value.name != skin.name) ? 0 : Skins.First(x => x.Value.name == skin.name).Key;
        }

        public HatBehaviour[] GetOwnedHats() => Hats.Where(h => h.Key < 10_000_000 ? !HatManager.IsMapStuff(h.Value.ProdId) && h.Value.LimitedMonth == 0 || SaveManager.GetPurchase(h.Value.ProductId) : IsOwned(h.Key)).OrderByDescending(o => o.Value.Order).ThenBy(o => o.Key).Select(x => x.Value).ToArray();
        public PetBehaviour[] GetOwnedPets() => Pets.Where(p => p.Key < 10_000_000 ? p.Value.Free || SaveManager.GetPurchase(p.Value.ProductId) : IsOwned(p.Key)).Select(x => x.Value).ToArray();
        public SkinData[] GetOwnedSkins() => Skins.Where(s => s.Key < 10_000_000 ? !HatManager.IsMapStuff(s.Value.ProdId) || SaveManager.GetPurchase(s.Value.ProdId) : IsOwned(s.Key)).OrderByDescending(o => o.Value.Order).ThenBy(o => o.Key).Select(x => x.Value).ToArray();

        public void SetHat(uint id, HatBehaviour behaviour, bool isFree) {
            Hats[id] = behaviour;
            Owned[id] = isFree;
        }

        public void SetPet(uint id, PetBehaviour behaviour, bool isFree) {
            Pets[id] = behaviour;
            Owned[id] = isFree;
        }

        public void SetSkin(uint id, SkinData behaviour, bool isFree) {
            Skins[id] = behaviour;
            Owned[id] = isFree;
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