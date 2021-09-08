namespace Polus.Enums {
    public enum ResponseType : byte {
        DownloadStarted = 0x00,
        DownloadEnded = 0x01,
        DownloadFailed = 0x02,
        DownloadInvalid = 0x03
    }
}