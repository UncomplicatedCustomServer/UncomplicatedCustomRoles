using CommandSystem;
using System;
using System.Collections.Generic;
using Exiled.Permissions.Extensions;
using System.Linq;
using UncomplicatedCustomRoles.API.Interfaces;
using UncomplicatedCustomRoles.Manager;

namespace UncomplicatedCustomRoles.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class CommandParent : ParentCommand
    {
        public CommandParent() => LoadGeneratedCommands();

        public override string Command { get; } = "ucr";

        public override string[] Aliases { get; } = new string[] { };

        public override string Description { get; } = "Manage the UCR features";

        public override void LoadGeneratedCommands() 
        {
            RegisteredCommands.Add(new List());
            RegisteredCommands.Add(new Owner());
            RegisteredCommands.Add(new Role());
            RegisteredCommands.Add(new Spawn());
            RegisteredCommands.Add(new Reload());
            RegisteredCommands.Add(new SpawnPoint());
            RegisteredCommands.Add(new Generate());
            RegisteredCommands.Add(new Show());
            RegisteredCommands.Add(new Version());
            RegisteredCommands.Add(new Debug());
        }

        public List<IUCRCommand> RegisteredCommands { get; } = new();

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count() == 0)
            {
                // Help page
                response = $"\n>> UncomplicatedCustomRoles v{Plugin.Instance.Version}{(VersionManager.VersionInfo?.CustomName is not null ? $" '{VersionManager.VersionInfo.CustomName}'" : string.Empty)} <<\nby {Plugin.Instance.Author}\n\nAvailable commands:";

                foreach (IUCRCommand Command in RegisteredCommands)
                {
                    response += $"\n- ucr {Command.Name}  ->  {Command.Description}  [{Command.RequiredPermission}]";
                }

                return true;
            } 
            else
            {
                // Arguments compactor:
                List<string> Arguments = new();
                foreach (string Argument in arguments.Where(arg => arg != arguments.At(0)))
                {
                    Arguments.Add(Argument);
                }

                IUCRCommand Command = RegisteredCommands.Where(command => command.Name == arguments.At(0)).FirstOrDefault();

                if (Command is not null)
                    if (sender.CheckPermission(Command.RequiredPermission))
                        return Command.Executor(Arguments, sender, out response);
                    else
                    {
                        response = $"You don't have enough permission(s) to execute that command!\nNeeded: {Command.RequiredPermission}";
                        return false;
                    }
                else
                {
                    response = "Command not found!";
                    return false;
                }
            }
        }
    }
}
