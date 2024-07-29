using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using PlayerRoles;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomRoles.Interfaces;
using UnityEngine;
using Exiled.CustomItems.API.Features;
using System;
using UncomplicatedCustomRoles.Extensions;
using MEC;
using Exiled.Permissions.Extensions;
using UncomplicatedCustomRoles.API.Features;

// Mormora, la gente mormora
// falla tacere praticando l'allegria

namespace UncomplicatedCustomRoles.Manager
{
    internal class SpawnManager
    {
        public static void ClearCustomTypes(Player player)
        {
            if (SummonedCustomRole.TryGet(player, out SummonedCustomRole role))
                role.Destroy();
        }

        public static void SummonCustomSubclass(Player player, int id, bool doBypassRoleOverwrite = true)
        {
            // Does the role exists?
            if (!CustomRole.CustomRoles.ContainsKey(id))
            {
                LogManager.Warn($"Sorry but the role with the Id {id} is not registered inside UncomplicatedCustomRoles!", "CR0092");
                return;
            }

            ICustomRole Role = CustomRole.CustomRoles[id];

            if (Role.SpawnSettings is null)
            {
                LogManager.Warn($"Tried to spawn a custom role without spawn_settings, aborting the SummonCustomSubclass(...) action!\nRole: {Role.Name} ({Role.Id})", "CR0093");
                return;
            }

            if (!doBypassRoleOverwrite && !Role.SpawnSettings.CanReplaceRoles.Contains(player.Role.Type))
            {
                LogManager.Debug($"Can't spawn the player {player.Nickname} as UCR custom role {Role.Name} because it's role is not in the overwrittable list of custom role!\nStrange because this should be managed correctly by the plugin!");
                return;
            }

            // This will allow us to avoid the loop of another OnSpawning
            Spawn.Spawning.TryAdd(player.Id);

            Vector3 BasicPosition = player.Position;

            RoleSpawnFlags SpawnFlag = RoleSpawnFlags.None;

            if (Role.SpawnSettings.Spawn == SpawnLocationType.KeepRoleSpawn)
                SpawnFlag = RoleSpawnFlags.UseSpawnpoint;

            player.Role.Set(Role.Role, SpawnFlag);

            if (Role.SpawnSettings.Spawn == SpawnLocationType.KeepCurrentPositionSpawn)
                player.Position = BasicPosition;

            if (SpawnFlag == RoleSpawnFlags.None)
            {
                switch (Role.SpawnSettings.Spawn)
                {
                    case SpawnLocationType.ZoneSpawn:
                        player.Position = Room.List.Where(room => room.Zone == Role.SpawnSettings.SpawnZones.RandomItem() && room.TeslaGate is null).GetRandomValue().Position.AddY(1.5f);
                        break;
                    case SpawnLocationType.CompleteRandomSpawn:
                        player.Position = Room.List.Where(room => room.TeslaGate is null).GetRandomValue().Position.AddY(1.5f);
                        break;
                    case SpawnLocationType.RoomsSpawn:
                        player.Position = Room.Get(Role.SpawnSettings.SpawnRooms.RandomItem()).Position.AddY(1.5f);

                        if (Role.SpawnSettings.SpawnOffset != new Vector3())
                            player.Position += Role.SpawnSettings.SpawnOffset;

                        break;
                    case SpawnLocationType.PositionSpawn:
                        player.Position = Role.SpawnSettings.SpawnPosition;
                        break;
                };
            }

            SummonSubclassApplier(player, Role);
        }

