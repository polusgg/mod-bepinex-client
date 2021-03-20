using System;
using System.Buffers.Binary;
using System.Linq;
using HarmonyLib;
using Hazel;
using PolusApi.Net;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.UI;

namespace PolusMod.Pno {
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