using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using Polus.Enums;
using Polus.Extensions;
using TMPro;
using UnhollowerRuntimeLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Polus.Patches.Temporary {
    // I really learned how to patch IEnumerators from town of us today
    // this is the saddest day of my life
    // can't wait til i need to use this info oh wait i do it like 2mil different times now

    [HarmonyPatch]
    public class ServersideIntroPatch {
        [HarmonyTargetMethods]
        public static System.Collections.Generic.IEnumerable<MethodBase> Methods() {
            yield return AccessTools.Method(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate));
            yield return AccessTools.Method(typeof(IntroCutscene), nameof(IntroCutscene.BeginImpostor));
        }
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
            __instance.ImpostorText.gameObject.SetActive(true);
            __instance.ImpostorText.text = PolusMod.RoleData.IntroDesc;
            __instance.BackgroundBar.material.color = PolusMod.RoleData.IntroColor;
        }
    }

    public class ServersideOutroPatches {
        private static TextMeshPro _winDescText;
        private static Color _descColor = UnityEngine.Color.white;
        private static readonly int Color = Shader.PropertyToID("_Color");
        private static float stingerTime = 1f;

        [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.SetEverythingUp))]
        public class SetYoMamaUp {
            private static void GetStingerVol(AudioSource source, float dt) {
                stingerTime += dt * 0.75f;
                source.volume = Mathf.Clamp(1f / stingerTime, 0f, 1f);
            }

            [HarmonyPrefix]
            public static bool Prefix(EndGameManager __instance) {
                TempData.winners.Clear();
                TempData.winners.Count.Log(2, "player count for outro");

                __instance.WinText.text = PolusMod.RoleData.OutroName;
                __instance.BackgroundBar.material.SetColor(Color, PolusMod.RoleData.OutroColor);
                __instance.gameObject.AddComponent<SetYoMamaUpTheIncredibleQuadrilogy>();
                _winDescText = Object.Instantiate(__instance.WinText.gameObject).GetComponent<TextMeshPro>();
                _winDescText.transform.position = new Vector3(0, 1.4f, -14f);
                _winDescText.fontSizeMax = 4f;
                _winDescText.fontSize = 4f;
                _winDescText.text = PolusMod.RoleData.OutroDesc;

                AudioClip sound = null;
                switch (PolusMod.RoleData.WinSound) {
                    case WinSounds.CustomSound:
                        sound = PogusPlugin.Cache.CachedFiles[PolusMod.RoleData.WinSoundCustom].Get<AudioClip>();
                        break;
                    case WinSounds.CrewmateWin: {
                        sound = __instance.CrewStinger;
                        break;
                    }
                    case WinSounds.ImpostorWin: {
                        sound = __instance.ImpostorStinger;
                        break;
                    }
                    case WinSounds.Disconnect:
                        sound = __instance.DisconnectStinger;
                        break;
                    default:
                        PolusMod.RoleData.WinSound.Log(comment: "uwu susussys");
                        break;
                }

                if (sound is not null)
                    //only play this using non dynamic sound system
                    SoundManager.Instance.PlayNamedSound("Stinger", sound, false, true);
                    // SoundManager.Instance.PlayDynamicSound("Stinger", sound, false,
                    //     new Action<AudioSource, float>(GetStingerVol), true);

                System.Collections.Generic.List<WinningPlayerData> list = PolusMod.RoleData.OutroPlayers
                    .OrderBy(b => b.IsYou ? -1 : 0)
                    .ToList();

                for (int i = 0; i < list.Count; i++) {
                    WinningPlayerData winningPlayerData2 = list[i];
                    int num = i % 2 == 0 ? -1 : 1;
                    int num2 = (i + 1) / 2;
                    float num3 = 1f - num2 * 0.075f;
                    float num4 = 1f - num2 * 0.035f;
                    float num5 = i == 0 ? -8 : -1;
                    PoolablePlayer poolablePlayer = Object.Instantiate(__instance.PlayerPrefab, __instance.transform);
                    poolablePlayer.transform.localPosition = new Vector3(0.8f * num * num2 * num4,
                        EndGameManager.BaseY - 0.25f + num2 * 0.1f, num5 + num2 * 0.01f) * 0.9f;
                    poolablePlayer.transform.localScale = new Vector3(num3, num3, num3) * 0.9f;
                    if (winningPlayerData2.IsDead) {
                        poolablePlayer.Body.sprite = __instance.GhostSprite;
                        poolablePlayer.SetDeadFlipX(i % 2 != 0);
                    } else {
                        poolablePlayer.SetFlipX(i % 2 == 0);
                    }

                    if (!winningPlayerData2.IsDead) {
                        poolablePlayer.SetSkin(winningPlayerData2.SkinId.Log(comment: "Wake up babe new end game skin"));
                        // DestroyableSingleton<HatManager>.Instance.SetSkin(poolablePlayer.Skin.layer,
                        //     winningPlayerData2.SkinId);
                    } else {
                        poolablePlayer.HatSlot.color = new Color(1f, 1f, 1f, 0.5f);
                    }

                    PlayerControl.SetPlayerMaterialColors(winningPlayerData2.ColorId, poolablePlayer.Body);
                    poolablePlayer.HatSlot.SetHat(winningPlayerData2.HatId, winningPlayerData2.ColorId);
                    PlayerControl.SetPetImage(winningPlayerData2.PetId, winningPlayerData2.ColorId,
                        poolablePlayer.PetSlot);
                    poolablePlayer.NameText.text = winningPlayerData2.Name;

                    if (winningPlayerData2.Name.StartsWith("<sprite index="))
                    {
                        int spriteEndPos = 13;
                        while (winningPlayerData2.Name[spriteEndPos] != '>' &&
                               spriteEndPos < winningPlayerData2.Name.Length)
                        {
                            spriteEndPos++;
                        }

                        if (spriteEndPos < winningPlayerData2.Name.Length)
                        {
                            poolablePlayer.NameText.text = poolablePlayer.NameText.text.Substring(spriteEndPos + 2);
                        }
                    }

                    poolablePlayer.NameText.transform.localScale = new Vector3(num3, num3, num3) * 1.25f;
                }

                StatsManager.Instance.GamesFinished++;
                return false;
            }
        }

        [HarmonyPatch(typeof(EndGameManager._CoBegin_d__21), nameof(EndGameManager._CoBegin_d__21.MoveNext))]
        public class SetYoMamaUpTheSequel {
            [HarmonyPrefix]
            public static void YoMama(EndGameManager._CoBegin_d__21 __instance) {
                if (TempData.EndReason != (GameOverReason) 7) return;
                float num = Mathf.Min(1f, __instance._timer_5__5 / 3f);
                _descColor.a = Mathf.Lerp(0f, PolusMod.RoleData.OutroColor.a, (num - 0.3f) * 3f);
                _winDescText.color = _descColor;
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
                    manager.PlayAgainButton.gameObject.SetActive(PolusMod.RoleData.ShowPlayAgain);
                    manager.ExitButton.gameObject.SetActive(PolusMod.RoleData.ShowQuit);

                    enabled = false;
                }
            }
        }
    }
}