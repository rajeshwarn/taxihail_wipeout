using System;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Controllers;
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
    [RoutePrefix("api/v2/app")]
    public class ApplicationInfoController : BaseApiController
    {
        private readonly ApplicationInfoService _applicationInfoService;
        private readonly AppStartUpLogService _appStartUpLogStartUpLogService;


        public ApplicationInfoController(IServerSettings serverSettings, IAppStartUpLogDao appStartUpLogDao)
        {
            _applicationInfoService = new ApplicationInfoService(serverSettings);

            _appStartUpLogStartUpLogService = new AppStartUpLogService(appStartUpLogDao);
        }

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);

            PrepareApiServices(_appStartUpLogStartUpLogService, _applicationInfoService);
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
