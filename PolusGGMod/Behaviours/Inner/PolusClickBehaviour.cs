#if no
using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hazel;
using PolusGG.Enums;
using PolusGG.Extensions;
using PolusGG.Net;
using TMPro;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace PolusGG.Behaviours.Inner {
    public class PolusClickBehaviour : PnoBehaviour {
        private static readonly int Percent = Shader.PropertyToID("_Percent");
        private static readonly int Desat = Shader.PropertyToID("_Desat");
        internal static readonly List<PolusClickBehaviour> Buttons = new();
        private static Material funnyButtonMaterial;
        internal PolusNetworkTransform netTransform;
        private PolusGraphic graphic;
        private PassiveButton button;
        private Color32 color;
        private bool active;
        private bool isCountingDown;
        private bool lastCounting;
        private float currentTimer;
        private float maxTimer;
        private TMP_Text timerText;

        static PolusClickBehaviour() {
            ClassInjector.RegisterTypeInIl2Cpp<PolusClickBehaviour>();
        }

        public PolusClickBehaviour(IntPtr ptr) : base(ptr) { }

        private void Start() {
            pno = PogusPlugin.ObjectManager.LocateNetObject(this);
            Buttons.Add(this);
            button = GetComponent<PassiveButton>();
            netTransform = GetComponent<PolusNetworkTransform>();
            button.OnClick = new Button.ButtonClickedEvent();
            button.OnClick.AddListener(new Action(OnClick));
            button.Colliders = button.Colliders.AddItem(GetComponent<BoxCollider2D>()).ToArray();
            graphic = GetComponent<PolusGraphic>();
            KillButtonManager kb = HudManager.Instance.KillButton;
            if (!funnyButtonMaterial) {
                funnyButtonMaterial = Instantiate(kb.renderer.GetMaterial()).DontDestroy();
                funnyButtonMaterial.name.Log();
            }
            graphic.renderer.SetMaterial(funnyButtonMaterial);
            timerText = Instantiate(kb.TimerText, transform);
        }

        private void Update() {
            if (pno.HasData()) {
                Deserialize(pno.GetSpawnData());
                CooldownHelpers.SetCooldownNormalizedUvs(graphic.renderer);
            }

            if (netTransform._aspectPosition.Alignment == 0 && lastCounting != PlayerControl.LocalPlayer.CanMove) {
                lastCounting = PlayerControl.LocalPlayer.CanMove;
                SetCountingDown(lastCounting);
            }

            if (isCountingDown) {
                currentTimer -= Time.deltaTime;
                if (currentTimer < 0) currentTimer = 0;
            }

            SetCooldown();
        }

        private void OnEnable() {
            SetCountingDown(true);
        }

        private void OnDisable() {
            SetCountingDown(false);
        }

        private void OnDestroy() {
            Buttons.Remove(this);
        }

        private void Deserialize(MessageReader reader) {
            "Button changed!!!!!"
                .Log();            maxTimer = reader.ReadSingle();
            currentTimer = reader.ReadSingle();
            isCountingDown = reader.ReadBoolean();
            active = reader.ReadBoolean();
            color = new Color32(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
        }

        public void SetCooldown() {
            // return;
            float num = Mathf.Clamp(currentTimer / maxTimer, 0f, 1f);
            graphic.renderer.material.SetFloat(Percent, num);
            bool isCoolingDown = num > 0f || isCountingDown || !PlayerControl.LocalPlayer.CanMove;
            graphic.renderer.material.SetFloat(Desat, isCoolingDown || !active ? 1f : 0f);
            if (isCoolingDown) {
                graphic.renderer.color = Palette.DisabledClear;
                timerText.gameObject.SetActive(currentTimer != 0);
                if (currentTimer == 0) return;
                timerText.text = Mathf.CeilToInt(currentTimer).ToString();
                timerText.color = color;
                return;
            }

            timerText.gameObject.SetActive(false);
            graphic.renderer.color = Palette.EnabledColor;
        }

        public void SetCountingDown(bool isCountingDown) {
            // TODO ANTI-CHEAT THIS USING CURRENT TIME DIFFERENCE OF 6ISH SECONDS
            // TODO MAKE SURE THAT THIS IS LIMITED ON THE SERVER TO PREVENT EARLY COOLDOWN BYPASS
            // TODO DON'T BE STUPID DUMB IDIOT WHEN WRITING THAT ANTI-CHEAT CODE

            isCountingDown.Log(comment: "YOU");
            
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(pno.NetId, (byte) PolusRpcCalls.SetCountingDown, SendOption.Reliable);
            // this.isCountingDown = isCountingDown;
            writer.Write(isCountingDown);
            writer.Write(currentTimer);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public void OnClick() => AmongUsClient.Instance.SendRpcImmediately(pno.NetId, (byte) PolusRpcCalls.Click);
    }
}
#endif