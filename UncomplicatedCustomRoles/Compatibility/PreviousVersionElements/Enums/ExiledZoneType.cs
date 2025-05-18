// -----------------------------------------------------------------------
// <copyright file="ZoneType.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

using System;

namespace UncomplicatedCustomRoles.Compatibility.PreviousVersionElements.Enums
{
    [Flags]
    public enum ExiledZoneType
    {
        Unspecified = 0,
        LightContainment = 1,
        HeavyContainment = 2,
        Entrance = 4,
        Surface = 8,
        Pocket = 16,
        Other = 32,

    }
}
