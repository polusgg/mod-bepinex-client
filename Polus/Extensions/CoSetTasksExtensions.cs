using System.Collections;
using UnhollowerBaseLib;
using UnityEngine;

namespace Polus.Extensions
{
    public static class CoSetTasksExtensions
    {
        public static IEnumerator CoSetTasks(this PlayerControl player, Il2CppSystem.Collections.Generic.List<GameData.TaskInfo> tasks)
        {
            while (!ShipStatus.Instance)
            {
                yield return null;
            }
            if (player.AmOwner)
            {
                DestroyableSingleton<HudManager>.Instance.TaskStuff.SetActive(true);
                StatsManager instance = StatsManager.Instance;
                uint num = instance.GamesStarted;
                instance.GamesStarted = num + 1U;
                if (player.Data.IsImpostor)
                {
                    StatsManager instance2 = StatsManager.Instance;
                    num = instance2.TimesImpostor;
                    instance2.TimesImpostor = num + 1U;
                    StatsManager.Instance.CrewmateStreak = 0U;
                }
                else
                {
                    StatsManager instance3 = StatsManager.Instance;
                    num = instance3.TimesCrewmate;
                    instance3.TimesCrewmate = num + 1U;
                    StatsManager instance4 = StatsManager.Instance;
                    num = instance4.CrewmateStreak;
                    instance4.CrewmateStreak = num + 1U;
                    DestroyableSingleton<HudManager>.Instance.KillButton.gameObject.SetActive(false);
                }
            }

            foreach (var pt in player.myTasks)
            {
                Object.Destroy(pt);
            }
            player.myTasks.Clear();
            if (player.Data.IsImpostor)
            {
                ImportantTextTask importantTextTask = new GameObject("_Player").AddComponent<ImportantTextTask>();
                importantTextTask.transform.SetParent(PlayerControl.LocalPlayer.transform, false);
                importantTextTask.Text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ImpostorTask, new Il2CppReferenceArray<Il2CppSystem.Object>(0)) + "\r\n<color=#FFFFFFFF>" + DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.FakeTasks, new Il2CppReferenceArray<Il2CppSystem.Object>(0)) + "</color>";
                player.myTasks.Insert(0, importantTextTask);
            }

            foreach (var task in tasks)
            {
                GameData.TaskInfo taskInfo = task;
                NormalPlayerTask normalPlayerTask = Object.Instantiate<NormalPlayerTask>(ShipStatus.Instance.GetTaskById(taskInfo.TypeId), player.transform);
                normalPlayerTask.Id = taskInfo.Id;
                normalPlayerTask.Owner = player;
                normalPlayerTask.Initialize();
                player.myTasks.Add(normalPlayerTask);
            }
            yield break;
        }
    }
}