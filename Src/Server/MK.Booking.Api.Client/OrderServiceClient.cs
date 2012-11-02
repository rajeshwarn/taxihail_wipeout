using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Entity;
using OrderRatings = apcurium.MK.Booking.Api.Contract.Resources.OrderRatings;

namespace apcurium.MK.Booking.Api.Client
{
    public class OrderServiceClient : BaseServiceClient
    {
        public OrderServiceClient(string url, string sessionId)
            : base(url, sessionId)
        {
        }

        public OrderStatusDetail CreateOrder(CreateOrder order)
        {
            var req = string.Format("/account/orders");
            var result = Client.Post<OrderStatusDetail>(req, order);
            return result;
        }

        public void CancelOrder(Guid orderId)
        {
            var req = string.Format("/account/orders/{0}/cancel", orderId);
            Client.Post<string>(req, new CancelOrder { OrderId = orderId  });            
        }

        public void SendReceipt(Guid orderId)
        {
            var req = string.Format("/account/orders/{0}/sendreceipt", orderId);
            Client.Post<string>(req, new SendReceipt());
        }

        public IList<Order> GetOrders()
        {            
            var req = string.Format("/account/orders");
            var result = Client.Get<IList<Order>>(req).ToArray();
            return result;
        }

        public Order GetOrder(Guid orderId)
        {
            var req = string.Format("/account/orders/{0}", orderId);
            var result = Client.Get<Order>(req);
            return result;
        }

        public void RemoveFromHistory(Guid orderId)
        {
            var req = string.Format("/account/orders/{0}", orderId);
            Client.Delete<string>(req);
        }

        public OrderStatusDetail GetOrderStatus(Guid orderId)
        {
            var req = string.Format("/account/orders/{0}/status", orderId);
            var result = Client.Get<OrderStatusDetail>(req);
            return result;
        }

        public List<RatingType> GetRatingTypes()
        {
            return Client.Get<List<RatingType>>("/ratingtypes");
        }

        public void RateOrder(OrderRatingsRequest orderRatingRequest)
        {
            Client.Post<string>("/ratings/", orderRatingRequest);
        }

        public OrderRatings GetOrderRatings(Guid orderId)
        {
            var req = string.Format("/ratings/{0}", orderId);

            return Client.Get<OrderRatings>(req);
        }
    }
}
