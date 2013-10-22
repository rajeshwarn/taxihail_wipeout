using System;
using System.Collections.Generic;
using System.Linq;
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



    }
}
