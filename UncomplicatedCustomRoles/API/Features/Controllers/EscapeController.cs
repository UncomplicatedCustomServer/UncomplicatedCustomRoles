/*
 * This file is a part of the UncomplicatedCustomRoles project.
 *
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 *
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using LabApi.Events.Arguments.PlayerEvents;
using PlayerRoles;
using UncomplicatedCustomRoles.Events;
using UnityEngine;

namespace UncomplicatedCustomRoles.API.Features.Controllers;

internal class EscapeController : MonoBehaviour
{
    private SummonedCustomRole _role;

    private bool _wasInEscapeZone;

    private void Update()
    {
        if (_role is null || PlayerEventHandler.Instance is null)
            return;

        bool inZone = false;
        foreach (Bounds escapeZone in global::Escape.EscapeZones)
            if (escapeZone.Contains(_role.Player.Position))
            {
                inZone = true;

                if (!_wasInEscapeZone)
                    PlayerEventHandler.Instance.OnEscaping(new PlayerEscapingEventArgs(_role.Player.ReferenceHub, _role.Player.Role, RoleTypeId.ChaosConscript, global::Escape.EscapeScenarioType.Custom, escapeZone));

                break;
            }

        _wasInEscapeZone = inZone;
    }

    private void OnDestroy()
    {
        _role = null;
    }

    public void Init(SummonedCustomRole role)
    {
        _role = role;
    }
}