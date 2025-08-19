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
using UncomplicatedCustomRoles.Manager;

namespace UncomplicatedCustomRoles.API.Features.CustomModules
{
    public class ColorfulNickname : CustomModule
    {
        public override List<string> RequiredArgs => new()
        {
            "color"
        };

        internal string Color => TryGetStringValue("color", string.Empty);

        public override void OnAdded()
        {
            if (Color == string.Empty)
                return;
            string nick = CustomRole.Player.DisplayName.Replace("<color=#855439>*</color>", "");
            string color = Color.StartsWith("#") ? Color : $"#{Color}";
            if (!Misc.AcceptedColours.Contains(color.Replace("#", "")))
            {
                LogManager.Warn($"The color {color} is not acceptable by the game in ColorfulNicknames! Please use a valid hex color code.");
                return;
            }
            CustomRole.Player.CustomInfo = CustomRole.Player.CustomInfo.Replace(nick, $"<color={color}>{nick}</color>");
        }
    }
}