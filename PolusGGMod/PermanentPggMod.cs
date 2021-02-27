using System;
using HarmonyLib;
using PolusGGMod.Framework.Common.Utilities;

namespace PolusGGMod {
    public class PermanentPggMod : PggMod {
        public override void LoadPatches(string harmonyName, Type[] types) {
            PogusPlugin.Logger.LogInfo(types.Length);

            foreach (Type type in types) {
                PogusPlugin.Logger.LogInfo($"LSdakdada af {type.FullName}");
            }

            if (_harmony == null) _harmony = new Harmony(harmonyName);
            foreach (var type in types)
            {
                ToggledPatches.AddRange(PatchManagerUtils.ResolvePatchDetails(type, true));
            }
        }
    }
}