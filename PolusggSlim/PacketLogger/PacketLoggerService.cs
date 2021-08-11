using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using PolusggSlim.Configuration;
using PolusggSlim.Utils;

namespace PolusggSlim.PacketLogger
{
    public class PacketLoggerService : IDisposable
    {
        private readonly Thread _recorderThread;
        private readonly BlockingCollection<byte[]> _packetQueue = new();
        private FileStream _logFileStream;

        private PacketLoggerConfig Config => PluginSingleton<PolusggMod>.Instance.Configuration.PacketLogger;

        public PacketLoggerService()
        {
            _recorderThread = new Thread(Execute)
            {
                Name = "PacketRecorderThread"
            };
        }

        public void Start()
        {
            Directory.CreateDirectory(Config.LogFolder);
            _logFileStream = new FileStream(Config.LogFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            _recorderThread.Start();
        }

        public void Dispose()
        {
            _logFileStream.Dispose();
            _recorderThread.Join();
        }

        public void RecordPacket(byte[] packet)
        {
            _packetQueue.Add(packet);
        }

        private void Execute()
        {
            try
            {
                foreach (var packet in _packetQueue.GetConsumingEnumerable())
                {
                    _logFileStream.Write(packet);
                }
                _logFileStream.Flush(true);
            }
            catch (ThreadAbortException) { /* ignored */ }
        }
    }
}