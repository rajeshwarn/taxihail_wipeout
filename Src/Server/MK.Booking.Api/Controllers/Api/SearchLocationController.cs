using System.Threading.Tasks;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Maps;
using apcurium.MK.Booking.ReadModel.Query.Contract;

namespace apcurium.MK.Web.Controllers.Api
{
    [RoutePrefix("api/v2/searchlocation")]
    public class SearchLocationController : BaseApiController
    {
        public SearchLocationsService SearchLocationsService { get; }

        public SearchLocationController(IAddresses addressClient, IAccountDao accountDao)
        {
            SearchLocationsService = new SearchLocationsService(addressClient, accountDao);
        }
        
        [HttpPost]
        public async Task<IHttpActionResult> SearchLocation(SearchLocationsRequest request)
        {
            var result = await SearchLocationsService.Post(request);

            return GenerateActionResult(result);
        }

    }
}
