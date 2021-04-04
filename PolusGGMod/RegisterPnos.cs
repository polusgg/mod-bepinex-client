using PolusApi;
using PolusApi.Net;
using PolusMod.Inner;
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

			PolusDeadBody polusDeadBody = gameObject.AddComponent<PolusDeadBody>();
			gameObject.AddComponent<PolusNetworkTransform>();
			gameObject.AddComponent<PolusClickBehaviour>();
			AspectPosition position = gameObject.AddComponent<AspectPosition>();
			position.enabled = false;

			return polusDeadBody;
		}

		public static PnoBehaviour CreateImage() {
			GameObject imageObject = new("Button") {active = false};
			imageObject.AddComponent<PolusNetworkTransform>();
			return imageObject.AddComponent<PolusGraphic>();
		}
		public static PnoBehaviour CreateButton() {
			GameObject imageObject = new("Button") {active = false};
			imageObject.DontDestroy();
			GameObject timerObject = new("Cooldown");
			timerObject.transform.parent = imageObject.transform;

			imageObject.AddComponent<PolusNetworkTransform>();
			imageObject.AddComponent<PolusGraphic>();
			PolusClickBehaviour button = imageObject.AddComponent<PolusClickBehaviour>();
			imageObject.AddComponent<SpriteRenderer>();
			BoxCollider2D collider = imageObject.AddComponent<BoxCollider2D>();
			collider.size = new Vector2(1.15f, 1.15f);
			imageObject.AddComponent<PassiveButton>();

			timerObject.AddComponent<TextRenderer>();

			return button;
		}
	}
}