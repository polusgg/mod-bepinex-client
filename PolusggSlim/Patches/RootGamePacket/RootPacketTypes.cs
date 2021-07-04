namespace PolusggSlim.Patches.RootGamePacket
{
    public enum RootPacketTypes : byte
    {
        FetchResource = 0x80,
        Resize = 0x81,
        DisplayStartGameScreen = 0x82,
        OverwriteGameOver = 0x83,
        SetString = 0x84,
        DeclareHat = 0x85,
        DeclarePet = 0x86,
        DeclareSkin = 0x87,
        DeclareKillAnim = 0x88,
        SetGameOption = 0x89,
        DeleteGameOption = 0x8A,
        DisplaySystemAlert = 0xFA
    }
}