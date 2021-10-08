using System;
using System.Collections.Generic;
using BepInEx.Logging;
using Hazel;
using Polus.Enums;
using Polus.Extensions;
using Polus.Utils;
using UnhollowerBaseLib.Attributes;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace Polus.Behaviours {
    public class ChatManager : MonoBehaviour {
        public ChatManager(IntPtr ptr) : base(ptr) { }
        static ChatManager() => ClassInjector.RegisterTypeInIl2Cpp<ChatManager>();
        private static readonly int MaskLayer = Shader.PropertyToID("_MaskLayer");
        private Dictionary<Guid, ChatBubble> Bubbles = new();
        public ChatController Chat;
        public const float DisabledPitch = -1000f;

        public static bool InstanceExists => HudManager.InstanceExists;
        public static ChatManager Instance => InstanceExists ? HudManager.Instance.Chat.TryGetComponent(UnhollowerRuntimeLib.Il2CppType.Of<ChatManager>(), out Component component) ? component.Cast<ChatManager>() : HudManager.Instance.Chat.gameObject.AddComponent<ChatManager>().Initialize() : null;

        private ChatManager Initialize() {
            Chat = HudManager.Instance.Chat;
            return this;
        }

        [HideFromIl2Cpp]
        public void Reset() {
            ":throwinghandsup:".Log();
            Bubbles = new Dictionary<Guid, ChatBubble>();
        }

        public void ReceiveChatMessage(MessageReader reader) {
            if (!HudManager.InstanceExists) {
                "Received a chat message, but there was no hud to display it on, rejected.".Log();
                return;
            }
            Guid guid = reader.ReadGuid().Log(comment: "guid");
            MessageAlignment align = (MessageAlignment) reader.ReadByte();
            string playerName = reader.ReadString();
            bool dead = reader.ReadBoolean();
            bool voted = reader.ReadBoolean();
            uint hatId = reader.ReadPackedUInt32();
            uint petId = reader.ReadPackedUInt32();
            uint skinId = reader.ReadPackedUInt32();
            Color backColor = reader.ReadColor();
            Color bodyColor = reader.ReadColor();
            Color visorColor = reader.ReadColor();
            float pitch = reader.ReadSingle();
            string text = reader.ReadBoolean() ? QuickChatNetData.Deserialize(reader) : reader.ReadString();
            
            $"New chat message {guid} - {playerName}: {text}".Log();

            if (Chat.chatBubPool.NotInUse == 0) Chat.chatBubPool.ReclaimOldest();

            ChatBubble chatBubble = Chat.chatBubPool.GetBetter<ChatBubble>();

            CatchHelper.TryCatch(() => {
                chatBubble.transform.SetParent(Chat.scroller.Inner);
                chatBubble.transform.localScale = Vector3.one;
                switch (align) {
                    case MessageAlignment.Left:
                        chatBubble.SetLeft();
                        break;
                    case MessageAlignment.Right:
                        chatBubble.SetRight();
                        break;
                    case MessageAlignment.Center:
                        chatBubble.SetNotification();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(align));
                }

                int num = 51 + chatBubble.PoolIndex;
                chatBubble.MaskArea.material.SetInt(MaskLayer, num);
                chatBubble.Player.Body.material.SetInt(MaskLayer, num);
                chatBubble.Player.Skin.SetMaskLayer(num);
                chatBubble.Player.HatSlot.SetMaskLayer(num);
                chatBubble.Player.PetSlot.material.SetInt(MaskLayer, num);
                chatBubble.Background.material.SetInt(MaskLayer, num);
                chatBubble.Player.Body.SetPlayerMaterialColors(backColor, bodyColor);
                chatBubble.Player.Body.material.SetColor("_VisorColor", visorColor);
                chatBubble.Xmark.material.SetInt(MaskLayer, num);
                SecondaryHatSpriteBehaviour hat = chatBubble.Player.HatSlot.GetSecondary();
                hat.parent.SetHat(hatId, 0);
                hat.SetColor(backColor, bodyColor);
                chatBubble.Player.PetSlot.SetPetImage(petId, backColor, bodyColor);
                // TODO: Actually display the pet in a good position
                chatBubble.Player.SetSkin(skinId);
                chatBubble.SetName(playerName, dead, voted, Color.white);

                if (SaveManager.CensorChat) text = BlockedWords.CensorWords(text);

                chatBubble.SetText(text);
                chatBubble.AlignChildren();
                Chat.AlignAllBubbles();
                if (!Chat.IsOpen && Chat.notificationRoutine == null) Chat.notificationRoutine = Chat.StartCoroutine(Chat.BounceDot());

                if (Math.Abs(pitch - DisabledPitch) > 0.0001f) SoundManager.Instance.PlaySound(Chat.MessageSound, false, 1f).pitch = pitch;
                Bubbles.Add(guid, chatBubble);
            }, catchAction: () => {
                CatchHelper.TryCatch(() => Chat.chatBubPool.Reclaim(chatBubble));
            });
        }

        public void DeleteMessage(Guid guid)
        {
            ChatBubble bubble = Bubbles[guid];
            string text = bubble.TextArea.text;
            Chat.chatBubPool.Reclaim(bubble);
            Chat.AlignAllBubbles();
            Bubbles.Remove(guid);
            $"Deleted chat message {guid} - {bubble.NameText.text}: {text}".Log();
        }
    }
}