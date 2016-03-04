using System.Net.Http;
using System.Web.Http.Controllers;
using apcurium.MK.Common.Http;

namespace apcurium.MK.Booking.Api.Services
{
    public class BaseApiService
    {
        public SessionEntity Session { get; set; } = new SessionEntity();

        public HttpRequestContext HttpRequestContext { get; set; }

        public HttpRequestMessage HttpRequest { get; set; }
    }
}
