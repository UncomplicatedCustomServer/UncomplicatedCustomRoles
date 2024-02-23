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
using System.Net.Http;
using Exiled.Events.EventArgs.Server;
using Newtonsoft.Json;

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
        protected CoroutineHandle EffectCoroutine;
        public void OnRoundStarted()
        {
            Plugin.PlayerRegistry = new();
            Plugin.RolesCount = new();
            foreach (KeyValuePair<int, ICustomRole> Data in Plugin.CustomRoles)
            {
                Plugin.RolesCount[Data.Key] = new();
            }
            Plugin.Instance.DoSpawnBasicRoles = false;
            if (EffectCoroutine.IsRunning)
            {
                Timing.KillCoroutines(EffectCoroutine);
            }
            EffectCoroutine = Timing.RunCoroutine(DoSetInfiniteEffectToPlayers());
            Timing.CallDelayed(5, () =>
            {
                Plugin.Instance.DoSpawnBasicRoles = true;
            });
            foreach (Player Player in Player.List.Where(player => !player.IsNPC))
            {
                DoEvaluateSpawnForPlayer(Player);
            }
        }

        public void OnPlayerSpawned(SpawnedEventArgs Spawned)
        {
            SpawnManager.LimitedClearCustomTypes(Spawned.Player);
            if (!Plugin.Instance.DoSpawnBasicRoles)
            {
                return;
            }

            if (Plugin.PlayerRegistry.ContainsKey(Spawned.Player.Id))
            {
                return;
            }

            if (Spawned.Player.IsNPC)
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
                LogReason = " [going with a respawn wave]";
            }

            Log.Debug($"Player {Spawned.Player.Nickname} spawned{LogReason}, going to assign a role if needed!");

            Timing.CallDelayed(0.1f, () =>
            {
                DoEvaluateSpawnForPlayer(Spawned.Player);
            });
        }

        public void OnDied(DiedEventArgs Died)
        {
            Died.Player.CustomName = null;
            SpawnManager.ClearCustomTypes(Died.Player);
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
                    return;
                }
            }

            // If we are still here let's send the event
            API.Features.Events.__CallEvent(UCREvents.Escaping, Escaping);
        }

        public void OnRespawningWave(RespawningTeamEventArgs Respawn)
        {
            foreach (Player Player in Respawn.Players)
            {
                Plugin.RoleSpawnQueue.Add(Player.Id);
            }
        }

        public void OnItemUsed(UsedItemEventArgs UsedItem)
        {
            if (SpawnManager.TryGetCustomRole(UsedItem.Player) is not null && Plugin.PermanentEffectStatus.ContainsKey(UsedItem.Player.Id)  && UsedItem.Item.Type == ItemType.SCP500)
            {
                foreach (IUCREffect Effect in Plugin.PermanentEffectStatus[UsedItem.Player.Id])
                {
                    if (Effect.Removable)
                    {
                        Plugin.PermanentEffectStatus[UsedItem.Player.Id].Remove(Effect);
                    }
                }
                SpawnManager.SetAllActiveEffect(UsedItem.Player);
            }
        }

        public IEnumerator<float> DoSetInfiniteEffectToPlayers()
        {
            while (Round.InProgress)
            {
                foreach (Player Player in Player.List.Where(player => Plugin.PermanentEffectStatus.ContainsKey(player.Id) && player.IsAlive))
                {
                    SpawnManager.SetAllActiveEffect(Player);
                }
                yield return Timing.WaitForSeconds(10f);
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

                    if (Plugin.RolesCount[RoleId].Count() < Plugin.CustomRoles[RoleId].MaxPlayers)
                    {
                        Timing.RunCoroutine(DoSpawnPlayer(Player, RoleId, false));
                        Plugin.RolesCount[RoleId].Add(Player.Id);
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