using Exiled.Events.EventArgs.Interfaces;
using System;
using System.Collections.Generic;
using UncomplicatedCustomRoles.Elements;
using UncomplicatedCustomRoles.Structures;

#pragma warning disable IDE1006
#pragma warning disable IDE0038
#nullable enable
namespace UncomplicatedCustomRoles.API.Features
{
    public class Events
    {
        public static Dictionary<UCREvents, List<Func<ICustomRoleEvent, ICustomRoleEvent>>> Handlers { get; set; } = new();

        public static void AddListener(UCREvents Event, Func<ICustomRoleEvent, ICustomRoleEvent> Handler, bool Prioritize = false)
        {
            Listen(Event, Handler, Prioritize);
        }

        public static void RegisterListener(UCREvents Event, Func<ICustomRoleEvent, ICustomRoleEvent> Handler, bool Prioritize = false)
        {
            Listen(Event, Handler, Prioritize);
        }

        public static void Listen(UCREvents Event, Func<ICustomRoleEvent, ICustomRoleEvent> Handler, bool Prioritize = false)
        {
            if (Handlers.ContainsKey(Event))
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

        public static void Unlisten(UCREvents Event, Func<ICustomRoleEvent, ICustomRoleEvent> Handler)
        {
            RemoveListener(Event, Handler);
        }

        public static void UnregisterListener(UCREvents Event, Func<ICustomRoleEvent, ICustomRoleEvent> Handler)
        {
            RemoveListener(Event, Handler);
        }

        public static void RemoveListener(UCREvents Event, Func<ICustomRoleEvent, ICustomRoleEvent> Handler)
        {
            if (Handlers.ContainsKey(Event) && Handlers[Event].Contains(Handler))
            {
                Handlers.Remove(Event);
            }
        }

        public static void ClearAllListeners()
        {
            Handlers = new();
        }

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
            if (Handlers.ContainsKey(Event))
            {
                foreach (Func<ICustomRoleEvent, ICustomRoleEvent> Action in Handlers[Event])
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
