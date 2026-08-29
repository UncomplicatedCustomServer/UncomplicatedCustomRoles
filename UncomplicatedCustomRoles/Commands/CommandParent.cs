/*
 * This file is a part of the UncomplicatedCustomRoles project.
 *
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 *
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using LabApi.Features.Permissions;
using UncomplicatedCustomRoles.API.Interfaces;
using UncomplicatedCustomRoles.Extensions;
using UncomplicatedCustomRoles.Manager;

namespace UncomplicatedCustomRoles.Commands;

[CommandHandler(typeof(RemoteAdminCommandHandler))]
internal class CommandParent : ParentCommand
{
    public CommandParent()
    {
        LoadGeneratedCommands();
    }

    public override string Command { get; } = "ucr";

    public override string[] Aliases { get; } = [];

    public override string Description { get; } = "Manage the UCR features";

    public List<IUCRCommand> RegisteredCommands { get; } = [];

    public override void LoadGeneratedCommands()
    {
        RegisteredCommands.Add(new List());
        RegisteredCommands.Add(new Info());
        RegisteredCommands.Add(new Role());
        RegisteredCommands.Add(new Spawn());
        RegisteredCommands.Add(new CustomInfo());
        RegisteredCommands.Add(new Reload());
        RegisteredCommands.Add(new SpawnPoint());
        RegisteredCommands.Add(new Percentages());
        RegisteredCommands.Add(new Errors());
        RegisteredCommands.Add(new Generate());
        RegisteredCommands.Add(new Update());
        RegisteredCommands.Add(new Owner());
        RegisteredCommands.Add(new Version());
        RegisteredCommands.Add(new Debug());
    }

    protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!arguments.Any())
        {
            // Help page
            response = $"\n>> UncomplicatedCustomRoles v{Plugin.Instance.Version}{(VersionManager.VersionInfo?.CustomName is not null ? $" '{VersionManager.VersionInfo.CustomName}'" : string.Empty)} <<\nby {Plugin.Instance.Author}\n\nAvailable commands:";

            foreach (IUCRCommand Command in RegisteredCommands)
                response += $"\n• <b>ucr {Command.Name.GenerateWithBuffer(12)}</b> → {Command.Description}";

            response += "\n<size=1>OwO</size>";

            return true;
        }

        {
            List<string> Arguments = arguments.Skip(1).ToList();

            IUCRCommand Command = RegisteredCommands.FirstOrDefault(command => command.Name == arguments.At(0));

            if (Command is not null)
            {
                if (sender.HasPermissions(Command.RequiredPermission)) return Command.Executor(Arguments, sender, out response);

                response = $"You don't have enough permission(s) to execute that command!\nNeeded: {Command.RequiredPermission}";
                return false;
            }

            response = "Command not found!";
            return false;
        }
    }
}