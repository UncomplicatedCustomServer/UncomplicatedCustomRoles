using Exiled.API.Features;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using UncomplicatedCustomRoles.API.Enums;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Interfaces;

namespace UncomplicatedCustomRoles.Manager.NET
{
    internal class SpawnPointApiCommunicator
    {
        /// <summary>
        /// Gets the API endpoint
        /// </summary>
        public static string Endpoint => "https://api.ucserver.it/spawnpoints";

        /// <summary>
        /// Gets the file path for the local spawnpoints of this server
        /// </summary>
        public static string FilePath => Path.Combine(Paths.Configs, $".{Server.Port}-spawnpoints.json");

        /// <summary>
        /// Gets whether the spawnpoints should be local or "global"
        /// </summary>
        public static bool Local => Plugin.Instance.Config.LocalSpawnPoints;

        /// <summary>
        /// Gets the  maximum number of SpawnPoints per server
        /// </summary>
        public const int MaxSpawnPoints = 100; // Don't worry, the check is also in the APIs backend :wink:

        /// <summary>
        /// Init the Communicator
        /// </summary>
        public static void Init()
        {
            if (!Plugin.HttpManager.IsAllowed)
                return;

            if (!File.Exists(FilePath))
                File.WriteAllText(FilePath, JsonConvert.SerializeObject(new SpawnPoint[] { }));

            Task.Run(LoadFromCloud);
        }

        /// <summary>
        /// Retrive <see cref="SpawnPointOld"/>s loaded on UCS cloud
        /// </summary>
        public static void LoadFromCloud()
        {
            // We need first to reset the list
            SpawnPoint.List.Clear();

            if (Local)
            {
                TryLoadSpawnPoints(File.ReadAllText(FilePath));
                return;
            }

            HttpResponseMessage Response = Plugin.HttpManager.HttpGetRequest($"{Endpoint}/list?port={Server.Port}");
            if (Response.StatusCode is HttpStatusCode.NotFound)
                LogManager.System("Failed to load SpawnPoints from UCS cloud: not found! - Generated (ig)");
            else
                try
                {
                    TryLoadSpawnPoints(Plugin.HttpManager.RetriveString(Response));
                }
                catch (Exception e)
                {
                    LogManager.Error($"Error while acting SpawnPointApiCommunicator::PushSpawnPoints(): {e.GetType().FullName} - {e.Message}\n{e.Source} -- {e.InnerException}\n{e.StackTrace}");
                }
        }

        /// <summary>
        /// Push the <see cref="SpawnPoint"/>s inside UCS cloud - useful if the list has been updated!<br></br>
        /// Every server has a limit of 10 ports with 10 spawnpoints for each one
        /// </summary>
        /// <returns></returns>
        public static void PushSpawnPoints()
        {
            if (Local)
            {
                File.WriteAllText(FilePath, JsonConvert.SerializeObject(SpawnPoint.List));
                return;
            }

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

        public static string AskIp() => Plugin.HttpManager.RetriveString(Plugin.HttpManager.HttpGetRequest($"{Endpoint}/ip"));

        /// <summary>
        /// Check every <see cref="ICustomRole"/> in order to find if any of them are with an invalid (non-existing) SpawnPoint
        /// </summary>
        private static void CustomRoleSpawnCompatibilityChecker()
        {
            foreach (ICustomRole role in CustomRole.CustomRoles.Values.Where(role => role.SpawnSettings is not null && role.SpawnSettings.SpawnPoints is not null && role.SpawnSettings.Spawn is SpawnType.SpawnPointSpawn))
                foreach (string spawnPoint in role.SpawnSettings.SpawnPoints)
                    if (!SpawnPoint.Exists(spawnPoint))
                        LogManager.Warn($"CustomRole {role.Name} {role.Id} has an invalid SpawnPoint '{role.SpawnSettings.SpawnPoints}' inside it's configuration: the selected SpawnPoint does not exists!");
        }

        private static void TryLoadSpawnPoints(string json)
        {
            List<SpawnPoint> List = JsonConvert.DeserializeObject<List<SpawnPoint>>(json);

            foreach (SpawnPoint SpawnPoint in List)
                SpawnPoint.List.Add(SpawnPoint);

            LogManager.Info($"Loaded {List.Count} SpawnPoints from our central servers!");

            CustomRoleSpawnCompatibilityChecker();
        }
    }
}
