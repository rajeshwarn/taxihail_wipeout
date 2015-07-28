using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Client.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class MetricsServiceClient : BaseServiceClient
    {
		public MetricsServiceClient(string url, string sessionId, IPackageInfo packageInfo)
			: base(url, sessionId, packageInfo)
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