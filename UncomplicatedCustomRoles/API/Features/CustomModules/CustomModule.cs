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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LabApi.Events.Arguments.Interfaces;
using LabApi.Features.Wrappers;
using UncomplicatedCustomRoles.Manager;

namespace UncomplicatedCustomRoles.API.Features.CustomModules;

public abstract class CustomModule
{
    private readonly Dictionary<ArgKey, object> _castedLists = [];

    private readonly Dictionary<ArgKey, object> _castedValues = [];

    private readonly HashSet<ArgKey> _unconvertibleValues = [];

    private Dictionary<string, string> _stringArgs;

    /// <summary>
    ///     Gets the display name of the given <see cref="CustomModule" />
    /// </summary>
    /// <value>Default one is the class' name</value>
    public virtual string Name => GetType().Name;

    /// <summary>
    ///     Gets the list of events that this <see cref="CustomModule" /> will listen for.
    /// </summary>
    /// <remarks>The <see cref="OnEvent(string, IPlayerEvent)" /> will be invoked only for the given events!</remarks>
    public virtual List<string> TriggerOnEvents { get; } = [];

    /// <summary>
    ///     Gets the list of required argument names for the current <see cref="CustomModule" />
    /// </summary>
    public virtual List<string> RequiredArgs { get; } = [];

    /// <summary>
    ///     Gets the args of the current <see cref="CustomModule" />
    /// </summary>
    /// <remarks>Every value is a <see cref="string" /></remarks>
    public Dictionary<string, object> Args { get; private set; }

    /// <summary>
    ///     Gets the args of the current <see cref="CustomModule" /> with the value converted as string
    /// </summary>
    public Dictionary<string, string> StringArgs
    {
        get
        {
            if (_stringArgs is not null)
                return _stringArgs;

            _stringArgs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (Args is not null)
            {
                foreach (KeyValuePair<string, object> kvp in Args)
                    _stringArgs[kvp.Key] = kvp.Value?.ToString();
            }

            return _stringArgs;
        }
    }

    /// <summary>
    ///     Gets the instance of the <see cref="SummonedCustomRole" /> in which the current <see cref="CustomModule" /> is
    ///     embedded
    /// </summary>
    public SummonedCustomRole CustomRole { get; private set; }

    /// <summary>
    ///     Gets the instance of the <see cref="LabApi.Features.Wrappers.Player" /> in which the current
    ///     <see cref="CustomModule" /> is embedded
    /// </summary>
    public Player Player => CustomRole.Player;

    internal void Initialize(SummonedCustomRole summonedCustomRole, Dictionary<string, object> args)
    {
        CustomRole = summonedCustomRole;
        Args = args is null
            ? new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, object>(args, StringComparer.OrdinalIgnoreCase);

        InvalidateArgsCache();
    }

    public void InvalidateArgsCache()
    {
        _stringArgs = null;
        _castedValues.Clear();
        _castedLists.Clear();
        _unconvertibleValues.Clear();
    }

    public List<string> GetMissingArgs()
    {
        List<string> missing = [];

        if (RequiredArgs is null)
            return missing;

        foreach (string arg in RequiredArgs)
        {
            if (Args is null || !Args.TryGetValue(arg, out object value) || value is null ||
                (value is string text && string.IsNullOrWhiteSpace(text)))
                missing.Add(arg);
        }

        return missing;
    }

    /// <summary>
    ///     The added event function
    /// </summary>
    /// <remarks>Invoked when the <see cref="CustomModule" /> has been added to the <see cref="SummonedCustomRole" /></remarks>
    public virtual void OnAdded()
    {
    }

    /// <summary>
    ///     The removed event function
    /// </summary>
    /// <remarks>
    ///     Invoked when the <see cref="CustomModule" /> has been removed from the <see cref="SummonedCustomRole" />
    /// </remarks>
    public virtual void OnRemoved()
    {
    }

    /// <summary>
    ///     The generic event function
    /// </summary>
    /// <param name="name"></param>
    /// <param name="ev"></param>
    /// <returns>Invoked only for the events listed in <see cref="TriggerOnEvents" /></returns>
    public virtual bool OnEvent(string name, IPlayerEvent ev)
    {
        return true;
    }

    public virtual bool Validate(out string error)
    {
        error = null;
        return true;
    }

    /// <summary>
    ///     A generic function
    /// </summary>
    /// <remark>This won't be invoked by UCR</remark>
    public virtual void Execute()
    {
    }

