using System;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Web.Security;

namespace apcurium.MK.Web.Controllers.Api
{
    [RoutePrefix("app")]
    public class ApplicationInfoController : BaseApiController
    {
        private readonly ApplicationInfoService _applicationInfoService;
        private readonly AppStartUpLogService _appStartUpLogStartUpLogService;


        public ApplicationInfoController(IServerSettings serverSettings, IAppStartUpLogDao appStartUpLogDao)
        {
            _applicationInfoService = new ApplicationInfoService(serverSettings)
            {
                Session = GetSession(),
                HttpRequestContext = RequestContext
            };

            _appStartUpLogStartUpLogService = new AppStartUpLogService(appStartUpLogDao)
            {
                Session = GetSession()
            };
        }

        [HttpGet, Route("info")]
        public IHttpActionResult GetApplicationInfo()
        {
            var result = _applicationInfoService.Get();

            return GenerateActionResult(result);
        }

        [HttpGet, Route("starts/{lastMinutes}"), Auth(Role = RoleName.Admin)]
        public IHttpActionResult AppStartUpLog(long lastMinutes)
        {
            var result = _appStartUpLogStartUpLogService.Get(new AppStartUpLogRequest() {LastMinutes = lastMinutes});

            return GenerateActionResult(result);
        }
    }
}
