using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common;

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class MetricsServiceClient : BaseServiceClient
    {
        public MetricsServiceClient(string url, string sessionId, IPackageInfo packageInfo, IConnectivityService connectivityService)
            : base(url, sessionId, packageInfo, connectivityService)
		{
		}

        public Task LogApplicationStartUp(LogApplicationStartUpRequest request)
        {
            return Client.PostAsync<string>("/account/logstartup", request);
        }

        public Task LogOriginalRideEta(LogOriginalEtaRequest request)
        {
            return Client.PostAsync<string>("/order/logeta", request);
        }
    }
}