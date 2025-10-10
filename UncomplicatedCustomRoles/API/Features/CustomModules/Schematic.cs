using System.Collections.Generic;
using UncomplicatedCustomRoles.API.Features.Controllers;

namespace UncomplicatedCustomRoles.API.Features.CustomModules
{
    internal class Schematic : CustomModule
    {
        public override List<string> RequiredArgs => new()
        {
            "name"
        };

        private string TargetName => StringArgs.TryGetValue("name", out string name) ? name : null;

        public override void OnAdded()
        {
            if (TargetName is null)
                return;

            SchematicController controller = CustomRole.Player.GameObject.AddComponent<SchematicController>();
            controller.Init(TargetName);
        }

        public override void OnRemoved()
        {
            if (TargetName is null)
                return;

            UnityEngine.Object.Destroy(CustomRole.Player.GameObject.GetComponent<EscapeController>());
        }
    }
}