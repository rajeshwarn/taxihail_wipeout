using System;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.ServiceModel;
using apcurium.MK.Booking.Api.Contract.Http;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [RestService("/account/orders/{OrderId}/pairing/", "GET")]
    public class OrderPairingRequest : BaseDTO
    {
        public Guid OrderId { get; set; }
    }

    [NoCache]
    public class OrderPairingResponse : OrderPairingDetail, IHasResponseStatus
    {
        public ResponseStatus ResponseStatus { get; set; }
    }
}