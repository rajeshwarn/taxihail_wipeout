using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Security;
using apcurium.MK.Web.Security;

namespace apcurium.MK.Web.Controllers.Api
{
    public class ApplicationInfoController : BaseApiController
    {
        public ApplicationInfoService InfoService { get; private set; }
        public AppStartUpLogService AppStartUpLogStartUpLogService { get; private set; }


        public ApplicationInfoController(ApplicationInfoService infoService, AppStartUpLogService appStartUpLogStartUpLogService)
        {
            InfoService = infoService;
            AppStartUpLogStartUpLogService = appStartUpLogStartUpLogService;
        }

        [HttpGet, Route("api/v2/app/info")]
        public IHttpActionResult GetApplicationInfo()
        {
            var result = InfoService.Get();

            return GenerateActionResult(result);
        }

        [HttpGet, Route("api/v2/app/starts/{lastMinutes}"), Auth(Role = RoleName.Admin)]
        public IHttpActionResult AppStartUpLog(long lastMinutes)
        {
            var result = AppStartUpLogStartUpLogService.Get(new AppStartUpLogRequest() {LastMinutes = lastMinutes});

            return GenerateActionResult(result);
        }
    }
}
