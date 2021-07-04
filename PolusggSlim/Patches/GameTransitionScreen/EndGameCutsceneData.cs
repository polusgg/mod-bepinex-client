namespace PolusggSlim.Patches.GameTransitionScreen
{
    public class EndGameCutsceneData : CutsceneData
    {
        public bool DisplayQuit { get; set; } = true;
        public bool DisplayPlayAgain { get; set; } = true;
    }
}