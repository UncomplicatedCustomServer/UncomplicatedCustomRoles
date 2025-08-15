﻿/*
 * This file is a part of the UncomplicatedCustomRoles project.
 * 
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 * 
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using MEC;
using PlayerRoles;
using System.Collections.Generic;
using UncomplicatedCustomRoles.Events;
using UnityEngine;

#nullable enable
namespace UncomplicatedCustomRoles.API.Features
{
    public class InfiniteEffect
    {
        /// <summary>
        /// Whether the infinite effect coroutine is running or not
        /// </summary>
        public static bool IsRunning => CoroutineHandle.IsRunning;

        internal static CoroutineHandle CoroutineHandle { get; private set; }

        internal static bool EffectAssociationAllowed { get; set; } = false;

        /// <summary>
        /// Start the coroutine
        /// </summary>
        public static void Start()
        {
            if (IsRunning) 
                return;

            CoroutineHandle = Timing.RunCoroutine(Actor());
        }

        /// <summary>
        /// Stop the coroutine
        /// </summary>
        public static void Stop()
        {
            if (!IsRunning)
                return;

            Timing.KillCoroutines(CoroutineHandle);
        }

        internal static IEnumerator<float> Actor()
        {
            while (EffectAssociationAllowed)
            {
                SummonedCustomRole.InfiniteEffectActor();

                var zones = global::Escape.EscapeZones;
                if (zones != null && zones.Count > 0)
                {
                    foreach (var role in SummonedCustomRole.List)
                    {
                        if (!role.Player.IsSCP || !role.Role.CanEscape)
                            continue;

                        Vector3 pos = role.Player.Position;
                        Bounds? found = null;
                        foreach (Bounds escapeZone in zones)
                        {
                            if (escapeZone.Contains(pos))
                            {
                                found = escapeZone;
                                break;
                            }
                        }

                        if (found.HasValue)
                            EventHandler.OnEscaping(new(role.Player.ReferenceHub, role.Player.Role, RoleTypeId.ChaosConscript, global::Escape.EscapeScenarioType.Custom, found.Value));
                    }
                }

                yield return Timing.WaitForSeconds(2.5f);
            }
        }

        internal static void Terminate()
        {
            EffectAssociationAllowed = false;
            Stop();
        }
    }
}
