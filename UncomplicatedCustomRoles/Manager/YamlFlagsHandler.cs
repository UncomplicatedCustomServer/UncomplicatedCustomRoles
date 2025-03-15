using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UncomplicatedCustomRoles.API.Features.CustomModules;

namespace UncomplicatedCustomRoles.Manager
{
#nullable enable

    class YamlFlagsHandler
    {
        public static Type[] Modules
        {
            get
            {
                _modules ??= GetModules();
                return _modules;
            }
        }

        private static Type[]? _modules = null;

        public static Dictionary<string, Dictionary<string, string>?>? Decode(List<object> flags)
        {
            if (flags is null)
                return null;

            Dictionary<string, Dictionary<string, string>?> result = new();

            foreach (object flag in flags)
            {
                if (flag is Dictionary<string, Dictionary<string, string>> str)
                    foreach (KeyValuePair<string, Dictionary<string, string>> res in str)
                        result[res.Key] = res.Value;
                else
                    result[flag.ToString()] = null;
            }

            return result;
        }

        public static Type[] GetModules()
        {
            List<Type> types = new();

            foreach (Assembly assembly in ImportManager.AvailableAssemblies)
                foreach (Type type in assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(CustomModule))))
                    types.Add(type);

            return types.ToArray();
        }
    }
}
