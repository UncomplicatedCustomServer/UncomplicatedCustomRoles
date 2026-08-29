/*
 * This file is a part of the UncomplicatedCustomRoles project.
 *
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 *
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

namespace UncomplicatedCustomRoles.API.Features.CustomModules;

internal class KeepInventoryOnEscape : CustomModule
{
    public bool DropItems => TryGetCastedValue("drop", true);

    public override bool Validate(out string error)
    {
        if (Args.TryGetValue("drop", out object raw) && raw is not null && !bool.TryParse(raw.ToString(), out _))
        {
            error = $"'drop' must be true or false, got '{raw}'.";
            return false;
        }

        error = null;
        return true;
    }
}