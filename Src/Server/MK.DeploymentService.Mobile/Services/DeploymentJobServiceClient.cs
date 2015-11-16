using System;
using CustomerPortal.Web.Entities;
using System.Net.Http;
using System.Net.Http.Formatting;
using log4net;
using System.Configuration;
using MK.DeploymentService.Mobile.Helper;

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
            using (var client = CustomerPortalHttpClientProvider.Get())
            {                             
				var serverName = ConfigurationManager.AppSettings["ServerName"];      
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

        public void UpdateStatus(string jobId, string details, JobStatus? status = null)
        {
            try
            {
                using (var client = CustomerPortalHttpClientProvider.Get())
                {
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



