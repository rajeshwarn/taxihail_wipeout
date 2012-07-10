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

        public string Cancel(CancelOrder order)
        {
            var req = string.Format("/account/{0}/orders/{1}/cancel", order.AccountId, order.OrderId);
            var result = Client.Post<string>(req, order);
            return result;
        }

        public IList<Order> GetOrdersByAccount(AccountOrderListRequest order)
        {
            var req = string.Format("/account/{0}/orders", order.AccountId);
            var result = Client.Get<IList<Order>>(req);
            return result;
        }

        public Order GetOrder(OrderRequest order)
        {
            var req = string.Format("/account/{0}/orders/{1}", order.AccountId, order.OrderId);
            var result = Client.Get<Order>(req);
            return result;
        }
    }
}
