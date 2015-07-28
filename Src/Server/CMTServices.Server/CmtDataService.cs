#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using ServiceStack.CacheAccess;
using ServiceStack.ServiceInterface;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class CmtDataService : Service
    {
        // same as apcurium.MK.Booking.Api.Contract.Requests.ReferenceListRequest, keep these in sync
        public class ReferenceListRequest
        {
            public string ListName { get; set; }
            public string SearchText { get; set; }
            public bool coreFieldsOnly { get; set; }
            public int size { get; set; }
        }

        private readonly IServerSettings _serverSettings;
        private HttpClient restClient;

        public CmtDataService(IServerSettings serverSettings)
        {
            _serverSettings = serverSettings;
            InitializeHttpClient();
        }

        private void InitializeHttpClient()
        {
            restClient = new HttpClient();
            restClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            restClient.Timeout = TimeSpan.FromMilliseconds(10000);
        }

        public object Get(ReferenceListRequest request)
        {
            var getUri = new Uri(_serverSettings.ServerData.Gds.ServiceUrl + "/reference/" + request.ListName.ToLower() +
                (request.SearchText.IsNullOrEmpty() ? "" : "/search/" + request.SearchText));
            var response = restClient.GetAsync(getUri).ConfigureAwait(continueOnCapturedContext: false).GetAwaiter().GetResult();
            var resultInfo = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            JArray result;
            if (request.SearchText.IsNullOrEmpty())
            {
                result = JArray.Parse(resultInfo);
            }
            else
            {
                var searchResult = JObject.Parse(resultInfo);
                result = (JArray)searchResult["items"];
            }
            result.ForEach(i => {
                if (i["name"] == null) {
                    i["name"] = i["id"];
                }
            });
            if (request.size > 0)
            {
                result = new JArray(result.Take(request.size));
            }
            if (request.coreFieldsOnly)
            {
                return result.Select(i => new 
                { 
                    id = i["id"].ToString(),
                    type = i["type"].ToString(),
                    name = i["name"].ToString()
                }).ToList();
            }
            else
            {
                return result.ToString(Formatting.None);
            }
        }

    }
}