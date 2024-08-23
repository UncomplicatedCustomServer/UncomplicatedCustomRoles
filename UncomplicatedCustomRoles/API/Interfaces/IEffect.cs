using Exiled.API.Enums;
    
namespace UncomplicatedCustomRoles.API.Interfaces
{
    public interface IEffect
    {
        public abstract EffectType EffectType { get; set; }

        public abstract float Duration { get; set; }

        public abstract byte Intensity { get; set; }

        public abstract bool Removable { get; set; }
    }
}