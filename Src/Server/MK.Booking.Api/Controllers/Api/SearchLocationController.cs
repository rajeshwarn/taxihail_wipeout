using System.Threading.Tasks;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Maps;
using apcurium.MK.Booking.ReadModel.Query.Contract;

namespace apcurium.MK.Web.Controllers.Api
{
    public class SearchLocationController : BaseApiController
    {
        public SearchLocationsService SearchLocationsService { get; private set; }

        public SearchLocationController(IAddresses addressClient, IAccountDao accountDao)
        {
            SearchLocationsService = new SearchLocationsService(addressClient, accountDao);
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
