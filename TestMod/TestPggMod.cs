using System;
using BepInEx.Logging;
using PolusApi;
using PolusApi.Net;

namespace TestMod {
    [Mod(Id)]
    public class TestPggMod : Mod {
        public const string Id = "TestMod Lmoa";
        private static ManualLogSource Logger;
        public override void Load(ManualLogSource logger) {
            Logger = logger;
            System.Console.WriteLine($"PogU {logger == null}");
            Logger.LogInfo($"Yo mama from {Id}");
        }

        public override void Unload() {
            Logger.LogInfo($"Your mother from {Id}");
        }

        public override string Name => "TestMod";
    }
}