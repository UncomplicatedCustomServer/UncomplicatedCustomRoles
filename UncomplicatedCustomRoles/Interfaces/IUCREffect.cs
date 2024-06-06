using Exiled.API.Enums;
    
namespace UncomplicatedCustomRoles.Interfaces
{
    public interface IUCREffect
    {
        public abstract EffectType EffectType { get; set; }

        public abstract float Duration { get; set; }

        public abstract byte Intensity { get; set; }

        public abstract bool Removable { get; set; }
    }
}