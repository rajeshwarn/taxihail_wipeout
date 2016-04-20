using System.Threading.Tasks;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;

namespace apcurium.MK.Web.Controllers.Api
{
    public class NetworkRoamingController : BaseApiController
    {
        public NetworkRoamingService NetworkRoamingService { get; private set; }

        public NetworkRoamingController(NetworkRoamingService networkRoamingService)
        {
            NetworkRoamingService = networkRoamingService;
        }

        [Route("api/roaming/market"), HttpGet]
        public async Task<IHttpActionResult> FindMarket([FromUri] FindMarketRequest request)
        {
            var market = await NetworkRoamingService.Get(request);

            return GenerateActionResult(market);
        }

        [Route("api/roaming/marketsettings"), HttpGet]
        public async Task<IHttpActionResult> FindMarket([FromUri] FindMarketSettingsRequest request)
        {
            var marketSettings = await NetworkRoamingService.Get(request);

            return GenerateActionResult(marketSettings);
        }

        [Route("api/roaming/networkfleets"), HttpGet]
        public async Task<IHttpActionResult> GetNetworkFleets()
        {
            var marketSettings = await NetworkRoamingService.Get(new NetworkFleetsRequest());

            return GenerateActionResult(marketSettings);
        }

        [Route("api/roaming/externalMarketVehicleTypes"), HttpGet]
        public async Task<IHttpActionResult> GetExternalMarketVehicleTypes([FromUri]MarketVehicleTypesRequest request)
        {
            var result = await NetworkRoamingService.Get(request);

            return GenerateActionResult(result);
        } 


    }
}
