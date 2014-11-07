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

        public object Get(LocalMarketRequest request)
        {
            return _taxiHailNetworkServiceClient.GetLocalCompanyMarket(null, request.Latitude, request.Longitude);
        }
    }
}
