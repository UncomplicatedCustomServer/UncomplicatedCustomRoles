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
    private const string ColorPrefix = "<color=#FFFFFF></color>";
    private bool _detached;
    private Player _lastOwner;
    private bool _nativeNickname = true;
    private bool _nativeRole = true;
    private bool _nativeUnit = true;

    public string Nickname
    {
        get;
        set
        {
            field = value ?? string.Empty;
            ;
            if (_lastOwner is not null)
                UpdateInfo(_lastOwner);
        }
    }

    public string Role
    {
        get;
        set
        {
            field = value ?? string.Empty;
            ;
            if (_lastOwner is not null)
                UpdateInfo(_lastOwner);
        }
    }

    public string Info
    {
        get;
        set
        {
            field = value ?? string.Empty;
            ;
            if (_lastOwner is not null)
                UpdateInfo(_lastOwner);
        }
    }

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

    internal void Detach()
    {
        _detached = true;
        _lastOwner = null;
    }

    internal PlayerInfoArea ApplyAreas(PlayerInfoArea value, PlayerInfoArea? original = null)
    {
        PlayerInfoArea restore = original ?? value;

        value |= PlayerInfoArea.CustomInfo;

        value = _nativeNickname ? value | (restore & PlayerInfoArea.Nickname) : value & ~PlayerInfoArea.Nickname;

        value = _nativeRole ? value | (restore & PlayerInfoArea.Role) : value & ~PlayerInfoArea.Role;

        value = _nativeUnit ? value | (restore & PlayerInfoArea.UnitName) : value & ~PlayerInfoArea.UnitName;

        return value;
    }

    public void UpdateInfo(Player player)
    {
        if (_detached)
            return;

        _lastOwner = player;

        bool previousSuppress = SuppressExternalSync;
        SuppressExternalSync = true;
        try
        {
            bool hasCustomRole = player.TryGetSummonedInstance(out SummonedCustomRole summonedCustomRole);

            InfoTag infoTag = null;
            CustomInfoOrder customInfoOrderModule = null;
            ColorfulNickname colorfulNickname = null;

            if (hasCustomRole)
            {
                summonedCustomRole.TryGetModule(out infoTag);
                summonedCustomRole.TryGetModule(out customInfoOrderModule);
                summonedCustomRole.TryGetModule(out colorfulNickname);
            }

            bool customLayout = infoTag is not null || customInfoOrderModule is not null;

            _nativeRole = !customLayout && IsNativeRoleName(player, summonedCustomRole);

            _nativeNickname = _nativeRole && colorfulNickname is null && (string.IsNullOrEmpty(Nickname) || Nickname == player.DisplayName);

            bool hidesUnitName = hasCustomRole && summonedCustomRole.HasModule<NoUnitName>();

            _nativeUnit = _nativeRole && !hidesUnitName;

            player.InfoArea = ApplyAreas(player.InfoArea, summonedCustomRole?.PlayerInfoArea);

            string rawCustomInfo = $"{ColorPrefix}%custominfo%%nickname%%rolename%";
            string rawNickname = Nickname;
            string rawInfo = Info;
            string rawRole = Role;

            if (!NicknameSync.ValidateCustomInfo(Info.SanitizeCustomInfo(), out string customInfoError) && !string.IsNullOrEmpty(Info))
            {
                LogManager.Error($"CustomInfo is not correct, therefore the custom info part of player {player.PlayerId} won't be shown.\nCustomInfo: {Info}\nError: {customInfoError}");
                rawCustomInfo = rawCustomInfo.Replace("%custominfo%", "");
                rawInfo = string.Empty;
            }

            if (!_nativeRole && !NicknameSync.ValidateCustomInfo(Role.SanitizeCustomInfo(), out string roleNameError) && !string.IsNullOrEmpty(Role))
            {
                LogManager.Error($"RoleName is not correct, therefore the role name part of player {player.PlayerId} won't be shown.\nRoleName: {Role}\nError: {roleNameError}");
                rawCustomInfo = rawCustomInfo.Replace("%rolename%", "");
                rawRole = string.Empty;
            }

            if (hasCustomRole)
            {
                rawInfo = PlaceholderManager.ApplyPlaceholders(rawInfo, player, summonedCustomRole.Role);

                Team infoTeam = summonedCustomRole.Role.Role.GetTeam();
                if (DisguiseTeam.List.TryGetValue(player.PlayerId, out Team infoFakeTeam))
                    infoTeam = infoFakeTeam;

                string rawUnit = string.Empty;
                bool showUnit = false;

                if (!_nativeUnit && !hidesUnitName && !string.IsNullOrEmpty(rawRole) && TryGetUnitName(player, infoTeam, out string ownUnit))
                {
                    showUnit = true;
                    rawUnit = ownUnit;
                }

                if (infoTag is not null)
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

                if (customInfoOrderModule is not null)
                    rawCustomInfo = $"{ColorPrefix}{customInfoOrderModule.Order}";

                if (colorfulNickname is not null)
                {
                    LogManager.Debug($"Applying ColorfulNickname module to player {player.PlayerId} with color {colorfulNickname.Color} and nickname {Nickname}");

                    if (string.IsNullOrEmpty(colorfulNickname.Color))
                    {
                        LogManager.Warn($"The ColorfulNickname module of player {player.PlayerId} has no color set, skipping the colouring.");
                    }
                    else
                    {
                        string nick = Nickname?.Replace("<color=#855439>*</color>", "") ?? string.Empty;
                        if (string.IsNullOrEmpty(nick))
                            nick = player.Nickname;
                        string color = colorfulNickname.Color.StartsWith("#") ? colorfulNickname.Color : $"#{colorfulNickname.Color}";
                        if (!Misc.AcceptedColours.Contains(color.Replace("#", "")))
                            LogManager.Warn($"The color {color} is not acceptable by the game in ColorfulNicknames! Please use a valid hex color code.");
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

            if (_nativeNickname)
            {
                rawCustomInfo = rawCustomInfo.Replace("%nickname%", "");
                rawNickname = string.Empty;
            }

            if (_nativeRole)
            {
                rawCustomInfo = rawCustomInfo.Replace("%rolename%", "");
                rawRole = string.Empty;
            }

            if (string.IsNullOrEmpty(rawInfo))
                rawCustomInfo = rawCustomInfo.Replace("%custominfo%", "");

            if (!_nativeNickname && string.IsNullOrEmpty(rawNickname))
                rawNickname = player.Nickname;

            string composed = rawCustomInfo.Replace("%%", "%\n%").BulkReplace(new Dictionary<string, object>
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
            }, "%<val>%");

            if (string.IsNullOrWhiteSpace(composed.Replace(ColorPrefix, string.Empty)))
            {
                player.CustomInfo = string.Empty;
                return;
            }

            ApplyCustomInfo(player, composed);
        }
        finally
        {
            SuppressExternalSync = previousSuppress;
        }
    }

    private bool IsNativeRoleName(Player player, SummonedCustomRole summonedCustomRole)
    {
        RoleTypeId shownRole = summonedCustomRole is null ? player.Role : summonedCustomRole.Appearance != RoleTypeId.None ? summonedCustomRole.Appearance : summonedCustomRole.Role.Role;

        return string.Equals(Role, shownRole.GetFullName(), StringComparison.Ordinal);
    }

    private static bool TryGetUnitName(Player player, Team team, out string unitName)
    {
        unitName = string.Empty;

        if (!NamingRulesManager.TryGetNamingRule(team, out UnitNamingRule namingRule))
            return false;

        if (!DisguiseTeam.RoleBaseList.ContainsKey(player.PlayerId) && player.RoleBase is HumanRole humanRole && humanRole.Team == team)
        {
            string ownUnitName = NamingRulesManager.ClientFetchReceived(team, humanRole.UnitNameId);
            if (!string.IsNullOrEmpty(ownUnitName))
            {
                unitName = ownUnitName;
                return true;
            }
        }

        unitName = namingRule.LastGeneratedName;
        return !string.IsNullOrEmpty(unitName);
    }

    private void ApplyCustomInfo(Player player, string composed)
    {
        string cleaned = composed.SanitizeCustomInfo();

        if (cleaned != composed)
        {
            LogManager.Debug($"Removed the characters the game does not accept in a name tag from the tag of player {player.PlayerId}.\nBefore: {composed}\nAfter: {cleaned}");
            composed = cleaned;
        }

        if (!string.IsNullOrEmpty(composed) && composed.Length > 400)
        {
            LogManager.Error($"The name tag of player {player.PlayerId} is {composed.Length} characters long, but the game only accepts 400, so it won't be shown.\n" + $"Composed tag: {composed}\n" + "Shorten the 'custom_info' of the role, or the InfoTag layout building this tag.");
            composed = string.Empty;
        }

        if (!string.IsNullOrEmpty(composed) && !NicknameSync.ValidateCustomInfo(composed, out string error))
        {
            LogManager.Error($"The name tag of player {player.PlayerId} would be rejected by the game and won't be shown: {error}\n" + $"Composed tag: {composed}\n" + "Likely causes: a colour that isn't on the allowed list written inside 'custom_info', or a rich text tag the game does not allow.");
            composed = string.Empty;
        }

        if (string.IsNullOrEmpty(composed))
        {
            _nativeNickname = true;
            _nativeRole = true;
            _nativeUnit = true;

            player.InfoArea |= PlayerInfoArea.Nickname | PlayerInfoArea.Role | PlayerInfoArea.UnitName;
            player.CustomInfo = string.Empty;
        }
        else
        {
            player.CustomInfo = composed;
        }
    }
}