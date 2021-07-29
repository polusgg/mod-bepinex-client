﻿namespace Polus.Enums {
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
        MarkAsBrown = 0x9B,
        DisplaySystemAnnouncement = 0xFA
    }
}