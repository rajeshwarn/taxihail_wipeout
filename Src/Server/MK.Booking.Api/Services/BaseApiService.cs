using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using apcurium.MK.Common.Http;

namespace apcurium.MK.Booking.Api.Services
{
    public class BaseApiService
    {
        public SessionEntity Session { get; set; }

        public HttpRequestContext HttpRequestContext { get; set; }
    }
}
