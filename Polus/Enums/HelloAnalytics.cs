namespace Polus.Enums {
    public class HelloAnalytics {
        public enum Platform : byte {
            Unknown,                                     // can't find store, (android: is an apk if store couldn't be found!)
            Epic,
            Steam,
            Mac,
            WindowsStore,
            Itch,
            IOS,
            PlayStore,
            Apk,
            Web,
        }
    }
}