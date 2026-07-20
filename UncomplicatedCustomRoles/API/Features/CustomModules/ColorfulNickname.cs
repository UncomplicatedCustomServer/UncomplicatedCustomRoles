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
using MEC;

namespace UncomplicatedCustomRoles.API.Features.CustomModules;

[Obsolete("This module is deprecated and will be removed in a future version. Use InfoTag instead.")]
public class ColorfulNickname : CustomModule
{
    public override List<string> RequiredArgs => ["color"];

    internal string Color
    {
        get
        {
            var raw = TryGetStringValue("color", string.Empty).TrimStart('#');
            return Misc.AcceptedColours.FirstOrDefault(c =>
                string.Equals(c, raw, StringComparison.OrdinalIgnoreCase)) ?? raw;
        }
    }

    public override bool Validate(out string error)
    {
        var raw = TryGetStringValue("color", string.Empty).TrimStart('#');
        if (!Misc.AcceptedColours.Any(c => string.Equals(c, raw, StringComparison.OrdinalIgnoreCase)))
        {
            error =
                $"'color' '{raw}' is not a color the game allows for nicknames. Allowed colors: {string.Join(", ", Misc.AcceptedColours)}.";
            return false;
        }

        error = null;
        return true;
    }

    public override void OnAdded()
    {
        Timing.CallDelayed(Timing.WaitForOneFrame, () => { CustomRole.CustomInfo.UpdateInfo(CustomRole.Player); });
        base.OnAdded();
    }
}