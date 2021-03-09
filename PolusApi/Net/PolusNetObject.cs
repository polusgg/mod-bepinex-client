using System;
using Hazel;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PolusApi.Net {
    public abstract class PolusNetObject : MonoBehaviour, IComparable<InnerNetObject>, IComparable<PolusNetObject> {
	    public PolusNetObject(IntPtr ptr) : base(ptr) {}
		public bool AmOwner
		{
			get
			{
				return OwnerId == AmongUsClient.Instance.ClientId;
			}
		}

		public void Despawn() {
			Destroy(gameObject);
		}

		public virtual void PreSerialize()
		{
		}

		public abstract void HandleRpc(byte callId, MessageReader reader);

		public abstract bool Serialize(MessageWriter writer, bool initialState);

		public abstract void Deserialize(MessageReader reader, bool initialState);

		public int CompareTo(InnerNetObject other)
		{
			if (NetId > other.NetId)
			{
				return 1;
			}
			if (NetId < other.NetId)
			{
				return -1;
			}
			return 0;
		}

		public int CompareTo(PolusNetObject other)
		{
			if (NetId > other.NetId)
			{
				return 1;
			}
			if (NetId < other.NetId)
			{
				return -1;
			}
			return 0;
		}

		protected void SetDirtyBit(uint val)
		{
			DirtyBits |= val;
		}

		public uint SpawnId;

		public uint NetId;

		public uint DirtyBits;

		public SpawnFlags SpawnFlags;

		public SendOption sendMode = SendOption.Reliable;

		public int OwnerId;

		protected bool DespawnOnDestroy = true;
    }
}