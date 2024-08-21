using MEC;

namespace UncomplicatedCustomRoles.API.Interfaces
{
    internal interface ICoroutineRole : ICustomRole
    {
        public abstract CoroutineHandle CoroutineHandler { get; internal set; }

        public abstract float TickRate { get; set; }

        public abstract long Frame { get; internal set; }

        public abstract void Tick();
    }
}
