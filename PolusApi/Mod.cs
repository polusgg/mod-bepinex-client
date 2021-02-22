using System;
using BepInEx.Logging;

namespace PolusApi {
    public abstract class Mod {
        public abstract void Load(ManualLogSource logger);
    }
}