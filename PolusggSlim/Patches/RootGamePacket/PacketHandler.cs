using System;
using HarmonyLib;
using Hazel;
using InnerNet;
using PolusggSlim.Patches.GameTransitionScreen;
using PolusggSlim.Utils;
using PolusggSlim.Utils.Extensions;

namespace PolusggSlim.Patches.RootGamePacket
{
    public static class PacketHandler
    {
        [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.HandleMessage))]
        public static class HandleMessage
        {
            public static void Prefix(InnerNetClient __instance, [HarmonyArgument(0)] MessageReader reader, [HarmonyArgument(1)] SendOption yoMama)
            {
                if (!(reader.Tag > 0x80))
                    return;

                var tag = (RootPacketTypes) reader.Tag;
                PggLog.Message($"Received RootGamePacket {reader.Tag} - {tag.ToString()}");

                switch (tag)
                {
                    case RootPacketTypes.FetchResource:
                        break;
                    case RootPacketTypes.Resize:
                        break;
                    case RootPacketTypes.DisplayStartGameScreen:
                        IntroCutscenePatch.Data = new CutsceneData
                        {
                            TitleText =  reader.ReadString(),
                            SubtitleText = reader.ReadString(),
                            BackgroundColor =  reader.ReadColor(),
                            YourTeam = reader.ReadBytes(reader.Length - reader.Position)
                        };
                        
                        break;
                    case RootPacketTypes.OverwriteGameOver:
                        break;
                    case RootPacketTypes.SetString:
                        break;
                    case RootPacketTypes.DeclareHat:
                        break;
                    case RootPacketTypes.DeclarePet:
                        break;
                    case RootPacketTypes.DeclareSkin:
                        break;
                    case RootPacketTypes.DeclareKillAnim:
                        break;
                    case RootPacketTypes.SetGameOption:
                        break;
                    case RootPacketTypes.DeleteGameOption:
                        break;
                    case RootPacketTypes.DisplaySystemAlert:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}