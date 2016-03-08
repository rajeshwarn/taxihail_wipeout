using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Api.Services.Maps;
using apcurium.MK.Booking.MapDataProvider;
using apcurium.MK.Common.Diagnostic;

namespace apcurium.MK.Web.Controllers.Api.Maps
{
    [RoutePrefix("api/v2/addressFromPostalCode")]
    public class PostalCodeController : BaseApiController
    {
        public PostalCodeService PostalCodeService { get; set; }
        public PostalCodeController(IPostalCodeService postalCodeService, ILogger log)
        {
            PostalCodeService = new PostalCodeService(postalCodeService, log);
        }

        [HttpPost]
        public IHttpActionResult GetPostalCode([FromBody] GetAddressesFromPostcalCodeRequest request)
        {
            var result = PostalCodeService.Post(request);

            return GenerateActionResult(result);
        }
    }
}
