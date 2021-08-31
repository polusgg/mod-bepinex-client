using System.Collections;
using System.Collections.Generic;
using BepInEx.Logging;
using HarmonyLib;
using Il2CppSystem;
using Polus.Behaviours;
using Polus.Extensions;
using UnhollowerBaseLib;
using UnityEngine;
using Debug = Il2CppMono.Unity.Debug;
using Object = Il2CppSystem.Object;

namespace Polus.Patches.Permanent
{
    [HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.Select))]
    public class VoteSelectPatch
    {
        [HarmonyPrefix]
        public static bool Select(PlayerVoteArea __instance)
        {
            if (MeetingHud.Instance.amDead)
                return false;
            
            return true;
        }
    }
}