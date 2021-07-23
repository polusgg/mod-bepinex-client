namespace Polus.Enums {
    public enum PolusRootPackets : byte {
        FetchResource = 0x80,
        Resize = 0x81,
        Intro = 0x82,
        EndGame = 0x83,
        SetString = 0x84,
        DeclareHat = 0x85,
        SetGameOption = 0x89,
        DeleteGameOption = 0x8A,
        LoadHat = 0x8B,
        SetHudVisibility = 0x8C,
        AllowTaskInteraction = 0x8D,
        DisplaySystemAnnouncement = 0xFA
    }
}