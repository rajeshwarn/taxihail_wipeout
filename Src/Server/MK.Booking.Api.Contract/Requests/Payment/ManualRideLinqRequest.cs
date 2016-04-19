using System;
using apcurium.MK.Common.Http;

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment
{
    [RouteDescription("/accounts/manualridelinq/{OrderId}/status", "GET")]
    [RouteDescription("/accounts/manualridelinq/{OrderId}/unpair", "DELETE")]
    public class ManualRideLinqRequest
    {
        public Guid OrderId { get; set; }
    }
}
