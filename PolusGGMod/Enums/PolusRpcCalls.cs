﻿namespace PolusGG.Enums {
    public enum PolusRpcCalls : byte {
        SnapTo = RpcCalls.SnapTo,
        ChatVisibility = 0x80,
        SetRole = 0x82,
        CloseHud = 0x83,
        Revive = 0x84,
        PlaySound = 0x85,
        Click = 0x86,
        Use = 0x87,
        SetHat = 0x88,
        DespawnAllVents = 0x89,
        SetOutline = 0x8a,
        SetOpacity = 0x8b,
        BeginAnimationPlayer = 0x8c,
        BeginAnimationCamera = 0x8d,
        EnterVent = 0x8e,
        
    }
}