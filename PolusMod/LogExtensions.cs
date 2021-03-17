namespace PolusMod {
    public static class LogExtensions {
        public static T Log<T>(this T value, int times = 1, string comment = "") {
            if (times == 1) TestPggMod._loggee.LogInfo($"{value} {comment}");
            else {
                for (int i = 0; i < times; i++) {
                    TestPggMod._loggee.LogInfo($"{value} {comment}");
                }
            }

            return value;
        } 
    }
}