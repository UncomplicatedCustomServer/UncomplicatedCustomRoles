using Exiled.Events.EventArgs.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomRoles.Manager;
using static Mono.Security.X509.X520;

namespace UncomplicatedCustomRoles.API.Features.CustomModules
{
    public abstract class CustomModule
    {
        /// <summary>
        /// The display name of the given <see cref="CustomModule"/>
        /// </summary>
        /// <def
        public virtual string Name
        {
            get
            {
                return GetType().Name;
            }
        }

        public virtual List<string> TriggerOnEvents { get; } = new();

        public virtual List<string> RequiredArgs { get; } = new();

        private Dictionary<string, string> Args { get; set; }

        private SummonedCustomRole CustomRole { get; set; }

        internal void Initialize(SummonedCustomRole summonedCustomRole, Dictionary<string, string> args)
        {
            CustomRole = summonedCustomRole;
            Args = args;
        }

        public virtual void OnAdded()
        { }

        public virtual void OnRemoved()
        { }

        public virtual bool OnEvent(string name, IPlayerEvent ev) => true;

        public virtual void Execute()
        { }

#nullable enable
        internal static List<CustomModule> Load(List<object> modules, SummonedCustomRole summonedCustomRole)
        {
            Dictionary<string, Dictionary<string, string>?> data = YamlFlagsHandler.Decode(modules) ?? new();

            List<CustomModule> mods = new();

            foreach (KeyValuePair<string, Dictionary<string, string>?> module in data)
                if (InitializeCustomModule(module.Key, module.Value, YamlFlagsHandler.Modules, summonedCustomRole) is CustomModule mod)
                    mods.Add(mod);

            LogManager.Debug($"Successfully loaded {mods.Count} CustomModules for player {summonedCustomRole.Player.Nickname}!");

            return mods;
        }

        internal static CustomModule? FastAdd(Type type, SummonedCustomRole role, Dictionary<string, string>? args = null)
        {
            if (Activator.CreateInstance(type) is not CustomModule module)
            {
                LogManager.Error($"Failed to enable CustomModule '{type?.Name}'!\nError: ERR_CUSTOM_MODULE_NULLREFERENCE", "CM0003");
                return null;
            }

            module.Initialize(role, args ?? new());
            module.OnAdded(); // Invoke added event

            return module;
        }

        private static CustomModule? InitializeCustomModule(string name, Dictionary<string, string>? args, Type[] types, SummonedCustomRole summonedCustomRole)
        {
            Type type = types.FirstOrDefault(t => t.Name == name);

            if (type is null)
            {
                LogManager.Error($"Failed to enable CustomModule '{name}'!\nError: ERR_CUSTOM_MODULE_NOT_FOUND", "CM0001");
                return null;
            }

            if (Activator.CreateInstance(type) is not CustomModule module)
            {
                LogManager.Error($"Failed to enable CustomModule '{name}'!\nError: ERR_CUSTOM_MODULE_NULLREFERENCE", "CM0002");
                return null;
            }

            module.Initialize(summonedCustomRole, args ?? new());
            module.OnAdded(); // Invoke added event

            LogManager.Silent($"CustomModule '{name}' successfully enabled for player {summonedCustomRole.Player.Nickname} ({summonedCustomRole.Player.Id}) and CustomRole {summonedCustomRole.Role.Id} ({summonedCustomRole.Role.Name})!");

            return module;
        }
    }
}