        public static void SummonSubclassApplier(Player Player, ICustomRole Role)
        {
            Player.ResetInventory(Role.Inventory);

            if (Role.CustomItemsInventory.Count() > 0)
                foreach (uint ItemId in Role.CustomItemsInventory)
                    if (!Player.IsInventoryFull)
                        try
                        {
                            if (Exiled.Loader.Loader.GetPlugin("UncomplicatedCustomItems") is not null)
                            {
                                Type AssemblyType = Exiled.Loader.Loader.GetPlugin("UncomplicatedCustomItems").Assembly.GetType("UncomplicatedCustomItems.API.Utilities");
                                if ((bool)AssemblyType?.GetMethod("IsCustomItem")?.Invoke(null, new object[] { ItemId }))
                                {
                                    object CustomItem = AssemblyType?.GetMethod("GetCustomItem")?.Invoke(null, new object[] { ItemId });

                                    Exiled.Loader.Loader.GetPlugin("UncomplicatedCustomItems").Assembly.GetType("UncomplicatedCustomItems.API.Features.SummonedCustomItem")?.GetMethods().Where(method => method.Name == "Summon" && method.GetParameters().Length == 2).FirstOrDefault()?.Invoke(null, new object[]
                                    {
                                        CustomItem,
                                        Player
                                    });
                                }
                            }
                            else
                                CustomItem.Get(ItemId)?.Give(Player);
                        }
                        catch (Exception ex)
                        {
                            LogManager.Debug($"Error while giving a custom item.\nError: {ex.Message}");
                        }

            if (Role.Ammo.GetType() == typeof(Dictionary<AmmoType, ushort>) && Role.Ammo.Count() > 0)
                foreach (KeyValuePair<AmmoType, ushort> Ammo in Role.Ammo)
                    Player.AddAmmo(Ammo.Key, Ammo.Value);

            if (Role.CustomInfo != null && Role.CustomInfo != string.Empty)
                Player.CustomInfo += $"\n{Role.CustomInfo}";

            // Apply every required stats
            Role.Health?.Apply(Player);
            Role.Ahp?.Apply(Player);
            Role.Stamina?.Apply(Player);

            List<IEffect> PermanentEffects = new();
            if (Role.Effects.Count() > 0 && Role.Effects != null)
            {
                foreach (IEffect effect in Role.Effects)
                {
                    if (effect.Duration < 0)
                    {
                        effect.Duration = 15f;
                        PermanentEffects.Add(effect);
                        continue;
                    }
                    Player.EnableEffect(effect.EffectType, effect.Duration);
                    Player.ChangeEffectIntensity(effect.EffectType, effect.Intensity, effect.Duration);
                }
            }

            if (Role.Scale != Vector3.zero && Role.Scale != Vector3.one)
                Player.Scale = Role.Scale;

            if (Role.SpawnBroadcast != string.Empty)
            {
                Player.ClearBroadcasts();
                Player.Broadcast(Role.SpawnBroadcastDuration, Role.SpawnBroadcast);
            }

            if (Role.SpawnHint != string.Empty)
                Player.ShowHint(Role.SpawnHint, Role.SpawnHintDuration);

            KeyValuePair<string, string>? Badge = null;
            if (Role.BadgeName is not null && Role.BadgeName.Length > 1 && Role.BadgeColor is not null && Role.BadgeColor.Length > 2)
            {
                Badge = new(Player.RankName ?? "", Player.RankColor ?? "");
                LogManager.Debug($"Badge detected, putting {Role.BadgeName}@{Role.BadgeColor} to player {Player.Id}");

                Player.RankName = Role.BadgeName;
                Player.RankColor = Role.BadgeColor;
            }

            // Changing nickname if needed
            bool ChangedNick = false;
            if (Plugin.Instance.Config.AllowNicknameEdit && Role.Nickname is not null && Role.Nickname != string.Empty)
            {
                Role.Nickname = Role.Nickname.Replace("%dnumber%", new System.Random().Next(1000, 9999).ToString()).Replace("%nick%", Player.Nickname).Replace("%rand%", new System.Random().Next(0, 9).ToString()).Replace("%unitid%", Player.UnitId.ToString()).Replace("%unitname%", Player.UnitName);
                if (Role.Nickname.Contains(","))
                    Player.DisplayNickname = Role.Nickname.Split(',').RandomItem();
                else
                    Player.DisplayNickname = Role.Nickname;

                ChangedNick = true;
            }

            if (Role.RoleAppearance != Role.Role)
            {
                LogManager.Debug($"Changing the appearance of the role {Role.Id} [{Role.Name}] to {Role.RoleAppearance}");
                Timing.CallDelayed(1f, () =>
                {
                    Player.ChangeAppearance(Role.RoleAppearance, true);
                });
            }

            LogManager.Debug($"{Player} successfully spawned as {Role.Name} ({Role.Id})!");

            new SummonedCustomRole(Player, Role, Badge, PermanentEffects, ChangedNick);

            LogManager.Debug($"{Player} successfully spawned as {Role.Name} ({Role.Id})! [2VDS]");
        }

