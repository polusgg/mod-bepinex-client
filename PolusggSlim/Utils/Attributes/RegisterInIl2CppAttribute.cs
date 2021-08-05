using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnhollowerRuntimeLib;

namespace PolusggSlim.Utils.Attributes
{
    /// <summary>
    ///     Utility attribute for automatically calling
    ///     <see cref="ClassInjector.RegisterTypeInIl2Cpp(System.Type)" />
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class RegisterInIl2CppAttribute : Attribute
    {
        private static readonly AccessTools.FieldRef<object, HashSet<string>> InjectedTypes
            = AccessTools.FieldRefAccess<HashSet<string>>(typeof(ClassInjector), "InjectedTypes");

        private static readonly Func<Type, IntPtr> ReadClassPointerForType =
            AccessTools.MethodDelegate<Func<Type, IntPtr>>(
                AccessTools.Method(typeof(ClassInjector), "ReadClassPointerForType")
            );

        public static void Register()
        {
            Register(Assembly.GetCallingAssembly());
        }

        public static void Register(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
                if (type.GetCustomAttribute<RegisterInIl2CppAttribute>() != null)
                    Register(type);
        }

        private static void Register(Type type)
        {
            if (type.BaseType?.GetCustomAttribute<RegisterInIl2CppAttribute>() != null)
                Register(type.BaseType);

            if (IsInjected(type))
                return;

            try
            {
                ClassInjector.RegisterTypeInIl2Cpp(type);
            }
            catch (Exception e)
            {
                PggLog.Warning($"Failed to register {type.FullDescription()}: {e}");
            }
        }

        private static bool IsInjected(Type type)
        {
            if (ReadClassPointerForType(type) != IntPtr.Zero)
                return true;

            var injectedTypes = InjectedTypes();

            lock (injectedTypes)
            {
                if (injectedTypes.Contains(type.FullName))
                    return true;
            }

            return false;
        }
    }
}