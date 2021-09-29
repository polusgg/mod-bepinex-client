using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hazel;
using Polus.Enums;
using Polus.Extensions;
using Polus.Net.Objects;
using TMPro;
using UnhollowerBaseLib.Attributes;
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
        private KeyCode[] keys;

        public bool IsHudButton => netTransform.IsOnHud;
        public bool IsLocked => IsHudButton && locks[(int) ButtonLocks.PlayerCanMove];

        public static bool GetLock(ButtonLocks key) {
            return locks[(int) key];
        }

        static PolusClickBehaviour() {
            ClassInjector.RegisterTypeInIl2Cpp<PolusClickBehaviour>();
        }

        public PolusClickBehaviour(IntPtr ptr) : base(ptr) { }
        
        [HideFromIl2Cpp]
        public static void UnlockAll() {
            locks = new[]{false, false};
        }

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
            CheckLocks();
        }

        private void Update() {
            if (pno.HasData()) {
                Deserialize(pno.GetData());
                CooldownHelpers.SetCooldownNormalizedUvs(graphic.renderer);
            }

            if (keys.Any(Input.GetKeyDown)) OnClick();

            graphic.renderer.enabled = !locks[(int) ButtonLocks.SetHudActive];

            if (isCountingDown) {
                currentTimer -= Time.deltaTime;
                if (currentTimer < 0) currentTimer = 0;
            }

            SetCooldown();
        }

        private void CheckLocks() {
            if (anyLocked == IsLocked) return;
            $"yeah,,, {locks[0]} {locks[1]} {locks.Any(lck => lck)}".Log(3);
            anyLocked = locks[(int) ButtonLocks.PlayerCanMove];
            SetCountingDown(!anyLocked);
        }

        private static void CheckAllLocks() {
            foreach (PolusClickBehaviour polusClickBehaviour in Buttons.Where(btn => btn.IsHudButton)) {
                polusClickBehaviour.CheckLocks();
            }
        }

        public static void SetLock(ButtonLocks key, bool value) {
            lock (locks) {
                locks[(int) key] = value;
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
            keys = new KeyCode[reader.BytesRemaining / 2];
            for (int i = 0; i < keys.Length; i++) keys[i] = (KeyCode) reader.ReadUInt16();
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