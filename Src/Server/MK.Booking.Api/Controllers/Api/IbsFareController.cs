using System.Web.Http;
using System.Web.Http.Controllers;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;

namespace apcurium.MK.Web.Controllers.Api
{
    public class IbsFareController : BaseApiController
    {
        public IbsFareService IBSFareService { get; private set; }

        public IbsFareController(IbsFareService ibsFareService)
        {
            IBSFareService = ibsFareService;
        }

        [HttpGet, Route("api/ibsfare")]
        public IHttpActionResult GetIbsFare([FromUri]IbsFareRequest request)
        {
            var result = IBSFareService.Get(request);
            
            return GenerateActionResult(result);
        }

        [HttpGet, Route("api/ibsdistance")]
        public IHttpActionResult Get([FromUri]IbsDistanceRequest request)
        {
            var result = IBSFareService.Get(request);
            
            return GenerateActionResult(result);
        }
    }
}
