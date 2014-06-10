#region

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using CustomerPortal.Web.Entities;
using MK.DeploymentService.Properties;

#endregion

namespace MK.DeploymentService.Service
{
    public class DeploymentJobServiceClient
    {
        public DeploymentJob GetNext()
        {
            var url = GetUrl();


            using (
                var client =
                    new HttpClient(new HttpClientHandler
                    {
                        Credentials = new NetworkCredential("taxihail@apcurium.com", "apcurium5200!")
                    }))
            {
                client.BaseAddress = new Uri(url);
                var r = client.GetAsync(@"deployments/" + Settings.Default.ServerName + @"/next").Result;
                if (r.IsSuccessStatusCode)
                {
                    return r.Content.ReadAsAsync<DeploymentJob>()
                        .Result;
                }
                return null;
            }
        }


        private static string GetUrl()
        {
// ReSharper disable once RedundantAssignment
            var url = Settings.Default.CustomerPortalUrl;

//#if DEBUG
//            url = "http://localhost:2287/api/";
//#endif
            return url;
        }


        public void UpdateStatus(string jobId, string details, JobStatus? status = null)
        {
            var url = GetUrl();

            using (
                var client =
                    new HttpClient(new HttpClientHandler
                    {
                        Credentials = new NetworkCredential("taxihail@apcurium.com", "apcurium5200!")
                    }))
            {
                client.BaseAddress = new Uri(url);

                var d = new JobStatusDetails();
                d.Details = details;
                d.Status = status;
                var content = new ObjectContent<JobStatusDetails>(d, new JsonMediaTypeFormatter());
// ReSharper disable once UnusedVariable
                var message = client.PostAsync("deployments/" + jobId + "/updatedetails", content).Result;
            }
        }
    }
}