    /// <summary>
    ///     Try to get a generic <see cref="object" /> value from the <see cref="Args" /> and if not present just return a
    ///     default value.
    /// </summary>
    /// <param name="param"></param>
    /// <param name="def"></param>
    /// <returns></returns>
    public object TryGetValue(string param, object def = null)
    {
        return Args is not null && Args.TryGetValue(param, out object value) ? value : def;
    }

    /// <summary>
    ///     Try to get a value from the <see cref="Args" /> and if not present just return a default value.
    /// </summary>
    /// <param name="param"></param>
    /// <param name="def"></param>
    /// <returns></returns>
    public string TryGetStringValue(string param, string def = null)
    {
        return StringArgs.TryGetValue(param, out string value) ? value : def;
    }

    /// <summary>
    ///     Try to get a value from the <see cref="Args" /> and if not present just return a default value, with the value
    ///     converted to the given type <see cref="T" />.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="param"></param>
    /// <param name="def"></param>
    /// <returns></returns>
    public T TryGetCastedValue<T>(string param, T def = default)
    {
        ArgKey key = new(param, typeof(T));

        if (_castedValues.TryGetValue(key, out object cached))
            return (T)cached;

        if (_unconvertibleValues.Contains(key) || Args is null || !Args.TryGetValue(param, out object value))
            return def;

        try
        {
            T converted = (T)Convert.ChangeType(value, typeof(T));
            _castedValues[key] = converted;
            return converted;
        }
        catch
        {
            _unconvertibleValues.Add(key);
            return def;
        }
    }

    /// <summary>
    ///     Try to get a value from the <see cref="Args" /> and if not present just return a default value, with the value
    ///     converted to a list of the given type <see cref="T" />.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="param"></param>
    /// <returns></returns>
    public List<T> TryGetCastedListValue<T>(string param)
    {
        ArgKey key = new(param, typeof(T));

        if (_castedLists.TryGetValue(key, out object cached))
            return (List<T>)cached;

        List<T> list = BuildCastedList<T>(param);
        _castedLists[key] = list;

        return list;
    }

    private List<T> BuildCastedList<T>(string param)
    {
        if (Args is null || !Args.TryGetValue(param, out object value) || value is null)
            return [];
        switch (value)
        {
            case T t:
                return [t];
            case List<T> listT:
                return listT;
            case IEnumerable<T> enumT:
                return enumT.ToList();
            case IEnumerable nonGenericEnum:
                List<T> result = nonGenericEnum is ICollection col ? new List<T>(col.Count) : new List<T>();
                foreach (object o in nonGenericEnum)
                {
                    if (TryConvertTo(o, out T converted))
                        result.Add(converted);
                }

                return result;
            default:
                return TryConvertTo(value, out T single) ? [single] : [];
        }
    }

