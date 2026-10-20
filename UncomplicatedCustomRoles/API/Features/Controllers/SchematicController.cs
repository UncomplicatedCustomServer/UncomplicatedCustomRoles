/*
 * This file is a part of the UncomplicatedCustomRoles project.
 *
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 *
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using System.Reflection;
using UncomplicatedCustomRoles.Integrations;
using UncomplicatedCustomRoles.Manager;
using UnityEngine;

namespace UncomplicatedCustomRoles.API.Features.Controllers;

internal class SchematicController : MonoBehaviour
{
    private MonoBehaviour _schematic;

    private void OnDestroy()
    {
        if (_schematic is not null)
            Destroy(_schematic.gameObject);

        _schematic = null;
    }

    public void Init(string schematicName)
    {
        // Generate the schematic
        MethodInfo method = DynamicInvoke.GetMethod("MapEditorReborn", "MapEditorReborn.API.Features.ObjectSpawner.SpawnSchematic", methodCounter: 2);
        method ??= DynamicInvoke.GetMethod("ProjectMER", "ProjectMER.Features.ObjectSpawner.SpawnSchematic", true, 2);

        object schematic = method?.Invoke(null, [schematicName, Vector3.zero]);

        if (method is null)
        {
            LogManager.Error($"[MER Extension] Failed to import MER or ProjectMER schematic {schematicName}!\nMethod not found!");
            Destroy(this);
            return;
        }

        if (schematic is null)
        {
            LogManager.Error($"[MER Extension] Failed to import MER or ProjectMER schematic {schematicName}!\nSchematic not found!");
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

        Transform schematicTransform = monoSchematic.transform;
        schematicTransform.SetParent(transform, false);
        schematicTransform.localPosition = Vector3.zero;
        schematicTransform.localRotation = Quaternion.identity;
    }
}