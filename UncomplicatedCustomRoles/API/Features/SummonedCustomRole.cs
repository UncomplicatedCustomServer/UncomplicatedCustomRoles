using CustomPlayerEffects;
using Mirror;
using PluginAPI.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomRoles.API.Struct;
using UncomplicatedCustomRoles.Commands;
using UncomplicatedCustomRoles.Interfaces;
using UncomplicatedCustomRoles.Manager;

namespace UncomplicatedCustomRoles.API.Features
{
#pragma warning disable IDE1006 // Stili di denominazione
    public class SummonedCustomRole
    {
        /// <summary>
        /// Gets every <see cref="SummonedCustomRole"/>
        /// </summary>
        public static List<SummonedCustomRole> List { get; } = new();

        /// <summary>
        /// Gets the summon queue in order to keep trace of summoning players
        /// </summary>
        internal static List<int> SummonQueue { get; } = new();

        /// <summary>
        /// Gets if the current SummonedCustomRole is valid or not
        /// </summary>
        public bool IsValid => _InternalValid && Player.IsAlive;

        /// <summary>
        /// Gets the <see cref="Exiled.API.Features.Player"/>
        /// </summary>
        public Player Player { get; }

        /// <summary>
        /// Gets the <see cref="Player"/>'s <see cref="ICustomRole"/>
        /// </summary>
        public ICustomRole Role { get; }

        /// <summary>
        /// Gets the UNIX timestamp when the player spawned
        /// </summary>
        public long SpawnTime { get; }

        /// <summary>
        /// Gets the badge of the player if it has one
        /// </summary>
        public Triplet<string, string, bool>? Badge { get; internal set; }

        /// <summary>
        /// Gets the list of infinite <see cref="IEffect"/>
        /// </summary>
        public List<IEffect> InfiniteEffects { get; }
        
        /// <summary>
        /// Gets the current nickname of the player - if null the role didn't changed it!
        /// </summary>
        public bool IsCustomNickname { get; }

        /// <summary>
        /// Gets or sets the number of candies taken by this player as this <see cref="ICustomRole"/>
        /// </summary>
        public uint Scp330Count { get; internal set; } = 0;

        private bool _InternalValid { get; set; }

        public SummonedCustomRole(Player player, ICustomRole role, Triplet<string, string, bool>? badge, List<IEffect> infiniteEffects, bool isCustomNickname = false)
        {
            Player = player;
            Role = role;
            SpawnTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            Badge = badge;
            InfiniteEffects = infiniteEffects;
            IsCustomNickname = isCustomNickname;
            _InternalValid = true;
            List.Add(this);
        }

        /// <summary>
        /// Remove the SummonedCustomRole from the list by destroying it!
        /// </summary>
        public void Destroy()
        {
            Remove();
            _InternalValid = false;
            List.Remove(this);
        }

        public void Remove()
        {
            if (Role.BadgeName is not null && Role.BadgeName.Length > 1 && Role.BadgeColor is not null && Role.BadgeColor.Length > 2 && Badge is not null && Badge is Triplet<string, string, bool> badge)
            {
                Player.ReferenceHub.serverRoles.SetText(badge.First);
                Player.ReferenceHub.serverRoles.SetColor(badge.Second);
                Player.ReferenceHub.serverRoles.RefreshLocalTag();

                LogManager.Debug($"Badge detected, fixed");
            }

            Player.CustomInfo = string.Empty;

            LogManager.Debug("Scale reset to 1, 1, 1");
            Player.ReferenceHub.transform.localScale = new(1, 1, 1);

            // Thanks to EXILED!
            foreach (Player player in Player.GetPlayers())
                NetworkServer.SendSpawnMessage(player.ReferenceHub.networkIdentity, player.Connection);

            if (IsCustomNickname)
                Player.DisplayNickname = null;
        }

        /// <summary>
        /// Gets every <see cref="SummonedCustomRole"/> with the same <see cref="ICustomRole"/> as a <see cref="List{T}"/>
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public static List<SummonedCustomRole> Get(ICustomRole role) => List.Where(scr => scr.Role == role).ToList();

