using System;
using Discord;
using HarmonyLib;
using Hazel;
using Polus.Extensions;
using Polus.Mods.Patching;
using UnityEngine;

namespace Polus.Patches.Permanent {
    public class DiscordPatches {
        public static Discord.Discord discord;
        public const long ClientId = 832788555583062036;

        public static void HandleActivityJoin(string joinSecret) {
            //todo
        }

        public static void SussyBaka() {
            // return;
            // discord.GetActivityManager().UpdateActivity(new Activity {
            //     State = "POGGING IRL OMG NO WAY"
            // }, (Action<Result>) (r => r.Log(comment: "update activity result")));
        }

        public static void ClearPresence(Action callback) {
            // return;
            // discord?.GetActivityManager().ClearActivity((Action<Result>) (r => {
            //     if (r == Result.Ok)
            //         callback();
            //     else
            //         r.Log(comment: "Failed to clear presence!");
            // }));
        }

        public static void UpdateRichPresence(MessageReader reader) {
            if (discord == null) return;
            "sus".Log(20);
            Activity activity = new() {
                Type = ActivityType.Playing,
                ApplicationId = ClientId,
                Name = null,
                State = null,
                Details = null,
                Timestamps = new ActivityTimestamps(),
                Instance = true
            };

            if (reader.ReadBoolean()) {
                activity.State = reader.ReadString();
            }

            if (reader.ReadBoolean()) {
                activity.Details = reader.ReadString();
            }

            long? startTime = null;
            long? endTime = null;

            if (reader.ReadBoolean()) {
                startTime = ((long) reader.ReadUInt32() << 32) + reader.ReadUInt32();
            }

            if (reader.ReadBoolean()) {
                endTime = ((long) reader.ReadUInt32() << 32) + reader.ReadUInt32();
            }

            var timestamps = new ActivityTimestamps();

            if (startTime != null) {
                timestamps.Start = startTime.Value;
            }

            if (endTime != null) {
                timestamps.End = endTime.Value;
            }

            activity.Timestamps = timestamps;

            if (reader.ReadBoolean()) {
                activity.Assets.LargeImage = reader.ReadString();
            }

            if (reader.ReadBoolean()) {
                activity.Assets.LargeText = reader.ReadString();
            }

            if (reader.ReadBoolean()) {
                activity.Assets.SmallImage = reader.ReadString();
            }

            if (reader.ReadBoolean()) {
                activity.Assets.SmallText = reader.ReadString();
            }

            if (reader.ReadBoolean()) {
                activity.Party.Id = reader.ReadString();
            }

            if (reader.ReadBoolean()) {
                var size = new PartySize();
                size.CurrentSize = reader.ReadInt32();
                size.MaxSize = reader.ReadInt32();
                activity.Party.Size = size;
            }

            if (reader.ReadBoolean()) {
                activity.Secrets.Match = reader.ReadString();
            }

            if (reader.ReadBoolean()) {
                activity.Secrets.Spectate = reader.ReadString();
            }

            if (reader.ReadBoolean()) {
                activity.Secrets.Join = reader.ReadString();
            }

            discord.GetActivityManager().UpdateActivity(activity, (Action<Result>) (r => r.Log(comment: "update activity result")));
        }

        // [HarmonyPatch(typeof(DiscordManager), nameof(DiscordManager.SetInLobbyClient))]
        // public static class SetInLobbyClientPatch {
        //     [PermanentPatch]
        //     [HarmonyPrefix]
        //     public static bool SetInLobbyClient(DiscordManager __instance) {
        //         return false;
        //     }
        // }
        //
        // [HarmonyPatch(typeof(DiscordManager), nameof(DiscordManager.SetInLobbyHost))]
        // public static class SetInLobbyHostPatch {
        //     [PermanentPatch]
        //     [HarmonyPrefix]
        //     public static bool SetInLobbyClient(DiscordManager __instance) {
        //         return false;
        //     }
        // }
        //
        // [HarmonyPatch(typeof(DiscordManager), nameof(DiscordManager.SetPlayingGame))]
        // public static class SetPlayingGamePatch {
        //     [PermanentPatch]
        //     [HarmonyPrefix]
        //     public static bool SetInLobbyClient(DiscordManager __instance) {
        //         return false;
        //     }
        // }

        [HarmonyPatch(typeof(DiscordManager), nameof(DiscordManager.Start))]
        public static class DisableFixedUpdatePatch {
            [PermanentPatch]
            [HarmonyPrefix]
            public static bool Start(DiscordManager __instance) {
                if (DestroyableSingleton<DiscordManager>.Instance != __instance) {
                    return false;
                }

                try {
                    __instance.presence = new Discord.Discord(ClientId, 1UL);
                    __instance.presence.GetActivityManager()
                        .add_OnActivityJoin((Action<string>) HandleActivityJoin);
                    discord = __instance.presence;
                    SussyBaka();
                } catch {
                    Debug.LogWarning("Discord Waaaaaahhhhhhhh");
                }

                return false;
            }
        }
    }
}