using HarmonyLib;
using RemoteAdmin.Communication;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;
using UncomplicatedCustomRoles.API.Features;

using static HarmonyLib.AccessTools;

namespace UncomplicatedCustomRoles.HarmonyElements.Patch
{
    [HarmonyPatch(typeof(RaPlayer), nameof(RaPlayer.ReceiveData), typeof(CommandSender), typeof(string))]
    internal class RaPlayerInfoPatch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> newInstructions = new(instructions);
            bool found = false;
            int index = -1;

            for (int i = 0; i < newInstructions.Count; i++)
            {
                if (newInstructions[i].opcode == OpCodes.Ldstr && newInstructions[i].operand is string str && str is "\nPosition: ")
                    found = true;
                else if (found && newInstructions[i].opcode == OpCodes.Pop)
                {
                    index = i + 1;
                    break;
                }
            }

            if (index != -1)
                newInstructions.InsertRange(index, new CodeInstruction[]
                {
                    new(OpCodes.Ldloc_S, 14),
                    new(OpCodes.Ldloc_S, 6),
                    new(OpCodes.Call, Method(typeof(SummonedCustomRole), nameof(SummonedCustomRole.TryParseRemoteAdmin))),
                    new(OpCodes.Callvirt, Method(typeof(StringBuilder), nameof(StringBuilder.Append), new Type[] { typeof(string) })),
                    new(OpCodes.Pop),
                });

            return newInstructions;
        }
    }
}