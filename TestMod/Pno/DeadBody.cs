using Hazel;
using PolusApi.Net;

namespace TestMod.Pno {
	public class DeadBody : PolusNetObject {
		//todo recreate dead body lmao
		//todo patch murder player to not show dead body lmao
		//todo patch reporting to correctly allow reporting custom dead bodies
		public override void HandleRpc(byte callId, MessageReader reader) {
			
		}

		public override bool Serialize(MessageWriter writer, bool initialState) {
			throw new System.NotImplementedException();
		}

		public override void Deserialize(MessageReader reader, bool initialState) {
			throw new System.NotImplementedException();
		}
	}
}