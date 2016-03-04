using System.Linq;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Cryptography;
using apcurium.MK.Common.Extensions;
using CustomerPortal.Client;

namespace apcurium.MK.Booking.Api.Services
{
    public class NetworkRoamingService : BaseApiService
    {
        private readonly ITaxiHailNetworkServiceClient _taxiHailNetworkServiceClient;

        public NetworkRoamingService(ITaxiHailNetworkServiceClient taxiHailNetworkServiceClient)
        {
            _taxiHailNetworkServiceClient = taxiHailNetworkServiceClient;
        }

        public async Task<string> Get(FindMarketRequest request)
        {
            var market = await _taxiHailNetworkServiceClient.GetCompanyMarket(request.Latitude, request.Longitude);

            // Hash market so that client doesn't have direct access to its value
            return CryptographyHelper.GetHashString(market);
        }

        public async Task<MarketSettings> Get(FindMarketSettingsRequest request)
        {
            var marketSettings = await _taxiHailNetworkServiceClient.GetCompanyMarketSettings(request.Latitude, request.Longitude);

            return marketSettings != null
                ? new MarketSettings
                    {
                        HashedMarket = CryptographyHelper.GetHashString(marketSettings.Market),
                        EnableDriverBonus = marketSettings.EnableDriverBonus,
                        OverrideEnableAppFareEstimates = marketSettings.EnableAppFareEstimates,
                        EnableFutureBooking = marketSettings.EnableFutureBooking,
                        MarketTariff = marketSettings.MarketTariff,
                        DisableOutOfAppPayment = marketSettings.DisableOutOfAppPayment
                    }
                : new MarketSettings();
        }

        public async Task<NetworkFleet[]> Get(NetworkFleetsRequest request)
        {
            var networkFleet = await _taxiHailNetworkServiceClient.GetNetworkFleetAsync(null);
            return networkFleet.Select(f => new NetworkFleet
            {
                CompanyKey = f.CompanyKey,
                CompanyName = f.CompanyName
            })
            .ToArray();
        }

        public async Task<VehicleType[]> Get(MarketVehicleTypesRequest request)
        {
            var market = await _taxiHailNetworkServiceClient.GetCompanyMarket(request.Latitude, request.Longitude);
            if (!market.HasValue())
            {
                // In home market, no network vehicles
                return new VehicleType[0];
            }

            var marketVehicleTypes = await _taxiHailNetworkServiceClient.GetMarketVehicleTypes(market: market);

            return marketVehicleTypes.Select(v => new VehicleType
            {
                Id = v.Id,
                Name = v.Name,
                LogoName =  v.LogoName,
                ReferenceDataVehicleId = v.ReferenceDataVehicleId,
                MaxNumberPassengers = v.MaxNumberPassengers
            })
            .ToArray();
        }
    }
}