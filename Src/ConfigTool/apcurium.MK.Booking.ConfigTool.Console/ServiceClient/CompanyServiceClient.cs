using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CustomerPortal.Web.Entities;
using ServiceStack.ServiceClient.Web;
namespace apcurium.MK.Booking.ConfigTool.ServiceClient
{
    public class CompanyServiceClient
    {

        public IEnumerable<Company> GetCompanies()
        {
            var url = "http://localhost:2287";

            var client = new JsonServiceClient(url) { Timeout = new TimeSpan(0, 0, 2, 0, 0) };

            return client.Get<Company[]>(@"/api/company");

        }



        public Stream GetCompanyFiles( string id )
        {
            var url = "http://localhost:2287";

            //var client = new JsonServiceClient(url) { Timeout = new TimeSpan(0, 0, 2, 0, 0) };
            
            var c = new HttpClient();
            var r = c.GetAsync(url + @"/api/company/" + id + @"/files");

            r.Wait();

            r.Result.EnsureSuccessStatusCode();
            

            var objectTask = r.Result.Content.ReadAsStreamAsync();

            objectTask.Wait();

      
            return objectTask.Result;

        }

              

    }
}
