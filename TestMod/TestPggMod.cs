using System;
using BepInEx.Logging;
using PolusApi;

namespace TestMod {
    [Mod(Id)]
    public class TestPggMod : Mod {
        public const string Id = "TestMod Lmoa";
        public override void Load(ManualLogSource logger) {
            logger.LogInfo($"Yo mama from {Id}");
        }
    }
}