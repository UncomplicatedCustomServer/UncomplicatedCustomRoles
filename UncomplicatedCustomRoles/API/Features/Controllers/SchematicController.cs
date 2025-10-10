using UncomplicatedCustomRoles.Integrations;
using UncomplicatedCustomRoles.Manager;
using UnityEngine;

namespace UncomplicatedCustomRoles.API.Features.Controllers
{
    internal class SchematicController : MonoBehaviour
    {
        private MonoBehaviour _schematic;

        public void Init(string schematicName)
        {
            // Generate the schematic
            object schematic = DynamicInvoke.GetMethod("MapEditorReborn", "MapEditorReborn.API.Features.ObjectSpawner.SpawnSchematic").Invoke(null, new object[] { schematicName, Vector3.zero });
            schematic ??= DynamicInvoke.GetMethod("ProjectMER", "ProjectMER.Features.ObjectSpawner.SpawnSchematic").Invoke(null, new object[] { schematicName, Vector3.zero });

            if (schematic is null)
            {
                LogManager.Error($"[MER Extension] Failed to import MER or ProjectMER schematic {schematicName}!\nSchematic OR method not found!");
                Destroy(this);
                return;
            }

            if (schematic is not MonoBehaviour monoSchematic)
            {
                LogManager.Error($"[MER Extension] Failed to import MER or ProjectMER schematic {schematicName}!\nThe schematic object was not MonoBehaviour but {schematic.GetType().FullName}!");
                Destroy(this);
                return;
            }

            _schematic = monoSchematic;
        }

        private void Update()
        {
            if (_schematic is null)
                return;

            // This should reference to the player
            _schematic.transform.position = gameObject.transform.position;
            _schematic.transform.rotation = gameObject.transform.rotation;
        }

        private void OnDestroy()
        {
            Destroy(_schematic);
            _schematic = null;
        }
    }
}