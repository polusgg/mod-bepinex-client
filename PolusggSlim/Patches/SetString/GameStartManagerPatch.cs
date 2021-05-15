namespace PolusggSlim.Patches.SetString
{
    public static class GameStartManagerPatch
    {
        public static string GamePlayerCount { get; set; } = "__unset";

        // [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
        public static class GameStartManager_Update
        {
            public static void Postfix(GameStartManager __instance)
            {
                if (GamePlayerCount != "__unset")
                    __instance.PlayerCounter.text = GamePlayerCount;
            }
        }
    }
}