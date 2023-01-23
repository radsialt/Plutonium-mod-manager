using Newtonsoft.Json.Linq;
using System.IO;
using System.Net;
using System.Net.Cache;

namespace Plutonium_Mod_Manager
{
    public static class RequestsManager
    {
        //
        // Global web client
        //
        public static WebClient webClient = new WebClient();

        public static JObject getModList(string rawJsonURL)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(rawJsonURL);
            request.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            return JObject.Parse(new StreamReader(response.GetResponseStream()).ReadToEnd());
        }
    }
}
