using System;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace Polus.Behaviours {
    public class PlayOnlineButtonManager : MonoBehaviour {
        public PlayOnlineButtonManager(IntPtr ptr) : base(ptr) {}
        static PlayOnlineButtonManager() => ClassInjector.RegisterTypeInIl2Cpp<PlayOnlineButtonManager>();

        private SpriteRenderer renderer;
        private PassiveButton button;
        private ButtonRolloverHandler rolloverHandler;

        private void Start() {
            GameObject play = GameObject.Find("PlayOnlineButton");
            transform.parent = play.transform; 
            renderer = play.GetComponent<SpriteRenderer>();
            button = play.GetComponent<PassiveButton>();
            rolloverHandler = play.GetComponent<ButtonRolloverHandler>();
        }

        private void Update() {
            transform.position = new Vector3(0, -0.95f, 0);
            button.enabled = true;
            if (rolloverHandler.OverColor != Color.green) {
                rolloverHandler.SetEnabledColors();
            }
        }
    }
}