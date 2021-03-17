using System;
using Hazel;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PolusApi.Net {
    public class PolusNetObject : MonoBehaviour {
	    public uint SpawnId;
	    public uint NetId;

	    public PolusNetObject(IntPtr ptr) : base(ptr) {}

		public virtual void Despawn() {
			Destroy(gameObject);
		}

		public virtual void HandleRpc(byte callId, MessageReader reader) {
			
		}

		public virtual void Deserialize(MessageReader reader, bool initialState) {
		}
    }
}