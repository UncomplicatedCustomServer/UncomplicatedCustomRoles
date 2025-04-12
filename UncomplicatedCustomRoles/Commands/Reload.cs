/*
 * This file is a part of the UncomplicatedCustomRoles project.
 * 
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 * 
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using CommandSystem;
using Exiled.API.Features;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Interfaces;
using UncomplicatedCustomRoles.Extensions;
using UncomplicatedCustomRoles.Manager;

namespace UncomplicatedCustomRoles.Commands
{
#pragma warning disable CS0618 // Obsolete
    public class Reload : IUCRCommand
    {
        public string Name { get; } = "reload";

        public string Description { get; } = "Reload every custom role loaded and search for new";

        public string RequiredPermission { get; } = "ucr.reload";

        public bool Executor(List<string> arguments, ICommandSender sender, out string response)
        {
            if (!Round.IsStarted)
            {
                response = "Sorry but you can't use this command if the round is not started!";
                return false;
            }

            Dictionary<int, ICustomRole> oldRoles = CustomRole.CustomRoles.Clone();

            CustomRole.CustomRoles = new();
            CustomRole.NotLoadedRoles.Clear();
            CustomRole.OutdatedRoles.Clear();

            FileConfigs.LoadAll();
            FileConfigs.LoadAll(Server.Port.ToString());

            IEnumerable<int> removedRoles = oldRoles.Keys.Except(CustomRole.CustomRoles.Keys);

            foreach (int role in removedRoles)
                SummonedCustomRole.RemoveSpecificRole(role);

            response = $"\nSuccessfully reloaded UncomplicatedCustomRoles\n<color=#5db30c>➕</color> Added <b>{CustomRole.CustomRoles.Count - (oldRoles.Count + removedRoles.Count())}</b> Custom Roles\n<color=#c23636>➖</color> Removed <b>{removedRoles.Count()}</b> Custom Roles\n\"<color=#00ffff>🔢</color> Loaded a total of <b>{CustomRole.CustomRoles.Count}</b> Custom Roles\n<color=#ffff00>⚠️</color> If you have changed some stats of the Custom Roles such as health and inventory the changes won't took place on already spawned players with these custom roles!";
            return true;
        }
    }
}