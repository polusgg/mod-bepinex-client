using System;
using System.Collections.Generic;
using PolusGG.Extensions;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace PolusGG.Behaviours {
    public class PvaManager : MonoBehaviour {
        static PvaManager() {
            ClassInjector.RegisterTypeInIl2Cpp<PvaManager>();
        }

        public PvaManager(IntPtr ptr) : base(ptr) { }

        public PlayerVoteArea pva;
        public SpriteRenderer renderer;
        public bool dead;
        public bool disabled;
        public bool reported;

        public void Initialize(PlayerVoteArea voteArea) {
            pva = voteArea;
            renderer = GetComponent<SpriteRenderer>();
        }

        public void Update() {
            bool disabledState = pva.Parent.state is
                MeetingHud.VoteStates.Animating or
                MeetingHud.VoteStates.Discussion or
                MeetingHud.VoteStates.Proceeding;
            bool disable = dead || disabled || disabledState;
            if (disable) ControllerManager.Instance.RemoveSelectableUiElement(pva.PlayerButton);
            else ControllerManager.Instance.AddSelectableUiElement(pva.PlayerButton);
            if (pva.TargetPlayerId == PlayerVoteArea.SkippedVote)
                renderer.enabled = !disabled;
            else {
                pva.Flag.enabled = pva.DidVote && !pva.resultsShowing;
                pva.AmDead = dead;
                pva.DidReport = reported;
                pva.Megaphone.enabled = reported;
                // if (!disable) return;
                pva.Overlay.gameObject.SetActive(disable);
                pva.XMark.gameObject.SetActive(dead);
            }
        }

        public void SetState(bool dead, bool disabled, bool reported) {
            this.dead = dead;
            this.disabled = disabled;
            this.reported = reported;
        }
    }
}