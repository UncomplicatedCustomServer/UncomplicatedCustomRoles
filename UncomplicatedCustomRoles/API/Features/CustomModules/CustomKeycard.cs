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
    public override List<string> RequiredArgs => ["KeycardType"];

    private KeycardItem _keycardItem;

    private static readonly Dictionary<string, ItemType> KeycardTypeAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Management", ItemType.KeycardCustomManagement },
        { "Metal", ItemType.KeycardCustomMetalCase },
        { "MetalCase", ItemType.KeycardCustomMetalCase },
        { "Site02", ItemType.KeycardCustomSite02 },
        { "Site", ItemType.KeycardCustomSite02 },
        { "TaskForce", ItemType.KeycardCustomTaskForce },
    };

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

    private static readonly string ValidKeycardTypes =
        string.Join(", ", KeycardTypeAliases.Keys.OrderBy(k => k));

    public override void OnAdded()
    {
        Timing.CallDelayed(Timing.WaitForOneFrame, () =>
        {
            if (KeycardType == ItemType.None)
            {
                LogManager.Error($"[CustomKeycard] 'KeycardType' is missing or invalid for player {Player?.Nickname}. Valid values: {ValidKeycardTypes}");
                return;
            }

            if (!KeycardType.TryGetTemplate<InventorySystem.Items.Keycards.KeycardItem>(out var template) ||
                !template.Customizable)
            {
                LogManager.Error($"[CustomKeycard] '{KeycardType}' is not a customizable keycard type for player {Player?.Nickname}. Valid values: {ValidKeycardTypes}");
                return;
            }

            _keycardItem = KeycardType switch
            {
                ItemType.KeycardCustomManagement => KeycardItem.CreateCustomKeycardManagement(
                    Player, ItemName, CardLabel, Permissions, KeycardColor, PermissionsColor, LabelColor),

                ItemType.KeycardCustomMetalCase => KeycardItem.CreateCustomKeycardMetal(
                    Player, ItemName, HolderName, CardLabel, Permissions, KeycardColor, PermissionsColor, LabelColor, WearLevel, SerialLabel),

                ItemType.KeycardCustomSite02 => KeycardItem.CreateCustomKeycardSite02(
                    Player, ItemName, HolderName, CardLabel, Permissions, KeycardColor, PermissionsColor, LabelColor, WearLevel),

                ItemType.KeycardCustomTaskForce => KeycardItem.CreateCustomKeycardTaskForce(
                    Player, ItemName, HolderName, Permissions, KeycardColor, PermissionsColor, SerialLabel, RankIndex),

                _ => null
            };

            if (_keycardItem is null)
                LogManager.Error($"[CustomKeycard] Failed to create keycard of type '{KeycardType}' for player {Player?.Nickname}. This is likely a bug, please report it.");
        });
        base.OnAdded();
    }
    
    private KeycardLevels BuildPermissions()
    {
        bool hasLevels = HasArg("ContainmentLevel") || HasArg("ArmoryLevel") || HasArg("AdminLevel");

        KeycardLevels levels = new(
            TryGetCastedValue("ContainmentLevel", 0),
            TryGetCastedValue("ArmoryLevel", 0),
            TryGetCastedValue("AdminLevel", 0));

        DoorPermissionFlags rawFlags = ParseFlags("Permissions");

        if (!hasLevels && rawFlags == DoorPermissionFlags.None)
            return levels;

        return new KeycardLevels(levels.Permissions | rawFlags);
    }

    private bool HasArg(string param) => Args is not null && Args.ContainsKey(param);

    private DoorPermissionFlags ParseFlags(string param)
    {
        if (Args is null || !Args.TryGetValue(param, out object raw) || raw is null)
            return DoorPermissionFlags.None;

        string joined = raw as string ?? (raw is IEnumerable enumerable
            ? string.Join(",", enumerable.Cast<object>().Where(o => o is not null).Select(o => o.ToString()))
            : raw.ToString());

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
