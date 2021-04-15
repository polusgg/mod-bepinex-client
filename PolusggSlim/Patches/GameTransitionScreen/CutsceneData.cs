using UnityEngine;

namespace PolusggSlim.Patches.GameTransitionScreen
{
    public class CutsceneData
    {
        public string TitleText { get; set; } = "";
        public string SubtitleText { get; set; } = "";
        public Color BackgroundColor { get; set; } = Color.clear;
        public byte[] YourTeam { get; set; } = { };
    }
}