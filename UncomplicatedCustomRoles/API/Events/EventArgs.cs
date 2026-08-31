/*
 * This file is a part of the UncomplicatedCustomRoles project.
 *
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 *
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using LabApi.Features.Wrappers;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Interfaces;

namespace UncomplicatedCustomRoles.API.Events;

public class CustomRoleRegisteringEventArgs(ICustomRole role)
{
    /// <summary>
    ///     Gets the <see cref="ICustomRole" /> that is being registered.
    /// </summary>
    public ICustomRole Role { get; } = role;

    /// <summary>
    ///     Gets or sets whether the registration is allowed.
    /// </summary>
    public bool IsAllowed { get; set; } = true;
}

public class CustomRoleRegisteredEventArgs(ICustomRole role)
{
    /// <summary>
    ///     Gets the <see cref="ICustomRole" /> that has been registered.
    /// </summary>
    public ICustomRole Role { get; } = role;
}

public class CustomRoleUnregisteredEventArgs(ICustomRole role)
{
    /// <summary>
    ///     Gets the <see cref="ICustomRole" /> that has been unregistered.
    /// </summary>
    public ICustomRole Role { get; } = role;
}

public class CustomRoleSpawningEventArgs(Player player, ICustomRole role)
{
    /// <summary>
    ///     Gets the <see cref="Player" /> that is being spawned as a custom role.
    /// </summary>
    public Player Player { get; } = player;

    /// <summary>
    ///     Gets the <see cref="ICustomRole" /> that is being applied.
    /// </summary>
    public ICustomRole Role { get; } = role;

    /// <summary>
    ///     Gets or sets whether the spawn is allowed.
    /// </summary>
    public bool IsAllowed { get; set; } = true;
}

public class CustomRoleSpawnedEventArgs(SummonedCustomRole instance)
{
    /// <summary>
    ///     Gets the <see cref="SummonedCustomRole" /> instance that has been created.
    /// </summary>
    public SummonedCustomRole Instance { get; } = instance;

    /// <summary>
    ///     Gets the <see cref="Player" /> that has been spawned as a custom role.
    /// </summary>
    public Player Player => Instance.Player;

    /// <summary>
    ///     Gets the <see cref="ICustomRole" /> that has been applied.
    /// </summary>
    public ICustomRole Role => Instance.Role;
}

public class CustomRoleRemovedEventArgs(SummonedCustomRole instance)
{
    /// <summary>
    ///     Gets the (now invalid) <see cref="SummonedCustomRole" /> instance that has been removed.
    /// </summary>
    public SummonedCustomRole Instance { get; } = instance;

    /// <summary>
    ///     Gets the <see cref="Player" /> the custom role has been removed from.
    /// </summary>
    public Player Player => Instance.Player;

    /// <summary>
    ///     Gets the <see cref="ICustomRole" /> that has been removed.
    /// </summary>
    public ICustomRole Role => Instance.Role;
}