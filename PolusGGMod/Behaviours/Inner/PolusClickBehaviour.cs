using System;
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

namespace PolusGG.Behaviours.Inner {
    public class PolusClickBehaviour : PnoBehaviour {
        public PolusClickBehaviour(IntPtr ptr) : base(ptr) { }

        static PolusClickBehaviour() {
            ClassInjector.RegisterTypeInIl2Cpp<PolusClickBehaviour>();
        }

        private float maxTimer;
        private float currentTimer;
        private bool counting;
        private Color32 color;
        private PassiveButton button;
        private PolusGraphic graphic;
        private TMP_Text timerText;

        private static readonly int Percent = Shader.PropertyToID("_Percent");
        private static readonly int Desat = Shader.PropertyToID("_Desat");

        private void Start() {
            pno = PogusPlugin.ObjectManager.LocateNetObject(this);
            pno.OnData = Deserialize;
            button = GetComponent<PassiveButton>();
            button.OnClick = new Button.ButtonClickedEvent();
            button.OnClick.AddListener(new Action(OnClick));
            button.Colliders = button.Colliders.AddItem(GetComponent<BoxCollider2D>()).ToArray();
            graphic = GetComponent<PolusGraphic>();
            KillButtonManager kb = HudManager.Instance.KillButton;
            Material m = kb.renderer.GetMaterial();
            m.name.Log();
            graphic.renderer.SetMaterial(m);
            timerText = Instantiate(kb.TimerText, transform);
        }

        private void FixedUpdate() {
            if (pno.HasSpawnData()) {
                Deserialize(pno.GetSpawnData());
                CooldownHelpers.SetCooldownNormalizedUvs(graphic.renderer);
            }

            if (counting) {
                currentTimer -= Time.fixedDeltaTime;
                if (currentTimer < 0) currentTimer = 0;
            }

            SetCooldown();
        }

        private void Deserialize(MessageReader reader) {
            if ((maxTimer = reader.ReadSingle()) > 0) {
                currentTimer = reader.ReadSingle();
                counting = reader.ReadBoolean();
                color = new Color32(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
            } else {
                counting = false;
                currentTimer = 0;
                color = Palette.EnabledColor;
            }
        }

        public void SetCooldown() {
            // return;
            float num = Mathf.Clamp(currentTimer / maxTimer, 0f, 1f);
            graphic.renderer.material.SetFloat(Percent, num);
            bool isCoolingDown = num > 0f && counting && PlayerControl.LocalPlayer.CanMove;
            if (isCoolingDown) {
                graphic.renderer.material.SetFloat(Desat, 1f);
                graphic.renderer.color = Palette.EnabledColor;
                timerText.text = Mathf.CeilToInt(currentTimer).ToString();
                timerText.gameObject.SetActive(true);
                timerText.color = color;
                return;
            }

            timerText.gameObject.SetActive(false);
            graphic.renderer.color = Palette.EnabledColor;
            graphic.renderer.material.SetFloat(Desat, 0f);
        }

        public void OnClick() =>
            AmongUsClient.Instance.SendRpcImmediately(pno.NetId, (byte) PolusRpcCalls.Click);
    }
}