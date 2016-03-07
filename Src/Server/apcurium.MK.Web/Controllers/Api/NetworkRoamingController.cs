using System.Threading.Tasks;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using CustomerPortal.Client;

namespace apcurium.MK.Web.Controllers.Api
{
    [RoutePrefix("api/v2/roaming")]
    public class NetworkRoamingController : BaseApiController
    {
        public NetworkRoamingService NetworkRoamingService { get; }

        public NetworkRoamingController(ITaxiHailNetworkServiceClient taxiHailNetworkServiceClient)
        {
            NetworkRoamingService = new NetworkRoamingService(taxiHailNetworkServiceClient);
        }

        [Route("market"), HttpGet]
        public async Task<IHttpActionResult> FindMarket([FromUri] FindMarketRequest request)
        {
            var market = await NetworkRoamingService.Get(request);

            return GenerateActionResult(market);
        }

        [Route("marketsettings"), HttpGet]
        public async Task<IHttpActionResult> FindMarket([FromUri] FindMarketSettingsRequest request)
        {
            var marketSettings = await NetworkRoamingService.Get(request);

            return GenerateActionResult(marketSettings);
        }

        [Route("networkfleets"), HttpGet]
        public async Task<IHttpActionResult> GetNetworkFleets()
        {
            var marketSettings = await NetworkRoamingService.Get(new NetworkFleetsRequest());

            return GenerateActionResult(marketSettings);
        }

        [Route("externalMarketVehicleTypes"), HttpGet]
        public async Task<IHttpActionResult> GetExternalMarketVehicleTypes([FromUri]MarketVehicleTypesRequest request)
        {
            var result = await NetworkRoamingService.Get(request);

            return GenerateActionResult(result);
        } 


    }
}
