using Exiled.API.Enums;
using InventorySystem.Items.Firearms.Attachments;
using System.Collections.Generic;
using UnityEngine;

#nullable enable
namespace UncomplicatedCustomRoles.Structures
{
    public interface ICustomFirearm
    {
        public abstract int Id { get; set; }
        public abstract FirearmType Item {  get; set; }
        public abstract byte? MaxAmmo { get; set; }
        public abstract float? FireRate { get; set; }
        public abstract List<AttachmentName> Attachments { get; set; }
        public abstract Vector3 Scale { get; set; }

    }
}
