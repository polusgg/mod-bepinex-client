using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Polus.Behaviours;
using Polus.Enums;
using Polus.Extensions;
using Polus.Mods.Patching;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Polus.Patches.Permanent {
    // #if RELEASE
    public static class StupidModStampPatches {
        private static TextMeshPro textObj;
        public static Color? TextColor;
        public static string Suffix = "";
        public static QRStamp qr;
        private static int UILayer = LayerMask.NameToLayer("UI");

        public static bool QrActuallyVisible;
        public static bool QrToggled = true;

        public static bool QrVisible {
            set => qr.gameObject.SetActive((value || QrActuallyVisible) && QrToggled);
            get => qr.gameObject.active && QrActuallyVisible && QrToggled;
        }

        public static void Reset() {
            QrActuallyVisible = false;
            QrVisible = false;
            TextColor = null;
            Suffix = "";
        }

        [HarmonyPatch(typeof(ModManager), nameof(ModManager.ShowModStamp))]
        public static class ShowThatStupidStampPatch {

            [PermanentPatch]
            [HarmonyPrefix]
            public static void ShowModStamp(ModManager __instance) {
                textObj = Object.Instantiate(PogusPlugin.Bundle
                        .LoadAsset("Assets/Mods/OfficialAssets/PlayingOnPog.prefab"), __instance.ModStamp.transform)
                        .Cast<GameObject>()
                        .GetComponent<TextMeshPro>();
                textObj.transform.localPosition = new Vector3(0.75f, 0, 100f);
                // textObj.gameObject.layer = UILayer; // :hahaa:
                GameObject qrObject = new("QR");
                qr = qrObject.AddComponent<QRStamp>();
                qrObject.transform.parent = __instance.transform;
                qrObject.transform.position = new Vector3(0, 0, 0);
            }
        }

        [HarmonyPatch(typeof(TaskPanelBehaviour), nameof(TaskPanelBehaviour.Update))]
        public static class ReimplementTaskPanelSighPatch {
            [PermanentPatch]
            [HarmonyPrefix]
            public static bool Update(TaskPanelBehaviour __instance) {
                __instance.background.transform.localScale = __instance.TaskText.textBounds.size.x > 0f ? new Vector3(__instance.TaskText.textBounds.size.x + 0.2f, __instance.TaskText.textBounds.size.y + 0.2f, 1f) : Vector3.zero;
                Vector3 vector = __instance.background.sprite.bounds.extents;
                vector.y = -vector.y;
                vector = global::Extensions.Mul(vector, __instance.background.transform.localScale);
                __instance.background.transform.localPosition = vector;
                Vector3 vector2 = __instance.tab.sprite.bounds.extents;
                vector2 = global::Extensions.Mul(vector2, __instance.tab.transform.localScale);
                vector2.y = -vector2.y;
                vector2.x += vector.x * 2f;
                __instance.tab.transform.localPosition = vector2;
                Vector3 closedPosition = __instance.ClosedPosition;
                Vector3 openPosition = __instance.OpenPosition;
                closedPosition.y = openPosition.y = 1.2f;
                closedPosition.x = -__instance.background.sprite.bounds.size.x * __instance.background.transform.localScale.x;
                __instance.timer = __instance.open ? Mathf.Min(1f, __instance.timer + Time.deltaTime / __instance.Duration) : Mathf.Max(0f, __instance.timer - Time.deltaTime / __instance.Duration);
                Vector3 relativePos = new(Mathf.SmoothStep(closedPosition.x, openPosition.x, __instance.timer), Mathf.SmoothStep(closedPosition.y, openPosition.y, __instance.timer), openPosition.z);
                __instance.transform.localPosition = AspectPosition.ComputePosition(AspectPosition.EdgeAlignments.LeftTop, relativePos);
                return false;
            }
        }

        [HarmonyPatch(typeof(ModManager), nameof(ModManager.LateUpdate))]
        public static class ModStampLateUpdatePatch {
            public static Dictionary<string, (Vector2, AspectPosition.EdgeAlignments)> StampPositions = new () {
                [GameScenes.MMOnline] = (
                    new Vector2(1f, 0.425f),
                    AspectPosition.EdgeAlignments.RightTop
                ),
                [GameScenes.OnlineGame] = (
                    new Vector2(0.425f, 1.15f),
                    AspectPosition.EdgeAlignments.RightTop
                ),
            };

            private static (Vector2, AspectPosition.EdgeAlignments) defaultLocation = (
                new Vector2(0.425f, 0.425f),
                AspectPosition.EdgeAlignments.RightTop
            );

            [HarmonyPrefix]
            public static bool LateUpdate(ModManager __instance) {
                if (!__instance.ModStamp.enabled)
                    return false;

                if (!__instance.localCamera) __instance.localCamera = DestroyableSingleton<HudManager>.InstanceExists ? DestroyableSingleton<HudManager>.Instance.GetComponentInChildren<Camera>() : Camera.main;

                bool inGame = AmongUsClient.Instance && AmongUsClient.Instance.InOnlineScene && ShipStatus.Instance;
                string name = SceneManager.GetActiveScene().name;

                textObj.gameObject.SetActive(inGame);

                if (inGame) {
                    __instance.ModStamp.transform.position = AspectPosition.ComputeWorldPosition(__instance.localCamera, AspectPosition.EdgeAlignments.LeftTop, new Vector3(QrVisible ? 1f : 0.4f, 0.85f, __instance.localCamera.nearClipPlane + 0.1f));
                    textObj.color = TextColor ?? Color.white;
                    textObj.text = $"Playing on Polus.gg {Suffix}";
                    qr.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
                    qr.transform.position = AspectPosition.ComputeWorldPosition(__instance.localCamera, AspectPosition.EdgeAlignments.LeftTop, new Vector3(0.4f, 0.85f, __instance.localCamera.nearClipPlane + 100f));
                } else {
                    (Vector2 position, AspectPosition.EdgeAlignments alignment) = StampPositions.Count(scene => scene.Key == name) == 1 ? StampPositions.Single(scene => scene.Key == SceneManager.GetActiveScene().name).Value : defaultLocation;
                    __instance.ModStamp.transform.position = AspectPosition.ComputeWorldPosition(__instance.localCamera, alignment, new Vector3(position.x, position.y, __instance.localCamera.nearClipPlane + 100f));
                    QrVisible = name == GameScenes.OnlineGame;
                    qr.transform.position = AspectPosition.ComputeWorldPosition(__instance.localCamera, AspectPosition.EdgeAlignments.RightBottom, new Vector3(2.1f, 0.7f, __instance.localCamera.nearClipPlane + 100f));
                    qr.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
                }

                return false;
            }
        }
    }
    // #endif
}