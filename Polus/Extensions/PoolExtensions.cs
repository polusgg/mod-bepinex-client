using Il2CppSystem.Collections.Generic;
using UnhollowerBaseLib;

namespace Polus.Extensions {
    public static class PoolExtensions {
        public static T GetBetter<T>(this ObjectPoolBehavior pool) where T : Il2CppObjectBase {
            List<PoolableBehavior> obj = pool.inactiveChildren;
            PoolableBehavior poolableBehavior;
            lock (obj)
            {
                if (pool.inactiveChildren.Count == 0)
                {
                    if (pool.activeChildren.Count == 0)
                    {
                        pool.InitPool(pool.Prefab);
                    }
                    else
                    {
                        pool.CreateOneInactive(pool.Prefab);
                    }
                }
                poolableBehavior = pool.inactiveChildren.ToArray()[pool.inactiveChildren.Count - 1];
                pool.inactiveChildren.RemoveAt(pool.inactiveChildren.Count - 1);
                pool.activeChildren.Add(poolableBehavior);
                int num = pool.childIndex;
                pool.childIndex = num + 1;
                poolableBehavior.PoolIndex = num;
                if (pool.childIndex > pool.poolSize)
                {
                    pool.childIndex = 0;
                }
            }
            if (pool.DetachOnGet)
            {
                poolableBehavior.transform.SetParent(null, false);
            }
            poolableBehavior.gameObject.SetActive(true);
            poolableBehavior.Reset();
            return poolableBehavior.TryCast<T>();
        }
    }
}