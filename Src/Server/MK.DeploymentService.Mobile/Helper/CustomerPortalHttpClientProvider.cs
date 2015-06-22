using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace MK.DeploymentService.Mobile.Helper
{
    public static class CustomerPortalHttpClientProvider
    {
        public static HttpClient Get()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(
                    System.Text.ASCIIEncoding.ASCII.GetBytes(
                        string.Format("{0}:{1}",
                            ConfigurationManager.AppSettings["CustomerPortalUsername"],
                            ConfigurationManager.AppSettings["CustomerPortalPassword"]))));
            client.BaseAddress = new Uri(ConfigurationManager.AppSettings["CustomerPortalUrl"]);

            return client;
        }
    }
}
