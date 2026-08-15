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
using LabApi.Features.Wrappers;
using PlayerRoles;
using Respawning.NamingRules;
using UncomplicatedCustomRoles.API.Features.CustomModules;
using UncomplicatedCustomRoles.API.Interfaces;
using UncomplicatedCustomRoles.Extensions;
using UncomplicatedCustomRoles.Manager;

namespace UncomplicatedCustomRoles.API.Features;

public class CustomInfo
{
    private Player _lastOwner;

    private bool _detached;

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

    internal static bool SuppressExternalSync { get; set; }

    internal void Detach()
    {
        _detached = true;
        _lastOwner = null;
    }

    public void UpdateInfo(Player player)
    {
        if (_detached)
            return;

        _lastOwner = player;

        var previousSuppress = SuppressExternalSync;
        SuppressExternalSync = true;
        try
        {
            player.InfoArea |= PlayerInfoArea.CustomInfo;
            player.InfoArea &= ~PlayerInfoArea.Role;
            player.InfoArea &= ~PlayerInfoArea.Nickname;
            player.InfoArea &= ~PlayerInfoArea.UnitName;

            var rawCustomInfo = "<color=#FFFFFF></color>%custominfo%%nickname%%rolename%";
            var rawNickname = Nickname;
            var rawInfo = Info;
            var rawRole = Role;

            if (!NicknameSync.ValidateCustomInfo(Info.SanitizeCustomInfo(), out var customInfoError) &&
                !string.IsNullOrEmpty(Info))
            {
                LogManager.Error(
                    $"CustomInfo is not correct, therefore the custom info part of player {player.PlayerId} won't be shown.\nCustomInfo: {Info}\nError: {customInfoError}");
                rawCustomInfo = rawCustomInfo.Replace("%custominfo%", "");
                rawInfo = string.Empty;
            }

            if (!NicknameSync.ValidateCustomInfo(Role.SanitizeCustomInfo(), out var roleNameError) &&
                !string.IsNullOrEmpty(Role))
            {
                LogManager.Error(
                    $"RoleName is not correct, therefore the role name part of player {player.PlayerId} won't be shown.\nRoleName: {Role}\nError: {roleNameError}");
                rawCustomInfo = rawCustomInfo.Replace("%rolename%", "");
                rawRole = string.Empty;
            }

            if (player.TryGetSummonedInstance(out var summonedCustomRole))
            {
                rawInfo = PlaceholderManager.ApplyPlaceholders(rawInfo, player, summonedCustomRole.Role);

                var infoTeam = summonedCustomRole.Role.Role.GetTeam();
                if (DisguiseTeam.List.TryGetValue(player.PlayerId, out var infoFakeTeam))
                    infoTeam = infoFakeTeam;

                var rawUnit = string.Empty;
                var showUnit = false;
                if (!string.IsNullOrEmpty(rawRole) && !summonedCustomRole.HasModule<NoUnitName>()
                    && infoTeam is Team.FoundationForces
                    && NamingRulesManager.TryGetNamingRule(infoTeam, out var infoUnitRule)
                    && !string.IsNullOrEmpty(infoUnitRule.LastGeneratedName))
                {
                    showUnit = true;
                    rawUnit = infoUnitRule.LastGeneratedName;
                }

                if (summonedCustomRole.TryGetModule(out InfoTag infoTag))
                {
                    if (infoTag.ShowBadge)
                        player.InfoArea |= PlayerInfoArea.Badge;
                    else
                        player.InfoArea &= ~PlayerInfoArea.Badge;

                    if (infoTag.ShowPowerStatus)
                        player.InfoArea |= PlayerInfoArea.PowerStatus;
                    else
                        player.InfoArea &= ~PlayerInfoArea.PowerStatus;

                    ApplyCustomInfo(player, infoTag.Compose(player, rawInfo, rawNickname, rawRole, rawUnit, showUnit));
                    return;
                }

                if (summonedCustomRole.TryGetModule(out CustomInfoOrder customInfoOrderModule))
                    rawCustomInfo = $"<color=#FFFFFF></color>{customInfoOrderModule.Order}";

                if (summonedCustomRole.TryGetModule(out ColorfulNickname colorfulNickname))
                {
                    LogManager.Debug(
                        $"Applying ColorfulNickname module to player {player.PlayerId} with color {colorfulNickname.Color} and nickname {Nickname}");

                    if (string.IsNullOrEmpty(colorfulNickname.Color))
                    {
                        LogManager.Warn(
                            $"The ColorfulNickname module of player {player.PlayerId} has no color set, skipping the colouring.");
                    }
                    else
                    {
                        var nick = Nickname?.Replace("<color=#855439>*</color>", "") ?? string.Empty;
                        if (string.IsNullOrEmpty(nick))
                            nick = player.Nickname;
                        var color = colorfulNickname.Color.StartsWith("#")
                            ? colorfulNickname.Color
                            : $"#{colorfulNickname.Color}";
                        if (!Misc.AcceptedColours.Contains(color.Replace("#", "")))
                            LogManager.Warn(
                                $"The color {color} is not acceptable by the game in ColorfulNicknames! Please use a valid hex color code.");
                        else
                            rawNickname = $"<color={color}>{nick}</color>";
                    }
                }

                if (showUnit)
                    rawRole = $"{rawRole} ({rawUnit})";
            }
            else
            {
                rawInfo = PlaceholderManager.ApplyPlaceholders(rawInfo, player, null);
            }

            if (string.IsNullOrEmpty(rawInfo))
                rawCustomInfo = rawCustomInfo.Replace("%custominfo%", "");

            if (string.IsNullOrEmpty(rawNickname))
                rawNickname = player.Nickname;

            if (string.IsNullOrEmpty(rawInfo) && string.IsNullOrEmpty(rawRole) && string.IsNullOrEmpty(player.Nickname))
            {
                player.InfoArea |= PlayerInfoArea.Nickname | PlayerInfoArea.Role | PlayerInfoArea.UnitName;
                player.CustomInfo = string.Empty;
                return;
            }

            ApplyCustomInfo(player, rawCustomInfo.Replace("%%", "%\n%").BulkReplace(new Dictionary<string, object>
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
                }
            }, "%<val>%"));
        }
        finally
        {
            SuppressExternalSync = previousSuppress;
        }
    }
    
    private static void ApplyCustomInfo(Player player, string composed)
    {
        var cleaned = composed.SanitizeCustomInfo();

        if (cleaned != composed)
        {
            LogManager.Debug(
                $"Removed the characters the game does not accept in a name tag from the tag of player {player.PlayerId}.\nBefore: {composed}\nAfter: {cleaned}");
            composed = cleaned;
        }

        if (!string.IsNullOrEmpty(composed) && composed.Length > 400)
        {
            LogManager.Error(
                $"The name tag of player {player.PlayerId} is {composed.Length} characters long, but the game only accepts 400, so it won't be shown.\n" +
                $"Composed tag: {composed}\n" +
                "Shorten the 'custom_info' of the role, or the InfoTag layout building this tag.");
            composed = string.Empty;
        }

        if (!string.IsNullOrEmpty(composed) && !NicknameSync.ValidateCustomInfo(composed, out var error))
        {
            LogManager.Error(
                $"The name tag of player {player.PlayerId} would be rejected by the game and won't be shown: {error}\n" +
                $"Composed tag: {composed}\n" +
                "Likely causes: a colour that isn't on the allowed list written inside 'custom_info', or a rich text tag the game does not allow.");
            composed = string.Empty;
        }

        if (string.IsNullOrEmpty(composed))
        {
            player.InfoArea |= PlayerInfoArea.Nickname | PlayerInfoArea.Role | PlayerInfoArea.UnitName;
            player.CustomInfo = string.Empty;
        }
        else
        {
            player.CustomInfo = composed;
        }
    }
}