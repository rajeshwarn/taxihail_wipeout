using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.TaxiHail;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
    public class NetworkRoamingService : BaseService, INetworkRoamingService
    {
        public Task<string> GetCompanyMarket(double latitude, double longitude)
        {
			return UseServiceClientAsync<NetworkRoamingServiceClient, string>(service => service.GetCompanyMarket(latitude, longitude));
        }
    }
}