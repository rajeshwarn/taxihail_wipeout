using System;
using System.Linq;
using System.Web.Http;
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
    [RoutePrefix("ibsfare")]
    public class IbsFareController : BaseApiController
    {
        private readonly IbsFareService _ibsFareService;

        public IbsFareController(IVehicleTypeDao vehicleTypeDao, IServerSettings serverSettings, IIBSServiceProvider ibsServiceProvider)
        {
            _ibsFareService = new IbsFareService(ibsServiceProvider, serverSettings, vehicleTypeDao)
            {
                Session = GetSession(),
                HttpRequestContext = RequestContext
            };
        }

        [HttpGet, Route("ibsfares")]
        public IHttpActionResult GetIbsFare([FromUri]IbsFareRequest request)
        {
            var result = _ibsFareService.Get(request);


            return GenerateActionResult(result);
        }

        [HttpGet, Route("")]
        public IHttpActionResult Get([FromUri]IbsDistanceRequest request)
        {
            var result = _ibsFareService.Get(request);


            return GenerateActionResult(result);
        }
    }
}
