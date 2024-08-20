using UncomplicatedCustomRoles.API.Enums;
using UncomplicatedCustomRoles.API.Features;

namespace UncomplicatedCustomRoles.API.Interfaces
{
    internal interface ICustomModule
    {
        public abstract SummonedCustomRole Instance { get; }

        public abstract void Execute();
    }
}