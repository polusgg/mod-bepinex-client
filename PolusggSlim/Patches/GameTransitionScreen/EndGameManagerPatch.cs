namespace PolusggSlim.Patches.GameTransitionScreen
{
    public static class EndGameManagerPatch
    {
        public static CutsceneData Data = new();

        
        
        public static class EndGameManager_ShowButtons
        {
            public static bool Prefix()
            {
                
            }
        }
    }
}