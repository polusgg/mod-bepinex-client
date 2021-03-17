using System;
using UnhollowerBaseLib;
using UnhollowerBaseLib.Runtime;

namespace PolusMod {
    public static class ObjectBaseExtensions {
        // public static Il2CppObjectBase TryTypedCast(this Il2CppObjectBase obj, Type typed)
        // {
        //     System.IntPtr nativeClassPtr = Il2CppClassPointerStore<>.NativeClassPtr;
        //     if (nativeClassPtr == IntPtr.Zero)
        //         throw new System.ArgumentException(string.Format("{0} is not al Il2Cpp reference type", typed));
        //     System.IntPtr num = IL2CPP.il2cpp_object_get_class(obj.Pointer);
        //     if (!IL2CPP.il2cpp_class_is_assignable_from(nativeClassPtr, num)) { }
        //     if (RuntimeSpecificsStore.IsInjected(num))
        //         return (Il2CppObjectBase) ClassInjectorBase.GetMonoObjectFromIl2CppPointer(obj.Pointer);
        //     return (Il2CppObjectBase) Activator.CreateInstance(typed, (object) obj.Pointer);
        // }
        // public static Il2CppObjectBase TypedCast() {
        //     
        // }
    }
}