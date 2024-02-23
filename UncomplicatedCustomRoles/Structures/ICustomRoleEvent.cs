using Exiled.API.Features;
using Exiled.Events.EventArgs.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UncomplicatedCustomRoles.Structures
{
    public interface ICustomRoleEvent
    {
        public abstract Player Player { get; }
        public abstract ICustomRole Role {  get; }
        public abstract UCREvents EventType { get; }
        public abstract IPlayerEvent Event { get; }
        public bool IsAllowed { get; set; }
    }
}
