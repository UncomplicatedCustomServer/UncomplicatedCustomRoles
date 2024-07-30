using CommandSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomRoles.Interfaces;

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
        }

        public List<IUCRCommand> RegisteredCommands { get; } = new();

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count() == 0)
            {
                // Help page
                response = $"\n>> UncomplicatedCustomRoles v{Plugin.Version} <<\nby FoxWorn3365 & Dr.Agenda\n\nAvailable commands:";

                foreach (IUCRCommand Command in RegisteredCommands)
                {
                    response += $"\n- ucr {Command.Name}  ->  {Command.Description}";
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

                if (Command is not null && sender.CheckPermission(Command.RequiredPermission))
                {
                    // Let's call the command
                    return Command.Executor(Arguments, sender, out response);
                }
                else
                {
                    response = "Command not found";
                    return false;
                }
            }
        }
    }
}
