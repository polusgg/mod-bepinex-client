using System;
using BepInEx.Logging;

namespace PolusApi {
    public abstract class Mod : MarshalByRefObject {
        public abstract void Load(ManualLogSource logger);
        public abstract void Unload();
        public abstract string Name { get; }
    }
}