        public static KeyValuePair<bool, object> ParseEscapeRole(string roleAfterEscape, Player player)
        {
            List<string> Role = new();

            if (roleAfterEscape is not null && roleAfterEscape != string.Empty)
            {
                if (roleAfterEscape.Contains(","))
                {
                    string[] roles = roleAfterEscape.Split(',');
                    foreach (string role in roles)
                        foreach (string rolePart in role.Split(':')) 
                            Role.Add(rolePart);
                }

                int SearchIndex = 0;

                if (player.IsCuffed && player.Cuffer is not null)
                    SearchIndex = player.Cuffer.Role.Team switch
                    {
                        Team.FoundationForces => 2,
                        Team.ChaosInsurgency => 4,
                        Team.Scientists => 6,
                        Team.ClassD => 8,
                        _ => 0
                    };

                // Let's proceed
                if (Role.Count >= SearchIndex + 2)
                    if (Role[SearchIndex] is "IR")
                        return new(false, Role[SearchIndex + 1]);
                    else if (Role[SearchIndex] is "CR")
                        return new(true, Role[SearchIndex + 1]);
                    else
                        LogManager.Error($"Error while parsing role_after_escape for player {player.Nickname} ({player.Id}): the first string was not 'IR' nor 'CR', found '{Role[SearchIndex]}'!\nPlease see our documentation: https://github.com/UncomplicatedCustomServer/UncomplicatedCustomRoles/wiki/Specifics#role-after-escape");
                else
                    LogManager.Debug($"Error while parsing role_after_escape: index is out of range!\nExpected to found {SearchIndex}, total: {Role.Count}!");
            }

            return new(false, null);
        }

#nullable enable
#pragma warning disable CS8602 // <Element> can be null at this point! (added a check!)
        public static ICustomRole? DoEvaluateSpawnForPlayer(Player player, RoleTypeId? role = null)
        {
            role ??= player.Role.Type;

            RoleTypeId NewRole = (RoleTypeId)role;

            Dictionary<RoleTypeId, List<ICustomRole>> RolePercentage = new()
            {
                { RoleTypeId.ClassD, new() },
                { RoleTypeId.Scientist, new() },
                { RoleTypeId.NtfPrivate, new() },
                { RoleTypeId.NtfSergeant, new() },
                { RoleTypeId.NtfCaptain, new() },
                { RoleTypeId.NtfSpecialist, new() },
                { RoleTypeId.ChaosConscript, new() },
                { RoleTypeId.ChaosMarauder, new() },
                { RoleTypeId.ChaosRepressor, new() },
                { RoleTypeId.ChaosRifleman, new() },
                { RoleTypeId.Tutorial, new() },
                { RoleTypeId.Scp049, new() },
                { RoleTypeId.Scp0492, new() },
                { RoleTypeId.Scp079, new() },
                { RoleTypeId.Scp173, new() },
                { RoleTypeId.Scp939, new() },
                { RoleTypeId.Scp096, new() },
                { RoleTypeId.Scp106, new() },
                { RoleTypeId.Scp3114, new() },
                { RoleTypeId.FacilityGuard, new() }
            };

            foreach (ICustomRole Role in CustomRole.CustomRoles.Values.Where(cr => cr.SpawnSettings is not null))
                if (!Role.IgnoreSpawnSystem && Player.List.Count() >= Role.SpawnSettings.MinPlayers && SummonedCustomRole.Count(Role) < Role.SpawnSettings.MaxPlayers)
                {
                    if (Role.SpawnSettings.RequiredPermission != null && Role.SpawnSettings.RequiredPermission != string.Empty && !player.CheckPermission(Role.SpawnSettings.RequiredPermission))
                    {
                        LogManager.Debug($"[NOTICE] Ignoring the role {Role.Id} [{Role.Name}] while creating the list for the player {player.Nickname} due to: cannot [permissions].");
                        continue;
                    }

                    foreach (RoleTypeId RoleType in Role.SpawnSettings.CanReplaceRoles)
                        for (int a = 0; a < Role.SpawnSettings.SpawnChance; a++)
                            RolePercentage[RoleType].Add(Role);
                }

            if (player.HasCustomRole())
            {
                LogManager.Debug("Was evalutating role select for an already custom role player, stopping");
                return null;
            }

            if (RolePercentage.ContainsKey(player.Role.Type))
                if (new System.Random().Next(0, 100) < RolePercentage[NewRole].Count())
                    return CustomRole.CustomRoles[RolePercentage[NewRole].RandomItem().Id];

            return null;
        }
    }  
}