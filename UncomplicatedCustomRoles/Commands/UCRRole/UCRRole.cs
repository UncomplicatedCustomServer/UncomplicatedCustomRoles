using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;
using UncomplicatedCustomRoles.Manager;
using UncomplicatedCustomRoles.Structures;

namespace UncomplicatedCustomRoles.Commands.UCRSpawn
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class UCRRole : ParentCommand
    {
        public UCRRole() => LoadGeneratedCommands();

        public override string Command { get; } = "ucrrole";

        public override string[] Aliases { get; } = new string[] { };

        public override string Description { get; } = "See if one or more player are a custom role";

        public override void LoadGeneratedCommands() { }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!((CommandSender)sender).CheckPermission("ucr.role"))
            {
                response = "You do not have permission to use this command!";
                return false;
            }

            if (arguments.Count > 1)
            {
                response = "Usage: ucrrole  -  ucrrole (Player ID or Name)";
                return false;
            }

            if (arguments.Count == 1)
            {
                Player Player = Player.Get(arguments.At(0));
                int? RoleId = SpawnManager.TryGetCustomRole(Player);
                if (RoleId is not null)
                {
                    ICustomRole Role = Plugin.CustomRoles[RoleId.Value];
                    response = $"Player {Player.Nickname} {Player.UserId} [{Player.Id}] is the custom role {Role.Name} [{Role.Id}]";
                    return true;
                }
                response = $"Player {Player.Nickname} {Player.UserId} [{Player.Id}] is not a custom role!";
                return true;
            }
            else
            {
                response = "Custom roles of every player:\n";
                foreach (Player Player in Player.List)
                {
                    int? RoleId = SpawnManager.TryGetCustomRole(Player);
                    if (RoleId is not null)
                    {
                        ICustomRole Role = Plugin.CustomRoles[RoleId.Value];
                        response += $"\n - Player {Player.Nickname} {Player.UserId} [{Player.Id}] is the custom role {Role.Name} [{Role.Id}]";
                    } else
                    {
                        response += $"\n - Player {Player.Nickname} {Player.UserId} [{Player.Id}] is not a custom role!";
                    }
                }
                return true;
            }
        }
    }
}