using System.Linq;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace PolusggSlim.Patches.Misc
{
    public static class TextMeshProPatch
    {
        [HarmonyPatch(typeof(TextBoxTMP), nameof(TextBoxTMP.Start))]
        public static class TextBoxTMP_Start
        {
            private static bool _executed;
            public static void Postfix()
            {
                if (_executed)
                    return;

                TextBoxTMP.SymbolChars.Add('#');
                TextBoxTMP.SymbolChars.Add('$');
                TextBoxTMP.SymbolChars.Add('*');
                TextBoxTMP.SymbolChars.Add('"');
                TextBoxTMP.SymbolChars.Add('\'');
                TextBoxTMP.SymbolChars.Add('<');
                TextBoxTMP.SymbolChars.Add('>');
                TextBoxTMP.SymbolChars.Add('[');
                TextBoxTMP.SymbolChars.Add(']');
                TextBoxTMP.SymbolChars.Add('{');
                TextBoxTMP.SymbolChars.Add('}');
                TextBoxTMP.SymbolChars.Add('|');
                TextBoxTMP.SymbolChars.Add('`');
                TextBoxTMP.SymbolChars.Add('@');

                _executed = true;
            }
        }

        [HarmonyPatch(typeof(TextMeshProExtensions), nameof(TextMeshProExtensions.CursorPos))]
        public static class TextMeshProExtensionPatch
        {
            public static void Postfix(TextMeshPro self, ref Vector2 __result)
            {
                if (__result == Vector2.zero)
                {
                    __result = self.GetTextInfo(" ").lineInfo.First().lineExtents.max;
                }
            }
        }
    }
}