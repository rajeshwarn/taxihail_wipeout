using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using System.Text;
using System.Threading.Tasks;
using CustomerPortal.Web.Entities;
using System.Net.Http;
using System.Net;



namespace MK.DeploymentService.Service
{
    public class CompanyServiceClient
    {
        public IEnumerable<Company> GetCompanies()
        {
			var url = GetUrl();
			using (var client = new HttpClient(new HttpClientHandler{ Credentials = new NetworkCredential("taxihail@apcurium.com", "apcurium5200!")}))
			{
				client.BaseAddress = new Uri(url);                
				var r = client.GetAsync(@"company").Result;
				if (r.IsSuccessStatusCode)
				{
					var re = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<Company>> (r.Content.ReadAsStringAsync ().Result);
					return re.OrderBy( c=>c.CompanyKey );
				}
			    return null;
			}
		}

		private static string GetUrl()
		{
			var url =  "http://customer.taxihail.com/api/";
//			#if DEBUG
//			url = "http://localhost:2287/api/";
//			#endif
			return url;
		}

        public Stream GetCompanyFiles( string id , string type)
        {
			var url = GetUrl();
			using (var client = new HttpClient (new HttpClientHandler{ Credentials = new NetworkCredential ("taxihail@apcurium.com", "apcurium5200!") })) 
			{
				var r = client.GetAsync (url + @"company/" + id + @"/files?type=" + type);
				r.Wait ();
				r.Result.EnsureSuccessStatusCode ();
				var objectTask = r.Result.Content.ReadAsStreamAsync ();
                objectTask.Wait ();
				return objectTask.Result;
			}
        }
    }
}
