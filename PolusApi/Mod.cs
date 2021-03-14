using System;
using BepInEx.Logging;
using Hazel;
using PolusApi.Net;
using PolusApi.Resources;

namespace PolusApi {
    public abstract class Mod /*: MarshalByRefObject*/ {
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
        public abstract void HandleRoot(MessageReader reader);
        public abstract string Name { get; }
        public abstract ManualLogSource Logger { get; set; }
    }
}