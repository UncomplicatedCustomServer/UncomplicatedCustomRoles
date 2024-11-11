using Exiled.API.Features;
using HarmonyLib;
using MEC;
using PlayerRoles;
using PlayerRoles.PlayableScps;
using System;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomRoles.API.Enums;
using UncomplicatedCustomRoles.API.Features.CustomModules;
using UncomplicatedCustomRoles.API.Features.CustomModules.ItemBan;
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
        public Triplet<string, string, bool>? Badge { get; private set; }

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
        /// Gets the <see cref="CoroutineHandle"/> of a generic Coroutine that can be used by the custom role manager
        /// </summary>
        public CoroutineHandle GenericCoroutine { get; private set; }

        /// <summary>
        /// Gets the <see cref="CustomActions"/> <see cref="List{T}"/> where you'll be able to add custom actions that will be executed during the <see cref="GenericCoroutine"/> execution.<br></br>
        /// You must return a <see cref="bool"/>: if false the coroutine will skip the precoded actions
        /// </summary>
        public List<Func<SummonedCustomRole, bool>> CustomActions { get; } = new();

        /// <summary>
        /// Gets whether the current <see cref="ICustomRole"/> has a different team base with a different <see cref="PlayerRoleBase"/>
        /// </summary>
        public bool IsOverwrittenRole => _roleBase is not null;

        /// <summary>
        /// Gets whether the current <see cref="SummonedCustomRole"/> implements a coroutine for handling basic plugin features
        /// </summary>
        public bool IsDefaultCoroutineRole => (Role.Health?.HumeShield ?? 0) > 0 && (Role.Health?.HumeShieldRegenerationAmount ?? 0) > 0;

        /// <summary>
        /// Gets whether the current <see cref="ICustomRole"/> implements a custom coroutine
        /// </summary>
        public bool IsCoroutineRole => Role is ICoroutineRole;

        /// <summary>
        /// Gets if the current SummonedCustomRole is valid or not
        /// </summary>
        public bool IsValid => _internalValid && Player.IsAlive;

        /// <summary>
        /// Gets whether the current <see cref="Player"/> is a UCS Employee
        /// </summary>
        public bool IsEmployee => Plugin.HttpManager.OrgPlayerRole.ContainsKey(Player.UserId);

        /// <summary>
        /// Gets the time in UNIX timestamp (seconds) when the <see cref="Player"/> received the last damage
        /// </summary>
        public long LastDamageTime { get; internal set; }

        /// <summary>
        /// Gets a <see cref="IReadOnlyCollection{T}"/> of every installed <see cref="CustomModule"/>
        /// </summary>
        public IReadOnlyCollection<ICustomModule> CustomModules => _customModules;

        private PlayerRoleBase _roleBase { get; set; } = null;

        private bool _internalValid { get; set; }

        private bool _isRegeneratingHume { get; set; }

        private List<ICustomModule> _customModules { get; }

        /// <summary>
        /// The duration of a tick
        /// </summary>
        public const float TickDuration = 0.25f;

        internal SummonedCustomRole(Player player, ICustomRole role, Triplet<string, string, bool>? badge, List<IEffect> infiniteEffects, PlayerInfoArea playerInfo, bool isCustomNickname = false)
        {
            Id = Guid.NewGuid().ToString();
            Player = player;
            Role = role;
            SpawnTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            Badge = badge;
            InfiniteEffects = infiniteEffects;
            IsCustomNickname = isCustomNickname;
            PlayerInfoArea = playerInfo;
            _internalValid = true;

            if (IsDefaultCoroutineRole)
                GenericCoroutine = Timing.RunCoroutine(RoleTickCoroutine());

            _customModules = CustomModule.Load(Role.CustomFlags ?? CustomFlags.None, this);

            if (Role is ICoroutineRole coroutineRole)
                if (coroutineRole.TickRate > 0)
                {
                    coroutineRole.CoroutineHandler = Timing.RunCoroutine(CoroutineRoleCoroutine());
                    coroutineRole.Frame = -1;
                }

            foreach (CoroutineModule coroutineModule in GetModules<CoroutineModule>())
                coroutineModule.Execute();

            EvaluateRoleBase();

            ItemBanBase.CheckInventoryAll(this);

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
                _roleBase = Player.Role.Base as HumanRole;
            SpawnManager.UpdateChaosModifier();
        }

        /// <summary>
        /// Runs every custom action in <see cref="CustomActions"/> and evaluate their results
        /// </summary>
        /// <returns></returns>
        private bool EvaluateCustomActions()
        {
            bool _result = false;
            foreach (Func<SummonedCustomRole, bool> func in CustomActions)
                _result &= func(this);
            return _result;
        }

        /// <summary>
        /// Remove the SummonedCustomRole from the list by destroying it!
        /// </summary>
        public void Destroy()
        {
            LogManager.Silent($"Destroying instance of CR {Role.Id} of PL {Player}");
            Remove();
            List.Remove(this);
            SpawnManager.UpdateChaosModifier();
        }

        /// <summary>
        /// Remove the current CustomRole from the player without destroying the instance
        /// </summary>
        public void Remove()
        {
            try
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
                Player.ReferenceHub.nicknameSync.Network_customPlayerInfoString = string.Empty;

                LogManager.Debug("Scale reset to 1, 1, 1");
                Player.Scale = new(1, 1, 1);

                if (IsCustomNickname)
                {
                    Player.DisplayNickname = null;
                }

                if (IsDefaultCoroutineRole && GenericCoroutine.IsRunning)
                    Timing.KillCoroutines(GenericCoroutine);

                if (Role is ICoroutineRole role && role.CoroutineHandler.IsRunning)
                    Timing.KillCoroutines(role.CoroutineHandler);

                foreach (CoroutineModule coroutineModule in GetModules<CoroutineModule>())
                    if (coroutineModule.CoroutineHandler.IsRunning)
                        Timing.KillCoroutines(coroutineModule.CoroutineHandler);
            }
            catch (Exception e)
            {
                LogManager.Error($"Failed to act SummonedCustomRole::Remove() - {e.GetType().FullName}: {e.Message}\n{e.StackTrace}");
            }

            _customModules.Clear();
            _internalValid = false;
        }

        /// <summary>
        /// If the role is <see cref="IsDfaultCoroutineRole"/> this coroutine will handle every functions that requires one
        /// </summary>
        /// <returns></returns>
        private IEnumerator<float> RoleTickCoroutine()
        {
            while (_internalValid && Player.IsAlive && IsDefaultCoroutineRole)
            {
                if (EvaluateCustomActions() && Player.HumeShield < Role.Health.HumeShield && DateTimeOffset.UtcNow.ToUnixTimeSeconds() - LastDamageTime >= Role.Health.HumeShieldRegenerationDelay && !_isRegeneratingHume)
                    Timing.RunCoroutine(HumeShieldCoroutine());

                yield return Timing.WaitForSeconds(TickDuration);
            }
        }

        /// <summary>
        /// The custom coroutine actor for the <see cref="ICoroutineRole"/>
        /// </summary>
        /// <returns></returns>
        private IEnumerator<float> CoroutineRoleCoroutine()
        {
            while (_internalValid && Player.IsAlive && Role is ICoroutineRole coroutineRole)
            {
                coroutineRole.Frame++;
                coroutineRole.Tick(this);

                yield return Timing.WaitForSeconds(coroutineRole.TickRate);
            }
        }

        /// <summary>
        /// Parse the current <see cref="SummonedCustomRole"/> instance as a RemoteAdmin text part
        /// </summary>
        /// <returns></returns>
        internal string ParseRemoteAdmin() => $"\n<size=26><color=#f55505>UncomplicatedCustomRoles</color></size>\nCustom Role: <color={Exiled.API.Extensions.RoleExtensions.GetColor(Role.Role).ToHex()}>{Role.Name}</color> [Id={Role.Id}]{LoadRoleFlags()}\n{LoadBadge()}";

        private string LoadRoleFlags()
        {
            List<string> output = new();

            if (IsCoroutineRole || IsDefaultCoroutineRole)
                output.Add("<color=#599e09>[COROUTINE]</color>");

            if (_customModules.Count > 0)
                output.Add("<color=#a343f7>[CUSTOM MODULES]</color>");

            if (Role.Role.GetTeam() != (Role?.Team ?? Role.Role.GetTeam()))
                output.Add("<color=#eb441e>[TEAM OVERRIDE]</color>");

            if (output.Count > 0)
            {
                output.Insert(0, "                                ");
            }

            return string.Join(" ", output);
        }

        private string LoadBadge()
        {
            string output = "Badge: ";

            if (Role.BadgeColor != string.Empty && Role.BadgeName != string.Empty)
                if (SpawnManager.colorMap.ContainsKey(Role.BadgeColor))
                    output += $"<color={SpawnManager.colorMap[Role.BadgeColor]}>{Role.BadgeName}</color>";
                else
                    output += $"{Role.BadgeName}";
            else
                output += "None";

            if (Plugin.HttpManager.Credits.TryGetValue(Player.UserId, out Triplet<string, string, bool> tag))
                if (IsEmployee)
                    output += $"                                 <color=#168eba>[UCR EMPLOYEE]</color> <color={SpawnManager.colorMap[tag.Second]}>{tag.First}</color>";
                else
                    output += $"                                 <color=#168eba>[UCR CONTRIBUTOR]</color> <color={SpawnManager.colorMap[tag.Second]}>{tag.First}</color>";

            return output;
        }

        /// <summary>
        /// The coroutine to regenerate Hume Shield
        /// </summary>
        /// <returns></returns>
        public IEnumerator<float> HumeShieldCoroutine()
        {
            _isRegeneratingHume = true;
            while (_internalValid && Player.IsAlive && Player.HumeShield < Role.Health.HumeShield && DateTimeOffset.UtcNow.ToUnixTimeSeconds() - LastDamageTime >= Role.Health.HumeShieldRegenerationDelay)
            {
                Player.HumeShield += Role.Health.HumeShieldRegenerationAmount;
                yield return Timing.WaitForSeconds(1f);
            }
            _isRegeneratingHume = false;
        }

        /// <summary>
        /// Gets a <see cref="CustomModule"/> that this custom role implements
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetModule<T>() where T : CustomModule => _customModules.Where(cm => cm.GetType() == typeof(T)).FirstOrDefault() as T;

        /// <summary>
        /// Gets a <see cref="CustomModule"/> array that contains every custom module with the same type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T[] GetModules<T>() where T : CustomModule
        {
            T[] result = new T[] { };
            foreach (ICustomModule module in _customModules.Where(cm => cm.GetType() == typeof(T)))
                result.AddItem(module);
            return result;
        }

        /// <summary>
        /// Try to get a <see cref="CustomModule"/> if its implemented
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="module"></param>
        /// <returns></returns>
        public bool GetModule<T>(out T module) where T : CustomModule
        {
            module = GetModule<T>();
            return module != null;
        }

        /// <summary>
        /// Gets if the current <see cref="SummonedCustomRole"/> implements the given <see cref="CustomModule"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool HasModule<T>() where T : CustomModule => _customModules.Any(cm => cm.GetType() == typeof(T));

        /// <summary>
        /// Add a new <see cref="CustomModule"/> to the current <see cref="SummonedCustomRole"/> instance
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void AddModule<T>() where T : CustomModule => _customModules.Add(CustomModule.Load(typeof(T), this));

        /// <summary>
        /// Try to remove the first <see cref="CustomModule"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void RemoveModule<T>() where T : CustomModule
        {
            if (GetModule(out T module))
            {
                if (module is CoroutineModule coroutineModule && coroutineModule.CoroutineHandler.IsRunning)
                    Timing.KillCoroutines(coroutineModule.CoroutineHandler);
                _customModules.Remove(module);
            }
        }

        /// <summary>
        /// Remove every <see cref="CustomModule"/> with the same given type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void RemoveModules<T>() where T : CustomModule
        {
            foreach (ICustomModule _ in GetModules<T>())
                RemoveModule<T>();
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

        /// <summary>
        /// Try to get the Remote Admin text from a <see cref="ReferenceHub"/>
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static string TryParseRemoteAdmin(ReferenceHub player)
        {
            if (TryGet(player, out SummonedCustomRole role))
                return role.ParseRemoteAdmin();
            return "\nCustom Role: None";
        }

        /// <summary>
        /// Handle the infinite effects for every <see cref="SummonedCustomRole"/> instance
        /// </summary>
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
