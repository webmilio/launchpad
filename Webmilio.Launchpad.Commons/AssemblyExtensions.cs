using System;
using System.Collections.Generic;
using System.Reflection;

namespace Webmilio.Launchpad.Commons
{
    public static class AssemblyExtensions
    {
        public static IEnumerable<Type> GetAllSubtypesOf<T>(this Assembly assembly)
        {
            foreach (TypeInfo type in assembly.DefinedTypes)
            {
                if (type.IsInterface || type.IsAbstract)
                    continue;

                if (type.IsSubclassOf(typeof(T)))
                    yield return type.AsType();
            }
        }
    }
}