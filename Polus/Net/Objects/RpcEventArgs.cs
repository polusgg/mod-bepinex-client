using System;
using Hazel;

namespace Polus.Net.Objects {
    public class RpcEventArgs : EventArgs {
        public readonly byte callId;

        public readonly MessageReader reader;

        // (InnerNetObject netObject, byte call, MessageReader reader)
        public RpcEventArgs(byte callId, MessageReader reader) {
            this.callId = callId;
            this.reader = reader;
        }
    }
}