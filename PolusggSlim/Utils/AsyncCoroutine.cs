using System;
using System.Collections;
using System.Threading.Tasks;

namespace PolusggSlim.Utils
{
    public static class AsyncCoroutine
    {
        public static IEnumerator CoContinueTaskWith(Task<bool> task, Action continueWith)
        {
            while (!task.IsCompleted)
                yield return null;

            if (task.Result)
            {
                continueWith.Invoke();
            }
        }
    }
}