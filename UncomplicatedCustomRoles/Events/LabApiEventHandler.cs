using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using UncomplicatedCustomRoles.API.Features;

namespace UncomplicatedCustomRoles.Events
{
    internal class LabApiEventHandler : CustomEventsHandler
    {
        public override void OnPlayerRequestedRaPlayerInfo(PlayerRequestedRaPlayerInfoEventArgs ev)
        {
            SummonedCustomRole.TryParseRemoteAdmin(ev.Target.ReferenceHub, ev.InfoBuilder);
        }
    }
}
