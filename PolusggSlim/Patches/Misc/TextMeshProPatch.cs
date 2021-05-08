using HarmonyLib;
using UnityEngine;

namespace PolusggSlim.Patches.Misc
{
    public static class TextMeshProPatch
    {
        [HarmonyPatch(typeof(TextBoxTMP), nameof(TextBoxTMP.Start))]
        public class TextBoxTMP_Start
        {
            public static void Postfix()
            {
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
            }
        }

        [HarmonyPatch(typeof(TextMeshProExtensions), nameof(TextMeshProExtensions.CursorPos))]
        public static class TextMeshProExtensionPatch
        {
            public static void Postfix(ref Vector2 __result)
            {
                if (__result == Vector2.zero)
                {
                   __result = new Vector2(0, 0.25f);
                }
            }
        }
    }
}