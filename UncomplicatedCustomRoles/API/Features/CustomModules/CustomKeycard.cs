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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Interactables.Interobjects.DoorUtils;
using InventorySystem;
using LabApi.Features.Wrappers;
using MEC;
using UncomplicatedCustomRoles.Manager;
using UnityEngine;

namespace UncomplicatedCustomRoles.API.Features.CustomModules;

public class CustomKeycard : CustomModule
{
    private static readonly Dictionary<string, ItemType> KeycardTypeAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Management", ItemType.KeycardCustomManagement },
        { "Metal", ItemType.KeycardCustomMetalCase },
        { "MetalCase", ItemType.KeycardCustomMetalCase },
        { "Site02", ItemType.KeycardCustomSite02 },
        { "Site", ItemType.KeycardCustomSite02 },
        { "TaskForce", ItemType.KeycardCustomTaskForce }
    };

    private static readonly string ValidKeycardTypes = string.Join(", ", KeycardTypeAliases.Keys.OrderBy(k => k));

    private KeycardItem _keycardItem;
    public override List<string> RequiredArgs => ["KeycardType"];

    internal ItemType KeycardType
    {
        get
        {
            string raw = TryGetStringValue("KeycardType")?.Trim();
            if (string.IsNullOrEmpty(raw))
                return ItemType.None;

            if (KeycardTypeAliases.TryGetValue(raw, out ItemType alias))
                return alias;

            return Enum.TryParse(raw, true, out ItemType parsed) ? parsed : ItemType.None;
        }
    }

    internal string ItemName => PlaceholderManager.ApplyPlaceholders(TryGetStringValue("ItemName", "Custom Keycard"), Player, CustomRole.Role);

    internal string HolderName => PlaceholderManager.ApplyPlaceholders(TryGetStringValue("HolderName", "Unknown"), Player, CustomRole.Role);

    internal string CardLabel => PlaceholderManager.ApplyPlaceholders(TryGetStringValue("CardLabel", string.Empty), Player, CustomRole.Role);

    internal KeycardLevels Permissions => BuildPermissions();
    internal Color KeycardColor => ParseColor("KeycardColor", Color.white);
    internal Color PermissionsColor => ParseColor("PermissionsColor", Color.white);
    internal Color LabelColor => ParseColor("LabelColor", Color.white);
    internal byte WearLevel => TryGetCastedValue<byte>("WearLevel");
    internal string SerialLabel => TryGetStringValue("SerialLabel", "000000000000");
    internal int RankIndex => TryGetCastedValue("RankIndex", 0);

    public override bool Validate(out string error)
    {
        if (KeycardType == ItemType.None)
        {
            error = $"'KeycardType' '{TryGetStringValue("KeycardType")}' is not a valid keycard. Valid values: {ValidKeycardTypes}.";
            return false;
        }

        if (!KeycardType.TryGetTemplate(out InventorySystem.Items.Keycards.KeycardItem template) || !template.Customizable)
        {
            error = $"'{KeycardType}' is not a customizable keycard type. Valid values: {ValidKeycardTypes}.";
            return false;
        }

        foreach (string level in new[] { "ContainmentLevel", "ArmoryLevel", "AdminLevel" })
            if (HasArg(level))
            {
                string raw = TryGetStringValue(level);
                if (!int.TryParse(raw, out int value) || value is < 0 or > 3)
                {
                    error = $"'{level}' must be a whole number between 0 and 3, got '{raw}'.";
                    return false;
                }
            }

        if (HasArg("WearLevel") && !byte.TryParse(TryGetStringValue("WearLevel"), out _))
        {
            error = $"'WearLevel' must be a whole number between 0 and 255, got '{TryGetStringValue("WearLevel")}'.";
            return false;
        }

        foreach (string colorParam in new[] { "KeycardColor", "PermissionsColor", "LabelColor" })
            if (HasArg(colorParam) && !TryParseColor(TryGetStringValue(colorParam)))
            {
                error = $"'{colorParam}' '{TryGetStringValue(colorParam)}' is not a valid hex color. Use a value like #FF0000.";
                return false;
            }

        if (HasArg("Permissions"))
        {
            string joined = JoinArg("Permissions");
            if (!string.IsNullOrWhiteSpace(joined) && !Enum.TryParse(joined.Replace(" ", string.Empty), true, out DoorPermissionFlags _))
            {
                error = $"'Permissions' value '{joined}' contains invalid door permission flag(s). Valid flags: {string.Join(", ", Enum.GetNames(typeof(DoorPermissionFlags)))}.";
                return false;
            }
        }

        error = null;
        return true;
    }

    private static bool TryParseColor(string raw)
    {
        if (string.IsNullOrEmpty(raw))
            return false;

        if (!raw.StartsWith("#"))
            raw = "#" + raw;

        return ColorUtility.TryParseHtmlString(raw, out _);
    }

    private string JoinArg(string param)
    {
        if (Args is null || !Args.TryGetValue(param, out object raw) || raw is null)
            return string.Empty;

        return raw is string s ? s : raw is IEnumerable enumerable ? string.Join(",", enumerable.Cast<object>().Where(o => o is not null).Select(o => o.ToString())) : raw.ToString();
    }

    public override void OnAdded()
    {
        Timing.CallDelayed(Timing.WaitForOneFrame, () =>
        {
            if (Player is null || !Player.IsAlive)
                return;

            if (Player.IsInventoryFull)
            {
                LogManager.Warn($"[CustomKeycard] Can't give the '{KeycardType}' keycard to {Player.Nickname}: their inventory is already full. Free a slot in 'inventory' or remove one of the role's CustomKeycard flags.");
                return;
            }

            _keycardItem = KeycardType switch
            {
                ItemType.KeycardCustomManagement => KeycardItem.CreateCustomKeycardManagement(Player, ItemName, CardLabel, Permissions, KeycardColor, PermissionsColor, LabelColor),

                ItemType.KeycardCustomMetalCase => KeycardItem.CreateCustomKeycardMetal(Player, ItemName, HolderName, CardLabel, Permissions, KeycardColor, PermissionsColor, LabelColor, WearLevel, SerialLabel),

                ItemType.KeycardCustomSite02 => KeycardItem.CreateCustomKeycardSite02(Player, ItemName, HolderName, CardLabel, Permissions, KeycardColor, PermissionsColor, LabelColor, WearLevel),

                ItemType.KeycardCustomTaskForce => KeycardItem.CreateCustomKeycardTaskForce(Player, ItemName, HolderName, Permissions, KeycardColor, PermissionsColor, SerialLabel, RankIndex),

                _ => null
            };

            if (_keycardItem is null)
                LogManager.Error($"[CustomKeycard] Failed to create keycard of type '{KeycardType}' for player {Player?.Nickname}. If the type is a valid customizable keycard this is a bug, please report it.");
        });
        base.OnAdded();
    }

    private KeycardLevels BuildPermissions()
    {
        bool hasLevels = HasArg("ContainmentLevel") || HasArg("ArmoryLevel") || HasArg("AdminLevel");

        KeycardLevels levels = new(TryGetCastedValue("ContainmentLevel", 0), TryGetCastedValue("ArmoryLevel", 0), TryGetCastedValue("AdminLevel", 0));

        DoorPermissionFlags rawFlags = ParseFlags("Permissions");

        if (!hasLevels && rawFlags == DoorPermissionFlags.None)
            return levels;

        return new KeycardLevels(levels.Permissions | rawFlags);
    }

    private bool HasArg(string param)
    {
        return Args is not null && Args.ContainsKey(param);
    }

    private DoorPermissionFlags ParseFlags(string param)
    {
        string joined = JoinArg(param);

        if (string.IsNullOrWhiteSpace(joined))
            return DoorPermissionFlags.None;

        if (!Enum.TryParse(joined.Replace(" ", string.Empty), true, out DoorPermissionFlags result))
        {
            LogManager.Warn($"[CustomKeycard] Invalid value '{joined}' for '{param}'. Valid flags: {string.Join(", ", Enum.GetNames(typeof(DoorPermissionFlags)))}. Ignoring it.");
            return DoorPermissionFlags.None;
        }

        return result;
    }

    private Color ParseColor(string param, Color def)
    {
        string raw = TryGetStringValue(param);
        if (raw is null)
            return def;

        if (!raw.StartsWith("#"))
            raw = "#" + raw;

        if (!ColorUtility.TryParseHtmlString(raw, out Color color))
        {
            LogManager.Warn($"[CustomKeycard] Invalid color '{TryGetStringValue(param)}' for '{param}'. Expected a hex color like #FF0000. Using default (white).");
            return def;
        }

        return color;
    }
}