using HarmonyLib;
using PlayerRoles;
using Respawning;
using System.Collections.Generic;
using System.Reflection.Emit;
using UncomplicatedCustomRoles.Events.Args;
using static HarmonyLib.AccessTools;
using static Escape;
using UncomplicatedCustomRoles.Events;

namespace UncomplicatedCustomRoles.HarmonyElements.Patch
{
    //[HarmonyPatch(typeof(Escape), nameof(Escape.ServerHandlePlayer))]
    internal class EscapeHandler
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> Instructions = new(instructions);
            int Index = 0;
            foreach (CodeInstruction Instruction in instructions)
            {
                if (Instruction.opcode == OpCodes.Ret)
                {
                    break;
                }
                Index++;
            }

            Label Continue = generator.DefineLabel();

            LocalBuilder ev = generator.DeclareLocal(typeof(EscapingEventArgs));

            Instructions.InsertRange(Index, new List<CodeInstruction>()
            {
                // ReferenceHub
                new(OpCodes.Ldarg_0),

                // NewRole
                new(OpCodes.Ldloc_0),

                // Scenario
                new(OpCodes.Ldloc_1),

                // Team
                new(OpCodes.Ldloc_2),

                // Tokens
                new(OpCodes.Ldloc_3),
                new(OpCodes.Newobj, typeof(EscapingEventArgs).GetConstructor(new[] { typeof(ReferenceHub), typeof(RoleTypeId), typeof(EscapeScenarioType), typeof(SpawnableTeamType), typeof(float) })),
                new(OpCodes.Dup),
                new(OpCodes.Dup),
                new(OpCodes.Stloc, ev.LocalIndex),

                new(OpCodes.Ldstr, "PlayerEscaping"),
                new(OpCodes.Ldloc, ev.LocalIndex),
                new(OpCodes.Call, Method(typeof(EventManager), nameof(EventManager.InvokeEvent))),
                new(OpCodes.Stloc, ev.LocalIndex+1),

                new(OpCodes.Pop),

                new(OpCodes.Ldloc, ev.LocalIndex+1),
                new(OpCodes.Callvirt, PropertyGetter(typeof(KeyValuePair<bool, EscapingEventArgs>), nameof(KeyValuePair<bool, EscapingEventArgs>.Key))),
                new(OpCodes.Brtrue, Continue),

                // Return
                new(OpCodes.Ret),

                new CodeInstruction(OpCodes.Ldloc, ev.LocalIndex+1).WithLabels(Continue),
                new(OpCodes.Callvirt, PropertyGetter(typeof(KeyValuePair<bool, EscapingEventArgs>), nameof(KeyValuePair<bool, EscapingEventArgs>.Value.NewRole))),
                new(OpCodes.Stloc_0),

                new(OpCodes.Ldloc, ev.LocalIndex+1),
                new(OpCodes.Callvirt, PropertyGetter(typeof(KeyValuePair<bool, EscapingEventArgs>), nameof(KeyValuePair<bool, EscapingEventArgs>.Value.Scenario))),
                new(OpCodes.Stloc_1),

                new(OpCodes.Ldloc, ev.LocalIndex+1),
                new(OpCodes.Callvirt, PropertyGetter(typeof(KeyValuePair<bool, EscapingEventArgs>), nameof(KeyValuePair<bool, EscapingEventArgs>.Value.Team))),
                new(OpCodes.Stloc_2),

                new(OpCodes.Ldloc, ev.LocalIndex+1),
                new(OpCodes.Callvirt, PropertyGetter(typeof(KeyValuePair<bool, EscapingEventArgs>), nameof(KeyValuePair<bool, EscapingEventArgs>.Value.Tokens))),
                new(OpCodes.Stloc_3),
            });

            return Instructions;
        }
    }
}
