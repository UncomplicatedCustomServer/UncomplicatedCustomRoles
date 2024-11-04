using Exiled.API.Extensions;
using Exiled.API.Features.Pickups;
using System.Linq;
using UncomplicatedCustomRoles.API.Enums;
using UncomplicatedCustomRoles.API.Interfaces;

namespace UncomplicatedCustomRoles.API.Features.CustomModules.ItemBan
{
    internal abstract class ItemBanBase : CustomModule
    {
        public abstract ItemCategory Category { get; }

        public ItemBanBase(SummonedCustomRole role) : base(role) 
        { }

        public void CheckInventory() => Instance?.Player.RemoveItem(item => item.Category == Category);

        public bool ValidatePickup(Pickup pickup) => pickup.Type.GetCategory() != Category;

        public static void CheckInventoryAll(SummonedCustomRole role)
        {
            foreach (ICustomModule module in role.CustomModules.Where(module => module.Name.StartsWith("Ban")))
            {
                if (module is ItemBanBase itemBan)
                    itemBan.CheckInventory();
            }
        }

        public static bool CheckPickup(SummonedCustomRole role, Pickup pickup)
        {
            ICustomModule module = role.CustomModules.FirstOrDefault(m => m.Name.StartsWith("Ban") && m is ItemBanBase itemBan && itemBan.Category == pickup.Type.GetCategory());
            if (module is not null && module is ItemBanBase itemBan)
                return itemBan.ValidatePickup(pickup);

            return true; // Poco restrittivi
        }
    }

    internal class BanKeycards : ItemBanBase
    {
        public new static CustomFlags Flag => CustomFlags.BanKeycards;

        public override ItemCategory Category => ItemCategory.Keycard;

        public BanKeycards(SummonedCustomRole role) : base(role) 
        { }
    }

    internal class BanMedicals : ItemBanBase
    {
        public new static CustomFlags Flag => CustomFlags.BanMedicals;

        public override ItemCategory Category => ItemCategory.Medical;

        public BanMedicals(SummonedCustomRole role) : base(role)
        { }
    }

    internal class BanRadios : ItemBanBase
    {
        public new static CustomFlags Flag => CustomFlags.BanRadios;

        public override ItemCategory Category => ItemCategory.Radio;

        public BanRadios(SummonedCustomRole role) : base(role)
        { }
    }

    internal class BanFirearms : ItemBanBase
    {
        public new static CustomFlags Flag => CustomFlags.BanFirearms;

        public override ItemCategory Category => ItemCategory.Firearm;

        public BanFirearms(SummonedCustomRole role) : base(role)
        { }
    }

    internal class BanGrenades : ItemBanBase
    {
        public new static CustomFlags Flag => CustomFlags.BanGrenades;

        public override ItemCategory Category => ItemCategory.Grenade;

        public BanGrenades(SummonedCustomRole role) : base(role)
        { }
    }

    internal class BanSCPItems : ItemBanBase
    {
        public new static CustomFlags Flag => CustomFlags.BanSCPItems;

        public override ItemCategory Category => ItemCategory.SCPItem;

        public BanSCPItems(SummonedCustomRole role) : base(role)
        { }
    }

    internal class BanMicroHID : ItemBanBase
    {
        public new static CustomFlags Flag => CustomFlags.BanMicroHID;

        public override ItemCategory Category => ItemCategory.MicroHID;

        public BanMicroHID(SummonedCustomRole role) : base(role)
        { }
    }

    internal class BanArmors : ItemBanBase
    {
        public new static CustomFlags Flag => CustomFlags.BanArmors;

        public override ItemCategory Category => ItemCategory.Armor;

        public BanArmors(SummonedCustomRole role) : base(role)
        { }
    }
}
