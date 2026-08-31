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
using LabApi.Features.Wrappers;
using MEC;

namespace UncomplicatedCustomRoles.API.Features.CustomModules;

public class DropItemOnDeath : CustomModule
{
    public override List<string> RequiredArgs => ["item"];

    public ItemType? Item =>
        StringArgs.TryGetValue("item", out string rawItem) && Enum.TryParse(rawItem, true, out ItemType item) && item is not ItemType.None ? item : null;

    public override bool Validate(out string error)
    {
        if (Item is null)
        {
            error = $"'item' value '{TryGetStringValue("item")}' is not a valid ItemType. Examples: Medkit, KeycardScientist, GunCOM15, Coin.";
            return false;
        }

        error = null;
        return true;
    }

    public override void OnRemoved()
    {
        if (Item is ItemType item)
            Timing.CallDelayed(0.5f, () =>
            {
                Pickup pickup = Pickup.Create(item, CustomRole.Player.Position);
                pickup?.Spawn();
            });
    }
}