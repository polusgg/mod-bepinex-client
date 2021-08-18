using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Polus.Extensions;
using TMPro;
using UnhollowerBaseLib;
using UnityEngine;
using static UnityEngine.Object;
using Object = System.Object;

namespace Polus.Patches.Permanent {
    [HarmonyPatch(typeof(HatsTab), nameof(HatsTab.OnEnable))]
    public class HatTabSeparationPatch {
        [HarmonyPrefix]
        public static bool OnEnable(HatsTab __instance) {
            //TODO Separate pgg specific cosmetics by productId but keep innersloth stuff the same
            return true;
            Il2CppReferenceArray<HatBehaviour> allHats = DestroyableSingleton<HatManager>.Instance.GetUnlockedHats();
            SortedList<string, List<HatBehaviour>> hatGroups = new(
                new PaddedComparer<string>("Vanilla", "")
            );

            foreach (HatBehaviour hat in allHats) {
                if (HatManager.Instance.GetIdFromHat(hat) < 10000000) hat.StoreName = "Vanilla";
                if (!hatGroups.ContainsKey(hat.StoreName))
                    hatGroups[hat.StoreName] = new List<HatBehaviour>();
                hatGroups[hat.StoreName].Add(hat);
            }

            foreach (ColorChip instanceColorChip in __instance.ColorChips)
                Destroy(instanceColorChip.gameObject);

            __instance.ColorChips.Clear();
            
            TextMeshPro groupNameText = __instance.transform.parent.parent.GetComponentInChildren<GameSettingMenu>(true).GetComponentInChildren<TextMeshPro>(true);

            int hatIdx = 0;
            foreach ((string groupName, List<HatBehaviour> hats) in hatGroups) {
                GameObject text = Instantiate(groupNameText.gameObject, __instance.scroller.Inner, true);
                text.transform.localScale = Vector3.one;

                TextMeshPro tmp = text.GetComponent<TextMeshPro>();
                tmp.text = groupName;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.fontSize = 3f;
                tmp.fontSizeMax = 3f;
                tmp.fontSizeMin = 0f;

                hatIdx = (hatIdx + 3) / 4 * 4;

                float xLerp = __instance.XRange.Lerp(0.5f);
                float yLerp = __instance.YStart - hatIdx / __instance.NumPerRow * __instance.YOffset;
                text.transform.localPosition = new Vector3(xLerp, yLerp, -1f);

                hatIdx += 4;
                foreach (HatBehaviour hat in hats.OrderBy(HatManager.Instance.GetIdFromHat)) {
                    float num = __instance.XRange.Lerp(hatIdx % __instance.NumPerRow / (__instance.NumPerRow - 1f));
                    float num2 = __instance.YStart - hatIdx / __instance.NumPerRow * __instance.YOffset;
                    ColorChip colorChip = Instantiate(__instance.ColorTabPrefab, __instance.scroller.Inner);
                    colorChip.transform.localPosition = new Vector3(num, num2, -1f);
                    colorChip.Button.OnClick.AddListener((Action) (() => __instance.SelectHat(hat)));
                    colorChip.Inner.SetHat(hat, PlayerControl.LocalPlayer.Data.ColorId);
                    colorChip.Inner.transform.localPosition = hat.ChipOffset + new Vector2(0f, -0.3f);
                    colorChip.Tag = hat;
                    __instance.ColorChips.Add(colorChip);
                    hatIdx += 1;
                }
            }

            __instance.scroller.YBounds.max = -(__instance.YStart - (hatIdx + 1) / __instance.NumPerRow * __instance.YOffset) - 3f;
            return true;
        }
    }

    public class PaddedComparer<T> : IComparer<T> where T : IComparable {
        private readonly T[] forcedToBottom;

        public PaddedComparer(params T[] forcedToBottom) {
            this.forcedToBottom = forcedToBottom;
        }

        public int Compare(T x, T y) {
            if (forcedToBottom.Contains(x) && forcedToBottom.Contains(y))
                return StringComparer.InvariantCulture.Compare(x, y);

            if (forcedToBottom.Contains(x))
                return 1;
            if (forcedToBottom.Contains(y))
                return -1;

            return StringComparer.InvariantCulture.Compare(x, y);
        }
    }
}