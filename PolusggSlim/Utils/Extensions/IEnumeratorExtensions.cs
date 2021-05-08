using System.Collections;

namespace PolusggSlim.Utils.Extensions
{
    public static class IEnumeratorExtensions
    {
        public static IEnumerator StartAsCoroutine(this IEnumerator enumerator)
        {
            return Coroutine.Start(enumerator);
        }
    }
}