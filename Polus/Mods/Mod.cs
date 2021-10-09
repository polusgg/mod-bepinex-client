using System;
using BepInEx.Logging;
using Hazel;
using UnityEngine.SceneManagement;

namespace Polus.Mods {
    public abstract class Mod {
        /// <summary>
        /// The name of the mod to be logged and saved 
        /// </summary>
        public abstract string Name { get; }
        /// <summary>
        /// If this is null, no extra data will be written to the Hello packet 
        /// </summary>
        /// <seealso cref="WriteExtraData"/>
        public abstract byte? ProtocolId { get; }
        /// <summary>
        /// Called when the mod is started up
        ///
        /// This is called when the mod is reloaded, or when the main menu is loaded
        /// </summary>
        public abstract void Start();

        /// <summary>
        /// Called when the mod is stopped
        ///
        /// This is called when the mod is unloaded, when the user selects a non modded region 
        /// </summary>
        public abstract void Stop();
        /// <summary>
        /// Handles the reception of root packets
        /// </summary>
        /// <param name="reader">Message reader</param>
        public abstract void RootPacketReceived(MessageReader reader);
        // TODO handle RPC and GameData packets here
        /// <summary>
        /// Called on a fixed interval, see <see href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.FixedUpdate.html">Unity docs</see> for more info
        /// </summary>
        public abstract void FixedUpdate();
        /// <summary>
        /// Called every frame, see <see href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.Update.html">Unity docs</see> for more info
        /// </summary>
        public abstract void Update();
        /// <summary>
        /// Called when the game establishes a connection (game only, does not include announcement protocol)
        /// </summary>
        public abstract void ConnectionEstablished();
        /// <summary>
        /// Called when the game ends a connection (game only, does not include announcement protocol)
        /// </summary>
        public abstract void ConnectionDestroyed();
        /// <summary>
        /// Called for the mod to write data onto a Hello packet (game only, does not include announcement protocol)
        /// </summary>
        /// <returns>Whether </returns>
        public abstract void WriteExtraData(MessageWriter writer);
        /// <summary>
        /// Called when the user joins a lobby
        /// </summary>
        public abstract void LobbyJoined();
        /// <summary>
        /// Called when the user leaves/is disconnected from the lobby
        /// </summary>
        public abstract void LobbyLeft();
        /// <summary>
        /// Called at the end of a player's Start event
        /// </summary>
        /// <param name="player"></param>
        public abstract void PlayerSpawned(PlayerControl player);
        /// <summary>
        /// Called directly before player's OnDestroy event
        /// </summary>
        /// <param name="player">The player which is being destroyed</param>
        public abstract void PlayerDestroyed(PlayerControl player);
        /// <summary>
        /// Called when you gain acting host in a lobby
        /// </summary>
        public abstract void BecameHost();
        /// <summary>
        /// Called when you lose host in a lobby
        /// </summary>
        public abstract void LostHost();
        /// <summary>
        /// Used to do cleanup on references to destroyed objects
        /// </summary>
        public abstract void GameEnded();
        /// <summary>
        /// Called when the scene is changed
        /// </summary>
        public abstract void SceneChanged(Scene scene);
        /// <summary>
        /// Called whenever the debug GUI is painted
        /// </summary>
        public abstract void DebugGui();
    }
}