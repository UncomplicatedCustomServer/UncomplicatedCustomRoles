using Exiled.API.Features;
using MEC;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Utf8Json.Formatters;
using Utf8Json.Internal.DoubleConversion;

namespace UncomplicatedCustomRoles.Manager
{
    internal class HttpManager
    {
        public static CoroutineHandle HttpPresenceCoroutine { get; set; }

        public static int LogChunk { get; set; } = 50;

        public static List<float> ChunkList { get; set; } = new();

        public static int FailedHttp { get; set; } = 0;

        public static HttpClient HttpClient;

        public static void StartHttpJob()
        {
            if (HttpPresenceCoroutine.IsRunning)
            {
                Timing.KillCoroutines(HttpPresenceCoroutine);
            }
            if (!File.Exists(Path.Combine(Paths.Configs, "UncomplicatedCustomRoles", ".nohttp")))
            {
                Log.Info($"Selecting server for UCS presence...\nFound {Plugin.Instance.PresenceUrl.Replace("https://", "").Split('/')[0]}");
                HttpPresenceCoroutine = Timing.RunCoroutine(DoHttpPresence());
            }
        }

        public static void StartAll()
        {
            HttpClient = new();
            CheckForNewVersion();
            StartHttpJob();
            if (File.Exists(Path.Combine(Paths.Configs, "UncomplicatedCustomRoles", ".chunksize")))
            {
                LogChunk = int.Parse(File.ReadAllText(Path.Combine(Paths.Configs, "UncomplicatedCustomRoles", ".chunksize")));
            }
        }

        public static async void CheckForNewVersion()
        {
            int Version = int.Parse(await HttpClient.GetStringAsync("https://ucs.fcosma.it/api/plugin/latest_raw"));
            if (Version > int.Parse(Plugin.Instance.Version.ToString().Remove('.')))
            {
                Log.Warn("Found a new version of UncomplicatedCustomRoles!\nPlease update the plugin: https://github.com/FoxWorn3365/UncomplicatedCustomRoles\nLatest relase:\nhttps://github.com/FoxWorn3365/UncomplicatedCustomRoles/releases/latest");
            }
        }

        public static HttpStatusCode ProposeOwner(string DiscordId)
        {
            Task<HttpResponseMessage> Task = System.Threading.Tasks.Task.Run(() => HttpClient.GetAsync($"https://ucs.fcosma.it/api/plugin/owner_raw?discordid={DiscordId}"));
            Task.Wait();
            return Task.Result.StatusCode;
        }

        private static void SendLogs()
        {
            float Sum = 0f;

            foreach (float Chunk in ChunkList)
            {
                Sum += Chunk;
            }

            float Average = Sum / ChunkList.Count();

            Log.Info($"[UCS HTTP Presence] >> Put the presence for {ChunkList.Count()} times with success!\nAverage response time (ms): {Average}\nFailed HTTP Request(s): {FailedHttp}\nFail chance: {Average/100*FailedHttp}%\nTo disable this report, create a file named '.chunksize' in the 'UncomplicatedCustomRoles' and write 1 or -1, as your wish.");
        }

        private static async void TaskGetHttpResponse()
        {
            long Start = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            HttpResponseMessage RawData = await HttpClient.GetAsync($"{Plugin.Instance.PresenceUrl}?port={Server.Port}&cores={Environment.ProcessorCount}&ram=0&version={Plugin.Instance.Version}");
            string Data = RawData.Content.ReadAsStringAsync().Result;

            Dictionary<string, string> Response = JsonConvert.DeserializeObject<Dictionary<string, string>>(Data);

            if (Response["status"] == "200")
            {
                ChunkList.Add(DateTimeOffset.Now.ToUnixTimeMilliseconds() - Start);
            }
            else
            {
                FailedHttp++;
                Log.Warn($"[UCS HTTP Presence] >> Failed to put data in the UCS server for presence! HTTP-CODE: {Response["status"]}, server says: {Response["message"]}");
                ChunkList.Add(-1);
            }
        }

        public static IEnumerator<float> DoHttpPresence()
        {
            Log.Info("[UCS HTTP Presence] >> Started the presence task manager");

            while (true)
            {
                if (FailedHttp > 5)
                {
                    Log.Error($"[UCS HTTP Presence] >> Failed to put data on stream for {FailedHttp} times, disabling the function...");
                    yield break;
                }
                
                if (ChunkList.Count() >= LogChunk && LogChunk > 1)
                {
                    SendLogs();
                }

                TaskGetHttpResponse();

                yield return Timing.WaitForSeconds(500);
            }
        }
    }
}
