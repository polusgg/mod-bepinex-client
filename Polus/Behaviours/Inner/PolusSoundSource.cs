using System;
using Hazel;
using Polus.Enums;
using Polus.Extensions;
using Polus.Net.Objects;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace Polus.Behaviours.Inner {
    public class PolusSoundSource : PnoBehaviour {
        // private float falloffMultiplier;
        // private float falloffRadius;
        private AudioSource source;

        static PolusSoundSource() {
            ClassInjector.RegisterTypeInIl2Cpp<PolusSoundSource>();
        }

        public PolusSoundSource(IntPtr ptr) : base(ptr) { }

        private void Start() {
            source = GetComponent<AudioSource>();
        }

        private void FixedUpdate() {
            if (pno.HasData()) Deserialize(pno.GetData());

            if (PlayerControl.LocalPlayer) {
                source.rolloffMode = AudioRolloffMode.Linear;
                // source.rolloffFactor = falloffMultiplier;
                // source.maxDistance = falloffRadius;
            }
        }

        private void Deserialize(MessageReader reader) {
            source.clip = PogusPlugin.Cache.CachedFiles[reader.ReadPackedUInt32()].Get<AudioClip>();//todo listen for cache update for this
            source.pitch = reader.ReadSingle();
            source.volume = reader.ReadSingle();
            source.loop = reader.ReadBoolean();
            source.volume *= (SoundType) reader.ReadByte() switch {
                SoundType.None => 1f,
                SoundType.SoundEffect => Mathf.Clamp01(SoundManager.SfxVolume / 80 + 1),
                SoundType.Music => Mathf.Clamp01(SoundManager.MusicVolume / 80 + 1),
                _ => throw new Exception("Invalid sound type")
            };
            source.time = reader.ReadSingle() / 1000f;
            if (reader.ReadBoolean()) source.Pause();
            else source.Play();
        }
    }
}