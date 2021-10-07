using System.Linq;
using Polus.Extensions;
using Polus.Net.Objects;
using Polus.Patches.Permanent;
using UnityEngine;
using UnityEngine.UI;

namespace Polus.Behaviours.Inner {
    public static class RegisterPnos {
        public static PnoBehaviour CreateDeadBodyPrefab(IObjectManager objectManager) {
            PlayerControl pc = AmongUsClient.Instance.PlayerPrefab;
            DeadBody prefab = Object.Instantiate(pc.KillAnimations[0].bodyPrefab);
            prefab.hideFlags = HideFlags.HideInHierarchy;
            GameObject gameObject = prefab.gameObject;
            gameObject.active = false;
            Object.DontDestroyOnLoad(gameObject);

            PolusDeadBody polusDeadBody = gameObject.AddComponent<PolusDeadBody>();
            gameObject.AddComponent<PolusNetworkTransform>();
            
            objectManager.RegisterType<PolusDeadBody>();
            objectManager.RegisterType<PolusNetworkTransform>();

            return polusDeadBody;
        }

        public static PnoBehaviour CreateImage(IObjectManager objectManager) {
            GameObject imageObject = new("Image") {active = false};
            imageObject.DontDestroy();
            objectManager.RegisterType<PolusGraphic>();
            objectManager.RegisterType<PolusNetworkTransform>();
            imageObject.AddComponent<PolusGraphic>();
            return imageObject.AddComponent<PolusNetworkTransform>();
        }

        public static PnoBehaviour CreateButton(IObjectManager objectManager) {
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
            
            objectManager.RegisterType<PolusNetworkTransform>();
            objectManager.RegisterType<PolusGraphic>();
            objectManager.RegisterType<PolusClickBehaviour>();

            return button;
        }

        public static PnoBehaviour CreatePrefabHandle(IObjectManager objectManager) {
            GameObject prefabObject = new("PrefabHandle") {active = false};
            prefabObject.DontDestroy();

            objectManager.RegisterType<PolusPrefabHandle>();
            objectManager.RegisterType<PolusNetworkTransform>();
            prefabObject.AddComponent<PolusPrefabHandle>();
            return prefabObject.AddComponent<PolusNetworkTransform>();
        }

        public static PnoBehaviour CreatePoi(IObjectManager objectManager) {
            GameObject pointyObject = new("PointOfInterest") {active = false};
            pointyObject.DontDestroy();

            objectManager.RegisterType<PolusPoi>();
            objectManager.RegisterType<PolusGraphic>();
            objectManager.RegisterType<PolusNetworkTransform>();
            
            pointyObject.AddComponent<PolusPoi>();
            pointyObject.AddComponent<PolusGraphic>();
            return pointyObject.AddComponent<PolusNetworkTransform>();
        }

        public static PnoBehaviour CreateConsole(IObjectManager objectManager) {
            GameObject burger = new("Console") {active = false};
            burger.DontDestroy();

            objectManager.RegisterType<PolusGraphic>();
            objectManager.RegisterType<PolusNetworkTransform>();
            burger.AddComponent<PolusConsole>();
            return burger.AddComponent<PolusNetworkTransform>();
        }

        public static PnoBehaviour CreateSoundSource(IObjectManager objectManager) {
            GameObject wap = new("SoundSource") {active = false};
            wap.DontDestroy();

            wap.AddComponent<PolusSoundSource>();
            wap.AddComponent<AudioSource>();
            objectManager.RegisterType<PolusSoundSource>();
            objectManager.RegisterType<PolusNetworkTransform>();
            return wap.AddComponent<PolusNetworkTransform>();
        }

        public static PnoBehaviour CreateVent(IObjectManager objectManager) {
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

            objectManager.RegisterType<PolusVent>();
            return pee.AddComponent<PolusVent>();
        }

        public static PnoBehaviour CreateCameraController(IObjectManager objectManager) {
            GameObject cameraController = new("uwuCameraManager") {active = false};
            cameraController.DontDestroy();
            objectManager.RegisterType<PolusCameraController>();
            return cameraController.AddComponent<PolusCameraController>();
        }
    }
}