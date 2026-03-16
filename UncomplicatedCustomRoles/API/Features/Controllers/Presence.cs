using System;
using System.Collections;
using System.Text.Json;
using UncomplicatedCustomRoles.API.Features.Messages;
using UnityEngine;

namespace UncomplicatedCustomRoles.API.Features.Controllers
{
    internal class Presence : MonoBehaviour
    {
        private DateTime _lastUpdate = default;
        private readonly string _endpoint = "https://api.ucserver.it/v3/plugin/ucr/presence";
        private readonly int _delay = 60;

        public void Update()
        {
            if ((DateTime.UtcNow - _lastUpdate).TotalSeconds < _delay)
                return;

            _lastUpdate = DateTime.UtcNow;
            StartCoroutine(PresenceRequest());
        }

        public void OnDestroy()
        {
            StopAllCoroutines();
        }

        public IEnumerator PresenceRequest()
        {
            yield return HttpQuery.PostCoroutine(_endpoint, JsonSerializer.Serialize(new PresenceMessage()), "application/json").Wait();
        }
    }
}
