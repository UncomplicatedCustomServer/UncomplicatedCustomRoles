using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomRoles.Manager;
using UncomplicatedCustomRoles.Structures;
using MEC;
using Exiled.Events.EventArgs.Player;
using PlayerRoles;
using Exiled.Permissions.Extensions;
using Newtonsoft.Json;
using System.Net.Http;
using Exiled.Events.EventArgs.Server;
using Exiled.CustomRoles;

/*
 * Il mio canto libero - Lucio Battisti
 * 
 * In un mondo che
 * Non ci vuole più
 * Il mio canto libero sei tu
 * E l'immensità
 * Si apre intorno a noi
 * Al di là del limite degli occhi tuoi
 * Nasce il sentimento
 * Nasce in mezzo al pianto
 * E s'innalza altissimo e va
 * E vola sulle accuse della gente
 * A tutti i suoi retaggi indifferente
 * Sorretto da un anelito d'amore
 * Di vero amore
 * In un mondo che (Pietre, un giorno case)
 * Prigioniero è (Ricoperte dalle rose selvatiche)
 * Respiriamo liberi io e te (Rivivono, ci chiamano)
 * E la verità (Boschi abbandonati)
 * Si offre nuda a noi (Perciò sopravvissuti, vergini)
 * E limpida è l'immagine (Si aprono)
 * Ormai (Ci abbracciano)
 * Nuove sensazioni
 * Giovani emozioni
 * Si esprimono purissime in noi
 * La veste dei fantasmi del passato
 * Cadendo lascia il quadro immacolato
 * E s'alza un vento tiepido d'amore
 * Di vero amore
 * E riscopro te
 * 
 * Dolce compagna che
 * Non sai domandare, ma sai
 * Che ovunque andrai
 * Al fianco tuo mi avrai
 * Se tu lo vuoi
 * 
 * Pietre, un giorno case
 * Ricoperte dalle rose selvatiche
 * Rivivono, ci chiamano
 * Boschi abbandonati
 * E perciò sopravvissuti vergini
 * Si aprono, ci abbracciano
 * 
 * In un mondo che
 * Prigioniero è
 * Respiriamo liberi
 * Io e te
 * E la verità
 * Si offre nuda a noi
 * E limpida è l'immagine ormai
 * Nuove sensazioni
 * Giovani emozioni
 * Si esprimono purissime in noi
 * La veste dei fantasmi del passato
 * Cadendo lascia il quadro immacolato
 * E s'alza un vento tiepido d'amore
 * Di vero amore
 * E riscopro te
 */

namespace UncomplicatedCustomRoles.Events
{
    public class EventHandler
    {
        public void OnRoundStarted()
        {
            Plugin.Instance.DoSpawnBasicRoles = false;
            Timing.CallDelayed(5, () =>
            {
                Plugin.Instance.DoSpawnBasicRoles = true;
            });
            foreach (Player Player in Player.List)
            {
                DoEvaluateSpawnForPlayer(Player);
            }
        }

        public void OnPlayerSpawned(SpawnedEventArgs Spawned)
        {
            if (!Plugin.Instance.DoSpawnBasicRoles)
            {
                return;
            }

            if (Plugin.PlayerRegistry.ContainsKey(Spawned.Player.Id))
            {
                return;
            }

            string LogReason = string.Empty;
            if (Plugin.Instance.Config.AllowOnlyNaturalSpawns && !Plugin.RoleSpawnQueue.Contains(Spawned.Player.Id))
            {
                Log.Debug("The player is not in the queue for respawning!");
                return;
            }
            else if (Plugin.RoleSpawnQueue.Contains(Spawned.Player.Id))
            {
                Plugin.RoleSpawnQueue.Remove(Spawned.Player.Id);
                LogReason = "[going with a respawn wave] ";
            }

            Log.Debug($"Player {Spawned.Player.Nickname} spawned {LogReason}, going to assign a role if needed!");

            Timing.CallDelayed(0.1f, () =>
            {
                DoEvaluateSpawnForPlayer(Spawned.Player);
            });
        }

        public void OnDied(DiedEventArgs Died)
        {
            int? CustomRole = SpawnManager.TryGetCustomRole(Died.Player);
            SpawnManager.ClearCustomTypes(Died.Player);
            SpawnManager.TriggerNpcEvent(CustomRole, PlayerEvent.Died, Died);
        }

