using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using apcurium.MK.Common.Configuration;

namespace HoneyBadger
{
    public class BaseServiceClient
    {
        protected BaseServiceClient(IServerSettings serverSettings)
        {
            Settings = serverSettings;

            var baseHoneyBadgerUrl = serverSettings.ServerData.HoneyBadger.ServiceUrl.Split('?')[0];

            Client = new HttpClient
            {
                BaseAddress = new Uri(baseHoneyBadgerUrl)  
            };
        }

        protected HttpClient Client { get; private set; }

        protected IServerSettings Settings { get; private set; }

        protected static string BuildQueryString(IEnumerable<KeyValuePair<string, string>> @params, string paramsFromSetting = null)
        {
            var requestPrefix = paramsFromSetting != null ? "&" : string.Empty;

            return "?" + paramsFromSetting + requestPrefix + string.Join("&", @params.Select(x => string.Join("=", x.Key, x.Value)));
        }
    }
}
