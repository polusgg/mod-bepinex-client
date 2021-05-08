using System;
using Hazel;
using PolusGG.Enums;
using PolusGG.Net;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace PolusGG.Behaviours.Inner {
    public class PolusSoundSource : PnoBehaviour {
        // private float falloffMultiplier;
        // private float falloffRadius;
        private AudioSource source;

        static PolusSoundSource() {
            ClassInjector.RegisterTypeInIl2Cpp<PolusSoundSource>();
        }

        public PolusSoundSource(IntPtr ptr) : base(ptr) { }

        private void Start() {
            pno = PogusPlugin.ObjectManager.LocateNetObject(this);
            pno.OnData = Deserialize;
            source = GetComponent<AudioSource>();
        }

        private void FixedUpdate() {
            if (pno.HasSpawnData()) Deserialize(pno.GetSpawnData());

            if (PlayerControl.LocalPlayer) {
                source.rolloffMode = AudioRolloffMode.Linear;
                // source.rolloffFactor = falloffMultiplier;
                // source.maxDistance = falloffRadius;
            }
        }

        private void Deserialize(MessageReader reader) {
            source.clip = PogusPlugin.Cache.CachedFiles[reader.ReadPackedUInt32()].Get<AudioClip>();
            source.pitch = reader.ReadSingle();
            source.volume = reader.ReadSingle();
            source.loop = reader.ReadBoolean();
            source.volume *= (SoundType) reader.ReadByte() switch {
                SoundType.None => 1f,
                SoundType.SoundEffect => SoundManager.SfxVolume,
                SoundType.Music => SoundManager.MusicVolume,
                _ => throw new Exception("Invalid sound type")
            };
            source.time = reader.ReadSingle() / 1000f;
            if (reader.ReadBoolean()) source.Pause();
            else source.UnPause();
        }
    }
}