using System.Linq;
using PolusGG.Extensions;
using PolusGG.Net;
using UnityEngine;
using UnityEngine.UI;

namespace PolusGG.Behaviours.Inner {
    public class RegisterPnos {
        public static PnoBehaviour CreateDeadBodyPrefab() {
            PlayerControl pc = AmongUsClient.Instance.PlayerPrefab;
            DeadBody prefab = Object.Instantiate(pc.KillAnimations[0].bodyPrefab);
            prefab.hideFlags = HideFlags.HideInHierarchy;
            GameObject gameObject = prefab.gameObject;
            gameObject.active = false;
            Object.DontDestroyOnLoad(gameObject);

            PolusDeadBody polusDeadBody = gameObject.AddComponent<PolusDeadBody>();
            gameObject.AddComponent<PolusNetworkTransform>();
            gameObject.AddComponent<PolusClickBehaviour>();

            return polusDeadBody;
        }

        public static PnoBehaviour CreateImage() {
            GameObject imageObject = new("Image") {active = false};
            imageObject.DontDestroy();
            imageObject.AddComponent<PolusGraphic>();
            return imageObject.AddComponent<PolusNetworkTransform>();
        }

        public static PnoBehaviour CreateButton() {
            GameObject buttonObject = new("Button") {active = false};
            buttonObject.DontDestroy();

            buttonObject.AddComponent<PolusNetworkTransform>();
            buttonObject.AddComponent<PolusGraphic>();
            PolusClickBehaviour button = buttonObject.AddComponent<PolusClickBehaviour>();
            buttonObject.AddComponent<SpriteRenderer>();
            BoxCollider2D collider = buttonObject.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(1.15f, 1.15f);
            collider.isTrigger = true;
            buttonObject.AddComponent<PassiveButton>();

            return button;
        }

        public static PnoBehaviour CreatePrefabHandle() {
            GameObject prefabObject = new("PrefabHandle") {active = false};
            prefabObject.DontDestroy();

            prefabObject.AddComponent<PolusPrefabHandle>();
            return prefabObject.AddComponent<PolusNetworkTransform>();
        }

        public static PnoBehaviour CreatePoi() {
            GameObject pointyObject = new("PointOfInterest") {active = false};
            pointyObject.DontDestroy();

            pointyObject.AddComponent<PolusPoi>();
            return pointyObject.AddComponent<PolusNetworkTransform>();
        }

        public static PnoBehaviour CreateConsole() {
            GameObject burger = new("Console") {active = false};
            burger.DontDestroy();

            burger.AddComponent<PolusConsole>();
            return burger.AddComponent<PolusNetworkTransform>();
        }

        public static PnoBehaviour CreateSoundSource() {
            GameObject wap = new("SoundSource") {active = false};
            wap.DontDestroy();

            wap.AddComponent<PolusSoundSource>();
            wap.AddComponent<AudioSource>();
            return wap.AddComponent<PolusNetworkTransform>();
        }

        public static PnoBehaviour CreateVent() {
            GameObject pee = new("Venting") {active = false};
            pee.DontDestroy();

            BoxCollider2D collidey = pee.AddComponent<BoxCollider2D>();
            collidey.size = new Vector2(0.75f, 0.34f);
            pee.AddComponent<SpriteRenderer>();
            Vent vent = pee.AddComponent<Vent>();
            vent.Buttons = Enumerable.Range(0, 3).Select(_ => {
                GameObject obj = new("Arrow");
                obj.transform.parent = pee.transform;
                obj.AddComponent<BoxCollider2D>().size = new Vector2(1.22f, 0.75f);
                obj.AddComponent<SpriteRenderer>();
                ButtonBehavior buttonBehavior = obj.AddComponent<ButtonBehavior>();
                buttonBehavior.OnClick = new Button.ButtonClickedEvent();
                buttonBehavior.OnDown = true;
                buttonBehavior.OnUp = false;
                return buttonBehavior;
            }).ToArray();

            return pee.AddComponent<PolusVent>();
        }

        public static PnoBehaviour CreateCameraController() {
            GameObject cameraController = new("uwuCameraManager") {active = false};
            cameraController.DontDestroy();
            return cameraController.AddComponent<PolusCameraController>();
        }
    }
}