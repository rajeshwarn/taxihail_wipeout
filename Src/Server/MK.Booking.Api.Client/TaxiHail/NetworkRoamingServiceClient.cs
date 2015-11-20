using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class NetworkRoamingServiceClient : BaseServiceClient
    {
        public NetworkRoamingServiceClient(string url, string sessionId, IPackageInfo packageInfo)
            : base(url, sessionId, packageInfo)
        {
        }

        public Task<string> GetHashedCompanyMarket(double latitude, double longitude)
        {
            var @params = new Dictionary<string, string>
                {
                    {"latitude", latitude.ToString(CultureInfo.InvariantCulture) },
                    {"longitude", longitude.ToString(CultureInfo.InvariantCulture) }
                };

            var queryString = BuildQueryString(@params);

            return Client.GetAsync<string>("/roaming/market" + queryString);
        }

        public Task<List<NetworkFleet>> GetNetworkFleets()
        {
            return Client.GetAsync<List<NetworkFleet>>("/network/networkfleets");
        }

        public Task<List<VehicleType>> GetExternalMarketVehicleTypes(double latitude, double longitude)
        {
            var @params = new Dictionary<string, string>
                {
                    {"latitude", latitude.ToString(CultureInfo.InvariantCulture) },
                    {"longitude", longitude.ToString(CultureInfo.InvariantCulture) }
                };

            var queryString = BuildQueryString(@params);

            return Client.GetAsync<List<VehicleType>>("/roaming/externalMarketVehicleTypes" + queryString);
        }
    }
}
