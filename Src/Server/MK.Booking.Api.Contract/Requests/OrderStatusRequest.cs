#region

using System;
using System.Collections;
using System.Collections.Generic;
using apcurium.MK.Booking.Api.Contract.Http;
using apcurium.MK.Common.Entity;
using ServiceStack.ServiceHost;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [Route("/account/orders/{OrderId}/status/", "GET")]
    public class OrderStatusRequest : BaseDto
    {
        public Guid OrderId { get; set; }
    }

    [Authenticate]
    [Route("/account/orders/status/active", "GET")]
    public class ActiveOrderStatusRequest : BaseDto
    {
    }

    [NoCache]
    public class OrderStatusRequestResponse : OrderStatusDetail, IHasResponseStatus
    {
        public ResponseStatus ResponseStatus { get; set; }
    }

    [NoCache]
    public class ActiveOrderStatusRequestResponse : IEnumerable<OrderStatusDetail>, IHasResponseStatus
    {
        private readonly IList<OrderStatusDetail> _details;

        public ActiveOrderStatusRequestResponse()
        {
            _details = new List<OrderStatusDetail>();
        }

        public IEnumerator<OrderStatusDetail> GetEnumerator()
        {
            return _details.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public ResponseStatus ResponseStatus { get; set; }

        public void Add(OrderStatusDetail detail)
        {
            _details.Add(detail);
        }
    }
}