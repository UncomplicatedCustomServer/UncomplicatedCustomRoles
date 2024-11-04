using UncomplicatedCustomRoles.API.Enums;
using UncomplicatedCustomRoles.API.Features;

namespace UncomplicatedCustomRoles.API.Interfaces
{
    public interface ICustomModule
    {
        public abstract SummonedCustomRole Instance { get; }

        public abstract string Name { get; }

        public abstract void Execute();
    }
}