using HarmonyLib;
using Il2CppSystem;
using UnhollowerBaseLib;
using UnityEngine;

namespace Polus.Patches.Temporary {
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
    public class AuRevoirLesBadKillsPatch {
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
        public static class MurderPlayerPatch
        {
            [HarmonyPrefix]
            public static bool MurderPlayer(PlayerControl __instance, PlayerControl target)
            {
				if (!target || __instance.Data.IsDead || !__instance.Data.IsImpostor || __instance.Data.Disconnected)
				{
					int num = target ? ((int)target.PlayerId) : -1;
					Debug.LogWarning(string.Format("Bad kill from {0} to {1}", __instance.PlayerId, num));
					return false;
				}
				GameData.PlayerInfo data = target.Data;
				if (data == null || data.IsDead)
				{
					Debug.LogWarning("Missing target data for kill");
					return false;
				}
				if (__instance.AmOwner)
				{
					StatsManager instance = StatsManager.Instance;
					uint num2 = instance.ImpostorKills;
					instance.ImpostorKills = num2 + 1U;
					if (Constants.ShouldPlaySfx())
					{
						SoundManager.Instance.PlaySound(__instance.KillSfx, false, 0.8f);
					}
					__instance.SetKillTimer(PlayerControl.GameOptions.KillCooldown);
				}
				target.gameObject.layer = LayerMask.NameToLayer("Ghost");
				if (target.AmOwner)
				{
					StatsManager instance2 = StatsManager.Instance;
					uint num2 = instance2.TimesMurdered;
					instance2.TimesMurdered = num2 + 1U;
					if (Minigame.Instance)
					{
						try
						{
							Minigame.Instance.Close();
							Minigame.Instance.Close();
						}
						catch
						{
						}
					}
					DestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(__instance.Data, data);
					DestroyableSingleton<HudManager>.Instance.ShadowQuad.gameObject.SetActive(false);
					target.nameText.GetComponent<MeshRenderer>().material.SetInt("_Mask", 0);
					target.RpcSetScanner(false);
					ImportantTextTask importantTextTask = new GameObject("_Player").AddComponent<ImportantTextTask>();
					importantTextTask.transform.SetParent(__instance.transform, false);
					if (!PlayerControl.GameOptions.GhostsDoTasks)
					{
						target.ClearTasks();
						importantTextTask.Text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GhostIgnoreTasks, new Il2CppReferenceArray<Il2CppSystem.Object>(Array.Empty<Il2CppSystem.Object>()));
					}
					else
					{
						importantTextTask.Text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GhostDoTasks,  new Il2CppReferenceArray<Il2CppSystem.Object>(Array.Empty<Il2CppSystem.Object>()));
					}
					target.myTasks.Insert(0, importantTextTask);
				}

				KillAnimation anim = __instance.KillAnimations[UnityEngine.Random.Range(0, __instance.KillAnimations.Count)];
				DestroyableSingleton<AchievementManager>.Instance.OnMurder(__instance.AmOwner, target.AmOwner);
				__instance.MyPhysics.StartCoroutine(anim.CoPerformKill(__instance, target));
				return false;
            }
        }
        
        [HarmonyPrefix]
        public static void SetImpostor(PlayerControl __instance, ref bool __state) {
            __state = !__instance.Data.IsImpostor;
            if (__state) __instance.Data.IsImpostor = true;
        }

        [HarmonyPostfix]
        public static void UnsetImpostor(PlayerControl __instance, ref bool __state) {
            if (__state) __instance.Data.IsImpostor = false;
        }
    }
}