using System.Linq;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Cryptography;
using apcurium.MK.Common.Extensions;
using CustomerPortal.Client;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Services
{
    public class NetworkRoamingService : Service
    {
        private readonly ITaxiHailNetworkServiceClient _taxiHailNetworkServiceClient;
        private readonly IServerSettings _serverSettings;

        public NetworkRoamingService(ITaxiHailNetworkServiceClient taxiHailNetworkServiceClient, IServerSettings serverSettings)
        {
            _taxiHailNetworkServiceClient = taxiHailNetworkServiceClient;
            _serverSettings = serverSettings;
        }

        public object Get(FindMarketRequest request)
        {
            var market = _taxiHailNetworkServiceClient.GetCompanyMarket(request.Latitude, request.Longitude);

            // Hash market so that client doesn't have direct access to its value
            return CryptographyHelper.GetHashString(market);
        }

        public object Get(FindMarketSettingsRequest request)
        {
            var marketSettings = _taxiHailNetworkServiceClient.GetCompanyMarketSettings(request.Latitude, request.Longitude);

            return marketSettings != null
                ? new MarketSettings
                    {
                        HashedMarket = CryptographyHelper.GetHashString(marketSettings.Market),
                        EnableDriverBonus = marketSettings.EnableDriverBonus,
                        EnableFutureBooking = string.IsNullOrEmpty(marketSettings.Market) ? !_serverSettings.ServerData.DisableFutureBooking : marketSettings.EnableFutureBooking,
                    }
                : new MarketSettings();
        }

        public object Get(NetworkFleetsRequest request)
        {
            var networkFleet = _taxiHailNetworkServiceClient.GetNetworkFleet(null);
            return networkFleet.Select(f => new NetworkFleet
            {
                CompanyKey = f.CompanyKey,
                CompanyName = f.CompanyName
            });
        }

        public object Get(MarketVehicleTypesRequest request)
        {
            var market = _taxiHailNetworkServiceClient.GetCompanyMarket(request.Latitude, request.Longitude);
            if (!market.HasValue())
            {
                // In home market, no network vehicles
                return new VehicleType[0];
            }

            var marketVehicleTypes = _taxiHailNetworkServiceClient.GetMarketVehicleTypes(market: market);

            return marketVehicleTypes.Select(v => new VehicleType
            {
                Id = v.Id,
                Name = v.Name,
                LogoName =  v.LogoName,
                ReferenceDataVehicleId = v.ReferenceDataVehicleId,
                MaxNumberPassengers = v.MaxNumberPassengers
            });
        }
    }
}