        public void OnDying(DyingEventArgs Dying)
        {
            SpawnManager.TriggerNpcEvent(PlayerEvent.Dying, Dying);
        }

        public void OnInteractingDoor(InteractingDoorEventArgs InteractingDoor)
        {
            SpawnManager.TriggerNpcEvent(PlayerEvent.InteractingDoor, InteractingDoor);
        }

        public void OnInteractingElevator(InteractingElevatorEventArgs InteractingElevator)
        {
            SpawnManager.TriggerNpcEvent(PlayerEvent.InteractingElevator, InteractingElevator);
        }

        public void OnInteractingLocker(InteractingLockerEventArgs InteractingLocker)
        {
            SpawnManager.TriggerNpcEvent(PlayerEvent.InteractingLocker, InteractingLocker);
        }

        public void OnUsingItem(UsingItemEventArgs UsingItem)
        {
            SpawnManager.TriggerNpcEvent(PlayerEvent.UsingItem, UsingItem);
        }

        public void OnUsedItem(UsedItemEventArgs UsedItem)
        {
            SpawnManager.TriggerNpcEvent(PlayerEvent.UsedItem, UsedItem);
        }

        public void OnHurting(HurtingEventArgs Hurting)
        {
            SpawnManager.TriggerNpcEvent(PlayerEvent.Hurting, Hurting);
        }

        public void OnHurt(HurtEventArgs Hurt)
        {
            SpawnManager.TriggerNpcEvent(PlayerEvent.Hurt, Hurt);
        }

        public void OnShooting(ShootingEventArgs Shooting)
        {
            SpawnManager.TriggerNpcEvent(PlayerEvent.Shooting, Shooting);
        }

        public void OnShot(ShotEventArgs Shot)
        {
            SpawnManager.TriggerNpcEvent(PlayerEvent.Shot, Shot);
        }

        public void OnChangingItem(ChangingItemEventArgs ChangingItem)
        {
            SpawnManager.TriggerNpcEvent(PlayerEvent.ChangingItem, ChangingItem);
        }

        public void OnTriggeringTesla(TriggeringTeslaEventArgs TriggeringTesla)
        {
            SpawnManager.TriggerNpcEvent(PlayerEvent.TriggeringTesla, TriggeringTesla);
        }

        public void OnUsingRadioBattery(UsingRadioBatteryEventArgs UseRadioBattery)
        {
            SpawnManager.TriggerNpcEvent(PlayerEvent.UsingRadioBattery, UseRadioBattery);
        }

        public void OnFlippingCoin(FlippingCoinEventArgs FlippingCoin)
        {
            SpawnManager.TriggerNpcEvent(PlayerEvent.FlippingCoin, FlippingCoin);
        }

        public void OnMakingNoise(MakingNoiseEventArgs MakingNoise)
        {
            SpawnManager.TriggerNpcEvent(PlayerEvent.MakingNoise, MakingNoise);
        }

        public void OnJumping(JumpingEventArgs Jumping)
        {
            SpawnManager.TriggerNpcEvent(PlayerEvent.Jumping, Jumping);
        }

        public void OnTransmitting(TransmittingEventArgs Transmitting)
        {
            SpawnManager.TriggerNpcEvent(PlayerEvent.Transmitting, Transmitting);
        }

        public void OnKilling(KillingPlayerEventArgs KillingPlayer)
        {
            SpawnManager.TriggerNpcEvent(PlayerEvent.KillPlayer, KillingPlayer);
        }

        public void OnSpawning(SpawningEventArgs Spawning)
        {
            SpawnManager.ClearCustomTypes(Spawning.Player);
        }

        public void OnEscaping(EscapingEventArgs Escaping)
        {
            if (Plugin.PlayerRegistry.ContainsKey(Escaping.Player.Id))
            {
                int RoleId = Plugin.PlayerRegistry[Escaping.Player.Id];
                ICustomRole Role = Plugin.CustomRoles[RoleId];

                if (!Role.CanEscape)
                {
                    Escaping.IsAllowed = false;
                }
            }
        }

