using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using MEC;
using System;
using Handler = UncomplicatedCustomRoles.Events.EventHandler;

namespace UncomplicatedCustomRoles.Commands.UCRSpawn
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class UCRSpawn : ParentCommand
    {
        public UCRSpawn() => LoadGeneratedCommands();

        public override string Command { get; } = "ucrspawn";

        public override string[] Aliases { get; } = new string[] { };

        public override string Description { get; } = "Summon an UCR custom role";

        public override void LoadGeneratedCommands() { }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!((CommandSender)sender).CheckPermission("ucr.spawn"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if (arguments.Count != 2)
            {
                response = "Usage: ucrspawn (Player Id / name) (Role Id)";
                return false;
            }

            Player Player = Player.Get(arguments.At(0));
            if (Player == null)
            {
                response = $"Player not found: {arguments.At(0)}";
                return false;
            }

            if (arguments.At(1) != null)
            {
                Log.Debug("Selected role Id as Int32: " + Int32.Parse(arguments.At(1)));
                if (!Plugin.CustomRoles.ContainsKey(Int32.Parse(arguments.At(1))))
                {
                    response = $"Role with the Id {arguments.At(1)} was not found!";
                    return false;
                } else
                {
                    int Role = Int32.Parse(arguments.At(1));
                    // Summon the player to the role
                    response = $"Player {Player.Nickname} was successfully spawned as {Role}!";
                    Timing.RunCoroutine(Handler.DoSpawnPlayer(Player, Role));
                    return true;
                }
            } else
            {
                response = $"You must define a role Id!";
                return false;
            }
        }
    }
}