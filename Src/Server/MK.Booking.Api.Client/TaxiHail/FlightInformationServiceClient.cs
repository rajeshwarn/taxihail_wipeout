using System.Threading.Tasks;
using apcurium.MK.Common;


#if !CLIENT
using apcurium.MK.Booking.Api.Client.Extensions;
#endif
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
	public class FlightInformationServiceClient : BaseServiceClient
	{
        public FlightInformationServiceClient(string url, string sessionId, IPackageInfo packageInfo, IConnectivityService connectivityService)
            : base(url, sessionId, packageInfo, connectivityService)
        {
        }
		public Task<FlightInformation> GetTerminal(FlightInformationRequest request)
		{
			return Client.PostAsync(request);
		}
	}
}
