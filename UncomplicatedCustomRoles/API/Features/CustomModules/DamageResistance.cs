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
using InventorySystem.Items.Scp1509;
using LabApi.Events.Arguments.Interfaces;
using LabApi.Events.Arguments.PlayerEvents;
using PlayerRoles.PlayableScps.Scp1507;
using PlayerRoles.PlayableScps.Scp3114;
using PlayerStatsSystem;
using UncomplicatedCustomRoles.API.Enums;

namespace UncomplicatedCustomRoles.API.Features.CustomModules;

internal class DamageResistance : CustomModule
{
    // -----------------------------------------------------------------------
    // <copyright file="DamageTypeExtensions.cs" company="ExMod Team">
    // Copyright (c) ExMod Team. All rights reserved.
    // Licensed under the CC BY-SA 3.0 license.
    // </copyright>
    // -----------------------------------------------------------------------

    private static readonly Dictionary<ItemType, DamageType> ItemConversion = new()
    {
        { ItemType.GunCrossvec, DamageType.Crossvec },
        { ItemType.GunLogicer, DamageType.Logicer },
        { ItemType.GunRevolver, DamageType.Revolver },
        { ItemType.GunShotgun, DamageType.Shotgun },
        { ItemType.GunAK, DamageType.AK },
        { ItemType.GunCOM15, DamageType.Com15 },
        { ItemType.GunCom45, DamageType.Com45 },
        { ItemType.GunCOM18, DamageType.Com18 },
        { ItemType.GunFSP9, DamageType.Fsp9 },
        { ItemType.GunE11SR, DamageType.E11Sr },
        { ItemType.MicroHID, DamageType.MicroHid },
        { ItemType.ParticleDisruptor, DamageType.ParticleDisruptor },
        { ItemType.Jailbird, DamageType.Jailbird },
        { ItemType.GunFRMG0, DamageType.Frmg0 },
        { ItemType.GunA7, DamageType.A7 },
        { ItemType.GunSCP127, DamageType.Scp127 }
    };

    private static readonly Dictionary<DeathTranslation, DamageType> TranslationConversion = new()
    {
        { DeathTranslations.Asphyxiated, DamageType.Asphyxiation },
        { DeathTranslations.Bleeding, DamageType.Bleeding },
        { DeathTranslations.Crushed, DamageType.Crushed },
        { DeathTranslations.Decontamination, DamageType.Decontamination },
        { DeathTranslations.Explosion, DamageType.Explosion },
        { DeathTranslations.Falldown, DamageType.Falldown },
        { DeathTranslations.Poisoned, DamageType.Poison },
        { DeathTranslations.Recontained, DamageType.Recontainment },
        { DeathTranslations.Scp049, DamageType.Scp049 },
        { DeathTranslations.Scp096, DamageType.Scp096 },
        { DeathTranslations.Scp173, DamageType.Scp173 },
        { DeathTranslations.Scp207, DamageType.Scp207 },
        { DeathTranslations.Scp939Lunge, DamageType.Scp939 },
        { DeathTranslations.Scp939Other, DamageType.Scp939 },
        { DeathTranslations.Scp3114Slap, DamageType.Scp3114 },
        { DeathTranslations.Tesla, DamageType.Tesla },
        { DeathTranslations.Unknown, DamageType.Unknown },
        { DeathTranslations.Warhead, DamageType.Warhead },
        { DeathTranslations.Zombie, DamageType.Scp0492 },
        { DeathTranslations.BulletWounds, DamageType.Firearm },
        { DeathTranslations.PocketDecay, DamageType.PocketDimension },
        { DeathTranslations.SeveredHands, DamageType.SeveredHands },
        { DeathTranslations.FriendlyFireDetector, DamageType.FriendlyFireDetector },
        { DeathTranslations.UsedAs106Bait, DamageType.FemurBreaker },
        { DeathTranslations.MicroHID, DamageType.MicroHid },
        { DeathTranslations.Hypothermia, DamageType.Hypothermia },
        { DeathTranslations.MarshmallowMan, DamageType.Marshmallow },
        { DeathTranslations.Scp1344, DamageType.SeveredEyes },
        { DeathTranslations.Scp1509, DamageType.Scp1509 }
    };

    private static readonly Dictionary<byte, DamageType> TranslationIdConversion = TranslationConversion.ToDictionary(x => x.Key.Id, x => x.Value);

    private Dictionary<DamageType, uint> _damageTypes;
    public override List<string> RequiredArgs => ["damages"];

    public override List<string> TriggerOnEvents => ["Hurting"];

    public override bool Validate(out string error)
    {
        return ParseDamages(out error) is not null;
    }

    public override void OnAdded()
    {
        _damageTypes = ParseDamages(out _);
    }