        public void OnRespawningWave(RespawningTeamEventArgs Respawn)
        {
            foreach (Player Player in Respawn.Players)
            {
                Plugin.RoleSpawnQueue.Add(Player.Id);
            }
        }

        public static IEnumerator<float> DoSpawnPlayer(Player Player, int Id, bool DoBypassRoleOverwrite = true)
        {
            yield return Timing.WaitForSeconds(0.1f);
            SpawnManager.SummonCustomSubclass(Player, Id, DoBypassRoleOverwrite);
        }

        public static void DoEvaluateSpawnForPlayer(Player Player)
        {
            Dictionary<RoleTypeId, List<ICustomRole>> RolePercentage = Factory.RoleIstance();

            foreach (KeyValuePair<int, ICustomRole> Role in Plugin.CustomRoles)
            {
                if (!Role.Value.IgnoreSpawnSystem && Player.List.Count() >= Role.Value.MinPlayers)
                {
                    if (Role.Value.RequiredPermission != null && Role.Value.RequiredPermission != string.Empty && !Player.CheckPermission(Role.Value.RequiredPermission))
                    {
                        Log.Debug($"[NOTICE] Ignoring the role {Role.Value.Id} [{Role.Value.Name}] while creating the list for the player {Player.Nickname} due to: cannot [permissions].");
                        continue;
                    }

                    foreach (RoleTypeId RoleType in Role.Value.CanReplaceRoles)
                    {
                        for (int a = 0; a < Role.Value.SpawnChance; a++)
                        {
                            RolePercentage[RoleType].Add(Role.Value);
                        }
                    }
                }
            }
            if (Plugin.PlayerRegistry.ContainsKey(Player.Id))
            {
                Log.Debug("Was evalutating role select for an already custom role player, stopping");
                return;
            }
            if (RolePercentage.ContainsKey(Player.Role.Type))
            {
                // We can proceed with the chance
                int Chance = new Random().Next(0, 100);
                if (Chance < RolePercentage[Player.Role.Type].Count())
                {
                    // The role exists, good, let's give the player a role
                    int RoleId = RolePercentage[Player.Role.Type].RandomItem().Id;

                    if (Plugin.RolesCount[RoleId] < Plugin.CustomRoles[RoleId].MaxPlayers)
                    {
                        Timing.RunCoroutine(DoSpawnPlayer(Player, RoleId, false));
                        Plugin.RolesCount[RoleId]++;
                        Log.Debug($"Player {Player.Nickname} spawned as CustomRole {RoleId}");
                    }
                    else
                    {
                        Log.Debug($"Player {Player.Nickname} won't be spawned as CustomRole {RoleId} because it has reached the maximus number");
                    }
                }
            }
        }

        public async void TaskGetHttpResponse()
        {
            long Start = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            HttpResponseMessage RawData = await Plugin.HttpClient.GetAsync($"{Plugin.Instance.PresenceUrl}?port={Server.Port}&cores={Environment.ProcessorCount}&ram=0&version={Plugin.Instance.Version}");
            string Data = RawData.Content.ReadAsStringAsync().Result;
            Dictionary<string, string> Response = JsonConvert.DeserializeObject<Dictionary<string, string>>(Data);

            if (Response["status"] == "200")
            {
                Log.Info($"[UCR Online Presence by UCS] >> Data successflly put in the UCS server - Took (only) {DateTimeOffset.Now.ToUnixTimeMilliseconds() - Start}ms! - Server says: {Response["message"]}");
            }
            else
            {
                Plugin.Instance.FailedHttp++;
                Log.Warn($"[UCR Online Presence by UCS] >> Failed to put data in the UCS server for presence! HTTP-CODE: {Response["status"]}, server says: {Response["message"]}");
            }
        }

        public IEnumerator<float> DoHttpPresence()
        {
            Log.Info("[UCR Online Presence by UCS] >> Started the presence task manager");
            while (true)
            {
                if (Plugin.Instance.FailedHttp > 5)
                {
                    Log.Error($"[UCR Online Presence by UCS] >> Failed to put data on stream for {Plugin.Instance.FailedHttp} times, disabling the function...");
                    yield break;
                }

                TaskGetHttpResponse();
                yield return Timing.WaitForSeconds(500);
            }
        }
    }
}