using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Client.Extensions;

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class MetricsServiceClient : BaseServiceClient
    {
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