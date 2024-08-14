using HarmonyLib;
using Respawning;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UncomplicatedCustomRoles.Events;
using UncomplicatedCustomRoles.Events.Args;
using UncomplicatedCustomRoles.Events.Enums;
using UncomplicatedCustomRoles.Manager;
using static HarmonyLib.AccessTools;

namespace UncomplicatedCustomRoles.HarmonyElements.Patch
{
    [HarmonyPatch(typeof(RespawnManager), nameof(RespawnManager.Spawn))]
    internal class RespawningTeamHandler
    {
        public static string ActorEventName => EventName.RespawningTeam.ToString();
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            int Index = -1;

            List<CodeInstruction> Instructions = new(instructions);

            LocalBuilder ev = generator.DeclareLocal(typeof(RespawningTeamEventArgs));

            Label Continue = generator.DefineLabel();

            for (int i = 0; i < Instructions.Count(); i++)
            {
                if (Instructions[i].opcode == OpCodes.Callvirt && Instructions[i].operand is MethodInfo methodInfo && methodInfo == Method(typeof(SpawnableTeamHandlerBase), nameof(SpawnableTeamHandlerBase.GenerateQueue)))
                {
                    Index = i+1;
                    break;
                }
            }

            //Index = 2;

            if (Index != -1)
            {
                Instructions.InsertRange(Index, new List<CodeInstruction>()
                {
                    // RespawningTeamEventArgs(respawnQueue, roleQueue, maxSize, nextKnownTeam)
                    // Load every useful args for our event - 1: respawnQueue
                    new(OpCodes.Ldloc_S, 6),

                    // roleQueue
                    new(OpCodes.Ldloc_S, 7),

                    // maxSize
                    new(OpCodes.Ldloc_2),

                    // nextKnownTeam
                    new(OpCodes.Ldarg_0),
                    new(OpCodes.Ldfld, Field(typeof(RespawnManager), nameof(RespawnManager.NextKnownTeam))),

                    // Call the method and save the results
                    new(OpCodes.Newobj, GetDeclaredConstructors(typeof(RespawningTeamEventArgs))[0]),
                    new(OpCodes.Stloc_S, ev.LocalIndex),

                    // Get args for the event - name
                    new(OpCodes.Ldstr, ActorEventName),
                    
                    // Object
                    new(OpCodes.Ldloc_S, ev.LocalIndex),

                    // execute the event
                    new(OpCodes.Call, Method(typeof(EventManager), nameof(EventManager.InvokeEvent))),

                    // Check if allowed
                    new(OpCodes.Ldloc_S, ev.LocalIndex),
                    new(OpCodes.Callvirt, PropertyGetter(typeof(RespawningTeamEventArgs), nameof(RespawningTeamEventArgs.IsAllowed))),
                    new(OpCodes.Brtrue_S, Continue),
                    new(OpCodes.Ret),

                    // Now apply the results
                    // First: respawnQueue
                    new CodeInstruction(OpCodes.Ldloc, ev.LocalIndex).WithLabels(Continue),
                    new(OpCodes.Callvirt, PropertyGetter(typeof(RespawningTeamEventArgs), nameof(RespawningTeamEventArgs.RespawnQueue))),
                    new(OpCodes.Stloc_S, 6),

                    // roleQueue
                    new(OpCodes.Ldloc_S, ev.LocalIndex),
                    new(OpCodes.Callvirt, PropertyGetter(typeof(RespawningTeamEventArgs), nameof(RespawningTeamEventArgs.RoleQueue))),
                    new(OpCodes.Stloc_S, 7),

                    // NextKnownTeam
                    new(OpCodes.Ldarg_0),
                    new(OpCodes.Ldloc_S, ev.LocalIndex),
                    new(OpCodes.Callvirt, PropertyGetter(typeof(RespawningTeamEventArgs), nameof(RespawningTeamEventArgs.NextKnownTeam))),
                    new(OpCodes.Stfld, Field(typeof(RespawnManager), nameof(RespawnManager.NextKnownTeam))),
                });
            }

            LogManager.System($"Successfully patched the {ActorEventName} event!");

            return Instructions;
        }
    }
}
