using MEC;
using System.Collections.Generic;

namespace UncomplicatedCustomRoles.API.Features.CustomModules
{
    internal abstract class CoroutineModule : CustomModule
    {
        public CoroutineHandle CoroutineHandler { get; internal set; }

        public virtual float TickRate { get; } = 0.5f;

        public virtual long Frame { get; internal set; } = -1;

        public bool IsAllowed { get; set; } = true;

        public virtual void Tick() { }

        private IEnumerator<float> TickHandler()
        {
            while (IsAllowed && TickRate > 0)
            {
                Frame++;
                Tick();

                yield return Timing.WaitForSeconds(TickRate);
            }
        }

        public override void Execute()
        {
            CoroutineHandler = Timing.RunCoroutine(TickHandler());
        }
    }
}
