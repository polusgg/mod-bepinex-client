using System;
using Hazel;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PolusApi.Net {
    public abstract class PolusNetObject : MonoBehaviour, IComparable<InnerNetObject>, IComparable<PolusNetObject> {
		public bool AmOwner
		{
			get
			{
				return this.OwnerId == AmongUsClient.Instance.ClientId;
			}
		}

		public void Despawn()
		{
			IObjectManager.Instance.Despawn(this);
			Object.Destroy(base.gameObject);
		}

		public virtual void OnDestroy()
		{
			if (AmongUsClient.Instance && this.NetId != 4294967295U)
			{
				if (this.DespawnOnDestroy && this.AmOwner)
				{
					IObjectManager.Instance.Despawn(this);
					return;
				}
				IObjectManager.Instance.RemoveNetObject(this);
			}
		}

		public virtual void PreSerialize()
		{
		}

		public abstract void HandleRpc(byte callId, MessageReader reader);

		public abstract bool Serialize(MessageWriter writer, bool initialState);

		public abstract void Deserialize(MessageReader reader, bool initialState);

		public int CompareTo(InnerNetObject other)
		{
			if (this.NetId > other.NetId)
			{
				return 1;
			}
			if (this.NetId < other.NetId)
			{
				return -1;
			}
			return 0;
		}

		public int CompareTo(PolusNetObject other)
		{
			if (this.NetId > other.NetId)
			{
				return 1;
			}
			if (this.NetId < other.NetId)
			{
				return -1;
			}
			return 0;
		}

		protected void SetDirtyBit(uint val)
		{
			this.DirtyBits |= val;
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