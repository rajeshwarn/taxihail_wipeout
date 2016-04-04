using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common;
using apcurium.MK.Common.Diagnostic;

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class MetricsServiceClient : BaseServiceClient
    {
        public MetricsServiceClient(string url, string sessionId, IPackageInfo packageInfo, IConnectivityService connectivityService, ILogger logger)
            : base(url, sessionId, packageInfo, connectivityService, logger)
		{
		}

        public Task LogApplicationStartUp(LogApplicationStartUpRequest request)
        {
            return Client.PostAsync<string>("/accounts/logstartup", request, logger: Logger);
        }

        public Task LogOriginalRideEta(LogOriginalEtaRequest request)
        {
            return Client.PostAsync<string>("/orders/logeta", request, logger: Logger);
        }
    }
}