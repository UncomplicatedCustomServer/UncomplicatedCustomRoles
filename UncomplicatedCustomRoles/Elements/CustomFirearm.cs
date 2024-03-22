using Exiled.API.Enums;
using InventorySystem.Items.Firearms.Attachments;
using System.Collections.Generic;
using UncomplicatedCustomRoles.Structures;
using UnityEngine;

#nullable enable
namespace UncomplicatedCustomRoles.Elements
{
    public class CustomFirearm : ICustomFirearm
    {
        public int Id { get; set; } = 972;
        public FirearmType Item { get; set; } = FirearmType.Com18;
        public byte? MaxAmmo { get; set; } = null;
        public float? FireRate { get; set; } = null;
        public List<AttachmentName> Attachments { get; set; } = new()
        {
            AttachmentName.Flashlight
        };
        public Vector3 Scale { get; set; } = new();
    }
}
