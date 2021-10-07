namespace Polus.Extensions {
    public static class AbstractMethodExtensions {
        public static void BaseBegin(this Minigame minigame, PlayerTask task) => minigame.Cast<Minigame>().Begin(task);
    }
}