using Exiled.Events.EventArgs.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomRoles.Elements;
using UncomplicatedCustomRoles.Structures;

#pragma warning disable IDE1006
#pragma warning disable IDE0038
#nullable enable
namespace UncomplicatedCustomRoles.API.Features
{
    public class Events
    {
        internal static Dictionary<UCREvents, List<Func<ICustomRoleEvent, ICustomRoleEvent>>> Handlers { get; set; } = new();

        /// <summary>
        /// Register a new <see cref="Func{ICustomRoleEvent, ICustomRoleEvent}">Event Handler</see> for a <see cref="UCREvents"/>.
        /// </summary>
        public static void AddListener(UCREvents Event, Func<ICustomRoleEvent, ICustomRoleEvent> Handler, bool Prioritize = false)
        {
            Listen(Event, Handler, Prioritize);
        }

        /// <summary>
        /// Register a new <see cref="Func{ICustomRoleEvent, ICustomRoleEvent}">Event Handler</see> for a <see cref="UCREvents"/>.
        /// </summary>
        public static void RegisterListener(UCREvents Event, Func<ICustomRoleEvent, ICustomRoleEvent> Handler, bool Prioritize = false)
        {
            Listen(Event, Handler, Prioritize);
        }

        /// <summary>
        /// Register a new <see cref="Func{ICustomRoleEvent, ICustomRoleEvent}">Event Handler</see> for a <see cref="UCREvents"/>.
        /// </summary>
        public static void Listen(UCREvents Event, Func<ICustomRoleEvent, ICustomRoleEvent> Handler, bool Prioritize = false)
        {
            if (!Handlers.ContainsKey(Event))
            {
                Handlers.Add(Event, new() {
                    Handler
                });
            } 
            else
            {
                if (Prioritize)
                {
                    List<Func<ICustomRoleEvent, ICustomRoleEvent>> NewHandlers = new()
                    {
                        Handler
                    };
                    foreach (Func<ICustomRoleEvent, ICustomRoleEvent> Action in Handlers[Event])
                    {
                        NewHandlers.Add(Action);
                    }
                    Handlers[Event] = NewHandlers;
                } else
                {
                    Handlers[Event].Add(Handler);
                }
            }
        }

        /// <summary>
        /// Unregister a <see cref="Func{ICustomRoleEvent, ICustomRoleEvent}">Event Handler</see> of a <see cref="UCREvents"/>.
        /// </summary>
        public static void Unlisten(UCREvents Event, Func<ICustomRoleEvent, ICustomRoleEvent> Handler)
        {
            RemoveListener(Event, Handler);
        }

        /// <summary>
        /// Unregister a <see cref="Func{ICustomRoleEvent, ICustomRoleEvent}">Event Handler</see> of a <see cref="UCREvents"/>.
        /// </summary>
        public static void UnregisterListener(UCREvents Event, Func<ICustomRoleEvent, ICustomRoleEvent> Handler)
        {
            RemoveListener(Event, Handler);
        }

        /// <summary>
        /// Unregister a <see cref="Func{ICustomRoleEvent, ICustomRoleEvent}">Event Handler</see> of a <see cref="UCREvents"/>.
        /// </summary>
        public static void RemoveListener(UCREvents Event, Func<ICustomRoleEvent, ICustomRoleEvent> Handler)
        {
            if (Handlers.ContainsKey(Event) && Handlers[Event].Contains(Handler))
            {
                Handlers.Remove(Event);
            }
        }

        /// <summary>
        /// Unregister all of the <see cref="Func{ICustomRoleEvent, ICustomRoleEvent}">Event Handler</see> of every <see cref="UCREvents"/>.
        /// </summary>
        public static void ClearAllListeners()
        {
            Handlers = new();
        }

        /// <summary>
        /// Unregister all of the <see cref="Func{ICustomRoleEvent, ICustomRoleEvent}">Event Handler</see> of a specific <see cref="UCREvents"/>.
        /// </summary>
        public static void ClearAllListeners(UCREvents Event)
        {
            if (Handlers.ContainsKey(Event))
            {
                Handlers[Event] = new();
            }
        }

        internal static IPlayerEvent? __CallEvent(UCREvents Event, IPlayerEvent Base)
        {
            // Let's see if the player is a custom role
            if (!Plugin.PlayerRegistry.ContainsKey(Base.Player.Id))
            {
                return null;
            }
            ICustomRoleEvent InternalEvent = new CustomRoleEvent(Plugin.CustomRoles[Plugin.PlayerRegistry[Base.Player.Id]], Event, Base);
            bool IsAllowed = true;
            if (Base is IDeniableEvent)
            {
                IsAllowed = ((IDeniableEvent)Base).IsAllowed;
            }
            if (Handlers.ContainsKey(Event))
            {
                foreach (Func<ICustomRoleEvent, ICustomRoleEvent> Action in Handlers[Event].Where(action => action is not null))
                {
                    IsAllowed = IsAllowed && Action(InternalEvent).IsAllowed;
                }
            }
            if (Base is IDeniableEvent)
            {
                ((IDeniableEvent)Base).IsAllowed = IsAllowed;
            }
            return Base;
        }
    }
}
