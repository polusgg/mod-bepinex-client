using System;

namespace PolusGG.Mods {
    [AttributeUsage(AttributeTargets.Class)]
    public class ModAttribute : Attribute {
        public ModAttribute(string name, string version = "1.0", string author = "Probably Sanae6") { }
    }
}