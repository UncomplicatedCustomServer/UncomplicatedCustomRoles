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

namespace UncomplicatedCustomRoles.API.Features.CustomModules;

public class ItemBan : CustomModule
{
    public override List<string> RequiredArgs => ["item_type"];

    public List<ItemType> Items => TryGetCastedListValue<ItemType>("item_type");

    public override bool Validate(out string error)
    {
        var invalid = GetInvalidEnumEntries<ItemType>("item_type");
        if (invalid.Count > 0)
        {
            error =
                $"'item_type' contains invalid ItemType value(s): {string.Join(", ", invalid)}. Examples: GunAK, Medkit, KeycardO5.";
            return false;
        }

        if (Items.Count == 0)
        {
            error = "'item_type' must list at least one valid ItemType (e.g. GunAK, Medkit, KeycardO5).";
            return false;
        }

        error = null;
        return true;
    }
}