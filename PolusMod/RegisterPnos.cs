using PolusApi.Net;
using PolusMod.Pno;
using PowerTools;
using UnityEngine;

namespace PolusMod {
	public class RegisterPnos {
		public static PolusNetObject CreateDeadBodyPrefab() {
			PlayerControl pc = AmongUsClient.Instance.PlayerPrefab;
			DeadBody prefab = Object.Instantiate(pc.KillAnimations[0].bodyPrefab);
			prefab.hideFlags = HideFlags.HideInHierarchy;
			var gameObject = prefab.gameObject;
			gameObject.active = false;
			Object.DontDestroyOnLoad(gameObject);
			gameObject.AddComponent<Rigidbody2D>();

			PolusDeadBody polusDeadBody = prefab.gameObject.AddComponent<PolusDeadBody>();
			polusDeadBody.anim = prefab.GetComponent<SpriteAnim>();
			polusDeadBody.deadBody = prefab;
			polusDeadBody.netTransform = prefab.gameObject.AddComponent<PolusNetworkTransform>();

			return polusDeadBody;
		}
	}
}