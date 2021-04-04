using System.Linq;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using PolusApi;
using UnhollowerRuntimeLib;
using UnityEngine;
using IntPtr = System.IntPtr;
using Object = UnityEngine.Object;

namespace PolusMod.Patches {
    // I really learned how to patch IEnumerators from town of us today
    // this is the saddest day of my life
    // can't wait til i need to use 
    
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
    public class IntroCrewmatePatch {
        [HarmonyPrefix]
        public static void Prefix([HarmonyArgument(0)] ref List<PlayerControl> team) {
            team.Clear();
            foreach (PlayerControl playerControl in PolusMod.RoleData.IntroPlayers.Select(x => GameData.Instance.GetPlayerById(x).Object)) {
                team.Add(playerControl);
            }
        }
        [HarmonyPostfix]
        public static void Postfix(IntroCutscene __instance) {
            __instance.Title.Text = PolusMod.RoleData.IntroName;
            __instance.Title.Color = PolusMod.RoleData.IntroColor;
            __instance.ImpostorText.Text = PolusMod.RoleData.IntroDesc;
            __instance.BackgroundBar.material.color = PolusMod.RoleData.IntroColor;
        }
    }
    
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginImpostor))]
    public class IntroImpostorPatch {
        [HarmonyPrefix]
        public static void Prefix([HarmonyArgument(0)] ref List<PlayerControl> team) {
            IntroCrewmatePatch.Prefix(ref team);
        }

        [HarmonyPostfix]
        public static void Postfix(IntroCutscene __instance) {
            IntroCrewmatePatch.Postfix(__instance);
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
                TempData.winners.Clear();
                foreach (WinningPlayerData winningPlayerData in PolusMod.RoleData.OutroPlayers) {
                    TempData.winners.Add(winningPlayerData);
                }
            }

            [HarmonyPostfix]
            public static void Postfix(EndGameManager __instance) {
                SoundManager.Instance.StopSound(__instance.DisconnectStinger);
                __instance.WinText.Text = PolusMod.RoleData.OutroName;
                __instance.BackgroundBar.material.SetColor(Color, PolusMod.RoleData.OutroColor);
                __instance.gameObject.AddComponent<SetYoMamaUpTheIncredibleQuadrilogy>();
                _winDescText = Object.Instantiate(__instance.WinText.gameObject).GetComponent<TextRenderer>();
                _winDescText.transform.position = new Vector3(0, 0.75f, -14f);
                _winDescText.scale = 0.6f;
                _winDescText.Text = PolusMod.RoleData.OutroDesc;
            }
        }

        [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.CoBegin__d.MoveNext))]
        public class SetYoMamaUpTheSequel {
            [HarmonyPrefix]
            public static void YoMama(EndGameManager.CoBegin__d __instance) {
                float num = Mathf.Min(1f, __instance.timer / 3f);
                _descColor.a = Mathf.Lerp(0f, PolusMod.RoleData.OutroColor.a, (num - 0.3f) * 3f);
                _winDescText.Color = _descColor;
            }
        }

        [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.Start))]
        public class SetYoMamaUpAClassicOutroTrilogy {
            [HarmonyPostfix]
            public static void Start(EndGameManager __instance) {
                __instance.CancelInvoke();
                
            }
        }
        
        public class SetYoMamaUpTheIncredibleQuadrilogy : MonoBehaviour {
            public float endTime = 1.1f;
            public float timer = 0f;
            public EndGameManager manager;
            public SetYoMamaUpTheIncredibleQuadrilogy(IntPtr ptr) : base(ptr) {}

            static SetYoMamaUpTheIncredibleQuadrilogy (){
                ClassInjector.RegisterTypeInIl2Cpp<SetYoMamaUpTheIncredibleQuadrilogy>();
            }

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