#region

using System;
using apcurium.MK.Booking.Api.Contract.Http;
using apcurium.MK.Common.Entity;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.ServiceModel;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [Route("/account/orders/{OrderId}/pairing/", "GET")]
    public class OrderPairingRequest : BaseDto
    {
        public Guid OrderId { get; set; }
    }

    [NoCache]
    public class OrderPairingResponse : OrderPairingDetail, IHasResponseStatus
    {
        public ResponseStatus ResponseStatus { get; set; }
    }
}