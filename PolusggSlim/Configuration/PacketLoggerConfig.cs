using System;
using System.IO;
using BepInEx;

namespace PolusggSlim.Configuration
{
    public class PacketLoggerConfig
    {
        public string LogFolder => Path.Combine(Paths.GameRootPath, "PacketLogger");
        public string LogName { get; } = $"Log_{DateTime.Now:d}.txt";
        public string LogFile => Path.Combine(LogFolder, LogName);
    }
}