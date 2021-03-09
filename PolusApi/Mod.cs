using System;
using BepInEx.Logging;
using Hazel;
using PolusApi.Net;

namespace PolusApi {
    public abstract class Mod : MarshalByRefObject {
        public abstract void Load(ManualLogSource logger, IObjectManager objectManager);
        public abstract void Unload();
        public abstract void HandleRoot(MessageReader reader);
        public abstract string Name { get; }
    }
}