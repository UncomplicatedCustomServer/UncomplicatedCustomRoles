using Exiled.API.Enums;
using System.ComponentModel;
using UncomplicatedCustomRoles.Structures;

namespace UncomplicatedCustomRoles.Manager
{
    public class UCREffect : IUCREffect
    {
        [Description("The effect EffectType found on the EXILED discord server")]
        public EffectType EffectType { get; set; } = EffectType.MovementBoost;
        [Description("The duration of the effect, -1 if you want to have an infinite effect")]
        public float Duration { get; set; } = -1;
        [Description("The intensity of the effect from 1 to 255")]
        public byte Intensity { get; set; } = 1;
        [Description("Can the effect be removed with SCP-500?")]
        public bool Removable { get; set; } = false;
    }
}