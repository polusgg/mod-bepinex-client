namespace PolusggSlim.Patches.SetString
{
    public static class RoomTrackerPatch
    {
        public static string RoomString { get; set; } = "__unset";

        // [HarmonyPatch(typeof(RoomTracker), nameof(RoomTracker.FixedUpdate))]
        public class RoomTrackerTextPatch
        {
            public static void Postfix(RoomTracker __instance)
            {
                if (RoomString != "__unset")
                    __instance.text.text = RoomString;
            }
        }
    }
}