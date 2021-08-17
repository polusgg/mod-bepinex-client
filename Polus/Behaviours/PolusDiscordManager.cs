using System;
using Discord;
using Polus.Enums;
using Polus.Extensions;
using UnhollowerBaseLib;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using DateTime = Il2CppSystem.DateTime;

namespace Polus.Behaviours {
    public class PolusDiscordManager : MonoBehaviour {
        public static PolusDiscordManager Instance;
        private Discord.Discord presence;

        static PolusDiscordManager() {
            ClassInjector.RegisterTypeInIl2Cpp<PolusDiscordManager>();
        }

        public PolusDiscordManager(IntPtr ptr) : base(ptr) { }

        public DateTime? StartTime { get; set; }

        private void Start() {
            try {
                // presence = new Discord.Discord(832788555583062036L, (ulong) CreateFlags.NoRequireDiscord);
                // ActivityManager activityManager = presence.GetActivityManager();
                // activityManager.RegisterSteam(945360U);
                // activityManager.add_OnActivityJoin(new Action<string>(JoinRequest));
                // Instance = this;

                // SceneManager.add_sceneLoaded(new Action<Scene, LoadSceneMode>(OnSceneChanged));
            } catch {
                "ignore or smth idk".Log(); //putting comments in logs is funny
                // Destroy(gameObject);
            }
        }

        public void FixedUpdate() {
            if (presence == null) return;

            try {
                presence.RunCallbacks();
            } catch (Il2CppException) {
                presence.Dispose();
                presence = null;
            }
        }

        private void OnDestroy() {
            Instance = null;
        }

        private void OnSceneChanged(Scene scene, LoadSceneMode loadArgs) {
            string name = scene.name;
            if (name == GameScenes.MatchMaking || name == GameScenes.MMOnline || name == GameScenes.MainMenu) SetInMenus();
        }

        public void SetInMenus() {
            if (presence == null) return;

            ClearActivity();
            StartTime = null;
            Activity activity = new() {
                State = "In Menus",
                Assets = {LargeImage = "polusicon"}
            };
            presence.GetActivityManager().UpdateActivity(activity, new Action<Result>(HandleNothing));
        }

        private void HandleNothing(Result _) { }

        public void SetPlayingGame() {
            if (presence == null) return;

            if (StartTime == null) StartTime = DateTime.UtcNow;

            Activity activity = new() {
                State = "In Game",
                // Details = "Playing on Polus.gg",
                Assets = {LargeImage = "polusicon"},
                Timestamps = new ActivityTimestamps() {Start = DiscordManager.ToUnixTime(StartTime.Value)}
            };
            presence.GetActivityManager().UpdateActivity(activity, new Action<Result>(HandleNothing));
        }

        public void SetInLobby(int numPlayers, int gameId) {
            if (presence == null) return;

            StartTime ??= DateTime.UtcNow;

            // string text = GameCode.IntToGameName(gameId);
            Activity activity = new() {
                State = "In Lobby",
                // Details = "Playing on ",
                Assets = {LargeImage = "polusicon"}
                // Party = {Id = text},
                // Party = {Size = new() {CurrentSize = numPlayers, MaxSize = 10}},
                // Secrets = {Join = "join" + text, Match = "match" + text}
            };
            presence.GetActivityManager().UpdateActivity(activity, new Action<Result>(HandleNothing));
        }

        public void ClearActivity() { }

        private void JoinRequest(string obj) {
            // throw new NotImplementedException();
        }
    }
}