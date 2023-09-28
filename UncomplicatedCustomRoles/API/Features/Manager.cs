using Exiled.API.Features;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
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
            if (Plugin.PlayerRegistry.ContainsKey(Player.Id))
            {
                return true;
            }
            return false;
        }
       public static bool IsRegistered(int Id)
        {
            if (Plugin.CustomRoles.ContainsKey(Id))
            {
                return true;
            }
            return false;
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
        public static int Count(ICustomRole Role)
        {
            return Plugin.RolesCount[Role.Id];
        }
        public static int Count(int Role)
        {
            return Plugin.RolesCount[Role];
        }
        public static int Count()
        {
            int total = 0;
            foreach (KeyValuePair<int, int> Count in Plugin.RolesCount)
            {
                total += Count.Value;
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
    }
}
