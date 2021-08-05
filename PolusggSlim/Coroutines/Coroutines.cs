using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PolusggSlim.Utils;
using UnhollowerBaseLib;
using UnityEngine;

namespace PolusggSlim.Coroutines
{
    public static class Coroutine
    {
        public static readonly List<CoroutineTuple> CoroutinesStore = new();
        private static readonly List<IEnumerator> NextFrameCoroutines = new();

        private static readonly List<IEnumerator> TempList = new();
        private static readonly List<IEnumerator> WaitForEndOfFrameCoroutines = new();
        private static readonly List<IEnumerator> WaitForFixedUpdateCoroutines = new();

        public static IEnumerator Start(IEnumerator routine)
        {
            ProcessNextOfCoroutine(routine);
            return routine;
        }

        public static void Stop(IEnumerator enumerator)
        {
            if (NextFrameCoroutines.Contains(enumerator)) // the coroutine is running itself
            {
                NextFrameCoroutines.Remove(enumerator);
            }
            else
            {
                var coroutineTupleIndex = CoroutinesStore.FindIndex(c => c.Coroutine == enumerator);
                if (coroutineTupleIndex == -1) return;
                var waitCondition = CoroutinesStore[coroutineTupleIndex].WaitCondition;
                if (waitCondition is IEnumerator waitEnumerator) Stop(waitEnumerator);

                CoroutinesStore.RemoveAt(coroutineTupleIndex);
            }
        }

        private static void ProcessCoroutineList(List<IEnumerator> target)
        {
            if (target.Count == 0) return;

            // use a temp list to make sure waits made during processing are not handled by same processing invocation
            // additionally, a temp list reduces allocations compared to an array
            TempList.AddRange(target);
            target.Clear();
            foreach (var enumerator in TempList) ProcessNextOfCoroutine(enumerator);
            TempList.Clear();
        }

        public static void Process()
        {
            for (var i = CoroutinesStore.Count - 1; i >= 0; i--)
            {
                var tuple = CoroutinesStore[i];
                if (!(tuple.WaitCondition is WaitForSeconds waitForSeconds)) continue;
                if ((waitForSeconds.m_Seconds -= Time.deltaTime) > 0) continue;
                CoroutinesStore.RemoveAt(i);
                ProcessNextOfCoroutine(tuple.Coroutine);
            }

            ProcessCoroutineList(NextFrameCoroutines);
        }

        internal static void ProcessWaitForFixedUpdate()
        {
            ProcessCoroutineList(WaitForFixedUpdateCoroutines);
        }

        internal static void ProcessWaitForEndOfFrame()
        {
            ProcessCoroutineList(WaitForEndOfFrameCoroutines);
        }

        private static void ProcessNextOfCoroutine(IEnumerator enumerator)
        {
            try
            {
                if (!enumerator.MoveNext()
                ) // Run the next step of the coroutine. If it's done, restore the parent routine
                {
                    var indices = CoroutinesStore.Select((it, idx) => (idx, it))
                        .Where(it => it.it.WaitCondition == enumerator).Select(it => it.idx).ToList();
                    for (var i = indices.Count - 1; i >= 0; i--)
                    {
                        var index = indices[i];
                        NextFrameCoroutines.Add(CoroutinesStore[index].Coroutine);
                        CoroutinesStore.RemoveAt(index);
                    }

                    return;
                }
            }
            catch (Exception e)
            {
                PggLog.Error(e.ToString());
                Stop(FindOriginalCoroutine(
                    enumerator)); // We want the entire coroutine hierarchy to stop when an error happen
            }

            var next = enumerator.Current;
            switch (next)
            {
                case null:
                {
                    NextFrameCoroutines.Add(enumerator);
                    return;
                }
                case WaitForFixedUpdate:
                {
                    WaitForFixedUpdateCoroutines.Add(enumerator);
                    return;
                }
                case WaitForEndOfFrame:
                {
                    WaitForEndOfFrameCoroutines.Add(enumerator);
                    return;
                }
                case WaitForSeconds:
                {
                    break; // do nothing, this one is supported in Process
                }
                case Il2CppObjectBase il2CppObjectBase:
                {
                    var nextAsEnumerator =
                        il2CppObjectBase.TryCast<Il2CppSystem.Collections.IEnumerator>();
                    if (nextAsEnumerator != null) // il2cpp IEnumerator also handles CustomYieldInstruction
                        next = new Il2CppEnumeratorWrapper(nextAsEnumerator);
                    else
                        PggLog.Warning(
                            $"Unknown coroutine yield object of type {il2CppObjectBase} for coroutine {enumerator}");
                    break;
                }
            }

            CoroutinesStore.Add(new CoroutineTuple {WaitCondition = next, Coroutine = enumerator});

            if (next is IEnumerator nextCoroutine)
                ProcessNextOfCoroutine(nextCoroutine);
        }

        private static IEnumerator FindOriginalCoroutine(IEnumerator enumerator)
        {
            var index = CoroutinesStore.FindIndex(ct => ct.WaitCondition == enumerator);
            return index == -1 ? enumerator : FindOriginalCoroutine(CoroutinesStore[index].Coroutine);
        }

        public struct CoroutineTuple
        {
            public object WaitCondition;
            public IEnumerator Coroutine;
        }

        public class Il2CppEnumeratorWrapper : IEnumerator
        {
            private readonly Il2CppSystem.Collections.IEnumerator _il2CPPEnumerator;

            public Il2CppEnumeratorWrapper(Il2CppSystem.Collections.IEnumerator il2CppEnumerator)
            {
                _il2CPPEnumerator = il2CppEnumerator;
            }

            public bool MoveNext()
            {
                return _il2CPPEnumerator.MoveNext();
            }

            public void Reset()
            {
                _il2CPPEnumerator.Reset();
            }

            public object Current => _il2CPPEnumerator.Current;
        }
    }
}