    private Dictionary<DamageType, uint> ParseDamages(out string error)
    {
        error = null;

        if (!Args.TryGetValue("damages", out object raw) || raw is null)
        {
            error = "'damages' is missing. Provide a mapping like 'Firearm: 50' (50% less firearm damage).";
            return null;
        }

        if (raw is Dictionary<DamageType, uint> typed)
            return typed;

        if (raw is not IDictionary map)
        {
            error = $"'damages' must be a mapping of DamageType: reduction%, e.g. 'Firearm: 50'. Got a {raw.GetType().Name}.";
            return null;
        }

        Dictionary<DamageType, uint> result = new();
        foreach (DictionaryEntry entry in map)
        {
            string key = entry.Key?.ToString();
            if (!Enum.TryParse(key, true, out DamageType damageType))
            {
                error = $"'{key}' is not a valid DamageType. Valid values: {string.Join(", ", Enum.GetNames(typeof(DamageType)))}.";
                return null;
            }

            if (!uint.TryParse(entry.Value?.ToString(), out uint reduction) || reduction > 100)
            {
                error = $"the reduction for '{key}' must be a whole number between 0 and 100, got '{entry.Value}'.";
                return null;
            }

            result[damageType] = reduction;
        }

        return result;
    }

    public override bool OnEvent(string name, IPlayerEvent ev)
    {
        if (_damageTypes is null)
            return true;

        if (ev is not PlayerHurtingEventArgs hurting)
            return true;

        if (hurting.DamageHandler is not StandardDamageHandler standardDamageHandler)
            return true;

        DamageType damageType = GetDamageType(hurting.DamageHandler);
        if (_damageTypes.TryGetValue(damageType, out uint reduction))
            standardDamageHandler.Damage *= (100f - reduction) / 100f;

        return true;
    }

    public override void OnRemoved()
    {
        _damageTypes = null;
    }

    private static DamageType GetDamageType(DamageHandlerBase damageHandlerBase)
    {
        switch (damageHandlerBase)
        {
            case CustomReasonDamageHandler:
                return DamageType.Custom;
            case WarheadDamageHandler:
                return DamageType.Warhead;
            case ExplosionDamageHandler:
                return DamageType.Explosion;
            case Scp018DamageHandler:
                return DamageType.Scp018;
            case RecontainmentDamageHandler:
                return DamageType.Recontainment;
            case Scp096DamageHandler:
                return DamageType.Scp096;
            case MicroHidDamageHandler:
                return DamageType.MicroHid;
            case DisruptorDamageHandler:
                return DamageType.ParticleDisruptor;
            case Scp1507DamageHandler:
                return DamageType.Scp1507;
            case Scp956DamageHandler:
                return DamageType.Scp956;
            case SnowballDamageHandler:
                return DamageType.SnowBall;
            case GrayCandyDamageHandler:
                return DamageType.GrayCandy;
            case Scp1509DamageHandler:
                return DamageType.Scp1509;
            case Scp049DamageHandler scp049DamageHandler:
                return scp049DamageHandler.DamageSubType switch
                {
                    Scp049DamageHandler.AttackType.CardiacArrest => DamageType.CardiacArrest,
                    Scp049DamageHandler.AttackType.Instakill => DamageType.Scp049,
                    Scp049DamageHandler.AttackType.Scp0492 => DamageType.Scp0492,
                    _ => DamageType.Unknown
                };
            case Scp3114DamageHandler scp3114DamageHandler:
                return scp3114DamageHandler.Subtype switch
                {
                    Scp3114DamageHandler.HandlerType.Strangulation => DamageType.Strangled,
                    Scp3114DamageHandler.HandlerType.SkinSteal => DamageType.Scp3114,
                    Scp3114DamageHandler.HandlerType.Slap => DamageType.Scp3114,
                    _ => DamageType.Unknown
                };
            case FirearmDamageHandler firearmDamageHandler:
                return ItemConversion.TryGetValue(firearmDamageHandler.WeaponType, out DamageType value) ? value : DamageType.Firearm;

            case ScpDamageHandler scpDamageHandler:
            {
                DeathTranslation translation = DeathTranslations.TranslationsById[scpDamageHandler._translationId];
                if (translation.Id == DeathTranslations.PocketDecay.Id)
                    return DamageType.Scp106;

                return TranslationIdConversion.TryGetValue(translation.Id, out DamageType value1) ? value1 : DamageType.Scp;
            }

            case UniversalDamageHandler universal:
            {
                DeathTranslation translation = DeathTranslations.TranslationsById[universal.TranslationId];

                return TranslationIdConversion.TryGetValue(translation.Id, out DamageType damageType) ? damageType : DamageType.Unknown;
            }
        }

        return DamageType.Unknown;
    }
}