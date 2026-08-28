using System;
using System.Collections.Generic;
using System.Text.Json;
using MEC;
using UncomplicatedCustomRoles.API.Features.Messages;
using UncomplicatedCustomRoles.Manager;
using UncomplicatedCustomRoles.Manager.NET;

namespace UncomplicatedCustomRoles.API.Features.Controllers;

internal static class Presence
{
    private const string Endpoint = "https://api.ucserver.it/v3/plugin/ucr/presence";

    internal static IEnumerator<float> PresenceCoroutine()
    {
        while (true)
        {
            var payload = BuildPayload();

            if (payload is not null)
                yield return Timing.WaitUntilDone(WebQuery.Post(Endpoint, payload, "application/json", OnAnswer));

            yield return Timing.WaitForSeconds(60f);
        }
    }

    private static string BuildPayload()
    {
        try
        {
            return JsonSerializer.Serialize(new PresenceMessage());
        }
        catch (Exception e)
        {
            LogManager.Error($"Failed to build the presence data: {e.Message}");
            return null;
        }
    }

    private static void OnAnswer(HttpResponse response)
    {
        if (!response.IsSuccess)
            LogManager.Debug($"Failed to send the presence data: {response.Reason}");
    }
}