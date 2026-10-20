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
using PlayerRoles;
using UncomplicatedCustomRoles.API.Interfaces;
using UncomplicatedCustomRoles.Extensions;

namespace UncomplicatedCustomRoles.API.Features.CustomModules;

public class Scp1509Resurrection : CustomModule
{
    private static readonly HashSet<RoleTypeId> InvalidRoles = [RoleTypeId.None, RoleTypeId.Spectator, RoleTypeId.Overwatch, RoleTypeId.Filmmaker, RoleTypeId.Destroyed, RoleTypeId.CustomRole];

    public override List<string> RequiredArgs => ["roles"];
    
    public bool GiveStartingItems => TryGetCastedValue("give_starting_items", false);
    
    public List<KeyValuePair<bool, object>> Roles => GetRawListEntries("roles").Select(raw => TryParseEntry(raw, out KeyValuePair<bool, object> entry) ? entry : (KeyValuePair<bool, object>?)null).Where(entry => entry is not null).Select(entry => entry.Value).ToList();
    
    public bool TryPickRole(out RoleTypeId role, out ICustomRole customRole)
    {
        role = RoleTypeId.None;
        customRole = null;

        List<KeyValuePair<bool, object>> candidates = Roles.Where(entry => !entry.Key || API.Features.CustomRole.CustomRoles.ContainsKey((int)entry.Value)).ToList();

        if (candidates.Count is 0)
            return false;

        KeyValuePair<bool, object> picked = candidates.RandomValue();

        if (picked.Key)
        {
            customRole = API.Features.CustomRole.CustomRoles[(int)picked.Value];
            role = customRole.Role;
        }
        else
        {
            role = (RoleTypeId)picked.Value;
        }

        return true;
    }

    public override bool Validate(out string error)
    {
        List<string> rawEntries = GetRawListEntries("roles");

        if (rawEntries.Count is 0)
        {
            error = "'roles' must list at least one role (e.g. 'IR ClassD', 'CR 5' or just 'Scientist').";
            return false;
        }

        foreach (string raw in rawEntries)
        {
            if (!TryParseEntry(raw, out KeyValuePair<bool, object> entry))
            {
                error = $"'roles' contains an invalid entry '{raw}'. Use 'InternalRole <RoleTypeId>' (IR), 'CustomRole <id>' (CR) or just a RoleTypeId, e.g. 'IR ClassD', 'CR 5', 'Scientist'.";
                return false;
            }

            if (!entry.Key)
                continue;

            if (!API.Features.CustomRole.CustomRoles.TryGetValue((int)entry.Value, out ICustomRole customRole))
            {
                error = $"'roles' contains the CustomRole {entry.Value}, but no CustomRole with that id is registered.";
                return false;
            }

            if (InvalidRoles.Contains(customRole.Role))
            {
                error = $"'roles' contains the CustomRole {entry.Value}, but its base role can't be used for a resurrection.";
                return false;
            }
        }

        if (Args.TryGetValue("give_starting_items", out object rawGive) && rawGive is not null && !bool.TryParse(rawGive.ToString(), out _))
        {
            error = $"'give_starting_items' must be true or false, got '{rawGive}'.";
            return false;
        }

        error = null;
        return true;
    }

    private static bool TryParseEntry(string raw, out KeyValuePair<bool, object> entry)
    {
        entry = default;

        if (string.IsNullOrWhiteSpace(raw))
            return false;

        string[] elements = raw.Split([' '], StringSplitOptions.RemoveEmptyEntries);

        switch (elements.Length)
        {
            case 1 when TryParseRole(elements[0], out RoleTypeId plainRole):
                entry = new KeyValuePair<bool, object>(false, plainRole);
                return true;
            case 2 when elements[0].Equals("CustomRole", StringComparison.OrdinalIgnoreCase) || elements[0].Equals("CR", StringComparison.OrdinalIgnoreCase):
                if (!int.TryParse(elements[1], out int id))
                    return false;

                entry = new KeyValuePair<bool, object>(true, id);
                return true;
            case 2 when elements[0].Equals("InternalRole", StringComparison.OrdinalIgnoreCase) || elements[0].Equals("IR", StringComparison.OrdinalIgnoreCase):
                if (!TryParseRole(elements[1], out RoleTypeId role))
                    return false;

                entry = new KeyValuePair<bool, object>(false, role);
                return true;
            default:
                return false;
        }
    }

    private static bool TryParseRole(string raw, out RoleTypeId role)
    {
        return Enum.TryParse(raw, true, out role) && Enum.IsDefined(typeof(RoleTypeId), role) && !InvalidRoles.Contains(role);
    }
}