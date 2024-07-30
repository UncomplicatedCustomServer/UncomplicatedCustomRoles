namespace UncomplicatedCustomRoles.Events.Interfaces
{
    public interface IDeniableEvent
    {
        public abstract bool IsAllowed { get; set; }
    }
}
