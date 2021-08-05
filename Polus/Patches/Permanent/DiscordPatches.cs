using System;
using Discord;
using HarmonyLib;
using Hazel;
using Polus.Mods.Patching;
using UnityEngine;

namespace Polus.Patches.Permanent
{
    public class DiscordPatches
    {
        public static Discord.Discord discord;
        
        public static void HandleActivityJoin(string joinSecret)
        {
            //todo
        }

        public static void UpdateRichPresence(MessageReader reader)
        {
            Activity activity = new();

            if (reader.ReadBoolean()) {activity.State = reader.ReadString();}
            if (reader.ReadBoolean()) {activity.Details = reader.ReadString();}
            if (reader.ReadBoolean()) {var a = activity.Timestamps; a.Start = ((long)reader.ReadUInt32() << 32) + reader.ReadUInt32();}
            if (reader.ReadBoolean()) {var a = activity.Timestamps; a.End = ((long)reader.ReadUInt32() << 32) + reader.ReadUInt32();}
            if (reader.ReadBoolean()) {var a = activity.Assets; a.LargeImage = reader.ReadString();}
            if (reader.ReadBoolean()) {var a = activity.Assets; a.LargeText = reader.ReadString();}
            if (reader.ReadBoolean()) {var a = activity.Assets; a.SmallImage = reader.ReadString();}
            if (reader.ReadBoolean()) {var a = activity.Assets; a.SmallText = reader.ReadString();}
            if (reader.ReadBoolean()) {var a = activity.Party; a.Id = reader.ReadString();}
            if (reader.ReadBoolean()) {var a = activity.Party; a.Size = new PartySize();
                var p = a.Size;
                p.CurrentSize = reader.ReadInt32();
                p.MaxSize = reader.ReadInt32();
            }
            if (reader.ReadBoolean()) {var a = activity.Secrets; a.Match = reader.ReadString();}
            if (reader.ReadBoolean()) {var a = activity.Secrets; a.Spectate = reader.ReadString();}
            if (reader.ReadBoolean()) {var a = activity.Secrets; a.Join = reader.ReadString();}
            
            discord.GetActivityManager().UpdateActivity(activity, (Action<Result>) delegate(Result r) { });
        }
        
        [HarmonyPatch(typeof(DiscordManager), nameof(DiscordManager.Start))]
        public static class DisableFixedUpdatePatch {
            [PermanentPatch]
            [HarmonyPrefix]
            public static bool Start(DiscordManager __instance)
            {
                if (DestroyableSingleton<DiscordManager>.Instance != __instance)
                {
                    return false;
                }

                try
                {
                    __instance.presence = new Discord.Discord(832788555583062036L, 1UL);
                    __instance.presence.GetActivityManager()
                        .add_OnActivityJoin((Action<string>) DiscordPatches.HandleActivityJoin);
                    DiscordPatches.discord = __instance.presence;
                }
                catch
                {
                    Debug.LogWarning("Discord Waaaaaahhhhhhhh");
                }

                return false;
            }
        }
    }
}