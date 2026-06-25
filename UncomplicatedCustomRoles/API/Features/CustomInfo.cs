/*
 * This file is a part of the UncomplicatedCustomRoles project.
 * 
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 * 
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using LabApi.Features.Wrappers;
using UncomplicatedCustomRoles.API.Features.CustomModules;
using UncomplicatedCustomRoles.API.Interfaces;
using UncomplicatedCustomRoles.Extensions;
using UncomplicatedCustomRoles.Manager;

namespace UncomplicatedCustomRoles.API.Features
{
    public class CustomInfo
    {
        public string Nickname
        {
            get;
            set
            {
                field = value;
                if (_lastOwner is not null)
                    UpdateInfo(_lastOwner);
            }
        }

        public string Role
        {
            get;
            set
            {
                field = value;
                if (_lastOwner is not null)
                    UpdateInfo(_lastOwner);
            }
        }

        public string Info
        {
            get;
            set
            {
                field = value;
                if (_lastOwner is not null)
                    UpdateInfo(_lastOwner);
            }
        }

        private Player _lastOwner;
        
        internal static bool SuppressExternalSync { get; set; }

        public CustomInfo(string nickname, string role, string info)
        {
            Nickname = nickname;
            Role = role;
            Info = info;
        }

        public CustomInfo(Player player, string info)
        {
            Nickname = player.Nickname;
            Role = player.Role.GetFullName();
            Info = info;

            UpdateInfo(player);
        }

        public CustomInfo(Player player, ICustomRole role)
        {
            Nickname = player.Nickname;
            Role = role.OverrideRoleName ? role.Name : role.Role.GetFullName();
            Info = role.CustomInfo;

            UpdateInfo(player);
        }

        public void UpdateInfo(Player player)
        {
            _lastOwner = player;

            bool previousSuppress = SuppressExternalSync;
            SuppressExternalSync = true;
            try
            {
                player.InfoArea |= PlayerInfoArea.CustomInfo;
                player.InfoArea &= ~PlayerInfoArea.Role;
                player.InfoArea &= ~PlayerInfoArea.Nickname;
                player.InfoArea &= ~PlayerInfoArea.UnitName;

                string rawCustomInfo = "<color=#FFFFFF></color>%custominfo%%nickname%%rolename%";
                string rawNickname = Nickname;
                string rawInfo = Info;
                string rawRole = Role;

                if (!NicknameSync.ValidateCustomInfo(Info, out string customInfoError) && !string.IsNullOrEmpty(Info))
                {
                    LogManager.Error($"CustomInfo is not correct, therefore the custom info part of player {player.PlayerId} won't be shown.\nCustomInfo: {Info}\nError: {customInfoError}");
                    rawCustomInfo = rawCustomInfo.Replace("%custominfo%", "");
                    rawInfo = string.Empty;
                }

                if (!NicknameSync.ValidateCustomInfo(Role, out string roleNameError) && !string.IsNullOrEmpty(Role))
                {
                    LogManager.Error($"RoleName is not correct, therefore the role name part of player {player.PlayerId} won't be shown.\nRoleName: {Role}\nError: {roleNameError}");
                    rawCustomInfo = rawCustomInfo.Replace("%rolename%", "");
                    rawRole = string.Empty;
                }

                if (player.TryGetSummonedInstance(out SummonedCustomRole summonedCustomRole))
                {
                    rawInfo = PlaceholderManager.ApplyPlaceholders(rawInfo, player, summonedCustomRole.Role);

                    if (summonedCustomRole.TryGetModule(out CustomInfoOrder customInfoOrderModule))
                        rawCustomInfo = $"<color=#FFFFFF></color>{customInfoOrderModule.Order}";

                    if (summonedCustomRole.TryGetModule(out ColorfulNickname colorfulNickname))
                    {
                        LogManager.Debug($"Applying ColorfulNickname module to player {player.PlayerId} with color {colorfulNickname.Color} and nickname {Nickname}");

                        if (string.IsNullOrEmpty(colorfulNickname.Color))
                        {
                            LogManager.Warn($"The ColorfulNickname module of player {player.PlayerId} has no color set, skipping the colouring.");
                        }
                        else
                        {
                            string nick = Nickname?.Replace("<color=#855439>*</color>", "") ?? string.Empty;
                            string color = colorfulNickname.Color.StartsWith("#") ? colorfulNickname.Color : $"#{colorfulNickname.Color}";
                            if (!Misc.AcceptedColours.Contains(color.Replace("#", "")))
                                LogManager.Warn($"The color {color} is not acceptable by the game in ColorfulNicknames! Please use a valid hex color code.");
                            else
                                rawNickname = $"<color={color}>{nick}</color>";
                        }
                    }
                }
                else
                {
                    rawInfo = PlaceholderManager.ApplyPlaceholders(rawInfo, player, null);
                }

                if (string.IsNullOrEmpty(rawInfo))
                    rawCustomInfo = rawCustomInfo.Replace("%custominfo%", "");

                if (string.IsNullOrEmpty(rawNickname))
                    rawNickname = player.Nickname;

                player.CustomInfo = rawCustomInfo.Replace("%%", "%\n%").BulkReplace(new()
                {
                    {
                        "custominfo",
                        rawInfo
                    },
                    {
                        "nickname",
                        rawNickname
                    },
                    {
                        "rolename",
                        rawRole
                    },
                }, "%<val>%");
            }
            finally
            {
                SuppressExternalSync = previousSuppress;
            }
        }
    }
}
