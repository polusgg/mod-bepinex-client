using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hazel;
using Polus.Enums;
using Polus.Extensions;
using Polus.Net.Objects;
using TMPro;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.UI;

namespace Polus.Behaviours.Inner {
    public class PolusClickBehaviour : PnoBehaviour {
        private static readonly int Percent = Shader.PropertyToID("_Percent");
        private static readonly int Desat = Shader.PropertyToID("_Desat");
        private static bool[] locks = {false, false};
        private bool anyLocked;
        public static readonly List<PolusClickBehaviour> Buttons = new();
        private static Material _funnyButtonMaterial;
        public PolusNetworkTransform netTransform;
        private PolusGraphic graphic;
        private PassiveButton button;
        private Color32 color;
        private bool saturated;
        private bool isCountingDown;
        private float currentTimer;
        private float maxTimer;
        private TMP_Text timerText;

        public bool IsHudButton => netTransform.IsHudButton;
        public bool IsLocked => IsHudButton && locks.Any(lck => lck);

        static PolusClickBehaviour() {
            ClassInjector.RegisterTypeInIl2Cpp<PolusClickBehaviour>();
        }

        public PolusClickBehaviour(IntPtr ptr) : base(ptr) { }

        private void Start() {
            Buttons.Add(this);
            button = GetComponent<PassiveButton>();
            netTransform = GetComponent<PolusNetworkTransform>();
            button.OnClick = new Button.ButtonClickedEvent();
            button.OnClick.AddListener(new Action(OnClick));
            button.Colliders = button.Colliders.AddItem(GetComponent<BoxCollider2D>()).ToArray();
            graphic = GetComponent<PolusGraphic>();
            KillButtonManager kb = HudManager.Instance.KillButton;
            if (!_funnyButtonMaterial) {
                _funnyButtonMaterial = Instantiate(kb.renderer.GetMaterial()).DontDestroy();
                _funnyButtonMaterial.name.Log();
            }
            graphic.renderer.SetMaterial(_funnyButtonMaterial);
            timerText = Instantiate(kb.TimerText, transform);
        }

        private void Update() {
            if (pno.HasData()) {
                Deserialize(pno.GetSpawnData());
                CooldownHelpers.SetCooldownNormalizedUvs(graphic.renderer);
            }

            graphic.renderer.enabled = !locks[(int) ButtonLocks.SetHudActive];

            if (isCountingDown) {
                currentTimer -= Time.deltaTime;
                if (currentTimer < 0) currentTimer = 0;
            }

            SetCooldown();
        }

        private void CheckLocks() {
            $"yeah,,, {locks[0]} {locks[1]} {locks.Any(lck => lck)}".Log(3);
            if (anyLocked == IsLocked) return;
            anyLocked = locks.Any(lck => lck);
                SetCountingDown(!anyLocked);
        }

        private static void CheckAllLocks() {
            foreach (PolusClickBehaviour polusClickBehaviour in Buttons.Where(btn => btn.IsHudButton)) {
                polusClickBehaviour.CheckLocks();
            }
        }

        public static void SetLock(ButtonLocks index, bool value) {
            lock (locks) {
                locks[(int) index] = value;
                CheckAllLocks();
            }
        }

        private void OnDestroy() {
            Buttons.Remove(this);
            if (!PlayerControl.LocalPlayer || PlayerControl.LocalPlayer.CanMove) SetLock(ButtonLocks.PlayerCanMove, false);
            if (!HudManager.InstanceExists || HudManager.Instance.UseButton.gameObject.active) SetLock(ButtonLocks.SetHudActive, false);
        }

        private void Deserialize(MessageReader reader) {
            maxTimer = reader.ReadSingle();
            currentTimer = reader.ReadSingle();
            isCountingDown = reader.ReadBoolean().Log(3, "now counting down");
            saturated = reader.ReadBoolean();
            color = new Color32(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
        }

        public void SetCooldown() {
            float num = Mathf.Clamp(currentTimer / maxTimer, 0f, 1f);
            graphic.renderer.material.SetFloat(Percent, num);
            graphic.renderer.material.SetFloat(Desat, saturated ? 0f : 1f);
            graphic.renderer.color = saturated ? Palette.EnabledColor : Palette.DisabledClear;
            bool isCoolingDown = currentTimer > 0f;
            timerText.gameObject.SetActive(isCoolingDown && graphic.renderer.enabled);
            if (isCoolingDown) {
                if (currentTimer == 0) return;
                timerText.text = Mathf.CeilToInt(currentTimer).ToString();
                timerText.color = color;
                // return;
            }

            // graphic.renderer.material.SetFloat(Desat, 0f);
            // timerText.gameObject.SetActive(false);
            // graphic.renderer.color = Palette.EnabledColor;
        }

        public void SetCountingDown(bool shouldCountDown) {
            // TODO ANTI-CHEAT THIS USING CURRENT TIME DIFFERENCE OF AT MOST 6 SECONDS
            // TODO MAKE SURE THAT THIS IS LIMITED ON THE SERVER TO PREVENT EARLY COOLDOWN BYPASS
            // TODO DON'T BE STUPID DUMB IDIOT WHEN WRITING THAT ANTI-CHEAT CODE

            shouldCountDown.Log(comment: "button is counting down");
            
            MessageWriter writer = AmongUsClient.Instance.StartRpc(pno.NetId, (byte) PolusRpcCalls.SetCountingDown, SendOption.Reliable);
            // this.isCountingDown = isCountingDown;
            writer.Write(shouldCountDown);
            writer.Write(currentTimer);
            writer.EndMessage();
            // AmongUsClient.Instance.FinishRpcImmediately(writer);
            
        }

        public void OnClick() {
            if (IsLocked) return;
            AmongUsClient.Instance.SendRpcImmediately(pno.NetId, (byte) PolusRpcCalls.Click);
        }
    }
}