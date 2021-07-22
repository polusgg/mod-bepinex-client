using HarmonyLib;
using Polus.Extensions;

namespace Polus.Patches.Temporary
{
    // This is required cuz CoSetTasks coroutine is messed up for no reason.
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetTasks))]
    public class TasksAreNotBrokenAnymorePatch
    {
        [HarmonyPrefix]
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)]Il2CppSystem.Collections.Generic.List<GameData.TaskInfo> tasks)
        {
            __instance.StartCoroutine(CoSetTasksExtensions.CoSetTasks(__instance, tasks));
            return false;
        }
    }
}