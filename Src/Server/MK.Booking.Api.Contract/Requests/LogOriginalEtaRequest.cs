using System;
using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/orders/logeta", "POST")]
    public class LogOriginalEtaRequest
    {
        public Guid OrderId { get; set; }

        public long? OriginalEta { get; set; }
    }
}