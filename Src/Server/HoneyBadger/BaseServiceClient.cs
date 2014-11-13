using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace HoneyBadger
{
    public class BaseServiceClient
    {
        protected BaseServiceClient()
        {
            Client = new HttpClient { BaseAddress = new Uri(GetUrl()) };
        }

        protected HttpClient Client { get; private set; }

        private string GetUrl()
        {
            return "http://insight.cmtapi.com:8081/v1.1/";
        }

        protected static string BuildQueryString(IEnumerable<KeyValuePair<string, string>> @params)
        {
            return "?" + string.Join("&", @params.Select(x => string.Join("=", x.Key, x.Value)));
        }
    }
}
