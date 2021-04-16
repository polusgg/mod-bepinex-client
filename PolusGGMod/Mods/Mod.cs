using System;
using BepInEx.Logging;
using Hazel;
using PolusGG.Net;
using PolusGG.Resources;

namespace PolusGG.Mods {
    public abstract class Mod : MarshalByRefObject {
        /// <summary>
        /// Mid-BepInEx 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="objectManager"></param>
        public abstract void Load();

        /// <summary>
        /// Post-BepInEx startup event
        /// </summary>
        public abstract void Start(IObjectManager objectManager, ICache cache);
        public abstract void Unload();
        public abstract void RootPacketReceived(MessageReader reader);
        public abstract void FixedUpdate();
        public abstract void LobbyJoined();
        public abstract void LobbyLeft();
        public abstract string Name { get; }
        public abstract ManualLogSource Logger { get; set; }
    }
}