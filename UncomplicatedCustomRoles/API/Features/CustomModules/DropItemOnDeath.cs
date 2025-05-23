/*
 * This file is a part of the UncomplicatedCustomRoles project.
 * 
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 * 
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using LabApi.Features.Wrappers;
using MEC;
using System;
using System.Collections.Generic;

namespace UncomplicatedCustomRoles.API.Features.CustomModules
{
    public class DropItemOnDeath : CustomModule
    {
        public override List<string> RequiredArgs => new()
        {
            "item"
        };

        public ItemType? Item => Args.TryGetValue("item", out string rawItem) && Enum.TryParse(rawItem, out ItemType item) && item is not ItemType.None ? item : null;

        public override void OnRemoved()
        {
            if (Item is ItemType item)
                Timing.CallDelayed(0.5f, () => Pickup.Create(item, CustomRole.Player.Position));
        }
    }
}
