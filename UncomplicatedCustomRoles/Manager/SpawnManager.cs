using Exiled.API.Enums;
using Exiled.API.Features;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Exiled.CustomItems.API.Features;
using System;
using UncomplicatedCustomRoles.Extensions;
using MEC;
using Exiled.Permissions.Extensions;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Struct;
using UncomplicatedCustomRoles.API.Enums;
using UncomplicatedCustomRoles.API.Interfaces;
using Exiled.API.Extensions;
using PlayerRoles;
using PlayerStatsSystem;
using Subtitles;
using Utils.Networking;
using UncomplicatedCustomRoles.API.Features.CustomModules;
using System.Text.RegularExpressions;
using UncomplicatedCustomRoles.Integrations;

// Mormora, la gente mormora
// falla tacere praticando l'allegria

namespace UncomplicatedCustomRoles.Manager
{
    internal class SpawnManager
    {
        public static IReadOnlyDictionary<string, string> colorMap = new Dictionary<string, string>()
        {
            { "pink", "#FF96DE" },
            { "red", "#C50000" },
            { "brown", "#944710" },
            { "silver", "#A0A0A0" },
            { "light_green", "#32CD32" },
            { "crimson", "#DC143C" },
            { "cyan", "#00B7EB" },
            { "aqua", "#00FFFF" },
            { "deep_pink", "#FF1493" },
            { "tomato", "#FF6448" },
            { "yellow", "#FAFF86" },
            { "magenta", "#FF0090" },
            { "blue_green", "#4DFFB8" },
            { "orange", "#FF9966" },
            { "lime", "#BFFF00" },
            { "green", "#228B22" },
            { "emerald", "#50C878" },
            { "carmine", "#960018" },
            { "nickel", "#727472" },
            { "mint", "#98FB98" },
            { "army_green", "#4B5320" },
            { "pumpkin", "#EE7600" }
        };

        private static int DiffTargetCount { get; set; } = 0;

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

            if (Role is null)
            {
                LogManager.Warn($"Tried to spawn a custom role with the Id {id} but it seems to not exists!");
                return;
            }

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

            if (Role.SpawnSettings.Spawn == SpawnType.KeepRoleSpawn)
                SpawnFlag = RoleSpawnFlags.UseSpawnpoint;

            player.Role.Set(Role.Role, SpawnFlag);

            if (Role.SpawnSettings.Spawn == SpawnType.KeepCurrentPositionSpawn)
                player.Position = BasicPosition;

            if (SpawnFlag == RoleSpawnFlags.None)
            {
                switch (Role.SpawnSettings.Spawn)
                {
                    case SpawnType.ZoneSpawn:
                        player.Position = Room.List.Where(room => room.Zone == Role.SpawnSettings.SpawnZones.RandomItem() && room.TeslaGate is null).GetRandomValue().Position.AddY(1.5f);
                        break;
                    case SpawnType.CompleteRandomSpawn:
                        player.Position = Room.List.Where(room => room.TeslaGate is null).GetRandomValue().Position.AddY(1.5f);
                        break;
                    case SpawnType.RoomsSpawn:
                        LogManager.Silent($"Going to spawn CR {Role.Name} ({Role.Id}) ({player.Nickname}) at a Room - Count: {Role.SpawnSettings.SpawnRooms.Count}");
                        player.Position = Room.Get(Role.SpawnSettings.SpawnRooms.RandomItem()).Position.AddY(1.5f);
                        break;
                    case SpawnType.SpawnPointSpawn:
                        if (Role.SpawnSettings.SpawnPoints is not null && Role.SpawnSettings.SpawnPoints.GetType() == typeof(List<string>) && SpawnPoint.TryGet(Role.SpawnSettings.SpawnPoints.RandomItem(), out SpawnPoint spawn))
                            spawn.Spawn(player);
                        else
                        {
                            LogManager.Warn($"Failed to spawn player {player.Nickname} ({player.Id}) as CustomRole {Role.Name} ({Role.Id}): selected SpawnPoint '{Role.SpawnSettings.SpawnPoints}' does not exists, set the spawn position to the previous one...");
                            player.Position = BasicPosition;
                        }
                        break;
                    case SpawnType.ClassDCell:
                        player.Position = RoleTypeId.ClassD.GetRandomSpawnLocation().Position;
                        break;
                };
            }

            SummonSubclassApplier(player, Role);
        }

