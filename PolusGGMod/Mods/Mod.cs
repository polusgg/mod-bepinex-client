using System;
using BepInEx.Logging;
using Hazel;
using PolusGG.Net;
using PolusGG.Resources;

namespace PolusGG.Mods {
    public abstract class Mod : MarshalByRefObject {
        public abstract string Name { get; }
        public abstract ManualLogSource Logger { get; set; }

        /// <summary>
        ///     Post-BepInEx startup event
        /// </summary>
        public abstract void Start(IObjectManager objectManager, ICache cache);

        public abstract void Stop();
        public abstract void RootPacketReceived(MessageReader reader);
        public abstract void FixedUpdate();
        public abstract void LobbyJoined();
        public abstract void LobbyLeft();
    }
}