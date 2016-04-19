using System;
using apcurium.MK.Common.Http;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RouteDescription("/orders/logeta", "POST")]
    public class LogOriginalEtaRequest
    {
        public Guid OrderId { get; set; }

        public long? OriginalEta { get; set; }
    }
}