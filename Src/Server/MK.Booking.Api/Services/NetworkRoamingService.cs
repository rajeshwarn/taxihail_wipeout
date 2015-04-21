using System.Linq;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Cryptography;
using CustomerPortal.Client;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Services
{
    public class NetworkRoamingService : Service
    {
        private readonly ITaxiHailNetworkServiceClient _taxiHailNetworkServiceClient;

        public NetworkRoamingService(ITaxiHailNetworkServiceClient taxiHailNetworkServiceClient)
        {
            _taxiHailNetworkServiceClient = taxiHailNetworkServiceClient;
        }

        public object Get(FindMarketRequest request)
        {
            var market = _taxiHailNetworkServiceClient.GetCompanyMarket(request.Latitude, request.Longitude);
            return CryptographyHelper.GetHashString(market);
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