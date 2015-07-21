using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment
{
    [Authenticate]
    [Route("/account/orders/{OrderId}/ridelinqinfo", "GET")]
    public class RidelinqInfoRequest : IReturn<RideLinqInfoResponse>
    {
        public Guid OrderId { get; set; }
    }
}