using System;
using UncomplicatedCustomRoles.Events.Enums;

namespace UncomplicatedCustomRoles.Events.Attributes
{
    internal class InternalPluginEvent : Attribute
    {
        public EventName Name { get; }

        public InternalPluginEvent(EventName name)
        {
            Name = name;
        }
    }
}
