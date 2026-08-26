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
using System.Linq;
using System.Text.RegularExpressions;
using CustomPlayerEffects;
using InventorySystem.Configs;
using MapGeneration;
using PlayerRoles;
using UncomplicatedCustomRoles.API.Enums;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Features.CustomModules;
using UncomplicatedCustomRoles.API.Interfaces;
using UncomplicatedCustomRoles.Extensions;
using UncomplicatedCustomRoles.Integrations;

namespace UncomplicatedCustomRoles.Manager;

internal static class RoleValidator
{
    private static string[] _effectNames;

    private static readonly string[] KnownPlaceholders =
    [
        "nick", "displayname", "rand", "dnumber", "unitid", "unitname", "rolename",
        "customrolename", "customroleid", "customrolebadge",
        "health", "max_health", "ahp", "max_ahp", "hume", "max_hume"
    ];

    private static readonly Regex PlaceholderRegex =
        new("%([A-Za-z_]+)%", RegexOptions.Compiled);

    private static string[] EffectNames => _effectNames ??= ResolveEffectNames();

    private static string[] ResolveEffectNames()
    {
        try
        {
            return typeof(StatusEffectBase).Assembly.GetTypes()
                .Where(t => !t.IsAbstract && typeof(StatusEffectBase).IsAssignableFrom(t))
                .Select(t => t.Name)
                .OrderBy(n => n)
                .ToArray();
        }
        catch (Exception e)
        {
            LogManager.Warn(
                $"[Role Validator] Could not enumerate the game's status effects, 'effects' values won't be validated: {e.Message}");
            return [];
        }
    }

    internal static void Validate(ICustomRole role, out List<string> errors, out List<string> warnings)
    {
        errors = [];
        warnings = [];

        if (role is null)
        {
            errors.Add("the role is null.");
            return;
        }

        ValidateIdentity(role, errors, warnings);
        ValidateRoles(role, errors, warnings);
        ValidateHealthLike(role, errors, warnings);
        ValidateEffects(role, warnings);
        ValidateInventory(role, warnings);
        ValidateMisc(role, warnings);
        ValidateSpawnSettings(role, errors, warnings);
        ValidateRoleAfterEscape(role, warnings);
    }

    internal static bool IsValid(ICustomRole role, out string error)
    {
        Validate(role, out var errors, out _);
        error = errors.Count == 0 ? string.Empty : string.Join("\n", errors.Select(e => " - " + e));
        return errors.Count == 0;
    }

    private static void ValidateIdentity(ICustomRole role, List<string> errors, List<string> warnings)
    {
        if (role.Id < 0)
            warnings.Add($"'id' is negative ({role.Id}); ids should be 0 or greater.");

        if (string.IsNullOrWhiteSpace(role.Name))
            warnings.Add("'name' is empty; it is used to identify the role in logs and commands.");

        if (!string.IsNullOrEmpty(role.CustomInfo))
        {
            var sanitized = role.CustomInfo.SanitizeCustomInfo();

            if (sanitized != role.CustomInfo)
                warnings.Add(
                    "'custom_info' contains characters the game does not accept in a name tag (square brackets, emoji, ...); they are removed automatically, so the text will be shown without them.");

            if (!NicknameSync.ValidateCustomInfo(sanitized, out var customInfoError))
                warnings.Add($"'custom_info' will be rejected by the game: {customInfoError}");
        }

        ValidatePlaceholders("nickname", role.Nickname, warnings);
        ValidatePlaceholders("custom_info", role.CustomInfo, warnings);

        if (!string.IsNullOrEmpty(role.Nickname) && role.Nickname.Contains(",") &&
            role.Nickname.Split(',').Any(string.IsNullOrWhiteSpace))
            warnings.Add(
                "'nickname' contains an empty variant between commas; a player could spawn with an empty name.");

        ValidateBadge(role, warnings);
    }

    private static void ValidatePlaceholders(string field, string value, List<string> warnings)
    {
        if (string.IsNullOrEmpty(value))
            return;

        foreach (Match match in PlaceholderRegex.Matches(value))
        {
            var name = match.Groups[1].Value;
            if (!KnownPlaceholders.Contains(name))
                warnings.Add(
                    $"'{field}' contains the unknown placeholder '%{name}%'; it will be shown literally. Valid placeholders: {string.Join(", ", KnownPlaceholders.Select(p => $"%{p}%"))}.");
        }
    }

