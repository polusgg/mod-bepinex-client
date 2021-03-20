using PolusApi.Net;
using PolusMod.Pno;
using PowerTools;
using UnityEngine;

namespace PolusMod {
	public class RegisterPnos {
		public static PnoBehaviour CreateDeadBodyPrefab() {
			PlayerControl pc = AmongUsClient.Instance.PlayerPrefab;
			DeadBody prefab = Object.Instantiate(pc.KillAnimations[0].bodyPrefab);
			prefab.hideFlags = HideFlags.HideInHierarchy;
			var gameObject = prefab.gameObject;
			gameObject.active = false;
			Object.DontDestroyOnLoad(gameObject);
			gameObject.AddComponent<Rigidbody2D>();

			PolusDeadBody polusDeadBody = prefab.gameObject.AddComponent<PolusDeadBody>();
			polusDeadBody.netTransform = prefab.gameObject.AddComponent<PolusNetworkTransform>();

			return polusDeadBody;
		}
		public static PnoBehaviour CreateImage() {
			GameObject imageObject = new("ImagePrefab") {active = false};
			Object.DontDestroyOnLoad(imageObject);

			PolusClickBehaviour cbp = imageObject.AddComponent<PolusClickBehaviour>();
			imageObject.AddComponent<PolusGraphic>();
			imageObject.AddComponent<PolusNetworkTransform>();
			imageObject.AddComponent<SpriteRenderer>();
			imageObject.AddComponent<BoxCollider2D>();
			imageObject.AddComponent<PassiveButton>();

			return cbp;
		}
	}
}