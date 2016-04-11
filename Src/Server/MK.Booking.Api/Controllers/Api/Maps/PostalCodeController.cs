using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Api.Services.Maps;

namespace apcurium.MK.Web.Controllers.Api.Maps
{
    [RoutePrefix("api/v2/addressFromPostalCode")]
    public class PostalCodeController : BaseApiController
    {
        public PostalCodeService PostalCodeService { get; private set; }
        public PostalCodeController(PostalCodeService postalCodeService)
        {
            PostalCodeService = postalCodeService;
        }

        [HttpPost]
        public IHttpActionResult GetPostalCode([FromBody] GetAddressesFromPostcalCodeRequest request)
        {
            var result = PostalCodeService.Post(request);

            return GenerateActionResult(result);
        }
    }
}
