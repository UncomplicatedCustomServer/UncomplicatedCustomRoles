using HarmonyLib;
using Respawning;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UncomplicatedCustomRoles.Events;
using UncomplicatedCustomRoles.Events.Args;
using static HarmonyLib.AccessTools;

namespace UncomplicatedCustomRoles.HarmonyElements.Patch
{
    [HarmonyPatch(typeof(RespawnManager), nameof(RespawnManager.Spawn))]
    internal class RespawningTeamHandler
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            int Index = -1;

            List<CodeInstruction> Instructions = new(instructions);

            LocalBuilder ev = generator.DeclareLocal(typeof(RespawningTeamEventArgs));

            Label Continue = generator.DefineLabel();

            for (int i = 0; i < Instructions.Count(); i++)
            {
                if (Instructions[i].opcode == OpCodes.Callvirt && Instructions[i].operand is MethodInfo methodInfo && methodInfo == Method(typeof(Respawning.SpawnableTeamHandlerBase), nameof(Respawning.SpawnableTeamHandlerBase.GenerateQueue)))
                {
                    Index = i;
                    break;
                }
            }

            if (Index != -1)
            {
                Instructions.InsertRange(Index, new List<CodeInstruction>()
                {
                    new(OpCodes.Pop),

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

                    new(OpCodes.Pop),

                    // Get args for the event - name
                    new(OpCodes.Ldstr, "RespawningTeam"),
                    
                    // Object
                    new(OpCodes.Ldloc_S, ev.LocalIndex),

                    // execute the event
                    new(OpCodes.Call, Method(typeof(EventManager), nameof(EventManager.InvokeEvent))),
                    new(OpCodes.Stloc_S, ev.LocalIndex+1),

                    // Check if allowed
                    new(OpCodes.Callvirt, PropertyGetter(typeof(KeyValuePair<string, RespawningTeamEventArgs>), nameof(KeyValuePair<string, RespawningTeamEventArgs>.Key))),
                    new(OpCodes.Brtrue_S, Continue),

                    new(OpCodes.Ret),

                    // Now apply the results
                    // First: respawnQueue
                    new(OpCodes.Ldloc, ev.LocalIndex+1),
                    new CodeInstruction(OpCodes.Callvirt, PropertyGetter(typeof(KeyValuePair<string, RespawningTeamEventArgs>), nameof(KeyValuePair<string, RespawningTeamEventArgs>.Value.RespawnQueue))).WithLabels(Continue),
                    new(OpCodes.Stloc_S, 6),

                    // roleQueue
                    new(OpCodes.Ldloc, ev.LocalIndex+1),
                    new(OpCodes.Callvirt, PropertyGetter(typeof(KeyValuePair<string, RespawningTeamEventArgs>), nameof(KeyValuePair<string, RespawningTeamEventArgs>.Value.RoleQueue))),
                    new(OpCodes.Stloc_S, 7),

                    // maxSize
                    new(OpCodes.Ldloc, ev.LocalIndex+1),
                    new(OpCodes.Callvirt, PropertyGetter(typeof(KeyValuePair<string, RespawningTeamEventArgs>), nameof(KeyValuePair<string, RespawningTeamEventArgs>.Value.MaxWaveSize))),
                    new(OpCodes.Stloc_2),

                    // NextKnownTeam
                    new(OpCodes.Ldloc, ev.LocalIndex+1),
                    new(OpCodes.Callvirt, PropertyGetter(typeof(KeyValuePair<string, RespawningTeamEventArgs>), nameof(KeyValuePair<string, RespawningTeamEventArgs>.Value.NextKnownTeam))),
                    new(OpCodes.Ldarg_0),
                    new(OpCodes.Stfld, Field(typeof(RespawnManager), nameof(RespawnManager.NextKnownTeam))),
                });
            }

            return Instructions;
        }
    }
}
