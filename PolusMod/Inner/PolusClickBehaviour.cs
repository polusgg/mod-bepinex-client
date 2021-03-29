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

        private void Start() {
            pno = IObjectManager.Instance.LocateNetObject(this);
            pno.OnData = Deserialize;
            button = GetComponent<PassiveButton>();
            button.OnClick = new Button.ButtonClickedEvent();
            button.OnClick.AddListener(new Action(OnClick));
            button.Colliders = button.Colliders.AddItem(GetComponent<BoxCollider2D>()).ToArray();
            graphic = GetComponent<PolusGraphic>();
        }

        private void FixedUpdate() {
            if (pno.HasSpawnData()) Deserialize(pno.GetSpawnData());
            if (counting) {
                currentTimer -= Time.deltaTime;
                if (currentTimer < 0) currentTimer = 0;
            }
        }

        private void Deserialize(MessageReader reader) {
            if ((maxTimer = reader.ReadSingle()) > 0) {
                currentTimer = reader.ReadSingle();
                counting = reader.ReadBoolean();
                currentTimer = reader.ReadSingle();
            }
        }

        public void OnClick() =>
            AmongUsClient.Instance.SendRpcImmediately(pno.NetId, (byte) PolusRpcCalls.Click, SendOption.Reliable);
    }
}