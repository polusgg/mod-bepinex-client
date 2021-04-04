using System;
using System.Linq;
using HarmonyLib;
using Hazel;
using PolusApi.Net;
using PolusMod.Enums;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.UI;

namespace PolusMod.Inner {
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
        private TextRenderer timerText;

        private static readonly int Percent = Shader.PropertyToID("_Percent");
        // private 

        private void Start() {
            pno = IObjectManager.Instance.LocateNetObject(this);
            pno.OnData = Deserialize;
            button = GetComponent<PassiveButton>();
            button.OnClick = new Button.ButtonClickedEvent();
            button.OnClick.AddListener(new Action(OnClick));
            button.Colliders = button.Colliders.AddItem(GetComponent<BoxCollider2D>()).ToArray();
            graphic = GetComponent<PolusGraphic>();
            timerText = GetComponentInChildren<TextRenderer>();
            KillButtonManager kb = HudManager.Instance.KillButton;
            graphic.renderer.SetMaterial(kb.renderer.GetMaterial());
            timerText.render.SetMaterial(kb.TimerText.render.GetMaterial());
            timerText.FontData = kb.TimerText.FontData;
        }

        private void FixedUpdate() {
            if (pno.HasSpawnData()) Deserialize(pno.GetSpawnData());
            if (counting) {
                currentTimer -= Time.deltaTime;
                if (currentTimer < 0) currentTimer = 0;
            }
            SetCooldown();
        }

        private void Deserialize(MessageReader reader) {
            if ((maxTimer = reader.ReadSingle()) > 0) {
                currentTimer = reader.ReadSingle();
                counting = reader.ReadBoolean();
                color = new Color32(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
            }
        }

        public void SetCooldown()
        {
            float num = Mathf.Clamp(currentTimer / maxTimer, 0f, 1f);
            graphic.renderer.material.SetFloat(Percent, num);
            bool isCoolingDown = num > 0f && counting;
            if (isCoolingDown)
            {
                timerText.Text = Mathf.CeilToInt(currentTimer).ToString();
                timerText.gameObject.SetActive(true);
                timerText.Color = color;
                return;
            }
            timerText.gameObject.SetActive(false);
        }

        public void OnClick() =>
            AmongUsClient.Instance.SendRpcImmediately(pno.NetId, (byte) PolusRpcCalls.Click, SendOption.Reliable);
    }
}