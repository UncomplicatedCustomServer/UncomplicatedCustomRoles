/*
 * This file is a part of the UncomplicatedCustomRoles project.
 *
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 *
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;
using System.Linq;
using LabApi.Features.Wrappers;
using MEC;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Features.Behaviour;
using UncomplicatedCustomRoles.API.Interfaces;
using UncomplicatedCustomRoles.Extensions;
using Random = UnityEngine.Random;

namespace UncomplicatedCustomRoles.Manager;

internal static class DelayedSpawnManager
{
    private static readonly List<CoroutineHandle> Scheduled = [];

    internal static void ScheduleAll()
    {
        Cancel();

        foreach (ICustomRole role in CustomRole.CustomRoles.Values)
        {
            if (role?.SpawnSettings is null || role.IgnoreSpawnSystem || role.SpawnSettings.SpawnDelay <= 0)
                continue;

            int id = role.Id;
            float delay = role.SpawnSettings.SpawnDelay;

            LogManager.Debug($"Scheduling the delayed spawn of {role.Name} ({id}) in {delay} second(s)");
            Scheduled.Add(Timing.CallDelayed(delay, () => Execute(id)));
        }
    }

    internal static void Cancel()
    {
        foreach (CoroutineHandle handle in Scheduled.Where(handle => handle.IsRunning))
            Timing.KillCoroutines(handle);

        Scheduled.Clear();
    }

    private static void Execute(int id)
    {
        if (!CustomRole.CustomRoles.TryGetValue(id, out ICustomRole role) || role?.SpawnSettings is null)
        {
            LogManager.Debug($"The delayed spawn of the role {id} fired but the role is no longer registered");
            return;
        }

        if (!Round.IsRoundStarted || Round.IsRoundEnded)
        {
            LogManager.Debug($"Skipping the delayed spawn of {role.Name} ({id}): the round is not running anymore");
            return;
        }

        SpawnBehaviour settings = role.SpawnSettings;

        int readyPlayers = Player.ReadyList.Count();
        if (readyPlayers < settings.MinPlayers)
        {
            LogManager.Debug($"Skipping the delayed spawn of {role.Name} ({id}): min_players is {settings.MinPlayers} but only {readyPlayers} player(s) are on the server");
            return;
        }

        int slots = settings.MaxPlayers - SummonedCustomRole.Count(role);
        if (slots < 1)
        {
            LogManager.Debug($"Skipping the delayed spawn of {role.Name} ({id}): max_players ({settings.MaxPlayers}) is already reached");
            return;
        }

        List<Player> candidates = Player.ReadyList.Where(player => IsEligible(player, role)).ToList();
        if (candidates.Count == 0)
        {
            LogManager.Debug($"Skipping the delayed spawn of {role.Name} ({id}): nobody currently holds one of its can_replace_roles ({string.Join(", ", settings.CanReplaceRoles ?? [])})");
            return;
        }

        candidates.ShuffleList();

        int spawned = 0;
        foreach (Player player in candidates)
        {
            if (spawned >= slots)
                break;

            if (settings.SpawnChance < 100 && Random.Range(0f, 100f) >= settings.SpawnChance)
                continue;

            SpawnManager.SummonCustomSubclass(player, id);
            spawned++;
        }

        LogManager.Debug($"The delayed spawn of {role.Name} ({id}) spawned {spawned} player(s) out of {candidates.Count} candidate(s)");
    }

    private static bool IsEligible(Player player, ICustomRole role)
    {
        if (player is null || player.HasCustomRole())
            return false;

        if (Plugin.Instance.Config.IgnoreNpcs && player.IsNpc)
            return false;

        if (role.SpawnSettings.CanReplaceRoles is not { } canReplaceRoles || !canReplaceRoles.Contains(player.Role))
            return false;

        return SpawnManager.HasRequiredPermission(player, role);
    }
}