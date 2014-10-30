using apcurium.MK.Common.Configuration;
using System;
using System.Net;
using System.Net.Http;

namespace CustomerPortal.Client
{
    public class BaseServiceClient
    {
        private readonly IServerSettings _serverSettings;

        protected BaseServiceClient(IServerSettings serverSettings)
        {
            _serverSettings = serverSettings;

            var userName=_serverSettings.ServerData.CustomerPortal.UserName;
            var password=_serverSettings.ServerData.CustomerPortal.Password;

            Client = new HttpClient(new HttpClientHandler
            {
                Credentials = new NetworkCredential(userName, password)
            });
            Client.BaseAddress = new Uri(GetUrl());
        }

        protected HttpClient Client { get; private set; }

        private string GetUrl()
        {
            var url = _serverSettings.ServerData.CustomerPortal.Url;
#if DEBUG
            url = "http://localhost/CustomerPortal.Web/api/";
#endif
            return url;
        }
    }
}