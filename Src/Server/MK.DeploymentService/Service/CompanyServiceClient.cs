#region

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using CustomerPortal.Web.Entities;
using Newtonsoft.Json;

#endregion

namespace MK.DeploymentService.Service
{
    public class CompanyServiceClient
    {
        public IEnumerable<Company> GetCompanies()
        {
            var url = GetUrl();
            using (
                var client =
                    new HttpClient(new HttpClientHandler
                    {
                        Credentials = new NetworkCredential(ConfigurationManager.AppSettings["CustomerPortalUsername"], ConfigurationManager.AppSettings["CustomerPortalPassword"])
                    }))
            {
                client.BaseAddress = new Uri(url);
                var r = client.GetAsync(@"company").Result;
                if (r.IsSuccessStatusCode)
                {
                    var re = JsonConvert.DeserializeObject<IEnumerable<Company>>(r.Content.ReadAsStringAsync().Result);
                    return re.OrderBy(c => c.CompanyKey);
                }
                return null;
            }
        }

        private static string GetUrl()
        {
            var url = ConfigurationManager.AppSettings["CustomerPortalUrl"];
//			#if DEBUG
//			url = "http://localhost:2287/api/";
//			#endif
            return url;
        }

        public Stream GetCompanyFiles(string id, string type)
        {
            var url = GetUrl();
            using (
                var client =
                    new HttpClient(new HttpClientHandler
                    {
                        Credentials = new NetworkCredential(ConfigurationManager.AppSettings["CustomerPortalUsername"], ConfigurationManager.AppSettings["CustomerPortalPassword"])
                    }))
            {
                var r = client.GetAsync(url + @"company/" + id + @"/files?type=" + type);
                r.Wait();
                r.Result.EnsureSuccessStatusCode();
                var objectTask = r.Result.Content.ReadAsStreamAsync();
                objectTask.Wait();
                return objectTask.Result;
            }
        }
    }
}