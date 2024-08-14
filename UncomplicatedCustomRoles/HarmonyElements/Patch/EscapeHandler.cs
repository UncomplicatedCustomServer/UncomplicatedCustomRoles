using HarmonyLib;
using PlayerRoles;
using Respawning;
using System.Collections.Generic;
using System.Reflection.Emit;
using UncomplicatedCustomRoles.Events.Args;
using static HarmonyLib.AccessTools;
using static Escape;
using UncomplicatedCustomRoles.Events;
using System.Linq;
using UncomplicatedCustomRoles.Events.Enums;

namespace UncomplicatedCustomRoles.HarmonyElements.Patch
{
    [HarmonyPatch(typeof(Escape), nameof(Escape.ServerHandlePlayer))]
    internal class EscapeHandler
    {
        public static string ActorEventName => EventName.PlayerEscaping.ToString();
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> Instructions = new(instructions);

            int Index = -1;
            bool Found = false;

            for (int i = 0; i < Instructions.Count(); i++)
            {
                if (Instructions[i].opcode == OpCodes.Ldarg_0)
                    if (Found)
                    {
                        Index = i;
                        break;
                    }
                    else
                        Found = true;
            }

            Label Continue = generator.DefineLabel();

            LocalBuilder ev = generator.DeclareLocal(typeof(EscapingEventArgs));

            if (Index != -1)
            {
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
                    new(OpCodes.Stloc_S, ev.LocalIndex),

                    new(OpCodes.Ldstr, ActorEventName),
                    new(OpCodes.Ldloc_S, ev.LocalIndex),
                    new(OpCodes.Call, Method(typeof(EventManager), nameof(EventManager.InvokeEvent))),

                    new(OpCodes.Ldloc_S, ev.LocalIndex),
                    new(OpCodes.Callvirt, PropertyGetter(typeof(EscapingEventArgs), nameof(EscapingEventArgs.IsAllowed))),
                    new(OpCodes.Brtrue_S, Continue),

                    new(OpCodes.Ret),
                    //new CodeInstruction(OpCodes.Ret).WithLabels(Continue),

                    new CodeInstruction(OpCodes.Ldloc_S, ev.LocalIndex).WithLabels(Continue),
                    new(OpCodes.Callvirt, PropertyGetter(typeof(EscapingEventArgs), nameof(EscapingEventArgs.Hub))),
                    new(OpCodes.Starg_S, 0),

                    new(OpCodes.Ldloc_S, ev.LocalIndex),
                    new(OpCodes.Callvirt, PropertyGetter(typeof(EscapingEventArgs), nameof(EscapingEventArgs.NewRole))),
                    new(OpCodes.Stloc_0),

                    new(OpCodes.Ldloc_S, ev.LocalIndex),
                    new(OpCodes.Callvirt, PropertyGetter(typeof(EscapingEventArgs), nameof(EscapingEventArgs.Scenario))),
                    new(OpCodes.Stloc_1),

                    new(OpCodes.Ldloc_S, ev.LocalIndex),
                    new(OpCodes.Callvirt, PropertyGetter(typeof(EscapingEventArgs), nameof(EscapingEventArgs.Team))),
                    new(OpCodes.Stloc_2),

                    new(OpCodes.Ldloc_S, ev.LocalIndex),
                    new(OpCodes.Callvirt, PropertyGetter(typeof(EscapingEventArgs), nameof(EscapingEventArgs.Tokens))),
                    new(OpCodes.Stloc_3),
                }); ;
            }

            return Instructions;
        }
    }
}
