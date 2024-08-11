﻿using Exiled.API.Features;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomRoles.API.Struct;
using UncomplicatedCustomRoles.Commands;
using UncomplicatedCustomRoles.Extensions;
using UnityEngine;

namespace UncomplicatedCustomRoles.API.Features
{
    internal class SpawnPoint
    {
        public static HashSet<SpawnPoint> List { get; } = new();

        public string Name { get; }

        public string RoomId { get; }

        public Triplet<float, float, float> PositionBase { get; }

        public Quadruple<float, float, float, float> RotationBase { get; }

        public Triplet<float, float, float> RoomRotationBase { get; }

        [JsonIgnore]
        public Vector3 Position => new(PositionBase.First, PositionBase.Second, PositionBase.Third);

        [JsonIgnore]
        public Quaternion Rotation => new(RotationBase.First, RotationBase.Second, RotationBase.Third, RotationBase.Fourth);

        [JsonIgnore]
        public Vector3 RoomRotation => new(RoomRotationBase.First, RoomRotationBase.Second, RoomRotationBase.Third);

        [JsonIgnore]
        public Room Room => Room.List.Where(room => room.Identifier.name == RoomId).FirstOrDefault();

        [JsonIgnore]
        public bool HasRoom => Room is not null;

        [JsonConstructor]
        public SpawnPoint(string name, string roomId, Triplet<float, float, float> positionBase, Quadruple<float, float, float, float> rotationBase, Triplet<float, float, float> roomRotationBase)
        {
            Name = name;
            RoomId = roomId;
            PositionBase = positionBase;
            RotationBase = rotationBase;
            RoomRotationBase = roomRotationBase;
            List.Add(this);
        }

        public SpawnPoint(string name, Player player) : this(name, player.CurrentRoom.Identifier.name, (player.CurrentRoom.Position - player.Position).ToTriplet(), new(player.Rotation.x, player.Rotation.y, player.Rotation.z, player.Rotation.w), player.CurrentRoom.Rotation.eulerAngles.ToTriplet()) { }

        public void Destroy() => List.Remove(this);

        public Vector3 CorrectLocation()
        {
            if (Room.Rotation.eulerAngles == RoomRotation)
                return Position;

            return Quaternion.AngleAxis(Room.Rotation.eulerAngles.y - RoomRotation.y, Vector3.up) * Position;
        }

        public void Spawn(Player player)
        {
            player.Position = Room.Position - CorrectLocation();
            player.Rotation = Rotation;
        }

        public override string ToString() => $"SpawnPoint '{Name}' at {Room.Name} ({Position} @ {RoomRotation}) [{HasRoom}]";

        public static SpawnPoint Get(string name) => List.Where(sp => sp.Name == name).FirstOrDefault();

        public static bool TryGet(string name, out SpawnPoint spawnPoint)
        {
            spawnPoint = Get(name);
            return spawnPoint != null;
        }

        public static bool Exists(string name) => List.Where(sp => sp.Name == name).Count() > 0;
    }
}