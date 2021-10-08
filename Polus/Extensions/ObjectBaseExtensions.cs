using System;
using UnhollowerBaseLib;
using UnhollowerBaseLib.Runtime;

namespace Polus.Extensions {
    public static class ObjectBaseExtensions {
        private static Type cps;

        public static T TryTypedCast<T>(this Il2CppObjectBase obj, Type typed) where T : Il2CppObjectBase {
            cps ??= typeof(Il2CppClassPointerStore<Il2CppObjectBase>).GetGenericTypeDefinition();
            IntPtr nativeClassPtr = (IntPtr) (cps.MakeGenericType(typed).GetField(nameof(Il2CppClassPointerStore<Object>.NativeClassPtr))?.GetValue(null) ?? IntPtr.Zero);
            if (nativeClassPtr == IntPtr.Zero)
                throw new ArgumentException($"{typed} is not an Il2Cpp reference type");
            IntPtr num = IL2CPP.il2cpp_object_get_class(obj.Pointer);
            if (!IL2CPP.il2cpp_class_is_assignable_from(nativeClassPtr, num))
                return default(T);

            if (RuntimeSpecificsStore.IsInjected(num))
                return (T) ClassInjectorBase.GetMonoObjectFromIl2CppPointer(obj.Pointer);
            return (T) Activator.CreateInstance(typed, (object) obj.Pointer);
        }

        public static bool TryGetCast<T>(this Il2CppObjectBase obj, out T casted) where T : Il2CppObjectBase {
            casted = obj.TryCast<T>();
            return casted is not null;
        }
    }
}