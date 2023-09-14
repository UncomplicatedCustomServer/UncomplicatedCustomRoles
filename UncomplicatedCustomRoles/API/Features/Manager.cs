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
        public static int? Get(Player Player)
        {
            if (HasCustomRole(Player))
            {
                return Plugin.PlayerRegistry[Player.Id];
            }
            return null;
        }
        public static ICustomRole Get(int Id)
        {
            return Plugin.CustomRoles[Id];
        }
        public static void SummonRole(Player Player, int Id)
        {
            EventHandler.DoSpawnPlayer(Player, Id);
        }
        public static void Register(ICustomRole Role)
        {
            SpawnManager.RegisterCustomSubclass(Role);
        }
    }
}
