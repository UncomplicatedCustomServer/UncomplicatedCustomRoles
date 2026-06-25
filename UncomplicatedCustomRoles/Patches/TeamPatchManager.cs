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
using HarmonyLib;
using UncomplicatedCustomRoles.Manager;

namespace UncomplicatedCustomRoles.Patches
{
    internal static class TeamPatchManager
    {
        internal const string Category = "UncomplicatedCustomRoles.DynamicTeamPatch";

        private static readonly object Sync = new();

        private static Harmony _harmony;

        private static bool IsPatched { get; set; }
        
        internal static void Initialize()
        {
            lock (Sync)
            {
                _harmony = new Harmony($"com.ucs.ucr_labapi.teampatch-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}");
                IsPatched = false;
            }
        }
        
        internal static void EnsurePatched()
        {
            if (IsPatched)
                return;

            lock (Sync)
            {
                if (IsPatched || _harmony is null)
                    return;

                try
                {
                    _harmony.PatchCategory(Plugin.Assembly, Category);
                    IsPatched = true;
                    LogManager.Debug("Dynamic team patches applied - at least one player is now disguised.");
                }
                catch (Exception e)
                {
                    LogManager.Error($"Failed to apply the dynamic team patches: {e}");
                }
            }
        }
        
        internal static void EnsureUnpatched()
        {
            if (!IsPatched)
                return;

            lock (Sync)
            {
                if (!IsPatched || _harmony is null)
                    return;

                try
                {
                    _harmony.UnpatchCategory(Plugin.Assembly, Category);
                    IsPatched = false;
                    LogManager.Debug("Dynamic team patches removed - nobody is disguised anymore.");
                }
                catch (Exception e)
                {
                    LogManager.Error($"Failed to remove the dynamic team patches: {e}");
                }
            }
        }
        
        internal static void Shutdown()
        {
            lock (Sync)
            {
                if (_harmony is not null && IsPatched)
                {
                    try
                    {
                        _harmony.UnpatchCategory(Plugin.Assembly, Category);
                    }
                    catch (Exception e)
                    {
                        LogManager.Error($"Failed to remove the dynamic team patches during shutdown: {e}");
                    }
                }

                _harmony = null;
                IsPatched = false;
            }
        }
    }
}
