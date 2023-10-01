using Exiled.API.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UncomplicatedCustomRoles.Structures;

namespace UncomplicatedCustomRoles.Manager
{
    public class UCREffect : IEffect
    {
        public EffectType EffectType { get; set; } = EffectType.Scp207;
        public float Duration { get; set; } = 10;
        public byte Intensity { get; set; } = 255;
    }
}