        /// <summary>
        /// Gets a <see cref="SummonedCustomRole"/> by the <see cref="Exiled.API.Features.Player"/>
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static SummonedCustomRole Get(Player player) => List.Where(scr => scr.Player.PlayerId == player.PlayerId).FirstOrDefault();

        /// <summary>
        /// Gets a <see cref="SummonedCustomRole"/> by the <see cref="ReferenceHub"/>
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static SummonedCustomRole Get(ReferenceHub player) => List.Where(scr => scr.Player.PlayerId == player.PlayerId).FirstOrDefault();

        /// <summary>
        /// Try to get a <see cref="SummonedCustomRole"/> by the <see cref="Exiled.API.Features.Player"/>
        /// </summary>
        /// <param name="player"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        public static bool TryGet(Player player, out SummonedCustomRole role)
        {
            role = Get(player);
            return role != null;
        }

        /// <summary>
        /// Try to get a <see cref="SummonedCustomRole"/> by the <see cref="ReferenceHub"/>
        /// </summary>
        /// <param name="player"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        public static bool TryGet(ReferenceHub player, out SummonedCustomRole role)
        {
            role = Get(player);
            return role != null;
        }

        /// <summary>
        /// Gets the number of <see cref="SummonedCustomRole"/> with the same <see cref="ICustomRole"/>
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public static int Count(ICustomRole role) => List.Where(scr => scr.Role == role).Count();

        /// <summary>
        /// Gets the number of <see cref="SummonedCustomRole"/> with the same Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static int Count(int id) => List.Where(scr => scr.Role.Id == id).Count();

        /// <summary>
        /// Summon a new instance of <see cref="SummonedCustomRole"/> by spawning a player
        /// </summary>
        /// <param name="player"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        public static SummonedCustomRole Summon(Player player, ICustomRole role)
        {
            if (role.SpawnSettings is not null)
                SpawnManager.SummonCustomSubclass(player, role.Id);
            else
                SpawnManager.SummonSubclassApplier(player, role);

            return Get(player);
        }

        internal static void InfiniteEffectActor()
        {
            foreach (SummonedCustomRole Role in List)
                if (Role.InfiniteEffects.Count() > 0)
                    foreach (IEffect Effect in Role.InfiniteEffects)
                        if (!Role.Player.ReferenceHub.playerEffectsController.AllEffects.Contains(Role.Player.EffectsManager.GetEffect<SeveredHands>()))
                            Role.Player.EffectsManager.EnableEffect<SeveredHands>(Effect.Intensity, false);
        }

        public static int TryGetInventoryLimitForGivenCategory(ItemCategory category, ReferenceHub player, int original)
        {
            SummonedCustomRole Role = List.Where(scr => scr.Player.PlayerId == player.PlayerId).FirstOrDefault();

            if (Role is null)
                return original;

            LogManager.Info($"Player {player.PlayerId} is customrole");

            if (Role.Role.CustomInventoryLimits is null || !Role.Role.CustomInventoryLimits.ContainsKey(category))
                return original;

            LogManager.Info($"Put maximum: {Role.Role.CustomInventoryLimits[category]} for {category} instead of {original}");
            return Role.Role.CustomInventoryLimits[category];
        }

#nullable enable
        public static bool EvaluateInventoryLimit(ItemCategory category, ReferenceHub player, int count, sbyte categoryCount)
        {
            LogManager.Info($"Player {player.PlayerId} might be a customRole, {count}, {categoryCount}");
            SummonedCustomRole Role = List.Where(scr => scr.Player.PlayerId == player.PlayerId).FirstOrDefault();

            if (Role is null || Role.Role.CustomInventoryLimits is null || !Role.Role.CustomInventoryLimits.ContainsKey(category))
                return count >= categoryCount;

            LogManager.Info($"Updated categoryCount to {Role.Role.CustomInventoryLimits[category]}");
            return count >= Role.Role.CustomInventoryLimits[category];
        }
    }
}
