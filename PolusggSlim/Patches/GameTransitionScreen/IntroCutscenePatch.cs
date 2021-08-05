using System.Linq;
using HarmonyLib;
using UnhollowerBaseLib;
using UnityEngine;

namespace PolusggSlim.Patches.GameTransitionScreen
{
    public static class IntroCutscenePatch
    {
        public static CutsceneData Data = new();

        public static void IntroCutscenePrefix(out Il2CppReferenceArray<PlayerControl> yourTeam)
        {
            yourTeam = Data.YourTeam
                .Select(x => GameData.Instance.GetPlayerById(x).Object)
                .ToArray();
        }

        public static void IntroCutscenePostfix(ref IntroCutscene __instance)
        {
            __instance.Title.text = Data.TitleText;
            __instance.Title.color = Data.BackgroundColor;

            __instance.ImpostorText.gameObject.active = true;
            __instance.ImpostorText.text = Data.SubtitleText;
            __instance.ImpostorText.color = Color.white;

            __instance.BackgroundBar.material.color = Data.BackgroundColor;
        }

        // [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
        public static class IntroCutscene_BeginCrewmate
        {
            public static void Prefix([HarmonyArgument(0)] out Il2CppReferenceArray<PlayerControl> yourTeam)
            {
                IntroCutscenePrefix(out yourTeam);
            }

            public static void Postfix([HarmonyArgument(0)] ref IntroCutscene __instance)
            {
                IntroCutscenePostfix(ref __instance);
            }
        }

        // [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginImpostor))]
        public static class IntroCutscene_BeginImpostor
        {
            public static void Prefix([HarmonyArgument(0)] out Il2CppReferenceArray<PlayerControl> yourTeam)
            {
                IntroCutscenePrefix(out yourTeam);
            }

            public static void Postfix([HarmonyArgument(0)] ref IntroCutscene __instance)
            {
                IntroCutscenePostfix(ref __instance);
            }
        }
    }
}