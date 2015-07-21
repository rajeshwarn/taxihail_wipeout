using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class RideLinqInfoServiceClient : BaseServiceClient
    {
        public RideLinqInfoServiceClient(string url, string sessionId, IPackageInfo packageInfo)
            : base(url, sessionId, packageInfo)
        {
        }

        public Task<RideLinqInfoResponse> GetRideLinqInfoForEHail(Guid OrderId)
        {
            return Client.GetAsync(new RidelinqInfoRequest { OrderId = OrderId });
        }
    }
}
