using System;
using HarmonyLib;
using Polus.Extensions;
using UnityEngine;

namespace Polus.Patches.Permanent.Qol {
    // a bunch of patches relating to sound manager, to play sounds as one shots instead of overwriting the current sound 
    public class SoundPatches {
        [HarmonyPatch(typeof(SoundManager), nameof(SoundManager.PlayDynamicSound))]
        public class PlayDynamicSoundPatch {
            [HarmonyPrefix]
            public static bool PlayDynamicSound(SoundManager __instance, out AudioSource __result, [HarmonyArgument(0)] string name, [HarmonyArgument(1)] AudioClip clip, [HarmonyArgument(2)] bool loop, [HarmonyArgument(3)] DynamicSound.GetDynamicsFunction volumeFunc, [HarmonyArgument(4)] bool playAsSfx) {
                DynamicSound dynamicSound = null;
                for (int i = 0; i < __instance.soundPlayers.Count; i++) {
                    ISoundPlayer soundPlayer = __instance.soundPlayers[(Index) i].Cast<ISoundPlayer>();
                    if (soundPlayer.Name == name && soundPlayer.TryGetCast(out dynamicSound))
                        break;
                }

                if (dynamicSound == null) {
                    dynamicSound = new DynamicSound {
                        Name = name,
                        Player = __instance.gameObject.AddComponent<AudioSource>()
                    };
                    dynamicSound.Player.outputAudioMixerGroup = ((loop && !playAsSfx) ? __instance.musicMixer : __instance.sfxMixer);
                    dynamicSound.Player.playOnAwake = false;
                    __instance.soundPlayers.Add(dynamicSound.Cast<ISoundPlayer>());
                }

                dynamicSound.Player.loop = loop;
                dynamicSound.SetTarget(clip, volumeFunc);

                __result = dynamicSound.Player;

                return false;
            }
        }

        [HarmonyPatch(typeof(DynamicSound), nameof(DynamicSound.SetTarget))]
        public class DynamicSoundSetTarget {
            public static void SetTarget(DynamicSound __instance, [HarmonyArgument(0)] AudioClip clip, [HarmonyArgument(1)] DynamicSound.GetDynamicsFunction volumeFunc) {
                __instance.volumeFunc = volumeFunc;
                __instance.Player.clip = clip;
                __instance.volumeFunc.Invoke(__instance.Player, 1f);
                if (__instance.Player.loop)
                    __instance.Player.Play();
                else
                    __instance.Player.PlayOneShot(clip);
            }
        }

        [HarmonyPatch(typeof(SoundManager), nameof(SoundManager.PlaySound))]
        public class PlaySoundPatch {
            [HarmonyPrefix]
            public static bool PlaySound(SoundManager __instance, out AudioSource __result, [HarmonyArgument(0)] AudioClip clip, [HarmonyArgument(1)] bool loop, [HarmonyArgument(2)] float volume) {
                if (clip == null) {
                    Debug.LogWarning("Missing audio clip");
                    __result = null;
                    return false;
                }

                AudioSource audioSource;
                if (__instance.allSources.ContainsKey(clip)) {
                    audioSource = __instance.allSources[clip];
                    if (!audioSource.isPlaying || !loop) {
                        audioSource.volume = volume;
                        audioSource.loop = loop;
                        if (loop)
                            audioSource.Play();
                        else
                            audioSource.PlayOneShot(clip);
                    }
                } else {
                    audioSource = __instance.gameObject.AddComponent<AudioSource>();
                    audioSource.outputAudioMixerGroup = (loop ? __instance.musicMixer : __instance.sfxMixer);
                    audioSource.playOnAwake = false;
                    audioSource.volume = volume;
                    audioSource.loop = loop;
                    audioSource.clip = clip;
                    audioSource.Play();
                    __instance.allSources.Add(clip, audioSource);
                }

                __result = audioSource;

                return false;
            }
        }

        [HarmonyPatch(typeof(SoundManager), nameof(SoundManager.PlaySoundImmediate))]
        public class PlaySoundImmediatePatch {
            [HarmonyPrefix]
            public static bool PlaySoundImmediate(SoundManager __instance, out AudioSource __result, [HarmonyArgument(0)] AudioClip clip, [HarmonyArgument(1)] bool loop, [HarmonyArgument(2)] float volume, [HarmonyArgument(3)] float pitch) {
                if (clip == null) {
                    Debug.LogWarning("Missing audio clip");
                    __result = null;
                    return false;
                }

                AudioSource audioSource;
                if (__instance.allSources.ContainsKey(clip)) {
                    audioSource = __instance.allSources[clip];
                    audioSource.pitch = pitch;
                    audioSource.loop = loop;
                    if (loop)
                        audioSource.Play();
                    else
                        audioSource.PlayOneShot(clip);
                } else {
                    audioSource = __instance.gameObject.AddComponent<AudioSource>();
                    audioSource.outputAudioMixerGroup = (loop ? __instance.musicMixer : __instance.sfxMixer);
                    audioSource.playOnAwake = false;
                    audioSource.volume = volume;
                    audioSource.pitch = pitch;
                    audioSource.loop = loop;
                    audioSource.clip = clip;
                    audioSource.Play();
                    __instance.allSources.Add(clip, audioSource);
                }

                __result = audioSource;

                return false;
            }
        }
    }
}