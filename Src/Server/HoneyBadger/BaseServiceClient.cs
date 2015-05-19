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
            Client = new HttpClient
            {
                BaseAddress = new Uri(serverSettings.ServerData.HoneyBadger.ServiceUrl)
            };
        }

        protected HttpClient Client { get; private set; }

        protected static string BuildQueryString(IEnumerable<KeyValuePair<string, string>> @params)
        {
            return "?" + string.Join("&", @params.Select(x => string.Join("=", x.Key, x.Value)));
        }
    }
}
