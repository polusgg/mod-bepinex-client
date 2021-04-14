using System;
using Hazel;
using PolusGG.Net;
using UnhollowerRuntimeLib;

namespace PolusGG.Behaviours.Inner {
    public class PolusConsole : PnoBehaviour {
        // private float timer;
        public PolusConsole(IntPtr ptr) : base(ptr) { }
    
        static PolusConsole() {
            ClassInjector.RegisterTypeInIl2Cpp<PolusConsole>();
        }
        
        private void Start() {
            pno = PogusPlugin.ObjectManager.LocateNetObject(this);
            pno.OnData = Deserialize;
        }
        
        private void FixedUpdate() {
            if (pno.HasSpawnData()) Deserialize(pno.GetSpawnData());
        }
    
        private void Update() {
            
        }
    
        private void Deserialize(MessageReader reader) {
        }
    }
}