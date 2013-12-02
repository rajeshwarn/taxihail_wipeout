using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using System.Text;
using System.Threading.Tasks;
using CustomerPortal.Web.Entities;
using System.Net.Http;
using System.Net;
using System.Net.Http.Formatting;



namespace MK.DeploymentService.Service
{
    public class DeploymentJobServiceClient
    {

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



    }
}



