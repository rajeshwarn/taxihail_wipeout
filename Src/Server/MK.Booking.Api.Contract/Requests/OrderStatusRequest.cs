using System;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.ServiceModel;
using apcurium.MK.Booking.Api.Contract.Http;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [RestService("/account/orders/{OrderId}/status/", "GET")]
    public class OrderStatusRequest : BaseDTO
    {
        public Guid OrderId { get; set; }
    }

    [NoCache]
    public class OrderStatusRequestResponse: Resources.OrderStatusDetail, IHasResponseStatus
    {
        public ResponseStatus ResponseStatus { get; set; }
    }
}