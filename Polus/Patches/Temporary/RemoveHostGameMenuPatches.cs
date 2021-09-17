using System;
using Hazel;
using Polus.Enums;
using Polus.Extensions;
using Polus.Utils;
using PowerTools;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Polus.Patches.Temporary {
    public class RemoveHostGameMenuPatches {
        public static void ChangeCreateGame() {
            JoinGameButton jgb = GameObject.Find("JoinGameButton").GetComponent<JoinGameButton>();
            GameObject cgb = GameObject.Find("CreateGameButton");
            GameObject connect = Object.Instantiate(jgb.connectIcon.gameObject, cgb.transform.parent, false);
            // connect.transform.localPosition = jgb.connectIcon.transform.localPosition;
            HostGameButton host = cgb.AddComponent<HostGameButton>();
            host.connectClip = jgb.connectClip;
            host.connectIcon = connect.GetComponent<SpriteAnim>();
            host.targetScene = GameScenes.OnlineGame;
            host.GameMode = GameModes.OnlineGame;
            PassiveButton pb = cgb.GetComponent<PassiveButton>();
            CatchHelper.TryCatch(()=>pb.ClickSound = Object.Instantiate(GameObject.Find("arrowEnter").GetComponent<PassiveButton>().ClickSound));
            (pb.OnClick = new Button.ButtonClickedEvent()).AddListener((Action) (() => {
                if (!AmongUsClient.Instance.AmConnected) host.OnClick();
            }));
        } 
    }
}