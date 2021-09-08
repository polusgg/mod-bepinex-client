using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Polus.Extensions;
using Polus.Resources;
using UnhollowerRuntimeLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Polus.Behaviours {
    public class CosmeticManager : MonoBehaviour {
        public const uint CosmeticStartId = 10000000;
        static CosmeticManager() {
            ClassInjector.RegisterTypeInIl2Cpp<CosmeticManager>();
        }
        public CosmeticManager(IntPtr ptr) : base(ptr) {}
        private HatManager manager;
        private Dictionary<uint, HatBehaviour> hats = new();
        private Dictionary<uint, PetBehaviour> pets = new();
        private Dictionary<uint, SkinData> skins = new();
        private readonly Dictionary<uint, bool> owned = new();
        private CacheListener listener;
        private bool initialized = false;

        public static bool InstanceExists => HatManager.InstanceExists;
        // unreadable :)
        public static CosmeticManager Instance => InstanceExists ? HatManager.Instance.TryGetComponent(UnhollowerRuntimeLib.Il2CppType.Of<CosmeticManager>(), out Component component) ? component.Cast<CosmeticManager>() : HatManager.Instance.gameObject.AddComponent<CosmeticManager>().Initialize() : null;

        private CosmeticManager Initialize() {
            if (initialized) throw new Exception("CosmeticManager is already initialized!");
            manager = GetComponent<HatManager>();
            listener = new CacheListener((id, current, old) => {
                id--;
                if (current.Type == ResourceType.Asset && GetAny(id, out Object obj)) {
                    Object cosmetic = current.Get<Object>();
                    switch (obj.GetIl2CppType().Name) {
                        case nameof(HatBehaviour): {
                            hats[id] = cosmetic.Cast<HatBehaviour>();
                            break;
                        }
                        case nameof(PetBehaviour): {
                            pets[id] = cosmetic.Cast<GameObject>().GetComponent<PetBehaviour>();
                            break;
                        }
                        case nameof(SkinData): {
                            skins[id] = cosmetic.Cast<SkinData>();
                            break;
                        }
                    }
                }
            });

            for (int i = 0; i < manager.AllHats.Count; i++) {
                hats[(uint) i] = manager.AllHats[(Index) i].Cast<HatBehaviour>();
            }

            for (int i = 0; i < manager.AllPets.Count; i++) {
                pets[(uint) i] = manager.AllPets[(Index) i].Cast<PetBehaviour>();
            }

            for (int i = 0; i < manager.AllSkins.Count; i++) {
                skins[(uint) i] = manager.AllSkins[(Index) i].Cast<SkinData>();
            }

            initialized = true;
            return this;
        }

        public bool GetAny(uint id, out Object bhv) {
            if (hats.TryGetValue(id, out HatBehaviour hat)) bhv = hat;
            else if (pets.TryGetValue(id, out PetBehaviour pet)) bhv = pet;
            else if (skins.TryGetValue(id, out SkinData skin)) bhv = skin;
            else bhv = null;
            return bhv == null;
        }

        public void Reset() {
            hats = hats.Where(kvp => kvp.Key < CosmeticManager.CosmeticStartId).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            pets = pets.Where(kvp => kvp.Key < CosmeticManager.CosmeticStartId).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            skins = skins.Where(kvp => kvp.Key < CosmeticManager.CosmeticStartId).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            listener.Dispose();
            initialized = false;
        }

        public bool IsOwned(uint id) => owned.ContainsKey(id) ? owned[id] : owned[id] = false;

        public HatBehaviour GetHatById(uint hatId) => hats.TryGetValue(hatId, out HatBehaviour hat) ? hat : manager.NoneHat;
        public PetBehaviour GetPetById(uint petId) => pets.TryGetValue(petId, out PetBehaviour pet) ? pet : pets[0];
        public SkinData GetSkinById(uint skinId) => skins.TryGetValue(skinId, out SkinData skin) ? skin : skins[0];

        public uint GetIdByHat(HatBehaviour hat) {
            return hats.All(x => x.Value.Pointer != hat.Pointer) ? 0 : hats.First(x => x.Value.Pointer == hat.Pointer).Key;
        }

        public uint GetIdByPet(PetBehaviour pet) {
            return pets.All(x => x.Value.name != pet.name) ? 0 : pets.First(x => x.Value.name == pet.name).Key;
        }

        public uint GetIdBySkin(SkinData skin) {
            return skins.All(x => x.Value.name != skin.name) ? 0 : skins.First(x => x.Value.name == skin.name).Key;
        }

        public HatBehaviour[] GetOwnedHats() => hats.Where(h => h.Key < CosmeticStartId ? !HatManager.IsMapStuff(h.Value.ProdId) && h.Value.LimitedMonth == 0 || SaveManager.GetPurchase(h.Value.ProductId) : IsOwned(h.Key)).OrderByDescending(o => o.Value.Order).ThenBy(o => o.Key).Select(x => x.Value).ToArray();
        public PetBehaviour[] GetOwnedPets() => pets.Where(p => p.Key < CosmeticStartId ? p.Value.Free || SaveManager.GetPurchase(p.Value.ProductId) : IsOwned(p.Key)).Select(x => x.Value).ToArray();
        public SkinData[] GetOwnedSkins() => skins.Where(s => s.Key < CosmeticStartId ? !HatManager.IsMapStuff(s.Value.ProdId) || SaveManager.GetPurchase(s.Value.ProdId) : IsOwned(s.Key)).OrderByDescending(o => o.Value.Order).ThenBy(o => o.Key).Select(x => x.Value).ToArray();

        public void SetHat(uint id, HatBehaviour behaviour, bool isFree) {
            hats[id] = behaviour;
            owned[id] = isFree;
        }

        public void SetPet(uint id, PetBehaviour behaviour, bool isFree) {
            pets[id] = behaviour;
            owned[id] = isFree;
        }

        public void SetSkin(uint id, SkinData behaviour, bool isFree) {
            skins[id] = behaviour;
            owned[id] = isFree;
        }

        public void AddAllOwnedVanillaCosmetics() {
            DateTime utcNow = DateTime.UtcNow;
            for (uint i = 0; i < hats.Count; i++) {
                HatBehaviour hatBehaviour = hats[i].Cast<HatBehaviour>();
                if (!hatBehaviour.ProdId.StartsWith("pet_") && (hatBehaviour.LimitedMonth == utcNow.Month || hatBehaviour.LimitedMonth == 0) && (hatBehaviour.LimitedYear == utcNow.Year || hatBehaviour.LimitedYear == 0) && !hatBehaviour.NotInStore && !HatManager.IsMapStuff(hatBehaviour.ProdId) && !SaveManager.GetPurchase(hatBehaviour.ProductId)) {
                    SaveManager.SetPurchased(hatBehaviour.ProductId);
                }
            }
        }
    }
}