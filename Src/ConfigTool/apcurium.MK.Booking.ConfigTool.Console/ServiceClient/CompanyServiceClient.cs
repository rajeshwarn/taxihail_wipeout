using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using System.Text;
using System.Threading.Tasks;
using CustomerPortal.Web.Entities;
using System.Net.Http;
using System.Net;



namespace apcurium.MK.Booking.ConfigTool.ServiceClient
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
					//var ree = r.Content.ReadAsStringAsync ().Result;
					var re = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<Company>> (r.Content.ReadAsStringAsync ().Result);
					return re.OrderBy( c=>c.CompanyKey );


				}
				else
				{
					return null;
				}

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


        public Stream GetCompanyFiles( string id )
        {
			var url = GetUrl();

            //var client = new JsonServiceClient(url) { Timeout = new TimeSpan(0, 0, 2, 0, 0) };
            
			using (var client = new HttpClient (new HttpClientHandler{ Credentials = new NetworkCredential ("taxihail@apcurium.com", "apcurium5200!") })) 
			{

				var r = client.GetAsync (url + @"company/" + id + @"/files");

				r.Wait ();

				r.Result.EnsureSuccessStatusCode ();
            

				var objectTask = r.Result.Content.ReadAsStreamAsync ();

				objectTask.Wait ();

      
				return objectTask.Result;
			}

        }

              

    }
}
