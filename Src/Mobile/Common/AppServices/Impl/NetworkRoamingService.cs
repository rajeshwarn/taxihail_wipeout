using System.Collections.Generic;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
    public class NetworkRoamingService : BaseService, INetworkRoamingService
    {
        public Task<string> GetCompanyMarket(double latitude, double longitude)
        {
            var tcs = new TaskCompletionSource<string>();

            try
            {
                var result =
                    UseServiceClientAsync<NetworkRoamingServiceClient, string>(
                        service => service.GetCompanyMarket(latitude, longitude)).Result;
                tcs.TrySetResult(result);
            }
            catch
            {
                tcs.TrySetResult(string.Empty);
            }

            return tcs.Task;
        }

        public Task<List<NetworkFleet>> GetNetworkFleets()
        {
            return UseServiceClientAsync<NetworkRoamingServiceClient, List<NetworkFleet>>(service => service.GetNetworkFleets());
        }
    }
}