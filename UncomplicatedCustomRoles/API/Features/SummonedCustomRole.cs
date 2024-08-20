using Exiled.API.Features;
using PlayerRoles;
using PlayerRoles.PlayableScps;
using System;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomRoles.API.Interfaces;
using UncomplicatedCustomRoles.API.Struct;
using UncomplicatedCustomRoles.Commands;
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
        /// Gets if the current SummonedCustomRole is valid or not
        /// </summary>
        public bool IsValid => _internalValid && Player.IsAlive;

        /// <summary>
        /// The unique identifier for this instance of <see cref="SummonedCustomRole"/>
        /// </summary>
        public string Id { get; }

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
        /// Gets the <see cref="CustomRoleEventHandler"/> instance of the current <see cref="SummonedCustomRole"/> instance
        /// </summary>
        public CustomRoleEventHandler EventHandler { get; }

        /// <summary>
        /// Gets or sets the number of candies taken by this player as this <see cref="ICustomRole"/>
        /// </summary>
        public uint Scp330Count { get; internal set; } = 0;

        /// <summary>
        /// Gets the original <see cref="PlayerInfoArea"/> of the player
        /// </summary>
        public PlayerInfoArea PlayerInfoArea { get; }

        /// <summary>
        /// Gets whether the current <see cref="ICustomRole"/> has a different team base with a different <see cref="PlayerRoleBase"/>
        /// </summary>
        public bool IsOverwrittenRole => _roleBase is not null;

        private PlayerRoleBase _roleBase { get; set; } = null;

        private bool _internalValid { get; set; }

        internal SummonedCustomRole(Player player, ICustomRole role, Triplet<string, string, bool>? badge, List<IEffect> infiniteEffects, PlayerInfoArea playerInfo, bool isCustomNickname = false)
        {
            Id = Guid.NewGuid().ToString();
            Player = player;
            Role = role;
            SpawnTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            Badge = badge;
            InfiniteEffects = infiniteEffects;
            IsCustomNickname = isCustomNickname;
            _internalValid = true;
            EvaluateRoleBase();
            EventHandler = new(this);
            List.Add(this);
        }

        /// <summary>
        /// Try to set <see cref="_roleBase"/> in order to override the current Player.Role.Base to trick the server into thinking that the player is / is not an Human
        /// </summary>
        private void EvaluateRoleBase()
        {
            if (Role.Role.GetTeam() != Role.Team && Role.Role.GetTeam() is not Team.SCPs && Role.Team is Team.SCPs)
                _roleBase = Player.Role.Base as FpcStandardScp;
            else if (Role.Role.GetTeam() != Role.Team && Role.Role.GetTeam() is Team.SCPs && Role.Team is not Team.SCPs)
                _roleBase = Player.Role.Base as PlayerRoles.HumanRole;
            SpawnManager.UpdateChaosModifier();
        }

        /// <summary>
        /// Remove the SummonedCustomRole from the list by destroying it!
        /// </summary>
        public void Destroy()
        {
            Remove();
            _internalValid = false;
            List.Remove(this);
            SpawnManager.UpdateChaosModifier();
        }

        /// <summary>
        /// Remove the current CustomRole from the player without destroying the instance
        /// </summary>
        public void Remove()
        {
            if (Role.BadgeName is not null && Role.BadgeName.Length > 1 && Role.BadgeColor is not null && Role.BadgeColor.Length > 2 && Badge is not null && Badge is Triplet<string, string, bool> badge)
            {
                Player.RankName = badge.First;
                Player.RankColor = badge.Second;
                Player.ReferenceHub.serverRoles.RefreshLocalTag();

                LogManager.Debug($"Badge detected, fixed");
            }

            Player.ReferenceHub.nicknameSync.Network_playerInfoToShow = PlayerInfoArea;
            Player.IsUsingStamina = true;
            Player.CustomInfo = string.Empty;

            LogManager.Debug("Scale reset to 1, 1, 1");
            Player.Scale = new(1, 1, 1);

            if (IsCustomNickname)
            {
                Player.DisplayNickname = null;
            }
        }

        /// <summary>
        /// Gets every <see cref="SummonedCustomRole"/> with the same <see cref="ICustomRole"/> as a <see cref="List{T}"/>
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public static List<SummonedCustomRole> Get(ICustomRole role) => List.Where(scr => scr.Role == role).ToList();

        /// <summary>
        /// Gets a <see cref="SummonedCustomRole"/> instance by the <see cref="Exiled.API.Features.Player"/>
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static SummonedCustomRole Get(Player player) => List.Where(scr => scr.Player.Id == player.Id).FirstOrDefault();

        /// <summary>
        /// Gets a <see cref="SummonedCustomRole"/> instance by the <see cref="ReferenceHub"/>
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static SummonedCustomRole Get(ReferenceHub player) => List.Where(scr => scr.Player.Id == player.PlayerId).FirstOrDefault();

        /// <summary>
        /// Gets a <see cref="SummonedCustomRole"/> instance by the Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static SummonedCustomRole Get(string id) => List.Where(scr => scr.Id == id).FirstOrDefault();

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

        /// <summary>
        /// Try to get the custom <see cref="Team"/> of the <see cref="ICustomRole"/> of the found <see cref="SummonedCustomRole"/>
        /// </summary>
        /// <param name="player"></param>
        /// <param name="team"></param>
        /// <returns></returns>
        public static bool TryPatchCustomRole(ReferenceHub player, out Team team)
        {
            if (player is not null && TryGet(player, out SummonedCustomRole customRole) && customRole.Role.Team is not null && customRole.Role.Team != customRole.Role.Role.GetTeam())
            {
                team = (Team)customRole.Role.Team;
                return true;
            }

            team = player?.GetRoleId().GetTeam() ?? Team.OtherAlive;
            return false;
        }

        /// <summary>
        /// Try to get the custom <see cref="PlayerRoleBase"/> of the <see cref="ICustomRole"/> of the found <see cref="SummonedCustomRole"/> and override it only if necessary
        /// </summary>
        /// <param name="player"></param>
        /// <param name="roleBase"></param>
        /// <returns></returns>
        public static bool TryPatchRoleBase(ReferenceHub player, out PlayerRoleBase roleBase)
        {
            if (player is not null && TryGet(player, out SummonedCustomRole customRole) && customRole._roleBase is not null)
            {
                roleBase = customRole._roleBase;
                return true;
            }

            roleBase = null;
            return false;
        }

        /// <summary>
        /// Try to check if the custom <see cref="Team"/> of the <see cref="ICustomRole"/> of the found <see cref="SummonedCustomRole"/> is equal to the given <see cref="Team"/>
        /// </summary>
        /// <param name="player"></param>
        /// <param name="team"></param>
        /// <returns></returns>
        public static bool TryCheckForCustomTeam(ReferenceHub player, Team teamCheck, out bool result)
        {
            if (TryPatchCustomRole(player, out Team customTeam))
            {
                result = customTeam == teamCheck;
                return true;
            }

            result = false;
            return false;
        }

        /// <summary>
        /// Try to get the custom <see cref="Team"/> of the <see cref="ICustomRole"/> of the found <see cref="SummonedCustomRole"/>, otherwise return the given default <see cref="Team"/>
        /// </summary>
        /// <param name="player"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        public static Team TryGetCusomTeam(ReferenceHub player, Team? def = null)
        {
            if (TryGet(player, out SummonedCustomRole customRole) && customRole.Role.Team is not null && customRole.Role.Team != customRole.Role.Role.GetTeam())
                return (Team)customRole.Role.Team;

            return def ?? player.GetRoleId().GetTeam();
        }

        internal static void InfiniteEffectActor()
        {
            foreach (SummonedCustomRole Role in List)
                if (Role.InfiniteEffects.Count() > 0)
                    foreach (IEffect Effect in Role.InfiniteEffects)
                        if (!Role.Player.ActiveEffects.Contains(Role.Player.GetEffect(Effect.EffectType)))
                            Role.Player.EnableEffect(Effect.EffectType, Effect.Intensity, float.MaxValue);
        }
    }
}
