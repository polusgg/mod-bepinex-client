using Hazel;

namespace PolusApi.Net {
    public interface IObjectManager {
        public static IObjectManager Instance;

        public void Spawn(PolusNetObject netObjParent, int ownerId = -2, SpawnFlags flags = SpawnFlags.None);
        public void Despawn(PolusNetObject objToDespawn);
        public void WriteSpawnMessage(PolusNetObject netObjParent, int ownerId, SpawnFlags flags, MessageWriter msg);
        public void RemoveNetObject(PolusNetObject obj);
    }
}