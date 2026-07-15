/*
 * This file is a part of the UncomplicatedCustomRoles project.
 *
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 *
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using UncomplicatedCustomRoles.Manager;

namespace UncomplicatedCustomRoles.API.Events;

public static class CustomRoleEvents
{
    /// <summary>
    ///     Invoked before a <see cref="Interfaces.ICustomRole" /> is registered.
    ///     Set <see cref="CustomRoleRegisteringEventArgs.IsAllowed" /> to false to deny the registration.
    /// </summary>
    public static event Action<CustomRoleRegisteringEventArgs> Registering;

    /// <summary>
    ///     Invoked after a <see cref="Interfaces.ICustomRole" /> has been successfully registered.
    /// </summary>
    public static event Action<CustomRoleRegisteredEventArgs> Registered;

    /// <summary>
    ///     Invoked after a <see cref="Interfaces.ICustomRole" /> has been unregistered.
    /// </summary>
    public static event Action<CustomRoleUnregisteredEventArgs> Unregistered;

    /// <summary>
    ///     Invoked before a player is spawned as a custom role.
    ///     Set <see cref="CustomRoleSpawningEventArgs.IsAllowed" /> to false to deny the spawn.
    /// </summary>
    public static event Action<CustomRoleSpawningEventArgs> Spawning;

    /// <summary>
    ///     Invoked after a player has been spawned as a custom role and the related
    ///     <see cref="Features.SummonedCustomRole" /> instance has been created.
    /// </summary>
    public static event Action<CustomRoleSpawnedEventArgs> Spawned;

    /// <summary>
    ///     Invoked after a custom role has been removed from a player.
    /// </summary>
    public static event Action<CustomRoleRemovedEventArgs> Removed;

    internal static void OnRegistering(CustomRoleRegisteringEventArgs args)
    {
        InvokeSafely(Registering, args, nameof(Registering));
    }

    internal static void OnRegistered(CustomRoleRegisteredEventArgs args)
    {
        InvokeSafely(Registered, args, nameof(Registered));
    }

    internal static void OnUnregistered(CustomRoleUnregisteredEventArgs args)
    {
        InvokeSafely(Unregistered, args, nameof(Unregistered));
    }

    internal static void OnSpawning(CustomRoleSpawningEventArgs args)
    {
        InvokeSafely(Spawning, args, nameof(Spawning));
    }

    internal static void OnSpawned(CustomRoleSpawnedEventArgs args)
    {
        InvokeSafely(Spawned, args, nameof(Spawned));
    }

    internal static void OnRemoved(CustomRoleRemovedEventArgs args)
    {
        InvokeSafely(Removed, args, nameof(Removed));
    }

    private static void InvokeSafely<T>(Action<T> ev, T args, string name)
    {
        if (ev is null)
            return;

        foreach (var handler in ev.GetInvocationList())
            try
            {
                ((Action<T>)handler)(args);
            }
            catch (Exception e)
            {
                LogManager.Error(
                    $"An exception has been thrown by an external handler of the event CustomRoleEvents.{name} ({handler.Method?.DeclaringType?.FullName}::{handler.Method?.Name}): {e}");
            }
    }
}