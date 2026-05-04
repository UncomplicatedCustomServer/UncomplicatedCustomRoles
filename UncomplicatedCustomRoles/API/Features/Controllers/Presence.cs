using System;
using System.Collections.Generic;
using System.Text.Json;
using MEC;
using UncomplicatedCustomRoles.API.Features.Messages;
using UncomplicatedCustomRoles.Manager;

namespace UncomplicatedCustomRoles.API.Features.Controllers
{
    internal static class Presence
    {
        internal static IEnumerator<float> PresenceCoroutine()
        {
            while (true)
            {
                yield return Timing.WaitForSeconds(60f);
                try
                {
                    HttpQuery.Post("https://api.ucserver.it/v3/plugin/ucr/presence", JsonSerializer.Serialize(new PresenceMessage()), "application/json");
                }
                catch (Exception e)
                {
                    LogManager.Error($"Failed to send presence data: {e.Message}");
                }
            }
        }
    }
}
