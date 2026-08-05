/*
 * This file is a part of the UncomplicatedCustomRoles project.
 *
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 *
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomRoles.API.Features.CustomModules;
using UncomplicatedCustomRoles.Extensions;

namespace UncomplicatedCustomRoles.Manager;
#nullable enable

internal class YamlFlagsHandler
{
    private static Type[]? _modules;

    public static Type[] Modules
    {
        get
        {
            _modules ??= GetModules();
            return _modules;
        }
    }

    internal static void InvalidateCache()
    {
        _modules = null;
    }

    public static List<KeyValuePair<string, Dictionary<string, object>?>>? Decode(List<object> flags)
    {
        if (flags is null)
            return null;

        List<KeyValuePair<string, Dictionary<string, object>?>> result = [];

        foreach (var flag in flags)
            if (flag is Dictionary<object, object> str)
            {
                foreach (var res in str)
                    if (res.Value is Dictionary<object, object> dict)
                        result.Add(new KeyValuePair<string, Dictionary<string, object>?>(res.Key.ToString(),
                            dict.ConvertKeyToString()));
                    else if (res.Value is null)
                        result.Add(new KeyValuePair<string, Dictionary<string, object>?>(res.Key.ToString(), null));
                    else
                        LogManager.Warn(
                            $"[CM Loader] The custom flag '{res.Key}' has its settings written as '{res.Value}' instead of a list of 'setting: value' lines, so it can't be read and will be ignored.");
            }
            else
            {
                result.Add(new KeyValuePair<string, Dictionary<string, object>?>(flag.ToString(), null));
            }

        return result;
    }

    public static Type[] GetModules()
    {
        List<Type> types = [];

        foreach (var assembly in ImportManager.AvailableAssemblies)
        foreach (var type in assembly.GetTypes()
                     .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(CustomModule))))
            types.Add(type);

        return types.ToArray();
    }
}