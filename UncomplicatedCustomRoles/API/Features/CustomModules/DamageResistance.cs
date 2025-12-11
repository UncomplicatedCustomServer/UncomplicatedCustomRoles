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
using System.Linq;
using InventorySystem.Items.Scp1509;
using LabApi.Events.Arguments.Interfaces;
using LabApi.Events.Arguments.PlayerEvents;
using PlayerRoles.PlayableScps.Scp1507;
using PlayerRoles.PlayableScps.Scp3114;
using PlayerStatsSystem;
using UncomplicatedCustomRoles.API.Enums;

namespace UncomplicatedCustomRoles.API.Features.CustomModules
{
    internal class DamageResistance : CustomModule
    {
        public override List<string> RequiredArgs => new()
        {
            "damages"
        };

        public override List<string> TriggerOnEvents => new()
        {
            "Hurting"
        };

        internal Dictionary<DamageType, uint> DamageTypes = null;

        public override void OnAdded()
        {
            DamageTypes = TryGetValue("damages", new Dictionary<DamageType, uint>()) as Dictionary<DamageType, uint>;

            if (DamageTypes is null)
                ThrowError($"DamageResistance CustomFlag/CustomModule expected a Dictionary<DamageType, uint> in 'damages', got a {TryGetValue("damages", null)?.GetType().FullName}");
        }

        public override bool OnEvent(string name, IPlayerEvent ev)
        {
            if (DamageTypes is null)
                return true;

            if (ev is not PlayerHurtingEventArgs hurting)
                return true;

            if (hurting.DamageHandler is not StandardDamageHandler standardDamageHandler)
                return true;
            
            DamageType damageType = GetDamageType(hurting.DamageHandler);
            if (DamageTypes.TryGetValue(damageType, out uint reduction))
            {
                standardDamageHandler.Damage *= (100f - reduction) / 100f;
            }

            return true;
        }

        public override void OnRemoved()
        {
            DamageTypes = null;
        }
        
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
            { ItemType.GunSCP127, DamageType.Scp127 },
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
            { DeathTranslations.Scp1509, DamageType.Scp1509 },
        };
        
        private static readonly Dictionary<byte, DamageType> TranslationIdConversion = TranslationConversion.ToDictionary(x => x.Key.Id, x => x.Value);
        
        public static DamageType GetDamageType(DamageHandlerBase damageHandlerBase)
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
                        _ => DamageType.Unknown,
                    };
                case Scp3114DamageHandler scp3114DamageHandler:
                    return scp3114DamageHandler.Subtype switch
                    {
                        Scp3114DamageHandler.HandlerType.Strangulation => DamageType.Strangled,
                        Scp3114DamageHandler.HandlerType.SkinSteal => DamageType.Scp3114,
                        Scp3114DamageHandler.HandlerType.Slap => DamageType.Scp3114,
                        _ => DamageType.Unknown,
                    };
                case FirearmDamageHandler firearmDamageHandler:
                    return ItemConversion.TryGetValue(firearmDamageHandler.WeaponType, out var value) ? value : DamageType.Firearm;

                case ScpDamageHandler scpDamageHandler:
                    {
                        DeathTranslation translation = DeathTranslations.TranslationsById[scpDamageHandler._translationId];
                        if (translation.Id == DeathTranslations.PocketDecay.Id)
                            return DamageType.Scp106;

                        return TranslationIdConversion.TryGetValue(translation.Id, out var value1)
                            ? value1
                            : DamageType.Scp;
                    }

                case UniversalDamageHandler universal:
                    {
                        DeathTranslation translation = DeathTranslations.TranslationsById[universal.TranslationId];

                        return TranslationIdConversion.TryGetValue(translation.Id, out var damageType) ? damageType : DamageType.Unknown;
                    }
            }

            return DamageType.Unknown;
        }
    }
}