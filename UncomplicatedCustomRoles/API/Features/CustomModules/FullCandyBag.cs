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
using InventorySystem.Items;
using InventorySystem.Items.Usables.Scp330;

namespace UncomplicatedCustomRoles.API.Features.CustomModules;

public class FullCandyBag : CustomModule
{
    public override List<string> RequiredArgs => ["candies"];

    internal List<CandyKindID> Kinds => TryGetCastedListValue<CandyKindID>("candies");

    public override bool Validate(out string error)
    {
        List<string> invalid = GetInvalidEnumEntries<CandyKindID>("candies");
        if (invalid.Count > 0)
        {
            error = $"'candies' contains invalid candy value(s): {string.Join(", ", invalid)}. Valid values: {string.Join(", ", Enum.GetNames(typeof(CandyKindID)))}.";
            return false;
        }

        if (Kinds.Count == 0)
        {
            error = $"'candies' must list at least one valid candy. Valid values: {string.Join(", ", Enum.GetNames(typeof(CandyKindID)))}.";
            return false;
        }

        error = null;
        return true;
    }

    public override void OnAdded()
    {
        foreach (CandyKindID kind in Kinds)
            CustomRole.Player.GiveCandy(kind, ItemAddReason.AdminCommand);
    }
}