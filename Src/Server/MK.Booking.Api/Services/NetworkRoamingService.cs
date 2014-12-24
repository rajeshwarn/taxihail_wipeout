using System.Linq;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
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
            return _taxiHailNetworkServiceClient.GetCompanyMarket(request.Latitude, request.Longitude);
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
    }
}