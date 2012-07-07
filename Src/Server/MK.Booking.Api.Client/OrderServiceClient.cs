using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Api.Client
{
    public class OrderServiceClient : BaseServiceClient
    {
        public OrderServiceClient(string url, AuthInfo credential) : base(url, credential)
        {
        }

        public Guid CreateOrder(CreateOrder order)
        {
            var req = string.Format("/account/{0}/orders", order.AccountId);
            var result = Client.Post<Order>(req, order);
            return result.Id;
        }
    }
}
