using Exiled.API.Features;
using System.Collections.Generic;
using UncomplicatedCustomRoles.Manager;
using UncomplicatedCustomRoles.Structures;
using UncomplicatedCustomRoles.Events;

namespace UncomplicatedCustomRoles.API.Features
{
#nullable enable
    public partial class Manager
    {
        public static Dictionary<int, ICustomRole> GetList()
        {
            return Plugin.CustomRoles;
        }
        public static bool HasCustomRole(Player Player)
        {
            return Plugin.PlayerRegistry.ContainsKey(Player.Id);
        }
       public static bool IsRegistered(int Id)
        {
            return Plugin.CustomRoles.ContainsKey(Id);
        }
        public static ICustomRole Get(int Id)
        {
            return Plugin.CustomRoles[Id];
        }
        public static void Summon(Player Player, int Id)
        {
            EventHandler.DoSpawnPlayer(Player, Id);
        }
        public static void Summon(Player Player, ICustomRole Role)
        {
            Summon(Player, Role.Id);
        }
        public static void Register(ICustomRole Role)
        {
            SpawnManager.RegisterCustomSubclass(Role);
        }
        public static ICustomRole? Get(Player Player)
        {
            if (HasCustomRole(Player))
            {
                return Get(Plugin.PlayerRegistry[Player.Id]);
            }
            return null;
        }
        public static bool TryGet(Player Player, out ICustomRole? Role)
        {
            Role = Get(Player);
            if (Role is null)
            {
                return false;
            }
            return true;
        }
        public static int Count(ICustomRole Role)
        {
            return Plugin.RolesCount[Role.Id].Count;
        }
        public static int Count(int Role)
        {
            return Plugin.RolesCount[Role].Count;
        }
        public static int Count()
        {
            int total = 0;
            foreach (KeyValuePair<int, List<int>> Count in Plugin.RolesCount)
            {
                total += Count.Value.Count;
            }
            return total;
        }
        public static void Unregister(int Role)
        {
            if (IsRegistered(Role))
            {
                Plugin.CustomRoles.Remove(Role);
            }
        }
        public static void Unregister(ICustomRole Role)
        {
            Unregister(Role.Id);
        }
        public static List<IUCREffect>? InfiniteEffects(int Id)
        {
            if (Plugin.PermanentEffectStatus.ContainsKey(Id))
            {
                return Plugin.PermanentEffectStatus[Id];
            }
            return null;
        }
        public static List<IUCREffect>? InfiniteEffects(Player Player)
        {
            return InfiniteEffects(Player.Id);
        }
        public static bool AddInfiniteEffect(IUCREffect Effect, int Id)
        {
            if (Plugin.PlayerRegistry.ContainsKey(Id) && Plugin.PermanentEffectStatus.ContainsKey(Id))
            {
                Plugin.PermanentEffectStatus[Id].Add(Effect);
                return true;
            }
            return false;
        }
        public static bool AddInfiniteEffect(IUCREffect Effect, Player Player)
        {
            return AddInfiniteEffect(Effect, Player.Id);
        }
    }
}
