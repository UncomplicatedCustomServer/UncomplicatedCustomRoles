using Exiled.API.Features;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.Interfaces;

namespace UncomplicatedCustomRoles.Manager.NET
{
    internal class SpawnPointApiCommunicator
    {
        /// <summary>
        /// Gets the API endpoint
        /// </summary>
        public static string Endpoint => "https://ucs.fcosma.it/api/spawnpoints";

        /// <summary>
        /// Gets the  maximum number of SpawnPoints per server
        /// </summary>
        public const int MaxSpawnPoints = 35; // Don't worry, the check is also in the APIs backend :wink:

        /// <summary>
        /// Init the Communicator
        /// </summary>
        public static void Init()
        {
            if (!Plugin.HttpManager.IsAllowed)
                return;

            Task.Run(LoadFromCloud);
        }

        /// <summary>
        /// Retrive <see cref="SpawnPointOld"/>s loaded on UCS cloud
        /// </summary>
        public static void LoadFromCloud()
        {
            // We need first to reset the list
            SpawnPoint.List.Clear();

            HttpResponseMessage Response = Plugin.HttpManager.HttpGetRequest($"{Endpoint}/list?port={Server.Port}");
            if (Response.StatusCode is HttpStatusCode.NotFound)
                LogManager.System("Failed to load SpawnPoints from UCS cloud: not found! - Generated (ig)");
            else
                try
                {
                    List<SpawnPoint> List = JsonConvert.DeserializeObject<List<SpawnPoint>>(Plugin.HttpManager.RetriveString(Response));

                    foreach (SpawnPoint SpawnPoint in List)
                        SpawnPoint.List.Add(SpawnPoint);

                    LogManager.Info($"Loaded {List.Count} SpawnPoints from our central servers!");

                    CustomRoleSpawnCompatibilityChecker();
                }
                catch (Exception e)
                {
                    LogManager.Error($"Error while acting SpawnPointApiCommunicator::PushSpawnPoints(): {e.GetType().FullName} - {e.Message}\n{e.Source} -- {e.InnerException}\n{e.StackTrace}");
                }
        }

        /// <summary>
        /// Push the <see cref="SpawnPointOld"/>s inside UCS cloud - useful if the list has been updated!<br></br>
        /// Every server has a limit of 10 ports with 10 spawnpoints for each one
        /// </summary>
        /// <returns></returns>
        public static void PushSpawnPoints()
        {
            try
            {
                HttpResponseMessage Response = Plugin.HttpManager.HttpPutRequest($"{Endpoint}/update?port={Server.Port}", JsonConvert.SerializeObject(SpawnPoint.List));
                string Answer = Plugin.HttpManager.RetriveString(Response);
                if (Response.StatusCode is HttpStatusCode.OK)
                    if (Plugin.Instance.Config.EnableBasicLogs)
                        LogManager.Info($"Your list of SpawnPoints on UCS cloud has been updated!\nServer says: {Answer}");
                    else
                        LogManager.Silent($"Your list of SpawnPoints on UCS cloud has been updated!\nServer says: {Answer}");
                else if (Response.StatusCode is HttpStatusCode.NotAcceptable)
                    LogManager.Warn($"UCS cloud has declined the request: you have reached the maximum number of SpawnPoints: the current limit is: {MaxSpawnPoints} SpawnPoints per Server port and 10 total Server port!\nPlease contact us through our Discord! -- Server says: {Answer}");
                else if (Response.StatusCode is HttpStatusCode.InternalServerError)
                    LogManager.Warn($"Failed to update your SpawnPoints on the UCS cloud: it seems to be broken!\nContact us as fast as possible!\nServer says: {Answer}");
            }
            catch (Exception e)
            {
                LogManager.Error($"Error while acting SpawnPointApiCommunicator::PushSpawnPoints(): {e.GetType().FullName} - {e.Message}\n{e.Source} -- {e.InnerException}\n{e.StackTrace}");
            }
        }

        /// <summary>
        /// Async call the function <see cref="PushSpawnPoints"/>
        /// </summary>
        /// <returns></returns>
        public static Task AsyncPushSpawnPoints() => Task.Run(PushSpawnPoints);

        /// <summary>
        /// Send a migration request to our central servers
        /// </summary>
        /// <param name="newPort"></param>
        /// <returns></returns>
        public static HttpResponseMessage PushMigrationRequest(int newPort) => Plugin.HttpManager.HttpGetRequest($"{Endpoint}/migrate?port={Server.Port}&to={newPort}");

        /// <summary>
        /// Send a downloadUrl request to our central request and share the answer
        /// </summary>
        /// <returns></returns>
        public static string AskDownloadUrl() => Plugin.HttpManager.RetriveString(Plugin.HttpManager.HttpGetRequest($"{Endpoint}/download?port={Server.Port}"));

        /// <summary>
        /// Check every <see cref="ICustomRole"/> in order to find if any of them are with an invalid (non-existing) SpawnPoint
        /// </summary>
        private static void CustomRoleSpawnCompatibilityChecker()
        {
            foreach (ICustomRole role in CustomRole.CustomRoles.Values.Where(role => role.SpawnSettings is not null && role.SpawnSettings.SpawnPoint is not null && role.SpawnSettings.Spawn is SpawnLocationType.SpawnPointSpawn))
                if (!SpawnPoint.Exists(role.SpawnSettings.SpawnPoint))
                    LogManager.Warn($"CustomRole {role.Name} {role.Id} has an invalid SpawnPoint '{role.SpawnSettings.SpawnPoint}' inside it's configuration: the selected SpawnPoint does not exists!");
        }
    }
}
