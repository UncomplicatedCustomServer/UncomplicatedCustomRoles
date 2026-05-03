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
using Interactables.Interobjects.DoorUtils;
using InventorySystem;
using LabApi.Features.Wrappers;
using MEC;
using UncomplicatedCustomRoles.Manager;
using UnityEngine;

namespace UncomplicatedCustomRoles.API.Features.CustomModules;

public class CustomKeycard : CustomModule
{
    public override List<string> RequiredArgs => new()
    {
        "KeycardType"
    };

    private KeycardItem _keycardItem;

    internal ItemType KeycardType => ParseEnum("KeycardType", ItemType.None);
    internal string ItemName => TryGetStringValue("ItemName", "Custom Keycard");
    internal string HolderName => TryGetStringValue("HolderName", "Unknown");
    internal string CardLabel => TryGetStringValue("CardLabel", string.Empty);
    internal KeycardLevels Permissions => new(ParseEnum("Permissions", DoorPermissionFlags.None));
    internal Color KeycardColor => ParseColor("KeycardColor", Color.white);
    internal Color PermissionsColor => ParseColor("PermissionsColor", Color.white);
    internal Color LabelColor => ParseColor("LabelColor", Color.white);
    internal byte WearLevel => TryGetCastedValue<byte>("WearLevel");
    internal string SerialLabel => TryGetStringValue("SerialLabel", "000000000000");
    internal int RankIndex => TryGetCastedValue("RankIndex", 0);

    private const string ValidKeycardTypes =
        "KeycardCustomManagement, KeycardCustomMetalCase, KeycardCustomSite02, KeycardCustomTaskForce";

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

    private T ParseEnum<T>(string param, T def) where T : struct, Enum
    {
        string raw = TryGetStringValue(param);
        if (raw is null)
            return def;

        if (!Enum.TryParse(raw, true, out T result))
        {
            LogManager.Warn($"[CustomKeycard] Invalid value '{raw}' for '{param}'. Valid values: {string.Join(", ", Enum.GetNames(typeof(T)))}. Using default: {def}");
            return def;
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
