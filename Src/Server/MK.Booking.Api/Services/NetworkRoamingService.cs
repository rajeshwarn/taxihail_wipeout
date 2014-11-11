using apcurium.MK.Booking.Api.Contract;
using apcurium.MK.Booking.Api.Contract.Requests;
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
    }
}