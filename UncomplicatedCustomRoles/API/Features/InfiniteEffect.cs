using MEC;
using PlayerRoles;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomRoles.Manager;
using UnityEngine;
using PluginAPI.Core;

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

                // Really funny we have also to check for SCPs near the escaping point
                foreach (Player Player in Player.GetPlayers().Where(player => player.IsSCP && Vector3.Distance(new(123.85f, 988.8f, 18.9f), player.Position) < 7.5f))
                {
                    LogManager.Debug("Calling respawn event for player -> position -- It's an SCP!");
                    // Let's make this SCP escape
                    Plugin.Instance.Handler.OnEscaping(new(Player.ReferenceHub, RoleTypeId.ChaosConscript, global::Escape.EscapeScenarioType.None, Respawning.SpawnableTeamType.ChaosInsurgency, 0f));
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
