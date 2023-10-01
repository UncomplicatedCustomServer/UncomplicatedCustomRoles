using Exiled.API.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UncomplicatedCustomRoles.Structures
{
    public interface IEffect
    {
        public abstract EffectType EffectType { get; set; }
        public abstract float Duration { get; set; }
        public abstract byte Intensity { get; set; }
    }
}