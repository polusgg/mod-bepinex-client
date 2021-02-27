using PolusApi.Net;
using UnityEngine;

namespace TestMod {
	public class RegisterPnos {
		public static PolusNetObject CreateDeadBodyPrefab() {
			GameObject prefab = new GameObject();
			prefab.hideFlags = HideFlags.HideInHierarchy;
			prefab.active = false;

			Pno.DeadBody deadBody = prefab.AddComponent<Pno.DeadBody>();
			//todo recreate dead body hierarchy here
			return deadBody;
		}
	}
}