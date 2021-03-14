using System;
using Hazel;
using PolusApi.Net;
using PowerTools;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace TestMod.Pno {
	public class PolusDeadBody : PolusNetObject {
		// todo recreate dead body lmao
		// todo patch murder player to not show dead body lmao
		// todo patch reporting to correctly allow reporting custom dead bodies

		public PolusDeadBody(IntPtr ptr) : base(ptr) { }

		static PolusDeadBody() {
			ClassInjector.RegisterTypeInIl2Cpp<PolusDeadBody>();
		}

		public override void HandleRpc(byte callId, MessageReader reader) {
			
		}

		private void Start() {
			rend = GetComponent<SpriteRenderer>();
		}

		public override bool Serialize(MessageWriter writer, bool initialState) {
			//fuck this
			throw new NotImplementedException();
		}

		public override void Deserialize(MessageReader reader, bool initialState) {
			anim.SetTime(reader.ReadBoolean() ? 0 : anim.m_currAnim.length);
			rend.flipX = reader.ReadBoolean();
			// transform.localScale = new Vector3(reader.ReadBoolean() ? -0.7f : 0.7f, 0.7f, 0.7f);
			rend.material.SetColor(BackColor, new Color32(reader.ReadByte(),reader.ReadByte(),reader.ReadByte(),reader.ReadByte()));
			rend.material.SetColor(BodyColor, new Color32(reader.ReadByte(),reader.ReadByte(),reader.ReadByte(),reader.ReadByte()));
		}

		public SpriteAnim anim;
		public DeadBody deadBody;
		public SpriteRenderer rend;
		private static readonly int BodyColor = Shader.PropertyToID("_BodyColor");
		private static readonly int BackColor = Shader.PropertyToID("_BackColor");
	}
}