    private static void ValidateBadge(ICustomRole role, List<string> warnings)
    {
        var nameUsable = role.BadgeName is not null && role.BadgeName.Length > 1;
        var colorUsable = role.BadgeColor is not null && role.BadgeColor.Length > 2;
        var nameSet = !string.IsNullOrWhiteSpace(role.BadgeName);
        var colorSet = !string.IsNullOrWhiteSpace(role.BadgeColor);

        if ((nameSet || colorSet) && (!nameUsable || !colorUsable))
        {
            if (nameSet && !nameUsable)
                warnings.Add(
                    $"'badge_name' ('{role.BadgeName}') is too short (at least 2 characters); the badge will not be applied.");
            if (colorSet && !colorUsable)
                warnings.Add($"'badge_color' ('{role.BadgeColor}') is too short; the badge will not be applied.");
            if (nameUsable && !colorSet)
                warnings.Add("'badge_name' is set but 'badge_color' is empty; the badge will not be applied.");
        }

        if (nameUsable && colorUsable && role.BadgeColor is not "default" &&
            !SpawnManager.ColorMap.ContainsKey(role.BadgeColor))
            warnings.Add(
                $"'badge_color' '{role.BadgeColor}' is not a badge color the game knows, clients may show it as white. Known colors: default, {string.Join(", ", SpawnManager.ColorMap.Keys)}.");
    }

    private static void ValidateRoles(ICustomRole role, List<string> errors, List<string> warnings)
    {
        if (role.Role is RoleTypeId.None || role.Role.GetTeam() is Team.Dead)
            errors.Add(
                $"'role' must be a valid role, got '{role.Role}'. Examples: ClassD, Scientist, NtfSergeant, Scp0492.");

        if (role.RoleAppearance is not RoleTypeId.None && role.RoleAppearance.GetTeam() is Team.Dead)
            warnings.Add(
                $"'role_appearance' '{role.RoleAppearance}' is not a valid alive role; the role will keep the appearance of '{role.Role}'.");
    }

    private static void ValidateHealthLike(ICustomRole role, List<string> errors, List<string> warnings)
    {
        if (role.Health is not null)
        {
            if (role.Health.Maximum < 1)
                errors.Add($"'health.maximum' must be at least 1, got {role.Health.Maximum}.");
            if (role.Health.Amount < 1)
                warnings.Add($"'health.amount' is {role.Health.Amount}; the player would spawn (nearly) dead.");
        }

        if (role.Ahp is not null)
        {
            if (role.Ahp.Amount < 0)
                warnings.Add($"'ahp.amount' is negative ({role.Ahp.Amount}); it will be treated as 0.");
            if (role.Ahp.Limit < 0)
                warnings.Add($"'ahp.limit' is negative ({role.Ahp.Limit}).");
            if (role.Ahp.Efficacy is < 0f or > 1f)
                warnings.Add(
                    $"'ahp.efficacy' should be between 0 and 1 (fraction of damage absorbed), got {role.Ahp.Efficacy}.");
            if (role.Ahp.Decay < 0)
                warnings.Add($"'ahp.decay' is negative ({role.Ahp.Decay}); the AHP would grow instead of decaying.");
            if (role.Ahp.Sustain < 0)
                warnings.Add($"'ahp.sustain' is negative ({role.Ahp.Sustain}).");
        }

        if (role.HumeShield is not null)
        {
            if (role.HumeShield.Amount < 0)
                warnings.Add($"'hume_shield.amount' is negative ({role.HumeShield.Amount}).");
            if (role.HumeShield.Maximum < 0)
                warnings.Add($"'hume_shield.maximum' is negative ({role.HumeShield.Maximum}).");
            if (role.HumeShield.Amount > 0 && role.HumeShield.Maximum < role.HumeShield.Amount)
                warnings.Add(
                    $"'hume_shield.maximum' ({role.HumeShield.Maximum}) is below 'hume_shield.amount' ({role.HumeShield.Amount}).");
            if (role.HumeShield.RegenerationAmount < 0)
                warnings.Add(
                    $"'hume_shield.regeneration_amount' is negative ({role.HumeShield.RegenerationAmount}); the regeneration only runs when it is above 0, so the shield will never regenerate.");
            if (role.HumeShield.RegenerationDelay < 0)
                warnings.Add(
                    $"'hume_shield.regeneration_delay' is negative ({role.HumeShield.RegenerationDelay}); use 0 for no delay.");
            if (role.HumeShield.RegenerationSpeed < 0)
                warnings.Add(
                    $"'hume_shield.regeneration_speed' is negative ({role.HumeShield.RegenerationSpeed}); use 0 to regenerate every frame.");
            if (role.HumeShield.Maximum > 0 && role.HumeShield.RegenerationAmount == 0 &&
                role.HumeShield.Amount < role.HumeShield.Maximum)
                warnings.Add(
                    "'hume_shield.regeneration_amount' is 0, so the shield will never regenerate up to its maximum.");
        }

        if (role.Stamina is not null)
        {
            if (role.Stamina.RegenMultiplier < 0)
                warnings.Add($"'stamina.regen_multiplier' is negative ({role.Stamina.RegenMultiplier}).");
            if (role.Stamina.UsageMultiplier < 0)
                warnings.Add($"'stamina.usage_multiplier' is negative ({role.Stamina.UsageMultiplier}).");
        }
    }

