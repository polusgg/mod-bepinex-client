namespace PolusggSlim.Patches.SetString
{
    public static class PingTrackerPatch
    {
        public static string PingText { get; set; } = "__unset";

        // [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
        public class PingTrackerTextPatch
        {
            public static void Postfix(PingTracker __instance)
            {
                if (PingText != "__unset")
                    __instance.text.text = PingText;
            }
        }
    }
}