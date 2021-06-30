using InnerNet;
using UnityEngine;

namespace PolusGG.Behaviours.Ino {
    public class RegisterInos {
        public static InnerNetObject CreateDeadBodyPrefab() {
            PlayerControl pc = AmongUsClient.Instance.PlayerPrefab;
            DeadBody prefab = Object.Instantiate(pc.KillAnimations[0].bodyPrefab);
            prefab.hideFlags = HideFlags.HideInHierarchy;
            GameObject gameObject = prefab.gameObject;
            gameObject.active = false;
            Object.DontDestroyOnLoad(gameObject);

            PogusDeadBody polusDeadBody = gameObject.AddComponent<PogusDeadBody>();
            gameObject.AddComponent<PogusNetTransform>();

            return polusDeadBody;
        }
    }
}