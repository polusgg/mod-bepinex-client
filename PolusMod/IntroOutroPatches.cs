using System.Linq;
using System.Reflection;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using UnhollowerRuntimeLib;

namespace PolusMod {
    // I really learned how to patch IEnumerators from town of us today
    // this is the saddest day of my life
    // can't wait til i need to use 
    
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
    public class IntroCrewmatePatch {
        [HarmonyPrefix]
        public static void Prefix([HarmonyArgument(0)] ref List<PlayerControl> team) {
            team.Clear();
            foreach (PlayerControl playerControl in TestPggMod.RoleData.IntroPlayers.Select(x => GameData.Instance.GetPlayerById(x).Object)) {
                team.Add(playerControl);
            }
        }
        [HarmonyPostfix]
        public static void Postfix(IntroCutscene __instance) {
            __instance.Title.Text = TestPggMod.RoleData.IntroName;
            __instance.Title.Color = TestPggMod.RoleData.IntroColor;
            __instance.ImpostorText.Text = TestPggMod.RoleData.IntroDesc;
            __instance.BackgroundBar.material.color = TestPggMod.RoleData.IntroColor;
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
}