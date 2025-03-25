using Exiled.Events.EventArgs.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomRoles.Manager;

namespace UncomplicatedCustomRoles.API.Features.CustomModules
{
    public abstract class CustomModule
    {
        /// <summary>
        /// Gets the display name of the given <see cref="CustomModule"/>
        /// </summary>
        /// <value>Default one is the class' name</value>
        public virtual string Name
        {
            get
            {
                return GetType().Name;
            }
        }

        /// <summary>
        /// Gets the list of events that this <see cref="CustomModule"/> will listen for.
        /// </summary>
        /// <remarks>The <see cref="OnEvent(string, IPlayerEvent)"/> will be invoked only for the given events!</remarks>
        public virtual List<string> TriggerOnEvents { get; } = new();

        /// <summary>
        /// Gets the list of required argument names for the current <see cref="CustomModule"/>
        /// </summary>
        public virtual List<string> RequiredArgs { get; } = new();

        /// <summary>
        /// Gets the args of the current <see cref="CustomModule"/>
        /// </summary>
        /// <remarks>Every value is a <see cref="string"/></remarks>
        internal Dictionary<string, string> Args { get; private set; }

        /// <summary>
        /// Gets the instance of the <see cref="SummonedCustomRole"/> in which the current <see cref="CustomModule"/> is embedded
        /// </summary>
        internal SummonedCustomRole CustomRole { get; private set; }

        internal void Initialize(SummonedCustomRole summonedCustomRole, Dictionary<string, string> args)
        {
            CustomRole = summonedCustomRole;
            Args = args;
        }

        /// <summary>
        /// The added event function
        /// </summary>
        /// <remarks>Invoked when the <see cref="CustomModule"/> has been added to the <see cref="SummonedCustomRole"/></remarks>
        public virtual void OnAdded()
        { }

        /// <summary>
        /// The removed event function
        /// </summary>
        /// <remarks>Invoked when the <see cref="CustomModule"/> has been removed from the <see cref="SummonedCustomRole"/></remarks>
        public virtual void OnRemoved()
        { }

        /// <summary>
        /// The generic event function
        /// </summary>
        /// <param name="name"></param>
        /// <param name="ev"></param>
        /// <returns>Invoked only for the events listed in <see cref="TriggerOnEvents"/></returns>
        public virtual bool OnEvent(string name, IPlayerEvent ev) => true;

        /// <summary>
        /// A generic function
        /// </summary>
        /// <remark>This won't be invoked by UCR</remark>
        public virtual void Execute()
        { }

#nullable enable
        internal static List<CustomModule> Load(List<object> modules, SummonedCustomRole summonedCustomRole)
        {
            LogManager.Silent($"[CM Loader] Initialize loading for {summonedCustomRole}\nPreloaded {YamlFlagsHandler.Modules.Length} modules...");

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
                LogManager.Error($"Failed to enable CustomModule '{type?.Name}'!\nError: ERR_CUSTOM_MODULE_NULLREFERENCE_OR_NOTMODULE", "CM0003");
                return null;
            }

            module.Initialize(role, args ?? new());
            module.OnAdded(); // Invoke added event

            return module;
        }

        private static CustomModule? InitializeCustomModule(string name, Dictionary<string, string>? args, Type[] types, SummonedCustomRole summonedCustomRole)
        {
            try
            {
                LogManager.Silent($"[CM Loader] Initialize loading module '{name}' for {summonedCustomRole}");

                Type type = types.FirstOrDefault(t => t.Name == name);

                if (type is null)
                {
                    LogManager.Error($"[CM Loader] Failed to enable CustomModule '{name}'!\nError: ERR_CUSTOM_MODULE_NOT_FOUND", "CM0001");
                    return null;
                }

                if (Activator.CreateInstance(type) is not CustomModule module)
                {
                    LogManager.Error($"[CM Loader] Failed to enable CustomModule '{name}'!\nError: ERR_CUSTOM_MODULE_NULLREFERENCE_OR_NOTMODULE", "CM0002");
                    return null;
                }

                module.Initialize(summonedCustomRole, args ?? new());
                module.OnAdded(); // Invoke added event

                LogManager.Silent($"[CM Loader] CustomModule '{name}' successfully enabled for {summonedCustomRole}!");

                return module;
            }
            catch (Exception e)
            {
                LogManager.Error(e.ToString());

                return null;
            }
        }
    }
}
