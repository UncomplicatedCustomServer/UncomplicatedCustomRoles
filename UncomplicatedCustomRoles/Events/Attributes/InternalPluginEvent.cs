using System;

namespace UncomplicatedCustomRoles.Events.Attributes
{
    internal class InternalPluginEvent : Attribute
    {
        public string Name { get; }

        public InternalPluginEvent(string name)
        {
            Name = name;
        }
    }
}
