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
using LabApi.Events.CustomHandlers;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Features.CustomModules;

namespace UncomplicatedCustomRoles.Events
{
    internal class LabApiEventHandler : CustomEventsHandler
    {
        public override void OnPlayerRequestedRaPlayerInfo(PlayerRequestedRaPlayerInfoEventArgs ev)
        {
            SummonedCustomRole.TryParseRemoteAdmin(ev.Target.ReferenceHub, ev.InfoBuilder);
        }

        public override void OnPlayerRaPlayerListAddingPlayer(PlayerRaPlayerListAddingPlayerEventArgs ev)
        {
            if (SummonedCustomRole.TryGet(ev.Target.ReferenceHub, out SummonedCustomRole customRole))
                if (customRole.TryGetModule(out ColorfulRaName colorfulRaName))
                    ev.Body = ev.Body.Replace("{RA_ClassColor}", $"#{colorfulRaName.Color.TrimStart('#')}");
        }
    }
}
