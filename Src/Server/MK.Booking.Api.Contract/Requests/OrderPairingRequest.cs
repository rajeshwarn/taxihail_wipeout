#region

using System;
using apcurium.MK.Booking.Api.Contract.Http;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Http;
using apcurium.MK.Common.Http.Response;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RouteDescription("/account/orders/{OrderId}/pairing/", "GET")]
    public class OrderPairingRequest : BaseDto
    {
        public Guid OrderId { get; set; }
    }
    
    public class OrderPairingResponse : OrderPairingDetail, IHasResponseStatus
    {
        public ResponseStatus ResponseStatus { get; set; }
    }
}