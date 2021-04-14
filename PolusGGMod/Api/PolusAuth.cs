namespace PolusGG.Api {
    public class PolusAuth {
        public static bool IsPlayerSignedIn { get; } 
        public static string Token { get; }
        public static byte[] Uuid { get; }
        public static string DisplayName { get; }
        public static string[] Perks { get; }

        public static void Login() {
            
        }
    }
}