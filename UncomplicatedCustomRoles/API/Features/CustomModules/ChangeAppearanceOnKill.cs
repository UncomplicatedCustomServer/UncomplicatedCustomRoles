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
using PlayerRoles;

namespace UncomplicatedCustomRoles.API.Features.CustomModules;

internal class ChangeAppearanceOnKill : CustomModule
{
    public override List<string> RequiredArgs => ["new_appearance"];

    public RoleTypeId NewAppearance => Enum.TryParse(TryGetStringValue("new_appearance", "None"), true, out RoleTypeId role) ? role : RoleTypeId.None;

    public uint Duration => TryGetCastedValue<uint>("duration");

    public bool Forever => TryGetCastedValue("forever", false);

    public bool AlreadyChanged { get; internal set; } = false;

    public override bool Validate(out string error)
    {
        string raw = TryGetStringValue("new_appearance");
        if (NewAppearance is RoleTypeId.None)
        {
            error = $"'new_appearance' value '{raw}' is not a valid role. Examples: Scientist, ClassD, NtfSergeant, Scp0492.";
            return false;
        }

        if (Args.TryGetValue("duration", out object rawDuration) && rawDuration is not null && !uint.TryParse(rawDuration.ToString(), out _))
        {
            error = $"'duration' must be a whole number of seconds (0 or greater), got '{rawDuration}'.";
            return false;
        }

        if (Args.TryGetValue("forever", out object rawForever) && rawForever is not null && !bool.TryParse(rawForever.ToString(), out _))
        {
            error = $"'forever' must be true or false, got '{rawForever}'.";
            return false;
        }

        error = null;
        return true;
    }
}