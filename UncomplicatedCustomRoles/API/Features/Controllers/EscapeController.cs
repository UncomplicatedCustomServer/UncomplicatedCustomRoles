using PlayerRoles;
using UncomplicatedCustomRoles.Events;
using UnityEngine;

namespace UncomplicatedCustomRoles.API.Features.Controllers
{
    internal class EscapeController : MonoBehaviour
    {
        private SummonedCustomRole _role;

        public void Init(SummonedCustomRole role)
        {
            _role = role;
        }

        private void Update()
        {
            foreach (Bounds escapeZone in global::Escape.EscapeZones)
                if (escapeZone.Contains(_role.Player.Position))
                    EventHandler.OnEscaping(new(_role.Player.ReferenceHub, _role.Player.Role, RoleTypeId.ChaosConscript, global::Escape.EscapeScenarioType.Custom, escapeZone));
        }

        private void OnDestroy()
        {
            _role = null;
        }
    }
}