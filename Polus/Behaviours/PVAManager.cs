using System;
using System.Linq;
using Il2CppSystem.Collections;
using Il2CppSystem.Collections.Generic;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace Polus.Behaviours {
    public class PvaManager : MonoBehaviour {
        static PvaManager() {
            ClassInjector.RegisterTypeInIl2Cpp<PvaManager>();
        }

        public PvaManager(IntPtr ptr) : base(ptr) { }

        public PlayerControl player;
        public PlayerVoteArea pva;
        public SpriteRenderer renderer;
        public bool dead;
        public bool disabled;
        public bool reported;

        public void Initialize(PlayerVoteArea voteArea, byte targetId) {
            pva = voteArea;
            renderer = GetComponent<SpriteRenderer>();
            if (targetId < 252) player = PlayerControl.AllPlayerControls.Find((Func<PlayerControl, bool>) (pc => pc.PlayerId == targetId));
        }

        public void Update() {
            bool disabledState = pva.Parent.state is
                MeetingHud.VoteStates.Animating or
                MeetingHud.VoteStates.Discussion or
                MeetingHud.VoteStates.Proceeding;
            if (pva.NameText && player) {
                pva.NameText.text = player.nameText.text;
                pva.NameText.color = Color.white;
            }

            bool disable = dead || disabled || disabledState;
            if (disable) ControllerManager.Instance.RemoveSelectableUiElement(pva.PlayerButton);
            else ControllerManager.Instance.AddSelectableUiElement(pva.PlayerButton);
            pva.AmDead = dead;
            pva.DidReport = reported;
            if (pva.TargetPlayerId == PlayerVoteArea.SkippedVote)
                renderer.enabled = !disabled;
            else {
                pva.Flag.enabled = pva.DidVote && !pva.resultsShowing;
                pva.Megaphone.enabled = reported;
                pva.Overlay.gameObject.SetActive(disable);
                pva.XMark.gameObject.SetActive(dead);

                if ((disabled || pva.voteComplete) && pva.Buttons.active/* && ControllerManager.Instance.IsMenuActiveAtAll(name)*/) {
                    ControllerManager.Instance.CloseOverlayMenu(name);
                    pva.Buttons.active = false;
                }
            }
        }

        public void SetState(bool dead, bool disabled, bool reported) {
            this.dead = dead;
            this.disabled = disabled;
            this.reported = reported;
        }

        public void Select() {
            if (PlayerControl.LocalPlayer.Data.IsDead || dead || pva.Parent)
                return;

            if (!pva.voteComplete && pva.Parent.Select(pva.TargetPlayerId)) {
                if (pva.Buttons.active) return;
                pva.Buttons.SetActive(true);
                float startPos = pva.AnimateButtonsFromLeft ? 0.2f : 1.95f;
                StartCoroutine(Effects.All(new[] {
                    Effects.Lerp(0.25f, (Action<float>) (t => pva.CancelButton.transform.localPosition = Vector2.Lerp(Vector2.right * startPos, Vector2.right * 1.3f, Effects.ExpOut(t)))),
                    Effects.Lerp(0.35f, (Action<float>) (t => pva.ConfirmButton.transform.localPosition = Vector2.Lerp(Vector2.right * startPos, Vector2.right * 0.65f, Effects.ExpOut(t))))
                }));
                List<UiElement> selectableElements = new();
                selectableElements.Add(pva.CancelButton);
                selectableElements.Add(pva.ConfirmButton);
                ControllerManager.Instance.OpenOverlayMenu(name, pva.CancelButton, pva.ConfirmButton, selectableElements);
            }
        }
    }
}