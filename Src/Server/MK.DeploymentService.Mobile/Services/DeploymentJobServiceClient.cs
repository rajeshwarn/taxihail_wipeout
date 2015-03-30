using System;
using CustomerPortal.Web.Entities;
using System.Net.Http;
using System.Net;
using System.Net.Http.Formatting;
using log4net;

namespace MK.DeploymentService.Service
{
    public class DeploymentJobServiceClient
    {
        private readonly ILog _logger;

        public DeploymentJobServiceClient()
        {
            _logger = LogManager.GetLogger ("DeploymentJobService");
        }

        public DeploymentJob GetNext()
        {
            var url = GetUrl();
            using (var client = new HttpClient(new HttpClientHandler{ Credentials = new NetworkCredential("taxihail@apcurium.com", "apcurium5200!")}))
            {
                client.BaseAddress = new Uri(url);          
				var serverName = System.Configuration.ConfigurationManager.AppSettings["ServerName"];      
				var r = client.GetAsync(@"deployments/" + serverName+ @"/next").Result;
                if (r.IsSuccessStatusCode)
				{
					var re = Newtonsoft.Json.JsonConvert.DeserializeObject<DeploymentJob> (r.Content.ReadAsStringAsync ().Result);
					return re;
                }
                else
                {
                    return null;
                }
            }            
        }

        private static string GetUrl()
        {
			var url = System.Configuration.ConfigurationManager.AppSettings["CustomerPortalUrl"];
			return url;
        }

        public void UpdateStatus(string jobId, string details, JobStatus? status = null)
        {
            try
            {
                var url = GetUrl();

                using (var client = new HttpClient(new HttpClientHandler { Credentials = new NetworkCredential("taxihail@apcurium.com", "apcurium5200!") }))
                {
                    client.BaseAddress = new Uri(url);

                    var d = new JobStatusDetails();
                    d.Details = details;
                    d.Status = status;

                    var content = new ObjectContent<JobStatusDetails>(d, new JsonMediaTypeFormatter());

                    var result = client.PostAsync("deployments/" + jobId + "/updatedetails", content).Result;
                    result.ToString();
                }
            }
            catch(Exception ex)
            {
                _logger.Error("Error while updating Job Details", ex);
            }
        }
    }
}



