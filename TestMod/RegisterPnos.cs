using System;
using HarmonyLib;
using PolusApi.Net;
using PowerTools;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TestMod {
	public class RegisterPnos {
		public static PolusNetObject CreateDeadBodyPrefab() {
			PlayerControl pc = (PlayerControl) AmongUsClient.Instance.SpawnableObjects[4];
			DeadBody prefab = Object.Instantiate(pc.KillAnimations[0].bodyPrefab);
			prefab.hideFlags = HideFlags.HideInHierarchy;
			prefab.gameObject.active = false;

			Pno.PolusDeadBody polusDeadBody = prefab.gameObject.AddComponent<Pno.PolusDeadBody>();
			polusDeadBody.anim = prefab.GetComponent<SpriteAnim>();
			polusDeadBody.deadBody = prefab;

			//todo recreate dead body hierarchy here
			return polusDeadBody;
		}
	}
}