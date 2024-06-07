using Exiled.API.Features;
using Exiled.Loader;
using MEC;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace UncomplicatedCustomRoles.Manager
{
    internal class HttpManager
    {
        /// <summary>
        /// The <see cref="CoroutineHandle"/> of the presence coroutine.
        /// </summary>
        public CoroutineHandle PresenceCoroutine { get; internal set; }

        /// <summary>
        /// If <see cref="true"/> the message that confirm that the server is communicating correctly with our APIs has been sent in the console.
        /// </summary>
        public bool SentConfirmationMessage { get; internal set; } = false;

        /// <summary>
        /// The number of errors that has occurred. If this number exceed the <see cref="MaxErrors"/> quote then this feature will be deactivated.
        /// </summary>
        public uint Errors { get; internal set; } = 0;

        /// <summary>
        /// The maximum number of errors that can occur before deactivating the function.
        /// </summary>
        public uint MaxErrors { get; }

        /// <summary>
        /// If <see cref="true"/> this feature is active.
        /// </summary>
        public bool Active { get; internal set; } = false;

        /// <summary>
        /// The prefix of the plugin for our APIs
        /// </summary>
        public string Prefix { get; }

        /// <summary>
        /// The <see cref="HttpClient"/> public istance
        /// </summary>
        public HttpClient HttpClient { get; }

        /// <summary>
        /// The UCS APIs endpoint
        /// </summary>
        public string Endpoint { get; } = "https://ucs.fcosma.it/api/v2";

        /// <summary>
        /// An array of response times
        /// </summary>
        public List<float> ResponseTimes { get; } = new();

        /// <summary>
        /// Create a new istance of the HttpManager
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="maxErrors"></param>
        public HttpManager(string prefix, uint maxErrors = 5)
        {
            Prefix = prefix;
            MaxErrors = maxErrors;
            HttpClient = new();
        }

        internal HttpResponseMessage HttpGetRequest(string url)
        {
            try
            {
                Task<HttpResponseMessage> Response = Task.Run(() => HttpClient.GetAsync(url));

                Response.Wait();

                return Response.Result;
            } 
            catch(Exception)
            {
                return null;
            }
        }

        internal HttpResponseMessage HttpPutRequest(string url, string content)
        {
            try
            {
                Task<HttpResponseMessage> Response = Task.Run(() => HttpClient.PutAsync(url, new StringContent(content, Encoding.UTF8, "text/plain")));

                Response.Wait();

                return Response.Result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        internal string RetriveString(HttpResponseMessage response)
        {
            if (response is null)
                return string.Empty;

            return RetriveString(response.Content);
        }

        internal string RetriveString(HttpContent response)
        {
            if (response is null)
                return string.Empty;

            Task<string> String = Task.Run(response.ReadAsStringAsync);

            String.Wait();

            return String.Result;
        }

        public HttpStatusCode AddServerOwner(string discordId)
        {
            return HttpGetRequest($"{Endpoint}/owners/add?discordid={discordId}")?.StatusCode ?? HttpStatusCode.InternalServerError;
        }

        public Version LatestVersion()
        {
            return new(RetriveString(HttpGetRequest($"{Endpoint}/{Prefix}/version?vts=5")));
        }

        public bool IsLatestVersion(out Version latest)
        {
            latest = LatestVersion();
            if (latest.CompareTo(Plugin.Instance.Version) > 0)
                return false;

            return true;

        }

        public bool IsLatestVersion()
        {
            if (LatestVersion().CompareTo(Plugin.Instance.Version) > 0)
                return false;

            return true;
        }

        internal bool Presence(out HttpContent httpContent)
        {
            float Start = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            HttpResponseMessage Status = HttpGetRequest($"{Endpoint}/{Prefix}/presence?port={Server.Port}&cores={Environment.ProcessorCount}&ram=0&version={Plugin.Instance.Version}");
            httpContent = Status.Content;
            ResponseTimes.Add(DateTimeOffset.Now.ToUnixTimeMilliseconds() - Start);
            if (Status.StatusCode == HttpStatusCode.OK)
            {
                return true;
            }
            return false;
        }

        internal HttpStatusCode ShareLogs(string data, out HttpContent httpContent)
        {
            HttpResponseMessage Status = HttpPutRequest($"{Endpoint}/{Prefix}/error?port={Server.Port}&exiled_version={Loader.Version}&plugin_version={Plugin.Instance.Version}", data);
            httpContent = Status.Content;
            return Status.StatusCode;
        }

        internal IEnumerator<float> PresenceAction()
        {
            while (Active && Errors <= MaxErrors)
            {
                if (!Presence(out HttpContent content))
                {
                    try
                    {
                        Dictionary<string, string> Response = JsonConvert.DeserializeObject<Dictionary<string, string>>(RetriveString(content));
                        Errors++;
                        LogManager.Warn($"[UCS HTTP Manager] >> Error while trying to put data inside our APIs.\nThe endpoint say: {Response["message"]} ({Response["status"]})");
                    }
                    catch (Exception) { }
                }

                yield return Timing.WaitForSeconds(500.0f);
            }
        }
        
        public void Start()
        {
            if (Active)
                return;

            Active = true;
            PresenceCoroutine = Timing.RunCoroutine(PresenceAction());
        }

        public void Stop()
        {
            if (!Active)
                return;

            Active = false;
            Timing.KillCoroutines(PresenceCoroutine);
        }
    }
}