    private static void ValidateEffects(ICustomRole role, List<string> warnings)
    {
        if (role.Effects is null || EffectNames.Length == 0)
            return;

        for (var i = 0; i < role.Effects.Count; i++)
        {
            var effect = role.Effects[i];
            if (effect is null)
            {
                warnings.Add($"'effects' entry #{i + 1} is empty.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(effect.EffectType) ||
                !EffectNames.Any(n => string.Equals(n, effect.EffectType, StringComparison.InvariantCultureIgnoreCase)))
            {
                var closest = string.IsNullOrWhiteSpace(effect.EffectType)
                    ? null
                    : EffectNames.FirstOrDefault(n =>
                        n.StartsWith(effect.EffectType, StringComparison.InvariantCultureIgnoreCase));

                warnings.Add(
                    $"'effects' entry #{i + 1} has an unknown effect_type '{effect.EffectType}'; it will be skipped.{(closest is null ? string.Empty : $" Did you mean '{closest}'?")} Valid effects: {string.Join(", ", EffectNames)}.");
            }

            if (effect.Intensity == 0)
                warnings.Add(
                    $"'effects' entry #{i + 1} ('{effect.EffectType}') has intensity 0, which disables the effect; use at least 1.");
        }
    }

    private static void ValidateInventory(ICustomRole role, List<string> warnings)
    {
        if (role.Inventory is not null)
        {
            if (role.Inventory.Count > 8)
                warnings.Add(
                    $"'inventory' lists {role.Inventory.Count} items but a player only has 8 slots; the extra items will not fit.");

            foreach (var item in role.Inventory.Where(IsAmmo))
                warnings.Add(
                    $"'inventory' contains the ammo '{item}'; put ammo under 'ammo:' instead so the amount is respected.");

            if (role.Inventory.Any(i => i is ItemType.None))
                warnings.Add("'inventory' contains 'None' entries; they give nothing and should be removed.");
        }

        if (role.Ammo is not null)
            foreach (var ammo in role.Ammo.Keys.Where(k => !IsAmmo(k)))
                warnings.Add($"'ammo' contains '{ammo}', which is not an ammo type; only Ammo* values belong here.");

        ValidateInventoryLimits(role, warnings);
    }

    private static void ValidateInventoryLimits(ICustomRole role, List<string> warnings)
    {
        if (role.CustomInventoryLimits is null || role.CustomInventoryLimits.Count == 0)
            return;

        try
        {
            HashSet<ItemCategory> configurable = new(
                InventoryLimits.StandardCategoryLimits
                    .Where(kvp => kvp.Value >= 0)
                    .Select(kvp => kvp.Key));

            foreach (var category in role.CustomInventoryLimits.Keys.Where(c => !configurable.Contains(c)))
                warnings.Add(category is ItemCategory.Ammo
                    ? "'custom_inventory_limits' contains 'Ammo', which the game does not count in inventory slots; the entry does nothing. Ammo is limited per ammo type, not per category."
                    : $"'custom_inventory_limits' contains '{category}', which the game does not limit by slot count. UCR still applies the limit server-side, but the client's inventory HUD will not show it. Categories the game limits on its own: {string.Join(", ", configurable.OrderBy(c => c.ToString()))}.");
        }
        catch (Exception e)
        {
            LogManager.Debug($"[Role Validator] Could not read the game's standard category limits: {e.Message}");
        }
    }

    private static void ValidateMisc(ICustomRole role, List<string> warnings)
    {
        if (role.MaxScp330Candies < 0)
            warnings.Add($"'max_scp330_candies' is negative ({role.MaxScp330Candies}).");

        if (role.DamageMultiplier < 0)
            warnings.Add(
                $"'damage_multiplier' is negative ({role.DamageMultiplier}); the role would heal targets instead of damaging them.");

        if (role.SpawnHintDuration < 0)
            warnings.Add($"'spawn_hint_duration' is negative ({role.SpawnHintDuration}).");

        var scale = role.Scale;
        if (scale.x != 0 || scale.y != 0 || scale.z != 0)
        {
            if (scale.x < 0 || scale.y < 0 || scale.z < 0)
                warnings.Add(
                    $"'scale' has a negative axis ({scale.x}, {scale.y}, {scale.z}); the model will be turned inside out. Use 1 for the normal size.");
            else if (scale.x == 0 || scale.y == 0 || scale.z == 0)
                warnings.Add(
                    $"'scale' has an axis set to 0 ({scale.x}, {scale.y}, {scale.z}); the model will be flattened on it. Use 1 for the normal size, or 0 on every axis to keep the vanilla one.");
        }
    }

    private static void ValidateSpawnSettings(ICustomRole role, List<string> errors, List<string> warnings)
    {
        if (role.SpawnSettings is null)
        {
            errors.Add("'spawn_settings' is missing.");
            return;
        }

        switch (role.SpawnSettings.Spawn)
        {
            case SpawnType.ZoneSpawn when role.SpawnSettings.SpawnZones is null || !role.SpawnSettings.SpawnZones.Any():
                errors.Add("'spawn_settings.spawn' is ZoneSpawn but 'spawn_zones' is empty.");
                break;
            case SpawnType.RoomsSpawn
                when role.SpawnSettings.SpawnRooms is null || !role.SpawnSettings.SpawnRooms.Any():
                errors.Add("'spawn_settings.spawn' is RoomsSpawn but 'spawn_rooms' is empty.");
                break;
            case SpawnType.SpawnPointSpawn
                when role.SpawnSettings.SpawnPoints is null || !role.SpawnSettings.SpawnPoints.Any():
                errors.Add("'spawn_settings.spawn' is SpawnPointSpawn but 'spawn_points' is empty.");
                break;
            case SpawnType.RoleSpawn when role.SpawnSettings.SpawnRoles is null || !role.SpawnSettings.SpawnRoles.Any():
                errors.Add("'spawn_settings.spawn' is RoleSpawn but 'spawn_roles' is empty.");
                break;
        }

        if (role.SpawnSettings.SpawnZones is not null)
            foreach (var zone in role.SpawnSettings.SpawnZones.Where(z => z is FacilityZone.None))
                warnings.Add(
                    $"'spawn_settings.spawn_zones' contains '{zone}', which is not a real facility zone. Valid zones: LightContainment, HeavyContainment, Entrance, Surface.");

        if (role.SpawnSettings.SpawnRoles is not null)
            foreach (var spawnRole in role.SpawnSettings.SpawnRoles.Where(r =>
                         r is RoleTypeId.None || r.GetTeam() is Team.Dead))
                warnings.Add(
                    $"'spawn_settings.spawn_roles' contains '{spawnRole}', which is not a spawnable role to take a spawn position from.");

        if (role.IgnoreSpawnSystem)
            return;

        ValidateSpawnEligibility(role, warnings);
    }

    private static void ValidateSpawnEligibility(ICustomRole role, List<string> warnings)
    {
        if (role.SpawnSettings.SpawnChance <= 0)
            warnings.Add(
                $"'spawn_settings.spawn_chance' is {role.SpawnSettings.SpawnChance}; it has to be above 0 or the role will never spawn on its own (only 'ucr spawn' and the API can still hand it out).");

        if (role.SpawnSettings.MaxPlayers < 1)
            warnings.Add(
                $"'spawn_settings.max_players' is {role.SpawnSettings.MaxPlayers}; it is the number of players that can hold this role at the same time, so the role will never spawn on its own.");

        var delayed = role.SpawnSettings.SpawnDelay > 0;

        if (role.SpawnSettings.SpawnDelay < 0)
            warnings.Add(
                $"'spawn_settings.spawn_delay' is negative ({role.SpawnSettings.SpawnDelay}); use 0 to spawn the role together with the vanilla role it replaces.");

        if (role.SpawnSettings.CanReplaceRoles is not { } canReplaceRoles || !canReplaceRoles.Any())
        {
            warnings.Add(delayed
                ? "'spawn_settings.spawn_delay' is set but 'can_replace_roles' is empty; the delayed spawn has nobody to convert. List the roles the players should be taken from, e.g. 'Spectator'."
                : "'spawn_settings.can_replace_roles' is empty; with no delay the role is handed out by replacing one of these roles at spawn, so an empty list means it never spawns on its own. List the vanilla roles it should replace, e.g. 'ClassD'.");
            return;
        }

        if (!delayed)
            foreach (var replace in canReplaceRoles.Where(r => !SpawnManager.SpawnEvaluatedRoles.Contains(r)))
                warnings.Add(
                    $"'spawn_settings.can_replace_roles' contains '{replace}', which the spawn system never evaluates - it will never trigger a replacement. Usable roles: {string.Join(", ", SpawnManager.SpawnEvaluatedRoles.OrderBy(r => r.ToString()))}. Set 'spawn_delay' if you want the role to be handed out mid-round instead.");
    }

    private static void ValidateRoleAfterEscape(ICustomRole role, List<string> warnings)
    {
        if (role.RoleAfterEscape is null)
            return;

        foreach (var kvp in role.RoleAfterEscape)
        {
            if (kvp.Key is not "default")
            {
                var key = kvp.Key.Split(' ');
                if (key.Length != 4 || key[0] is not "cuffed" || key[1] is not "by")
                    warnings.Add(
                        $"'role_after_escape' key '{kvp.Key}' is invalid; use 'default' or 'cuffed by <InternalTeam|CustomTeam|CustomRole> <id>'.");
                else
                    switch (key[2])
                    {
                        case "InternalTeam" or "IT" when !Enum.TryParse(key[3], out Team _):
                            warnings.Add(
                                $"'role_after_escape' key '{kvp.Key}': '{key[3]}' is not a valid team. Valid teams: {string.Join(", ", Enum.GetNames(typeof(Team)))}.");
                            break;
                        case "CustomTeam" or "CT" when !uint.TryParse(key[3], out _):
                            warnings.Add(
                                $"'role_after_escape' key '{kvp.Key}': '{key[3]}' is not a valid custom team id (a number).");
                            break;
                        case "CustomRole" or "CR" when !int.TryParse(key[3], out _):
                            warnings.Add(
                                $"'role_after_escape' key '{kvp.Key}': '{key[3]}' is not a valid custom role id (a number).");
                            break;
                        case not ("InternalTeam" or "IT" or "CustomTeam" or "CT" or "CustomRole" or "CR"):
                            warnings.Add(
                                $"'role_after_escape' key '{kvp.Key}': unknown source '{key[2]}'; use InternalTeam (IT), CustomTeam (CT) or CustomRole (CR).");
                            break;
                    }
            }

            if (kvp.Value is "Deny" or "deny" or "DENY")
                continue;

            if (string.IsNullOrWhiteSpace(kvp.Value))
            {
                warnings.Add(
                    $"'role_after_escape' value for '{kvp.Key}' is empty; the escaping player would end up as a Spectator. Use 'Deny' to block the escape, or 'InternalRole <role>' / 'CustomRole <id>'.");
                continue;
            }

            var value = kvp.Value.Split(' ');
            if (value.Length != 2)
                warnings.Add(
                    $"'role_after_escape' value '{kvp.Value}' is invalid; use 'Deny', 'InternalRole <role>' or 'CustomRole <id>'.");
            else
                switch (value[0])
                {
                    case "InternalRole" or "IR" when !Enum.TryParse(value[1], out RoleTypeId _):
                        warnings.Add(
                            $"'role_after_escape' value '{kvp.Value}': '{value[1]}' is not a valid role. Examples: ClassD, ChaosConscript, NtfPrivate.");
                        break;
                    case "CustomRole" or "CR" when !int.TryParse(value[1], out _):
                        warnings.Add(
                            $"'role_after_escape' value '{kvp.Value}': '{value[1]}' is not a valid custom role id (a number).");
                        break;
                    case not ("InternalRole" or "IR" or "CustomRole" or "CR"):
                        warnings.Add(
                            $"'role_after_escape' value '{kvp.Value}': unknown source '{value[0]}'; use InternalRole (IR) or CustomRole (CR).");
                        break;
                }
        }
    }

    internal static void ValidatePostLoad(ICustomRole role)
    {
        var label = $"{role.Name} ({role.Id})";

        ValidateCustomFlags(role, label);
        ValidateEscapeReferences(role, label);
        ValidateCustomItems(role, label);
    }

    private static void ValidateCustomItems(ICustomRole role, string label)
    {
        if (role.CustomItemsInventory is null || role.CustomItemsInventory.Count == 0)
            return;

        if (UCI.Assembly is null || ECI.PluginInstance is not null)
            return;

        foreach (var id in role.CustomItemsInventory)
            try
            {
                if (!UCI.HasCustomItem(id, out _))
                    LogManager.Warn(
                        $"[Role Validator] {label}: 'custom_items_inventory' references custom item {id}, which is not registered in UncomplicatedCustomItems; nothing will be given for it.");
            }
            catch (Exception e)
            {
                LogManager.Debug($"[Role Validator] {label}: could not check custom item {id}: {e.Message}");
            }
    }

    private static void ValidateCustomFlags(ICustomRole role, string label)
    {
        if (role.CustomFlags is null || role.CustomFlags.Count == 0)
            return;

        List<KeyValuePair<string, Dictionary<string, object>>> flags;
        try
        {
            flags = YamlFlagsHandler.Decode(role.CustomFlags) ?? [];
        }
        catch (Exception e)
        {
            LogManager.Warn($"[Role Validator] {label}: 'custom_flags' could not be parsed: {e.Message}");
            return;
        }

        foreach (var flag in flags)
        {
            var type = YamlFlagsHandler.Modules.FirstOrDefault(t =>
                string.Equals(t.Name, flag.Key, StringComparison.OrdinalIgnoreCase));

            if (type is null)
            {
                LogManager.Warn(
                    $"[Role Validator] {label}: unknown custom flag '{flag.Key}'; it will be ignored. Available flags: {string.Join(", ", YamlFlagsHandler.Modules.Select(t => t.Name).OrderBy(n => n))}.");
                continue;
            }

            try
            {
                if (Activator.CreateInstance(type) is not CustomModule module)
                    continue;

                module.Initialize(null, flag.Value);

                var missing = module.RequiredArgs?.Where(arg => !module.Args.ContainsKey(arg)).ToList();
                if (missing is { Count: > 0 })
                {
                    LogManager.Warn(
                        $"[Role Validator] {label}: custom flag '{type.Name}' is missing required setting(s): {string.Join(", ", missing)}; it will be skipped on spawn.");
                    continue;
                }

                if (!module.Validate(out var error))
                    LogManager.Warn(
                        $"[Role Validator] {label}: custom flag '{type.Name}' has an invalid setting: {error} It will be skipped on spawn.");
            }
            catch (Exception e)
            {
                LogManager.Debug($"[Role Validator] {label}: could not dry-run custom flag '{type.Name}': {e.Message}");
            }
        }
    }

    private static void ValidateEscapeReferences(ICustomRole role, string label)
    {
        if (role.RoleAfterEscape is null)
            return;

        foreach (var kvp in role.RoleAfterEscape)
        {
            var key = kvp.Key.Split(' ');
            if (key.Length == 4 && key[2] is "CustomRole" or "CR" && int.TryParse(key[3], out var cuffedById) &&
                !CustomRole.CustomRoles.ContainsKey(cuffedById))
                LogManager.Warn(
                    $"[Role Validator] {label}: 'role_after_escape' key '{kvp.Key}' references custom role {cuffedById}, which is not registered.");

            var value = kvp.Value?.Split(' ') ?? [];
            if (value.Length == 2 && value[0] is "CustomRole" or "CR" && int.TryParse(value[1], out var targetId) &&
                !CustomRole.CustomRoles.ContainsKey(targetId))
                LogManager.Warn(
                    $"[Role Validator] {label}: 'role_after_escape' value '{kvp.Value}' references custom role {targetId}, which is not registered - escaping players would go nowhere.");
        }
    }

    private static bool IsAmmo(ItemType item)
    {
        return item.ToString().StartsWith("Ammo", StringComparison.Ordinal);
    }
}