        public static void SummonSubclassApplier(Player Player, ICustomRole Role)
        {
            Player.ResetInventory(Role.Inventory);

            LogManager.Silent($"Can we assign custom roles? Let's see: {Role.CustomItemsInventory.Count()}");
            if (Role.CustomItemsInventory.Count() > 0)
                foreach (uint itemId in Role.CustomItemsInventory)
                    if (!Player.IsInventoryFull)
                        try
                        {
                            if (UCI.HasCustomItem(itemId, out _))
                                UCI.GiveCustomItem(itemId, Player);
                            else
                                CustomItem.Get(itemId)?.Give(Player);
                        }
                        catch (Exception ex)
                        {
                            LogManager.Debug($"Error while giving a custom item.\nError: {ex.Message}");
                        }

            Player.ClearAmmo();
            if (Role.Ammo.GetType() == typeof(Dictionary<AmmoType, ushort>) && Role.Ammo.Count() > 0)
                foreach (KeyValuePair<AmmoType, ushort> Ammo in Role.Ammo)
                    Player.AddAmmo(Ammo.Key, Ammo.Value);

            Player.ReferenceHub.nicknameSync.Network_playerInfoToShow |= PlayerInfoArea.Nickname;

            PlayerInfoArea InfoArea = Player.ReferenceHub.nicknameSync.Network_playerInfoToShow;

            if (!Role.OverrideRoleName && (Role.CustomFlags & CustomFlags.ShowOnlyCustomInfo) == CustomFlags.ShowOnlyCustomInfo)
                Player.ReferenceHub.nicknameSync.Network_playerInfoToShow &= ~PlayerInfoArea.Role;

            if (Role.CustomInfo != null && Role.CustomInfo != string.Empty)
                if (Role.OverrideRoleName)
                    Player.ApplyCustomInfoAndRoleName(Role.CustomInfo, Role.Name);
                else
                    Player.ApplyClearCustomInfo(Role.CustomInfo);

            // Apply every required stats
            Role.Health?.Apply(Player);
            Role.Ahp?.Apply(Player);
            Role.Stamina?.Apply(Player);

            if (Role.Scale != Vector3.zero && Role.Scale != Vector3.one)
                Player.Scale = Role.Scale;

            List<IEffect> PermanentEffects = new();
            if (Role.Effects.Count() > 0 && Role.Effects != null)
            {
                foreach (IEffect effect in Role.Effects)
                {
                    if (effect.Duration < 0)
                    {
                        effect.Duration = int.MaxValue;
                        PermanentEffects.Add(effect);
                        continue;
                    }
                    Player.EnableEffect(effect.EffectType, effect.Duration);
                    Player.ChangeEffectIntensity(effect.EffectType, effect.Intensity, effect.Duration);
                }
            }

            if (Role.SpawnBroadcast != string.Empty)
            {
                Player.ClearBroadcasts();
                Player.Broadcast(Role.SpawnBroadcastDuration, Role.SpawnBroadcast);
            }

            if (Role.SpawnHint != string.Empty)
                Player.ShowHint(Role.SpawnHint, Role.SpawnHintDuration);

            Triplet<string, string, bool>? Badge = null;
            if (Role.BadgeName is not null && Role.BadgeName.Length > 1 && Role.BadgeColor is not null && Role.BadgeColor.Length > 2)
            {
                Badge = new(Player.RankName ?? "", Player.RankColor ?? "", Player.ReferenceHub.serverRoles.HasBadgeHidden);
                LogManager.Debug($"Badge detected, putting {Role.BadgeName}@{Role.BadgeColor} to player {Player.Id}");

                Player.RankName = Role.BadgeName.Replace("@hidden", "");
                Player.RankColor = Role.BadgeColor;

                if (Role.BadgeName.Contains("@hidden"))
                    if (Player.ReferenceHub.serverRoles.TryHideTag())
                        LogManager.Debug("Tag successfully hidden!");
            }

            // Changing nickname if needed
            bool ChangedNick = false;
            if (Plugin.Instance.Config.AllowNicknameEdit && Role.Nickname is not null && Role.Nickname != string.Empty)
            {
                string Nick = Role.Nickname.Replace("%dnumber%", new System.Random().Next(1000, 9999).ToString()).Replace("%nick%", Player.Nickname).Replace("%rand%", new System.Random().Next(0, 9).ToString()).Replace("%unitid%", Player.UnitId.ToString()).Replace("%unitname%", Player.UnitName);
                if (Role.Nickname.Contains(","))
                    Player.DisplayNickname = Nick.Split(',').RandomItem();
                else
                    Player.DisplayNickname = Nick;

                ChangedNick = true;
            }

            // We need the role appereance also here!
            if (Role.RoleAppearance != Role.Role)
            {
                LogManager.Debug($"Changing the appearance of the role {Role.Id} [{Role.Name}] to {Role.RoleAppearance}");
                Timing.CallDelayed(0.75f, () => Player.ChangeAppearance(Role.RoleAppearance, LoadAppearanceAffectedPlayers(Player), true));
            }

            LogManager.Debug($"{Player} successfully spawned as {Role.Name} ({Role.Id})!");

            new SummonedCustomRole(Player, Role, Badge, PermanentEffects, InfoArea, ChangedNick);

            if (Spawn.Spawning.Contains(Player.Id))
                Spawn.Spawning.Remove(Player.Id);

            if (API.Features.Escape.Bucket.Contains(Player.Id))
                API.Features.Escape.Bucket.Remove(Player.Id);

            LogManager.Debug($"{Player} successfully spawned as {Role.Name} ({Role.Id})! [2VDS]");
        }

