using System;
using HarmonyLib;
using PolusApi.Net;
using PowerTools;
using UnhollowerRuntimeLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TestMod {
	public class RegisterPnos {
		public static PolusNetObject CreateDeadBodyPrefab() {
			PlayerControl pc = AmongUsClient.Instance.PlayerPrefab;
			DeadBody prefab = Object.Instantiate(pc.KillAnimations[0].bodyPrefab);
			prefab.hideFlags = HideFlags.HideInHierarchy;
			prefab.gameObject.active = false;

			Pno.PolusDeadBody polusDeadBody = prefab.gameObject.AddComponent<Pno.PolusDeadBody>();
			polusDeadBody.anim = prefab.GetComponent<SpriteAnim>();
			polusDeadBody.deadBody = prefab;

			return polusDeadBody;
		}
	}
}