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
using UnityEngine;

namespace UncomplicatedCustomRoles.API.Features.CustomModules;

public class ColorfulRaName : CustomModule
{
    public override List<string> RequiredArgs => ["color"];

    internal string Color => TryGetStringValue("color", string.Empty);

    public override bool Validate(out string error)
    {
        var raw = TryGetStringValue("color", string.Empty);
        var hex = raw.StartsWith("#") ? raw : "#" + raw;

        if (!ColorUtility.TryParseHtmlString(hex, out _))
        {
            error = $"'color' '{raw}' is not a valid hex color. Use a hex value like FF0000 or #FF0000.";
            return false;
        }

        error = null;
        return true;
    }
}