        public static KeyValuePair<bool, object> ParseEscapeRole(Dictionary<string, string> roleAfterEscape, Player player)
        {
            Dictionary<Team, KeyValuePair<bool, object>> AsCuffedByInternalTeam = new();
            // Dictionary<uint, KeyValuePair<bool, object>> AsCuffedByCustomTeam = new(); we will add the support to UCT and UIU-RS
            // cuffed by InternalTeam FoundationForces
            //   0     1       2             3           = 4
            Dictionary<int, KeyValuePair<bool, object>> AsCuffedByCustomRole = new();
            KeyValuePair<bool, object> Default = new(false, RoleTypeId.Spectator);

            foreach (KeyValuePair<string, string> kvp in roleAfterEscape)
            {
                KeyValuePair<bool, object> Data = ParseEscapeString(kvp.Value);
                if (kvp.Key is "default")
                    Default = Data;
                else
                {
                    List<string> Elements = kvp.Key.Split(' ').ToList();

                    if (Elements.Count != 4)
                    {
                        LogManager.Warn($"Failed to parse an EscapeRole[key]: syntax should be cuffed by <source> <id>, found {Elements.Count} args!\nSource: {kvp.Key}");
                        return new(false, RoleTypeId.Spectator);
                    }

                    if (Elements[0] is not "cuffed")
                    {
                        LogManager.Warn($"Failed to parse an EscapeRole[key]: syntax should be cuffed by <source> <id>, found {Elements.Count} args!\nSource: {kvp.Key}");
                        return new(false, RoleTypeId.Spectator);
                    }

                    if (Elements[1] is not "by")
                    {
                        LogManager.Warn($"Failed to parse an EscapeRole[key]: syntax should be cuffed by <source> <id>, found {Elements.Count} args!\nSource: {kvp.Key}");
                        return new(false, RoleTypeId.Spectator);
                    }

                    if ((Elements[2] is "InternalTeam" || Elements[2] is "IT") && Enum.TryParse(Elements[3], out Team team))
                        AsCuffedByInternalTeam.TryAdd(team, Data);
                    else if ((Elements[2] is "CustomRole" || Elements[2] is "CR") && int.TryParse(Elements[3], out int id) && CustomRole.CustomRoles.ContainsKey(id))
                        AsCuffedByCustomRole.TryAdd(id, Data);
                    else
                        LogManager.Warn($"Function SpawnManager::ParseEscapeRole[2](<...>) failed!\nPossible causes can be:\n- The source is not valid. Allowed: InternalTeam / IT / CustomRole / CR. Found: {Elements[2]}\n- The target is not a CustomRole / InternalRole. Found: {Elements[3]} (int32: {int.Parse(Elements[3])}");
                }
            }

            // Now let's assign
            if (!player.IsCuffed)
                return Default;
            else if (player.IsCuffed && player.Cuffer is not null)
                if (player.Cuffer.TryGetSummonedInstance(out SummonedCustomRole role) && AsCuffedByCustomRole.ContainsKey(role.Role.Id))
                    return AsCuffedByCustomRole[role.Role.Id];
                else if (AsCuffedByInternalTeam.ContainsKey(player.Cuffer.Role.Team))
                    return AsCuffedByInternalTeam[player.Cuffer.Role.Team];

            LogManager.Silent($"Returing default type for escaping evaluation of player {player.Id} who's cuffed by {player.Cuffer?.Role.Team}");
            return Default;
        }

