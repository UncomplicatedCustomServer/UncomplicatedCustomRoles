/*
 * This file is a part of the UncomplicatedCustomRoles project.
 * 
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 * 
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Text;

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
            Log.Info($"STARTED CM CFNICKNAME - COLOR: {Color}");
            if (Color == string.Empty)
                return;

            string nick = CustomRole.Player.DisplayNickname.Replace("<color=#855439>*</color>", "");
            string color = Color.StartsWith("#") ? Color : $"#{Color}";
            CustomRole.Player.CustomInfo = CustomRole.Player.CustomInfo.Replace(nick, $"<color={color}>{nick}</color>");
        }
    }
}
