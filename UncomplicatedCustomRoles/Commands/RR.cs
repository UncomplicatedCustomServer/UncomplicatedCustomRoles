using CommandSystem;
using Footprinting;
using PlayerRoles;
using System;
using LabApi.Features.Wrappers;

namespace UncomplicatedCustomRoles.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class RR : ParentCommand
    {
        public RR() => LoadGeneratedCommands();

        public override string Command { get; } = "rra";

        public override string[] Aliases { get; } = new string[] { };

        public override string Description { get; } = "RR";

        public override void LoadGeneratedCommands() { }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!Player.TryGet(sender, out Player player))
            {
                response = "This command can only be executed by a player!";
                return false;
            }

            response = $"Role: {player.Role}\nTeam: {player.RoleBase.Team}\nTeam 2: {player.ReferenceHub.GetTeam()}\nTeam 3: {new Footprint(player.ReferenceHub).Role.GetTeam()}\nTeam type: {player.ReferenceHub.roleManager.CurrentRole.GetType().FullName}";
            return true;
        }
    }
}