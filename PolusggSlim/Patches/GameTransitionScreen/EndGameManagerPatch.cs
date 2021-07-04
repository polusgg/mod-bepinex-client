namespace PolusggSlim.Patches.GameTransitionScreen
{
    public static class EndGameManagerPatch
    {
        public static EndGameCutsceneData Data = new();

        // [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.ShowButtons))]
        public static class EndGameManager_ShowButtons
        {
            public static void Postfix(EndGameManager __instance)
            {
                __instance.PlayAgainButton.gameObject.active = Data.DisplayPlayAgain;
                __instance.ExitButton.gameObject.active = Data.DisplayQuit;
            }
        }
    }
}