        public static KeyValuePair<bool, object> ParseEscapeString(string escape)
        {
            List<string> Elements = escape.Split(' ').ToList();
            if (Elements.Count != 2)
            {
                LogManager.Warn($"Failed to parse an EscapeString[value]: syntax should be <source> <id> (2 args), found {Elements.Count} args!\nSource: {escape}");
                return new(false, RoleTypeId.Spectator);
            }

            if (Elements[0] is "CustomRole" || Elements[0] is "CR")
                return new(true, int.Parse(Elements[1]));
            else if ((Elements[0] is "InternalRole" || Elements[0] is "IR") && Enum.TryParse(Elements[1], out RoleTypeId role))
                return new(false, role);
            else
                LogManager.Warn($"Function SpawnManager::ParseEscapeString(string escape) failed!\nPossible causes can be:\n- The source is not valid. Allowed: InternalRole / IR / CustomRole / CR. Found: {Elements[0]}\n- The target is not a CustomRole / InternalRole. Found: {Elements[1]} (int32: {int.Parse(Elements[1])}");

            return new(false, RoleTypeId.Spectator);
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
                if (!Role.IgnoreSpawnSystem && Player.List.Count(pl => !pl.IsHost) >= Role.SpawnSettings.MinPlayers && SummonedCustomRole.Count(Role) < Role.SpawnSettings.MaxPlayers)
                {
                    if (Role.SpawnSettings.RequiredPermission is not null && Role.SpawnSettings.RequiredPermission != string.Empty && !player.CheckPermission(Role.SpawnSettings.RequiredPermission))
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

            if (RolePercentage.ContainsKey(NewRole))
                if (UnityEngine.Random.Range(0, 100) < RolePercentage[NewRole].Count())
                    return CustomRole.CustomRoles[RolePercentage[NewRole].RandomItem().Id];

            return null;
        }

        public static void UpdateChaosModifier()
        {
            int diff = 0;
            foreach (SummonedCustomRole role in SummonedCustomRole.List.Where(role => role.IsOverwrittenRole))
            {
                if (role.Role.Team is not Team.SCPs && PlayerRolesUtils.GetTeam(role.Role.Role) is Team.SCPs)
                    diff--;
                else if (role.Role.Team is Team.SCPs && PlayerRolesUtils.GetTeam(role.Role.Role) is not Team.SCPs)
                    diff++;
            }

            Round.ChaosTargetCount -= DiffTargetCount;

            DiffTargetCount = diff;
            Round.ChaosTargetCount += diff;
        }

        public static void HandleRecontainmentAnnoucement(CustomScpAnnouncer element)
        {
            HandleRecontainmentAnnoucement(element.DamageHandler, element.Instance);
            element.Execute();
        }

        public static void HandleRecontainmentAnnoucement(DamageHandlerBase baseHandler, SummonedCustomRole role)
        {
            float num = AlphaWarheadController.Detonated ? 3.5f : 1f;
            TryGetPublicFormat(role.Role.Name, role.Role.Role, out string cassie, out string subtitle);
            NineTailedFoxAnnouncer.singleton.ServerOnlyAddGlitchyPhrase($"{cassie} {baseHandler.CassieDeathAnnouncement.Announcement}", UnityEngine.Random.Range(0.1f, 0.14f) * num, UnityEngine.Random.Range(0.07f, 0.08f) * num);
            List<SubtitlePart> list = new()
            {
                new(SubtitleType.SCP, new string[] { subtitle }),
            };
            list.AddRange(baseHandler.CassieDeathAnnouncement.SubtitleParts);
            new SubtitleMessage(list.ToArray()).SendToAuthenticated(0);
        }

        private static void TryGetPublicFormat(string input, RoleTypeId def, out string cassie, out string subtitle)
        {
            if (!input.Contains("SCP-"))
                NineTailedFoxAnnouncer.ConvertSCP(def, out subtitle, out cassie);
            else
                TryExtractScpNumber(input, out cassie, out subtitle);
        }

        private static void TryExtractScpNumber(string input, out string cassie, out string subtitle)
        {
            char[] allowed = new[] { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0' }; 
            cassie = string.Empty;
            subtitle = string.Empty;
            foreach (char c in ToPublicFormat(Regex.Replace(input, "<.*?>", string.Empty)).ToCharArray())
                if (allowed.Contains(c))
                {
                    cassie += $"{c} ";
                    subtitle += c;
                }

            if (ClutterSpawner.IsHolidayActive(Holidays.AprilFools))
            {
                cassie = "1 0 4";
                subtitle = "104";
            }
        }

        private static string ToPublicFormat(string input) => input.Replace("SCP", "").Replace(" ", "").ToUpper();

        private static IEnumerable<Player> LoadAppearanceAffectedPlayers(Player target)
        {
            List<Player> result = new();
            foreach (Player player in Player.List.Where(p => p.Id != target.Id))
                if (player.TryGetSummonedInstance(out SummonedCustomRole role) && !role.HasModule<NotAffectedByAppearance>())
                    result.Add(player);
                else if (!player.TryGetSummonedInstance(out _))
                    result.Add(player);

            return result;
        }
    }  
}