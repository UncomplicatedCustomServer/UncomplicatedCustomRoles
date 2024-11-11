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
        SilentAnnouncer = 1 << 8,
        TutorialRagdoll = 1 << 9,
        DoNotTrigger096 = 1 << 10,
        BanKeycards = 1 << 11,
        BanMedicals = 1 << 12,
        BanRadios = 1 << 13,
        BanFirearms = 1 << 14,
        BanGrenades = 1 << 15,
        BanSCPItems = 1 << 16,
        BanMicroHID = 1 << 17,
        BanArmors = 1 << 18,
    }
}