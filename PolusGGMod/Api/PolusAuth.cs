using System.Text;

namespace PolusGG.Api {
    public class PolusAuth {
        public static bool IsPlayerSignedIn { get; } 
        public static string Token => "piss";
        public static byte[] Uuid => Encoding.UTF8.GetBytes("9a8e0c94d64945b59fcf2c8870aa8975");
        public static string DisplayName => "Sane6";

        public static string[] Perks => new[] {
            "yo mama"
        };

        public static void Login() {
            
        }
    }
}