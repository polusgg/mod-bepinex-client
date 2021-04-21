using System;
using System.Linq;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using UnhollowerRuntimeLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PolusGG.Patches.Temporary {
    // I really learned how to patch IEnumerators from town of us today
    // this is the saddest day of my life
    // can't wait til i need to use this info oh wait i do it like 2mil different times now

    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
    public class IntroCrewmatePatch {
        [HarmonyPrefix]
        public static void Prefix([HarmonyArgument(0)] ref List<PlayerControl> team) {
            IntroImpostorPatch.Prefix(ref team);
        }

        [HarmonyPostfix]
        public static void Postfix(IntroCutscene __instance) {
            IntroImpostorPatch.Postfix(__instance);
            __instance.ImpostorText.gameObject.SetActive(true);
        }
    }

    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginImpostor))]
    public class IntroImpostorPatch {
        [HarmonyPrefix]
        public static void Prefix([HarmonyArgument(0)] ref List<PlayerControl> team) {
            team.Clear();
            foreach (PlayerControl playerControl in PolusMod.RoleData.IntroPlayers.Select(x =>
                GameData.Instance.GetPlayerById(x).Object)) team.Add(playerControl);
        }

        [HarmonyPostfix]
        public static void Postfix(IntroCutscene __instance) {
            __instance.Title.text = PolusMod.RoleData.IntroName;
            __instance.Title.color = PolusMod.RoleData.IntroColor;
            __instance.ImpostorText.text = PolusMod.RoleData.IntroDesc;
            __instance.BackgroundBar.material.color = PolusMod.RoleData.IntroColor;
        }
    }

    public class OutroPatches {
        private static TextRenderer _winDescText;
        private static Color _descColor = UnityEngine.Color.white;
        private static readonly int Color = Shader.PropertyToID("_Color");

        [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.SetEverythingUp))]
        public class SetYoMamaUp {
            [HarmonyPrefix]
            public static void Prefix() {
                if (TempData.EndReason != (GameOverReason) 7) return;
                TempData.winners.Clear();
                foreach (WinningPlayerData winningPlayerData in PolusMod.RoleData.OutroPlayers)
                    TempData.winners.Add(winningPlayerData);
            }

            [HarmonyPostfix]
            public static void Postfix(EndGameManager __instance) {
                if (TempData.EndReason != (GameOverReason) 7) return;
                SoundManager.Instance.StopSound(__instance.DisconnectStinger);
                __instance.WinText.text = PolusMod.RoleData.OutroName;
                __instance.BackgroundBar.material.SetColor(Color, PolusMod.RoleData.OutroColor);
                __instance.gameObject.AddComponent<SetYoMamaUpTheIncredibleQuadrilogy>();
                _winDescText = Object.Instantiate(__instance.WinText.gameObject).GetComponent<TextRenderer>();
                _winDescText.transform.position = new Vector3(0, 0.75f, -14f);
                _winDescText.scale = 0.6f;
                _winDescText.Text = PolusMod.RoleData.OutroDesc;
            }
        }

        [HarmonyPatch(typeof(EndGameManager._CoBegin_d__18), nameof(EndGameManager._CoBegin_d__18.MoveNext))]
        public class SetYoMamaUpTheSequel {
            [HarmonyPrefix]
            public static void YoMama(EndGameManager._CoBegin_d__18 __instance) {
                if (TempData.EndReason != (GameOverReason) 7) return;
                float num = Mathf.Min(1f, __instance._timer_5__5 / 3f);
                _descColor.a = Mathf.Lerp(0f, PolusMod.RoleData.OutroColor.a, (num - 0.3f) * 3f);
                _winDescText.Color = _descColor;
            }
        }

        [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.Start))]
        public class SetYoMamaUpAClassicOutroTrilogy {
            [HarmonyPostfix]
            public static void Start(EndGameManager __instance) {
                if (TempData.EndReason != (GameOverReason) 7) return;
                __instance.CancelInvoke();
            }
        }

        public class SetYoMamaUpTheIncredibleQuadrilogy : MonoBehaviour {
            public float endTime = 1.1f;
            public float timer;
            public EndGameManager manager;

            static SetYoMamaUpTheIncredibleQuadrilogy() {
                ClassInjector.RegisterTypeInIl2Cpp<SetYoMamaUpTheIncredibleQuadrilogy>();
            }

            public SetYoMamaUpTheIncredibleQuadrilogy(IntPtr ptr) : base(ptr) { }

            private void Start() {
                manager = GetComponent<EndGameManager>();
            }

            private void Update() {
                timer += Time.deltaTime;
                if (timer > endTime) {
                    manager.FrontMost.gameObject.SetActive(false);
                    if (PolusMod.RoleData.ShowPlayAgain) manager.PlayAgainButton.gameObject.SetActive(true);
                    if (PolusMod.RoleData.ShowQuit) manager.ExitButton.gameObject.SetActive(true);

                    enabled = false;
                }
            }
        }
    }
}