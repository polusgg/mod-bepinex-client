using System.Collections;
using System.Collections.Generic;
using BepInEx.Logging;
using HarmonyLib;
using Il2CppSystem;
using Polus.Behaviours;
using Polus.Extensions;
using UnhollowerBaseLib;
using UnityEngine;
using Debug = Il2CppMono.Unity.Debug;
using Object = Il2CppSystem.Object;

namespace Polus.Patches.Temporary {
	[HarmonyPatch(typeof(ExileController._Animate_d__15), nameof(ExileController._Animate_d__15.MoveNext))]
	public class BaseExileControllerReplacePatch {
		[HarmonyPrefix]
		public static bool Prefix(ExileController._Animate_d__15 __instance, ref bool __result) {
			__instance.__4__this.gameObject.EnsureComponent<CoroutineManager>().Start(BaseCoroutines.Animate(__instance.__4__this));
			return false;
		}
	}
	
	[HarmonyPatch(typeof(AirshipExileController._HandleText_d__12), nameof(AirshipExileController._HandleText_d__12.MoveNext))]
	public class AirshipControllerReplacePatch {
		[HarmonyPrefix]
		public static bool Prefix(AirshipExileController._HandleText_d__12 __instance, ref bool __result) {
			__instance.__4__this.gameObject.EnsureComponent<CoroutineManager>().Start(AirshipCoroutines.HandleText(__instance.__4__this));
			return false;
		}
	}
	
	[HarmonyPatch(typeof(PbExileController._HandleText_d__7), nameof(PbExileController._HandleText_d__7.MoveNext))]
	public class PolusControllerReplacePatch {
		[HarmonyPrefix]
		public static bool Prefix(PbExileController._HandleText_d__7 __instance, ref bool __result) {
			__instance.__4__this.gameObject.EnsureComponent<CoroutineManager>().Start(PolusCoroutines.HandleText(__instance.__4__this));
			return false;
		}
	}
	
	[HarmonyPatch(typeof(PbExileController._Animate_d__6), nameof(PbExileController._Animate_d__6.MoveNext))]
	public class PolusControllerReplacePatchTwo {
		[HarmonyPrefix]
		public static bool Prefix(PbExileController._Animate_d__6 __instance, ref bool __result) {
			__instance.__4__this.gameObject.EnsureComponent<CoroutineManager>().Start(PolusCoroutines.Animate(__instance.__4__this));
			return false;
		}
	}
	
	[HarmonyPatch(typeof(MiraExileController._HandleText_d__3), nameof(MiraExileController._HandleText_d__3.MoveNext))]
	public class MiraControllerReplacePatch {
		[HarmonyPrefix]
		public static bool Prefix(MiraExileController._HandleText_d__3 __instance, ref bool __result) {
			__instance.__4__this.gameObject.EnsureComponent<CoroutineManager>().Start(MiraCoroutines.HandleText(__instance.__4__this));
			return false;
		}
	}

