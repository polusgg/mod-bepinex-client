using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnhollowerBaseLib;
using UnhollowerBaseLib.Attributes;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace Polus.Behaviours {
    public sealed class CoroutineManager : MonoBehaviour {
        private readonly List<CoroutineTuple> coroutinesStore = new();
        private readonly List<IEnumerator> nextFrameCoroutines = new();

        private readonly List<IEnumerator> tempList = new();
        private readonly List<IEnumerator> waitForEndOfFrameCoroutines = new();
        private readonly List<IEnumerator> waitForFixedUpdateCoroutines = new();

        static CoroutineManager() {
            ClassInjector.RegisterTypeInIl2Cpp<CoroutineManager>();
        }

        public CoroutineManager(IntPtr intPtr) : base(intPtr) { }

        [HideFromIl2Cpp]
        // ReSharper disable once Unity.IncorrectMethodSignature
        public IEnumerator Start(IEnumerator routine) {
            ProcessNextOfCoroutine(routine);
            return routine;
        }

        [HideFromIl2Cpp]
        // ReSharper disable once Unity.IncorrectMethodSignature
        public IEnumerator Start(object owner, IEnumerator routine) {
            ProcessNextOfCoroutine(routine);
            return routine;
        }

        private void Update() {
            for (int i = coroutinesStore.Count - 1; i >= 0; i--) {
                CoroutineTuple tuple = coroutinesStore[i];
                if (!(tuple.WaitCondition is WaitForSeconds waitForSeconds)) continue;
                if ((waitForSeconds.m_Seconds -= Time.deltaTime) > 0) continue;
                coroutinesStore.RemoveAt(i);
                ProcessNextOfCoroutine(tuple.Coroutine);
            }

            ProcessCoroutineList(nextFrameCoroutines);
        }

        [HideFromIl2Cpp]
        public void Stop(IEnumerator enumerator) {
            if (nextFrameCoroutines.Contains(enumerator)) // the coroutine is running itself
            {
                nextFrameCoroutines.Remove(enumerator);
            } else {
                int coroutineTupleIndex = coroutinesStore.FindIndex(c => c.Coroutine == enumerator);
                if (coroutineTupleIndex == -1) return;
                object waitCondition = coroutinesStore[coroutineTupleIndex].WaitCondition;
                if (waitCondition is IEnumerator waitEnumerator) Stop(waitEnumerator);

                coroutinesStore.RemoveAt(coroutineTupleIndex);
            }
        }

        [HideFromIl2Cpp]
        private void ProcessCoroutineList(List<IEnumerator> target) {
            if (target.Count == 0) return;

            // use a temp list to make sure waits made during processing are not handled by same processing invocation
            // additionally, a temp list reduces allocations compared to an array
            tempList.AddRange(target);
            target.Clear();
            foreach (IEnumerator enumerator in tempList) ProcessNextOfCoroutine(enumerator);
            tempList.Clear();
        }

        internal void ProcessWaitForFixedUpdate() {
            ProcessCoroutineList(waitForFixedUpdateCoroutines);
        }

        internal void ProcessWaitForEndOfFrame() {
            ProcessCoroutineList(waitForEndOfFrameCoroutines);
        }

        [HideFromIl2Cpp]
        private void ProcessNextOfCoroutine(IEnumerator enumerator) {
            try {
                if (!enumerator.MoveNext()
                ) // Run the next step of the coroutine. If it's done, restore the parent routine
                {
                    List<int> indices = coroutinesStore.Select((it, idx) => (idx, it))
                        .Where(it => it.it.WaitCondition == enumerator).Select(it => it.idx).ToList();
                    for (int i = indices.Count - 1; i >= 0; i--) {
                        int index = indices[i];
                        nextFrameCoroutines.Add(coroutinesStore[index].Coroutine);
                        coroutinesStore.RemoveAt(index);
                    }

                    return;
                }
            } catch (Exception e) {
                PogusPlugin.Logger.LogError(e.ToString());
                Stop(FindOriginalCoroutine(
                    enumerator)); // We want the entire coroutine hierarchy to stop when an error happen
            }

            object next = enumerator.Current;
            switch (next) {
                case null: {
                    nextFrameCoroutines.Add(enumerator);
                    return;
                }
                case WaitForFixedUpdate: {
                    waitForFixedUpdateCoroutines.Add(enumerator);
                    return;
                }
                case WaitForEndOfFrame: {
                    waitForEndOfFrameCoroutines.Add(enumerator);
                    return;
                }
                case WaitForSeconds: {
                    break; // do nothing, this one is supported in Process
                }
                case Il2CppObjectBase il2CppObjectBase: {
                    Il2CppSystem.Collections.IEnumerator nextAsEnumerator =
                        il2CppObjectBase.TryCast<Il2CppSystem.Collections.IEnumerator>();
                    if (nextAsEnumerator != null) // il2cpp IEnumerator also handles CustomYieldInstruction
                        next = new Il2CppEnumeratorWrapper(nextAsEnumerator);
                    else
                        PogusPlugin.Logger.LogWarning(
                            $"Unknown coroutine yield object of type {il2CppObjectBase} for coroutine {enumerator}");
                    break;
                }
            }

            coroutinesStore.Add(new CoroutineTuple {WaitCondition = next, Coroutine = enumerator});

            if (next is IEnumerator nextCoroutine)
                ProcessNextOfCoroutine(nextCoroutine);
        }

        [HideFromIl2Cpp]
        private IEnumerator FindOriginalCoroutine(IEnumerator enumerator) {
            int index = coroutinesStore.FindIndex(ct => ct.WaitCondition == enumerator);
            return index == -1 ? enumerator : FindOriginalCoroutine(coroutinesStore[index].Coroutine);
        }

        private struct CoroutineTuple {
            public object WaitCondition;
            public object Owner;
            public IEnumerator Coroutine;
        }

        private class Il2CppEnumeratorWrapper : IEnumerator {
            private readonly Il2CppSystem.Collections.IEnumerator _il2CPPEnumerator;

            public Il2CppEnumeratorWrapper(Il2CppSystem.Collections.IEnumerator il2CppEnumerator) {
                _il2CPPEnumerator = il2CppEnumerator;
            }

            public bool MoveNext() {
                return _il2CPPEnumerator.MoveNext();
            }

            public void Reset() {
                _il2CPPEnumerator.Reset();
            }

            public object Current => _il2CPPEnumerator.Current;
        }
    }
}