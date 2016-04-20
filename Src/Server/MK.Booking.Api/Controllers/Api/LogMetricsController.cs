using System.Web.Http;
using System.Web.Http.Controllers;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Web.Security;

namespace apcurium.MK.Web.Controllers.Api
{
    public class LogMetricsController : BaseApiController
    {
        public LogMetricsService MetricsService { get; private set; }

        public LogMetricsController(LogMetricsService metricsService)
        {
            MetricsService = metricsService;
        }

        [HttpPost, Auth, Route("api/accounts/logstartup")]
        public IHttpActionResult LogApplicationStartUp([FromBody] LogApplicationStartUpRequest request)
        {
            MetricsService.Post(request);

            return Ok();
        }
        [HttpPost, Auth, Route("api/orders/logeta")]
        public IHttpActionResult LogApplicationStartUp([FromBody] LogOriginalEtaRequest request)
        {
            MetricsService.Post(request);

            return Ok();
        }

    }
}