	public static class BaseCoroutines {
		public static IEnumerator Animate(ExileController instance) {
			yield return DestroyableSingleton<HudManager>.Instance.CoFadeFullScreen(Color.black, Color.clear, 0.2f);
			yield return new WaitForSeconds(0.2f);
			if (instance.exiled != null && instance.EjectSound)
			{
				var action = new System.Action<AudioSource, float>((source, dt) =>
				{
					instance.SoundDynamics(source, dt);
				});
				SoundManager.Instance.PlayDynamicSound("PlayerEjected", instance.EjectSound, true,
					action, true);
			}

			yield return new WaitForSeconds(0.8f);
			float num = Camera.main.orthographicSize * Camera.main.aspect + 1f;
			Vector2 left = Vector2.left * num;
			Vector2 right = Vector2.right * num;
			string filteredString = Replace(instance.completeString);
			int pointer = 0;
			int fakePointer = -1;
			for (float t = 0f; t <= instance.Duration; t += Time.deltaTime)
			{
				float num2 = t / instance.Duration;
				instance.Player.transform.localPosition = Vector2.Lerp(left, right, instance.LerpCurve.Evaluate(num2));
				float num3 = (t + 0.75f) * 25f / Mathf.Exp(t * 0.75f + 1f);
				instance.Player.transform.Rotate(new Vector3(0f, 0f, num3 * Time.deltaTime * 30f));
				if (num2 >= 0.3f)
				{
					int num4 = (int) (Mathf.Min(1f, (num2 - 0.3f) / 0.3f) * (float) filteredString.Length);
					if (num4 > pointer)
					{
						fakePointer += Mathf.CeilToInt(num4 - pointer);
						pointer = Mathf.CeilToInt(num4);
						char c = instance.completeString[fakePointer];

						while (c == '<')
						{
							fakePointer++;
							while (c != '>')
							{
								c = instance.completeString[fakePointer];
								fakePointer++;
							}

							c = instance.completeString[fakePointer];
						}

						instance.Text.text = instance.completeString.Substring(0, fakePointer + 1);
						instance.Text.gameObject.SetActive(true);
						if (instance.completeString[fakePointer] != ' ')
						{
							SoundManager.Instance.PlaySoundImmediate(instance.TextSound, false, 0.8f, 1f);
						}
					}
				}

				yield return null;
			}

			instance.Text.text = instance.completeString;
			if (PlayerControl.GameOptions.ConfirmImpostor)
			{
				instance.ImpostorText.gameObject.SetActive(true);
			}

			yield return Effects.Bloop(0f, instance.ImpostorText.transform, 1f, 0.5f);
			yield return new WaitForSeconds(0.5f);
			yield return DestroyableSingleton<HudManager>.Instance.CoFadeFullScreen(Color.clear, Color.black, 0.2f);
			instance.WrapUp();
		}

		public static string Replace(string s) {
			var inTag = false;
			var finalString = "";

			foreach (char c in s)
			{
				if (c == '>' && inTag)
				{
					inTag = false;
					continue;
				}

				if (c == '<' || inTag)
				{
					inTag = true;
					continue;
				}

				finalString = finalString.Insert(finalString.Length, c.ToString());
			}

			return finalString;
		}
	}

	public static class AirshipCoroutines {
		public static IEnumerator HandleText(AirshipExileController instance) {
			yield return Effects.Wait(1.75f);
			if (instance.exiled != null)
			{
				instance.CloudSlowMo = 0.1f;
				instance.PlayerSlowMo = 0.1f;
				SoundManager.Instance.PlaySound(instance.Stinger, false, 1f);
			}

			float newDur = instance.Duration * 0.5f;
			int pointer = 0;
			int fakePointer = -1;
			string filteredString = BaseCoroutines.Replace(instance.completeString);
			for (float t = 0f; t <= newDur; t += Time.deltaTime)
			{
				int num4 = (int) (t / newDur * (float)filteredString.Length);
				if (num4 > pointer)
				{
					fakePointer += Mathf.CeilToInt(num4 - pointer);
					pointer = Mathf.CeilToInt(num4);
					char c = instance.completeString[fakePointer];

					while (c == '<')
					{
						fakePointer++;
						while (c != '>')
						{
							c = instance.completeString[fakePointer];
							fakePointer++;
						}

						c = instance.completeString[fakePointer];
					}

					instance.Text.text = instance.completeString.Substring(0, fakePointer + 1);
					instance.Text.gameObject.SetActive(true);
					if (instance.completeString[fakePointer] != ' ')
					{
						SoundManager.Instance.PlaySoundImmediate(instance.TextSound, false, 0.8f, 1f);
					}
				}

				yield return null;
			}

			instance.Text.text = instance.completeString;
			yield return Effects.Wait(1f);
			instance.CloudSlowMo = 1f;
			instance.PlayerSlowMo = 6f;
			yield return Effects.Wait(0.5f);
			if (PlayerControl.GameOptions.ConfirmImpostor)
			{
				instance.ImpostorText.gameObject.SetActive(true);
			}

			yield return Effects.Bloop(0f, instance.ImpostorText.transform, 1f, 0.5f);
			yield break;
		}
	}