    private static bool TryConvertTo<T>(object o, out T result)
    {
        try
        {
            result = ConvertTo<T>(o);
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }

    protected List<string> GetRawListEntries(string param)
    {
        if (Args is null || !Args.TryGetValue(param, out object value) || value is null)
            return [];

        if (value is string s)
            return [s];

        if (value is IEnumerable enumerable)
        {
            List<string> result = [];
            foreach (object o in enumerable)
            {
                if (o is not null)
                    result.Add(o.ToString());
            }

            return result;
        }

        return [value.ToString()];
    }

    protected List<string> GetInvalidEnumEntries<T>(string param) where T : struct
    {
        List<string> invalid = [];
        foreach (string raw in GetRawListEntries(param))
        {
            if (!Enum.TryParse(raw, true, out T _))
                invalid.Add(raw);
        }

        return invalid;
    }

    private static T ConvertTo<T>(object o)
    {
        Type type = typeof(T);
        if (!type.IsEnum)
            return (T)Convert.ChangeType(o, type);
        if (o is string s)
            return (T)Enum.Parse(type, s, true);
        return (T)Enum.ToObject(type, Convert.ChangeType(o, Enum.GetUnderlyingType(type)));
    }

    /// <summary>
    ///     Logs an error message indicating that the custom module failed to load or had an issue.
    /// </summary>
    public void ThrowError(string message)
    {
        LogManager.Error($"[CustomModule] Failed to load CustomModule '{Name}': {message}");
    }

#nullable enable
    internal static List<CustomModule> Load(List<object> modules, SummonedCustomRole summonedCustomRole)
    {
        LogManager.Silent($"[CM Loader] Initialize loading for {summonedCustomRole}\nPreloaded {YamlFlagsHandler.Modules.Length} modules...");

        List<KeyValuePair<string, Dictionary<string, object>?>> data = YamlFlagsHandler.Decode(modules) ?? [];

        List<CustomModule> mods = [];

        foreach (KeyValuePair<string, Dictionary<string, object>?> module in data)
        {
            if (InitializeCustomModule(module.Key, module.Value, YamlFlagsHandler.Modules, summonedCustomRole) is
                { } mod)
                mods.Add(mod);
        }

        LogManager.Debug($"Successfully loaded {mods.Count} CustomModules for player {summonedCustomRole.Player.Nickname}!");

        return mods;
    }

    internal static CustomModule? FastAdd(Type type, SummonedCustomRole role, Dictionary<string, object>? args = null)
    {
        if (Activator.CreateInstance(type) is not CustomModule module)
        {
            LogManager.Error(
                $"Failed to enable CustomModule '{type?.Name}'!\nError: ERR_CUSTOM_MODULE_NULLREFERENCE_OR_NOTMODULE",
                "CM0003");
            return null;
        }

        module.Initialize(role, args ?? new Dictionary<string, object>());

        if (!ValidateModule(module, type?.Name ?? module.Name, role))
            return null;

        module.OnAdded(); // Invoke added event

        return module;
    }

    private static CustomModule? InitializeCustomModule(string name, Dictionary<string, object>? args, Type[] types,
        SummonedCustomRole summonedCustomRole)
    {
        try
        {
            LogManager.Silent($"[CM Loader] Initialize loading module '{name}' for {summonedCustomRole}");

            Type? type = types.FirstOrDefault(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase));

            if (type is null)
            {
                LogManager.Error(
                    $"[CM Loader] Unknown CustomModule '{name}' on role {RoleLabel(summonedCustomRole)} - it will be ignored.\n" +
                    $"Available flags: {string.Join(", ", types.Select(t => t.Name).OrderBy(n => n))}", "CM0001");
                return null;
            }

            if (Activator.CreateInstance(type) is not CustomModule module)
            {
                LogManager.Error(
                    $"[CM Loader] Failed to instantiate CustomModule '{type.Name}' on role {RoleLabel(summonedCustomRole)}.",
                    "CM0002");
                return null;
            }

            module.Initialize(summonedCustomRole, args ?? new Dictionary<string, object>());

            if (!ValidateModule(module, type.Name, summonedCustomRole))
                return null;

            module.OnAdded(); // Invoke added event

            LogManager.Silent($"[CM Loader] CustomModule '{name}' successfully enabled for {summonedCustomRole}!");

            return module;
        }
        catch (Exception e)
        {
            LogManager.Error($"[CM Loader] Unexpected error while enabling CustomModule '{name}' on role {RoleLabel(summonedCustomRole)}:\n{e}");

            return null;
        }
    }

    private static bool ValidateModule(CustomModule module, string name, SummonedCustomRole role)
    {
        List<string> missing = module.GetMissingArgs();

        if (missing.Count > 0)
        {
            LogManager.Error(
                $"[CM Loader] CustomModule '{name}' on role {RoleLabel(role)} is missing required setting(s): {string.Join(", ", missing)}.\n" +
                $"Provided setting(s): {(module.Args.Count == 0 ? "(none)" : string.Join(", ", module.Args.Keys))}.\n" +
                "This flag will be skipped.", "CM0004");
            return false;
        }

        if (!module.Validate(out string? error))
        {
            LogManager.Error(
                $"[CM Loader] CustomModule '{name}' on role {RoleLabel(role)} has an invalid setting: {error}\n" +
                "This flag will be skipped.", "CM0005");
            return false;
        }

        return true;
    }

    private static string RoleLabel(SummonedCustomRole role)
    {
        return role?.Role is null ? "?" : $"{role.Role.Name} ({role.Role.Id})";
    }

    private readonly struct ArgKey : IEquatable<ArgKey>
    {
        private readonly string _param;

        private readonly Type _type;

        internal ArgKey(string param, Type type)
        {
            _param = param;
            _type = type;
        }

        public bool Equals(ArgKey other)
        {
            return _type == other._type && string.Equals(_param, other._param, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object? obj)
        {
            return obj is ArgKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_param is null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(_param)) * 397) ^
                       (_type?.GetHashCode() ?? 0);
            }
        }
    }
}