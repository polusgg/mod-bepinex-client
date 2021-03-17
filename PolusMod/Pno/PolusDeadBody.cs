using System;
using System.Collections;
using Hazel;
using PolusApi.Net;
using PowerTools;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace PolusMod.Pno {
	public class PolusDeadBody : PolusNetObject {
		// todo recreate dead body lmao
		// todo patch murder player to not show dead body lmao
		// todo patch reporting to correctly allow reporting custom dead bodies

		public PolusDeadBody(IntPtr ptr) : base(ptr) { }

		static PolusDeadBody() {
			ClassInjector.RegisterTypeInIl2Cpp<PolusDeadBody>();
		}

		public uint SpawnId { get; }
		public uint NetId { get; }

		public override void HandleRpc(byte callId, MessageReader reader) {
			
		}

		private void Start() {
			rend = GetComponent<SpriteRenderer>();
		}

		public IEnumerator Lmao() {
			yield break;
		}

		public override void Deserialize(MessageReader reader, bool initialState) {
			reader.Position.Log(5, "LmaoOOOoAo");
			anim.SetTime(reader.ReadBoolean() ? 0 : anim.m_currAnim.length);
			rend.flipX = reader.ReadBoolean();
			// transform.localScale = new Vector3(reader.ReadBoolean() ? -0.7f : 0.7f, 0.7f, 0.7f);
			rend.material.SetColor(BackColor, new Color32(reader.ReadByte(),reader.ReadByte(),reader.ReadByte(),reader.ReadByte()));
			rend.material.SetColor(BodyColor, new Color32(reader.ReadByte(),reader.ReadByte(),reader.ReadByte(),reader.ReadByte()));
		}

		public SpriteAnim anim;
		public DeadBody deadBody;
		public SpriteRenderer rend;
		public PolusNetworkTransform netTransform;
		private static readonly int BodyColor = Shader.PropertyToID("_BodyColor");
		private static readonly int BackColor = Shader.PropertyToID("_BackColor");
	}
}