namespace Polus.Patches.Temporary {
    public class DiscordPatches {
        //i just realized i'm destroying discordmanager i'm actually so fucking stupid it's hilarious

        // [HarmonyPatch(typeof(DiscordManager), nameof(DiscordManager.SetInLobbyClient))]
        // public class ManagerHandleClientPresence {
        //     public static bool Prefix([HarmonyArgument(0)] int players, int id) {
        //         if (PolusDiscordManager.Instance) {
        //             PolusDiscordManager.Instance.SetInLobby(players, id);
        //             return false;
        //         }
        //
        //         return true;
        //     }
        // }

        // [HarmonyPatch(typeof(Discord.Discord), MethodType.Constructor)]
        // public class DiscordConstructorPresenceThingy {
        //     public static bool Prefix([HarmonyArgument(0)] ref ulong application) {
        //         if (PogusPlugin.ModManager.AllPatched) {
        //             // application = 
        //         }
        //
        //         return true;
        //     }
        // }

        // [HarmonyPatch(typeof(DiscordManager), nameof(DiscordManager.SetInLobbyHost))]
        // public class ManagerHandleHostPresence {
        //     public static bool Prefix([HarmonyArgument(0)] int players, int id) {
        //         if (PolusDiscordManager.Instance) {
        //             PolusDiscordManager.Instance.SetInLobby(players, id);
        //             return false;
        //         }
        //
        //         return true;
        //     }
        // }
    }
}