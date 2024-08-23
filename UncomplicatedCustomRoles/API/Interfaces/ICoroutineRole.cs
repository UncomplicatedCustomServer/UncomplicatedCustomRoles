using MEC;
using UncomplicatedCustomRoles.API.Features;

namespace UncomplicatedCustomRoles.API.Interfaces
{
    public interface ICoroutineRole : ICustomRole
    {
        public abstract CoroutineHandle CoroutineHandler { get; set; }

        public abstract float TickRate { get; set; }

        public abstract long Frame { get; set; }

        public abstract void Tick(SummonedCustomRole roleInstance);
    }
}
