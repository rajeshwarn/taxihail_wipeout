using System.Threading.Tasks;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;

namespace apcurium.MK.Web.Controllers.Api
{
    public class SearchLocationController : BaseApiController
    {
        public SearchLocationsService SearchLocationsService { get; private set; }

        public SearchLocationController(SearchLocationsService searchLocationsService)
        {
            SearchLocationsService = searchLocationsService;
        }

        [HttpPost, Route("api/v2/searchlocation")]
        public async Task<IHttpActionResult> SearchLocation([FromUri]string name, [FromUri]double? lat, [FromUri]double? lng)
        {
            var result = await SearchLocationsService.Post(new SearchLocationsRequest
            {
                Lat = lat,
                Lng = lng,
                Name = name
            });

            return GenerateActionResult(result);
        }

    }
}
