using System;
using Hazel;

namespace PolusApi.Net {
	public class RpcEventArgs : EventArgs{
		// (InnerNetObject netObject, byte call, MessageReader reader)
		public RpcEventArgs(byte callId, MessageReader reader) {
			this.callId = callId;
			this.reader = reader;
		}
		public readonly byte callId;
		public readonly MessageReader reader;
	}
}