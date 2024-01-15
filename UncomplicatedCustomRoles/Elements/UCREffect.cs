using Exiled.API.Enums;
using UncomplicatedCustomRoles.Structures;

namespace UncomplicatedCustomRoles.Manager
{
    public class UCREffect : IUCREffect
    {
        public EffectType EffectType { get; set; } = EffectType.Scp207;
        public float Duration { get; set; } = 10;
        public byte Intensity { get; set; } = 255;
    }
}