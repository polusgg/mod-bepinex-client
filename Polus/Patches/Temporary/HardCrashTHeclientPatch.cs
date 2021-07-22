// using System;
// using HarmonyLib;
// using Il2CppSystem.Collections;

// namespace PolusGG.Patches.Temporary {
//     [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
//     public class HardCrashTHeclientPatch {
//         [HarmonyPrefix]
//         public static void P() {
//             HudManager.Instance.KillOverlay.queue.Enqueue(new Func<IEnumerator>(() => {
//                 OverlayKillAnimation oka = UnityEngine.Object.Instantiate(HudManager.Instance.KillOverlay.KillAnims[0], HudManager.Instance.KillOverlay.transform);
//                 oka.Initialize(PlayerControl.LocalPlayer.Data, PlayerControl.LocalPlayer.Data);
//                 oka.gameObject.SetActive(false);
//                 return Effects.All(new IEnumerator[] {
//                     HudManager.Instance.KillOverlay.CoShowOne(oka)
//                 });
//             }));
//             HudManager.Instance.KillOverlay.showAll ??=
//                 HudManager.Instance.KillOverlay.StartCoroutine(HudManager.Instance.KillOverlay.ShowAll());        }
//     }
// }