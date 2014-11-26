using System.Collections.Generic;
using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Api.Client.Extensions;

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class NetworkRoamingServiceClient : BaseServiceClient
    {
        public NetworkRoamingServiceClient(string url, string sessionId, IPackageInfo packageInfo)
            : base(url, sessionId, packageInfo)
        {
        }

        public Task<string> GetCompanyMarket(double latitude, double longitude)
        {
            var @params = new Dictionary<string, string>
                {
                    {"latitude", latitude.ToString() },
                    {"longitude", longitude.ToString() }
                };

            var queryString = BuildQueryString(@params);

            return Client.GetAsync<string>("/roaming/market" + queryString);
        }
    }
}
