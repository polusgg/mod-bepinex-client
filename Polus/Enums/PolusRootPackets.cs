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
        SetHudVisibility = 0x8C,
        AllowTaskInteraction = 0x8D,
        LoadHat = 0x96,
        LoadPet = 0x97,
        SetBody = 0x98,
        LoadSkin = 0x99,
        ChangeScene = 0x9A,
        MarkAssBrown = 0x9B,
        StampSetString = 0x9C,
        SendMessage = 0x9D,
        DeleteMessage = 0x9E,
        DisplaySystemAnnouncement = 0xFA,
        UpdateDiscordRichPresence = 0xFB,
        SetQrCodeContents = 0xFC
    }
}