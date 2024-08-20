using System;

namespace UncomplicatedCustomRoles.API.Enums
{
    [Flags]
    public enum CustomFlags
    {
        NotExecutable = -1,
        None = 0,
        DoNotTriggerTeslaGates = 1 << 0,
        LifeStealer = 1 << 1,
        HalfLifeStealer = 1 << 2,
        NotAffectedByAppearance = 1 << 3,
        PacifismUntilDamage = 1 << 4,
        CustomScpAnnouncer = 1 << 5,
        ShowOnlyCustomInfo = 1 << 6,
        SilentWalker = 1 << 7,
    }
}