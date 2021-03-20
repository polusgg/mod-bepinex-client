namespace PolusMod {
    public enum PolusRpcCalls : byte {
        // 
        SnapTo = RpcCalls.SnapTo,
        
        Revive = 0x84,
        PlaySound = 0x85,
        CloseHud = 0x83,
        Click = 0x8a,
        SetString = 0x8b,
        ChatVisibility = 0x8d,
        Use = 0x8e
    }
}