using System;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Resources;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Web.Controllers.Api
{
    [RoutePrefix("api/v2")]
    public class IbsFareController : BaseApiController
    {
        private readonly IbsFareService _ibsFareService;

        public IbsFareController(IVehicleTypeDao vehicleTypeDao, IServerSettings serverSettings, IIBSServiceProvider ibsServiceProvider)
        {
            _ibsFareService = new IbsFareService(ibsServiceProvider, serverSettings, vehicleTypeDao);
        }

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);

            PrepareApiServices(_ibsFareService);
        }

        [HttpGet, Route("ibsfare")]
        public IHttpActionResult GetIbsFare([FromUri]IbsFareRequest request)
        {
            var result = _ibsFareService.Get(request);


            return GenerateActionResult(result);
        }

        [HttpGet, Route("ibsdistance")]
        public IHttpActionResult Get([FromUri]IbsDistanceRequest request)
        {
            var result = _ibsFareService.Get(request);


            return GenerateActionResult(result);
        }
    }
}
