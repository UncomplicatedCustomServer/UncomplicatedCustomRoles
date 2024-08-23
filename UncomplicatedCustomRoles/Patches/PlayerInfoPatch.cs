using Exiled.Loader;
using HarmonyLib;
using RemoteAdmin.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.Manager;
using static HarmonyLib.AccessTools;

namespace UncomplicatedCustomRoles.Patches
{
    //[HarmonyPatch(typeof(RaPlayer), nameof(RaPlayer.ReceiveData), typeof(CommandSender), typeof(string))]
    internal class PlayerInfoPatch
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

        internal static void TryPatchCedMod()
        {
            LogManager.Silent("Trying to patch CedMod");
            Assembly cedModAssembly = Loader.Plugins.Where(p => p.Name is "CedMod").FirstOrDefault()?.Assembly;
            MethodInfo targetMethod = cedModAssembly?.GetType("CedMod.Patches.RaPlayerPatch")?.GetMethod("RaPlayerCoRoutine");

            if (targetMethod is not null)
            {
                LogManager.Silent("Patched CedMod");
                Plugin.Instance._harmony.Patch(targetMethod, transpiler: new(Method(typeof(PlayerInfoPatch), nameof(PlayerInfoPatch.Transpiler))));
            }
            else
            {
                LogManager.Silent("Patched RaPlayer");
                Plugin.Instance._harmony.Patch(Method(typeof(RaPlayer), nameof(RaPlayer.ReceiveData), new Type[] { typeof(CommandSender), typeof(string) }), transpiler: new(Method(typeof(PlayerInfoPatch), nameof(PlayerInfoPatch.Transpiler))));
            }
        }
    }
}
