using System.Collections;
using PolusggSlim.Coroutines;

namespace PolusggSlim.Utils.Extensions
{
    public static class EnumeratorExtensions
    {
        public static IEnumerator StartAsCoroutine(this IEnumerator enumerator)
        {
            return Coroutine.Start(enumerator);
        }
    }
}