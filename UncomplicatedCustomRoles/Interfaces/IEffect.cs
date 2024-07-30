namespace UncomplicatedCustomRoles.Interfaces
{
    public interface IEffect
    {
        public abstract string EffectType { get; set; }

        public abstract float Duration { get; set; }

        public abstract byte Intensity { get; set; }

        public abstract bool Removable { get; set; }
    }
}