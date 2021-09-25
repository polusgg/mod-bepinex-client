using System;

namespace Polus.Enums {
    [Flags]
    public enum SendOptionPlus : byte {
        None = 0,
        Reliable = 1,
        Fragmented = 4,
    }
}