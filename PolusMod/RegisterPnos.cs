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

			PolusDeadBody polusDeadBody = gameObject.AddComponent<PolusDeadBody>();
			polusDeadBody.netTransform = gameObject.AddComponent<PolusNetworkTransform>();
			AspectPosition position = gameObject.AddComponent<AspectPosition>();
			position.enabled = false;

			return polusDeadBody;
		}
		public static PnoBehaviour CreateButton() {
			GameObject imageObject = new("UnityExplorerAndies") {active = false};
			Object.DontDestroyOnLoad(imageObject);
			GameObject timerObject = new("SussyCooldown");

			PolusButton button = imageObject.AddComponent<PolusButton>();
			imageObject.AddComponent<PolusGraphic>();
			imageObject.AddComponent<PolusNetworkTransform>();
			imageObject.AddComponent<SpriteRenderer>();
			imageObject.AddComponent<BoxCollider2D>();
			imageObject.AddComponent<PassiveButton>();

			timerObject.AddComponent<TextRenderer>();

			return button;
		}
	}
}