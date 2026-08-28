/*
 * This file is a part of the UncomplicatedCustomRoles project.
 *
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 *
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;
using System.Net;
using CommandSystem;
using LabApi.Features.Wrappers;
using UncomplicatedCustomRoles.API.Interfaces;
using UncomplicatedCustomRoles.Extensions;
using UncomplicatedCustomRoles.Manager.NET;

namespace UncomplicatedCustomRoles.Commands;

public class Owner : IUCRCommand
{
    public string Name { get; } = "owner";

    public string Description { get; } = "Get the 'Server Owner' role on our Discord server";

    public string RequiredPermission { get; } = "ucr.owner";

    public bool Executor(List<string> arguments, ICommandSender sender, out string response)
    {
        if (arguments.Count != 1)
        {
            response = "Usage: ucr owner <Discord ID>";
            return false;
        }

        if (!Player.TryGet(sender, out var player))
        {
            response = "This command can only be executed by a player.";
            return false;
        }

        HttpManager.AddServerOwner(player, arguments[0], answer => Answer(sender, answer));

        response = "Asking our central server to give you the 'Server Owner' role...";
        return true;
    }

    private static void Answer(ICommandSender sender, HttpResponse answer)
    {
        if (!answer.Completed)
        {
            sender.Respond($"Failed to reach the UCS Central Server: {answer.Reason}", false);
            return;
        }

        var code = answer.Body.GetStatusCode(out var message);

        sender.Respond($"{code} - {message}", code is HttpStatusCode.OK);
    }
}