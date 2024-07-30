using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UncomplicatedCustomRoles.Events.Args;
using UncomplicatedCustomRoles.Events.Attributes;
using UncomplicatedCustomRoles.Events.Interfaces;

namespace UncomplicatedCustomRoles.Events
{
    public class EventManager
    {
        public static readonly Dictionary<string, List<MethodInfo>> Events = new();

        public static void RegisterEvents(object handler)
        {
            foreach (MethodInfo Method in handler.GetType().GetMethods(BindingFlags.Static | BindingFlags.Public))
            {
                InternalPluginEvent Attribute = Method.GetCustomAttributes(typeof(InternalPluginEvent), false).FirstOrDefault() as InternalPluginEvent;

                if (Attribute is not null)
                    if (Events.ContainsKey(Attribute.Name))
                        Events[Attribute.Name].Add(Method);
                    else
                        Events.Add(Attribute.Name, new() { Method });
            }
        }

        public static void InvokeEvent(string name, EventArgs param)
        {
            if (Events.ContainsKey(name) && Events[name].Count() > 0)
                foreach (MethodInfo Method in Events[name])
                    Method.Invoke(null, new object[] { param });
        }
    }
}
