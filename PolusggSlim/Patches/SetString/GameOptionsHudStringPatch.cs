namespace PolusggSlim.Patches.SetString
{
    public static class GameOptionsHudStringPatch
    {
        public static string GameOptionsString { get; set; } = "__unset";

        // [HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.FixedUpdate))]
        public static class LobbyBehaviour_FixedUpdate
        {
            public static void Postfix()
            {
                if (GameOptionsString != "__unset")
                    DestroyableSingleton<HudManager>.Instance.GameSettings.text = GameOptionsString;
            }
        }
    }
}