/*
 * This file is a part of the UncomplicatedCustomRoles project.
 * 
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 * 
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using LabApi.Features.Wrappers;
using UncomplicatedCustomRoles.API.Struct;
using UncomplicatedCustomRoles.Extensions;
using UnityEngine;

namespace UncomplicatedCustomRoles.API.Features
{
    public class SpawnPoint
    {
        /// <summary>
        /// Gets the list of every synced <see cref="SpawnPoint"/> in the server
        /// </summary>
        public static HashSet<SpawnPoint> List { get; } = new();

        /// <summary>
        /// Gets the list of every unsynced <see cref="SpawnPoint"/> in the server
        /// </summary>
        public static HashSet<SpawnPoint> UnsyncedList { get; } = new();

        /// <summary>
        /// Gets the name of the <see cref="SpawnPoint"/>
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the Room ID of the <see cref="SpawnPoint"/>
        /// </summary>
        public string RoomId { get; }

        /// <summary>
        /// Gets the base position of the <see cref="SpawnPoint"/>
        /// </summary>
        public Triplet<float, float, float> PositionBase { get; }

        /// <summary>
        /// Gets the base rotation of the <see cref="SpawnPoint"/>
        /// </summary>
        public Quadruple<float, float, float, float> RotationBase { get; }

        /// <summary>
        /// Gets the base room rotation of the <see cref="SpawnPoint"/>
        /// </summary>
        public Triplet<float, float, float> RoomRotationBase { get; }

        /// <summary>
        /// Gets whether the <see cref="SpawnPoint"/> is synced with the UCS cloud (or local file) or not
        /// </summary>
        [JsonIgnore]
        public bool Sync { get; set; }

        /// <summary>
        /// Gets whether the <see cref="SpawnPoint"/> is fixed in the position or not (if fixed then it's NOT linked to the room)
        /// </summary>
        [JsonIgnore]
        public bool Fixed { get; set; }

        /// <summary>
        /// Gets the position of the <see cref="SpawnPoint"/> as a <see cref="Vector3"/>
        /// </summary>
        [JsonIgnore]
        public Vector3 Position => new(PositionBase.First, PositionBase.Second, PositionBase.Third);

        /// <summary>
        /// Gets the rotation of the <see cref="SpawnPoint"/> as a <see cref="Quaternion"/>
        /// </summary>
        [JsonIgnore]
        public Quaternion Rotation => new(RotationBase.First, RotationBase.Second, RotationBase.Third, RotationBase.Fourth);

        /// <summary>
        /// Gets the room rotation of the <see cref="SpawnPoint"/> as a <see cref="Vector3"/>
        /// </summary>
        [JsonIgnore]
        public Vector3 RoomRotation => new(RoomRotationBase.First, RoomRotationBase.Second, RoomRotationBase.Third);

        /// <summary>
        /// Gets the <see cref="Room"/> linked to the <see cref="SpawnPoint"/>, or null if not found
        /// </summary>
        [JsonIgnore]
        public Room Room => RoomId != "" ? Room.List.FirstOrDefault(room => room.GameObject.name.Replace("Christmas", "").Replace("Halloween", "") == RoomId) : null;

        /// <summary>
        /// Gets a value indicating whether the <see cref="Room"/> property is not <see langword="null"/>.
        /// </summary>
        [JsonIgnore]
        public bool HasRoom => Room is not null;

        [JsonConstructor]
        internal SpawnPoint(string name, string roomId, Triplet<float, float, float> positionBase, Quadruple<float, float, float, float> rotationBase, Triplet<float, float, float> roomRotationBase, bool sync = true, bool @fixed = false)
        {
            Name = name;
            RoomId = roomId.Replace("Christmas", "").Replace("Halloween", "");
            PositionBase = positionBase;
            RotationBase = rotationBase;
            RoomRotationBase = roomRotationBase;
            Sync = sync;
            Fixed = @fixed;

            if (!Sync)
                UnsyncedList.Add(this);
            else
                List.Add(this);
        }

        internal SpawnPoint(string name, Player player) : this(name, player.Room?.GameObject.name ?? string.Empty, (player.Room is not null ? player.Room.Position - player.Position : player.Position).ToTriplet(), new(player.Rotation.x, player.Rotation.y, player.Rotation.z, player.Rotation.w), player.Room?.Rotation.eulerAngles.ToTriplet() ?? new(0f, 0f, 0f)) { }

        /// <summary>
        /// Destroys the <see cref="SpawnPoint"/>, removing it from the list
        /// </summary>
        public void Destroy() => List.Remove(this);

        /// <summary>
        /// Gets the corrected location of the <see cref="SpawnPoint"/>, taking into account the room rotation
        /// </summary>
        /// <returns></returns>
        public Vector3 CorrectLocation()
        {
            if (Fixed)
                return Position;

            if (Room.Rotation.eulerAngles == RoomRotation)
                return Position;

            return Quaternion.AngleAxis(Room.Rotation.eulerAngles.y - RoomRotation.y, Vector3.up) * Position;
        }

        /// <summary>
        /// Sets the specified player's position and rotation to match the spawn point.
        /// </summary>
        /// <remarks>If a room is available, the player's position is set relative to the room's position;
        /// otherwise, the player's position is set to the spawn point's position. In both cases, the player's rotation
        /// is set to match the spawn point.</remarks>
        /// <param name="player">The player to be spawned. Cannot be <c>null</c>.</param>
        public void Spawn(Player player)
        {
            if (HasRoom)
                player.Position = Room.Position - CorrectLocation();
            else
                player.Position = Position;

            player.Rotation = Rotation;
        }

        public override string ToString() => $"SpawnPoint '{Name}' at {(Room != null ? Room.GameObject.name.Replace("Christmas", "").Replace("Halloween", "") : "RoomWasNotFound")} ({Position} @ {RoomRotation}) [{HasRoom}]";

        /// <summary>
        /// Creates a new <see cref="SpawnPoint"/> instance that is not synchronized with the network.
        /// </summary>
        /// <param name="name">The unique name to assign to the spawn point. Cannot be null or empty.</param>
        /// <param name="roomId">The identifier of the room to which the spawn point belongs. Cannot be null or empty.</param>
        /// <param name="positionBase">The base position of the spawn point, specified as a triplet of coordinates.</param>
        /// <param name="rotationBase">The base rotation of the spawn point, specified as a quadruple representing rotation values.</param>
        /// <param name="roomRotationBase">The base rotation of the room, specified as a triplet of rotation values.</param>
        /// <returns>A <see cref="SpawnPoint"/> instance that is not registered for network synchronization.</returns>
        public static SpawnPoint CreateNotSync(string name, string roomId, Triplet<float, float, float> positionBase, Quadruple<float, float, float, float> rotationBase, Triplet<float, float, float> roomRotationBase) => new(name, roomId, positionBase, rotationBase, roomRotationBase, sync: false);

        /// <summary>
        /// Creates a <see cref="SpawnPoint"/> at a fixed position and rotation with the specified name.
        /// </summary>
        /// <param name="name">The name to assign to the spawn point. Cannot be <see langword="null"/>.</param>
        /// <param name="positionBase">The world position where the spawn point will be placed.</param>
        /// <param name="rotationBase">The world rotation to apply to the spawn point.</param>
        /// <returns>A <see cref="SpawnPoint"/> instance configured at the specified position and rotation, with fixed
        /// (non-randomized) placement.</returns>
        public static SpawnPoint CreateFixed(string name, Vector3 positionBase, Quaternion rotationBase) => new(name, "", Triplet<float, float, float>.FromVector3(positionBase), Quadruple<float, float, float, float>.FromQuaternion(rotationBase), new(0f, 0f, 0f), false, true);

        public static SpawnPoint Get(string name) => List.FirstOrDefault(sp => sp.Name == name) ?? UnsyncedList.FirstOrDefault(sp => sp.Name == name);

        public static bool TryGet(string name, out SpawnPoint spawnPoint)
        {
            spawnPoint = Get(name);
            return spawnPoint != null;
        }

        public static bool Exists(string name)
        {
            return List.Any(sp => sp.Name == name) || UnsyncedList.Any(sp => sp.Name == name);
        }
    }
}