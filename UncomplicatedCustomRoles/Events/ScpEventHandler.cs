using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.Scp049Events;
using LabApi.Events.Arguments.Scp096Events;
using LabApi.Events.Arguments.Scp939Events;
using LabApi.Events.Handlers;
using PlayerRoles;
using UncomplicatedCustomRoles.API.Features.CustomModules;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Interfaces;
using UncomplicatedCustomRoles.Extensions;
using UncomplicatedCustomRoles.Manager;

namespace UncomplicatedCustomRoles.Events
{
    internal class ScpEventHandler : EventHandlerBase
    {
        internal override void OnRegistered()
        {
            // SCP-049
            Scp049Events.ResurrectingBody += OnResurrectingBody;

            // SCP-096
            Scp096Events.AddingTarget += OnAddingTarget;

            // SCP-330
            PlayerEvents.InteractingScp330 += OnInteractingScp330;

            // SCP-939
            Scp939Events.CreatingAmnesticCloud += OnScp939CreatingAmnesticCloud;
        }

        internal override void OnUnregistered()
        {
            // SCP-049
            Scp049Events.ResurrectingBody -= OnResurrectingBody;

            // SCP-096
            Scp096Events.AddingTarget -= OnAddingTarget;

            // SCP-330
            PlayerEvents.InteractingScp330 -= OnInteractingScp330;

            // SCP-939
            Scp939Events.CreatingAmnesticCloud -= OnScp939CreatingAmnesticCloud;
        }

        public void OnAddingTarget(Scp096AddingTargetEventArgs ev)
        {
            if (!ev.IsAllowed)
                return;

            if (ev.Target.TryGetSummonedInstance(out SummonedCustomRole summonedInstance))
            {
                if (ev.Target.ReferenceHub.GetTeam() is Team.SCPs)
                    ev.IsAllowed = false;

                if (summonedInstance.HasModule<DoNotTrigger096>())
                    ev.IsAllowed = false;

                if (summonedInstance.HasModule<PacifismUntilDamage>())
                    ev.IsAllowed = false;
            }
        }

        public void OnResurrectingBody(Scp049ResurrectingBodyEventArgs ev)
        {
            ICustomRole Role = SpawnManager.DoEvaluateSpawnForPlayer(ev.Target, RoleTypeId.Scp0492);
            LogManager.Silent($"{ev.Target} recalled by {ev.Player}, found {Role?.Id} {Role?.Name}");

            if (Role is not null)
            {
                ev.IsAllowed = false;
                ev.Target.SetCustomRole(Role);
            }
        }

        public void OnInteractingScp330(PlayerInteractingScp330EventArgs ev)
        {
            if (!ev.IsAllowed)
                return;

            if (SummonedCustomRole.TryGet(ev.Player, out SummonedCustomRole role))
            {
                role.Scp330Count++;

                LogManager.Debug($"Player {ev.Player} took {role.Scp330Count} candies!");
            }
        }

        public void OnScp939CreatingAmnesticCloud(Scp939CreatingAmnesticCloudEventArgs ev)
        {
            if (ev.Player.TryGetSummonedInstance(out SummonedCustomRole role) && role.HasModule<AmnesiaResistance>())
                ev.IsAllowed = false;
        }
    }
}