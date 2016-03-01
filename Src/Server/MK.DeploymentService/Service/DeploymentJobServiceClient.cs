#region

using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using CustomerPortal.Web.Entities;

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
                        Credentials = new NetworkCredential(ConfigurationManager.AppSettings["CustomerPortalUsername"], ConfigurationManager.AppSettings["CustomerPortalPassword"])
                    }))
            {
                client.BaseAddress = new Uri(url);

                try
                {
                    var r = client.GetAsync(@"deployments/" + ConfigurationManager.AppSettings["ServerName"] + @"/next").Result;
                    if (r.IsSuccessStatusCode)
                    {
                        return r.Content.ReadAsAsync<DeploymentJob>()
                            .Result;
                    }
                    return null;
                }
                catch
                {
                    return null;
                }
            }
        }


        private static string GetUrl()
        {
            // ReSharper disable once RedundantAssignment
            var url = ConfigurationManager.AppSettings["CustomerPortalUrl"];

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
                        Credentials = new NetworkCredential(ConfigurationManager.AppSettings["CustomerPortalUsername"], ConfigurationManager.AppSettings["CustomerPortalPassword"])
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