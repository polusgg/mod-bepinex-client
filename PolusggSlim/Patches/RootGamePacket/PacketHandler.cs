using System;
using HarmonyLib;
using Hazel;
using InnerNet;
using PolusggSlim.Patches.GameTransitionScreen;
using PolusggSlim.Patches.SetString;
using PolusggSlim.Utils.Extensions;
using TMPro;
using UnityEngine;

namespace PolusggSlim.Patches.RootGamePacket
{
    public static class PacketHandler
    {
        // [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.HandleMessage))]
        public static class HandleMessage
        {
            public static void Prefix(InnerNetClient __instance, [HarmonyArgument(0)] MessageReader reader,
                [HarmonyArgument(1)] SendOption yoMama)
            {
                if (!(reader.Tag > 0x80))
                    return;

                var tag = (RootPacketTypes) reader.Tag;
                //TODO: PggLog.Message($"Received RootGamePacket {reader.Tag} - {tag.ToString()}");

                switch (tag)
                {
                    case RootPacketTypes.FetchResource:
                    {
                        break;
                    }
                    case RootPacketTypes.Resize:
                    {
                        break;
                    }
                    case RootPacketTypes.DisplayStartGameScreen:
                    {
                        IntroCutscenePatch.Data = new CutsceneData
                        {
                            TitleText = reader.ReadString(),
                            SubtitleText = reader.ReadString(),
                            BackgroundColor = reader.ReadColor(),
                            YourTeam = reader.ReadBytes(reader.Length - reader.Position)
                        };

                        break;
                    }
                    case RootPacketTypes.OverwriteGameOver:
                    {
                        EndGameManagerPatch.Data = new EndGameCutsceneData
                        {
                            TitleText = reader.ReadString(),
                            SubtitleText = reader.ReadString(),
                            BackgroundColor = reader.ReadColor(),
                            YourTeam = reader.ReadBytes(reader.Length - reader.Position - 2),
                            DisplayQuit = reader.ReadBoolean(),
                            DisplayPlayAgain = reader.ReadBoolean()
                        };

                        break;
                    }
                    case RootPacketTypes.SetString:
                    {
                        var newString = reader.ReadString();
                        var location = (SetStringLocations) reader.ReadByte();

                        if (location == SetStringLocations.GameCode)
                        {
                            GameStartManager.Instance.GameRoomName.text = newString;
                        }
                        else if (location == SetStringLocations.GamePlayerCount)
                        {
                            GameStartManagerPatch.GamePlayerCount = newString;
                        }
                        else if (location == SetStringLocations.TaskText)
                        {
                            //TODO: Implementation detail uncertain
                        }
                        else if (location == SetStringLocations.RoomTracker)
                        {
                            RoomTrackerPatch.RoomString = newString;
                        }
                        else if (location == SetStringLocations.PingTracker)
                        {
                            PingTrackerPatch.PingText = newString;
                        }
                        else if (location == SetStringLocations.TaskCompletion)
                        {
                            DestroyableSingleton<HudManager>.Instance.TaskCompleteOverlay.GetComponent<TextMeshPro>()
                                .text = newString;
                        }
                        else if (location == SetStringLocations.GameOptions)
                        {
                            GameOptionsHudStringPatch.GameOptionsString = newString;
                        }
                        else
                        {
                            throw new ArgumentOutOfRangeException();
                        }

                        break;
                    }
                    case RootPacketTypes.DeclareHat:
                    {
                        var hat = ScriptableObject.CreateInstance<HatBehaviour>();
                        var bounce = reader.ReadBoolean();
                        var hasBack = reader.ReadBoolean();

                        if (hasBack)
                            hat.BackImage = new Sprite();
                        else
                            hat.MainImage = new Sprite();

                        hat.FloorImage = new Sprite();
                        hat.ClimbImage = new Sprite();
                        hat.ChipOffset = reader.ReadVector2();

                        hat.NoBounce = !bounce;

                        var accessible = reader.ReadBoolean();
                        if (!accessible)
                            hat.LimitedYear = 1900;

                        break;
                    }
                    case RootPacketTypes.DeclarePet:
                    {
                        throw new NotImplementedException();
                    }
                    case RootPacketTypes.DeclareSkin:
                    {
                        throw new NotImplementedException();
                    }
                    case RootPacketTypes.DeclareKillAnim:
                    {
                        throw new NotImplementedException();
                    }
                    case RootPacketTypes.SetGameOption:
                    {
                        throw new NotImplementedException();
                    }
                    case RootPacketTypes.DeleteGameOption:
                    {
                        throw new NotImplementedException();
                    }
                    case RootPacketTypes.DisplaySystemAlert:
                    {
                        throw new NotImplementedException();
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}