	public static class PolusCoroutines {
		public static IEnumerator Animate(PbExileController instance) {
			yield return DestroyableSingleton<HudManager>.Instance.CoFadeFullScreen(Color.black, Color.clear, 0.2f);
			yield return Effects.Wait(0.75f);
			instance.StartCoroutine(Effects.All(new []
			{
				instance.PlayerFall(),
				instance.PlayerSpin(),
				instance.HandleText()
			}));
			yield return Effects.Wait(instance.Duration);
			if (PlayerControl.GameOptions.ConfirmImpostor)
			{
				instance.ImpostorText.gameObject.SetActive(true);
			}
			yield return Effects.Bloop(0f, instance.ImpostorText.transform, 1f, 0.5f);
			yield return Effects.Wait(1f);
			yield return DestroyableSingleton<HudManager>.Instance.CoFadeFullScreen(Color.clear, Color.black, 0.2f);
			instance.WrapUp();
			yield break;
		}
		public static IEnumerator HandleText(PbExileController instance) {
			yield return Effects.Wait(instance.Duration * 0.5f);
			float newDur = instance.Duration * 0.5f;
			int pointer = 0;
			int fakePointer = -1;
			string filteredString = BaseCoroutines.Replace(instance.completeString);
			for (float t = 0f; t <= newDur; t += Time.deltaTime)
			{
				int num4 = (int) (t / newDur * (float)filteredString.Length);
				if (num4 > pointer)
				{
					fakePointer += Mathf.CeilToInt(num4 - pointer);
					pointer = Mathf.CeilToInt(num4);
					char c = instance.completeString[fakePointer];

					while (c == '<')
					{
						fakePointer++;
						while (c != '>')
						{
							c = instance.completeString[fakePointer];
							fakePointer++;
						}

						c = instance.completeString[fakePointer];
					}

					instance.Text.text = instance.completeString.Substring(0, fakePointer + 1);
					instance.Text.gameObject.SetActive(true);
					if (instance.completeString[fakePointer] != ' ')
					{
						SoundManager.Instance.PlaySoundImmediate(instance.TextSound, false, 0.8f, 1f);
					}
				}
				yield return null;
			}
			instance.Text.text = instance.completeString;
			yield break;
		}
	}

	public static class MiraCoroutines {
		public static IEnumerator HandleText(MiraExileController instance)
		{
			yield return Effects.Wait(instance.Duration * 0.5f);
			float newDur = instance.Duration * 0.5f;
			int pointer = 0;
			int fakePointer = -1;
			string filteredString = BaseCoroutines.Replace(instance.completeString);
			for (float t = 0f; t <= newDur; t += Time.deltaTime)
			{
				int num4 = (int) (t / newDur * (float)filteredString.Length);
				if (num4 > pointer)
				{
					fakePointer += Mathf.CeilToInt(num4 - pointer);
					pointer = Mathf.CeilToInt(num4);
					char c = instance.completeString[fakePointer];

					while (c == '<')
					{
						fakePointer++;
						while (c != '>')
						{
							c = instance.completeString[fakePointer];
							fakePointer++;
						}

						c = instance.completeString[fakePointer];
					}

					instance.Text.text = instance.completeString.Substring(0, fakePointer + 1);
					instance.Text.gameObject.SetActive(true);
					if (instance.completeString[fakePointer] != ' ')
					{
						SoundManager.Instance.PlaySoundImmediate(instance.TextSound, false, 0.8f, 1f);
					}
				}
				yield return null;
			}
			instance.Text.text = instance.completeString;
			yield break;
		}
	}
}